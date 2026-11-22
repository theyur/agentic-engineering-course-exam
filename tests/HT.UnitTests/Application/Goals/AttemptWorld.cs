using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Application.Reports;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T051): the fakes the shared read path's unit tests need, built on <see cref="GoalWorld"/>'s,
/// <see cref="Records.RecordWorld"/>'s, <see cref="ReportWorld"/>'s and <see cref="Exemptions.ExemptionWorld"/>'s
/// pattern: the slice-4 fakes, a <see cref="FakeRecordStore"/> sharing the goal store's goals, a fake clock and the UTC
/// installation default, with a registered user in UTC, another registered user and an administrator. Goals are either
/// seeded through the attempts constructor of <see cref="Goal"/> with any attempts, versions and endings
/// (<see cref="Seed"/>), or created at an earlier supplied time through <see cref="GoalService"/> and then changed,
/// ended and restarted at later times through the store's conditional writes, as the goal-change use cases will make
/// them (<see cref="ChangeSettingsAsync"/>, <see cref="EndAsync"/>, <see cref="StartAttemptAsync"/>): each reads
/// <see cref="IGoalStore.ReadOwnerGoalAsync"/>, decides in the Domain at the clock's time and writes. Records go through
/// <see cref="RecordingService"/> or straight into the fake; reads through <see cref="GoalService"/>,
/// <see cref="RecordingService"/>, <see cref="ReportService"/>, <see cref="CatalogService"/> and
/// <see cref="ProfileService"/>. Each test owns its world and its clock, which only moves forward.
/// </summary>
public sealed class AttemptWorld
{
    private static readonly CancellationToken None = CancellationToken.None;

    private readonly ReportWorld _reports;

