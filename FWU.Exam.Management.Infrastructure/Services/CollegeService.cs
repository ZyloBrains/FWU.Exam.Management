using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeService : ICollegeService
{
    private readonly AppDbContext _context;

    public CollegeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<College> Items, int TotalCount)> GetCollegesAsync(int page, int pageSize, string? search, string sort, string sortDir)
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

    public async Task<List<College>> GetFilteredItemsAsync(string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<College?> GetCollegeByIdAsync(int id)
    {
        return await _context.Colleges
            .Include(c => c.CollegeType)
            .Include(c => c.Address)
            .ThenInclude(a => a.LocalLevel)
            .ThenInclude(ll => ll.District)
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
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            college.AddressId = address.Id;
        }

        _context.Colleges.Add(college);
        await _context.SaveChangesAsync();
        return college.Id;
    }

    public async Task<int> UpdateCollegeAsync(College college, string? localLevelId, string? wardNumber, string? toleStreet, string? houseNumber)
    {
        if (!string.IsNullOrEmpty(localLevelId))
        {
            var address = await _context.Addresses.FindAsync(college.AddressId);
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
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                college.AddressId = address.Id;
            }
            else
            {
                address.LocalLevelId = int.Parse(localLevelId);
                address.WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber);
                address.ToleStreet = toleStreet;
                address.HouseNumber = houseNumber;
                _context.Addresses.Update(address);
            }
        }

        _context.Colleges.Update(college);
        await _context.SaveChangesAsync();
        return college.Id;
    }

    public async Task DeleteCollegeAsync(int id)
    {
        var college = await _context.Colleges.FindAsync(id);
        if (college != null)
        {
            _context.Colleges.Remove(college);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CollegeExistsAsync(int id)
    {
        return await _context.Colleges.AnyAsync(c => c.Id == id);
    }

    public async Task<List<CollegeType>> GetCollegeTypesAsync()
    {
        return await _context.CollegeTypes.AsNoTracking().ToListAsync();
    }

    private IQueryable<College> BuildQuery(string? search)
    {
        var query = _context.Colleges
            .Include(c => c.CollegeType)
            .Include(c => c.Address)
            .ThenInclude(a => a.LocalLevel)
            .ThenInclude(ll => ll.District)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Code.ToString().Contains(search) ||
                c.Name.Contains(search) ||
                c.CollegeNameNepali.Contains(search) ||
                c.ShortName.Contains(search) ||
                c.Email.Contains(search) ||
                c.Phone1.Contains(search) ||
                c.Phone2.Contains(search) ||
                c.PrincipalName.Contains(search) ||
                c.Remarks.Contains(search) ||
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
            "shortname" => c => c.ShortName,
            "district" => c => c.Address.LocalLevel.District.DistrictName,
            "collegetype" => c => c.CollegeType.Code,
            "displayorder" => c => c.DisplayOrder,
            "isactive" => c => c.IsActive,
            _ => c => c.DisplayOrder
        };
    }
}
