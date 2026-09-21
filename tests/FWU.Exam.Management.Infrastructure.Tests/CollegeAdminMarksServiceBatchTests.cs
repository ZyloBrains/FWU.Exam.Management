using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CollegeAdminMarksServiceBatchTests
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

    private static ExamRegistration Registration(int id) => new()
    {
        Id = id,
        TenantId = TestData.TenantId,
        AcademicYearId = TestData.AcademicYearId,
        CollegeId = TestData.CollegeId,
        ProgramsId = TestData.ProgramId,
        ExamScheduleId = 21,
        RegistrationDate = DateTime.UtcNow,
        Status = FWU.Exam.Management.Domain.Enums.RegistrationStatus.Registered,
        IsActive = true,
        IsAppliedByStudent = true
    };

    private static void SeedMarksEntry(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.CollegePrograms.Add(new FWU.Exam.Management.Domain.Entities.Colleges.CollegeProgram
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

    private static InternalMarksSaveDto SaveDto(params StudentInternalMarksRowDto[] rows) => new()
    {
        CollegeId = TestData.CollegeId,
        ExamScheduleId = 21,
        SubjectOfferingId = 101,
        SubmitAll = true,
        Students = rows.ToList()
    };

    [Fact]
    public async Task SaveInternalMarks_BatchLoadsExistingResults_UpdatesInPlaceWithoutDuplicates()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedMarksEntry);

        var result = await CreateService(db).SaveInternalMarksAsync(SaveDto(
            new StudentInternalMarksRowDto { ExamRegistrationId = 1, TheoryInternal = 28f },
            new StudentInternalMarksRowDto { ExamRegistrationId = 2, TheoryInternal = 30f }));

        Assert.True(result.Success);

        var rows = await db.Context.ExamSubjectResults
            .Where(esr => esr.ExamRegistrationId == 1 || esr.ExamRegistrationId == 2)
            .ToListAsync();

        // Both forms updated in place; the pre-existing row is reused, not duplicated.
        Assert.Equal(2, rows.Count);
        Assert.Equal(28f, rows.Single(r => r.ExamRegistrationId == 1).ObtainedMarksTheoryInternal);
        Assert.Equal(30f, rows.Single(r => r.ExamRegistrationId == 2).ObtainedMarksTheoryInternal);
    }

    [Fact]
    public async Task SaveInternalMarks_NewEntitiesAreCreatedForMissingStudents()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedMarksEntry);

        var result = await CreateService(db).SaveInternalMarksAsync(SaveDto(
            new StudentInternalMarksRowDto { ExamRegistrationId = 2, TheoryInternal = 30f }));

        Assert.True(result.Success);
        Assert.Equal(1, result.SavedCount);

        var row = await db.Context.ExamSubjectResults
            .SingleAsync(esr => esr.ExamRegistrationId == 2);
        Assert.Equal(30f, row.ObtainedMarksTheoryInternal);
    }

    [Fact]
    public async Task SaveInternalMarks_ReExam_UsesCorrectExistingRow()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddOldTheoryOffering(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx);
            MarksEntryTestSeed.AddReExamStudents(ctx);

            // Student 2 holds a pinned re-exam row on the old-curriculum offering.
            ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(2, 2,
                MarksEntryTestSeed.OldTheoryOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial,
                theory: true, practical: true));
        });

        var result = await CreateService(db).SaveCollegeMarksBulkAsync(
            new BulkMarksSaveDto
            {
                SubjectOfferingId = MarksEntryTestSeed.OldTheoryOfferingId,
                ExamScheduleId = MarksEntryTestSeed.ScheduleId,
                Students =
                [
                    new StudentMarksRowDto { ExamRegistrationId = 2, TheoryInternal = 14 }
                ]
            },
            TestData.CollegeId,
            MarksEntryTestSeed.AdminUserId);

        Assert.True(result.Success);

        var rows = await db.Context.ExamSubjectResults
            .Where(esr => esr.ExamRegistrationId == 2
                       && esr.ExamScheduleId == MarksEntryTestSeed.ScheduleId
                       && esr.IsActive)
            .ToListAsync();

        // The pinned row is reused, keeps the batch grading scheme, and receives the value.
        var row = Assert.Single(rows);
        Assert.Equal(14, row.ObtainedMarksTheoryInternal);
        Assert.Equal(MarksEntryTestSeed.BatchSchemeId, row.GradingSchemeId);
    }
}