using System.Globalization;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeAdminMarksService(
    AppDbContext context,
    IUserContext userContext,
    ICollegeAdminSubjectAssignmentService assignmentService,
    IGradeCalculationService gradeCalculationService,
    IAuditLogWriter auditLogWriter) : ICollegeAdminMarksService
{
    public async Task<InternalMarksPageViewModel> GetInternalMarksPageAsync()
    {
        var vm = new InternalMarksPageViewModel
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
            vm.AcademicYears = await GetAcademicYearsAsync(userContext.CollegeId.Value);
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
                      && so.Semester != null && so.Semester.Number == semesterNumber);

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
                 InternalTheoryFullMarks = so.InternalTheoryFullMarks
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
            InternalTheoryPassMarks = subjectOffering.InternalTheoryPassMarks
        };
    }

    public async Task<StudentInternalMarksViewModel> GetStudentsForInternalMarksAsync(int examScheduleId, int subjectOfferingId, int collegeId)
    {
        var effectiveCollege = GetEffectiveCollegeId(collegeId);

        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .Include(es => es.SemesterInstance)
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
            // Leg-aware re-exam forms may register a student for a single paper;
            // keep the row when either leg is registered (null flags = legacy).
            .Where(x => x.existing == null
                     || x.existing.IsTheoryRegistered != false
                     || x.existing.IsPracticalRegistered != false)
            .Select(x =>
        {
            var existing = x.existing;
            var er = x.er;
            registrationNumbers.TryGetValue(er.Id, out var regNum);

            return new StudentInternalMarksRowDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                RegistrationNumber = regNum ?? "",
                SymbolNumber = er.SymbolNumber ?? er.ExamRollNumber ?? "",
                TheoryInternal = existing?.ObtainedMarksTheoryInternal,
                PracticalInternal = existing?.ObtainedMarksPracticalInternal,
                IsSubmitted = existing?.IsSubmitted ?? false
            };
        }).ToList();

        return new StudentInternalMarksViewModel
        {
            ExamScheduleId = examScheduleId,
            SubjectOfferingId = subjectOfferingId,
            HasPractical = subjectOffering.HasPractical,
            InternalTheoryFullMarks = subjectOffering.InternalTheoryFullMarks,
            Students = rows
        };
    }

    public async Task<BulkSaveResult> SaveInternalMarksAsync(InternalMarksSaveDto dto)
    {
        var effectiveCollege = GetEffectiveCollegeId(dto.CollegeId);

        var bulkDto = new BulkMarksSaveDto
        {
            SubjectOfferingId = dto.SubjectOfferingId,
            ExamScheduleId = dto.ExamScheduleId,
            SubmitAll = dto.SubmitAll,
            Students = dto.Students
                .Select(s => new StudentMarksRowDto
                {
                    ExamRegistrationId = s.ExamRegistrationId,
                    ExamSubjectResultId = s.ExamSubjectResultId,
                    TheoryInternal = s.TheoryInternal,
                    PracticalInternal = s.PracticalInternal,
                    IsSubmitted = s.IsSubmitted
                }).ToList()
        };

        return await SaveMarksCoreAsync(bulkDto, effectiveCollege);
    }

    private async Task<Dictionary<int, string>> GetRegistrationNumbersForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
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

        if (admissionIds.Count == 0) return new Dictionary<int, string>();

        var regByAdmission = await context.StudentRegistrations!
            .AsNoTracking()
            .Where(sr => sr.StudentAdmissionId != null && admissionIds.Contains(sr.StudentAdmissionId!.Value))
            .Select(sr => new { AdmissionId = sr.StudentAdmissionId!.Value, sr.RegistrationNumber })
            .Where(x => x.RegistrationNumber != null)
            .Distinct()
            .ToDictionaryAsync(x => x.AdmissionId, x => x.RegistrationNumber!);

        var result = new Dictionary<int, string>();
        foreach (var se in semEnrollments)
        {
            if (se.ExamRegistrations == null) continue;
            var regNum = se.StudentAdmission != null && regByAdmission.TryGetValue(se.StudentAdmission.Id, out var rn) ? rn : "";
            foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
            {
                result[er.Id] = regNum;
            }
        }

        return result;
    }

    public async Task<BulkSaveResult> SaveMarksBulkAsync(BulkMarksSaveDto dto, string collegeAdminUserId)
    {
        if (!await assignmentService.IsCollegeAdminAssignedToSubjectAsync(collegeAdminUserId, dto.SubjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        return await SaveMarksCoreAsync(dto, GetEffectiveCollegeId(null));
    }

    public async Task<BulkSaveResult> SaveCollegeMarksBulkAsync(BulkMarksSaveDto dto, int collegeId, string collegeAdminUserId)
    {
        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .FirstOrDefaultAsync(so => so.Id == dto.SubjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var belongsToCollege = await context.CollegePrograms
            .AnyAsync(cp => cp.CollegeId == collegeId
                         && cp.ProgramId == subjectOffering.ProgramId
                         && cp.IsActive);

        if (!belongsToCollege)
            throw new UnauthorizedAccessException("This subject is not offered at your college.");

        return await SaveMarksCoreAsync(dto, collegeId);
    }

    private async Task<BulkSaveResult> SaveMarksCoreAsync(BulkMarksSaveDto dto, int effectiveCollege)
    {
        var schedule = await ScopedScheduleQuery(effectiveCollege)
            .Include(es => es.SemesterInstance)
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

        var result = new BulkSaveResult { Success = true };

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

                entity.ObtainedMarksTheoryInternal = student.TheoryInternal;
                entity.ObtainedMarksPracticalInternal = student.PracticalInternal;
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

        await auditLogWriter.LogAsync(ActivityTypes.MarksSaved,
            $"Marks saved for subject offering {dto.SubjectOfferingId} (schedule {dto.ExamScheduleId}, submitted: {dto.SubmitAll})",
            new { subjectOfferingId = dto.SubjectOfferingId, examScheduleId = dto.ExamScheduleId, submitAll = dto.SubmitAll, savedCount = result.SavedCount, errorCount = result.Errors.Count },
            entityName: "ExamSubjectResult", entityId: dto.SubjectOfferingId.ToString(), actorUserId: userContext.UserId);

        return result;
    }

    public async Task<CollegeAdminDashboardDto> GetCollegeAdminDashboardAsync(string collegeAdminUserId)
    {
        var assignments = await assignmentService.GetAssignmentsAsync(collegeAdminUserId);
        var subjectOfferingIds = assignments.Select(a => a.SubjectOfferingId).Distinct().ToList();

        var subjectOfferings = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Program)
            .Include(so => so.Semester)
            .Where(so => subjectOfferingIds.Contains(so.Id))
            .ToListAsync();

        return new CollegeAdminDashboardDto
        {
            CollegeAdminUserId = collegeAdminUserId,
            TotalAssignedSubjects = subjectOfferings.Count,
            AssignedSubjects = subjectOfferings.Select(so =>
            {
                var examScheduleIds = assignments
                    .Where(a => a.SubjectOfferingId == so.Id && a.ExamScheduleId != null)
                    .Select(a => a.ExamScheduleId!.Value)
                    .Distinct()
                    .ToList();

                var registeredCount = context.ExamRegistrations
                    .Count(er => examScheduleIds.Contains(er.ExamScheduleId)
                              && er.ProgramsId == so.ProgramId
                              && er.IsActive);

                var marksEnteredCount = context.ExamSubjectResults
                    .Count(esr => examScheduleIds.Contains(esr.ExamScheduleId ?? 0)
                               && esr.SubjectOfferingId == so.Id
                               && esr.IsSubmitted);

                return new CollegeAdminSubjectInfo
                {
                    SubjectOfferingId = so.Id,
                    SubjectName = so.SubjectCatalog?.SubjectName ?? "Unknown",
                    SubjectCode = so.SubjectCatalog?.SubjectCode ?? "",
                    ProgramName = so.Program?.ProgramName ?? "",
                    SemesterName = so.Semester?.Name ?? "",
                    RegisteredStudentCount = registeredCount,
                    MarksEnteredCount = marksEnteredCount
                };
            }).ToList()
        };
    }

    public async Task<MarksEntryViewModel> GetMarksEntryViewAsync(int subjectOfferingId, int examScheduleId, string collegeAdminUserId)
    {
        if (!await assignmentService.IsCollegeAdminAssignedToSubjectAsync(collegeAdminUserId, subjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();

        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                       && esr.ExamScheduleId == examScheduleId)
            .ToListAsync();

        var studentRows = examRegistrations.Select(er =>
        {
            var existing = existingResults.FirstOrDefault(esr => esr.ExamRegistrationId == er.Id);
            studentNames.TryGetValue(er.Id, out var name);
            registrationNumbers.TryGetValue(er.Id, out var regNum);

            return new StudentMarksRowDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                StudentName = name ?? $"Student #{er.Id}",
                SymbolNumber = er.ExamRollNumber ?? "",
                RegistrationNumber = regNum ?? "",
                TheoryMarks = existing?.ObtainedMarksTheory,
                TheoryConfirm = existing?.ObtainedMarksTheoryConfirm,
                PracticalMarks = existing?.ObtainedMarksPractical,
                PracticalConfirm = existing?.ObtainedMarksPracticalConfirm,
                TheoryInternal = existing?.ObtainedMarksTheoryInternal,
                PracticalInternal = existing?.ObtainedMarksPracticalInternal,
                TotalMarks = existing?.ObtainedMarks,
                GradeLetter = existing?.GradeLetter,
                IsSubmitted = existing?.IsSubmitted ?? false
            };
        }).ToList();

        return new MarksEntryViewModel
        {
            SubjectOfferingId = subjectOfferingId,
            ExamScheduleId = examScheduleId,
            SubjectName = subjectOffering.SubjectCatalog?.SubjectName ?? "Unknown",
            SubjectCode = subjectOffering.SubjectCatalog?.SubjectCode ?? "",
            TheoryFullMarks = subjectOffering.TheoryFullMarks,
            TheoryPassMarks = subjectOffering.TheoryPassMarks,
            PracticalFullMarks = subjectOffering.PracticalFullMarks,
            PracticalPassMarks = subjectOffering.PracticalPassMarks,
            HasTheory = subjectOffering.HasTheory,
            HasPractical = subjectOffering.HasPractical,
            HasInternal = subjectOffering.HasInternal,
            Students = studentRows
        };
    }

    public async Task<ExcelImportResultDto> ImportMarksFromExcelAsync(Stream excelStream, int subjectOfferingId, int examScheduleId, string collegeAdminUserId)
    {
        if (!await assignmentService.IsCollegeAdminAssignedToSubjectAsync(collegeAdminUserId, subjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var subjectOffering = await context.SubjectOfferings
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var result = new ExcelImportResultDto();

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var range = worksheet.RangeUsed();
        if (range == null) return result;

        var allRows = range.RowsUsed().ToList();
        if (allRows.Count < 2) return result;

        result.TotalRows = allRows.Count - 1;

        var headerCells = allRows[0].CellsUsed().Select(c => c.GetString().Trim().ToLowerInvariant()).ToList();

        int colSymbolNo = -1, colRegNo = -1, colStudentName = -1;
        int colTheory = -1, colTheoryConfirm = -1;
        int colPractical = -1, colPracticalConfirm = -1;
        int colTheoryInternal = -1, colPracticalInternal = -1;

        for (int i = 0; i < headerCells.Count; i++)
        {
            var h = headerCells[i];
            if (h.Contains("symbol") || h.Contains("roll no") || h.Contains("exam roll")) colSymbolNo = i;
            else if (h.Contains("reg no") || h == "regno" || h.Contains("registration")) colRegNo = i;
            else if (h.Contains("student")) colStudentName = i;
            else if ((h.Contains("theory") || h.Contains("th.")) && h.Contains("confirm")) colTheoryConfirm = i;
            else if ((h.Contains("theory") || h.Contains("th.")) && !h.Contains("internal") && colTheory == -1) colTheory = i;
            else if ((h.Contains("practical") || h.Contains("pr.")) && h.Contains("confirm")) colPracticalConfirm = i;
            else if ((h.Contains("practical") || h.Contains("pr.")) && !h.Contains("internal") && colPractical == -1) colPractical = i;
            else if (h.Contains("theory") && h.Contains("internal")) colTheoryInternal = i;
            else if (h.Contains("practical") && h.Contains("internal")) colPracticalInternal = i;
        }

        if (colSymbolNo == -1 && colRegNo == -1 && colStudentName == -1)
        {
            var foundHeaders = string.Join(", ", headerCells.Select(c => $"\"{c}\""));
            result.Errors.Add($"Could not identify student identifier columns (Symbol No., Reg No., or Student Name). Found headers: {foundHeaders}");
            result.ErrorCount++;
            result.Success = false;
            return result;
        }

        foreach (var row in allRows.Skip(1))
        {
            try
            {
                var examRollNumber = colSymbolNo >= 0 ? row.Cell(colSymbolNo + 1).GetString().Trim() : "";
                var regNo = colRegNo >= 0 ? row.Cell(colRegNo + 1).GetString().Trim() : "";
                var studentName = colStudentName >= 0 ? row.Cell(colStudentName + 1).GetString().Trim() : "";

                var theoryStr = colTheory >= 0 ? row.Cell(colTheory + 1).GetString().Trim() : "";
                var theoryConfirmStr = colTheoryConfirm >= 0 ? row.Cell(colTheoryConfirm + 1).GetString().Trim() : "";
                var practicalStr = colPractical >= 0 ? row.Cell(colPractical + 1).GetString().Trim() : "";
                var practicalConfirmStr = colPracticalConfirm >= 0 ? row.Cell(colPracticalConfirm + 1).GetString().Trim() : "";
                var theoryInternalStr = colTheoryInternal >= 0 ? row.Cell(colTheoryInternal + 1).GetString().Trim() : "";
                var practicalInternalStr = colPracticalInternal >= 0 ? row.Cell(colPracticalInternal + 1).GetString().Trim() : "";

                var examRegs = await context.ExamRegistrations
                    .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive)
                    .ToListAsync();

                var examReg = examRegs.FirstOrDefault(er => er.ExamRollNumber == examRollNumber);

                if (examReg == null && !string.IsNullOrEmpty(studentName))
                {
                    var erIds = examRegs.Select(er => er.Id).ToList();
                    var names = await GetStudentNamesForExamRegistrationsAsync(erIds);
                    examReg = examRegs.FirstOrDefault(er =>
                        names.TryGetValue(er.Id, out var name) &&
                        string.Equals(name, studentName, StringComparison.OrdinalIgnoreCase));
                }

                if (examReg == null && !string.IsNullOrEmpty(regNo))
                {
                    var erIds = examRegs.Select(er => er.Id).ToList();
                    var regNos = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);
                    examReg = examRegs.FirstOrDefault(er =>
                        regNos.TryGetValue(er.Id, out var rn) &&
                        string.Equals(rn, regNo, StringComparison.OrdinalIgnoreCase));
                }

                if (examReg == null)
                {
                    var identifier = !string.IsNullOrEmpty(examRollNumber) ? examRollNumber
                        : !string.IsNullOrEmpty(regNo) ? regNo
                        : studentName;
                    result.Errors.Add($"Row {row.RowNumber()}: No exam registration found for '{identifier}'.");
                    result.ErrorCount++;
                    continue;
                }

                var existing = await context.ExamSubjectResults
                    .FirstOrDefaultAsync(esr => esr.ExamRegistrationId == examReg.Id
                                             && esr.SubjectOfferingId == subjectOfferingId
                                             && esr.ExamScheduleId == examScheduleId);

                float? theoryMarks = float.TryParse(theoryStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var tVal) ? tVal : null;
                float? theoryConfirmMarks = float.TryParse(theoryConfirmStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var tcVal) ? tcVal : null;
                float? practicalMarks = float.TryParse(practicalStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var pVal) ? pVal : null;
                float? practicalConfirmMarks = float.TryParse(practicalConfirmStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var pcVal) ? pcVal : null;
                float? theoryInternal = float.TryParse(theoryInternalStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var tiVal) ? tiVal : null;
                float? practicalInternal = float.TryParse(practicalInternalStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var piVal) ? piVal : null;

                if (existing != null)
                {
                    existing.ObtainedMarksTheory = theoryMarks;
                    existing.ObtainedMarksTheoryConfirm = theoryConfirmMarks;
                    existing.ObtainedMarksPractical = practicalMarks;
                    existing.ObtainedMarksPracticalConfirm = practicalConfirmMarks;
                    existing.ObtainedMarksTheoryInternal = theoryInternal;
                    existing.ObtainedMarksPracticalInternal = practicalInternal;
                    existing.ObtainedMarks = gradeCalculationService.CalculateTotalMarks(theoryMarks, practicalMarks, theoryInternal, practicalInternal);
                    existing.GradeLetter = gradeCalculationService.CalculateGrade(existing.ObtainedMarks.Value, subjectOffering).GradeLetter;
                }
                else
                {
                    var totalMarks = gradeCalculationService.CalculateTotalMarks(theoryMarks, practicalMarks, theoryInternal, practicalInternal);
                    var examTypeId = await context.ExamSchedules
                        .Where(es => es.Id == examScheduleId)
                        .Select(es => es.ExamTypeId)
                        .FirstOrDefaultAsync();

                    var newResult = new ExamSubjectResult
                    {
                        TenantId = 1,
                        ExamRegistrationId = examReg.Id,
                        ExamTypeId = examTypeId,
                        SubjectOfferingId = subjectOfferingId,
                        ExamScheduleId = examScheduleId,
                        ObtainedMarksTheory = theoryMarks,
                        ObtainedMarksTheoryConfirm = theoryConfirmMarks,
                        ObtainedMarksPractical = practicalMarks,
                        ObtainedMarksPracticalConfirm = practicalConfirmMarks,
                        ObtainedMarksTheoryInternal = theoryInternal,
                        ObtainedMarksPracticalInternal = practicalInternal,
                        ObtainedMarks = totalMarks,
                        GradeLetter = gradeCalculationService.CalculateGrade(totalMarks, subjectOffering).GradeLetter,
                        IsActive = true,
                        IsSubmitted = false
                    };
                    context.ExamSubjectResults.Add(newResult);
                }

                result.SuccessCount++;
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                result.ErrorCount++;
            }
        }

        result.SkippedCount = result.ErrorCount;

        await context.SaveChangesAsync();
        result.Success = result.Errors.Count == 0;
        return result;
    }

    public async Task<byte[]> ExportMarksTemplateAsync(int subjectOfferingId, int examScheduleId)
    {
        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks");

        ws.Cell(1, 1).Value = "S.N.";
        ws.Cell(1, 2).Value = "Student Name";
        ws.Cell(1, 3).Value = "Symbol No.";
        ws.Cell(1, 4).Value = "Reg No.";
        ws.Cell(1, 5).Value = $"Theory Marks (Full: {subjectOffering.TheoryFullMarks})";
        ws.Cell(1, 6).Value = "Theory Marks (Confirm)";
        ws.Cell(1, 7).Value = subjectOffering.HasPractical
            ? $"Practical Marks (Full: {subjectOffering.PracticalFullMarks})"
            : "Practical Marks";
        ws.Cell(1, 8).Value = "Practical Marks (Confirm)";
        ws.Cell(1, 9).Value = subjectOffering.HasInternal ? "Theory Internal" : "";
        ws.Cell(1, 10).Value = subjectOffering.HasInternal ? "Practical Internal" : "";

        var headerRange = ws.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        var sn = 1;
        foreach (var er in examRegistrations)
        {
            studentNames.TryGetValue(er.Id, out var name);
            registrationNumbers.TryGetValue(er.Id, out var regNo);

            ws.Cell(row, 1).Value = sn++;
            ws.Cell(row, 2).Value = name ?? "";
            ws.Cell(row, 3).Value = er.ExamRollNumber ?? "";
            ws.Cell(row, 4).Value = regNo ?? "";
            ws.Cell(row, 5).Value = "";
            ws.Cell(row, 6).Value = "";
            ws.Cell(row, 7).Value = "";
            ws.Cell(row, 8).Value = "";
            ws.Cell(row, 9).Value = "";
            ws.Cell(row, 10).Value = "";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportMarksAsync(int subjectOfferingId, int examScheduleId)
    {
        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                       && esr.ExamScheduleId == examScheduleId)
            .ToDictionaryAsync(esr => esr.ExamRegistrationId);

        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks");

        ws.Cell(1, 1).Value = "S.N.";
        ws.Cell(1, 2).Value = "Student Name";
        ws.Cell(1, 3).Value = "Symbol No.";
        ws.Cell(1, 4).Value = "Reg No.";
        ws.Cell(1, 5).Value = $"Theory (Full: {subjectOffering.TheoryFullMarks})";
        ws.Cell(1, 6).Value = "Theory (Confirm)";
        ws.Cell(1, 7).Value = subjectOffering.HasPractical
            ? $"Practical (Full: {subjectOffering.PracticalFullMarks})"
            : "Practical";
        ws.Cell(1, 8).Value = "Practical (Confirm)";
        ws.Cell(1, 9).Value = subjectOffering.HasInternal ? "Theory Internal" : "";
        ws.Cell(1, 10).Value = subjectOffering.HasInternal ? "Practical Internal" : "";
        ws.Cell(1, 11).Value = "Total";
        ws.Cell(1, 12).Value = "Grade";
        ws.Cell(1, 13).Value = "Status";
        ws.Cell(1, 14).Value = "Submitted";

        var headerRange = ws.Range(1, 1, 1, 14);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        var sn = 1;
        foreach (var er in examRegistrations)
        {
            studentNames.TryGetValue(er.Id, out var name);
            registrationNumbers.TryGetValue(er.Id, out var regNo);
            existingResults.TryGetValue(er.Id, out var result);

            ws.Cell(row, 1).Value = sn++;
            ws.Cell(row, 2).Value = name ?? "";
            ws.Cell(row, 3).Value = er.ExamRollNumber ?? "";
            ws.Cell(row, 4).Value = regNo ?? "";
            ws.Cell(row, 5).Value = result?.ObtainedMarksTheory ?? 0;
            ws.Cell(row, 6).Value = result?.ObtainedMarksTheoryConfirm ?? 0;
            ws.Cell(row, 7).Value = result?.ObtainedMarksPractical ?? 0;
            ws.Cell(row, 8).Value = result?.ObtainedMarksPracticalConfirm ?? 0;
            ws.Cell(row, 9).Value = result?.ObtainedMarksTheoryInternal?.ToString() ?? "";
            ws.Cell(row, 10).Value = result?.ObtainedMarksPracticalInternal?.ToString() ?? "";
            ws.Cell(row, 11).Value = result?.ObtainedMarks?.ToString() ?? "";
            ws.Cell(row, 12).Value = result?.GradeLetter ?? "";
            ws.Cell(row, 13).Value = result == null
                ? ""
                : gradeCalculationService.IsStudentPassing(result.ObtainedMarksTheory, result.ObtainedMarksPractical, subjectOffering) ? "Pass" : "Fail";
            ws.Cell(row, 14).Value = result?.IsSubmitted == true ? "Yes" : "No";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private async Task<Dictionary<int, string>> GetStudentNamesForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => examRegistrationIds.Contains(er.Id)))
            .ToListAsync();

        var userIds = semEnrollments
            .Select(se => se.StudentAdmission?.AppUserId)
            .Where(id => id != null)
            .Distinct()
            .Cast<string>()
            .ToList();

        var userNames = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        var names = new Dictionary<int, string>();
        foreach (var se in semEnrollments)
        {
            if (se.ExamRegistrations == null) continue;
            var appUserId = se.StudentAdmission?.AppUserId;
            var name = appUserId != null && userNames.TryGetValue(appUserId, out var n) ? n : "";
            foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
            {
                names[er.Id] = name;
            }
        }

        return names;
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

        throw new UnauthorizedAccessException("You are not authorized to manage marks.");
    }
}
