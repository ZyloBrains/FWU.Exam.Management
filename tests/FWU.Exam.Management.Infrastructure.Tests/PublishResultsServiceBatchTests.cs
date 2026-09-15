using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class PublishResultsServiceBatchTests
{
    private static PublishResultsService CreateService(TestDb db) =>
        new(db.Context, new GradeCalculationService(db.Context));

    private static ExamRegistration PartialRegistration(int id, int scheduleId, int voucherId)
    {
        var reg = TestData.ExamRegistration(id, scheduleId, voucherId);
        reg.Status = FWU.Exam.Management.Domain.Enums.RegistrationStatus.CollegeVerified;
        reg.SymbolNumber = $"SYM{id}";
        reg.SemesterEnrollmentId = 1;
        return reg;
    }

    /// <summary>One student fully linked: student registration, admission, enrollment,
    /// voucher, exam registration, and a pinned marks row.</summary>
    private static void SeedSingleStudent(AppDbContext ctx)
    {
        MarksEntryTestSeed.SeedPartialContext(ctx);
        MarksEntryTestSeed.AddPartialSchedule(ctx);

        var sr = TestData.StudentRegistration(1, "stu1@test.com");
        sr.StudentAdmissionId = 1;
        ctx.StudentRegistrations.Add(sr);

        ctx.Users.Add(TestData.User("stu-user-1", "stu1@test.com"));

        var admission = TestData.Admission(1, "stu-user-1");
        admission.DateOfBirthBS = "2057-01-02";
        ctx.StudentAdmissions.Add(admission);

        ctx.Set<SemesterEnrollment>().Add(TestData.Enrollment(1, 1, 1));
        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, MarksEntryTestSeed.ScheduleId));
        ctx.ExamRegistrations.Add(PartialRegistration(1, MarksEntryTestSeed.ScheduleId, 1));
        ctx.ExamSubjectResults.Add(MarksEntryTestSeed.PinnedResult(1, 1,
            MarksEntryTestSeed.CurrentOfferingId, MarksEntryTestSeed.ScheduleId, TestData.Partial));
    }

    [Fact]
    public async Task GetPreviewAsync_PopulatesStudentLookups_InSingleBatch()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedSingleStudent);
        var service = CreateService(db);

        var preview = await service.GetPreviewAsync(MarksEntryTestSeed.ScheduleId, TestData.CollegeId);

        Assert.NotNull(preview);
        Assert.Equal(1, preview.TotalStudents);
        var student = Assert.Single(preview.Students);
        Assert.Equal("Test Student", student.StudentName);
        Assert.Equal("SYM1", student.SymbolNumber);
        Assert.Equal("REG1", student.RegistrationNumber);
        Assert.Equal("Other", student.Sex);
        Assert.Equal("2057-01-02", student.DateOfBirthBs);
    }

    [Fact]
    public async Task GetPreviewAsync_EmptyRegistrations_ReturnsEmptyList()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            MarksEntryTestSeed.SeedPartialContext(ctx);
            MarksEntryTestSeed.AddPartialSchedule(ctx);
        });
        var service = CreateService(db);

        var preview = await service.GetPreviewAsync(MarksEntryTestSeed.ScheduleId, TestData.CollegeId);

        Assert.NotNull(preview);
        Assert.Empty(preview.Students);
        Assert.Equal(0, preview.TotalStudents);
    }
}