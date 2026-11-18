using Bunit;
using Bunit.TestDoubles;
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
/// Slice 1 US6-AS10, HP-013, HP-015, amended by slice 2 HP-013 (US5-AS3) and slice 4 HP-010 (US8-AS5): the signed-in
/// Home shows the heading "Today"; a registered user with no goals also gets the Goals page's EmptyState, a registered
/// user with goals and an administrator nothing else. Today is Tuesday 17 November 2026 in UTC.
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

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void US6_AS10_a_signed_in_user_sees_the_heading_today(string role)
    {
        _authorization.SignIn(role);

        var cut = Render<Home>();

        Assert.Equal("Today", Assert.Single(cut.FindAll("h1")).TextContent);
        Assert.Empty(cut.FindAll(".ht-hero, .ht-habit"));
        Assert.DoesNotContain("Popular in the catalog", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void US8_AS5_a_registered_user_with_no_goals_gets_the_goals_empty_state_under_today()
    {
        _authorization.SignIn("RegisteredUser");

        var cut = Render<Home>();

        var container = cut.Find(".ht-container");
        Assert.Equal(["ht-page-head", "ht-card ht-empty"], container.Children.Select(c => c.ClassName));
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
        Assert.DoesNotMatch(@"\d", container.TextContent);
    }

    [Fact]
    public async Task US8_AS5_a_registered_user_with_goals_gets_nothing_but_the_heading()
    {
        var accountId = _authorization.SignIn("RegisteredUser");
        await CreateGoalAsync(accountId);

        var cut = Render<Home>();

        Assert.Equal("Today", cut.Find(".ht-container").TextContent.Trim());
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-list, .ht-goalitem, .ht-stat, .ht-stats, .ht-hero, .ht-empty"));
        Assert.DoesNotContain("Hs Reading", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Goals come in a later version.", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_013_an_administrators_home_has_nothing_but_its_heading()
    {
        _authorization.SignIn("Administrator");

        var cut = Render<Home>();

        Assert.Equal("Today", cut.Find(".ht-container").TextContent.Trim());
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-stat, .ht-stats, .ht-hero, .ht-empty"));
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Good ", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d", cut.Find(".ht-container").TextContent);
    }

    [Fact]
    public void HP_015_a_registered_users_home_has_no_greeting_name_or_date()
    {
        _authorization.SignIn("RegisteredUser");

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll("h2, ul, ol, time, table, .ht-stat, .ht-stats, .ht-hero"));
        Assert.DoesNotContain("Good ", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
    }
}
