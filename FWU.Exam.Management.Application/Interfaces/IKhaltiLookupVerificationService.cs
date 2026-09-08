using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

/// <summary>
/// Verification of a Khalti transaction, calling the live Khalti Lookup API rather than
/// trusting the local database or a browser callback. Used by the admin verification page.
/// </summary>
public interface IKhaltiLookupVerificationService
{
    /// <summary>
    /// Looks up a Khalti transaction by its pidx (or transaction id) and cross-references
    /// it with any matching local payment record.
    /// </summary>
    Task<KhaltiVerificationResultDto> VerifyByPidxAsync(string pidx);

    /// <summary>
    /// Resolves a local payment record by invoice number, then verifies its Khalti
    /// transaction via the live Lookup API.
    /// </summary>
    Task<KhaltiVerificationResultDto> VerifyByInvoiceAsync(string invoiceNumber);
}
