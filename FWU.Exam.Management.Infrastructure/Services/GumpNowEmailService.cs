using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GumpNowEmailService(AppDbContext context, HttpClient httpClient) : IGumpNowEmailService
{
    public async Task SendEmailAsync(string toAddr, string subject, string templateId, Dictionary<string, string> contextData)
    {
        var config = await context.GumpNowEmailConfigurations.FirstOrDefaultAsync(c => c.IsActive);
        if (config == null)
            return;

        var payload = new GumpNowEmailRequest
        {
            FromAddr = config.FromAddr,
            ToAddr = [toAddr],
            Html = new HtmlContent
            {
                Context = contextData,
                Template = templateId
            },
            Subject = subject,
            Mode = config.Mode ?? "prod",
            OverrideUnsubscription = config.OverrideUnsubscription
        };

        await SendRequestAsync(config, payload);
    }

    public async Task SendHtmlEmailAsync(string toAddr, string subject, string htmlContent)
    {
        var config = await context.GumpNowEmailConfigurations.FirstOrDefaultAsync(c => c.IsActive);
        if (config == null)
            return;

        var payload = new GumpNowEmailRequest
        {
            FromAddr = config.FromAddr,
            ToAddr = [toAddr],
            Html = new HtmlContent
            {
                Content = htmlContent
            },
            Subject = subject,
            Mode = config.Mode ?? "prod",
            OverrideUnsubscription = config.OverrideUnsubscription
        };

        await SendRequestAsync(config, payload);
    }

    private async Task SendRequestAsync(Domain.Entities.GumpNowEmailConfiguration config, GumpNowEmailRequest payload)
    {
        var log = new GumpNowEmailLog
        {
            ToAddr = string.Join(",", payload.ToAddr ?? []),
            FromAddr = payload.FromAddr,
            Subject = payload.Subject,
            TemplateId = payload.Html?.Template,
            ContextJson = payload.Html?.Context is { Count: > 0 } contextData
                ? JsonSerializer.Serialize(contextData)
                : null,
            Mode = payload.Mode,
            Status = "Sending",
            SentAt = DateTime.UtcNow
        };

        Exception? failure = null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, config.ApiUrl)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("x-gumpnow-auth", config.ApiKey);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.Status = "Failed";
                log.ErrorMessage = $"{(int)response.StatusCode} ({response.StatusCode}): {responseBody}";
                failure = new InvalidOperationException(log.ErrorMessage);
            }
            else
            {
                log.Status = "Success";
            }
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            failure = ex;
        }
        finally
        {
            context.GumpNowEmailLogs.Add(log);
            await context.SaveChangesAsync();
        }

        if (failure != null)
            throw failure;
    }
}

internal class GumpNowEmailRequest
{
    [JsonPropertyName("from_addr")]
    public string? FromAddr { get; set; }

    [JsonPropertyName("to_addr")]
    public string[]? ToAddr { get; set; }

    [JsonPropertyName("html")]
    public HtmlContent? Html { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("override_unsubscription")]
    public bool OverrideUnsubscription { get; set; }
}

internal class HtmlContent
{
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Context { get; set; }

    [JsonPropertyName("template")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; set; }

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }
}
