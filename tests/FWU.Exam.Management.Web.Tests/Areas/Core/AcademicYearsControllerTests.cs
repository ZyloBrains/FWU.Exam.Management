using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Areas.Core.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Core;

public class AcademicYearsControllerTests
{
    private readonly IAcademicYearService _academicYearService;
    private readonly AcademicYearsController _controller;

    public AcademicYearsControllerTests()
    {
        _academicYearService = Substitute.For<IAcademicYearService>();

        var httpContext = new DefaultHttpContext();
        _controller = new AcademicYearsController(_academicYearService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        _academicYearService.GetAllAcademicYearsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>())
            .Returns((new List<AcademicYear>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        var result = _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_WithValidModel_ShouldRedirectToIndex()
    {
        var academicYear = new AcademicYear { AcademicYearCode = "2024", AcademicYearName = "2024-2025" };

        _controller.ModelState.Clear();
        var result = await _controller.Create(academicYear);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WithInvalidModel_ShouldReturnView()
    {
        var academicYear = new AcademicYear();
        _controller.ModelState.AddModelError("AcademicYearCode", "Required");

        var result = await _controller.Create(academicYear);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Details_WithNullId_ShouldReturnNotFound()
    {
        var result = await _controller.Details(null);

        result.Should().BeOfType<NotFoundResult>();
    }
}
