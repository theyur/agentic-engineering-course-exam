using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Accounts;

/// <summary>
/// <see cref="ProfileService"/>'s time zone (US10, FR-024 to FR-027; research R-04, R-06, R-08): reading it, refusing a
/// value that is not a named time zone, the same zone, each goal's versions after a change and what the goal use cases
/// then give, "today" in the new zone, who may use it, and the retries after conflicting writes. Kyiv is UTC+2, New York
/// UTC−5 and Tokyo UTC+9 throughout November and December 2026; every instant below is worked out by hand.
/// US10-AS9 (no method takes another account's identifier) is enforced structurally by <c>UseCaseParameterTests</c>.
/// </summary>
public sealed class TimeZoneTests
{
    private const string KyivId = "Europe/Kyiv";
    private const string NewYorkId = "America/New_York";
    private const string TokyoId = "Asia/Tokyo";

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);
    private static readonly DateOnly Tue1Dec = Date(2026, 12, 1);

    /// <summary>12:00 in New York on Thursday 19 November 2026 (US10-AS10).</summary>
    private static readonly DateTimeOffset NewYorkNoonThursday = Utc(2026, 11, 19, 17);

    private readonly GoalWorld _world = new();
    private readonly CatalogHabit _reading;

    public TimeZoneTests() => _reading = _world.CatalogHabits.Add(HabitSamples.Reading());

    private static CancellationToken None => CancellationToken.None;

    private static DateTimeOffset Utc(int year, int month, int day, int hour, int minute = 0) =>
        new(year, month, day, hour, minute, 0, TimeSpan.Zero);

    private static ActingUser Registered(GoalWorld world, string? zone) => GoalInputs.RegisteredUser(world.Accounts, zone);

    private string? StoredZoneOf(ActingUser user) => _world.Accounts.FindById(user.AccountId!.Value)!.StoredTimeZone;

    private (DateOnly From, string Zone)[] VersionsOf(GoalId id) =>
        _world.Goals.Goals.Single(g => g.Id == id).Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id)).ToArray();

    /// <summary>A goal that began on Monday 16 November in Kyiv, stored as creation on that day would have left it.</summary>
    private GoalId Started(ActingUser user, GoalTarget target) =>
        _world.AddGoal(user.AccountId!.Value, _reading, target, (Mon16, Kyiv)).Id;

    private async Task<GoalId> CreateAsync(GoalWorld world, ActingUser user, GoalTargetFields target, DateOnly start)
    {
        var result = await world.Service.CreateAsync(user, GoalInputs.Input(GoalInputs.Catalog(_reading), start, target), None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private async Task<TimeZoneChangeResult> ChangeAsync(ActingUser user, string? zone, ProfileService? profiles = null)
    {
        var result = await (profiles ?? _world.Profiles).ChangeTimeZoneAsync(user, zone, None);
        return result;
    }

    private async Task<string?> ReadZoneAsync(ActingUser user)
    {
        var result = await _world.Profiles.GetTimeZoneAsync(user, None);
        Assert.Equal(TimeZoneReadOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.TimeZone;
    }

    private async Task<GoalDetails> ReadGoalAsync(ActingUser user, GoalId id)
    {
        var result = await _world.Service.GetAsync(user, id, None);
        Assert.Equal(GoalReadOutcome.Found, result.Outcome);
        return result.Goal!;
    }

    private async Task<GoalCalendarView> CalendarAsync(ActingUser user, GoalId id, DateOnly first, DateOnly last)
    {
        var result = await _world.Service.GetCalendarAsync(user, id, first, last, None);
        Assert.Equal(GoalCalendarOutcome.Found, result.Outcome);
        return result.Calendar!;
    }

    // ---- Reading -------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(KyivId)]
    [InlineData("Asia/Calcutta")]
    public async Task GetTimeZoneAsync_gives_the_stored_zone_exactly_as_given(string stored)
    {
        var user = Registered(_world, stored);

        Assert.Equal(stored, await ReadZoneAsync(user));
    }

    [Theory]
    [InlineData(null, "UTC")]
    [InlineData(KyivId, KyivId)]
    public async Task GetTimeZoneAsync_gives_the_installations_default_when_none_is_stored(string? configured, string expected)
    {
        var world = new GoalWorld(configured);
        var user = GoalInputs.RegisteredUserWithDefaultZone(world.Accounts);

        var result = await world.Profiles.GetTimeZoneAsync(user, None);

        Assert.Equal((TimeZoneReadOutcome.Found, expected), (result.Outcome, result.TimeZone));
    }

    // ---- Refusals and the same zone ------------------------------------------------------------------------------

    [Theory]
    [InlineData("Mars/Olympus")]
    [InlineData("FLE Standard Time")]
    [InlineData("Etc/GMT+3")]
    [InlineData("Etc/UTC")]
    [InlineData("EST")]
    [InlineData("+02:00")]
    [InlineData("europe/kyiv")]
    [InlineData(" Europe/Kyiv")]
    [InlineData("")]
    [InlineData(null)]
    public async Task US10_AS7_a_value_that_is_not_a_named_time_zone_is_refused_and_changes_nothing(string? value)
    {
        var user = Registered(_world, KyivId);
        var goal = Started(user, PerDay());

        var result = await ChangeAsync(user, value);

        Assert.Equal(TimeZoneChangeOutcome.NotANamedTimeZone, result.Outcome);
        Assert.Equal("Choose a time zone from the list.", result.Message);
        Assert.Equal(KyivId, StoredZoneOf(user));
        Assert.Equal([(Mon16, KyivId)], VersionsOf(goal));
        Assert.Equal(0, _world.Goals.TimeZoneWriteCount);
        Assert.Equal(0, _world.Goals.RevisionOf(user.AccountId!.Value));
    }

    [Fact]
    public async Task The_same_zone_is_Unchanged_and_writes_nothing()
    {
        var user = Registered(_world, KyivId);
        var goal = Started(user, PerDay());

        var result = await ChangeAsync(user, KyivId);

        Assert.Equal((TimeZoneChangeOutcome.Unchanged, (string?)null), (result.Outcome, result.Message));
        Assert.Equal(KyivId, StoredZoneOf(user));
        Assert.Equal([(Mon16, KyivId)], VersionsOf(goal));
        Assert.Equal(0, _world.Goals.TimeZoneWriteCount);
        Assert.Equal(0, _world.Goals.RevisionOf(user.AccountId!.Value));
    }

    [Fact]
    public async Task The_installations_default_is_the_same_zone_for_an_account_with_none_stored()
    {
        var world = new GoalWorld(KyivId);
        var user = GoalInputs.RegisteredUserWithDefaultZone(world.Accounts);

        var result = await world.Profiles.ChangeTimeZoneAsync(user, KyivId, None);

        Assert.Equal(TimeZoneChangeOutcome.Unchanged, result.Outcome);
        Assert.Null(world.Accounts.FindById(user.AccountId!.Value)!.StoredTimeZone);
        Assert.Equal(0, world.Goals.TimeZoneWriteCount);
    }

    [Fact]
    public async Task A_link_is_stored_and_read_back_as_given()
    {
        var user = Registered(_world, KyivId);

        var result = await ChangeAsync(user, "Europe/Kiev");

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal("Europe/Kiev", await ReadZoneAsync(user));
    }

    // ---- US10-AS4 to AS6 and AS10 --------------------------------------------------------------------------------

    /// <summary>A Kyiv user with a Mon/Wed/Fri goal, a per-day goal and a per-week goal begun on Monday 16 November, and a per-day goal starting on 1 December.</summary>
    private async Task<(ActingUser User, GoalId MonWedFri, GoalId PerDay, GoalId PerWeek, GoalId December)> ArrangeKyivGoalsAsync()
    {
        var user = Registered(_world, KyivId);
        var monWedFri = Started(user, OnDays(MonWedFri));
        var perDay = Started(user, PerDay());
        var perWeek = Started(user, PerWeek());
        var december = await CreateAsync(_world, user, YesNoTimes(1, SchedulePeriod.Day), Tue1Dec);
        return (user, monWedFri, perDay, perWeek, december);
    }

    [Fact]
    public async Task US10_AS4_AS5_a_change_at_noon_in_Kyiv_on_Wednesday_writes_each_goal_from_its_next_day_or_week()
    {
        var (user, monWedFri, perDay, perWeek, december) = await ArrangeKyivGoalsAsync();
        var revision = _world.Goals.RevisionOf(user.AccountId!.Value);

        var result = await ChangeAsync(user, NewYorkId);

        Assert.Equal((TimeZoneChangeOutcome.Changed, (string?)null), (result.Outcome, result.Message));
        Assert.Equal(NewYorkId, StoredZoneOf(user));
        Assert.Equal(NewYorkId, await ReadZoneAsync(user));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], VersionsOf(monWedFri));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], VersionsOf(perDay));
        Assert.Equal([(Mon16, KyivId), (Mon23, NewYorkId)], VersionsOf(perWeek));
        Assert.Equal([(Tue1Dec, NewYorkId)], VersionsOf(december));
        Assert.Equal(revision + 1, _world.Goals.RevisionOf(user.AccountId!.Value));
        Assert.Equal(1, _world.Goals.TimeZoneChangeCount);
    }

    [Fact]
    public async Task US10_AS4_the_Mon_Wed_Fri_calendar_keeps_Kyiv_midnights_through_Wednesday_and_uses_New_York_from_Friday()
    {
        var (user, monWedFri, _, _, _) = await ArrangeKyivGoalsAsync();
        await ChangeAsync(user, NewYorkId);

        var calendar = await CalendarAsync(user, monWedFri, Mon16, Mon23);

        Assert.Equal(
            [
                new Commitment(Mon16, Utc(2026, 11, 15, 22), Utc(2026, 11, 16, 22)),
                new Commitment(Wed18, Utc(2026, 11, 17, 22), Utc(2026, 11, 18, 22)),
                new Commitment(Fri20, Utc(2026, 11, 20, 5), Utc(2026, 11, 21, 5)),
                new Commitment(Mon23, Utc(2026, 11, 23, 5), Utc(2026, 11, 24, 5)),
            ],
            calendar.Commitments);
        Assert.Empty(calendar.Periods);
    }

    [Fact]
    public async Task US10_AS5_the_per_day_goal_uses_New_York_from_Thursday_the_per_week_goal_from_Monday_23_and_December_from_its_start()
    {
        var (user, _, perDay, perWeek, december) = await ArrangeKyivGoalsAsync();
        await ChangeAsync(user, NewYorkId);

        Assert.Equal(
            [
                new EvaluationPeriod(Wed18, Wed18, Utc(2026, 11, 17, 22), Utc(2026, 11, 18, 22)),
                new EvaluationPeriod(Thu19, Thu19, Utc(2026, 11, 19, 5), Utc(2026, 11, 20, 5)),
                new EvaluationPeriod(Fri20, Fri20, Utc(2026, 11, 20, 5), Utc(2026, 11, 21, 5)),
            ],
            (await CalendarAsync(user, perDay, Wed18, Fri20)).Periods);
        Assert.Equal(
            [
                new EvaluationPeriod(Mon16, Date(2026, 11, 22), Utc(2026, 11, 15, 22), Utc(2026, 11, 22, 22)),
                new EvaluationPeriod(Mon23, Sun29, Utc(2026, 11, 23, 5), Utc(2026, 11, 30, 5)),
            ],
            (await CalendarAsync(user, perWeek, Mon16, Sun29)).Periods);
        Assert.Equal(
            [new EvaluationPeriod(Tue1Dec, Tue1Dec, Utc(2026, 12, 1, 5), Utc(2026, 12, 2, 5))],
            (await CalendarAsync(user, december, Tue1Dec, Tue1Dec)).Periods);
    }

    [Fact]
    public async Task US10_AS6_reading_the_Mon_Wed_Fri_goal_gives_Kyiv_in_effect_on_Wednesday_and_New_York_from_Thursday()
    {
        var (user, monWedFri, _, _, _) = await ArrangeKyivGoalsAsync();
        var before = await ReadGoalAsync(user, monWedFri);

        await ChangeAsync(user, NewYorkId);
        var wednesday = await ReadGoalAsync(user, monWedFri);
        _world.Clock.SetUtcNow(Utc(2026, 11, 20, 17));
        var friday = await ReadGoalAsync(user, monWedFri);

        Assert.Equal((KyivId, Mon16, (string?)NewYorkId, (DateOnly?)Thu19),
            (wednesday.TimeZone, wednesday.TimeZoneAppliesFrom, wednesday.LaterTimeZone, wednesday.LaterTimeZoneAppliesFrom));
        Assert.Equal((NewYorkId, Thu19, (string?)null, (DateOnly?)null),
            (friday.TimeZone, friday.TimeZoneAppliesFrom, friday.LaterTimeZone, friday.LaterTimeZoneAppliesFrom));
        foreach (var after in new[] { wednesday, friday })
        {
            Assert.Equal(
                (before.Name, before.TargetText, before.ScheduleText, before.Kind, before.StartDate, before.AttemptNumber, before.AttemptStartDate),
                (after.Name, after.TargetText, after.ScheduleText, after.Kind, after.StartDate, after.AttemptNumber, after.AttemptStartDate));
            Assert.True(after.Days.SetEquals(before.Days));
        }
    }

    [Fact]
    public async Task US10_AS10_a_second_change_at_noon_in_New_York_on_Thursday_replaces_or_adds_each_goals_later_version()
    {
        var (user, monWedFri, perDay, perWeek, december) = await ArrangeKyivGoalsAsync();
        await ChangeAsync(user, NewYorkId);
        _world.Clock.SetUtcNow(NewYorkNoonThursday);

        var result = await ChangeAsync(user, TokyoId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal([(Mon16, KyivId), (Mon23, TokyoId)], VersionsOf(perWeek));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId), (Date(2026, 11, 20), TokyoId)], VersionsOf(perDay));
        Assert.Equal([(Tue1Dec, TokyoId)], VersionsOf(december));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId), (Fri20, TokyoId)], VersionsOf(monWedFri));
        Assert.Equal(TokyoId, StoredZoneOf(user));
    }

    [Fact]
    public async Task Today_is_decided_in_the_new_zone_at_once()
    {
        // 23:30 UTC on Wednesday 18 November: 01:30 on Thursday in Kyiv, 18:30 on Wednesday in New York.
        var world = new GoalWorld(now: Utc(2026, 11, 18, 23, 30));
        var user = Registered(world, KyivId);
        Assert.Equal(Thu19, (await world.Service.ListAsync(user, None)).Today);

        var result = await world.Profiles.ChangeTimeZoneAsync(user, NewYorkId, None);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(Wed18, (await world.Service.ListAsync(user, None)).Today);
        Assert.Equal(Wed18, (await world.Service.StartNewGoalAsync(user, null, null, None)).StartingValues!.StartDate);
    }

    // ---- Who may use it (US10-AS8) -------------------------------------------------------------------------------

    [Fact]
    public async Task US10_AS8_an_administrator_reads_and_changes_their_own_zone()
    {
        var admin = GoalInputs.Administrator(_world.Accounts, KyivId);

        Assert.Equal(KyivId, await ReadZoneAsync(admin));
        var result = await ChangeAsync(admin, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(NewYorkId, await ReadZoneAsync(admin));
    }

    public static TheoryData<string, ActingUser> NotSignedIn => new()
    {
        { "anonymous visitor", ActingUser.Anonymous },
        { "registered user with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser) },
        { "administrator with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator) },
    };

    [Theory]
    [MemberData(nameof(NotSignedIn))]
    public async Task US10_AS8_a_user_who_is_not_signed_in_gets_You_are_not_signed_in_before_any_other_check(string who, ActingUser user)
    {
        Assert.NotNull(who);
        var read = await _world.Profiles.GetTimeZoneAsync(user, None);
        var refusedValue = await ChangeAsync(user, "Mars/Olympus");
        var namedValue = await ChangeAsync(user, NewYorkId);

        Assert.Equal((TimeZoneReadOutcome.NotSignedIn, "You are not signed in.", (string?)null), (read.Outcome, read.Message, read.TimeZone));
        Assert.Equal(nameof(TimeZoneReadOutcome.NotSignedIn), read.Outcome.ToString());
        foreach (var change in new[] { refusedValue, namedValue })
        {
            Assert.Equal((TimeZoneChangeOutcome.NotSignedIn, "You are not signed in."), (change.Outcome, change.Message));
        }

        Assert.Equal(0, _world.Goals.TimeZoneWriteCount);
    }

    // ---- Conflicts (research R-08) -------------------------------------------------------------------------------

    [Fact]
    public async Task A_goal_created_between_the_read_and_the_write_makes_the_service_read_again_and_change_it_too()
    {
        var user = Registered(_world, KyivId);
        var perDay = Started(user, PerDay());
        GoalId? created = null;
        var profiles = _world.ProfilesChangingBeforeFirstTimeZoneWrite(async () =>
            created = await CreateAsync(_world, user, YesNoDays(BuildOrReduce.Build, MonWedFri), Wed18));

        var result = await ChangeAsync(user, NewYorkId, profiles);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(2, _world.Goals.TimeZoneWriteCount);
        Assert.Equal(1, _world.Goals.TimeZoneChangeCount);
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], VersionsOf(perDay));
        Assert.Equal([(Wed18, KyivId), (Thu19, NewYorkId)], VersionsOf(created!.Value));
        Assert.Equal(NewYorkId, StoredZoneOf(user));
    }

    [Fact]
    public async Task Nine_conflicting_writes_are_retried_and_the_tenth_write_changes_the_zone()
    {
        var user = Registered(_world, KyivId);
        var perDay = Started(user, PerDay());
        _world.Goals.ConflictOnNextTimeZoneWrites(ProfileService.MaxTimeZoneWrites - 1);

        var result = await ChangeAsync(user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(10, _world.Goals.TimeZoneWriteCount);
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], VersionsOf(perDay));
    }

    [Fact]
    public async Task Ten_conflicting_writes_throw_a_fault_that_names_no_value_and_change_nothing()
    {
        var user = Registered(_world, KyivId);
        var perDay = Started(user, PerDay());
        _world.Goals.ConflictOnNextTimeZoneWrites(ProfileService.MaxTimeZoneWrites);

        var fault = await Assert.ThrowsAsync<InvalidOperationException>(() => ChangeAsync(user, NewYorkId));

        Assert.Equal(10, _world.Goals.TimeZoneWriteCount);
        Assert.Equal(0, _world.Goals.TimeZoneChangeCount);
        foreach (var value in new[] { KyivId, NewYorkId, "Kyiv", "New_York", "Reading", user.AccountId!.Value.ToString() })
        {
            Assert.DoesNotContain(value, fault.Message, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(KyivId, StoredZoneOf(user));
        Assert.Equal([(Mon16, KyivId)], VersionsOf(perDay));
    }

    // ---- The store's ownership rule (constitution III) -----------------------------------------------------------

    [Fact]
    public async Task The_goal_store_refuses_a_write_that_names_another_accounts_goal_and_writes_nothing()
    {
        var a = Registered(_world, KyivId);
        var b = Registered(_world, KyivId);
        var aGoal = Started(a, PerDay());
        var bGoal = Started(b, PerDay());
        var owner = a.AccountId!.Value;
        var change = GoalVersionChange.Add(Thu19, NewYork);

        var written = await _world.Goals.TryChangeTimeZoneAsync(
            owner, KyivId, 0, NewYork, [new GoalVersionWrite(aGoal, change), new GoalVersionWrite(bGoal, change)], None);
        var unknown = await _world.Goals.TryChangeTimeZoneAsync(
            owner, KyivId, 0, NewYork, [new GoalVersionWrite(new GoalId(Guid.NewGuid()), change)], None);

        Assert.False(written);
        Assert.False(unknown);
        Assert.Equal([(Mon16, KyivId)], VersionsOf(aGoal));
        Assert.Equal([(Mon16, KyivId)], VersionsOf(bGoal));
        Assert.Equal((KyivId, KyivId), (StoredZoneOf(a), StoredZoneOf(b)));
        Assert.Equal(0, _world.Goals.RevisionOf(owner));
    }

    // ---- Slice 8: ended goals and waiting attempts (FR-021; T056) ----

    [Fact]
    public async Task Slice_8_a_time_zone_change_adds_no_version_to_an_ended_goal_and_replaces_a_waiting_attempts_first_version()
    {
        // Both goals start on Monday 2 November in Kyiv; both are ended at 2026-11-16T10:00:00Z (Monday 16 there), and
        // the second gets a waiting attempt 2 from Wednesday 18. The zone changes at 2026-11-17T10:00:00Z.
        var world = new AttemptWorld();
        var user = world.UserIn(KyivId);
        var daily = DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);
        var created = new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
        var ended = await world.CreateGoalAsync(daily, Date(2026, 11, 2), created, user);
        var restarted = await world.CreateGoalAsync(daily, Date(2026, 11, 2), created, user);
        world.At(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));
        await world.EndAsync(ended, user: user);
        await world.EndAsync(restarted, user: user);
        await world.StartAttemptAsync(restarted, daily, Date(2026, 11, 18), user);
        world.At(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));

        var result = await world.Profiles.ChangeTimeZoneAsync(user, "America/New_York", CancellationToken.None);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal([(KyivId, Date(2026, 11, 2))], world.GoalOf(ended).Versions.Select(v => (v.Settings.TimeZone.Id, v.AppliesFrom)));
        var restartedGoal = world.GoalOf(restarted);
        Assert.Equal([("America/New_York", Date(2026, 11, 18))], restartedGoal.Versions.Select(v => (v.Settings.TimeZone.Id, v.AppliesFrom)));
        Assert.Equal([(KyivId, Date(2026, 11, 2))], restartedGoal.VersionsOf(1).Select(v => (v.Settings.TimeZone.Id, v.AppliesFrom)));
    }
}
