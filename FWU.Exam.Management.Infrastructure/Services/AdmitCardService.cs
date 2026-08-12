using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AdmitCardService(AppDbContext context, IUserContext userContext, ITenantContext tenantContext) : IAdmitCardService
{
    public async Task<(List<AdmitCard> Items, int TotalCount)> GetAdmitCardsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = BuildQuery(search, sort, sortDir, examScheduleId).ApplyScope(userContext);

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

    public async Task<List<AdmitCard>> GetFilteredItemsAsync(string? search, int? examScheduleId = null)
    {
        var query = BuildQuery(search, "Id", "asc", examScheduleId).ApplyScope(userContext);
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
        var admitCard = await context.AdmitCards
            .AsNoTracking()
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er!.College)
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er!.ExamCenter)
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er!.Program)
            .Include(e => e.ExamRegistration)
                .ThenInclude(er => er!.ApplicationVoucher)
                    .ThenInclude(v => v!.StudentRegistration)
            .Include(e => e.ExamSchedule)
                .ThenInclude(s => s!.Semester)
            .Include(e => e.ExamSchedule)
                .ThenInclude(s => s!.Program)
                    .ThenInclude(p => p!.Level)
            .Include(e => e.ExamSchedule)
                .ThenInclude(s => s!.Level)
            .Include(e => e.ExamSchedule)
                .ThenInclude(s => s!.ExamType)
            .Include(e => e.ExamSchedule)
                .ThenInclude(s => s!.AcademicYear)
            .Include(e => e.StudentRegistration)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (admitCard == null) return null;

        if (admitCard.ExamSchedule != null)
        {
            var offeringQuery = context.SubjectOfferings
                .AsNoTracking()
                .Include(so => so.SubjectCatalog)
                .Where(so => so.ProgramId == admitCard.ExamSchedule.ProgramId
                          && so.SemesterId == admitCard.ExamSchedule.SemesterId);

            var subjectOfferings = await offeringQuery
                .OrderBy(so => so.DisplayOrder)
                .ToListAsync();

            admitCard.Subjects = subjectOfferings
                .Where(so => so.SubjectCatalog != null)
                .Select(so => new Subject
                {
                    Code = so.SubjectCatalog!.SubjectCode,
                    Name = so.SubjectCatalog.SubjectName,
                    Theory = so.HasTheory,
                    Practical = so.HasPractical,
                    Remarks = null
                })
                .ToList();
        }

        var sr = admitCard.StudentRegistration
            ?? admitCard.ExamRegistration?.ApplicationVoucher?.StudentRegistration;

        if (sr != null)
        {
            admitCard.StudentRegistrationId ??= sr.Id;
            admitCard.StudentRegistration ??= sr;
            admitCard.RegistrationNumber ??= sr.RegistrationNumber;

            if (string.IsNullOrEmpty(admitCard.PhotoPath) || string.IsNullOrEmpty(admitCard.SignaturePath))
            {
                if (!string.IsNullOrEmpty(sr.RegistrationNumber))
                {
                    // Students have UserName = RegistrationNumber; legacy rows stored the
                    // registration number in the Email column instead.
                    var appUser = await context.Users.FirstOrDefaultAsync(u =>
                        u.NormalizedUserName == sr.RegistrationNumber.ToUpperInvariant()
                        || (u.Email != null && u.Email == sr.RegistrationNumber));
                    if (appUser != null)
                    {
                        admitCard.PhotoPath ??= appUser.ProfilePath;
                        admitCard.SignaturePath ??= appUser.SignaturePath;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(admitCard.Campus))
            admitCard.Campus = admitCard.ExamRegistration?.College?.Name;

        if (string.IsNullOrEmpty(admitCard.Level))
            admitCard.Level = admitCard.ExamSchedule?.Level?.LevelName;

        if (string.IsNullOrEmpty(admitCard.Program))
            admitCard.Program = admitCard.ExamSchedule?.Program?.ProgramName
                ?? admitCard.ExamRegistration?.Program?.ProgramName;

        if (string.IsNullOrEmpty(admitCard.Semester))
            admitCard.Semester = admitCard.ExamSchedule?.Semester?.Name;

        if (string.IsNullOrEmpty(admitCard.ExamType))
            admitCard.ExamType = admitCard.ExamSchedule?.ExamType?.Name;

        if (string.IsNullOrEmpty(admitCard.Year))
            admitCard.Year = admitCard.ExamSchedule?.AcademicYear?.AcademicYearCode;

        if (string.IsNullOrEmpty(admitCard.ExamRollNo))
            admitCard.ExamRollNo = admitCard.ExamRegistration?.ExamRollNumber
                ?? admitCard.ExamRegistration?.SymbolNumber;

        if (string.IsNullOrEmpty(admitCard.ControllerSignaturePath) && admitCard.ExamRegistration?.CollegeId != null)
        {
            var tenant = await context.Tenants.FindAsync(tenantContext.TenantId);
            if (!string.IsNullOrEmpty(tenant?.ControllerSignaturePath))
                admitCard.ControllerSignaturePath = tenant.ControllerSignaturePath;
        }

        return admitCard;
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
            .Include(er => er.College)
            .FirstOrDefaultAsync(er => er.Id == examRegistrationId)
            ?? throw new InvalidOperationException("Exam registration not found.");

        if (string.IsNullOrEmpty(registration.SymbolNumber))
        {
            throw new InvalidOperationException(
                "Cannot generate admit card: symbol number has not been assigned. " +
                "Please assign symbol numbers first via Exam Center Distribution.");
        }

        var studentUser = await ResolveStudentUserAsync(registration);
        var controllerSignaturePath = await ResolveControllerSignatureAsync();

        int? resolvedSrId = null;
        string? resolvedStudentRegNumber = null;
        if (registration.ApplicationVoucherId.HasValue)
        {
            var voucher = await context.ApplicationVouchers.FindAsync(registration.ApplicationVoucherId.Value);
            if (voucher?.StudentRegistrationId.HasValue == true)
            {
                resolvedSrId = voucher.StudentRegistrationId.Value;
                var sr = await context.StudentRegistrations.FindAsync(voucher.StudentRegistrationId.Value);
                resolvedStudentRegNumber = sr?.RegistrationNumber;
            }
        }

        var admitCard = new AdmitCard
        {
            ExamRegistrationId = examRegistrationId,
            ExamScheduleId = registration.ExamScheduleId,
            StudentRegistrationId = resolvedSrId,
            RegistrationNumber = resolvedStudentRegNumber,
            AdmitCardNumber = $"AC-{registration.ExamScheduleId:D4}-{registration.Id:D6}",
            ExamRollNo = registration.SymbolNumber,
            PhotoPath = studentUser?.ProfilePath,
            SignaturePath = studentUser?.SignaturePath,
            ControllerSignaturePath = controllerSignaturePath,
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
            .Include(er => er.College)
            .ToListAsync();

        var missingSymbol = registrations.Where(r => string.IsNullOrEmpty(r.SymbolNumber)).ToList();
        if (missingSymbol.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot generate admit cards: {missingSymbol.Count} registration(s) are missing symbol numbers. " +
                "Please assign symbol numbers first via Exam Center Distribution before generating admit cards.");
        }

        var registrationIds = registrations.Select(r => r.Id).ToList();
        var existingAdmitCards = await context.AdmitCards
            .Where(ht => registrationIds.Contains(ht.ExamRegistrationId) && ht.IsActive)
            .ToListAsync();
        var existingRegistrationIds = new HashSet<int>(existingAdmitCards.Select(ht => ht.ExamRegistrationId));

        var voucherIds = registrations.Where(r => r.ApplicationVoucherId.HasValue).Select(r => r.ApplicationVoucherId!.Value).Distinct().ToList();
        var vouchers = voucherIds.Count > 0
            ? await context.ApplicationVouchers.Where(v => voucherIds.Contains(v.Id)).ToListAsync()
            : [];
        var erIdToSrId = vouchers.Where(v => v.StudentRegistrationId.HasValue).ToDictionary(v => v.Id, v => v.StudentRegistrationId!.Value);

        var srIds = erIdToSrId.Values.Distinct().ToList();
        var studentRegistrations = srIds.Count > 0
            ? await context.StudentRegistrations.Where(sr => srIds.Contains(sr.Id)).ToListAsync()
            : [];
        var srLookup = studentRegistrations.ToDictionary(sr => sr.Id);

        var currentTenant = await context.Tenants.FindAsync(tenantContext.TenantId);
        var currentTenantSignature = currentTenant?.ControllerSignaturePath;

        var admitCards = new List<AdmitCard>();
        foreach (var registration in registrations)
        {
            if (existingRegistrationIds.Contains(registration.Id))
                continue;

            string? controllerSignaturePath = currentTenantSignature;

            string? photoPath = null;
            string? signaturePath = null;
            string? resolvedStudentRegNumber = null;
            int? resolvedSrId = null;

            if (registration.ApplicationVoucherId.HasValue
                && erIdToSrId.TryGetValue(registration.ApplicationVoucherId.Value, out var srId)
                && srLookup.TryGetValue(srId, out var sr))
            {
                resolvedSrId = srId;
                resolvedStudentRegNumber = sr.RegistrationNumber;

                if (!string.IsNullOrEmpty(sr.RegistrationNumber))
                {
                    var appUser = await context.Users.FirstOrDefaultAsync(u => u.Email == sr.RegistrationNumber);
                    if (appUser != null)
                    {
                        photoPath = appUser.ProfilePath;
                        signaturePath = appUser.SignaturePath;
                    }
                }
            }

            var admitCard = new AdmitCard
            {
                ExamRegistrationId = registration.Id,
                ExamScheduleId = examScheduleId,
                StudentRegistrationId = resolvedSrId,
                RegistrationNumber = resolvedStudentRegNumber,
                AdmitCardNumber = $"AC-{examScheduleId:D4}-{registration.Id:D6}",
                ExamRollNo = registration.SymbolNumber,
                PhotoPath = photoPath,
                SignaturePath = signaturePath,
                ControllerSignaturePath = controllerSignaturePath,
                GeneratedDate = DateTime.UtcNow,
                IsDownloaded = false,
                IsActive = true
            };
            admitCards.Add(admitCard);
        }

        if (admitCards.Count > 0)
        {
            context.AdmitCards.AddRange(admitCards);
            await context.SaveChangesAsync();
        }

        return admitCards;
    }

    private async Task<FWU.Exam.Management.Infrastructure.Data.Models.AppUser?> ResolveStudentUserAsync(Domain.Entities.Exams.ExamRegistration registration)
    {
        if (registration.ApplicationVoucherId == null) return null;
        var voucher = await context.ApplicationVouchers.FindAsync(registration.ApplicationVoucherId.Value);
        if (voucher?.StudentRegistrationId == null) return null;
        var sr = await context.StudentRegistrations.FindAsync(voucher.StudentRegistrationId.Value);
        if (sr?.RegistrationNumber == null) return null;
        // Students have UserName = RegistrationNumber; legacy rows stored the
        // registration number in the Email column instead.
        return await context.Users.FirstOrDefaultAsync(u =>
            u.NormalizedUserName == sr.RegistrationNumber.ToUpperInvariant()
            || (u.Email != null && u.Email == sr.RegistrationNumber));
    }

    private async Task<string?> ResolveControllerSignatureAsync()
    {
        var tenant = await context.Tenants.FindAsync(tenantContext.TenantId);
        return tenant?.ControllerSignaturePath;
    }

    public async Task<AdmitCardSelectListsDto> GetSelectListDataAsync(AdmitCard? admitCard = null)
    {
        var examSchedules = await context.ExamSchedules.AsNoTracking().ToListAsync();
        var examRegistrations = await context.ExamRegistrations.AsNoTracking().ToListAsync();
        var examCenters = await context.ExamCenters.AsNoTracking().ToListAsync();

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
