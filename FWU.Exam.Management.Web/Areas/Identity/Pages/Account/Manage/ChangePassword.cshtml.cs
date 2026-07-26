// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ILogger<ChangePasswordModel> logger) : PageModel
{
    private const string MustChangePasswordClaimType = "must_change_password";

    [BindProperty]
    public InputModel Input { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var hasPassword = await userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToPage("./SetPassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        var claims = await userManager.GetClaimsAsync(user);
        var mustChangeClaims = claims.Where(c => c.Type == MustChangePasswordClaimType).ToList();
        foreach (var claim in mustChangeClaims)
        {
            await userManager.RemoveClaimAsync(user, claim);
        }

        await signInManager.SignOutAsync();
        logger.LogInformation("User changed password and was signed out.");

        var tenantCode = HttpContext.Items["TenantCode"] as string
            ?? HttpContext.Request.Cookies["tenant_code"];
        var loginUrl = string.IsNullOrEmpty(tenantCode)
            ? Url.Page("/Account/Login", new { area = "Identity" })
            : $"/tenant/{tenantCode}/Identity/Account/Login";

        return Redirect(loginUrl);
    }
}
