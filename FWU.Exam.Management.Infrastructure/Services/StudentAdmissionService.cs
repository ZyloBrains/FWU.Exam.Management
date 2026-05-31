using System.Linq.Expressions;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class StudentAdmissionService(AppDbContext context) : IStudentAdmissionService
{
    public async Task<(List<StudentAdmission> Items, int TotalCount)> GetAdmissionsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null)
    {
        var query = BuildQuery(search, collegeId);

        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<StudentAdmission>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null)
    {
        var query = BuildQuery(search, collegeId);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<StudentAdmission?> GetAdmissionByIdAsync(int id)
    {
        return await context.StudentAdmissions
            .Include(sa => sa.College)
            .Include(sa => sa.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.Id == id);
    }

    public async Task CreateAdmissionAsync(StudentAdmission admission)
    {
        context.StudentAdmissions.Add(admission);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAdmissionAsync(StudentAdmission admission)
    {
        context.StudentAdmissions.Update(admission);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAdmissionAsync(int id)
    {
        var admission = await context.StudentAdmissions.FindAsync(id);
        if (admission != null)
        {
            context.StudentAdmissions.Remove(admission);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> AdmissionExistsAsync(int id)
    {
        return await context.StudentAdmissions.AnyAsync(sa => sa.Id == id);
    }

    public async Task CompleteAdmissionAsync(int id, string userId)
    {
        var admission = await context.StudentAdmissions.FindAsync(id);
        if (admission != null)
        {
            admission.IsCompleted = true;
            admission.CheckedBy = int.TryParse(userId, out var parsed) ? parsed : null;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Program>> GetCollegeProgramsAsync(int collegeId)
    {
        return await context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId && cp.IsActive)
            .Include(cp => cp.Program)
            .Select(cp => cp.Program)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetCollegeSelectListAsync()
    {
        return await context.Colleges
            .AsNoTracking()
            .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<StudentAdmission?> GetAdmissionByUserIdAsync(string userId)
    {
        return await context.StudentAdmissions
            .Include(sa => sa.College)
            .Include(sa => sa.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);
    }

    private IQueryable<StudentAdmission> BuildQuery(string? search, int? collegeId = null)
    {
        var query = context.StudentAdmissions
            .Include(sa => sa.College)
            .Include(sa => sa.Program)
            .AsNoTracking();

        if (collegeId.HasValue)
        {
            query = query.Where(sa => sa.CollegeId == collegeId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(sa =>
                sa.CollegeRollNumber.Contains(search) ||
                sa.College.Name.Contains(search) ||
                sa.Program.ProgramName.Contains(search));
        }

        return query;
    }

    private static Expression<Func<StudentAdmission, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "program" => sa => sa.Program.ProgramName,
            "college" => sa => sa.College.Name,
            "collegerollnumber" => sa => sa.CollegeRollNumber,
            "admissiondate" => sa => sa.AdmissionDate,
            "iscompleted" => sa => sa.IsCompleted,
            "isactive" => sa => sa.IsActive,
            _ => sa => sa.AdmissionDate
        };
    }
}
