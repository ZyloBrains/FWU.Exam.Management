using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamCenterServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private async Task SeedDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelName = "Bachelor", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        await context.SaveChangesAsync();

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId,
            ExamScheduleName = "Final 2081",
            AcademicYearId = 1,
            ProgramId = 1,
            SemesterId = 1,
            ExamTypeId = 1,
            IsActive = true,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var college = new College { TenantId = TestTenantId, Code = "COL01", Name = "Test College", Email = "col@test.com", PrincipalName = "Principal", PrincipalContactNumber = "9800000000", IsActive = true };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateExamCenter_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);
        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        var center = new ExamCenter
        {
            ExamScheduleId = 1,
            Code = "EC001",
            CollegeId = 1,
            IsActive = true
        };

        await service.CreateExamCenterAsync(center);

        var result = await service.GetExamCenterByIdAsync(center.Id);
        result.Should().NotBeNull();
        result!.Code.Should().Be("EC001");
    }

    [Fact]
    public async Task GetExamCenters_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC001", IsActive = true });
        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC002", IsActive = true });
        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC003", IsActive = true });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        var (items, totalCount) = await service.GetExamCentersAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExamCenters_WithSearch_ShouldFilterByCode()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC001", IsActive = true });
        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC002", IsActive = true });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        var (items, totalCount) = await service.GetExamCentersAsync(1, 10, "EC001", "Code", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].Code.Should().Be("EC001");
    }

    [Fact]
    public async Task UpdateExamCenter_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC001", IsActive = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        var center = await service.GetExamCenterByIdAsync(1);
        center.Should().NotBeNull();

        center!.Remark = "Updated Remark";
        await service.UpdateExamCenterAsync(center);

        context.ChangeTracker.Clear();
        var updated = await service.GetExamCenterByIdAsync(1);
        updated!.Remark.Should().Be("Updated Remark");
    }

    [Fact]
    public async Task DeleteExamCenter_ShouldSetInactive()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC001", IsActive = true });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        await service.DeleteExamCenterAsync(1);

        var deleted = await context.Set<ExamCenter>().FindAsync(1);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateExamCenterWithColleges_ShouldCreateRelatedEntities()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        var college2 = new College { TenantId = TestTenantId, Code = "COL02", Name = "Venue College", Email = "v@test.com", PrincipalName = "P", PrincipalContactNumber = "9800000001", IsActive = true };
        context.Set<College>().Add(college2);
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamCenterService(context, userContext);

        var center = new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, CollegeId = 1, Code = "EC-MAIN", IsActive = true };

        await service.CreateExamCenterWithCollegesAsync(center, new List<int> { 2 }, new List<int> { 1 });

        var saved = await service.GetExamCenterByIdAsync(center.Id);
        saved.Should().NotBeNull();

        var venues = await service.GetVenueCollegesAsync(center.Id);
        venues.Should().HaveCount(1);
        venues[0].Code.Should().Be("COL02");

        var sources = await service.GetSourceCollegesAsync(center.Id);
        sources.Should().HaveCount(1);
        sources[0].Code.Should().Be("COL01");
    }
}
