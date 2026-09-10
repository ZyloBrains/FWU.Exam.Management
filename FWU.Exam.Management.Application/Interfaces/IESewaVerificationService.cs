using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

/// <summary>
/// Verification of an eSewa transaction, calling the live eSewa transaction
/// status endpoint rather than trusting the local database or a browser callback.
/// Used by the admin verification page.
/// </summary>
public interface IESewaVerificationService
{
    /// <summary>
    /// Looks up an eSewa transaction by its transaction_uuid and cross-references
    /// it with any matching local payment record. The optional <paramref name="amountOverride"/>
    /// is used only when no local record is found, since eSewa requires the exact
    /// total_amount to answer a status query.
    /// </summary>
    Task<ESewaVerificationResultDto> VerifyByUuidAsync(string transactionUuid, decimal? amountOverride = null);

    /// <summary>
    /// Resolves a local payment record by invoice number, then verifies its eSewa
    /// transaction via the live status endpoint.
    /// </summary>
    Task<ESewaVerificationResultDto> VerifyByInvoiceAsync(string invoiceNumber);
}