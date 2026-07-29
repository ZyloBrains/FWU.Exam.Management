using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Authorization;

public class PermissionHandlerTests
{
    private readonly IPermissionService _permissionService;
    private readonly UserManager<AppUser> _userManager;
    private readonly PermissionHandler _handler;

    public PermissionHandlerTests()
    {
        _permissionService = Substitute.For<IPermissionService>();

        var userStore = Substitute.For<IUserStore<AppUser>>();
        _userManager = Substitute.For<UserManager<AppUser>>(userStore, null, null, null, null, null, null, null, null);
        _userManager.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns("user-123");

        _handler = new PermissionHandler(_permissionService, _userManager);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserNotAuthenticated()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new PermissionRequirement("ViewStudents");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenUserIsSuperAdmin()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "super-admin"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        ], "test"));

        var requirement = new PermissionRequirement("AnyPermission");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldSucceed_WhenUserHasPermission()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "Test User")
        ], "test"));

        _permissionService.HasPermissionAsync("user-123", "ViewStudents").Returns(true);

        var requirement = new PermissionRequirement("ViewStudents");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserDoesNotHavePermission()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "Test User")
        ], "test"));

        _permissionService.HasPermissionAsync("user-123", "DeleteStudents").Returns(false);

        var requirement = new PermissionRequirement("DeleteStudents");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUserIdIsEmpty()
    {
        _userManager.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns((string?)null);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Name, "No Id User")
        ], "test"));

        var requirement = new PermissionRequirement("ViewStudents");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}

public class RequirePermissionAttributeTests
{
    [Fact]
    public void CreateAttribute_ShouldSetPolicy()
    {
        var attribute = new RequirePermissionAttribute("ViewStudents");

        attribute.Permission.Should().Be("ViewStudents");
        attribute.Policy.Should().Be("Permission_ViewStudents");
    }

    [Fact]
    public void PolicyPrefix_ShouldBeCorrect()
    {
        RequirePermissionAttribute.PolicyPrefix.Should().Be("Permission_");
    }
}

public class PermissionPolicyProviderTests
{
    [Fact]
    public async Task GetPolicyAsync_ShouldCreatePolicy_ForPermissionPrefix()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions());
        var provider = new PermissionPolicyProvider(options);

        var policy = await provider.GetPolicyAsync("Permission_ViewStudents");

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionRequirement>()
            .Which.Permission.Should().Be("ViewStudents");
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldFallback_ForNonPermissionPolicy()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions());
        var provider = new PermissionPolicyProvider(options);

        var policy = await provider.GetPolicyAsync("SomeOtherPolicy");

        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ShouldReturnDefault()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions());
        var provider = new PermissionPolicyProvider(options);

        var policy = await provider.GetDefaultPolicyAsync();

        policy.Should().NotBeNull();
    }
}
