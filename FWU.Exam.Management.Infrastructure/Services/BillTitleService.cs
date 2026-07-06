using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BillTitleService(AppDbContext context) : IBillTitleService
{
    public async Task<(List<BillTitle> Items, int TotalCount)> GetBillTitlesAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null)
    {
        IQueryable<BillTitle> query = context.Set<BillTitle>().AsNoTracking()
            .Include(bt => bt.ExamSchedule)
            .Include(bt => bt.Program);

        if (collegeId.HasValue)
        {
            var collegeProgramIds = context.CollegePrograms
                .Where(cp => cp.CollegeId == collegeId.Value)
                .Select(cp => cp.ProgramId)
                .Distinct();
            query = query.Where(bt => bt.ProgramsId != null && collegeProgramIds.Contains(bt.ProgramsId.Value));
        }
        else if (facultyId.HasValue)
        {
            var facultyDeptIds = context.Departments
                .Where(d => d.FacultyId == facultyId.Value)
                .Select(d => d.Id);
            query = query.Where(bt => bt.ProgramsId != null && context.Programs.Any(p => p.Id == bt.ProgramsId && facultyDeptIds.Contains(p.DepartmentId)));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(bt =>
                bt.BillTitleName.Contains(search) ||
                bt.Category.Contains(search));
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

    public async Task<List<BillTitle>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null)
    {
        IQueryable<BillTitle> query = context.Set<BillTitle>().AsNoTracking()
            .Include(bt => bt.ExamSchedule)
            .Include(bt => bt.Program);

        if (collegeId.HasValue)
        {
            var collegeProgramIds = context.CollegePrograms
                .Where(cp => cp.CollegeId == collegeId.Value)
                .Select(cp => cp.ProgramId)
                .Distinct();
            query = query.Where(bt => bt.ProgramsId != null && collegeProgramIds.Contains(bt.ProgramsId.Value));
        }
        else if (facultyId.HasValue)
        {
            var facultyDeptIds = context.Departments
                .Where(d => d.FacultyId == facultyId.Value)
                .Select(d => d.Id);
            query = query.Where(bt => bt.ProgramsId != null && context.Programs.Any(p => p.Id == bt.ProgramsId && facultyDeptIds.Contains(p.DepartmentId)));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(bt =>
                bt.BillTitleName.Contains(search) ||
                bt.Category.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<BillTitle?> GetBillTitleByIdAsync(int id)
    {
        return await context.Set<BillTitle>()
            .Include(bt => bt.ExamSchedule)
            .Include(bt => bt.Program)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateBillTitleAsync(BillTitle billTitle)
    {
        context.Set<BillTitle>().Add(billTitle);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBillTitleAsync(BillTitle billTitle)
    {
        context.Set<BillTitle>().Update(billTitle);
        await context.SaveChangesAsync();
    }

    public async Task DeleteBillTitleAsync(int id)
    {
        var billTitle = await context.Set<BillTitle>().FindAsync(id);
        if (billTitle != null)
        {
            context.Set<BillTitle>().Remove(billTitle);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> BillTitleExistsAsync(int id)
    {
        return await context.Set<BillTitle>().AnyAsync(bt => bt.Id == id);
    }

    public async Task<List<ExamSchedule>> GetExamSchedulesAsync(int? collegeId = null, int? facultyId = null)
    {
        IQueryable<ExamSchedule> query = context.ExamSchedules.AsNoTracking();

        if (collegeId.HasValue)
        {
            var programIds = context.CollegePrograms!
                .Where(cp => cp.CollegeId == collegeId.Value)
                .Select(cp => cp.ProgramId)
                .Distinct()
                .ToList();

            query = query.Where(e => programIds.Contains(e.ProgramId));
        }
        else if (facultyId.HasValue)
        {
            var collegeIds = context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == facultyId.Value))
                .Select(c => c.Id)
                .ToList();

            var programIds = context.CollegePrograms!
                .Where(cp => collegeIds.Contains(cp.CollegeId))
                .Select(cp => cp.ProgramId)
                .Distinct()
                .ToList();

            query = query.Where(e => programIds.Contains(e.ProgramId));
        }

        return await query.ToListAsync();
    }

    public async Task<List<Domain.Entities.Program>> GetProgramsAsync()
    {
        return await context.Programs.AsNoTracking().ToListAsync();
    }

    private static Expression<Func<BillTitle, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "billtitlename" => bt => bt.BillTitleName ?? "",
            "category" => bt => bt.Category ?? "",
            "amount" => bt => bt.Amount ?? 0,
            "isactive" => bt => bt.IsActive,
            "applicabledate" => bt => bt.ApplicableDate ?? DateTime.MinValue,
            "throughdate" => bt => bt.ThroughDate ?? DateTime.MinValue,
            _ => bt => bt.BillTitleName ?? ""
        };
    }
}
