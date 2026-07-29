using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class FacultyServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        ctx.IsFacultyAdmin.Returns(false);
        ctx.IsCollegeAdmin.Returns(false);
        ctx.FacultyId.Returns((int?)null);
        return ctx;
    }

    private static UserManager<AppUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<AppUser>>();
        return Substitute.For<UserManager<AppUser>>(store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task GetAllFacultiesAsync_ShouldReturnAll()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var logger = Substitute.For<ILogger<FacultyService>>();
        var userManager = CreateUserManager();
        var service = new FacultyService(context, userManager, logger, userCtx);

        context.Set<Faculty>().Add(new Faculty
        {
            Name = "Science", OfficeCode = "SCI", ContactNumber = "01-5550001",
            Address = "KTM", Email = "sci@fwu.edu.np", TenantId = TestTenantId
        });
        context.Set<Faculty>().Add(new Faculty
        {
            Name = "Management", OfficeCode = "MGT", ContactNumber = "01-5550002",
            Address = "KTM", Email = "mgt@fwu.edu.np", TenantId = TestTenantId
        });
        await context.SaveChangesAsync();

        var items = await service.GetAllFacultiesAsync();

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFacultiesPagedAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var logger = Substitute.For<ILogger<FacultyService>>();
        var userManager = CreateUserManager();
        var service = new FacultyService(context, userManager, logger, userCtx);

        context.Set<Faculty>().Add(new Faculty
        {
            Name = "Science", OfficeCode = "SCI", ContactNumber = "01-5550001",
            Address = "KTM", Email = "sci@fwu.edu.np", TenantId = TestTenantId
        });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetFacultiesPagedAsync(1, 10, null, "name", "asc");

        totalCount.Should().Be(1);
        items[0].Name.Should().Be("Science");
    }

    [Fact]
    public async Task GetFacultyByIdAsync_ShouldReturnFaculty()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var logger = Substitute.For<ILogger<FacultyService>>();
        var userManager = CreateUserManager();
        var service = new FacultyService(context, userManager, logger, userCtx);

        var faculty = new Faculty
        {
            Name = "Education", OfficeCode = "EDU", ContactNumber = "01-5550003",
            Address = "KTM", Email = "edu@fwu.edu.np", TenantId = TestTenantId
        };
        context.Set<Faculty>().Add(faculty);
        await context.SaveChangesAsync();

        var result = await service.GetFacultyByIdAsync(faculty.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Education");
    }

    [Fact]
    public async Task FacultyExistsAsync_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var logger = Substitute.For<ILogger<FacultyService>>();
        var userManager = CreateUserManager();
        var service = new FacultyService(context, userManager, logger, userCtx);

        var faculty = new Faculty
        {
            Name = "Law", OfficeCode = "LAW", ContactNumber = "01-5550004",
            Address = "KTM", Email = "law@fwu.edu.np", TenantId = TestTenantId
        };
        context.Set<Faculty>().Add(faculty);
        await context.SaveChangesAsync();

        var exists = await service.FacultyExistsAsync(faculty.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFacultyAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var logger = Substitute.For<ILogger<FacultyService>>();
        var userManager = CreateUserManager();
        var service = new FacultyService(context, userManager, logger, userCtx);

        var faculty = new Faculty
        {
            Name = "Engineering", OfficeCode = "ENG", ContactNumber = "01-5550005",
            Address = "KTM", Email = "eng@fwu.edu.np", TenantId = TestTenantId
        };
        context.Set<Faculty>().Add(faculty);
        await context.SaveChangesAsync();
        var id = faculty.Id;

        await service.DeleteFacultyAsync(id);

        var exists = await service.FacultyExistsAsync(id);
        exists.Should().BeFalse();
    }
}
