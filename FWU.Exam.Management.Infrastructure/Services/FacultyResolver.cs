using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FWU.Exam.Management.Infrastructure.Services;

public class FacultyResolver(AppDbContext context, IMemoryCache cache) : IFacultyResolver
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private static readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        SlidingExpiration = _cacheDuration,
        Priority = CacheItemPriority.High
    };

    public async Task<CurrentFaculty?> ResolveFacultyAsync(string hostname)
    {
        var dotIndex = hostname.IndexOf('.');
        if (dotIndex < 0) return null;

        var subdomain = hostname[..dotIndex];
        var cacheKey = CacheKeys.FacultyBySubdomain(subdomain);

        if (cache.TryGetValue(cacheKey, out CurrentFaculty? cached) && cached != null)
            return cached;

        var faculty = await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name.Contains(subdomain) || f.OfficeCode.Contains(subdomain));

        if (faculty == null) return null;

        var result = new CurrentFaculty
        {
            Id = faculty.Id,
            Name = faculty.Name,
            OfficeCode = faculty.OfficeCode,
            LogoPath = faculty.LogoPath
        };

        cache.Set(cacheKey, result, _cacheOptions);
        return result;
    }

    public async Task<CurrentFaculty?> ResolveFacultyByCodeAsync(string officeCode)
    {
        var cacheKey = CacheKeys.FacultyByOfficeCode(officeCode);

        if (cache.TryGetValue(cacheKey, out CurrentFaculty? cached) && cached != null)
            return cached;

        var faculty = await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.OfficeCode == officeCode);

        if (faculty == null) return null;

        var result = new CurrentFaculty
        {
            Id = faculty.Id,
            Name = faculty.Name,
            OfficeCode = faculty.OfficeCode,
            LogoPath = faculty.LogoPath
        };

        cache.Set(cacheKey, result, _cacheOptions);
        return result;
    }
}