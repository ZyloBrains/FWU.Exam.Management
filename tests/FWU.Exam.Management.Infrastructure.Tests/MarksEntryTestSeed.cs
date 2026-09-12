using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Infrastructure.Tests;

/// <summary>
/// Shared seeding for marks-entry re-exam tests. Partial schedules are sat by
/// older cohorts, so the fixtures model a schedule-year curriculum version
/// ("New 2081") plus an old-cohort version ("Old 2080") with two grading schemes
/// — the batch scheme only resolves for the old cohort's academic year.
/// </summary>
internal static class MarksEntryTestSeed
{
    public const string AdminUserId = "marks-admin";
    public const string AdminEmail = "marksadmin@test.com";

    public const int ScheduleId = 21;
    public const int OldYearId = 2;
    public const int NewVersionId = 6;
    public const int OldVersionId = 5;
    public const int ScheduleSchemeId = 200;
    public const int BatchSchemeId = 201;
    public const int CurrentOfferingId = 101;
    public const int OldTheoryOfferingId = 401;
    public const int OldPracticalOfferingId = 402;

    public static TestUserContext CollegeAdmin() =>
        new TestUserContext().WithUser(AdminUserId, null, TestData.CollegeId, [], [Role.CollegeAdmin]);

    public static GradingScheme Scheme(int id, string letter)
    {
        var scheme = new GradingScheme { Id = id, Name = $"Scheme {id}", IsActive = true };
        scheme.GradeDefinitions.Add(new GradeDefinition
        {
            Id = id * 100 + 1,
            GradeLetter = letter,
            MinPercentage = 0,
            MaxPercentage = 100,
            GradePoint = letter == "B" ? 3.0m : 3.5m,
            IsPass = true,
            DisplayOrder = 1,
            Remark = "Pass",
            GradingSchemeId = id
        });
        return scheme;
    }

    /// <summary>Base for every marks test: college-program link, college-faculty (query
    /// filter requirement), curriculum versions, grading schemes, and the schedule.</summary>
    public static void SeedPartialContext(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);
        TestData.SeedCollegeForStandardTenant(ctx);

        ctx.CollegePrograms.Add(new CollegeProgram
        {
            TenantId = TestData.TenantId,
            CollegeId = TestData.CollegeId,
            ProgramId = TestData.ProgramId,
            IsActive = true
        });

        ctx.AcademicYears.Add(TestData.AcademicYear(OldYearId, "2080"));

        // Schedule year 2081 (AcademicYearId 1) resolves to the "New 2081" version;
        // the old cohort's version is effective from 2080 (AcademicYearId 2).
        ctx.CurriculumVersions!.Add(new CurriculumVersion
        {
            Id = OldVersionId,
            TenantId = TestData.TenantId,
            Name = "Old 2080",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = OldYearId,
            IsActive = true
        });
        ctx.CurriculumVersions!.Add(new CurriculumVersion
        {
            Id = NewVersionId,
            TenantId = TestData.TenantId,
            Name = "New 2081",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TestData.AcademicYearId,
            IsActive = true
        });

        ctx.GradingSchemes.Add(Scheme(ScheduleSchemeId, "S"));
        ctx.GradingSchemes.Add(Scheme(BatchSchemeId, "B"));
        ctx.GradingSchemePrograms.Add(new GradingSchemeProgram
        {
            GradingSchemeId = ScheduleSchemeId,
            ProgramId = TestData.ProgramId,
            AcademicYearId = TestData.AcademicYearId,
            IsActive = true
        });
        ctx.GradingSchemePrograms.Add(new GradingSchemeProgram
        {
            GradingSchemeId = BatchSchemeId,
            ProgramId = TestData.ProgramId,
            AcademicYearId = OldYearId,
            IsActive = true
        });

