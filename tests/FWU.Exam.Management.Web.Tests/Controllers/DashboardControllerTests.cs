using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Controllers;
using FWU.Exam.Management.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly IDashboardService _dashboardService = Substitute.For<IDashboardService>();
    private readonly IStudentDashboardService _studentDashboardService = Substitute.For<IStudentDashboardService>();
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new DashboardController(_dashboardService, _studentDashboardService, _userManager, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(), Substitute.For<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView_WithDashboardViewModel()
    {
        var user = new AppUser { Id = "test-id", UserName = "testuser", Email = "test@example.com", CollegeId = 1 };
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "Admin" });
        _dashboardService.GetCollegeDashboardStatsAsync(Arg.Any<int>()).Returns(new DashboardStats());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<DashboardViewModel>();
    }

    [Fact]
    public async Task Index_ShouldReturnChallenge_WhenUserNotFound()
    {
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns((AppUser?)null);

        var result = await _controller.Index();

        result.Should().BeOfType<ChallengeResult>();
    }
}
