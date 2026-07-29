using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using FWU.Exam.Management.Web.Areas.Core.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Core;

public class FacultyControllerTests
{
    private readonly IFacultyService _facultyService;
    private readonly IFileUploadHelper _fileUploadHelper;
    private readonly FacultyController _controller;

    public FacultyControllerTests()
    {
        _facultyService = Substitute.For<IFacultyService>();
        _fileUploadHelper = Substitute.For<IFileUploadHelper>();

        _controller = new FacultyController(_facultyService, _fileUploadHelper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        _facultyService.GetFacultiesPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((new List<Faculty>(), 0));

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
    public async Task Details_WithNullId_ShouldReturnNotFound()
    {
        var result = await _controller.Details(null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WithInvalidId_ShouldReturnNotFound()
    {
        _facultyService.GetFacultyByIdAsync(999).Returns((Faculty?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
