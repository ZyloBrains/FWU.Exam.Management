using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SemesterEnrollmentServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private async Task<StudentAdmission> SeedAdmissionAsync(AppDbContext context)
    {
        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);

        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);

        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);

        await context.SaveChangesAsync();

        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = level.Id, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);

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
        context.Set<StudentAdmission>().Add(admission);
        await context.SaveChangesAsync();

        return admission;
    }

    [Fact]
    public async Task CreateEnrollmentAsync_ShouldSetDefaults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterEnrollmentService(context, userCtx);

        var admission = await SeedAdmissionAsync(context);

        await SeedAcademicYearAsync(context);
        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        var enrollment = new SemesterEnrollment
        {
            TenantId = TestTenantId,
            StudentAdmissionId = admission.Id,
            SemesterId = semester.Id
        };

        await service.CreateEnrollmentAsync(enrollment);

        var result = await service.GetEnrollmentByIdAsync(enrollment.Id);
        result.Should().NotBeNull();
        result!.EnrollmentStatus.Should().Be(StudentEnrollmentStatus.Active);
        result.PaymentStatus.Should().Be(PaymentStatus.Pending);
        result.ResultStatus.Should().Be(ResultStatus.Incomplete);
        result.EnrolledDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetEnrollmentsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterEnrollmentService(context, userCtx);

        var admission = await SeedAdmissionAsync(context);
        await SeedAcademicYearAsync(context);
        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        for (int i = 0; i < 3; i++)
        {
            context.Set<SemesterEnrollment>().Add(new SemesterEnrollment
            {
                TenantId = TestTenantId,
                StudentAdmissionId = admission.Id,
                SemesterId = semester.Id,
                EnrollmentStatus = StudentEnrollmentStatus.Active,
                PaymentStatus = PaymentStatus.Pending,
                ResultStatus = ResultStatus.Incomplete,
                EnrolledDate = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetEnrollmentsAsync(1, 2, null, "enrolleddate", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateEnrollmentAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterEnrollmentService(context, userCtx);

        var admission = await SeedAdmissionAsync(context);
        await SeedAcademicYearAsync(context);
        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        var enrollment = new SemesterEnrollment { TenantId = TestTenantId, StudentAdmissionId = admission.Id, SemesterId = semester.Id };
        context.Set<SemesterEnrollment>().Add(enrollment);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        enrollment.EnrollmentStatus = StudentEnrollmentStatus.Active;
        await service.UpdateEnrollmentAsync(enrollment);

        var updated = await service.GetEnrollmentByIdAsync(enrollment.Id);
        updated!.EnrollmentStatus.Should().Be(StudentEnrollmentStatus.Active);
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterEnrollmentService(context, userCtx);

        var admission = await SeedAdmissionAsync(context);
        await SeedAcademicYearAsync(context);
        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);
        await context.SaveChangesAsync();

        var enrollment = new SemesterEnrollment { TenantId = TestTenantId, StudentAdmissionId = admission.Id, SemesterId = semester.Id };
        context.Set<SemesterEnrollment>().Add(enrollment);
        await context.SaveChangesAsync();

        await service.DeleteEnrollmentAsync(enrollment.Id);

        var exists = await service.EnrollmentExistsAsync(enrollment.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveAdmissionsAsync_ShouldReturnActive()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SemesterEnrollmentService(context, userCtx);

        var admission = await SeedAdmissionAsync(context);

        var items = await service.GetActiveAdmissionsAsync();

        items.Should().ContainSingle(a => a.Id == admission.Id);
    }
}
