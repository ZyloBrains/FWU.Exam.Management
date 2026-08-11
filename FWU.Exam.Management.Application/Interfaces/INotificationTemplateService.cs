using FWU.Exam.Management.Domain.Entities.Notifications;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.Interfaces;

public sealed record RenderedEmailTemplate(string Subject, string BodyHtml);

public sealed record RenderedSmsTemplate(string Body);

/// <summary>
/// Resolves and renders notification templates by Code + Channel. Placeholders use
/// {{Token}} syntax. If no active template exists for a (Code, Channel), the built-in
/// fallback default is used so sending always works.
/// </summary>
public interface INotificationTemplateService
{
    Task<RenderedEmailTemplate> RenderEmailAsync(string code, IReadOnlyDictionary<string, string> context);

    Task<RenderedSmsTemplate> RenderSmsAsync(string code, IReadOnlyDictionary<string, string> context);

    Task<List<NotificationTemplate>> GetAllAsync();

    Task<NotificationTemplate?> GetByIdAsync(int id);

    Task CreateAsync(NotificationTemplate template);

    Task UpdateAsync(NotificationTemplate template);

    Task DeleteAsync(int id);

    Task<bool> ExistsAsync(string code, NotificationChannel channel, int? excludeId = null);
}
