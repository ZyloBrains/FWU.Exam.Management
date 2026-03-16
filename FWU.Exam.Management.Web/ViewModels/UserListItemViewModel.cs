namespace fwu_examination_management_system.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? OrganizationName { get; set; }
        public IList<string> Roles { get; set; } = [];
    }
}
