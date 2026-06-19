using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FWU.Exam.Management.Web.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private static readonly string[] _staticExtensions =
        [".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot"];

    private static readonly TimeSpan _tenantCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        SlidingExpiration = _tenantCacheDuration,
        Priority = CacheItemPriority.High
    };

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext, IMemoryCache cache)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. URL already has tenant prefix — extract and proceed
        if (TryExtractTenant(path, out var tenantCode, out var remainingPath))
        {
            if (await TrySetTenantAsync(context, tenantContext, dbContext, cache, tenantCode!))
            {
                context.Items["TenantCode"] = tenantCode;
                context.Items["OriginalPath"] = path;
                context.Request.PathBase = $"/tenant/{tenantCode}";
                context.Request.Path = remainingPath;

                SetTenantCookie(context, tenantCode!);
            }
            else
            {
                context.Response.StatusCode = 404;
                return;
            }

            await next(context);
            return;
        }

        // 2. Static files — never require tenant
        if (IsStaticFile(path))
        {
            await next(context);
            return;
        }

        // 3. Tenant select page — always accessible without tenant
        if (IsTenantSelectPath(path))
        {
            await next(context);
            return;
        }

        // 4. Check for tenant cookie
        var cookieTenantCode = context.Request.Cookies["tenant_code"];

        // 5. If user has a tenant cookie, enforce tenant in URL
        if (!string.IsNullOrEmpty(cookieTenantCode))
        {
            if (await TrySetTenantAsync(context, tenantContext, dbContext, cache, cookieTenantCode))
            {
                context.Items["TenantCode"] = cookieTenantCode;

                // Redirect GET requests to tenant-prefixed URL so tenant is visible in address bar
                if (string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    var tenantPath = $"/tenant/{cookieTenantCode}{path}";
                    var queryString = context.Request.QueryString.Value;
                    context.Response.Redirect($"{tenantPath}{queryString}");
                    return;
                }

                // Non-GET requests keep the non-tenant URL but have tenant context set
                await next(context);
                return;
            }
            else
            {
                context.Response.Cookies.Delete("tenant_code");
                context.Response.Redirect("/TenantSelect/Index");
                return;
            }
        }

        // 6. Identity pages (login, register, forgot password) — allow without tenant
        if (IsIdentityPath(path))
        {
            await next(context);
            return;
        }

        // 7. No tenant, not a public path — redirect to tenant selection
        context.Response.Redirect("/TenantSelect/Index");
    }

    private static void SetTenantCookie(HttpContext context, string tenantCode)
    {
        context.Response.Cookies.Append("tenant_code", tenantCode, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            MaxAge = TimeSpan.FromHours(24)
        });
    }

    private static async Task<bool> TrySetTenantAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext, IMemoryCache cache, string tenantCode)
    {
        var cacheKey = $"tenant_{tenantCode}";
        if (!cache.TryGetValue(cacheKey, out Tenant? tenant) || tenant == null)
        {
            tenant = await dbContext.Set<Tenant>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OfficeCode == tenantCode);

            if (tenant != null)
                cache.Set(cacheKey, tenant, _cacheOptions);
        }

        if (tenant == null) return false;

        tenantContext.SetTenant(tenant.Id, tenant.OfficeCode, tenant.TenantType);
        return true;
    }

    private static bool TryExtractTenant(string path, out string? tenantCode, out string remainingPath)
    {
        tenantCode = null;
        remainingPath = path;

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 &&
            segments[0].Equals("tenant", StringComparison.OrdinalIgnoreCase))
        {
            tenantCode = segments[1];
            remainingPath = "/" + string.Join("/", segments.Skip(2));
            return true;
        }

        return false;
    }

    private static bool IsStaticFile(string path)
    {
        return _staticExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTenantSelectPath(string path)
    {
        var lower = path.TrimEnd('/').ToLowerInvariant();
        return lower is "" or "/" or "/tenantselect" or "/tenantselect/index";
    }

    private static bool IsIdentityPath(string path)
    {
        return path.StartsWith("/identity/", StringComparison.OrdinalIgnoreCase);
    }
}
