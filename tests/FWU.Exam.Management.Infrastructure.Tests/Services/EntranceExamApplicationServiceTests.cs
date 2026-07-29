using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class EntranceExamApplicationServiceTests : TestBase
{
    private (EntranceExamApplicationService Service, IUserContext UserContext) CreateService(AppDbContext context)
    {
        var mockUserStore = Substitute.For<IUserStore<AppUser>>();
        var userManager = Substitute.For<UserManager<AppUser>>(mockUserStore, null, null, null, null, null, null, null, null);
        var userContext = Substitute.For<IUserContext>();
        userContext.IsSuperAdmin.Returns(true);
        var emailService = Substitute.For<IEmailService>();
        var smsService = Substitute.For<ISmsService>();
        var service = new EntranceExamApplicationService(context, userManager, userContext, emailService, smsService);
        return (service, userContext);
    }

    private async Task SeedBaseEntitiesAsync(AppDbContext context)
    {
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);

        if (!await context.Set<College>().AnyAsync())
        {
            context.Set<College>().Add(new College
            {
                Id = 1,
                TenantId = TestTenantId,
                Code = "C001",
                Name = "Test College",
                Email = "college@test.com",
                PrincipalName = "Principal",
                PrincipalContactNumber = "9854321123",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Level>().AnyAsync())
        {
            context.Set<Level>().Add(new Level
            {
                Id = 1,
                LevelName = "Bachelor",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Program>().AnyAsync())
        {
            context.Set<Program>().Add(new Program
            {
                Id = 1,
                LevelId = 1,
                ProgramCode = "CSIT",
                ProgramName = "B.Sc. CSIT",
                ShortName = "CSIT",
                Duration = 4,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Gender>().AnyAsync())
        {
            context.Set<Gender>().Add(new Gender
            {
                Id = 1,
                GenderName = "Male",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }
    }

    private EntranceExamApplication CreateTestApplication()
    {
        return new EntranceExamApplication
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ProgramId = 1,
            GenderId = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirthBS = "2055/05/15",
            Email = "john.doe@test.com",
            ContactNumber = "9841234567",
            FatherName = "Father",
            MotherName = "Mother"
        };
    }

    [Fact]
    public async Task SubmitApplication_ShouldPersistAndReturnId()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var application = CreateTestApplication();
        var id = await service.SubmitApplicationAsync(application, null, null, null, null);

        id.Should().BeGreaterThan(0);

        var saved = await context.EntranceExamApplications.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(ApplicationStatus.Submitted);
        saved.FirstName.Should().Be("John");
        saved.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task SubmitApplication_ShouldCreateAddress_WhenProvided()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        if (!await context.Set<Province>().AnyAsync())
        {
            context.Set<Province>().Add(new Province { Id = 1, ProvinceName = "Sudurpashchim", ProvinceCode = "SP" });
            await context.SaveChangesAsync();
        }
        if (!await context.Set<District>().AnyAsync())
        {
            context.Set<District>().Add(new District { Id = 1, DistrictName = "Kailali", DistrictCode = "KL", ProvinceId = 1, IsActive = true });
            await context.SaveChangesAsync();
        }
        if (!await context.Set<LocalLevel>().AnyAsync())
        {
            context.Set<LocalLevel>().Add(new LocalLevel { Id = 1, LocalLevelName = "Dhangadhi", DistrictId = 1, IsActive = true });
            await context.SaveChangesAsync();
        }

        var (service, _) = CreateService(context);

        var application = CreateTestApplication();
        var id = await service.SubmitApplicationAsync(application, "1", "5", "Main Street", "101");

        id.Should().BeGreaterThan(0);

        var saved = await context.EntranceExamApplications.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.PermanentAddressId.Should().NotBeNull();

        var address = await context.Addresses.FindAsync(saved.PermanentAddressId);
        address.Should().NotBeNull();
        address!.LocalLevelId.Should().Be(1);
        address.WardNumber.Should().Be(5);
        address.ToleStreet.Should().Be("Main Street");
        address.HouseNumber.Should().Be("101");
        address.AddressType.Should().Be(AddressType.Permanent);
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnApplication_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var application = CreateTestApplication();
        var id = await service.SubmitApplicationAsync(application, null, null, null, null);
        context.ChangeTracker.Clear();

        var result = await service.GetApplicationByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnNull_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var result = await service.GetApplicationByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationByVoucherId_ShouldReturnApplication()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        if (!await context.Set<ExamType>().AnyAsync())
        {
            context.Set<ExamType>().Add(new ExamType { Id = 1, Name = "Entrance", Code = "4", IsActive = true });
            await context.SaveChangesAsync();
        }
        if (!await context.Set<Semester>().AnyAsync())
        {
            context.Set<Semester>().Add(new Semester { Id = 1, Number = 1, Year = 1, Name = "First", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1), AcademicYearId = 1 });
            await context.SaveChangesAsync();
        }
        if (!await context.Set<ExamSchedule>().AnyAsync())
        {
            context.Set<ExamSchedule>().Add(new ExamSchedule
            {
                Id = 1,
                TenantId = TestTenantId,
                ExamScheduleName = "Test Schedule",
                AcademicYearId = 1,
                ProgramId = 1,
                SemesterId = 1,
                ExamTypeId = 1,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0),
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        var voucher = new ApplicationVoucher
        {
            TenantId = TestTenantId,
            VoucherNumber = "VCH-001",
            StudentName = "John Doe",
            ContactNumber = "9841234567",
            Amount = 1500,
            VoucherDate = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
            ExamScheduleId = 1
        };
        context.ApplicationVouchers.Add(voucher);
        await context.SaveChangesAsync();

        var (service, _) = CreateService(context);

        var application = CreateTestApplication();
        application.ApplicationVoucherId = voucher.Id;
        application.PaymentVerified = true;
        var appId = await service.SubmitApplicationAsync(application, null, null, null, null);
        context.ChangeTracker.Clear();

        var result = await service.GetApplicationByVoucherIdAsync(voucher.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(appId);
        result.ApplicationVoucherId.Should().Be(voucher.Id);
    }

    [Fact]
    public async Task GetAllApplications_ShouldReturnPaginatedResults()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        for (int i = 1; i <= 5; i++)
        {
            var app = new EntranceExamApplication
            {
                TenantId = TestTenantId,
                AcademicYearId = 1,
                CollegeId = 1,
                ProgramId = 1,
                GenderId = 1,
                FirstName = $"Student{i}",
                LastName = "Test",
                DateOfBirthBS = "2055/05/15",
                Email = $"student{i}@test.com",
                ContactNumber = $"984123456{i}",
                Status = ApplicationStatus.Submitted,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            context.EntranceExamApplications.Add(app);
        }
        await context.SaveChangesAsync();

        var (data, totalCount) = await service.GetPagedApplicationsAsync(null, null, null, null, 1, 2);

        totalCount.Should().Be(5);
        data.Should().HaveCount(2);
        data[0].FullName.Should().Be("Student1 Test");
    }

    [Fact]
    public async Task DeleteApplication_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var application = CreateTestApplication();
        var id = await service.SubmitApplicationAsync(application, null, null, null, null);

        var existsBefore = await service.ApplicationExistsAsync(id);
        existsBefore.Should().BeTrue();

        await service.DeleteApplicationAsync(id);

        var existsAfter = await service.ApplicationExistsAsync(id);
        existsAfter.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedApplications_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var app1 = CreateTestApplication();
        app1.FirstName = "Alice";
        app1.Email = "alice@test.com";
        await service.SubmitApplicationAsync(app1, null, null, null, null);

        var app2 = CreateTestApplication();
        app2.FirstName = "Bob";
        app2.LastName = "Smith";
        app2.Email = "bob@test.com";
        await service.SubmitApplicationAsync(app2, null, null, null, null);

        var (data, totalCount) = await service.GetPagedApplicationsAsync("Alice", null, null, null, 1, 10);

        totalCount.Should().Be(1);
        data.Should().HaveCount(1);
        data[0].FullName.Should().Be("Alice Doe");
    }

    [Fact]
    public async Task GetPagedApplications_WithStatusFilter_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var app1 = CreateTestApplication();
        app1.Email = "approved@test.com";
        var id1 = await service.SubmitApplicationAsync(app1, null, null, null, null);

        var app2 = CreateTestApplication();
        app2.FirstName = "Jane";
        app2.Email = "rejected@test.com";
        var id2 = await service.SubmitApplicationAsync(app2, null, null, null, null);

        await service.ReviewApplicationAsync(id1, ApplicationStatus.Approved, null);
        await service.ReviewApplicationAsync(id2, ApplicationStatus.Rejected, "Not eligible");

        var (data, totalCount) = await service.GetPagedApplicationsAsync(null, ApplicationStatus.Approved, null, null, 1, 10);

        totalCount.Should().Be(1);
        data[0].Email.Should().Be("approved@test.com");
    }
}
