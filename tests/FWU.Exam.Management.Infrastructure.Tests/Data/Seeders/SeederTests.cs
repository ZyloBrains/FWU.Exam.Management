using FluentAssertions;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Data.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Data.Seeders;

public class SeederTests : TestBase
{
    private static IServiceProvider CreateServiceProvider(AppDbContext context)
    {
        var services = new ServiceCollection();
        services.AddSingleton(context);

        var rolesCache = new Dictionary<string, IdentityRole>();
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null!, null!, null!, null!);
        roleManager.FindByNameAsync(Arg.Any<string>()).Returns(callInfo =>
        {
            var name = callInfo.Arg<string>();
            return rolesCache.TryGetValue(name!, out var r) ? r : null;
        });
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(callInfo =>
        {
            var role = callInfo.Arg<IdentityRole>();
            role.Id = Guid.NewGuid().ToString();
            rolesCache[role.Name!] = role;
            return IdentityResult.Success;
        });
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);
        services.AddSingleton(roleManager);

        var userStore = Substitute.For<IUserStore<AppUser>>();
        var userMgr = Substitute.For<UserManager<AppUser>>(userStore, null!, null!, null!, null!, null!, null!, null!, null!);
        userMgr.FindByEmailAsync(Arg.Any<string>()).Returns((AppUser?)null);
        userMgr.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userMgr.AddToRoleAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        services.AddSingleton(userMgr);

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedDefaults:AdminPassword"] = "Test@123"
            });
        services.AddSingleton<IConfiguration>(configBuilder.Build());

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task AcademicYearSeeder_Seeds14Years()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await AcademicYearSeeder.SeedAcademicYearsAsync(sp);

        var years = await context.AcademicYears!.ToListAsync();
        years.Should().HaveCount(14);
        years.Should().Contain(y => y.AcademicYearCode == "2024");
        years.Should().Contain(y => y.AcademicYearCode == "2013");

        // Idempotency: calling again should not add more
        await AcademicYearSeeder.SeedAcademicYearsAsync(sp);
        var yearsAgain = await context.AcademicYears!.ToListAsync();
        yearsAgain.Should().HaveCount(14);
    }

    [Fact]
    public async Task LocationSeeder_SeedsProvincesAndDistricts()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await LocationSeeder.SeedLocationDataAsync(sp);

        var provinces = await context.Provinces!.ToListAsync();
        provinces.Should().HaveCount(7);
        provinces.Should().Contain(p => p.ProvinceCode == "P1");
        provinces.Should().Contain(p => p.ProvinceCode == "P7");

        var districts = await context.Districts!.ToListAsync();
        districts.Should().HaveCount(77);

        var localLevels = await context.LocalLevels!.ToListAsync();
        localLevels.Should().NotBeEmpty();

        // Idempotency
        await LocationSeeder.SeedLocationDataAsync(sp);
        (await context.Provinces!.CountAsync()).Should().Be(7);
        (await context.Districts!.CountAsync()).Should().Be(77);
    }

    [Fact]
    public async Task PermissionSeeder_SeedsAllPermissions()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await PermissionSeeder.SeedPermissionsAsync(sp);

        var permissions = await context.Permissions!.ToListAsync();
        permissions.Should().HaveCount(Permissions.All.Count);

        // Check specific groups exist
        permissions.Select(p => p.Group).Distinct().Should().Contain(new[]
        {
            "dashboard", "faculties", "colleges", "users", "roles", "permissions"
        });

        // Idempotency
        await PermissionSeeder.SeedPermissionsAsync(sp);
        (await context.Permissions!.CountAsync()).Should().Be(Permissions.All.Count);
    }

    [Fact]
    public async Task PermissionSeeder_SeedRolePermissions_CreatesRolesAndAssigns()
    {
        using var context = await CreateContextAsync();
        var roles = Permissions.RolePermissions.Keys.Select(r => new IdentityRole(r)
        {
            Id = Guid.NewGuid().ToString(),
            NormalizedName = r.ToUpperInvariant()
        }).ToList();

        context.Set<IdentityRole>().AddRange(roles);
        await context.SaveChangesAsync();

        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null!, null!, null!, null!);
        roleManager.FindByNameAsync(Arg.Any<string>()).Returns(callInfo =>
        {
            var name = callInfo.Arg<string>();
            return roles.FirstOrDefault(r => r.Name == name);
        });
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(IdentityResult.Success);
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(true);

        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddSingleton(roleManager);
        var sp = services.BuildServiceProvider();

        await PermissionSeeder.SeedPermissionsAsync(sp);

        await PermissionSeeder.SeedRolePermissionsAsync(sp);

        var rolePermissions = await context.RolePermissions!.ToListAsync();
        rolePermissions.Should().NotBeEmpty();

        // Idempotency
        await PermissionSeeder.SeedRolePermissionsAsync(sp);
        var again = await context.RolePermissions!.CountAsync();
        again.Should().Be(rolePermissions.Count);
    }

    [Fact]
    public async Task UserSeeder_SeedsRolesAndSuperAdmin()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await UserSeeder.SeedRolesAsync(sp);
        await UserSeeder.SeedSuperAdminAsync(sp);

        // Verify via mocks - UserManager.CreateAsync should have been called
        var userMgr = sp.GetRequiredService<UserManager<AppUser>>();
        await userMgr.Received(1).CreateAsync(
            Arg.Is<AppUser>(u => u.Email == "admin@gmail.com"),
            Arg.Any<string>());
    }

    [Fact]
    public async Task ReferenceDataSeeder_SeedsPaymentTypes()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await ReferenceDataSeeder.SeedPaymentTypesAsync(sp);

        var paymentTypes = await context.Set<PaymentType>().ToListAsync();
        paymentTypes.Should().HaveCount(3);
        paymentTypes.Select(pt => pt.PaymentTypeName).Should().Contain(new[] { "eSewa", "Khalti", "ConnectIPS" });

        // Idempotency
        await ReferenceDataSeeder.SeedPaymentTypesAsync(sp);
        (await context.Set<PaymentType>().CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task ReferenceDataSeeder_SeedsGendersLevelsAndPreviousLevels()
    {
        using var context = await CreateContextAsync();
        var sp = CreateServiceProvider(context);

        await ReferenceDataSeeder.SeedReferenceDataAsync(sp);

        var genders = await context.Genders!.ToListAsync();
        genders.Should().HaveCount(3);
        genders.Select(g => g.GenderName).Should().Contain(new[] { "Male", "Female", "Other" });

        var levels = await context.Levels!.ToListAsync();
        levels.Should().HaveCount(4);
        levels.Select(l => l.LevelName).Should().Contain(new[] { "Undergraduate", "Graduate", "MPhil Leading to Ph.D", "Ph.D." });

        var previousLevels = await context.PreviousLevels!.ToListAsync();
        previousLevels.Should().HaveCount(4);

        // Idempotency
        await ReferenceDataSeeder.SeedReferenceDataAsync(sp);
        (await context.Genders!.CountAsync()).Should().Be(3);
        (await context.Levels!.CountAsync()).Should().Be(4);
        (await context.PreviousLevels!.CountAsync()).Should().Be(4);
    }
}
