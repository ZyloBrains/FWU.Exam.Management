using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Areas.Exams.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class EntraceExamControllerTests
{
    private readonly IExamRegistrationService _examRegistrationService = Substitute.For<IExamRegistrationService>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly AppDbContext _context;
    private readonly ExamRegistrationsController _controller;

    public EntraceExamControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ExamRegistrationsController(_examRegistrationService, _userContext, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView_WithApplications()
    {
        _examRegistrationService.GetExamRegistrationsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
            .Returns((new List<ExamRegistration>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_GET_ShouldReturnView()
    {
        _examRegistrationService.GetSelectListDataAsync(Arg.Any<ExamRegistration?>())
            .Returns(new ExamRegistrationSelectListsDto());

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_POST_ShouldRedirect_OnSuccess()
    {
        var examRegistration = new ExamRegistration();

        var result = await _controller.Create(examRegistration);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_POST_ShouldReturnView_OnInvalidModel()
    {
        _controller.ModelState.AddModelError("Error", "Invalid");
        var examRegistration = new ExamRegistration();
        _examRegistrationService.GetSelectListDataAsync(Arg.Any<ExamRegistration?>())
            .Returns(new ExamRegistrationSelectListsDto());

        var result = await _controller.Create(examRegistration);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Details_ShouldReturnView_WhenExists()
    {
        _examRegistrationService.GetExamRegistrationByIdAsync(1).Returns(new ExamRegistration());

        var result = await _controller.Details(1);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Details_ShouldReturnNotFound_WhenNotExists()
    {
        _examRegistrationService.GetExamRegistrationByIdAsync(1).Returns((ExamRegistration?)null);

        var result = await _controller.Details(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteConfirmed_POST_ShouldRedirect()
    {
        var result = await _controller.DeleteConfirmed(1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }
}
