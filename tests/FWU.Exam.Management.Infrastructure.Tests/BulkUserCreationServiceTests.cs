using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class BulkUserCreationServiceTests
{
    private static BulkUserCreationService CreateService(TestDb db, IUserContext userContext)
    {
        var scopeFactory = new ServiceCollection()
            .BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();
        return new BulkUserCreationService(db.Context, scopeFactory, db.Tenant, userContext, new TestAuditLogWriter());
    }

    private static void SeedSecondCollege(AppDbContext ctx)
    {
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
    }

    [Fact]
    public async Task GetStudentsWithoutUsersAsync_CollegeAdmin_SeesOwnCollegeStudentsOnly()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedSecondCollege(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "s1@test.com"));
            var other = TestData.StudentRegistration(2, "s2@test.com");
            other.CollegeId = 2;
            ctx.StudentRegistrations.Add(other);
        });

        var uc = new TestUserContext();
        uc.SetUser("admin-1", null, TestData.CollegeId, [], [Role.CollegeAdmin]);
        var service = CreateService(db, uc);

        var (data, totalCount) = await service.GetStudentsWithoutUsersAsync(null, null, null, 1, 50);

        Assert.Equal(1, totalCount);
        Assert.Single(data);
        Assert.Equal("s1@test.com", data[0].Email);
    }

    [Fact]
    public async Task GetStudentsWithoutUsersAsync_FacultyAdmin_SeesFacultyStudentsOnly()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Faculties.Add(new Faculty { Id = 1, Name = "Faculty One", OfficeCode = "FAC1" });
            ctx.Faculties.Add(new Faculty { Id = 2, Name = "Faculty Two", OfficeCode = "FAC2" });
            var own = TestData.StudentRegistration(1, "s1@test.com");
            own.FacultyId = 1;
            ctx.StudentRegistrations.Add(own);
            var other = TestData.StudentRegistration(2, "s2@test.com");
            other.FacultyId = 2;
            ctx.StudentRegistrations.Add(other);
        });

        var uc = new TestUserContext();
        uc.SetUser("admin-1", 1, null, [TestData.CollegeId], [Role.FacultyAdmin]);
        var service = CreateService(db, uc);

        var (data, totalCount) = await service.GetStudentsWithoutUsersAsync(null, null, null, 1, 50);

        Assert.Equal(1, totalCount);
        Assert.Equal("s1@test.com", data[0].Email);
    }

    [Fact]
    public async Task GetStudentsWithoutUsersAsync_SuperAdmin_SeesAllStudents()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedSecondCollege(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "s1@test.com"));
            var other = TestData.StudentRegistration(2, "s2@test.com");
            other.CollegeId = 2;
            ctx.StudentRegistrations.Add(other);
        });

        var uc = new TestUserContext();
        uc.SetUser("admin-1", null, null, [], [Role.SuperAdmin]);
        var service = CreateService(db, uc);

        var (_, totalCount) = await service.GetStudentsWithoutUsersAsync(null, null, null, 1, 50);

        Assert.Equal(2, totalCount);
    }
}
