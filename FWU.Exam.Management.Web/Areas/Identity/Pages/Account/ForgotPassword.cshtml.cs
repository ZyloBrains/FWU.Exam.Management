// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

public class ForgotPasswordModel(UserManager<AppUser> userManager, IEmailSender emailSender, IAuditLogWriter auditLogWriter) : PageModel
{

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
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var user = await userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No account found with this email address.");
                return Page();
            }

            if (!(await userManager.IsEmailConfirmedAsync(user)))
            {
                ModelState.AddModelError(string.Empty, "Email is not confirmed. Please confirm your email before resetting your password.");
                return Page();
            }

            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
                Input.Email,
                "Reset Password",
                EmailTemplateHelper.ResetPassword(Input.Email, callbackUrl));

            await auditLogWriter.LogAsync(ActivityTypes.UserPasswordResetRequested,
                $"Password reset link requested for {Input.Email}",
                new { email = Input.Email });

            TempData["StatusMessage"] = "Password reset link has been sent to your email.";
            return RedirectToPage("./ForgotPasswordConfirmation");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred while sending the reset email. Please try again. ({ex.Message})");
            return Page();
        }
    }
}
