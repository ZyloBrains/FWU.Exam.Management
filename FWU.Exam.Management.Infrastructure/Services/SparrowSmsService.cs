using System.Web;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SparrowSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly SparrowSmsOptions _options;
    private readonly ILogger<SparrowSmsService> _logger;

    public SparrowSmsService(HttpClient httpClient, IOptions<SparrowSmsOptions> options, ILogger<SparrowSmsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            _logger.LogWarning("SMS not sent: phone number is empty.");
            return;
        }

        try
        {
            var url = $"http://api.sparrowsms.com/v2/sms/?token={HttpUtility.UrlEncode(_options.Token)}&from={HttpUtility.UrlEncode(_options.Sender)}&to={HttpUtility.UrlEncode(toPhoneNumber)}&text={HttpUtility.UrlEncode(message)}";

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SMS sending failed to {PhoneNumber}. Status: {Status}, Response: {Response}", toPhoneNumber, response.StatusCode, content);
            }
            else
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. Response: {Response}", toPhoneNumber, content);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toPhoneNumber);
        }
    }
}
