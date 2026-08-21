using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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

        ctx.SubjectCatalogs.Add(new SubjectCatalog { Id = 2, TenantId = TestData.TenantId, SubjectCode = "SUB2", SubjectName = "Subject 2", SubjectTypeId = 1, IsActive = true });
        ctx.SubjectOfferings.Add(new SubjectOffering
        {
            Id = 301,
            TenantId = TestData.TenantId,
            SubjectCatalogId = 2,
            ProgramId = TestData.ProgramId,
            SemesterId = 1,
            IsCompulsory = true,
            DisplayOrder = 2,
            HasTheory = true,
            HasPractical = false,
            HasInternal = true,
            TheoryFullMarks = 100,
            TheoryPassMarks = 40
        });

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

    private static void SeedFormsWithUserDocuments(AppDbContext ctx)
    {
        SeedForms(ctx);

        // Students have UserName = RegistrationNumber and a personal Email.
        var studentUser = TestData.User("stu-user-1", "student1@test.com");
        studentUser.UserName = "REG1";
        studentUser.ProfilePath = "/uploads/photos/student1.jpg";
        studentUser.SignaturePath = "/uploads/signatures/student1.jpg";
        ctx.Users.Add(studentUser);
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

    private static void SeedFormsWithoutPaymentLog(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);
        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = TestData.TenantId });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId, FacultyId = 1 });

        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu1@test.com"));
        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));

        var form = TestData.ExamRegistration(1, 21, 1);
        form.CollegeId = TestData.CollegeId;

        ctx.ExamRegistrations.Add(form);
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
        Assert.False(pending.CanAdminApprove);
        Assert.Equal(RegistrationStatus.Pending, pending.Status);

        var approved = Assert.Single(result.Forms, f => f.ExamRegistrationId == 3);
        Assert.False(approved.CanApprove);
        Assert.True(approved.CanAdminApprove);
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
        Assert.Equal(101, subject.SubjectOfferingId);
        Assert.DoesNotContain(detail.Subjects, s => s.Code == "SUB2");
        Assert.True(detail.CanEditSubjects);

        var outOfScope = await service.GetStudentExamFormDetailAsync(2);
        Assert.Null(outOfScope);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_ShowsOnlySelectedSubjects_FromConfirmedPaymentLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var detail = await service.GetStudentExamFormDetailAsync(1);

        Assert.NotNull(detail);
        var codes = detail!.Subjects.Select(s => s.Code).OrderBy(c => c).ToList();
        Assert.Equal(new[] { "SUB1" }, codes);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_ShowsNoSubjects_WhenNoConfirmedPaymentLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedFormsWithoutPaymentLog);
        var service = CreateService(db);

        var detail = await service.GetStudentExamFormDetailAsync(1);

        Assert.NotNull(detail);
        Assert.Empty(detail!.Subjects);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_ResolvesPhotoAndSignature_WhenUserNameMatchesRegistrationNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedFormsWithUserDocuments);
        var service = CreateService(db);

        var detail = await service.GetStudentExamFormDetailAsync(1);

        Assert.NotNull(detail);
        Assert.Equal("/uploads/photos/student1.jpg", detail!.PhotoPath);
        Assert.Equal("/uploads/signatures/student1.jpg", detail.SignaturePath);
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

    [Fact]
    public async Task GetEditableSubjectsAsync_ListsAllOfferings_WithCurrentSelection()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var model = await service.GetEditableSubjectsAsync(1);

        Assert.NotNull(model);
        Assert.True(model!.CanEdit);
        Assert.Null(model.NotEditableReason);
        Assert.Equal(2, model.AvailableSubjects.Count);

        var selected = Assert.Single(model.AvailableSubjects, s => s.IsSelected);
        Assert.Equal(101, selected.SubjectOfferingId);
        Assert.Equal("SUB1", selected.Code);

        var unselected = Assert.Single(model.AvailableSubjects, s => !s.IsSelected);
        Assert.Equal(301, unselected.SubjectOfferingId);

        var outOfScope = await service.GetEditableSubjectsAsync(2);
        Assert.Null(outOfScope);
    }

    [Fact]
    public async Task GetEditableSubjectsAsync_NotEditable_AfterFinalApproval()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var reg = await db.Context.ExamRegistrations!.SingleAsync(r => r.Id == 1);
        reg.Status = RegistrationStatus.AdminVerified;
        await db.Context.SaveChangesAsync();

        var model = await service.GetEditableSubjectsAsync(1);

        Assert.NotNull(model);
        Assert.False(model!.CanEdit);
        Assert.Contains("final approval", model.NotEditableReason);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_SwapsSubjects_UpdatesPaymentLogAndResultRows()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedFormsWithExistingResult);
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(1, [301]);

        Assert.True(success, message);

        var log = db.Context.PaymentRequestLogs!.Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("301", log.SelectedSubjectIds);

        var removed = db.Context.ExamSubjectResults!.Single(r => r.Id == 1);
        Assert.False(removed.IsActive);

        var added = db.Context.ExamSubjectResults!.Single(r => r.SubjectOfferingId == 301 && r.ExamRegistrationId == 1);
        Assert.True(added.IsActive);
        Assert.Equal(21, added.ExamScheduleId);
        Assert.Equal(TestData.Regular, added.ExamTypeId);
        Assert.False(added.IsSupplementary);
        Assert.True(added.IsTheoryRegistered);
        Assert.False(added.IsPracticalRegistered);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_CarriesForwardMarks_ForPartialExamAdditions()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialForm);
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(6, [101, 301]);

        Assert.True(success, message);

        var log = db.Context.PaymentRequestLogs!.Single(l => l.ExamScheduleId == 24 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("101,301", log.SelectedSubjectIds);

        var carried = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.SubjectOfferingId == 101);
        Assert.True(carried.IsSupplementary);
        Assert.Equal(50f, carried.ObtainedMarksPractical);
        Assert.Equal(40f, carried.ObtainedMarksTheoryInternal);
        Assert.Equal(45f, carried.ObtainedMarksPracticalInternal);

        var fresh = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.SubjectOfferingId == 301);
        Assert.True(fresh.IsSupplementary);
        Assert.Null(fresh.ObtainedMarksPractical);
        Assert.Null(fresh.ObtainedMarksTheoryInternal);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_RejectsOtherCollegeForm()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(2, [301]);

        Assert.False(success);
        Assert.Equal("Exam form not found.", message);
        Assert.Empty(db.Context.ExamSubjectResults!);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_RejectsOfferingFromAnotherProgramOrSemester()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        // Offering 202 belongs to program 2 / semester 2; offering 102 to semester 2.
        var (success, _) = await service.UpdateRegistrationSubjectsAsync(1, [202]);
        Assert.False(success);

        (success, _) = await service.UpdateRegistrationSubjectsAsync(1, [102]);
        Assert.False(success);

        (success, _) = await service.UpdateRegistrationSubjectsAsync(1, []);
        Assert.False(success);

        Assert.Empty(db.Context.ExamSubjectResults!);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_Blocked_WhenAdmitCardExistsOrNoPaymentLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedFormsWithExistingResult);
        var service = CreateService(db);

        db.Context.AdmitCards!.Add(new AdmitCard
        {
            TenantId = TestData.TenantId,
            ExamRegistrationId = 1,
            ExamScheduleId = 21,
            AdmitCardNumber = "AC-1",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await db.Context.SaveChangesAsync();

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(1, [301]);
        Assert.False(success);
        Assert.Contains("admit card", message);

        // Registration 3 shares voucher 1; point it at voucher 2 (student 2) which has no confirmed payment log.
        var reg3 = await db.Context.ExamRegistrations!.SingleAsync(r => r.Id == 3);
        reg3.ApplicationVoucherId = 2;
        await db.Context.SaveChangesAsync();

        var (noPaymentSuccess, noPaymentMessage) = await service.UpdateRegistrationSubjectsAsync(3, [301]);
        Assert.False(noPaymentSuccess);
        Assert.Contains("Payment", noPaymentMessage);
    }

    private static void SeedFormsWithExistingResult(AppDbContext ctx)
    {
        SeedForms(ctx);

        ctx.ExamSubjectResults!.Add(TestData.Result(1, 1, 101, TestData.Regular, "D", 21));
    }

    private static void SeedPartialForm(AppDbContext ctx)
    {
        SeedForms(ctx);

        // Previous regular attempt for the same semester (schedule 25).
        ctx.ExamSchedules.Add(TestData.Schedule(25, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-5)), DateTime.UtcNow.AddMonths(-4)));
        ctx.ApplicationVouchers.Add(TestData.Voucher(5, 1, 25));

        var previousReg = TestData.ExamRegistration(7, 25, 5);
        previousReg.CollegeId = TestData.CollegeId;
        ctx.ExamRegistrations.Add(previousReg);

        var previousResult = TestData.Result(10, 7, 101, TestData.Regular, "C", 25);
        previousResult.ObtainedMarksPractical = 50;
        previousResult.ObtainedMarksTheoryInternal = 40;
        previousResult.ObtainedMarksPracticalInternal = 45;
        ctx.ExamSubjectResults!.Add(previousResult);

        // Partial re-exam schedule (id 24) with a paid form for the same student.
        ctx.ExamSchedules.Add(TestData.Schedule(24, 1, TestData.Partial,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));
        ctx.ApplicationVouchers.Add(TestData.Voucher(4, 1, 24));

        var partialReg = TestData.ExamRegistration(6, 24, 4);
        partialReg.CollegeId = TestData.CollegeId;
        partialReg.IsSupplementary = true;
        ctx.ExamRegistrations.Add(partialReg);

        ctx.PaymentRequestLogs.Add(new PaymentRequestLog
        {
            TenantId = TestData.TenantId,
            PaymentRequestLogStatus = 1,
            InvoiceNumber = "INV-PARTIAL",
            ForwardedTimestamp = DateTime.UtcNow,
            FullName = "Test Student",
            Amount = 500,
            FullRequestContent = "{}",
            PaymentTypeId = 1,
            StudentRegistrationId = 1,
            ExamScheduleId = 24,
            SelectedSubjectIds = "101"
        });
    }
}
