// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Data.Models;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

public class LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext context, ILogger<LoginModel> logger) : PageModel
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

    public SelectList FacultyOptions { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "Please select your organization")]
        [Display(Name = "Organization")]
        public int SelectedFacultyId { get; set; }
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

        FacultyOptions = new SelectList(
            await context.Faculties.OrderBy(f => f.Name).AsNoTracking().ToListAsync(),
            "Id", "Name");

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        FacultyOptions = new SelectList(
            await context.Faculties.OrderBy(f => f.Name).AsNoTracking().ToListAsync(),
            "Id", "Name");

        if (ModelState.IsValid)
        {
            AppUser? user;

            if (Input.SelectedFacultyId == -1)
            {
                user = await userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == Input.Email && u.FacultyId == null);
            }
            else
            {
                user = await userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == Input.Email && u.FacultyId == Input.SelectedFacultyId);
            }

            if (user != null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, Input.Password, false);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");

                    await signInManager.SignInAsync(user, Input.RememberMe);

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

                    if (await userManager.IsInRoleAsync(user, "Student"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    if (user.FacultyId != null)
                    {
                        var faculty = await context.Faculties.FindAsync(user.FacultyId.Value);
                        if (faculty != null && !string.IsNullOrWhiteSpace(faculty.OfficeCode))
                        {
                            return RedirectToAction("Index", "FacultyDashboard", new { officeCode = faculty.OfficeCode });
                        }
                    }

                    var roles = await userManager.GetRolesAsync(user);
                    if (roles.Contains("FacultyAdmin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "" });
                    }

                    return LocalRedirect(returnUrl);
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        return Page();
    }
}
