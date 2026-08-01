using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ConnectIPSService(AppDbContext context, IConfiguration configuration, HttpClient httpClient, ILogger<ConnectIPSService> logger) : IConnectIPSService
{
    private const string LoginPagePath = "/connectipswebgw/loginpage";
    private const string ValidateTxnPath = "/connectipswebws/api/creditor/validatetxn";

    private RSA? _rsa;

    public async Task<ConnectIpsFormData?> GeneratePaymentFormDataAsync(decimal amountNpr, string txnId, string referenceId, string remarks, string particulars)
    {
        var config = await GetConfigAsync();
        if (!IsConfigured(config))
            return null;

        var rsa = GetPrivateKey(config!);
        if (rsa == null)
            return null;

        var txnAmtPaisa = ConvertAmountToPaisa(amountNpr);
        var txnDate = DateTime.Now.ToString("dd-MM-yyyy");
        var txnCurrency = !string.IsNullOrWhiteSpace(config!.TransactionCurrency) ? config.TransactionCurrency.Trim() : "NPR";

        var message = string.Join(",",
            $"MERCHANTID={config.MerchantId}",
            $"APPID={config.AppId}",
            $"APPNAME={config.AppName}",
            $"TXNID={txnId}",
            $"TXNDATE={txnDate}",
            $"TXNCRNCY={txnCurrency}",
            $"TXNAMT={txnAmtPaisa}",
            $"REFERENCEID={referenceId}",
            $"REMARKS={remarks}",
            $"PARTICULARS={particulars}",
            "TOKEN=TOKEN");

        var token = Sign(message, rsa);

        return new ConnectIpsFormData
        {
            FormActionUrl = $"{config.GatewayUrl!.TrimEnd('/')}{LoginPagePath}",
            MerchantId = config.MerchantId!,
            AppId = config.AppId!,
            AppName = config.AppName!,
            TxnId = txnId,
            TxnDate = txnDate,
            TxnCurrency = txnCurrency,
            TxnAmt = txnAmtPaisa.ToString(),
            ReferenceId = referenceId,
            Remarks = remarks,
            Particulars = particulars,
            Token = token
        };
    }

    public async Task<ConnectIpsValidateResponse?> ValidateTransactionAsync(string txnId, decimal amountNpr)
    {
        var config = await GetConfigAsync();
        if (!IsConfigured(config))
            return null;

        var rsa = GetPrivateKey(config!);
        if (rsa == null)
            return null;

        var txnAmtPaisa = ConvertAmountToPaisa(amountNpr);

        var message = string.Join(",",
            $"MERCHANTID={config!.MerchantId}",
            $"APPID={config.AppId}",
            $"REFERENCEID={txnId}",
            $"TXNAMT={txnAmtPaisa}");

        var token = Sign(message, rsa);

        var payload = new
        {
            merchantId = config.MerchantId,
            appId = config.AppId,
            referenceId = txnId,
            txnAmt = txnAmtPaisa,
            token
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{config.ValidationApiUrl!.TrimEnd('/')}{ValidateTxnPath}")
        {
            Content = content
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UsernameForValidationApi}:{config.PasswordForValidationApi}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        logger.LogInformation("Sending ConnectIPS validate request for txnId={TxnId}", txnId);

        try
        {
            var response = await httpClient.SendAsync(requestMessage);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("ConnectIPS validate failed: status={Status}, body={Body}", (int)response.StatusCode, responseJson);
                return null;
            }

            logger.LogInformation("ConnectIPS validate response: {Response}", responseJson);
            return JsonSerializer.Deserialize<ConnectIpsValidateResponse>(responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ConnectIPS validate request failed for txnId={TxnId}", txnId);
            return null;
        }
    }

    private async Task<ConnectIpsPaymentConfiguration?> GetConfigAsync()
    {
        return await context.ConnectIpsPaymentConfigurations!.AsNoTracking().OrderBy(c => c.Id).FirstOrDefaultAsync();
    }

    private static bool IsConfigured(ConnectIpsPaymentConfiguration? config)
    {
        return config != null
            && !string.IsNullOrWhiteSpace(config.GatewayUrl)
            && !string.IsNullOrWhiteSpace(config.MerchantId)
            && !string.IsNullOrWhiteSpace(config.AppId)
            && !string.IsNullOrWhiteSpace(config.AppName)
            && !string.IsNullOrWhiteSpace(config.ValidationApiUrl)
            && !string.IsNullOrWhiteSpace(config.UsernameForValidationApi)
            && !string.IsNullOrWhiteSpace(config.PasswordForValidationApi)
            && !string.IsNullOrWhiteSpace(config.PasswordForCreditorPfx);
    }

    private RSA? GetPrivateKey(ConnectIpsPaymentConfiguration config)
    {
        if (_rsa != null)
            return _rsa;

        var certPath = configuration["ConnectIPS:CertPath"] ?? "Certs/CREDITOR.pfx";
        if (!File.Exists(certPath))
        {
            logger.LogWarning("ConnectIPS certificate not found at {CertPath}", certPath);
            return null;
        }

        try
        {
            using var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, config.PasswordForCreditorPfx, X509KeyStorageFlags.Exportable);
            _rsa = cert.GetRSAPrivateKey();
            return _rsa;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load ConnectIPS certificate from {CertPath}", certPath);
            return null;
        }
    }

    private static string Sign(string message, RSA rsa)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var signature = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    private static long ConvertAmountToPaisa(decimal amountNpr)
    {
        return (long)Math.Round(amountNpr * 100m, 0, MidpointRounding.AwayFromZero);
    }
}
