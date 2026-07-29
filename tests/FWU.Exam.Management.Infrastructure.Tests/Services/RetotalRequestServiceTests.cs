using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class RetotalRequestServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private async Task<RetotalRequest> SeedRetotalRequestAsync(AppDbContext context)
    {
        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);

        var gender = new Gender { GenderName = "Male", IsActive = true };
        context.Set<Gender>().Add(gender);

        var studentCategory = new StudentCategory { StudentCategoryName = "General", IsActive = true };
        context.Set<StudentCategory>().Add(studentCategory);

        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);

        await context.SaveChangesAsync();

        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);
        await context.SaveChangesAsync();

        var subjectCatalog = new SubjectCatalog { SubjectCode = "MTH101", SubjectName = "Mathematics", SubjectTypeId = subjectType.Id, IsActive = true };
        context.Set<SubjectCatalog>().Add(subjectCatalog);

        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = level.Id, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);

        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = academicYear.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Set<Semester>().Add(semester);

        await context.SaveChangesAsync();

        var subjectOffering = new SubjectOffering { TenantId = TestTenantId, SubjectCatalogId = subjectCatalog.Id, ProgramId = program.Id, SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1, HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40 };
        context.Set<SubjectOffering>().Add(subjectOffering);

        var examSchedule = new ExamSchedule { ExamScheduleName = "Final 2081", TenantId = TestTenantId, AcademicYearId = academicYear.Id, ProgramId = program.Id, SemesterId = semester.Id, ExamTypeId = examType.Id, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), IsActive = true };
        context.Set<ExamSchedule>().Add(examSchedule);

        var studentReg = new StudentRegistration { FirstName = "John", LastName = "Doe", Email = "john@test.com", LevelId = level.Id, CollegeId = college.Id, GenderId = gender.Id, DateOfBirthBS = "2055-01-01", StudentCategoryId = studentCategory.Id, AcademicYearId = academicYear.Id, IsActive = true, TenantId = TestTenantId, RegistrationNumber = "REG001" };
        context.Set<StudentRegistration>().Add(studentReg);

        await context.SaveChangesAsync();

        var examReg = new ExamRegistration { ExamScheduleId = examSchedule.Id, AcademicYearId = academicYear.Id, CollegeId = college.Id, TenantId = TestTenantId, IsActive = true };
        context.Set<ExamRegistration>().Add(examReg);

        await context.SaveChangesAsync();

        var examSubjectResult = new ExamSubjectResult { ExamRegistrationId = examReg.Id, ExamTypeId = examType.Id, SubjectOfferingId = subjectOffering.Id, IsActive = true, TenantId = TestTenantId };
        context.Set<ExamSubjectResult>().Add(examSubjectResult);
        await context.SaveChangesAsync();

        var retotal = new RetotalRequest
        {
            TenantId = TestTenantId,
            ExamSubjectResultId = examSubjectResult.Id,
            StudentRegistrationId = studentReg.Id,
            ExamRegistrationId = examReg.Id,
            RequestedDate = DateTime.UtcNow,
            Reason = "Check marks",
            Status = RetotalStatus.Pending,
            OriginalGradeLetter = "B",
            OriginalObtainedMarks = 65,
            FeeAmount = 500,
            FeePaid = true,
            IsActive = true
        };
        context.Set<RetotalRequest>().Add(retotal);
        await context.SaveChangesAsync();

        return retotal;
    }

    [Fact]
    public async Task CreateRetotalRequestAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        var seeded = await SeedRetotalRequestAsync(context);

        var request = new RetotalRequest
        {
            TenantId = TestTenantId,
            ExamSubjectResultId = seeded.ExamSubjectResultId,
            StudentRegistrationId = seeded.StudentRegistrationId,
            ExamRegistrationId = seeded.ExamRegistrationId,
            RequestedDate = DateTime.UtcNow,
            Reason = "Re-evaluation",
            Status = RetotalStatus.Pending,
            FeeAmount = 500,
            FeePaid = true,
            IsActive = true
        };

        await service.CreateRetotalRequestAsync(request);

        var result = await service.GetRetotalRequestByIdAsync(request.Id);
        result.Should().NotBeNull();
        result!.Reason.Should().Be("Re-evaluation");
    }

    [Fact]
    public async Task GetRetotalRequestsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        await SeedRetotalRequestAsync(context);

        var (items, totalCount) = await service.GetRetotalRequestsAsync(1, 10, null, "id", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateRetotalRequestAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        var request = await SeedRetotalRequestAsync(context);
        context.ChangeTracker.Clear();

        request.Reason = "Updated reason";
        await service.UpdateRetotalRequestAsync(request);

        context.ChangeTracker.Clear();
        var updated = await service.GetRetotalRequestByIdAsync(request.Id);
        updated!.Reason.Should().Be("Updated reason");
    }

    [Fact]
    public async Task DeleteRetotalRequestAsync_ShouldSetInactive()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        var request = await SeedRetotalRequestAsync(context);

        await service.DeleteRetotalRequestAsync(request.Id);

        var exists = await service.RetotalRequestExistsAsync(request.Id);
        exists.Should().BeTrue();

        var fetched = await context.Set<RetotalRequest>().FindAsync(request.Id);
        fetched.Should().NotBeNull();
        fetched!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task MarkUnderReview_Approve_Reject_ShouldTransitionStatus()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        var request = await SeedRetotalRequestAsync(context);

        await service.MarkUnderReviewAsync(request.Id, "reviewer1");
        var underReview = await service.GetRetotalRequestByIdAsync(request.Id);
        underReview!.Status.Should().Be(RetotalStatus.UnderReview);

        await service.ApproveRetotalRequestAsync(request.Id, "A", 90.0f, "Approved", "reviewer1");
        var approved = await service.GetRetotalRequestByIdAsync(request.Id);
        approved!.Status.Should().Be(RetotalStatus.Approved);
        approved.RetotalledGradeLetter.Should().Be("A");
        approved.RetotalledObtainedMarks.Should().Be(90.0f);
    }

    [Fact]
    public async Task RejectRetotalRequest_ShouldSetRejected()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new RetotalRequestService(context, userCtx);

        var request = await SeedRetotalRequestAsync(context);

        await service.RejectRetotalRequestAsync(request.Id, "Invalid reason", "reviewer1");

        var rejected = await service.GetRetotalRequestByIdAsync(request.Id);
        rejected!.Status.Should().Be(RetotalStatus.Rejected);
        rejected.AdminRemarks.Should().Be("Invalid reason");
    }
}
