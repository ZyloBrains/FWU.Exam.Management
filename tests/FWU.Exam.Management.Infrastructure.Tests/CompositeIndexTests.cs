using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CompositeIndexTests
{
    private static IReadOnlyList<string[]> IndexKeys(string tableName)
    {
        using var db = new TestDb(TestTenantContext.Standard());
        var entity = db.Context.Model.GetEntityTypes().Single(e => e.GetTableName() == tableName);
        return entity.GetIndexes()
            .Select(i => i.Properties.Select(p => p.Name).ToArray())
            .ToList();
    }

    private static void AssertIndex(string tableName, params string[] columns)
    {
        var keys = IndexKeys(tableName);
        Assert.True(
            keys.Any(k => k.SequenceEqual(columns)),
            $"Expected index on {tableName}({string.Join(", ", columns)}). Existing: " +
            string.Join(" | ", keys.Select(k => $"({string.Join(", ", k)})")));
    }

    [Fact]
    public void ExamSubjectResults_HasCompositeIndexOnScheduleAndOffering()
    {
        AssertIndex("ExamSubjectResults", "ExamScheduleId", "SubjectOfferingId", "IsActive");
    }

    [Fact]
    public void ExamSubjectResults_HasCompositeIndexOnRegistrationAndSchedule()
    {
        AssertIndex("ExamSubjectResults", "ExamRegistrationId", "ExamScheduleId", "IsActive");
    }

    [Fact]
    public void ExamRegistrations_HasCompositeIndexes()
    {
        AssertIndex("ExamRegistrations", "ExamScheduleId", "CollegeId", "IsActive", "Status");
        AssertIndex("ExamRegistrations", "ExamScheduleId", "ProgramsId", "IsActive");
        AssertIndex("ExamRegistrations", "SemesterEnrollmentId");
    }

    [Fact]
    public void PaymentRequestLogs_HasCompositeIndexOnScheduleStudentStatus()
    {
        AssertIndex("PaymentRequestLogs", "ExamScheduleId", "StudentRegistrationId", "PaymentRequestLogStatus");
    }

    [Fact]
    public void SemesterEnrollments_HasCompositeIndexes()
    {
        AssertIndex("SemesterEnrollments", "StudentAdmissionId", "EnrollmentStatus");
        AssertIndex("SemesterEnrollments", "StudentAdmissionId", "SemesterInstanceId");
    }

    [Fact]
    public void CollegeAdminSubjectAssignments_HasCompositeIndexOnOfferingSchedule()
    {
        AssertIndex("CollegeAdminSubjectAssignments", "SubjectOfferingId", "ExamScheduleId");
    }
}