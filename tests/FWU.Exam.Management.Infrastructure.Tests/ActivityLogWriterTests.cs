using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Interceptor;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ActivityLogWriterTests
{
    private sealed class TestUserProvider : IAuditUserProvider
    {
        public string? UserName { get; set; }
        public TestUserProvider(string? userName) => UserName = userName;
        public string? GetCurrentUserName() => UserName;
    }

    private sealed class WriterFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestTenantContext Tenant { get; }
        public TestUserContext UserContext { get; }
        public TestUserProvider UserProvider { get; }
        public ServiceProvider Provider { get; }

        public WriterFixture(TestTenantContext tenant, TestUserContext userContext, string currentUserName = "tester")
        {
            Tenant = tenant;
            UserContext = userContext;
            UserProvider = new TestUserProvider(currentUserName);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();
            services.AddSingleton<ITenantContext>(tenant);
            services.AddSingleton<IUserContext>(userContext);
            services.AddSingleton<IAuditUserProvider>(UserProvider);
            services.AddScoped(_ =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;
                return new AppDbContext(options, NullLogger<AppDbContext>.Instance, tenant);
            });

            Provider = services.BuildServiceProvider();

            using var scope = Provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            if (tenant.TenantId > 0)
            {
                context.Tenants.Add(new Tenant
                {
                    Id = tenant.TenantId,
                    Name = "Test College",
                    OfficeCode = "TST",
                    ContactNumber = "000",
                    Address = "Kathmandu",
                    Email = "t@t.com",
                    TenantType = Domain.Enums.TenantType.Standard,
                    IsActive = true
                });
                context.SaveChanges();
            }
        }

        public AuditLogWriter CreateWriter() => new(
            Provider.GetRequiredService<IServiceScopeFactory>(),
            UserContext,
            Tenant,
            UserProvider,
            NullLogger<AuditLogWriter>.Instance);

        public AuditLogWriter CreateWriter(IAuditUserProvider userProvider) => new(
            Provider.GetRequiredService<IServiceScopeFactory>(),
            UserContext,
            Tenant,
            userProvider,
            NullLogger<AuditLogWriter>.Instance);

        public void Dispose()
        {
            Provider.Dispose();
            _connection.Dispose();
        }
    }

    [Fact]
    public async Task LogAsync_WritesActivityRow()
    {
        var tenant = TestTenantContext.Standard();
        var userContext = new TestUserContext();
        userContext.SetUser("user-1", null, TestData.CollegeId, [], [Role.CollegeAdmin]);

        using var fx = new WriterFixture(tenant, userContext);
        var writer = fx.CreateWriter();

        await writer.LogAsync(
            ActivityTypes.UserLogin,
            "User logged in",
            new { method = "password" },
            entityName: "AppUser",
            entityId: "user-1");

        using var scope = fx.Provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await context.AuditLogs.SingleAsync();

        Assert.Equal(AuditLogKinds.Activity, row.Kind);
        Assert.Equal(ActivityTypes.UserLogin, row.ActivityType);
        Assert.Equal("User logged in", row.Description);
        Assert.Equal("tester", row.UserName);
        Assert.Equal("user-1", row.UserId);
        Assert.Equal(TestData.TenantId, row.TenantId);
        Assert.Contains("password", row.DetailsJson);
        Assert.Equal("AppUser", row.EntityName);
        Assert.Equal("user-1", row.EntityId);
    }

    [Fact]
    public async Task LogAsync_CentralTenant_NullTenantId()
    {
        using var fx = new WriterFixture(TestTenantContext.Central(), new TestUserContext());
        var writer = fx.CreateWriter();

        await writer.LogAsync(ActivityTypes.UserLogin, "Central login");

        using var scope = fx.Provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await context.AuditLogs.SingleAsync();
        Assert.Null(row.TenantId);
    }

    [Fact]
    public async Task LogAsync_ExplicitActorUser_ResolvesUserName()
    {
        using var fx = new WriterFixture(TestTenantContext.Standard(), new TestUserContext());
        using (var seedScope = fx.Provider.CreateScope())
        {
            var seedContext = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            seedContext.Users.Add(TestData.User("admin-1", "admin@test.com"));
            await seedContext.SaveChangesAsync();
        }

        var writer = fx.CreateWriter(new TestUserProvider(null));
        await writer.LogAsync(ActivityTypes.UserCreated, "User created", actorUserId: "admin-1");

        using var readScope = fx.Provider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await readContext.AuditLogs.SingleAsync();
        Assert.Equal("admin@test.com", row.UserName);
        Assert.Equal("admin-1", row.UserId);
    }
}
