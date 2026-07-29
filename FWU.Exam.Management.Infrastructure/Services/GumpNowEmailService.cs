using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GumpNowEmailService(AppDbContext context, HttpClient httpClient) : IGumpNowEmailService
{
    public async Task SendEmailAsync(string toAddr, string subject, int templateId, Dictionary<string, string> contextData)
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
        var request = new HttpRequestMessage(HttpMethod.Post, config.ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("x-gumpnow-auth", config.ApiKey);

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GumpNow email API returned {(int)response.StatusCode} ({response.StatusCode}): {responseBody}");
        }
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Template { get; set; }

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }
}
