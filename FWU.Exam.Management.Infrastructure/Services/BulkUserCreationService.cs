using System.Security.Claims;
using System.Text.Json;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BulkUserCreationService(
    AppDbContext context,
    UserManager<AppUser> userManager,
    IServiceScopeFactory scopeFactory,
    ILogger<BulkUserCreationService> logger) : IBulkUserCreationService
{
    private const int BatchSize = 200;

    public async Task<(List<StudentWithoutUserDto> Data, int TotalCount)> GetStudentsWithoutUsersAsync(
        int? collegeId, int? facultyId, int page, int pageSize)
    {
        var query = context.StudentRegistrations
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Where(s => !context.Users.Any(u => u.Email != null && u.Email == s.Email)
                     && !context.Users.Any(u => u.UserName != null && u.UserName == s.RegistrationNumber));

        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentWithoutUserDto
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
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
        var job = new BulkUserCreationJob
        {
            UserId = userId,
            TotalStudents = registrationIds.Count,
            Status = "Running",
            CreatedAt = DateTime.UtcNow
        };

        context.BulkUserCreationJobs!.Add(job);
        await context.SaveChangesAsync();

        var jobId = job.Id;
        var idsJson = JsonSerializer.Serialize(registrationIds);

        _ = Task.Run(async () => await ProcessJobBackgroundAsync(jobId, idsJson));

        return job;
    }

    public async Task<BulkUserCreationJob> StartJobFromFiltersAsync(int? collegeId, int? facultyId, string userId)
    {
        var query = context.StudentRegistrations
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Where(s => !context.Users.Any(u => u.Email != null && u.Email == s.Email)
                     && !context.Users.Any(u => u.UserName != null && u.UserName == s.RegistrationNumber));

        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);

        var ids = await query.Select(s => s.Id).ToListAsync();
        return await StartJobAsync(ids, userId);
    }

    public async Task<BulkUserCreationJob?> GetJobStatusAsync(int jobId)
    {
        return await context.BulkUserCreationJobs!
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId);
    }

    private async Task ProcessJobBackgroundAsync(int jobId, string idsJson)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<BulkUserCreationService>>();

        try
        {
            var job = await scopedContext.BulkUserCreationJobs!.FindAsync(jobId);
            if (job == null) return;

            var registrationIds = JsonSerializer.Deserialize<List<int>>(idsJson) ?? new List<int>();
            var totalBatches = (int)Math.Ceiling(registrationIds.Count / (double)BatchSize);

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = registrationIds.Skip(i * BatchSize).Take(BatchSize).ToList();

                var registrations = await scopedContext.StudentRegistrations
                    .Include(s => s.Faculty)
                    .Include(s => s.College)
                    .Where(s => batch.Contains(s.Id))
                    .ToListAsync();

                foreach (var reg in registrations)
                {
                    try
                    {
                        var loginId = !string.IsNullOrWhiteSpace(reg.Email)
                            ? reg.Email
                            : reg.RegistrationNumber;

                        if (string.IsNullOrWhiteSpace(loginId))
                        {
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        var existingUser = await scopedUserManager.FindByEmailAsync(loginId);
                        if (existingUser == null)
                            existingUser = await scopedUserManager.Users.FirstOrDefaultAsync(u => u.UserName == loginId);

                        if (existingUser != null)
                        {
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        var user = new AppUser
                        {
                            UserName = loginId,
                            Email = loginId,
                            EmailConfirmed = true,
                            FullName = $"{reg.FirstName} {reg.LastName}".Trim(),
                            IsActive = true,
                            FacultyId = reg.FacultyId,
                            CollegeId = reg.CollegeId
                        };

                        var createResult = await scopedUserManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            scopedLogger.LogWarning("Failed to create user for {Name}: {Errors}",
                                $"{reg.FirstName} {reg.LastName}",
                                string.Join(", ", createResult.Errors.Select(e => e.Description)));
                            job.FailedCount++;
                            job.ProcessedCount++;
                            continue;
                        }

                        var password = reg.DateOfBirthBS;
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            SetPasswordHashDirectly(user, password);
                            await scopedUserManager.UpdateAsync(user);
                        }

                        if (!await scopedUserManager.IsInRoleAsync(user, "Student"))
                            await scopedUserManager.AddToRoleAsync(user, "Student");

                        await scopedUserManager.AddClaimAsync(user, new Claim("must_change_password", "true"));

                        var token = await scopedUserManager.GenerateEmailConfirmationTokenAsync(user);
                        await scopedUserManager.ConfirmEmailAsync(user, token);

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

                await scopedContext.SaveChangesAsync();
            }

            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            await scopedContext.SaveChangesAsync();
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
            }
            catch (Exception innerEx)
            {
                scopedLogger.LogError(innerEx, "Failed to update job status for {JobId}", jobId);
            }
        }
    }

    private static void SetPasswordHashDirectly(AppUser user, string password)
    {
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);
    }
}
