using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BillTitleService(AppDbContext context, IUserContext userContext) : IBillTitleService
{
    public async Task<(List<BillTitle> Items, int TotalCount)> GetBillTitlesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        IQueryable<BillTitle> query = context.Set<BillTitle>().AsNoTracking()
            .Include(bt => bt.ExamSchedule)
            .Include(bt => bt.Program);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeProgramIds = context.CollegePrograms
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .Distinct();
                query = query.Where(bt => bt.ProgramsId != null && collegeProgramIds.Contains(bt.ProgramsId.Value));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            {
                var facultyProgramIds = context.Programs
                    .Where(p => p.FacultyId == userContext.FacultyId.Value)
                    .Select(p => p.Id)
                    .Distinct();
                query = query.Where(bt => bt.ProgramsId != null && facultyProgramIds.Contains(bt.ProgramsId.Value));
            }
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

    public async Task<List<BillTitle>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        IQueryable<BillTitle> query = context.Set<BillTitle>().AsNoTracking()
            .Include(bt => bt.ExamSchedule)
            .Include(bt => bt.Program);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeProgramIds = context.CollegePrograms
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .Distinct();
                query = query.Where(bt => bt.ProgramsId != null && collegeProgramIds.Contains(bt.ProgramsId.Value));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            {
                var facultyProgramIds = context.Programs
                    .Where(p => p.FacultyId == userContext.FacultyId.Value)
                    .Select(p => p.Id)
                    .Distinct();
                query = query.Where(bt => bt.ProgramsId != null && facultyProgramIds.Contains(bt.ProgramsId.Value));
            }
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

    public async Task<List<ExamSchedule>> GetExamSchedulesAsync()
    {
        IQueryable<ExamSchedule> query = context.ExamSchedules.AsNoTracking();

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var programIds = context.CollegePrograms!
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .Distinct()
                    .ToList();
                query = query.Where(e => programIds.Contains(e.ProgramId));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            {
                var programIds = context.Programs
                    .Where(p => p.FacultyId == userContext.FacultyId.Value)
                    .Select(p => p.Id)
                    .Distinct()
                    .ToList();
                query = query.Where(e => programIds.Contains(e.ProgramId));
            }
        }

        return await query.ToListAsync();
    }

    public async Task<List<Domain.Entities.Program>> GetProgramsAsync()
    {
        var query = context.Programs.AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeProgramIds = await context.CollegePrograms.AsNoTracking()
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .ToListAsync();
                query = query.Where(p => collegeProgramIds.Contains(p.Id));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
            {
                query = query.Where(p => p.FacultyId == userContext.FacultyId.Value);
            }
        }
        return await query.ToListAsync();
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
