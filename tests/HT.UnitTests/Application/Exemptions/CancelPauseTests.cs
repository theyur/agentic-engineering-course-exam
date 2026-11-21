using HT.Application.Exemptions;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Cancelling an upcoming pause (FR-011, US2-AS7, US5-AS4; T057) over the fakes of <see cref="ExemptionWorld"/>. Every
/// goal starts on Monday 2 November 2026 and is created at 08:00Z that day, in UTC. "Today" is the goal's current date
/// (research R-03): at 08:00Z on Wednesday 18 November it is Wednesday 18. Every instant is worked out by hand.
/// </summary>
public sealed class CancelPauseTests
{
    private const string NotSignedIn = "You are not signed in.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string GoalNotFound = "This goal does not exist.";
    private const string PauseNotFound = "This pause does not exist.";
    private const string HasBegun = "Only a pause that has not begun can be cancelled. Resume the goal instead.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);
    private static readonly DateOnly Fri27 = Date(2026, 11, 27);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");
    private static readonly DateTimeOffset Wed18EightOClock = Utc("2026-11-18T08:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>"Yoga, 3 times a week": a yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private Task<GoalId> GoalAsync(GoalTargetFields? target = null) => _world.CreateGoalAsync(target ?? DrinkingWater, Mon2, Mon2Morning);

    /// <summary>
    /// US2-AS7's goal: at 09:00Z on Monday 16, paused from Monday 16 to Sunday 22 (scenario 1) and from Monday 23 to
    /// Friday 27 (upcoming). Returns the two pauses.
    /// </summary>
    private async Task<(GoalId Goal, PauseId Current, PauseId Upcoming)> TwoPausesAsync()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        var current = await _world.PauseAsync(goal, Mon16, Sun22, "Family visit");
        var upcoming = await _world.PauseAsync(goal, Mon23, Fri27);
        return (goal, current.Pause!.Id, upcoming.Pause!.Id);
    }

    private static void AssertRefused(CancelPauseResult result, CancelPauseOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
    }

    private static void AssertCancelled(CancelPauseResult result)
    {
        Assert.Equal(CancelPauseOutcome.Cancelled, result.Outcome);
        Assert.Equal("pause cancelled", result.Message);
        Assert.Equal(PauseTexts.Cancelled, result.Message);
    }

    // ---- access (FR-011 steps 1 to 3, as FR-002's) ----

    [Fact]
    public async Task FR_011_1_an_anonymous_visitor_is_refused_and_the_pause_stays()
    {
        var (goal, _, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, upcoming, ActingUser.Anonymous), CancelPauseOutcome.NotSignedIn, NotSignedIn);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    [Fact]
    public async Task FR_011_2_an_administrator_is_refused_and_the_pause_stays()
    {
        var (goal, _, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(
            await _world.CancelPauseAsync(goal, upcoming, _world.Administrator), CancelPauseOutcome.NotForAdministrators, NotForAdministrators);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    [Fact]
    public async Task FR_011_3_an_account_that_is_not_stored_is_refused_as_not_signed_in()
    {
        var (goal, _, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        AssertRefused(await _world.CancelPauseAsync(goal, upcoming, gone), CancelPauseOutcome.NotSignedIn, NotSignedIn);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    [Fact]
    public async Task US5_AS4_another_users_goal_gives_This_goal_does_not_exist_and_the_pause_stays()
    {
        var (goal, _, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, upcoming, _world.OtherUser), CancelPauseOutcome.NotFound, GoalNotFound);
        AssertRefused(await _world.CancelPauseAsync(new GoalId(Guid.NewGuid()), upcoming), CancelPauseOutcome.NotFound, GoalNotFound);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    // ---- the scenarios ----

    [Fact]
    public async Task US2_AS7_an_upcoming_pause_is_cancelled_and_the_goal_has_no_pause_on_those_dates()
    {
        var (goal, current, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertCancelled(await _world.CancelPauseAsync(goal, upcoming));

        var exemptions = _world.Records.ExemptionsOfGoal(goal);
        Assert.Equal(current, Assert.Single(exemptions.Pauses).Id);
        for (var day = Mon23; day <= Fri27; day = day.AddDays(1))
        {
            Assert.False(exemptions.IsPaused(day));
        }

        Assert.Equal(EvaluationState.Upcoming, (await _world.EntryAsync(goal, Mon23)).State);
    }

    [Fact]
    public async Task US2_AS7_the_current_pause_cannot_be_cancelled()
    {
        var (goal, current, _) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, current), CancelPauseOutcome.HasBegun, HasBegun);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    [Fact]
    public async Task FR_011_5_a_past_pause_cannot_be_cancelled()
    {
        var goal = await GoalAsync();
        var past = _world.SeedPause(goal, Mon2, Wed4);
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, past), CancelPauseOutcome.HasBegun, HasBegun);
        Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task FR_011_5_a_pause_starting_today_has_begun_and_one_starting_tomorrow_has_not()
    {
        var goal = await GoalAsync();
        var today = _world.SeedPause(goal, Wed18, Wed18);
        var tomorrow = _world.SeedPause(goal, Thu19, Sun22);
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, today), CancelPauseOutcome.HasBegun, HasBegun);
        AssertCancelled(await _world.CancelPauseAsync(goal, tomorrow));
        Assert.Equal(today, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).Id);
    }

    [Fact]
    public async Task FR_011_4_an_unknown_pause_gives_This_pause_does_not_exist()
    {
        var (goal, _, _) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, new PauseId(Guid.NewGuid())), CancelPauseOutcome.PauseNotFound, PauseNotFound);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
        Assert.DoesNotContain("DeletePauseAsync", _world.Records.Calls);
    }

    [Fact]
    public async Task FR_021_an_upcoming_pause_of_another_of_the_users_goals_gives_This_pause_does_not_exist()
    {
        var goal = await GoalAsync();
        var other = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Mon2Morning);
        var othersPause = _world.SeedPause(other, Mon23, Fri27);
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, othersPause), CancelPauseOutcome.PauseNotFound, PauseNotFound);
        Assert.Single(_world.Records.ExemptionsOfGoal(other).Pauses);
    }

    [Fact]
    public async Task FR_011_order_another_goals_current_pause_gives_step_4_not_step_5()
    {
        var goal = await GoalAsync();
        var other = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Mon2Morning);
        var othersCurrent = _world.SeedPause(other, Mon16, Sun22);
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.CancelPauseAsync(goal, othersCurrent), CancelPauseOutcome.PauseNotFound, PauseNotFound);
    }

    [Fact]
    public async Task Edge_cancelling_the_upcoming_pause_that_pauses_the_current_week_makes_it_Due_again()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18EightOClock);
        var pause = (await _world.PauseAsync(goal, Thu19, Wed25)).Pause!.Id;
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);

        AssertCancelled(await _world.CancelPauseAsync(goal, pause));

        Assert.Equal(EvaluationState.Due, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task A_pause_removed_before_the_write_gives_This_pause_does_not_exist()
    {
        var (goal, _, upcoming) = await TwoPausesAsync();
        _world.At(Wed18EightOClock);

        // The fake store completes synchronously, so the removal has happened when the hook returns.
        _world.Records.BeforeNextWrite = () =>
            _ = _world.Records.DeletePauseAsync(_world.User.AccountId!.Value, goal, upcoming, CancellationToken.None);

        AssertRefused(await _world.CancelPauseAsync(goal, upcoming), CancelPauseOutcome.PauseNotFound, PauseNotFound);
    }
}
