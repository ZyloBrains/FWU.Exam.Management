using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamSubjectResultServiceTests : TestBase
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
        context.Set<Level>().Add(new Level { LevelCode = "BACH", LevelName = "Bachelor", IsActive = true });
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
            TenantId = TestTenantId, ExamScheduleName = "Final 2081",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        context.Set<SubjectType>().Add(new SubjectType { Code = "TH", Name = "Theory", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<SubjectCatalog>().Add(new SubjectCatalog { SubjectCode = "MTH101", SubjectName = "Mathematics", SubjectTypeId = 1, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<SubjectOffering>().Add(new SubjectOffering { TenantId = TestTenantId, SubjectCatalogId = 1, ProgramId = 1, SemesterId = 1, DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var college = new College { TenantId = TestTenantId, Code = "COL01", Name = "Test College", Email = "c@test.com", PrincipalName = "P", PrincipalContactNumber = "9800000000", IsActive = true };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateExamSubjectResult_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);
        var userContext = CreateSuperAdminContext();
        var service = new ExamSubjectResultService(context, userContext);

        var result = new ExamSubjectResult
        {
            TenantId = TestTenantId,
            ExamRegistrationId = 1,
            ExamTypeId = 1,
            SubjectOfferingId = 1,
            ExamScheduleId = 1,
            GradeLetter = "A",
            ObtainedMarks = 85,
            IsActive = true
        };

        await service.CreateExamSubjectResultAsync(result);

        var saved = await service.GetExamSubjectResultByIdAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.GradeLetter.Should().Be("A");
        saved.ObtainedMarks.Should().Be(85);
    }

    [Fact]
    public async Task GetExamSubjectResults_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<ExamSubjectResult>().Add(new ExamSubjectResult
            {
                TenantId = TestTenantId, ExamRegistrationId = 1, ExamTypeId = 1,
                SubjectOfferingId = 1, ExamScheduleId = 1,
                GradeLetter = i == 1 ? "A" : "B", ObtainedMarks = 80 + i, IsActive = true
            });
        }
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamSubjectResultService(context, userContext);

        var (items, totalCount) = await service.GetExamSubjectResultsAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExamSubjectResults_WithSearch_ShouldFilterByGrade()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSubjectResult>().Add(new ExamSubjectResult
        {
            TenantId = TestTenantId, ExamRegistrationId = 1, ExamTypeId = 1,
            SubjectOfferingId = 1, ExamScheduleId = 1,
            GradeLetter = "A", ObtainedMarks = 90, IsActive = true
        });
        context.Set<ExamSubjectResult>().Add(new ExamSubjectResult
        {
            TenantId = TestTenantId, ExamRegistrationId = 1, ExamTypeId = 1,
            SubjectOfferingId = 1, ExamScheduleId = 1,
            GradeLetter = "B", ObtainedMarks = 70, IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamSubjectResultService(context, userContext);

        var (items, totalCount) = await service.GetExamSubjectResultsAsync(1, 10, "A", "Id", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].GradeLetter.Should().Be("A");
    }

    [Fact]
    public async Task UpdateExamSubjectResult_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSubjectResult>().Add(new ExamSubjectResult
        {
            TenantId = TestTenantId, ExamRegistrationId = 1, ExamTypeId = 1,
            SubjectOfferingId = 1, ExamScheduleId = 1,
            GradeLetter = "A", ObtainedMarks = 85, IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamSubjectResultService(context, userContext);

        var entity = await service.GetExamSubjectResultByIdAsync(1);
        entity.Should().NotBeNull();

        entity!.GradeLetter = "A+";
        entity.ObtainedMarks = 95;
        await service.UpdateExamSubjectResultAsync(entity);

        context.ChangeTracker.Clear();
        var updated = await service.GetExamSubjectResultByIdAsync(1);
        updated!.GradeLetter.Should().Be("A+");
        updated.ObtainedMarks.Should().Be(95);
    }

    [Fact]
    public async Task DeleteExamSubjectResult_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamSubjectResult>().Add(new ExamSubjectResult
        {
            TenantId = TestTenantId, ExamRegistrationId = 1, ExamTypeId = 1,
            SubjectOfferingId = 1, ExamScheduleId = 1,
            GradeLetter = "B", ObtainedMarks = 70, IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamSubjectResultService(context, userContext);

        await service.DeleteExamSubjectResultAsync(1);

        var exists = await service.ExamSubjectResultExistsAsync(1);
        exists.Should().BeFalse();
    }
}
