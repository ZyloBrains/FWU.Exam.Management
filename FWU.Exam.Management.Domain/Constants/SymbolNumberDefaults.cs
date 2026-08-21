namespace FWU.Exam.Management.Domain.Constants;

public static class SymbolNumberDefaults
{
    public const string Prefix = "814";
    public const int SequenceDigits = 4;
    public const int DefaultStartSequence = 1;

    public static string Format(int sequence) =>
        Prefix + sequence.ToString(new string('0', SequenceDigits));

    public static bool TryParseSequence(string? symbolNumber, out int sequence)
    {
        sequence = 0;
        if (string.IsNullOrWhiteSpace(symbolNumber)) return false;
        if (!symbolNumber.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var tail = symbolNumber[Prefix.Length..];
        return int.TryParse(tail, out sequence);
    }
}
