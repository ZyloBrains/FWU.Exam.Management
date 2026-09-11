using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class MarksEntryCrossCohortTests
{
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

    private static CollegeAdminMarksService CreateInternalService(TestDb db, IUserContext uc) =>
        new(db.Context, uc, new NullAssignmentService(), new GradeCalculationService(db.Context), new NullAuditLogWriter());

    private static TheoryMarksService CreateTheoryService(TestDb db, IUserContext uc) =>
        new(db.Context, uc, new GradeCalculationService(db.Context));

    private static PracticalMarksService CreatePracticalService(TestDb db, IUserContext uc) =>
        new(db.Context, uc, new GradeCalculationService(db.Context));

    private static TestUserContext SuperAdmin() =>
        new TestUserContext().WithUser(null, null, null, [], [Role.SuperAdmin]);

    // Schedule 30 = 1st-sem partial opened in AY 2. Two versions exist for the
    // same subject (V90 "Batch 2025", V91 "New 2026"). Student 1 is from batch
    // 2025 (AY 1, offering 301) and student 2 from the newer batch (AY 2,
    // offering 401). Both ticked their own version's offering on the partial form.
    private static void SeedCrossCohortMarks(AppDbContext ctx)
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

        ctx.AcademicYears.Add(new AcademicYear
        {
            Id = 2,
            TenantId = TestData.TenantId,
            AcademicYearCode = "2082",
            AcademicYearName = "2082",
            IsActive = true
        });

        // Schedule instance belongs to the newer AY 2, but the partial students
        // came from batches of both AY 1 and AY 2.
        ctx.SemesterInstances.Add(new SemesterInstance
        {
            Id = 20,
            TenantId = TestData.TenantId,
            AcademicYearId = 2,
            SemesterId = 1,
            ProgramId = TestData.ProgramId,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(5)
        });

        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 90,
            TenantId = TestData.TenantId,
            Name = "Batch 2025",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = 1,
            IsActive = true
        });
        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 91,
            TenantId = TestData.TenantId,
            Name = "New 2026",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = 2,
            IsActive = true
        });

        var oldOffering = TestData.Offering(301, 1, TestData.ProgramId);
        oldOffering.Id = 301;
        oldOffering.CurriculumVersionId = 90;
        oldOffering.HasPractical = true;
        ctx.SubjectOfferings.Add(oldOffering);

        var newOffering = TestData.Offering(401, 1, TestData.ProgramId);
        newOffering.Id = 401;
        newOffering.CurriculumVersionId = 91;
        newOffering.HasPractical = true;
        ctx.SubjectOfferings.Add(newOffering);

        ctx.ExamSchedules.Add(TestData.Schedule(30, 20, TestData.Partial, FutureDate, DateTime.UtcNow.AddMonths(2)));
        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, FutureDate, DateTime.UtcNow.AddMonths(2)));

        var oldBatchStudent = TestData.StudentRegistration(1, "old@test.com");
        oldBatchStudent.AcademicYearId = 1;
        ctx.StudentRegistrations.Add(oldBatchStudent);

        var newBatchStudent = TestData.StudentRegistration(2, "new@test.com");
        newBatchStudent.AcademicYearId = 2;
        ctx.StudentRegistrations.Add(newBatchStudent);

        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 30));
        ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, 30));

        var oldEr = TestData.ExamRegistration(1, 30, 1);
        oldEr.Status = RegistrationStatus.CollegeVerified;
        oldEr.IsSupplementary = true;
        ctx.ExamRegistrations.Add(oldEr);

        var newEr = TestData.ExamRegistration(2, 30, 2);
        newEr.Status = RegistrationStatus.CollegeVerified;
        newEr.IsSupplementary = true;
        ctx.ExamRegistrations.Add(newEr);

        ctx.ExamSubjectResults.Add(TestData.Result(100, 1, 301, TestData.Partial, grade: null, examScheduleId: 30));
        ctx.ExamSubjectResults.Add(TestData.Result(101, 2, 401, TestData.Partial, grade: null, examScheduleId: 30));
    }

    [Fact]
    public async Task GetSubjectsByScheduleAsync_CrossCohortPartial_ReturnsBothVersions()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        var internalSubjects = await CreateInternalService(db, SuperAdmin()).GetSubjectsByScheduleAsync(30, TestData.CollegeId);
        Assert.Contains(internalSubjects, s => s.Id == 301);
        Assert.Contains(internalSubjects, s => s.Id == 401);
        Assert.Contains(internalSubjects, s => s.Id == 101);

        var theorySubjects = await CreateTheoryService(db, SuperAdmin()).GetSubjectsByScheduleAsync(30, TestData.CollegeId);
        Assert.Contains(theorySubjects, s => s.Id == 301);
        Assert.Contains(theorySubjects, s => s.Id == 401);

        var practicalSubjects = await CreatePracticalService(db, SuperAdmin()).GetSubjectsByScheduleAsync(30, TestData.CollegeId);
        Assert.Contains(practicalSubjects, s => s.Id == 301);
        Assert.Contains(practicalSubjects, s => s.Id == 401);
        Assert.DoesNotContain(practicalSubjects, s => s.Id == 101); // unversioned offering has no practical leg
    }

    [Fact]
    public async Task GetSubjectsByScheduleAsync_CrossCohortPartial_AppendsVersionLabels()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        var subject = (await CreateInternalService(db, SuperAdmin()).GetSubjectsByScheduleAsync(30, TestData.CollegeId))
            .Single(s => s.Id == 301);

        Assert.Equal("Subject 1 (Batch 2025)", subject.Name);
    }

    [Fact]
    public async Task GetSubjectsByScheduleAsync_RegularSchedule_StillResolvesSingleVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        // The regular schedule (semester instance in AY 1) resolves version 90 only.
        var subjects = await CreateInternalService(db, SuperAdmin()).GetSubjectsByScheduleAsync(21, TestData.CollegeId);

        var subject = Assert.Single(subjects);
        Assert.Equal(301, subject.Id);
    }

    [Fact]
    public async Task GetStudentsForInternalMarksAsync_CrossCohortPartial_ByOlderBatchOffering_ReturnsOnlyThatBatch()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        var grid = await CreateInternalService(db, SuperAdmin())
            .GetStudentsForInternalMarksAsync(30, 301, TestData.CollegeId);

        Assert.True(grid.IsReExamSchedule);
        var row = Assert.Single(grid.Students);
        Assert.Equal(1, row.ExamRegistrationId);
        Assert.True(row.IsPartial);
    }

    [Fact]
    public async Task GetStudentsForTheoryMarksAsync_CrossCohortPartial_ByOlderBatchOffering_LoadsBatchRows()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        var grid = await CreateTheoryService(db, SuperAdmin())
            .GetStudentsForTheoryMarksAsync(30, 301, TestData.CollegeId);

        // The older-batch offering is accepted on the re-exam schedule and the
        // batch-2025 student's row is present (the other batch shows a blank row
        // because it did not tick theory on 301).
        var row = grid.Students.Single(s => s.ExamRegistrationId == 1);
        Assert.Equal(100, row.ExamSubjectResultId);
        Assert.Equal(2, grid.Students.Count);
        Assert.Null(grid.Students.Single(s => s.ExamRegistrationId == 2).ExamSubjectResultId);
    }

    [Fact]
    public async Task GetStudentsForPracticalMarksAsync_CrossCohortPartial_ByNewerBatchOffering_LoadsBatchRows()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);

        var grid = await CreatePracticalService(db, SuperAdmin())
            .GetStudentsForPracticalMarksAsync(30, 401, TestData.CollegeId);

        var row = grid.Students.Single(s => s.ExamRegistrationId == 2);
        Assert.Equal(101, row.ExamSubjectResultId);
        Assert.Equal(2, grid.Students.Count);
    }

    [Fact]
    public async Task SaveInternalMarksAsync_CrossCohortPartial_OlderBatchOffering_Saves()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);
        var service = CreateInternalService(db, SuperAdmin());

        var result = await service.SaveInternalMarksAsync(new InternalMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = 30,
            SubjectOfferingId = 301,
            Students =
            [
                new StudentInternalMarksRowDto
                {
                    ExamRegistrationId = 1,
                    ExamSubjectResultId = 100,
                    TheoryInternal = 45
                }
            ]
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(1, result.SavedCount);

        var saved = db.Context.ExamSubjectResults.Single(e => e.Id == 100);
        Assert.Equal(45, saved.ObtainedMarksTheoryInternal);
    }

    [Fact]
    public async Task SaveTheoryMarksAsync_CrossCohortPartial_OlderBatchOffering_Saves()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);
        var service = CreateTheoryService(db, SuperAdmin());

        var result = await service.SaveTheoryMarksAsync(new TheoryMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = 30,
            SubjectOfferingId = 301,
            Students =
            [
                new StudentTheoryMarksRowDto
                {
                    ExamRegistrationId = 1,
                    ExamSubjectResultId = 100,
                    Theory = 55
                }
            ]
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(1, result.SavedCount);

        var saved = db.Context.ExamSubjectResults.Single(e => e.Id == 100);
        Assert.Equal(55, saved.ObtainedMarksTheory);
    }

    [Fact]
    public async Task SavePracticalMarksAsync_CrossCohortPartial_NewerBatchOffering_Saves()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedCrossCohortMarks);
        var service = CreatePracticalService(db, SuperAdmin());

        var result = await service.SavePracticalMarksAsync(new PracticalMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = 30,
            SubjectOfferingId = 401,
            Students =
            [
                new StudentPracticalMarksRowDto
                {
                    ExamRegistrationId = 2,
                    ExamSubjectResultId = 101,
                    Practical = 60
                }
            ]
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(1, result.SavedCount);

        var saved = db.Context.ExamSubjectResults.Single(e => e.Id == 101);
        Assert.Equal(60, saved.ObtainedMarksPractical);
    }

    private sealed class NullAssignmentService : ICollegeAdminSubjectAssignmentService
    {
        public Task<List<CollegeAdminSubjectAssignment>> GetAssignmentsAsync(string? collegeAdminUserId = null)
            => Task.FromResult(new List<CollegeAdminSubjectAssignment>());
        public Task<CollegeAdminSubjectAssignment?> GetByIdAsync(int id) => Task.FromResult<CollegeAdminSubjectAssignment?>(null);
        public Task CreateAsync(CollegeAdminSubjectAssignment assignment) => Task.CompletedTask;
        public Task UpdateAsync(CollegeAdminSubjectAssignment assignment) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<List<int>> GetAssignedSubjectOfferingIdsAsync(string collegeAdminUserId) => Task.FromResult(new List<int>());
        public Task<List<int>> GetAssignedExamScheduleIdsAsync(string collegeAdminUserId) => Task.FromResult(new List<int>());
        public Task<bool> IsCollegeAdminAssignedToSubjectAsync(string collegeAdminUserId, int subjectOfferingId) => Task.FromResult(true);
    }

    private sealed class NullAuditLogWriter : IAuditLogWriter
    {
        public Task LogAsync(
            string activityType,
            string? description = null,
            object? details = null,
            string severity = AuditSeverity.Info,
            string? entityName = null,
            string? entityId = null,
            string? actorUserId = null)
            => Task.CompletedTask;
    }
}