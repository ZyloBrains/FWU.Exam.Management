using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamScheduleService(AppDbContext context, IUserContext userContext) : IExamScheduleService
{
    public async Task<(List<ExamSchedule> Items, int TotalCount)> GetExamSchedulesAsync(int page, int pageSize, string? search, string sort, string sortDir, string? examTypeName = null)
    {
        var query = BuildQuery(search, sort, sortDir, examTypeName).ApplyScope(userContext);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeactivateExpiredSchedulesAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await context.ExamSchedules
            .Where(e => e.IsActive && e.EndDate != null && e.EndDate < today)
            .ToListAsync();

        foreach (var schedule in expired)
        {
            schedule.IsActive = false;
        }

        if (expired.Count > 0)
            await context.SaveChangesAsync();
    }

    public async Task<List<ExamSchedule>> GetFilteredItemsAsync(string? search, string? examTypeName = null)
    {
        var query = BuildQuery(search, "Id", "asc", examTypeName).ApplyScope(userContext);
        return await query
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .ToListAsync();
    }

    public async Task<ExamSchedule?> GetExamScheduleByIdAsync(int id)
    {
        return await context.ExamSchedules
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                TenantId = e.TenantId,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                SemesterId = e.SemesterId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                Semester = e.Semester,
                ExamType = e.ExamType
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateExamScheduleAsync(ExamSchedule examSchedule)
    {
        context.ExamSchedules.Add(examSchedule);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamScheduleAsync(ExamSchedule examSchedule)
    {
        var existing = await context.ExamSchedules.FindAsync(examSchedule.Id);
        if (existing == null)
            throw new InvalidOperationException("Exam schedule not found.");

        if (examSchedule.ExtendedDate.HasValue && examSchedule.EndDate.HasValue)
        {
            var extendedChanged = existing.ExtendedDate != examSchedule.ExtendedDate;

            if (extendedChanged)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var threshold = examSchedule.EndDate.Value.AddDays(-7);
                if (today < threshold && today < examSchedule.EndDate.Value)
                {
                    throw new InvalidOperationException("Extension is only allowed when the exam end date is near or has passed.");
                }

                if (DateOnly.FromDateTime(examSchedule.ExtendedDate.Value) <= examSchedule.EndDate.Value)
                {
                    throw new InvalidOperationException("Extended date must be after the original end date.");
                }
            }
        }

        context.Entry(existing).CurrentValues.SetValues(examSchedule);
        await context.SaveChangesAsync();
    }

    public async Task DeleteExamScheduleAsync(int id)
    {
        if (!await context.ExamSchedules.AnyAsync(e => e.Id == id))
            return;

        if (await context.ExamRegistrations.AnyAsync(r => r.ExamScheduleId == id))
            throw new InvalidOperationException(
                "Cannot delete this exam schedule because students are already registered for it. Delete the registrations first.");

        if (await context.ExamSubjectResults.AnyAsync(r => r.ExamScheduleId == id)
            || await context.ResultRecords.AnyAsync(r => r.ExamScheduleId == id)
            || await context.ApplicationVouchers.AnyAsync(v => v.ExamScheduleId == id))
            throw new InvalidOperationException(
                "Cannot delete this exam schedule because exam results or vouchers already exist for it.");

        await using var transaction = await context.Database.BeginTransactionAsync();

        await context.ExamScheduleCollegeApprovals.Where(a => a.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.ExamSlots.Where(s => s.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.ExamRollNumberSetup.Where(r => r.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.ExamFees.Where(f => f.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.CollegeAdminSubjectAssignments.Where(a => a.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.Set<BillTitle>().Where(b => b.ExamScheduleId == id).ExecuteDeleteAsync();

        var examCenterIds = context.ExamCenters.Where(c => c.ExamScheduleId == id).Select(c => c.Id);
        await context.ExamCenterSymbolRanges.Where(r => examCenterIds.Contains(r.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenterVenues.Where(v => examCenterIds.Contains(v.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenterColleges.Where(c => examCenterIds.Contains(c.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenters.Where(c => c.ExamScheduleId == id).ExecuteDeleteAsync();

        await context.AdmitCards.Where(a => a.ExamScheduleId == id).ExecuteDeleteAsync();

        var paymentRequestLogIds = context.PaymentRequestLogs.Where(p => p.ExamScheduleId == id).Select(p => p.Id);
        await context.PaymentPracticalSubjects.Where(p => paymentRequestLogIds.Contains(p.PaymentRequestLogId)).ExecuteDeleteAsync();
        await context.PaymentResponseLogs.Where(p => paymentRequestLogIds.Contains(p.PaymentRequestLogId)).ExecuteDeleteAsync();
        await context.PaymentRequestLogs.Where(p => p.ExamScheduleId == id).ExecuteDeleteAsync();

        await context.ExamSchedules.Where(e => e.Id == id).ExecuteDeleteAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> ExamScheduleExistsAsync(int id)
    {
        return await context.ExamSchedules.AnyAsync(e => e.Id == id);
    }

    public async Task<ExamScheduleSelectListsDto> GetSelectListDataAsync(ExamSchedule? examSchedule = null)
    {
        var academicYears = await context.AcademicYears.AsNoTracking().ToListAsync();
        var examTypes = await context.ExamTypes.AsNoTracking().ToListAsync();
        var programsQuery = context.Programs.AsNoTracking().ApplyScope(userContext);
        var programs = await programsQuery.ToListAsync();
        var semestersQuery = context.Semesters.AsNoTracking().ApplyScope(userContext);
        if (examSchedule != null && examSchedule.AcademicYearId > 0)
            semestersQuery = semestersQuery.Where(s => s.AcademicYearId == examSchedule.AcademicYearId);
        var allowUpperSemesters = examSchedule != null
                                  && examSchedule.ProgramId > 0
                                  && await context.Programs.AsNoTracking().AnyAsync(p => p.Id == examSchedule.ProgramId && p.Duration >= 10);
        if (!allowUpperSemesters)
            semestersQuery = semestersQuery.Where(s => s.Number <= 8);
        var semesters = await semestersQuery.OrderBy(s => s.Number).ToListAsync();

        return new ExamScheduleSelectListsDto
        {
            AcademicYears = [.. academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName })],
            ExamTypes = [.. examTypes.Select(et => new SelectOption { Id = et.Id, Name = et.Name })],
            Programs = [.. programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName })],
            Semesters = [.. semesters.Select(s => new SelectOption { Id = s.Id, Name = s.Name + " (" + s.Code + ")" })]
        };
    }

    public async Task<List<SelectOption>> GetSemestersByAcademicYearAsync(int academicYearId, int? programId = null)
    {
        var allowUpperSemesters = programId is > 0
                                  && await context.Programs.AsNoTracking().AnyAsync(p => p.Id == programId.Value && p.Duration >= 10);

        return await context.Semesters.AsNoTracking()
            .ApplyScope(userContext)
            .Where(s => s.AcademicYearId == academicYearId
                        && (s.Number <= 8 || allowUpperSemesters))
            .OrderBy(s => s.Number)
            .Select(s => new SelectOption
            {
                Id = s.Id,
                Name = s.Name + " (" + s.Code + ")"
            })
            .ToListAsync();
    }

    private IQueryable<ExamSchedule> BuildQuery(string? search, string sort, string sortDir, string? examTypeName = null)
    {
        var query = context.ExamSchedules.AsNoTracking();

        if (!string.IsNullOrEmpty(examTypeName))
            query = query.Where(s => s.ExamType != null && s.ExamType.Name == examTypeName);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.ExamScheduleName != null && s.ExamScheduleName.Contains(search)) ||
                (s.ExamScheduleCode != null && s.ExamScheduleCode.Contains(search)) ||
                (s.Remarks != null && s.Remarks.Contains(search)) ||
                (s.AcademicYear != null && s.AcademicYear.AcademicYearName != null && s.AcademicYear.AcademicYearName.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)) ||
                (s.ExamType != null && s.ExamType.Name != null && s.ExamType.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(e => e.ExamScheduleName) : query.OrderBy(e => e.ExamScheduleName),
            "code" => descending ? query.OrderByDescending(e => e.ExamScheduleCode) : query.OrderBy(e => e.ExamScheduleCode),
            "academicyear" => descending
                ? query.OrderByDescending(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty)
                : query.OrderBy(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty),
            "level" => descending
                ? query.OrderByDescending(e => e.Program != null ? e.Program.ProgramName : string.Empty)
                : query.OrderBy(e => e.Program != null ? e.Program.ProgramName : string.Empty),
            "examtype" => descending
                ? query.OrderByDescending(e => e.ExamType != null ? e.ExamType.Name : string.Empty)
                : query.OrderBy(e => e.ExamType != null ? e.ExamType.Name : string.Empty),
            "startdate" => descending ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
