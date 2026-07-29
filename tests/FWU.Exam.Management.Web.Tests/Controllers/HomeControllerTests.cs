using System.Security.Claims;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void Index_ShouldRedirectToLogin_WhenNotAuthenticated()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = controller.Index();

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/Identity/Account/Login");
    }

    [Fact]
    public void Index_ShouldRedirectToDashboard_WhenAuthenticated()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "Test User")
        ], "test"));

        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

        var result = controller.Index();

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Dashboard");
    }

    [Fact]
    public void Privacy_ShouldReturnView()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = controller.Privacy();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ShouldReturnView_WithErrorViewModel()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = controller.Error();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ErrorViewModel>();
    }

    [Fact]
    public void Entrance_ShouldRedirectToVerifyPayment()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = controller.Entrance();

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("VerifyPayment");
        redirectResult.ControllerName.Should().Be("Entrance");
    }
}
