// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

public class LogoutModel(SignInManager<AppUser> signInManager, ILogger<LogoutModel> logger) : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        HttpContext.Response.Cookies.Delete("tenant_code");
        logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }
}
