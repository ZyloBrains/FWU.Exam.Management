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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class ExamControllerTests
{
    private readonly IExamScheduleService _examScheduleService = Substitute.For<IExamScheduleService>();
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ExamSchedulesController _controller;

    public ExamControllerTests()
    {
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ExamSchedulesController(_examScheduleService, _userManager, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact(Skip = "EF Core InMemory provider can't compile GroupBy + ToDictionaryAsync; requires a real database")]
    public async Task Index_ShouldReturnView()
    {
        _examScheduleService.GetExamSchedulesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns((new List<ExamSchedule>(), 0));
        _examScheduleService.DeactivateExpiredSchedulesAsync().Returns(Task.CompletedTask);

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_GET_ShouldReturnView()
    {
        _examScheduleService.GetSelectListDataAsync(Arg.Any<ExamSchedule?>())
            .Returns(new ExamScheduleSelectListsDto());

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_POST_ShouldRedirect_OnSuccess()
    {
        var examSchedule = new ExamSchedule();
        var selectLists = new ExamScheduleSelectListsDto
        {
            ExamTypes = [new SelectOption { Id = 1, Name = "Regular" }],
            Semesters = [new SelectOption { Id = 1, Name = "First" }]
        };
        _examScheduleService.GetSelectListDataAsync(Arg.Any<ExamSchedule?>()).Returns(selectLists);

        var result = await _controller.Create(examSchedule);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task DeleteConfirmed_POST_ShouldRedirect()
    {
        var result = await _controller.DeleteConfirmed(1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }
}
