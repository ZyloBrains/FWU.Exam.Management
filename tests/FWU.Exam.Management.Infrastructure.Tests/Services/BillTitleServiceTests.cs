using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class BillTitleServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    [Fact]
    public async Task CreateBillTitle_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new BillTitleService(context, CreateSuperAdminContext());

        var bt = new BillTitle
        {
            BillTitleName = "Exam Fee",
            Category = "Exam",
            Amount = 1000,
            IsActive = true
        };
        await service.CreateBillTitleAsync(bt);

        var result = await service.GetBillTitleByIdAsync(bt.Id);
        result.Should().NotBeNull();
        result!.BillTitleName.Should().Be("Exam Fee");
    }

    [Fact]
    public async Task GetBillTitles_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<BillTitle>().AddRange(
            new BillTitle { BillTitleName = "Title A", Category = "Cat1", IsActive = true },
            new BillTitle { BillTitleName = "Title B", Category = "Cat2", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        var (items, totalCount) = await service.GetBillTitlesAsync(1, 10, null, "billtitlename", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBillTitles_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<BillTitle>().AddRange(
            new BillTitle { BillTitleName = "Exam Fee", Category = "Exam", IsActive = true },
            new BillTitle { BillTitleName = "Lab Fee", Category = "Lab", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        var (items, totalCount) = await service.GetBillTitlesAsync(1, 10, "Exam", "billtitlename", "asc");

        totalCount.Should().Be(1);
        items[0].BillTitleName.Should().Be("Exam Fee");
    }

    [Fact]
    public async Task UpdateBillTitle_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bt = new BillTitle { BillTitleName = "Original", Category = "Cat", IsActive = true };
        context.Set<BillTitle>().Add(bt);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BillTitleService(context, CreateSuperAdminContext());

        var existing = await service.GetBillTitleByIdAsync(bt.Id);
        existing!.BillTitleName = "Updated";
        await service.UpdateBillTitleAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetBillTitleByIdAsync(bt.Id);
        updated!.BillTitleName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteBillTitle_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bt = new BillTitle { BillTitleName = "Delete Me", Category = "Cat", IsActive = true };
        context.Set<BillTitle>().Add(bt);
        await context.SaveChangesAsync();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        await service.DeleteBillTitleAsync(bt.Id);

        var exists = await service.BillTitleExistsAsync(bt.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task BillTitleExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bt = new BillTitle { BillTitleName = "Exists", Category = "Cat", IsActive = true };
        context.Set<BillTitle>().Add(bt);
        await context.SaveChangesAsync();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        var exists = await service.BillTitleExistsAsync(bt.Id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetExamSchedules_ShouldReturnList()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var level = new Level { LevelName = "Bachelor", IsActive = true };
        context.Set<Level>().Add(level);
        await context.SaveChangesAsync();

        var program = new Domain.Entities.Program
        {
            LevelId = level.Id,
            ProgramCode = "BCA",
            ProgramName = "Bachelor in Computer Application",
            ShortName = "BCA",
            Duration = 4,
            IsActive = true
        };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        var academicYear = new AcademicYear
        {
            AcademicYearCode = "2081-82",
            AcademicYearName = "2081-2082",
            AcademicYearNameNepali = "२०८१-२०८२",
            IsActive = true
        };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var semester = new Semester
        {
            Number = 1,
            Year = 1,
            Name = "First Semester",
            Code = "SEM1",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            AcademicYearId = academicYear.Id
        };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);
        await context.SaveChangesAsync();

        var schedule = new ExamSchedule
        {
            ExamScheduleName = "Exam 2081",
            ProgramId = program.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            ExamTypeId = examType.Id,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(10, 0),
            StartDate = new DateOnly(2025, 1, 1),
            IsActive = true
        };
        context.Set<ExamSchedule>().Add(schedule);
        await context.SaveChangesAsync();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        var schedules = await service.GetExamSchedulesAsync();

        schedules.Should().HaveCount(1);
        schedules[0].Id.Should().Be(schedule.Id);
    }

    [Fact]
    public async Task GetPrograms_ShouldReturnList()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);

        var program = new Domain.Entities.Program
        {
            LevelId = levelId,
            ProgramCode = "BBA",
            ProgramName = "Bachelor in Business Administration",
            ShortName = "BBA",
            Duration = 4,
            IsActive = true
        };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        var service = new BillTitleService(context, CreateSuperAdminContext());
        var programs = await service.GetProgramsAsync();

        programs.Should().HaveCount(1);
        programs[0].ProgramCode.Should().Be("BBA");
    }
}
