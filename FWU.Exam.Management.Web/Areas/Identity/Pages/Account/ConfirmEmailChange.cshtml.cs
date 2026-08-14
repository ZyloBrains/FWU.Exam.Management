// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

public class ConfirmEmailChangeModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context) : PageModel
{

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
        {
            StatusMessage = "Error changing email.";
            return Page();
        }

        // Do NOT change the username column: students log in with their registration
        // number, so it must be preserved. Only the Email column is updated.

        // Keep the StudentRegistration email in sync with the new address.
        var registration = await context.StudentRegistrations
            .FirstOrDefaultAsync(sr => sr.StudentAdmissionId != null
                && context.StudentAdmissions.Any(sa => sa.Id == sr.StudentAdmissionId && sa.AppUserId == user.Id));
        registration ??= await context.StudentRegistrations
            .FirstOrDefaultAsync(sr => sr.RegistrationNumber == user.UserName);
        if (registration != null && registration.Email != email)
        {
            registration.Email = email;
            await context.SaveChangesAsync();
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Thank you for confirming your email change.";
        return Page();
    }
}
