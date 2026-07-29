using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
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

public class RolePermissionManagerControllerTests
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly RolePermissionManagerController _controller;

    public RolePermissionManagerControllerTests()
    {
        var roleStore = Substitute.For<IRoleStore<IdentityRole>, IQueryableRoleStore<IdentityRole>>();
        var rolesList = new List<IdentityRole> { new IdentityRole("Admin") { Id = "1" } };
        ((IQueryableRoleStore<IdentityRole>)roleStore).Roles.Returns(rolesList.AsQueryable());
        _roleManager = Substitute.For<RoleManager<IdentityRole>>(roleStore, null, null, null, null);

        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        _permissionService = Substitute.For<IPermissionService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "test-user")], "test"));
        _controller = new RolePermissionManagerController(_roleManager, _userManager, _context, _permissionService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    [Fact]
    public async Task Edit_WithNullId_ShouldReturnNotFound()
    {
        _roleManager.FindByIdAsync(Arg.Is<string?>(s => s == null)).Returns((IdentityRole?)null);

        var result = await _controller.Edit(null!);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_WithInvalidId_ShouldReturnNotFound()
    {
        _roleManager.FindByIdAsync("invalid").Returns((IdentityRole?)null);

        var result = await _controller.Edit("invalid");

        result.Should().BeOfType<NotFoundResult>();
    }
}
