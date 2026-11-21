using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// HP-004, US2-AS7, US7-AS8, FR-020, and slice 5 HP-001 and US1-AS9: the goal page over the real GoalService,
/// RecordingService and the fakes, with "Record" when something can be recorded now. Today is Tuesday 17 November
/// 2026 in UTC, the zone of the signed-in user (no stored zone, the UTC default), as in the design's preview.
/// </summary>
public sealed class GoalDetailPageTests : BunitContext
{
    private const string Sentence = "Changes apply from the next commitment. Past results keep their rules.";
    private const string NoticeText = "This habit is no longer in the public catalog. Your goal keeps working.";

    private static readonly DateOnly Today = new(2026, 11, 17);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeCustomHabitStore _customHabits;
    private readonly FakeGoalStore _goals;
    private readonly FakeRecordStore _records;
    private int _notFound;

    public GoalDetailPageTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        _customHabits = this.AddCustomHabits();
        _goals = this.AddGoals();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        Services.GetRequiredService<BunitNavigationManager>().OnNotFound += (_, _) => _notFound++;
    }

    private ActingUser SignInUser() => ActingUser.ForAccount(new AccountId(_authorization.SignIn("RegisteredUser")), Role.RegisteredUser);

    private IRenderedComponent<GoalDetail> RenderGoal(GoalId id) => Render<GoalDetail>(p => p.Add(x => x.Id, id.Value));

    private async Task<GoalId> CreateAsync(ActingUser user, GoalHabit habit, GoalTargetFields target, DateOnly start)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static List<(string Label, string Value)> Rows(IRenderedComponent<GoalDetail> cut) =>
        cut.FindAll("dl.ht-kv dt").Zip(cut.FindAll("dl.ht-kv dd"), (dt, dd) => (Text(dt), Text(dd))).ToList();

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields YesNoOnMonWedFri => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    };

    [Fact]
    public void US2_AS7_a_commitment_based_goal_that_has_started_shows_the_head_the_attempt_line_and_the_Settings_card()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var yoga = _catalog.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var start = new DateOnly(2026, 7, 14);
        Assert.True(GoalTarget.TryCreate(YesNoOnMonWedFri, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(yoga.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(yoga.Id, HabitKind.Catalog, "Yoga", Category.Body, true));

        var cut = RenderGoal(goal.Id);

        Assert.Equal(0, _notFound);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Goals", back.TextContent);
        Assert.Equal("/goals", back.GetAttribute("href"));
        Assert.Equal("Yoga, 3 days a week", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Body", "Build", "Commitment-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · started 14 Jul", cut.Find(".ht-head-tags .ht-attempt").TextContent);
        // Slice 5 (T055): the History card in the left column, the Settings card in the right one; slice 6 (HP-001): the
        // GoalStats card, then the TrendChart card, above the History card.
        var columns = cut.FindAll(".ht-grid-2 > .ht-stack");
        Assert.Equal(2, columns.Count);
        Assert.Equal(["Consistency and streaks", "Days done per week", "History"], columns[0].QuerySelectorAll(".ht-card .ht-card__title").Select(Text));
        Assert.Equal("Settings", columns[1].QuerySelector(".ht-card .ht-card__title")!.TextContent);
        Assert.Equal(
            [
                ("Habit", "Yoga · catalog"), ("Build or reduce", "Build"), ("Measure", "Yes / no"), ("Target", "3 days a week"),
                ("Schedule", "Mon · Wed · Fri"), ("Evaluated", "Each scheduled day"), ("Started", "14 Jul 2026"),
            ],
            Rows(cut));
        Assert.Equal(Sentence, cut.Find(".ht-grid-2 > .ht-stack:last-child .ht-card .ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
        // Tuesday 17 November is not scheduled, but Monday 16 is Pending: "Record" is the page head's primary action
        // (HP-001), after slice 7's "Pause" for a goal that is not paused (T064).
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        foreach (var absent in new[] { "Edit settings", "End goal", "Earlier attempts" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }

        // Slice 7 (T042): the GoalStats legend now lists "Excused" and "Paused", so the markup contains "Excuse"; "Excuse
        // instead" is only in the RecordDialog and "Remove excusal" only on an Excused row (T052).
        var buttons = cut.FindAll("button").Select(b => b.TextContent.Trim()).ToList();
        foreach (var absent in new[] { "Excuse instead", "Remove excusal" })
        {
            Assert.DoesNotContain(absent, buttons);
        }
    }

    [Fact]
    public async Task US2_AS7_a_period_based_goal_on_a_custom_habit_that_starts_later_says_starts_and_its_first_week()
    {
        var user = SignInUser();
        var stretch = _customHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Evening stretch", Category.Body));
        var id = await CreateAsync(user, new GoalHabit(stretch.Id, HabitKind.Custom), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, new DateOnly(2026, 11, 25));

        var cut = RenderGoal(id);

        Assert.Equal("Evening stretch, 3 times a week", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Body", "Build", "Period-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · starts 25 Nov", cut.Find(".ht-attempt").TextContent);
        Assert.Equal(
            [
                ("Habit", "Evening stretch · custom"), ("Build or reduce", "Build"), ("Measure", "Yes / no"), ("Target", "3 times a week"),
                ("Schedule", "Flexible · 3 times a week"), ("Evaluated", "Each week"), ("First week", "30 Nov 2026"), ("Starts", "25 Nov 2026"),
            ],
            Rows(cut));
        Assert.Equal(Sentence, cut.Find(".ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
        // Slice 7 (T064): a goal that has not started can be paused from the account's today, so "Pause" is its one action.
        Assert.Equal(["Pause"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        Assert.DoesNotContain(">Record<", cut.Markup, StringComparison.Ordinal);
        // Slice 5 (T055): no current date, so no History card; the Settings card stands alone in ht-grid-2.
        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Single(cut.FindAll(".ht-grid-2 > section.ht-card"));
        Assert.Empty(cut.FindAll(".ht-grid-2 > .ht-stack"));
    }

    [Fact]
    public async Task HP_001_a_goal_with_a_Due_commitment_has_the_primary_action_Record_after_the_secondary_Pause()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);

        var cut = RenderGoal(id);

        // Slice 7 (T064): "Pause" comes before "Record", both in an ht-inline group after ht-spacer.
        var actions = cut.FindAll(".ht-page-head button");
        Assert.Equal(["Pause", "Record"], actions.Select(b => b.TextContent));
        var record = actions[1];
        Assert.Equal(["ht-btn", "ht-btn--primary"], record.ClassList.ToArray());
        Assert.Equal("dialog", record.GetAttribute("aria-haspopup"));
        Assert.Equal(["ht-btn", "ht-btn--secondary"], actions[0].ClassList.ToArray());
        Assert.Equal("dialog", actions[0].GetAttribute("aria-haspopup"));
        Assert.Equal("ht-inline", record.ParentElement!.ClassName);
        Assert.Equal("ht-spacer", record.ParentElement!.PreviousElementSibling!.ClassName);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public async Task HP_001_a_per_week_goal_in_its_first_partial_week_has_no_Record()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        // Tuesday 17 November: the first week starts on Monday 23, so nothing can be recorded yet.
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, Today);

        var cut = RenderGoal(id);

        Assert.Equal("Attempt 1 · started 17 Nov", cut.Find(".ht-attempt").TextContent);
        // Slice 7 (T064): no "Record", but the goal can be paused.
        Assert.Equal(["Pause"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        // T055: a current date but no begun entry, so no History card; the Settings card stands alone in ht-grid-2.
        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(cut.FindAll(".ht-grid-2 > .ht-stack"));
        Assert.Empty(cut.FindAll(".ht-stats"));
        Assert.Empty(cut.FindAll(".ht-bars"));
    }

    [Fact]
    public void Slice_6_US1_AS8_HP_001_the_GoalStats_card_shows_the_consistency_above_the_History_card()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC. At 10:00 on Tuesday 17 November the days to Saturday 14
        // are final: 10, 11 and 13 Met (20 min), 12 Failed (10 min), 14 Missed; Sunday 15 and Monday 16 are Pending and
        // Tuesday 17 is Due.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var (day, minutes) in new[] { (10, 20), (11, 20), (12, 10), (13, 20) })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(minutes));
        }

        var cut = RenderGoal(goal.Id);

        var left = cut.FindAll(".ht-grid-2 > .ht-stack")[0];
        var card = left.Children[0];
        Assert.Equal("This attempt", card.QuerySelector(".ht-eyebrow")!.TextContent);
        Assert.Equal("Consistency and streaks", card.QuerySelector("h2.ht-card__title")!.TextContent);
        Assert.Equal(
            [("Consistency", "60% (3 of 5)"), ("Streak, commitments", "0"), ("Longest, commitments", "2")],
            card.QuerySelectorAll(".ht-stat").Select(s => (s.QuerySelector(".ht-stat__label")!.TextContent, s.QuerySelector(".ht-stat__value")!.TextContent)));
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 2 pending", card.QuerySelector(".ht-strip")!.GetAttribute("aria-label"));
        Assert.Equal("Minutes per day", left.Children[1].QuerySelector(".ht-card__title")!.TextContent);
        Assert.Equal("History", left.Children[2].QuerySelector(".ht-card__title")!.TextContent);
    }

    [Fact]
    public void Slice_6_US4_HP_001_the_TrendChart_card_follows_GoalStats_with_its_bars_and_target_line()
    {
        // As above: 20, 20, 10 and 20 minutes on 10 to 13 November, nothing on 14 to 17. The 8 begun commitments are the
        // bars; the largest of the values and the target is 20, so 20 minutes is 100% and 10 minutes 50%.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var (day, minutes) in new[] { (10, 20), (11, 20), (12, 10), (13, 20) })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(minutes));
        }

        var cut = RenderGoal(goal.Id);

        var card = cut.FindAll(".ht-grid-2 > .ht-stack")[0].Children[1];
        Assert.Equal("section", card.LocalName);
        Assert.Equal("Minutes per day", card.QuerySelector("h2.ht-card__title")!.TextContent);
        Assert.Equal("Last 14 days", card.QuerySelector(".ht-card__sub")!.TextContent);
        var chart = card.QuerySelector(".ht-bars")!;
        Assert.Equal(("img", "Minutes per day, last 14 days; target 20"), (chart.GetAttribute("role"), chart.GetAttribute("aria-label")));
        var bars = chart.QuerySelectorAll("i.ht-bar");
        Assert.Equal(
            ["height:100%", "height:100%", "height:50%", "height:100%", null, null, null, null],
            bars.Select(b => b.GetAttribute("style")));
        Assert.Equal(4, chart.QuerySelectorAll("i.ht-bar--empty").Length);
        Assert.Equal(
            ["Tue 10 Nov: 20 of 20 min", "Wed 11 Nov: 20 of 20 min", "Thu 12 Nov: 10 of 20 min", "Fri 13 Nov: 20 of 20 min",
             "Sat 14 Nov: no record", "Sun 15 Nov: no record", "Mon 16 Nov: no record", "Tue 17 Nov: no record"],
            bars.Select(b => b.GetAttribute("title")));
        var line = chart.QuerySelector(".ht-bars__target")!;
        Assert.Equal(("Target 20 min", "bottom:100%"), (line.TextContent, line.GetAttribute("style")));
        Assert.Equal(
            ["10 Nov", "11 Nov", "12 Nov", "13 Nov", "14 Nov", "15 Nov", "16 Nov", "17 Nov"],
            card.QuerySelectorAll(".ht-bars__labels > span").Select(s => s.TextContent));
    }

    [Fact]
    public async Task Slice_6_HP_001_a_goal_not_started_has_no_GoalStats_and_no_TrendChart_card()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, new DateOnly(2026, 11, 25));

        var cut = RenderGoal(id);

        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(cut.FindAll(".ht-stats"));
        Assert.Empty(cut.FindAll(".ht-bars"));
        Assert.DoesNotContain("Consistency", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Minutes per day", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US1_AS9_Record_opens_the_RecordDialog_and_a_save_records_today_and_closes_it()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);
        var cut = RenderGoal(id);
        Assert.Equal("—", Text(cut.Find("table[aria-label=History] tbody tr").QuerySelectorAll("td")[1]));

        cut.Find(".ht-page-head .ht-btn--primary").Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Build · at least 20 min · every day", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal(["Today, Tue 17 Nov"], dialog.QuerySelectorAll("select option").Select(o => o.TextContent));
        Assert.Equal("Time spent", dialog.QuerySelector(".ht-stepper input")!.GetAttribute("aria-label"));

        cut.Find(".ht-stepper input").Change("12");
        Assert.Equal("12 of 20 min", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        var record = Assert.Single(_records.RecordsOfGoal(id));
        Assert.Equal((Today, 12), (record.Date, record.Value.Amount!.Value));
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        // T055: after a save the history is read again.
        var row = Assert.Single(cut.FindAll("table[aria-label=History] tbody tr"));
        Assert.Equal(["Tue 17 Nov", "12 min", "Due today"], row.QuerySelectorAll("td").Take(3).Select(Text));
    }

    [Fact]
    public async Task US3_a_Missed_rows_Add_record_and_then_Edit_open_the_RecordDialog_for_that_row_and_give_the_focus_back_to_it()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC; at 10:00 on Tuesday 17 November Saturday 14's grace
        // period ended at 00:00 that day, so it is Missed.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        var cut = RenderGoal(goal.Id);
        AngleSharp.Dom.IElement Row() =>
            cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");
        Assert.Equal(["Sat 14 Nov", "—", "Missed"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        var button = Row().QuerySelector("td.ht-actions > button:last-of-type")!;
        // The row's record control stays its last button, so the focus goes back to the same element after each save
        // (specs/006-note-editing: its "Add note" comes before it while the row has no record; the owner's order,
        // 2026-11-20).
        var buttonId = button.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(buttonId));
        Assert.Equal("Add record for Sat 14 Nov", button.GetAttribute("aria-label"));

        await button.ClickAsync(new MouseEventArgs());
        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Sat 14 Nov", Text(dialog.QuerySelector(".ht-field > span:not(.ht-field__label)")!));
        Assert.Empty(dialog.QuerySelectorAll("select"));
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "20 min", "Met"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        var edit = Row().QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(("Edit", "Edit record of Sat 14 Nov"), (edit.TextContent, edit.GetAttribute("aria-label")));
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, focused[1]);

        await edit.ClickAsync(new MouseEventArgs());
        Assert.Equal("20", cut.Find(".ht-stepper input").GetAttribute("value"));
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        Assert.Equal("Add record", Row().QuerySelector("td.ht-actions > button:last-of-type")!.TextContent);
        Assert.Empty(_records.RecordsOfGoal(goal.Id));
        var refocused = JSInterop.VerifyFocusAsyncInvoke(4).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, refocused[3]);
    }

    [Fact]
    public async Task Slice_6_US3_FR_008_a_record_saved_for_a_Missed_day_shows_the_new_consistency_without_a_reload()
    {
        // As in Slice_6_US1_AS8: Reading, 20 min daily, from Tuesday 10 November in UTC; at 10:00 on Tuesday 17 November
        // 10, 11 and 13 are Met, 12 Failed and Saturday 14 Missed ("60% (3 of 5)", streak 0, longest 2). Saving 20 minutes
        // for Saturday 14 makes it Met ("80% (4 of 5)", streak 2); deleting that record makes it Missed again.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var (day, minutes) in new[] { (10, 20), (11, 20), (12, 10), (13, 20) })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(minutes));
        }

        var cut = RenderGoal(goal.Id);
        List<(string Label, string Value)> Stats() =>
            cut.FindAll(".ht-stats .ht-stat").Select(s => (s.QuerySelector(".ht-stat__label")!.TextContent, s.QuerySelector(".ht-stat__value")!.TextContent)).ToList();
        AngleSharp.Dom.IElement RecordControl() =>
            cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov")
                .QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal([("Consistency", "60% (3 of 5)"), ("Streak, commitments", "0"), ("Longest, commitments", "2")], Stats());
        var buttonId = RecordControl().GetAttribute("blazor:elementReference");

        await RecordControl().ClickAsync(new MouseEventArgs());
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal([("Consistency", "80% (4 of 5)"), ("Streak, commitments", "2"), ("Longest, commitments", "2")], Stats());
        Assert.Equal("Last 7 commitments: 4 met, 1 failed, 2 pending", cut.Find(".ht-strip").GetAttribute("aria-label"));
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, focused[1]);

        await RecordControl().ClickAsync(new MouseEventArgs());
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal([("Consistency", "60% (3 of 5)"), ("Streak, commitments", "0"), ("Longest, commitments", "2")], Stats());
    }

    [Fact]
    public async Task US4_a_Missed_rows_Add_note_opens_the_NoteDialog_and_a_save_shows_the_note_in_the_row_and_in_Recent_notes()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC; at 10:00 on Tuesday 17 November Saturday 14's grace
        // period ended at 00:00 that day, so it is Missed.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        var cut = RenderGoal(goal.Id);
        AngleSharp.Dom.IElement Row() =>
            cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");
        // specs/006-note-editing HP-002: the note control is the first button of the actions column, before "Add record".
        AngleSharp.Dom.IElement NoteButton() => Row().QuerySelectorAll("td.ht-actions > button")[0];
        // No note yet: no Recent notes card, and the right column holds the Settings card alone.
        Assert.Equal(["Consistency and streaks", "Minutes per day", "History", "Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Equal(["Add note", "Add record"], Row().QuerySelectorAll("td.ht-actions > button").Select(Text));
        Assert.Equal(("Add note", "Add note for Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("—", Text(Row().QuerySelectorAll("td")[3]));
        // The note control stays the same element when the save turns it into "Edit note", so the focus goes back to it.
        var buttonId = NoteButton().GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(buttonId));

        await NoteButton().ClickAsync(new MouseEventArgs());
        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Add note · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sat 14 Nov · Missed", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        cut.Find("textarea").Input("Away for the weekend.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Away for the weekend.", Text(Row().QuerySelectorAll("td")[3]));
        Assert.Empty(Row().QuerySelectorAll("td")[3].Children);
        Assert.Equal(("Edit note", "Edit note of Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("Missed", Text(Row().QuerySelectorAll("td")[2]));
        var right = cut.FindAll(".ht-grid-2 > .ht-stack")[1];
        Assert.Equal(["Settings", "Recent notes"], right.QuerySelectorAll(".ht-card__title").Select(Text));
        var item = Assert.Single(right.QuerySelectorAll("div.ht-goal"));
        Assert.Equal(["\"Away for the weekend.\"", "Missed"], item.QuerySelectorAll("div.ht-goal__top > span:not(.ht-spacer)").Select(Text));
        Assert.Equal("Sat 14 Nov", Text(item.QuerySelector("div.ht-goal__meta")!));
        // Focus: the TextArea on open, then the row's note control, now "Edit note", on close.
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, focused[1]);

        await NoteButton().ClickAsync(new MouseEventArgs());
        Assert.Equal("Edit note · Reading, 20 min daily", cut.Find("[role=dialog] h2").TextContent);
        Assert.Equal("Away for the weekend.", cut.Find("textarea").GetAttribute("value") ?? cut.Find("textarea").TextContent);
        cut.Find("textarea").Input("   ");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal(("Add note", "Add note for Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("—", Text(Row().QuerySelectorAll("td")[3]));
        Assert.Equal(["Consistency and streaks", "Minutes per day", "History", "Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(_records.NotesOfGoal(goal.Id));
    }

    // ---- specs/006-note-editing US2-AS4, US2-AS5, SC-004: the note of a History row ----

    private static readonly DateOnly Sat14 = new(2026, 11, 14);

    /// <summary>
    /// Reading, 20 min daily, from Tuesday 10 November in UTC, owned by the signed-in user; at 10:00 on Tuesday 17
    /// November Saturday 14's grace period ended at 00:00 that day, so Saturday 14 is final (Missed without a record).
    /// With <paramref name="minutes"/>, Saturday 14 has that record; with <paramref name="note"/>, that note.
    /// </summary>
    private GoalId ReadingWithSat14(int? minutes, string? note)
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        if (minutes is { } amount)
        {
            _records.Seed(goal.Id, Sat14, RecordValue.FromAmount(amount));
        }

        if (note is not null)
        {
            Assert.True(NoteText.TryCreate(note, out var text, out _));
            _records.SeedNote(goal.Id, Sat14, text!);
        }

        return goal.Id;
    }

    private static AngleSharp.Dom.IElement Sat14Row(IRenderedComponent<GoalDetail> cut) =>
        cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");

    private static List<string> Sat14Actions(IRenderedComponent<GoalDetail> cut) =>
        Sat14Row(cut).QuerySelectorAll("td.ht-actions > button").Select(Text).ToList();

    private static string NoteField(IRenderedComponent<GoalDetail> cut) =>
        cut.Find("[role=dialog] textarea").GetAttribute("value") ?? string.Empty;

    [Fact]
    public async Task US2_AS5_Add_record_on_a_Missed_row_with_a_note_opens_the_RecordDialog_with_the_note_in_its_field()
    {
        var cut = RenderGoal(ReadingWithSat14(null, "Away for the weekend."));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));

        await Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[1].ClickAsync(new MouseEventArgs());

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sat 14 Nov", Text(dialog.QuerySelector(".ht-field > span:not(.ht-field__label)")!));
        Assert.Equal("Away for the weekend.", NoteField(cut));
    }

    [Fact]
    public async Task US2_AS4_deleting_the_only_record_of_a_row_with_a_note_shows_its_note_and_Edit_note_again_and_focuses_Add_record()
    {
        var goal = ReadingWithSat14(20, "Away for the weekend.");
        var cut = RenderGoal(goal);
        Assert.Equal(["Sat 14 Nov", "20 min", "Met", "Away for the weekend."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit"], Sat14Actions(cut));
        var recordControl = Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.GetAttribute("blazor:elementReference");

        await Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.ClickAsync(new MouseEventArgs());
        Assert.Equal("Away for the weekend.", NoteField(cut));
        cut.Find("[role=dialog] textarea").Input(string.Empty);
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed", "Away for the weekend."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));
        Assert.Equal("Edit note of Sat 14 Nov", Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[0].GetAttribute("aria-label"));
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Equal("Away for the weekend.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
        // Focus: the dialog's first field on open, then the row's "Add record", the same element as its "Edit", on close.
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        var addRecord = Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(("Add record", recordControl), (addRecord.TextContent, focused[1]));
    }

    [Fact]
    public async Task SC_004_on_a_row_with_a_record_Edit_clearing_the_field_and_Save_record_remove_the_note()
    {
        var goal = ReadingWithSat14(20, "Away for the weekend.");
        var cut = RenderGoal(goal);

        // The three actions: open, edit, save.
        await Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] textarea").Input(string.Empty);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "20 min", "Met", "—"], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit"], Sat14Actions(cut));
        Assert.Single(_records.RecordsOfGoal(goal));
        Assert.Empty(_records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task SC_004_on_a_row_without_a_record_Edit_note_changing_the_text_and_Save_note_replace_the_note()
    {
        var goal = ReadingWithSat14(null, "Away for the weekend.");
        var cut = RenderGoal(goal);

        // The three actions: open, edit, save.
        await Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[0].ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] textarea").Input("Visiting family.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed", "Visiting family."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Equal("Visiting family.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
    }

    // ---- slice 7: excusing from "Excuse instead" and "Remove excusal" (HP-001, HP-003, FR-024; T052) ----

    private static readonly DateOnly Sun15 = new(2026, 11, 15);

    /// <summary>
    /// As in Slice_6_US1_AS8: Reading, 20 min daily, from Tuesday 10 November in UTC, read at 10:00 on Tuesday 17
    /// November: 10, 11 and 13 are Met, 12 Failed, Saturday 14 Missed, Sunday 15 Pending (grace end 00:00 on Wednesday 18,
    /// 14 h left), Monday 16 Pending and Tuesday 17 Due ("60% (3 of 5)"). <paramref name="arrange"/> may seed more.
    /// </summary>
    private GoalId ReadingFromTue10(Action<GoalId>? arrange = null) => OwnedReadingFromTue10(arrange).Goal;

    /// <summary><see cref="ReadingFromTue10"/>, with the signed-in owner, for a test that changes the store as another session would.</summary>
    private (GoalId Goal, AccountId Owner) OwnedReadingFromTue10(Action<GoalId>? arrange = null)
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var (day, minutes) in new[] { (10, 20), (11, 20), (12, 10), (13, 20) })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(minutes));
        }

        arrange?.Invoke(goal.Id);
        return (goal.Id, owner);
    }

    private static AngleSharp.Dom.IElement HistoryRow(IRenderedComponent<GoalDetail> cut, string date) =>
        cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == date);

    private static List<string> HistoryActions(IRenderedComponent<GoalDetail> cut, string date) =>
        HistoryRow(cut, date).QuerySelectorAll("td.ht-actions > button").Select(b => b.TextContent).ToList();

    private static string StripLabel(IRenderedComponent<GoalDetail> cut) => cut.Find(".ht-strip").GetAttribute("aria-label")!;

    private List<string> FocusedIds(int count) =>
        JSInterop.VerifyFocusAsyncInvoke(count).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public async Task Slice_7_HP_003_FR_024_Excuse_instead_opens_the_Excuse_dialog_and_excusing_shows_Excused_and_the_new_strip_without_a_reload()
    {
        var goal = ReadingFromTue10();
        var cut = RenderGoal(goal);
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 2 pending", StripLabel(cut));
        var record = cut.Find(".ht-page-head .ht-btn--primary");
        var recordId = record.GetAttribute("blazor:elementReference");

        await record.ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] select").Change("2026-11-15");
        var excuseInstead = cut.FindAll(".ht-dialog__foot .ht-btn")[^1];
        Assert.Equal("Excuse instead", excuseInstead.TextContent);
        await excuseInstead.ClickAsync(new MouseEventArgs());

        // The Excuse dialog takes the RecordDialog's place.
        var dialog = Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.Equal("Excuse · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sun 15 Nov · Pending, 14 h left", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        cut.Find("[role=dialog] input.ht-input").Input("Ill");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Ill", _records.ExemptionsOfGoal(goal).ExcusalOf(Sun15)!.Reason!.Value);
        Assert.Equal(["Sun 15 Nov", "—"], HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td").Take(2).Select(Text));
        Assert.Equal(["Excused", "Ill"], HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2].QuerySelectorAll(".ht-inline > *").Select(Text));
        Assert.Equal(["Add note", "Remove excusal"], HistoryActions(cut, "Sun 15 Nov"));
        // An excused Pending commitment was never counted, so the consistency stays; the strip now shows it Excused.
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 1 excused, 1 pending", StripLabel(cut));
        Assert.Equal("60% (3 of 5)", cut.Find(".ht-stats .ht-stat .ht-stat__value").TextContent);
        // The focus goes back to the head's "Record", which opened the RecordDialog and is still there.
        Assert.Equal(recordId, FocusedIds(4)[^1]);
    }

    [Fact]
    public async Task Slice_7_HP_007_cancelling_the_Excuse_dialog_from_a_rows_Add_record_gives_the_focus_back_to_that_Add_record()
    {
        var goal = ReadingFromTue10();
        var cut = RenderGoal(goal);
        var addRecord = HistoryRow(cut, "Mon 16 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        var addRecordId = addRecord.GetAttribute("blazor:elementReference");

        await addRecord.ClickAsync(new MouseEventArgs());
        await cut.FindAll(".ht-dialog__foot .ht-btn")[^1].ClickAsync(new MouseEventArgs());
        Assert.Equal("Mon 16 Nov · Pending, 38 h left", cut.Find("[role=dialog] .ht-dialog__sub").TextContent);
        await cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
        Assert.Equal(addRecordId, FocusedIds(4)[^1]);
    }

    [Fact]
    public async Task Slice_7_HP_001_FR_004_Remove_excusal_makes_the_row_Pending_with_Add_record_and_focuses_it()
    {
        var goal = ReadingFromTue10(g => _records.SeedExcusal(g, Sun15));
        var cut = RenderGoal(goal);
        Assert.Equal(["Sun 15 Nov", "—", "Excused"], HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td").Take(3).Select(Text));
        var remove = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal("Remove excusal of Sun 15 Nov", remove.GetAttribute("aria-label"));

        await remove.ClickAsync(new MouseEventArgs());

        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
        Assert.Equal(["Sun 15 Nov", "—", "Pending"], HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td").Take(3).Select(Text));
        Assert.Equal(["Add note", "Add record"], HistoryActions(cut, "Sun 15 Nov"));
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 2 pending", StripLabel(cut));
        Assert.Empty(cut.FindAll("[role=alert]"));
        var addRecord = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(addRecord.GetAttribute("blazor:elementReference"), FocusedIds(1)[^1]);
    }

    [Fact]
    public async Task Slice_7_HP_001_a_Remove_excusal_too_late_shows_its_refusal_under_the_History_head_and_keeps_the_row_Excused()
    {
        var goal = ReadingFromTue10(g => _records.SeedExcusal(g, Sun15));
        var cut = RenderGoal(goal);
        var remove = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button:last-of-type")!;
        var noteId = HistoryRow(cut, "Sun 15 Nov").QuerySelector("td.ht-actions > button")!.GetAttribute("blazor:elementReference");

        // Sunday 15's grace period ends at 00:00 on Wednesday 18, while the page is open.
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 18, 0, 0, 0, TimeSpan.Zero));
        await remove.ClickAsync(new MouseEventArgs());

        var alert = cut.Find("table[aria-label=History]").Closest("section")!.QuerySelector("p.ht-field__error[role=alert]")!;
        Assert.Equal("This excusal can no longer be removed.", alert.TextContent);
        Assert.NotNull(_records.ExemptionsOfGoal(goal).ExcusalOf(Sun15));
        Assert.Equal("Excused", Text(HistoryRow(cut, "Sun 15 Nov").QuerySelectorAll("td")[2]));
        Assert.Equal(["Add note"], HistoryActions(cut, "Sun 15 Nov"));
        Assert.Equal(noteId, FocusedIds(1)[^1]);
    }

    [Fact]
    public async Task US7_AS8_a_goal_on_an_Unpublished_habit_shows_the_Notice_under_the_head()
    {
        var user = SignInUser();
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        var id = await CreateAsync(user, new GoalHabit(coffee.Id, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        }, Today);
        Assert.True(await _catalog.TrySetStateAsync(coffee.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));

        var cut = RenderGoal(id);

        Assert.Equal("Coffee, at most 2 cups a day", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Health", "Reduce", "Period-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · started 17 Nov", cut.Find(".ht-attempt").TextContent);
        var notice = cut.Find(".ht-notice");
        Assert.Contains("ht-notice--neutral", notice.ClassList);
        Assert.Equal(NoticeText, Text(notice));
        Assert.Equal("ht-page-head", notice.PreviousElementSibling!.ClassName?.Split(' ')[0]);
        Assert.Equal(
            [
                ("Habit", "Coffee · catalog"), ("Build or reduce", "Reduce"), ("Measure", "Count"), ("Target", "at most 2 cups a day"),
                ("Schedule", "Every day"), ("Evaluated", "Each day"), ("Started", "17 Nov 2026"),
            ],
            Rows(cut));
    }

    [Fact]
    public async Task A_goal_on_a_Published_habit_has_no_Notice()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);

        var cut = RenderGoal(id);

        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.DoesNotContain(NoticeText, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FR_020_an_unknown_goal_gets_the_Not_found_page()
    {
        SignInUser();

        var cut = RenderGoal(new GoalId(Guid.NewGuid()));

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    [Fact]
    public async Task FR_020_another_accounts_goal_gets_the_Not_found_page_and_shows_nothing_of_it()
    {
        var other = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(other, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);
        SignInUser();

        var cut = RenderGoal(id);

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
        Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_administrator_gets_the_Not_found_page()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderGoal(id);

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
        Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
    }

    // ---- slice 7: pauses on the goal page (T064; HP-001, HP-002, HP-007, FR-009 to FR-012, US2-AS14) ----
    // ReadingFromTue10 at 10:00 on Tuesday 17 November: Saturday 14 is Missed, Sunday 15 and Monday 16 are Pending and
    // Tuesday 17 is Due, so "Record" stays in the head of a paused goal; the pause date is Tuesday 17.

    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);

    private static string NoticeOf(IRenderedComponent<GoalDetail> cut) => Text(cut.Find(".ht-notice > span"));

    private static AngleSharp.Dom.IElement PausesCard(IRenderedComponent<GoalDetail> cut) =>
        cut.FindAll("section.ht-card").Single(card => card.QuerySelector(".ht-card__title")?.TextContent == "Pauses");

    private static List<string> PauseRanges(IRenderedComponent<GoalDetail> cut) =>
        PausesCard(cut).QuerySelectorAll(".ht-goal .ht-goal__name").Select(Text).ToList();

    private static ExemptionReason? Reason(string text)
    {
        Assert.True(ExemptionReason.TryCreate(text, out var reason, out _));
        return reason;
    }

    [Fact]
    public async Task Slice_7_US2_AS14_Pause_opens_the_PauseDialog_and_saving_it_shows_the_Notice_and_the_Pauses_card()
    {
        var goal = ReadingFromTue10();
        var cut = RenderGoal(goal);
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.DoesNotContain(cut.FindAll(".ht-card__title"), title => title.TextContent == "Pauses");
        var pause = cut.Find(".ht-page-head .ht-btn--secondary");
        Assert.Equal("Pause", pause.TextContent);

        await pause.ClickAsync(new MouseEventArgs());

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Pause · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        var from = cut.Find("[role=dialog] input[type=date]");
        Assert.Equal("2026-11-17", from.GetAttribute("value"));
        Assert.Equal("2026-11-17", from.GetAttribute("min"));
        cut.FindAll("[role=dialog] input[type=date]")[1].Change("2026-11-22");
        cut.Find("[role=dialog] input[type=text]").Input("Family visit");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        var stored = Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal((Today, Sun22, "Family visit"), (stored.FirstDay, stored.LastDay, stored.Reason!.Value));
        Assert.Equal("Paused until Sun 22 Nov", NoticeOf(cut));
        var resume = cut.Find(".ht-notice button");
        Assert.Equal("Resume now", resume.TextContent);
        Assert.Equal(["ht-btn", "ht-btn--primary", "ht-btn--sm"], resume.ClassList.ToArray());
        Assert.Equal("ht-page-head", cut.Find(".ht-notice").PreviousElementSibling!.ClassName?.Split(' ')[0]);
        // "Pause" leaves the head while the pause is current; "Record" stays for the Pending days before it.
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        var right = cut.FindAll(".ht-grid-2 > .ht-stack")[1];
        Assert.Equal(["Settings", "Pauses"], right.QuerySelectorAll(".ht-card__title").Select(Text));
        Assert.Equal(["Tue 17 Nov – Sun 22 Nov"], PauseRanges(cut));
        Assert.Equal("Family visit", Text(PausesCard(cut).QuerySelector(".ht-goal__meta.ht-muted")!));
        Assert.Equal("Resume now, Tue 17 Nov – Sun 22 Nov", PausesCard(cut).QuerySelector("button")!.GetAttribute("aria-label"));
        Assert.Equal(["Tue 17 Nov", "—", "Paused"], HistoryRow(cut, "Tue 17 Nov").QuerySelectorAll("td").Take(3).Select(Text));
        Assert.Equal(["Add note"], HistoryActions(cut, "Tue 17 Nov"));
        // The re-read removed "Pause", the opener, so the focus goes to the Notice's "Resume now" (HP-007).
        Assert.Equal(cut.Find(".ht-notice button").GetAttribute("blazor:elementReference"), FocusedIds(2)[^1]);
    }

    [Fact]
    public async Task Slice_7_HP_002_a_refusal_of_the_PauseDialog_shows_under_its_field_and_keeps_the_dialog_open()
    {
        var goal = ReadingFromTue10(g => _records.SeedPause(g, new DateOnly(2026, 11, 20), Sun22));
        var cut = RenderGoal(goal);
        await cut.Find(".ht-page-head .ht-btn--secondary").ClickAsync(new MouseEventArgs());

        cut.FindAll("[role=dialog] input[type=date]")[1].Change("2026-11-21");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Single(cut.FindAll("[role=dialog]"));
        var until = cut.FindAll("[role=dialog] input[type=date]")[1];
        Assert.Equal("This goal already has a pause on these dates.", cut.Find("#" + until.GetAttribute("aria-describedby")).TextContent);
        Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task Slice_7_FR_010_the_Notices_Resume_now_resumes_the_goal_and_gives_the_focus_to_Pause()
    {
        // A pause from Tuesday 17, today, to Sunday 22: resuming on its first day removes it.
        var goal = ReadingFromTue10(g => _records.SeedPause(g, Today, Sun22, Reason("Family visit")));
        var cut = RenderGoal(goal);
        Assert.Equal("Paused until Sun 22 Nov", NoticeOf(cut));
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));

        await cut.Find(".ht-notice button").ClickAsync(new MouseEventArgs());

        Assert.Empty(_records.ExemptionsOfGoal(goal).Pauses);
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Empty(cut.FindAll("[role=alert]"));
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        Assert.DoesNotContain(cut.FindAll(".ht-card__title"), title => title.TextContent == "Pauses");
        Assert.Equal(["Tue 17 Nov", "—", "Due today"], HistoryRow(cut, "Tue 17 Nov").QuerySelectorAll("td").Take(3).Select(Text));
        Assert.Equal(cut.Find(".ht-page-head .ht-btn--secondary").GetAttribute("blazor:elementReference"), FocusedIds(1)[^1]);
    }

    [Fact]
    public async Task Slice_7_FR_010_a_refusal_of_the_Notices_Resume_now_shows_in_the_Notices_place()
    {
        var (goal, owner) = OwnedReadingFromTue10(g => _records.SeedPause(g, Mon16, Sun22));
        var cut = RenderGoal(goal);
        Assert.Equal("Paused until Sun 22 Nov", NoticeOf(cut));

        // Another session cancels the pause after the page was read.
        var pause = Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
        await _records.DeletePauseAsync(owner, goal, pause.Id, CancellationToken.None);
        await cut.Find(".ht-notice button").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll(".ht-notice"));
        var alert = cut.Find("p.ht-field__error[role=alert]");
        Assert.Equal("This goal is not paused.", alert.TextContent);
        Assert.Equal("ht-page-head", alert.PreviousElementSibling!.ClassName?.Split(' ')[0]);
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
    }

    [Fact]
    public async Task Slice_7_FR_011_FR_012_the_Pauses_card_lists_the_pauses_and_Cancel_pause_cancels_an_upcoming_one()
    {
        var goal = ReadingFromTue10(g =>
        {
            _records.SeedPause(g, new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 11), Reason("Ill"));
            _records.SeedPause(g, new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 27));
        });
        var cut = RenderGoal(goal);
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        Assert.Equal(["Settings", "Pauses"], cut.FindAll(".ht-grid-2 > .ht-stack")[1].QuerySelectorAll(".ht-card__title").Select(Text));
        Assert.Equal(["Mon 23 Nov – Fri 27 Nov", "Tue 10 Nov – Wed 11 Nov"], PauseRanges(cut));
        var cancel = Assert.Single(PausesCard(cut).QuerySelectorAll("button"));
        Assert.Equal("Cancel pause", cancel.TextContent);
        Assert.Equal("Cancel pause, Mon 23 Nov – Fri 27 Nov", cancel.GetAttribute("aria-label"));
        var pauseId = cut.Find(".ht-page-head .ht-btn--secondary").GetAttribute("blazor:elementReference");

        await cancel.ClickAsync(new MouseEventArgs());

        Assert.Equal(new DateOnly(2026, 11, 10), Assert.Single(_records.ExemptionsOfGoal(goal).Pauses).FirstDay);
        Assert.Equal(["Tue 10 Nov – Wed 11 Nov"], PauseRanges(cut));
        Assert.Empty(PausesCard(cut).QuerySelectorAll("button"));
        Assert.Equal(pauseId, FocusedIds(1)[^1]);
    }

    [Fact]
    public async Task Slice_7_FR_010_the_Pauses_cards_Resume_now_ends_the_current_pause_on_the_day_before()
    {
        var goal = ReadingFromTue10(g => _records.SeedPause(g, Mon16, Sun22));
        var cut = RenderGoal(goal);
        var resume = Assert.Single(PausesCard(cut).QuerySelectorAll("button"));
        Assert.Equal("Resume now, Mon 16 Nov – Sun 22 Nov", resume.GetAttribute("aria-label"));

        await resume.ClickAsync(new MouseEventArgs());

        var stored = Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal((Mon16, Mon16), (stored.FirstDay, stored.LastDay));
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Equal(["Mon 16 Nov – Mon 16 Nov"], PauseRanges(cut));
        Assert.Empty(PausesCard(cut).QuerySelectorAll("button"));
        Assert.Equal(["Pause", "Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        Assert.Equal("Paused", Text(HistoryRow(cut, "Mon 16 Nov").QuerySelectorAll("td")[2]));
        Assert.Equal("Due today", Text(HistoryRow(cut, "Tue 17 Nov").QuerySelectorAll("td")[2]));
    }

    [Fact]
    public async Task Slice_7_FR_011_a_refusal_of_Cancel_pause_shows_under_the_Pauses_card_title()
    {
        var (goal, owner) = OwnedReadingFromTue10(g =>
        {
            _records.SeedPause(g, new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 11));
            _records.SeedPause(g, new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 27));
        });
        var cut = RenderGoal(goal);

        // Another session cancels the upcoming pause after the page was read.
        var upcoming = _records.ExemptionsOfGoal(goal).Pauses.Single(p => p.FirstDay == new DateOnly(2026, 11, 23));
        await _records.DeletePauseAsync(owner, goal, upcoming.Id, CancellationToken.None);
        await PausesCard(cut).QuerySelector("button")!.ClickAsync(new MouseEventArgs());

        var alert = PausesCard(cut).QuerySelector("p.ht-field__error[role=alert]")!;
        Assert.Equal("This pause does not exist.", alert.TextContent);
        Assert.Equal("ht-card__title", alert.PreviousElementSibling!.ClassName);
        Assert.Equal(["Tue 10 Nov – Wed 11 Nov"], PauseRanges(cut));
    }

    [Fact]
    public async Task Slice_7_a_goal_that_has_not_started_can_be_paused_and_shows_its_Pauses_card_under_Settings()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, new DateOnly(2026, 11, 25));
        var cut = RenderGoal(id);
        Assert.Equal(["Pause"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));

        await cut.Find(".ht-page-head button").ClickAsync(new MouseEventArgs());
        Assert.Equal("2026-11-17", cut.Find("[role=dialog] input[type=date]").GetAttribute("min"));
        cut.FindAll("[role=dialog] input[type=date]")[1].Change("2026-11-22");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal("Paused until Sun 22 Nov", NoticeOf(cut));
        Assert.Empty(cut.FindAll(".ht-page-head button"));
        var column = Assert.Single(cut.FindAll(".ht-grid-2 > .ht-stack"));
        Assert.Equal(["Settings", "Pauses"], column.QuerySelectorAll(".ht-card__title").Select(Text));
    }

    // ---- slice 7: reports leave excused and paused results out (T072; HP-001, HP-006, FR-013 to FR-016, FR-024) ----

    private static AngleSharp.Dom.IElement StatsCard(IRenderedComponent<GoalDetail> cut) => cut.FindAll(".ht-grid-2 > .ht-stack")[0].Children[0];

    private static AngleSharp.Dom.IElement TrendCard(IRenderedComponent<GoalDetail> cut) => cut.FindAll(".ht-grid-2 > .ht-stack")[0].Children[1];

    [Fact]
    public void Slice_7_US3_AS8_the_goal_page_shows_Excused_and_Paused_cells_and_marked_bars()
    {
        // Spec US3's goal on Reading: 10 minutes a day from Monday 2 November in UTC, 10 minutes on each date to 16
        // November except 9, 11 and 13 to 15; 9 November excused and 13 to 15 November paused (seeded, as they were made
        // while current). Read at 12:00 on Thursday 19 November: 17 ended results, 19 November Due.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 2);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily with { Amount = 10 }, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var day in new[] { 2, 3, 4, 5, 6, 7, 8, 10, 12, 16 })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(10));
        }

        _records.SeedExcusal(goal.Id, new DateOnly(2026, 11, 9));
        _records.SeedPause(goal.Id, new DateOnly(2026, 11, 13), Sun15);
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero));

        var cut = RenderGoal(goal.Id);

        var stats = StatsCard(cut);
        Assert.Equal(["90% (10 of 11)", "2", "8"], stats.QuerySelectorAll(".ht-stat__value").Select(v => v.TextContent));
        Assert.Equal("Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending", stats.QuerySelector(".ht-strip")!.GetAttribute("aria-label"));
        Assert.Single(stats.QuerySelectorAll(".ht-strip > i.ht-cell--excused"));
        Assert.Equal(3, stats.QuerySelectorAll(".ht-strip > i.ht-cell--paused").Length);
        Assert.Equal(["Met", "Failed", "Missed", "Excused", "Paused", "Pending"], stats.QuerySelectorAll(".ht-legend > span").Select(Text));
        var chart = TrendCard(cut).QuerySelector(".ht-bars")!;
        Assert.Equal("Minutes per day, last 14 days; target 10; 1 excused, 3 paused", chart.GetAttribute("aria-label"));
        Assert.Equal(["Mon 9 Nov: Excused"], chart.QuerySelectorAll("i.ht-bar--excused").Select(b => b.GetAttribute("title")));
        Assert.Equal(
            ["Fri 13 Nov: Paused", "Sat 14 Nov: Paused", "Sun 15 Nov: Paused"],
            chart.QuerySelectorAll("i.ht-bar--paused").Select(b => b.GetAttribute("title")));
        Assert.Equal(4, chart.QuerySelectorAll("i.ht-bar--empty").Length);
    }

    [Fact]
    public async Task Slice_7_FR_024_after_pausing_in_the_PauseDialog_the_trend_and_the_GoalStats_card_are_read_again_without_a_reload()
    {
        // ReadingFromTue10, paused from Tuesday 17 November (today, Due) until Sunday 22. Tuesday 17 has begun, so its trend
        // bar is marked Paused; it has not ended, so the strip and the figures stay as they were: a pause made at the
        // pause date changes no ended result (FR-006, FR-015). The cards equal those of a fresh render, a reload.
        var goal = ReadingFromTue10();
        var cut = RenderGoal(goal);
        Assert.Equal("Minutes per day, last 14 days; target 20", TrendCard(cut).QuerySelector(".ht-bars")!.GetAttribute("aria-label"));
        Assert.Empty(TrendCard(cut).QuerySelectorAll("i.ht-bar--paused"));

        await cut.Find(".ht-page-head .ht-btn--secondary").ClickAsync(new MouseEventArgs());
        cut.FindAll("[role=dialog] input[type=date]")[1].Change("2026-11-22");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        var chart = TrendCard(cut).QuerySelector(".ht-bars")!;
        Assert.Equal("Minutes per day, last 14 days; target 20; 1 paused", chart.GetAttribute("aria-label"));
        var bars = chart.QuerySelectorAll("i.ht-bar");
        Assert.Equal(8, bars.Length);
        Assert.Equal(["ht-bar", "ht-bar--paused"], bars[^1].ClassList.ToArray());
        Assert.Equal("Tue 17 Nov: Paused", bars[^1].GetAttribute("title"));
        Assert.Equal(3, chart.QuerySelectorAll("i.ht-bar--empty").Length);
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 2 pending", StripLabel(cut));
        Assert.Equal("60% (3 of 5)", cut.Find(".ht-stats .ht-stat .ht-stat__value").TextContent);
        var reloaded = RenderGoal(goal);
        Assert.Equal(Text(StatsCard(reloaded)), Text(StatsCard(cut)));
        Assert.Equal(StatsCard(reloaded).QuerySelector(".ht-strip")!.GetAttribute("aria-label"), StatsCard(cut).QuerySelector(".ht-strip")!.GetAttribute("aria-label"));
        Assert.Equal(TrendCard(reloaded).InnerHtml, TrendCard(cut).InnerHtml);
    }
}
