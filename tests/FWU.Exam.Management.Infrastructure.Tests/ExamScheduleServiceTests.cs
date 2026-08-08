using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleServiceTests
{
    private static ExamScheduleService CreateService(TestDb db)
    {
        var uc = new TestUserContext();
        uc.SetUser("admin-1", null, null, [], ["SuperAdmin"]);
        return new(db.Context, uc);
    }

    [Fact]
    public async Task GetSelectListDataAsync_LoadsAcademicYearSemesters()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);
        var schedule = TestData.Schedule(31, 2, TestData.Regular, null, null);

        var dto = await service.GetSelectListDataAsync(schedule);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, dto.Semesters.Select(s => s.Id));
    }
}
