using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
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
        Assert.Equal("301:T", log.SelectedSubjectIds);

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
        // Both rows are new additions stamped with their offering's papers
        // (theory-only in this seed).
        Assert.Equal("101:T,301:T", log.SelectedSubjectIds);

        var carried = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.SubjectOfferingId == 101);
        Assert.True(carried.IsSupplementary);
        // Offering 101 registers the theory leg only; its external was already
        // sat, so everything carries forward untouched.
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

    [Fact]
    public async Task RejectExamRegistrationAsync_RejectsPendingForm_AndRecordsReason()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var (success, message) = await service.RejectExamRegistrationAsync(1, "Invalid documents");

        Assert.True(success, message);
        var reg = db.Context.ExamRegistrations!.Single(r => r.Id == 1);
        Assert.Equal(RegistrationStatus.Rejected, reg.Status);
        Assert.Contains("Invalid documents", reg.Remarks);
        Assert.Contains("[Rejected by", reg.Remarks);
        Assert.Contains(AdminEmail, reg.Remarks);
    }

    [Fact]
    public async Task RejectExamRegistrationAsync_RejectsCollegeVerifiedForm()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var (success, message) = await service.RejectExamRegistrationAsync(3, "Wrong student details");

        Assert.True(success, message);
        Assert.Equal(RegistrationStatus.Rejected, db.Context.ExamRegistrations!.Single(r => r.Id == 3).Status);
    }

    [Fact]
    public async Task RejectExamRegistrationAsync_RequiresReason_AndBlocksFinalApprovedAndOtherCollege()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedForms);
        var service = CreateService(db);

        var (noReason, noReasonMessage) = await service.RejectExamRegistrationAsync(1, "   ");
        Assert.False(noReason);
        Assert.Contains("reason", noReasonMessage, StringComparison.OrdinalIgnoreCase);

        var reg1 = await db.Context.ExamRegistrations!.SingleAsync(r => r.Id == 1);
        reg1.Status = RegistrationStatus.AdminVerified;
        await db.Context.SaveChangesAsync();

        var (finalApproved, finalApprovedMessage) = await service.RejectExamRegistrationAsync(1, "test");
        Assert.False(finalApproved);
        Assert.Contains("pending or college-verified", finalApprovedMessage, StringComparison.OrdinalIgnoreCase);

        var (otherCollege, otherCollegeMessage) = await service.RejectExamRegistrationAsync(2, "test");
        Assert.False(otherCollege);
        Assert.Equal("Exam form not found.", otherCollegeMessage);

        // Nothing was changed by the failed attempts.
        Assert.Equal(RegistrationStatus.AdminVerified, reg1.Status);
        Assert.Equal(RegistrationStatus.Pending, db.Context.ExamRegistrations!.Single(r => r.Id == 2).Status);
    }

    [Fact]
    public async Task RejectExamRegistrationAsync_Blocked_WhenAdmitCardExists()
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

        var (success, message) = await service.RejectExamRegistrationAsync(1, "test");
        Assert.False(success);
        Assert.Contains("admit card", message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(RegistrationStatus.Pending, db.Context.ExamRegistrations!.Single(r => r.Id == 1).Status);
    }

    private static void SeedCrossCohortPartial(AppDbContext ctx, bool stampSupplementaryFlag = true)
    {
        TestData.SeedBase(ctx);
        TestData.SeedCollegeForStandardTenant(ctx);
        ctx.Users.Add(TestData.User(AdminUserId, AdminEmail));
        ctx.Set<PaymentType>().Add(new PaymentType { Id = 1, PaymentTypeName = "Online", IsActive = true });

        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = TestData.TenantId });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId, FacultyId = 1 });
        ctx.AcademicYears.Add(new AcademicYear { Id = 2, TenantId = TestData.TenantId, AcademicYearCode = "2082", AcademicYearName = "2026", AcademicYearNameNepali = "2082", IsActive = true });

        // Schedule instance belongs to the newer AY 2, but the student's batch is AY 1.
        ctx.SemesterInstances.Add(new SemesterInstance
        {
            Id = 20,
            TenantId = TestData.TenantId,
            AcademicYearId = 2,
            SemesterId = 1,
            ProgramId = TestData.ProgramId,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(5)
        });

        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 90,
            TenantId = TestData.TenantId,
            Name = "Batch 2025",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = 1,
            IsActive = true
        });
        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 91,
            TenantId = TestData.TenantId,
            Name = "New 2026",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = 2,
            IsActive = true
        });

        ctx.SubjectCatalogs.Add(new SubjectCatalog { Id = 2, TenantId = TestData.TenantId, SubjectCode = "SUB2", SubjectName = "Subject 2", SubjectTypeId = 1, IsActive = true });

        var oldOffering = TestData.Offering(301, 1, TestData.ProgramId);
        oldOffering.Id = 301;
        oldOffering.SubjectCatalogId = 2;
        oldOffering.CurriculumVersionId = 90;
        ctx.SubjectOfferings.Add(oldOffering);

        var newOffering = TestData.Offering(401, 1, TestData.ProgramId);
        newOffering.Id = 401;
        newOffering.SubjectCatalogId = 2;
        newOffering.CurriculumVersionId = 91;
        ctx.SubjectOfferings.Add(newOffering);

        ctx.ExamSchedules.Add(TestData.Schedule(30, 20, TestData.Partial,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));

        var sr = TestData.StudentRegistration(1, "stu1@test.com");
        sr.AcademicYearId = 1;
        ctx.StudentRegistrations.Add(sr);

        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 30));

        var er = TestData.ExamRegistration(1, 30, 1);
        er.CollegeId = TestData.CollegeId;
        er.IsSupplementary = stampSupplementaryFlag;
        ctx.ExamRegistrations.Add(er);

        ctx.PaymentRequestLogs.Add(new PaymentRequestLog
        {
            TenantId = TestData.TenantId,
            PaymentRequestLogStatus = 1,
            InvoiceNumber = "INV-CROSS",
            ForwardedTimestamp = DateTime.UtcNow,
            FullName = "Test Student",
            Amount = 800,
            FullRequestContent = "{}",
            PaymentTypeId = 1,
            StudentRegistrationId = 1,
            ExamScheduleId = 30,
            SelectedSubjectIds = "301",
            CollegeId = TestData.CollegeId
        });
    }

    [Fact]
    public async Task GetEditableSubjectsAsync_CrossCohortPartial_ShowsBatchVersionSubject()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), ctx => SeedCrossCohortPartial(ctx));
        var service = CreateService(db);

        var model = await service.GetEditableSubjectsAsync(1);

        Assert.NotNull(model);
        Assert.True(model!.CanEdit);
        var subject = Assert.Single(model.AvailableSubjects);
        Assert.Equal(301, subject.SubjectOfferingId);
        Assert.True(subject.IsSelected);
        Assert.DoesNotContain(model.AvailableSubjects, s => s.SubjectOfferingId == 401);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_CrossCohortPartial_ShowsPaidBatchVersionSubject()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), ctx => SeedCrossCohortPartial(ctx));
        var service = CreateService(db);

        var result = await service.GetStudentExamFormDetailAsync(1);

        Assert.NotNull(result);
        var subject = Assert.Single(result!.Subjects);
        Assert.Equal(301, subject.SubjectOfferingId);
    }

    [Fact]
    public async Task GetEditableSubjectsAsync_CrossCohortPartial_FallsBackToUnversioned_WhenBatchSubjectNotSelected()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), ctx => SeedCrossCohortPartial(ctx));
        db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-CROSS").SelectedSubjectIds = "101";
        db.Context.SaveChanges();
        var service = CreateService(db);

        var model = await service.GetEditableSubjectsAsync(1);

        Assert.NotNull(model);
        Assert.True(model!.CanEdit);
        var subject = Assert.Single(model.AvailableSubjects);
        Assert.Equal(101, subject.SubjectOfferingId);
        Assert.True(subject.IsSelected);
    }

    [Fact]
    public async Task GetStudentExamFormDetailAsync_CrossCohortPartial_LegacyRowWithoutFlag_ShowsPaidSubjectAndBatchYear()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId),
            ctx => SeedCrossCohortPartial(ctx, stampSupplementaryFlag: false));
        var service = CreateService(db);

        var detail = await service.GetStudentExamFormDetailAsync(1);

        Assert.NotNull(detail);
        var subject = Assert.Single(detail!.Subjects);
        Assert.Equal(301, subject.SubjectOfferingId);
        // The student's cohort year (AY 1), not the schedule instance's (AY 2).
        Assert.Equal("2081", detail.AcademicYearName);
    }

    [Fact]
    public async Task GetEditableSubjectsAsync_CrossCohortPartial_LegacyRowWithoutFlag_ShowsBatchVersionSubjectAndYear()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId),
            ctx => SeedCrossCohortPartial(ctx, stampSupplementaryFlag: false));
        var service = CreateService(db);

        var model = await service.GetEditableSubjectsAsync(1);

        Assert.NotNull(model);
        Assert.True(model!.CanEdit);
        var subject = Assert.Single(model.AvailableSubjects);
        Assert.Equal(301, subject.SubjectOfferingId);
        Assert.True(subject.IsSelected);
        Assert.DoesNotContain(model.AvailableSubjects, s => s.SubjectOfferingId == 401);
        Assert.Equal("2081", model.AcademicYearName);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_CrossCohortPartial_LegacyRowWithoutFlag_SavesBatchVersionSubject()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId),
            ctx => SeedCrossCohortPartial(ctx, stampSupplementaryFlag: false));
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(1, [301]);

        Assert.True(success, message);
        var log = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-CROSS");
        // Legacy plain-id input is normalized to an explicit theory-leg token
        // (offering 301 is theory-only).
        Assert.Equal("301:T", log.SelectedSubjectIds);
        Assert.Contains(db.Context.ExamSubjectResults!, r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 301 && r.IsActive);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_ExplicitLegs_RegisterOnlyChosenPapers()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), ctx =>
        {
            SeedPartialForm(ctx);

            var both = TestData.Offering(302, 1, TestData.ProgramId);
            both.HasPractical = true;
            ctx.SubjectOfferings.Add(both);
        });
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(6,
            [101, 302],
            new Dictionary<int, ReExamLegs>
            {
                [101] = ReExamLegs.Theory,
                [302] = ReExamLegs.Theory | ReExamLegs.Practical
            });

        Assert.True(success, message);

        var log = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-PARTIAL");
        Assert.Equal("101:T,302:TP", log.SelectedSubjectIds);

        var theoryRow = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.SubjectOfferingId == 101);
        Assert.True(theoryRow.IsTheoryRegistered);
        Assert.False(theoryRow.IsPracticalRegistered);

        var bothRow = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.SubjectOfferingId == 302);
        Assert.True(bothRow.IsTheoryRegistered);
        Assert.True(bothRow.IsPracticalRegistered);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_LegChangeOnRegisteredSubject_RetiresAndRecreatesRow()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), ctx =>
        {
            SeedPartialForm(ctx);

            var both = TestData.Offering(302, 1, TestData.ProgramId);
            both.HasPractical = true;
            ctx.SubjectOfferings.Add(both);

            // Previous regular attempt sat both papers of 302.
            var previousBoth = TestData.Result(11, 7, 302, TestData.Regular, "C", 25);
            previousBoth.IsTheoryRegistered = true;
            previousBoth.IsPracticalRegistered = true;
            previousBoth.ObtainedMarksTheory = 48;
            previousBoth.ObtainedMarksPractical = 55;
            previousBoth.ObtainedMarksTheoryInternal = 12;
            previousBoth.ObtainedMarksPracticalInternal = 14;
            ctx.ExamSubjectResults!.Add(previousBoth);

            // Currently registered on the partial form for both papers.
            var currentRow = TestData.Result(20, 6, 302, TestData.Partial, null, 24);
            currentRow.IsTheoryRegistered = true;
            currentRow.IsPracticalRegistered = true;
            currentRow.ObtainedMarksPractical = 55;
            currentRow.ObtainedMarksTheoryInternal = 12;
            currentRow.ObtainedMarksPracticalInternal = 14;
            ctx.ExamSubjectResults!.Add(currentRow);
        });
        db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-PARTIAL").SelectedSubjectIds = "302";
        db.Context.SaveChanges();
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(6,
            [302], new Dictionary<int, ReExamLegs> { [302] = ReExamLegs.Practical });

        Assert.True(success, message);

        var retired = db.Context.ExamSubjectResults!.Single(r => r.Id == 20);
        Assert.False(retired.IsActive);

        var recreated = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 6 && r.IsActive);
        Assert.False(recreated.IsTheoryRegistered);
        Assert.True(recreated.IsPracticalRegistered);
        // The re-sat practical's external is cleared for fresh entry; the passed
        // theory external and both internals carry forward.
        Assert.Null(recreated.ObtainedMarksPractical);
        Assert.Equal(48f, recreated.ObtainedMarksTheory);
        Assert.Equal(12f, recreated.ObtainedMarksTheoryInternal);
        Assert.Equal(14f, recreated.ObtainedMarksPracticalInternal);

        var log = db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-PARTIAL");
        Assert.Equal("302:P", log.SelectedSubjectIds);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_UnavailableLeg_IsRejected()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialForm);
        var service = CreateService(db);

        // Offering 301 is theory-only.
        var (success, message) = await service.UpdateRegistrationSubjectsAsync(6,
            [301], new Dictionary<int, ReExamLegs> { [301] = ReExamLegs.Practical });

        Assert.False(success);
        Assert.Contains("Select at least one available exam paper", message);
        Assert.DoesNotContain(db.Context.ExamSubjectResults!, r => r.ExamRegistrationId == 6);
        Assert.Equal("101", db.Context.PaymentRequestLogs!.Single(l => l.InvoiceNumber == "INV-PARTIAL").SelectedSubjectIds);
    }

    [Fact]
    public async Task UpdateRegistrationSubjectsAsync_RegularForm_IgnoresSubmittedLegs()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedFormsWithExistingResult);
        var service = CreateService(db);

        var (success, message) = await service.UpdateRegistrationSubjectsAsync(1,
            [301], new Dictionary<int, ReExamLegs> { [301] = ReExamLegs.Theory | ReExamLegs.Practical });

        Assert.True(success, message);

        // Regular forms register whole subjects: the fallback papers only ("T" here).
        var log = db.Context.PaymentRequestLogs!.Single(l => l.ExamScheduleId == 21 && l.StudentRegistrationId == 1 && l.PaymentRequestLogStatus == 1);
        Assert.Equal("301:T", log.SelectedSubjectIds);

        var row = db.Context.ExamSubjectResults!.Single(r => r.ExamRegistrationId == 1 && r.SubjectOfferingId == 301);
        Assert.True(row.IsTheoryRegistered);
        Assert.False(row.IsPracticalRegistered);
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
