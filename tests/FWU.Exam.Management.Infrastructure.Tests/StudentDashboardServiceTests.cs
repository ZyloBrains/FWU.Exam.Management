using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class StudentDashboardServiceTests
{
    private const string UserId = "user-1";
    private const string Email = "stu@test.com";

    private static StudentDashboardService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext(), NullLogger<StudentDashboardService>.Instance);

    private static DateOnly Past => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
    private static DateOnly Future => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsEmpty_WhenStudentHasNoAdmission()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);
        var student = TestData.StudentRegistration(1, Email);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsEmpty_WhenStudentHasNoEnrollment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Future, null));
        });

        var student = db.Context.StudentRegistrations!.FirstOrDefault() ?? TestData.StudentRegistration(1, Email);
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsSchedulesForAllEnrolledSemesters()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Future, null));       // regular sem1 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Regular, Future, null));       // regular sem2 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(13, 3, TestData.Regular, Future, null));       // regular sem3 (not enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(14, 2, TestData.Supplementary, Future, null)); // supplementary own semester => hidden (strict-below rule)
            ctx.ExamSchedules.Add(TestData.Schedule(15, 2, TestData.Entrance, Future, null));      // entrance (excluded)
            ctx.ExamSchedules.Add(TestData.Schedule(16, 2, TestData.Regular, Future, null, TestData.ProgramIdOther)); // other program

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 12));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 12, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 102, TestData.Regular, "F", 12)); // failed in sem2
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == 11);
        Assert.Contains(result, s => s.Id == 12);
        Assert.DoesNotContain(result, s => s.Id == 13);
        Assert.DoesNotContain(result, s => s.Id == 14);
        Assert.DoesNotContain(result, s => s.Id == 15);
        Assert.DoesNotContain(result, s => s.Id == 16);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsLowerSemesterPartial_ForPromotedStudent_WithoutHistory()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Partial, Future, null)); // sem1 partial < enrolled sem2
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Partial, Future, null)); // own semester => hidden
            ctx.ExamSchedules.Add(TestData.Schedule(13, 3, TestData.Partial, Future, null)); // higher semester => hidden
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsLowerSemesterPartial_WhenStudentFailedThere()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));   // original sem1 exam
            ctx.ExamSchedules.Add(TestData.Schedule(12, 1, TestData.Partial, Future, null)); // partial sem1

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "F", 11)); // failed sem1 subject
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(12, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_HidesLowerSemesterPartial_WhenHistoryExistsAndNoFailures()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));   // original sem1 exam
            ctx.ExamSchedules.Add(TestData.Schedule(12, 1, TestData.Partial, Future, null)); // partial sem1

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "A", 11)); // passed everything
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsPartial_WhenOnlyPendingResultsExistAfterPayment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));   // original sem1 exam
            ctx.ExamSchedules.Add(TestData.Schedule(12, 1, TestData.Partial, Future, null)); // partial sem1

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "F", 11));

            ctx.ApplicationVouchers.Add(TestData.Voucher(2, 1, 12));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(2, 12, 1));
            ctx.ExamSubjectResults.Add(new ExamSubjectResult
            {
                Id = 2,
                TenantId = TestData.TenantId,
                ExamRegistrationId = 2,
                SubjectOfferingId = 101,
                ExamTypeId = TestData.Partial,
                ExamScheduleId = 12,
                GradeLetter = null,
                IsActive = true,
                IsSubmitted = false
            });
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Contains(result, s => s.Id == 12);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_HidesPartial_WhenRetakePassedWithGradedResult()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));   // original sem1 exam
            ctx.ExamSchedules.Add(TestData.Schedule(12, 1, TestData.Partial, Future, null)); // partial sem1

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "F", 11));

            ctx.ApplicationVouchers.Add(TestData.Voucher(2, 1, 12));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(2, 12, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(2, 2, 101, TestData.Partial, "A", 12));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task IsScheduleVisibleToStudentAsync_MatchesListingRules()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Partial, Future, null)); // visible
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Partial, Future, null)); // own semester
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        Assert.True(await service.IsScheduleVisibleToStudentAsync(student, UserId, 11));
        Assert.False(await service.IsScheduleVisibleToStudentAsync(student, UserId, 12));
        Assert.False(await service.IsScheduleVisibleToStudentAsync(student, UserId, 999));
    }

    [Fact]
    public async Task GetReExamSelectableOfferingsAsync_UsesStudentBatchCurriculumVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedCollegeForStandardTenant(ctx);
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            sr.AcademicYearId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));

            ctx.AcademicYears.Add(TestData.AcademicYear(2, "2026"));
            ctx.SemesterInstances.Add(new SemesterInstance { Id = 20, TenantId = TestData.TenantId, SemesterId = 2, AcademicYearId = 2, ProgramId = TestData.ProgramId });
            ctx.CurriculumVersions.Add(new CurriculumVersion { Id = 90, TenantId = TestData.TenantId, Name = "Batch 2025", ProgramId = TestData.ProgramId, EffectiveAcademicYearId = 1, IsActive = true });
            ctx.CurriculumVersions.Add(new CurriculumVersion { Id = 91, TenantId = TestData.TenantId, Name = "New 2026", ProgramId = TestData.ProgramId, EffectiveAcademicYearId = 2, IsActive = true });

            ctx.SubjectOfferings.Add(new SubjectOffering { Id = 301, TenantId = TestData.TenantId, SubjectCatalogId = 1, ProgramId = TestData.ProgramId, SemesterId = 2, CurriculumVersionId = 90, IsActive = true, IsCompulsory = true, DisplayOrder = 1, HasTheory = true });
            ctx.SubjectOfferings.Add(new SubjectOffering { Id = 302, TenantId = TestData.TenantId, SubjectCatalogId = 1, ProgramId = TestData.ProgramId, SemesterId = 2, CurriculumVersionId = 91, IsActive = true, IsCompulsory = true, DisplayOrder = 2, HasTheory = true });

            ctx.ExamSchedules.Add(TestData.Schedule(30, 20, TestData.Partial, Future, null));
        });

        var service = CreateService(db);

        var registration = await service.GetStudentRegistrationByUserIdAsync(UserId);
        Assert.NotNull(registration);
        Assert.Equal(1, registration!.AcademicYearId);

        var resolved = await CurriculumVersionResolver.ResolveAsync(db.Context, TestData.ProgramId, registration.AcademicYearId);
        Assert.Equal(90, resolved);

        var offerings = await service.GetReExamSelectableOfferingsAsync(30, UserId);

        var ids = offerings.Select(o => o.Id).ToList();
        Assert.Contains(301, ids);          // student's batch version
        Assert.DoesNotContain(302, ids);    // newer schedule-year version
        Assert.DoesNotContain(102, ids);    // unversioned leftovers
    }

    [Fact]
    public async Task GetReExamSelectableOfferingsAsync_FallsBackToUnversioned_WhenNoBatchVersionMatches()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedCollegeForStandardTenant(ctx);
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            sr.AcademicYearId = 1; // no curriculum version effective by this year
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));

            ctx.AcademicYears.Add(TestData.AcademicYear(2, "2026"));
            ctx.SemesterInstances.Add(new SemesterInstance { Id = 20, TenantId = TestData.TenantId, SemesterId = 2, AcademicYearId = 2, ProgramId = TestData.ProgramId });
            ctx.CurriculumVersions.Add(new CurriculumVersion { Id = 91, TenantId = TestData.TenantId, Name = "New 2026", ProgramId = TestData.ProgramId, EffectiveAcademicYearId = 2, IsActive = true });
            ctx.SubjectOfferings.Add(new SubjectOffering { Id = 302, TenantId = TestData.TenantId, SubjectCatalogId = 1, ProgramId = TestData.ProgramId, SemesterId = 2, CurriculumVersionId = 91, IsActive = true, IsCompulsory = true, DisplayOrder = 2, HasTheory = true });

            ctx.ExamSchedules.Add(TestData.Schedule(30, 20, TestData.Partial, Future, null));
        });

        var service = CreateService(db);

        var offerings = await service.GetReExamSelectableOfferingsAsync(30, UserId);

        var ids = offerings.Select(o => o.Id).ToList();
        Assert.Contains(102, ids);          // unversioned sem-2 offering
        Assert.DoesNotContain(302, ids);    // newer version excluded
    }

    [Fact]
    public async Task GetReExamSelectableOfferingsAsync_FallsBackToScheduleResolution_WhenBatchVersionHasNoOfferings()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedCollegeForStandardTenant(ctx);
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            sr.AcademicYearId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));

            // Exclude SeedBase's unversioned sem-2 offering so both earlier
            // fallback legs yield nothing and the schedule resolution applies.
            // (Entities added in this lambda are not saved yet, so detach the
            // tracked instance instead of issuing a database delete.)
            var seedSem2Offering = ctx.ChangeTracker.Entries<SubjectOffering>()
                .First(e => e.Entity.Id == 102);
            seedSem2Offering.State = EntityState.Detached;

            ctx.AcademicYears.Add(TestData.AcademicYear(2, "2026"));
            ctx.SemesterInstances.Add(new SemesterInstance { Id = 20, TenantId = TestData.TenantId, SemesterId = 2, AcademicYearId = 2, ProgramId = TestData.ProgramId });
            ctx.CurriculumVersions.Add(new CurriculumVersion { Id = 90, TenantId = TestData.TenantId, Name = "Batch 2025", ProgramId = TestData.ProgramId, EffectiveAcademicYearId = 1, IsActive = true });
            ctx.CurriculumVersions.Add(new CurriculumVersion { Id = 91, TenantId = TestData.TenantId, Name = "New 2026", ProgramId = TestData.ProgramId, EffectiveAcademicYearId = 2, IsActive = true });
            ctx.SubjectOfferings.Add(new SubjectOffering { Id = 302, TenantId = TestData.TenantId, SubjectCatalogId = 1, ProgramId = TestData.ProgramId, SemesterId = 2, CurriculumVersionId = 91, IsActive = true, IsCompulsory = true, DisplayOrder = 2, HasTheory = true });

            ctx.ExamSchedules.Add(TestData.Schedule(30, 20, TestData.Partial, Future, null));
        });

        var service = CreateService(db);

        var offerings = await service.GetReExamSelectableOfferingsAsync(30, UserId);

        Assert.Equal([302], offerings.Select(o => o.Id).ToList());
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsLatestActiveEnrollmentSemester()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 3));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(3, 1, 4, StudentEnrollmentStatus.Dropped));
        });

        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Equal(3, semesterId);
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsNull_WhenStudentHasNoAdmission()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Null(semesterId);
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsNull_WhenNoActiveEnrollment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2, StudentEnrollmentStatus.Inactive));
        });

        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Null(semesterId);
    }

    [Fact]
    public async Task GetSubjectOfferingsForScheduleAsync_FallsBackToAllOfferings_WhenNoCurriculumVersionMatches()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.SubjectOfferings.Add(TestData.Offering(210, 2, TestData.ProgramId));
            ctx.ExamSchedules.Add(TestData.Schedule(31, 2, TestData.Regular, Past, null));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForScheduleAsync(31);

        Assert.Equal(new[] { 102, 210 }, offerings.Select(o => o.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetSubjectOfferingsForScheduleAsync_ReturnsOnlyOfferingsForResolvedCurriculumVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.AcademicYears.Add(TestData.AcademicYear(2, "2082"));
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 1,
                Name = "Old",
                IsActive = true
            });
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 2,
                TenantId = TestData.TenantId,
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 2,
                Name = "New",
                IsActive = true
            });

            ctx.SubjectOfferings.Local.First(o => o.Id == 102).CurriculumVersionId = 1;
            var v2Offering = TestData.Offering(210, 2, TestData.ProgramId);
            v2Offering.CurriculumVersionId = 2;
            ctx.SubjectOfferings.Add(v2Offering);

            // Batch year 1 resolves to version 1 (latest EffectiveAcademicYearId <= 1).
            ctx.ExamSchedules.Add(TestData.Schedule(31, 2, TestData.Regular, Past, null));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForScheduleAsync(31);

        Assert.Equal(new[] { 102 }, offerings.Select(o => o.Id));
    }

    [Fact]
    public async Task GetSubjectOfferingsForScheduleAsync_UsesLatestVersionAtOrBeforeBatchYear()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.AcademicYears.Add(TestData.AcademicYear(2024, "2024"));
            ctx.AcademicYears.Add(TestData.AcademicYear(2025, "2025"));
            ctx.AcademicYears.Add(TestData.AcademicYear(2026, "2026"));
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 2024,
                Name = "2024 Curriculum",
                IsActive = true
            });
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 2,
                TenantId = TestData.TenantId,
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 2026,
                Name = "2026 Curriculum",
                IsActive = true
            });

            ctx.SubjectOfferings.Local.First(o => o.Id == 102).CurriculumVersionId = 1;
            var v2Offering = TestData.Offering(210, 2, TestData.ProgramId);
            v2Offering.CurriculumVersionId = 2;
            ctx.SubjectOfferings.Add(v2Offering);

            // Batch year 2025 sits between the two versions; must use the 2024 one.
            ctx.SemesterInstances.Add(new SemesterInstance
            {
                Id = 20,
                TenantId = TestData.TenantId,
                SemesterId = 2,
                AcademicYearId = 2025,
                ProgramId = TestData.ProgramId,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(-1).AddMonths(6)
            });
            ctx.ExamSchedules.Add(TestData.Schedule(31, 20, TestData.Regular, Past, null));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForScheduleAsync(31);

        Assert.Equal(new[] { 102 }, offerings.Select(o => o.Id));
    }

    [Fact]
    public async Task GetSubjectOfferingsForScheduleAsync_MatchesOfferingsAcrossYearsBySemesterNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.AcademicYears.Add(TestData.AcademicYear(2014, "2014"));
            ctx.AcademicYears.Add(TestData.AcademicYear(2022, "2022"));

            // Two global semesters with the same Number (2)
            var sem2014 = TestData.Semester(50, 2);
            sem2014.Name = "Semester 2 (2014)";
            sem2014.Code = "SEM2-2014";
            ctx.Semesters.Add(sem2014);

            var sem2022 = TestData.Semester(51, 2);
            sem2022.Name = "Semester 2 (2022)";
            sem2022.Code = "SEM2-2022";
            ctx.Semesters.Add(sem2022);

            // Instances bind each semester to a different academic year
            ctx.SemesterInstances.Add(new SemesterInstance
            {
                Id = 50,
                TenantId = TestData.TenantId,
                SemesterId = 50,
                AcademicYearId = 2014,
                ProgramId = TestData.ProgramId,
                StartDate = DateTime.UtcNow.AddYears(-12),
                EndDate = DateTime.UtcNow.AddYears(-12).AddMonths(6)
            });
            ctx.SemesterInstances.Add(new SemesterInstance
            {
                Id = 51,
                TenantId = TestData.TenantId,
                SemesterId = 51,
                AcademicYearId = 2022,
                ProgramId = TestData.ProgramId,
                StartDate = DateTime.UtcNow.AddYears(-4),
                EndDate = DateTime.UtcNow.AddYears(-4).AddMonths(6)
            });

            // Offering linked to 2014's Semester 2 (SemesterId=50)
            var offering = TestData.Offering(210, 50, TestData.ProgramId);
            ctx.SubjectOfferings.Add(offering);

            // Schedule for 2022's Semester 2 (SemesterInstanceId=51) — different ID, same Number
            ctx.ExamSchedules.Add(TestData.Schedule(31, 51, TestData.Regular, Past, null));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForScheduleAsync(31);

        // Should find offering 210 (Semester.Number=2 in 2014) even though schedule uses SemesterId=51 (Number=2 in 2022)
        Assert.Contains(offerings, o => o.Id == 210);
    }

    [Fact]
    public async Task GetSubjectOfferingsForStudentAsync_ReturnsActiveVersionOfferingsForEnrolledSemester()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 5,
                TenantId = TestData.TenantId,
                Name = "Default - BCA (2081)",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });
            ctx.SubjectOfferings.Local.First(o => o.Id == 102).CurriculumVersionId = 5;
            ctx.SubjectOfferings.Add(TestData.Offering(210, 2, TestData.ProgramId));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForStudentAsync(UserId, TestData.ProgramId);

        var offering = Assert.Single(offerings);
        Assert.Equal(102, offering.Id);
    }

    [Fact]
    public async Task GetSubjectOfferingsForStudentAsync_FallsBackToAllSemesterOfferings_WhenNoActiveVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.SubjectOfferings.Add(TestData.Offering(210, 2, TestData.ProgramId));
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForStudentAsync(UserId, TestData.ProgramId);

        Assert.Equal(new[] { 102, 210 }, offerings.Select(o => o.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetSubjectOfferingsForStudentAsync_IncludesSubjectTypeForElectiveGrouping()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.SubjectTypes.Add(new SubjectType { Id = 7, Code = "EL", Name = "Elective Group A", IsActive = true });
            ctx.SubjectCatalogs.Add(new SubjectCatalog { Id = 2, TenantId = TestData.TenantId, SubjectCode = "EL1", SubjectName = "Elective One", SubjectTypeId = 7, IsActive = true });
            var elective = TestData.Offering(210, 2, TestData.ProgramId);
            elective.SubjectCatalogId = 2;
            elective.IsCompulsory = false;
            ctx.SubjectOfferings.Add(elective);
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForStudentAsync(UserId, TestData.ProgramId);

        Assert.Equal(2, offerings.Count);
        Assert.All(offerings, o =>
        {
            Assert.NotNull(o.SubjectCatalog);
            Assert.NotNull(o.SubjectCatalog!.SubjectType);
        });
        var electiveRow = Assert.Single(offerings, o => !o.IsCompulsory);
        Assert.Equal("Elective Group A", electiveRow.SubjectCatalog!.SubjectType!.Name);
    }

    [Fact]
    public async Task GetSubjectOfferingsForStudentAsync_ResolvesVersionByEnrolledSemesterAcademicYear()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            var secondYear = new AcademicYear
            {
                Id = 2,
                TenantId = TestData.TenantId,
                AcademicYearCode = "2082",
                AcademicYearName = "2082",
                AcademicYearNameNepali = "2082",
                IsActive = true,
                IsRunning = false
            };
            ctx.AcademicYears.Add(secondYear);

            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 5,
                TenantId = TestData.TenantId,
                Name = "Version 2081",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 6,
                TenantId = TestData.TenantId,
                Name = "Version 2082",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 2,
                IsActive = true
            });

            ctx.SubjectOfferings.Local.First(o => o.Id == 102).CurriculumVersionId = 5;
            ctx.SubjectOfferings.Local.First(o => o.Id == 102).SubjectCatalogId = 1;
            var offering210 = TestData.Offering(210, 2, TestData.ProgramId);
            offering210.CurriculumVersionId = 6;
            offering210.SubjectCatalogId = 1;
            ctx.SubjectOfferings.Add(offering210);
        });
        var service = CreateService(db);

        var offerings = await service.GetSubjectOfferingsForStudentAsync(UserId, TestData.ProgramId);

        var offering = Assert.Single(offerings);
        Assert.Equal(102, offering.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_HidesSchedule_WhenStartDateIsInFuture()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            var schedule = TestData.Schedule(11, 2, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null);
            ctx.ExamSchedules.Add(schedule);
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSchedule_WhenWithinDateWindow()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Future, null));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_HidesSchedule_WhenEndDateHasPassed()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSchedule_WhenExtendedDateIsInFuture()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            var schedule = TestData.Schedule(11, 2, TestData.Regular, Past, null);
            schedule.ExtendedDate = DateTime.UtcNow.AddDays(5);
            ctx.ExamSchedules.Add(schedule);
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSchedule_WhenNoDateWindowSet()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, null, null));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetResultRecordsAsync_LoadsExamScheduleBelongingToAnotherTenant()
    {
        using var db = new TestDb(TestTenantContext.Standard(2), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Tenants.Add(new Tenant
            {
                Id = 2,
                Name = "EEO",
                OfficeCode = "EEO",
                ContactNumber = "0",
                Address = "Mahendranagar",
                Email = "eng@test.com",
                TenantType = TenantType.Standard,
                IsActive = true
            });

            ctx.AcademicYears.Local.Single(a => a.Id == TestData.AcademicYearId).TenantId = 2;

            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, null));

            ctx.Faculties.Add(new Faculty { Id = 5, Name = "Engineering", OfficeCode = "L091", TenantId = 2 });
            ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = 2, CollegeId = TestData.CollegeId, FacultyId = 5 });

            ctx.ResultRecords.Add(new ResultRecord
            {
                Id = 1,
                TenantId = 2,
                AcademicYearId = TestData.AcademicYearId,
                ProgramsId = TestData.ProgramId,
                ExamTypeId = TestData.Regular,
                CollegeId = TestData.CollegeId,
                ExamScheduleId = 21,
                RegistrationNumber = "REG10",
                SymbolNumber = "SYM1",
                Year = "I",
                Part = "I",
                DateOfBirthBs = "2050-01-01",
                ResultRecordMasterId = 1,
                TotalObtainedGrade = "B",
                Gpa = "3.00",
                Result = "Pass",
                StudentName = "Test Student"
            });
        });

        var service = CreateService(db);

        var result = await service.GetResultRecordsAsync("REG10");

        var rr = Assert.Single(result);
        Assert.Equal(21, rr.ExamScheduleId);
        Assert.NotNull(rr.ExamSchedule);
        Assert.Equal("Schedule 21", rr.ExamSchedule!.ExamScheduleName);
        Assert.NotNull(rr.ExamSchedule.SemesterInstance?.Semester);
        Assert.Equal(1, rr.ExamSchedule.SemesterInstance!.Semester!.Number);
    }

    [Fact]
    public async Task GetStudentExamRegistrationsAsync_KeepsRegistrationsWhenScheduleBelongsToAnotherTenant()
    {
        using var db = new TestDb(TestTenantContext.Standard(2), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Tenants.Add(new Tenant
            {
                Id = 2,
                Name = "EEO",
                OfficeCode = "EEO",
                ContactNumber = "0",
                Address = "Mahendranagar",
                Email = "eng@test.com",
                TenantType = TenantType.Standard,
                IsActive = true
            });
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, null));
            ctx.Faculties.Add(new Faculty { Id = 5, Name = "Engineering", OfficeCode = "L091", TenantId = 2 });
            ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = 2, CollegeId = TestData.CollegeId, FacultyId = 5 });

            ctx.SubjectCatalogs.Add(new SubjectCatalog
            {
                Id = 7,
                TenantId = 2,
                SubjectCode = "SUB7",
                SubjectName = "Subject 7",
                SubjectTypeId = 1,
                IsActive = true
            });
            ctx.SubjectOfferings.Add(new SubjectOffering
            {
                Id = 301,
                TenantId = 2,
                SubjectCatalogId = 7,
                ProgramId = TestData.ProgramId,
                SemesterId = 1,
                IsCompulsory = true,
                DisplayOrder = 1,
                HasTheory = true,
                HasPractical = false,
                HasInternal = true,
                TheoryFullMarks = 100,
                TheoryPassMarks = 40
            });

            ctx.Users.Add(TestData.User(UserId, Email));

            var sr = TestData.StudentRegistration(2, Email);
            sr.TenantId = 2;
            ctx.StudentRegistrations.Add(sr);

            var voucher = TestData.Voucher(2, 2, 21);
            voucher.TenantId = 2;
            ctx.ApplicationVouchers.Add(voucher);

            var examRegistration = TestData.ExamRegistration(2, 21, 2);
            examRegistration.TenantId = 2;
            ctx.ExamRegistrations.Add(examRegistration);

            var subjectResult = TestData.Result(2, 2, 301, TestData.Regular, "B", 21);
            subjectResult.TenantId = 2;
            ctx.ExamSubjectResults.Add(subjectResult);
        });

        var service = CreateService(db);

        var result = await service.GetStudentExamRegistrationsAsync(UserId);

        var registration = Assert.Single(result);
        Assert.Equal(21, registration.ExamScheduleId);
        Assert.NotNull(registration.ExamSchedule);
        Assert.Equal("Schedule 21", registration.ExamSchedule!.ExamScheduleName);

        var subject = Assert.Single(registration.ExamSubjectResults!);
        Assert.NotNull(subject.SubjectOffering?.SubjectCatalog);
        Assert.Equal("Subject 7", subject.SubjectOffering!.SubjectCatalog!.SubjectName);
    }

    private static void SeedLocationData(AppDbContext ctx)
    {
        ctx.Provinces.Add(new Province { Id = 1, ProvinceName = "Bagmati", IsActive = true });
        ctx.Districts.Add(new District { Id = 1, ProvinceId = 1, DistrictName = "Kathmandu", IsActive = true });
        ctx.LocalLevels.Add(new LocalLevel
        {
            Id = 1,
            DistrictId = 1,
            LocalLevelName = "Kathmandu Metropolitan",
            LocalLevelType = LocalLevelType.Metropolitan,
            IsActive = true
        });
        ctx.Addresses.Add(new Address { Id = 1, LocalLevelId = 1, WardNumber = 1, IsActive = true });
        ctx.Ethnicities.Add(new Ethnicity { Id = 1, EthnicityName = "Brahmin", IsActive = true });
    }

    private static StudentRegistration CompleteRegistration(int id)
    {
        var sr = TestData.StudentRegistration(id, Email);
        sr.ContactNumber = "9800000000";
        sr.GenderId = 1;
        sr.EthnicityId = 1;
        sr.PermanentAddressId = 1;
        return sr;
    }

    [Fact]
    public async Task GetMissingMandatoryProfileFieldsAsync_ReturnsEmpty_WhenAllFieldsPresent()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            SeedLocationData(ctx);
            ctx.StudentRegistrations.Add(CompleteRegistration(1));
        });
        var service = CreateService(db);

        var missing = await service.GetMissingMandatoryProfileFieldsAsync(null, Email, "9800000000", "uploads/profile.png", "uploads/sign.png");

        Assert.Empty(missing);
    }

    [Fact]
    public async Task GetMissingMandatoryProfileFieldsAsync_ReturnsAllFields_WhenNothingPresent()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var missing = await service.GetMissingMandatoryProfileFieldsAsync(null, null, null, null, null);

        Assert.Equal(new[] { "Phone Number", "Province", "District", "Local Level", "Gender", "Ethnicity", "Profile Photo", "Student Signature" }, missing);
    }

    [Fact]
    public async Task GetMissingMandatoryProfileFieldsAsync_ReturnsProfileFields_WhenRegistrationIsIncomplete()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
        });
        var service = CreateService(db);

        var missing = await service.GetMissingMandatoryProfileFieldsAsync(null, Email, "9800000000", "uploads/profile.png", "uploads/sign.png");

        Assert.Equal(new[] { "Province", "District", "Local Level", "Gender", "Ethnicity" }, missing);
    }

    [Fact]
    public async Task GetMissingMandatoryProfileFieldsAsync_ReturnsUploads_WhenPhotosMissing()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            SeedLocationData(ctx);
            ctx.StudentRegistrations.Add(CompleteRegistration(1));
        });
        var service = CreateService(db);

        var missing = await service.GetMissingMandatoryProfileFieldsAsync(null, Email, "9800000000", null, null);

        Assert.Equal(new[] { "Profile Photo", "Student Signature" }, missing);
    }

    [Fact]
    public async Task GetMissingMandatoryProfileFieldsAsync_FallsBackToRegistrationContactNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedLocationData(ctx);
            ctx.StudentRegistrations.Add(CompleteRegistration(1));
        });
        var service = CreateService(db);

        var missing = await service.GetMissingMandatoryProfileFieldsAsync(null, Email, null, "uploads/profile.png", "uploads/sign.png");

        Assert.DoesNotContain("Phone", missing);
    }

    [Fact]
    public async Task GetStudentRegistrationByEmailAsync_LoadsPermanentAddressChain()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            SeedLocationData(ctx);
            ctx.StudentRegistrations.Add(CompleteRegistration(1));
        });
        var service = CreateService(db);

        var reg = await service.GetStudentRegistrationByEmailAsync(Email);

        Assert.NotNull(reg);
        Assert.NotNull(reg!.PermanentAddress);
        Assert.NotNull(reg.PermanentAddress!.LocalLevel);
        Assert.NotNull(reg.PermanentAddress.LocalLevel!.District);
        Assert.NotNull(reg.PermanentAddress.LocalLevel.District!.Province);
        Assert.Equal("Bagmati", reg.PermanentAddress.LocalLevel.District!.Province!.ProvinceName);
    }

    [Fact]
    public async Task GetStudentRegistrationByEmailAsync_DoesNotMatchRegistrationNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            var sr = TestData.StudentRegistration(1, "student@example.com");
            sr.RegistrationNumber = "REG-SPECIAL";
            ctx.StudentRegistrations.Add(sr);
        });
        var service = CreateService(db);

        var byEmail = await service.GetStudentRegistrationByEmailAsync("REG-SPECIAL");
        var byRegNumber = await service.GetStudentRegistrationByEmailAsync("student@example.com");

        Assert.Null(byEmail);
        Assert.NotNull(byRegNumber);
    }

    [Fact]
    public async Task GetStudentRegistrationByUserIdAsync_LoadsRegistrationViaAdmissionLink()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            SeedLocationData(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = CompleteRegistration(1);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
        });
        var service = CreateService(db);

        var reg = await service.GetStudentRegistrationByUserIdAsync(UserId);

        Assert.NotNull(reg);
        Assert.Equal(1, reg!.Id);
        Assert.Equal(1, reg.StudentAdmissionId);
    }

    [Fact]
    public async Task GetStudentRegistrationByUserIdAsync_ResolvesViaUserNameRegistrationNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            TestData.SeedCollegeForStandardTenant(ctx);
            SeedLocationData(ctx);
            var sr = CompleteRegistration(1);
            sr.StudentAdmissionId = null;
            ctx.StudentRegistrations.Add(sr);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.Users.Local.Single().UserName = sr.RegistrationNumber;
        });
        var service = CreateService(db);

        var reg = await service.GetStudentRegistrationByUserIdAsync(UserId);

        Assert.NotNull(reg);
        Assert.Equal(1, reg!.Id);
        Assert.Equal("REG1", reg.RegistrationNumber);
    }

    private static void SeedRejectedForm(AppDbContext ctx, bool withPayment = true)
    {
        TestData.SeedBase(ctx);
        ctx.Users.Add(TestData.User(UserId, Email));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), null));
        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));

        if (withPayment)
        {
            ctx.Set<PaymentType>().Add(new PaymentType { Id = 1, PaymentTypeName = "Online", IsActive = true });
            ctx.PaymentRequestLogs.Add(new PaymentRequestLog
            {
                TenantId = TestData.TenantId,
                PaymentRequestLogStatus = 1,
                InvoiceNumber = "INV-1",
                ForwardedTimestamp = DateTime.UtcNow,
                FullName = "Test Student",
                Amount = 1000,
                FullRequestContent = "{}",
                PaymentTypeId = 1,
                StudentRegistrationId = 1,
                ExamScheduleId = 21,
                SelectedSubjectIds = "101"
            });
        }

        var rejected = TestData.ExamRegistration(1, 21, 1);
        rejected.Status = RegistrationStatus.Rejected;
        rejected.Remarks = "[Rejected by admin on 2026-08-20 10:00 UTC] Wrong subject selected";
        ctx.ExamRegistrations.Add(rejected);

        var result = TestData.Result(1, 1, 101, TestData.Regular, null, 21);
        result.GradeLetter = null;
        result.IsSubmitted = false;
        ctx.ExamSubjectResults.Add(result);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_RevivesSameForm_AndSyncsSubjectsAndLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            ctx.SubjectOfferings.Add(TestData.Offering(302, 1, TestData.ProgramId));
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 });

        Assert.True(success, message);

        var er = db.Context.ExamRegistrations!.Single(e => e.Id == 1);
        Assert.Equal(RegistrationStatus.Pending, er.Status);
        Assert.Contains("[Re-applied by", er.Remarks!);

        var log = db.Context.PaymentRequestLogs!
            .Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("302", log.SelectedSubjectIds);

        var results = db.Context.ExamSubjectResults!.Where(r => r.ExamRegistrationId == 1).ToList();
        Assert.False(results.Single(r => r.SubjectOfferingId == 101).IsActive);
        Assert.True(results.Single(r => r.SubjectOfferingId == 302).IsActive);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_PartialForm_SubjectFromOlderCurriculumVersion_IsAccepted()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            ctx.ExamSchedules.Local.Single(s => s.Id == 21).ExamTypeId = TestData.Partial;

            // The schedule's academic year resolves to version 2…
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                Name = "Curriculum v1",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 1,
                IsActive = true
            });
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 2,
                TenantId = TestData.TenantId,
                Name = "Curriculum v2",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });

            // …but the student's subject belongs to the older version 1.
            var oldOffering = TestData.Offering(302, 1, TestData.ProgramId);
            oldOffering.CurriculumVersionId = 1;
            ctx.SubjectOfferings.Add(oldOffering);
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 });

        Assert.True(success, message);
        Assert.True(db.Context.ExamSubjectResults!
            .Single(r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 302).IsActive);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_RegularForm_SubjectOutsideResolvedVersion_IsRejected()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);

            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                Name = "Curriculum v1",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = 1,
                IsActive = true
            });
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 2,
                TenantId = TestData.TenantId,
                Name = "Curriculum v2",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });

            // Regular forms stay pinned to the schedule's resolved version.
            var oldOffering = TestData.Offering(302, 1, TestData.ProgramId);
            oldOffering.CurriculumVersionId = 1;
            ctx.SubjectOfferings.Add(oldOffering);
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 });

        Assert.False(success);
        Assert.Equal("One or more selected subjects are not offered for this exam schedule.", message);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_ReplacesRemarks_NeverAppends()
    {
        var longReason = new string('x', 200);
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            ctx.SubjectOfferings.Add(TestData.Offering(302, 1, TestData.ProgramId));
            ctx.ExamRegistrations.Local.Single(e => e.Id == 1).Remarks =
                $"[Rejected by admin on 2026-08-20 10:00 UTC] {longReason}";
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 });

        Assert.True(success, message);
        var remarks = db.Context.ExamRegistrations!.Single(e => e.Id == 1).Remarks!;
        // Replacement semantics: only the re-applied marker remains, well under nvarchar(255).
        Assert.DoesNotContain("Rejected by", remarks);
        Assert.StartsWith("[Re-applied by", remarks);
        Assert.True(remarks.Length <= 255);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_Blocked_WhenLiveFormExists()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            ctx.ApplicationVouchers.Add(TestData.Voucher(2, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(2, 21, 2));
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 101 });

        Assert.False(success);
        Assert.Contains("not rejected", message);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_Blocked_WithoutConfirmedPayment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => SeedRejectedForm(ctx, withPayment: false));
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 101 });

        Assert.False(success);
        Assert.Contains("Payment has not been confirmed", message);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_Blocked_ForOfferingOutsideSchedule()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => SeedRejectedForm(ctx));
        var service = CreateService(db);

        // Offering 102 belongs to semester 2; the schedule covers semester 1.
        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 102 });

        Assert.False(success);
        Assert.Contains("not offered for this exam schedule", message);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_WithLegSelection_StampsRowFlagsAndTokenLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            var offering = TestData.Offering(302, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 },
            new Dictionary<int, ReExamLegs> { [302] = ReExamLegs.Practical });

        Assert.True(success, message);

        var row = db.Context.ExamSubjectResults!
            .Single(r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 302 && r.IsActive);
        Assert.False(row.IsTheoryRegistered);
        Assert.True(row.IsPracticalRegistered);

        var log = db.Context.PaymentRequestLogs!
            .Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("302:P", log.SelectedSubjectIds);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_BothLegsOnPartial_CarriesTheoryInternalButClearsPracticalExternal()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            // Previous regular attempt in the same semester with marks to carry.
            ctx.ExamSchedules.Add(TestData.Schedule(25, 1, TestData.Regular, Past, null));
            ctx.ApplicationVouchers.Add(TestData.Voucher(9, 1, 25));
            var previousReg = TestData.ExamRegistration(7, 25, 9);
            previousReg.CollegeId = TestData.CollegeId;
            ctx.ExamRegistrations.Add(previousReg);

            var previousResult = TestData.Result(10, 7, 302, TestData.Regular, "C", 25);
            previousResult.ObtainedMarksPractical = 50f;
            previousResult.ObtainedMarksPracticalInternal = 45f;
            previousResult.ObtainedMarksTheoryInternal = 40f;
            ctx.ExamSubjectResults!.Add(previousResult);

            var offering = TestData.Offering(302, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);

            // Make the rejected form supplementary so carry-forward kicks in.
            ctx.ExamRegistrations.Local.Single(e => e.Id == 1).IsSupplementary = true;
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 302 },
            new Dictionary<int, ReExamLegs> { [302] = ReExamLegs.Theory | ReExamLegs.Practical });

        Assert.True(success, message);

        var row = db.Context.ExamSubjectResults!
            .Single(r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 302 && r.IsActive);
        Assert.True(row.IsTheoryRegistered);
        Assert.True(row.IsPracticalRegistered);
        // Practical external is re-sat → cleared; internals carry forward.
        Assert.Null(row.ObtainedMarksPractical);
        Assert.Equal(45f, row.ObtainedMarksPracticalInternal);
        Assert.Equal(40f, row.ObtainedMarksTheoryInternal);
    }

    [Fact]
    public async Task CreatePaymentRequestLogWithSubjectsAsync_WritesTokensAndCountsOnlyPracticalLegs()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Future, null));
            ctx.Set<PaymentType>().Add(new PaymentType { Id = 1, PaymentTypeName = "Online", IsActive = true });
        });
        var service = CreateService(db);

        var logId = await service.CreatePaymentRequestLogWithSubjectsAsync(
            21, 1, 1500, "Online", "INV-LEGS",
            new Dictionary<int, ReExamLegs>
            {
                [301] = ReExamLegs.Theory,
                [302] = ReExamLegs.Theory | ReExamLegs.Practical
            });

        var log = await db.Context.PaymentRequestLogs!.SingleAsync(l => l.Id == logId);
        Assert.Equal("301:T,302:TP", log.SelectedSubjectIds);

        var practicalCount = await db.Context.Set<PaymentPracticalSubjects>()
            .Where(pps => pps.PaymentRequestLogId == logId)
            .Select(pps => (int?)pps.PracticalSubjectsCount)
            .SingleOrDefaultAsync();
        Assert.Equal(1, practicalCount);
    }

    [Fact]
    public async Task GetFailedSubjectOptionsForStudentAsync_PerPartGrades_ResolvesFailedLegsOnly()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));

            // Passed theory, failed practical on the previous attempt.
            var offering = TestData.Offering(301, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);

            ctx.ExamSchedules.Add(TestData.Schedule(25, 1, TestData.Regular, Past, null));
            ctx.ExamSchedules.Add(TestData.Schedule(24, 1, TestData.Partial, Future, null));
            ctx.ApplicationVouchers.Add(TestData.Voucher(5, 1, 25));
            var previousReg = TestData.ExamRegistration(7, 25, 5);
            ctx.ExamRegistrations.Add(previousReg);

            var result = TestData.Result(10, 7, 301, TestData.Regular, null, 25);
            result.GradeLetterTheory = "A";
            result.GradeLetterPractical = "F";
            ctx.ExamSubjectResults!.Add(result);
        });
        var service = CreateService(db);

        // Schedule 24 is the same program/semester partial exam.
        var options = await service.GetFailedSubjectOptionsForStudentAsync(24, UserId);

        var option = Assert.Single(options);
        Assert.Equal(301, option.SubjectOfferingId);
        Assert.Equal(ReExamLegs.Practical, option.FailedLegs);
    }

    private static ExamSchedule FeeSchedule(int id = 21)
    {
        var schedule = TestData.Schedule(id, 1, TestData.Regular, Future, null);
        schedule.ExamFee = 1000;
        schedule.PracticalSubjectFee = 1500;
        return schedule;
    }

    [Fact]
    public async Task ComputeSelectionFeeAsync_FlatExamFeePlusPerPracticalLeg()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(FeeSchedule());
        });
        var service = CreateService(db);

        var total = await service.ComputeSelectionFeeAsync(21, new Dictionary<int, ReExamLegs>
        {
            [301] = ReExamLegs.Theory,
            [302] = ReExamLegs.Theory | ReExamLegs.Practical,
            [303] = ReExamLegs.Practical
        });

        // Flat exam fee + one practical charge per ticked practical leg.
        Assert.Equal(1000m + 2 * 1500m, total);
    }

    [Fact]
    public async Task ComputeSelectionFeeAsync_EmptySelection_ReturnsFlatExamFee()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(FeeSchedule());
        });
        var service = CreateService(db);

        var total = await service.ComputeSelectionFeeAsync(21, new Dictionary<int, ReExamLegs>());

        Assert.Equal(1000m, total);
    }

    [Fact]
    public async Task ComputeSelectionFeeAsync_LegacyPlainIdToken_ChargesAvailablePracticalLeg()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(FeeSchedule());
            var offering = TestData.Offering(401, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);
        });
        var service = CreateService(db);

        // "401" without a leg suffix is a legacy token meaning both available
        // papers were registered, so its practical leg is charged too.
        var total = await service.ComputeSelectionFeeAsync(21, new Dictionary<int, ReExamLegs>
        {
            [401] = ReExamLegs.None
        });

        Assert.Equal(1000m + 1500m, total);
    }

    [Fact]
    public async Task ComputeSelectionFeeAsync_MixedExplicitAndLegacyTokens_OnlyAvailablePracticalsCharged()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(FeeSchedule());
            // Theory-only subject: its legacy token contributes no practical fee.
            ctx.SubjectOfferings.Add(TestData.Offering(402, 1, TestData.ProgramId));
        });
        var service = CreateService(db);

        var total = await service.ComputeSelectionFeeAsync(21, new Dictionary<int, ReExamLegs>
        {
            [302] = ReExamLegs.Theory | ReExamLegs.Practical,
            [402] = ReExamLegs.None
        });

        Assert.Equal(1000m + 1500m, total);
    }

    [Fact]
    public async Task ComputeSelectionFeeAsync_AddsExtendedDateCharge_WhenAppliedAfterEndDate()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var schedule = FeeSchedule();
            schedule.EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));
            schedule.ExtendedDate = DateTime.Today.AddDays(3);
            schedule.ExtendedDateCharge = 1000;
            ctx.ExamSchedules.Add(schedule);
        });
        var service = CreateService(db);

        var total = await service.ComputeSelectionFeeAsync(21, new Dictionary<int, ReExamLegs>
        {
            [301] = ReExamLegs.Theory,
            [302] = ReExamLegs.Theory | ReExamLegs.Practical,
            [303] = ReExamLegs.Practical
        });

        // Flat fee + practical legs + extended date charge (today > EndDate, today <= ExtendedDate).
        Assert.Equal(1000m + 2 * 1500m + 1000m, total);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_LegChangeOnExistingRow_PatchesFlagsAndClearsNewPracticalExternal()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);

            // The kept subject gains a practical paper and the row originally
            // registered theory only with a carried external practical.
            ctx.SubjectOfferings.Local.Single(o => o.Id == 101).HasPractical = true;
            var existingRow = ctx.ExamSubjectResults!.Local.Single(r => r.SubjectOfferingId == 101);
            existingRow.IsTheoryRegistered = true;
            existingRow.IsPracticalRegistered = false;
            existingRow.ObtainedMarksPractical = 55f;
            existingRow.ObtainedMarksTheoryInternal = 40f;
            existingRow.ObtainedMarksPracticalInternal = 45f;
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 101 },
            new Dictionary<int, ReExamLegs> { [101] = ReExamLegs.Theory | ReExamLegs.Practical });

        Assert.True(success, message);

        var row = db.Context.ExamSubjectResults!
            .Single(r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 101 && r.IsActive);
        Assert.True(row.IsTheoryRegistered);
        Assert.True(row.IsPracticalRegistered);
        // Newly re-sat practical external is cleared; internals carry forward.
        Assert.Null(row.ObtainedMarksPractical);
        Assert.Equal(40f, row.ObtainedMarksTheoryInternal);
        Assert.Equal(45f, row.ObtainedMarksPracticalInternal);

        var log = db.Context.PaymentRequestLogs!
            .Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("101:TP", log.SelectedSubjectIds);
    }

    [Fact]
    public async Task ReapplyExamRegistrationAsync_RefreshesPracticalCountOnOriginalLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);
            ctx.SaveChanges();
            var paidLogId = ctx.PaymentRequestLogs!.Local.Single().Id;
            ctx.Set<PaymentPracticalSubjects>().Add(new PaymentPracticalSubjects
            {
                PaymentRequestLogId = paidLogId,
                PracticalSubjectsCount = 0,
                TotalAmount = 1000
            });

            var offering = TestData.Offering(302, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);
        });
        var service = CreateService(db);

        var (success, message) = await service.ReapplyExamRegistrationAsync(
            21, UserId, 1, new List<int> { 101, 302 },
            new Dictionary<int, ReExamLegs>
            {
                [101] = ReExamLegs.Theory,
                [302] = ReExamLegs.Practical
            });

        Assert.True(success, message);

        var paidLogId = db.Context.PaymentRequestLogs!
            .Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1).Id;
        Assert.Equal("101:T,302:P",
            db.Context.PaymentRequestLogs!.Single(l => l.Id == paidLogId).SelectedSubjectIds);

        var count = db.Context.Set<PaymentPracticalSubjects>()
            .Where(pps => pps.PaymentRequestLogId == paidLogId)
            .Select(pps => (int?)pps.PracticalSubjectsCount)
            .SingleOrDefault();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TryCompleteApplyAgainTopUpAsync_SecondConfirmedLog_ReappliesWithItsTokens()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);

            ctx.Set<PaymentType>().Add(new PaymentType { Id = 2, PaymentTypeName = "Online Extra", IsActive = true });
            ctx.PaymentRequestLogs.Add(new PaymentRequestLog
            {
                TenantId = TestData.TenantId,
                PaymentRequestLogStatus = 1,
                InvoiceNumber = "INV-2",
                ForwardedTimestamp = DateTime.UtcNow,
                FullName = "Test Student",
                Amount = 500,
                FullRequestContent = "{}",
                PaymentTypeId = 2,
                StudentRegistrationId = 1,
                ExamScheduleId = 21,
                SelectedSubjectIds = "302:P"
            });

            var offering = TestData.Offering(302, 1, TestData.ProgramId);
            offering.HasPractical = true;
            ctx.SubjectOfferings.Add(offering);
        });
        var topUpLogId = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-2").Id;
        var service = CreateService(db);

        var handled = await service.TryCompleteApplyAgainTopUpAsync(topUpLogId, UserId);

        Assert.True(handled);
        Assert.Equal(RegistrationStatus.Pending,
            db.Context.ExamRegistrations!.Single(e => e.Id == 1).Status);

        var row = Assert.Single(db.Context.ExamSubjectResults!.Where(r => r.ExamRegistrationId == 1 && r.IsActive));
        Assert.Equal(302, row.SubjectOfferingId);
        Assert.False(row.IsTheoryRegistered);
        Assert.True(row.IsPracticalRegistered);
    }

    [Fact]
    public async Task TryCompleteApplyAgainTopUpAsync_NoPriorConfirmedPayment_ReturnsFalse()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx, withPayment: false);
            ctx.Set<PaymentType>().Add(new PaymentType { Id = 1, PaymentTypeName = "Online", IsActive = true });
            ctx.PaymentRequestLogs.Add(new PaymentRequestLog
            {
                TenantId = TestData.TenantId,
                PaymentRequestLogStatus = 1,
                InvoiceNumber = "INV-ONLY",
                ForwardedTimestamp = DateTime.UtcNow,
                FullName = "Test Student",
                Amount = 1000,
                FullRequestContent = "{}",
                PaymentTypeId = 1,
                StudentRegistrationId = 1,
                ExamScheduleId = 21,
                SelectedSubjectIds = "101"
            });
        });
        var service = CreateService(db);

        var logId = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-ONLY").Id;
        var handled = await service.TryCompleteApplyAgainTopUpAsync(logId, UserId);

        Assert.False(handled);
        Assert.Equal(RegistrationStatus.Rejected,
            db.Context.ExamRegistrations!.Single(e => e.Id == 1).Status);
        Assert.False(await service.HasOpenApplyAgainPaymentAsync(21, 1));
    }

    [Fact]
    public async Task SupersedeOpenApplyAgainPaymentsAsync_MarksOlderOpenLogsButKeepsConfirmedOnes()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedRejectedForm(ctx);

            ctx.PaymentRequestLogs.Add(new PaymentRequestLog
            {
                TenantId = TestData.TenantId,
                InvoiceNumber = "INV-STALE",
                ForwardedTimestamp = DateTime.UtcNow,
                FullName = "Test Student",
                Amount = 500,
                FullRequestContent = "{}",
                PaymentTypeId = 1,
                StudentRegistrationId = 1,
                ExamScheduleId = 21,
                SelectedSubjectIds = "301:P"
            });

            ctx.PaymentRequestLogs.Add(new PaymentRequestLog
            {
                TenantId = TestData.TenantId,
                InvoiceNumber = "INV-CURRENT",
                ForwardedTimestamp = DateTime.UtcNow,
                FullName = "Test Student",
                Amount = 500,
                FullRequestContent = "{}",
                PaymentTypeId = 1,
                StudentRegistrationId = 1,
                ExamScheduleId = 21,
                SelectedSubjectIds = "302:P"
            });
        });
        var paidLogId = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-1").Id;
        var staleLogId = db.Context.PaymentRequestLogs.Single(l => l.InvoiceNumber == "INV-STALE").Id;
        var currentLogId = db.Context.PaymentRequestLogs.Single(l => l.InvoiceNumber == "INV-CURRENT").Id;
        var service = CreateService(db);

        Assert.True(await service.HasOpenApplyAgainPaymentAsync(21, 1));

        await service.SupersedeOpenApplyAgainPaymentsAsync(21, 1, exceptLogId: currentLogId);

        Assert.Equal(2, db.Context.PaymentRequestLogs!.Single(l => l.Id == staleLogId).PaymentRequestLogStatus);
        Assert.Equal(1, db.Context.PaymentRequestLogs.Single(l => l.Id == paidLogId).PaymentRequestLogStatus);
        Assert.NotEqual(2, db.Context.PaymentRequestLogs.Single(l => l.Id == currentLogId).PaymentRequestLogStatus);
        Assert.True(await service.HasOpenApplyAgainPaymentAsync(21, 1));

        // Passing an unmatched id supersedes every remaining open log.
        await service.SupersedeOpenApplyAgainPaymentsAsync(21, 1, exceptLogId: 0);

        Assert.False(await service.HasOpenApplyAgainPaymentAsync(21, 1));
    }
}
