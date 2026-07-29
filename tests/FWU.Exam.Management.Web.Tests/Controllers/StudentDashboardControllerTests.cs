using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Areas.Students.Controllers;
using FWU.Exam.Management.Web.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Claims;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class StudentDashboardControllerTests
{
    private readonly IStudentDashboardService _dashboardService = Substitute.For<IStudentDashboardService>();
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IESewaService _esewaService = Substitute.For<IESewaService>();
    private readonly IKhaltiService _khaltiService = Substitute.For<IKhaltiService>();
    private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
    private readonly ILogger<StudentDashboardController> _logger = Substitute.For<ILogger<StudentDashboardController>>();
    private readonly IFileUploadHelper _fileUploadHelper = Substitute.For<IFileUploadHelper>();
    private readonly IRetotalRequestService _retotalRequestService = Substitute.For<IRetotalRequestService>();
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env = Substitute.For<IWebHostEnvironment>();
    private readonly StudentDashboardController _controller;

    public StudentDashboardControllerTests()
    {
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        _signInManager = Substitute.For<SignInManager<AppUser>>(
            _userManager, Substitute.For<IHttpContextAccessor>(), Substitute.For<IUserClaimsPrincipalFactory<AppUser>>(),
            null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new StudentDashboardController(
            _dashboardService, _userManager, _signInManager, _emailSender,
            _esewaService, _khaltiService, _configuration, _logger,
            _fileUploadHelper, _retotalRequestService, _context, _env)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Profile_ShouldReturnView()
    {
        var user = new AppUser { Id = "test-id", UserName = "testuser", Email = "test@example.com" };
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

        var result = await _controller.Profile();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task ExamForms_ShouldReturnView()
    {
        var user = new AppUser { Id = "test-id", UserName = "testuser", Email = "test@example.com" };
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

        var result = await _controller.ExamForms();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Marksheet_ShouldReturnView()
    {
        var user = new AppUser { Id = "test-id", UserName = "testuser", Email = "test@example.com" };
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

        var result = await _controller.Marksheet();

        result.Should().BeOfType<ViewResult>();
    }
}
