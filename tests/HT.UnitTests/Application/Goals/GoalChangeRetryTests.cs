using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T064; research R-05, contracts/application-api.md "Writes"): the write-and-retry loop of
/// <see cref="GoalChangeService"/>, through <see cref="GoalChangeService.ChangeSettingsAsync"/>. The fake goal store's
/// <c>BeforeNextWrite</c> hook makes a concurrent write between the use case's read and its write: a revision bump,
/// an ending or a time zone change. A <see cref="GoalChangeWriteOutcome.Stale"/> reads and decides again, at most
/// <see cref="GoalService.MaxCreateAttempts"/> times. "Reading, 15 min daily" starts on Monday 2 November 2026 in UTC.
/// </summary>
public sealed class GoalChangeRetryTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private async Task<GoalId> ReadingAsync()
    {
        var goal = await _world.CreateNamedGoalAsync("Secret plan", MinutesDaily(15), Mon2, Utc("2026-11-01T10:00:00Z"));
        _world.At(Utc("2026-11-11T10:00:00Z"));
        return goal;
    }

    private Task<SettingsChangeResult> ChangeTo20Async(GoalId goal) =>
        _world.Changes.ChangeSettingsAsync(_world.User, goal, new GoalSettingsInput { Target = MinutesDaily(20), Name = "Secret plan" }, None);

    /// <summary>Makes the next <paramref name="count"/> writes stale, each by a revision bump just before it.</summary>
    private void StaleNext(int count)
    {
        var owner = _world.User.AccountId!.Value;
        var left = count;
        void Bump()
        {
            _world.Goals.IncrementRevision(owner);
            if (--left > 0)
            {
                _world.Goals.BeforeNextWrite = Bump;
            }
        }

        _world.Goals.BeforeNextWrite = Bump;
    }

    [Fact]
    public async Task A_stale_write_reads_again_and_succeeds()
    {
        var goal = await ReadingAsync();
        var (reads, writes) = (_world.Goals.OwnerGoalReads, _world.Goals.ChangeWriteCount);
        StaleNext(1);

        var result = await ChangeTo20Async(goal);

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        Assert.Equal((reads + 2, writes + 2), (_world.Goals.OwnerGoalReads, _world.Goals.ChangeWriteCount));
        Assert.Equal([15, 20], _world.GoalOf(goal).Versions.Select(v => v.Settings.Target.Amount));
    }

    [Fact]
    public async Task Nine_stale_writes_are_retried_and_the_tenth_write_succeeds()
    {
        var goal = await ReadingAsync();
        var writes = _world.Goals.ChangeWriteCount;
        StaleNext(GoalService.MaxCreateAttempts - 1);

        var result = await ChangeTo20Async(goal);

        Assert.Equal(SettingsChangeOutcome.Changed, result.Outcome);
        Assert.Equal(writes + GoalService.MaxCreateAttempts, _world.Goals.ChangeWriteCount);
    }

    [Fact]
    public async Task Ten_stale_writes_throw_an_InvalidOperationException_that_names_no_value()
    {
        var goal = await ReadingAsync();
        var writes = _world.Goals.ChangeWriteCount;
        StaleNext(GoalService.MaxCreateAttempts);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => ChangeTo20Async(goal));

        Assert.Equal(writes + GoalService.MaxCreateAttempts, _world.Goals.ChangeWriteCount);
        foreach (var value in new[] { "20", "15", "Secret plan", "Reading", goal.Value.ToString(), _world.User.AccountId!.Value.Value.ToString() })
        {
            Assert.DoesNotContain(value, thrown.Message, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Single(_world.GoalOf(goal).Versions);
    }

    [Fact]
    public async Task An_ending_between_the_read_and_the_write_gives_This_goal_has_ended_on_the_retry()
    {
        var goal = await ReadingAsync();
        _world.Goals.BeforeNextWrite = () => _world.EndNow(goal);

        var result = await ChangeTo20Async(goal);

        Assert.Equal((SettingsChangeOutcome.GoalEnded, "This goal has ended.", (DateOnly?)null), (result.Outcome, result.Message, result.AppliesFrom));
        Assert.True(_world.GoalOf(goal).IsEnded);
        Assert.All(_world.GoalOf(goal).Versions, v => Assert.Equal(15, v.Settings.Target.Amount));
    }

    [Fact]
    public async Task A_time_zone_change_between_the_read_and_the_write_merges_into_the_one_later_version_with_the_new_zone()
    {
        var goal = await ReadingAsync();
        _world.Goals.BeforeNextWrite = () => _world.Profiles.ChangeTimeZoneAsync(_world.User, "Europe/Kyiv", None).GetAwaiter().GetResult();

        var result = await ChangeTo20Async(goal);

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        var versions = _world.GoalOf(goal).Versions;
        Assert.Equal([Mon2, Thu12], versions.Select(v => v.AppliesFrom));
        Assert.Equal(("Europe/Kyiv", 20), (versions[1].Settings.TimeZone.Id, versions[1].Settings.Target.Amount!.Value));
    }
}
