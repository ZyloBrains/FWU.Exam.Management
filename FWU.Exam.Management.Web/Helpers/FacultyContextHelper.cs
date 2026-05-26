using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Web.Helpers;

public static class FacultyContextHelper
{
    public static CurrentFaculty? GetCurrentFaculty(this HttpContext httpContext)
    {
        return httpContext.Items["CurrentFaculty"] as CurrentFaculty;
    }

    public static bool HasFacultyContext(this HttpContext httpContext)
    {
        return httpContext.Items["CurrentFaculty"] is CurrentFaculty;
    }
}
