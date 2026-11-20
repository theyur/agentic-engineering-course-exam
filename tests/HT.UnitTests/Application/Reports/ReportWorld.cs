using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Application.Records;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The fakes a <see cref="ReportService"/> unit test needs, on the pattern of <see cref="GoalWorld"/> and built on
/// <see cref="RecordWorld"/>: the slice-4 fakes, a <see cref="FakeRecordStore"/> sharing the goal store's goals, a fake
/// clock and the UTC installation default, with a registered user in UTC, another registered user and an
/// administrator. Goals are created at an earlier supplied instant through <see cref="GoalService"/>, whose start date
/// cannot be in the past; records are added through <see cref="RecordingService"/> or <see cref="FakeRecordStore.Seed"/>;
/// then the clock is moved forward and the report is read through <see cref="ReportService"/>. Each test owns its world
/// and its clock.
/// </summary>
public sealed class ReportWorld
{
    private readonly RecordWorld _recording;

    /// <summary>A world whose clock starts at 00:00 UTC on 1 January 2026, so a goal can have a year of results.</summary>
    public ReportWorld()
        : this(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
    {
    }

    /// <summary>A world whose clock starts at <paramref name="clockStart"/>.</summary>
    public ReportWorld(DateTimeOffset clockStart)
    {
        _recording = new RecordWorld(clockStart);
    }

    public FakeAccountStore Accounts => _recording.Accounts;

    public FakeCatalogHabitStore CatalogHabits => _recording.CatalogHabits;

    public FakeGoalStore Goals => _recording.Goals;

    public FakeRecordStore Records => _recording.Records;

    /// <summary>The clock; it only moves forward.</summary>
    public FakeTimeProvider Clock => _recording.Clock;

    public InstallationTimeZone DefaultZone => _recording.DefaultZone;

    /// <summary>A registered user with no stored time zone, so the goals are in UTC.</summary>
    public ActingUser User => _recording.User;

    /// <summary>Another registered user, in UTC.</summary>
    public ActingUser OtherUser => _recording.OtherUser;

    public ActingUser Administrator => _recording.Administrator;

    /// <summary>The Published catalog habit "Reading" the goals are based on.</summary>
    public CatalogHabit Habit => _recording.Habit;

    public ReportService Service => new(Goals, Records, Accounts, Clock, DefaultZone);

    public RecordingService Recording => _recording.Service;

    /// <summary>A registered user stored with <paramref name="timeZone"/>.</summary>
    public ActingUser UserIn(string timeZone) => _recording.UserIn(timeZone);

    /// <summary>Sets the clock to <paramref name="instant"/> (UTC).</summary>
    public void At(DateTimeOffset instant) => _recording.At(instant);

    /// <summary>
    /// Creates a goal of <paramref name="user"/> (default <see cref="User"/>) with <paramref name="target"/>, starting on
    /// <paramref name="start"/>, through <see cref="GoalService"/> at <paramref name="at"/>, and leaves the clock there.
    /// </summary>
    public Task<GoalId> CreateGoalAsync(GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null) =>
        _recording.CreateGoalAsync(target, start, at, user);

    /// <summary>Records for <see cref="User"/> (or <paramref name="user"/>) at the clock's time, and expects it recorded.</summary>
    public async Task<RecordId> RecordAsync(GoalId goal, DateOnly date, RecordInput input, ActingUser? user = null)
    {
        var result = await _recording.RecordAsync(goal, date, input, user);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        return result.RecordId!.Value;
    }

    /// <summary>
    /// Stores a goal of <paramref name="owner"/> directly, with one settings version per entry of
    /// <paramref name="versions"/>, as later time-zone changes would leave it (<see cref="RecordWorld.AddGoal"/>).
    /// </summary>
    public GoalId AddGoal(ActingUser owner, GoalTarget target, params (DateOnly From, NamedTimeZone Zone)[] versions) =>
        _recording.AddGoal(owner, target, versions);

    /// <summary>Changes a record for <see cref="User"/> (or <paramref name="user"/>) at the clock's time, and expects it changed.</summary>
    public async Task ChangeAsync(GoalId goal, RecordId record, RecordInput input, ActingUser? user = null)
    {
        var result = await _recording.ChangeAsync(goal, record, input, user);
        Assert.Equal(RecordWriteOutcome.Changed, result.Outcome);
    }

    /// <summary>Deletes a record for <see cref="User"/> (or <paramref name="user"/>) at the clock's time, and expects it deleted.</summary>
    public async Task DeleteAsync(GoalId goal, RecordId record, ActingUser? user = null)
    {
        var result = await _recording.DeleteAsync(goal, record, user);
        Assert.Equal(RecordWriteOutcome.Deleted, result.Outcome);
    }

    /// <summary>Stores a duration or count record directly, with no clock or ownership check.</summary>
    public RecordId SeedAmount(GoalId goal, DateOnly date, int amount) => Records.Seed(goal, date, RecordValue.FromAmount(amount));

    /// <summary>Stores a yes/no record directly, with no clock or ownership check.</summary>
    public RecordId SeedAnswer(GoalId goal, DateOnly date, YesNoAnswer answer) => Records.Seed(goal, date, RecordValue.FromAnswer(answer));

    /// <summary>The report of a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<GoalReportResult> ReadAsync(GoalId goal, ActingUser? user = null) =>
        Service.GetGoalReportAsync(user ?? User, goal, CancellationToken.None);

    /// <summary>The report of a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time, expected found.</summary>
    public async Task<GoalReport> ReportAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await ReadAsync(goal, user);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Report!;
    }
}
