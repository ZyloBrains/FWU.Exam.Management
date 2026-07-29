using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamRegistrationServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private async Task SeedMinimalDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelCode = "BACH", LevelName = "Bachelor", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Final", Code = "FIN", IsActive = true });
        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        context.Set<Gender>().Add(new Gender { GenderName = "Male", IsActive = true });
        context.Set<StudentCategory>().Add(new StudentCategory { StudentCategoryName = "Regular", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "Bachelor of Computer Application", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<College>().Add(new College { TenantId = TestTenantId, Code = "FWU01", Name = "Far Western University College", Email = "college@fwu.edu.np", PrincipalName = "Principal", PrincipalContactNumber = "9800000000", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamSchedule>().Add(new ExamSchedule { TenantId = TestTenantId, ExamScheduleName = "2081 BCA Final", AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1, IsActive = true, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(11, 0) });
        await context.SaveChangesAsync();

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, Code = "EC001", IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetExamRegistrations_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<ExamRegistration>().Add(new ExamRegistration
            {
                TenantId = TestTenantId,
                AcademicYearId = 1,
                CollegeId = 1,
                ExamScheduleId = 1,
                ProgramsId = 1,
                ExamCenterId = 1,
                ExamRollNumber = $"RN-{i:D3}",
                RegistrationDate = DateTime.UtcNow,
                Status = RegistrationStatus.Pending,
                IsActive = true,
                IsAppliedByStudent = true
            });
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var (items, totalCount) = await service.GetExamRegistrationsAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExamRegistrations_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            Remarks = "Regular",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        });
        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-002",
            Remarks = "Supplementary",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var (items, totalCount) = await service.GetExamRegistrationsAsync(1, 10, "RN-001", "Id", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].ExamRollNumber.Should().Be("RN-001");
    }

    [Fact]
    public async Task GetFilteredItems_WithSearch_ShouldReturnMatching()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            Remarks = "Special case",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        });
        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-002",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var items = await service.GetFilteredItemsAsync("Special");

        items.Should().HaveCount(1);
        items[0].ExamRollNumber.Should().Be("RN-001");
    }

    [Fact]
    public async Task GetStudentExamForms_ShouldReturnAggregatedData()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<StudentRegistration>().Add(new StudentRegistration
        {
            TenantId = TestTenantId,
            LevelId = 1,
            CollegeId = 1,
            RegistrationNumber = "FWU-2024-001",
            FirstName = "Ram",
            LastName = "Sharma",
            DateOfBirthBS = "2056/01/01",
            GenderId = 1,
            StudentCategoryId = 1,
            AcademicYearId = 1,
            IsActive = true
        });
        await context.SaveChangesAsync();

        context.Set<PaymentType>().Add(new PaymentType { PaymentTypeName = "eSewa", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ApplicationVoucher>().Add(new ApplicationVoucher
        {
            TenantId = TestTenantId,
            VoucherNumber = "VCH-001",
            StudentName = "Ram Sharma",
            Amount = 1000,
            ExamScheduleId = 1,
            StudentRegistrationId = 1,
            ContactNumber = "9800000000"
        });
        await context.SaveChangesAsync();

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            ApplicationVoucherId = 1,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        });

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-002",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Registered,
            IsActive = true,
            IsAppliedByStudent = true
        });
        await context.SaveChangesAsync();

        context.Set<PaymentRequestLog>().Add(new PaymentRequestLog
        {
            TenantId = TestTenantId,
            PaymentRequestLogStatus = 1,
            InvoiceNumber = "INV-001",
            ForwardedTimestamp = DateTime.UtcNow,
            FullName = "Ram Sharma",
            FullRequestContent = "{}",
            PaymentTypeId = 1,
            ExamScheduleId = 1,
            StudentRegistrationId = 1,
            Amount = 1000,
            StudentCount = 1
        });
        await context.SaveChangesAsync();

        context.Set<AdmitCard>().Add(new AdmitCard
        {
            TenantId = TestTenantId,
            ExamRegistrationId = 2,
            ExamScheduleId = 1,
            StudentRegistrationId = 1,
            AdmitCardNumber = "AC-0001-000002",
            GeneratedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var result = await service.GetStudentExamFormsAsync(null, null, 1, 10);

        result.TotalCount.Should().Be(2);
        result.Forms.Should().HaveCount(2);
        result.Forms.Should().Contain(f => f.ExamRegistrationId == 1 && f.PaymentConfirmed && f.InvoiceNumber == "INV-001");
        result.Forms.Should().Contain(f => f.ExamRegistrationId == 2 && f.HasAdmitCard);
        result.PaymentConfirmedCount.Should().Be(1);
        result.AdmitCardGeneratedCount.Should().Be(1);
        result.PendingAdmitCardCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateExamRegistration_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var entity = new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true
        };

        await service.CreateExamRegistrationAsync(entity);

        var saved = await context.Set<ExamRegistration>().FindAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.ExamRollNumber.Should().Be("RN-001");
    }

    [Fact]
    public async Task UpdateExamRegistration_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        var existing = await service.GetExamRegistrationByIdAsync(1);
        existing!.Remarks = "Updated";
        await service.UpdateExamRegistrationAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetExamRegistrationByIdAsync(1);
        updated!.Remarks.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteExamRegistration_ShouldSetInactive()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        await service.DeleteExamRegistrationAsync(1);

        var deleted = await context.Set<ExamRegistration>().FindAsync(1);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyExamRegistration_ShouldChangeStatus()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        await service.VerifyExamRegistrationAsync(1);

        var verified = await context.Set<ExamRegistration>().FindAsync(1);
        verified!.Status.Should().Be(RegistrationStatus.CollegeVerified);
    }

    [Fact]
    public async Task ApproveExamRegistration_ShouldChangeStatus()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedMinimalDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId,
            AcademicYearId = 1,
            CollegeId = 1,
            ExamScheduleId = 1,
            ProgramsId = 1,
            ExamCenterId = 1,
            ExamRollNumber = "RN-001",
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.CollegeVerified,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var userContext = CreateSuperAdminContext();
        var service = new ExamRegistrationService(context, userContext);

        await service.ApproveExamRegistrationAsync(1);

        var approved = await context.Set<ExamRegistration>().FindAsync(1);
        approved!.Status.Should().Be(RegistrationStatus.AdminVerified);
    }
}
