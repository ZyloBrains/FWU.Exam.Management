using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class DepartmentService(AppDbContext context) : IDepartmentService
{
    public async Task<(List<Department> Items, int TotalCount)> GetDepartmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null)
    {
        var query = context.Departments.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d =>
                d.DepartmentCode.Contains(search) ||
                d.DepartmentName.Contains(search) ||
                (d.ShortName != null && d.ShortName.Contains(search)) ||
                (d.Remarks != null && d.Remarks.Contains(search)));
        }

        if (facultyId.HasValue)
        {
            query = query.Where(d => d.FacultyId == facultyId.Value);
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Department>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null)
    {
        var query = context.Departments.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d =>
                d.DepartmentCode.Contains(search) ||
                d.DepartmentName.Contains(search) ||
                (d.ShortName != null && d.ShortName.Contains(search)) ||
                (d.Remarks != null && d.Remarks.Contains(search)));
        }

        if (facultyId.HasValue)
        {
            query = query.Where(d => d.FacultyId == facultyId.Value);
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await context.Departments.FindAsync(id);
    }

    public async Task CreateDepartmentAsync(Department department)
    {
        context.Departments.Add(department);
        await context.SaveChangesAsync();
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        context.Departments.Update(department);
        await context.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await context.Departments.FindAsync(id);
        if (department != null)
        {
            context.Departments.Remove(department);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> DepartmentExistsAsync(int id)
    {
        return await context.Departments.AnyAsync(d => d.Id == id);
    }

    private static Expression<Func<Department, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "departmentcode" => d => d.DepartmentCode,
            "departmentname" => d => d.DepartmentName,
            "shortname" => d => d.ShortName,
            "remarks" => d => d.Remarks,
            "isactive" => d => d.IsActive,
            _ => d => d.DepartmentName
        };
    }
}
