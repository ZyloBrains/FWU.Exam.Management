using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamRegistrationServiceTests
{
    private const string AdminUserId = "admin-1";
    private const string AdminEmail = "admin@test.com";

    private static TestUserContext CollegeAdmin() =>
        new TestUserContext().WithUser(AdminUserId, null, TestData.CollegeId, [], [Role.CollegeAdmin]);

    private static void SeedForms(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);
        ctx.Users.Add(TestData.User(AdminUserId, AdminEmail));

        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = TestData.TenantId });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId, FacultyId = 1 });

        ctx.Colleges.Add(new College
        {
            Id = 2,
            Code = "CLG2",
            Name = "Second College",
            Email = "c2@c.com",
            PrincipalName = "Principal",
            PrincipalContactNumber = "000",
            CollegeTypeId = 1,
            IsActive = true
        });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = 2, FacultyId = 1 });

        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu1@test.com"));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(2, "stu2@test.com"));
        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
        ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, 21));

        var pending = TestData.ExamRegistration(1, 21, 1);
        pending.CollegeId = TestData.CollegeId;

        var otherCollegePending = TestData.ExamRegistration(2, 21, 2);
        otherCollegePending.CollegeId = 2;

        var approved = TestData.ExamRegistration(3, 21, 1);
        approved.CollegeId = TestData.CollegeId;
        approved.Status = RegistrationStatus.CollegeVerified;

        var notStudentApplied = TestData.ExamRegistration(4, 21, 1);
        notStudentApplied.CollegeId = TestData.CollegeId;
        notStudentApplied.IsAppliedByStudent = false;

        ctx.ExamRegistrations.AddRange(pending, otherCollegePending, approved, notStudentApplied);
    }

    private static void SeedMasterLevelForm(AppDbContext ctx)
    {
        SeedForms(ctx);

        ctx.Levels.Add(new Level { Id = 2, LevelCode = "M", LevelName = "Master", IsActive = true });
        ctx.Programs.Add(new Program
        {
            Id = 3,
            LevelId = 2,
            ProgramCode = "MCA",
            ProgramName = "Master in Computer Application",
            ShortName = "MCA",
            Duration = 2,
            IsActive = true
        });

        ctx.ExamSchedules.Add(TestData.Schedule(22, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2), programId: 3));

        ctx.StudentRegistrations.Add(TestData.StudentRegistration(3, "stu3@test.com"));
        ctx.ApplicationVouchers.Add(TestData.Voucher(3, 3, 22));

        var masterForm = TestData.ExamRegistration(5, 22, 3);
        masterForm.CollegeId = TestData.CollegeId;
        masterForm.ProgramsId = 3;

        ctx.ExamRegistrations.Add(masterForm);
    }

    private static ExamRegistrationService CreateService(TestDb db) =>
        new(db.Context, CollegeAdmin());

    [Fact]
    public async Task GetStudentExamFormsAsync_CollegeAdmin_SeesOwnCollegeOnly_AndCountsPendingPerSchedule()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var result = await service.GetStudentExamFormsAsync(null, null, null, null, 1, 50);

        Assert.Equal(2, result.TotalCount);
        Assert.DoesNotContain(result.Forms, f => f.ExamRegistrationId == 2);
        Assert.DoesNotContain(result.Forms, f => f.ExamRegistrationId == 4);

        var pending = Assert.Single(result.Forms, f => f.ExamRegistrationId == 1);
        Assert.True(pending.CanApprove);
        Assert.Equal(RegistrationStatus.Pending, pending.Status);

        var approved = Assert.Single(result.Forms, f => f.ExamRegistrationId == 3);
        Assert.False(approved.CanApprove);
        Assert.Equal(RegistrationStatus.CollegeVerified, approved.Status);

        Assert.Equal(1, result.PendingApprovalCount);
        var bySchedule = Assert.Single(result.PendingBySchedule);
        Assert.Equal("Schedule 21", bySchedule.ScheduleName);
        Assert.Equal(1, bySchedule.PendingCount);
    }

    [Fact]
    public async Task VerifyExamRegistrationAsync_SetsCollegeVerifiedAndApprovalTrail()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        await service.VerifyExamRegistrationAsync(1);

        var reg = db.Context.ExamRegistrations!.Single(r => r.Id == 1);
        Assert.Equal(RegistrationStatus.CollegeVerified, reg.Status);
        Assert.Equal(AdminEmail, reg.VerifiedByUsername);
        Assert.NotNull(reg.VerifiedDate);
    }

    [Fact]
    public async Task VerifyExamRegistrationAsync_DoesNothing_WhenStatusIsNotPending()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        await service.VerifyExamRegistrationAsync(3);

        var reg = db.Context.ExamRegistrations!.Single(r => r.Id == 3);
        Assert.Equal(RegistrationStatus.CollegeVerified, reg.Status);
        Assert.Null(reg.VerifiedByUsername);
        Assert.Null(reg.VerifiedDate);
    }

    [Fact]
    public async Task ApproveExamRegistrationAsync_SetsAdminVerifiedAndApprovalTrail()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        await service.ApproveExamRegistrationAsync(3);

        var reg = db.Context.ExamRegistrations!.Single(r => r.Id == 3);
        Assert.Equal(RegistrationStatus.AdminVerified, reg.Status);
        Assert.Equal(AdminEmail, reg.AdminVerifiedByUsername);
        Assert.NotNull(reg.AdminVerifiedDate);
    }

    [Fact]
    public async Task GetStudentExamFormsAsync_FiltersByAcademicYearAndLevel()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMasterLevelForm);
        var service = CreateService(db);

        var masterOnly = await service.GetStudentExamFormsAsync(TestData.AcademicYearId, 2, null, null, 1, 50);
        Assert.Equal(1, masterOnly.TotalCount);
        Assert.Single(masterOnly.Forms, f => f.ExamRegistrationId == 5);

        var bachelorOnly = await service.GetStudentExamFormsAsync(TestData.AcademicYearId, 1, null, null, 1, 50);
        Assert.Equal(2, bachelorOnly.TotalCount);
        Assert.DoesNotContain(bachelorOnly.Forms, f => f.ExamRegistrationId == 5);

        var bySchedule = await service.GetStudentExamFormsAsync(TestData.AcademicYearId, 1, 21, null, 1, 50);
        Assert.Equal(2, bySchedule.TotalCount);

        var noMatch = await service.GetStudentExamFormsAsync(TestData.AcademicYearId, 99, null, null, 1, 50);
        Assert.Equal(0, noMatch.TotalCount);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_ReturnsEnrichedDetail_AndNullsOutOfScope()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var detail = await service.GetStudentExamFormDetailAsync(1);
        Assert.NotNull(detail);
        Assert.Equal("Test Student", detail!.StudentName);
        Assert.Equal("REG1", detail.RegistrationNumber);
        Assert.Equal("Test College", detail.CollegeName);
        Assert.Equal("Bachelor in Computer Application", detail.ProgramName);
        Assert.True(detail.CanApprove);
        Assert.Equal(RegistrationStatus.Pending, detail.Status);

        var subject = Assert.Single(detail.Subjects);
        Assert.Equal("SUB1", subject.Code);
        Assert.Equal("Subject 1", subject.Name);
        Assert.True(subject.Theory);
        Assert.False(subject.Practical);

        var outOfScope = await service.GetStudentExamFormDetailAsync(2);
        Assert.Null(outOfScope);
    }

    [Fact]
    public async Task RejectExamRegistrationAsync_SetsRejectedWithRemark_OnlyWhenPending()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        await service.RejectExamRegistrationAsync(1, "Incomplete documents");
        var rejected = db.Context.ExamRegistrations!.Single(r => r.Id == 1);
        Assert.Equal(RegistrationStatus.Rejected, rejected.Status);
        Assert.Equal("Incomplete documents", rejected.Remarks);

        await service.RejectExamRegistrationAsync(3, "Late");
        var approved = db.Context.ExamRegistrations!.Single(r => r.Id == 3);
        Assert.Equal(RegistrationStatus.CollegeVerified, approved.Status);
        Assert.Null(approved.Remarks);
    }

    [Fact]
    public async Task GetFilterOptions_ReturnDistinctYearsLevelsAndSchedules()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMasterLevelForm);
        var service = CreateService(db);

        var years = await service.GetFilterAcademicYearsAsync();
        Assert.Contains(years, y => y.Id == TestData.AcademicYearId);

        var levels = await service.GetFilterLevelsAsync(TestData.AcademicYearId);
        Assert.Equal(2, levels.Count);
        Assert.Contains(levels, l => l.Name == "Bachelor");
        Assert.Contains(levels, l => l.Name == "Master");

        var schedules = await service.GetFilterExamSchedulesAsync(TestData.AcademicYearId, 1);
        Assert.Single(schedules);
        Assert.Equal(21, schedules[0].Id);
    }
}
