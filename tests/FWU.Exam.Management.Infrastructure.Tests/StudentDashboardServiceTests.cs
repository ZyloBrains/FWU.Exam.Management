using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
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

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsEmpty_WhenStudentHasNoAdmission()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);
        var student = TestData.StudentRegistration(1, Email);

        var result = await service.GetExamSchedulesForStudentAsync(student);

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
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
        });

        var student = db.Context.StudentRegistrations!.FirstOrDefault() ?? TestData.StudentRegistration(1, Email);
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student);

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

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));       // regular sem1 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Regular, Past, null));       // regular sem2 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(13, 3, TestData.Regular, Past, null));       // regular sem3 (not enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(14, 2, TestData.Supplementary, Past, null)); // supplementary sem2 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(15, 2, TestData.Entrance, Past, null));      // entrance (excluded)
            ctx.ExamSchedules.Add(TestData.Schedule(16, 2, TestData.Regular, Past, null, TestData.ProgramIdOther)); // other program
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, s => s.Id == 11);
        Assert.Contains(result, s => s.Id == 12);
        Assert.Contains(result, s => s.Id == 14);
        Assert.DoesNotContain(result, s => s.Id == 13);
        Assert.DoesNotContain(result, s => s.Id == 15);
        Assert.DoesNotContain(result, s => s.Id == 16);
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
    public async Task GetSubjectOfferingsForScheduleAsync_ReturnsAllSemesterOfferings()
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
    public async Task GetExamSchedulesForStudentAsync_HidesSchedule_WhenCollegeHasNotApproved()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
            ctx.ExamScheduleCollegeApprovals.Add(new ExamScheduleCollegeApproval
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                CollegeId = TestData.CollegeId,
                Status = ExamScheduleApprovalStatus.Pending,
                IsActive = true
            });
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSchedule_WhenCollegeApproved()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
            ctx.ExamScheduleCollegeApprovals.Add(new ExamScheduleCollegeApproval
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                CollegeId = TestData.CollegeId,
                Status = ExamScheduleApprovalStatus.Approved,
                ApprovedDate = DateTime.UtcNow,
                IsActive = true
            });
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSchedule_WhenNoApprovalRequested()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
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

            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, null));

            ctx.TenantColleges.Add(new TenantCollege { TenantId = 2, CollegeId = TestData.CollegeId });

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
        Assert.NotNull(rr.ExamSchedule.Semester);
        Assert.Equal(1, rr.ExamSchedule.Semester!.Year);
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
            ctx.TenantColleges.Add(new TenantCollege { TenantId = 2, CollegeId = TestData.CollegeId });

            ctx.SubjectCatalogs.Add(new SubjectCatalog
            {
                Id = 7,
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
}
