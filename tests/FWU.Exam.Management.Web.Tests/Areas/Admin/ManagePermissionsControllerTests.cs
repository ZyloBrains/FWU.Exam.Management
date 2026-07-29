using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Areas.Admin.Controllers;
using FWU.Exam.Management.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Admin;

public class ManagePermissionsControllerTests
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly ManagePermissionsController _controller;

    public ManagePermissionsControllerTests()
    {
        var roleStore = Substitute.For<IRoleStore<IdentityRole>, IQueryableRoleStore<IdentityRole>>();
        var rolesList = new List<IdentityRole> { new IdentityRole("Admin") { Id = "1" } };
        ((IQueryableRoleStore<IdentityRole>)roleStore).Roles.Returns(rolesList.AsQueryable());
        _roleManager = Substitute.For<RoleManager<IdentityRole>>(roleStore, null, null, null, null);

        _permissionService = Substitute.For<IPermissionService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new ManagePermissionsController(_roleManager, _context, _permissionService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Edit_WithInvalidId_ShouldReturnNotFound()
    {
        _roleManager.FindByIdAsync("invalid").Returns((IdentityRole?)null);

        var result = await _controller.Edit("invalid");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_WithValidId_ShouldReturnView()
    {
        var role = new IdentityRole("Admin") { Id = "role1" };
        _roleManager.FindByIdAsync("role1").Returns(role);
        _permissionService.GetAllPermissionsAsync().Returns(new List<Permission>());
        _permissionService.GetRolePermissionIdsAsync("role1").Returns(new List<int>());

        var result = await _controller.Edit("role1");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<RolePermissionViewModel>();
    }
}
