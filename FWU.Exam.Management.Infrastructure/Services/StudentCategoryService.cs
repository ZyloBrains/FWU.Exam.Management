using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class StudentCategoryService : IStudentCategoryService
{
    private readonly AppDbContext _context;

    public StudentCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<StudentCategory> Items, int TotalCount)> GetStudentCategoriesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.StudentCategories.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.StudentCategoryName != null && s.StudentCategoryName.Contains(search));
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

    public async Task<List<StudentCategory>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.StudentCategories.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.StudentCategoryName != null && s.StudentCategoryName.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<StudentCategory?> GetStudentCategoryByIdAsync(int id)
    {
        return await _context.StudentCategories.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateStudentCategoryAsync(StudentCategory studentCategory)
    {
        var existing = await _context.StudentCategories
            .FirstOrDefaultAsync(s => s.StudentCategoryName == studentCategory.StudentCategoryName);

        if (existing != null)
            throw new InvalidOperationException($"Student category '{studentCategory.StudentCategoryName}' already exists.");

        _context.StudentCategories.Add(studentCategory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStudentCategoryAsync(StudentCategory studentCategory)
    {
        var existing = await _context.StudentCategories
            .FirstOrDefaultAsync(s => s.StudentCategoryName == studentCategory.StudentCategoryName && s.Id != studentCategory.Id);

        if (existing != null)
            throw new InvalidOperationException($"Student category '{studentCategory.StudentCategoryName}' already exists.");

        _context.StudentCategories.Update(studentCategory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentCategoryAsync(int id)
    {
        var studentCategory = await _context.StudentCategories.FindAsync(id);
        if (studentCategory != null)
        {
            _context.StudentCategories.Remove(studentCategory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> StudentCategoryExistsAsync(int id)
    {
        return await _context.StudentCategories.AnyAsync(e => e.Id == id);
    }

    private static System.Linq.Expressions.Expression<Func<StudentCategory, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "name" or "studentcategoryname" => s => s.StudentCategoryName ?? "",
            "isactive" => s => s.IsActive,
            "remarks" => s => s.Remarks ?? "",
            _ => s => s.StudentCategoryName ?? ""
        };
    }
}
