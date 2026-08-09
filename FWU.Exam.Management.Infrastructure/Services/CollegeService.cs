using System.Linq.Expressions;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeService(AppDbContext context, IUserContext userContext) : ICollegeService
{
    public async Task<(List<College> Items, int TotalCount)> GetCollegesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
        query = query.ApplyScope(userContext);

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

    public async Task<List<College>> GetFilteredItemsAsync(string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
        query = query.ApplyScope(userContext);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<College?> GetCollegeByIdAsync(int id)
    {
        return await context.Colleges
            .Include(c => c.CollegeType)
            .Include(c => c.Address)
            .ThenInclude(a => a!.LocalLevel)
            .ThenInclude(ll => ll!.District)
            .ThenInclude(d => d!.Province)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> CreateCollegeAsync(College college, string? localLevelId, string? wardNumber, string? toleStreet, string? houseNumber)
    {
        if (!string.IsNullOrEmpty(localLevelId))
        {
            var address = new Address
            {
                LocalLevelId = int.Parse(localLevelId),
                WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber),
                ToleStreet = toleStreet,
                HouseNumber = houseNumber,
                AddressType = AddressType.Current,
                IsActive = true
            };
            context.Addresses.Add(address);
            await context.SaveChangesAsync();
            college.AddressId = address.Id;
        }

        context.Colleges.Add(college);
        await context.SaveChangesAsync();
        return college.Id;
    }

    public async Task<int> UpdateCollegeAsync(College college, string? localLevelId, string? wardNumber, string? toleStreet, string? houseNumber)
    {
        var existingCollege = await context.Colleges
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.Id == college.Id)
            ?? throw new InvalidOperationException($"College with Id {college.Id} not found.");

        existingCollege.Code = college.Code;
        existingCollege.Name = college.Name;
        existingCollege.CollegeNameNepali = college.CollegeNameNepali;
        existingCollege.ShortName = college.ShortName;
        existingCollege.EstablishedDate = college.EstablishedDate;
        existingCollege.ClosedDate = college.ClosedDate;
        existingCollege.Website = college.Website;
        existingCollege.Email = college.Email;
        existingCollege.Phone1 = college.Phone1;
        existingCollege.Phone2 = college.Phone2;
        existingCollege.PrincipalName = college.PrincipalName;
        existingCollege.PrincipalContactNumber = college.PrincipalContactNumber;
        existingCollege.Fax = college.Fax;
        existingCollege.Remarks = college.Remarks;
        existingCollege.IsExamCenterOnly = college.IsExamCenterOnly;
        existingCollege.IsActive = college.IsActive;
        existingCollege.AllocatedAmount = college.AllocatedAmount;
        existingCollege.DisplayOrder = college.DisplayOrder;
        existingCollege.CollegeTypeId = college.CollegeTypeId;
        existingCollege.CollegeProfileId = college.CollegeProfileId;

        if (!string.IsNullOrEmpty(localLevelId))
        {
            var address = existingCollege.Address;
            if (address == null)
            {
                address = new Address
                {
                    LocalLevelId = int.Parse(localLevelId),
                    WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber),
                    ToleStreet = toleStreet,
                    HouseNumber = houseNumber,
                    AddressType = AddressType.Current,
                    IsActive = true
                };
                context.Addresses.Add(address);
                await context.SaveChangesAsync();
                existingCollege.AddressId = address.Id;
            }
            else
            {
                address.LocalLevelId = int.Parse(localLevelId);
                address.WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber);
                address.ToleStreet = toleStreet;
                address.HouseNumber = houseNumber;
            }
        }

        await context.SaveChangesAsync();
        return existingCollege.Id;
    }

    public async Task DeleteCollegeAsync(int id)
    {
        var college = await context.Colleges.FindAsync(id);
        if (college != null)
        {
            context.Colleges.Remove(college);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> CollegeExistsAsync(int id)
    {
        return await context.Colleges.AnyAsync(c => c.Id == id);
    }

    public async Task<List<CollegeType>> GetCollegeTypesAsync()
    {
        return await context.CollegeTypes.AsNoTracking().ToListAsync();
    }

    private IQueryable<College> BuildQuery(string? search)
    {
        var query = context.Colleges
            .Include(c => c.CollegeType)
            .Include(c => c.Address)
            .ThenInclude(a => a!.LocalLevel)
            .ThenInclude(ll => ll!.District)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Code.ToString().Contains(search) ||
                c.Name.Contains(search) ||
                (c.ShortName ?? "").Contains(search) ||
                c.Email.Contains(search) ||
                (c.Phone1 ?? "").Contains(search) ||
                (c.Phone2 ?? "").Contains(search) ||
                c.PrincipalName.Contains(search) ||
                (c.Remarks ?? "").Contains(search) ||
                (c.Address != null && c.Address.LocalLevel != null && c.Address.LocalLevel.District != null && c.Address.LocalLevel.District.DistrictName.Contains(search)) ||
                (c.CollegeType != null && c.CollegeType.Code.Contains(search)));
        }

        return query;
    }

    private static Expression<Func<College, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => c => c.Code,
            "name" => c => c.Name,
            "shortname" => c => c.ShortName ?? "",
            "district" => c => (c.Address != null && c.Address.LocalLevel != null && c.Address.LocalLevel.District != null) ? c.Address.LocalLevel.District.DistrictName : "",
            "collegetype" => c => c.CollegeType != null ? c.CollegeType.Code : "",
            "displayorder" => c => c.DisplayOrder ?? 0,
            "isactive" => c => c.IsActive,
            _ => c => c.DisplayOrder ?? 0
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

    public async Task<List<Province>> GetProvincesAsync()
    {
        var provinces = await context.Provinces.AsNoTracking().ToListAsync();
        return provinces;
    }
}
