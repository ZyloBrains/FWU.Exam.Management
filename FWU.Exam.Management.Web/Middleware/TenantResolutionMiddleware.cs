using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private static readonly string[] _staticExtensions =
        [".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot"];

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? "";
        var tenantCode = context.GetRouteValue("tenantCode") as string;

        if (!string.IsNullOrEmpty(tenantCode))
        {
            var tenant = await dbContext.Set<Tenant>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OfficeCode == tenantCode);

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            tenantContext.SetTenant(tenant.Id, tenant.OfficeCode, tenant.TenantType);
        }
        else if (!IsPublicPath(path))
        {
            context.Response.Redirect("/TenantSelect/Index");
            return;
        }

        await next(context);
    }

    private static bool IsPublicPath(string path)
    {
        if (_staticExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
            return true;

        var lower = path.TrimEnd('/').ToLowerInvariant();
        return lower is "" or "/" or "/tenantselect" or "/tenantselect/index";
    }
}
