using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class DashboardServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private static UserManager<AppUser> CreateUserManager(IQueryable<AppUser> users)
    {
        var store = Substitute.For<IQueryableUserStore<AppUser>>();
        store.Users.Returns(users);

        var options = Substitute.For<IOptions<IdentityOptions>>();
        options.Value.Returns(new IdentityOptions());

        var mgr = Substitute.For<UserManager<AppUser>>(
            store,
            options,
            Substitute.For<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<AppUser>>>());
        mgr.Users.Returns(users);
        return mgr;
    }

    private static RoleManager<IdentityRole> CreateRoleManager(IQueryable<IdentityRole> roles)
    {
        var store = Substitute.For<IQueryableRoleStore<IdentityRole>>();
        store.Roles.Returns(roles);

        var mgr = Substitute.For<RoleManager<IdentityRole>>(
            store,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<ILogger<RoleManager<IdentityRole>>>());
        mgr.Roles.Returns(roles);
        return mgr;
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturnCounts()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);

        var bank = new Bank { BankName = "Test Bank", BankCode = "TB", IsActive = true };
        context.Set<Bank>().Add(bank);

        var country = new Country { CountryName = "Nepal", IsActive = true };
        context.Set<Country>().Add(country);

        var board = new Board { BoardName = "Test Board", IsActive = true, Country = country };
        context.Set<Board>().Add(board);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);

        var subjectCatalog = new SubjectCatalog { SubjectCode = "MTH101", SubjectName = "Mathematics", SubjectType = subjectType, IsActive = true };
        context.Set<SubjectCatalog>().Add(subjectCatalog);

        var level = new Level { LevelName = "Bachelor", IsActive = true };
        context.Set<Level>().Add(level);

        var gender = new Gender { GenderName = "Male", IsActive = true };
        context.Set<Gender>().Add(gender);

        var studentCategory = new StudentCategory { StudentCategoryName = "Regular", IsActive = true };
        context.Set<StudentCategory>().Add(studentCategory);

        var collegeType = new CollegeType { Code = "UT", Name = "University", IsActive = true };
        context.Set<CollegeType>().Add(collegeType);

        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var faculty = new Faculty { Name = "Science", OfficeCode = "SCI", ContactNumber = "123", Address = "Ktm", Email = "sci@test.com", TenantId = TestTenantId };
        context.Set<Faculty>().Add(faculty);

        var college1 = new College { TenantId = TestTenantId, Code = "C001", Name = "College A", Email = "a@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = collegeType.Id };
        context.Set<College>().Add(college1);

        var college2 = new College { TenantId = TestTenantId, Code = "C002", Name = "College B", Email = "b@test.com", PrincipalName = "P2", PrincipalContactNumber = "456", IsActive = true, CollegeTypeId = collegeType.Id };
        context.Set<College>().Add(college2);

        var program1 = new Program { LevelId = level.Id, ProgramCode = "BScCSIT", ProgramName = "BSc CSIT", ShortName = "CSIT", Duration = 4, IsActive = true };
        context.Set<Program>().Add(program1);

        var program2 = new Program { LevelId = level.Id, ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, IsActive = true };
        context.Set<Program>().Add(program2);

        var semester = new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = academicYear.Id, Faculty = faculty };
        context.Set<Semester>().Add(semester);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var batch = new Batch { AcademicYearId = academicYear.Id, BatchName = "2081 Batch", IsActive = true };
        context.Set<Batch>().Add(batch);

        var examSchedule1 = new ExamSchedule { TenantId = TestTenantId, ExamScheduleName = "Final 2081", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), IsActive = true, AcademicYearId = academicYear.Id, ProgramId = program1.Id, SemesterId = semester.Id, ExamTypeId = examType.Id };
        context.Set<ExamSchedule>().Add(examSchedule1);

        var examSchedule2 = new ExamSchedule { TenantId = TestTenantId, ExamScheduleName = "Mid 2081", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), IsActive = true, AcademicYearId = academicYear.Id, ProgramId = program2.Id, SemesterId = semester.Id, ExamTypeId = examType.Id };
        context.Set<ExamSchedule>().Add(examSchedule2);

        var student1 = new StudentRegistration { TenantId = TestTenantId, LevelId = level.Id, CollegeId = college1.Id, FirstName = "Ram", LastName = "Sharma", DateOfBirthBS = "2055-01-01", GenderId = gender.Id, IsActive = true, StudentCategoryId = studentCategory.Id, AcademicYearId = academicYear.Id };
        context.Set<StudentRegistration>().Add(student1);

        var student2 = new StudentRegistration { TenantId = TestTenantId, LevelId = level.Id, CollegeId = college2.Id, FirstName = "Sita", LastName = "Gurung", DateOfBirthBS = "2056-02-02", GenderId = gender.Id, IsActive = true, StudentCategoryId = studentCategory.Id, AcademicYearId = academicYear.Id };
        context.Set<StudentRegistration>().Add(student2);

        var examReg1 = new ExamRegistration { TenantId = TestTenantId, AcademicYearId = academicYear.Id, CollegeId = college1.Id, ExamSchedule = examSchedule1, Status = RegistrationStatus.Registered, IsActive = true };
        context.Set<ExamRegistration>().Add(examReg1);

        var examReg2 = new ExamRegistration { TenantId = TestTenantId, AcademicYearId = academicYear.Id, CollegeId = college2.Id, ExamSchedule = examSchedule2, Status = RegistrationStatus.Registered, IsActive = true };
        context.Set<ExamRegistration>().Add(examReg2);

        context.Users.Add(new AppUser { Id = "user-1", UserName = "admin", Email = "admin@test.com" });
        context.Roles.Add(new IdentityRole("Admin") { Id = "role-1" });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var userManager = CreateUserManager(context.Users.AsQueryable());
        var roleManager = CreateRoleManager(context.Roles.AsQueryable());
        var service = new DashboardService(context, userManager, roleManager, userContext);

        var stats = await service.GetDashboardStatsAsync();

        stats.TotalFaculties.Should().Be(1);
        stats.TotalColleges.Should().Be(2);
        stats.TotalPrograms.Should().Be(2);
        stats.TotalStudents.Should().Be(2);
        stats.TotalExamSchedules.Should().Be(2);
        stats.TotalExamRegistrations.Should().Be(2);
        stats.TotalSubjects.Should().Be(1);
        stats.TotalAcademicYears.Should().Be(1);
        stats.TotalBanks.Should().Be(1);
        stats.TotalBoards.Should().Be(1);
        stats.TotalBatches.Should().Be(1);
        stats.TotalUsers.Should().Be(1);
        stats.TotalRoles.Should().Be(1);
        stats.ActiveColleges.Should().Be(2);
        stats.ActivePrograms.Should().Be(2);
        stats.ActiveStudents.Should().Be(2);
        stats.ActiveExamSchedules.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturnZeroCounts_WhenNoData()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var userManager = CreateUserManager(context.Users.AsQueryable());
        var roleManager = CreateRoleManager(context.Roles.AsQueryable());
        var service = new DashboardService(context, userManager, roleManager, userContext);

        var stats = await service.GetDashboardStatsAsync();

        stats.TotalFaculties.Should().Be(0);
        stats.TotalColleges.Should().Be(0);
        stats.TotalPrograms.Should().Be(0);
        stats.TotalStudents.Should().Be(0);
        stats.TotalExamSchedules.Should().Be(0);
        stats.TotalExamRegistrations.Should().Be(0);
        stats.TotalSubjects.Should().Be(0);
        stats.TotalAcademicYears.Should().Be(0);
        stats.TotalBanks.Should().Be(0);
        stats.TotalBoards.Should().Be(0);
        stats.TotalBatches.Should().Be(0);
        stats.TotalUsers.Should().Be(0);
        stats.TotalRoles.Should().Be(0);
        stats.ActiveColleges.Should().Be(0);
        stats.ActivePrograms.Should().Be(0);
        stats.ActiveStudents.Should().Be(0);
        stats.ActiveExamSchedules.Should().Be(0);
    }
}
