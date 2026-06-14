using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Permissions;

public class Permission
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Group { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
