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

public class ExamSubjectResultsControllerTests
{
    private readonly IExamSubjectResultService _examSubjectResultService;
    private readonly IUserContext _userContext;
    private readonly AppDbContext _context;
    private readonly ExamSubjectResultsController _controller;

    public ExamSubjectResultsControllerTests()
    {
        _examSubjectResultService = Substitute.For<IExamSubjectResultService>();
        _userContext = Substitute.For<IUserContext>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ExamSubjectResultsController(_examSubjectResultService, _userContext, _context)
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
        _examSubjectResultService.GetRegistrationsWithSubjectResultsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<int?>())
            .Returns((new List<ExamRegistrationGroupedDto>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        _examSubjectResultService.GetSelectListDataAsync().Returns(new ExamSubjectResultSelectListsDto());

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_WithNullId_ShouldReturnNotFound()
    {
        var result = await _controller.Edit(null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_WithInvalidId_ShouldReturnNotFound()
    {
        _examSubjectResultService.GetExamSubjectResultByIdAsync(999).Returns((ExamSubjectResult?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
