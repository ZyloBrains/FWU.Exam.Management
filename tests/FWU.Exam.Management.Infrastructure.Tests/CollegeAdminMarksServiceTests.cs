using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CollegeAdminMarksServiceTests
{
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
}