using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CollegeProgramServiceTests
{
    private const int EngineeringFacultyId = 7;
    private const int ProgramWithFacultyId = 500;

    /// <summary>
    /// Mirrors the real database: every program has a FacultyId, and the college has
    /// no CollegeFaculty row at all. This is the combination that used to throw
    /// "The college is not affiliated with the program's faculty".
    /// </summary>
    private static void SeedCollegeWithNoFacultyAffiliation(AppDbContext ctx)
    {
        ctx.Faculties.Add(new Faculty
        {
            Id = EngineeringFacultyId,
            Name = "Engineering",
            OfficeCode = "ENG"
        });

        ctx.Programs.Add(new Program
        {
            Id = ProgramWithFacultyId,
            LevelId = TestData.LevelId,
            ProgramCode = "BENG",
            ProgramName = "Bachelor of Engineering",
            ShortName = "BENG",
            Duration = 4,
            FacultyId = EngineeringFacultyId,
            IsActive = true
        });
    }

    [Fact]
    public async Task CreateCollegeProgramsAsync_SavesWhenCollegeHasNoFacultyAffiliation()
    {
        var tenant = TestTenantContext.Standard();
        using var db = new TestDb(tenant, ctx =>
        {
            TestData.SeedBase(ctx);
            SeedCollegeWithNoFacultyAffiliation(ctx);
        });

        var service = new CollegeProgramService(db.Context, new TestUserContext());

        // No CollegeFaculty row exists for this college + faculty, which is exactly
        // the case that used to throw before reaching SaveChanges.
        var exception = await Record.ExceptionAsync(() => service.CreateCollegeProgramsAsync(
        [
            new CollegeProgram
            {
                CollegeId = TestData.CollegeId,
                ProgramId = ProgramWithFacultyId,
                NumberOfStudents = 10,
                IsActive = true
            }
        ]));

        Assert.Null(exception);

        var saved = await db.Context.CollegePrograms
            .AsNoTracking()
            .SingleAsync(cp => cp.CollegeId == TestData.CollegeId && cp.ProgramId == ProgramWithFacultyId);

        Assert.Equal(TestData.TenantId, saved.TenantId);
    }

    [Fact]
    public async Task UpdateCollegeProgramAsync_PreservesTenantIdWhenNotPosted()
    {
        var tenant = TestTenantContext.Standard();
        using var db = new TestDb(tenant, ctx =>
        {
            TestData.SeedBase(ctx);
            SeedCollegeWithNoFacultyAffiliation(ctx);
            ctx.CollegePrograms.Add(new CollegeProgram
            {
                TenantId = TestData.TenantId,
                CollegeId = TestData.CollegeId,
                ProgramId = TestData.ProgramId,
                NumberOfStudents = 5,
                IsActive = true
            });
        });

        var service = new CollegeProgramService(db.Context, new TestUserContext());

        // The seeded entity is still tracked; production gets a fresh per-request context.
        db.Context.ChangeTracker.Clear();

        // Mirrors the Edit POST, whose bind list omits TenantId so it arrives as 0.
        var formEntity = new CollegeProgram
        {
            Id = db.Context.CollegePrograms.AsNoTracking().Select(cp => cp.Id).Single(),
            CollegeId = TestData.CollegeId,
            ProgramId = TestData.ProgramId,
            NumberOfStudents = 99,
            IsActive = true
        };

        var exception = await Record.ExceptionAsync(() => service.UpdateCollegeProgramAsync(formEntity));
        Assert.Null(exception);

        var saved = await db.Context.CollegePrograms.AsNoTracking().SingleAsync();
        Assert.Equal(99, saved.NumberOfStudents);
        Assert.Equal(TestData.TenantId, saved.TenantId);
    }
}
