using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Areas.Exams.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Exams;

public class ExamRegistrationsControllerTests
{
    private readonly IExamRegistrationService _examRegistrationService;
    private readonly IUserContext _userContext;
    private readonly AppDbContext _context;
    private readonly ExamRegistrationsController _controller;

    public ExamRegistrationsControllerTests()
    {
        _examRegistrationService = Substitute.For<IExamRegistrationService>();
        _userContext = Substitute.For<IUserContext>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ExamRegistrationsController(_examRegistrationService, _userContext, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        _examRegistrationService.GetExamRegistrationsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
            .Returns((new List<ExamRegistration>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        _examRegistrationService.GetSelectListDataAsync().Returns(new ExamRegistrationSelectListsDto());

        var result = await _controller.Create();

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
        _examRegistrationService.GetExamRegistrationByIdAsync(999).Returns((ExamRegistration?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
