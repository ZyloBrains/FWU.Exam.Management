using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AttendanceSheetService(AppDbContext context, IUserContext userContext) : IAttendanceSheetService
{
    public async Task<AttendanceSheetReportDto?> BuildAsync(
        int? examScheduleId,
        int? collegeId,
        int? programId = null)
    {
        if (!examScheduleId.HasValue) return null;

        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(es => es.College)
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.ExamType)
            .Include(es => es.SemesterInstance)
                .ThenInclude(si => si!.Semester)
            .Include(es => es.SemesterInstance)
                .ThenInclude(si => si!.AcademicYear)
            .ApplyScope(userContext)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId.Value);

        if (schedule == null) return null;

        var registrations = await LoadEligibleAsync(examScheduleId.Value, collegeId, programId);

        var identityIds = registrations.Select(er => er.Id).ToList();
        var identities = await StudentIdentityResolver.ResolveAsync(context, identityIds);

        var students = registrations
            .Select(er =>
            {
                identities.TryGetValue(er.Id, out var identity);
                return new AttendanceSheetStudentDto
                {
                    RollNo = er.ExamRollNumber ?? er.SymbolNumber,
                    StudentName = identity?.StudentName ?? er.ApplicationVoucher?.StudentName
                };
            })
            .ToList();

        return new AttendanceSheetReportDto
        {
            ExamScheduleName = schedule.ExamScheduleName,
            ExamScheduleCode = schedule.ExamScheduleCode,
            AcademicYearName = schedule.SemesterInstance?.AcademicYear?.AcademicYearName,
            LevelName = schedule.Level?.LevelName,
            SemesterName = schedule.SemesterInstance?.Semester?.Name,
            ExamTypeName = schedule.ExamType?.Name,
            CollegeName = registrations.FirstOrDefault(r => r.College != null)?.College?.Name
                ?? schedule.College?.Name,
            ProgramName = schedule.Program?.ProgramName ?? schedule.Program?.ShortName,
            CenterName = registrations.FirstOrDefault(r => r.ExamCenter != null)?.ExamCenter?.Code,
            GeneratedDate = DateTime.UtcNow,
            Students = students
        };
    }

    private async Task<List<Domain.Entities.Exams.ExamRegistration>> LoadEligibleAsync(
        int examScheduleId,
        int? collegeId,
        int? programId)
    {
        var query = context.ExamRegistrations
            .AsNoTracking()
            .Include(er => er.College)
            .Include(er => er.Program)
            .Include(er => er.ExamCenter)
            .Include(er => er.ApplicationVoucher)
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.IsActive
                      && er.Status >= RegistrationStatus.CollegeVerified)
            .AsQueryable();

        if (collegeId.HasValue)
            query = query.Where(er => er.CollegeId == collegeId.Value);

        if (programId.HasValue)
            query = query.Where(er => er.ProgramsId == programId.Value);

        var list = await query.ApplyScope(userContext).ToListAsync();

        return list
            .OrderBy(er => er.ExamRollNumber ?? "", StringComparer.OrdinalIgnoreCase)
            .ThenBy(er => er.SymbolNumber ?? "", StringComparer.OrdinalIgnoreCase)
            .ThenBy(er => er.Id)
            .ToList();
    }
}