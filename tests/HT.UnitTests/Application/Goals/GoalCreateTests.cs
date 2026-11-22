using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Domain.Time;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// <c>CreateAsync</c> (FR-001, FR-002, FR-005, FR-006, FR-008, FR-009, FR-011, FR-018, FR-023; research R-08): the
/// refusals in FR-006's order with their fields, the stored goal, version 1 and attempt 1, and the expected-zone retry.
/// </summary>
public sealed class GoalCreateTests
{
    private const string DistinctiveName = "Zq-Private morning pages";
    private const string DistinctiveUnit = "zq-private-units";

    private readonly GoalWorld _world = new();

    private Task<GoalCreateResult> CreateAsync(ActingUser user, GoalInput input) =>
        _world.Service.CreateAsync(user, input, CancellationToken.None);

    private async Task<Goal> CreatedAsync(ActingUser user, GoalInput input)
    {
        var result = await CreateAsync(user, input);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        Assert.Null(result.Message);
        Assert.Null(result.Field);
        return Assert.Single(_world.Goals.Goals, g => g.Id == result.GoalId!.Value);
    }

    private static GoalTargetFields FieldsOf(GoalStartingValues values) => new()
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

    [Fact]
    public async Task US1_AS5_unchanged_starting_values_create_the_goal_with_exactly_those_settings()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, GoalInputs.Kyiv);
        var values = (await _world.Service.StartNewGoalAsync(user, Catalog(reading), null, CancellationToken.None)).StartingValues!;

        var goal = await CreatedAsync(user, new GoalInput
        {
            Habit = new GoalHabit(values.Habit!.Id, values.Habit.Kind),
            Target = FieldsOf(values),
            StartDate = values.StartDate,
            Name = values.Name,
        });

        Assert.Equal(user.AccountId, goal.Owner);
        Assert.Equal(new GoalHabit(reading.Id, HabitKind.Catalog), goal.Habit);
        Assert.Equal(GoalWorld.Today, goal.StartDate);
        Assert.Null(goal.OwnName);
        var version = Assert.Single(goal.Versions);
        Assert.Equal(GoalWorld.Today, version.AppliesFrom);
        Assert.Equal(Zone(GoalInputs.Kyiv), version.Settings.TimeZone);
        Assert.Equal(Target(FifteenMinutesOnWeekdays), version.Settings.Target);
        Assert.Equal(new GoalAttempt(1, GoalWorld.Today), goal.Attempt);
        Assert.Equal("Reading, 15 min, 5 days a week", GoalText.Name(goal, "Reading"));
        var listed = Assert.Single(await _world.Goals.ListAsync(user.AccountId!.Value, CancellationToken.None));
        Assert.Equal(goal.Id, listed.Goal.Id);
    }

    [Fact]
    public async Task US1_AS6_Reduce_count_at_most_2_cups_a_day()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var goal = await CreatedAsync(RegisteredUser(_world.Accounts),
            Input(Catalog(reading), GoalWorld.Today, CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));

        var target = goal.Versions[0].Settings.Target;
        Assert.Equal(BuildOrReduce.Reduce, target.BuildOrReduce);
        Assert.Equal(Measurement.Count, target.Measurement);
        Assert.Equal(2, target.Amount);
        Assert.Equal("cups", target.CountUnit);
        Assert.Equal(1, target.Schedule.Times);
        Assert.Equal(SchedulePeriod.Day, target.Schedule.Period);
        Assert.Equal("at most 2 cups a day", GoalText.TargetText(target));
    }

    /// <summary>One input per FR-006 refusal, each with only that problem.</summary>
    public static TheoryData<string, GoalCreateOutcome, string, GoalField> Refusals => new()
    {
        { "no habit", GoalCreateOutcome.HabitMissing, "Choose a habit.", GoalField.Habit },
        { "no build or reduce", GoalCreateOutcome.BuildOrReduceMissing, "Choose Build or Reduce.", GoalField.BuildOrReduce },
        { "no measurement", GoalCreateOutcome.MeasurementMissing, "Choose how it is measured.", GoalField.Measurement },
        { "yes/no with an amount", GoalCreateOutcome.YesNoHasAmountOrUnit, "A yes/no goal has no amount or unit.", GoalField.Measurement },
        { "yes/no with a unit", GoalCreateOutcome.YesNoHasAmountOrUnit, "A yes/no goal has no amount or unit.", GoalField.Measurement },
        { "Build amount 0", GoalCreateOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount },
        { "Build amount missing", GoalCreateOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999.", GoalField.Amount },
        { "Reduce amount 1000", GoalCreateOutcome.ReduceAmountOutOfRange, "Enter a whole number from 0 to 999.", GoalField.Amount },
        { "count unit spaces", GoalCreateOutcome.CountUnitMissing, "Enter a unit, for example pages.", GoalField.Unit },
        { "count unit 21", GoalCreateOutcome.CountUnitTooLong, "The unit can have at most 20 characters.", GoalField.Unit },
        { "no duration unit", GoalCreateOutcome.DurationUnitMissing, "Choose minutes or hours.", GoalField.Unit },
        { "no schedule", GoalCreateOutcome.ScheduleMissing, "Choose fixed days or how many times.", GoalField.Schedule },
        { "no day", GoalCreateOutcome.DaysMissing, "Choose at least one day.", GoalField.Days },
        { "times 21", GoalCreateOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20.", GoalField.Times },
        { "times missing", GoalCreateOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20.", GoalField.Times },
        { "no period", GoalCreateOutcome.PeriodMissing, "Choose per day or per week.", GoalField.Period },
        { "Reduce amount 2 times", GoalCreateOutcome.ReduceAmountNeedsOneTime, "A Reduce goal with an amount has 1 time per period.", GoalField.Times },
        { "no start date", GoalCreateOutcome.StartDateMissing, "Choose a start date.", GoalField.StartDate },
        { "yesterday", GoalCreateOutcome.StartDateInPast, "The start date cannot be in the past.", GoalField.StartDate },
        { "366 days ahead", GoalCreateOutcome.StartDateTooFar, "Choose a start date within a year from today.", GoalField.StartDate },
        { "name 61", GoalCreateOutcome.NameTooLong, "The name can have at most 60 characters.", GoalField.Name },
    };

    private static GoalInput RefusedInput(string which, GoalHabit habit)
    {
        var valid = FifteenMinutesOnWeekdays;
        return which switch
        {
            "no habit" => Input(null, GoalWorld.Today),
            "no build or reduce" => Input(habit, GoalWorld.Today, valid with { BuildOrReduce = null }),
            "no measurement" => Input(habit, GoalWorld.Today, valid with { Measurement = null }),
            "yes/no with an amount" => Input(habit, GoalWorld.Today, YesNoDays(BuildOrReduce.Build, Weekdays) with { Amount = 5 }),
            "yes/no with a unit" => Input(habit, GoalWorld.Today, YesNoDays(BuildOrReduce.Build, Weekdays) with { CountUnit = DistinctiveUnit }),
            "Build amount 0" => Input(habit, GoalWorld.Today, valid with { Amount = 0 }),
            "Build amount missing" => Input(habit, GoalWorld.Today, valid with { Amount = null }),
            "Reduce amount 1000" => Input(habit, GoalWorld.Today, CountTimes(1000, DistinctiveUnit, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)),
            "count unit spaces" => Input(habit, GoalWorld.Today, CountTimes(20, "   ", 3, SchedulePeriod.Week)),
            "count unit 21" => Input(habit, GoalWorld.Today, CountTimes(20, DistinctiveUnit + "xxxxx", 3, SchedulePeriod.Week)),
            "no duration unit" => Input(habit, GoalWorld.Today, valid with { DurationUnit = null }),
            "no schedule" => Input(habit, GoalWorld.Today, valid with { ScheduleKind = null }),
            "no day" => Input(habit, GoalWorld.Today, valid with { Days = new HashSet<DayOfWeek>() }),
            "times 21" => Input(habit, GoalWorld.Today, YesNoTimes(21, SchedulePeriod.Week)),
            "times missing" => Input(habit, GoalWorld.Today, YesNoTimes(1, SchedulePeriod.Week) with { Times = null }),
            "no period" => Input(habit, GoalWorld.Today, YesNoTimes(3, SchedulePeriod.Week) with { Period = null }),
            "Reduce amount 2 times" => Input(habit, GoalWorld.Today, CountTimes(2, DistinctiveUnit, 2, SchedulePeriod.Day, BuildOrReduce.Reduce)),
            "no start date" => Input(habit, null),
            "yesterday" => Input(habit, GoalWorld.Today.AddDays(-1)),
            "366 days ahead" => Input(habit, GoalWorld.Today.AddDays(366)),
            "name 61" => Input(habit, GoalWorld.Today, name: DistinctiveName + new string('x', 61 - DistinctiveName.Length)),
            _ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
        };
    }

    [Theory]
    [MemberData(nameof(Refusals))]
    public async Task US1_AS7_each_refusal_has_its_message_and_field_and_creates_nothing(
        string which, GoalCreateOutcome outcome, string message, GoalField field)
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, null);

        var result = await CreateAsync(user, RefusedInput(which, Catalog(reading)));

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Equal(field, result.Field);
        Assert.Null(result.GoalId);
        Assert.Empty(_world.Goals.Goals);
        Assert.Equal(0, _world.Goals.RevisionOf(user.AccountId!.Value));
        Assert.Null(_world.Accounts.FindById(user.AccountId!.Value)!.StoredTimeZone);
        // HP-011: a refusal echoes no input value.
        Assert.DoesNotContain(DistinctiveName, result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DistinctiveUnit, result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("1000", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US1_AS7_the_refusals_come_one_at_a_time_in_FR_006_order()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var input = new GoalInput { Name = new string('n', 61) };
        var seen = new List<GoalCreateOutcome>();

        var steps = new Func<GoalInput, GoalInput>[]
        {
            i => i with { Habit = Catalog(reading) },
            i => i with { Target = i.Target with { BuildOrReduce = BuildOrReduce.Reduce } },
            i => i with { Target = i.Target with { Measurement = Measurement.Count, Amount = 1000 } },
            i => i with { Target = i.Target with { Amount = 2, CountUnit = " " } },
            i => i with { Target = i.Target with { CountUnit = new string('u', 21) } },
            i => i with { Target = i.Target with { CountUnit = "cups" } },
            i => i with { Target = i.Target with { ScheduleKind = ScheduleKind.Times, Times = 0 } },
            i => i with { Target = i.Target with { Times = 2 } },
            i => i with { Target = i.Target with { Period = SchedulePeriod.Day } },
            i => i with { Target = i.Target with { Times = 1 } },
            i => i with { StartDate = GoalWorld.Today.AddDays(-1) },
            i => i with { StartDate = GoalWorld.Today.AddDays(366) },
            i => i with { StartDate = GoalWorld.Today.AddDays(365) },
            i => i with { Name = new string('n', 60) },
        };

        foreach (var step in steps)
        {
            seen.Add((await CreateAsync(user, input)).Outcome);
            input = step(input);
        }

        seen.Add((await CreateAsync(user, input)).Outcome);
        Assert.Equal(
        [
            GoalCreateOutcome.HabitMissing, GoalCreateOutcome.BuildOrReduceMissing, GoalCreateOutcome.MeasurementMissing,
            GoalCreateOutcome.ReduceAmountOutOfRange, GoalCreateOutcome.CountUnitMissing, GoalCreateOutcome.CountUnitTooLong,
            GoalCreateOutcome.ScheduleMissing, GoalCreateOutcome.TimesOutOfRange, GoalCreateOutcome.PeriodMissing,
            GoalCreateOutcome.ReduceAmountNeedsOneTime, GoalCreateOutcome.StartDateMissing, GoalCreateOutcome.StartDateInPast,
            GoalCreateOutcome.StartDateTooFar, GoalCreateOutcome.NameTooLong, GoalCreateOutcome.Created,
        ], seen);
    }

    [Fact]
    public async Task The_latest_start_date_and_the_longest_name_are_accepted()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var goal = await CreatedAsync(RegisteredUser(_world.Accounts),
            Input(Catalog(reading), GoalWorld.Today.AddDays(365), name: "  " + new string('n', 60) + "  "));

        Assert.Equal(GoalWorld.Today.AddDays(365), goal.StartDate);
        Assert.Equal(new string('n', 60), goal.OwnName!.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FR_009_an_empty_name_gives_no_name(string? name)
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var goal = await CreatedAsync(RegisteredUser(_world.Accounts), Input(Catalog(reading), GoalWorld.Today, name: name));

        Assert.Null(goal.OwnName);
    }

    [Fact]
    public async Task FR_009_an_own_name_is_stored_trimmed()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var goal = await CreatedAsync(RegisteredUser(_world.Accounts), Input(Catalog(reading), GoalWorld.Today, name: "  Morning pages  "));

        Assert.Equal("Morning pages", GoalText.Name(goal, "Reading"));
    }

    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Unpublished)]
    public async Task US1_AS8_a_catalog_habit_that_is_not_Published_is_not_in_the_catalog(CatalogHabitState state)
    {
        var habit = _world.CatalogHabits.Add(HabitSamples.Reading(state));

        AssertNotInCatalog(await CreateAsync(RegisteredUser(_world.Accounts), Input(Catalog(habit), GoalWorld.Today)));
    }

    [Fact]
    public async Task US1_AS8_a_custom_habits_identifier_given_as_a_catalog_habit_is_not_in_the_catalog()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        AssertNotInCatalog(await CreateAsync(user, Input(new GoalHabit(custom.Id, HabitKind.Catalog), GoalWorld.Today)));
    }

    [Fact]
    public async Task US1_AS8_an_unknown_identifier_is_not_in_the_catalog()
    {
        AssertNotInCatalog(await CreateAsync(RegisteredUser(_world.Accounts),
            Input(new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog), GoalWorld.Today)));
    }

    [Fact]
    public async Task The_habit_lookup_comes_before_the_target_checks()
    {
        AssertNotInCatalog(await CreateAsync(RegisteredUser(_world.Accounts),
            Input(new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog), null, new GoalTargetFields())));
    }

    [Fact]
    public async Task Another_users_custom_habit_does_not_exist()
    {
        var other = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(other.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        var result = await CreateAsync(RegisteredUser(_world.Accounts), Input(Custom(custom), GoalWorld.Today));

        Assert.Equal(GoalCreateOutcome.CustomHabitNotFound, result.Outcome);
        Assert.Equal("This custom habit does not exist.", result.Message);
        Assert.Equal(GoalField.Habit, result.Field);
        Assert.Empty(_world.Goals.Goals);
    }

    [Fact]
    public async Task An_own_custom_habit_gets_a_goal()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        var goal = await CreatedAsync(user, Input(Custom(custom), GoalWorld.Today, YesNoDays(BuildOrReduce.Build, AllDays)));

        Assert.Equal(new GoalHabit(custom.Id, HabitKind.Custom), goal.Habit);
        Assert.Equal("Cold shower, daily", GoalText.Name(goal, "Cold shower"));
    }

    [Fact]
    public async Task A_catalog_habit_unpublished_before_the_store_writes_is_not_in_the_catalog()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var service = _world.ServiceChangingBeforeFirstCreate(async () => Assert.True(await _world.CatalogHabits
            .TrySetStateAsync(reading.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None)));

        var result = await service.CreateAsync(RegisteredUser(_world.Accounts), Input(Catalog(reading), GoalWorld.Today), CancellationToken.None);

        AssertNotInCatalog(result);
        Assert.Empty(_world.Goals.Goals);
    }

    [Fact]
    public async Task A_custom_habit_deleted_before_the_store_writes_does_not_exist()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));
        var service = _world.ServiceChangingBeforeFirstCreate(async () => Assert.Equal(DeleteCustomHabitOutcome.Deleted,
            await _world.CustomHabits.DeleteAsync(user.AccountId!.Value, custom.Id, CancellationToken.None)));

        var result = await service.CreateAsync(user, Input(Custom(custom), GoalWorld.Today), CancellationToken.None);

        Assert.Equal(GoalCreateOutcome.CustomHabitNotFound, result.Outcome);
        Assert.Equal("This custom habit does not exist.", result.Message);
        Assert.Equal(GoalField.Habit, result.Field);
        Assert.Empty(_world.Goals.Goals);
    }

    [Fact]
    public async Task US1_AS10_a_second_goal_on_Reading_exists_independently()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);

        var first = await CreatedAsync(user, Input(Catalog(reading), GoalWorld.Today));
        var second = await CreatedAsync(user, Input(Catalog(reading), GoalWorld.Today));
        var third = await CreatedAsync(user, Input(Catalog(reading), GoalWorld.Today, CountTimes(20, "pages", 3, SchedulePeriod.Week)));

        Assert.Equal(3, new[] { first.Id, second.Id, third.Id }.Distinct().Count());
        Assert.Equal(3, (await _world.Goals.ListAsync(user.AccountId!.Value, CancellationToken.None)).Count);
        Assert.Equal(3, _world.Goals.RevisionOf(user.AccountId!.Value));
    }

    [Theory]
    [InlineData("2026-11-16T21:30:00Z", GoalCreateOutcome.Created)]
    [InlineData("2026-11-16T22:30:00Z", GoalCreateOutcome.StartDateInPast)]
    public async Task US4_AS8_creating_with_Mondays_date_at_23_30_and_at_00_30(string now, GoalCreateOutcome outcome)
    {
        // 23:30 on Monday 16 November in Kyiv (UTC+2) is 21:30 UTC; 00:30 on Tuesday is 22:30 UTC.
        var world = new GoalWorld(now: Instants.Utc(now));
        var reading = world.CatalogHabits.Add(HabitSamples.Reading());

        var result = await world.Service.CreateAsync(
            RegisteredUser(world.Accounts, GoalInputs.Kyiv), Input(Catalog(reading), new DateOnly(2026, 11, 16)), CancellationToken.None);

        Assert.Equal(outcome, result.Outcome);
    }

    [Fact]
    public async Task An_account_without_a_stored_zone_gets_the_installations_default_with_its_first_goal()
    {
        var world = new GoalWorld("Europe/Kyiv");
        var reading = world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUserWithDefaultZone(world.Accounts);

        var result = await world.Service.CreateAsync(user, Input(Catalog(reading), GoalWorld.Today), CancellationToken.None);

        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        Assert.Equal(Zone(GoalInputs.Kyiv), Assert.Single(world.Goals.Goals).Versions[0].Settings.TimeZone);
        Assert.Equal(GoalInputs.Kyiv, world.Accounts.FindById(user.AccountId!.Value)!.StoredTimeZone);
    }

    [Fact]
    public async Task One_TimeZoneChanged_makes_the_service_check_the_start_date_again_in_the_new_zone()
    {
        // 22:30 UTC on Wednesday 18 November is Wednesday in UTC and 00:30 on Thursday 19 November in Kyiv.
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.Clock.SetUtcNow(Instants.Utc("2026-11-18T22:30:00Z"));
        var user = RegisteredUser(_world.Accounts, "UTC");
        var service = _world.ServiceChangingBeforeFirstCreate(() =>
        {
            _world.Accounts.SetStoredTimeZone(user.AccountId!.Value, GoalInputs.Kyiv);
            return Task.CompletedTask;
        });

        var result = await service.CreateAsync(user, Input(Catalog(reading), new DateOnly(2026, 11, 18)), CancellationToken.None);

        Assert.Equal(GoalCreateOutcome.StartDateInPast, result.Outcome);
        Assert.Equal("The start date cannot be in the past.", result.Message);
        Assert.Equal(1, _world.Goals.CreateCount);
        Assert.Empty(_world.Goals.Goals);
    }

    [Fact]
    public async Task After_a_TimeZoneChanged_the_goal_gets_the_new_zone()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.Clock.SetUtcNow(Instants.Utc("2026-11-18T22:30:00Z"));
        var user = RegisteredUser(_world.Accounts, "UTC");
        var service = _world.ServiceChangingBeforeFirstCreate(() =>
        {
            _world.Accounts.SetStoredTimeZone(user.AccountId!.Value, GoalInputs.Kyiv);
            return Task.CompletedTask;
        });

        var result = await service.CreateAsync(user, Input(Catalog(reading), new DateOnly(2026, 11, 19)), CancellationToken.None);

        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        Assert.Equal(2, _world.Goals.CreateCount);
        Assert.Equal(Zone(GoalInputs.Kyiv), Assert.Single(_world.Goals.Goals).Versions[0].Settings.TimeZone);
    }

    [Fact]
    public async Task Nine_TimeZoneChanged_in_a_row_still_create_the_goal()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.Goals.ReportTimeZoneChangedOnNextCreates(9);

        var result = await CreateAsync(RegisteredUser(_world.Accounts), Input(Catalog(reading), GoalWorld.Today));

        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        Assert.Equal(10, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task Ten_TimeZoneChanged_in_a_row_throw_a_fault_that_names_no_value()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.Goals.ReportTimeZoneChangedOnNextCreates(10);
        var input = Input(Catalog(reading), GoalWorld.Today, CountTimes(20, DistinctiveUnit, 3, SchedulePeriod.Week), DistinctiveName);

        var fault = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateAsync(RegisteredUser(_world.Accounts), input));

        Assert.Equal(10, _world.Goals.CreateCount);
        Assert.Empty(_world.Goals.Goals);
        Assert.DoesNotContain(DistinctiveName, fault.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DistinctiveUnit, fault.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Reading", fault.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(GoalInputs.Kyiv, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_registered_user_whose_account_is_not_stored_is_not_signed_in()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        var result = await CreateAsync(gone, Input(Catalog(reading), GoalWorld.Today));

        Assert.Equal(GoalCreateOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Null(result.Field);
        Assert.Equal(0, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task FR_023_an_administrator_gets_the_role_refusal_before_any_field()
    {
        var result = await CreateAsync(Administrator(_world.Accounts), new GoalInput());

        Assert.Equal(GoalCreateOutcome.NotForAdministrators, result.Outcome);
        Assert.Equal("Goals are for registered users.", result.Message);
        Assert.Null(result.Field);
        Assert.Equal(0, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task FR_023_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await CreateAsync(ActingUser.Anonymous, new GoalInput());

        Assert.Equal(GoalCreateOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Null(result.Field);
    }

    private static void AssertNotInCatalog(GoalCreateResult result)
    {
        Assert.Equal(GoalCreateOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
        Assert.Equal(GoalField.Habit, result.Field);
        Assert.Null(result.GoalId);
    }

    /// <summary>
    /// Slice 8 plan "Risks": every problem creation can produce parses as a <see cref="GoalCreateOutcome"/>, the name
    /// <c>CreateAsync</c> gives its refusal. <see cref="GoalProblem.StartDateNotAfterEnding"/> belongs only to starting a
    /// new attempt, so creation has no outcome for it.
    /// </summary>
    [Fact]
    public void Every_problem_creation_can_produce_parses_as_an_outcome()
    {
        var creationProblems = Enum.GetValues<GoalProblem>()
            .Where(problem => problem is not (GoalProblem.None or GoalProblem.StartDateNotAfterEnding));

        Assert.All(creationProblems, problem => Assert.True(Enum.TryParse<GoalCreateOutcome>(problem.ToString(), out _), problem.ToString()));
        Assert.False(Enum.TryParse<GoalCreateOutcome>(nameof(GoalProblem.StartDateNotAfterEnding), out _));
    }
}
