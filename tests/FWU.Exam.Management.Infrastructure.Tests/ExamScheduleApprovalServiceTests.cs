using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleApprovalServiceTests
{
    private const int FacultyId = 1;
    private const int CollegeIdOther = 2;
    private const string AdminUserId = "admin-1";

    private static ExamScheduleApprovalService CreateService(TestDb db) => new(db.Context);

    private static void LinkCollegeToFaculty(AppDbContext ctx, int id, Faculty faculty)
    {
        var college = ctx.ChangeTracker.Entries<College>().Select(e => e.Entity).SingleOrDefault(c => c.Id == id);
        if (college == null)
        {
            college = new College
            {
                Id = id,
                Code = "OTH",
                Name = "Other College",
                Email = "oth@c.com",
                PrincipalName = "Principal",
                PrincipalContactNumber = "000",
                CollegeTypeId = 1,
                IsActive = true
            };
            ctx.Colleges.Add(college);
        }
        college.Faculties.Add(faculty);
        ctx.TenantColleges.Add(new TenantCollege { TenantId = TestData.TenantId, CollegeId = id });
    }

    private static Faculty Faculty() => new()
    {
        Id = FacultyId,
        Name = "Science & Tech",
        OfficeCode = "SCI",
        ShortName = "SCI",
        ContactNumber = "000",
        Address = "Kathmandu",
        Email = "fac@c.com",
        TenantId = TestData.TenantId
    };

    private static ExamScheduleCollegeApproval Approval(int id, int scheduleId, int collegeId,
        ExamScheduleApprovalStatus status = ExamScheduleApprovalStatus.Pending) => new()
    {
        Id = id,
        TenantId = TestData.TenantId,
        ExamScheduleId = scheduleId,
        CollegeId = collegeId,
        Status = status,
        IsActive = true
    };

    [Fact]
    public async Task CreateApprovalsForScheduleAsync_CreatesPendingRows_ForCollegesOfferingProgram()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var faculty = Faculty();
            ctx.Faculties.Add(faculty);
            LinkCollegeToFaculty(ctx, TestData.CollegeId, faculty);
            LinkCollegeToFaculty(ctx, CollegeIdOther, faculty);

            ctx.CollegePrograms.Add(new CollegeProgram
            {
                TenantId = TestData.TenantId,
                CollegeId = TestData.CollegeId,
                ProgramId = TestData.ProgramId,
                IsActive = true
            });

            ctx.ChangeTracker.Entries<Program>().Select(e => e.Entity).Single(p => p.Id == TestData.ProgramId).FacultyId = FacultyId;

            var schedule = TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null);
            schedule.CollegeApprovalDate = DateTime.UtcNow.AddDays(5);
            ctx.ExamSchedules.Add(schedule);
        });

        var service = CreateService(db);
        await service.CreateApprovalsForScheduleAsync(11);

        var rows = db.Context.ExamScheduleCollegeApprovals.Where(a => a.ExamScheduleId == 11).ToList();
        var row = Assert.Single(rows);
        Assert.Equal(TestData.CollegeId, row.CollegeId);
        Assert.Equal(ExamScheduleApprovalStatus.Pending, row.Status);
        Assert.NotNull(row.RequestedApprovalDate);
    }

    [Fact]
    public async Task CreateApprovalsForScheduleAsync_FallsBack_ToAllFacultyColleges_WhenNoCollegeOffersProgram()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var faculty = Faculty();
            ctx.Faculties.Add(faculty);
            LinkCollegeToFaculty(ctx, TestData.CollegeId, faculty);
            LinkCollegeToFaculty(ctx, CollegeIdOther, faculty);

            ctx.ChangeTracker.Entries<Program>().Select(e => e.Entity).Single(p => p.Id == TestData.ProgramId).FacultyId = FacultyId;
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
        });

        var service = CreateService(db);
        await service.CreateApprovalsForScheduleAsync(11);

        var collegeIds = db.Context.ExamScheduleCollegeApprovals
            .Where(a => a.ExamScheduleId == 11)
            .Select(a => a.CollegeId)
            .ToList();
        Assert.Equal(2, collegeIds.Count);
        Assert.Contains(TestData.CollegeId, collegeIds);
        Assert.Contains(CollegeIdOther, collegeIds);
    }

    [Fact]
    public async Task CreateApprovalsForScheduleAsync_IsIdempotent()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var faculty = Faculty();
            ctx.Faculties.Add(faculty);
            LinkCollegeToFaculty(ctx, TestData.CollegeId, faculty);
            ctx.ChangeTracker.Entries<Program>().Select(e => e.Entity).Single(p => p.Id == TestData.ProgramId).FacultyId = FacultyId;
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
        });

        var service = CreateService(db);
        await service.CreateApprovalsForScheduleAsync(11);
        await service.CreateApprovalsForScheduleAsync(11);

        Assert.Single(db.Context.ExamScheduleCollegeApprovals.Where(a => a.ExamScheduleId == 11));
    }

    [Fact]
    public async Task ApproveAsync_SetsApproved_AndScheduleVisibleToCollege()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(AdminUserId, "admin@test.com"));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamScheduleCollegeApprovals.Add(Approval(1, 11, TestData.CollegeId));
        });

        var service = CreateService(db);
        await service.ApproveAsync(11, TestData.CollegeId, AdminUserId);

        var row = db.Context.ExamScheduleCollegeApprovals.Single(a => a.ExamScheduleId == 11);
        Assert.Equal(ExamScheduleApprovalStatus.Approved, row.Status);
        Assert.NotNull(row.ApprovedDate);
        Assert.Equal(AdminUserId, row.ApprovedByUserId);
        Assert.True(await service.IsScheduleApprovedForCollegeAsync(11, TestData.CollegeId));
    }

    [Fact]
    public async Task RejectAsync_SetsRejected_WithProposedDateAndRemarks()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(AdminUserId, "admin@test.com"));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamScheduleCollegeApprovals.Add(Approval(1, 11, TestData.CollegeId));
        });

        var proposed = DateTime.UtcNow.AddDays(9);
        var service = CreateService(db);
        await service.RejectAsync(11, TestData.CollegeId, proposed, "Exam overlaps with internal assessment", AdminUserId);

        var row = db.Context.ExamScheduleCollegeApprovals.Single(a => a.ExamScheduleId == 11);
        Assert.Equal(ExamScheduleApprovalStatus.Rejected, row.Status);
        Assert.Equal(proposed, row.ProposedDate);
        Assert.Equal("Exam overlaps with internal assessment", row.Remarks);
        Assert.False(await service.IsScheduleApprovedForCollegeAsync(11, TestData.CollegeId));
    }

    [Fact]
    public async Task ResubmitAsync_ResetsRejectedToPending_AndKeepsApproved()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Colleges.Add(new College
            {
                Id = CollegeIdOther,
                Code = "OTH",
                Name = "Other College",
                Email = "oth@c.com",
                PrincipalName = "Principal",
                PrincipalContactNumber = "000",
                CollegeTypeId = 1,
                IsActive = true
            });
            var schedule = TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null);
            schedule.CollegeApprovalDate = DateTime.UtcNow.AddDays(5);
            ctx.ExamSchedules.Add(schedule);

            var rejected = Approval(1, 11, TestData.CollegeId, ExamScheduleApprovalStatus.Rejected);
            rejected.ProposedDate = DateTime.UtcNow.AddDays(6);
            rejected.Remarks = "Please move the date";
            rejected.RejectedDate = DateTime.UtcNow;
            ctx.ExamScheduleCollegeApprovals.Add(rejected);

            var approved = Approval(2, 11, CollegeIdOther, ExamScheduleApprovalStatus.Approved);
            approved.ApprovedDate = DateTime.UtcNow;
            ctx.ExamScheduleCollegeApprovals.Add(approved);
        });

        var service = CreateService(db);
        await service.ResubmitAsync(11);

        var rejectedRow = db.Context.ExamScheduleCollegeApprovals.Single(a => a.CollegeId == TestData.CollegeId);
        Assert.Equal(ExamScheduleApprovalStatus.Pending, rejectedRow.Status);
        Assert.Null(rejectedRow.Remarks);
        Assert.Null(rejectedRow.ProposedDate);
        Assert.NotNull(rejectedRow.RequestedApprovalDate);

        var approvedRow = db.Context.ExamScheduleCollegeApprovals.Single(a => a.CollegeId == CollegeIdOther);
        Assert.Equal(ExamScheduleApprovalStatus.Approved, approvedRow.Status);
    }

    [Fact]
    public async Task GetPendingCountForCollegeAsync_CountsOnlyPending()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(11)), null));
            ctx.ExamSchedules.Add(TestData.Schedule(13, 2, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)), null));

            ctx.ExamScheduleCollegeApprovals.Add(Approval(1, 11, TestData.CollegeId));                              // pending
            ctx.ExamScheduleCollegeApprovals.Add(Approval(2, 12, TestData.CollegeId, ExamScheduleApprovalStatus.Approved));
            ctx.ExamScheduleCollegeApprovals.Add(Approval(3, 13, TestData.CollegeId, ExamScheduleApprovalStatus.Rejected));
        });

        var service = CreateService(db);
        var count = await service.GetPendingCountForCollegeAsync(TestData.CollegeId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetApprovalsForScheduleAsync_MapsCollegeNameAndStatus()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.TenantColleges.Add(new TenantCollege { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId });
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamScheduleCollegeApprovals.Add(Approval(1, 11, TestData.CollegeId));
        });

        var service = CreateService(db);
        var rows = await service.GetApprovalsForScheduleAsync(11);

        var row = Assert.Single(rows);
        Assert.Equal("Test College", row.CollegeName);
        Assert.Equal(ExamScheduleApprovalStatus.Pending, row.Status);
    }

    [Fact]
    public async Task IsScheduleApprovedForCollegeAsync_ReturnsFalse_WhenNoRowsExist()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);

        var service = CreateService(db);
        var approved = await service.IsScheduleApprovedForCollegeAsync(99, TestData.CollegeId);

        Assert.False(approved);
    }
}
