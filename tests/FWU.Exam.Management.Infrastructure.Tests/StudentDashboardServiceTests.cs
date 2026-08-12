using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
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
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
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

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));       // regular sem1 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Regular, Past, null));       // regular sem2 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(13, 3, TestData.Regular, Past, null));       // regular sem3 (not enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(14, 2, TestData.Supplementary, Past, null)); // supplementary sem2 (enrolled)
            ctx.ExamSchedules.Add(TestData.Schedule(15, 2, TestData.Entrance, Past, null));      // entrance (excluded)
            ctx.ExamSchedules.Add(TestData.Schedule(16, 2, TestData.Regular, Past, null, TestData.ProgramIdOther)); // other program

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 12));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 12, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 102, TestData.Regular, "F", 12)); // failed in sem2 => supplementary shown
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

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
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
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
            ctx.Users.Add(TestData.User(UserId, Email));
            var sr = TestData.StudentRegistration(1, Email);
            sr.StudentAdmissionId = 1;
            ctx.StudentRegistrations.Add(sr);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
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
            ctx.Faculties.Add(new Faculty { Id = 5, Name = "Engineering", OfficeCode = "L091", TenantId = 2 });
            ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = 2, CollegeId = TestData.CollegeId, FacultyId = 5 });

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

        Assert.Equal(new[] { "Email Address", "Phone Number", "Province", "District", "Local Level", "Gender", "Ethnicity", "Profile Photo", "Student Signature" }, missing);
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
}
