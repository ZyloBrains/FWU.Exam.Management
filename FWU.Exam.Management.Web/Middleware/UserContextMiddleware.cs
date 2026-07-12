using System.Security.Claims;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                var userContext = context.RequestServices.GetRequiredService<IUserContext>();

                var user = await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);

                    List<int> facultyCollegeIds = [];
                    if (user.FacultyId.HasValue)
                    {
                        facultyCollegeIds = await dbContext.Colleges
                            .Where(c => c.Faculties!.Any(f => f.Id == user.FacultyId))
                            .Select(c => c.Id)
                            .ToListAsync();
                    }

                    userContext.SetUser(
                        userId: user.Id,
                        facultyId: user.FacultyId,
                        collegeId: user.CollegeId,
                        facultyCollegeIds: facultyCollegeIds,
                        roles: (IReadOnlyList<string>)roles);
                }
            }
        }

        await next(context);
    }
}
