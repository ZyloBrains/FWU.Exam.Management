namespace FWU.Exam.Management.Domain.Interfaces;

public interface IUserContext
{
    string? UserId { get; }
    int? FacultyId { get; }
    int? CollegeId { get; }
    IReadOnlyList<int> FacultyCollegeIds { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsSuperAdmin { get; }
    bool IsFacultyAdmin { get; }
    bool IsCollegeAdmin { get; }
    bool IsAuthenticated { get; }

    void SetUser(string? userId, int? facultyId, int? collegeId, IReadOnlyList<int> facultyCollegeIds, IReadOnlyList<string> roles);
}
