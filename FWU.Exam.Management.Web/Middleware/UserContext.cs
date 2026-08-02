using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Web.Middleware;

public class UserContext : IUserContext
{
    public string? UserId { get; private set; }
    public int? FacultyId { get; private set; }
    public int? CollegeId { get; private set; }
    public IReadOnlyList<int> FacultyCollegeIds { get; private set; } = [];
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public bool IsSuperAdmin { get; private set; }
    public bool IsFacultyAdmin { get; private set; }
    public bool IsCollegeAdmin { get; private set; }
    public bool IsAuthenticated { get; private set; }

    public void SetUser(
        string? userId,
        int? facultyId,
        int? collegeId,
        IReadOnlyList<int> facultyCollegeIds,
        IReadOnlyList<string> roles)
    {
        UserId = userId;
        FacultyId = facultyId;
        CollegeId = collegeId;
        FacultyCollegeIds = facultyCollegeIds ?? [];
        Roles = roles ?? [];
        IsAuthenticated = userId != null;
        IsSuperAdmin = roles?.Contains(Role.SuperAdmin) ?? false;
        IsFacultyAdmin = roles?.Contains(Role.FacultyAdmin) ?? false;
        IsCollegeAdmin = roles?.Contains(Role.CollegeAdmin) ?? false;
    }
}
