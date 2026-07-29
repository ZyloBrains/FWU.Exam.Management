using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class BulkUserCreationServiceTests : TestBase
{
    [Fact]
    public async Task GetStudentsWithoutUsers_ShouldReturnStudents_WhenNoUsersExist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var genderId = await SeedGenderAsync(context);
        var catId = await SeedStudentCategoryAsync(context);
        var collegeId = await SeedCollegeAsync(context);
        var ayId = await SeedAcademicYearAsync(context);

        var student = new StudentRegistration
        {
            FirstName = "Ram",
            LastName = "Sharma",
            Email = "ram@test.com",
            RegistrationNumber = "REG001",
            IsActive = true,
            DateOfBirthBS = "2055-01-01",
            LevelId = levelId,
            GenderId = genderId,
            CollegeId = collegeId,
            StudentCategoryId = catId,
            AcademicYearId = ayId,
            TenantId = TestTenantId
        };
        context.Set<StudentRegistration>().Add(student);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var tenantContext = Substitute.For<ITenantContext>();
        var logger = NullLogger<BulkUserCreationService>.Instance;

        var service = new BulkUserCreationService(context, null!, scopeFactory, tenantContext, logger);

        var (data, totalCount) = await service.GetStudentsWithoutUsersAsync(null, null, 1, 10);

        totalCount.Should().Be(1);
        data.Should().HaveCount(1);
        data[0].Email.Should().Be("ram@test.com");
    }

    [Fact]
    public async Task GetStudentsWithoutUsers_ShouldExcludeStudents_WhenUserAlreadyExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var genderId = await SeedGenderAsync(context);
        var catId = await SeedStudentCategoryAsync(context);
        var collegeId = await SeedCollegeAsync(context);
        var ayId = await SeedAcademicYearAsync(context);

        var existingUser = new AppUser { UserName = "ram@test.com", Email = "ram@test.com" };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var student = new StudentRegistration
        {
            FirstName = "Ram",
            LastName = "Sharma",
            Email = "ram@test.com",
            RegistrationNumber = "REG001",
            IsActive = true,
            DateOfBirthBS = "2055-01-01",
            LevelId = levelId,
            GenderId = genderId,
            CollegeId = collegeId,
            StudentCategoryId = catId,
            AcademicYearId = ayId,
            TenantId = TestTenantId
        };
        context.Set<StudentRegistration>().Add(student);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var tenantContext = Substitute.For<ITenantContext>();
        var logger = NullLogger<BulkUserCreationService>.Instance;

        var service = new BulkUserCreationService(context, null!, scopeFactory, tenantContext, logger);

        var (data, totalCount) = await service.GetStudentsWithoutUsersAsync(null, null, 1, 10);

        totalCount.Should().Be(0);
        data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStudentsWithoutUsers_ShouldFilterByCollege()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var genderId = await SeedGenderAsync(context);
        var catId = await SeedStudentCategoryAsync(context);
        var collegeId = await SeedCollegeAsync(context);
        var collegeId2 = await SeedCollegeAsync(context);
        var ayId = await SeedAcademicYearAsync(context);

        context.Set<StudentRegistration>().AddRange(
            new StudentRegistration
            {
                FirstName = "Ram",
                LastName = "Sharma",
                Email = "ram@test.com",
                RegistrationNumber = "REG001",
                IsActive = true,
                CollegeId = collegeId,
                DateOfBirthBS = "2055-01-01",
                LevelId = levelId,
                StudentCategoryId = catId,
                AcademicYearId = ayId,
                GenderId = genderId,
                TenantId = TestTenantId
            },
            new StudentRegistration
            {
                FirstName = "Shyam",
                LastName = "Sharma",
                Email = "shyam@test.com",
                RegistrationNumber = "REG002",
                IsActive = true,
                CollegeId = collegeId2,
                DateOfBirthBS = "2056-01-01",
                LevelId = levelId,
                StudentCategoryId = catId,
                AcademicYearId = ayId,
                GenderId = genderId,
                TenantId = TestTenantId
            }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var tenantContext = Substitute.For<ITenantContext>();
        var logger = NullLogger<BulkUserCreationService>.Instance;

        var service = new BulkUserCreationService(context, null!, scopeFactory, tenantContext, logger);

        var (data, totalCount) = await service.GetStudentsWithoutUsersAsync(collegeId, null, 1, 10);

        totalCount.Should().Be(1);
        data[0].RegistrationNumber.Should().Be("REG001");
    }

    [Fact]
    public async Task StartJob_ShouldCreateJobAndReturn()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var genderId = await SeedGenderAsync(context);
        var catId = await SeedStudentCategoryAsync(context);
        var collegeId = await SeedCollegeAsync(context);
        var ayId = await SeedAcademicYearAsync(context);

        var student = new StudentRegistration
        {
            FirstName = "Ram",
            LastName = "Sharma",
            Email = "ram@test.com",
            RegistrationNumber = "REG001",
            IsActive = true,
            DateOfBirthBS = "2055-01-01",
            LevelId = levelId,
            GenderId = genderId,
            CollegeId = collegeId,
            StudentCategoryId = catId,
            AcademicYearId = ayId,
            TenantId = TestTenantId
        };
        context.Set<StudentRegistration>().Add(student);
        await context.SaveChangesAsync();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(TestTenantId);
        tenantContext.TenantCode.Returns(TestTenantCode);
        tenantContext.Type.Returns(TenantType.Standard);
        var logger = NullLogger<BulkUserCreationService>.Instance;

        var service = new BulkUserCreationService(context, null!, scopeFactory, tenantContext, logger);

        var job = await service.StartJobAsync([student.Id], "user1");

        job.Should().NotBeNull();
        job.TotalStudents.Should().Be(1);
        job.UserId.Should().Be("user1");
        job.Status.Should().Be("Running");
    }

    [Fact]
    public async Task GetJobStatus_ShouldReturnJob_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var job = new BulkUserCreationJob
        {
            UserId = "user1",
            TotalStudents = 5,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        };
        context.Set<BulkUserCreationJob>().Add(job);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var tenantContext = Substitute.For<ITenantContext>();
        var logger = NullLogger<BulkUserCreationService>.Instance;

        var service = new BulkUserCreationService(context, null!, scopeFactory, tenantContext, logger);

        var result = await service.GetJobStatusAsync(job.Id);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.TotalStudents.Should().Be(5);
    }
}
