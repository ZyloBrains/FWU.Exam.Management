using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class AuditLogInterceptorTests
{
    private sealed class TestAuditUserProvider : IAuditUserProvider
    {
        public string? UserName { get; set; } = "test-user";
        public string? GetCurrentUserName() => UserName;
    }

    private sealed class InterceptorFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext Context { get; }
        public TestTenantContext Tenant { get; }
        public TestAuditUserProvider UserProvider { get; } = new();
        public TestUserContext UserContext { get; } = new();

        public InterceptorFixture(bool standardTenant = true)
        {
            Tenant = standardTenant ? TestTenantContext.Standard() : TestTenantContext.Central();
            UserContext.SetUser("user-1", null, TestData.CollegeId, [], [Role.CollegeAdmin]);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var interceptor = new AuditLogInterceptor(UserProvider, Tenant, UserContext, NullLogger<AuditLogInterceptor>.Instance);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(interceptor)
                .Options;

            Context = new AppDbContext(options, NullLogger<AppDbContext>.Instance, Tenant);
            Context.Database.EnsureCreated();

            if (standardTenant)
            {
                Context.Tenants.Add(new Tenant
                {
                    Id = TestData.TenantId,
                    Name = "Test College",
                    OfficeCode = "TST",
                    ContactNumber = "000",
                    Address = "Kathmandu",
                    Email = "t@t.com",
                    TenantType = Domain.Enums.TenantType.Standard,
                    IsActive = true
                });
                Context.SaveChanges();
            }
        }

        public void SeedCollegeType()
        {
            Context.CollegeTypes.Add(new CollegeType { Id = 1, Code = "UNI", Name = "University", IsActive = true });
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }

    private static College College(int id, string name) => new()
    {
        Id = id,
        Code = $"CLG{id}",
        Name = name,
        Email = $"c{id}@c.com",
        PrincipalName = "Principal",
        PrincipalContactNumber = "000",
        CollegeTypeId = 1,
        IsActive = true
    };

    [Fact]
    public void SavingChanges_NewEntity_WritesDataChangeAuditLog()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();
        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.SaveChanges();

        var log = fx.Context.AuditLogs.Single(l => l.EntityName == "College");
        Assert.Equal(AuditLogKinds.DataChange, log.Kind);
        Assert.Equal("College", log.EntityName);
        Assert.Equal("Created", log.Action);
        Assert.Equal("1", log.EntityId);
        Assert.Equal("test-user", log.UserName);
        Assert.Equal("user-1", log.UserId);
        Assert.Equal(TestData.TenantId, log.TenantId);
        Assert.NotNull(log.ChangesJson);
        Assert.Null(log.RowCount);
    }

    [Fact]
    public void SavingChanges_ModifiedEntity_CapturesFieldChanges()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();
        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.SaveChanges();
        fx.Context.ChangeTracker.Clear();

        var college = fx.Context.Colleges.IgnoreQueryFilters().Single();
        college.Name = "Renamed College";
        fx.Context.SaveChanges();

        var log = fx.Context.AuditLogs.Single(a => a.Action == "Updated");
        Assert.Equal("College", log.EntityName);
        Assert.Equal("1", log.EntityId);
        Assert.Contains("Renamed College", log.ChangesJson);
        Assert.Contains("First College", log.ChangesJson);
    }

    [Fact]
    public void SavingChanges_DeletedEntity_CapturesOldValues()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();
        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.SaveChanges();
        fx.Context.ChangeTracker.Clear();

        var college = fx.Context.Colleges.IgnoreQueryFilters().Single();
        fx.Context.Colleges.Remove(college);
        fx.Context.SaveChanges();

        var log = fx.Context.AuditLogs.Single(a => a.Action == "Deleted");
        Assert.Equal("College", log.EntityName);
        Assert.Equal("1", log.EntityId);
        Assert.Contains("First College", log.ChangesJson);
    }

    [Fact]
    public void SavingChanges_DoesNotLogAuditLogRows()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();

        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.SaveChanges();
        var countBefore = fx.Context.AuditLogs.Count();

        // Mutating the AuditLogs table itself must not produce new audit rows.
        fx.Context.AuditLogs.Add(new AuditLog
        {
            TenantId = TestData.TenantId,
            Kind = AuditLogKinds.Activity,
            ActivityType = ActivityTypes.UserLogin,
            Timestamp = DateTime.UtcNow
        });
        fx.Context.SaveChanges();

        Assert.Equal(countBefore + 1, fx.Context.AuditLogs.Count());
    }

    [Fact]
    public void SavingChanges_DoesNotLogTenantEntities()
    {
        using var fx = new InterceptorFixture();
        fx.Context.Tenants.Add(new Tenant
        {
            Id = 99,
            Name = "Second Tenant",
            OfficeCode = "TST2",
            ContactNumber = "000",
            Address = "Kathmandu",
            Email = "t2@t.com",
            TenantType = Domain.Enums.TenantType.Standard,
            IsActive = true
        });
        fx.Context.SaveChanges();

        Assert.Empty(fx.Context.AuditLogs);
    }

    [Fact]
    public void SavingChanges_CentralTenant_DoesNotWriteAuditLogs()
    {
        using var fx = new InterceptorFixture(standardTenant: false);
        fx.SeedCollegeType();
        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.SaveChanges();

        Assert.Empty(fx.Context.AuditLogs);
    }

    [Fact]
    public void SavingChanges_BulkEntities_CollapsesIntoSingleSummaryRow()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();

        for (var i = 1; i <= 60; i++)
            fx.Context.Colleges.Add(College(i, $"College {i}"));

        fx.Context.SaveChanges();

        var summary = fx.Context.AuditLogs.Single(l => l.EntityName == "College");
        Assert.Equal(AuditLogKinds.DataChange, summary.Kind);
        Assert.Equal("College", summary.EntityName);
        Assert.Equal("Created", summary.Action);
        Assert.Equal(60, summary.RowCount);
        Assert.Null(summary.EntityId);
        Assert.Contains("Bulk", summary.Description);
    }

    [Fact]
    public void SavingChanges_MultipleEntities_WritesOneRowPerEntity()
    {
        using var fx = new InterceptorFixture();
        fx.SeedCollegeType();

        fx.Context.Colleges.Add(College(1, "First College"));
        fx.Context.Colleges.Add(College(2, "Second College"));
        fx.Context.SaveChanges();

        var logs = fx.Context.AuditLogs.Where(l => l.EntityName == "College").ToList();
        Assert.Equal(2, logs.Count);
        Assert.Equal(new[] { "1", "2" }, logs.Select(l => l.EntityId).OrderBy(x => x).ToArray());
    }
}
