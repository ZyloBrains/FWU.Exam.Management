using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
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

public class CollegeAdminMarksControllerTests
{
    private readonly ICollegeAdminMarksService _collegeAdminMarksService;
    private readonly ICollegeAdminSubjectAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly CollegeAdminMarksController _controller;

    public CollegeAdminMarksControllerTests()
    {
        _collegeAdminMarksService = Substitute.For<ICollegeAdminMarksService>();
        _assignmentService = Substitute.For<ICollegeAdminSubjectAssignmentService>();
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new CollegeAdminMarksController(_collegeAdminMarksService, _assignmentService, _userManager, _context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };
    }

    [Fact]
    public async Task Dashboard_ShouldReturnView()
    {
        var user = new AppUser { Id = "test-user", UserName = "test" };
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _collegeAdminMarksService.GetCollegeAdminDashboardAsync(user.Id).Returns(new CollegeAdminDashboardDto());

        var result = await _controller.Dashboard();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Dashboard_WhenUserNull_ShouldReturnChallenge()
    {
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns((AppUser?)null);

        var result = await _controller.Dashboard();

        result.Should().BeOfType<ChallengeResult>();
    }

    [Fact]
    public async Task Import_Get_ShouldReturnView()
    {
        var result = await _controller.Import(1, 2);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData["SubjectOfferingId"].Should().Be(1);
        viewResult.ViewData["ExamScheduleId"].Should().Be(2);
    }


}
