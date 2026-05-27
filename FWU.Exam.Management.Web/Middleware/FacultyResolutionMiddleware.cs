using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                var user = await dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.Faculty)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.FacultyId != null);

                if (user?.Faculty != null)
                {
                    context.Items["CurrentFaculty"] = new Application.DTOs.CurrentFaculty
                    {
                        Id = user.Faculty.Id,
                        Name = user.Faculty.Name,
                        OfficeCode = user.Faculty.OfficeCode,
                        LogoPath = user.Faculty.LogoPath
                    };
                }
            }
        }

        await next(context);
    }
}
