using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Entities.Location;
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

    public async Task<IActionResult> Edit()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var primaryRole = roles.FirstOrDefault() ?? Role.Student;
        var isStudent = roles.Contains(Role.Student);

        var baseVm = await BuildBaseViewModelAsync(user, roles, primaryRole);

        var vm = new EditProfileViewModel
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

        vm.Provinces = await context.Provinces
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProvinceName)
            .Select(p => new DropdownItem { Id = p.Id, Name = p.ProvinceName })
            .ToListAsync();
        vm.Districts = await context.Districts
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DistrictName)
            .Select(d => new DropdownItem { Id = d.Id, Name = d.DistrictName, ParentId = d.ProvinceId })
            .ToListAsync();
        vm.LocalLevels = await context.LocalLevels
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.LocalLevelName)
            .Select(l => new DropdownItem { Id = l.Id, Name = l.LocalLevelName, ParentId = l.DistrictId })
            .ToListAsync();

        if (isStudent)
        {
            var registration = await studentDashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
            if (registration != null)
            {
                Address? permanentAddress = registration.PermanentAddress;
                if (registration.PermanentAddressId is int permanentAddressId && permanentAddress?.LocalLevel == null)
                {
                    permanentAddress = await context.Addresses
                        .AsNoTracking()
                        .Include(a => a.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
                        .FirstOrDefaultAsync(a => a.Id == permanentAddressId);
                }

                vm.PermanentLocalLevelId = permanentAddress?.LocalLevelId;
                vm.PermanentDistrictId = permanentAddress?.LocalLevel?.DistrictId;
                vm.PermanentProvinceId = permanentAddress?.LocalLevel?.District?.ProvinceId;
                vm.RegistrationGenderId = registration.GenderId;
                vm.RegistrationEthnicityId = registration.EthnicityId;
            }

            vm.Genders = await context.Genders
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.GenderName)
                .Select(g => new DropdownItem { Id = g.Id, Name = g.GenderName })
                .ToListAsync();
            vm.Ethnicities = await context.Ethnicities
                .AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.EthnicityName)
                .Select(e => new DropdownItem { Id = e.Id, Name = e.EthnicityName })
                .ToListAsync();
        }
        else if (user.CollegeId.HasValue)
        {
            var college = await context.Colleges
                .AsNoTracking()
                .Include(c => c.Address).ThenInclude(a => a!.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
                .FirstOrDefaultAsync(c => c.Id == user.CollegeId.Value);
            if (college?.Address != null)
            {
                vm.PermanentLocalLevelId = college.Address.LocalLevelId;
                vm.PermanentDistrictId = college.Address.LocalLevel?.DistrictId;
                vm.PermanentProvinceId = college.Address.LocalLevel?.District?.ProvinceId;
            }
        }

        return View("EditProfile", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        string? fullName,
        string? designation,
        string? email,
        string? phoneNumber,
        IFormFile? photo,
        IFormFile? signature,
        int? provinceId,
        int? districtId,
        int? localLevelId,
        int? genderId,
        int? ethnicityId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? Role.Student;
        var isStudent = roles.Contains(Role.Student);
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        var errors = new List<string>();

        if (!isStudent)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                errors.Add("Full name is required.");
            else if (fullName.Length > 50)
                errors.Add("Full name cannot exceed 50 characters.");

            if (designation != null && designation.Length > 100)
                errors.Add("Designation cannot exceed 100 characters.");

            if (errors.Count > 0)
                return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);

            user.FullName = fullName!.Trim();
            user.Designation = string.IsNullOrWhiteSpace(designation) ? null : designation.Trim();
        }

        var registration = isStudent
            ? await studentDashboardService.GetStudentRegistrationByUserIdAsync(user.Id)
            : null;

        if (isStudent && registration != null)
        {
            registration = await context.StudentRegistrations
                .FirstOrDefaultAsync(sr => sr.Id == registration.Id);
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var trimmedPhone = phoneNumber.Trim();
            if (trimmedPhone.Length is < 7 or > 15 || !System.Text.RegularExpressions.Regex.IsMatch(trimmedPhone, @"^\+?[0-9\s\-()]+$"))
                errors.Add("Phone number must be 7–15 digits and may include +, -, ( ) and spaces.");
            else
                user.PhoneNumber = trimmedPhone;
        }
        else
        {
            user.PhoneNumber = null;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else
        {
            var trimmedEmail = email.Trim();
            if (trimmedEmail.Length > 50)
                errors.Add("Email cannot exceed 50 characters.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Please enter a valid email address.");

            if (errors.Count == 0 && !string.Equals(trimmedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var normalized = userManager.NormalizeEmail(trimmedEmail);
                var emailTaken = await context.Users.AnyAsync(u => u.NormalizedEmail == normalized && u.Id != user.Id);
                if (emailTaken)
                    errors.Add("That email address is already in use by another account.");
                else
                {
                    user.Email = trimmedEmail;
                    user.EmailConfirmed = false;
                }
            }
        }

        if (isStudent)
        {
            if (provinceId is null or <= 0)
                errors.Add("Province is required.");
            if (districtId is null or <= 0)
                errors.Add("District is required.");
            if (localLevelId is null or <= 0)
                errors.Add("Local Level is required.");
            if (genderId is null or <= 0)
                errors.Add("Gender is required.");
            if (ethnicityId is null or <= 0)
                errors.Add("Ethnicity is required.");
        }

        if (errors.Count > 0)
            return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);

        if (photo != null && photo.Length > 0)
        {
            try
            {
                var path = await fileUploadHelper.UploadAsync(photo, "uploads/photos", Helpers.FileUploadHelper.MaxPhotoSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                if (path != null)
                    user.ProfilePath = path;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Profile photo upload rejected: {Message}", ex.Message);
                errors.Add(ex.Message);
                return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
            }
        }

        if (signature != null && signature.Length > 0
            && (primaryRole == Role.SuperAdmin || primaryRole == Role.FacultyAdmin || primaryRole == Role.Student))
        {
            try
            {
                var path = await fileUploadHelper.UploadAsync(signature, "uploads/signatures", Helpers.FileUploadHelper.MaxSignatureSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                if (path != null)
                    user.SignaturePath = path;
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Signature upload rejected: {Message}", ex.Message);
                errors.Add(ex.Message);
                return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            errors.AddRange(updateResult.Errors.Select(e => e.Description));
            return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
        }

        if (roles.Contains(Role.Student) && registration != null)
        {
            if (user.Email != null && user.Email != registration.Email)
            {
                var emailTaken = await context.StudentRegistrations
                    .IgnoreQueryFilters()
                    .AnyAsync(sr => sr.Id != registration.Id && sr.Email == user.Email);
                if (!emailTaken)
                {
                    registration.Email = user.Email;
                }
            }
            registration.ContactNumber = user.PhoneNumber;

            if (localLevelId is > 0)
            {
                var localLevel = await context.LocalLevels
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == localLevelId.Value && l.IsActive);
                if (localLevel == null)
                {
                    errors.Add("The selected local level is not valid.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }

                if (districtId is > 0 && localLevel.DistrictId != districtId.Value)
                {
                    errors.Add("The selected local level does not belong to the selected district.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }

                if (registration.PermanentAddressId.HasValue)
                {
                    var address = await context.Addresses.FindAsync(registration.PermanentAddressId.Value);
                    if (address != null)
                    {
                        address.LocalLevelId = localLevelId.Value;
                    }
                    else
                    {
                        var newAddress = new Address { LocalLevelId = localLevelId.Value, IsActive = true };
                        context.Addresses.Add(newAddress);
                        await context.SaveChangesAsync();
                        registration.PermanentAddressId = newAddress.Id;
                    }
                }
                else
                {
                    var newAddress = new Address { LocalLevelId = localLevelId.Value, IsActive = true };
                    context.Addresses.Add(newAddress);
                    await context.SaveChangesAsync();
                    registration.PermanentAddressId = newAddress.Id;
                }
            }

            if (districtId is > 0 && provinceId is > 0)
            {
                var district = await context.Districts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == districtId.Value && d.IsActive);
                if (district == null)
                {
                    errors.Add("The selected district is not valid.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }

                if (district.ProvinceId != provinceId.Value)
                {
                    errors.Add("The selected district does not belong to the selected province.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
            }

            if (genderId is > 0)
            {
                if (!await context.Genders.AnyAsync(g => g.Id == genderId.Value && g.IsActive))
                {
                    errors.Add("The selected gender is not valid.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
                registration.GenderId = genderId.Value;
            }

            if (ethnicityId is > 0)
            {
                if (!await context.Ethnicities.AnyAsync(e => e.Id == ethnicityId.Value && e.IsActive))
                {
                    errors.Add("The selected ethnicity is not valid.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
                registration.EthnicityId = ethnicityId.Value;
            }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    errors.Add("That email address is already in use by another student account.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
            }

        if (!isStudent && user.CollegeId.HasValue && localLevelId is > 0)
        {
            var localLevel = await context.LocalLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == localLevelId.Value && l.IsActive);
            if (localLevel == null)
            {
                errors.Add("The selected local level is not valid.");
                return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
            }

            if (districtId is > 0 && localLevel.DistrictId != districtId.Value)
            {
                errors.Add("The selected local level does not belong to the selected district.");
                return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
            }

            if (provinceId is > 0)
            {
                var district = await context.Districts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == districtId.Value && d.IsActive);
                if (district == null || district.ProvinceId != provinceId.Value)
                {
                    errors.Add("The selected district does not belong to the selected province.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
            }

            var college = await context.Colleges
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == user.CollegeId.Value);
            if (college != null)
            {
                if (college.AddressId.HasValue)
                {
                    var address = await context.Addresses.FindAsync(college.AddressId.Value);
                    if (address != null)
                    {
                        address.LocalLevelId = localLevelId.Value;
                    }
                    else
                    {
                        var newAddress = new Address { LocalLevelId = localLevelId.Value, IsActive = true };
                        context.Addresses.Add(newAddress);
                        await context.SaveChangesAsync();
                        college.AddressId = newAddress.Id;
                    }
                }
                else
                {
                    var newAddress = new Address { LocalLevelId = localLevelId.Value, IsActive = true };
                    context.Addresses.Add(newAddress);
                    await context.SaveChangesAsync();
                    college.AddressId = newAddress.Id;
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    errors.Add("Unable to save the college address. Please try again.");
                    return isAjax ? Json(new { success = false, errors }) : BadRequestResponse(errors);
                }
            }
        }

        if (isAjax)
        {
            var missingFields = isStudent
                ? await studentDashboardService.GetMissingMandatoryProfileFieldsAsync(user.Id, user.Email, user.PhoneNumber, user.ProfilePath, user.SignaturePath)
                : new List<string>();
            return Json(new { success = true, message = "Profile updated successfully.", missingFields });
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult BadRequestResponse(List<string> errors)
    {
        TempData["ErrorMessage"] = string.Join(" ", errors);
        return RedirectToAction(nameof(Edit));
    }

    private async Task<ProfileBaseViewModel> BuildBaseViewModelAsync(AppUser user, List<string> roles, string primaryRole)
    {
        var tenantCode = HttpContext.Items["TenantCode"] as string;
        string? tenantName = null;
        string? tenantLogo = null;
        string? orgName = null;
        string? orgLogo = null;
        string? bannerImagePath = null;

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
                bannerImagePath = tenant.BannerImagePath;
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
            CoverImagePath = string.IsNullOrEmpty(bannerImagePath) ? "/images/oce.png" : bannerImagePath,
            CanUploadSignature = primaryRole == Role.SuperAdmin || primaryRole == Role.FacultyAdmin || primaryRole == Role.Student,
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

        var registration = await studentDashboardService.GetStudentRegistrationByUserIdAsync(user.Id);

        Address? permanentAddress = registration?.PermanentAddress;
        if (registration?.PermanentAddressId is int permanentAddressId && permanentAddress?.LocalLevel == null)
        {
            permanentAddress = await context.Addresses
                .AsNoTracking()
                .Include(a => a.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
                .FirstOrDefaultAsync(a => a.Id == permanentAddressId);
        }

        if (registration != null)
        {
            vm.RegistrationId = registration.Id;
            vm.RegistrationNumber = registration.RegistrationNumber;
            vm.FirstName = registration.FirstName;
            vm.MiddleName = registration.MiddleName;
            vm.LastName = registration.LastName;
            vm.FullName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName);
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
            vm.Address = permanentAddress?.FullAddress
                ?? permanentAddress?.ToleStreet;
            vm.PermanentProvinceName = permanentAddress?.LocalLevel?.District?.Province?.ProvinceName;
            vm.PermanentDistrictName = permanentAddress?.LocalLevel?.District?.DistrictName;
            vm.PermanentLocalLevelName = permanentAddress?.LocalLevel?.LocalLevelName;

            vm.PermanentLocalLevelId = permanentAddress?.LocalLevelId;
            vm.PermanentDistrictId = permanentAddress?.LocalLevel?.DistrictId;
            vm.PermanentProvinceId = permanentAddress?.LocalLevel?.District?.ProvinceId;
            vm.RegistrationGenderId = registration.GenderId;
            vm.RegistrationEthnicityId = registration.EthnicityId;

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
                .Include(se => se.SemesterInstance).ThenInclude(si => si!.Semester)
                .OrderByDescending(se => se.SemesterInstance!.Semester!.Number)
                .FirstOrDefaultAsync();
            if (enrollment?.SemesterInstance?.Semester != null)
            {
                vm.CurrentSemester = SemesterDisplayHelper.Format(enrollment.SemesterInstance.Semester);
            }
        }
        else if (registration?.Program != null)
        {
            vm.Program = registration.Program.ProgramName;
            vm.ProgramCode = registration.Program.ProgramCode;
        }

        ViewData["Provinces"] = await context.Provinces
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProvinceName)
            .ToListAsync();
        ViewData["Districts"] = await context.Districts
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DistrictName)
            .ToListAsync();
        ViewData["LocalLevels"] = await context.LocalLevels
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.LocalLevelName)
            .ToListAsync();
        ViewData["Genders"] = await context.Genders
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.GenderName)
            .ToListAsync();
        ViewData["Ethnicities"] = await context.Ethnicities
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.EthnicityName)
            .ToListAsync();

        vm.MissingMandatoryFields = await studentDashboardService
            .GetMissingMandatoryProfileFieldsAsync(user.Id, user.Email, user.PhoneNumber, user.ProfilePath, user.SignaturePath);
        vm.ShowProfileCompletionPopup = vm.MissingMandatoryFields.Count > 0;

        var mandatoryFieldOrder = new[]
        {
            "Phone Number", "Profile Photo", "Student Signature",
            "Province", "District", "Local Level", "Gender", "Ethnicity"
        };
        vm.MandatoryFields = mandatoryFieldOrder
            .Select(name => new MandatoryProfileFieldStatusViewModel
            {
                Name = name,
                IsComplete = !vm.MissingMandatoryFields.Contains(name),
            })
            .ToList();

        return vm;
    }

    private static string FormatRoleLabel(string role) =>
        string.IsNullOrEmpty(role) ? "User" :
        System.Text.RegularExpressions.Regex.Replace(role, "([a-z])([A-Z])", "$1 $2");
}
