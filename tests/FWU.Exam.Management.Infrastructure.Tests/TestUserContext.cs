using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class TestUserContext : IUserContext
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

    public void SetUser(string? userId, int? facultyId, int? collegeId, IReadOnlyList<int> facultyCollegeIds, IReadOnlyList<string> roles)
    {
        UserId = userId;
        FacultyId = facultyId;
        CollegeId = collegeId;
        FacultyCollegeIds = facultyCollegeIds;
        Roles = roles;
        IsAuthenticated = !string.IsNullOrEmpty(userId);
        IsSuperAdmin = roles.Contains("SuperAdmin");
        IsFacultyAdmin = roles.Contains("FacultyAdmin");
        IsCollegeAdmin = roles.Contains("CollegeAdmin");
    }
}
