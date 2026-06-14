using FWU.Exam.Management.Domain.Entities.Permissions;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<string>> GetUserPermissionsAsync(string userId, string group);
    Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions);
    Task InvalidateCacheAsync(string userId);
    Task<List<(string RoleId, string RoleName)>> GetAllRolesWithPermissionAsync(string permission);
    Task UpdateRolePermissionsAsync(string roleId, List<int> permissionIds);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<List<int>> GetRolePermissionIdsAsync(string roleId);
}