        // The schedule-year offering belongs to the current curriculum version.
        var current = ctx.SubjectOfferings.Local.First(so => so.Id == CurrentOfferingId);
        current.CurriculumVersionId = NewVersionId;
    }

    /// <summary>Old-curriculum theory offering (semester 1, HasTheory, no internal).</summary>
    public static void AddOldTheoryOffering(AppDbContext ctx)
    {
        ctx.SubjectCatalogs.Add(new SubjectCatalog
        {
            Id = 2,
            TenantId = TestData.TenantId,
            SubjectCode = "OLD1",
            SubjectName = "Old Subject",
            SubjectTypeId = 1,
            IsActive = true
        });
        ctx.SubjectOfferings.Add(new SubjectOffering
        {
            Id = OldTheoryOfferingId,
            TenantId = TestData.TenantId,
            SubjectCatalogId = 2,
            ProgramId = TestData.ProgramId,
            SemesterId = 1,
            IsActive = true,
            IsCompulsory = true,
            DisplayOrder = 1,
            HasTheory = true,
            HasPractical = false,
            HasInternal = false,
            TheoryFullMarks = 50,
            TheoryPassMarks = 20,
            CurriculumVersionId = OldVersionId
        });
    }

    /// <summary>Old-curriculum practical-only offering (semester 1).</summary>
    public static void AddOldPracticalOffering(AppDbContext ctx)
    {
        ctx.SubjectOfferings.Add(new SubjectOffering
        {
            Id = OldPracticalOfferingId,
            TenantId = TestData.TenantId,
            SubjectCatalogId = 1,
            ProgramId = TestData.ProgramId,
            SemesterId = 1,
            IsActive = true,
            IsCompulsory = true,
            DisplayOrder = 1,
            HasTheory = false,
            HasPractical = true,
            HasInternal = false,
            PracticalFullMarks = 40,
            PracticalPassMarks = 16,
            CurriculumVersionId = null
        });
    }

    public static void AddPartialSchedule(AppDbContext ctx, int scheduleId = ScheduleId,
        int examTypeId = TestData.Partial, int programId = TestData.ProgramId, int semesterInstanceId = 1)
    {
        ctx.ExamSchedules.Add(TestData.Schedule(scheduleId, semesterInstanceId, examTypeId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), null, programId));
    }

    /// <summary>Three registrations for the schedule: id 1 and 2 belong to old-cohort
    /// students (batch year 2080), id 3 is a leftover that never logged a marks row.</summary>
    public static void AddReExamStudents(AppDbContext ctx, int scheduleId = ScheduleId)
    {
        var sr1 = TestData.StudentRegistration(1, "stu1@test.com");
        sr1.StudentAdmissionId = 1;
        sr1.AcademicYearId = OldYearId;
        var sr2 = TestData.StudentRegistration(2, "stu2@test.com");
        sr2.StudentAdmissionId = 2;
        sr2.AcademicYearId = OldYearId;
        ctx.StudentRegistrations.AddRange(sr1, sr2);

        ctx.Users.Add(TestData.User("stu-user-1", "stu1@test.com"));
        ctx.Users.Add(TestData.User("stu-user-2", "stu2@test.com"));

        ctx.StudentAdmissions.Add(TestData.Admission(1, "stu-user-1"));
        ctx.StudentAdmissions.Add(TestData.Admission(2, "stu-user-2"));

        ctx.Set<SemesterEnrollment>().Add(TestData.Enrollment(1, 1, 1));
        ctx.Set<SemesterEnrollment>().Add(TestData.Enrollment(2, 2, 1));

        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, scheduleId));
        ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, scheduleId));

        var er1 = TestData.ExamRegistration(1, scheduleId, 1);
        er1.Status = RegistrationStatus.CollegeVerified;
        er1.SemesterEnrollmentId = 1;
        er1.ExamRollNumber = "R101";
        er1.SymbolNumber = "SYM101";

        var er2 = TestData.ExamRegistration(2, scheduleId, 2);
        er2.Status = RegistrationStatus.CollegeVerified;
        er2.SemesterEnrollmentId = 2;
        er2.ExamRollNumber = "R102";
        er2.SymbolNumber = "SYM102";

        var er3 = TestData.ExamRegistration(3, scheduleId, 1);
        er3.Status = RegistrationStatus.CollegeVerified;
        er3.ExamRollNumber = "R103";
        er3.SymbolNumber = "SYM103";

        ctx.ExamRegistrations.AddRange(er1, er2, er3);
    }

    public static ExamSubjectResult PinnedResult(int id, int examRegistrationId, int subjectOfferingId,
        int scheduleId, int examTypeId, bool theory = false, bool practical = false) => new()
    {
        Id = id,
        TenantId = TestData.TenantId,
        ExamRegistrationId = examRegistrationId,
        SubjectOfferingId = subjectOfferingId,
        ExamScheduleId = scheduleId,
        ExamTypeId = examTypeId,
        IsTheoryRegistered = theory,
        IsPracticalRegistered = practical,
        IsActive = true,
        IsSubmitted = false
    };
}