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

        if (TryExtractTenant(path, out var tenantCode, out var remainingPath))
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

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            tenantContext.SetTenant(tenant.Id, tenant.OfficeCode, tenant.TenantType);
            context.Items["TenantCode"] = tenantCode;
            context.Items["OriginalPath"] = path;
            context.Request.PathBase = $"/tenant/{tenantCode}";
            context.Request.Path = remainingPath;
        }
        else if (!IsPublicPath(path))
        {
            context.Response.Redirect("/TenantSelect/Index");
            return;
        }

        await next(context);
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

    private static bool IsPublicPath(string path)
    {
        if (_staticExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
            return true;

        var lower = path.TrimEnd('/').ToLowerInvariant();
        return lower is "" or "/" or "/tenantselect" or "/tenantselect/index";
    }
}
