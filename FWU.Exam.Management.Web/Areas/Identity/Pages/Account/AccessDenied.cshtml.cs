using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FWU.Exam.Management.Web.Areas.Identity.Pages.Account;

public class AccessDeniedModel : PageModel
{
    public List<string> CurrentRoles { get; set; } = new();

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var claims = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            CurrentRoles = claims;
        }
    }
}
