using NepDate;

namespace FWU.Exam.Management.Domain.Helpers;

public static class NepaliDateConverter
{
    public static DateTime? BsToAd(int bsYear, int bsMonth, int bsDay)
    {
        try
        {
            return new NepaliDate(bsYear, bsMonth, bsDay).EnglishDate;
        }
        catch
        {
            return null;
        }
    }

    public static (int Year, int Month, int Day) AdToBs(DateTime adDate)
    {
        var nepaliDate = new NepaliDate(adDate);
        return (nepaliDate.Year, nepaliDate.Month, nepaliDate.Day);
    }

    public static string? AdToBsString(DateOnly adDate)
    {
        var (year, month, day) = AdToBs(adDate.ToDateTime(TimeOnly.MinValue));
        return $"{year:D4}-{month:D2}-{day:D2}";
    }

    public static bool TryParseBs(string? bsDate, out DateTime? adDate)
    {
        adDate = null;
        if (string.IsNullOrWhiteSpace(bsDate)) return false;

        var parts = bsDate.Split('-');
        if (parts.Length != 3
            || !int.TryParse(parts[0], out var y)
            || !int.TryParse(parts[1], out var m)
            || !int.TryParse(parts[2], out var d))
            return false;

        adDate = BsToAd(y, m, d);
        return adDate.HasValue;
    }
}
