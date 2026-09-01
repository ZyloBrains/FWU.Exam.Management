namespace FWU.Exam.Management.Domain.Constants;

public static class RoleRules
{
    /// <summary>
    /// Maps a user's roles to the highest built-in admin role they hold
    /// (one of Role.SuperAdmin, Role.FacultyAdmin, Role.CollegeAdmin).
    /// Returns null when the caller holds none of the built-in admin roles —
    /// such callers are governed by the permission-subset rule instead.
    /// </summary>
    public static string? FromRoles(IEnumerable<string> userRoles)
    {
        ArgumentNullException.ThrowIfNull(userRoles);
        var roles = userRoles as ICollection<string> ?? userRoles.ToList();
        if (roles.Contains(Role.SuperAdmin)) return Role.SuperAdmin;
        if (roles.Contains(Role.FacultyAdmin)) return Role.FacultyAdmin;
        if (roles.Contains(Role.CollegeAdmin)) return Role.CollegeAdmin;
        return null;
    }

    /// <summary>
    /// Roles the caller may assign to (or remove from) other users.
    /// SuperAdmin manages everything; FacultyAdmin manages CollegeAdmin + Student;
    /// CollegeAdmin manages Student only.
    /// </summary>
    public static IReadOnlySet<string> AssignableRoles(string callerRole)
        => callerRole switch
        {
            Role.SuperAdmin => [.. Role.AllRoles],
            Role.CollegeAdmin => [Role.Student],
            Role.FacultyAdmin => new HashSet<string> { Role.CollegeAdmin, Role.Student },
            _ => throw new ArgumentOutOfRangeException(nameof(callerRole), callerRole, "Not a built-in admin role."),
        };

    public static bool IsAssignableRole(string callerRole, string? role)
        => !string.IsNullOrEmpty(role) && AssignableRoles(callerRole).Contains(role);

    /// <summary>
    /// Whether the caller may manage (edit/delete/toggle/assign roles to) a user holding the given roles.
    /// SuperAdmin can manage anyone; FacultyAdmin cannot manage SuperAdmin/FacultyAdmin;
    /// CollegeAdmin can only manage Student-role users.
    /// </summary>
    public static bool CanManageTarget(string callerRole, IEnumerable<string> targetRoles)
    {
        ArgumentNullException.ThrowIfNull(targetRoles);
        var roles = targetRoles as ICollection<string> ?? [.. targetRoles];
        return callerRole switch
        {
            Role.SuperAdmin => true,
            Role.CollegeAdmin => roles.All(r => r == Role.Student),
            Role.FacultyAdmin => !roles.Contains(Role.SuperAdmin) && !roles.Contains(Role.FacultyAdmin),
            _ => throw new ArgumentOutOfRangeException(nameof(callerRole), callerRole, "Not a built-in admin role."),
        };
    }
}
