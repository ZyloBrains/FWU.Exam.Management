using System.Security.Claims;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext)
    {
        string? tenantCode = null;

        // 1. Try route: /tenant/{tenantCode}/...
        var path = context.Request.Path.Value;
        if (path != null)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals("tenant", StringComparison.OrdinalIgnoreCase))
                {
                    tenantCode = segments[i + 1];
                    break;
                }
            }
        }

        // 2. Try user claim as fallback (authenticated users have TenantId claim or org association)
        if (string.IsNullOrEmpty(tenantCode) && context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = await dbContext.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user?.Tenant != null)
                {
                    tenantCode = user.Tenant.OfficeCode;
                }
            }
        }

        if (!string.IsNullOrEmpty(tenantCode))
        {
            var tenant = await dbContext.Set<Tenant>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OfficeCode == tenantCode);

            if (tenant != null)
            {
                tenantContext.SetTenant(tenant.Id, tenant.OfficeCode, tenant.TenantType);
            }
        }

        await next(context);
    }
}
