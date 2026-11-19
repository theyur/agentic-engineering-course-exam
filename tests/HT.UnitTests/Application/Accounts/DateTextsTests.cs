using HT.Application.Accounts;

namespace HT.UnitTests.Application.Accounts;

/// <summary>
/// The one date formatter (research R-11): fixed English names whatever the culture (AC-088). Every test runs inside
/// a non-English culture whose own names, digits or calendar differ, so a culture read would change the text.
/// </summary>
public sealed class DateTextsTests
{
    /// <summary>Monday 16 to Sunday 22 November 2026: every weekday once.</summary>
    public static TheoryData<int, string, string> Week => new()
    {
        { 16, "Monday", "Mon" },
        { 17, "Tuesday", "Tue" },
        { 18, "Wednesday", "Wed" },
        { 19, "Thursday", "Thu" },
        { 20, "Friday", "Fri" },
        { 21, "Saturday", "Sat" },
        { 22, "Sunday", "Sun" },
    };

    /// <summary>The 14th of every month of 2026.</summary>
    public static TheoryData<int, string, string> Year => new()
    {
        { 1, "January", "Jan" },
        { 2, "February", "Feb" },
        { 3, "March", "Mar" },
        { 4, "April", "Apr" },
        { 5, "May", "May" },
        { 6, "June", "Jun" },
        { 7, "July", "Jul" },
        { 8, "August", "Aug" },
        { 9, "September", "Sep" },
        { 10, "October", "Oct" },
        { 11, "November", "Nov" },
        { 12, "December", "Dec" },
    };

    [Theory]
    [MemberData(nameof(Week))]
    public void Every_weekday_has_its_English_names(int day, string weekday, string shortWeekday)
    {
        using var culture = new CultureScope("ar-SA");
        var date = new DateOnly(2026, 11, day);

        Assert.Equal(weekday, DateTexts.Weekday(date));
        Assert.Equal($"{weekday}, {day} November", DateTexts.LongDate(date));
        Assert.Equal($"{shortWeekday} {day} Nov", DateTexts.ShortDate(date));
    }

    [Theory]
    [MemberData(nameof(Year))]
    public void Every_month_has_its_English_names(int month, string name, string shortName)
    {
        using var culture = new CultureScope("uk-UA");
        var date = new DateOnly(2026, month, 14);

        Assert.Equal($"14 {shortName}", DateTexts.DayMonth(date));
        Assert.Equal($"14 {shortName} 2026", DateTexts.DayMonthYear(date));
        Assert.Equal($"{name} 2026", DateTexts.MonthYear(date));
        Assert.EndsWith($", 14 {name}", DateTexts.LongDate(date), StringComparison.Ordinal);
        Assert.EndsWith($" 14 {shortName}", DateTexts.ShortDate(date), StringComparison.Ordinal);
    }

    [Fact]
    public void The_spec_examples_read_as_written()
    {
        using var culture = new CultureScope("th-TH");

        Assert.Equal("Tuesday, 17 November", DateTexts.LongDate(new DateOnly(2026, 11, 17)));
        Assert.Equal("Tue 17 Nov", DateTexts.ShortDate(new DateOnly(2026, 11, 17)));
        Assert.Equal("23 Nov", DateTexts.DayMonth(new DateOnly(2026, 11, 23)));
        Assert.Equal("14 Jul 2026", DateTexts.DayMonthYear(new DateOnly(2026, 7, 14)));
        Assert.Equal("November 2026", DateTexts.MonthYear(new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Member_since_writes_the_month_as_MonthYear_does()
    {
        using var culture = new CultureScope("uk-UA");

        Assert.Equal("member since November 2026", AccountTexts.MemberSince(2026, 11));
        Assert.Equal("member since January 2027", AccountTexts.MemberSince(2027, 1));
    }

    [Theory]
    [InlineData("ar-SA")]
    [InlineData("uk-UA")]
    [InlineData("th-TH")]
    public void A_week_range_names_the_month_once_within_a_month_and_both_months_across_a_month_or_year_end(string cultureName)
    {
        using var culture = new CultureScope(cultureName);

        Assert.Equal("10 – 16 Aug", DateTexts.WeekRange(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 16)));
        Assert.Equal("28 Sep – 4 Oct", DateTexts.WeekRange(new DateOnly(2026, 9, 28), new DateOnly(2026, 10, 4)));
        Assert.Equal("28 Dec – 3 Jan", DateTexts.WeekRange(new DateOnly(2026, 12, 28), new DateOnly(2027, 1, 3)));
    }

    [Fact]
    public void A_week_range_has_an_en_dash_between_spaces_and_no_leading_zero()
    {
        using var culture = new CultureScope("ar-SA");

        var text = DateTexts.WeekRange(new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 1));

        Assert.Equal("26 Oct – 1 Nov", text);
        Assert.Equal("1 – 7 Feb", DateTexts.WeekRange(new DateOnly(2027, 2, 1), new DateOnly(2027, 2, 7)));
    }

    [Fact]
    public void A_single_digit_day_has_no_leading_zero()
    {
        using var culture = new CultureScope("ar-SA");

        Assert.Equal("Thursday, 1 January", DateTexts.LongDate(new DateOnly(2026, 1, 1)));
        Assert.Equal("Sat 3 Jan", DateTexts.ShortDate(new DateOnly(2026, 1, 3)));
        Assert.Equal("5 Jan 2026", DateTexts.DayMonthYear(new DateOnly(2026, 1, 5)));
    }
}
