using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class DashboardScopeTests
{
    private static TestUserContext SuperAdmin() =>
        new TestUserContext().WithUser(null, null, null, [], [Role.SuperAdmin]);

    private static TestUserContext FacultyAdmin(int facultyId, params int[] collegeIds) =>
        new TestUserContext().WithUser(null, facultyId, null, collegeIds, [Role.FacultyAdmin]);

    private static TestUserContext CollegeAdmin(int collegeId) =>
        new TestUserContext().WithUser(null, null, collegeId, [], [Role.CollegeAdmin]);

    private static TestUserContext Student() =>
        new TestUserContext().WithUser("student-1", null, null, [], [Role.Student]);

    private static void SeedMultiTenantData(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.Tenants.Add(new Tenant
        {
            Id = 2,
            Name = "Second Tenant",
            OfficeCode = "T2",
            ContactNumber = "000",
            Address = "Pokhara",
            Email = "t2@t.com",
            IsActive = true
        });

        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = 1 });
        ctx.Faculties.Add(new Faculty { Id = 2, Name = "Science", OfficeCode = "L002", ShortName = "SC", TenantId = 2 });

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

        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = 1, CollegeId = TestData.CollegeId, FacultyId = 1 });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = 2, CollegeId = 2, FacultyId = 2 });

        var bca = ctx.Programs.Local.Single(p => p.Id == TestData.ProgramId);
        bca.FacultyId = 1;

        ctx.Programs.Add(new Program
        {
            Id = 3,
            LevelId = TestData.LevelId,
            FacultyId = 2,
            ProgramCode = "MBA",
            ProgramName = "Master in Business Administration",
            ShortName = "MBA",
            Duration = 2,
            IsActive = true
        });

        ctx.Banks.Add(new Bank { Id = 1, TenantId = 1, BankName = "Bank One", BankCode = "B1", IsActive = true });
        ctx.Banks.Add(new Bank { Id = 2, TenantId = 2, BankName = "Bank Two", BankCode = "B2", IsActive = true });

        ctx.StudentRegistrations.Add(Registration(1, collegeId: TestData.CollegeId, programId: TestData.ProgramId, facultyId: 1, tenantId: 1));
        ctx.StudentRegistrations.Add(Registration(2, collegeId: TestData.CollegeId, programId: TestData.ProgramId, facultyId: null, tenantId: 1));
        ctx.StudentRegistrations.Add(Registration(3, collegeId: 2, programId: 3, facultyId: 2, tenantId: 2));
        ctx.StudentRegistrations.Add(Registration(4, collegeId: 2, programId: 3, facultyId: null, tenantId: 2));
    }

    private static StudentRegistration Registration(int id, int collegeId, int programId, int? facultyId, int tenantId)
    {
        var reg = TestData.StudentRegistration(id, $"stu{id}@test.com", programId);
        reg.TenantId = tenantId;
        reg.CollegeId = collegeId;
        reg.FacultyId = facultyId;
        return reg;
    }

    [Fact]
    public async Task SuperAdmin_InCentralTenant_SeesAllTenantsData()
    {
        using var db = new TestDb(TestTenantContext.Central(), SeedMultiTenantData);
        var uc = SuperAdmin();

        // Central-tenant query filters short-circuit, so all tenants' data is visible.
        Assert.Equal(2, await db.Context.Faculties.CountAsync());
        Assert.Equal(4, await db.Context.StudentRegistrations.CountAsync());
        Assert.Equal(2, await db.Context.Colleges.CountAsync());
        Assert.Equal(2, await db.Context.Banks.CountAsync());

        // ApplyScope adds no restriction for superadmin.
        Assert.Equal(2, await db.Context.Faculties.ApplyScope(uc).CountAsync());
        Assert.Equal(4, await db.Context.StudentRegistrations.ApplyScope(uc).CountAsync());
        Assert.Equal(2, await db.Context.Colleges.ApplyScope(uc).CountAsync());
    }

    [Fact]
    public async Task StandardTenant_TenantFilterRestrictsToOwnTenant()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);

        // Under a standard tenant the query filters hide the second tenant's rows.
        Assert.Equal(1, await db.Context.Faculties.CountAsync());
        Assert.Equal(2, await db.Context.StudentRegistrations.CountAsync());
        Assert.Equal(1, await db.Context.Colleges.CountAsync());
        Assert.Equal(1, await db.Context.Banks.CountAsync());
    }

    [Fact]
    public async Task StandardTenant_TenantFilterIsParameterized_NotInlinedAfterCentralWarmup()
    {
        // Warm the shared compiled query cache under a Central tenant first. If the query
        // filter were inlined as a constant (the pre-fix behavior), the Standard tenant would
        // then see all rows. Context-rooted filters are parameterized instead, so each query
        // re-evaluates the filter against the current ambient tenant.
        using (var central = new TestDb(TestTenantContext.Central(), SeedMultiTenantData))
        {
            await central.Context.Faculties.CountAsync();
            await central.Context.StudentRegistrations.CountAsync();
        }

        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);
        var sql = db.Context.Faculties.ToQueryString();

        Assert.Contains("FilterTenantId", sql);
        Assert.Contains("FilterIsCentral", sql);

        Assert.Equal(1, await db.Context.Faculties.CountAsync());
        Assert.Equal(2, await db.Context.StudentRegistrations.CountAsync());
        Assert.Equal(1, await db.Context.Colleges.CountAsync());
        Assert.Equal(1, await db.Context.Banks.CountAsync());
    }

    [Fact]
    public async Task FacultyAdmin_CountsStudentsByFacultyColumnOrProgramFaculty()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);
        var uc = FacultyAdmin(1, TestData.CollegeId);

        // reg1 matches via FacultyId column; reg2 via Program.FacultyId fallback.
        Assert.Equal(2, await db.Context.StudentRegistrations.ApplyScope(uc).CountAsync());
    }

    [Fact]
    public async Task FacultyAdmin_CollegesComeFromFacultyCollegeIds()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);
        var uc = FacultyAdmin(1, TestData.CollegeId);

        Assert.Equal(1, await db.Context.Colleges.ApplyScope(uc).CountAsync());
        Assert.Equal(1, await db.Context.Programs.ApplyScope(uc).CountAsync());
    }

    [Fact]
    public async Task CollegeAdmin_SeesOwnCollegeOnly()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);
        var uc = CollegeAdmin(TestData.CollegeId);

        Assert.Equal(2, await db.Context.StudentRegistrations.ApplyScope(uc).CountAsync());
        Assert.Equal(1, await db.Context.Colleges.ApplyScope(uc).CountAsync());
        Assert.Empty(await db.Context.Faculties.ApplyScope(uc).ToListAsync());
    }

    [Fact]
    public async Task Student_SeesNoAdminData()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedMultiTenantData);
        var uc = Student();

        Assert.Empty(await db.Context.Faculties.ApplyScope(uc).ToListAsync());
        Assert.Empty(await db.Context.StudentRegistrations.ApplyScope(uc).ToListAsync());
        Assert.Empty(await db.Context.Colleges.ApplyScope(uc).ToListAsync());
    }
}

internal static class TestUserContextExtensions
{
    public static TestUserContext WithUser(
        this TestUserContext ctx,
        string? userId,
        int? facultyId,
        int? collegeId,
        int[] facultyCollegeIds,
        string[] roles)
    {
        ctx.SetUser(userId, facultyId, collegeId, facultyCollegeIds, roles);
        return ctx;
    }
}
