#nullable disable

using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account.Manage;

public class IndexModel(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppDbContext context) : PageModel
{
    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public bool IsStudent { get; set; }

    public StudentRegistration StudentProfile { get; set; }

    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }

    private async Task LoadAsync(AppUser user)
    {
        var userName = await userManager.GetUserNameAsync(user);
        var phoneNumber = await userManager.GetPhoneNumberAsync(user);

        Username = userName;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber
        };

        IsStudent = await userManager.IsInRoleAsync(user, "Student");

        if (IsStudent && !string.IsNullOrWhiteSpace(user.Email))
        {
            StudentProfile = await context.StudentRegistrations
                .Include(s => s.AcademicYear)
                .Include(s => s.Level)
                .Include(s => s.College)
                .Include(s => s.Gender)
                .Include(s => s.StudentCategory)
                .Include(s => s.Ethnicity)
                .Include(s => s.PermanentAddress).ThenInclude(a => a.LocalLevel).ThenInclude(ll => ll.District).ThenInclude(d => d.Province)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email == user.Email);
        }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return RedirectToPage("./ChangePassword");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        if (await userManager.IsInRoleAsync(user, "Student"))
        {
            await LoadAsync(user);
            StatusMessage = "Students cannot update profile here.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var phoneNumber = await userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}
