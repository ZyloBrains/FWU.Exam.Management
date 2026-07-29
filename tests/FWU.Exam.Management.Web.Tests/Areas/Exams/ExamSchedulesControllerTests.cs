using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Areas.Exams.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Exams;

public class ExamSchedulesControllerTests
{
    private readonly IExamScheduleService _examScheduleService;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ExamSchedulesController _controller;

    public ExamSchedulesControllerTests()
    {
        _examScheduleService = Substitute.For<IExamScheduleService>();
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ExamSchedulesController(_examScheduleService, _userManager, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        _examScheduleService.GetSelectListDataAsync().Returns(new ExamScheduleSelectListsDto());

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
        _examScheduleService.GetExamScheduleByIdAsync(999).Returns((ExamSchedule?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
