using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CollegeAdminSubjectAssignmentServiceTests : TestBase
{
    private static int _counter;

    private static IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private async Task<int> SeedSubjectOfferingAsync(AppDbContext context)
    {
        var n = Interlocked.Increment(ref _counter);
        var subjectType = new SubjectType { Code = $"TH{n}", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        var level = new Level { LevelName = "Bachelor", LevelCode = $"BACH{n}", IsActive = true };
        context.Set<Level>().Add(level);
        var academicYear = new AcademicYear { AcademicYearCode = $"2081/082-{n}", AcademicYearName = $"2081/082-{n}", AcademicYearCodeNepali = $"२०८१/०८२-{n}", AcademicYearNameNepali = $"२०८१/०८२-{n}", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var cat = new SubjectCatalog { SubjectCode = $"MTH{n}", SubjectName = $"Mathematics {n}", SubjectTypeId = subjectType.Id, IsActive = true };
        context.Set<SubjectCatalog>().Add(cat);
        var program = new Domain.Entities.Program { ProgramCode = $"BSC{n}", ProgramName = $"B.Sc. {n}", ShortName = $"BSc{n}", LevelId = level.Id, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var semester = new Semester { Name = $"First Semester {n}", Code = $"SEM1-{n}", Number = 1, Year = 1, AcademicYearId = academicYear.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        var offering = new SubjectOffering { TenantId = TestTenantId, SubjectCatalogId = cat.Id, ProgramId = program.Id, SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1, HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40 };
        context.Set<SubjectOffering>().Add(offering);
        await context.SaveChangesAsync();
        return offering.Id;
    }

    private async Task SeedUserAsync(AppDbContext context, string userId)
    {
        if (!await context.Set<AppUser>().AnyAsync(u => u.Id == userId))
        {
            context.Users.Add(new AppUser { Id = userId, UserName = userId, Email = $"{userId}@test.com" });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CreateAssignment_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var subjectOfferingId = await SeedSubjectOfferingAsync(context);
        await SeedUserAsync(context, "user1");
        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());

        var assignment = new CollegeAdminSubjectAssignment
        {
            CollegeAdminUserId = "user1",
            SubjectOfferingId = subjectOfferingId,
            IsActive = true
        };
        await service.CreateAsync(assignment);

        var result = await service.GetByIdAsync(assignment.Id);
        result.Should().NotBeNull();
        result!.CollegeAdminUserId.Should().Be("user1");
    }

    [Fact]
    public async Task GetAssignments_ShouldReturnForUser()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var soId = await SeedSubjectOfferingAsync(context);
        var soId2 = await SeedSubjectOfferingAsync(context);
        var soId3 = await SeedSubjectOfferingAsync(context);
        await SeedUserAsync(context, "user1");
        await SeedUserAsync(context, "user2");

        context.Set<CollegeAdminSubjectAssignment>().AddRange(
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user1", SubjectOfferingId = soId, IsActive = true },
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user1", SubjectOfferingId = soId2, IsActive = true },
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user2", SubjectOfferingId = soId3, IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());
        var assignments = await service.GetAssignmentsAsync("user1");

        assignments.Should().HaveCount(2);
        assignments.Should().AllSatisfy(a => a.CollegeAdminUserId.Should().Be("user1"));
    }

    [Fact]
    public async Task DeleteAssignment_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var soId = await SeedSubjectOfferingAsync(context);
        await SeedUserAsync(context, "user1");

        var assignment = new CollegeAdminSubjectAssignment
        {
            CollegeAdminUserId = "user1",
            SubjectOfferingId = soId,
            IsActive = true
        };
        context.Set<CollegeAdminSubjectAssignment>().Add(assignment);
        await context.SaveChangesAsync();

        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());
        await service.DeleteAsync(assignment.Id);

        var result = await service.GetByIdAsync(assignment.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsCollegeAdminAssignedToSubject_ShouldReturnTrue_WhenAssigned()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var soId = await SeedSubjectOfferingAsync(context);
        await SeedUserAsync(context, "user1");

        var assignment = new CollegeAdminSubjectAssignment
        {
            CollegeAdminUserId = "user1",
            SubjectOfferingId = soId,
            IsActive = true
        };
        context.Set<CollegeAdminSubjectAssignment>().Add(assignment);
        await context.SaveChangesAsync();

        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());
        var result = await service.IsCollegeAdminAssignedToSubjectAsync("user1", soId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCollegeAdminAssignedToSubject_ShouldReturnFalse_WhenNotAssigned()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());

        var result = await service.IsCollegeAdminAssignedToSubjectAsync("user1", 999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAssignedSubjectOfferingIds_ShouldReturnDistinctIds()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var soId = await SeedSubjectOfferingAsync(context);
        var soId2 = await SeedSubjectOfferingAsync(context);
        await SeedUserAsync(context, "user1");

        context.Set<CollegeAdminSubjectAssignment>().AddRange(
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user1", SubjectOfferingId = soId, IsActive = true },
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user1", SubjectOfferingId = soId2, IsActive = true },
            new CollegeAdminSubjectAssignment { CollegeAdminUserId = "user1", SubjectOfferingId = soId, IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeAdminSubjectAssignmentService(context, CreateSuperAdminContext());
        var ids = await service.GetAssignedSubjectOfferingIdsAsync("user1");

        ids.Should().HaveCount(2);
        ids.Should().Contain([soId, soId2]);
    }
}
