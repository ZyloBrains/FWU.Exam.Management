// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Data.Models;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext context, ILogger<LoginModel> logger, IAuditLogWriter auditLogWriter, ITenantContext tenantContext) : PageModel
{
    private const string MustChangePasswordClaimType = "must_change_password";

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Email or Registration Number")]
        public string EmailOrRegNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            logger.LogWarning("Login attempt for: {Input}", Input.EmailOrRegNumber);
            var user = await ResolveUserAsync(Input.EmailOrRegNumber);
            logger.LogWarning("Resolved user: {UserId}, UserName: {UserName}, IsActive: {IsActive}, PasswordHash present: {HasHash}",
                user?.Id, user?.UserName, user?.IsActive, user?.PasswordHash != null);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    logger.LogWarning("Login attempt by inactive user {Input}", Input.EmailOrRegNumber);
                    await auditLogWriter.LogAsync(ActivityTypes.UserLoginFailed,
                        $"Login blocked for inactive user {Input.EmailOrRegNumber}",
                        new { username = Input.EmailOrRegNumber, reason = "inactive" }, AuditSeverity.Warning);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, Input.Password, false);
                logger.LogWarning("CheckPasswordSignIn result: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}",
                    result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");

                    var tenant = await ResolveUserTenantAsync(user);
                    if (tenant != null)
                    {
                        tenantContext.SetTenant(tenant.Id, tenant.OfficeCode, tenant.TenantType);
                    }

                    await signInManager.SignInAsync(user, Input.RememberMe);
                    await auditLogWriter.LogAsync(ActivityTypes.UserLogin, "User signed in",
                        new { username = user.UserName, userId = user.Id }, entityName: "AppUser", entityId: user.Id);

                    var tenantCode = tenant?.OfficeCode;
                    if (tenantCode != null)
                    {
                        SetTenantCookie(tenantCode);
                    }

                    var claims = await userManager.GetClaimsAsync(user);
                    var mustChangePassword = claims.Any(c => c.Type == MustChangePasswordClaimType && c.Value == "true");
                    if (mustChangePassword)
                    {
                        var faculty = user.FacultyId != null ? await context.Faculties.FindAsync(user.FacultyId.Value) : null;
                        var postChangeReturnUrl = faculty != null && !string.IsNullOrWhiteSpace(faculty.OfficeCode)
                            ? $"/faculty/{faculty.OfficeCode}"
                            : returnUrl;

                        return RedirectToPage("/Account/Manage/ChangePassword", new { area = "Identity", returnUrl = postChangeReturnUrl });
                    }

                    var roles = await userManager.GetRolesAsync(user);
                    if (roles.Contains(Role.SuperAdmin))
                    {
                        return tenantCode != null
                            ? Redirect($"/tenant/{tenantCode}/Dashboard/Index")
                            : RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    if (roles.Contains(Role.FacultyAdmin))
                    {
                        return tenantCode != null
                            ? Redirect($"/tenant/{tenantCode}/Dashboard/Index")
                            : RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    if (roles.Contains(Role.CollegeAdmin))
                    {
                        return tenantCode != null
                            ? Redirect($"/tenant/{tenantCode}/Dashboard/Index")
                            : RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    if (roles.Contains(Role.Student))
                    {
                        return tenantCode != null
                            ? Redirect($"/tenant/{tenantCode}/Dashboard/Index")
                            : RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    await auditLogWriter.LogAsync(ActivityTypes.UserLoginLockedOut,
                        $"Login blocked for locked-out user {Input.EmailOrRegNumber}",
                        new { username = Input.EmailOrRegNumber, reason = "locked_out" }, AuditSeverity.Warning);
                    return RedirectToPage("./Lockout");
                }
            }

            logger.LogWarning("Login failed for {Input} - user not found or invalid", Input.EmailOrRegNumber);
            await auditLogWriter.LogAsync(ActivityTypes.UserLoginFailed,
                $"Login failed for {Input.EmailOrRegNumber} - user not found or invalid credentials",
                new { username = Input.EmailOrRegNumber, reason = "invalid_credentials" }, AuditSeverity.Warning);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        return Page();
    }

    private async Task<Tenant> ResolveUserTenantAsync(AppUser user)
    {
        if (user.FacultyId != null)
        {
            var faculty = await context.Faculties
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(f => f.Tenant)
                .FirstOrDefaultAsync(f => f.Id == user.FacultyId.Value);

            return faculty?.Tenant;
        }

        if (user.CollegeId != null)
        {
            var collegeTenant = await context.CollegeFaculties
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(cf => cf.CollegeId == user.CollegeId.Value
                    && cf.Faculty != null
                    && cf.Faculty.Tenant != null
                    && cf.Faculty.Tenant.IsActive)
                .Select(cf => cf.Faculty!.Tenant!)
                .FirstOrDefaultAsync();

            return collegeTenant;
        }

        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantType == TenantType.Central && t.IsActive);
    }

    private void SetTenantCookie(string tenantCode)
    {
        HttpContext.Response.Cookies.Append("tenant_code", tenantCode, new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            MaxAge = TimeSpan.FromHours(24)
        });
    }

    private async Task<AppUser> ResolveUserAsync(string emailOrRegNumber)
    {
        if (string.IsNullOrWhiteSpace(emailOrRegNumber))
            return null;

        var input = emailOrRegNumber.Trim();

        if (input.Contains('@'))
        {
            return await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == input);
        }

        var studentEmail = await context.StudentRegistrations
            .AsNoTracking()
            .Where(s => s.RegistrationNumber == input && s.Email != null)
            .Select(s => s.Email)
            .FirstOrDefaultAsync();

        if (studentEmail != null)
        {
            var userByEmail = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == studentEmail);
            if (userByEmail != null)
                return userByEmail;
        }

        return await userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == input);
    }
}
