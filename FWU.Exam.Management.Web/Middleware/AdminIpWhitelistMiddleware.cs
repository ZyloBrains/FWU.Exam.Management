using System.Net;

namespace FWU.Exam.Management.Web.Middleware;

public class AdminIpWhitelistMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly HashSet<IPAddress> _allowedIps = configuration
        .GetSection("AdminIpWhitelist:AllowedIps")
        .Get<string[]>()
        ?.Select(IPAddress.Parse)
        .ToHashSet() ?? [];

    private static readonly string[] AdminPaths =
    [
        "/Admin",
        "/Core",
        "/Colleges",
        "/Exams/CollegeAdmin",
        "/Payments",
        "/Reports",
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (_allowedIps.Count != 0)
        {
            var path = context.Request.Path.Value?.TrimEnd('/') ?? "";
            var isAdminPath = AdminPaths.Any(p =>
                path.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));

            if (isAdminPath)
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                if (remoteIp == null || !_allowedIps.Contains(remoteIp))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Access denied. Your IP is not authorized to access this resource.");
                    return;
                }
            }
        }

        await next(context);
    }
}
