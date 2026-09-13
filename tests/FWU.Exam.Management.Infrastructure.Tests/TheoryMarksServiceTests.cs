using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class TheoryMarksServiceTests
{
    private static TheoryMarksService CreateService(TestDb db) =>
        new(db.Context, MarksEntryTestSeed.CollegeAdmin(), new GradeCalculationService(db.Context));

    private static void SeedRichReview(AppDbContext ctx)
    {
        MarksEntryTestSeed.SeedPartialContext(ctx);
        MarksEntryTestSeed.AddOldTheoryOffering(ctx);
        MarksEntryTestSeed.AddPartialSchedule(ctx);
        MarksEntryTestSeed.AddReExamStudents(ctx);

        // Old-cohort students pinned their rows to the cohort's curriculum:
        // ER 1 → schedule-year offering (101), ER 2 → old offering (401).
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(1, 1,
            MarksEntryTestSeed.CurrentOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: true));
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
            MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: true));
    }

    [Fact]
    public async Task GetSubjects_OnPartialSchedule_SurfacesRegisteredOldCurriculumOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedRichReview);

        var subjects = await CreateService(db).GetSubjectsByScheduleAsync(MarksEntryTestSeed.ScheduleId, TestData.CollegeId);

        var names = subjects.ToDictionary(s => s.Id);
        Assert.True(names.ContainsKey(MarksEntryTestSeed.CurrentOfferingId));
        Assert.True(names.ContainsKey(MarksEntryTestSeed.OldTheoryOfferingId));

        var old = names[MarksEntryTestSeed.OldTheoryOfferingId];
        Assert.Equal("Old Subject", old.Name);
        Assert.True(old.IsStaleCohort);
        Assert.Equal("Old 2080", old.CurriculumVersionName);

        var current = names[MarksEntryTestSeed.CurrentOfferingId];
        Assert.False(current.IsStaleCohort);
        Assert.Equal("New 2081", current.CurriculumVersionName);
    }

    [Fact]
    public async Task GetStudents_OnPartialSchedule_OnlyListsStudentsPinnedToThatOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedRichReview);

        var current = await CreateService(db).GetStudentsForTheoryMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.CurrentOfferingId, TestData.CollegeId);
        Assert.Equal([1], current.Students.Select(s => s.ExamRegistrationId).ToArray());
        var currentRow = Assert.Single(current.Students);
        Assert.Equal("REG1", currentRow.RegistrationNumber);
        Assert.Equal("2080", currentRow.AcademicYearName);

        var old = await CreateService(db).GetStudentsForTheoryMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldTheoryOfferingId, TestData.CollegeId);
        Assert.Equal([2], old.Students.Select(s => s.ExamRegistrationId).ToArray());
        var oldRow = Assert.Single(old.Students);
        Assert.Equal("REG2", oldRow.RegistrationNumber);
        Assert.Equal("2080", oldRow.AcademicYearName);
    }

    [Fact]
    public async Task SaveTheory_OnPartialSchedule_UpdatesPinnedRowInPlaceWithBatchScheme()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedRichReview);

        var result = await CreateService(db).SaveTheoryMarksAsync(new TheoryMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = MarksEntryTestSeed.ScheduleId,
            SubjectOfferingId = MarksEntryTestSeed.OldTheoryOfferingId,
            Students =
            [
                new StudentTheoryMarksRowDto { ExamRegistrationId = 2, Theory = 25 }
            ]
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.SavedCount);

        var rows = await db.Context.ExamSubjectResults
            .Where(esr => esr.ExamRegistrationId == 2
                       && esr.ExamScheduleId == MarksEntryTestSeed.ScheduleId
                       && esr.IsActive)
            .ToListAsync();

        // Updated in place under the pinned offering — never duplicated.
        var row = Assert.Single(rows);
        Assert.Equal(MarksEntryTestSeed.OldTheoryOfferingId, row.SubjectOfferingId);
        Assert.Equal(25, row.ObtainedMarksTheory);
        Assert.Equal(MarksEntryTestSeed.BatchSchemeId, row.GradingSchemeId);
        // Graded with the batch scheme, not the schedule-year scheme.
        Assert.Equal("B", row.GradeLetter);
    }

    [Fact]
    public async Task GetStudents_OnRegularSchedule_StillListsStudentsWithoutRows()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx, examTypeId: TestData.Regular);
            MarksEntryTestSeed.AddReExamStudents(ctx);
        });

        var rows = await CreateService(db).GetStudentsForTheoryMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.CurrentOfferingId, TestData.CollegeId);

        Assert.Equal([1, 2, 3], rows.Students.Select(s => s.ExamRegistrationId).OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task GetStudents_OnPartialSchedule_IncludesPendingRegistrationsWithPinnedRows()
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
                theory: true));
            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true));
            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(10, 4,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true));
        });

        var old = await CreateService(db).GetStudentsForTheoryMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldTheoryOfferingId, TestData.CollegeId);

        Assert.Equal([2, 4], old.Students.Select(s => s.ExamRegistrationId).OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task GetStudents_WhenUserFullNameMissesMiddleName_UsesAdmissionFirstMiddleLast()
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
            er.ExamRollNumber = "R109";
            er.SymbolNumber = "SYM109";
            ctx.ExamRegistrations.Add(er);

            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(90, 9,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true));
        });

        var view = await CreateService(db).GetStudentsForTheoryMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldTheoryOfferingId, TestData.CollegeId);

        var row = Assert.Single(view.Students);
        Assert.Equal("Ram Bahadur Thapa", row.StudentName);
    }
}