namespace FWU.Exam.Management.Web.Routing;

public class FacultyHostRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        var hostname = httpContext?.Request.Host.Host;
        if (string.IsNullOrEmpty(hostname)) return false;

        var dotIndex = hostname.IndexOf('.');
        if (dotIndex < 0) return false;

        var subdomain = hostname[..dotIndex];
        return !string.IsNullOrEmpty(subdomain) && !string.Equals(subdomain, "www", StringComparison.OrdinalIgnoreCase);
    }
}
