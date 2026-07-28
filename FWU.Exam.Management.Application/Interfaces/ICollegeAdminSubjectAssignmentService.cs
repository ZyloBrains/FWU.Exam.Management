using FWU.Exam.Management.Domain.Entities.CollegeAdmins;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeAdminSubjectAssignmentService
{
    Task<List<CollegeAdminSubjectAssignment>> GetAssignmentsAsync(string? collegeAdminUserId = null);
    Task<CollegeAdminSubjectAssignment?> GetByIdAsync(int id);
    Task CreateAsync(CollegeAdminSubjectAssignment assignment);
    Task UpdateAsync(CollegeAdminSubjectAssignment assignment);
    Task DeleteAsync(int id);
    Task<List<int>> GetAssignedSubjectOfferingIdsAsync(string collegeAdminUserId);
    Task<List<int>> GetAssignedExamScheduleIdsAsync(string collegeAdminUserId);
    Task<bool> IsCollegeAdminAssignedToSubjectAsync(string collegeAdminUserId, int subjectOfferingId);
}
