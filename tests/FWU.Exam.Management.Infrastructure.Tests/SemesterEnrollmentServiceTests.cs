using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
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
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, created);
        var next = db.Context.SemesterEnrollments!.Single(se => se.SemesterId == 2);
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
        });
        var service = CreateService(db);

        var first = await service.PromoteCompletedSemestersAsync();
        var second = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(db.Context.SemesterEnrollments!.Where(se => se.SemesterId == 2));
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
        var enrollments = db.Context.SemesterEnrollments!.Where(se => se.SemesterId == 2).ToList();
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
        Assert.Equal(2, db.Context.SemesterEnrollments!.Count(se => se.SemesterId == 2));
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
        Assert.Contains(db.Context.SemesterEnrollments!, se => se.StudentAdmissionId == 2 && se.SemesterId == 2);
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
    public async Task GetEnrollmentCandidatesAsync_ReturnsStudentsWithRegistrationAndEnrollmentFlag()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            var reg = TestData.StudentRegistration(1, Email);
            reg.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(reg);
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

        var withReg = candidates.Single(c => c.StudentName != null);
        Assert.Equal("Test Student", withReg.StudentName);
        Assert.Equal("REG1", withReg.RegistrationNumber);
        Assert.Equal("2081", withReg.AcademicYearName);
        Assert.Equal("ROLL1", withReg.CollegeRollNumber);
    }

    private static void SeedVersions(AppDbContext ctx)
    {
        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 1,
            TenantId = TestData.TenantId,
            Name = "Default - BCA (2081)",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TestData.AcademicYearId,
            IsActive = true
        });
        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 2,
            TenantId = TestData.TenantId,
            Name = "Old BCA",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TestData.AcademicYearId,
            IsActive = false
        });
    }

    private static void SeedProgramSemesters(AppDbContext ctx)
    {
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 1, ProgramId = TestData.ProgramId, SemesterId = 1, IsActive = true });
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 2, ProgramId = TestData.ProgramId, SemesterId = 2, IsActive = true });
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_ConsultsActiveVersionSchedule_OverInactiveVersionSchedule()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            SeedVersions(ctx);
            SeedProgramSemesters(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));

            var activeVersionSchedule = TestData.Schedule(21, 1, TestData.Regular, Future, PastDateTime);
            activeVersionSchedule.CurriculumVersionId = 1;
            var inactiveVersionSchedule = TestData.Schedule(22, 1, TestData.Regular, Past, PastDateTime);
            inactiveVersionSchedule.CurriculumVersionId = 2;
            ctx.ExamSchedules.Add(activeVersionSchedule);
            ctx.ExamSchedules.Add(inactiveVersionSchedule);
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_Promotes_WhenActiveVersionScheduleEnded_EvenIfInactiveVersionHasNot()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            SeedVersions(ctx);
            SeedProgramSemesters(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));

            var activeVersionSchedule = TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime);
            activeVersionSchedule.CurriculumVersionId = 1;
            var inactiveVersionSchedule = TestData.Schedule(22, 1, TestData.Regular, Future, PastDateTime);
            inactiveVersionSchedule.CurriculumVersionId = 2;
            ctx.ExamSchedules.Add(activeVersionSchedule);
            ctx.ExamSchedules.Add(inactiveVersionSchedule);
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, created);
    }
}
