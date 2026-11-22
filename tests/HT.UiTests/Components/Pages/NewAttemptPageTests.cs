using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.UiTests.Components.Pages.MeditationAttempts;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 8 (T092; HP-004, HP-009, US3-AS1, AS2, AS6, AS7, AS14, contracts/host.md "Start new attempt page"): the Start new
/// attempt page over the real GoalService, GoalChangeService and the fakes, for "Meditation, 10 min daily" ended on Monday
/// 16 November 2026 (<see cref="MeditationAttempts"/>). The user has no stored zone: UTC.
/// </summary>
public sealed class NewAttemptPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeGoalStore _goals;
    private int _notFound;

    public NewAttemptPageTests()
    {
        _authorization = AddAuthorization();
        this.AddCatalog();
        this.AddCustomHabits();
        _goals = this.AddGoals();
        Navigation.OnNotFound += (_, _) => _notFound++;
    }

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    private FakeTimeProvider Clock => (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();

    private ActingUser SignIn(string role = "RegisteredUser") =>
        ActingUser.ForAccount(new AccountId(_authorization.SignIn(role)), role == "Administrator" ? Role.Administrator : Role.RegisteredUser);

    private IRenderedComponent<NewAttempt> RenderPage(GoalId id) => Render<NewAttempt>(p => p.Add(x => x.Id, id.Value));

    private static IElement Button(IRenderedComponent<NewAttempt> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text);

    private static List<string> Errors(IRenderedComponent<NewAttempt> cut) =>
        cut.FindAll(".ht-field__error").Select(e => e.TextContent).ToList();

    private Goal GoalOf(GoalId goal) => _goals.Goals.Single(g => g.Id == goal);

    [Fact]
    public async Task HP_004_US3_AS1_the_owner_gets_the_back_link_the_heading_the_line_and_the_form_with_the_starting_values()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(18, 8));

        var cut = RenderPage(goal);

        Assert.Equal(0, _notFound);
        var back = cut.Find("a.ht-back");
        Assert.Equal(("← " + Name, $"/goals/{goal.Value}"), (back.TextContent, back.GetAttribute("href")));
        Assert.Equal("Start new attempt", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal("Attempt 2 starts with its own history and a streak from zero.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.Equal("10", cut.Find("#new-attempt-amount").GetAttribute("value"));
        Assert.Equal("2026-11-18", cut.Find("#new-attempt-start").GetAttribute("value"));
        Assert.Equal(7, cut.FindAll(".ht-days button[aria-pressed=true]").Count);
        Assert.Empty(cut.FindAll("[id$=-name]"));
        Assert.Equal(["Start attempt", "Cancel"], cut.FindAll(".ht-form-actions .ht-btn").Select(a => a.TextContent));
        Assert.Equal($"/goals/{goal.Value}", cut.Find(".ht-form-actions a").GetAttribute("href"));
        Assert.Equal("Meditation: at least 10 min daily, starting Wed 18 Nov.", cut.Find("aside.ht-summary .ht-card__title").TextContent);
        // HP-009: the first field gets the focus when the page opens.
        Assert.Single(JSInterop.VerifyFocusAsyncInvoke(1));
    }

    [Fact]
    public async Task US3_AS1_on_the_day_of_the_ending_the_start_date_is_the_day_after_the_ending_date()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(16, 11));

        var cut = RenderPage(goal);

        Assert.Equal("2026-11-17", cut.Find("#new-attempt-start").GetAttribute("value"));
    }

    [Fact]
    public async Task US3_AS2_AS14_Start_attempt_with_the_values_unchanged_starts_attempt_2_and_opens_the_goal_page()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(18, 8));
        var cut = RenderPage(goal);

        Button(cut, "Start attempt").Click();

        Assert.EndsWith($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
        var stored = GoalOf(goal);
        Assert.False(stored.IsEnded);
        Assert.Equal((2, Wed18), (stored.Attempt.Number, stored.Attempt.StartDate));
    }

    [Fact]
    public async Task US3_AS6_a_start_date_in_the_past_is_refused_under_Start_date_and_nothing_changes()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(18, 8));
        var cut = RenderPage(goal);

        cut.Find("#new-attempt-start").Change("2026-11-17");
        Button(cut, "Start attempt").Click();

        Assert.Equal(["The start date cannot be in the past."], Errors(cut));
        Assert.Equal("true", cut.Find("#new-attempt-start").GetAttribute("aria-invalid"));
        Assert.Equal("new-attempt-StartDate-error", cut.Find("#new-attempt-start").GetAttribute("aria-describedby"));
        Assert.True(GoalOf(goal).IsEnded);
        Assert.DoesNotContain($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US3_AS6_the_ending_date_is_refused_on_the_day_of_the_ending_and_a_target_refusal_replaces_it()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(16, 11));
        var cut = RenderPage(goal);

        cut.Find("#new-attempt-start").Change("2026-11-16");
        Button(cut, "Start attempt").Click();
        Assert.Equal(["A new attempt starts after the day the goal ended."], Errors(cut));

        // One refusal at a time: a target refusal comes before the start date's.
        cut.Find("#new-attempt-amount").Change("");
        Button(cut, "Start attempt").Click();
        Assert.Equal(["Enter a whole number from 1 to 999."], Errors(cut));
        Assert.Equal("true", cut.Find("#new-attempt-amount").GetAttribute("aria-invalid"));
        Assert.Null(cut.Find("#new-attempt-start").GetAttribute("aria-invalid"));
        Assert.True(GoalOf(goal).IsEnded);
    }

    [Fact]
    public async Task A_goal_restarted_while_the_page_was_open_shows_the_refusal_above_the_form()
    {
        var user = SignIn();
        var goal = await EndedAsync(this, user);
        Clock.SetUtcNow(Utc(18, 8));
        var cut = RenderPage(goal);
        await StartAttemptAsync(this, user, goal, Utc(18, 8), MinutesDaily(10), Wed18);

        Button(cut, "Start attempt").Click();

        Assert.Equal("Only an ended goal can start a new attempt.", cut.Find("form > p.ht-field__error[role=alert]").TextContent);
        Assert.Equal(["Only an ended goal can start a new attempt."], Errors(cut));
        Assert.Equal(2, GoalOf(goal).Attempt.Number);
    }

    [Fact]
    public async Task HP_004_US3_AS7_a_goal_that_is_not_ended_returns_to_its_goal_page()
    {
        var user = SignIn();
        var goal = await RestartedAsync(this, user, Utc(19, 10));

        var cut = RenderPage(goal);

        Assert.EndsWith($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("form"));
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public async Task HP_004_another_accounts_goal_an_unknown_goal_and_an_administrator_get_the_Not_found_page()
    {
        var owner = SignIn();
        var goal = await EndedAsync(this, owner);
        Clock.SetUtcNow(Utc(18, 8));

        SignIn();
        var other = RenderPage(goal);
        var unknown = RenderPage(new GoalId(Guid.NewGuid()));
        SignIn("Administrator");
        var administrator = RenderPage(goal);

        Assert.Equal(3, _notFound);
        foreach (var cut in new[] { other, unknown, administrator })
        {
            Assert.Empty(cut.FindAll("form"));
            Assert.DoesNotContain("Meditation", cut.Markup, StringComparison.Ordinal);
        }

        Assert.True(GoalOf(goal).IsEnded);
    }
}
