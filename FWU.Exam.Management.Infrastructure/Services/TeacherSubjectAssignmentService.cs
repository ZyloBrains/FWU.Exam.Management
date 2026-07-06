using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Teachers;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class TeacherSubjectAssignmentService(AppDbContext context) : ITeacherSubjectAssignmentService
{
    public async Task<List<TeacherSubjectAssignment>> GetAssignmentsAsync(string? teacherUserId = null, int? collegeId = null, int? facultyId = null)
    {
        var query = context.TeacherSubjectAssignments
            .AsNoTracking()
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so.SubjectCatalog)
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so.Program)
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so.Semester)
            .Include(tsa => tsa.ExamSchedule)
            .AsQueryable();

        if (!string.IsNullOrEmpty(teacherUserId))
            query = query.Where(tsa => tsa.TeacherUserId == teacherUserId);

        if (collegeId.HasValue)
        {
            var teacherIds = await context.Users
                .Where(u => u.CollegeId == collegeId)
                .Select(u => u.Id)
                .ToListAsync();
            query = query.Where(tsa => teacherIds.Contains(tsa.TeacherUserId));
        }

        if (facultyId.HasValue)
        {
            query = query.Where(tsa => tsa.SubjectOffering != null && tsa.SubjectOffering.Program != null && tsa.SubjectOffering.Program.Department != null && tsa.SubjectOffering.Program.Department.FacultyId == facultyId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<TeacherSubjectAssignment?> GetByIdAsync(int id)
    {
        return await context.TeacherSubjectAssignments
            .Include(tsa => tsa.SubjectOffering)
            .Include(tsa => tsa.ExamSchedule)
            .FirstOrDefaultAsync(tsa => tsa.Id == id);
    }

    public async Task CreateAsync(TeacherSubjectAssignment assignment)
    {
        context.TeacherSubjectAssignments.Add(assignment);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TeacherSubjectAssignment assignment)
    {
        var existing = await context.TeacherSubjectAssignments.FindAsync(assignment.Id);
        if (existing != null)
        {
            assignment.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(assignment);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var assignment = await context.TeacherSubjectAssignments.FindAsync(id);
        if (assignment != null)
        {
            context.TeacherSubjectAssignments.Remove(assignment);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<int>> GetAssignedSubjectOfferingIdsAsync(string teacherUserId)
    {
        return await context.TeacherSubjectAssignments
            .Where(tsa => tsa.TeacherUserId == teacherUserId && tsa.IsActive)
            .Select(tsa => tsa.SubjectOfferingId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<int>> GetAssignedExamScheduleIdsAsync(string teacherUserId)
    {
        return await context.TeacherSubjectAssignments
            .Where(tsa => tsa.TeacherUserId == teacherUserId && tsa.IsActive && tsa.ExamScheduleId != null)
            .Select(tsa => tsa.ExamScheduleId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> IsTeacherAssignedToSubjectAsync(string teacherUserId, int subjectOfferingId)
    {
        return await context.TeacherSubjectAssignments
            .AnyAsync(tsa => tsa.TeacherUserId == teacherUserId
                          && tsa.SubjectOfferingId == subjectOfferingId
                          && tsa.IsActive);
    }
}
