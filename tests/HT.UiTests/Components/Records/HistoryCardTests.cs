using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Ui.Components.Records;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Records;

/// <summary>
/// The goal page's History card (HP-001, US5-AS5; design/pages/PageGoalDetail, slice 5 state), rendered from the
/// history that the real RecordingService reads over the fakes. The user has no stored time zone, so the goals are in
/// UTC and every time below is a UTC literal. Records and notes are seeded straight into the fake record store; each
/// goal is created at 08:00 on its start date, and its history read at the instant the test names.
/// </summary>
public sealed class HistoryCardTests : BunitContext
{
    private const string Sentence = "Corrections are evaluated with the rules of that date.";

    private static readonly DateOnly Mon2Nov = new(2026, 11, 2);
    private static readonly DateOnly Tue3Nov = new(2026, 11, 3);
    private static readonly DateOnly Fri13Nov = new(2026, 11, 13);
    private static readonly DateOnly Mon16Nov = new(2026, 11, 16);
    private static readonly DateOnly Mon10Aug = new(2026, 8, 10);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

    public HistoryCardTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
    }

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields YogaThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
        ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Creates the goal at 08:00 UTC on <paramref name="start"/>, its start date.</summary>
    private async Task<GoalId> GoalAsync(GoalTargetFields target, DateOnly start)
    {
        _clock.SetUtcNow(new DateTimeOffset(start.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero));
        var habit = _catalog.Add(HabitSamples.Catalog("Reading"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>The history from <paramref name="first"/> to <paramref name="last"/> at <paramref name="at"/>.</summary>
    private async Task<GoalHistory> HistoryAsync(GoalId goal, DateOnly first, DateOnly last, DateTimeOffset at)
    {
        _clock.SetUtcNow(at);
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(_user, goal, first, last, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>The history of the 366 days up to <paramref name="current"/>, as the goal page reads it, at <paramref name="at"/>.</summary>
    private Task<GoalHistory> PageHistoryAsync(GoalId goal, DateOnly current, DateTimeOffset at) =>
        HistoryAsync(goal, current.AddDays(-365), current, at);

    private IRenderedComponent<HistoryCard> RenderCard(GoalHistory history) =>
        Render<HistoryCard>(p => p.Add(x => x.History, history));

    private static List<string> Dates(IRenderedComponent<HistoryCard> cut) =>
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("td")!)).ToList();

    private static string Cell(IRenderedComponent<HistoryCard> cut, string date, int column) =>
        Text(cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("td")!) == date).QuerySelectorAll("td")[column]);

    /// <summary>
    /// Reading, 20 min daily, from Monday 2 November 2026, read at 10:00Z on Friday 13 November: 12 begun days, 20 min on
    /// the 2nd (Met), 12 min on the 3rd (Failed) with the note "Fell asleep early.", nothing since.
    /// </summary>
    private async Task<IRenderedComponent<HistoryCard>> TwelveDaysAsync()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);
        _records.Seed(goal, Mon2Nov, RecordValue.FromAmount(20));
        _records.Seed(goal, Tue3Nov, RecordValue.FromAmount(12));
        Assert.True(NoteText.TryCreate("Fell asleep early.", out var note, out _));
        _records.SeedNote(goal, Tue3Nov, note!);
        return RenderCard(await PageHistoryAsync(goal, Fri13Nov, new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task The_card_head_has_the_title_History_and_the_sentence_beside_it()
    {
        var cut = await TwelveDaysAsync();

        var head = cut.Find("section.ht-card > div.ht-card__head");
        Assert.Equal("History", Text(head.QuerySelector("h2.ht-card__title")!));
        Assert.NotNull(head.QuerySelector("span.ht-spacer"));
        Assert.Equal(Sentence, Text(head.QuerySelector("p.ht-card__sub")!));
    }

    [Fact]
    public async Task The_table_has_the_five_columns_the_last_one_empty_headed()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal("History", cut.Find("table.ht-table").GetAttribute("aria-label"));
        Assert.Equal(["Date", "Recorded", "Result", "Note", ""], cut.FindAll("thead th").Select(Text));
    }

    [Fact]
    public async Task The_rows_are_newest_first_with_the_first_10_then_Show_all_N()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal(
            ["Fri 13 Nov", "Thu 12 Nov", "Wed 11 Nov", "Tue 10 Nov", "Mon 9 Nov", "Sun 8 Nov", "Sat 7 Nov", "Fri 6 Nov", "Thu 5 Nov", "Wed 4 Nov"],
            Dates(cut));
        var showAll = cut.Find("section.ht-card > button.ht-btn.ht-btn--ghost.ht-btn--sm");
        Assert.Equal("Show all 12 →", Text(showAll));
        Assert.Equal("false", showAll.GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Show_all_shows_the_rest_in_place()
    {
        var cut = await TwelveDaysAsync();

        cut.Find("section.ht-card > button").Click();

        Assert.Equal(12, cut.FindAll("tbody tr").Count);
        Assert.Equal(["Tue 3 Nov", "Mon 2 Nov"], Dates(cut)[10..]);
        Assert.Empty(cut.FindAll("section.ht-card > button"));
    }

    [Fact]
    public async Task Each_row_has_its_record_its_chip_its_note_text_and_an_empty_actions_cell()
    {
        var cut = await TwelveDaysAsync();
        cut.Find("section.ht-card > button").Click();

        Assert.Equal("12 min", Cell(cut, "Tue 3 Nov", 1));
        Assert.Equal("Failed", Cell(cut, "Tue 3 Nov", 2));
        Assert.Equal("Fell asleep early.", Cell(cut, "Tue 3 Nov", 3));
        Assert.Equal("20 min", Cell(cut, "Mon 2 Nov", 1));
        Assert.Equal("Met", Cell(cut, "Mon 2 Nov", 2));
        Assert.Equal("", Cell(cut, "Mon 2 Nov", 3));
        Assert.Equal("Due today", Text(cut.Find("tbody tr td span.ht-chip.ht-status.ht-status--due")));
        Assert.All(cut.FindAll("tbody tr"), row =>
        {
            var actions = row.QuerySelectorAll("td")[4];
            Assert.True(actions.ClassList.Contains("ht-actions"));
            Assert.Empty(actions.Children);
            Assert.Equal("", actions.TextContent);
        });
        Assert.Empty(cut.FindAll("tbody button"));
    }

    [Fact]
    public async Task A_row_without_a_record_reads_a_dash()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal("—", Cell(cut, "Fri 13 Nov", 1));
        Assert.Equal("—", Cell(cut, "Wed 4 Nov", 1));
        Assert.Equal("Missed", Cell(cut, "Wed 4 Nov", 2));
    }

    [Fact]
    public async Task Ten_rows_have_no_Show_all()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 11, 11), new DateTimeOffset(2026, 11, 11, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(10, cut.FindAll("tbody tr").Count);
        Assert.Empty(cut.FindAll("section.ht-card > button"));
        Assert.DoesNotContain("Show all", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_week_reads_10_to_16_Aug_and_its_records_sit_side_by_side()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, Mon10Aug);
        _records.Seed(goal, new DateOnly(2026, 8, 11), RecordValue.FromAnswer(YesNoAnswer.Yes));
        _records.Seed(goal, new DateOnly(2026, 8, 12), RecordValue.FromAnswer(YesNoAnswer.No));

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(["17 – 23 Aug", "10 – 16 Aug"], Dates(cut));
        Assert.Equal("Due this week", Cell(cut, "17 – 23 Aug", 2));
        var week = cut.FindAll("tbody tr")[1];
        Assert.Equal(["Yes", "No"], week.QuerySelectorAll("td")[1].QuerySelectorAll("span.ht-inline > span").Select(Text));
        Assert.Equal("Pending", Cell(cut, "10 – 16 Aug", 2));
    }

    [Fact]
    public async Task Only_begun_entries_have_a_row()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon16Nov);

        var cut = RenderCard(await HistoryAsync(goal, Mon16Nov, new DateOnly(2026, 11, 20), new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(["Tue 17 Nov", "Mon 16 Nov"], Dates(cut));
    }

    [Fact]
    public async Task A_history_of_Upcoming_entries_only_renders_no_card()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon16Nov);

        var history = await HistoryAsync(goal, new DateOnly(2026, 11, 18), new DateOnly(2026, 11, 20), new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        var cut = RenderCard(history);

        Assert.Equal(3, history.Entries.Count);
        Assert.False(HistoryCard.HasRows(history));
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    [Fact]
    public async Task A_goal_without_a_begun_entry_renders_no_card()
    {
        // A per-week goal from Wednesday 19 August 2026 has its first week from Monday 24 August.
        var goal = await GoalAsync(YogaThreeTimesAWeek, new DateOnly(2026, 8, 19));

        var history = await PageHistoryAsync(goal, new DateOnly(2026, 8, 20), new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero));
        var cut = RenderCard(history);

        Assert.Empty(history.Entries);
        Assert.False(HistoryCard.HasRows(history));
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }
}
