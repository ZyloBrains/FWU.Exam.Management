using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using FWU.Exam.Management.Web.Areas.Subjects.Controllers;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class SubjectOfferingsControllerTests
{
    [Fact]
    public async Task Create_Get_RendersView()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                Name = "CV 1",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });
        });

        var userContext = new TestUserContext();
        userContext.SetUser("test", null, null, [], [Role.SuperAdmin]);
        var service = new SubjectOfferingService(db.Context, userContext);
        var controller = new SubjectOfferingsController(service);

        var result = await controller.Create(1);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_RestoresArchivedOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.CurriculumVersions.Add(new CurriculumVersion
            {
                Id = 1,
                TenantId = TestData.TenantId,
                Name = "CV 1",
                ProgramId = TestData.ProgramId,
                EffectiveAcademicYearId = TestData.AcademicYearId,
                IsActive = true
            });
            ctx.SubjectOfferings.Add(new SubjectOffering
            {
                Id = 500,
                TenantId = TestData.TenantId,
                SubjectCatalogId = 1,
                ProgramId = TestData.ProgramId,
                SemesterId = 1,
                CurriculumVersionId = 1,
                IsActive = false,
                IsCompulsory = true,
                DisplayOrder = 1,
                HasTheory = true,
                TheoryFullMarks = 75,
                TheoryPassMarks = 27
            });
        });

        var userContext = new TestUserContext();
        userContext.SetUser("test", null, null, [], [Role.SuperAdmin]);
        var service = new SubjectOfferingService(db.Context, userContext);
        var controller = new SubjectOfferingsController(service);
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        var model = new SubjectOfferingBulkCreateViewModel
        {
            ProgramId = TestData.ProgramId,
            CurriculumVersionId = 1,
            Semesters = new List<SemesterSubjectOfferingGroup>
            {
                new SemesterSubjectOfferingGroup
                {
                    SemesterId = 1,
                    SemesterName = "Semester 1",
                    Subjects = new List<SubjectOfferingItemViewModel>
                    {
                        new SubjectOfferingItemViewModel
                        {
                            OfferingId = 500,
                            IsArchived = true,
                            SubjectCatalogId = 1,
                            IsCompulsory = true,
                            DisplayOrder = 1,
                            HasTheory = true,
                            TheoryFullMarks = 75,
                            TheoryPassMarks = 27
                        }
                    }
                }
            }
        };

        var result = await controller.Create(model);

        Assert.IsType<RedirectToActionResult>(result);

        db.Context.ChangeTracker.Clear();
        var restored = await db.Context.SubjectOfferings.FindAsync(500);
        Assert.NotNull(restored);
        Assert.True(restored.IsActive);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        private readonly Dictionary<string, object> _data = new();

        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            _data.Clear();
            foreach (var pair in values)
                _data[pair.Key] = pair.Value;
        }
    }
}
