using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class PracticalMarksServiceTests
{
    private static PracticalMarksService CreateService(TestDb db) =>
        new(db.Context, MarksEntryTestSeed.CollegeAdmin(), new GradeCalculationService(db.Context));

    private static void SeedPracticalReview(AppDbContext ctx)
    {
        MarksEntryTestSeed.SeedPartialContext(ctx);
        MarksEntryTestSeed.AddOldPracticalOffering(ctx);
        MarksEntryTestSeed.AddPartialSchedule(ctx);
        MarksEntryTestSeed.AddReExamStudents(ctx);

        // ER 1 registered the current offering for the theory paper only; ER 2
        // registered the old cohort's practical offering.
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(1, 1,
            MarksEntryTestSeed.CurrentOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: true, practical: false));
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
            MarksEntryTestSeed.OldPracticalOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
            theory: false, practical: true));
    }

    [Fact]
    public async Task GetSubjects_OnPartialSchedule_SurfacesRegisteredPracticalOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedPracticalReview);

        var subjects = await CreateService(db).GetSubjectsByScheduleAsync(MarksEntryTestSeed.ScheduleId, TestData.CollegeId);

        var old = Assert.Single(subjects, s => s.Id == MarksEntryTestSeed.OldPracticalOfferingId);
        Assert.Equal(40f, old.PracticalFullMarks.GetValueOrDefault());
        Assert.Equal(16f, old.PracticalPassMarks.GetValueOrDefault());
        Assert.False(old.HasTheory);
    }

    [Fact]
    public async Task GetStudents_OnPartialSchedule_OnlyListsStudentsPinnedToThatOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedPracticalReview);

        var old = await CreateService(db).GetStudentsForPracticalMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.OldPracticalOfferingId, TestData.CollegeId);
        Assert.Equal([2], old.Students.Select(s => s.ExamRegistrationId).ToArray());
        var oldRow = Assert.Single(old.Students);
        Assert.Equal("REG2", oldRow.RegistrationNumber);
        Assert.Equal("2080", oldRow.AcademicYearName);

        var current = await CreateService(db).GetStudentsForPracticalMarksAsync(
            MarksEntryTestSeed.ScheduleId, MarksEntryTestSeed.CurrentOfferingId, TestData.CollegeId);
        Assert.Empty(current.Students);
    }

    [Fact]
    public async Task SavePractical_OnPartialSchedule_UpdatesPinnedRowInPlaceWithBatchScheme()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedPracticalReview);

        var result = await CreateService(db).SavePracticalMarksAsync(new PracticalMarksSaveDto
        {
            CollegeId = TestData.CollegeId,
            ExamScheduleId = MarksEntryTestSeed.ScheduleId,
            SubjectOfferingId = MarksEntryTestSeed.OldPracticalOfferingId,
            Students =
            [
                new StudentPracticalMarksRowDto { ExamRegistrationId = 2, Practical = 10 }
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

        var row = Assert.Single(rows);
        Assert.Equal(MarksEntryTestSeed.OldPracticalOfferingId, row.SubjectOfferingId);
        Assert.Equal(10, row.ObtainedMarksPractical);
        Assert.Equal(MarksEntryTestSeed.BatchSchemeId, row.GradingSchemeId);
        Assert.Equal("B", row.GradeLetter);
    }
}