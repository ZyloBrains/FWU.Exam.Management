namespace FWU.Exam.Management.Domain.Entities.Permissions;

public class RolePermission
{
    public string RoleId { get; set; } = string.Empty;
    public int PermissionId { get; set; }

    public virtual Permission Permission { get; set; } = null!;
}
