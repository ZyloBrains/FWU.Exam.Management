using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SmsService(AppDbContext context, HttpClient httpClient) : ISmsService
{
    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        var config = await context.SmsConfigurations.FirstOrDefaultAsync(c => c.IsActive);
        if (config == null)
            return;

        var payload = new GumpNowSmsRequest
        {
            ToAddr = toPhoneNumber,
            Plain = new PlainContent { Content = message },
            Preserve = false,
            Mode = config.Mode ?? "prod",
            Tags = (config.Tags ?? "entrance").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        };

        var log = new SmsLog
        {
            ToAddr = toPhoneNumber,
            Message = message,
            Mode = payload.Mode,
            TagsJson = payload.Tags is { Length: > 0 } tags ? JsonSerializer.Serialize(tags) : null,
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
            context.SmsLogs.Add(log);
            await context.SaveChangesAsync();
        }

        if (failure != null)
            throw failure;
    }
}

internal class GumpNowSmsRequest
{
    [JsonPropertyName("to_addr")]
    public string? ToAddr { get; set; }

    [JsonPropertyName("plain")]
    public PlainContent? Plain { get; set; }

    [JsonPropertyName("preserve")]
    public bool Preserve { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }
}

internal class PlainContent
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
