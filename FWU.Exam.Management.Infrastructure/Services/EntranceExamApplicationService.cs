using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class EntranceExamApplicationService(AppDbContext context) : IEntranceExamApplicationService
{
    public async Task<int> SubmitApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
    {
        if (!string.IsNullOrEmpty(permanentLocalLevelId))
        {
            var permanentAddress = new Address
            {
                LocalLevelId = int.Parse(permanentLocalLevelId),
                WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber),
                ToleStreet = permanentToleStreet,
                HouseNumber = permanentHouseNumber,
                AddressType = AddressType.Permanent,
                IsActive = true
            };
            context.Addresses.Add(permanentAddress);
            await context.SaveChangesAsync();
            application.PermanentAddressId = permanentAddress.Id;
        }

        application.Status = ApplicationStatus.Submitted;
        application.CreatedAt = DateTime.UtcNow;

        context.EntranceExamApplications.Add(application);
        await context.SaveChangesAsync();
        return application.Id;
    }

    public async Task<EntranceExamApplication?> GetApplicationByIdAsync(int id)
    {
        return await context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .Include(a => a.Gender)
            .Include(a => a.PermanentAddress)
                .ThenInclude(pa => pa != null ? pa.LocalLevel : null)
                    .ThenInclude(ll => ll != null ? ll.District : null)
                        .ThenInclude(d => d != null ? d.Province : null)
            .Include(a => a.PreviousLevel)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<(List<EntranceExamApplicationListDto> Data, int TotalCount)> GetPagedApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId, int page, int pageSize)
    {
        var query = context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(a =>
                (a.FirstName != null && a.FirstName.ToLower().Contains(lowerSearch)) ||
                (a.LastName != null && a.LastName.ToLower().Contains(lowerSearch)) ||
                (a.Email != null && a.Email.ToLower().Contains(lowerSearch)) ||
                (a.ContactNumber != null && a.ContactNumber.ToLower().Contains(lowerSearch)));
        }

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (programId.HasValue)
            query = query.Where(a => a.ProgramId == programId.Value);

        if (academicYearId.HasValue)
            query = query.Where(a => a.AcademicYearId == academicYearId.Value);

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new EntranceExamApplicationListDto
            {
                Id = a.Id,
                FullName = (a.FirstName + " " + a.LastName).Trim(),
                Email = a.Email ?? "-",
                ContactNumber = a.ContactNumber ?? "-",
                AcademicYear = a.AcademicYear != null ? a.AcademicYear.AcademicYearName : "-",
                College = a.College != null ? a.College.Name : "-",
                Program = a.Program != null ? a.Program.ProgramName : "-",
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task ReviewApplicationAsync(int id, ApplicationStatus status, string? remarks)
    {
        var application = await context.EntranceExamApplications.FindAsync(id);
        if (application != null)
        {
            application.Status = status;
            application.ReviewDate = DateTime.UtcNow;
            application.ReviewRemarks = remarks;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteApplicationAsync(int id)
    {
        var application = await context.EntranceExamApplications.FindAsync(id);
        if (application != null)
        {
            context.EntranceExamApplications.Remove(application);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ApplicationExistsAsync(int id)
    {
        return await context.EntranceExamApplications.AnyAsync(a => a.Id == id);
    }

    public async Task<EntranceExamApplicationSelectListsDto> GetSelectListsAsync()
    {
        var academicYears = await context.AcademicYears.Where(ay => ay.AcademicYearName != null && ay.IsActive).AsNoTracking().ToListAsync();
        var colleges = await context.Colleges.Where(c => c.Name != null && c.IsActive).AsNoTracking().ToListAsync();
        var programs = await context.Programs.Where(p => p.ProgramName != null && p.IsActive).AsNoTracking().ToListAsync();
        var genders = await context.Genders.Where(g => g.GenderName != null && g.IsActive).AsNoTracking().ToListAsync();
        var previousLevels = await context.PreviousLevels.Where(pl => pl.PreviousLevelName != null && pl.IsActive).AsNoTracking().ToListAsync();
        var provinces = await context.Provinces.AsNoTracking().ToListAsync();

        return new EntranceExamApplicationSelectListsDto
        {
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            Genders = genders.Select(g => new SelectOption { Id = g.Id, Name = g.GenderName }).ToList(),
            PreviousLevels = previousLevels.Select(pl => new SelectOption { Id = pl.Id, Name = pl.PreviousLevelName }).ToList(),
            Provinces = provinces.Select(p => new SelectOption { Id = p.Id, Name = p.ProvinceName }).ToList(),
        };
    }

    public async Task<List<SelectOption>> GetDistrictsByProvinceAsync(int provinceId)
    {
        return await context.Districts
            .Where(d => d.ProvinceId == provinceId && d.IsActive)
            .Select(d => new SelectOption { Id = d.Id, Name = d.DistrictName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetLocalLevelsByDistrictAsync(int districtId)
    {
        return await context.LocalLevels
            .Where(l => l.DistrictId == districtId && l.IsActive)
            .Select(l => new SelectOption { Id = l.Id, Name = l.LocalLevelName })
            .ToListAsync();
    }

    public List<Province> GetProvinces()
    {
        return context.Provinces.AsNoTracking().ToList();
    }

    public async Task<List<EntranceExamApplication>> GetAllApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId)
    {
        var query = context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .Include(a => a.Gender)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(a =>
                (a.FirstName != null && a.FirstName.ToLower().Contains(lowerSearch)) ||
                (a.LastName != null && a.LastName.ToLower().Contains(lowerSearch)) ||
                (a.Email != null && a.Email.ToLower().Contains(lowerSearch)) ||
                (a.ContactNumber != null && a.ContactNumber.ToLower().Contains(lowerSearch)));
        }

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (programId.HasValue) query = query.Where(a => a.ProgramId == programId.Value);
        if (academicYearId.HasValue) query = query.Where(a => a.AcademicYearId == academicYearId.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }
  
}
