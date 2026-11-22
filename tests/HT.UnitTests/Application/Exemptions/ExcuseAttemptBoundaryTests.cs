using HT.Application.Exemptions;
using HT.Domain.Time;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Slice 8 (T116, plan "Spec discrepancies" 2): the exact edges of an attempt for an excusal, on
/// <see cref="AttemptWorld"/> in UTC. An ended attempt's last date and a later attempt's start date belong to that
/// attempt, so a period-based attempt there refuses with "Only commitments can be excused", not "nothing scheduled".
/// </summary>
public sealed class ExcuseAttemptBoundaryTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);

    private readonly AttemptWorld _world = new();

    [Fact]
    public async Task The_last_date_of_an_ended_per_day_attempt_is_a_period_not_a_date_after_the_attempt()
    {
        var goal = _world.Seed(AttemptFrom(1, Mon2, EndedOn(Mon16), SettingsFrom(PerDay(), Mon2, NamedTimeZone.Utc)));
        _world.At(Utc("2026-11-16T12:00:00Z"));

        var result = await _world.Excusals.ExcuseAsync(_world.User, goal, Sun15, null, None);

        Assert.Equal(ExcuseOutcome.NotACommitment, result.Outcome);
        Assert.Equal(ExcusalTexts.NotACommitment, result.Message);
    }

    [Fact]
    public async Task The_start_date_of_a_per_day_attempt_after_a_fixed_days_attempt_is_a_period_not_a_date_between_attempts()
    {
        var goal = _world.Seed(
            AttemptFrom(1, Mon2, EndedOn(Thu12), SettingsFrom(OnDays(MonWedFri), Mon2, NamedTimeZone.Utc)),
            AttemptFrom(2, Fri13, null, SettingsFrom(PerDay(), Fri13, NamedTimeZone.Utc)));
        _world.At(Utc("2026-11-13T12:00:00Z"));

        var result = await _world.Excusals.ExcuseAsync(_world.User, goal, Fri13, null, None);

        Assert.Equal(ExcuseOutcome.NotACommitment, result.Outcome);
        Assert.Equal(ExcusalTexts.NotACommitment, result.Message);
    }
}
