using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;
public class StudentRegistrationService(AppDbContext context, UserManager<AppUser> userManager, ILogger<StudentRegistrationService> logger) : IStudentRegistrationService
{
    private const string MustChangePasswordClaimType = "must_change_password";

    public async Task<List<StudentRegistration>> GetAllStudentRegistrationsAsync(List<int>? collegeIds = null)
    {
        var query = context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Department)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .AsNoTracking();

        if (collegeIds != null && collegeIds.Count > 0)
        {
            query = query.Where(s => collegeIds.Contains(s.CollegeId));
        }

        return await query
            .OrderByDescending(s => s.Id)
            .ToListAsync();
    }

    public async Task<StudentRegistration?> GetStudentRegistrationByIdAsync(int id)
    {
        return await context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Department)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> CreateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
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
                studentRegistration.PermanentAddressId = permanentAddress.Id;
            }

            context.StudentRegistrations.Add(studentRegistration);
            await context.SaveChangesAsync();

            await EnsureStudentAppUserAsync(studentRegistration);

            await transaction.CommitAsync();
            return studentRegistration.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            if (!string.IsNullOrEmpty(permanentLocalLevelId))
            {
                var address = await context.Addresses.FindAsync(studentRegistration.PermanentAddressId);
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
                    context.Addresses.Add(address);
                    await context.SaveChangesAsync();
                    studentRegistration.PermanentAddressId = address.Id;
                }
                else
                {
                    address.LocalLevelId = int.Parse(permanentLocalLevelId);
                    address.WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber);
                    address.ToleStreet = permanentToleStreet;
                    address.HouseNumber = permanentHouseNumber;
                    context.Addresses.Update(address);
                }
            }

            context.StudentRegistrations.Update(studentRegistration);
            await context.SaveChangesAsync();

            await EnsureStudentAppUserAsync(studentRegistration);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteStudentRegistrationAsync(int id)
    {
        var studentRegistration = await context.StudentRegistrations.FindAsync(id);
        if (studentRegistration != null)
        {
            context.StudentRegistrations.Remove(studentRegistration);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> StudentRegistrationExistsAsync(int id)
    {
        return await context.StudentRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<StudentRegistrationListDto> Data, int TotalCount)> GetPagedDataAsync(string searchTerm, int page, int pageSize, List<int>? collegeIds = null)
    {
        var query = context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Department)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .AsNoTracking();

        if (collegeIds != null && collegeIds.Count > 0)
        {
            query = query.Where(s => collegeIds.Contains(s.CollegeId));
        }

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
            .Select(s => new StudentRegistrationListDto
            {
                Id = s.Id,
                RegistrationNumber = s.RegistrationNumber ?? "-",
                FullName = (s.FirstName + " " + s.LastName).Trim(),
                AcademicYear = s.AcademicYear != null ? s.AcademicYear.AcademicYearName : "-",
                Level = s.Level != null ? s.Level.LevelName : "-",
                College = s.College != null ? s.College.Name : "-",
                Faculty = s.Faculty != null ? s.Faculty.Name : "-",
                Program = s.Program != null ? s.Program.ProgramName : "-",
                Category = s.StudentCategory != null ? s.StudentCategory.StudentCategoryName : "-",
                ContactNumber = s.ContactNumber ?? "-",
                Email = s.Email ?? "-",
                DepartmentId = s.DepartmentId,
                Status = s.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task UpdateStatusAsync(int id, bool isActive)
    {
        var registration = await context.StudentRegistrations.FindAsync(id);
        if (registration != null)
        {
            registration.IsActive = isActive;
            await context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(registration.Email))
            {
                var user = await userManager.FindByEmailAsync(registration.Email);
                if (user != null)
                {
                    user.IsActive = isActive;
                    await userManager.UpdateAsync(user);
                }
            }
        }
    }

    public async Task<StudentRegistrationSelectListsDto> GetSelectListDataAsync(StudentRegistration? studentRegistration = null)
    {
        var academicYears = await context.AcademicYears.Where(ay => ay.AcademicYearName != null).AsNoTracking().ToListAsync();
        var levels = await context.Levels.Where(l => l.LevelName != null).AsNoTracking().ToListAsync();
        var departments = await context.Departments.Where(d => d.DepartmentName != null).AsNoTracking().ToListAsync();
        var colleges = await context.Colleges.Where(c => c.Name != null).AsNoTracking().ToListAsync();
        var genders = await context.Genders.Where(g => g.GenderName != null).AsNoTracking().ToListAsync();
        var studentCategories = await context.StudentCategories.Where(sc => sc.StudentCategoryName != null).AsNoTracking().ToListAsync();
        var ethnicities = await context.Ethnicities.Where(e => e.EthnicityName != null).AsNoTracking().ToListAsync();
        var localLevels = await context.LocalLevels.Where(ll => ll.LocalLevelName != null).AsNoTracking().ToListAsync();
        var faculties = await context.Faculties.Where(f => f.Name != null).AsNoTracking().ToListAsync();
        var programs = await context.Programs.Where(p => p.ProgramName != null).AsNoTracking().ToListAsync();
        var boards = await context.Boards.Where(b => b.BoardName != null).AsNoTracking().ToListAsync();
        var previousLevels = await context.PreviousLevels.Where(pl => pl.PreviousLevelName != null).AsNoTracking().ToListAsync();

        return new StudentRegistrationSelectListsDto
        {
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Levels = levels.Select(l => new SelectOption { Id = l.Id, Name = l.LevelName }).ToList(),
            Departments = departments.Select(d => new SelectOption { Id = d.Id, Name = d.DepartmentName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            Genders = genders.Select(g => new SelectOption { Id = g.Id, Name = g.GenderName }).ToList(),
            StudentCategories = studentCategories.Select(sc => new SelectOption { Id = sc.Id, Name = sc.StudentCategoryName }).ToList(),
            Ethnicities = ethnicities.Select(e => new SelectOption { Id = e.Id, Name = e.EthnicityName }).ToList(),
            LocalLevels = localLevels.Select(ll => new SelectOption { Id = ll.Id, Name = ll.LocalLevelName }).ToList(),
            Faculties = faculties.Select(f => new SelectOption { Id = f.Id, Name = f.Name }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            Boards = boards.Select(b => new SelectOption { Id = b.Id, Name = b.BoardName }).ToList(),
            PreviousLevels = previousLevels.Select(pl => new SelectOption { Id = pl.Id, Name = pl.PreviousLevelName }).ToList(),
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

    public async Task<List<SelectOption>> GetFacultiesByLevelAsync(int levelId)
    {
        var collegeIds = await context.Programs
            .Where(p => p.LevelId == levelId)
            .Join(context.CollegePrograms, p => p.Id, cp => cp.ProgramId, (p, cp) => cp.CollegeId)
            .Distinct()
            .ToListAsync();

        if (collegeIds.Count == 0) return [];

        return await context.Colleges
            .Where(c => collegeIds.Contains(c.Id))
            .SelectMany(c => c.Faculties)
            .Select(f => new SelectOption { Id = f.Id, Name = f.Name })
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetDepartmentsByCollegeAsync(int collegeId)
    {
        var departmentIds = await context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId)
            .Join(context.Programs, cp => cp.ProgramId, p => p.Id, (cp, p) => p.DepartmentId)
            .Distinct()
            .ToListAsync();

        if (departmentIds.Count == 0) return [];

        return await context.Departments
            .Where(d => departmentIds.Contains(d.Id))
            .Select(d => new SelectOption { Id = d.Id, Name = d.DepartmentName })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetProgramsByCollegeAsync(int collegeId, int? levelId = null, int? departmentId = null)
    {
        var query = context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId && cp.Program != null && cp.Program.ProgramName != null)
            .Include(cp => cp.Program)
            .AsQueryable();

        if (levelId.HasValue)
            query = query.Where(cp => cp.Program!.LevelId == levelId.Value);

        if (departmentId.HasValue)
            query = query.Where(cp => cp.Program!.DepartmentId == departmentId.Value);

        return await query
            .Select(cp => new SelectOption { Id = cp.Program!.Id, Name = cp.Program.ProgramName })
            .AsNoTracking()
            .ToListAsync();
    }

    public List<Province> GetProvinces()
    {
        var provinces =  context.Provinces.AsNoTracking().ToList();
        return provinces;
    }

    public async Task SaveQualificationsAsync(int studentRegistrationId, List<StudentQualification> qualifications)
    {
        var existing = await context.StudentQualifications
            .Where(q => q.StudentRegistrationId == studentRegistrationId)
            .ToListAsync();

        context.StudentQualifications.RemoveRange(existing);

        foreach (var q in qualifications)
        {
            q.Id = 0;
            q.StudentRegistrationId = studentRegistrationId;
            context.StudentQualifications.Add(q);
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<StudentQualification>> GetQualificationsByRegistrationAsync(int studentRegistrationId)
    {
        return await context.StudentQualifications
            .Include(q => q.Board)
            .Include(q => q.PreviousLevel)
            .Where(q => q.StudentRegistrationId == studentRegistrationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SaveGuardiansAsync(int studentRegistrationId, StudentGuardian guardian)
    {
        var existing = await context.StudentGuardians
            .Where(g => g.StudentRegistrationId == studentRegistrationId)
            .ToListAsync();

        context.StudentGuardians.RemoveRange(existing);

        if (guardian != null)
        {
            guardian.Id = 0;
            guardian.StudentRegistrationId = studentRegistrationId;
            context.StudentGuardians.Add(guardian);
        }

        await context.SaveChangesAsync();
    }

    public async Task<StudentGuardian?> GetGuardianByRegistrationAsync(int studentRegistrationId)
    {
        return await context.StudentGuardians
            .Where(g => g.StudentRegistrationId == studentRegistrationId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    private async Task EnsureStudentAppUserAsync(StudentRegistration studentRegistration)
    {
        if (string.IsNullOrWhiteSpace(studentRegistration.Email))
            return;

        var user = await userManager.FindByEmailAsync(studentRegistration.Email);

        if (user == null && studentRegistration.Id != 0)
        {
            var existingStudentEmail = await context.StudentRegistrations
                .AsNoTracking()
                .Where(s => s.Id == studentRegistration.Id)
                .Select(s => s.Email)
                .SingleOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(existingStudentEmail) &&
                !string.Equals(existingStudentEmail, studentRegistration.Email, StringComparison.OrdinalIgnoreCase))
            {
                user = await userManager.FindByEmailAsync(existingStudentEmail);
            }
        }

        if (user == null)
        {
            user = new AppUser
            {
                UserName = studentRegistration.Email,
                Email = studentRegistration.Email,
                FullName = $"{studentRegistration.FirstName} {studentRegistration.LastName}".Trim(),
                IsActive = true,
                FacultyId = studentRegistration.FacultyId,
                CollegeId = studentRegistration.CollegeId
            };

            var password = studentRegistration.DateOfBirthBS;
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"DateOfBirthBS is required to create login for student {studentRegistration.Email}");

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create AppUser for student {Email}: {Errors}", studentRegistration.Email, errors);
                throw new InvalidOperationException($"Failed to create user account for {studentRegistration.Email}: {errors}");
            }

            if (!await userManager.IsInRoleAsync(user, "Student"))
                await userManager.AddToRoleAsync(user, "Student");

            await userManager.AddClaimAsync(user, new Claim(MustChangePasswordClaimType, "true"));

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }
        else
        {
            var needsUpdate = false;

            if (user.Email != studentRegistration.Email)
            {
                user.Email = studentRegistration.Email;
                user.UserName = studentRegistration.Email;
                needsUpdate = true;
            }

            if (user.FullName != $"{studentRegistration.FirstName} {studentRegistration.LastName}".Trim())
            {
                user.FullName = $"{studentRegistration.FirstName} {studentRegistration.LastName}".Trim();
                needsUpdate = true;
            }

            if (user.IsActive != studentRegistration.IsActive)
            {
                user.IsActive = studentRegistration.IsActive;
                needsUpdate = true;
            }

            if (user.FacultyId != studentRegistration.FacultyId)
            {
                user.FacultyId = studentRegistration.FacultyId;
                needsUpdate = true;
            }

            if (user.CollegeId != studentRegistration.CollegeId)
            {
                user.CollegeId = studentRegistration.CollegeId;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to update existing student user {Email}: {Errors}", studentRegistration.Email, errors);
                    return;
                }
            }

            var isStudent = await userManager.IsInRoleAsync(user, "Student");
            if (!isStudent)
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, "Student");
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add existing user {Email} to Student role: {Errors}", studentRegistration.Email, errors);
                    return;
                }
            }

            var password = studentRegistration.DateOfBirthBS;
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"DateOfBirthBS is required to reset login for student {studentRegistration.Email}");

            var passwordValid = await userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to reset password for student {Email}: {Errors}", studentRegistration.Email, errors);
                    throw new InvalidOperationException($"Failed to reset user password for {studentRegistration.Email}: {errors}");
                }
            }
        }
    }
}
