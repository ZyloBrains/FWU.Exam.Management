using System.Text.RegularExpressions;
using FWU.Exam.Management.Domain.Helpers;

namespace FWU.Exam.Management.Domain.Constants;

public static class SymbolNumberDefaults
{
    public const int DefaultSequenceDigits = 4;
    public const int MaxSequenceDigits = 5;
    public const int DefaultStartSequence = 1;

    /// <summary>
    /// Prefix = last two digits of the current Bikram Sambat year + exam type id.
    /// e.g. BS 2082 with exam type 4 → "824".
    /// </summary>
    public static string BuildPrefix(int examTypeId, DateOnly? asOfDate = null)
    {
        var date = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var bsYear = NepaliDateConverter.AdToBs(date.ToDateTime(TimeOnly.MinValue)).Year;
        return (bsYear % 100).ToString("00") + examTypeId;
    }

    public static string Format(string prefix, int sequence, int sequenceDigits = DefaultSequenceDigits) =>
        prefix + sequence.ToString(new string('0', sequenceDigits));

    public static int MaxSequence(int sequenceDigits = DefaultSequenceDigits) =>
        int.Parse(new string('9', sequenceDigits));

    public static bool TryParseSequence(string prefix, string? symbolNumber, out int sequence)
    {
        sequence = 0;
        if (string.IsNullOrWhiteSpace(symbolNumber)) return false;
        if (!symbolNumber.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var tail = symbolNumber[prefix.Length..];
        return tail.Length > 0 && int.TryParse(tail, out sequence);
    }

    /// <summary>
    /// Strict structural validation: {prefix digits (BS year + exam type)}{4-5 digit sequence}.
    /// The prefix part is free-form digits so manual prefixes (e.g. "831…") validate.
    /// </summary>
    public static bool IsValidStrict(string? symbolNumber)
    {
        if (string.IsNullOrWhiteSpace(symbolNumber)) return false;
        return Regex.IsMatch(symbolNumber, $"^\\d{{3,6}}\\d{{{DefaultSequenceDigits},{MaxSequenceDigits}}}$");
    }
}
