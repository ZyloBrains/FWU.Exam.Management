using FWU.Exam.Management.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace FWU.Exam.Management.Web.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldAddSecurityHeaders()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var env = new TestHostEnvironment(environmentName: "Production");
        var middleware = new SecurityHeadersMiddleware(next, env);

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        httpContext.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");

        httpContext.Response.Headers.Should().ContainKey("X-Frame-Options");
        httpContext.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");

        httpContext.Response.Headers.Should().ContainKey("X-XSS-Protection");
        httpContext.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");

        httpContext.Response.Headers.Should().ContainKey("Referrer-Policy");
        httpContext.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");

        httpContext.Response.Headers.Should().ContainKey("Permissions-Policy");
        httpContext.Response.Headers["Permissions-Policy"].ToString().Should().Contain("camera=()");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCSP_WhenNotDevelopment()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var env = new TestHostEnvironment(environmentName: "Production");
        var middleware = new SecurityHeadersMiddleware(next, env);

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = httpContext.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("strict-dynamic");
        csp.Should().Contain("nonce-");
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotAddCSP_WhenInDevelopment()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
        var env = new TestHostEnvironment(environmentName: "Development");
        var middleware = new SecurityHeadersMiddleware(next, env);

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Headers.Should().NotContainKey("Content-Security-Policy");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var env = new TestHostEnvironment(environmentName: "Production");
        var middleware = new SecurityHeadersMiddleware(next, env);

        await middleware.InvokeAsync(httpContext);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Headers_ShouldBeSet_BeforeNextDelegate()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
        {
            ctx.Response.Headers.Should().ContainKey("X-Content-Type-Options");
            return Task.CompletedTask;
        };

        var env = new TestHostEnvironment(environmentName: "Production");
        var middleware = new SecurityHeadersMiddleware(next, env);

        await middleware.InvokeAsync(httpContext);
    }
}

public class TestHostEnvironment(string environmentName) : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "Test";
    public string WebRootPath { get; set; } = ".";
    public string ContentRootPath { get; set; } = ".";
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
