using System;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Interceptors;

public class TenantSaveChangesInterceptorTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly ITenantContext _tenantContext;
    private readonly TenantSaveChangesInterceptor _interceptor;

    public TenantSaveChangesInterceptorTests()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantContext = Substitute.For<ITenantContext>();
        _tenantContext.TenantId.Returns(42);

        _interceptor = new TenantSaveChangesInterceptor(
            _tenantContext,
            NullLogger<TenantSaveChangesInterceptor>.Instance);

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_interceptor)
            .Options;

        // Seed Tenants for FK references
        using var seedContext = new AppDbContext(_options, null, _tenantContext);
        seedContext.Database.EnsureCreated();
        seedContext.Set<Tenant>().Add(new Tenant { Id = 42, Name = "Test", OfficeCode = "T42", ContactNumber = "000", Address = "Addr", Email = "t42@test.com" });
        seedContext.Set<Tenant>().Add(new Tenant { Id = 99, Name = "Other", OfficeCode = "T99", ContactNumber = "000", Address = "Addr", Email = "t99@test.com" });
        seedContext.SaveChanges();
    }

    [Fact]
    public async Task SavingChanges_ShouldSetTenantId_OnNewTenantScopedEntities()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);

        var notice = new Notice
        {
            NoticeTitle = "Test",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();

        var saved = await context.Set<Notice>().FindAsync(notice.Id);
        saved.Should().NotBeNull();
        saved!.TenantId.Should().Be(42);
    }

    [Fact]
    public async Task SavingChanges_ShouldNotOverride_ExistingTenantId()
    {
        using var context = new AppDbContext(_options, null, _tenantContext);

        var notice = new Notice
        {
            TenantId = 99,
            NoticeTitle = "Other Tenant",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();

        var saved = await context.Set<Notice>().FindAsync(notice.Id);
        saved!.TenantId.Should().Be(99);
    }

    [Fact]
    public async Task SavingChanges_ShouldNotThrow_WhenTenantIdIsZero()
    {
        var zeroTenantContext = Substitute.For<ITenantContext>();
        zeroTenantContext.TenantId.Returns(0);

        var interceptor = new TenantSaveChangesInterceptor(
            zeroTenantContext,
            NullLogger<TenantSaveChangesInterceptor>.Instance);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        // Need a Tenant with Id=1 for FK reference
        using var seedContext = new AppDbContext(options, null, zeroTenantContext);
        await seedContext.Database.EnsureCreatedAsync();
        seedContext.Set<Tenant>().Add(new Tenant { Id = 1, Name = "Test", OfficeCode = "ZRO", ContactNumber = "000", Address = "Addr", Email = "zro@test.com" });
        await seedContext.SaveChangesAsync();

        using var context = new AppDbContext(options, null, zeroTenantContext);

        var notice = new Notice
        {
            TenantId = 1,
            NoticeTitle = "Test",
            NoticePreview = "Preview",
            NoticeContent = "Content"
        };
        context.Set<Notice>().Add(notice);

        Func<Task> act = async () => await context.SaveChangesAsync();
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
