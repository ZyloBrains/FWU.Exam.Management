using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class SemesterEnrollmentServiceTests
{
    private const string UserId = "user-1";
    private const string Email = "stu@test.com";

    private static SemesterEnrollmentService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext());

    private static SemesterEnrollmentService CreateService(TestDb db, IUserContext userContext) =>
        new(db.Context, userContext);

    private static void SeedPromotionBase(AppDbContext ctx)
    {
        ctx.Users.Add(TestData.User(UserId, Email));
    }

    private static DateOnly Past => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
    private static DateOnly Future => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
    private static DateTime PastDateTime => DateTime.UtcNow.AddDays(-5);
    private static DateTime FutureDateTime => DateTime.UtcNow.AddDays(5);

    [Fact]
    public async Task PromoteCompletedSemestersAsync_CreatesNextSemesterEnrollment_WhenExamEndedAndAdmitCardReleased()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 21, 1, TestData.ProgramId, semesterEnrollmentId: 1));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, created);
        var next = db.Context.SemesterEnrollments!.Single(se => se.SemesterInstanceId == 2);
        Assert.Equal(StudentEnrollmentStatus.Active, next.EnrollmentStatus);
        Assert.Equal(TestData.TenantId, next.TenantId);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenExamNotEndedYet()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Future, PastDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenAdmitCardNotReleasedYet()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, FutureDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenExtendedDateIsInFuture()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            var schedule = TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime);
            schedule.ExtendedDate = FutureDateTime;
            ctx.ExamSchedules.Add(schedule);
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_IsIdempotent_SecondRunCreatesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 21, 1, TestData.ProgramId, semesterEnrollmentId: 1));
        });
        var service = CreateService(db);

        var first = await service.PromoteCompletedSemestersAsync();
        var second = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(db.Context.SemesterEnrollments!.Where(se => se.SemesterInstanceId == 2));
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_SkipsStudentsWithoutNextSemesterInProgram()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId, TestData.ProgramIdOther));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 2, TestData.Regular, Past, PastDateTime, TestData.ProgramIdOther));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email, TestData.ProgramIdOther));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 21, 1, TestData.ProgramIdOther, semesterEnrollmentId: 1));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenAlreadyEnrolledInNextSemester()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 21, 1, TestData.ProgramId, semesterEnrollmentId: 1));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task BulkCreateEnrollmentsAsync_CreatesEnrollments_ForSelectedAdmissions()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(3, UserId));
        });
        var service = CreateService(db);

        var (created, skipped) = await service.BulkCreateEnrollmentsAsync([1, 2, 3], 2);

        Assert.Equal(3, created);
        Assert.Equal(0, skipped);
        var enrollments = db.Context.SemesterEnrollments!.Where(se => se.SemesterInstanceId == 2).ToList();
        Assert.Equal(3, enrollments.Count);
        Assert.All(enrollments, se =>
        {
            Assert.Equal(StudentEnrollmentStatus.Active, se.EnrollmentStatus);
            Assert.Equal(EnrollmentType.FullTime, se.EnrollmentType);
            Assert.Equal(PaymentStatus.Pending, se.PaymentStatus);
            Assert.Equal(ResultStatus.Incomplete, se.ResultStatus);
            Assert.Equal(TestData.TenantId, se.TenantId);
        });
    }

    [Fact]
    public async Task BulkCreateEnrollmentsAsync_RespectsProvidedEnrollmentType()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId));
        });
        var service = CreateService(db);

        var (created, skipped) = await service.BulkCreateEnrollmentsAsync([1, 2], 2, EnrollmentType.PartTime);

        Assert.Equal(2, created);
        Assert.Equal(0, skipped);
        Assert.All(db.Context.SemesterEnrollments!.Where(se => se.SemesterInstanceId == 2), se =>
            Assert.Equal(EnrollmentType.PartTime, se.EnrollmentType));
    }

    [Fact]
    public async Task BulkCreateEnrollmentsAsync_SkipsAlreadyEnrolledStudents()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var (created, skipped) = await service.BulkCreateEnrollmentsAsync([1, 2], 2);

        Assert.Equal(1, created);
        Assert.Equal(1, skipped);
        Assert.Equal(2, db.Context.SemesterEnrollments!.Count(se => se.SemesterInstanceId == 2));
    }

    [Fact]
    public async Task BulkCreateEnrollmentsAsync_RespectsCollegeAdminScope()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.Colleges.Add(new College
            {
                Id = 2,
                Code = "CLG2",
                Name = "Other College",
                Email = "c2@c.com",
                CollegeTypeId = 1,
                IsActive = true
            });
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(new StudentAdmission
            {
                Id = 2,
                TenantId = TestData.TenantId,
                ProgramsId = TestData.ProgramId,
                CollegeId = 2,
                AcademicYearId = TestData.AcademicYearId,
                AdmissionDate = DateTime.UtcNow,
                IsActive = true,
                CollegeRollNumber = "ROLL2",
                AppUserId = UserId
            });
        });

        var uc = new TestUserContext();
        uc.SetUser(UserId, null, TestData.CollegeId, [], [Role.CollegeAdmin]);
        var service = CreateService(db, uc);

        var (created, skipped) = await service.BulkCreateEnrollmentsAsync([1, 2], 2);

        Assert.Equal(1, created);
        Assert.Equal(1, skipped);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(1, enrollment.StudentAdmissionId);
    }

    [Fact]
    public async Task BulkCreateAllEnrollmentsAsync_EnrollsAllMatchingAndSkipsAlreadyEnrolled()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var (created, skipped) = await service.BulkCreateAllEnrollmentsAsync(null, null, null, null, 2);

        Assert.Equal(1, created);
        Assert.Equal(1, skipped);
        Assert.Equal(2, db.Context.SemesterEnrollments!.Count());
        Assert.Contains(db.Context.SemesterEnrollments!, se => se.StudentAdmissionId == 2 && se.SemesterInstanceId == 2);
    }

    [Fact]
    public async Task BulkCreateAllEnrollmentsAsync_RespectsCollegeAdminScope()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.Colleges.Add(new College
            {
                Id = 2,
                Code = "CLG2",
                Name = "Other College",
                Email = "c2@c.com",
                CollegeTypeId = 1,
                IsActive = true
            });
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(new StudentAdmission
            {
                Id = 2,
                TenantId = TestData.TenantId,
                ProgramsId = TestData.ProgramId,
                CollegeId = 2,
                AcademicYearId = TestData.AcademicYearId,
                AdmissionDate = DateTime.UtcNow,
                IsActive = true,
                CollegeRollNumber = "ROLL2",
                AppUserId = UserId
            });
        });

        var uc = new TestUserContext();
        uc.SetUser(UserId, null, TestData.CollegeId, [], [Role.CollegeAdmin]);
        var service = CreateService(db, uc);

        var (created, skipped) = await service.BulkCreateAllEnrollmentsAsync(null, null, null, null, 2);

        Assert.Equal(1, created);
        Assert.Equal(0, skipped);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(1, enrollment.StudentAdmissionId);
    }

    [Fact]
    public async Task GetEnrollmentCandidatesAsync_ReturnsStudentsWithEnrollmentFlag()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var (candidates, totalCount) = await service.GetEnrollmentCandidatesAsync(null, null, null, null, 2);

        Assert.Equal(2, totalCount);
        Assert.Equal(2, candidates.Count);
        Assert.True(candidates.Single(c => c.AdmissionId == 1).IsEnrolled);
        Assert.False(candidates.Single(c => c.AdmissionId == 2).IsEnrolled);

        var named = candidates.Single(c => c.AdmissionId == 1);
        Assert.Equal("Test Student", named.StudentName);
        Assert.Equal("2081", named.AcademicYearName);
        Assert.Equal("ROLL1", named.CollegeRollNumber);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenNoExamFormSubmitted()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
        });

        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenExamFormRejected()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            var reg = TestData.ExamRegistration(1, 21, 1, TestData.ProgramId, semesterEnrollmentId: 1);
            reg.Status = RegistrationStatus.Rejected;
            ctx.ExamRegistrations.Add(reg);
        });

        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task EnrollInFirstSemesterAsync_CreatesFirstSemesterEnrollment()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
        });
        var service = CreateService(db);

        var created = await service.EnrollInFirstSemesterAsync(1);

        Assert.True(created);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(1, enrollment.StudentAdmissionId);
        Assert.Equal(1, enrollment.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
    }

    [Fact]
    public async Task EnrollInFirstSemesterAsync_IsIdempotent()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
        });
        var service = CreateService(db);

        var created = await service.EnrollInFirstSemesterAsync(1);

        Assert.False(created);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(1, enrollment.SemesterInstanceId);
    }

    [Fact]
    public async Task EnrollInFirstSemesterAsync_ReturnsFalse_WhenAdmissionNotFound()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx => TestData.SeedBase(ctx));
        var service = CreateService(db);

        var created = await service.EnrollInFirstSemesterAsync(999);

        Assert.False(created);
        Assert.Empty(db.Context.SemesterEnrollments!);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReplacesEnrollments_WhenTargetSemesterInstanceExists()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.SemesterInstances.Add(new SemesterInstance
            {
                Id = 90,
                TenantId = TestData.TenantId,
                SemesterId = 1,
                AcademicYearId = TestData.AcademicYearId,
                ProgramId = TestData.ProgramIdOther
            });
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1);

        Assert.True(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(90, enrollment.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenSemesterInstanceMissing_AndDeletesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            // Program 2 has ProgramSemesters (SeedBase) but no SemesterInstance, so the
            // transfer must fail BEFORE the existing enrollment is removed.
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId);

        Assert.False(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(2, enrollment.SemesterInstanceId);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenProgramHasNoSemesters_AndDeletesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, 99, TestData.AcademicYearId);

        Assert.False(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(2, enrollment.SemesterInstanceId);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenAdmissionNotFound()
    {
        using var db = new TestDb(TestTenantContext.Central(), TestData.SeedBase);
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(999, TestData.ProgramIdOther, TestData.AcademicYearId);

        Assert.False(transferred);
        Assert.Empty(db.Context.SemesterEnrollments!);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_DefaultsToLowestSemesterNumber_WhenDisplayOrderTied()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId, programId: 99));
            ctx.Programs.Add(new Program { Id = 99, LevelId = TestData.LevelId, ProgramCode = "T99", ProgramName = "Test 99", ShortName = "T99", Duration = 4, IsActive = true });
            ctx.ProgramSemesters.Add(new ProgramSemester { Id = 100, ProgramId = 99, SemesterId = 2, IsActive = true, DisplayOrder = 0 });
            ctx.ProgramSemesters.Add(new ProgramSemester { Id = 101, ProgramId = 99, SemesterId = 1, IsActive = true, DisplayOrder = 0 });
            ctx.ProgramSemesters.Add(new ProgramSemester { Id = 102, ProgramId = 99, SemesterId = 3, IsActive = true, DisplayOrder = 0 });
            for (var semId = 1; semId <= 3; semId++)
            {
                ctx.SemesterInstances.Add(new SemesterInstance
                {
                    Id = 200 + semId,
                    TenantId = TestData.TenantId,
                    SemesterId = semId,
                    AcademicYearId = TestData.AcademicYearId,
                    ProgramId = 99
                });
            }
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, 99, TestData.AcademicYearId);

        Assert.True(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(201, enrollment.SemesterInstanceId);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_RecreatesEnrollment_WhenProgramAndYearUnchanged()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramId, TestData.AcademicYearId);

        Assert.True(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        // Default (no target semester) resolves to the first program semester — instance 1.
        Assert.Equal(1, enrollment.SemesterInstanceId);
    }

    private static void SeedProgramSemesters(AppDbContext ctx)
    {
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 1, ProgramId = TestData.ProgramId, SemesterId = 1, IsActive = true });
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 2, ProgramId = TestData.ProgramId, SemesterId = 2, IsActive = true });
    }
}
