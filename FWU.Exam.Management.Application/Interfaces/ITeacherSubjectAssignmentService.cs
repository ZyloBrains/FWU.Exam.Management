using FWU.Exam.Management.Domain.Entities.Teachers;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ITeacherSubjectAssignmentService
{
    Task<List<TeacherSubjectAssignment>> GetAssignmentsAsync(string? teacherUserId = null, int? collegeId = null);
    Task<TeacherSubjectAssignment?> GetByIdAsync(int id);
    Task CreateAsync(TeacherSubjectAssignment assignment);
    Task UpdateAsync(TeacherSubjectAssignment assignment);
    Task DeleteAsync(int id);
    Task<List<int>> GetAssignedSubjectOfferingIdsAsync(string teacherUserId);
    Task<List<int>> GetAssignedExamScheduleIdsAsync(string teacherUserId);
    Task<bool> IsTeacherAssignedToSubjectAsync(string teacherUserId, int subjectOfferingId);
}
