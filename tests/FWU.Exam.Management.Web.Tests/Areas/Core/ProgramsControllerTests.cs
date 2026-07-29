using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Areas.Core.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Core;

public class ProgramsControllerTests
{
    private readonly IProgramService _programService;
    private readonly IUserContext _userContext;
    private readonly AppDbContext _context;
    private readonly ProgramsController _controller;

    public ProgramsControllerTests()
    {
        _programService = Substitute.For<IProgramService>();
        _userContext = Substitute.For<IUserContext>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ProgramsController(_programService, _userContext, _context)
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
        _programService.GetProgramsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((new List<Program>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        _programService.GetSelectListsAsync().Returns((new List<Board>(), new List<Level>()));

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
        _programService.GetProgramByIdAsync(999).Returns((Program?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
