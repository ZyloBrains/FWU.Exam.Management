using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Verifies eSewa transactions against the live eSewa transaction status endpoint and
/// cross-references the result with the local <see cref="PaymentRequestLog"/>. This is the
/// "verify a completed transaction" tool: it trusts eSewa's response, not the local DB or a
/// browser callback. Note that eSewa requires the exact <c>total_amount</c> to answer a status
/// query, so the amount is taken from the local record when one exists.
/// </summary>
public class ESewaVerificationService(
    AppDbContext context,
    IESewaService eSewaService,
    ILogger<ESewaVerificationService> logger) : IESewaVerificationService
{
    public async Task<ESewaVerificationResultDto> VerifyByUuidAsync(string transactionUuid, decimal? amountOverride = null)
    {
        if (string.IsNullOrWhiteSpace(transactionUuid))
            return new ESewaVerificationResultDto { LookupSucceeded = false, Verdict = "NOREQ", Message = "No eSewa transaction UUID was provided." };

        var trimmed = transactionUuid.Trim();
        var local = await FindLocalByUuidAsync(trimmed);

        decimal amount;
        if (local != null)
        {
            amount = local.Amount;
        }
        else if (amountOverride.HasValue && amountOverride.Value > 0)
        {
            amount = amountOverride.Value;
        }
        else
        {
            return new ESewaVerificationResultDto
            {
                LookupSucceeded = false,
                Verdict = "NOTFOUND",
                Message = $"No local payment record was found for UUID '{trimmed}', and no amount was provided. eSewa requires the exact total_amount to check a status."
            };
        }

        return await LookupAndBuildAsync(trimmed, amount, local);
    }

    public async Task<ESewaVerificationResultDto> VerifyByInvoiceAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return new ESewaVerificationResultDto { LookupSucceeded = false, Verdict = "NOREQ", Message = "No invoice number was provided." };

        var local = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.InvoiceNumber == invoiceNumber.Trim());

        if (local == null)
            return new ESewaVerificationResultDto
            {
                LookupSucceeded = false,
                Verdict = "NOTFOUND",
                Message = $"No local payment record found for invoice '{invoiceNumber.Trim()}'."
            };

        var uuid = local.ProviderReferenceId ?? local.TransactionId;
        if (string.IsNullOrWhiteSpace(uuid))
            return new ESewaVerificationResultDto
            {
                LookupSucceeded = false,
                Verdict = "NOREF",
                Message = $"Invoice '{invoiceNumber.Trim()}' matched local log #{local.Id} but it has no eSewa transaction UUID stored, so the gateway cannot be queried.",
                LocalRecord = MapLocal(local)
            };

        return await LookupAndBuildAsync(uuid, local.Amount, local, invoiceNumber.Trim());
    }

    private async Task<PaymentRequestLog?> FindLocalByUuidAsync(string uuid)
    {
        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Where(l => l.ProviderReferenceId == uuid || l.TransactionId == uuid || l.ProviderTransactionId == uuid)
            .OrderByDescending(l => l.ForwardedTimestamp)
            .FirstOrDefaultAsync();
    }

    private async Task<ESewaVerificationResultDto> LookupAndBuildAsync(
        string uuid, decimal amount, PaymentRequestLog? local, string? invoiceHint = null)
    {
        var result = new ESewaVerificationResultDto
        {
            LocalRecord = local == null ? null : MapLocal(local),
            TransactionUuid = uuid
        };

        ESewaVerifyResponse? response;
        try
        {
            response = await eSewaService.QueryTransactionStatusAsync(uuid, amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ESewaVerification: status lookup failed for uuid={Uuid}", uuid);
            result.LookupSucceeded = false;
            result.Verdict = "ERROR";
            result.Message = $"eSewa status endpoint returned an error: {ex.Message}";
            return result;
        }

        if (response == null)
        {
            result.LookupSucceeded = false;
            result.Verdict = "ERROR";
            result.Message = "eSewa status endpoint returned no response, or rejected the lookup. Check the transaction UUID and that the product amount matches what was sent at checkout.";
            return result;
        }

        if (string.IsNullOrWhiteSpace(response.Status))
        {
            result.LookupSucceeded = false;
            result.Verdict = "ERROR";
            result.Message = "eSewa status endpoint returned a response without a status field. This usually means the lookup was rejected (wrong amount or unknown UUID).";
            return result;
        }

        result.LookupSucceeded = true;
        result.TransactionUuid = response.TransactionUuid ?? uuid;
        result.TotalAmount = response.TotalAmount;
        result.Status = response.Status;
        result.TransactionCode = response.TransactionCode ?? response.RefId;
        result.RefId = response.RefId;
        result.ProductCode = response.ProductCode;

        // Cross-reference with the local record.
        if (local == null)
        {
            result.Verdict = "NOTMATCHED";
            result.Message = "The eSewa transaction was found, but no matching local payment record exists in the database.";
            return result;
        }

        var expected = decimal.Round(local.Amount);
        if (response.TotalAmount != expected)
        {
            result.Verdict = "AMOUNTMISMATCH";
            result.Message = $"eSewa reports Rs. {response.TotalAmount:N0} but the local record expects Rs. {expected:N0}.";
            return result;
        }

        if (!string.Equals(response.Status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
        {
            result.Verdict = "NOTCOMPLETED";
            result.Message = $"eSewa reports status '{response.Status}'. The payment is not completed.";
            return result;
        }

        result.Verdict = "COMPLETED";
        result.Message = "Verified — eSewa reports COMPLETE and the amount matches the local record.";
        return result;
    }

    private static LocalPaymentRecordDto MapLocal(PaymentRequestLog log) => new()
    {
        Id = log.Id,
        InvoiceNumber = log.InvoiceNumber,
        AmountRupees = log.Amount,
        PaymentStatus = log.PaymentStatus,
        PaymentProvider = log.PaymentProvider,
        ProviderReferenceId = log.ProviderReferenceId,
        ProviderTransactionId = log.ProviderTransactionId,
        StudentName = string.IsNullOrWhiteSpace(log.FullName) ? null : log.FullName,
        InitiatedAt = log.InitiatedAt,
        PaidAt = log.PaidAt,
        VerifiedAt = log.VerifiedAt,
        ForwardedAt = log.ForwardedTimestamp
    };
}