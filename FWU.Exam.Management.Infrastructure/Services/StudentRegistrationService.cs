using System.Security.Claims;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;
public class StudentRegistrationService(AppDbContext context, UserManager<AppUser> userManager, ILogger<StudentRegistrationService> logger, IGumpNowEmailService gumpEmailService, ISmsService smsService, IUserContext userContext) : IStudentRegistrationService
{
    private const string MustChangePasswordClaimType = "must_change_password";

    public async Task<List<StudentRegistration>> GetAllStudentRegistrationsAsync(List<int>? collegeIds = null, string? academicYear = null, int? facultyId = null, int? collegeId = null, int? levelId = null, string? status = null)
    {
        var query = context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.PermanentAddress)
                .ThenInclude(a => a!.LocalLevel)
                .ThenInclude(ll => ll!.District)
                .ThenInclude(d => d!.Province)
            .Include(s => s.StudentGuardians)
            .Include(s => s.StudentQualifications)
                .ThenInclude(q => q.Board)
            .Include(s => s.StudentQualifications)
                .ThenInclude(q => q.PreviousLevel)
            .AsNoTracking();
        query = query.ApplyScope(userContext);

        if (userContext.IsSuperAdmin && collegeIds != null && collegeIds.Count > 0)
        {
            query = query.Where(s => collegeIds.Contains(s.CollegeId));
        }

