using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamScheduleServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private async Task SeedDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelCode = "BACH", LevelName = "Bachelor", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Back", Code = "BCK", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        await context.SaveChangesAsync();
    }

    private async Task SeedCollegeAsync(AppDbContext context)
    {
        context.Set<College>().Add(new College { TenantId = TestTenantId, Code = "COL01", Name = "Test College", Email = "c@test.com", PrincipalName = "P", PrincipalContactNumber = "9800000000", IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateExamSchedule_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);
        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        var schedule = new ExamSchedule
        {
            TenantId = TestTenantId,
            ExamScheduleName = "Final 2081",
            AcademicYearId = 1,
            ProgramId = 1,
            SemesterId = 1,
            ExamTypeId = 1,
            IsActive = true,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        };

        await service.CreateExamScheduleAsync(schedule);

        var result = await service.GetExamScheduleByIdAsync(schedule.Id);
        result.Should().NotBeNull();
        result!.ExamScheduleName.Should().Be("Final 2081");
    }

    [Fact]
    public async Task GetExamSchedules_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<ExamSchedule>().Add(new ExamSchedule
            {
                TenantId = TestTenantId, ExamScheduleName = $"Schedule {i}",
                AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
                IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
            });
        }
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        var (items, totalCount) = await service.GetExamSchedulesAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExamSchedules_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId, ExamScheduleName = "Alpha", ExamScheduleCode = "ALP",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId, ExamScheduleName = "Beta", ExamScheduleCode = "BET",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        var (items, totalCount) = await service.GetExamSchedulesAsync(1, 10, "Alpha", "Name", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].ExamScheduleName.Should().Be("Alpha");
    }

    [Fact]
    public async Task UpdateExamSchedule_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId, ExamScheduleName = "Original",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        var schedule = await service.GetExamScheduleByIdAsync(1);
        schedule.Should().NotBeNull();

        schedule!.ExamScheduleName = "Updated";
        await service.UpdateExamScheduleAsync(schedule);

        context.ChangeTracker.Clear();
        var updated = await service.GetExamScheduleByIdAsync(1);
        updated!.ExamScheduleName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteExamSchedule_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId, ExamScheduleName = "ToDelete",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        await service.DeleteExamScheduleAsync(1);

        var exists = await service.ExamScheduleExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateExpiredSchedules_ShouldDeactivate()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId, ExamScheduleName = "Expired",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        await service.DeactivateExpiredSchedulesAsync();

        var expired = await context.Set<ExamSchedule>().FindAsync(1);
        expired!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSelectListData_ShouldReturnAllLists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);
        await SeedCollegeAsync(context);

        var userContext = CreateSuperAdminContext();
        var service = new ExamScheduleService(context, userContext);

        var dto = await service.GetSelectListDataAsync();

        dto.AcademicYears.Should().HaveCount(1);
        dto.ExamTypes.Should().HaveCount(2);
        dto.Programs.Should().HaveCount(1);
        dto.Semesters.Should().HaveCount(1);
    }
}
