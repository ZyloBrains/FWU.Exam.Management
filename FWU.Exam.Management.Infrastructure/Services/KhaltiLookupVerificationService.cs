using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Verifies Khalti transactions against the live Khalti Lookup API and cross-references the
/// result with the local <see cref="PaymentRequestLog"/>. This is the "verify a completed
/// transaction" tool: it trusts Khalti's response, not the local DB or a browser callback.
/// The Khalti secret key is used only inside <see cref="KhaltiService"/> and is never exposed.
/// </summary>
public class KhaltiLookupVerificationService(
    AppDbContext context,
    IKhaltiService khaltiService,
    ILogger<KhaltiLookupVerificationService> logger) : IKhaltiLookupVerificationService
{
    public async Task<KhaltiVerificationResultDto> VerifyByPidxAsync(string pidx)
    {
        if (string.IsNullOrWhiteSpace(pidx))
            return new KhaltiVerificationResultDto { LookupSucceeded = false, Verdict = "NOREQ", Message = "No pidx / transaction reference was provided." };

        var trimmed = pidx.Trim();
        var local = await FindLocalByReferenceAsync(trimmed);
        return await LookupAndBuildAsync(trimmed, local);
    }

    public async Task<KhaltiVerificationResultDto> VerifyByInvoiceAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return new KhaltiVerificationResultDto { LookupSucceeded = false, Verdict = "NOREQ", Message = "No invoice number was provided." };

        var local = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.InvoiceNumber == invoiceNumber.Trim());

        if (local == null)
            return new KhaltiVerificationResultDto
            {
                LookupSucceeded = false,
                Verdict = "NOTFOUND",
                Message = $"No local payment record found for invoice '{invoiceNumber.Trim()}'."
            };

        var reference = local.ProviderReferenceId ?? local.TransactionId;
        if (string.IsNullOrWhiteSpace(reference))
            return new KhaltiVerificationResultDto
            {
                LookupSucceeded = false,
                Verdict = "NOREF",
                Message = $"Invoice '{invoiceNumber.Trim()}' matched local log #{local.Id} but it has no Khalti pidx stored, so the gateway cannot be queried.",
                LocalRecord = MapLocal(local)
            };

        return await LookupAndBuildAsync(reference, local, invoiceNumber.Trim());
    }

    private async Task<PaymentRequestLog?> FindLocalByReferenceAsync(string reference)
    {
        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Where(l => l.ProviderReferenceId == reference || l.TransactionId == reference)
            .OrderByDescending(l => l.ForwardedTimestamp)
            .FirstOrDefaultAsync();
    }

    private async Task<KhaltiVerificationResultDto> LookupAndBuildAsync(
        string reference, PaymentRequestLog? local, string? invoiceHint = null)
    {
        var result = new KhaltiVerificationResultDto
        {
            LocalRecord = local == null ? null : MapLocal(local),
            Pidx = reference
        };

        KhaltiLookupResponse? lookup;
        try
        {
            lookup = await khaltiService.LookupPaymentAsync(reference);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "KhaltiLookupVerification: lookup failed for reference={Reference}", reference);
            result.LookupSucceeded = false;
            result.Verdict = "ERROR";
            result.Message = $"Khalti Lookup API returned an error: {ex.Message}";
            return result;
        }

        if (lookup == null)
        {
            result.LookupSucceeded = false;
            result.Verdict = "ERROR";
            result.Message = "Khalti Lookup API returned no response.";
            return result;
        }

        result.LookupSucceeded = true;
        result.Pidx = lookup.Pidx ?? reference;
        result.TotalAmountPaisa = lookup.TotalAmount;
        result.Status = lookup.Status;
        result.TransactionId = lookup.TransactionId;
        result.FeePaisa = lookup.Fee;
        result.Refunded = lookup.Refunded;

        // Cross-reference with the local record.
        if (local == null)
        {
            result.Verdict = "NOTMATCHED";
            result.Message = "The Khalti transaction was found, but no matching local payment record exists in the database.";
            return result;
        }

        var expectedPaisa = (long)(local.Amount * 100m);
        if (lookup.TotalAmount != expectedPaisa)
        {
            result.Verdict = "AMOUNTMISMATCH";
            result.Message = $"Khalti reports {lookup.TotalAmount} paisa (Rs. {lookup.TotalAmount / 100m:N2}) but the local record expects {expectedPaisa} paisa (Rs. {local.Amount:N2}).";
            return result;
        }

        if (!string.Equals(lookup.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            result.Verdict = "NOTCOMPLETED";
            result.Message = $"Khalti reports status '{lookup.Status}'. The payment is not completed.";
            return result;
        }

        result.Verdict = "COMPLETED";
        result.Message = "Verified — Khalti reports Completed and the amount matches the local record.";
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
