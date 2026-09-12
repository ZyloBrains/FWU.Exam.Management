using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
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
            vm.CollegeId = userContext.CollegeId.Value;
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
            .Include(es => es.SemesterInstance)
            .Include(es => es.ExamType)
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
            .Include(so => so.CurriculumVersion)
                .ThenInclude(cv => cv!.EffectiveAcademicYear)
            .Where(so => so.ProgramId == schedule.ProgramId
                      && so.Semester != null && so.Semester.Number == semesterNumber
                      && so.HasPractical);

        List<SubjectOffering> offerings;
        if (curriculumVersionId.HasValue)
        {
            offerings = await query.Where(so => so.CurriculumVersionId == curriculumVersionId.Value)
                .OrderBy(so => so.DisplayOrder).ThenBy(so => so.Id)
                .ToListAsync();
            if (offerings.Count == 0)
            {
                offerings = await query.Where(so => so.CurriculumVersionId == null)
                    .OrderBy(so => so.DisplayOrder).ThenBy(so => so.Id)
                    .ToListAsync();
            }
        }
        else
        {
            offerings = await query.Where(so => so.CurriculumVersionId == null)
                .OrderBy(so => so.DisplayOrder).ThenBy(so => so.Id)
                .ToListAsync();
        }

        if (ExamRegistrationBinder.IsReExamSchedule(schedule))
        {
            // Partial/re-exam schedules are sat by older cohorts: surface the
            // offerings actually registered on this schedule (they may belong to
            // an older curriculum) next to the schedule year's resolution.
            var registered = await ExamRegistrationBinder.GetRegisteredSubjectOfferingsAsync(context, examScheduleId);
            foreach (var ro in registered.Where(ro => ro.HasPractical
                                                   && ro.ProgramId == schedule.ProgramId
                                                   && ro.Semester?.Number == semesterNumber))
            {
                if (offerings.All(o => o.Id != ro.Id))
                    offerings.Add(ro);
            }

            offerings = offerings
                .OrderByDescending(o => curriculumVersionId.HasValue && o.CurriculumVersionId == curriculumVersionId.Value)
                .ThenBy(o => o.DisplayOrder)
                .ThenBy(o => o.Id)
                .ToList();
        }

        return offerings
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
                PracticalPassMarks = so.PracticalPassMarks,
                CurriculumVersionId = so.CurriculumVersionId,
                CurriculumVersionName = so.CurriculumVersion?.Name,
                IsStaleCohort = curriculumVersionId.HasValue
                                && so.CurriculumVersionId.HasValue
                                && so.CurriculumVersionId != curriculumVersionId.Value
            })
            .ToList();
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
            .Include(es => es.SemesterInstance)
            .Include(es => es.ExamType)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var isReExam = ExamRegistrationBinder.IsReExamSchedule(schedule);

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
                && (isReExam
                    || curriculumVersionId == null
                    || so.CurriculumVersionId == curriculumVersionId.Value
                    || so.CurriculumVersionId == null))
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                && er.CollegeId == effectiveCollege
                && er.IsActive
                && er.Status != RegistrationStatus.Withheld
                && er.Status != RegistrationStatus.Rejected)
            .OrderBy(er => er.ExamRollNumber)
            .ThenBy(er => er.Id)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var identities = await StudentIdentityResolver.ResolveAsync(context, erIds);

        var existingResultsQuery = context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.ExamScheduleId == examScheduleId
                       && esr.SubjectOfferingId == subjectOfferingId);
        if (isReExam)
        {
            // Older cohorts pin their marks rows to a cohort-specific offering,
            // so the student list must be drawn from the rows actually registered
            // on this schedule rather than every registration on it.
            existingResultsQuery = existingResultsQuery.Where(esr => erIds.Contains(esr.ExamRegistrationId));
        }

        var existingResults = await existingResultsQuery.ToListAsync();

        var rows = examRegistrations
            .Select(er => new { er, existing = existingResults.FirstOrDefault(esr => esr.ExamRegistrationId == er.Id) })
            .Where(x => !isReExam || x.existing != null)
            // Leg-aware re-exam forms may register a student for the theory
            // paper only; a null flag keeps legacy rows visible.
            .Where(x => x.existing?.IsPracticalRegistered != false)
            .Select(x =>
        {
            var existing = x.existing;
            var er = x.er;
            var identity = identities.GetValueOrDefault(er.Id);

            return new StudentPracticalMarksRowDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                StudentName = identity?.StudentName ?? "",
                RegistrationNumber = identity?.RegistrationNumber ?? "",
                SymbolNumber = er.SymbolNumber ?? er.ExamRollNumber ?? "",
                AcademicYearName = identity?.AcademicYearName ?? "",
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
            .Include(es => es.SemesterInstance)
            .Include(es => es.ExamType)
            .FirstOrDefaultAsync(es => es.Id == dto.ExamScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var isReExam = ExamRegistrationBinder.IsReExamSchedule(schedule);

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
                && (isReExam
                    || curriculumVersionId == null
                    || so.CurriculumVersionId == curriculumVersionId.Value
                    || so.CurriculumVersionId == null))
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var validRegistrationIds = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == dto.ExamScheduleId
                && er.CollegeId == effectiveCollege
                && er.IsActive
                && er.Status >= RegistrationStatus.CollegeVerified)
            .Select(er => er.Id)
            .ToHashSetAsync();

        Dictionary<int, int?> batchSchemes = [];
        if (isReExam)
        {
            var regIds = dto.Students.Select(s => s.ExamRegistrationId).Distinct().ToList();
            var batchYears = await ExamRegistrationBinder.ResolveBatchAcademicYearIdsAsync(context, regIds);
            batchSchemes = await ExamRegistrationBinder.ResolveBatchSchemeIdsAsync(context, schedule.ProgramId, batchYears);
        }

        foreach (var student in dto.Students)
        {
            try
            {
                if (!validRegistrationIds.Contains(student.ExamRegistrationId)) continue;

                ExamSubjectResult? entity;
                if (isReExam)
                {
                    entity = await context.ExamSubjectResults
                        .Include(esr => esr.SubjectOffering)
                        .FirstOrDefaultAsync(esr => esr.ExamRegistrationId == student.ExamRegistrationId
                                                 && esr.ExamScheduleId == dto.ExamScheduleId
                                                 && esr.IsActive);
                }
                else
                {
                    entity = await context.ExamSubjectResults
                        .FirstOrDefaultAsync(esr => esr.ExamRegistrationId == student.ExamRegistrationId
                                                 && esr.SubjectOfferingId == dto.SubjectOfferingId
                                                 && esr.ExamScheduleId == dto.ExamScheduleId);
                }

                var gradingOffering = subjectOffering;
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
                else if (isReExam && entity.SubjectOfferingId != dto.SubjectOfferingId)
                {
                    gradingOffering = entity.SubjectOffering ?? subjectOffering;
                }

                if (!entity.GradingSchemeId.HasValue)
                {
                    if (isReExam && batchSchemes.TryGetValue(student.ExamRegistrationId, out var batchScheme) && batchScheme.HasValue)
                    {
                        entity.GradingSchemeId = batchScheme.Value;
                    }
                    else
                    {
                        var scheduleScheme = gradeCalculationService.ResolveSchemeForProgram(
                            schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);
                        if (scheduleScheme != null) entity.GradingSchemeId = scheduleScheme.Id;
                    }
                }

                entity.ObtainedMarksPractical = student.Practical;
                gradeCalculationService.AssignGrades(entity, gradingOffering, entity.IsSupplementary);

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
}
