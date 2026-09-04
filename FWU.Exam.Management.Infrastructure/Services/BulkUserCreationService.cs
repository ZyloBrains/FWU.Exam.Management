using System.Security.Claims;
using System.Text.Json;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BulkUserCreationService(
    AppDbContext context,
    IServiceScopeFactory scopeFactory,
    ITenantContext tenantContext,
    IUserContext userContext,
    IAuditLogWriter auditLogWriter) : IBulkUserCreationService
{
    private const int BatchSize = 50;

    public async Task<(List<StudentWithoutUserDto> Data, int TotalCount)> GetStudentsWithoutUsersAsync(
        int? collegeId, int? facultyId, int? programId, int page, int pageSize)
    {
        var query = context.StudentRegistrations
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Where(s => !context.Users.Any(u => u.Email != null && u.Email == s.Email)
                     && !context.Users.Any(u => u.UserName != null && u.UserName == s.RegistrationNumber))
            .ApplyScope(userContext);

        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);
        if (programId.HasValue)
            query = query.Where(s => s.ProgramId == programId.Value);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentWithoutUserDto
            {
                Id = s.Id,
                FullName = string.Join(" ", new[] { s.FirstName, s.MiddleName, s.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                Email = s.Email,
                RegistrationNumber = s.RegistrationNumber,
                CollegeName = s.College != null ? s.College.Name : "",
                FacultyName = s.Faculty != null ? s.Faculty.Name : "",
                DateOfBirthBS = s.DateOfBirthBS,
                HasEmail = s.Email != null && s.Email != ""
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<BulkUserCreationJob> StartJobAsync(List<int> registrationIds, string userId)
    {
        var scopedIds = await context.StudentRegistrations
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(s => registrationIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var job = new BulkUserCreationJob
        {
            UserId = userId,
            TotalStudents = scopedIds.Count,
            Status = "Running",
            CreatedAt = DateTime.UtcNow
        };

        context.BulkUserCreationJobs!.Add(job);
        await context.SaveChangesAsync();

        var jobId = job.Id;
        var idsJson = JsonSerializer.Serialize(scopedIds);
        var capturedTenantId = tenantContext.TenantId;
        var capturedTenantCode = tenantContext.TenantCode;
        var capturedTenantType = tenantContext.Type;

        await auditLogWriter.LogAsync(ActivityTypes.UsersBulkCreationStarted, $"Bulk user creation job {jobId} started", new { jobId, totalStudents = job.TotalStudents, requestedCount = registrationIds.Count, scopedCount = scopedIds.Count }, entityName: "BulkUserCreationJob", entityId: jobId.ToString(), actorUserId: userId);

        _ = Task.Run(async () => await ProcessJobBackgroundAsync(jobId, idsJson, capturedTenantId, capturedTenantCode, capturedTenantType));

        return job;
    }

    public async Task<BulkUserCreationJob> StartJobFromFiltersAsync(int? collegeId, int? facultyId, int? programId, string userId)
    {
        var query = context.StudentRegistrations
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Where(s => !context.Users.Any(u => u.Email != null && u.Email == s.Email)
                     && !context.Users.Any(u => u.UserName != null && u.UserName == s.RegistrationNumber))
            .ApplyScope(userContext);

        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);
        if (programId.HasValue)
            query = query.Where(s => s.ProgramId == programId.Value);

        var ids = await query.Select(s => s.Id).ToListAsync();
        return await StartJobAsync(ids, userId);
    }

    public async Task<BulkUserCreationJob?> GetJobStatusAsync(int jobId)
    {
        var query = context.BulkUserCreationJobs!
            .AsNoTracking()
            .Where(j => j.Id == jobId);

        if (!userContext.IsSuperAdmin)
            query = query.Where(j => j.UserId == userContext.UserId);

        return await query.FirstOrDefaultAsync();
    }

    private async Task ProcessJobBackgroundAsync(int jobId, string idsJson, int tenantId, string tenantCode, TenantType tenantType)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<BulkUserCreationService>>();
        var scopedTenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        scopedTenantContext.SetTenant(tenantId, tenantCode, tenantType);

        try
        {
            var job = await scopedContext.BulkUserCreationJobs!.FindAsync(jobId);
            if (job == null) return;

            var registrationIds = JsonSerializer.Deserialize<List<int>>(idsJson) ?? new List<int>();

            // Pre-load ALL existing emails/usernames ONCE (2 queries total)
            var existingEmails = await scopedContext.Users
                .Where(u => u.Email != null)
                .Select(u => u.Email!)
                .ToListAsync();
            var existingUserNames = await scopedContext.Users
                .Where(u => u.UserName != null)
                .Select(u => u.UserName!)
                .ToListAsync();
            var existingEmailSet = new HashSet<string>(existingEmails, StringComparer.OrdinalIgnoreCase);
            var existingUserNameSet = new HashSet<string>(existingUserNames, StringComparer.OrdinalIgnoreCase);

            var totalBatches = (int)Math.Ceiling(registrationIds.Count / (double)BatchSize);

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = registrationIds.Skip(i * BatchSize).Take(BatchSize).ToList();

                var registrations = await scopedContext.StudentRegistrations
                    .Include(s => s.StudentAdmission)
                    .Where(s => batch.Contains(s.Id))
                    .ToListAsync();

                foreach (var reg in registrations)
                {
                    try
                    {
                        // The registration number is the primary login identifier for students.
                        var loginId = !string.IsNullOrWhiteSpace(reg.RegistrationNumber)
                            ? reg.RegistrationNumber
                            : reg.Email;

                        if (string.IsNullOrWhiteSpace(loginId))
                        {
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        // Check pre-loaded sets instead of individual DB queries
                        if (existingUserNameSet.Contains(loginId)
                            || (reg.Email != null && existingEmailSet.Contains(reg.Email)))
                        {
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        var user = new AppUser
                        {
                            UserName = loginId,
                            Email = reg.Email,
                            EmailConfirmed = false,
                            FullName = reg.FirstName.GetFullName(reg.MiddleName, reg.LastName),
                            IsActive = true,
                            FacultyId = reg.FacultyId,
                            CollegeId = reg.CollegeId
                        };

                        var createResult = await scopedUserManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        var password = reg.DateOfBirthBS;
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
                        }

                        await scopedUserManager.AddToRoleAsync(user, Role.Student);
                        await scopedUserManager.AddClaimAsync(user, new Claim("must_change_password", "true"));

                        if (reg.StudentAdmission != null)
                            reg.StudentAdmission.AppUserId = user.Id;

                        // Add to pre-loaded sets so duplicates within same batch are caught
                        existingUserNameSet.Add(loginId);
                        if (reg.Email != null)
                            existingEmailSet.Add(reg.Email);

                        job.SuccessCount++;
                        job.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        scopedLogger.LogError(ex, "Error creating user for registration {Id}", reg.Id);
                        job.FailedCount++;
                        job.ProcessedCount++;
                    }
                }

                // Save progress after EACH batch
                await scopedContext.SaveChangesAsync();
            }

            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            await scopedContext.SaveChangesAsync();

            var scopedAuditWriter = scope.ServiceProvider.GetRequiredService<IAuditLogWriter>();
            await scopedAuditWriter.LogAsync(ActivityTypes.UsersBulkCreationCompleted, $"Bulk user creation job {jobId} completed", new { jobId, successCount = job.SuccessCount, failedCount = job.FailedCount, totalStudents = job.TotalStudents }, entityName: "BulkUserCreationJob", entityId: jobId.ToString(), actorUserId: job.UserId);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Bulk user creation job {JobId} failed", jobId);
            try
            {
                var job = await scopedContext.BulkUserCreationJobs!.FindAsync(jobId);
                if (job != null)
                {
                    job.Status = "Failed";
                    job.ErrorMessage = ex.Message;
                    job.CompletedAt = DateTime.UtcNow;
                    await scopedContext.SaveChangesAsync();
                }

                var scopedAuditWriter = scope.ServiceProvider.GetRequiredService<IAuditLogWriter>();
                await scopedAuditWriter.LogAsync(ActivityTypes.UsersBulkCreationFailed, $"Bulk user creation job {jobId} failed", new { jobId, error = ex.Message }, AuditSeverity.Error, "BulkUserCreationJob", jobId.ToString(), job?.UserId);
            }
            catch (Exception innerEx)
            {
                scopedLogger.LogError(innerEx, "Failed to update job status for {JobId}", jobId);
            }
        }
    }
}
