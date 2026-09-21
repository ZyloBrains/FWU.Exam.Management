using System.Security.Claims;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class UserContextMiddlewareCacheTests
{
    private const string UserId = "u-1";
    private const string RoleId = "r-1";

    private static ClaimsPrincipal Principal() => new(
        new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, UserId)], "test"));

    private static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    private static void Seed(AppDbContext ctx)
    {
        ctx.Roles.Add(new IdentityRole("SuperAdmin") { Id = RoleId });
        var user = TestData.User(UserId, "admin@test.com");
        user.FacultyId = 1;
        user.CollegeId = 1;
        ctx.Users.Add(user);
        ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = UserId, RoleId = RoleId });
        ctx.CollegeFaculties.Add(new CollegeFaculty
        {
            TenantId = TestData.TenantId,
            CollegeId = 1,
            FacultyId = 1
        });
        ctx.SaveChanges();
    }

    private static ServiceProvider BuildProvider(SqliteConnection connection)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connection));
        services.AddMemoryCache();
        services.AddScoped<IUserContext>(_ => new TestUserContext());
        services.AddScoped<ITenantContext>(_ => TestTenantContext.Standard());
        services.AddIdentityCore<AppUser>(_ => { })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task InvokeAsync_PopulatesUserContext_AndSecondCallHitsCache()
    {
        using var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        connection.Open();
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var setup = new AppDbContext(contextOptions))
        {
            setup.Database.EnsureCreated();
            Seed(setup);
        }

        using var provider = BuildProvider(connection);
        var middleware = new UserContextMiddleware(_ => Task.CompletedTask);

        using (var scope = provider.CreateScope())
        {
            var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            http.User = Principal();
            await middleware.InvokeAsync(http);

            var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
            Assert.Equal(UserId, userContext.UserId);
            Assert.Equal([1], userContext.FacultyCollegeIds);
            Assert.Equal(["SuperAdmin"], userContext.Roles);

            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            Assert.False(tenantContext.IsCollegeAdmin);
        }

        // Add a competing college-faculty link behind the cache; a re-query would
        // include it, a cache hit must keep returning the original list.
        using (var setup = new AppDbContext(contextOptions))
        {
            setup.CollegeFaculties.Add(new CollegeFaculty
            {
                TenantId = TestData.TenantId,
                CollegeId = 9,
                FacultyId = 1
            });
            await setup.SaveChangesAsync();
        }

        using (var scope = provider.CreateScope())
        {
            var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            http.User = Principal();
            await middleware.InvokeAsync(http);

            var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
            Assert.Equal([1], userContext.FacultyCollegeIds);
        }
    }

    [Fact]
    public async Task InvokeAsync_AnonymousPrincipal_DoesNotTouchContext()
    {
        using var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        connection.Open();
        using var setup = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        setup.Database.EnsureCreated();
        Seed(setup);

        using var provider = BuildProvider(connection);
        var middleware = new UserContextMiddleware(_ => Task.CompletedTask);

        using var scope = provider.CreateScope();
        var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        http.User = Anonymous();
        await middleware.InvokeAsync(http);

        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        Assert.Null(userContext.UserId);
    }
}