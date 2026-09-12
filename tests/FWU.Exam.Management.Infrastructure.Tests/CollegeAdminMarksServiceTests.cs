using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
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