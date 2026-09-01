using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PracticalMarksService(
    AppDbContext context,
    IUserContext userContext,
    IGradeCalculationService gradeCalculationService) : IPracticalMarksService
{
    public async Task<PracticalMarksPageViewModel> GetPracticalMarksPageAsync()
    {
        var vm = new PracticalMarksPageViewModel
        {
            IsSuperAdmin = userContext.IsSuperAdmin,
            IsFacultyAdmin = userContext.IsFacultyAdmin,
            IsCollegeAdmin = userContext.IsCollegeAdmin
        };

        if (userContext.IsSuperAdmin)
        {
            vm.Faculties = await GetFacultiesAsync();
        }
        else if (userContext.IsFacultyAdmin)
        {
            vm.Colleges = await GetCollegesAsync(null);
        }
        else if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            vm.Colleges = await GetCollegesAsync(null);
        }

        return vm;
    }

    public Task<List<SelectOption>> GetFacultiesAsync()
    {
        if (!userContext.IsSuperAdmin)
            return Task.FromResult(new List<SelectOption>());

        return context.Faculties
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .Select(f => new SelectOption { Id = f.Id, Name = f.Name })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetCollegesAsync(int? facultyId)
    {
        if (userContext.IsCollegeAdmin)
        {
            if (!userContext.CollegeId.HasValue) return [];
            return await context.Colleges
                .AsNoTracking()
                .Where(c => c.Id == userContext.CollegeId.Value)
                .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        if (userContext.IsFacultyAdmin)
        {
            var collegeIds = userContext.FacultyCollegeIds;
            return await context.Colleges
                .AsNoTracking()
                .Where(c => collegeIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        if (userContext.IsSuperAdmin)
        {
            if (!facultyId.HasValue) return [];
            return await context.Colleges
                .AsNoTracking()
                .Where(c => c.CollegeFaculties!.Any(cf => cf.FacultyId == facultyId.Value))
                .OrderBy(c => c.Name)
                .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        return [];
    }

    public async Task<List<SelectOption>> GetAcademicYearsAsync(int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var yearIds = await ScopedScheduleQuery(effectiveCollege)
            .Select(es => es.SemesterInstance!.AcademicYearId)
            .Distinct()
            .ToListAsync();

        return await context.AcademicYears
            .AsNoTracking()
            .Where(ay => yearIds.Contains(ay.Id))
            .OrderByDescending(ay => ay.IsRunning)
            .ThenByDescending(ay => ay.Id)
            .Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetLevelsAsync(int collegeId, int academicYearId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var levelIds = await ScopedScheduleQuery(effectiveCollege)
            .Where(es => es.SemesterInstance != null && es.SemesterInstance.AcademicYearId == academicYearId && es.Program != null)
            .Select(es => es.Program!.LevelId)
            .Distinct()
            .ToListAsync();

        return await context.Levels
            .AsNoTracking()
            .Where(l => levelIds.Contains(l.Id) && l.IsActive)
            .OrderBy(l => l.LevelDisplayOrder)
            .ThenBy(l => l.LevelName)
            .Select(l => new SelectOption { Id = l.Id, Name = l.LevelName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetExamSchedulesAsync(int collegeId, int academicYearId, int levelId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        return await ScopedScheduleQuery(effectiveCollege)
            .Where(es => es.SemesterInstance != null && es.SemesterInstance.AcademicYearId == academicYearId
                && es.Program != null
                && es.Program.LevelId == levelId)
            .OrderBy(es => es.ExamScheduleName)
            .Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName })
            .ToListAsync();
    }

    public async Task<ScheduleDetailDto> GetScheduleDetailAsync(int examScheduleId, int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.AcademicYear)
            .Include(es => es.Program)
                .ThenInclude(p => p!.Level)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.Semester)
            .Include(es => es.ExamType)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        return new ScheduleDetailDto
        {
            ExamScheduleId = schedule.Id,
            AcademicYearName = schedule.SemesterInstance?.AcademicYear?.AcademicYearName ?? "",
            LevelName = schedule.Program?.Level?.LevelName ?? "",
            ProgramName = schedule.Program?.ProgramName ?? "",
            SemesterName = schedule.SemesterInstance?.Semester?.Name ?? "",
            ExamTypeName = schedule.ExamType?.Name ?? ""
        };
    }

    public async Task<List<SubjectOptionDto>> GetSubjectsByScheduleAsync(int examScheduleId, int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var semesterNumber = await context.Semesters
            .Where(s => s.Id == schedule.SemesterInstance!.SemesterId)
            .Select(s => (int?)s.Number)
            .FirstOrDefaultAsync();

        var curriculumVersionId = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);

        var query = context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Where(so => so.ProgramId == schedule.ProgramId
                      && so.Semester != null && so.Semester.Number == semesterNumber
                      && so.HasPractical);

        Func<IQueryable<FWU.Exam.Management.Domain.Entities.Subjects.SubjectOffering>, IQueryable<SubjectOptionDto>> project = q =>
            q.OrderBy(so => so.DisplayOrder).ThenBy(so => so.Id)
             .Select(so => new SubjectOptionDto
             {
                 Id = so.Id,
                 Name = so.SubjectCatalog != null ? so.SubjectCatalog.SubjectName : "Subject #" + so.Id,
                 Code = so.SubjectCatalog != null ? so.SubjectCatalog.SubjectCode : "",
                 HasTheory = so.HasTheory,
                 HasPractical = so.HasPractical,
                 TheoryFullMarks = so.TheoryFullMarks ?? 0f,
                 InternalTheoryFullMarks = so.InternalTheoryFullMarks,
                 PracticalFullMarks = so.PracticalFullMarks,
                 PracticalPassMarks = so.PracticalPassMarks
             });

        if (curriculumVersionId.HasValue)
        {
            var versioned = await project(query.Where(so => so.CurriculumVersionId == curriculumVersionId.Value))
                .ToListAsync();
            if (versioned.Count > 0) return versioned;
        }

        return await project(query.Where(so => so.CurriculumVersionId == null)).ToListAsync();
    }

    public async Task<SubjectDetailDto> GetSubjectDetailAsync(int subjectOfferingId, int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Where(so => so.Id == subjectOfferingId
                && so.Program != null
                && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == effectiveCollege && cp.IsActive))
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Subject offering not found.");

        return new SubjectDetailDto
        {
            SubjectOfferingId = subjectOffering.Id,
            Name = subjectOffering.SubjectCatalog?.SubjectName ?? "Unknown",
            Code = subjectOffering.SubjectCatalog?.SubjectCode ?? "",
            HasTheory = subjectOffering.HasTheory,
            HasPractical = subjectOffering.HasPractical,
            HasInternal = subjectOffering.HasInternal,
            TheoryFullMarks = subjectOffering.TheoryFullMarks ?? 0f,
            TheoryPassMarks = subjectOffering.TheoryPassMarks ?? 0f,
            InternalTheoryFullMarks = subjectOffering.InternalTheoryFullMarks,
            InternalTheoryPassMarks = subjectOffering.InternalTheoryPassMarks,
            PracticalFullMarks = subjectOffering.PracticalFullMarks,
            PracticalPassMarks = subjectOffering.PracticalPassMarks
        };
    }

    public async Task<StudentPracticalMarksViewModel> GetStudentsForPracticalMarksAsync(int examScheduleId, int subjectOfferingId, int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var semesterNumber = await context.Semesters
            .Where(s => s.Id == schedule.SemesterInstance!.SemesterId)
            .Select(s => (int?)s.Number)
            .FirstOrDefaultAsync();

        var curriculumVersionId = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);

        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId
                && so.ProgramId == schedule.ProgramId
                && so.Semester != null && so.Semester.Number == semesterNumber
                && (curriculumVersionId == null || so.CurriculumVersionId == curriculumVersionId.Value || so.CurriculumVersionId == null))
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                && er.CollegeId == effectiveCollege
                && er.IsActive
                && er.Status >= RegistrationStatus.CollegeVerified)
            .OrderBy(er => er.ExamRollNumber)
            .ThenBy(er => er.Id)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                && esr.ExamScheduleId == examScheduleId)
            .ToListAsync();

        var rows = examRegistrations
            .Select(er => new { er, existing = existingResults.FirstOrDefault(esr => esr.ExamRegistrationId == er.Id) })
            // Leg-aware re-exam forms may register a student for the theory
            // paper only; a null flag keeps legacy rows visible.
            .Where(x => x.existing?.IsPracticalRegistered != false)
            .Select(x =>
        {
            var existing = x.existing;
            var er = x.er;
            registrationNumbers.TryGetValue(er.Id, out var regNum);

            return new StudentPracticalMarksRowDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                RegistrationNumber = regNum ?? "",
                SymbolNumber = er.SymbolNumber ?? er.ExamRollNumber ?? "",
                Practical = existing?.ObtainedMarksPractical,
                IsSubmitted = existing?.IsSubmitted ?? false
            };
        }).ToList();

        return new StudentPracticalMarksViewModel
        {
            ExamScheduleId = examScheduleId,
            SubjectOfferingId = subjectOfferingId,
            PracticalFullMarks = subjectOffering.PracticalFullMarks,
            PracticalPassMarks = subjectOffering.PracticalPassMarks,
            Students = rows
        };
    }

    public async Task<BulkSaveResult> SavePracticalMarksAsync(PracticalMarksSaveDto dto)
    {
        var result = new BulkSaveResult { Success = true };
        var effectiveCollege = GetEffectiveCollegeId(dto.CollegeId);

        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .FirstOrDefaultAsync(es => es.Id == dto.ExamScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var semesterNumber = await context.Semesters
            .Where(s => s.Id == schedule.SemesterInstance!.SemesterId)
            .Select(s => (int?)s.Number)
            .FirstOrDefaultAsync();

        var curriculumVersionId = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);

        var subjectOffering = await context.SubjectOfferings
            .FirstOrDefaultAsync(so => so.Id == dto.SubjectOfferingId
                && so.ProgramId == schedule.ProgramId
                && so.Semester != null && so.Semester.Number == semesterNumber
                && (curriculumVersionId == null || so.CurriculumVersionId == curriculumVersionId.Value || so.CurriculumVersionId == null))
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var validRegistrationIds = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == dto.ExamScheduleId
                && er.CollegeId == effectiveCollege
                && er.IsActive
                && er.Status >= RegistrationStatus.CollegeVerified)
            .Select(er => er.Id)
            .ToHashSetAsync();

        foreach (var student in dto.Students)
        {
            try
            {
                if (!validRegistrationIds.Contains(student.ExamRegistrationId)) continue;

                var entity = await context.ExamSubjectResults
                    .FirstOrDefaultAsync(esr => esr.ExamRegistrationId == student.ExamRegistrationId
                        && esr.SubjectOfferingId == dto.SubjectOfferingId
                        && esr.ExamScheduleId == dto.ExamScheduleId);

                if (entity == null)
                {
                    entity = new ExamSubjectResult
                    {
                        TenantId = 1,
                        ExamRegistrationId = student.ExamRegistrationId,
                        ExamTypeId = schedule.ExamTypeId,
                        SubjectOfferingId = dto.SubjectOfferingId,
                        ExamScheduleId = dto.ExamScheduleId,
                        IsActive = true,
                        IsSubmitted = false
                    };
                    context.ExamSubjectResults.Add(entity);
                }

                entity.ObtainedMarksPractical = student.Practical;
                gradeCalculationService.AssignGrades(entity, subjectOffering, entity.IsSupplementary);

                if (dto.SubmitAll || student.IsSubmitted)
                {
                    entity.IsSubmitted = true;
                    entity.ExamSubmittedDateTime = DateTime.UtcNow;
                }

                result.SavedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Student '{student.ExamRegistrationId}': {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
        result.Success = result.Errors.Count == 0;

        return result;
    }

    private IQueryable<ExamSchedule> ScopedScheduleQuery(int effectiveCollegeId)
    {
        var collegeProgramIds = context.CollegePrograms
            .Where(cp => cp.CollegeId == effectiveCollegeId && cp.IsActive)
            .Select(cp => cp.ProgramId);

        var query = context.ExamSchedules
            .AsNoTracking()
            .Where(es => es.IsActive
                && (es.CollegeId == null || es.CollegeId == effectiveCollegeId)
                && collegeProgramIds.Contains(es.ProgramId));

        if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
        {
            var facultyId = userContext.FacultyId.Value;
            query = query.Where(es => es.Program != null && es.Program.FacultyId == facultyId);
        }

        return query;
    }

    private int GetEffectiveCollegeId(int? requestedCollegeId)
    {
        if (userContext.IsCollegeAdmin)
        {
            if (userContext.CollegeId is not int collegeId)
                throw new UnauthorizedAccessException("No college associated with your account.");
            return collegeId;
        }

        if (userContext.IsFacultyAdmin)
        {
            if (!requestedCollegeId.HasValue)
                throw new UnauthorizedAccessException("A college must be selected.");
            if (!userContext.FacultyCollegeIds.Contains(requestedCollegeId.Value))
                throw new UnauthorizedAccessException("You do not have access to this college.");
            return requestedCollegeId.Value;
        }

        if (userContext.IsSuperAdmin)
        {
            if (!requestedCollegeId.HasValue)
                throw new UnauthorizedAccessException("A college must be selected.");
            return requestedCollegeId.Value;
        }

        throw new UnauthorizedAccessException("You are not authorized to manage practical marks.");
    }

    private async Task<Dictionary<int, string>> GetRegistrationNumbersForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
        var result = new Dictionary<int, string>();

        var registrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => examRegistrationIds.Contains(er.Id) && er.ApplicationVoucherId != null)
            .Select(er => new { er.Id, er.ApplicationVoucherId })
            .ToListAsync();

        var voucherIds = registrations
            .Where(r => r.ApplicationVoucherId.HasValue)
            .Select(r => r.ApplicationVoucherId!.Value)
            .Distinct()
            .ToList();

        if (voucherIds.Count > 0)
        {
            var vouchers = await context.ApplicationVouchers!
                .AsNoTracking()
                .Where(v => voucherIds.Contains(v.Id) && v.StudentRegistrationId != null)
                .Select(v => new { v.Id, v.StudentRegistrationId })
                .ToListAsync();

            var erIdToSrId = registrations
                .Where(r => r.ApplicationVoucherId.HasValue)
                .Join(vouchers,
                    r => r.ApplicationVoucherId!.Value,
                    v => v.Id,
                    (r, v) => new { r.Id, SrId = v.StudentRegistrationId!.Value })
                .ToDictionary(x => x.Id, x => x.SrId);

            var srIds = erIdToSrId.Values.Distinct().ToList();
            if (srIds.Count > 0)
            {
                var regBySr = await context.StudentRegistrations!
                    .AsNoTracking()
                    .Where(sr => srIds.Contains(sr.Id) && sr.RegistrationNumber != null)
                    .ToDictionaryAsync(sr => sr.Id, sr => sr.RegistrationNumber!);

                foreach (var (erId, srId) in erIdToSrId)
                {
                    if (regBySr.TryGetValue(srId, out var rn))
                        result[erId] = rn;
                }
            }
        }

        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => examRegistrationIds.Contains(er.Id)))
            .ToListAsync();

        var admissionIds = semEnrollments
            .Where(se => se.StudentAdmission != null)
            .Select(se => se.StudentAdmission!.Id)
            .Distinct()
            .ToList();

        if (admissionIds.Count > 0)
        {
            var regByAdmission = await context.StudentRegistrations!
                .AsNoTracking()
                .Where(sr => sr.StudentAdmissionId != null && admissionIds.Contains(sr.StudentAdmissionId!.Value))
                .Select(sr => new { AdmissionId = sr.StudentAdmissionId!.Value, sr.RegistrationNumber })
                .Where(x => x.RegistrationNumber != null)
                .Distinct()
                .ToDictionaryAsync(x => x.AdmissionId, x => x.RegistrationNumber!);

            foreach (var se in semEnrollments)
            {
                if (se.ExamRegistrations == null) continue;
                var regNum = se.StudentAdmission != null && regByAdmission.TryGetValue(se.StudentAdmission.Id, out var rn) ? rn : "";
                foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
                {
                    result.TryAdd(er.Id, regNum);
                }
            }
        }

        return result;
    }
}
