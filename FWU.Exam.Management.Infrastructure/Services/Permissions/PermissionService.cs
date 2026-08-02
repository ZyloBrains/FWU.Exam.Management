using System.Collections.Concurrent;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FWU.Exam.Management.Infrastructure.Services.Permissions;

public class PermissionService(AppDbContext context, IMemoryCache cache) : IPermissionService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly ConcurrentDictionary<string, object> _locks = new();

    private static string UserCacheKey(string userId) => $"user_permissions_{userId}";

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = UserCacheKey(userId);
        if (cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
            return cached;

        var lockObj = _locks.GetOrAdd(cacheKey, _ => new object());
        lock (lockObj)
        {
            if (cache.TryGetValue(cacheKey, out cached) && cached != null)
                return cached;

            var roleIds = context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToList();

            var permissionNames = context.Set<Domain.Entities.Permissions.RolePermission>()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            cache.Set(cacheKey, permissionNames, CacheDuration);
            return permissionNames;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, string group)
    {
        var all = await GetUserPermissionsAsync(userId);
        return all.Where(p => p.StartsWith($"{group}.")).ToList();
    }

    public async Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions)
    {
        var userPerms = await GetUserPermissionsAsync(userId);
        return permissions.Any(p => userPerms.Contains(p));
    }

    public Task InvalidateCacheAsync(string userId)
    {
        cache.Remove(UserCacheKey(userId));
        return Task.CompletedTask;
    }

    public async Task<List<(string RoleId, string RoleName)>> GetAllRolesWithPermissionAsync(string permission)
    {
        var perm = await context.Permissions!.FirstOrDefaultAsync(p => p.Name == permission);
        if (perm == null) return [];

        var result = await context.RolePermissions!
            .Where(rp => rp.PermissionId == perm.Id)
            .Join(context.Roles, rp => rp.RoleId, r => r.Id, (rp, r) => new { r.Id, r.Name })
            .ToListAsync();

        return result.Select(x => (x.Id, x.Name ?? "")).ToList();
    }

    public async Task UpdateRolePermissionsAsync(string roleId, List<int> permissionIds)
    {
        var existing = await context.RolePermissions!
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        context.RolePermissions!.RemoveRange(existing);

        var newPermissions = permissionIds.Select(pid => new Domain.Entities.Permissions.RolePermission
        {
            RoleId = roleId,
            PermissionId = pid
        });

        await context.RolePermissions!.AddRangeAsync(newPermissions);
        await context.SaveChangesAsync();

        var userIds = await context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        foreach (var uid in userIds)
            cache.Remove(UserCacheKey(uid));
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await context.Permissions!
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<int>> GetRolePermissionIdsAsync(string roleId)
    {
        return await context.RolePermissions!
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
    }
}
