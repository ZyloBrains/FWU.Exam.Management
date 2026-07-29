using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace FWU.Exam.Management.Web.Tests.Middleware;

public class FacultyResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHostnameResolvesToFaculty_ShouldSetFacultyInContext()
    {
        var resolver = Substitute.For<IFacultyResolver>();
        resolver.ResolveFacultyAsync(Arg.Any<string>()).Returns(new CurrentFaculty
        {
            Id = 1,
            Name = "Engineering",
            OfficeCode = "ENG",
            LogoPath = "/logos/eng.png"
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("engineering.example.com");

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new FacultyResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, resolver);

        var faculty = httpContext.Items["CurrentFaculty"] as CurrentFaculty;
        faculty.Should().NotBeNull();
        faculty!.Id.Should().Be(1);
        faculty.Name.Should().Be("Engineering");
        faculty.OfficeCode.Should().Be("ENG");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenResolverReturnsNullAndUserUnauthenticated_ShouldNotSetFaculty()
    {
        var resolver = Substitute.For<IFacultyResolver>();
        resolver.ResolveFacultyAsync(Arg.Any<string>()).Returns((CurrentFaculty?)null);

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };
        httpContext.Request.Host = new HostString("unknown.example.com");

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new FacultyResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, resolver);

        httpContext.Items.Should().NotContainKey("CurrentFaculty");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        var resolver = Substitute.For<IFacultyResolver>();
        resolver.ResolveFacultyAsync(Arg.Any<string>()).Returns((CurrentFaculty?)null);

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new FacultyResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, resolver);

        nextCalled.Should().BeTrue();
    }
}
