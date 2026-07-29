using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Areas.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Admin;

public class CollegeAdminAssignmentsControllerTests
{
    private readonly ICollegeAdminSubjectAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserContext _userContext;
    private readonly AppDbContext _context;
    private readonly CollegeAdminAssignmentsController _controller;

    public CollegeAdminAssignmentsControllerTests()
    {
        _assignmentService = Substitute.For<ICollegeAdminSubjectAssignmentService>();
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        _userContext = Substitute.For<IUserContext>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new CollegeAdminAssignmentsController(_assignmentService, _userManager, _userContext, _context)
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
        _assignmentService.GetAssignmentsAsync().Returns(new List<CollegeAdminSubjectAssignment>());

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_WithInvalidId_ShouldReturnNotFound()
    {
        _assignmentService.GetByIdAsync(999).Returns((CollegeAdminSubjectAssignment?)null);

        var result = await _controller.Edit(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
