namespace FWU.Exam.Management.Domain.Constants;

public static class CacheKeys
{
    public const string UserContextPrefix = "user_context_";
    public const string FacultySubdomainPrefix = "faculty_subdomain_";
    public const string FacultyOfficeCodePrefix = "faculty_officecode_";

    public static string UserContext(string userId) => $"{UserContextPrefix}{userId}";
    public static string FacultyBySubdomain(string subdomain) => $"{FacultySubdomainPrefix}{subdomain}";
    public static string FacultyByOfficeCode(string officeCode) => $"{FacultyOfficeCodePrefix}{officeCode}";
}