        if (!string.IsNullOrWhiteSpace(academicYear))
            query = query.Where(s => s.AcademicYear != null && s.AcademicYear.AcademicYearName == academicYear);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);
        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (levelId.HasValue)
            query = query.Where(s => s.LevelId == levelId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var isActive = status.ToLower() == "active";
            query = query.Where(s => s.IsActive == isActive);
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
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .Include(s => s.PermanentAddress)
                .ThenInclude(a => a!.LocalLevel)
                .ThenInclude(ll => ll!.District)
                .ThenInclude(d => d!.Province)
            .Include(s => s.CurrentAddress)
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

            studentRegistration.IsActive = true;
            context.StudentRegistrations.Add(studentRegistration);
            await context.SaveChangesAsync();

            // Generate the registration number before creating the AppUser so the user's
            // UserName can be set to the registration number directly.
            var registrationNumber = await GenerateRegistrationNumberAsync(studentRegistration.Id);

            await EnsureStudentAppUserAsync(studentRegistration);

            await transaction.CommitAsync();

            await SendStudentRegistrationNotificationsAsync(studentRegistration, registrationNumber);

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
            var existingRegistration = await context.StudentRegistrations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == studentRegistration.Id);
            if (existingRegistration != null)
                studentRegistration.TenantId = existingRegistration.TenantId;

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

            if (existingRegistration != null && !string.IsNullOrEmpty(existingRegistration.RegistrationNumber))
            {
                studentRegistration.RegistrationNumber = existingRegistration.RegistrationNumber;
                studentRegistration.IsRegistrationNumberGenerated = existingRegistration.IsRegistrationNumberGenerated;
                studentRegistration.StudentRegistrationIndex = existingRegistration.StudentRegistrationIndex;
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
        var studentRegistration = await context.StudentRegistrations
            .Include(sr => sr.StudentGuardians)
            .Include(sr => sr.StudentQualifications)
            .Include(sr => sr.ApplicationVouchers)
            .Include(sr => sr.PaymentRequestLogs)
            .Include(sr => sr.StudentAdmission)
            .FirstOrDefaultAsync(sr => sr.Id == id);
        if (studentRegistration != null)
        {
            if (studentRegistration.StudentGuardians?.Count > 0)
                context.StudentGuardians.RemoveRange(studentRegistration.StudentGuardians);
            if (studentRegistration.StudentQualifications?.Count > 0)
                context.StudentQualifications.RemoveRange(studentRegistration.StudentQualifications);
            if (studentRegistration.ApplicationVouchers?.Count > 0)
                context.ApplicationVouchers.RemoveRange(studentRegistration.ApplicationVouchers);
            if (studentRegistration.PaymentRequestLogs?.Count > 0)
                context.PaymentRequestLogs.RemoveRange(studentRegistration.PaymentRequestLogs);
            if (studentRegistration.StudentAdmission != null)
                context.StudentAdmissions.Remove(studentRegistration.StudentAdmission);

            context.StudentRegistrations.Remove(studentRegistration);
            await context.SaveChangesAsync();
        }
    }

    public async Task<string?> GenerateRegistrationNumberAsync(int studentRegistrationId)
    {
        var student = await context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .FirstOrDefaultAsync(s => s.Id == studentRegistrationId);

        if (student == null) return null;
        if (student.IsRegistrationNumberGenerated == true)
        {
            await SyncStudentUserNameAsync(student);
            return student.RegistrationNumber;
        }
        if (!string.IsNullOrEmpty(student.RegistrationNumber))
        {
            student.IsRegistrationNumberGenerated = true;
            await context.SaveChangesAsync();
            await SyncStudentUserNameAsync(student);
            return student.RegistrationNumber;
        }

        var maxIndex = await context.StudentRegistrations
            .Where(s => s.AcademicYearId == student.AcademicYearId && s.StudentRegistrationIndex != null)
            .MaxAsync(s => (int?)s.StudentRegistrationIndex) ?? 0;

        var newIndex = maxIndex + 1;
        student.StudentRegistrationIndex = newIndex;

        student.RegistrationNumber = $"{student.Faculty?.ShortName ?? "00"}-{student.AcademicYear?.AcademicYearCode ?? "0"}-{student.LevelId}-{student.ProgramId}-{newIndex:D4}";

        student.IsRegistrationNumberGenerated = true;
        await context.SaveChangesAsync();

        await SyncStudentUserNameAsync(student);

        return student.RegistrationNumber;
    }

    private async Task SyncStudentUserNameAsync(StudentRegistration student)
    {
        if (string.IsNullOrWhiteSpace(student.RegistrationNumber))
            return;

        AppUser? user = null;

        if (!string.IsNullOrWhiteSpace(student.Email))
            user = await userManager.FindByEmailAsync(student.Email);

        if (user == null && student.StudentAdmissionId != null)
        {
            var appUserId = await context.StudentAdmissions
                .AsNoTracking()
                .Where(sa => sa.Id == student.StudentAdmissionId.Value && sa.AppUserId != null)
                .Select(sa => sa.AppUserId!)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(appUserId))
                user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == appUserId);
        }

        if (user == null || string.Equals(user.UserName, student.RegistrationNumber, StringComparison.OrdinalIgnoreCase))
            return;

        user.UserName = student.RegistrationNumber;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to sync UserName to registration number for student {Id}: {Errors}", student.Id, errors);
        }
    }

    public async Task<bool> StudentRegistrationExistsAsync(int id)
    {
        return await context.StudentRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<StudentRegistrationListDto> Data, int TotalCount)> GetPagedDataAsync(string searchTerm, int page, int pageSize, List<int>? collegeIds = null, string? academicYear = null, int? facultyId = null, int? collegeId = null, int? levelId = null, int? programId = null, string? status = null)
    {
        var query = context.StudentRegistrations
            .AsNoTracking();
        query = query.ApplyScope(userContext);

        if (userContext.IsSuperAdmin && collegeIds != null && collegeIds.Count > 0)
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

        if (!string.IsNullOrWhiteSpace(academicYear))
            query = query.Where(s => s.AcademicYear != null && s.AcademicYear.AcademicYearName == academicYear);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);
        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (levelId.HasValue)
            query = query.Where(s => s.LevelId == levelId.Value);
        if (programId.HasValue)
            query = query.Where(s => s.ProgramId == programId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var isActive = status.ToLower() == "active";
            query = query.Where(s => s.IsActive == isActive);
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
        var colleges = await context.Colleges.Where(c => c.Name != null).AsNoTracking().ApplyScope(userContext).ToListAsync();
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
        return await context.Programs
            .Where(p => p.LevelId == levelId && p.FacultyId != null)
            .Include(p => p.Faculty)
            .Select(p => new SelectOption { Id = p.Faculty!.Id, Name = p.Faculty.Name! })
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetCollegesByLevelAsync(int levelId)
    {
        return await context.CollegePrograms
            .Where(cp => cp.Program != null && cp.Program.LevelId == levelId && cp.College != null && cp.College.Name != null)
            .Select(cp => new SelectOption { Id = cp.College!.Id, Name = cp.College.Name! })
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetProgramsByCollegeAsync(int collegeId, int? levelId = null)
    {
        var query = context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId && cp.Program != null && cp.Program.ProgramName != null)
            .Include(cp => cp.Program)
            .AsQueryable();

        if (levelId.HasValue)
            query = query.Where(cp => cp.Program!.LevelId == levelId.Value);

        return await query
            .Select(cp => new SelectOption { Id = cp.Program!.Id, Name = cp.Program.ProgramName })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Province>> GetProvincesAsync()
    {
        return await context.Provinces.AsNoTracking().ToListAsync();
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

    public async Task SaveGuardiansAsync(int studentRegistrationId, StudentGuardian? guardian)
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

    private async Task<bool> EnsureStudentAppUserAsync(StudentRegistration studentRegistration)
    {
        // The registration number is the primary login identifier for students.
        var loginId = studentRegistration.RegistrationNumber;
        if (string.IsNullOrWhiteSpace(loginId))
            loginId = studentRegistration.Email;

        if (string.IsNullOrWhiteSpace(loginId))
            return false;

        var user = !string.IsNullOrWhiteSpace(studentRegistration.Email)
            ? await userManager.FindByEmailAsync(studentRegistration.Email)
            : null;

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
                UserName = loginId,
                Email = studentRegistration.Email,
                FullName = studentRegistration.FirstName.GetFullName(studentRegistration.LastName),
                IsActive = true,
                FacultyId = studentRegistration.FacultyId,
                CollegeId = studentRegistration.CollegeId
            };

            var password = studentRegistration.DateOfBirthBS;
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"DateOfBirthBS is required to create login for student {loginId}");

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create AppUser for student {LoginId}: {Errors}", loginId, errors);
                throw new InvalidOperationException($"Failed to create user account for {loginId}: {errors}");
            }

            SetPasswordHashDirectly(user, password);
            await userManager.UpdateAsync(user);

            if (!await userManager.IsInRoleAsync(user, Role.Student))
                await userManager.AddToRoleAsync(user, Role.Student);

            await userManager.AddClaimAsync(user, new Claim(MustChangePasswordClaimType, "true"));

            // Student emails are NOT auto-confirmed; the student verifies them from their profile.
            return true;
        }
        else
        {
            var needsUpdate = false;

            // Sync email only when the registration has one; a student-managed email (added or
            // changed in the profile) must never be overwritten with a null registration email.
            if (!string.IsNullOrWhiteSpace(studentRegistration.Email) &&
                !string.Equals(user.Email, studentRegistration.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = studentRegistration.Email;
                user.EmailConfirmed = false;
                needsUpdate = true;
            }

            // Username is the registration number for students.
            if (!string.IsNullOrWhiteSpace(studentRegistration.RegistrationNumber) &&
                !string.Equals(user.UserName, studentRegistration.RegistrationNumber, StringComparison.OrdinalIgnoreCase))
            {
                user.UserName = studentRegistration.RegistrationNumber;
                needsUpdate = true;
            }

            if (user.FullName != studentRegistration.FirstName.GetFullName(studentRegistration.LastName))
            {
                user.FullName = studentRegistration.FirstName.GetFullName(studentRegistration.LastName);
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
                    logger.LogError("Failed to update existing student user {LoginId}: {Errors}", loginId, errors);
                    return false;
                }
            }

            var isStudent = await userManager.IsInRoleAsync(user, Role.Student);
            if (!isStudent)
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, Role.Student);
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add existing user {LoginId} to Student role: {Errors}", loginId, errors);
                    return false;
                }
            }

            var password = studentRegistration.DateOfBirthBS;
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"DateOfBirthBS is required to reset login for student {loginId}");

            var passwordValid = await userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                SetPasswordHashDirectly(user, password);
                await userManager.UpdateAsync(user);
            }

            return false;
        }
    }

    private static void SetPasswordHashDirectly(AppUser user, string password)
    {
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);
    }

    public async Task<string?> SendRegistrationNotificationsAsync(int studentRegistrationId)
    {
        var studentRegistration = await context.StudentRegistrations.FirstOrDefaultAsync(s => s.Id == studentRegistrationId);
        if (studentRegistration == null)
            return null;

        var registrationNumber = await GenerateRegistrationNumberAsync(studentRegistrationId);
        var results = await SendStudentRegistrationNotificationsAsync(studentRegistration, registrationNumber);

        return results.Count > 0 ? string.Join(" ", results) : "Student has no email or phone number on file; nothing sent.";
    }

    private async Task<List<string>> SendStudentRegistrationNotificationsAsync(StudentRegistration studentRegistration, string? registrationNumber)
    {
        var results = new List<string>();
        var fullName = studentRegistration.FirstName.GetFullName(studentRegistration.LastName);
        var program = await context.Programs.Where(p => p.Id == studentRegistration.ProgramId).Select(p => p.ProgramName).FirstOrDefaultAsync();
        var college = await context.Colleges.Where(c => c.Id == studentRegistration.CollegeId).Select(c => c.Name).FirstOrDefaultAsync();
        var password = studentRegistration.DateOfBirthBS;

        if (!string.IsNullOrWhiteSpace(studentRegistration.Email))
        {
            try
            {
                var loginUrl = EmailTemplateHelper.SiteUrl;
                if (string.IsNullOrWhiteSpace(loginUrl))
                {
                    loginUrl = "/Identity/Account/Login";
                }
                else if (!loginUrl.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
                {
                    loginUrl = loginUrl.Trim().TrimEnd('/') + "/Identity/Account/Login";
                }

                var emailBody = EmailTemplateHelper.StudentRegistrationCredentials(fullName, registrationNumber ?? "", college ?? "", program ?? "", studentRegistration.Email, password, loginUrl);
                await gumpEmailService.SendHtmlEmailAsync(studentRegistration.Email, "Student Registration - Login Credentials", emailBody);
                results.Add($"Email sent to {studentRegistration.Email}.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send registration email to {Email}", studentRegistration.Email);
                results.Add($"Email to {studentRegistration.Email} failed.");
            }
        }

        var phone = studentRegistration.ContactNumber ?? studentRegistration.Phone;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                var smsLoginHint = !string.IsNullOrWhiteSpace(studentRegistration.Email)
                    ? $" or email {studentRegistration.Email}"
                    : "";
                var smsMessage = $"Dear {fullName}, your registration is complete. Reg No: {registrationNumber}, Password: {password}. Login with your registration number{smsLoginHint}. Please change password on first login. - FWU";
                await smsService.SendSmsAsync(phone, smsMessage);
                results.Add($"SMS sent to {phone}.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send registration SMS to {Phone}", phone);
                results.Add($"SMS to {phone} failed.");
            }
        }

        return results;
    }
}
