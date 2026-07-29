using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Threading;

namespace FWU.Exam.Management.Infrastructure.Tests;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private AppDbContext? _context;

    protected ITenantContext TenantContext { get; }
    protected ITenantContext CentralTenantContext { get; }
    protected int TestTenantId => 1;
    protected string TestTenantCode => "FWU";

    protected TestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        TenantContext = Substitute.For<ITenantContext>();
        TenantContext.TenantId.Returns(TestTenantId);
        TenantContext.TenantCode.Returns(TestTenantCode);
        TenantContext.Type.Returns(TenantType.Standard);
        TenantContext.IsCentralTenant.Returns(false);

        CentralTenantContext = Substitute.For<ITenantContext>();
        CentralTenantContext.TenantId.Returns(1);
        CentralTenantContext.TenantCode.Returns("Central");
        CentralTenantContext.Type.Returns(TenantType.Central);
        CentralTenantContext.IsCentralTenant.Returns(true);
    }

    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new TenantSaveChangesInterceptor(TenantContext, NullLogger<TenantSaveChangesInterceptor>.Instance))
            .Options;

        var context = new AppDbContext(options, null, TenantContext);
        context.Database.EnsureCreated();
        return context;
    }

    protected AppDbContext CreateCentralContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new TenantSaveChangesInterceptor(CentralTenantContext, NullLogger<TenantSaveChangesInterceptor>.Instance))
            .Options;

        var context = new AppDbContext(options, null, CentralTenantContext);
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task<AppDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new TenantSaveChangesInterceptor(TenantContext, NullLogger<TenantSaveChangesInterceptor>.Instance))
            .Options;

        var context = new AppDbContext(options, null, TenantContext);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    protected async Task SeedTenantAsync(AppDbContext context)
    {
        if (!await context.Set<Tenant>().AnyAsync(t => t.Id == TestTenantId))
        {
            context.Set<Tenant>().Add(new Tenant
            {
                Id = TestTenantId,
                Name = "Far Western University",
                OfficeCode = "FWU001",
                ContactNumber = "01-5551234",
                Address = "Mahendranagar",
                Email = "info@fwu.edu.np",
                TenantType = TenantType.Standard,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }
    }

    protected async Task<int> SeedAcademicYearAsync(AppDbContext context)
    {
        var existing = await context.Set<AcademicYear>().FirstOrDefaultAsync();
        if (existing != null) return existing.Id;

        var ay = new AcademicYear
        {
            AcademicYearCode = "2081/082",
            AcademicYearName = "2081/082",
            AcademicYearCodeNepali = "२०८१/०८२",
            AcademicYearNameNepali = "२०८१/०८२",
            IsRunning = true,
            IsActive = true
        };
        context.Set<AcademicYear>().Add(ay);
        await context.SaveChangesAsync();
        return ay.Id;
    }

    private static int _collegeCounter;
    protected async Task<int> SeedCollegeAsync(AppDbContext context)
    {
        var c = Interlocked.Increment(ref _collegeCounter);
        var college = new Domain.Entities.Colleges.College
        {
            TenantId = TestTenantId,
            Code = $"CLG{c}",
            Name = $"Test College {c}",
            Email = $"clg{c}@test.com",
            PrincipalName = "P1",
            PrincipalContactNumber = "123",
            IsActive = true
        };
        context.Set<Domain.Entities.Colleges.College>().Add(college);
        await context.SaveChangesAsync();
        return college.Id;
    }

    protected async Task<int> SeedLevelAsync(AppDbContext context)
    {
        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);
        await context.SaveChangesAsync();
        return level.Id;
    }

    protected async Task<int> SeedProvinceAsync(AppDbContext context)
    {
        var province = new Province { ProvinceName = "Sudurpashchim", ProvinceCode = "SP", IsActive = true };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();
        return province.Id;
    }

    protected async Task<int> SeedGenderAsync(AppDbContext context)
    {
        var gender = new Gender { GenderName = "Male", IsActive = true };
        context.Set<Gender>().Add(gender);
        await context.SaveChangesAsync();
        return gender.Id;
    }

    protected async Task<int> SeedStudentCategoryAsync(AppDbContext context)
    {
        var cat = new StudentCategory { StudentCategoryName = "General", IsActive = true };
        context.Set<StudentCategory>().Add(cat);
        await context.SaveChangesAsync();
        return cat.Id;
    }

    public void Dispose()
    {
        _context?.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