    /// <summary>A world whose clock starts at 00:00 UTC on 1 November 2026.</summary>
    public AttemptWorld()
        : this(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero))
    {
    }

    /// <summary>A world whose clock starts at <paramref name="clockStart"/>.</summary>
    public AttemptWorld(DateTimeOffset clockStart)
    {
        _reports = new ReportWorld(clockStart);
    }

    public FakeAccountStore Accounts => _reports.Accounts;

    public FakeCatalogHabitStore CatalogHabits => _reports.CatalogHabits;

    public FakeGoalStore Goals => _reports.Goals;

    public FakeRecordStore Records => _reports.Records;

    public FakeTimeProvider Clock => _reports.Clock;

    public InstallationTimeZone DefaultZone => _reports.DefaultZone;

    /// <summary>A registered user with no stored time zone, so the goals are in UTC.</summary>
    public ActingUser User => _reports.User;

    /// <summary>Another registered user, in UTC.</summary>
    public ActingUser OtherUser => _reports.OtherUser;

    public ActingUser Administrator => _reports.Administrator;

    /// <summary>The Published catalog habit "Reading" the goals are based on.</summary>
    public CatalogHabit Habit => _reports.Habit;

    /// <summary>The view of <see cref="Habit"/> that the goal store gives with a seeded goal.</summary>
    public GoalHabitView HabitView => new(Habit.Id, HabitKind.Catalog, Habit.Definition.Name.Value, Habit.Definition.Category, true);

    public GoalService GoalService => _reports.GoalService;

    public RecordingService Recording => _reports.Recording;

    public ReportService Reports => _reports.Service;

    /// <summary>The catalog as the host builds it, with the clock (slice 8 research R-08).</summary>
    public CatalogService Catalog => new(CatalogHabits, Goals, Clock);

    public ProfileService Profiles => new(Accounts, Goals, Clock, DefaultZone);

    public PauseService Pauses => new(Records, Goals, Accounts, Clock, DefaultZone);

    public ExcusalService Excusals => new(Records, Goals, Accounts, Clock);

    /// <summary>Slice 8 (T061): the goal-change use cases over the same fakes and clock.</summary>
    public GoalChangeService Changes => new(Goals, Accounts, Clock, DefaultZone);

    /// <summary>Slice 8 (T082): the attempt reads over the same fakes and clock.</summary>
    public AttemptService Attempts => new(Goals, Records, Accounts, Clock);

    /// <summary>A registered user stored with <paramref name="timeZone"/>.</summary>
    public ActingUser UserIn(string timeZone) => _reports.UserIn(timeZone);

    /// <summary>Sets the clock to <paramref name="instant"/> (UTC).</summary>
    public void At(DateTimeOffset instant) => _reports.At(instant);

    /// <summary>Creates a goal through <see cref="GoalService"/> at <paramref name="at"/>, and leaves the clock there.</summary>
    public Task<GoalId> CreateGoalAsync(GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null) =>
        _reports.CreateGoalAsync(target, start, at, user);

    /// <summary>Creates a goal with its own <paramref name="name"/> through <see cref="GoalService"/> at <paramref name="at"/>.</summary>
    public Task<GoalId> CreateNamedGoalAsync(string name, GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null) =>
        _reports.CreateNamedGoalAsync(name, target, start, at, user);

    /// <summary>
    /// Stores a goal of <paramref name="owner"/> (default <see cref="User"/>) on <see cref="Habit"/> with
    /// <paramref name="attempts"/>, built through the attempts constructor, so a test can have past attempts and endings
    /// that the use cases would refuse to write now.
    /// </summary>
    public GoalId Seed(ActingUser? owner, GoalName? ownName, params GoalAttemptVersions[] attempts)
    {
        var account = (owner ?? User).AccountId!.Value;
        var goal = new Goal(new GoalId(Guid.NewGuid()), account, GoalInputs.Catalog(Habit), ownName, attempts);
        Goals.Add(account, goal, HabitView);
        return goal.Id;
    }

    /// <summary>Stores a goal of <see cref="User"/> with <paramref name="attempts"/> and no own name.</summary>
    public GoalId Seed(params GoalAttemptVersions[] attempts) => Seed(null, null, attempts);

    /// <summary>The stored goal, with all its attempts, as the store holds it now.</summary>
    public Goal GoalOf(GoalId goal) => Goals.Goals.Single(g => g.Id == goal);

    /// <summary>
    /// Ends the goal at the clock's time, as ending will (FR-007, FR-008): the ending date is the goal's current date,
    /// or the account's today when it has not started; <see cref="GoalTimeline.EndingOf"/> gives the last date and the
    /// kept versions, which <see cref="IGoalStore.TryEndAsync"/> writes, cutting the pauses and excusals after the last
    /// date. Returns the ending date and the last date.
    /// </summary>
    public async Task<(DateOnly EndingDate, DateOnly LastDate)> EndAsync(
        GoalId goal, Outcome outcome = Outcome.Abandoned, string? note = null, ActingUser? user = null)
    {
        var (account, read) = await ReadAsync(goal, user);
        var now = Clock.GetUtcNow();
        var stored = read.Goal!.Goal;
        var endingDate = GoalTimeline.CurrentDate(stored, now) ?? ZoneOf(read).DateAt(now);
        Assert.True(EndingNote.TryCreate(note, out var endingNote, out _));
        var effect = GoalTimeline.EndingOf(stored, endingDate);
        var write = new GoalEndingWrite(goal, stored.Attempt.Number, new GoalEnding(endingDate, outcome, endingNote), effect.LastDate, effect.KeptVersions);
        Assert.Equal(GoalChangeWriteOutcome.Written, await Goals.TryEndAsync(account, read.StoredTimeZone, read.GoalsRevision, write, None));
        return (endingDate, effect.LastDate);
    }

    /// <summary>
    /// <see cref="EndAsync"/> run to completion at once, for a <c>BeforeNextWrite</c> hook that simulates an ending
    /// between a use case's check and its write; the fakes complete every call synchronously.
    /// </summary>
    public void EndNow(GoalId goal, ActingUser? user = null) => EndAsync(goal, user: user).GetAwaiter().GetResult();

    /// <summary>
    /// Starts a new attempt of an ended goal from <paramref name="start"/> with <paramref name="target"/>, in the account's
    /// current zone, as starting a new attempt will (FR-012), through <see cref="IGoalStore.TryStartAttemptAsync"/>.
    /// </summary>
    public async Task StartAttemptAsync(GoalId goal, GoalTargetFields target, DateOnly start, ActingUser? user = null)
    {
        var (account, read) = await ReadAsync(goal, user);
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(target, out var checkedTarget));
        var first = new GoalSettingsVersion(new GoalSettings(checkedTarget!, ZoneOf(read)), start);
        var write = new GoalAttemptWrite(goal, read.Goal!.Goal.Attempt.Number + 1, first);
        Assert.Equal(GoalChangeWriteOutcome.Written, await Goals.TryStartAttemptAsync(account, read.StoredTimeZone, read.GoalsRevision, write, None));
    }

    /// <summary>
    /// Changes the latest attempt's target at the clock's time, as a settings change will (FR-002), through
    /// <see cref="GoalTimeline.SettingsChangeOf"/> and <see cref="IGoalStore.TryChangeSettingsAsync"/>; the own name stays.
    /// Returns the date the change applies from.
    /// </summary>
    public async Task<DateOnly> ChangeSettingsAsync(GoalId goal, GoalTargetFields target, ActingUser? user = null)
    {
        var (account, read) = await ReadAsync(goal, user);
        var stored = read.Goal!.Goal;
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(target, out var checkedTarget));
        var change = GoalTimeline.SettingsChangeOf(stored, Clock.GetUtcNow(), checkedTarget!, ZoneOf(read));
        var write = new GoalSettingsWrite(goal, stored.Attempt.Number, stored.OwnName, change.Versions);
        Assert.Equal(GoalChangeWriteOutcome.Written, await Goals.TryChangeSettingsAsync(account, read.StoredTimeZone, read.GoalsRevision, write, None));
        return change.AppliesFrom;
    }

    /// <summary>Records for <see cref="User"/> (or <paramref name="user"/>) at the clock's time.</summary>
    public Task<RecordWriteResult> RecordAsync(GoalId goal, DateOnly date, RecordInput input, ActingUser? user = null) =>
        Recording.RecordAsync(user ?? User, goal, date, input, None);

    /// <summary>Stores a duration or count record directly, with no clock or ownership check.</summary>
    public RecordId SeedAmount(GoalId goal, DateOnly date, int amount) => Records.Seed(goal, date, RecordValue.FromAmount(amount));

    /// <summary>Stores a yes/no record directly, with no clock or ownership check.</summary>
    public RecordId SeedAnswer(GoalId goal, DateOnly date, YesNoAnswer answer) => Records.Seed(goal, date, RecordValue.FromAnswer(answer));

    /// <summary>Stores a note directly under the entry key <paramref name="date"/>, with no clock or ownership check.</summary>
    public void SeedNote(GoalId goal, DateOnly date, string text) => _reports.SeedNote(goal, date, text);

    /// <summary>The goal as <see cref="GoalService.GetAsync"/> gives it to <see cref="User"/> at the clock's time, expected found.</summary>
    public async Task<GoalDetails> DetailsAsync(GoalId goal)
    {
        var result = await GoalService.GetAsync(User, goal, None);
        Assert.Equal(GoalReadOutcome.Found, result.Outcome);
        return result.Goal!;
    }

    /// <summary>The goal list of <see cref="User"/> at the clock's time, expected found.</summary>
    public async Task<IReadOnlyList<GoalListItem>> ListAsync()
    {
        var result = await GoalService.ListAsync(User, None);
        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        return result.Goals;
    }

    /// <summary>The History of <see cref="User"/>'s goal from <paramref name="first"/> to <paramref name="last"/>, at the clock's time.</summary>
    public async Task<GoalHistory> HistoryAsync(GoalId goal, DateOnly first, DateOnly last)
    {
        var result = await Recording.GetHistoryAsync(User, goal, first, last, None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>What <see cref="User"/> can record now for the goal, at the clock's time.</summary>
    public async Task<GoalHistory> RecordableAsync(GoalId goal)
    {
        var result = await Recording.GetRecordableAsync(User, goal, None);
        Assert.Equal(RecordableOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>The recent notes of <see cref="User"/>'s goal, at the clock's time.</summary>
    public async Task<IReadOnlyList<RecentNote>> RecentNotesAsync(GoalId goal)
    {
        var result = await Recording.GetRecentNotesAsync(User, goal, None);
        Assert.Equal(RecentNotesOutcome.Found, result.Outcome);
        return result.Notes;
    }

    /// <summary>The report of <see cref="User"/>'s goal at the clock's time, expected found.</summary>
    public Task<GoalReport> ReportAsync(GoalId goal) => _reports.ReportAsync(goal);

    /// <summary>The goals overview of <see cref="User"/> at the clock's time, expected found.</summary>
    public Task<GoalsOverview> OverviewAsync() => _reports.OverviewAsync();

    /// <summary>The dashboard of <see cref="User"/> at the clock's time, expected found.</summary>
    public Task<Dashboard> DashboardAsync() => _reports.DashboardAsync();

    private async Task<(AccountId Account, OwnerGoal Read)> ReadAsync(GoalId goal, ActingUser? user)
    {
        var account = (user ?? User).AccountId!.Value;
        var read = await Goals.ReadOwnerGoalAsync(account, goal, None);
        Assert.NotNull(read);
        Assert.NotNull(read.Goal);
        return (account, read);
    }

    /// <summary>The account's stored zone, else the installation default (the account's today and a new version's zone).</summary>
    private NamedTimeZone ZoneOf(OwnerGoal read) =>
        NamedTimeZone.TryParse(read.StoredTimeZone, out var zone) ? zone : DefaultZone.Value;
}
