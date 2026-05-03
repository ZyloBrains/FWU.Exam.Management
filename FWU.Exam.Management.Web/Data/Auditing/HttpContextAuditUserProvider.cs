using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Http;

public class HttpContextAuditUserProvider : IAuditUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpContextAuditUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}
