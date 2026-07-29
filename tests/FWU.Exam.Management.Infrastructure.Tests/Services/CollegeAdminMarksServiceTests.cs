using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CollegeAdminMarksServiceTests : TestBase
{
    private async Task<(Domain.Entities.Program program, SubjectCatalog subjectCatalog, AcademicYear academicYear, ExamType examType, Semester semester)> SeedPrerequisitesAsync(AppDbContext context)
    {
        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);
        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        var examType = new ExamType { Name = "Final", Code = "FIN", IsActive = true };
        context.Set<ExamType>().Add(examType);
        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        var program = new Domain.Entities.Program { ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, LevelId = level.Id, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = academicYear.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        var subjectCatalog = new SubjectCatalog { SubjectCode = "CS101", SubjectName = "Programming", SubjectTypeId = subjectType.Id, IsActive = true };
        context.Set<SubjectCatalog>().Add(subjectCatalog);
        await context.SaveChangesAsync();

        return (program, subjectCatalog, academicYear, examType, semester);
    }

    private static (ICollegeAdminSubjectAssignmentService assignmentService, IGradeCalculationService gradeService) CreateMocks()
    {
        var assignmentService = Substitute.For<ICollegeAdminSubjectAssignmentService>();
        var gradeService = Substitute.For<IGradeCalculationService>();
        gradeService.CalculateTotalMarks(Arg.Any<float?>(), Arg.Any<float?>(), Arg.Any<float?>(), Arg.Any<float?>())
            .Returns(75);
        gradeService.CalculateGrade(Arg.Any<float>(), Arg.Any<SubjectOffering>())
            .Returns(new GradeResult { GradeLetter = "B" });
        gradeService.IsStudentPassing(Arg.Any<float?>(), Arg.Any<float?>(), Arg.Any<SubjectOffering>())
            .Returns(true);
        return (assignmentService, gradeService);
    }

    [Fact]
    public async Task GetCollegeAdminDashboard_ShouldReturnDashboard()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (program, subjectCatalog, _, _, semester) = await SeedPrerequisitesAsync(context);

        var subjectOffering = new SubjectOffering
        {
            TenantId = TestTenantId,
            SubjectCatalogId = subjectCatalog.Id,
            ProgramId = program.Id,
            SemesterId = semester.Id,
            HasTheory = true,
            HasPractical = false,
            DisplayOrder = 1
        };
        context.Set<SubjectOffering>().Add(subjectOffering);
        await context.SaveChangesAsync();

        var (assignmentService, gradeService) = CreateMocks();
        assignmentService.GetAssignmentsAsync("user1").Returns(Task.FromResult(new List<CollegeAdminSubjectAssignment>
        {
            new() { SubjectOfferingId = subjectOffering.Id, CollegeAdminUserId = "user1" }
        }));

        var service = new CollegeAdminMarksService(context, assignmentService, gradeService);

        var result = await service.GetCollegeAdminDashboardAsync("user1");

        result.Should().NotBeNull();
        result.AssignedSubjects.Should().HaveCount(1);
        result.AssignedSubjects[0].SubjectName.Should().Be("Programming");
    }

    [Fact]
    public async Task GetMarksEntryView_ShouldReturnViewModel()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (program, subjectCatalog, academicYear, examType, semester) = await SeedPrerequisitesAsync(context);

        var subjectOffering = new SubjectOffering
        {
            TenantId = TestTenantId,
            SubjectCatalogId = subjectCatalog.Id,
            ProgramId = program.Id,
            SemesterId = semester.Id,
            HasTheory = true,
            HasPractical = false,
            DisplayOrder = 1
        };
        context.Set<SubjectOffering>().Add(subjectOffering);
        await context.SaveChangesAsync();

        var examSchedule = new ExamSchedule
        {
            ExamScheduleName = "Final 2081", TenantId = TestTenantId,
            AcademicYearId = academicYear.Id, ProgramId = program.Id,
            SemesterId = semester.Id, ExamTypeId = examType.Id,
            StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0),
            IsActive = true
        };
        context.Set<ExamSchedule>().Add(examSchedule);
        await context.SaveChangesAsync();

        var (assignmentService, gradeService) = CreateMocks();
        assignmentService.IsCollegeAdminAssignedToSubjectAsync("user1", subjectOffering.Id)
            .Returns(Task.FromResult(true));

        var service = new CollegeAdminMarksService(context, assignmentService, gradeService);

        var result = await service.GetMarksEntryViewAsync(subjectOffering.Id, examSchedule.Id, "user1");

        result.Should().NotBeNull();
        result.SubjectName.Should().Be("Programming");
        result.SubjectOfferingId.Should().Be(subjectOffering.Id);
        result.ExamScheduleId.Should().Be(examSchedule.Id);
    }

    [Fact]
    public async Task GetMarksEntryView_ShouldThrow_WhenNotAssigned()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var (assignmentService, gradeService) = CreateMocks();
        assignmentService.IsCollegeAdminAssignedToSubjectAsync("user1", Arg.Any<int>())
            .Returns(Task.FromResult(false));

        var service = new CollegeAdminMarksService(context, assignmentService, gradeService);

        var act = async () => await service.GetMarksEntryViewAsync(1, 1, "user1");
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetMarksEntryView_ShouldThrow_WhenSubjectOfferingNotFound()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var (assignmentService, gradeService) = CreateMocks();
        assignmentService.IsCollegeAdminAssignedToSubjectAsync("user1", 999)
            .Returns(Task.FromResult(true));

        var service = new CollegeAdminMarksService(context, assignmentService, gradeService);

        var act = async () => await service.GetMarksEntryViewAsync(999, 1, "user1");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
