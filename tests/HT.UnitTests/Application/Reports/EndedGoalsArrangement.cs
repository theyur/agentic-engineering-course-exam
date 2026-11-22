using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T094, T095): spec User Story 4's account on an <see cref="AttemptWorld"/>, made through
/// <see cref="GoalService"/> and <see cref="GoalChangeService"/> at supplied times that only move forward. The user has no
/// stored zone, so everything is in UTC. Three goals on three Published catalog habits, all created on Monday 2 November
/// at 08:00Z with that start date, and on target on each date from 2 to 13 November (records stored directly):
/// <list type="bullet">
///   <item>"Reading, 15 min daily" (Reading, Build, duration 15 minutes, all seven days);</item>
///   <item>"Meditation, 10 min daily" (Meditation, Build, duration 10 minutes, all seven days), ended on Monday 16
///   November at 10:00Z with "Abandoned": ending date Monday 16, last date Sunday 15 November;</item>
///   <item>"Sugar, at most 1 sweet a day" (Sugar, Reduce, count at most 1 "sweet", 1 time per day), ended on Saturday 14
///   November at 09:00Z with "Achieved" and <see cref="SugarNote"/> (ending date Saturday 14, last date Friday 13
///   November), and restarted on Monday 16 November at 09:00Z with unchanged settings from that date.</item>
/// </list>
/// The clock is left at <see cref="Mon16At12"/>. Every instant is a UTC literal worked out by hand.
/// </summary>
internal sealed class EndedGoalsArrangement
{
    public const string ReadingName = "Reading, 15 min daily";
    public const string MeditationName = "Meditation, 10 min daily";
    public const string SugarName = "Sugar, at most 1 sweet a day";
    public const string SugarNote = "Three weeks without extra sweets.";

    public static readonly DateOnly Mon2 = Date(2026, 11, 2);
    public static readonly DateOnly Fri13 = Date(2026, 11, 13);
    public static readonly DateOnly Sat14 = Date(2026, 11, 14);
    public static readonly DateOnly Sun15 = Date(2026, 11, 15);
    public static readonly DateOnly Mon16 = Date(2026, 11, 16);

    public static readonly DateTimeOffset Created = Utc("2026-11-02T08:00:00Z");
    public static readonly DateTimeOffset SugarEndedAt = Utc("2026-11-14T09:00:00Z");
    public static readonly DateTimeOffset SugarRestartedAt = Utc("2026-11-16T09:00:00Z");
    public static readonly DateTimeOffset MeditationEndedAt = Utc("2026-11-16T10:00:00Z");

    /// <summary>Monday 16 November at 12:00Z, the read time of US4-AS1 to AS3.</summary>
    public static readonly DateTimeOffset Mon16At12 = Utc("2026-11-16T12:00:00Z");

    private static readonly CancellationToken None = CancellationToken.None;

    private EndedGoalsArrangement(CatalogHabit meditationHabit, CatalogHabit sugarHabit, GoalId reading, GoalId meditation, GoalId sugar)
    {
        MeditationHabit = meditationHabit;
        SugarHabit = sugarHabit;
        Reading = reading;
        Meditation = meditation;
        Sugar = sugar;
    }

    public CatalogHabit MeditationHabit { get; }

    public CatalogHabit SugarHabit { get; }

    public GoalId Reading { get; }

    public GoalId Meditation { get; }

    public GoalId Sugar { get; }

    public static GoalTargetFields ReadingTarget => DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    public static GoalTargetFields MeditationTarget => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    public static GoalTargetFields SugarTarget => CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    /// <summary>Makes the three goals of the class summary on <paramref name="world"/> and leaves its clock at <see cref="Mon16At12"/>.</summary>
    public static async Task<EndedGoalsArrangement> MakeAsync(AttemptWorld world)
    {
        var meditationHabit = world.CatalogHabits.Add(HabitSamples.Catalog("Meditation"));
        var sugarHabit = world.CatalogHabits.Add(HabitSamples.Catalog("Sugar", intent: Intent.Reduce));
        var reading = await CreateAsync(world, world.Habit, ReadingTarget);
        var meditation = await CreateAsync(world, meditationHabit, MeditationTarget);
        var sugar = await CreateAsync(world, sugarHabit, SugarTarget);
        for (var day = Mon2; day <= Fri13; day = day.AddDays(1))
        {
            world.SeedAmount(reading, day, 15);
            world.SeedAmount(meditation, day, 10);
            world.SeedAmount(sugar, day, 1);
        }

        world.At(SugarEndedAt);
        var sugarEnded = await world.Changes.EndAsync(world.User, sugar, Outcome.Achieved, SugarNote, None);
        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Sat14, (DateOnly?)Fri13), (sugarEnded.Outcome, sugarEnded.EndingDate, sugarEnded.LastDate));

        world.At(SugarRestartedAt);
        var restarted = await world.Changes.StartAttemptAsync(
            world.User, sugar, new NewAttemptInput { Target = SugarTarget, StartDate = Mon16 }, None);
        Assert.Equal((AttemptStartOutcome.Started, (int?)2), (restarted.Outcome, restarted.AttemptNumber));

        world.At(MeditationEndedAt);
        var meditationEnded = await world.Changes.EndAsync(world.User, meditation, Outcome.Abandoned, null, None);
        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Mon16, (DateOnly?)Sun15), (meditationEnded.Outcome, meditationEnded.EndingDate, meditationEnded.LastDate));

        world.At(Mon16At12);
        return new EndedGoalsArrangement(meditationHabit, sugarHabit, reading, meditation, sugar);
    }

    private static async Task<GoalId> CreateAsync(AttemptWorld world, CatalogHabit habit, GoalTargetFields target)
    {
        world.At(Created);
        var result = await world.GoalService.CreateAsync(world.User, GoalInputs.Input(GoalInputs.Catalog(habit), Mon2, target), None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }
}
