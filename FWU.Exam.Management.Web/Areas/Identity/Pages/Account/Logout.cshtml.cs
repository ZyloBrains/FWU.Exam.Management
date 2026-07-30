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
    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        var userId = signInManager.UserManager.GetUserId(User);
        if (userId != null)
        {
            // Invalidate permissions cache for the logging-out user
            var permissionService = HttpContext.RequestServices.GetRequiredService<Application.Interfaces.IPermissionService>();
            await permissionService.InvalidateCacheAsync(userId);
        }

        HttpContext.Session.Clear();
        HttpContext.Response.Cookies.Delete("tenant_code");
        HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new
            // request and the identity for the user gets updated.
            return RedirectToPage();
        }
    }
}
