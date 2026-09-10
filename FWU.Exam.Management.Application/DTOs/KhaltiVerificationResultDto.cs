using FWU.Exam.Management.Application.Interfaces;

namespace FWU.Exam.Management.Application.DTOs;

/// <summary>
/// Result of verifying a Khalti transaction against the live Khalti Lookup API and
/// cross-referencing it with the local payment record. Used by the admin verification page.
/// </summary>
public class KhaltiVerificationResultDto
{
    /// <summary>True when the Khalti Lookup API returned a usable response.</summary>
    public bool LookupSucceeded { get; set; }

    /// <summary>Human-readable summary of the outcome (match verdict / error).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Machine-readable verdict code used by the UI for styling.</summary>
    public string Verdict { get; set; } = string.Empty;

    // --- Raw live Khalti lookup values (the exact API response shape) ---
    public string? Pidx { get; set; }
    public long TotalAmountPaisa { get; set; }
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public long FeePaisa { get; set; }
    public bool Refunded { get; set; }

    // --- Derived display helpers ---
    public decimal TotalAmountRupees => TotalAmountPaisa / 100m;
    public decimal FeeRupees => FeePaisa / 100m;

    // --- Matched local payment record (may be null) ---
    public LocalPaymentRecordDto? LocalRecord { get; set; }

    /// <summary>True when the Khalti amount matches the local record's amount (paisa).</summary>
    public bool AmountMatches => LocalRecord != null && LocalRecord.AmountRupees == TotalAmountRupees;
}

public class LocalPaymentRecordDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal AmountRupees { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentProvider { get; set; }
    public string? ProviderReferenceId { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? StudentName { get; set; }
    public DateTime? InitiatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime ForwardedAt { get; set; }
}
