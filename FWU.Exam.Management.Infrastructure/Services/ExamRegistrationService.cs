using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamRegistrationService(AppDbContext context, IUserContext userContext) : IExamRegistrationService
{
    public async Task<(List<ExamRegistration> Items, int TotalCount)> GetExamRegistrationsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = BuildQuery(search, sort, sortDir, examScheduleId);
        query = query.ApplyScope(userContext);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                AttendancePercentage = e.AttendancePercentage,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                RollNumberIndex = e.RollNumberIndex,
                IsAppliedByStudent = e.IsAppliedByStudent,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                ExamCenter = e.ExamCenter,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamRegistration>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc", null);
        query = query.ApplyScope(userContext);
        return await query
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();
    }

    public async Task<ExamRegistration?> GetExamRegistrationByIdAsync(int id)
    {
        return await context.ExamRegistrations
            .AsNoTracking()
            .Include(e => e.ExamSchedule)
            .Include(e => e.College)
            .Include(e => e.ExamCenter)
            .Include(e => e.AcademicYear)
            .Include(e => e.Program)
            .Include(e => e.ApplicationVoucher)
            .Include(e => e.ExamSubjectResults)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        context.ExamRegistrations.Add(examRegistration);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        var existing = await context.ExamRegistrations.FindAsync(examRegistration.Id);
        if (existing != null)
        {
            examRegistration.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examRegistration);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null)
        {
            examRegistration.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamRegistrationExistsAsync(int id)
    {
        return await context.ExamRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task VerifyExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.Pending)
        {
            examRegistration.Status = RegistrationStatus.CollegeVerified;
            examRegistration.VerifiedByUsername = null;
            examRegistration.VerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task ApproveExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.CollegeVerified)
        {
            examRegistration.Status = RegistrationStatus.AdminVerified;
            examRegistration.AdminVerifiedByUsername = null;
            examRegistration.AdminVerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<ExamRegistrationSelectListsDto> GetSelectListDataAsync(ExamRegistration? examRegistration = null)
    {
        var examSchedulesQuery = context.ExamSchedules.AsNoTracking().ApplyScope(userContext);
        var examSchedules = await examSchedulesQuery.ToListAsync();

        var collegesQuery = context.Colleges.AsNoTracking().ApplyScope(userContext);
        var colleges = await collegesQuery.ToListAsync();

        var academicYears = await context.AcademicYears.AsNoTracking().ToListAsync();

        var programsQuery = context.Programs.AsNoTracking().ApplyScope(userContext);
        var programs = await programsQuery.ToListAsync();

        var examCentersQuery = context.ExamCenters.AsNoTracking();
        var examCenters = await examCentersQuery.ToListAsync();

        return new ExamRegistrationSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            ExamCenters = examCenters.Select(ec => new SelectOption { Id = ec.Id, Name = $"Center {ec.Code}" }).ToList()
        };
    }

    private IQueryable<ExamRegistration> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = context.ExamRegistrations.AsNoTracking();

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.ExamRollNumber != null && e.ExamRollNumber.Contains(search)) ||
                (e.Remarks != null && e.Remarks.Contains(search)) ||
                (e.Sgpa != null && e.Sgpa.Contains(search)) ||
                (e.ExamSchedule != null && e.ExamSchedule.ExamScheduleName != null && e.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (e.College != null && e.College.Name != null && e.College.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "rollnumber" => descending ? query.OrderByDescending(e => e.ExamRollNumber) : query.OrderBy(e => e.ExamRollNumber),
            "schedule" => descending
                ? query.OrderByDescending(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty)
                : query.OrderBy(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty),
            "college" => descending
                ? query.OrderByDescending(e => e.College != null ? e.College.Name : string.Empty)
                : query.OrderBy(e => e.College != null ? e.College.Name : string.Empty),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }

    public async Task<ExamFormsAdminResult> GetStudentExamFormsAsync(int? examScheduleId, string? search, int page, int pageSize)
    {
        var query = context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.IsAppliedByStudent == true && er.IsActive);

        if (examScheduleId.HasValue)
            query = query.Where(er => er.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(er =>
                (er.ExamRollNumber != null && er.ExamRollNumber.Contains(search)) ||
                (er.Remarks != null && er.Remarks.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(er => er.ExamSchedule)
            .Include(er => er.College)
            .Include(er => er.Program)
            .OrderByDescending(er => er.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (items.Count == 0)
        {
            return new ExamFormsAdminResult
            {
                Forms = [],
                TotalCount = totalCount,
                PaymentConfirmedCount = 0,
                AdmitCardGeneratedCount = 0,
                PendingAdmitCardCount = 0
            };
        }

        var registrationIds = items.Select(i => i.Id).ToList();
        var scheduleIds = items.Select(i => i.ExamScheduleId).Distinct().ToList();
        var collegeIds = items.Select(i => i.CollegeId).Distinct().ToList();
        var programIds = items.Where(i => i.ProgramsId.HasValue).Select(i => i.ProgramsId!.Value).Distinct().ToList();

        var admissions = await context.StudentAdmissions!
            .AsNoTracking()
            .Where(sa => sa.IsActive && sa.AppUserId != null
                      && collegeIds.Contains(sa.CollegeId)
                      && programIds.Contains(sa.ProgramsId))
            .ToListAsync();

        var admissionLookup = admissions
            .Where(a => a.AppUserId != null)
            .ToDictionary(
                a => (a.CollegeId, a.ProgramsId),
                a => a.AppUserId!);

        var appUserIds = admissions
            .Where(a => a.AppUserId != null)
            .Select(a => a.AppUserId!)
            .Distinct()
            .ToList();

        var appUsers = appUserIds.Count > 0
            ? await context.Users!
                .AsNoTracking()
                .Where(u => appUserIds.Contains(u.Id))
                .ToListAsync()
            : [];

        var userIdToEmail = appUsers
            .Where(u => u.Email != null)
            .ToDictionary(u => u.Id, u => u.Email!);

        var emails = appUsers
            .Where(u => u.Email != null)
            .Select(u => u.Email!)
            .Distinct()
            .ToList();

        var studentRegistrations = emails.Count > 0
            ? await context.StudentRegistrations!
                .AsNoTracking()
                .Where(sr => sr.IsActive && sr.Email != null && emails.Contains(sr.Email))
                .ToListAsync()
            : [];

        var emailToStudentReg = studentRegistrations
            .Where(sr => sr.Email != null)
            .ToDictionary(sr => sr.Email!, sr => sr);

        var paymentLogs = await context.PaymentRequestLogs!
            .AsNoTracking()
            .Where(prl => scheduleIds.Contains(prl.ExamScheduleId)
                       && prl.StudentRegistrationId != null)
            .ToListAsync();

        var paymentLogLookup = paymentLogs
            .Where(pl => pl.PaymentRequestLogStatus == 1)
            .ToDictionary(
                pl => (pl.ExamScheduleId, pl.StudentRegistrationId!.Value),
                pl => pl);

        var admitCards = await context.AdmitCards!
            .AsNoTracking()
            .Where(ac => registrationIds.Contains(ac.ExamRegistrationId) && ac.IsActive)
            .ToListAsync();

        var forms = items.Select(er =>
        {
            string? studentName = null;
            string? registrationNumber = null;
            bool paymentConfirmed = false;
            string? invoiceNumber = null;

            if (er.ProgramsId.HasValue
                && admissionLookup.TryGetValue((er.CollegeId, er.ProgramsId.Value), out var userId)
                && userIdToEmail.TryGetValue(userId, out var email)
                && emailToStudentReg.TryGetValue(email, out var sr))
            {
                studentName = string.Join(" ", new[] { sr.FirstName, sr.MiddleName, sr.LastName }.Where(x => !string.IsNullOrEmpty(x)));
                registrationNumber = sr.RegistrationNumber;

                if (paymentLogLookup.TryGetValue((er.ExamScheduleId, sr.Id), out var pl))
                {
                    paymentConfirmed = true;
                    invoiceNumber = pl.InvoiceNumber;
                }
            }

            return new ExamFormAdminDto
            {
                ExamRegistrationId = er.Id,
                StudentName = studentName,
                RegistrationNumber = registrationNumber,
                CollegeName = er.College?.Name,
                ExamScheduleId = er.ExamScheduleId,
                ExamScheduleName = er.ExamSchedule?.ExamScheduleName,
                ProgramName = er.Program?.ProgramName,
                FeeEnclosed = er.FeeEnclosed,
                Status = er.Status,
                PaymentConfirmed = paymentConfirmed,
                InvoiceNumber = invoiceNumber,
                HasAdmitCard = admitCards.Any(ac => ac.ExamRegistrationId == er.Id),
                RegistrationDate = er.RegistrationDate
            };
        }).ToList();

        var allPaymentConfirmedCount = paymentLogs.Count(pl => pl.PaymentRequestLogStatus == 1);

        var allAdmitCardCount = await context.AdmitCards!
            .AsNoTracking()
            .Where(ac => context.ExamRegistrations!
                .Any(er => er.Id == ac.ExamRegistrationId
                        && er.IsAppliedByStudent == true
                        && er.IsActive)
                && ac.IsActive)
            .CountAsync();

        return new ExamFormsAdminResult
        {
            Forms = forms,
            TotalCount = totalCount,
            PaymentConfirmedCount = allPaymentConfirmedCount,
            AdmitCardGeneratedCount = allAdmitCardCount,
            PendingAdmitCardCount = Math.Max(0, totalCount - allAdmitCardCount)
        };
    }
}
