using System.Text;

namespace FWU.Exam.Management.Application.Helpers;

/// <summary>
/// The exam components (legs) a student registers for on a single subject.
/// <see cref="None"/> inside a parsed selection means "no explicit choice"
/// (legacy plain-id entries) — consumers fall back to the offering's own
/// <c>HasTheory</c>/<c>HasPractical</c> flags.
/// </summary>
[Flags]
public enum ReExamLegs
{
    None = 0,
    Theory = 1,
    Practical = 2
}

/// <summary>
/// Parses/formats the <c>PaymentRequestLog.SelectedSubjectIds</c> payload.
/// Tokens are comma separated: <c>"301"</c>, <c>"301:T"</c>, <c>"301:P"</c>,
/// <c>"301:TP"</c>. Plain ids stay valid forever (legacy logs).
/// </summary>
public static class ReExamSubjectSelection
{
    public static Dictionary<int, ReExamLegs> Parse(string? selected)
    {
        var result = new Dictionary<int, ReExamLegs>();
        if (string.IsNullOrWhiteSpace(selected))
            return result;

        foreach (var rawToken in selected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = rawToken.IndexOf(':');
            var idPart = separator >= 0 ? rawToken[..separator] : rawToken;
            if (!int.TryParse(idPart, out var offeringId) || offeringId <= 0)
                continue;

            var legs = ReExamLegs.None;
            if (separator >= 0)
            {
                foreach (var c in rawToken[(separator + 1)..])
                {
                    if (c is 'T' or 't') legs |= ReExamLegs.Theory;
                    else if (c is 'P' or 'p') legs |= ReExamLegs.Practical;
                }
            }

            result[offeringId] = legs;
        }

        return result;
    }

    public static string Format(IReadOnlyDictionary<int, ReExamLegs> selection)
    {
        var sb = new StringBuilder();
        foreach (var (offeringId, legs) in selection.OrderBy(kvp => kvp.Key))
        {
            if (sb.Length > 0) sb.Append(',');
            sb.Append(offeringId);
            var suffix = FormatLegs(legs);
            if (suffix.Length > 0)
            {
                sb.Append(':');
                sb.Append(suffix);
            }
        }

        return sb.ToString();
    }

    public static string FormatLegs(ReExamLegs legs)
    {
        if (legs == ReExamLegs.Theory) return "T";
        if (legs == ReExamLegs.Practical) return "P";
        if (legs.HasFlag(ReExamLegs.Theory) && legs.HasFlag(ReExamLegs.Practical)) return "TP";
        return "";
    }
}
