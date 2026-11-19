using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
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
        Assert.Equal("Settings", cut.Find(".ht-grid-2 .ht-card .ht-card__title").TextContent);
        Assert.Equal(
            [
                ("Habit", "Yoga · catalog"), ("Build or reduce", "Build"), ("Measure", "Yes / no"), ("Target", "3 days a week"),
                ("Schedule", "Mon · Wed · Fri"), ("Evaluated", "Each scheduled day"), ("Started", "14 Jul 2026"),
            ],
            Rows(cut));
        Assert.Equal(Sentence, cut.Find(".ht-card .ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
        // Tuesday 17 November is not scheduled, but Monday 16 is Pending: "Record" is the page head's one action (HP-001).
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        foreach (var absent in new[] { "Edit settings", "Pause", "End goal", "Consistency", "streak", "Earlier attempts", "Excuse" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
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
        Assert.Empty(cut.FindAll(".ht-page-head button"));
        Assert.DoesNotContain(">Record<", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_001_a_goal_with_a_Due_commitment_has_the_primary_action_Record_and_no_other_action()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);

        var cut = RenderGoal(id);

        var actions = cut.FindAll(".ht-page-head button");
        var record = Assert.Single(actions);
        Assert.Equal("Record", record.TextContent);
        Assert.Equal(["ht-btn", "ht-btn--primary"], record.ClassList.ToArray());
        Assert.Equal("dialog", record.GetAttribute("aria-haspopup"));
        Assert.Equal("ht-spacer", record.PreviousElementSibling!.ClassName);
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
        Assert.Empty(cut.FindAll(".ht-page-head button"));
    }

    [Fact]
    public async Task US1_AS9_Record_opens_the_RecordDialog_and_a_save_records_today_and_closes_it()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);
        var cut = RenderGoal(id);

        cut.Find(".ht-page-head button").Click();

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
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
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
}
