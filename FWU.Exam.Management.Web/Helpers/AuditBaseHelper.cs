//using fwu_examination_management_system.Models;
//using Microsoft.AspNetCore.Http; // add this for IHttpContextAccessor
//using Microsoft.AspNetCore.Identity;
//using System.Text.Json;

//namespace fwu_examination_management_system.Helpers
//{
//    public class AuditBaseHelper
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly UserManager<AppUser> _userManager;
//        private readonly ILogger<AuditBaseHelper> _logger;

//        public AuditBaseHelper(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, ILogger<AuditBaseHelper> logger  )
//        {
//            _httpContextAccessor = httpContextAccessor;
//            _userManager = userManager;
//            _logger = logger;
//        }

//        public async Task<string?> GetCurrentUserNameAsync()
//        {


//            var httpContext = _httpContextAccessor.HttpContext;

//            if (httpContext == null)
//            {
//                _logger.LogWarning("HttpContext is null");
//                return null;
//            }

//            var user = await _userManager.GetUserAsync(httpContext.User);

//            if (user == null)
//            {
//                _logger.LogWarning("User not found");
//                return null;
//            }

//            var userJson = JsonSerializer.Serialize(user, new JsonSerializerOptions
//            {
//                WriteIndented = true // pretty print
//            });

//            _logger.LogInformation("Current User Details: {UserJson}", userJson);

//            return userJson;
//            //var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
//            //_logger.LogInformation("Current user retrieved");
//            //_logger.LogInformation(_userManager);
//            //_logger.LogInformation(user);
//            //return null;
//            //return user?.UserName ?? "System"; // fallback if no user
//        }

//    }
//}

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
            var userName = user?.Identity?.Name;

            // If you prefer to store the User ID (GUID) instead:
            // var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userName ?? "System";
        }
    }
}