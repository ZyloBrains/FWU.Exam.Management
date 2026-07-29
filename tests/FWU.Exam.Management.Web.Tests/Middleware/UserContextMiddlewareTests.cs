using System.Security.Claims;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FWU.Exam.Management.Web.Tests.Middleware;

public class UserContextMiddlewareTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static UserManager<AppUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<AppUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        options.Value.Returns(new IdentityOptions());
        return Substitute.For<UserManager<AppUser>>(
            store, options,
            Substitute.For<IPasswordHasher<AppUser>>(),
            Substitute.For<IEnumerable<IUserValidator<AppUser>>>(),
            Substitute.For<IEnumerable<IPasswordValidator<AppUser>>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<AppUser>>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAuthenticated_ShouldPopulateUserContext()
    {
        using var dbContext = CreateDbContext("UserAuthTest");
        dbContext.Users.Add(new AppUser
        {
            Id = "user-1",
            UserName = "testuser",
            FacultyId = null,
            CollegeId = 20
        });
        await dbContext.SaveChangesAsync();

        var userContext = new UserContext();
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(Arg.Any<AppUser>()).Returns(new List<string> { Role.SuperAdmin });

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IUserContext)).Returns(userContext);
        serviceProvider.GetService(typeof(AppDbContext)).Returns(dbContext);
        serviceProvider.GetService(typeof(UserManager<AppUser>)).Returns(userManager);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1")
            }, "test"))
        };

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new UserContextMiddleware(next);

        await middleware.InvokeAsync(httpContext);

        userContext.IsAuthenticated.Should().BeTrue();
        userContext.UserId.Should().Be("user-1");
        userContext.FacultyId.Should().BeNull();
        userContext.CollegeId.Should().Be(20);
        userContext.Roles.Should().Contain(Role.SuperAdmin);
        userContext.IsSuperAdmin.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAnonymous_ShouldNotSetUserContext()
    {
        var userContext = new UserContext();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IUserContext)).Returns(userContext);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new UserContextMiddleware(next);

        await middleware.InvokeAsync(httpContext);

        userContext.IsAuthenticated.Should().BeFalse();
        userContext.UserId.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIdNotFoundInDb_ShouldNotSetUserContext()
    {
        using var dbContext = CreateDbContext("UserNotFoundTest");

        var userContext = new UserContext();
        var userManager = CreateUserManager();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IUserContext)).Returns(userContext);
        serviceProvider.GetService(typeof(AppDbContext)).Returns(dbContext);
        serviceProvider.GetService(typeof(UserManager<AppUser>)).Returns(userManager);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "nonexistent-user")
            }, "test"))
        };

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new UserContextMiddleware(next);

        await middleware.InvokeAsync(httpContext);

        userContext.IsAuthenticated.Should().BeFalse();
        userContext.UserId.Should().BeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldExtractRolesCorrectly()
    {
        using var dbContext = CreateDbContext("RolesTest");
        dbContext.Users.Add(new AppUser { Id = "user-2", UserName = "roleuser" });
        await dbContext.SaveChangesAsync();

        var userContext = new UserContext();
        var userManager = CreateUserManager();
        userManager.GetRolesAsync(Arg.Any<AppUser>()).Returns(new List<string> { Role.FacultyAdmin, Role.CollegeAdmin });

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IUserContext)).Returns(userContext);
        serviceProvider.GetService(typeof(AppDbContext)).Returns(dbContext);
        serviceProvider.GetService(typeof(UserManager<AppUser>)).Returns(userManager);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-2")
            }, "test"))
        };

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new UserContextMiddleware(next);

        await middleware.InvokeAsync(httpContext);

        userContext.IsSuperAdmin.Should().BeFalse();
        userContext.IsFacultyAdmin.Should().BeTrue();
        userContext.IsCollegeAdmin.Should().BeTrue();
    }
}
