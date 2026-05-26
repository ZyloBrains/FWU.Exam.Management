using FWU.Exam.Management.Application.Interfaces;

namespace FWU.Exam.Management.Web.Middleware;

public class FacultyResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IFacultyResolver resolver)
    {
        var hostname = context.Request.Host.Host;
        var faculty = await resolver.ResolveFacultyAsync(hostname);
        if (faculty != null)
        {
            context.Items["CurrentFaculty"] = faculty;
        }
        await next(context);
    }
}
