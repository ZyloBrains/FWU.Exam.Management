namespace FWU.Exam.Management.Domain.Extensions;

public static class StringExtensions
{
    public static string GetInitials(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1) 
            return words[0][0].ToString().ToUpperInvariant();
        if (words.Length == 2)
            return string.Concat(words[0][0], words[1][0]).ToUpperInvariant();
        return string.Concat(words[0][0], words[^1][0]).ToUpperInvariant();
    }

    public static string GetFullName(this string? firstName, string? lastName)
    {
        return string.Join(" ", new[] { firstName?.Trim(), lastName?.Trim() }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    public static string GetFullName(this string? firstName, string? middleName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(middleName))
            return GetFullName(firstName, lastName);
        return string.Join(" ", new[] { firstName?.Trim(), middleName?.Trim(), lastName?.Trim() }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    public static string EscapeCsv(this string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
