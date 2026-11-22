using HT.Application.Goals;
using HT.Application.Reports;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T094; US4-AS3, FR-004, FR-015, AC-103): <see cref="AttemptService.GetGoalHistoryAsync"/> on
/// <see cref="AttemptWorld"/>: every ended attempt of the acting user's goals, the latest ending date first, then by name
/// (<see cref="HabitName.KeyOf"/>, ordinal), then by attempt number, each named as FR-004 gives it at the supplied time.
/// The user has no stored zone, so everything is in UTC. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class GoalHistoryTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Tue3 = Date(2026, 11, 3);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private async Task<IReadOnlyList<EndedAttemptView>> HistoryAsync()
    {
        var result = await _world.Attempts.GetGoalHistoryAsync(_world.User, None);
        Assert.Equal(GoalHistoryListOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Entries;
    }

    private async Task EndAsync(GoalId goal, DateTimeOffset at, Outcome outcome = Outcome.Abandoned, string? note = null)
    {
        _world.At(at);
        Assert.Equal(GoalEndOutcome.Ended, (await _world.Changes.EndAsync(_world.User, goal, outcome, note, None)).Outcome);
    }

    private async Task StartAsync(GoalId goal, GoalTargetFields target, DateOnly start, DateTimeOffset at)
    {
        _world.At(at);
        var result = await _world.Changes.StartAttemptAsync(_world.User, goal, new NewAttemptInput { Target = target, StartDate = start }, None);
        Assert.Equal(AttemptStartOutcome.Started, result.Outcome);
    }

    [Fact]
    public async Task US4_AS3_gives_Meditations_attempt_1_then_Sugars_attempt_1_with_their_fields()
    {
        var goals = await EndedGoalsArrangement.MakeAsync(_world);

        var history = await HistoryAsync();

        Assert.Equal(
            [
                new EndedAttemptView
                {
                    GoalId = goals.Meditation, Name = EndedGoalsArrangement.MeditationName, AttemptNumber = 1,
                    EndingDate = EndedGoalsArrangement.Mon16, Outcome = Outcome.Abandoned, EndingNote = null, IsLatestAttempt = true,
                },
                new EndedAttemptView
                {
                    GoalId = goals.Sugar, Name = EndedGoalsArrangement.SugarName, AttemptNumber = 1,
                    EndingDate = EndedGoalsArrangement.Sat14, Outcome = Outcome.Achieved, EndingNote = EndedGoalsArrangement.SugarNote,
                    IsLatestAttempt = false,
                },
            ],
            history);
    }

    [Fact]
    public async Task Endings_on_one_date_are_ordered_by_name_ignoring_case_then_by_attempt_number_after_the_later_dates()
    {
        // Created on Monday 2 November at 08:00Z with that start date. "Reading, 10 min daily" X ends on its start date
        // (attempt 1 has no date), restarts on Tuesday 3 November and ends again on Wednesday 4 November; Y, "alpha" and
        // "Beta" end on Wednesday 4 November. "alpha" sorts before "Beta" by HabitName.KeyOf, although "Beta" comes first
        // ordinally; X's attempt 2 comes after Y's attempt 1 under the same name.
        var x = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));
        var y = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));
        var beta = await _world.CreateNamedGoalAsync("Beta", MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));
        var alpha = await _world.CreateNamedGoalAsync("alpha", MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));
        await EndAsync(x, Utc("2026-11-02T09:00:00Z"));
        await StartAsync(x, MinutesDaily(10), Tue3, Utc("2026-11-03T09:00:00Z"));
        await EndAsync(x, Utc("2026-11-04T09:00:00Z"), Outcome.NoLongerRelevant);
        await EndAsync(y, Utc("2026-11-04T09:00:00Z"));
        await EndAsync(beta, Utc("2026-11-04T09:00:00Z"));
        await EndAsync(alpha, Utc("2026-11-04T09:00:00Z"), Outcome.Achieved);
        _world.At(Utc("2026-11-05T12:00:00Z"));

        var history = await HistoryAsync();

        Assert.Equal(
            [
                (alpha, "alpha", 1, Wed4, true),
                (beta, "Beta", 1, Wed4, true),
                (y, "Reading, 10 min daily", 1, Wed4, true),
                (x, "Reading, 10 min daily", 2, Wed4, true),
                (x, "Reading, 10 min daily", 1, Mon2, false),
            ],
            history.Select(e => (e.GoalId, e.Name, e.AttemptNumber, e.EndingDate, e.IsLatestAttempt)));
        Assert.Equal(Outcome.NoLongerRelevant, history[3].Outcome);
    }

    [Fact]
    public async Task AC_103_a_habit_renamed_after_the_ending_names_both_attempts_with_its_new_name()
    {
        var goals = await EndedGoalsArrangement.MakeAsync(_world);
        await _world.CatalogHabits.UpdateAsync(
            goals.SugarHabit.Id, HabitSamples.Definition("Sweets", intent: Intent.Reduce), goals.SugarHabit.SuggestedStartingGoals, None);
        await _world.CatalogHabits.UpdateAsync(
            goals.MeditationHabit.Id, HabitSamples.Definition("Mindfulness"), goals.MeditationHabit.SuggestedStartingGoals, None);

        var history = await HistoryAsync();

        Assert.Equal(
            [("Mindfulness, 10 min daily", 1), ("Sweets, at most 1 sweet a day", 1)],
            history.Select(e => (e.Name, e.AttemptNumber)));
    }

    [Fact]
    public async Task An_earlier_attempt_keeps_the_target_text_of_its_last_version_after_a_restart_with_another_target()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));
        await EndAsync(goal, Utc("2026-11-04T09:00:00Z"));
        await StartAsync(goal, MinutesDaily(20), Date(2026, 11, 5), Utc("2026-11-05T09:00:00Z"));
        _world.At(Utc("2026-11-06T12:00:00Z"));

        var history = await HistoryAsync();

        Assert.Equal([("Reading, 10 min daily", 1, Wed4, false)], history.Select(e => (e.Name, e.AttemptNumber, e.EndingDate, e.IsLatestAttempt)));
    }

    [Fact]
    public async Task A_user_with_no_ended_attempt_has_an_empty_goal_history()
    {
        await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Utc("2026-11-02T08:00:00Z"));

        Assert.Empty(await HistoryAsync());
    }

    [Fact]
    public async Task A_user_with_no_goal_has_an_empty_goal_history()
    {
        Assert.Empty(await HistoryAsync());
    }
}
