using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class StudentAdmissionServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private UserManager<AppUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<AppUser>>();
        var mgr = Substitute.For<UserManager<AppUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.FindByEmailAsync(Arg.Any<string>()).Returns((AppUser?)null);
        return mgr;
    }

    [Fact]
    public async Task CreateAdmissionAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);

        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);

        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var admission = new StudentAdmission
        {
            TenantId = TestTenantId,
            ProgramsId = program.Id,
            CollegeId = college.Id,
            AcademicYearId = academicYear.Id,
            AdmissionDate = DateTime.UtcNow,
            IsActive = true,
            CollegeRollNumber = "CLG001"
        };

        await service.CreateAdmissionAsync(admission);

        var result = await service.GetAdmissionByIdAsync(admission.Id);
        result.Should().NotBeNull();
        result!.CollegeRollNumber.Should().Be("CLG001");
    }

    [Fact]
    public async Task GetAdmissionsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        for (int i = 1; i <= 3; i++)
        {
            context.Set<StudentAdmission>().Add(new StudentAdmission
            {
                TenantId = TestTenantId, ProgramsId = program.Id, CollegeId = college.Id,
                AcademicYearId = academicYear.Id, AdmissionDate = DateTime.UtcNow, IsActive = true,
                CollegeRollNumber = $"CLG{i:D3}"
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetAdmissionsAsync(1, 2, null, "admissiondate", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAdmissionAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var admission = new StudentAdmission { TenantId = TestTenantId, ProgramsId = program.Id, CollegeId = college.Id, AcademicYearId = academicYear.Id, AdmissionDate = DateTime.UtcNow, IsActive = true, CollegeRollNumber = "CLG001" };
        context.Set<StudentAdmission>().Add(admission);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        admission.IsCompleted = true;
        await service.UpdateAdmissionAsync(admission);

        var updated = await service.GetAdmissionByIdAsync(admission.Id);
        updated!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAdmissionAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var admission = new StudentAdmission { TenantId = TestTenantId, ProgramsId = program.Id, CollegeId = college.Id, AcademicYearId = academicYear.Id, AdmissionDate = DateTime.UtcNow, IsActive = true, CollegeRollNumber = "CLG001" };
        context.Set<StudentAdmission>().Add(admission);
        await context.SaveChangesAsync();

        await service.DeleteAdmissionAsync(admission.Id);

        var exists = await service.AdmissionExistsAsync(admission.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAdmissionAsync_ShouldSetCompleted()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var admission = new StudentAdmission { TenantId = TestTenantId, ProgramsId = program.Id, CollegeId = college.Id, AcademicYearId = academicYear.Id, AdmissionDate = DateTime.UtcNow, IsActive = true, CollegeRollNumber = "CLG001" };
        context.Set<StudentAdmission>().Add(admission);
        await context.SaveChangesAsync();

        await service.CompleteAdmissionAsync(admission.Id, "99");

        var result = await service.GetAdmissionByIdAsync(admission.Id);
        result!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetCollegeSelectListAsync_ShouldReturnColleges()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var userManager = CreateUserManager();
        var service = new StudentAdmissionService(context, userManager, userCtx);

        context.Set<College>().Add(new College { Code = "C1", Name = "College 1", Email = "c1@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId });
        context.Set<College>().Add(new College { Code = "C2", Name = "College 2", Email = "c2@test.com", PrincipalName = "P2", PrincipalContactNumber = "456", IsActive = true, TenantId = TestTenantId });
        await context.SaveChangesAsync();

        var items = await service.GetCollegeSelectListAsync();

        items.Should().HaveCount(2);
    }
}
