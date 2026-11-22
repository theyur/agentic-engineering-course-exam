using Bunit;
using Bunit.TestDoubles;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 1 US6-AS10, HP-013, HP-015, amended by slice 2 HP-013 (US5-AS3) and slice 4 HP-010 (US8-AS5, US11): the signed-in
/// Home shows the greeting as heading-1 and then today's date, in place of slice 1's "Today"; a registered user with no
/// goals also gets the Goals page's EmptyState, an administrator nothing else; slice 6 HP-003 gives a registered user with
/// goals the dashboard (tiles, Today card, "Active goals" and "Recent notes") under the page head. The
/// clock is 10:00 UTC on Tuesday 17 November 2026, and the fake accounts have the UTC default unless a test stores a zone.
/// </summary>
public sealed class HomeSignedInTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly CatalogHabit _reading;

    public HomeSignedInTests()
    {
        _authorization = AddAuthorization();
        var store = this.AddCatalog();
        _reading = store.Add(Catalog("Hs Reading"));
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private async Task<GoalId> CreateGoalAsync(Guid accountId)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            ActingUser.ForAccount(new AccountId(accountId), Role.RegisteredUser),
            new GoalInput
            {
                Habit = new GoalHabit(_reading.Id, HabitKind.Catalog),
                Target = new GoalTargetFields
                {
                    BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
                    ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
                },
                StartDate = new DateOnly(2026, 11, 17),
            },
            CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private FakeAccountStore Accounts => (FakeAccountStore)Services.GetRequiredService<IAccountStore>();

    /// <summary>Signs in, and stores the account with a name and a time zone when given.</summary>
    private async Task SignInStoredAsync(string role, string? name = null, string? timeZone = null)
    {
        var accountId = new AccountId(_authorization.SignIn(role));
        Assert.NotNull(Accounts.FindById(accountId));
        if (timeZone is not null)
        {
            Accounts.SetStoredTimeZone(accountId, timeZone);
        }

        if (name is not null)
        {
            Assert.True(AccountName.TryCreate(name, out var accountName, out _));
            Assert.True(await Accounts.TrySetNameAsync(accountId, accountName, CancellationToken.None));
        }
    }

    /// <summary>The page head's children as (tag, class, text): the heading-1 greeting, then the muted date.</summary>
    private static (string Tag, string Class, string Text)[] PageHead(IRenderedComponent<Home> cut) =>
        cut.Find(".ht-container > .ht-page-head").Children
            .Select(c => (c.TagName, c.ClassName ?? string.Empty, c.TextContent))
            .ToArray();

    private static readonly (string, string, string)[] MorningHead =
        [("H1", string.Empty, "Good morning"), ("SPAN", "ht-muted", "Tuesday, 17 November")];

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void HP_010_a_signed_in_user_sees_the_greeting_as_heading_1_then_the_date_in_place_of_today(string role)
    {
        _authorization.SignIn(role);

        var cut = Render<Home>();

        Assert.Equal("Good morning", Assert.Single(cut.FindAll("h1")).TextContent);
        Assert.Equal(MorningHead, PageHead(cut));
        Assert.DoesNotContain(">Today<", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".ht-hero, .ht-habit"));
        Assert.DoesNotContain("Popular in the catalog", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public async Task US11_the_greeting_has_the_accounts_own_name(string role)
    {
        await SignInStoredAsync(role, name: "Yuri");

        var cut = Render<Home>();

        Assert.Equal(
            [("H1", string.Empty, "Good morning, Yuri"), ("SPAN", "ht-muted", "Tuesday, 17 November")],
            PageHead(cut));
    }

    [Fact]
    public async Task US11_AS5_the_greeting_and_the_date_are_those_of_the_accounts_time_zone_not_of_UTC()
    {
        // 23:30 UTC on Tuesday 17 November is 01:30 on Wednesday 18 November in Kyiv (UTC+2), still Tuesday in UTC.
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 23, 30, 0, TimeSpan.Zero));
        await SignInStoredAsync("RegisteredUser", timeZone: "Europe/Kyiv");

        var cut = Render<Home>();

        Assert.Equal(
            [("H1", string.Empty, "Good evening"), ("SPAN", "ht-muted", "Wednesday, 18 November")],
            PageHead(cut));
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void US8_AS5_a_registered_user_with_no_goals_gets_the_goals_empty_state_under_the_page_head()
    {
        _authorization.SignIn("RegisteredUser");

        var cut = Render<Home>();

        var container = cut.Find(".ht-container");
        Assert.Equal(["ht-page-head", "ht-card ht-empty"], container.Children.Select(c => c.ClassName));
        Assert.Equal(MorningHead, PageHead(cut));
        var empty = cut.Find(".ht-empty");
        // The list-checks icon (design/icons/list-checks.svg).
        var icon = Assert.Single(empty.QuerySelectorAll(".ht-empty__icon svg.ht-icon"));
        Assert.Equal(
            ["m3 17 2 2 4-4", "m3 7 2 2 4-4", "M13 6h8", "M13 12h8", "M13 18h8"],
            icon.QuerySelectorAll("path").Select(p => p.GetAttribute("d")));
        Assert.Equal("No goals yet", empty.QuerySelector("h3")!.TextContent);
        Assert.Equal(
            "Pick a habit from the catalog or create your own, then set a target and a schedule for it.",
            empty.QuerySelector("p")!.TextContent);
        var buttons = empty.QuerySelectorAll("a, button");
        Assert.Equal(["Browse the catalog", "Create custom habit"], buttons.Select(Text));
        Assert.Equal(["/catalog", "/custom-habits/new"], buttons.Select(b => b.GetAttribute("href")));
        Assert.Contains("ht-btn--primary", buttons[0].ClassList);
        Assert.Contains("ht-btn--secondary", buttons[1].ClassList);
        Assert.DoesNotContain("Goals come in a later version.", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(@"\d", empty.TextContent);
    }

    [Fact]
    public async Task Slice_6_HP_003_a_registered_user_with_goals_gets_the_dashboard_under_the_page_head()
    {
        // Slice 4 had only the page head here ("page head only until slice 6"). A yes/no goal once a day from today: Due.
        var accountId = _authorization.SignIn("RegisteredUser");
        var goal = await CreateGoalAsync(accountId);

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head", "ht-home"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(MorningHead, PageHead(cut));
        var home = cut.Find(".ht-home");
        Assert.Equal(["ht-home__panel", "ht-stack"], home.Children.Select(c => c.ClassName));
        Assert.Equal(
            ["Due now", "Pending · still can record", "Missed this week", "Active goals"],
            cut.FindAll(".ht-home__panel .ht-tile__label").Select(l => l.TextContent));
        Assert.Equal(["1", "0", "0", "1"], cut.FindAll(".ht-home__panel .ht-tile__value").Select(v => v.TextContent));
        var row = Assert.Single(cut.FindAll(".ht-home__panel .ht-row"));
        Assert.Equal("Due today", row.QuerySelector(".ht-status")!.TextContent);
        var right = home.Children[1];
        Assert.Equal(["Active goals"], right.QuerySelectorAll(".ht-card__title").Select(t => t.TextContent));
        var link = right.QuerySelector("a.ht-goal__name")!;
        Assert.Equal(("Hs Reading, once a day", $"/goals/{goal.Value}"), (link.TextContent, link.GetAttribute("href")));
        Assert.Equal(("N/A", "No finalized days yet"), (right.QuerySelector(".ht-goal__pct")!.TextContent, right.QuerySelector(".ht-goal__meta")!.TextContent));
        Assert.Equal("All goals →", right.QuerySelector("a.ht-card__link")!.TextContent);
        Assert.DoesNotContain("Recent notes", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".ht-hero, .ht-empty"));
    }

    [Fact]
    public async Task Slice_6_HP_003_the_right_column_has_at_most_5_goals_and_the_Recent_notes_card_with_a_note()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        var goals = new List<GoalId>();
        for (var i = 0; i < 6; i++)
        {
            goals.Add(await CreateGoalAsync(accountId));
        }

        await using (var scope = Services.CreateAsyncScope())
        {
            var recorded = await scope.ServiceProvider.GetRequiredService<RecordingService>().RecordAsync(
                ActingUser.ForAccount(new AccountId(accountId), Role.RegisteredUser), goals[0], new DateOnly(2026, 11, 17),
                new RecordInput { Answer = YesNoAnswer.Yes, Note = "Easy start." }, CancellationToken.None);
            Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        }

        var cut = Render<Home>();

        var right = cut.Find(".ht-home > .ht-stack");
        Assert.Equal(["Active goals", "Recent notes"], right.QuerySelectorAll(".ht-card__title").Select(t => t.TextContent));
        Assert.Equal(5, right.QuerySelectorAll(".ht-card:first-child a.ht-goal__name").Length);
        var note = right.QuerySelector(".ht-card:last-child .ht-goal")!;
        Assert.Equal("\"Easy start.\"", note.FirstChild!.TextContent);
        Assert.Equal("Hs Reading, once a day · Tue 17 Nov", note.QuerySelector(".ht-goal__meta")!.TextContent);
    }

    /// <summary>Pauses <paramref name="goal"/> through the PauseService at 10:00 on Tuesday 17, from that day until Sunday 22 November.</summary>
    private async Task PauseAsync(Guid accountId, GoalId goal)
    {
        await using var scope = Services.CreateAsyncScope();
        var paused = await scope.ServiceProvider.GetRequiredService<PauseService>().PauseAsync(
            ActingUser.ForAccount(new AccountId(accountId), Role.RegisteredUser), goal, new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 22),
            null, CancellationToken.None);
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
    }

    [Fact]
    public async Task Slice_7_HP_005_a_paused_goal_is_missing_from_the_Active_goals_card_and_the_tile_counts_1()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        var active = await CreateGoalAsync(accountId);
        var paused = await CreateGoalAsync(accountId);
        await PauseAsync(accountId, paused);

        var cut = Render<Home>();

        Assert.Equal(["1", "0", "0", "1"], cut.FindAll(".ht-home__panel .ht-tile__value").Select(v => v.TextContent));
        Assert.Equal("Active goals", cut.FindAll(".ht-home__panel .ht-tile__label")[3].TextContent);
        var card = cut.Find(".ht-home > .ht-stack > .ht-card");
        Assert.Equal("Active goals", card.QuerySelector(".ht-card__title")!.TextContent);
        Assert.Equal([$"/goals/{active.Value}"], card.QuerySelectorAll("a.ht-goal__name").Select(a => a.GetAttribute("href")));
        Assert.DoesNotContain($"/goals/{paused.Value}", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Slice_7_HP_005_a_user_whose_goals_are_all_paused_keeps_the_dashboard_with_0_active_goals()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        var paused = await CreateGoalAsync(accountId);
        await PauseAsync(accountId, paused);

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head", "ht-home"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(["0", "0", "0", "0"], cut.FindAll(".ht-home__panel .ht-tile__value").Select(v => v.TextContent));
        var card = cut.Find(".ht-home > .ht-stack > .ht-card");
        Assert.Empty(card.QuerySelectorAll(".ht-goal"));
        Assert.Equal("All goals →", card.QuerySelector("a.ht-card__link")!.TextContent);
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
    }

    /// <summary>Ends <paramref name="goal"/> through the GoalChangeService at the current time, as Abandoned with no note.</summary>
    private async Task EndAsync(Guid accountId, GoalId goal)
    {
        await using var scope = Services.CreateAsyncScope();
        var ended = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().EndAsync(
            ActingUser.ForAccount(new AccountId(accountId), Role.RegisteredUser), goal, Outcome.Abandoned, null, CancellationToken.None);
        Assert.Equal(GoalEndOutcome.Ended, ended.Outcome);
    }

    [Fact]
    public async Task Slice_8_a_user_whose_goals_are_all_ended_keeps_the_dashboard_with_the_ended_goals_Pending_entries()
    {
        // Design slice 8 state (T009): as for a user whose goals are all paused. The goal starts on Tuesday 17 November and
        // ends on Thursday 19; at 10:00 that day the 17th's and 18th's grace periods have not ended, so both stay Pending.
        var accountId = _authorization.SignIn("RegisteredUser");
        var goal = await CreateGoalAsync(accountId);
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));
        await EndAsync(accountId, goal);

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head", "ht-home"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(["0", "2", "0", "0"], cut.FindAll(".ht-home__panel .ht-tile__value").Select(v => v.TextContent));
        var card = cut.Find(".ht-home > .ht-stack > .ht-card");
        Assert.Equal("Active goals", card.QuerySelector(".ht-card__title")!.TextContent);
        Assert.Empty(card.QuerySelectorAll(".ht-goal"));
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Slice_8_a_user_whose_only_goal_ended_on_its_start_date_keeps_the_dashboard_with_nothing_to_record()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        var goal = await CreateGoalAsync(accountId);
        await EndAsync(accountId, goal);

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head", "ht-home"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(["0", "0", "0", "0"], cut.FindAll(".ht-home__panel .ht-tile__value").Select(v => v.TextContent));
        Assert.Empty(cut.Find(".ht-home > .ht-stack > .ht-card").QuerySelectorAll(".ht-goal"));
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_013_an_administrators_home_has_nothing_but_its_page_head()
    {
        _authorization.SignIn("Administrator");

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(MorningHead, PageHead(cut));
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-stat, .ht-stats, .ht-hero, .ht-empty"));
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
    }
}
