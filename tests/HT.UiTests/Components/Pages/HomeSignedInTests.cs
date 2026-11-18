using Bunit;
using Bunit.TestDoubles;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 1 US6-AS10, HP-013, HP-015, amended by slice 2 HP-013 (US5-AS3) and slice 4 HP-010 (US8-AS5, US11): the signed-in
/// Home shows the greeting as heading-1 and then today's date, in place of slice 1's "Today"; a registered user with no
/// goals also gets the Goals page's EmptyState, a registered user with goals and an administrator nothing else. The
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

    private async Task CreateGoalAsync(Guid accountId)
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
    }

    private FakeAccountStore Accounts => (FakeAccountStore)Services.GetRequiredService<IAccountStore>();

    /// <summary>Signs in, and stores the account with a name and a time zone when given.</summary>
    private async Task<Guid> SignInStoredAsync(string role, string? name = null, string? timeZone = null)
    {
        var id = _authorization.SignIn(role);
        var accountId = new AccountId(id);
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

        return id;
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
    public async Task US8_AS5_a_registered_user_with_goals_gets_nothing_but_the_page_head()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        await CreateGoalAsync(accountId);

        var cut = Render<Home>();

        Assert.Equal(["ht-page-head"], cut.Find(".ht-container").Children.Select(c => c.ClassName));
        Assert.Equal(MorningHead, PageHead(cut));
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-list, .ht-goalitem, .ht-stat, .ht-stats, .ht-hero, .ht-empty"));
        Assert.DoesNotContain("Hs Reading", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Goals come in a later version.", cut.Markup, StringComparison.Ordinal);
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
