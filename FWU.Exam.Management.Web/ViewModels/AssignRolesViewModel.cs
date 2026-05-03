namespace FWU.Exam.Management.Web.ViewModels;

public class AssignRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<RoleAssignmentItem> Roles { get; set; } = [];
}

public class RoleAssignmentItem
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}
