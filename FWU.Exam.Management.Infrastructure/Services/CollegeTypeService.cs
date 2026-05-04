using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeTypeService : ICollegeTypeService
{
    private readonly AppDbContext _context;

    public CollegeTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<CollegeType> Items, int TotalCount)> GetCollegeTypesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.CollegeTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Code.Contains(search) ||
                c.Name.Contains(search) ||
                (c.Remarks != null && c.Remarks.Contains(search)));
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

    public async Task<List<CollegeType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.CollegeTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Code.Contains(search) ||
                c.Name.Contains(search) ||
                (c.Remarks != null && c.Remarks.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<CollegeType?> GetCollegeTypeByIdAsync(int id)
    {
        return await _context.CollegeTypes.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateCollegeTypeAsync(CollegeType collegeType)
    {
        if (collegeType.IsDefault == true)
        {
            var existingDefault = await _context.CollegeTypes
                .FirstOrDefaultAsync(c => c.IsDefault == true && c.Id != collegeType.Id);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _context.Update(existingDefault);
            }
        }

        _context.CollegeTypes.Add(collegeType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCollegeTypeAsync(CollegeType collegeType)
    {
        if (collegeType.IsDefault == true)
        {
            var existingDefault = await _context.CollegeTypes
                .FirstOrDefaultAsync(c => c.IsDefault == true && c.Id != collegeType.Id);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _context.Update(existingDefault);
            }
        }

        _context.CollegeTypes.Update(collegeType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCollegeTypeAsync(int id)
    {
        var collegeType = await _context.CollegeTypes.FindAsync(id);
        if (collegeType != null)
        {
            _context.CollegeTypes.Remove(collegeType);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CollegeTypeExistsAsync(int id)
    {
        return await _context.CollegeTypes.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<CollegeType, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => c => c.Code,
            "name" => c => c.Name,
            "remarks" => c => c.Remarks ?? "",
            "isdefault" => c => c.IsDefault,
            "isactive" => c => c.IsActive,
            _ => c => c.Name
        };
    }
}
