using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace FWU.Exam.Management.Web.Tests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantCodeInUrl_ShouldSetTenantContext()
    {
        using var dbContext = CreateDbContext("TenantValidUrl");
        dbContext.Set<Tenant>().Add(new Tenant
        {
            Id = 1,
            OfficeCode = "TENANT001",
            TenantType = TenantType.Standard,
            Name = "Test Tenant"
        });
        await dbContext.SaveChangesAsync();

        var tenantContext = new TenantContext();
        var cache = Substitute.For<IMemoryCache>();
        object? cacheResult = null;
        cache.TryGetValue(Arg.Any<object>(), out cacheResult).Returns(false);
        var cacheEntry = Substitute.For<ICacheEntry>();
        cache.CreateEntry(Arg.Any<object>()).Returns(cacheEntry);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/tenant/TENANT001/dashboard";
        httpContext.Request.Method = "GET";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        tenantContext.IsResolved.Should().BeTrue();
        tenantContext.TenantCode.Should().Be("TENANT001");
        tenantContext.TenantId.Should().Be(1);
        httpContext.Items["TenantCode"].Should().Be("TENANT001");
        httpContext.Request.PathBase.Value.Should().Be("/tenant/TENANT001");
        httpContext.Request.Path.Value.Should().Be("/dashboard");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidTenantCode_ShouldReturn404()
    {
        using var dbContext = CreateDbContext("TenantInvalid");

        var tenantContext = Substitute.For<ITenantContext>();
        var cache = Substitute.For<IMemoryCache>();
        object? cacheResult = null;
        cache.TryGetValue(Arg.Any<object>(), out cacheResult).Returns(false);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/tenant/INVALID/dashboard";
        httpContext.Request.Method = "GET";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        httpContext.Response.StatusCode.Should().Be(404);
        nextCalled.Should().BeFalse();
        tenantContext.DidNotReceive().SetTenant(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TenantType>());
    }

    [Fact]
    public async Task InvokeAsync_WithStaticFilePath_ShouldSkipTenantResolution()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        var cache = Substitute.For<IMemoryCache>();
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var dbContext = Substitute.For<AppDbContext>(options, null, null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/style.css";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        nextCalled.Should().BeTrue();
        tenantContext.DidNotReceive().SetTenant(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TenantType>());
    }

    [Fact]
    public async Task InvokeAsync_WithIdentityPath_ShouldSkipTenantResolution()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        var cache = Substitute.For<IMemoryCache>();
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var dbContext = Substitute.For<AppDbContext>(options, null, null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/Identity/Account/Login";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        nextCalled.Should().BeTrue();
        tenantContext.DidNotReceive().SetTenant(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TenantType>());
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantCookieAndGetRequest_ShouldRedirect()
    {
        using var dbContext = CreateDbContext("TenantCookieGet");
        dbContext.Set<Tenant>().Add(new Tenant
        {
            Id = 2,
            OfficeCode = "TENANT002",
            TenantType = TenantType.Standard,
            Name = "Test Tenant"
        });
        await dbContext.SaveChangesAsync();

        var tenantContext = new TenantContext();
        var cache = Substitute.For<IMemoryCache>();
        object? cacheResult = null;
        cache.TryGetValue(Arg.Any<object>(), out cacheResult).Returns(false);
        var cacheEntry = Substitute.For<ICacheEntry>();
        cache.CreateEntry(Arg.Any<object>()).Returns(cacheEntry);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/dashboard";
        httpContext.Request.Method = "GET";
        httpContext.Request.Headers["Cookie"] = "tenant_code=TENANT002";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        httpContext.Response.StatusCode.Should().Be(302);
        httpContext.Response.Headers["Location"].ToString().Should().Be("/tenant/TENANT002/dashboard");
        tenantContext.IsResolved.Should().BeTrue();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantCookieAndNonGetRequest_ShouldSetTenantWithoutRedirect()
    {
        using var dbContext = CreateDbContext("TenantCookieNonGet");
        dbContext.Set<Tenant>().Add(new Tenant
        {
            Id = 3,
            OfficeCode = "TENANT003",
            TenantType = TenantType.Standard,
            Name = "Test Tenant"
        });
        await dbContext.SaveChangesAsync();

        var tenantContext = new TenantContext();
        var cache = Substitute.For<IMemoryCache>();
        object? cacheResult = null;
        cache.TryGetValue(Arg.Any<object>(), out cacheResult).Returns(false);
        var cacheEntry = Substitute.For<ICacheEntry>();
        cache.CreateEntry(Arg.Any<object>()).Returns(cacheEntry);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/dashboard";
        httpContext.Request.Method = "POST";
        httpContext.Request.Headers["Cookie"] = "tenant_code=TENANT003";

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        tenantContext.IsResolved.Should().BeTrue();
        tenantContext.TenantCode.Should().Be("TENANT003");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithoutTenantAndNoCookie_ShouldRedirectToLogin()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        var cache = Substitute.For<IMemoryCache>();
        object? cacheResult = null;
        cache.TryGetValue(Arg.Any<object>(), out cacheResult).Returns(false);
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var dbContext = Substitute.For<AppDbContext>(options, null, null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/dashboard";
        httpContext.Request.Method = "GET";

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next);

        await middleware.InvokeAsync(httpContext, tenantContext, dbContext, cache);

        httpContext.Response.StatusCode.Should().Be(302);
        httpContext.Response.Headers["Location"].ToString().Should().Be("/Identity/Account/Login");
        tenantContext.DidNotReceive().SetTenant(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TenantType>());
    }
}
