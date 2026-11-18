using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// <c>GetAsync</c> (US2-AS4, US1-AS9, US7-AS7, FR-016, FR-018, FR-020, FR-023, FR-034): every reading field, the name,
/// the habit's current name and state, the time zones, the first week, the attempt and whether the goal has started.
/// Kyiv is UTC+2 and New York UTC−5 throughout November 2026; every instant below is worked out by hand.
/// </summary>
public sealed class GoalReadTests
{
    private readonly GoalWorld _world = new();

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);

    private Task<GoalReadResult> GetAsync(ActingUser user, GoalId id) => _world.Service.GetAsync(user, id, CancellationToken.None);

    private async Task<GoalDetails> ReadAsync(ActingUser user, GoalId id)
    {
        var result = await GetAsync(user, id);
        Assert.Equal(GoalReadOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Goal!;
    }

    private async Task<GoalId> CreateAsync(ActingUser user, GoalHabit habit, GoalTargetFields target, DateOnly start, string? name = null)
    {
        var result = await _world.Service.CreateAsync(user, Input(habit, start, target, name), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    [Fact]
    public async Task US2_AS4_the_owner_reads_every_field_of_the_goal()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18);

        var goal = await ReadAsync(user, id);

        Assert.Equal(id, goal.Id);
        Assert.Equal("Reading, 15 min, 5 days a week", goal.Name);
        Assert.False(goal.HasOwnName);
        Assert.Equal(new GoalHabitView(reading.Id, HabitKind.Catalog, "Reading", Category.Mind, true), goal.Habit);
        Assert.Equal(BuildOrReduce.Build, goal.BuildOrReduce);
        Assert.Equal(Measurement.Duration, goal.Measurement);
        Assert.Equal(15, goal.Amount);
        Assert.Null(goal.CountUnit);
        Assert.Equal(DurationUnit.Minutes, goal.DurationUnit);
        Assert.Equal(ScheduleKind.FixedDays, goal.ScheduleKind);
        Assert.True(goal.Days.SetEquals(Weekdays));
        Assert.Null(goal.Times);
        Assert.Null(goal.Period);
        Assert.Equal(GoalScheduleKind.CommitmentBased, goal.GoalScheduleKind);
        Assert.Equal("15 min, 5 days a week", goal.TargetText);
        Assert.Equal("Mon–Fri", goal.ScheduleText);
        Assert.Equal("Duration · fixed days Mon–Fri", goal.Kind);
        Assert.Equal(Wed18, goal.StartDate);
        Assert.True(goal.HasStarted);
        Assert.Null(goal.FirstWeekStarts);
        Assert.Equal(GoalInputs.Kyiv, goal.TimeZone);
        Assert.Equal(Wed18, goal.TimeZoneAppliesFrom);
        Assert.Null(goal.LaterTimeZone);
        Assert.Null(goal.LaterTimeZoneAppliesFrom);
        Assert.Equal(1, goal.AttemptNumber);
        Assert.Equal(Wed18, goal.AttemptStartDate);
    }

    [Fact]
    public async Task US2_AS4_a_count_goal_of_N_times_reads_its_amount_unit_times_and_period()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), CountTimes(20, "pages", 3, SchedulePeriod.Week), Wed18);

        var goal = await ReadAsync(user, id);

        Assert.Equal(Measurement.Count, goal.Measurement);
        Assert.Equal(20, goal.Amount);
        Assert.Equal("pages", goal.CountUnit);
        Assert.Null(goal.DurationUnit);
        Assert.Equal(ScheduleKind.Times, goal.ScheduleKind);
        Assert.Empty(goal.Days);
        Assert.Equal(3, goal.Times);
        Assert.Equal(SchedulePeriod.Week, goal.Period);
        Assert.Equal(GoalScheduleKind.PeriodBased, goal.GoalScheduleKind);
        Assert.Equal("20 pages, 3 times a week", goal.TargetText);
        Assert.Equal("Flexible · 3 times a week", goal.ScheduleText);
    }

    [Fact]
    public async Task US2_AS4_a_Mon_Wed_Fri_goal_reads_its_schedule_text()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), YesNoDays(BuildOrReduce.Build, MonWedFri), Wed18);

        Assert.Equal("Mon · Wed · Fri", (await ReadAsync(user, id)).ScheduleText);
    }

    [Fact]
    public async Task US1_AS9_a_goal_without_a_name_has_the_derived_name_and_one_with_a_name_keeps_it()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var derived = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18);
        var named = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18, "Morning pages");

        var withoutName = await ReadAsync(user, derived);
        var withName = await ReadAsync(user, named);

        Assert.Equal("Reading, 15 min, 5 days a week", withoutName.Name);
        Assert.False(withoutName.HasOwnName);
        Assert.Equal("Morning pages", withName.Name);
        Assert.True(withName.HasOwnName);
        Assert.Equal("Reading", withName.Habit.Name);
    }

    [Fact]
    public async Task US7_AS7_a_renamed_habit_renames_the_goal_and_its_derived_name()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18);

        await _world.CatalogHabits.UpdateAsync(reading.Id, HabitSamples.Definition("Books"), reading.SuggestedStartingGoals, CancellationToken.None);
        var goal = await ReadAsync(user, id);

        Assert.Equal("Books", goal.Habit.Name);
        Assert.Equal("Books, 15 min, 5 days a week", goal.Name);
        Assert.Equal("15 min, 5 days a week", goal.TargetText);
    }

    [Fact]
    public async Task FR_034_IsInCatalog_is_true_while_Published_false_once_Unpublished_and_null_for_a_custom_habit()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));
        var onCatalog = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18);
        var onCustom = await CreateAsync(user, Custom(custom), YesNoTimes(1, SchedulePeriod.Day), Wed18);

        Assert.True((await ReadAsync(user, onCatalog)).Habit.IsInCatalog);
        var customGoal = await ReadAsync(user, onCustom);
        Assert.Null(customGoal.Habit.IsInCatalog);
        Assert.Equal(HabitKind.Custom, customGoal.Habit.Kind);
        Assert.Equal("Cold shower", customGoal.Habit.Name);

        Assert.True(await _world.CatalogHabits.TrySetStateAsync(reading.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        var unpublished = await ReadAsync(user, onCatalog);

        Assert.False(unpublished.Habit.IsInCatalog);
        Assert.Equal("Reading", unpublished.Habit.Name);
        Assert.Equal("Reading, 15 min, 5 days a week", unpublished.Name);
        Assert.Equal("15 min, 5 days a week", unpublished.TargetText);
    }

    [Fact]
    public async Task FR_016_the_time_zone_is_the_current_dates_version_and_the_later_one_follows_it()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, "America/New_York");
        var goal = _world.AddGoal(user.AccountId!.Value, reading, OnDays(AllDays), (Mon16, Zone(GoalInputs.Kyiv)), (Thu19, NewYork));

        // 12:00 in Kyiv on Wednesday 18 November: the current date is Wednesday, in the first version.
        var before = await ReadAsync(user, goal.Id);
        Assert.Equal(GoalInputs.Kyiv, before.TimeZone);
        Assert.Equal(Mon16, before.TimeZoneAppliesFrom);
        Assert.Equal("America/New_York", before.LaterTimeZone);
        Assert.Equal(Thu19, before.LaterTimeZoneAppliesFrom);

        // 00:00 in New York on Thursday 19 November: the current date is Thursday, in the second version.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 5, 0, 0, TimeSpan.Zero));
        var after = await ReadAsync(user, goal.Id);
        Assert.Equal("America/New_York", after.TimeZone);
        Assert.Equal(Thu19, after.TimeZoneAppliesFrom);
        Assert.Null(after.LaterTimeZone);
        Assert.Null(after.LaterTimeZoneAppliesFrom);
    }

    [Fact]
    public async Task FR_016_a_goal_with_no_current_date_reads_its_first_version_from_its_start_date()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed25);

        var goal = await ReadAsync(user, id);

        Assert.False(goal.HasStarted);
        Assert.Equal(GoalInputs.Kyiv, goal.TimeZone);
        Assert.Equal(Wed25, goal.TimeZoneAppliesFrom);
        Assert.Null(goal.LaterTimeZone);
        Assert.Null(goal.LaterTimeZoneAppliesFrom);
    }

    [Fact]
    public async Task US4_AS4_a_per_week_goal_gives_its_first_week_always()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), YesNoTimes(3, SchedulePeriod.Week), Wed18);

        Assert.Equal(Mon23, (await ReadAsync(user, id)).FirstWeekStarts);

        // 12:00 in Kyiv on Tuesday 24 November, after the first week began.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 24, 10, 0, 0, TimeSpan.Zero));
        Assert.Equal(Mon23, (await ReadAsync(user, id)).FirstWeekStarts);
    }

    [Fact]
    public async Task US4_AS4_a_per_week_goal_starting_on_a_Monday_has_its_first_week_on_the_start_date()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), YesNoTimes(3, SchedulePeriod.Week), Mon23);

        Assert.Equal(Mon23, (await ReadAsync(user, id)).FirstWeekStarts);
    }

    [Fact]
    public async Task FR_018_attempt_1_begins_on_the_start_date_and_HasStarted_turns_true_when_its_day_begins()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Thu19);

        // 23:59:59 in Kyiv on Wednesday 18 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 21, 59, 59, TimeSpan.Zero));
        var before = await ReadAsync(user, id);
        Assert.False(before.HasStarted);
        Assert.Equal(1, before.AttemptNumber);
        Assert.Equal(Thu19, before.AttemptStartDate);
        Assert.Equal(Thu19, before.StartDate);

        // 00:00 in Kyiv on Thursday 19 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 22, 0, 0, TimeSpan.Zero));
        var after = await ReadAsync(user, id);
        Assert.True(after.HasStarted);
        Assert.Equal(1, after.AttemptNumber);
        Assert.Equal(Thu19, after.AttemptStartDate);
    }

    [Fact]
    public async Task FR_020_an_unknown_identifier_and_another_users_goal_give_the_same_refusal()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var owner = RegisteredUser(_world.Accounts);
        var other = RegisteredUser(_world.Accounts);
        var othersGoal = await CreateAsync(other, Catalog(reading), FifteenMinutesOnWeekdays, Wed18, "Zq-Private goal");

        var unknown = await GetAsync(owner, new GoalId(Guid.NewGuid()));
        var notMine = await GetAsync(owner, othersGoal);

        foreach (var result in new[] { unknown, notMine })
        {
            Assert.Equal(GoalReadOutcome.NotFound, result.Outcome);
            Assert.Equal("This goal does not exist.", result.Message);
            Assert.Null(result.Goal);
        }
    }

    [Fact]
    public async Task FR_023_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await GetAsync(ActingUser.Anonymous, new GoalId(Guid.NewGuid()));

        Assert.Equal(GoalReadOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Null(result.Goal);
    }

    [Fact]
    public async Task FR_023_an_administrator_is_refused_before_the_account_and_the_goal_are_looked_up()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var id = await CreateAsync(user, Catalog(reading), FifteenMinutesOnWeekdays, Wed18);
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { Administrator(_world.Accounts), unstored })
        {
            var result = await GetAsync(administrator, id);
            Assert.Equal(GoalReadOutcome.NotForAdministrators, result.Outcome);
            Assert.Equal("Goals are for registered users.", result.Message);
            Assert.Null(result.Goal);
        }
    }

    [Fact]
    public async Task FR_023_a_registered_user_whose_account_is_not_stored_is_not_signed_in()
    {
        var result = await GetAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), new GoalId(Guid.NewGuid()));

        Assert.Equal(GoalReadOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
    }
}
