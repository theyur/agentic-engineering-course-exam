using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T062; US1-AS1, AS2, AS6 to AS14, FR-001 to FR-004, AC-021, AC-100): <see cref="GoalChangeService.ChangeSettingsAsync"/>
/// on <see cref="AttemptWorld"/>. The user has no stored zone, so everything is in UTC unless a test moves the account.
/// "Reading, 15 min daily" (Build, 15 minutes, every day) is created on Sunday 1 November at 10:00Z with the start date
/// Monday 2 November; a change on Wednesday 11 November at 10:00Z applies from Thursday 12 November. Every instant is a
/// UTC literal worked out by hand.
/// </summary>
public sealed class ChangeSettingsTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset ChangedAt = Utc("2026-11-11T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalSettingsInput Input(GoalTargetFields target, string? name = null) => new() { Target = target, Name = name };

    private Task<SettingsChangeResult> ChangeAsync(GoalId goal, GoalSettingsInput input, ActingUser? user = null) =>
        _world.Changes.ChangeSettingsAsync(user ?? _world.User, goal, input, None);

    private async Task<GoalId> ReadingAsync()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        foreach (var day in Enumerable.Range(0, 14))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 15);
        }

        return goal;
    }

    private static void AssertRefused(SettingsChangeResult result, SettingsChangeOutcome outcome, string message, GoalField? field)
    {
        Assert.Equal((outcome, message, field), (result.Outcome, result.Message, result.Field));
        Assert.Null(result.AppliesFrom);
    }

    [Fact]
    public async Task US1_AS1_AS2_a_change_applies_from_the_next_day_and_the_name_follows_it_at_midnight()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);

        var result = await ChangeAsync(goal, Input(MinutesDaily(20)));

        Assert.Equal((SettingsChangeOutcome.Changed, "settings changed", (DateOnly?)Thu12, (GoalField?)null), (result.Outcome, result.Message, result.AppliesFrom, result.Field));
        var now = await _world.DetailsAsync(goal);
        Assert.Equal(("Reading, 15 min daily", (int?)15, Mon2), (now.Name, now.Amount, now.TimeZoneAppliesFrom));
        Assert.Equal(((int?)20, Thu12), (now.Later!.Amount, now.Later.AppliesFrom));

        _world.At(Utc("2026-11-12T00:00:00Z"));
        var next = await _world.DetailsAsync(goal);
        Assert.Equal(("Reading, 20 min daily", (int?)20, Thu12), (next.Name, next.Amount, next.TimeZoneAppliesFrom));
        Assert.Null(next.Later);
    }

    [Fact]
    public async Task US1_AS6_a_second_change_before_the_boundary_replaces_the_waiting_version()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        Assert.Equal(SettingsChangeOutcome.Changed, (await ChangeAsync(goal, Input(MinutesDaily(20)))).Outcome);

        _world.At(Utc("2026-11-11T11:00:00Z"));
        var result = await ChangeAsync(goal, Input(MinutesDaily(25)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        var versions = _world.GoalOf(goal).Versions;
        Assert.Equal([Mon2, Thu12], versions.Select(v => v.AppliesFrom));
        Assert.Equal([15, 25], versions.Select(v => v.Settings.Target.Amount));
    }

    [Fact]
    public async Task US1_AS7_a_per_week_change_applies_from_the_next_Monday_and_the_current_week_keeps_its_target()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Created);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var result = await ChangeAsync(goal, Input(YesNoTimes(4, SchedulePeriod.Week)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Mon23), (result.Outcome, result.AppliesFrom));
        var details = await _world.DetailsAsync(goal);
        Assert.Equal(((int?)3, Mon2), (details.Times, details.TimeZoneAppliesFrom));
        Assert.Equal(((int?)4, Mon23), (details.Later!.Times, details.Later.AppliesFrom));
        Assert.Equal(3, GoalTimeline.VersionOf(_world.GoalOf(goal), Mon16).Settings.Target.Schedule.Times);
        Assert.Equal(3, GoalTimeline.VersionOf(_world.GoalOf(goal), Date(2026, 11, 22)).Settings.Target.Schedule.Times);
        Assert.Equal(4, GoalTimeline.VersionOf(_world.GoalOf(goal), Mon23).Settings.Target.Schedule.Times);
    }

    [Fact]
    public async Task US1_AS8_another_schedule_kind_is_refused_and_nothing_is_written()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        var writes = _world.Goals.ChangeWriteCount;

        var result = await ChangeAsync(goal, Input(DurationTimes(15, DurationUnit.Minutes, 5, SchedulePeriod.Week)));

        AssertRefused(result, SettingsChangeOutcome.ScheduleKindChanged,
            "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it.",
            GoalField.Schedule);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Single(_world.GoalOf(goal).Versions);
    }

    [Fact]
    public async Task US1_AS8_times_per_day_and_times_per_week_are_different_schedule_kinds()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(1, SchedulePeriod.Day), Mon2, Created);
        _world.At(ChangedAt);

        var result = await ChangeAsync(goal, Input(YesNoTimes(1, SchedulePeriod.Week)));

        Assert.Equal(SettingsChangeOutcome.ScheduleKindChanged, result.Outcome);
    }

    public static TheoryData<string, SettingsChangeOutcome, string, GoalField> TargetRefusals => new()
    {
        { "no side", SettingsChangeOutcome.BuildOrReduceMissing, "Choose Build or Reduce.", GoalField.BuildOrReduce },
        { "no measurement", SettingsChangeOutcome.MeasurementMissing, "Choose how it is measured.", GoalField.Measurement },
        { "yes/no with amount", SettingsChangeOutcome.YesNoHasAmountOrUnit, "A yes/no goal has no amount or unit.", GoalField.Measurement },
        { "build amount 0", SettingsChangeOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount },
        { "reduce amount 1000", SettingsChangeOutcome.ReduceAmountOutOfRange, "Enter a whole number from 0 to 999.", GoalField.Amount },
        { "no count unit", SettingsChangeOutcome.CountUnitMissing, "Enter a unit, for example pages.", GoalField.Unit },
        { "long count unit", SettingsChangeOutcome.CountUnitTooLong, "The unit can have at most 20 characters.", GoalField.Unit },
        { "no duration unit", SettingsChangeOutcome.DurationUnitMissing, "Choose minutes or hours.", GoalField.Unit },
        { "no schedule", SettingsChangeOutcome.ScheduleMissing, "Choose fixed days or how many times.", GoalField.Schedule },
        { "no days", SettingsChangeOutcome.DaysMissing, "Choose at least one day.", GoalField.Days },
        { "times 0", SettingsChangeOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20.", GoalField.Times },
        { "no period", SettingsChangeOutcome.PeriodMissing, "Choose per day or per week.", GoalField.Period },
        { "reduce twice a day", SettingsChangeOutcome.ReduceAmountNeedsOneTime, "A Reduce goal with an amount has 1 time per period.", GoalField.Times },
    };

    private static GoalTargetFields FieldsOf(string refusal) => refusal switch
    {
        "no side" => MinutesDaily(15) with { BuildOrReduce = null },
        "no measurement" => MinutesDaily(15) with { Measurement = null },
        "yes/no with amount" => MinutesDaily(15) with { Measurement = Measurement.YesNo },
        "build amount 0" => MinutesDaily(0),
        "reduce amount 1000" => DurationDays(1000, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays),
        "no count unit" => CountDays(10, null!, BuildOrReduce.Build, AllDays),
        "long count unit" => CountDays(10, new string('p', 21), BuildOrReduce.Build, AllDays),
        "no duration unit" => MinutesDaily(15) with { DurationUnit = null },
        "no schedule" => MinutesDaily(15) with { ScheduleKind = null },
        "no days" => MinutesDaily(15) with { Days = new HashSet<DayOfWeek>() },
        "times 0" => DurationTimes(15, DurationUnit.Minutes, 0, SchedulePeriod.Day),
        "no period" => DurationTimes(15, DurationUnit.Minutes, 1, SchedulePeriod.Day) with { Period = null },
        "reduce twice a day" => DurationTimes(15, DurationUnit.Minutes, 2, SchedulePeriod.Day, BuildOrReduce.Reduce),
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, null),
    };

    [Theory]
    [MemberData(nameof(TargetRefusals))]
    public async Task US1_AS9_each_target_refusal_has_its_text_and_field_and_writes_nothing(
        string refusal, SettingsChangeOutcome outcome, string message, GoalField field)
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(ChangedAt);
        var writes = _world.Goals.ChangeWriteCount;

        var result = await ChangeAsync(goal, Input(FieldsOf(refusal), "A new name"));

        AssertRefused(result, outcome, message, field);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Null(_world.GoalOf(goal).OwnName);
    }

    [Fact]
    public async Task FR_003_an_ended_goal_with_an_invalid_target_gives_This_goal_has_ended()
    {
        var goal = await ReadingAsync();
        _world.At(Utc("2026-11-16T10:00:00Z"));
        await _world.EndAsync(goal);
        var writes = _world.Goals.ChangeWriteCount;

        var result = await ChangeAsync(goal, Input(MinutesDaily(15) with { BuildOrReduce = null }));

        AssertRefused(result, SettingsChangeOutcome.GoalEnded, "This goal has ended.", null);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
    }

    [Fact]
    public async Task FR_003_an_invalid_target_with_another_schedule_kind_gives_the_targets_text()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);

        var result = await ChangeAsync(goal, Input(DurationTimes(0, DurationUnit.Minutes, 5, SchedulePeriod.Week)));

        AssertRefused(result, SettingsChangeOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount);
    }

    [Fact]
    public async Task FR_003_another_schedule_kind_with_a_long_name_gives_the_kinds_text_and_a_long_name_alone_its_own()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        var longName = new string('n', 61);

        var kind = await ChangeAsync(goal, Input(DurationTimes(15, DurationUnit.Minutes, 5, SchedulePeriod.Week), longName));
        var name = await ChangeAsync(goal, Input(MinutesDaily(20), longName));
        var sixty = await ChangeAsync(goal, Input(MinutesDaily(15), "  " + new string('n', 60) + "  "));

        Assert.Equal(SettingsChangeOutcome.ScheduleKindChanged, kind.Outcome);
        AssertRefused(name, SettingsChangeOutcome.NameTooLong, "The name can have at most 60 characters.", GoalField.Name);
        Assert.Equal(SettingsChangeOutcome.Changed, sixty.Outcome);
        Assert.Equal(new string('n', 60), _world.GoalOf(goal).OwnName!.Value.Value);
    }

    [Fact]
    public async Task US1_AS10_exactly_the_latest_settings_and_name_give_settings_unchanged_and_write_nothing()
    {
        var goal = await _world.CreateNamedGoalAsync("Morning pages", MinutesDaily(15), Mon2, Created);
        _world.At(ChangedAt);
        var writes = _world.Goals.ChangeWriteCount;
        var revision = _world.Goals.RevisionOf(_world.User.AccountId!.Value);

        // The name is compared after trimming, and values of another measurement are ignored (AC-021).
        var result = await ChangeAsync(goal, Input(MinutesDaily(15) with { CountUnit = "pages" }, "  Morning pages "));

        Assert.Equal((SettingsChangeOutcome.Unchanged, "settings unchanged", (DateOnly?)null, (GoalField?)null), (result.Outcome, result.Message, result.AppliesFrom, result.Field));
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Equal(revision, _world.Goals.RevisionOf(_world.User.AccountId!.Value));
        Assert.Single(_world.GoalOf(goal).Versions);
    }

    [Fact]
    public async Task AC_100_the_waiting_version_is_the_latest_settings_so_repeating_it_is_unchanged_and_the_settings_in_effect_are_not()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        Assert.Equal(SettingsChangeOutcome.Changed, (await ChangeAsync(goal, Input(MinutesDaily(20)))).Outcome);
        var writes = _world.Goals.ChangeWriteCount;

        _world.At(Utc("2026-11-11T12:00:00Z"));
        var again = await ChangeAsync(goal, Input(MinutesDaily(20)));
        Assert.Equal(SettingsChangeOutcome.Unchanged, again.Outcome);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);

        var back = await ChangeAsync(goal, Input(MinutesDaily(15)));
        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (back.Outcome, back.AppliesFrom));
        Assert.Equal([15, 15], _world.GoalOf(goal).Versions.Select(v => v.Settings.Target.Amount));
    }

    [Fact]
    public async Task US1_AS11_a_name_only_change_has_no_date_applies_at_once_and_clearing_it_names_the_goal_by_habit_and_target()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        var versions = _world.GoalOf(goal).Versions;

        var renamed = await ChangeAsync(goal, Input(MinutesDaily(15), " Morning pages "));

        Assert.Equal((SettingsChangeOutcome.Changed, "settings changed", (DateOnly?)null), (renamed.Outcome, renamed.Message, renamed.AppliesFrom));
        Assert.Equal("Morning pages", (await _world.DetailsAsync(goal)).Name);
        Assert.Equal(versions, _world.GoalOf(goal).Versions);

        _world.At(Utc("2026-11-12T10:00:00Z"));
        var cleared = await ChangeAsync(goal, Input(MinutesDaily(15), "   "));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)null), (cleared.Outcome, cleared.AppliesFrom));
        Assert.Null(_world.GoalOf(goal).OwnName);
        Assert.Equal("Reading, 15 min daily", (await _world.DetailsAsync(goal)).Name);
        Assert.Equal(versions, _world.GoalOf(goal).Versions);
    }

    [Fact]
    public async Task A_change_of_target_and_name_together_writes_both_and_keeps_the_new_name()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);

        var result = await ChangeAsync(goal, Input(MinutesDaily(20), "Evening pages"));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        Assert.Equal("Evening pages", _world.GoalOf(goal).OwnName!.Value.Value);
        Assert.Equal([15, 20], _world.GoalOf(goal).Versions.Select(v => v.Settings.Target.Amount));
    }

    [Fact]
    public async Task AC_021_values_outside_the_chosen_measurement_and_schedule_are_ignored_and_not_stored()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);

        var result = await ChangeAsync(goal, Input(MinutesDaily(20) with { CountUnit = "pages", Times = 7, Period = SchedulePeriod.Week }));

        Assert.Equal(SettingsChangeOutcome.Changed, result.Outcome);
        var stored = _world.GoalOf(goal).Versions[^1].Settings.Target;
        Assert.Null(stored.CountUnit);
        Assert.Null(stored.Schedule.Times);
        Assert.Null(stored.Schedule.Period);
        Assert.Equal(Target(MinutesDaily(20)), stored);
    }

    [Fact]
    public async Task US1_AS12_a_change_on_a_goal_that_has_not_begun_replaces_its_first_version_from_its_start_date()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon23, Utc("2026-11-16T10:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var result = await ChangeAsync(goal, Input(MinutesDaily(30)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Mon23), (result.Outcome, result.AppliesFrom));
        var version = Assert.Single(_world.GoalOf(goal).Versions);
        Assert.Equal((Mon23, (int?)30), (version.AppliesFrom, version.Settings.Target.Amount));
        Assert.Equal(Mon23, _world.GoalOf(goal).StartDate);
    }

    [Fact]
    public async Task US1_AS13_a_change_after_a_time_zone_change_merges_into_one_later_version_with_the_new_zone()
    {
        var goal = await ReadingAsync();
        _world.At(Utc("2026-11-11T09:00:00Z"));
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await _world.Profiles.ChangeTimeZoneAsync(_world.User, "Europe/Kyiv", None)).Outcome);
        Assert.Equal([Mon2, Thu12], _world.GoalOf(goal).Versions.Select(v => v.AppliesFrom));

        _world.At(ChangedAt);
        var result = await ChangeAsync(goal, Input(MinutesDaily(20)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        var versions = _world.GoalOf(goal).Versions;
        Assert.Equal([Mon2, Thu12], versions.Select(v => v.AppliesFrom));
        Assert.Equal((_world.DefaultZone.Value.Id, 15), (versions[0].Settings.TimeZone.Id, versions[0].Settings.Target.Amount!.Value));
        Assert.Equal(("Europe/Kyiv", 20), (versions[1].Settings.TimeZone.Id, versions[1].Settings.Target.Amount!.Value));
    }

    [Fact]
    public async Task US1_AS14_a_pause_does_not_block_a_change()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        foreach (var day in Enumerable.Range(0, 9))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 15);
        }

        _world.At(Utc("2026-11-11T09:00:00Z"));
        Assert.Equal(HT.Application.Exemptions.PauseOutcome.Paused,
            (await _world.Pauses.PauseAsync(_world.User, goal, Date(2026, 11, 11), Date(2026, 11, 13), null, None)).Outcome);

        _world.At(ChangedAt);
        var result = await ChangeAsync(goal, Input(MinutesDaily(20)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (result.Outcome, result.AppliesFrom));
        Assert.Equal([15, 20], _world.GoalOf(goal).Versions.Select(v => v.Settings.Target.Amount));
    }

    [Fact]
    public async Task FR_001_a_change_from_Build_to_Reduce_and_a_change_of_measurement_within_the_schedule_kind_are_accepted()
    {
        var reduce = await ReadingAsync();
        var count = await ReadingAsync();
        _world.At(ChangedAt);

        var toReduce = await ChangeAsync(reduce, Input(DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays)));
        var toCount = await ChangeAsync(count, Input(CountDays(12, "pages", BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Friday)));

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (toReduce.Outcome, toReduce.AppliesFrom));
        Assert.Equal(BuildOrReduce.Reduce, _world.GoalOf(reduce).Versions[^1].Settings.Target.BuildOrReduce);
        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu12), (toCount.Outcome, toCount.AppliesFrom));
        Assert.Equal((Measurement.Count, "pages"), (_world.GoalOf(count).Versions[^1].Settings.Target.Measurement, _world.GoalOf(count).Versions[^1].Settings.Target.CountUnit));
    }

    [Fact]
    public async Task FR_023_access_is_checked_first_in_order_and_another_accounts_goal_does_not_exist()
    {
        var goal = await ReadingAsync();
        _world.At(ChangedAt);
        var reads = _world.Goals.OwnerGoalReads;
        var invalid = Input(MinutesDaily(15) with { BuildOrReduce = null });

        AssertRefused(await ChangeAsync(goal, invalid, ActingUser.Anonymous), SettingsChangeOutcome.NotSignedIn, "You are not signed in.", null);
        AssertRefused(await ChangeAsync(goal, invalid, _world.Administrator), SettingsChangeOutcome.NotForAdministrators, "Goals are for registered users.", null);
        Assert.Equal(reads, _world.Goals.OwnerGoalReads);
        AssertRefused(await ChangeAsync(goal, invalid, _world.OtherUser), SettingsChangeOutcome.NotFound, "This goal does not exist.", null);
        AssertRefused(await ChangeAsync(new GoalId(Guid.NewGuid()), invalid), SettingsChangeOutcome.NotFound, "This goal does not exist.", null);
        AssertRefused(
            await ChangeAsync(goal, invalid, ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser)),
            SettingsChangeOutcome.NotSignedIn, "You are not signed in.", null);
        Assert.Equal(15, _world.GoalOf(goal).Versions[^1].Settings.Target.Amount);
    }
}
