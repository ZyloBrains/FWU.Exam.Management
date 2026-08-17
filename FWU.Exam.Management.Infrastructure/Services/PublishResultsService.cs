using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PublishResultsService(
    AppDbContext context,
    IGradeCalculationService gradeCalculationService) : IPublishResultsService
{
    public async Task<PublishResultsPreviewDto?> GetPreviewAsync(int examScheduleId, int collegeId)
    {
        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .Include(s => s.ExamType)
            .Include(s => s.Level)
            .FirstOrDefaultAsync(s => s.Id == examScheduleId);

        if (schedule == null) return null;

        var college = await context.Colleges
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == collegeId);

        var registrations = await LoadRegistrationsWithResults(examScheduleId, collegeId);

        var gradingScheme = await context.GradingSchemes
            .AsNoTracking()
            .Include(gs => gs.GradeDefinitions)
            .Where(gs => gs.ProgramId == schedule.ProgramId && gs.IsActive)
            .OrderByDescending(gs => gs.GradeGroupId.HasValue)
            .ThenBy(gs => gs.Id)
            .FirstOrDefaultAsync();

        var gradeGroupId = gradingScheme?.GradeGroupId;

        var subjectOfferings = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterId)
            .ToListAsync();

        var subjectOfferingMap = subjectOfferings.ToDictionary(so => so.Id);

        var students = new List<PublishResultsStudentDto>();

        foreach (var reg in registrations)
        {
            var studentName = await GetStudentNameAsync(reg);
            var dob = GetStudentDobAsync(reg);
            var sex = GetStudentSex(reg);

            var subjectDtos = new List<PublishResultsSubjectDto>();
            decimal totalWeightedPoints = 0;
            int totalCreditHours = 0;
            bool hasFail = false;

            var results = reg.ExamSubjectResults?
                .Where(r => r.IsActive)
                .ToList() ?? [];

            foreach (var result in results)
            {
                if (!subjectOfferingMap.TryGetValue(result.SubjectOfferingId, out var offering))
                    continue;

                var creditHours = offering.SubjectCatalog?.CreditHours ?? 0;

                var theory = result.ObtainedMarksTheory;
                var practical = result.ObtainedMarksPractical;
                var theoryInternal = result.ObtainedMarksTheoryInternal;
                var practicalInternal = result.ObtainedMarksPracticalInternal;

                var totalMarks = gradeCalculationService.CalculateTotalMarks(
                    theory, practical, theoryInternal, practicalInternal);

                var overall = gradeCalculationService.CalculateGrade(totalMarks, offering, gradingScheme);

                decimal? gradePoint = null;
                if (gradeGroupId.HasValue && !string.IsNullOrEmpty(overall.GradeLetter))
                {
                    gradePoint = gradeCalculationService.GetGradePointValue(overall.GradeLetter, gradeGroupId);
                }

                if (gradePoint.HasValue && creditHours > 0)
                {
                    totalWeightedPoints += gradePoint.Value * creditHours;
                    totalCreditHours += creditHours;
                }

                if (!overall.IsPass) hasFail = true;

                subjectDtos.Add(new PublishResultsSubjectDto
                {
                    SubjectOfferingId = offering.Id,
                    SubjectCode = offering.SubjectCatalog?.SubjectCode,
                    SubjectName = offering.SubjectCatalog?.SubjectName,
                    CreditHours = creditHours,
                    TheoryMarks = theory,
                    InternalMarks = theoryInternal,
                    PracticalMarks = practical,
                    TotalMarks = totalMarks,
                    GradeLetter = overall.GradeLetter,
                    GradePoint = gradePoint
                });
            }

            decimal? gpa = totalCreditHours > 0 ? Math.Round(totalWeightedPoints / totalCreditHours, 2) : null;

            students.Add(new PublishResultsStudentDto
            {
                ExamRegistrationId = reg.Id,
                StudentName = studentName,
                SymbolNumber = reg.SymbolNumber,
                RegistrationNumber = await GetRegistrationNumberAsync(reg),
                DateOfBirthBs = dob,
                Sex = sex,
                Subjects = subjectDtos,
                GPA = gpa,
                Result = hasFail ? "Fail" : "Pass"
            });
        }

        students = students.OrderBy(s => s.SymbolNumber).ThenBy(s => s.StudentName).ToList();

        return new PublishResultsPreviewDto
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule.ExamScheduleName,
            CollegeId = collegeId,
            CollegeName = college?.Name,
            ProgramName = schedule.Program?.ProgramName,
            SemesterName = schedule.Semester?.Name,
            AcademicYearName = schedule.AcademicYear?.AcademicYearName,
            Students = students,
            TotalStudents = students.Count,
            SubjectsCount = subjectOfferings.Count
        };
    }

    public async Task<PublishResultsResultDto> PublishResultsAsync(int examScheduleId, int collegeId, string publishedBy)
    {
        var preview = await GetPreviewAsync(examScheduleId, collegeId);
        if (preview == null)
            return new PublishResultsResultDto { Success = false, Message = "Exam schedule or college not found." };

        if (preview.Students.Count == 0)
            return new PublishResultsResultDto { Success = false, Message = "No students found for this schedule and college." };

        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Semester)
            .Include(s => s.ExamType)
            .Include(s => s.Level)
            .FirstOrDefaultAsync(s => s.Id == examScheduleId);

        if (schedule == null)
            return new PublishResultsResultDto { Success = false, Message = "Exam schedule not found." };

        var yearStr = RomanNumeral(schedule.Semester!.Year);
        var partStr = RomanNumeral(schedule.Semester.Number <= 2 ? 1 : schedule.Semester.Number <= 4 ? 2 : 3);

        var existingRecords = await context.ResultRecords
            .Where(r => r.ExamScheduleId == examScheduleId && r.CollegeId == collegeId)
            .ToListAsync();

        if (existingRecords.Count > 0)
            context.ResultRecords.RemoveRange(existingRecords);

        var records = new List<ResultRecord>();

        foreach (var student in preview.Students)
        {
            float? theoryObtained = null;
            float? internalObtained = null;
            float? practicalObtained = null;
            float? totalObtained = null;
            decimal? totalGradePoints = null;

            foreach (var subject in student.Subjects)
            {
                if (subject.TheoryMarks.HasValue)
                    theoryObtained = (theoryObtained ?? 0) + subject.TheoryMarks.Value;
                if (subject.InternalMarks.HasValue)
                    internalObtained = (internalObtained ?? 0) + subject.InternalMarks.Value;
                if (subject.PracticalMarks.HasValue)
                    practicalObtained = (practicalObtained ?? 0) + subject.PracticalMarks.Value;
                if (subject.TotalMarks.HasValue)
                    totalObtained = (totalObtained ?? 0) + subject.TotalMarks.Value;
            }

            if (student.GPA.HasValue)
            {
                totalGradePoints = student.GPA;
            }

            var record = new ResultRecord
            {
                TenantId = schedule.TenantId,
                AcademicYearId = schedule.AcademicYearId,
                LevelId = schedule.LevelId,
                ProgramsId = schedule.ProgramId,
                ExamTypeId = schedule.ExamTypeId,
                CollegeId = collegeId,
                Year = yearStr,
                Part = partStr,
                RegistrationNumber = student.RegistrationNumber,
                SymbolNumber = student.SymbolNumber ?? "",
                DateOfBirthBs = student.DateOfBirthBs ?? "",
                Sex = student.Sex,
                StudentName = student.StudentName,
                TheoryObtainedMarks = FormatMarks(theoryObtained),
                InternalObtainedMarks = FormatMarks(internalObtained),
                PracticalObtainedMarks = FormatMarks(practicalObtained),
                TotalObtainedMarks = FormatMarks(totalObtained),
                TotalGradePoints = totalGradePoints?.ToString("F2"),
                Gpa = student.GPA?.ToString("F2"),
                Result = student.Result,
                ResultRecordMasterId = 0,
                ExamScheduleId = examScheduleId,
                CreatedDate = DateTime.UtcNow
            };

            records.Add(record);
        }

        context.ResultRecords.AddRange(records);
        await context.SaveChangesAsync();

        return new PublishResultsResultDto
        {
            Success = true,
            RecordsCreated = records.Count,
            Message = $"Successfully published {records.Count} result records for {preview.ExamScheduleName} - {preview.CollegeName}."
        };
    }

    private async Task<List<Domain.Entities.Exams.ExamRegistration>> LoadRegistrationsWithResults(int examScheduleId, int collegeId)
    {
        return await context.ExamRegistrations
            .AsNoTracking()
            .Include(r => r.ExamSubjectResults!)
                .ThenInclude(esr => esr.SubjectOffering!)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Include(r => r.ExamSubjectResults!)
                .ThenInclude(esr => esr.ExamType)
            .Include(r => r.SemesterEnrollment!)
                .ThenInclude(se => se!.StudentAdmission)
            .Include(r => r.ApplicationVoucher)
            .Where(r => r.ExamScheduleId == examScheduleId && r.CollegeId == collegeId && r.IsActive)
            .ToListAsync();
    }

    private async Task<string> GetStudentNameAsync(Domain.Entities.Exams.ExamRegistration reg)
    {
        var admission = reg.SemesterEnrollment?.StudentAdmission;
        if (admission != null)
            return admission.FirstName.GetFullName(admission.LastName);

        if (reg.ApplicationVoucher?.StudentName is { Length: > 0 } voucherName)
            return voucherName;

        if (reg.SemesterEnrollment?.StudentAdmission?.AppUserId is { Length: > 0 } userId)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(user)) return user;
        }

        return "";
    }

    private string? GetStudentDobAsync(Domain.Entities.Exams.ExamRegistration reg)
    {
        var admission = reg.SemesterEnrollment?.StudentAdmission;
        if (admission?.DateOfBirthBS is { Length: > 0 } dob)
            return dob;

        return null;
    }

    private string? GetStudentSex(Domain.Entities.Exams.ExamRegistration reg)
    {
        var admission = reg.SemesterEnrollment?.StudentAdmission;
        if (admission?.GenderId.HasValue == true)
        {
            return context.Genders
                .AsNoTracking()
                .Where(g => g.Id == admission.GenderId.Value)
                .Select(g => g.GenderName)
                .FirstOrDefault();
        }
        return null;
    }

    private async Task<string?> GetRegistrationNumberAsync(Domain.Entities.Exams.ExamRegistration reg)
    {
        var admissionId = reg.SemesterEnrollment?.StudentAdmissionId;
        if (admissionId.HasValue)
        {
            var regNum = await context.StudentRegistrations
                .AsNoTracking()
                .Where(sr => sr.StudentAdmissionId == admissionId.Value)
                .Select(sr => sr.RegistrationNumber)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(regNum)) return regNum;
        }
        return null;
    }

    private static string FormatMarks(float? marks)
    {
        if (!marks.HasValue) return "";
        return marks.Value % 1 == 0 ? ((int)marks.Value).ToString() : marks.Value.ToString("F1");
    }

    private static string RomanNumeral(int value)
    {
        return value switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            _ => value.ToString()
        };
    }
}
