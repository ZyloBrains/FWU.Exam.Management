using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SemesterServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    [Fact]
    public async Task CreateSemesterAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new SemesterService(context);

        var semester = new Semester
        {
            Name = "First Semester",
            Code = "SEM1",
            Number = 1,
            Year = 1,
            AcademicYearId = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        await service.CreateSemesterAsync(semester);

        var result = await service.GetSemesterByIdAsync(semester.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("First Semester");
    }

    [Fact]
    public async Task GetSemestersAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterService(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<Semester>().Add(new Semester
            {
                Name = $"Semester {i}", Code = $"SEM{i}", Number = i, Year = 1, AcademicYearId = 1,
                StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6)
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetSemestersAsync(1, 2, null, "name", "asc", userCtx);

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSemestersAsync_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterService(context);

        context.Set<Semester>().Add(new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) });
        context.Set<Semester>().Add(new Semester { Name = "Second Semester", Code = "SEM2", Number = 2, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetSemestersAsync(1, 10, "First", "name", "asc", userCtx);

        totalCount.Should().Be(1);
        items[0].Name.Should().Be("First Semester");
    }

    [Fact]
    public async Task UpdateSemesterAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new SemesterService(context);

        var semester = new Semester { Name = "Old Name", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        semester.Name = "Updated Name";
        await service.UpdateSemesterAsync(semester);

        var updated = await service.GetSemesterByIdAsync(semester.Id);
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteSemesterAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new SemesterService(context);

        var semester = new Semester { Name = "To Delete", Code = "DEL", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        await service.DeleteSemesterAsync(semester.Id);

        var exists = await service.SemesterExistsAsync(semester.Id);
        exists.Should().BeFalse();
    }
}
