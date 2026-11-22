using Bunit;
using Bunit.TestDoubles;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Account;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 8 (T099; HP-006, US4-AS3, US4-AS7, FR-015, FR-023, design/pages/PageProfile "Slice 8 state"): the Profile's Goal
/// history tab over the real use cases and the fakes, in UTC. Spec User Story 4's goals, created on Monday 2 November 2026
/// at 08:00Z with that start date: "Sugar, at most 1 sweet a day" ended on Saturday 14 November at 09:00Z with "Achieved"
/// and "Three weeks without extra sweets." and restarted on Monday 16 at 09:00Z; "Meditation, 10 min daily" ended on
/// Monday 16 at 10:00Z with "Abandoned"; read on Monday 16 at 12:00Z. The page's session service is never called on these
/// tabs, so it is registered without its Identity services.
/// </summary>
public sealed class ProfilePageGoalHistoryTests : BunitContext
{
    private const string MeditationName = "Meditation, 10 min daily";
    private const string SugarName = "Sugar, at most 1 sweet a day";
    private const string SugarNote = "Three weeks without extra sweets.";

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeTimeProvider _clock;

    public ProfilePageGoalHistoryTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        this.AddCustomHabits();
        Services.AddSingleton(provider => new AccountService(
            provider.GetRequiredService<IAccountStore>(), provider.GetRequiredService<TimeProvider>(), InstallationTimeZone.From(null)));
        Services.AddSingleton(new AccountSession(null!, null!));
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
    }

    private ActingUser SignInUser() => ActingUser.ForAccount(new AccountId(_authorization.SignIn("RegisteredUser")), Role.RegisteredUser);

    private static GoalTargetFields MeditationTarget => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields SugarTarget => new()
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 1, CountUnit = "sweet",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    };

    private async Task<GoalId> CreateAsync(ActingUser user, CatalogHabit habit, GoalTargetFields target)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = Mon2 }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    /// <summary>The goals of the class summary for <paramref name="user"/>; the clock is left at Monday 16 at 12:00Z.</summary>
    private async Task<(GoalId Meditation, GoalId Sugar)> EndedGoalsAsync(ActingUser user)
    {
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
        var meditation = await CreateAsync(user, _catalog.Add(HabitSamples.Catalog("Meditation")), MeditationTarget);
        var sugar = await CreateAsync(user, _catalog.Add(HabitSamples.Catalog("Sugar", intent: Intent.Reduce)), SugarTarget);
        await using var scope = Services.CreateAsyncScope();
        var changes = scope.ServiceProvider.GetRequiredService<GoalChangeService>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 14, 9, 0, 0, TimeSpan.Zero));
        Assert.Equal(GoalEndOutcome.Ended, (await changes.EndAsync(user, sugar, Outcome.Achieved, SugarNote, CancellationToken.None)).Outcome);
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.Zero));
        var started = await changes.StartAttemptAsync(user, sugar, new NewAttemptInput { Target = SugarTarget, StartDate = Mon16 }, CancellationToken.None);
        Assert.Equal(AttemptStartOutcome.Started, started.Outcome);
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));
        Assert.Equal(GoalEndOutcome.Ended, (await changes.EndAsync(user, meditation, Outcome.Abandoned, null, CancellationToken.None)).Outcome);
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero));
        return (meditation, sugar);
    }

    private IRenderedComponent<Profile> RenderAt(string address)
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(address);
        return Render<Profile>();
    }

    private static (string Text, string? Href, string? Current)[] TabsOf(IRenderedComponent<Profile> cut)
    {
        var nav = cut.Find("nav.ht-tabs");
        Assert.Equal("Profile sections", nav.GetAttribute("aria-label"));
        return nav.QuerySelectorAll("a.ht-tab").Select(a => (a.TextContent, a.GetAttribute("href"), a.GetAttribute("aria-current"))).ToArray();
    }

    [Fact]
    public async Task HP_006_US4_AS7_the_Goal_history_tab_lists_the_ended_attempts_in_the_use_cases_order()
    {
        var (meditation, sugar) = await EndedGoalsAsync(SignInUser());

        var cut = RenderAt("/profile?tab=history");

        Assert.Equal(
            [("Account", "/profile", null), ("Goal history", "/profile?tab=history", "page"), ("Custom habits", "/profile?tab=custom-habits", null)],
            TabsOf(cut));
        var table = cut.Find(".ht-tabs + .ht-table-wrap > table.ht-table");
        Assert.Equal("Goal history", table.GetAttribute("aria-label"));
        Assert.Equal(["Goal", "Attempt", "Ended", "Outcome", "Note"], table.QuerySelectorAll("thead th[scope=col]").Select(th => th.TextContent));
        Assert.Equal(
            [
                (MeditationName, $"/goals/{meditation.Value}", "1", "Mon 16 Nov", "Abandoned", "—"),
                (SugarName, $"/goals/{sugar.Value}/attempts/1", "1", "Sat 14 Nov", "Achieved", SugarNote),
            ],
            table.QuerySelectorAll("tbody tr").Select(row =>
            {
                var cells = row.QuerySelectorAll("td");
                var link = cells[0].QuerySelector("b > a")!;
                return (link.TextContent, link.GetAttribute("href"), cells[1].TextContent, cells[2].TextContent,
                    cells[3].QuerySelector("span.ht-chip")!.TextContent, cells[4].TextContent);
            }));
        Assert.All(table.QuerySelectorAll("tbody tr"), row => Assert.Equal(["ht-muted"], row.QuerySelectorAll("td")[4].ClassList));
        Assert.Empty(cut.FindAll("h2.ht-card__title"));
        Assert.DoesNotContain("Community", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_006_with_no_ended_attempt_the_tab_says_No_ended_goals_yet()
    {
        SignInUser();

        var cut = RenderAt("/profile?tab=history");

        Assert.Equal("No ended goals yet.", cut.Find(".ht-tabs + div.ht-card > p.ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public async Task HP_006_the_Account_tab_keeps_its_content_and_has_Goal_history_as_its_second_tab()
    {
        await EndedGoalsAsync(SignInUser());

        var cut = RenderAt("/profile");

        Assert.Equal(
            [("Account", "/profile", "page"), ("Goal history", "/profile?tab=history", null), ("Custom habits", "/profile?tab=custom-habits", null)],
            TabsOf(cut));
        Assert.Equal("Account", cut.Find("h2.ht-card__title").TextContent);
        Assert.Empty(cut.FindAll("table"));
        Assert.DoesNotContain(MeditationName, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FR_015_another_users_ended_attempts_are_not_in_the_goal_history()
    {
        var other = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
        await EndedGoalsAsync(other);
        SignInUser();

        var cut = RenderAt("/profile?tab=history");

        Assert.Equal("No ended goals yet.", cut.Find(".ht-tabs + div.ht-card > p.ht-card__sub").TextContent);
        Assert.DoesNotContain(MeditationName, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(SugarNote, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FR_023_an_administrators_Profile_ignores_the_tab_and_keeps_its_Account_content_without_Tabs()
    {
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderAt("/profile?tab=history");

        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.Empty(cut.FindAll("table"));
        Assert.Equal("Account", cut.Find("h2.ht-card__title").TextContent);
        Assert.DoesNotContain("Goal history", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("No ended goals yet.", cut.Markup, StringComparison.Ordinal);
    }
}
