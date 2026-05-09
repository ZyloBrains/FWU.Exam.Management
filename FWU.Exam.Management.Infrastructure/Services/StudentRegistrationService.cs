using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;
public class StudentRegistrationService : IStudentRegistrationService
{
    private readonly AppDbContext _context;

    public StudentRegistrationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentRegistration>> GetAllStudentRegistrationsAsync()
    {
        return await _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .OrderByDescending(s => s.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<StudentRegistration?> GetStudentRegistrationByIdAsync(int id)
    {
        return await _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> CreateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
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
            _context.Addresses.Add(permanentAddress);
            await _context.SaveChangesAsync();
            studentRegistration.PermanentAddressId = permanentAddress.Id;
        }

        _context.StudentRegistrations.Add(studentRegistration);
        await _context.SaveChangesAsync();
        return studentRegistration.Id;
    }

    public async Task UpdateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
    {
        if (!string.IsNullOrEmpty(permanentLocalLevelId))
        {
            var address = await _context.Addresses.FindAsync(studentRegistration.PermanentAddressId);
            if (address == null)
            {
                address = new Address
                {
                    LocalLevelId = int.Parse(permanentLocalLevelId),
                    WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber),
                    ToleStreet = permanentToleStreet,
                    HouseNumber = permanentHouseNumber,
                    AddressType = AddressType.Permanent,
                    IsActive = true
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                studentRegistration.PermanentAddressId = address.Id;
            }
            else
            {
                address.LocalLevelId = int.Parse(permanentLocalLevelId);
                address.WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber);
                address.ToleStreet = permanentToleStreet;
                address.HouseNumber = permanentHouseNumber;
                _context.Addresses.Update(address);
            }
        }

        _context.StudentRegistrations.Update(studentRegistration);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentRegistrationAsync(int id)
    {
        var studentRegistration = await _context.StudentRegistrations.FindAsync(id);
        if (studentRegistration != null)
        {
            _context.StudentRegistrations.Remove(studentRegistration);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> StudentRegistrationExistsAsync(int id)
    {
        return await _context.StudentRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<object> Data, int TotalCount)> GetPagedDataAsync(string searchTerm, int page, int pageSize)
    {
        var query = _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(s =>
                (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(lowerSearchTerm)) ||
                (s.FirstName != null && s.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(lowerSearchTerm)) ||
                (s.Email != null && s.Email.ToLower().Contains(lowerSearchTerm)) ||
                (s.ContactNumber != null && s.ContactNumber.ToLower().Contains(lowerSearchTerm)));
        }

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var data = await query
            .OrderByDescending(s => s.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(s => new
            {
                id = s.Id,
                registrationNumber = s.RegistrationNumber ?? "-",
                fullName = $"{s.FirstName} {s.LastName}".Trim(),
                academicYear = s.AcademicYear != null ? s.AcademicYear.AcademicYearName : "-",
                level = s.Level != null ? s.Level.LevelName : "-",
                college = s.College != null ? s.College.Name : "-",
                category = s.StudentCategory != null ? s.StudentCategory.StudentCategoryName : "-",
                contactNumber = s.ContactNumber ?? "-",
                email = s.Email ?? "-",
                status = s.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync();

        return (data.Cast<object>().ToList(), totalCount);
    }

    public async Task UpdateStatusAsync(int id, bool isActive)
    {
        var registration = await _context.StudentRegistrations.FindAsync(id);
        if (registration != null)
        {
            registration.IsActive = isActive;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<StudentRegistrationSelectListsDto> GetSelectListDataAsync(StudentRegistration? studentRegistration = null)
    {
        var academicYears = await _context.AcademicYears.Where(ay => ay.AcademicYearName != null).AsNoTracking().ToListAsync();
        var levels = await _context.Levels.Where(l => l.LevelName != null).AsNoTracking().ToListAsync();
        var faculties = await _context.Faculties.Where(f => f.FacultyName != null).AsNoTracking().ToListAsync();
        var colleges = await _context.Colleges.Where(c => c.Name != null).AsNoTracking().ToListAsync();
        var genders = await _context.Genders.Where(g => g.GenderName != null).AsNoTracking().ToListAsync();
        var studentCategories = await _context.StudentCategories.Where(sc => sc.StudentCategoryName != null).AsNoTracking().ToListAsync();
        var ethnicities = await _context.Ethnicities.Where(e => e.EthnicityName != null).AsNoTracking().ToListAsync();
        var localLevels = await _context.LocalLevels.Where(ll => ll.LocalLevelName != null).AsNoTracking().ToListAsync();

        return new StudentRegistrationSelectListsDto
        {
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Levels = levels.Select(l => new SelectOption { Id = l.Id, Name = l.LevelName }).ToList(),
            Faculties = faculties.Select(f => new SelectOption { Id = f.Id, Name = f.FacultyName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            Genders = genders.Select(g => new SelectOption { Id = g.Id, Name = g.GenderName }).ToList(),
            StudentCategories = studentCategories.Select(sc => new SelectOption { Id = sc.Id, Name = sc.StudentCategoryName }).ToList(),
            Ethnicities = ethnicities.Select(e => new SelectOption { Id = e.Id, Name = e.EthnicityName }).ToList(),
            LocalLevels = localLevels.Select(ll => new SelectOption { Id = ll.Id, Name = ll.LocalLevelName }).ToList()
        };
    }

    public async Task<List<object>> GetDistrictsByProvinceAsync(int provinceId)
    {
        var districts = await _context.Districts
            .Where(d => d.ProvinceId == provinceId && d.IsActive)
            .Select(d => new { id = d.Id, name = d.DistrictName })
            .ToListAsync();
        return districts.Cast<object>().ToList();
    }

    public async Task<List<object>> GetLocalLevelsByDistrictAsync(int districtId)
    {
        var localLevels = await _context.LocalLevels
            .Where(l => l.DistrictId == districtId && l.IsActive)
            .Select(l => new { id = l.Id, name = l.LocalLevelName })
            .ToListAsync();
        return localLevels.Cast<object>().ToList();
    }

    public List<Province> GetProvinces()
    {
        var provinces =  _context.Provinces.AsNoTracking().ToList();
        return provinces;
    }
}
