using System.Globalization;
using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Pausing a goal (FR-006 to FR-008, US2-AS1 to AS4, AS8, SC-003, SC-004; T054) over the fakes of
/// <see cref="ExemptionWorld"/>. The goal "Drinking water, 8 glasses a day" (Build, count at least 8 "glasses", once per
/// day) starts on Monday 2 November 2026 and is created at 08:00Z that day. The user has no stored time zone, so the goal
/// is in UTC: a day runs from 00:00Z to 00:00Z and its grace period ends 48 hours after it. At 09:00Z on Monday 16
/// November the goal's current date is Monday 16; Sunday 15 ends at 2026-11-16T00:00:00Z and is Pending until
/// 2026-11-18T00:00:00Z; Friday 13 ends at 2026-11-14T00:00:00Z and is final from 2026-11-16T00:00:00Z. Every instant is
/// worked out by hand.
/// </summary>
public sealed class PauseAsyncTests
{
    private const string NotSignedIn = "You are not signed in.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string GoalNotFound = "This goal does not exist.";
    private const string DaysMissing = "Choose the first and the last day.";
    private const string StartsBeforeToday = "A pause cannot start before today.";
    private const string LastBeforeFirst = "The last day cannot be before the first day.";
    private const string Overlaps = "This goal already has a pause on these dates.";
    private const string ReasonTooLong = "Keep the reason to 200 characters or fewer.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sat21 = Date(2026, 11, 21);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Tue24 = Date(2026, 11, 24);
    private static readonly DateOnly Fri27 = Date(2026, 11, 27);

    /// <summary>08:00Z on Monday 2 November 2026, when the goal is created.</summary>
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>09:00Z on Monday 16 November: the goal's current date is Monday 16, so "today" is Monday 16.</summary>
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    private Task<GoalId> GoalAsync(GoalTargetFields? target = null) => _world.CreateGoalAsync(target ?? DrinkingWater, Mon2, Mon2Morning);

