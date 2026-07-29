using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class AdmitCardServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private async Task SeedMinimalDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelCode = "BACH", LevelName = "Bachelor", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Final", Code = "FIN", IsActive = true });
        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        context.Set<Gender>().Add(new Gender { GenderName = "Male", IsActive = true });
        context.Set<StudentCategory>().Add(new StudentCategory { StudentCategoryName = "Regular", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "Bachelor of Computer Application", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<College>().Add(new College { TenantId = TestTenantId, Code = "FWU01", Name = "Far Western University College", Email = "college@fwu.edu.np", PrincipalName = "Principal", PrincipalContactNumber = "9800000000", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamSchedule>().Add(new ExamSchedule { TenantId = TestTenantId, ExamScheduleName = "2081 BCA Final", AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1, IsActive = true, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(11, 0) });
        await context.SaveChangesAsync();

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, Code = "EC001", IsActive = true });
        await context.SaveChangesAsync();
    }

    private async Task<int> SeedExamRegistrationAsync(AppDbContext context, string examRollNumber, string? symbolNumber = null)
    {
        var er = new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = examRollNumber,
            SymbolNumber = symbolNumber,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Registered,
            IsActive = true,
            IsAppliedByStudent = true
        };
        context.Set<ExamRegistration>().Add(er);
        await context.SaveChangesAsync();
        return er.Id;
    }

    [Fact]
    public async Task GetAdmitCards_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        for (int i = 1; i <= 3; i++)
        {
            context.Set<AdmitCard>().Add(new AdmitCard
            {
                TenantId = TestTenantId,
                ExamRegistrationId = regId,
                ExamScheduleId = 1,
                AdmitCardNumber = $"AC-0001-{regId:D6}-{i}",
                GeneratedDate = DateTime.UtcNow.AddDays(-i),
                IsActive = true
            });
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var (items, totalCount) = await service.GetAdmitCardsAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdmitCards_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000002",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var (items, totalCount) = await service.GetAdmitCardsAsync(1, 10, "000001", "Id", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].AdmitCardNumber.Should().Be("AC-0001-000001");
    }

    [Fact]
    public async Task GetFilteredItems_ShouldReturnMatching()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000002",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var items = await service.GetFilteredItemsAsync("000001");

        items.Should().HaveCount(1);
        items[0].AdmitCardNumber.Should().Be("AC-0001-000001");
    }

    [Fact]
    public async Task GenerateAdmitCard_ShouldCreateAdmitCard()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var admitCard = await service.GenerateAdmitCardAsync(regId);

        admitCard.Should().NotBeNull();
        admitCard.ExamRegistrationId.Should().Be(regId);
        admitCard.ExamScheduleId.Should().Be(1);
        admitCard.AdmitCardNumber.Should().Be($"AC-0001-{regId:D6}");
        admitCard.ExamRollNo.Should().Be("SYM-001");
        admitCard.IsDownloaded.Should().BeFalse();
        admitCard.IsActive.Should().BeTrue();
        admitCard.GeneratedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var saved = await context.Set<AdmitCard>().FindAsync(admitCard.Id);
        saved.Should().NotBeNull();
        saved!.AdmitCardNumber.Should().Be(admitCard.AdmitCardNumber);
    }

    [Fact]
    public async Task GenerateAdmitCard_ShouldFail_WhenExamRegistrationNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var act = () => service.GenerateAdmitCardAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Exam registration not found.");
    }

    [Fact]
    public async Task GenerateAdmitCard_ShouldFail_WhenSymbolNumberMissing()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001");
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var act = () => service.GenerateAdmitCardAsync(regId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*symbol number*");
    }

    [Fact]
    public async Task CreateAdmitCard_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var entity = new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        };

        await service.CreateAdmitCardAsync(entity);

        var saved = await context.Set<AdmitCard>().FindAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.AdmitCardNumber.Should().Be("AC-0001-000001");
    }

    [Fact]
    public async Task UpdateAdmitCard_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var existing = await service.GetAdmitCardByIdAsync(1);
        existing.Should().NotBeNull();

        existing!.IsDownloaded = true;
        existing.DownloadedDate = DateTime.UtcNow;
        await service.UpdateAdmitCardAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetAdmitCardByIdAsync(1);
        updated!.IsDownloaded.Should().BeTrue();
        updated.DownloadedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAdmitCard_ShouldSetInactive()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        await service.DeleteAdmitCardAsync(1);

        var deleted = await context.Set<AdmitCard>().FindAsync(1);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AdmitCardExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var regId = await SeedExamRegistrationAsync(context, "RN-001", "SYM-001");

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = regId,
            ExamScheduleId = 1,
            AdmitCardNumber = "AC-0001-000001",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var exists = await service.AdmitCardExistsAsync(1);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AdmitCardExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var userContext = CreateSuperAdminContext();
        var service = new AdmitCardService(context, userContext);

        var exists = await service.AdmitCardExistsAsync(999);
        exists.Should().BeFalse();
    }
}
