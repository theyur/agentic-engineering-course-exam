using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
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
/// The goal page's Recent notes card (HP-001, US4-AS6, AS7; design/pages/PageGoalDetail, slice 5 state), rendered from
/// the notes the real RecordingService reads over the fakes, with the schedule kind and period of the goal's history.
/// The user has no stored time zone, so the goals are in UTC and every time below is a UTC literal. Records and notes
/// are seeded straight into the fake record store; each goal is created at 08:00 on its start date.
/// </summary>
public sealed class RecentNotesCardTests : BunitContext
{
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

    public RecentNotesCardTests()
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

    private void SeedNote(GoalId goal, DateOnly key, string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        _records.SeedNote(goal, key, note!);
    }

    /// <summary>Reads the recent notes and the goal's settings at <paramref name="at"/> and renders the card with them, as the goal page does.</summary>
    private async Task<IRenderedComponent<RecentNotesCard>> RenderCardAsync(GoalId goal, DateTimeOffset at)
    {
        _clock.SetUtcNow(at);
        await using var scope = Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<RecordingService>();
        var notes = await service.GetRecentNotesAsync(_user, goal, CancellationToken.None);
        var recordable = await service.GetRecordableAsync(_user, goal, CancellationToken.None);
        Assert.Equal(RecentNotesOutcome.Found, notes.Outcome);
        var history = recordable.History!;
        return Render<RecentNotesCard>(p => p
            .Add(x => x.Notes, notes.Notes)
            .Add(x => x.ScheduleKind, history.GoalScheduleKind)
            .Add(x => x.Period, history.Period));
    }

    private static List<(string Note, string Chip, string Date)> Items(IRenderedComponent<RecentNotesCard> cut) =>
        cut.FindAll("section.ht-card > div.ht-goal")
            .Select(item => (
                Text(item.QuerySelector("div.ht-goal__top > span:first-child")!),
                Text(item.QuerySelector("div.ht-goal__top > span.ht-chip.ht-status")!),
                Text(item.QuerySelector("div.ht-goal__meta")!)))
            .ToList();

    [Fact]
    public async Task US4_AS7_the_card_shows_the_5_latest_notes_newest_first_each_in_quotes_with_its_chip_and_date()
    {
        // At 10:00Z on Tuesday 17 November: Tuesday 17 is Due, Monday 16 and Sunday 15 are Pending, Saturday 14 (grace
        // end 00:00Z on Tuesday 17) and earlier are final.
        var goal = await GoalAsync(Reading20MinDaily, new DateOnly(2026, 11, 2));
        _records.Seed(goal, new DateOnly(2026, 11, 12), RecordValue.FromAmount(12));
        _records.Seed(goal, new DateOnly(2026, 11, 13), RecordValue.FromAmount(20));
        SeedNote(goal, new DateOnly(2026, 11, 3), "Too old to show.");
        SeedNote(goal, new DateOnly(2026, 11, 12), "Fell asleep early.");
        SeedNote(goal, new DateOnly(2026, 11, 13), "Read on the train.");
        SeedNote(goal, new DateOnly(2026, 11, 14), "Away for the weekend.");
        SeedNote(goal, new DateOnly(2026, 11, 16), "Busy day.");
        SeedNote(goal, new DateOnly(2026, 11, 17), "Started a new book.");

        var cut = await RenderCardAsync(goal, new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal("Recent notes", Text(cut.Find("section.ht-card > h2.ht-card__title")));
        Assert.Equal(
            [
                ("\"Started a new book.\"", "Due today", "Tue 17 Nov"),
                ("\"Busy day.\"", "Pending", "Mon 16 Nov"),
                ("\"Away for the weekend.\"", "Missed", "Sat 14 Nov"),
                ("\"Read on the train.\"", "Met", "Fri 13 Nov"),
                ("\"Fell asleep early.\"", "Failed", "Thu 12 Nov"),
            ],
            Items(cut));
        Assert.DoesNotContain("Too old to show.", cut.Markup, StringComparison.Ordinal);
        Assert.All(cut.FindAll("div.ht-goal__top"), top => Assert.NotNull(top.QuerySelector("span.ht-spacer + span.ht-chip")));
    }

    [Fact]
    public async Task US4_AS7_a_weeks_note_reads_its_date_range_and_a_Due_week_reads_Due_this_week()
    {
        // At 10:00Z on Tuesday 18 August 2026 the week of 17 August is Due and the week of 27 July (grace end 00:00Z on
        // Wednesday 5 August) is final, Failed with one Yes of 3.
        var goal = await GoalAsync(YogaThreeTimesAWeek, new DateOnly(2026, 7, 27));
        _records.Seed(goal, new DateOnly(2026, 7, 28), RecordValue.FromAnswer(YesNoAnswer.Yes));
        SeedNote(goal, new DateOnly(2026, 7, 27), "Knee was sore.");
        SeedNote(goal, new DateOnly(2026, 8, 17), "Better today.");

        var cut = await RenderCardAsync(goal, new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(
            [
                ("\"Better today.\"", "Due this week", "17 – 23 Aug"),
                ("\"Knee was sore.\"", "Failed", "27 Jul – 2 Aug"),
            ],
            Items(cut));
    }

    [Fact]
    public async Task A_goal_without_notes_renders_no_card()
    {
        var goal = await GoalAsync(Reading20MinDaily, new DateOnly(2026, 11, 2));
        _records.Seed(goal, new DateOnly(2026, 11, 3), RecordValue.FromAmount(20));

        var cut = await RenderCardAsync(goal, new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    [Fact]
    public void The_chip_of_a_note_takes_the_schedule_kind_the_page_passes()
    {
        var note = new RecentNote { FirstDate = new DateOnly(2026, 11, 17), LastDate = new DateOnly(2026, 11, 17), Note = "Two cups.", State = EvaluationState.Due };

        var cut = Render<RecentNotesCard>(p => p
            .Add(x => x.Notes, [note])
            .Add(x => x.ScheduleKind, GoalScheduleKind.PeriodBased)
            .Add(x => x.Period, SchedulePeriod.Day));

        Assert.Equal([("\"Two cups.\"", "Due today", "Tue 17 Nov")], Items(cut));
        Assert.Contains("ht-status--due", cut.Find("span.ht-chip").ClassList);
    }
}
