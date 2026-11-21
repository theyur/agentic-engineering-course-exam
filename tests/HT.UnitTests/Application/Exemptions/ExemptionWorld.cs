using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Application.Records;
using HT.UnitTests.Application.Reports;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// The fakes a slice-7 unit test needs (T037), built on <see cref="ReportWorld"/> and through it on
/// <see cref="RecordWorld"/>: the slice-4 fakes, a <see cref="FakeRecordStore"/> sharing the goal store's goals, a fake
/// clock and the UTC installation default, with a registered user in UTC, another registered user and an administrator.
/// Goals are created at an earlier supplied instant through <c>GoalService</c>; records are added through
/// <see cref="RecordingService"/>; excusals and pauses are seeded on the fake store with any dates, past ones included
/// (<see cref="SeedExcusal"/>, <see cref="SeedPause"/>), or excusals are made through <see cref="ExcusalService"/>
/// (<see cref="ExcuseAsync"/>, <see cref="RemoveExcusalAsync"/>); then the clock is moved
/// forward and the goal is read through <see cref="RecordingService"/> and <see cref="ReportService"/>. Each test owns
/// its world and its clock. Nothing here computes an instant: every instant a test passes is a hand-worked UTC literal.
/// </summary>
public sealed class ExemptionWorld
{
    private readonly ReportWorld _reports;

