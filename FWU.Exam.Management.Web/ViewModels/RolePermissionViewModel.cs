using FWU.Exam.Management.Domain.Entities.Permissions;

namespace FWU.Exam.Management.Web.ViewModels;

public class RolePermissionViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionGroupViewModel> Groups { get; set; } = [];
}

public class PermissionGroupViewModel
{
    public string GroupName { get; set; } = string.Empty;
    public string GroupDisplayName { get; set; } = string.Empty;
    public List<PermissionItemViewModel> Permissions { get; set; } = [];
}

public class PermissionItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAssigned { get; set; }
}
