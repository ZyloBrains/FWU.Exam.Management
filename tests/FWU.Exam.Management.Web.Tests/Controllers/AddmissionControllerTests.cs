using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Areas.Students.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class AddmissionControllerTests
{
    private readonly IStudentAdmissionService _admissionService = Substitute.For<IStudentAdmissionService>();
    private readonly IStudentRegistrationService _studentService = Substitute.For<IStudentRegistrationService>();
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly StudentAdmissionsController _controller;

    public AddmissionControllerTests()
    {
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new StudentAdmissionsController(_admissionService, _studentService, _userManager, _context, _userContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        _admissionService.GetAdmissionsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((new List<StudentAdmission>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_GET_ShouldReturnView()
    {
        _userContext.IsSuperAdmin.Returns(true);
        _admissionService.GetCollegeSelectListAsync().Returns([]);

        var result = await _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_POST_ShouldRedirect_OnSuccess()
    {
        var admission = new StudentAdmission();

        var result = await _controller.Create(admission, null);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Edit_GET_ShouldReturnView_WhenExists()
    {
        _admissionService.GetAdmissionByIdAsync(1).Returns(new StudentAdmission());
        _admissionService.GetCollegeSelectListAsync().Returns([]);
        _admissionService.GetCollegeProgramsAsync(Arg.Any<int>()).Returns([]);

        var result = await _controller.Edit(1);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task DeleteConfirmed_POST_ShouldRedirect()
    {
        var result = await _controller.DeleteConfirmed(1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }
}
