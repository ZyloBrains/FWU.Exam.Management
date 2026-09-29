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

    /// <summary>
    /// Rewrites every seeded ProgramSemester to DisplayOrder 0, matching the live database
    /// where that column is uniformly 0 because the creating migration backfilled it that way.
    /// Enumerated through the change tracker rather than the DbSet, because the rows are
    /// still Added and so are not returned by a query against the database.
    /// </summary>
    private static void ZeroAllProgramSemesterDisplayOrders(AppDbContext ctx)
    {
        foreach (var entry in ctx.ChangeTracker.Entries<ProgramSemester>())
            entry.Entity.DisplayOrder = 0;
    }

    // Faculty.TenantId is a real foreign key, so a faculty on a non-default tenant needs
    // that tenant to exist before the seed can be saved.
    private static void SeedTenant(AppDbContext ctx, int tenantId, string code)
    {
        ctx.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = $"Tenant {code}",
            OfficeCode = code,
            ContactNumber = "000",
            Address = "Kathmandu",
            Email = $"{code.ToLowerInvariant()}@t.com",
            TenantType = TenantType.Standard,
            IsActive = true
        });
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

    // Production reality: ProgramSemesters.DisplayOrder is 0 on every row, because the
    // migration that created the table backfilled 0 and the seeder that would have filled in
    // real values early-returns once the table has rows. The seeded fixtures set
    // DisplayOrder = semesterId, which is data the real database never contains, so the
    // other promotion tests cannot catch a regression that depends on this column.
    [Fact]
    public async Task PromoteCompletedSemestersAsync_CreatesNextSemester_WhenAllDisplayOrdersAreZero()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ZeroAllProgramSemesterDisplayOrders(ctx);
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
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenStudentIsInFinalSemester_AndDisplayOrdersAreZero()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ZeroAllProgramSemesterDisplayOrders(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            // SeedBase creates six semesters/instances for the program; the last one is 6.
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 6));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 6, TestData.Regular, Past, PastDateTime));
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
    public async Task TransferEnrollmentsAsync_ClosesOldAndAddsNew_WhenTargetSemesterInstanceExists()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            var old = TestData.Enrollment(1, 1, 2);
            old.TotalCredits = 45;
            old.GradePoints = 320;
            old.TotalFee = 50000;
            old.PaidAmount = 50000;
            old.SemesterResultDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            ctx.SemesterEnrollments.Add(old);
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

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program closed, moved to running program");

        Assert.True(transferred);
        var rows = db.Context.SemesterEnrollments!.ToList();
        Assert.Equal(2, rows.Count);

        // The prior row is closed, never deleted, and its academic history is intact.
        var closed = Assert.Single(rows, e => e.Id == 1);
        Assert.Equal(StudentEnrollmentStatus.Inactive, closed.EnrollmentStatus);
        Assert.NotNull(closed.DropDate);
        Assert.False(string.IsNullOrWhiteSpace(closed.DropReason));
        Assert.Equal(2, closed.SemesterInstanceId);
        Assert.Equal(45, closed.TotalCredits);
        Assert.Equal(320, closed.GradePoints);
        Assert.Equal(50000, closed.TotalFee);
        Assert.Equal(50000, closed.PaidAmount);
        Assert.NotNull(closed.SemesterResultDate);

        // Exactly one new active row in the explicitly chosen semester.
        var created = Assert.Single(rows, e => e.Id != 1);
        Assert.Equal(90, created.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, created.EnrollmentStatus);
        Assert.Null(created.DropDate);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_StampsNewRowWithFacultyTenant_NotAdmissionsOldTenant()
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
            // The target program belongs to a faculty in tenant 5 while the admission is
            // still on tenant 1. A program has no tenant of its own, so the faculty decides.
            SeedTenant(ctx, 5, "OTH");
            ctx.Faculties.Add(new Faculty { Id = 50, Name = "Other Faculty", OfficeCode = "OTH", TenantId = 5 });
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramIdOther).FacultyId = 50;
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Moved to the other faculty");

        Assert.True(transferred);
        var created = Assert.Single(db.Context.SemesterEnrollments!.ToList(), e => e.Id != 1);
        Assert.Equal(5, created.TenantId);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_FallsBackToCentralTenant_WhenTargetFacultyHasNoTenant()
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
            ctx.Faculties.Add(new Faculty { Id = 51, Name = "Null Tenant Faculty", OfficeCode = "NTF", TenantId = null });
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramIdOther).FacultyId = 51;
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Moved to the other faculty");

        Assert.True(transferred);
        var created = Assert.Single(db.Context.SemesterEnrollments!.ToList(), e => e.Id != 1);
        Assert.Equal(TenantDefaults.CentralTenantId, created.TenantId);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_LeavesClosedHistoryOnItsOriginalTenant()
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
            SeedTenant(ctx, 5, "OTH2");
            ctx.Faculties.Add(new Faculty { Id = 52, Name = "Other Faculty", OfficeCode = "OTH2", TenantId = 5 });
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramIdOther).FacultyId = 52;
        });
        var service = CreateService(db);

        await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Moved to the other faculty");

        // The closed row keeps pointing at the old SemesterInstance, so it must NOT be
        // restamped: that instance belongs to the old program and is shared with the rest
        // of the old cohort.
        var closed = Assert.Single(db.Context.SemesterEnrollments!.ToList(), e => e.Id == 1);
        Assert.Equal(TestData.TenantId, closed.TenantId);
        Assert.Equal(2, closed.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Inactive, closed.EnrollmentStatus);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ClosedHistoryStaysBehind_SoStudentCanChangeTenant()
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
                TenantId = 5,
                SemesterId = 1,
                AcademicYearId = TestData.AcademicYearId,
                ProgramId = TestData.ProgramIdOther
            });
            // Engineering-style faculty: administered by a different exam office than the one
            // holding the student, and its semester instance sits in that other tenant too.
            SeedTenant(ctx, 5, "ENG");
            ctx.Faculties.Add(new Faculty { Id = 53, Name = "Engineering", OfficeCode = "ENG", TenantId = 5 });
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramIdOther).FacultyId = 53;
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Moved into Engineering");

        Assert.True(transferred);
        var rows = db.Context.SemesterEnrollments!.ToList();

        // The new enrollment joins the new office, so it is visible to the faculty that runs it.
        var created = Assert.Single(rows, e => e.Id != 1);
        Assert.Equal(5, created.TenantId);
        Assert.Equal(90, created.SemesterInstanceId);

        // The closed row stays on the old tenant and keeps its old semester instance, which
        // is what makes the student-tenant move safe: the old office keeps the transcript
        // and the old cohort keeps its shared semester.
        var closed = Assert.Single(rows, e => e.Id == 1);
        Assert.Equal(TestData.TenantId, closed.TenantId);
        Assert.Equal(2, closed.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Inactive, closed.EnrollmentStatus);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenReasonMissing_AndClosesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            // The target semester instance exists, so the ONLY reason this can fail is the
            // missing reason. The check must happen before anything is written.
        });
        var service = CreateService(db);

        Assert.False(await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: ""));
        Assert.False(await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "   "));

        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
        Assert.Null(enrollment.DropDate);
        Assert.Null(enrollment.DropReason);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_RecordsReasonOnClosedRow_AndCapsAt500()
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

        var transferred = await service.TransferEnrollmentsAsync(
            1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program was closed");

        Assert.True(transferred);
        var closed = Assert.Single(db.Context.SemesterEnrollments!.Where(e => e.Id == 1).ToList());

        // DropReason is the only free-text field on SemesterEnrollment and this flow is its
        // sole writer, so it doubles as the "list of transferred students" source.
        Assert.Contains("Program was closed", closed.DropReason);
        Assert.StartsWith("Transferred to ", closed.DropReason);
        Assert.True(closed.DropReason!.Length <= 500);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_TruncatesOverlongReasonToColumnLimit()
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

        var transferred = await service.TransferEnrollmentsAsync(
            1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: new string('x', 900));

        Assert.True(transferred);
        var closed = Assert.Single(db.Context.SemesterEnrollments!.Where(e => e.Id == 1).ToList());
        Assert.Equal(500, closed.DropReason!.Length);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenSemesterInstanceMissing_AndClosesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            // Program 2 has ProgramSemesters (SeedBase) but no SemesterInstance, so the
            // transfer must fail BEFORE the existing enrollment is closed.
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program closed, moved to running program");

        Assert.False(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(2, enrollment.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
        Assert.Null(enrollment.DropDate);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenProgramHasNoSemesters_AndClosesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, 99, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program closed, moved to running program");

        Assert.False(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(2, enrollment.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
        Assert.Null(enrollment.DropDate);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenTargetSemesterNotProvided()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramId, TestData.AcademicYearId, targetSemesterId: 0, transferReason: "Program closed, moved to running program");

        Assert.False(transferred);
        var enrollment = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(StudentEnrollmentStatus.Active, enrollment.EnrollmentStatus);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ReturnsFalse_WhenAdmissionNotFound()
    {
        using var db = new TestDb(TestTenantContext.Central(), TestData.SeedBase);
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(999, TestData.ProgramIdOther, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program closed, moved to running program");

        Assert.False(transferred);
        Assert.Empty(db.Context.SemesterEnrollments!);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_UsesExplicitSemester_AndIgnoresDisplayOrder()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId, programId: 99));
            ctx.Programs.Add(new Program { Id = 99, LevelId = TestData.LevelId, ProgramCode = "T99", ProgramName = "Test 99", ShortName = "T99", Duration = 4, IsActive = true });
            // Every ProgramSemester ties on DisplayOrder. The service must no longer pick
            // the lowest semester number here; the caller's explicit choice wins.
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

        var transferred = await service.TransferEnrollmentsAsync(1, 99, TestData.AcademicYearId, targetSemesterId: 3, transferReason: "Program closed, moved to running program");

        Assert.True(transferred);
        var created = Assert.Single(db.Context.SemesterEnrollments!);
        Assert.Equal(203, created.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, created.EnrollmentStatus);
    }

    [Fact]
    public async Task TransferEnrollmentsAsync_ClosesAndReAdds_WhenProgramAndYearUnchanged()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
        });
        var service = CreateService(db);

        var transferred = await service.TransferEnrollmentsAsync(1, TestData.ProgramId, TestData.AcademicYearId, targetSemesterId: 1, transferReason: "Program closed, moved to running program");

        Assert.True(transferred);
        var rows = db.Context.SemesterEnrollments!.ToList();
        Assert.Equal(2, rows.Count);

        // Instance 2 is closed; instance 1 is the explicitly requested target.
        var closed = Assert.Single(rows, e => e.Id == 1);
        Assert.Equal(2, closed.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Inactive, closed.EnrollmentStatus);
        Assert.NotNull(closed.DropDate);

        var created = Assert.Single(rows, e => e.Id != 1);
        Assert.Equal(1, created.SemesterInstanceId);
        Assert.Equal(StudentEnrollmentStatus.Active, created.EnrollmentStatus);
    }

    private static void SeedProgramSemesters(AppDbContext ctx)
    {
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 1, ProgramId = TestData.ProgramId, SemesterId = 1, IsActive = true });
        ctx.ProgramSemesters.Add(new ProgramSemester { Id = 2, ProgramId = TestData.ProgramId, SemesterId = 2, IsActive = true });
    }

    private static void SeedMixedStatusEnrollments(AppDbContext ctx)
    {
        SeedPromotionBase(ctx);
        ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
        var closed = TestData.Enrollment(1, 1, 1, StudentEnrollmentStatus.Inactive);
        closed.DropDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        closed.DropReason = "Transfer to BIT";
        ctx.SemesterEnrollments.Add(closed);
        ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 2, StudentEnrollmentStatus.Active));
    }

    [Fact]
    public async Task GetEnrollmentsAsync_DefaultsToActiveOnly()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedMixedStatusEnrollments(ctx);
        });
        var service = CreateService(db);

        var (items, total) = await service.GetEnrollmentsAsync(1, 10, null, "EnrolledDate", "desc");

        // Transfers close rows instead of deleting them, so the default list must not show
        // the closed history row alongside the live one.
        Assert.Equal(1, total);
        Assert.Equal(2, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_ZeroStatusReturnsAll()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedMixedStatusEnrollments(ctx);
        });
        var service = CreateService(db);

        var (items, total) = await service.GetEnrollmentsAsync(1, 10, null, "EnrolledDate", "desc", enrollmentStatus: 0);

        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_FiltersBySpecificStatus()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedMixedStatusEnrollments(ctx);
        });
        var service = CreateService(db);

        var (items, total) = await service.GetEnrollmentsAsync(1, 10, null, "EnrolledDate", "desc",
            enrollmentStatus: (int)StudentEnrollmentStatus.Inactive);

        Assert.Equal(1, total);
        Assert.Equal(1, Assert.Single(items).Id);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_ProjectsHistoricalProgramFromSemesterInstance()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            // SeedBase's instances 1 and 2 belong to ProgramId. Give instance 2 the OTHER
            // program so the row can tell the two projections apart.
            ctx.SemesterInstances.Local.Single(si => si.Id == 2).ProgramId = TestData.ProgramIdOther;
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            var closed = TestData.Enrollment(1, 1, 2, StudentEnrollmentStatus.Inactive);
            closed.DropDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            closed.DropReason = "Transfer to BIT";
            ctx.SemesterEnrollments.Add(closed);
        });
        var service = CreateService(db);

        var (items, _) = await service.GetEnrollmentsAsync(1, 10, null, "EnrolledDate", "desc",
            enrollmentStatus: (int)StudentEnrollmentStatus.Inactive);

        // The admission already points at ProgramId, so reading the program from the
        // admission would mislabel this closed row as the current program. The
        // SemesterInstance is the per-row historical program.
        Assert.Equal("Bachelor in Information Technology", Assert.Single(items).ProgramName);
    }

    [Fact]
    public async Task GetEnrollmentsAsync_RespectsFacultyAdminScope()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.Faculties.Add(new Faculty { Id = 5, Name = "Engineering", OfficeCode = "ENG" });
            ctx.Faculties.Add(new Faculty { Id = 6, Name = "Management", OfficeCode = "MGT" });
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramId).FacultyId = 5;
            ctx.Programs.Local.Single(p => p.Id == TestData.ProgramIdOther).FacultyId = 6;

            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId, programId: TestData.ProgramId));
            ctx.StudentAdmissions.Add(TestData.Admission(2, UserId, programId: TestData.ProgramIdOther));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1, StudentEnrollmentStatus.Active));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 2, 1, StudentEnrollmentStatus.Active));
        });

        var uc = new TestUserContext();
        uc.SetUser(UserId, 5, null, [], [Role.FacultyAdmin]);
        var service = CreateService(db, uc);

        var (_, total) = await service.GetEnrollmentsAsync(1, 10, null, "EnrolledDate", "desc");

        // Without a FacultyAdmin branch in BuildQuery this returned both rows.
        Assert.Equal(1, total);
    }
}
