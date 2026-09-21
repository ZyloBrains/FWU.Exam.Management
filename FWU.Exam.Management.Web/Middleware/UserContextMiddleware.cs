using System.Security.Claims;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FWU.Exam.Management.Web.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    private static readonly TimeSpan _userContextCacheDuration = TimeSpan.FromMinutes(2);
    private static readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        SlidingExpiration = _userContextCacheDuration,
        Priority = CacheItemPriority.Normal
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                var userContext = context.RequestServices.GetRequiredService<IUserContext>();
                var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();

                var cacheKey = CacheKeys.UserContext(userId);
                if (!cache.TryGetValue(cacheKey, out CachedUserContext? cached) || cached == null)
                {
                    cached = await LoadUserContextAsync(userManager, dbContext, userId);
                    if (cached != null)
                        cache.Set(cacheKey, cached, _cacheOptions);
                }

                if (cached != null)
                {
                    userContext.SetUser(
                        userId: cached.UserId,
                        facultyId: cached.FacultyId,
                        collegeId: cached.CollegeId,
                        facultyCollegeIds: cached.FacultyCollegeIds,
                        roles: cached.Roles);

                    tenantContext.SetCollegeAdmin(cached.IsCollegeAdmin, cached.CollegeId, cached.CollegeTenantIds);
                }
            }
        }

        await next(context);
    }

    private static async Task<CachedUserContext?> LoadUserContextAsync(UserManager<AppUser> userManager, AppDbContext dbContext, string userId)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var roles = await userManager.GetRolesAsync(user);

        List<int> facultyCollegeIds = [];
        if (user.FacultyId.HasValue)
        {
            facultyCollegeIds = await dbContext.CollegeFaculties
                .Where(cf => cf.FacultyId == user.FacultyId)
                .Select(cf => cf.CollegeId)
                .Distinct()
                .ToListAsync();
        }

        var isCollegeAdmin = roles.Contains(Role.CollegeAdmin);
        List<int> collegeTenantIds = [];
        if (isCollegeAdmin && user.CollegeId.HasValue)
        {
            collegeTenantIds = await dbContext.CollegeFaculties
                .Where(cf => cf.CollegeId == user.CollegeId.Value)
                .Select(cf => cf.TenantId)
                .Distinct()
                .ToListAsync();
        }

        return new CachedUserContext(
            user.Id,
            user.FacultyId,
            user.CollegeId,
            facultyCollegeIds,
            (IReadOnlyList<string>)roles,
            isCollegeAdmin,
            collegeTenantIds);
    }

    private sealed record CachedUserContext(
        string UserId,
        int? FacultyId,
        int? CollegeId,
        List<int> FacultyCollegeIds,
        IReadOnlyList<string> Roles,
        bool IsCollegeAdmin,
        List<int> CollegeTenantIds);
}