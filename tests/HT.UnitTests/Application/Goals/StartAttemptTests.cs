using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T084; US3-AS1, AS2, AS4 to AS8, FR-011, FR-012, AC-064): <see cref="GoalChangeService.GetNewAttemptValuesAsync"/>
/// and <see cref="GoalChangeService.StartAttemptAsync"/> on <see cref="AttemptWorld"/>. The user has no stored zone, so
/// everything is in UTC. The spec's "Meditation, 10 min daily" is "Reading, 10 min daily" here (the world's catalog habit
/// is "Reading"): Build, 10 minutes, all seven days, created on Sunday 1 November at 10:00Z with the start date Monday 2
/// November, 10 minutes on each date from 2 to 13 November, ended on Monday 16 November at 10:00Z with "Abandoned" (ending
/// date Monday 16, last date Sunday 15 November). Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class StartAttemptTests
{
    private const string NotEnded = "Only an ended goal can start a new attempt.";
    private const string StartDateMissing = "Choose a start date.";
    private const string StartDateInPast = "The start date cannot be in the past.";
    private const string StartDateNotAfterEnding = "A new attempt starts after the day the goal ended.";
    private const string StartDateTooFar = "Choose a start date within a year from today.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");

    /// <summary>Monday 16 November at 11:00Z, the day of the ending.</summary>
    private static readonly DateTimeOffset EndingDayAt11 = Utc("2026-11-16T11:00:00Z");

    /// <summary>Wednesday 18 November at 08:00Z.</summary>
    private static readonly DateTimeOffset Wed18At8 = Utc("2026-11-18T08:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static NewAttemptInput Input(GoalTargetFields target, DateOnly? start) => new() { Target = target, StartDate = start };

    private Task<AttemptStartResult> StartAsync(GoalId goal, NewAttemptInput input, ActingUser? user = null) =>
        _world.Changes.StartAttemptAsync(user ?? _world.User, goal, input, None);

    private Task<NewAttemptValuesResult> ValuesAsync(GoalId goal, ActingUser? user = null) =>
        _world.Changes.GetNewAttemptValuesAsync(user ?? _world.User, goal, None);

    /// <summary>"Reading, 10 min daily" with 10 minutes on 2 to 13 November, ended at <see cref="EndedAt"/>; the clock stays there.</summary>
    private async Task<GoalId> EndedGoalAsync()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        foreach (var day in Enumerable.Range(0, 12))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 10);
        }

        _world.At(EndedAt);
        Assert.Equal(GoalEndOutcome.Ended, (await _world.Changes.EndAsync(_world.User, goal, Outcome.Abandoned, "Too busy this month.", None)).Outcome);
        return goal;
    }

    private static void AssertRefused(AttemptStartResult result, AttemptStartOutcome outcome, string message, GoalField? field)
    {
        Assert.Equal((outcome, message, field), (result.Outcome, result.Message, result.Field));
        Assert.Null(result.AttemptNumber);
        Assert.Null(result.StartDate);
    }

    private async Task<AttemptView> AttemptAsync(GoalId goal, int number)
    {
        var result = await _world.Attempts.GetAttemptsAsync(_world.User, goal, None);
        Assert.Equal(AttemptListOutcome.Found, result.Outcome);
        return result.Attempts.Single(attempt => attempt.Number == number);
    }

    [Fact]
    public async Task US3_AS1_the_starting_values_are_the_ended_attempts_last_settings_from_18_November()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        var result = await ValuesAsync(goal);

        Assert.Equal((NewAttemptValuesOutcome.Found, (string?)null), (result.Outcome, result.Message));
        var values = result.Values!;
        Assert.Equal(
            (BuildOrReduce.Build, Measurement.Duration, (int?)10, (DurationUnit?)DurationUnit.Minutes, (string?)null, ScheduleKind.FixedDays, (int?)null, (SchedulePeriod?)null),
            (values.BuildOrReduce, values.Measurement, values.Amount, values.DurationUnit, values.CountUnit, values.ScheduleKind, values.Times, values.Period));
        Assert.Equal(AllDays.ToHashSet(), values.Days.ToHashSet());
        Assert.Equal((Wed18, 2), (values.StartDate, values.AttemptNumber));
    }

    [Fact]
    public async Task US3_AS1_on_the_day_of_the_ending_the_start_date_is_the_day_after_the_ending_date()
    {
        var goal = await EndedGoalAsync();
        _world.At(EndingDayAt11);

        var result = await ValuesAsync(goal);

        Assert.Equal((NewAttemptValuesOutcome.Found, Tue17), (result.Outcome, result.Values!.StartDate));
    }

    [Fact]
    public async Task FR_011_the_starting_values_come_from_the_last_version_not_the_first()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.At(Utc("2026-11-05T10:00:00Z"));
        Assert.Equal(Date(2026, 11, 6), await _world.ChangeSettingsAsync(goal, MinutesDaily(20)));
        _world.At(EndedAt);
        await _world.EndAsync(goal);

        var result = await ValuesAsync(goal);

        Assert.Equal(20, result.Values!.Amount);
    }

    [Fact]
    public async Task US3_AS2_starting_with_the_values_unchanged_gives_attempt_2_from_18_November_with_NA_and_no_streak()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        var values = (await ValuesAsync(goal)).Values!;
        var target = new GoalTargetFields
        {
            BuildOrReduce = values.BuildOrReduce,
            Measurement = values.Measurement,
            Amount = values.Amount,
            CountUnit = values.CountUnit,
            DurationUnit = values.DurationUnit,
            ScheduleKind = values.ScheduleKind,
            Days = values.Days,
            Times = values.Times,
            Period = values.Period,
        };

        var result = await StartAsync(goal, Input(target, values.StartDate));

        Assert.Equal((AttemptStartOutcome.Started, "attempt started", (int?)2, (DateOnly?)Wed18, (GoalField?)null),
            (result.Outcome, result.Message, result.AttemptNumber, result.StartDate, result.Field));
        var stored = _world.GoalOf(goal);
        Assert.False(stored.IsEnded);
        Assert.Equal((2, Wed18), (stored.Attempt.Number, stored.Attempt.StartDate));
        Assert.Equal(Wed18, stored.Versions[0].AppliesFrom);
        Assert.Equal("UTC", stored.Versions[0].Settings.TimeZone.Id);
        var details = await _world.DetailsAsync(goal);
        Assert.Equal((false, 2, Wed18, "Reading, 10 min daily", Mon2), (details.IsEnded, details.AttemptNumber, details.AttemptStartDate, details.Name, details.StartDate));
        var report = await _world.ReportAsync(goal);
        Assert.Equal(("N/A", "0 commitments"), (report.Stats.ConsistencyText, report.Stats.CurrentStreakText));
    }

    [Fact]
    public async Task US3_AS4_new_settings_from_23_November_name_the_goal_at_once_and_attempt_1_keeps_its_own_name_and_dates()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        var result = await StartAsync(goal, Input(MinutesDaily(5), Mon23));

        Assert.Equal((AttemptStartOutcome.Started, (int?)2, (DateOnly?)Mon23), (result.Outcome, result.AttemptNumber, result.StartDate));
        var details = await _world.DetailsAsync(goal);
        Assert.Equal(("Reading, 5 min daily", false, (int?)5), (details.Name, details.IsEnded, details.Amount));
        var item = Assert.Single(await _world.ListAsync());
        Assert.Equal(Mon23, item.NextCommitment);
        Assert.Equal("Reading, 10 min daily", (await AttemptAsync(goal, 1)).Name);

        _world.At(Utc("2026-11-19T10:00:00Z"));
        Assert.Equal(Thu19, (await _world.RecordableAsync(goal)).CurrentDate);
        var recorded = await _world.RecordAsync(goal, Sun15, new RecordInput { Amount = 10 });
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        Assert.Equal("92% (13 of 14)", (await AttemptAsync(goal, 1)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task US3_AS5_a_new_attempt_can_change_the_schedule_kind_and_counts_its_streak_in_weeks()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        var result = await StartAsync(goal, Input(DurationTimes(10, DurationUnit.Minutes, 3, SchedulePeriod.Week), Wed18));

        Assert.Equal(AttemptStartOutcome.Started, result.Outcome);
        var report = await _world.ReportAsync(goal);
        Assert.Equal((StreakUnit.Weeks, "0 weeks"), (report.Stats.Unit, report.Stats.CurrentStreakText));
        Assert.Equal(StreakUnit.Commitments, (await AttemptAsync(goal, 1)).Stats.Unit);
    }

    [Fact]
    public async Task US3_AS6_a_start_date_in_the_past_is_refused_and_nothing_changes()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), Tue17)), AttemptStartOutcome.StartDateInPast, StartDateInPast, GoalField.StartDate);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.True(_world.GoalOf(goal).IsEnded);
        Assert.Single(_world.GoalOf(goal).Attempts);
    }

    [Fact]
    public async Task US3_AS6_a_start_date_more_than_365_days_ahead_is_refused_and_nothing_changes()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), Date(2027, 11, 20))), AttemptStartOutcome.StartDateTooFar, StartDateTooFar, GoalField.StartDate);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Single(_world.GoalOf(goal).Attempts);
    }

    [Fact]
    public async Task US3_AS6_the_ending_date_on_the_day_of_the_ending_is_refused_and_nothing_changes()
    {
        var goal = await EndedGoalAsync();
        _world.At(EndingDayAt11);
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), Mon16)), AttemptStartOutcome.StartDateNotAfterEnding, StartDateNotAfterEnding, GoalField.StartDate);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Single(_world.GoalOf(goal).Attempts);
    }

    [Fact]
    public async Task FR_012_no_start_date_gives_Choose_a_start_date()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), null)), AttemptStartOutcome.StartDateMissing, StartDateMissing, GoalField.StartDate);
    }

    [Fact]
    public async Task FR_012_a_date_before_the_account_today_is_in_the_past_even_when_it_is_also_on_or_before_the_ending_date()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), Mon16)), AttemptStartOutcome.StartDateInPast, StartDateInPast, GoalField.StartDate);
    }

    [Fact]
    public async Task FR_012_the_target_is_checked_before_the_start_date()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(0), null)), AttemptStartOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount);
    }

    public static TheoryData<string, AttemptStartOutcome, string, GoalField> TargetRefusals => new()
    {
        { "no side", AttemptStartOutcome.BuildOrReduceMissing, "Choose Build or Reduce.", GoalField.BuildOrReduce },
        { "no measurement", AttemptStartOutcome.MeasurementMissing, "Choose how it is measured.", GoalField.Measurement },
        { "yes/no with amount", AttemptStartOutcome.YesNoHasAmountOrUnit, "A yes/no goal has no amount or unit.", GoalField.Measurement },
        { "build amount 0", AttemptStartOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount },
        { "reduce amount 1000", AttemptStartOutcome.ReduceAmountOutOfRange, "Enter a whole number from 0 to 999.", GoalField.Amount },
        { "no count unit", AttemptStartOutcome.CountUnitMissing, "Enter a unit, for example pages.", GoalField.Unit },
        { "long count unit", AttemptStartOutcome.CountUnitTooLong, "The unit can have at most 20 characters.", GoalField.Unit },
        { "no duration unit", AttemptStartOutcome.DurationUnitMissing, "Choose minutes or hours.", GoalField.Unit },
        { "no schedule", AttemptStartOutcome.ScheduleMissing, "Choose fixed days or how many times.", GoalField.Schedule },
        { "no days", AttemptStartOutcome.DaysMissing, "Choose at least one day.", GoalField.Days },
        { "times 0", AttemptStartOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20.", GoalField.Times },
        { "no period", AttemptStartOutcome.PeriodMissing, "Choose per day or per week.", GoalField.Period },
        { "reduce twice a day", AttemptStartOutcome.ReduceAmountNeedsOneTime, "A Reduce goal with an amount has 1 time per period.", GoalField.Times },
    };

    private static GoalTargetFields FieldsOf(string refusal) => refusal switch
    {
        "no side" => MinutesDaily(10) with { BuildOrReduce = null },
        "no measurement" => MinutesDaily(10) with { Measurement = null },
        "yes/no with amount" => MinutesDaily(10) with { Measurement = Measurement.YesNo },
        "build amount 0" => MinutesDaily(0),
        "reduce amount 1000" => DurationDays(1000, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays),
        "no count unit" => CountDays(10, null!, BuildOrReduce.Build, AllDays),
        "long count unit" => CountDays(10, new string('p', 21), BuildOrReduce.Build, AllDays),
        "no duration unit" => MinutesDaily(10) with { DurationUnit = null },
        "no schedule" => MinutesDaily(10) with { ScheduleKind = null },
        "no days" => MinutesDaily(10) with { Days = new HashSet<DayOfWeek>() },
        "times 0" => DurationTimes(10, DurationUnit.Minutes, 0, SchedulePeriod.Day),
        "no period" => DurationTimes(10, DurationUnit.Minutes, 1, SchedulePeriod.Day) with { Period = null },
        "reduce twice a day" => DurationTimes(10, DurationUnit.Minutes, 2, SchedulePeriod.Day, BuildOrReduce.Reduce),
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, null),
    };

    [Theory]
    [MemberData(nameof(TargetRefusals))]
    public async Task US3_AS6_each_target_refusal_has_its_text_and_field_and_writes_nothing(
        string refusal, AttemptStartOutcome outcome, string message, GoalField field)
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await StartAsync(goal, Input(FieldsOf(refusal), Wed18)), outcome, message, field);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Single(_world.GoalOf(goal).Attempts);
    }

    [Fact]
    public async Task US3_AS7_a_goal_that_is_not_ended_gives_Only_an_ended_goal_for_both_use_cases()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.At(Wed18At8);
        var writes = _world.Goals.ChangeWriteCount;

        var values = await ValuesAsync(goal);
        var start = await StartAsync(goal, Input(MinutesDaily(0), null));

        Assert.Equal((NewAttemptValuesOutcome.NotEnded, NotEnded), (values.Outcome, values.Message));
        Assert.Null(values.Values);
        AssertRefused(start, AttemptStartOutcome.NotEnded, NotEnded, null);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
    }

    [Fact]
    public async Task US3_AS7_a_goal_with_a_running_second_attempt_is_not_ended_either()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, Input(MinutesDaily(10), Wed18))).Outcome);

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(10), Thu19)), AttemptStartOutcome.NotEnded, NotEnded, null);
        Assert.Equal(NewAttemptValuesOutcome.NotEnded, (await ValuesAsync(goal)).Outcome);
        Assert.Equal(2, _world.GoalOf(goal).Attempts.Count);
    }

    [Fact]
    public async Task US3_AS8_a_record_between_the_attempts_gives_Nothing_is_scheduled_on_this_date()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, Input(MinutesDaily(5), Mon23))).Outcome);
        _world.At(Utc("2026-11-24T10:00:00Z"));

        var result = await _world.RecordAsync(goal, Fri20, new RecordInput { Amount = 5 });

        Assert.Equal((RecordWriteOutcome.NothingScheduled, "Nothing is scheduled on this date."), (result.Outcome, result.Message));
    }

    [Fact]
    public async Task AC_064_a_goal_whose_catalog_habit_is_Unpublished_can_start_a_new_attempt()
    {
        var goal = await EndedGoalAsync();
        Assert.True(await _world.CatalogHabits.TrySetStateAsync(_world.Habit.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, None));
        _world.At(Wed18At8);

        var values = await ValuesAsync(goal);
        var result = await StartAsync(goal, Input(MinutesDaily(10), Wed18));

        Assert.Equal(NewAttemptValuesOutcome.Found, values.Outcome);
        Assert.Equal((AttemptStartOutcome.Started, (int?)2), (result.Outcome, result.AttemptNumber));
        var stored = _world.GoalOf(goal);
        Assert.Equal((2, _world.Habit.Id), (stored.Attempt.Number, stored.Habit.Id));
    }

    [Fact]
    public async Task FR_012_the_goal_keeps_its_own_name_through_a_new_attempt()
    {
        var goal = await _world.CreateNamedGoalAsync("Calm mornings", MinutesDaily(10), Mon2, Created);
        _world.At(EndedAt);
        await _world.EndAsync(goal);
        _world.At(Wed18At8);

        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, Input(YesNoTimes(3, SchedulePeriod.Week), Mon23))).Outcome);

        Assert.Equal("Calm mornings", (await _world.DetailsAsync(goal)).Name);
        Assert.Equal("Calm mornings", (await AttemptAsync(goal, 1)).Name);
    }

    [Fact]
    public async Task FR_012_the_new_attempts_first_version_takes_the_accounts_current_zone()
    {
        var user = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created, user);
        _world.At(EndedAt);
        await _world.EndAsync(goal, user: user);
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await _world.Profiles.ChangeTimeZoneAsync(user, "Asia/Tokyo", None)).Outcome);
        _world.At(Wed18At8);

        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, Input(MinutesDaily(10), Thu19), user)).Outcome);

        Assert.Equal("Asia/Tokyo", _world.GoalOf(goal).Versions[0].Settings.TimeZone.Id);
    }

    [Fact]
    public async Task FR_023_anonymous_visitors_and_administrators_are_refused_before_the_goal_and_another_users_goal_is_not_found()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        var input = Input(MinutesDaily(10), Wed18);
        var anonymous = ActingUser.Anonymous;

        AssertRefused(await StartAsync(goal, input, anonymous), AttemptStartOutcome.NotSignedIn, "You are not signed in.", null);
        AssertRefused(await StartAsync(goal, input, _world.Administrator), AttemptStartOutcome.NotForAdministrators, "Goals are for registered users.", null);
        AssertRefused(await StartAsync(goal, input, _world.OtherUser), AttemptStartOutcome.NotFound, "This goal does not exist.", null);
        Assert.Equal((NewAttemptValuesOutcome.NotSignedIn, "You are not signed in."), Pair(await ValuesAsync(goal, anonymous)));
        Assert.Equal((NewAttemptValuesOutcome.NotForAdministrators, "Goals are for registered users."), Pair(await ValuesAsync(goal, _world.Administrator)));
        Assert.Equal((NewAttemptValuesOutcome.NotFound, "This goal does not exist."), Pair(await ValuesAsync(goal, _world.OtherUser)));
        Assert.Single(_world.GoalOf(goal).Attempts);

        static (NewAttemptValuesOutcome, string?) Pair(NewAttemptValuesResult result)
        {
            Assert.Null(result.Values);
            return (result.Outcome, result.Message);
        }
    }

    [Fact]
    public async Task Research_R_05_a_start_racing_another_start_reads_again_and_gives_Only_an_ended_goal()
    {
        var goal = await EndedGoalAsync();
        _world.At(Wed18At8);
        _world.Goals.BeforeNextWrite = () => _world.StartAttemptAsync(goal, MinutesDaily(10), Wed18).GetAwaiter().GetResult();

        AssertRefused(await StartAsync(goal, Input(MinutesDaily(5), Thu19)), AttemptStartOutcome.NotEnded, NotEnded, null);

        var stored = _world.GoalOf(goal);
        Assert.Equal(2, stored.Attempts.Count);
        Assert.Equal((Wed18, (int?)10), (stored.Attempt.StartDate, stored.Versions[0].Settings.Target.Amount));
    }
}