    /// <summary>A world whose clock starts at 00:00 UTC on 1 November 2026, the month of the spec's scenarios.</summary>
    public ExemptionWorld()
        : this(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero))
    {
    }

    /// <summary>A world whose clock starts at <paramref name="clockStart"/>.</summary>
    public ExemptionWorld(DateTimeOffset clockStart)
    {
        _reports = new ReportWorld(clockStart);
    }

    public FakeGoalStore Goals => _reports.Goals;

    public FakeRecordStore Records => _reports.Records;

    /// <summary>The clock; it only moves forward, and every test sets it before it creates a goal.</summary>
    public FakeTimeProvider Clock => _reports.Clock;

    /// <summary>A registered user with no stored time zone, so the goals are in UTC.</summary>
    public ActingUser User => _reports.User;

    /// <summary>Another registered user, in UTC.</summary>
    public ActingUser OtherUser => _reports.OtherUser;

    public ActingUser Administrator => _reports.Administrator;

    public RecordingService Recording => _reports.Recording;

    public ReportService Reports => _reports.Service;

    /// <summary>The excusal use cases over the same fakes and clock (US1, T044).</summary>
    public ExcusalService Excusals => new(_reports.Records, _reports.Goals, _reports.Accounts, _reports.Clock);

    /// <summary>The pause use cases over the same fakes and clock, with the UTC installation default (US2, T055).</summary>
    public PauseService Pauses => new(_reports.Records, _reports.Goals, _reports.Accounts, _reports.Clock, _reports.DefaultZone);

    /// <summary>The profile use cases over the same fakes and clock, for changing the account's time zone (slice 4 FR-026).</summary>
    public ProfileService Profiles => new(_reports.Accounts, _reports.Goals, _reports.Clock, _reports.DefaultZone);

    /// <summary>A registered user stored with <paramref name="timeZone"/>.</summary>
    public ActingUser UserIn(string timeZone) => _reports.UserIn(timeZone);

    /// <summary>Sets the clock to <paramref name="instant"/> (UTC).</summary>
    public void At(DateTimeOffset instant) => _reports.At(instant);

    /// <summary>
    /// Creates a goal of <paramref name="user"/> (default <see cref="User"/>) with <paramref name="target"/>, starting on
    /// <paramref name="start"/>, at <paramref name="at"/>, and leaves the clock there.
    /// </summary>
    public Task<GoalId> CreateGoalAsync(GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null) =>
        _reports.CreateGoalAsync(target, start, at, user);

    /// <summary>
    /// Creates a goal with its own <paramref name="name"/>, as <see cref="CreateGoalAsync"/> does, so the goal order and
    /// the names on the Goals page and the Home can be tested.
    /// </summary>
    public Task<GoalId> CreateNamedGoalAsync(string name, GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null) =>
        _reports.CreateNamedGoalAsync(name, target, start, at, user);

    /// <summary>
    /// Stores a goal of <paramref name="owner"/> directly, with one settings version per entry of
    /// <paramref name="versions"/>, as later time-zone changes would leave it; the start date is the first version's.
    /// </summary>
    public GoalId AddGoal(ActingUser owner, GoalTarget target, params (DateOnly From, NamedTimeZone Zone)[] versions) =>
        _reports.AddGoal(owner, target, versions);

    /// <summary>Pauses a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<PauseResult> PauseAsync(GoalId goal, DateOnly? firstDay, DateOnly? lastDay, string? reason = null, ActingUser? user = null) =>
        Pauses.PauseAsync(user ?? User, goal, firstDay, lastDay, reason, CancellationToken.None);

    /// <summary>Resumes a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<ResumeResult> ResumeAsync(GoalId goal, ActingUser? user = null) =>
        Pauses.ResumeAsync(user ?? User, goal, CancellationToken.None);

    /// <summary>Cancels a pause of a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<CancelPauseResult> CancelPauseAsync(GoalId goal, PauseId pause, ActingUser? user = null) =>
        Pauses.CancelPauseAsync(user ?? User, goal, pause, CancellationToken.None);

    /// <summary>The pauses of a goal of <see cref="User"/> (or <paramref name="user"/>) at the clock's time, expected found.</summary>
    public async Task<IReadOnlyList<PauseView>> PausesAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await Pauses.GetPausesAsync(user ?? User, goal, CancellationToken.None);
        Assert.Equal(PausesOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Pauses;
    }

    /// <summary>The reason of <paramref name="text"/>, trimmed; null for none.</summary>
    public static ExemptionReason? Reason(string? text)
    {
        Assert.True(ExemptionReason.TryCreate(text, out var reason, out _));
        return reason;
    }

    /// <summary>Stores an excusal of <paramref name="date"/> directly, with no clock or ownership check.</summary>
    public void SeedExcusal(GoalId goal, DateOnly date, string? reason = null) => Records.SeedExcusal(goal, date, Reason(reason));

    /// <summary>Stores a pause from <paramref name="firstDay"/> to <paramref name="lastDay"/> directly, with no clock or ownership check.</summary>
    public PauseId SeedPause(GoalId goal, DateOnly firstDay, DateOnly lastDay, string? reason = null) =>
        Records.SeedPause(goal, firstDay, lastDay, Reason(reason));

    /// <summary>Stores a duration or count record directly, with no clock or ownership check.</summary>
    public RecordId SeedAmount(GoalId goal, DateOnly date, int amount) => _reports.SeedAmount(goal, date, amount);

    /// <summary>Stores a yes/no record directly, with no clock or ownership check.</summary>
    public RecordId SeedAnswer(GoalId goal, DateOnly date, YesNoAnswer answer) => _reports.SeedAnswer(goal, date, answer);

    /// <summary>Stores a note directly under the entry whose key is <paramref name="date"/>.</summary>
    public void SeedNote(GoalId goal, DateOnly date, string text) => _reports.SeedNote(goal, date, text);

    /// <summary>Records for <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<RecordWriteResult> RecordAsync(GoalId goal, DateOnly date, RecordInput input, ActingUser? user = null) =>
        Recording.RecordAsync(user ?? User, goal, date, input, CancellationToken.None);

    /// <summary>Changes a record for <see cref="User"/> at the clock's time.</summary>
    public Task<RecordWriteResult> ChangeAsync(GoalId goal, RecordId record, RecordInput input, ActingUser? user = null) =>
        Recording.ChangeRecordAsync(user ?? User, goal, record, input, CancellationToken.None);

    /// <summary>Deletes a record for <see cref="User"/> at the clock's time.</summary>
    public Task<RecordWriteResult> DeleteAsync(GoalId goal, RecordId record, ActingUser? user = null) =>
        Recording.DeleteRecordAsync(user ?? User, goal, record, CancellationToken.None);

    /// <summary>Saves the note of the entry of <paramref name="date"/> for <see cref="User"/> at the clock's time.</summary>
    public Task<NoteSaveResult> SaveNoteAsync(GoalId goal, DateOnly date, string? note, ActingUser? user = null) =>
        Recording.SaveNoteAsync(user ?? User, goal, date, note, CancellationToken.None);

    /// <summary>Excuses the commitment of <paramref name="date"/> for <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<ExcuseResult> ExcuseAsync(GoalId goal, DateOnly date, string? reason = null, ActingUser? user = null) =>
        Excusals.ExcuseAsync(user ?? User, goal, date, reason, CancellationToken.None);

    /// <summary>Removes the excusal of <paramref name="date"/> for <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<RemoveExcusalResult> RemoveExcusalAsync(GoalId goal, DateOnly date, ActingUser? user = null) =>
        Excusals.RemoveExcusalAsync(user ?? User, goal, date, CancellationToken.None);

    /// <summary>The history of <see cref="User"/>'s goal from <paramref name="first"/> to <paramref name="last"/>, expected found.</summary>
    public async Task<IReadOnlyList<HistoryEntry>> HistoryAsync(GoalId goal, DateOnly first, DateOnly last, ActingUser? user = null)
    {
        var result = await Recording.GetHistoryAsync(user ?? User, goal, first, last, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!.Entries;
    }

    /// <summary>The one history entry of <paramref name="date"/>'s commitment, day or week.</summary>
    public async Task<HistoryEntry> EntryAsync(GoalId goal, DateOnly date) => Assert.Single(await HistoryAsync(goal, date, date));

    /// <summary>What <see cref="User"/> can record now, expected found: its entries, newest first.</summary>
    public async Task<IReadOnlyList<HistoryEntry>> RecordableAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await Recording.GetRecordableAsync(user ?? User, goal, CancellationToken.None);
        Assert.Equal(RecordableOutcome.Found, result.Outcome);
        return result.History!.Entries;
    }

    /// <summary>The recent notes of <see cref="User"/>'s goal, expected found.</summary>
    public async Task<IReadOnlyList<RecentNote>> RecentNotesAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await Recording.GetRecentNotesAsync(user ?? User, goal, CancellationToken.None);
        Assert.Equal(RecentNotesOutcome.Found, result.Outcome);
        return result.Notes;
    }

    /// <summary>The report of a goal of <see cref="User"/> (or <paramref name="user"/>), expected found.</summary>
    public Task<GoalReport> ReportAsync(GoalId goal, ActingUser? user = null) => _reports.ReportAsync(goal, user);

    /// <summary>The goals overview of <see cref="User"/> (or <paramref name="user"/>), expected found.</summary>
    public Task<GoalsOverview> OverviewAsync(ActingUser? user = null) => _reports.OverviewAsync(user);

    /// <summary>The dashboard of <see cref="User"/> (or <paramref name="user"/>), expected found.</summary>
    public Task<Dashboard> DashboardAsync(ActingUser? user = null) => _reports.DashboardAsync(user);

    public static RecordInput Minutes(int minutes, string? note = null) => RecordWorld.Minutes(minutes, note);

    public static RecordInput Amount(int amount, string? note = null) => RecordWorld.Amount(amount, note);

    public static RecordInput Yes(string? note = null) => RecordWorld.Yes(note);

    public static RecordInput No(string? note = null) => RecordWorld.No(note);
}
