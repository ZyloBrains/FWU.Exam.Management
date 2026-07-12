using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamCenterService(AppDbContext context, IUserContext userContext) : IExamCenterService
{
    public async Task<(List<ExamCenter> Items, int TotalCount)> GetExamCentersAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
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

    public async Task<List<ExamCenter>> GetFilteredItemsAsync(string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));
        return await query.ToListAsync();
    }

    public async Task<ExamCenter?> GetExamCenterByIdAsync(int id)
    {
        return await context.ExamCenters
            .AsNoTracking()
            .Include(ec => ec.ExamSchedule)
            .Include(ec => ec.College)
            .Include(ec => ec.ExamCenterVenues)
                .ThenInclude(ecv => ecv.College)
            .Include(ec => ec.ExamCenterColleges)
                .ThenInclude(ecc => ecc.College)
            .FirstOrDefaultAsync(ec => ec.Id == id);
    }

    public async Task CreateExamCenterAsync(ExamCenter examCenter)
    {
        context.ExamCenters.Add(examCenter);
        await context.SaveChangesAsync();
    }

    public async Task CreateExamCenterWithCollegesAsync(ExamCenter examCenter, List<int> venueCollegeIds, List<int> sourceCollegeIds)
    {
        context.ExamCenters.Add(examCenter);
        await context.SaveChangesAsync();

        foreach (var collegeId in venueCollegeIds)
        {
            context.ExamCenterVenues.Add(new ExamCenterVenue
            {
                ExamCenterId = examCenter.Id,
                CollegeId = collegeId
            });
        }

        foreach (var collegeId in sourceCollegeIds)
        {
            context.ExamCenterColleges.Add(new ExamCenterCollege
            {
                ExamCenterId = examCenter.Id,
                CollegeId = collegeId
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdateExamCenterAsync(ExamCenter examCenter)
    {
        var existing = await context.ExamCenters.FindAsync(examCenter.Id);
        if (existing != null)
        {
            examCenter.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examCenter);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateExamCenterWithCollegesAsync(ExamCenter examCenter, List<int> venueCollegeIds, List<int> sourceCollegeIds)
    {
        var existing = await context.ExamCenters
            .Include(ec => ec.ExamCenterVenues)
            .Include(ec => ec.ExamCenterColleges)
            .FirstOrDefaultAsync(ec => ec.Id == examCenter.Id);

        if (existing != null)
        {
            examCenter.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examCenter);

            context.ExamCenterVenues.RemoveRange(existing.ExamCenterVenues ?? []);
            foreach (var collegeId in venueCollegeIds)
            {
                context.ExamCenterVenues.Add(new ExamCenterVenue
                {
                    ExamCenterId = examCenter.Id,
                    CollegeId = collegeId
                });
            }

            context.ExamCenterColleges.RemoveRange(existing.ExamCenterColleges ?? []);
            foreach (var collegeId in sourceCollegeIds)
            {
                context.ExamCenterColleges.Add(new ExamCenterCollege
                {
                    ExamCenterId = examCenter.Id,
                    CollegeId = collegeId
                });
            }

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamCenterAsync(int id)
    {
        var examCenter = await context.ExamCenters.FindAsync(id);
        if (examCenter != null)
        {
            examCenter.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamCenterExistsAsync(int id)
    {
        return await context.ExamCenters.AnyAsync(ec => ec.Id == id);
    }

    public async Task<List<College>> GetVenueCollegesAsync(int examCenterId)
    {
        return await context.ExamCenterVenues
            .Where(ecv => ecv.ExamCenterId == examCenterId)
            .Include(ecv => ecv.College)
            .Select(ecv => ecv.College!)
            .ToListAsync();
    }

    public async Task<List<College>> GetSourceCollegesAsync(int examCenterId)
    {
        return await context.ExamCenterColleges
            .Where(ecc => ecc.ExamCenterId == examCenterId)
            .Include(ecc => ecc.College)
            .Select(ecc => ecc.College!)
            .ToListAsync();
    }

    private IQueryable<ExamCenter> BuildQuery(string? search)
    {
        IQueryable<ExamCenter> query = context.ExamCenters
            .AsNoTracking()
            .Include(ec => ec.ExamSchedule)
            .Include(ec => ec.College)
            .Include(ec => ec.ExamCenterVenues)
                .ThenInclude(ecv => ecv.College)
            .Include(ec => ec.ExamCenterColleges)
                .ThenInclude(ecc => ecc.College)
            .Where(ec => ec.IsActive);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                query = query.Where(ec => ec.College != null && ec.College.Faculties!.Any(f => f.Id == userContext.FacultyId.Value));
            else if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(ec => ec.CollegeId == userContext.CollegeId.Value);
            else
                query = query.Where(ec => false);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(ec =>
                (ec.Code != null && ec.Code.Contains(search)) ||
                (ec.College != null && ec.College.Name != null && ec.College.Name.Contains(search)) ||
                (ec.College != null && ec.College.Code != null && ec.College.Code.Contains(search)) ||
                (ec.ExamCenterVenues != null && ec.ExamCenterVenues.Any(ecv => ecv.College != null && ecv.College.Name != null && ecv.College.Name.Contains(search))) ||
                (ec.ExamCenterColleges != null && ec.ExamCenterColleges.Any(ecc => ecc.College != null && ecc.College.Name != null && ecc.College.Name.Contains(search))) ||
                (ec.ExamSchedule != null && ec.ExamSchedule.ExamScheduleName != null && ec.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (ec.Remark != null && ec.Remark.Contains(search)));
        }

        return query;
    }

    private static Expression<Func<ExamCenter, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => ec => ec.Code ?? string.Empty,
            "college" => ec => ec.College != null ? ec.College.Name
                : ec.ExamCenterVenues != null && ec.ExamCenterVenues.Any()
                    ? ec.ExamCenterVenues.OrderBy(ecv => ecv.College != null ? ecv.College.Name : string.Empty).Select(ecv => ecv.College != null ? ecv.College.Name : string.Empty).FirstOrDefault() ?? string.Empty
                    : string.Empty,
            "schedule" => ec => ec.ExamSchedule != null ? ec.ExamSchedule.ExamScheduleName : string.Empty,
            "isactive" => ec => ec.IsActive,
            _ => ec => ec.Id
        };
    }
}
