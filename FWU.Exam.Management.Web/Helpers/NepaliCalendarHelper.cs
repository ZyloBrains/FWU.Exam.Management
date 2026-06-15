using NepDate;

namespace FWU.Exam.Management.Web.Helpers;

public static class NepaliCalendarHelper
{
    private static readonly string[] _nepaliMonths = {
        "बैशाख", "जेठ", "असार", "साउन", "भदौ", "असोज",
        "कार्तिक", "मंसिर", "पुष", "माघ", "फागुन", "चैत"
    };

    private static readonly string[] _nepaliDays = {
        "आइतबार", "सोमबार", "मंगलबार", "बुधबार", "बिहिबार",
        "शुक्रबार", "शनिबार"
    };

    private static readonly string[] _nepaliNumerals = {
        "०", "१", "२", "३", "४", "५", "६", "७", "८", "९"
    };

    private static string ToNepaliNumeral(int value)
    {
        var result = string.Empty;
        foreach (var c in value.ToString())
            result += _nepaliNumerals[c - '0'];
        return result;
    }

    public static string ToNepaliDateString(this DateTime adDate)
    {
        var nepaliDate = new NepaliDate(adDate);
        var dayOfWeek = (int)adDate.DayOfWeek;
        return $"{_nepaliDays[dayOfWeek]}, {_nepaliMonths[nepaliDate.Month - 1]} {ToNepaliNumeral(nepaliDate.Day)}, {ToNepaliNumeral(nepaliDate.Year)}";
    }

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
}
