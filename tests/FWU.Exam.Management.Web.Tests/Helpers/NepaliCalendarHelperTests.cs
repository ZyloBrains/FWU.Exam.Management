using FluentAssertions;
using FWU.Exam.Management.Web.Helpers;

namespace FWU.Exam.Management.Web.Tests.Helpers;

public class NepaliCalendarHelperTests
{
    [Fact]
    public void AdToBs_WithKnownDate_ReturnsCorrectNepaliDate()
    {
        var adDate = new DateTime(2024, 4, 13);

        var (year, month, day) = NepaliCalendarHelper.AdToBs(adDate);

        year.Should().Be(2081);
        month.Should().Be(1);
        day.Should().Be(1);
    }

    [Fact]
    public void AdToBs_WithMidYearDate_ReturnsCorrectValues()
    {
        var adDate = new DateTime(2024, 7, 16);

        var (year, month, day) = NepaliCalendarHelper.AdToBs(adDate);

        year.Should().Be(2081);
        month.Should().Be(4);
        day.Should().Be(1);
    }

    [Fact]
    public void BsToAd_WithValidBsDate_ReturnsAdDate()
    {
        var adDate = NepaliCalendarHelper.BsToAd(2081, 1, 1);

        adDate.Should().NotBeNull();
        adDate.Value.Year.Should().Be(2024);
        adDate.Value.Month.Should().Be(4);
        adDate.Value.Day.Should().Be(13);
    }

    [Fact]
    public void BsToAd_WithInvalidBsDate_ReturnsNull()
    {
        var result = NepaliCalendarHelper.BsToAd(9999, 99, 99);
        result.Should().BeNull();
    }

    [Fact]
    public void ToNepaliDateString_ReturnsFormattedString()
    {
        var adDate = new DateTime(2024, 4, 14);

        var result = adDate.ToNepaliDateString();

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("२०८१");
        result.Should().Contain("बैशाख");
    }

    [Fact]
    public void AdToBs_RoundTrip_Matches()
    {
        var original = new DateTime(2025, 1, 15);

        var (year, month, day) = NepaliCalendarHelper.AdToBs(original);
        var convertedBack = NepaliCalendarHelper.BsToAd(year, month, day);

        convertedBack.Should().NotBeNull();
        convertedBack.Value.Date.Should().Be(original.Date);
    }
}
