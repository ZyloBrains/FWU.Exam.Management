using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CollegeAdminMarksServiceTests
{
    private const string AdminUserId = "admin-1";

    private static TestUserContext CollegeAdmin() =>
        new TestUserContext().WithUser(AdminUserId, null, TestData.CollegeId, [], [Role.CollegeAdmin]);

    private static CollegeAdminMarksService CreateService(TestDb db)
    {
        var userContext = MarksEntryTestSeed.CollegeAdmin();
        return new CollegeAdminMarksService(
            db.Context,
            userContext,
            new CollegeAdminSubjectAssignmentService(db.Context, userContext),
            new GradeCalculationService(db.Context),
            new TestAuditLogWriter());
    }

    private static CollegeAdminMarksService Service(AppDbContext ctx)
    {
        var user = CollegeAdmin();
        return new CollegeAdminMarksService(
            ctx,
            user,
            new CollegeAdminSubjectAssignmentService(ctx, user),
            new GradeCalculationService(ctx),
            new TestAuditLogWriter());
    }

    private static void SeedInternalReview(AppDbContext ctx)
    {
        MarksEntryTestSeed.SeedPartialContext(ctx);
        MarksEntryTestSeed.AddOldTheoryOffering(ctx);
        MarksEntryTestSeed.AddPartialSchedule(ctx);
        MarksEntryTestSeed.AddReExamStudents(ctx);

        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(1, 1,
            MarksEntryTestSeed.CurrentOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: true, practical: true));
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
            MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: true, practical: true));
    }

    private static void SeedMarksEntry(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.CollegePrograms.Add(new CollegeProgram
        {
            Id = 1,
            TenantId = TestData.TenantId,
            CollegeId = TestData.CollegeId,
            ProgramId = TestData.ProgramId,
            IsActive = true
        });

        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), DateTime.UtcNow.AddDays(45)));

        ctx.ExamRegistrations.Add(Registration(1));
        ctx.ExamRegistrations.Add(Registration(2));

        ctx.ExamSubjectResults.Add(new ExamSubjectResult
        {
            Id = 1,
            TenantId = TestData.TenantId,
            ExamRegistrationId = 1,
            ExamTypeId = TestData.Regular,
            SubjectOfferingId = 101,
            ExamScheduleId = 21,
            ObtainedMarksTheoryInternal = 26f,
            IsActive = true,
            IsSubmitted = false
        });
    }

    private static ExamRegistration Registration(int id) => new()
    {
        Id = id,
        TenantId = TestData.TenantId,
        AcademicYearId = TestData.AcademicYearId,
        CollegeId = TestData.CollegeId,
        ProgramsId = TestData.ProgramId,
        ExamScheduleId = 21,
        RegistrationDate = DateTime.UtcNow,
        Status = RegistrationStatus.Registered,
        IsActive = true,
        IsAppliedByStudent = true
    };

    [Fact]
    public async Task GetSubjects_OnPartialSchedule_OnlySurfacesInternalOfferings()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddOldTheoryOffering(ctx);
            MarksEntryTestSeed.AddOldPracticalOffering(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx);
        });

        var subjects = await CreateService(db).GetSubjectsByScheduleAsync(
            MarksEntryTestSeed.ScheduleId, TestData.CollegeId);

        // Only the internal-capable current offering: old theory (401) and old
        // practical (402) offerings carry no internal marks and must not surface.
        Assert.Equal([MarksEntryTestSeed.CurrentOfferingId],
            subjects.Select(s => s.Id).ToArray());
    }

    [Fact]
    public async Task GetStudentsForInternalMarks_OnPartialSchedule_OnlyListsStudentsPinnedToThatOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedInternalReview);

        var current = await CreateService(db).GetStudentsForInternalMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.CurrentOfferingId, TestData.CollegeId);
        Assert.Equal([1], current.Students.Select(s => s.ExamRegistrationId).ToArray());
        var currentRow = Assert.Single(current.Students);
        Assert.Equal("REG1", currentRow.RegistrationNumber);
        Assert.Equal("2080", currentRow.AcademicYearName);

        var old = await CreateService(db).GetStudentsForInternalMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldTheoryOfferingId, TestData.CollegeId);
        Assert.Equal([2], old.Students.Select(s => s.ExamRegistrationId).ToArray());
        var oldRow = Assert.Single(old.Students);
        Assert.Equal("REG2", oldRow.RegistrationNumber);
        Assert.Equal("2080", oldRow.AcademicYearName);
    }

    [Fact]
    public async Task SaveInternal_OnPartialSchedule_UpdatesPinnedRowInPlaceWithBatchScheme()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedInternalReview);

        var result = await CreateService(db).SaveCollegeMarksBulkAsync(
            new BulkMarksSaveDto
            {
                SubjectOfferingId = MarksEntryTestSeed.OldTheoryOfferingId,
                ExamScheduleId = MarksEntryTestSeed.ScheduleId,
                Students =
                [
                    new StudentMarksRowDto { ExamRegistrationId = 2, TheoryInternal = 10 }
                ]
            },
            TestData.CollegeId,
            MarksEntryTestSeed.AdminUserId);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.SavedCount);

        var rows = await db.Context.ExamSubjectResults
            .Where(esr => esr.ExamRegistrationId == 2
                       && esr.ExamScheduleId == MarksEntryTestSeed.ScheduleId
                       && esr.IsActive)
            .ToListAsync();

        var row = Assert.Single(rows);
        Assert.Equal(MarksEntryTestSeed.OldTheoryOfferingId, row.SubjectOfferingId);
        Assert.Equal(10, row.ObtainedMarksTheoryInternal);
        Assert.Equal(MarksEntryTestSeed.BatchSchemeId, row.GradingSchemeId);
        Assert.Equal("B", row.GradeLetter);
    }

    [Fact]
    public async Task GetStudentsForInternalMarks_OnPartialSchedule_IncludesPendingRegistrationsWithPinnedRows()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddOldTheoryOffering(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx);
            MarksEntryTestSeed.AddReExamStudents(ctx);

            // A re-exam form stays Pending until the college verifies it, but the
            // student already holds a pinned marks row and must appear in the list.
            var er4 = TestData.ExamRegistration(4, MarksEntryTestSeed.ScheduleId, 1);
            er4.SemesterEnrollmentId = 1;
            ctx.ExamRegistrations.Add(er4);

            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(1, 1,
                MarksEntryTestSeed.CurrentOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true, practical: true));
            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true, practical: true));
            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(10, 4,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true, practical: true));
        });

        var old = await CreateService(db).GetStudentsForInternalMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldTheoryOfferingId, TestData.CollegeId);

        Assert.Equal([2, 4], old.Students.Select(s => s.ExamRegistrationId).OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task GetMarksEntryView_WhenUserFullNameMissesMiddleName_UsesAdmissionFirstMiddleLast()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddOldTheoryOffering(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx);

            const string userId = "stu-user-9";
            var sr = TestData.StudentRegistration(9, "stu9@test.com");
            sr.AcademicYearId = MarksEntryTestSeed.OldYearId;
            ctx.StudentRegistrations.Add(sr);

            // A legacy/stale user snapshot that omits the middle name.
            var user = TestData.User(userId, "stu9@test.com");
            user.FullName = "Ram Thapa";
            ctx.Users.Add(user);

            var admission = TestData.Admission(9, userId);
            admission.FirstName = "Ram";
            admission.MiddleName = "Bahadur";
            admission.LastName = "Thapa";
            ctx.StudentAdmissions.Add(admission);

            ctx.Set<SemesterEnrollment>().Add(TestData.Enrollment(9, 9, 1));
            ctx.ApplicationVouchers.Add(TestData.Voucher(9, 9, MarksEntryTestSeed.ScheduleId));

            var er = TestData.ExamRegistration(9, MarksEntryTestSeed.ScheduleId, 9);
            er.SemesterEnrollmentId = 9;
            er.Status = RegistrationStatus.Registered;
            er.ExamRollNumber = "R109";
            ctx.ExamRegistrations.Add(er);

            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(90, 9,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true, practical: false));
        });

        var view = await CreateService(db).GetMarksEntryViewAsync(
            MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.AdminUserId);

        var row = Assert.Single(view.Students);
        Assert.Equal("Ram Bahadur Thapa", row.StudentName);
    }

    [Fact]
    public async Task SaveInternalMarks_PartialSubmit_KeepsPreviouslySavedMarks()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedMarksEntry);
        var service = Service(db.Context);

        var result = await service.SaveInternalMarksAsync(new InternalMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = 21,
            SubjectOfferingId = 101,
            SubmitAll = true,
            Students =
            {
                new StudentInternalMarksRowDto { ExamRegistrationId = 1, TheoryInternal = null },
                new StudentInternalMarksRowDto { ExamRegistrationId = 2, TheoryInternal = 30f }
            }
        });

        Assert.True(result.Success);

        var first = await db.Context.ExamSubjectResults
            .SingleAsync(esr => esr.ExamRegistrationId == 1);
        var second = await db.Context.ExamSubjectResults
            .SingleAsync(esr => esr.ExamRegistrationId == 2);

        Assert.Equal(26f, first.ObtainedMarksTheoryInternal);
        Assert.Equal(30f, second.ObtainedMarksTheoryInternal);
        Assert.True(first.IsSubmitted);
        Assert.True(second.IsSubmitted);
    }

    [Fact]
    public async Task SaveInternalMarks_Edit_OverwritesExistingValue()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedMarksEntry);
        var service = Service(db.Context);

        var result = await service.SaveInternalMarksAsync(new InternalMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = 21,
            SubjectOfferingId = 101,
            SubmitAll = true,
            Students =
            {
                new StudentInternalMarksRowDto { ExamRegistrationId = 1, TheoryInternal = 28f }
            }
        });

        Assert.True(result.Success);

        var first = await db.Context.ExamSubjectResults
            .SingleAsync(esr => esr.ExamRegistrationId == 1);

        Assert.Equal(28f, first.ObtainedMarksTheoryInternal);
    }
}