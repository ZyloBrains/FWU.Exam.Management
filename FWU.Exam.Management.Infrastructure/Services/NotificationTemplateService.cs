using System.Text.RegularExpressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Notifications;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class NotificationTemplateService(AppDbContext context) : INotificationTemplateService
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{\s*(?<key>[A-Za-z0-9_.-]+)\s*\}\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<RenderedEmailTemplate> RenderEmailAsync(string code, IReadOnlyDictionary<string, string> context)
    {
        var template = await GetActiveAsync(code, NotificationChannel.Email);
        var definition = template is null ? NotificationTemplateDefaults.Get(code, NotificationChannel.Email) : null;

        var subject = template?.Subject ?? definition?.Subject ?? code;
        var body = template?.Body ?? definition?.Body ?? string.Empty;

        var renderedSubject = ReplacePlaceholders(subject, context);
        var renderedBody = ReplacePlaceholders(body, context);

        if (!renderedBody.Contains("<html", StringComparison.OrdinalIgnoreCase))
            renderedBody = EmailTemplateHelper.Layout(renderedSubject, renderedBody);

        return new RenderedEmailTemplate(renderedSubject, renderedBody);
    }

    public async Task<RenderedSmsTemplate> RenderSmsAsync(string code, IReadOnlyDictionary<string, string> context)
    {
        var template = await GetActiveAsync(code, NotificationChannel.Sms);
        var definition = template is null ? NotificationTemplateDefaults.Get(code, NotificationChannel.Sms) : null;

        var body = template?.Body ?? definition?.Body ?? string.Empty;
        return new RenderedSmsTemplate(ReplacePlaceholders(body, context));
    }

    private async Task<NotificationTemplate?> GetActiveAsync(string code, NotificationChannel channel)
    {
        return await context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == code && t.Channel == channel && t.IsActive);
    }

    private static string ReplacePlaceholders(string template, IReadOnlyDictionary<string, string> context)
    {
        if (string.IsNullOrEmpty(template) || context is null || context.Count == 0)
            return template;

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            var value = context
                .FirstOrDefault(kvp => string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                .Value;
            return value ?? string.Empty;
        });
    }

    public async Task<List<NotificationTemplate>> GetAllAsync()
    {
        return await context.NotificationTemplates
            .AsNoTracking()
            .OrderBy(t => t.Channel)
            .ThenBy(t => t.Code)
            .ToListAsync();
    }

    public async Task<NotificationTemplate?> GetByIdAsync(int id)
    {
        return await context.NotificationTemplates.FindAsync(id);
    }

    public async Task CreateAsync(NotificationTemplate template)
    {
        context.NotificationTemplates.Add(template);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotificationTemplate template)
    {
        context.NotificationTemplates.Update(template);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var template = await context.NotificationTemplates.FindAsync(id);
        if (template is null)
            return;

        context.NotificationTemplates.Remove(template);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string code, NotificationChannel channel, int? excludeId = null)
    {
        var query = context.NotificationTemplates
            .Where(t => t.Code == code && t.Channel == channel);

        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
