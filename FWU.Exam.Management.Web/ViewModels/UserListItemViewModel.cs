namespace FWU.Exam.Management.Web.ViewModels;

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public IList<string> Roles { get; set; } = [];
}
