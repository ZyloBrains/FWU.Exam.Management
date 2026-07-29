using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class StudentRegistrationServiceTests : TestBase
{
    private (StudentRegistrationService Service, IUserContext UserContext) CreateService(AppDbContext context)
    {
        var mockUserStore = Substitute.For<IUserStore<AppUser>>();
        var userManager = Substitute.For<UserManager<AppUser>>(mockUserStore, null, null, null, null, null, null, null, null);
        var userContext = Substitute.For<IUserContext>();
        userContext.IsSuperAdmin.Returns(true);
        var emailService = Substitute.For<IEmailService>();
        var smsService = Substitute.For<ISmsService>();
        var service = new StudentRegistrationService(context, userManager, NullLogger<StudentRegistrationService>.Instance, emailService, smsService, userContext);
        return (service, userContext);
    }

    private async Task SeedBaseEntitiesAsync(AppDbContext context)
    {
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);

        if (!await context.Set<Level>().AnyAsync())
        {
            context.Set<Level>().Add(new Level { Id = 1, LevelName = "Bachelor", LevelCode = "B", IsActive = true });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Gender>().AnyAsync())
        {
            context.Set<Gender>().Add(new Gender { Id = 1, GenderName = "Male", IsActive = true });
            context.Set<Gender>().Add(new Gender { Id = 2, GenderName = "Female", IsActive = true });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<StudentCategory>().AnyAsync())
        {
            context.Set<StudentCategory>().Add(new StudentCategory { Id = 1, StudentCategoryName = "General", IsActive = true });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Ethnicity>().AnyAsync())
        {
            context.Set<Ethnicity>().Add(new Ethnicity { Id = 1, EthnicityName = "Other", IsActive = true });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<Faculty>().AnyAsync())
        {
            context.Set<Faculty>().Add(new Faculty
            {
                Id = 1,
                Name = "Science and Technology",
                OfficeCode = "SCI",
                ShortName = "SCI",
                ContactNumber = "01-5550001",
                Address = "Mahendranagar",
                Email = "science@fwu.edu.np",
                TenantId = TestTenantId
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Set<College>().AnyAsync())
        {
            context.Set<College>().Add(new College
            {
                Id = 1,
                TenantId = TestTenantId,
                Code = "C001",
                Name = "Test College A",
                Email = "collegea@test.com",
                PrincipalName = "Principal A",
                PrincipalContactNumber = "9854321123",
                IsActive = true
            });
            context.Set<College>().Add(new College
            {
                Id = 2,
                TenantId = TestTenantId,
                Code = "C002",
                Name = "Test College B",
                Email = "collegeb@test.com",
                PrincipalName = "Principal B",
                PrincipalContactNumber = "9854321124",
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
                FacultyId = 1,
                ProgramCode = "CSIT",
                ProgramName = "B.Sc. CSIT",
                ShortName = "CSIT",
                Duration = 4,
                IsActive = true
            });
            context.Set<Program>().Add(new Program
            {
                Id = 2,
                LevelId = 1,
                FacultyId = 1,
                ProgramCode = "ECE",
                ProgramName = "B.E. Electronics",
                ShortName = "ECE",
                Duration = 4,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }
    }

    private StudentRegistration CreateStudentRegistration(int collegeId = 1, int index = 1)
    {
        return new StudentRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            LevelId = 1,
            CollegeId = collegeId,
            FacultyId = 1,
            ProgramId = 1,
            GenderId = 1,
            StudentCategoryId = 1,
            EthnicityId = 1,
            FirstName = $"Student{index}",
            LastName = "Test",
            Email = $"student{index}@test.com",
            ContactNumber = $"984123456{index}",
            DateOfBirthBS = "2055/05/15",
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllStudentRegistrations_ShouldReturnAll()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        context.Set<StudentRegistration>().Add(CreateStudentRegistration(1, 1));
        context.Set<StudentRegistration>().Add(CreateStudentRegistration(1, 2));
        await context.SaveChangesAsync();

        var (service, _) = CreateService(context);

        var results = await service.GetAllStudentRegistrationsAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllStudentRegistrations_WithCollegeFilter_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        context.Set<StudentRegistration>().Add(CreateStudentRegistration(1, 1));
        context.Set<StudentRegistration>().Add(CreateStudentRegistration(2, 2));
        await context.SaveChangesAsync();

        var (service, _) = CreateService(context);

        var results = await service.GetAllStudentRegistrationsAsync(new List<int> { 1 });

        results.Should().HaveCount(1);
        results[0].CollegeId.Should().Be(1);
    }

    [Fact]
    public async Task GetStudentRegistrationById_ShouldReturn_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        var registration = CreateStudentRegistration(1, 1);
        context.Set<StudentRegistration>().Add(registration);
        await context.SaveChangesAsync();
        var id = registration.Id;

        var (service, _) = CreateService(context);

        var result = await service.GetStudentRegistrationByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.FirstName.Should().Be("Student1");
    }

    [Fact]
    public async Task GetStudentRegistrationById_ShouldReturnNull_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);
        var (service, _) = CreateService(context);

        var result = await service.GetStudentRegistrationByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFilteredItems_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        context.Set<StudentRegistration>().Add(CreateStudentRegistration(1, 1));
        var reg2 = CreateStudentRegistration(1, 2);
        reg2.FirstName = "Alice";
        reg2.Email = "alice@test.com";
        reg2.RegistrationNumber = "REG-001";
        context.Set<StudentRegistration>().Add(reg2);
        await context.SaveChangesAsync();

        var (service, _) = CreateService(context);

        var (data, totalCount) = await service.GetPagedDataAsync("Alice", 1, 10);

        totalCount.Should().Be(1);
        data.Should().HaveCount(1);
        data[0].FullName.Should().Be("Alice Test");
    }

    [Fact]
    public async Task DeleteStudentRegistration_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedBaseEntitiesAsync(context);

        var registration = CreateStudentRegistration(1, 1);
        context.Set<StudentRegistration>().Add(registration);
        await context.SaveChangesAsync();
        var id = registration.Id;

        var (service, _) = CreateService(context);

        var existsBefore = await service.StudentRegistrationExistsAsync(id);
        existsBefore.Should().BeTrue();

        await service.DeleteStudentRegistrationAsync(id);

        var existsAfter = await service.StudentRegistrationExistsAsync(id);
        existsAfter.Should().BeFalse();
    }
}
