using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeAdminSubjectAssignmentService(AppDbContext context, IUserContext userContext) : ICollegeAdminSubjectAssignmentService
{
    public async Task<List<CollegeAdminSubjectAssignment>> GetAssignmentsAsync(string? collegeAdminUserId = null)
    {
        var query = context.CollegeAdminSubjectAssignments
            .AsNoTracking()
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so!.SubjectCatalog)
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so!.Program)
            .Include(tsa => tsa.SubjectOffering)
                .ThenInclude(so => so!.Semester)
            .Include(tsa => tsa.ExamSchedule)
            .AsQueryable();

        if (!string.IsNullOrEmpty(collegeAdminUserId))
            query = query.Where(tsa => tsa.CollegeAdminUserId == collegeAdminUserId);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeAdminIds = await context.Users
                    .Where(u => u.CollegeId == userContext.CollegeId.Value)
                    .Select(u => u.Id)
                    .ToListAsync();
                query = query.Where(tsa => collegeAdminIds.Contains(tsa.CollegeAdminUserId));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            {
                query = query.Where(tsa => tsa.SubjectOffering != null && tsa.SubjectOffering.Program != null && tsa.SubjectOffering.Program.FacultyId == userContext.FacultyId.Value);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<CollegeAdminSubjectAssignment?> GetByIdAsync(int id)
    {
        return await context.CollegeAdminSubjectAssignments
            .Include(tsa => tsa.SubjectOffering)
            .Include(tsa => tsa.ExamSchedule)
            .FirstOrDefaultAsync(tsa => tsa.Id == id);
    }

    public async Task CreateAsync(CollegeAdminSubjectAssignment assignment)
    {
        context.CollegeAdminSubjectAssignments.Add(assignment);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CollegeAdminSubjectAssignment assignment)
    {
        var existing = await context.CollegeAdminSubjectAssignments.FindAsync(assignment.Id);
        if (existing != null)
        {
            assignment.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(assignment);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var assignment = await context.CollegeAdminSubjectAssignments.FindAsync(id);
        if (assignment != null)
        {
            context.CollegeAdminSubjectAssignments.Remove(assignment);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<int>> GetAssignedSubjectOfferingIdsAsync(string collegeAdminUserId)
    {
        return await context.CollegeAdminSubjectAssignments
            .Where(tsa => tsa.CollegeAdminUserId == collegeAdminUserId && tsa.IsActive)
            .Select(tsa => tsa.SubjectOfferingId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<int>> GetAssignedExamScheduleIdsAsync(string collegeAdminUserId)
    {
        return await context.CollegeAdminSubjectAssignments
            .Where(tsa => tsa.CollegeAdminUserId == collegeAdminUserId && tsa.IsActive && tsa.ExamScheduleId != null)
            .Select(tsa => tsa.ExamScheduleId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> IsCollegeAdminAssignedToSubjectAsync(string collegeAdminUserId, int subjectOfferingId)
    {
        if (await context.CollegeAdminSubjectAssignments
            .AnyAsync(tsa => tsa.CollegeAdminUserId == collegeAdminUserId
                          && tsa.SubjectOfferingId == subjectOfferingId
                          && tsa.IsActive))
            return true;

        if (userContext.IsSuperAdmin)
            return true;

        if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            return await context.SubjectOfferings
                .AnyAsync(so => so.Id == subjectOfferingId
                             && so.Program != null
                             && so.Program.FacultyId == userContext.FacultyId.Value);

        if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            var collegeProgramIds = await context.CollegePrograms
                .AsNoTracking()
                .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                .Select(cp => cp.ProgramId)
                .ToListAsync();
            return await context.SubjectOfferings
                .AnyAsync(so => so.Id == subjectOfferingId && collegeProgramIds.Contains(so.ProgramId));
        }

        return false;
    }
}