    private static void AssertRefused(PauseResult result, PauseOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Pause);
    }

    private static PauseView AssertPaused(PauseResult result, DateOnly firstDay, DateOnly lastDay)
    {
        Assert.Equal(PauseOutcome.Paused, result.Outcome);
        Assert.Equal("paused", result.Message);
        var pause = Assert.IsType<PauseView>(result.Pause);
        Assert.Equal(firstDay, pause.FirstDay);
        Assert.Equal(lastDay, pause.LastDay);
        return pause;
    }

    private void AssertNoPause(GoalId goal) => Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);

    // ---- access (FR-006 steps 1 to 3, as FR-002's) ----

    [Fact]
    public async Task FR_006_1_an_anonymous_visitor_is_refused_with_no_store_call()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, null, ActingUser.Anonymous), PauseOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
        AssertNoPause(goal);
    }

    [Fact]
    public async Task FR_006_2_an_administrator_is_refused_with_no_store_call()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(
            await _world.PauseAsync(goal, Mon16, Sun22, null, _world.Administrator), PauseOutcome.NotForAdministrators, NotForAdministrators);
        Assert.Empty(_world.Records.Calls);
        AssertNoPause(goal);
    }

    [Fact]
    public async Task FR_006_3_an_account_that_is_not_stored_is_refused_as_not_signed_in()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, null, gone), PauseOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
        AssertNoPause(goal);
    }

    [Fact]
    public async Task FR_006_3_another_users_goal_and_no_goal_give_This_goal_does_not_exist()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, null, _world.OtherUser), PauseOutcome.NotFound, GoalNotFound);
        AssertRefused(await _world.PauseAsync(new GoalId(Guid.NewGuid()), Mon16, Sun22), PauseOutcome.NotFound, GoalNotFound);
        Assert.Empty(_world.Records.Calls);
        AssertNoPause(goal);
    }

    // ---- the scenarios ----

    [Fact]
    public async Task US2_AS1_the_goal_is_paused_from_today_and_today_is_Paused_while_yesterday_stays_Pending()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        var result = await _world.PauseAsync(goal, Mon16, Sun22, "Family visit");

        var pause = AssertPaused(result, Mon16, Sun22);
        Assert.Equal(PauseTexts.Paused, result.Message);
        var stored = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(stored.Id, pause.Id);
        Assert.Equal("Family visit", stored.Reason!.Value);
        Assert.Equal("Family visit", pause.Reason);
        Assert.Equal(PauseTiming.Current, pause.Timing);
        Assert.Equal("Mon 16 Nov – Sun 22 Nov", pause.RangeText);

        Assert.Equal(Sun22, Assert.Single((await _world.OverviewAsync()).Goals).PausedUntil);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon16)).State);
        Assert.Equal(EvaluationState.Pending, (await _world.EntryAsync(goal, Sun15)).State);
        var recordable = await _world.RecordableAsync(goal);
        Assert.DoesNotContain(recordable, entry => entry.FirstDate == Mon16);
        Assert.Contains(recordable, entry => entry.FirstDate == Sun15);
    }

    [Fact]
    public async Task US2_AS2_a_pause_from_yesterday_is_refused_and_nothing_changes()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Sun15, Sun22), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        AssertNoPause(goal);
        Assert.Equal(EvaluationState.Pending, (await _world.EntryAsync(goal, Sun15)).State);
    }

    [Fact]
    public async Task US2_AS3_a_last_day_before_the_first_day_is_refused()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Fri20, Thu19), PauseOutcome.LastBeforeFirst, LastBeforeFirst);
        AssertNoPause(goal);
    }

    [Fact]
    public async Task US2_AS4_a_pause_sharing_dates_with_another_is_refused()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22, "Family visit"), Mon16, Sun22);

        AssertRefused(await _world.PauseAsync(goal, Sat21, Tue24), PauseOutcome.Overlaps, Overlaps);
        Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task FR_006_a_touching_pause_is_accepted()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);

        var later = AssertPaused(await _world.PauseAsync(goal, Mon23, Fri27), Mon23, Fri27);

        Assert.Equal(PauseTiming.Upcoming, later.Timing);
        Assert.Equal(2, _world.Records.ExemptionsOfGoal(goal).Pauses.Count);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task FR_006_4_a_missing_day_gives_Choose_the_first_and_the_last_day(bool hasFirst, bool hasLast)
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(
            await _world.PauseAsync(goal, hasFirst ? Mon16 : null, hasLast ? Sun22 : null), PauseOutcome.DaysMissing, DaysMissing);
        AssertNoPause(goal);
    }

    [Fact]
    public async Task FR_006_8_a_reason_of_201_characters_is_refused_and_200_are_accepted()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, new string('a', 201)), PauseOutcome.ReasonTooLong, ReasonTooLong);
        AssertNoPause(goal);

        var pause = AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22, new string('a', 200)), Mon16, Sun22);
        Assert.Equal(200, pause.Reason!.Length);
    }

    [Fact]
    public async Task FR_006_a_reason_of_spaces_stores_none_and_a_reason_is_trimmed()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        Assert.Null(AssertPaused(await _world.PauseAsync(goal, Mon16, Tue17, "   "), Mon16, Tue17).Reason);
        Assert.Equal("Trip", AssertPaused(await _world.PauseAsync(goal, Wed18, Thu19, "  Trip  "), Wed18, Thu19).Reason);

        var stored = _world.Records.ExemptionsOfGoal(goal).Pauses;
        Assert.Null(stored.Single(p => p.FirstDay == Mon16).Reason);
        Assert.Equal("Trip", stored.Single(p => p.FirstDay == Wed18).Reason!.Value);
    }

    // ---- the order of FR-006's steps ----

    [Fact]
    public async Task FR_006_order_a_missing_day_with_a_first_day_before_today_gives_step_4()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Sun15, null), PauseOutcome.DaysMissing, DaysMissing);
    }

    [Fact]
    public async Task FR_006_order_a_first_day_before_today_with_a_reversed_range_gives_step_5()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Sun15, Sat14), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
    }

    [Fact]
    public async Task FR_006_order_a_reversed_range_that_overlaps_gives_step_6()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon16, Sun22);
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Fri20, Thu19), PauseOutcome.LastBeforeFirst, LastBeforeFirst);
    }

    [Fact]
    public async Task FR_006_order_an_overlap_with_a_reason_too_long_gives_step_7()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon16, Sun22);
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Sat21, Tue24, new string('a', 201)), PauseOutcome.Overlaps, Overlaps);
        Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    // ---- the pause date (research R-03, SC-003) ----

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public async Task SC_003_a_first_day_of_today_minus_1_is_refused_and_of_today_or_today_plus_1_is_accepted(int days, bool accepted)
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        var first = Mon16.AddDays(days);

        var result = await _world.PauseAsync(goal, first, Sun22);

        if (accepted)
        {
            AssertPaused(result, first, Sun22);
        }
        else
        {
            AssertRefused(result, PauseOutcome.StartsBeforeToday, StartsBeforeToday);
            AssertNoPause(goal);
        }
    }

    [Fact]
    public async Task R_03_a_goal_that_has_not_started_uses_the_accounts_today_in_its_stored_zone()
    {
        // The account is in Kyiv (UTC+2 in November). At 23:00Z on Monday 16 November it is 01:00 on Tuesday 17 in Kyiv,
        // so the account's today is Tuesday 17, while in UTC it is still Monday 16. The goal starts on Wednesday 18.
        var user = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(DrinkingWater, Wed18, Utc("2026-11-16T23:00:00Z"), user);

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, null, user), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        var pause = AssertPaused(await _world.PauseAsync(goal, Tue17, Sun22, null, user), Tue17, Sun22);
        Assert.Equal(PauseTiming.Current, pause.Timing);
    }

    [Fact]
    public async Task R_03_a_goal_starting_tomorrow_can_be_paused_from_today_but_not_from_yesterday()
    {
        var goal = await _world.CreateGoalAsync(DrinkingWater, Tue17, Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, Sun15, Sun22), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);
    }

    [Fact]
    public async Task R_03_a_goal_whose_zone_differs_from_the_accounts_uses_the_goals_current_date()
    {
        // The account is in Kyiv, the goal's only version in UTC. At 23:00Z on Monday 16 November the goal's current date
        // is Monday 16, while the account's today is Tuesday 17; the goal's date decides.
        var user = _world.UserIn("Europe/Kyiv");
        var goal = _world.AddGoal(user, Target(DrinkingWater), (Mon2, Zone("UTC")));
        _world.At(Utc("2026-11-16T23:00:00Z"));

        AssertRefused(await _world.PauseAsync(goal, Sun15, Sun22, null, user), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22, null, user), Mon16, Sun22);
    }

    [Fact]
    public async Task R_03_a_goal_in_a_zone_ahead_of_the_accounts_refuses_the_accounts_today()
    {
        // The account is in UTC, the goal's only version in Kyiv. At 23:00Z on Monday 16 November the goal's current date
        // is Tuesday 17 (01:00 in Kyiv), so a pause from Monday 16, the account's today, would start before today.
        var goal = _world.AddGoal(_world.User, Target(DrinkingWater), (Mon2, Kyiv));
        _world.At(Utc("2026-11-16T23:00:00Z"));

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        AssertPaused(await _world.PauseAsync(goal, Tue17, Sun22), Tue17, Sun22);
    }

    // ---- what a pause leaves unchanged (FR-008, SC-004) ----

    [Fact]
    public async Task US2_AS8_a_Failed_day_before_the_pause_stays_Failed()
    {
        var goal = await GoalAsync();

        // 20:00Z on Friday 13 November: 6 glasses, below the target; Friday 13 is final from 2026-11-16T00:00:00Z.
        _world.At(Utc("2026-11-13T20:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Fri13, Amount(6))).Outcome);
        _world.At(Mon16NineOClock);
        Assert.Equal(EvaluationState.Failed, (await _world.EntryAsync(goal, Fri13)).State);

        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);

        Assert.Equal(EvaluationState.Failed, (await _world.EntryAsync(goal, Fri13)).State);
    }

    [Theory]
    [InlineData("2026-11-11", EvaluationState.Missed)]
    [InlineData("2026-11-12", EvaluationState.Failed)]
    [InlineData("2026-11-13", EvaluationState.Met)]
    [InlineData("2026-11-15", EvaluationState.Pending)]
    public async Task SC_004_every_Pending_and_final_result_before_the_pause_keeps_its_state(string date, EvaluationState expected)
    {
        // Wednesday 11 has no record (Missed from 2026-11-14T00:00:00Z); Thursday 12 has 6 glasses (Failed from
        // 2026-11-15T00:00:00Z); Friday 13 has 8 (Met); Sunday 15 has no record and is Pending until 2026-11-18T00:00:00Z.
        var goal = await GoalAsync();
        _world.SeedAmount(goal, Thu12, 6);
        _world.SeedAmount(goal, Fri13, 8);
        _world.At(Mon16NineOClock);
        var day = DateOnly.Parse(date, CultureInfo.InvariantCulture);
        Assert.Equal(expected, (await _world.EntryAsync(goal, day)).State);

        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);

        Assert.Equal(expected, (await _world.EntryAsync(goal, day)).State);
    }

    [Fact]
    public async Task FR_008_a_Due_day_over_a_Reduce_limit_becomes_Paused_and_keeps_its_record()
    {
        // "Coffee, at most 2 cups a day" (Reduce). 08:00Z on Monday 16: 5 cups, over the limit, but the day is Due, not final.
        var goal = await GoalAsync(CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        _world.At(Utc("2026-11-16T08:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Mon16, Amount(5))).Outcome);
        Assert.Equal(EvaluationState.Due, (await _world.EntryAsync(goal, Mon16)).State);
        _world.At(Mon16NineOClock);

        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);

        var entry = await _world.EntryAsync(goal, Mon16);
        Assert.Equal(EvaluationState.Paused, entry.State);
        Assert.Single(entry.Records);
        Assert.Single(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task AC_088_a_pause_of_several_months_is_accepted_and_leaves_the_streak_before_it_unchanged()
    {
        // 8 glasses on every day from Monday 2 to Sunday 15 November. At 09:00Z on Monday 16, Monday 2 to Friday 13 are
        // final (a streak of 12 days); Saturday 14 and Sunday 15 are still Pending (grace ends 2026-11-17T00:00:00Z and
        // 2026-11-18T00:00:00Z) and become Met later, so the streak before the pause is 14 days once they are final.
        var goal = await GoalAsync();
        for (var day = Mon2; day <= Sun15; day = day.AddDays(1))
        {
            _world.SeedAmount(goal, day, 8);
        }

        _world.At(Mon16NineOClock);
        var before = await _world.ReportAsync(goal);
        Assert.Equal(12, before.Stats.CurrentStreak);

        var last = Date(2027, 2, 28);
        AssertPaused(await _world.PauseAsync(goal, Mon16, last, "Long trip"), Mon16, last);

        // 10:00Z on Monday 1 March 2027: the pause has ended; 1 March is Due, so nothing after the pause is counted yet.
        _world.At(Utc("2027-03-01T10:00:00Z"));
        var after = await _world.ReportAsync(goal);
        Assert.Equal(14, after.Stats.CurrentStreak);
        Assert.Equal(14, after.Stats.LongestStreak);
        Assert.Equal("100% (14 of 14)", after.Stats.ConsistencyText);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Date(2027, 1, 15))).State);
    }

    // ---- the store ----

    [Fact]
    public async Task R_04_a_concurrent_overlapping_pause_gives_This_goal_already_has_a_pause_on_these_dates()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedPause(goal, Wed18, Fri20, "First");

        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, "Second"), PauseOutcome.Overlaps, Overlaps);
        var stored = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal("First", stored.Reason!.Value);
    }

    [Fact]
    public async Task The_pause_is_written_once_for_the_owner_and_the_goal_after_the_read()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);

        AssertPaused(await _world.PauseAsync(goal, Mon16, Sun22), Mon16, Sun22);

        Assert.Equal(["ReadExemptionsAsync", "AddPauseAsync"], _world.Records.Calls);
    }

    [Fact]
    public async Task No_refusal_writes_to_the_store()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon23, Fri27);
        _world.At(Mon16NineOClock);

        AssertRefused(await _world.PauseAsync(goal, null, Sun22), PauseOutcome.DaysMissing, DaysMissing);
        AssertRefused(await _world.PauseAsync(goal, Sun15, Sun22), PauseOutcome.StartsBeforeToday, StartsBeforeToday);
        AssertRefused(await _world.PauseAsync(goal, Sun22, Mon16), PauseOutcome.LastBeforeFirst, LastBeforeFirst);
        AssertRefused(await _world.PauseAsync(goal, Sun22, Mon23), PauseOutcome.Overlaps, Overlaps);
        AssertRefused(await _world.PauseAsync(goal, Mon16, Sun22, new string('a', 201)), PauseOutcome.ReasonTooLong, ReasonTooLong);

        Assert.DoesNotContain("AddPauseAsync", _world.Records.Calls);
        Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }
}
