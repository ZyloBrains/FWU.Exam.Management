namespace FWU.Exam.Management.Application.DTOs;

/// <summary>
/// Result of verifying an eSewa transaction against the live eSewa transaction
/// status endpoint and cross-referencing it with the local payment record.
/// Used by the admin verification page.
/// </summary>
public class ESewaVerificationResultDto
{
    /// <summary>True when the eSewa status endpoint returned a usable response.</summary>
    public bool LookupSucceeded { get; set; }

    /// <summary>Human-readable summary of the outcome (match verdict / error).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Machine-readable verdict code used by the UI for styling.</summary>
    public string Verdict { get; set; } = string.Empty;

    // --- Raw live eSewa status values (the exact API response shape) ---
    public string? TransactionUuid { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? TransactionCode { get; set; }
    public string? RefId { get; set; }
    public string? ProductCode { get; set; }

    // --- Matched local payment record (may be null) ---
    public LocalPaymentRecordDto? LocalRecord { get; set; }

    /// <summary>True when the eSewa amount matches the local record's amount (whole rupees).</summary>
    public bool AmountMatches => LocalRecord != null && LocalRecord.AmountRupees == TotalAmount;
}