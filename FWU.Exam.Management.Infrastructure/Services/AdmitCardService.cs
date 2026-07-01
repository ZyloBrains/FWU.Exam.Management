using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AdmitCardService(AppDbContext context) : IAdmitCardService
{
    private IQueryable<AdmitCard> ApplyScope(IQueryable<AdmitCard> query, int? collegeId, int? facultyId)
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

    public async Task<(List<AdmitCard> Items, int TotalCount)> GetAdmitCardsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null)
    {
        var query = ApplyScope(BuildQuery(search, sort, sortDir, examScheduleId), collegeId, facultyId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new AdmitCard
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamScheduleId = e.ExamScheduleId,
                StudentRegistrationId = e.StudentRegistrationId,
                AdmitCardNumber = e.AdmitCardNumber,
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

    public async Task<List<AdmitCard>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, "Id", "asc", null), collegeId, facultyId);
        return await query
            .Select(e => new AdmitCard
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamScheduleId = e.ExamScheduleId,
                StudentRegistrationId = e.StudentRegistrationId,
                AdmitCardNumber = e.AdmitCardNumber,
                GeneratedDate = e.GeneratedDate,
                IsDownloaded = e.IsDownloaded,
                ExamRegistration = e.ExamRegistration,
                ExamSchedule = e.ExamSchedule,
                StudentRegistration = e.StudentRegistration
            })
            .ToListAsync();
    }

    public async Task<AdmitCard?> GetAdmitCardByIdAsync(int id)
    {
        return await context.AdmitCards
            .AsNoTracking()
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er.College)
            .Include(e => e.ExamSchedule)
            .Include(e => e.StudentRegistration)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateAdmitCardAsync(AdmitCard admitCard)
    {
        context.AdmitCards.Add(admitCard);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAdmitCardAsync(AdmitCard admitCard)
    {
        var existing = await context.AdmitCards.FindAsync(admitCard.Id);
        if (existing != null)
        {
            admitCard.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(admitCard);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAdmitCardAsync(int id)
    {
        var admitCard = await context.AdmitCards.FindAsync(id);
        if (admitCard != null)
        {
            admitCard.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> AdmitCardExistsAsync(int id)
    {
        return await context.AdmitCards.AnyAsync(e => e.Id == id);
    }

    public async Task<AdmitCard> GenerateAdmitCardAsync(int examRegistrationId)
    {
        var registration = await context.ExamRegistrations
            .Include(er => er.ExamSchedule)
            .FirstOrDefaultAsync(er => er.Id == examRegistrationId)
            ?? throw new InvalidOperationException("Exam registration not found.");

        var admitCard = new AdmitCard
        {
            ExamRegistrationId = examRegistrationId,
            ExamScheduleId = registration.ExamScheduleId,
            StudentRegistrationId = null,
            AdmitCardNumber = $"AC-{registration.ExamScheduleId:D4}-{registration.Id:D6}",
            GeneratedDate = DateTime.UtcNow,
            IsDownloaded = false,
            IsActive = true
        };

        context.AdmitCards.Add(admitCard);
        await context.SaveChangesAsync();
        return admitCard;
    }

    public async Task<List<AdmitCard>> GenerateBulkAdmitCardsAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status == Domain.Enums.RegistrationStatus.Registered)
            .ToListAsync();

        var admitCards = new List<AdmitCard>();
        foreach (var registration in registrations)
        {
            var existing = await context.AdmitCards
                .FirstOrDefaultAsync(ht => ht.ExamRegistrationId == registration.Id && ht.IsActive);

            if (existing == null)
            {
                var admitCard = new AdmitCard
                {
                    ExamRegistrationId = registration.Id,
                    ExamScheduleId = examScheduleId,
                    StudentRegistrationId = null,
                    AdmitCardNumber = $"AC-{examScheduleId:D4}-{registration.Id:D6}",
                    GeneratedDate = DateTime.UtcNow,
                    IsDownloaded = false,
                    IsActive = true
                };
                admitCards.Add(admitCard);
            }
        }

        if (admitCards.Count > 0)
        {
            context.AdmitCards.AddRange(admitCards);
            await context.SaveChangesAsync();
        }

        return admitCards;
    }

    public AdmitCardSelectListsDto GetSelectListData(AdmitCard? admitCard = null)
    {
        var examSchedules = context.ExamSchedules.AsNoTracking().ToList();
        var examRegistrations = context.ExamRegistrations.AsNoTracking().ToList();
        var examCenters = context.ExamCenters.AsNoTracking().ToList();

        return new AdmitCardSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            ExamRegistrations = examRegistrations.Select(er => new SelectOption { Id = er.Id, Name = $"Reg #{er.Id}" }).ToList(),
            ExamCenters = examCenters.Select(ec => new SelectOption { Id = ec.Id, Name = $"Center {ec.Code}" }).ToList()
        };
    }

    private IQueryable<AdmitCard> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        IQueryable<AdmitCard> query = context.AdmitCards.AsNoTracking()
            .Include(e => e.ExamRegistration)
            .Include(e => e.ExamSchedule)
            .Include(e => e.StudentRegistration);

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.AdmitCardNumber != null && e.AdmitCardNumber.Contains(search)) ||
                (e.ExamSchedule != null && e.ExamSchedule.ExamScheduleName != null && e.ExamSchedule.ExamScheduleName.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "admitcardnumber" => descending ? query.OrderByDescending(e => e.AdmitCardNumber) : query.OrderBy(e => e.AdmitCardNumber),
            "schedule" => descending
                ? query.OrderByDescending(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty)
                : query.OrderBy(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty),
            "date" => descending ? query.OrderByDescending(e => e.GeneratedDate) : query.OrderBy(e => e.GeneratedDate),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
