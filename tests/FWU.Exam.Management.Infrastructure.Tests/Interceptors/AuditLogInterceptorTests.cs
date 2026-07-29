using System;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Interceptors;

public class AuditLogInterceptorTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly ITenantContext _tenantContext;
    private readonly IUserContext _userContext;
    private readonly IAuditUserProvider _userProvider;
    private readonly AuditLogInterceptor _interceptor;
    private readonly DbContextOptions<AppDbContext> _optionsWithoutInterceptor;

    public AuditLogInterceptorTests()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantContext = Substitute.For<ITenantContext>();
        _tenantContext.TenantId.Returns(1);

        _userContext = Substitute.For<IUserContext>();
        _userContext.UserId.Returns("test-user-id");

        _userProvider = Substitute.For<IAuditUserProvider>();
        _userProvider.GetCurrentUserName().Returns("Test User");

        _interceptor = new AuditLogInterceptor(
            _userProvider,
            _tenantContext,
            _userContext,
            NullLogger<AuditLogInterceptor>.Instance);

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_interceptor)
            .Options;

        _optionsWithoutInterceptor = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
    }

    [Fact]
    public async Task SavingChanges_ShouldCreateAuditLog_OnEntityAdded()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);
        await context.Database.EnsureCreatedAsync();
        context.Set<Tenant>().Add(new Tenant { Id = 1, Name = "Test" });
        await context.SaveChangesAsync();

        context.Set<Notice>().Add(new Notice
        {
            TenantId = 1,
            NoticeTitle = "New Notice",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        });
        await context.SaveChangesAsync();

        var auditLogs = await context.Set<AuditLog>().ToListAsync();
        auditLogs.Should().NotBeEmpty();
        auditLogs.Should().Contain(al => al.EntityName == "Notice" && al.Action == "Created");
    }

    [Fact]
    public async Task SavingChanges_ShouldCreateAuditLog_OnEntityUpdated()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);
        await context.Database.EnsureCreatedAsync();
        context.Set<Tenant>().Add(new Tenant { Id = 1, Name = "Test" });
        await context.SaveChangesAsync();

        var notice = new Notice
        {
            TenantId = 1,
            NoticeTitle = "Original",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();

        var saved = await context.Set<Notice>().FindAsync(notice.Id);
        saved!.NoticeTitle = "Updated";
        await context.SaveChangesAsync();

        var auditLogs = await context.Set<AuditLog>().ToListAsync();
        auditLogs.Should().Contain(al => al.EntityName == "Notice" && al.Action == "Updated");
    }

    [Fact]
    public async Task SavingChanges_ShouldNotCreateAuditLog_ForTenantEntity()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);
        await context.Database.EnsureCreatedAsync();

        context.Set<Tenant>().Add(new Tenant { Id = 2, Name = "Tenant 2" });
        await context.SaveChangesAsync();

        var auditLogs = await context.Set<AuditLog>().ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChanges_ShouldNotCreateAuditLog_WhenTenantIdIsZero()
    {
        var zeroTenantContext = Substitute.For<ITenantContext>();
        zeroTenantContext.TenantId.Returns(0);

        var interceptor = new AuditLogInterceptor(
            _userProvider, zeroTenantContext, _userContext,
            NullLogger<AuditLogInterceptor>.Instance);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        using var context = new AppDbContext(options, null, zeroTenantContext);
        await context.Database.EnsureCreatedAsync();
        context.Set<Tenant>().Add(new Tenant { Id = 1, Name = "Test" });
        await context.SaveChangesAsync();

        context.Set<Notice>().Add(new Notice
        {
            TenantId = 1,
            NoticeTitle = "Test",
            NoticePreview = "P",
            NoticeContent = "C"
        });
        await context.SaveChangesAsync();

        var auditLogs = await context.Set<AuditLog>().ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task AuditLog_ShouldStoreChangesJson()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);
        await context.Database.EnsureCreatedAsync();
        context.Set<Tenant>().Add(new Tenant { Id = 1, Name = "Test" });
        await context.SaveChangesAsync();

        context.Set<Notice>().Add(new Notice
        {
            TenantId = 1,
            NoticeTitle = "JSON Test",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        });
        await context.SaveChangesAsync();

        var auditLogs = await context.Set<AuditLog>().ToListAsync();
        var createdLog = auditLogs.FirstOrDefault(al => al.Action == "Created");
        createdLog.Should().NotBeNull();
        createdLog!.ChangesJson.Should().NotBeNullOrEmpty();
        createdLog.ChangesJson.Should().Contain("NoticeTitle");
        createdLog.UserName.Should().Be("Test User");
        createdLog.UserId.Should().Be("test-user-id");
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
