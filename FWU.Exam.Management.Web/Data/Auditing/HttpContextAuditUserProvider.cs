using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.AspNetCore.Http;

public class HttpContextAuditUserProvider(IHttpContextAccessor httpContextAccessor) : IAuditUserProvider
{
    public string? GetCurrentUserName()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}
