using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace fwu_examination_management_system.Helpers
{
    public class AuditBaseHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditBaseHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // This gets the Username from the login cookie without hitting the DB
            //var userName = user?.Identity?.Name;

            // If you prefer to store the User ID (GUID) instead:
             var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId ?? "System";
        }
    }
}