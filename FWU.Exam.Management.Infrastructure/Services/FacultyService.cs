using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class FacultyService(
    AppDbContext context,
    UserManager<AppUser> userManager,
    ILogger<FacultyService> logger,
    IUserContext userContext) : IFacultyService
{
    private const string MustChangePasswordClaimType = "must_change_password";

    public async Task<List<Faculty>> GetAllFacultiesAsync()
    {
        return await context.Faculties
            .AsNoTracking()
            .ApplyScope(userContext)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<Faculty?> GetFacultyByIdAsync(int id)
    {
        return await context.Faculties.FindAsync(id);
    }

    public async Task<Faculty?> GetFacultyByOfficeCodeAsync(string officeCode)
    {
        return await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.OfficeCode == officeCode);
    }

    public async Task<string> CreateFacultyAsync(Faculty faculty, string adminPassword)
    {
        context.Faculties.Add(faculty);
        await context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(faculty.Email))
        {
            var password = string.IsNullOrWhiteSpace(adminPassword)
                ? GenerateRandomPassword()
                : adminPassword;

            var user = new AppUser
            {
                UserName = faculty.Email,
                Email = faculty.Email,
                FullName = faculty.Name,
                FacultyId = faculty.Id,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create AppUser for faculty admin {Email}: {Errors}", faculty.Email, errors);
                throw new InvalidOperationException($"Failed to create user account for {faculty.Email}: {errors}");
            }

            if (!await userManager.IsInRoleAsync(user, "FacultyAdmin"))
                await userManager.AddToRoleAsync(user, "FacultyAdmin");

            await userManager.AddClaimAsync(user, new Claim(MustChangePasswordClaimType, "true"));

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);

            return password;
        }

        return string.Empty;
    }

    public async Task UpdateFacultyAsync(Faculty faculty)
    {
        context.Faculties.Update(faculty);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await context.Faculties.FindAsync(id);
        if (faculty != null)
        {
            context.Faculties.Remove(faculty);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> FacultyExistsAsync(int id)
    {
        return await context.Faculties.AnyAsync(f => f.Id == id);
    }

    private static string GenerateRandomPassword()
    {
        var random = new Random();
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var prefix = new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"{prefix}@1";
    }
}
