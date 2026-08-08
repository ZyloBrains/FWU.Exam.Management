using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleServiceTests
{
    private const int VersionId = 5;

    private static ExamScheduleService CreateService(TestDb db)
    {
        var uc = new TestUserContext();
        uc.SetUser("admin-1", null, null, [], ["SuperAdmin"]);
        return new(db.Context, uc);
    }

    private static void SeedVersion(AppDbContext ctx, int versionId = VersionId, int programId = TestData.ProgramId,
        int effectiveAcademicYearId = TestData.AcademicYearId, bool isActive = true, string? name = null)
    {
        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = versionId,
        TenantId = TestData.TenantId,
            Name = name ?? $"Default - BCA ({effectiveAcademicYearId})",
            ProgramId = programId,
            EffectiveAcademicYearId = effectiveAcademicYearId,
            IsActive = isActive
        });
    }

    private static void AssignOfferings(AppDbContext ctx, int versionId, params int[] offeringIds)
    {
        foreach (var id in offeringIds)
            ctx.SubjectOfferings.Local.First(o => o.Id == id).CurriculumVersionId = versionId;
    }

    [Fact]
    public async Task GetCurriculumVersionsByProgramAsync_ReturnsOnlyProgramVersions_OrderedByEffectiveYearDescThenIdDesc()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.AcademicYears.Add(new AcademicYear
            {
                Id = 2,
                AcademicYearCode = "2082",
                AcademicYearName = "2082",
                AcademicYearNameNepali = "2082",
                IsActive = true,
                IsRunning = false
            });
            SeedVersion(ctx, 10, effectiveAcademicYearId: TestData.AcademicYearId, name: "V1 (2081)");
            SeedVersion(ctx, 11, effectiveAcademicYearId: 2, name: "V2 (2082)");
            SeedVersion(ctx, 12, programId: TestData.ProgramIdOther, effectiveAcademicYearId: 2, name: "Other (2082)");
        });
        var service = CreateService(db);

        var versions = await service.GetCurriculumVersionsByProgramAsync(TestData.ProgramId);

        Assert.Equal(new[] { 11, 10 }, versions.Select(v => v.Id));
    }

    [Fact]
    public async Task GetSemestersByCurriculumVersionAsync_ReturnsDistinctSemestersWithOfferings_OrderedByNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
    {
            TestData.SeedBase(ctx);
            SeedVersion(ctx);
            AssignOfferings(ctx, VersionId, 101, 102, 103, 104);
        });
        var service = CreateService(db);

        var semesters = await service.GetSemestersByCurriculumVersionAsync(VersionId);

        Assert.Equal(new[] { 1, 2, 3, 4 }, semesters.Select(s => s.Id));
    }

    [Fact]
    public async Task GetSemestersByCurriculumVersionAsync_ReturnsEmpty_WhenVersionHasNoOfferings()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedVersion(ctx);
        });

        var service = CreateService(db);
        var schedule = await service.GetExamScheduleByIdAsync(1);
        Assert.NotNull(schedule);

        schedule.ExamScheduleName = "Renamed";

        var semesters = await service.GetSemestersByCurriculumVersionAsync(VersionId);

        Assert.Empty(semesters);
    }

    [Fact]
    public async Task GetSelectListDataAsync_UsesVersionDerivedSemesters_WhenScheduleHasCurriculumVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedVersion(ctx);
            AssignOfferings(ctx, VersionId, 102, 103, 104);
        });

        var service = CreateService(db);
        var schedule = TestData.Schedule(31, 2, TestData.Regular, null, null);
        schedule.CurriculumVersionId = VersionId;

        var dto = await service.GetSelectListDataAsync(schedule);

        Assert.Equal(new[] { 2, 3, 4 }, dto.Semesters.Select(s => s.Id));
        Assert.Contains(dto.CurriculumVersions, v => v.Id == VersionId);
    }

    [Fact]
    public async Task GetSelectListDataAsync_FallsBackToAcademicYearSemesters_WhenScheduleHasNoCurriculumVersion()
        {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);
        var schedule = TestData.Schedule(31, 2, TestData.Regular, null, null);

        var dto = await service.GetSelectListDataAsync(schedule);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, dto.Semesters.Select(s => s.Id));
    }
}
