using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class HallTicketService(AppDbContext context) : IHallTicketService
{
    private IQueryable<HallTicket> ApplyScope(IQueryable<HallTicket> query, int? collegeId, int? facultyId)
    {
        if (collegeId.HasValue)
            return query.Where(e => e.ExamRegistration != null && e.ExamRegistration.CollegeId == collegeId.Value);

        if (facultyId.HasValue)
        {
            var collegeIds = context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == facultyId.Value))
                .Select(c => c.Id)
                .ToList();

            return query.Where(e => e.ExamRegistration != null && collegeIds.Contains(e.ExamRegistration.CollegeId));
        }

        return query;
    }

    public async Task<(List<HallTicket> Items, int TotalCount)> GetHallTicketsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null)
    {
        var query = ApplyScope(BuildQuery(search, sort, sortDir, examScheduleId), collegeId, facultyId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new HallTicket
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamScheduleId = e.ExamScheduleId,
                StudentRegistrationId = e.StudentRegistrationId,
                HallTicketNumber = e.HallTicketNumber,
                GeneratedDate = e.GeneratedDate,
                IsDownloaded = e.IsDownloaded,
                DownloadedDate = e.DownloadedDate,
                IsActive = e.IsActive,
                ExamRegistration = e.ExamRegistration,
                ExamSchedule = e.ExamSchedule,
                StudentRegistration = e.StudentRegistration
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<HallTicket>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, "Id", "asc", null), collegeId, facultyId);
        return await query
            .Select(e => new HallTicket
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamScheduleId = e.ExamScheduleId,
                StudentRegistrationId = e.StudentRegistrationId,
                HallTicketNumber = e.HallTicketNumber,
                GeneratedDate = e.GeneratedDate,
                IsDownloaded = e.IsDownloaded,
                ExamRegistration = e.ExamRegistration,
                ExamSchedule = e.ExamSchedule,
                StudentRegistration = e.StudentRegistration
            })
            .ToListAsync();
    }

    public async Task<HallTicket?> GetHallTicketByIdAsync(int id)
    {
        return await context.HallTickets
            .AsNoTracking()
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er.College)
            .Include(e => e.ExamSchedule)
            .Include(e => e.StudentRegistration)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateHallTicketAsync(HallTicket hallTicket)
    {
        context.HallTickets.Add(hallTicket);
        await context.SaveChangesAsync();
    }

    public async Task UpdateHallTicketAsync(HallTicket hallTicket)
    {
        var existing = await context.HallTickets.FindAsync(hallTicket.Id);
        if (existing != null)
        {
            hallTicket.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(hallTicket);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteHallTicketAsync(int id)
    {
        var hallTicket = await context.HallTickets.FindAsync(id);
        if (hallTicket != null)
        {
            hallTicket.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> HallTicketExistsAsync(int id)
    {
        return await context.HallTickets.AnyAsync(e => e.Id == id);
    }

    public async Task<HallTicket> GenerateHallTicketAsync(int examRegistrationId)
    {
        var registration = await context.ExamRegistrations
            .Include(er => er.ExamSchedule)
            .FirstOrDefaultAsync(er => er.Id == examRegistrationId)
            ?? throw new InvalidOperationException("Exam registration not found.");

        var hallTicket = new HallTicket
        {
            ExamRegistrationId = examRegistrationId,
            ExamScheduleId = registration.ExamScheduleId,
            StudentRegistrationId = null,
            HallTicketNumber = $"HT-{registration.ExamScheduleId:D4}-{registration.Id:D6}",
            GeneratedDate = DateTime.UtcNow,
            IsDownloaded = false,
            IsActive = true
        };

        context.HallTickets.Add(hallTicket);
        await context.SaveChangesAsync();
        return hallTicket;
    }

    public async Task<List<HallTicket>> GenerateBulkHallTicketsAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status == Domain.Enums.RegistrationStatus.Registered)
            .ToListAsync();

        var hallTickets = new List<HallTicket>();
        foreach (var registration in registrations)
        {
            var existing = await context.HallTickets
                .FirstOrDefaultAsync(ht => ht.ExamRegistrationId == registration.Id && ht.IsActive);

            if (existing == null)
            {
                var hallTicket = new HallTicket
                {
                    ExamRegistrationId = registration.Id,
                    ExamScheduleId = examScheduleId,
                    StudentRegistrationId = null,
                    HallTicketNumber = $"HT-{examScheduleId:D4}-{registration.Id:D6}",
                    GeneratedDate = DateTime.UtcNow,
                    IsDownloaded = false,
                    IsActive = true
                };
                hallTickets.Add(hallTicket);
            }
        }

        if (hallTickets.Count > 0)
        {
            context.HallTickets.AddRange(hallTickets);
            await context.SaveChangesAsync();
        }

        return hallTickets;
    }

    public HallTicketSelectListsDto GetSelectListData(HallTicket? hallTicket = null)
    {
        var examSchedules = context.ExamSchedules.AsNoTracking().ToList();
        var examRegistrations = context.ExamRegistrations.AsNoTracking().ToList();
        var examCenters = context.ExamCenters.AsNoTracking().ToList();

        return new HallTicketSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            ExamRegistrations = examRegistrations.Select(er => new SelectOption { Id = er.Id, Name = $"Reg #{er.Id}" }).ToList(),
            ExamCenters = examCenters.Select(ec => new SelectOption { Id = ec.Id, Name = $"Center {ec.Code}" }).ToList()
        };
    }

    private IQueryable<HallTicket> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        IQueryable<HallTicket> query = context.HallTickets.AsNoTracking()
            .Include(e => e.ExamRegistration)
            .Include(e => e.ExamSchedule)
            .Include(e => e.StudentRegistration);

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.HallTicketNumber != null && e.HallTicketNumber.Contains(search)) ||
                (e.ExamSchedule != null && e.ExamSchedule.ExamScheduleName != null && e.ExamSchedule.ExamScheduleName.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "hallticketnumber" => descending ? query.OrderByDescending(e => e.HallTicketNumber) : query.OrderBy(e => e.HallTicketNumber),
            "schedule" => descending
                ? query.OrderByDescending(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty)
                : query.OrderBy(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty),
            "date" => descending ? query.OrderByDescending(e => e.GeneratedDate) : query.OrderBy(e => e.GeneratedDate),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
