using HT.Application.Exemptions;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Reading a goal's pauses (FR-012, US2-AS13, AC-086; T057) over the fakes of <see cref="ExemptionWorld"/>. The goal
/// "Drinking water, 8 glasses a day" starts on Monday 2 November 2026 and is created at 08:00Z that day, in UTC, unless a
/// test says otherwise. Each pause's timing is at the pause date (research R-03). Every instant is worked out by hand.
/// </summary>
public sealed class PausesListTests
{
    private const string NotSignedIn = "You are not signed in.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string GoalNotFound = "This goal does not exist.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Fri27 = Date(2026, 11, 27);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    private Task<GoalId> GoalAsync() => _world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning);

    private static void AssertRefused(PausesResult result, PausesOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Empty(result.Pauses);
    }

    // ---- access (FR-012 with FR-021, FR-022) ----

    [Fact]
    public async Task An_anonymous_visitor_an_administrator_and_an_unstored_account_are_refused_with_no_store_call()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon16, Sun22, "Family visit");
        _world.At(Mon16NineOClock);
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        AssertRefused(await _world.Pauses.GetPausesAsync(ActingUser.Anonymous, goal, CancellationToken.None), PausesOutcome.NotSignedIn, NotSignedIn);
        AssertRefused(
            await _world.Pauses.GetPausesAsync(_world.Administrator, goal, CancellationToken.None), PausesOutcome.NotForAdministrators, NotForAdministrators);
        AssertRefused(await _world.Pauses.GetPausesAsync(gone, goal, CancellationToken.None), PausesOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task Another_users_goal_and_no_goal_give_This_goal_does_not_exist()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon16, Sun22, "Family visit");
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.Pauses.GetPausesAsync(_world.OtherUser, goal, CancellationToken.None), PausesOutcome.NotFound, GoalNotFound);
        AssertRefused(
            await _world.Pauses.GetPausesAsync(_world.User, new GoalId(Guid.NewGuid()), CancellationToken.None), PausesOutcome.NotFound, GoalNotFound);
        Assert.Empty(_world.Records.Calls);
    }

    // ---- the scenarios ----

    [Fact]
    public async Task US2_AS13_the_pause_of_scenario_1_is_listed_with_its_days_reason_and_current()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        var paused = await _world.PauseAsync(goal, Mon16, Sun22, "Family visit");

        var pause = Assert.Single(await _world.PausesAsync(goal));

        Assert.Equal(paused.Pause!.Id, pause.Id);
        Assert.Equal(Mon16, pause.FirstDay);
        Assert.Equal(Sun22, pause.LastDay);
        Assert.Equal("Family visit", pause.Reason);
        Assert.Equal(PauseTiming.Current, pause.Timing);
        Assert.Equal("Mon 16 Nov – Sun 22 Nov", pause.RangeText);
    }

    [Fact]
    public async Task US2_AS13_past_current_and_upcoming_pauses_are_listed_latest_first_day_first()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon2, Wed4, "Ill");
        _world.At(Mon16NineOClock);
        await _world.PauseAsync(goal, Mon16, Sun22, "Family visit");
        await _world.PauseAsync(goal, Mon23, Fri27);

        var pauses = await _world.PausesAsync(goal);

        Assert.Equal([Mon23, Mon16, Mon2], pauses.Select(p => p.FirstDay));
        Assert.Equal([PauseTiming.Upcoming, PauseTiming.Current, PauseTiming.Past], pauses.Select(p => p.Timing));
        Assert.Equal(["Mon 23 Nov – Fri 27 Nov", "Mon 16 Nov – Sun 22 Nov", "Mon 2 Nov – Wed 4 Nov"], pauses.Select(p => p.RangeText));
        Assert.Equal([null, "Family visit", "Ill"], pauses.Select(p => p.Reason));
    }

    [Fact]
    public async Task FR_012_a_pause_is_current_on_its_last_day_and_past_the_day_after()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon16, Sun22);

        // 23:59:59Z on Sunday 22: today is Sunday 22. 00:00:00Z on Monday 23: today is Monday 23.
        _world.At(Utc("2026-11-22T23:59:59Z"));
        Assert.Equal(PauseTiming.Current, Assert.Single(await _world.PausesAsync(goal)).Timing);
        _world.At(Utc("2026-11-23T00:00:00Z"));
        Assert.Equal(PauseTiming.Past, Assert.Single(await _world.PausesAsync(goal)).Timing);
    }

    [Fact]
    public async Task FR_012_a_goal_with_no_pause_lists_none()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        Assert.Empty(await _world.PausesAsync(goal));
    }

    [Fact]
    public async Task AC_086_a_goal_that_has_not_started_lists_its_pauses_by_the_accounts_today_in_its_zone()
    {
        // The account is in Kyiv (UTC+2 in November). At 23:00Z on Monday 16 November it is 01:00 on Tuesday 17 in Kyiv;
        // the goal starts on Friday 20, so it has no current date and today is Tuesday 17 (in UTC it would be Monday 16).
        var user = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(DrinkingWater, Fri20, Utc("2026-11-16T23:00:00Z"), user);
        _world.SeedPause(goal, Mon16, Mon16);
        _world.SeedPause(goal, Tue17, Wed18);
        _world.SeedPause(goal, Thu19, Fri20);

        var pauses = await _world.PausesAsync(goal, user);

        Assert.Equal([Thu19, Tue17, Mon16], pauses.Select(p => p.FirstDay));
        Assert.Equal([PauseTiming.Upcoming, PauseTiming.Current, PauseTiming.Past], pauses.Select(p => p.Timing));
        var result = await _world.Pauses.GetPausesAsync(user, goal, CancellationToken.None);
        Assert.Equal(Tue17, result.PauseDate);
    }

    [Fact]
    public async Task R_03_the_read_gives_the_pause_date_the_goals_current_date_and_none_on_a_refusal()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        Assert.Equal(Mon16, (await _world.Pauses.GetPausesAsync(_world.User, goal, CancellationToken.None)).PauseDate);
        Assert.Null((await _world.Pauses.GetPausesAsync(_world.OtherUser, goal, CancellationToken.None)).PauseDate);
    }
}
