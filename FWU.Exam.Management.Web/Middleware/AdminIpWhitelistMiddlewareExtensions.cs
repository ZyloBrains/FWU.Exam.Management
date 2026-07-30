namespace FWU.Exam.Management.Web.Middleware;

public static class AdminIpWhitelistMiddlewareExtensions
{
    public static IApplicationBuilder UseAdminIpWhitelist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AdminIpWhitelistMiddleware>();
    }
}
