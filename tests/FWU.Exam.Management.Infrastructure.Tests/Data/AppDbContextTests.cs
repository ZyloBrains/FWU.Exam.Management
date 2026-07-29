using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Data;

public class AppDbContextTests : TestBase
{
    [Fact]
    public async Task Database_Create_ShouldCreateTables()
    {
        using var context = await CreateContextAsync();

        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAndRetrieve_AcademicYear_ShouldWork()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var year = new AcademicYear
        {
            AcademicYearCode = "2081/082",
            AcademicYearName = "2081/082",
            AcademicYearNameNepali = "२०८१/०८२",
            IsRunning = true,
            IsActive = true
        };
        context.Set<AcademicYear>().Add(year);
        await context.SaveChangesAsync();

        var saved = await context.Set<AcademicYear>().FindAsync(year.Id);
        saved.Should().NotBeNull();
        saved!.AcademicYearCode.Should().Be("2081/082");
    }

    [Fact]
    public async Task SaveAndRetrieve_Tenant_ShouldWork()
    {
        using var context = await CreateContextAsync();

        var tenant = new Tenant
        {
            Name = "Test University",
            OfficeCode = "TU001",
            ContactNumber = "01-5550000",
            Address = "Test Address",
            Email = "test@test.edu.np",
            TenantType = TenantType.Standard,
            IsActive = true
        };
        context.Set<Tenant>().Add(tenant);
        await context.SaveChangesAsync();

        var saved = await context.Set<Tenant>().FindAsync(tenant.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test University");
    }

    [Fact]
    public async Task SaveAndRetrieve_Notice_ShouldWork()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var notice = new Notice
        {
            TenantId = TestTenantId,
            NoticeTitle = "Test Notice",
            NoticePreview = "Preview text",
            NoticeContent = "Full content here"
        };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();

        var saved = await context.Set<Notice>().FindAsync(notice.Id);
        saved.Should().NotBeNull();
        saved!.NoticeTitle.Should().Be("Test Notice");
        saved.TenantId.Should().Be(TestTenantId);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldFilterByTenant()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var conn2 = new SqliteConnection("DataSource=:memory:");
        conn2.Open();
        var ctx2Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn2)
            .Options;

        var tenant2Context = NSubstitute.Substitute.For<Domain.Interfaces.ITenantContext>();
        tenant2Context.TenantId.Returns(2);
        tenant2Context.TenantCode.Returns("T2");
        tenant2Context.IsCentralTenant.Returns(false);

        context.Set<Tenant>().Add(new Tenant { Id = 2, Name = "Tenant 2", OfficeCode = "T2", ContactNumber = "000", Address = "Addr", Email = "t2@test.com" });
        await context.SaveChangesAsync();

        var context2 = new AppDbContext(ctx2Options, null, tenant2Context);
        await context2.Database.EnsureCreatedAsync();
        context2.Set<Tenant>().Add(new Tenant { Id = 2, Name = "Tenant 2", OfficeCode = "T2b", ContactNumber = "000", Address = "Addr", Email = "t2b@test.com" });
        await context2.SaveChangesAsync();

        context.Set<Notice>().Add(new Notice { TenantId = TestTenantId, NoticeTitle = "T1 Notice", NoticePreview = "P1", NoticeContent = "C1" });
        context.Set<Notice>().Add(new Notice { TenantId = 2, NoticeTitle = "T2 Notice", NoticePreview = "P2", NoticeContent = "C2" });
        await context.SaveChangesAsync();

        var noticesForTenant1 = await context.Set<Notice>().ToListAsync();
        noticesForTenant1.Should().AllSatisfy(n => n.TenantId.Should().Be(TestTenantId));

        conn2.Close();
        conn2.Dispose();
        context2.Dispose();
    }

    [Fact]
    public async Task SaveAndRetrieve_Province_ShouldWork()
    {
        using var context = await CreateContextAsync();

        var province = new Province
        {
            ProvinceName = "Sudurpashchim Province",
            ProvinceCode = "SP"
        };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();

        var saved = await context.Set<Province>().FindAsync(province.Id);
        saved.Should().NotBeNull();
        saved!.ProvinceName.Should().Be("Sudurpashchim Province");
    }

    [Fact]
    public async Task SaveAndRetrieve_College_ShouldWork()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var college = new College
        {
            TenantId = TestTenantId,
            Name = "Test College",
            Code = "TC001",
            IsActive = true
        };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();

        var saved = await context.Set<College>().FindAsync(college.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test College");
    }
}
