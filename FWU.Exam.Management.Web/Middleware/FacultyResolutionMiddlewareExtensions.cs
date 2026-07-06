namespace FWU.Exam.Management.Web.Middleware;

public static class FacultyResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseFacultyResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FacultyResolutionMiddleware>();
    }
}
