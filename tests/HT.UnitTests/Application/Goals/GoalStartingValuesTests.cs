using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Domain.Time;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// <c>StartNewGoalAsync</c> (FR-007, FR-008, FR-012, FR-023): the starting values of the New goal form, the habit
/// lookup, the suggestion position and "today" in the acting user's time zone.
/// </summary>
public sealed class GoalStartingValuesTests
{
    private readonly GoalWorld _world = new();

    private Task<GoalStartResult> StartAsync(ActingUser user, GoalHabit? habit, int? position = null) =>
        _world.Service.StartNewGoalAsync(user, habit, position, CancellationToken.None);

    private async Task<GoalStartingValues> StartedAsync(ActingUser user, GoalHabit? habit, int? position = null)
    {
        var result = await StartAsync(user, habit, position);
        Assert.Equal(GoalStartOutcome.Started, result.Outcome);
        Assert.Null(result.Message);
        return result.StartingValues!;
    }

    [Fact]
    public async Task US1_AS1_without_a_position_the_marked_suggestion_gives_the_values()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var values = await StartedAsync(RegisteredUser(_world.Accounts), Catalog(reading));

        Assert.Equal(new GoalHabitView(reading.Id, HabitKind.Catalog, "Reading", Category.Mind, true) { Icon = "book-open" }, values.Habit);
        Assert.Equal(BuildOrReduce.Build, values.BuildOrReduce);
        Assert.Equal(Measurement.Duration, values.Measurement);
        Assert.Equal(15, values.Amount);
        Assert.Equal(DurationUnit.Minutes, values.DurationUnit);
        Assert.Null(values.CountUnit);
        Assert.Equal(ScheduleKind.FixedDays, values.ScheduleKind);
        Assert.True(values.Days.SetEquals(HabitSamples.Weekdays));
        Assert.Null(values.Times);
        Assert.Null(values.Period);
        Assert.Equal(GoalWorld.Today, values.StartDate);
        Assert.Null(values.Name);
        Assert.Equal("15 min, 5 days a week", values.CatalogSuggestionText);
    }

    [Fact]
    public async Task US1_AS2_position_2_gives_the_second_suggestion()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var values = await StartedAsync(RegisteredUser(_world.Accounts), Catalog(reading), 2);

        Assert.Equal(BuildOrReduce.Build, values.BuildOrReduce);
        Assert.Equal(Measurement.Count, values.Measurement);
        Assert.Equal(20, values.Amount);
        Assert.Equal("pages", values.CountUnit);
        Assert.Null(values.DurationUnit);
        Assert.Equal(ScheduleKind.Times, values.ScheduleKind);
        Assert.Empty(values.Days);
        Assert.Equal(3, values.Times);
        Assert.Equal(SchedulePeriod.Week, values.Period);
        Assert.Equal(GoalWorld.Today, values.StartDate);
        Assert.Equal("20 pages, 3 times a week", values.CatalogSuggestionText);
    }

    [Fact]
    public async Task US1_AS3_a_Reduce_habit_without_suggestions_gives_only_the_habit_Reduce_and_the_start_date()
    {
        var sugar = _world.CatalogHabits.Add(HabitSamples.Catalog("Sugar", category: Category.Health, intent: Intent.Reduce, icon: "candy"));

        var values = await StartedAsync(RegisteredUser(_world.Accounts), Catalog(sugar));

        Assert.Equal("Sugar", values.Habit!.Name);
        Assert.Equal(BuildOrReduce.Reduce, values.BuildOrReduce);
        Assert.Null(values.Measurement);
        Assert.Null(values.Amount);
        Assert.Null(values.CountUnit);
        Assert.Null(values.DurationUnit);
        Assert.Null(values.ScheduleKind);
        Assert.Empty(values.Days);
        Assert.Null(values.Times);
        Assert.Null(values.Period);
        Assert.Equal(GoalWorld.Today, values.StartDate);
        Assert.Null(values.CatalogSuggestionText);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task US1_AS4_a_position_the_habit_does_not_have_does_not_exist(int position)
    {
        var habit = _world.CatalogHabits.Add(HabitSamples.Catalog("Three goals", goals: HabitSamples.Goals(0,
            HabitSamples.YesNo(1, SchedulePeriod.Day), HabitSamples.YesNo(2, SchedulePeriod.Day), HabitSamples.YesNo(3, SchedulePeriod.Week))));

        var result = await StartAsync(RegisteredUser(_world.Accounts), Catalog(habit), position);

        Assert.Equal(GoalStartOutcome.SuggestionNotFound, result.Outcome);
        Assert.Equal("This suggested starting goal does not exist.", result.Message);
        Assert.Null(result.StartingValues);
    }

    [Fact]
    public async Task FR_007_a_Reduce_yes_no_suggestion_on_fixed_days_reads_as_the_goal_rules_write_it()
    {
        var phone = _world.CatalogHabits.Add(HabitSamples.Catalog("Phone at dinner", intent: Intent.Reduce, goals: HabitSamples.Goals(0,
            new SuggestedStartingGoalFields
            {
                BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
                Days = HabitSamples.Weekdays.ToHashSet(),
            })));

        var values = await StartedAsync(RegisteredUser(_world.Accounts), Catalog(phone));

        Assert.Equal("avoided on Mon–Fri", values.CatalogSuggestionText);
        Assert.Equal(BuildOrReduce.Reduce, values.BuildOrReduce);
        Assert.Equal(Measurement.YesNo, values.Measurement);
        Assert.True(values.Days.SetEquals(HabitSamples.Weekdays));
    }

    [Fact]
    public async Task A_suggestion_that_is_no_valid_goal_target_keeps_its_catalog_text()
    {
        var coffee = _world.CatalogHabits.Add(HabitSamples.Catalog("Coffee breaks", intent: Intent.Reduce, goals: HabitSamples.Goals(0,
            HabitSamples.Count(2, "cups", 3, SchedulePeriod.Day, BuildOrReduce.Reduce))));

        var values = await StartedAsync(RegisteredUser(_world.Accounts), Catalog(coffee));

        Assert.Equal("at most 2 cups, 3 times a day", values.CatalogSuggestionText);
        Assert.Equal(3, values.Times);
    }

    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Unpublished)]
    public async Task US1_AS8_a_catalog_habit_that_is_not_Published_is_not_in_the_catalog(CatalogHabitState state)
    {
        var habit = _world.CatalogHabits.Add(HabitSamples.Reading(state));

        var result = await StartAsync(RegisteredUser(_world.Accounts), Catalog(habit));

        AssertNotInCatalog(result);
    }

    [Fact]
    public async Task US1_AS8_a_custom_habits_identifier_given_as_a_catalog_habit_is_not_in_the_catalog()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        var result = await StartAsync(user, new GoalHabit(custom.Id, HabitKind.Catalog));

        AssertNotInCatalog(result);
    }

    [Fact]
    public async Task US1_AS8_an_unknown_identifier_is_not_in_the_catalog()
    {
        var result = await StartAsync(RegisteredUser(_world.Accounts), new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog));

        AssertNotInCatalog(result);
    }

    [Fact]
    public async Task Another_users_custom_habit_does_not_exist()
    {
        var other = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(other.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        var result = await StartAsync(RegisteredUser(_world.Accounts), Custom(custom));

        Assert.Equal(GoalStartOutcome.CustomHabitNotFound, result.Outcome);
        Assert.Equal("This custom habit does not exist.", result.Message);
        Assert.Null(result.StartingValues);
    }

    [Fact]
    public async Task A_catalog_habits_identifier_given_as_a_custom_habit_does_not_exist()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());

        var result = await StartAsync(RegisteredUser(_world.Accounts), new GoalHabit(reading.Id, HabitKind.Custom));

        Assert.Equal(GoalStartOutcome.CustomHabitNotFound, result.Outcome);
    }

    [Fact]
    public async Task An_own_custom_habit_gives_only_the_habit_its_intent_and_the_start_date()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("No phone in bed", Category.Home, Intent.Reduce, "smartphone"));

        var values = await StartedAsync(user, Custom(custom));

        Assert.Equal(new GoalHabitView(custom.Id, HabitKind.Custom, "No phone in bed", Category.Home, null) { Icon = "smartphone" }, values.Habit);
        Assert.Equal(BuildOrReduce.Reduce, values.BuildOrReduce);
        Assert.Null(values.Measurement);
        Assert.Null(values.ScheduleKind);
        Assert.Null(values.CatalogSuggestionText);
        Assert.Equal(GoalWorld.Today, values.StartDate);
    }

    [Fact]
    public async Task A_custom_habit_has_no_suggestion_position()
    {
        var user = RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));

        var result = await StartAsync(user, Custom(custom), 1);

        Assert.Equal(GoalStartOutcome.SuggestionNotFound, result.Outcome);
        Assert.Equal("This suggested starting goal does not exist.", result.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public async Task US8_AS4_no_habit_gives_only_todays_start_date(int? position)
    {
        var values = await StartedAsync(RegisteredUser(_world.Accounts), null, position);

        Assert.Equal(GoalWorld.Today, values.StartDate);
        Assert.Null(values.Measurement);
        Assert.Null(values.ScheduleKind);
        Assert.Null(values.CatalogSuggestionText);
        Assert.Null(values.Habit);
        Assert.Null(values.BuildOrReduce);
        Assert.Empty(values.Days);
    }

    [Theory]
    [InlineData("2026-11-16T21:30:00Z", 16)]
    [InlineData("2026-11-16T22:30:00Z", 17)]
    public async Task US4_AS8_the_start_date_is_today_in_the_users_zone(string now, int day)
    {
        // 23:30 on Monday 16 November in Kyiv (UTC+2) is 21:30 UTC; 00:30 on Tuesday is 22:30 UTC.
        var world = new GoalWorld(now: Instants.Utc(now));

        var values = (await world.Service.StartNewGoalAsync(RegisteredUser(world.Accounts, Kyiv), null, null, CancellationToken.None)).StartingValues!;

        Assert.Equal(new DateOnly(2026, 11, day), values.StartDate);
    }

    [Theory]
    [InlineData("Europe/Kyiv", null, 19)]
    [InlineData(null, null, 18)]
    [InlineData(null, "Europe/Kyiv", 19)]
    [InlineData("UTC", "Europe/Kyiv", 18)]
    public async Task Today_uses_the_stored_zone_or_the_installations_default(string? stored, string? installation, int day)
    {
        // 23:30 UTC on Wednesday 18 November is 01:30 on Thursday 19 November in Kyiv.
        var world = new GoalWorld(installation);
        world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 23, 30, 0, TimeSpan.Zero));

        var result = await world.Service.StartNewGoalAsync(RegisteredUser(world.Accounts, stored), null, null, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 11, day), result.StartingValues!.StartDate);
    }

    [Fact]
    public async Task A_registered_user_whose_account_is_not_stored_is_not_signed_in()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        var result = await StartAsync(gone, Catalog(reading));

        Assert.Equal(GoalStartOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Null(result.StartingValues);
    }

    [Fact]
    public async Task FR_023_an_administrator_with_an_unknown_habit_gets_the_role_refusal()
    {
        var result = await StartAsync(Administrator(_world.Accounts), new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog), 9);

        Assert.Equal(GoalStartOutcome.NotForAdministrators, result.Outcome);
        Assert.Equal("Goals are for registered users.", result.Message);
        Assert.Null(result.StartingValues);
    }

    [Fact]
    public async Task FR_023_an_administrator_whose_account_is_not_stored_still_gets_the_role_refusal()
    {
        var result = await StartAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator), null);

        Assert.Equal(GoalStartOutcome.NotForAdministrators, result.Outcome);
    }

    [Fact]
    public async Task FR_023_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await StartAsync(ActingUser.Anonymous, new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog), 9);

        Assert.Equal(GoalStartOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
    }

    [Fact]
    public async Task Starting_creates_nothing()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, null);

        await StartedAsync(user, Catalog(reading));

        Assert.Equal(0, _world.Goals.CreateCount);
        Assert.Empty(_world.Goals.Goals);
        Assert.Null(_world.Accounts.FindById(user.AccountId!.Value)!.StoredTimeZone);
    }

    private static void AssertNotInCatalog(GoalStartResult result)
    {
        Assert.Equal(GoalStartOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
        Assert.Null(result.StartingValues);
    }
}
