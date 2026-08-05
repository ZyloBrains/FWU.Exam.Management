using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Helpers;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize]
public class ProfileController(
    UserManager<AppUser> userManager,
    AppDbContext context,
    IStudentDashboardService studentDashboardService,
    IFileUploadHelper fileUploadHelper,
    ILogger<ProfileController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var primaryRole = roles.FirstOrDefault() ?? Role.Student;

        var baseVm = await BuildBaseViewModelAsync(user, roles, primaryRole);

        return primaryRole switch
        {
            Role.SuperAdmin => View("SuperAdmin", await BuildSuperAdminViewModelAsync(baseVm)),
            Role.FacultyAdmin => View("FacultyAdmin", await BuildFacultyAdminViewModelAsync(baseVm, user)),
            Role.CollegeAdmin => View("CollegeAdmin", await BuildCollegeAdminViewModelAsync(baseVm, user)),
            _ => View("Student", await BuildStudentViewModelAsync(baseVm, user)),
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        string? fullName,
        string? designation,
        string? phoneNumber,
        IFormFile? photo,
        IFormFile? signature)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? Role.Student;

        if (!string.IsNullOrWhiteSpace(fullName))
            user.FullName = fullName.Trim();

        user.Designation = string.IsNullOrWhiteSpace(designation) ? null : designation.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

        if (photo != null && photo.Length > 0)
        {
            try
            {
                var path = await fileUploadHelper.UploadAsync(photo, "uploads/photos");
                if (path != null)
                    user.ProfilePath = path;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Profile photo upload rejected: {Message}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        if (signature != null && signature.Length > 0
            && (primaryRole == Role.SuperAdmin || primaryRole == Role.FacultyAdmin))
        {
            try
            {
                var path = await fileUploadHelper.UploadAsync(signature, "uploads/signatures");
                if (path != null)
                    user.SignaturePath = path;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Signature upload rejected: {Message}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            TempData["ErrorMessage"] = "Failed to save profile. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        if (roles.Contains(Role.Student))
        {
            var registration = await context.StudentRegistrations
                .FirstOrDefaultAsync(sr => sr.Email == user.Email || sr.RegistrationNumber == user.Email);
            if (registration != null)
            {
                registration.ContactNumber = user.PhoneNumber;
                await context.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProfileBaseViewModel> BuildBaseViewModelAsync(AppUser user, List<string> roles, string primaryRole)
    {
        var tenantCode = HttpContext.Items["TenantCode"] as string;
        string? tenantName = null;
        string? tenantLogo = null;
        string? orgName = null;
        string? orgLogo = null;

        if (!string.IsNullOrEmpty(tenantCode))
        {
            var tenant = await context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.OfficeCode == tenantCode);
            if (tenant != null)
            {
                tenantName = tenant.Name;
                tenantLogo = tenant.LogoPath;
                orgName = tenant.Name;
                orgLogo = tenant.LogoPath;
            }
        }

        if (user.FacultyId.HasValue)
        {
            var faculty = await context.Faculties.AsNoTracking().FirstOrDefaultAsync(f => f.Id == user.FacultyId.Value);
            if (faculty != null)
            {
                orgName = faculty.Name;
                orgLogo = faculty.LogoPath;
            }
        }
        else if (user.CollegeId.HasValue)
        {
            var college = await context.Colleges.AsNoTracking().FirstOrDefaultAsync(c => c.Id == user.CollegeId.Value);
            if (college != null)
            {
                orgName = college.Name;
            }
        }

        return new ProfileBaseViewModel
        {
            UserId = user.Id,
            FullName = user.FullName ?? user.UserName ?? user.Email ?? "User",
            Email = user.Email,
            UserName = user.UserName,
            RoleLabel = FormatRoleLabel(primaryRole),
            Designation = user.Designation,
            ProfilePath = user.ProfilePath,
            SignaturePath = user.SignaturePath,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            TwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
            ValidFrom = user.ValidFrom,
            ValidTo = user.ValidTo,
            Roles = roles.Select(FormatRoleLabel).ToList(),
            TenantCode = tenantCode,
            TenantName = tenantName,
            TenantLogo = tenantLogo,
            OrganizationName = orgName,
            OrganizationLogo = orgLogo,
            CoverImagePath = "/images/oce.png",
            CanUploadSignature = primaryRole == Role.SuperAdmin || primaryRole == Role.FacultyAdmin,
        };
    }

    private async Task<SuperAdminProfileViewModel> BuildSuperAdminViewModelAsync(ProfileBaseViewModel baseVm)
    {
        return new SuperAdminProfileViewModel
        {
            UserId = baseVm.UserId,
            FullName = baseVm.FullName,
            Email = baseVm.Email,
            UserName = baseVm.UserName,
            RoleLabel = baseVm.RoleLabel,
            Designation = baseVm.Designation,
            ProfilePath = baseVm.ProfilePath,
            SignaturePath = baseVm.SignaturePath,
            PhoneNumber = baseVm.PhoneNumber,
            IsActive = baseVm.IsActive,
            EmailConfirmed = baseVm.EmailConfirmed,
            TwoFactorEnabled = baseVm.TwoFactorEnabled,
            ValidFrom = baseVm.ValidFrom,
            ValidTo = baseVm.ValidTo,
            Roles = baseVm.Roles,
            TenantCode = baseVm.TenantCode,
            TenantName = baseVm.TenantName,
            TenantLogo = baseVm.TenantLogo,
            OrganizationName = baseVm.OrganizationName,
            OrganizationLogo = baseVm.OrganizationLogo,
            CoverImagePath = baseVm.CoverImagePath,
            CanUploadSignature = baseVm.CanUploadSignature,
            TotalTenants = await context.Tenants.CountAsync(),
            TotalUsers = await userManager.Users.CountAsync(),
            TotalColleges = await context.Colleges.CountAsync(),
            TotalStudents = await context.StudentRegistrations.CountAsync(),
            TotalPrograms = await context.Programs.CountAsync(),
            ActiveExamSchedules = await context.ExamSchedules.CountAsync(es => es.IsActive),
        };
    }

    private async Task<FacultyAdminProfileViewModel> BuildFacultyAdminViewModelAsync(ProfileBaseViewModel baseVm, AppUser user)
    {
        var vm = new FacultyAdminProfileViewModel
        {
            UserId = baseVm.UserId,
            FullName = baseVm.FullName,
            Email = baseVm.Email,
            UserName = baseVm.UserName,
            RoleLabel = baseVm.RoleLabel,
            Designation = baseVm.Designation,
            ProfilePath = baseVm.ProfilePath,
            SignaturePath = baseVm.SignaturePath,
            PhoneNumber = baseVm.PhoneNumber,
            IsActive = baseVm.IsActive,
            EmailConfirmed = baseVm.EmailConfirmed,
            TwoFactorEnabled = baseVm.TwoFactorEnabled,
            ValidFrom = baseVm.ValidFrom,
            ValidTo = baseVm.ValidTo,
            Roles = baseVm.Roles,
            TenantCode = baseVm.TenantCode,
            TenantName = baseVm.TenantName,
            TenantLogo = baseVm.TenantLogo,
            OrganizationName = baseVm.OrganizationName,
            OrganizationLogo = baseVm.OrganizationLogo,
            CoverImagePath = baseVm.CoverImagePath,
            CanUploadSignature = baseVm.CanUploadSignature,
        };

        if (user.FacultyId.HasValue)
        {
            var faculty = await context.Faculties.AsNoTracking().FirstOrDefaultAsync(f => f.Id == user.FacultyId.Value);
            if (faculty != null)
            {
                vm.FacultyName = faculty.Name;
                vm.FacultyShortName = faculty.ShortName;
                vm.OfficeCode = faculty.OfficeCode;
                vm.FacultyContactNumber = faculty.ContactNumber;
                vm.FacultyAddress = faculty.Address;
                vm.FacultyEmail = faculty.Email;

                vm.CollegeCount = await context.CollegePrograms
                    .AsNoTracking()
                    .Where(cp => cp.Program != null && cp.Program.FacultyId == faculty.Id)
                    .Select(cp => cp.CollegeId)
                    .Distinct()
                    .CountAsync();

                vm.ProgramCount = await context.Programs
                    .AsNoTracking()
                    .CountAsync(p => p.FacultyId == faculty.Id);

                vm.StaffCount = await userManager.Users.CountAsync(u => u.FacultyId == faculty.Id);

                vm.ActiveExamScheduleCount = await context.ExamSchedules
                    .AsNoTracking()
                    .CountAsync(es => es.IsActive && es.Program != null && es.Program.FacultyId == faculty.Id);
            }
        }

        return vm;
    }

    private async Task<CollegeAdminProfileViewModel> BuildCollegeAdminViewModelAsync(ProfileBaseViewModel baseVm, AppUser user)
    {
        var vm = new CollegeAdminProfileViewModel
        {
            UserId = baseVm.UserId,
            FullName = baseVm.FullName,
            Email = baseVm.Email,
            UserName = baseVm.UserName,
            RoleLabel = baseVm.RoleLabel,
            Designation = baseVm.Designation,
            ProfilePath = baseVm.ProfilePath,
            SignaturePath = baseVm.SignaturePath,
            PhoneNumber = baseVm.PhoneNumber,
            IsActive = baseVm.IsActive,
            EmailConfirmed = baseVm.EmailConfirmed,
            TwoFactorEnabled = baseVm.TwoFactorEnabled,
            ValidFrom = baseVm.ValidFrom,
            ValidTo = baseVm.ValidTo,
            Roles = baseVm.Roles,
            TenantCode = baseVm.TenantCode,
            TenantName = baseVm.TenantName,
            TenantLogo = baseVm.TenantLogo,
            OrganizationName = baseVm.OrganizationName,
            OrganizationLogo = baseVm.OrganizationLogo,
            CoverImagePath = baseVm.CoverImagePath,
            CanUploadSignature = baseVm.CanUploadSignature,
        };

        if (user.CollegeId.HasValue)
        {
            var college = await context.Colleges
                .AsNoTracking()
                .Include(c => c.Address)
                .Include(c => c.CollegeType)
                .FirstOrDefaultAsync(c => c.Id == user.CollegeId.Value);
            if (college != null)
            {
                vm.CollegeName = college.Name;
                vm.CollegeCode = college.Code;
                vm.CollegeShortName = college.ShortName;
                vm.CollegeEmail = college.Email;
                vm.CollegePhone = college.Phone1;
                vm.CollegeWebsite = college.Website;
                vm.PrincipalName = college.PrincipalName;
                vm.CollegeType = college.CollegeType?.Name;
                vm.CollegeAddress = college.Address?.FullAddress ?? college.Address?.ToleStreet;

                vm.ProgramCount = await context.CollegePrograms
                    .AsNoTracking()
                    .CountAsync(cp => cp.CollegeId == college.Id);

                vm.StudentCount = await context.StudentRegistrations
                    .AsNoTracking()
                    .CountAsync(sr => sr.CollegeId == college.Id);

                vm.StaffCount = await userManager.Users.CountAsync(u => u.CollegeId == college.Id);
            }
        }

        return vm;
    }

    private async Task<StudentProfileDetailViewModel> BuildStudentViewModelAsync(ProfileBaseViewModel baseVm, AppUser user)
    {
        var vm = new StudentProfileDetailViewModel
        {
            UserId = baseVm.UserId,
            FullName = baseVm.FullName,
            Email = baseVm.Email,
            UserName = baseVm.UserName,
            RoleLabel = baseVm.RoleLabel,
            Designation = baseVm.Designation,
            ProfilePath = baseVm.ProfilePath,
            SignaturePath = baseVm.SignaturePath,
            PhoneNumber = baseVm.PhoneNumber,
            IsActive = baseVm.IsActive,
            EmailConfirmed = baseVm.EmailConfirmed,
            TwoFactorEnabled = baseVm.TwoFactorEnabled,
            ValidFrom = baseVm.ValidFrom,
            ValidTo = baseVm.ValidTo,
            Roles = baseVm.Roles,
            TenantCode = baseVm.TenantCode,
            TenantName = baseVm.TenantName,
            TenantLogo = baseVm.TenantLogo,
            OrganizationName = baseVm.OrganizationName,
            OrganizationLogo = baseVm.OrganizationLogo,
            CoverImagePath = baseVm.CoverImagePath,
            CanUploadSignature = baseVm.CanUploadSignature,
        };

        var registration = await studentDashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration != null)
        {
            vm.RegistrationId = registration.Id;
            vm.RegistrationNumber = registration.RegistrationNumber;
            vm.NepaliName = registration.NepaliName;
            vm.Gender = registration.Gender?.GenderName;
            vm.DateOfBirthBS = registration.DateOfBirthBS;
            vm.DateOfBirthAD = registration.DateOfBirthAD;
            vm.Ethnicity = registration.Ethnicity?.EthnicityName;
            vm.Category = registration.StudentCategory?.StudentCategoryName;
            vm.BloodGroup = registration.BloodGroup;
            vm.Nationality = registration.Nationality;
            vm.Religion = registration.Religion;
            vm.AcademicYear = registration.AcademicYear?.AcademicYearName;
            vm.College = registration.College?.Name;
            vm.Level = registration.Level?.LevelName;
            vm.Address = registration.PermanentAddress?.FullAddress
                ?? registration.PermanentAddress?.ToleStreet;

            var guardians = await context.StudentGuardians
                .AsNoTracking()
                .Where(g => g.StudentRegistrationId == registration.Id)
                .ToListAsync();
            vm.Guardians = guardians
                .SelectMany(g => new[]
                {
                    new StudentGuardianProfileViewModel
                    {
                        Relation = "Father",
                        Name = g.FatherName,
                        ContactNumber = g.FatherContactNumber,
                        Occupation = g.FatherProfession,
                    },
                    new StudentGuardianProfileViewModel
                    {
                        Relation = "Mother",
                        Name = g.MotherName,
                        ContactNumber = g.MotherContactNumber,
                        Occupation = g.MotherProfession,
                    },
                    new StudentGuardianProfileViewModel
                    {
                        Relation = g.RelationWithStudent ?? "Guardian",
                        Name = g.GuardianName,
                        ContactNumber = g.GuardianContactNumber,
                        Occupation = g.GuardianProfession,
                    },
                })
                .ToList();

            var qualifications = await context.StudentQualifications
                .AsNoTracking()
                .Where(q => q.StudentRegistrationId == registration.Id && q.IsActive)
                .ToListAsync();
            vm.Qualifications = qualifications
                .Select(q => new StudentQualificationProfileViewModel
                {
                    InstituteName = q.InstituteName,
                    ProgramName = q.ProgramName,
                    PassedYear = q.PassedYear,
                    Percentage = q.Percentage?.ToString("0.##"),
                })
                .ToList();

            var voucherIds = await context.ApplicationVouchers
                .AsNoTracking()
                .Where(av => av.StudentRegistrationId == registration.Id)
                .Select(av => av.Id)
                .ToListAsync();
            if (voucherIds.Count > 0)
            {
                vm.ExamRegistrationCount = await context.ExamRegistrations
                    .AsNoTracking()
                    .CountAsync(er => er.ApplicationVoucherId != null && voucherIds.Contains(er.ApplicationVoucherId!.Value) && er.IsActive);
            }

            vm.AdmitCardCount = await context.AdmitCards
                .AsNoTracking()
                .CountAsync(ac => ac.StudentRegistrationId == registration.Id && ac.IsActive);

            vm.PaymentCount = await context.PaymentRequestLogs
                .AsNoTracking()
                .CountAsync(prl => prl.StudentRegistrationId == registration.Id);
        }

        var admission = await studentDashboardService.GetStudentAdmissionByUserIdAsync(user.Id);
        if (admission != null)
        {
            vm.AdmissionDate = admission.AdmissionDate.ToString("yyyy-MM-dd");
            vm.CollegeRollNumber = admission.CollegeRollNumber;

            var program = await context.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == admission.ProgramsId);
            if (program != null)
            {
                vm.Program = program.ProgramName;
                vm.ProgramCode = program.ProgramCode;
            }

            var enrollment = await context.SemesterEnrollments
                .AsNoTracking()
                .Where(se => se.StudentAdmissionId == admission.Id && se.EnrollmentStatus == StudentEnrollmentStatus.Active)
                .Include(se => se.Semester)
                .OrderByDescending(se => se.Semester!.Year)
                .ThenByDescending(se => se.Semester!.Number)
                .FirstOrDefaultAsync();
            if (enrollment?.Semester != null)
            {
                vm.CurrentSemester = $"{enrollment.Semester.Name} ({enrollment.Semester.Year})";
            }
        }
        else if (registration?.Program != null)
        {
            vm.Program = registration.Program.ProgramName;
            vm.ProgramCode = registration.Program.ProgramCode;
        }

        return vm;
    }

    private static string FormatRoleLabel(string role) =>
        string.IsNullOrEmpty(role) ? "User" :
        System.Text.RegularExpressions.Regex.Replace(role, "([a-z])([A-Z])", "$1 $2");
}
