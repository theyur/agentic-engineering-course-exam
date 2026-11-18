using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// <c>ListAsync</c> (US2-AS1 to AS3, AS5, US4-AS4, FR-014, FR-015, FR-023): name order then creation order, the summary
/// fields, the next commitment, the start date and the first week by the goal's own timeline, and today. Kyiv is UTC+2,
/// New York UTC−5 and Tokyo UTC+9 throughout November 2026; every instant below is worked out by hand.
/// </summary>
public sealed class GoalListTests
{
    private readonly GoalWorld _world = new();

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Tue24 = Date(2026, 11, 24);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);
    private static readonly DateOnly Mon30 = Date(2026, 11, 30);

    private Task<GoalListResult> ListAsync(ActingUser user) => _world.Service.ListAsync(user, CancellationToken.None);

    private async Task<GoalListItem> OnlyItemAsync(ActingUser user)
    {
        var result = await ListAsync(user);
        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        return Assert.Single(result.Goals);
    }

    private async Task<GoalId> CreateAsync(ActingUser user, CatalogHabit habit, GoalTargetFields target, DateOnly start, string? name = null)
    {
        var result = await _world.Service.CreateAsync(user, Input(Catalog(habit), start, target, name), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    [Fact]
    public async Task US2_AS1_goals_come_in_name_order_with_every_summary_field()
    {
        var yoga = _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var yogaId = await CreateAsync(user, yoga, YesNoTimes(3, SchedulePeriod.Week), Wed18);
        var readingId = await CreateAsync(user, reading, FifteenMinutesOnWeekdays, Wed18);

        var result = await ListAsync(user);

        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(Wed18, result.Today);
        Assert.Equal(["Reading, 15 min, 5 days a week", "Yoga, 3 times a week"], result.Goals.Select(g => g.Name));
        Assert.Equal(new GoalListItem
        {
            Id = readingId,
            Name = "Reading, 15 min, 5 days a week",
            HabitName = "Reading",
            HabitKind = HabitKind.Catalog,
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.Duration,
            TargetText = "15 min, 5 days a week",
            ScheduleText = "Mon–Fri",
            ScheduleKind = GoalScheduleKind.CommitmentBased,
            StartDate = null,
            NextCommitment = Wed18,
            FirstWeekStarts = null,
        }, result.Goals[0]);
        Assert.Equal(new GoalListItem
        {
            Id = yogaId,
            Name = "Yoga, 3 times a week",
            HabitName = "Yoga",
            HabitKind = HabitKind.Catalog,
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.YesNo,
            TargetText = "3 times a week",
            ScheduleText = "Flexible · 3 times a week",
            ScheduleKind = GoalScheduleKind.PeriodBased,
            StartDate = null,
            NextCommitment = null,
            FirstWeekStarts = Mon23,
        }, result.Goals[1]);
    }

    [Fact]
    public async Task US2_AS1_name_order_ignores_case_and_equal_names_keep_their_creation_order()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        var beta = await CreateAsync(user, reading, FifteenMinutesOnWeekdays, Wed18, "Beta");
        var firstSame = await CreateAsync(user, reading, FifteenMinutesOnWeekdays, Wed18, "same");
        var alpha = await CreateAsync(user, reading, FifteenMinutesOnWeekdays, Wed18, "alpha");
        var secondSame = await CreateAsync(user, reading, FifteenMinutesOnWeekdays, Wed18, "Same");

        var result = await ListAsync(user);

        Assert.Equal([alpha, beta, firstSame, secondSame], result.Goals.Select(g => g.Id));
        Assert.Equal(["alpha", "Beta", "same", "Same"], result.Goals.Select(g => g.Name));
    }

    [Fact]
    public async Task US2_AS1_a_custom_habit_goal_has_the_custom_kind()
    {
        var user = RegisteredUser(_world.Accounts);
        var habit = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));
        var created = await _world.Service.CreateAsync(user, Input(Custom(habit), Wed18, YesNoTimes(1, SchedulePeriod.Day)), CancellationToken.None);

        var item = await OnlyItemAsync(user);

        Assert.Equal(created.GoalId, item.Id);
        Assert.Equal("Cold shower", item.HabitName);
        Assert.Equal(HabitKind.Custom, item.HabitKind);
        Assert.Equal("Every day", item.ScheduleText);
        Assert.Null(item.StartDate);
        Assert.Null(item.NextCommitment);
        Assert.Null(item.FirstWeekStarts);
    }

    [Fact]
    public async Task US2_AS2_a_started_Mon_Wed_Fri_goal_is_next_today_on_Wednesday_and_next_Friday_on_Thursday()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        AddGoal(user, reading, OnDays(MonWedFri), (Mon16, Zone(GoalInputs.Kyiv)));

        var wednesday = await ListAsync(user);
        Assert.Equal(Wed18, wednesday.Today);
        var item = Assert.Single(wednesday.Goals);
        Assert.Equal("Mon · Wed · Fri", item.ScheduleText);
        Assert.Equal(Wed18, item.NextCommitment);
        Assert.Null(item.StartDate);

        // 12:00 in Kyiv on Thursday 19 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));
        var thursday = await ListAsync(user);
        Assert.Equal(Thu19, thursday.Today);
        Assert.Equal(Fri20, Assert.Single(thursday.Goals).NextCommitment);
    }

    [Fact]
    public async Task US2_AS2_Wednesday_is_next_until_its_last_second_in_the_goals_time_zone()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoDays(BuildOrReduce.Build, MonWedFri), Wed18);

        // 23:59:59 in Kyiv on Wednesday 18 November, then 00:00 on Thursday 19 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 21, 59, 59, TimeSpan.Zero));
        Assert.Equal(Wed18, (await OnlyItemAsync(user)).NextCommitment);
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 22, 0, 0, TimeSpan.Zero));
        Assert.Equal(Fri20, (await OnlyItemAsync(user)).NextCommitment);
    }

    [Fact]
    public async Task US2_AS3_a_per_day_goal_starting_later_gives_its_start_date_and_no_next_commitment()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoTimes(1, SchedulePeriod.Day), Fri20);

        var item = await OnlyItemAsync(user);

        Assert.Equal(Fri20, item.StartDate);
        Assert.Null(item.NextCommitment);
        Assert.Null(item.FirstWeekStarts);
    }

    [Fact]
    public async Task US2_AS3_a_Mon_Wed_Fri_goal_starting_on_a_later_Tuesday_gives_that_date_and_the_Wednesday_after_it()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoDays(BuildOrReduce.Build, MonWedFri), Tue24);

        var item = await OnlyItemAsync(user);

        Assert.Equal(Tue24, item.StartDate);
        Assert.Equal(Wed25, item.NextCommitment);
        Assert.Null(item.FirstWeekStarts);
    }

    [Fact]
    public async Task US2_AS3_the_start_date_goes_once_the_start_dates_day_begins_in_the_goals_time_zone()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoTimes(1, SchedulePeriod.Day), Thu19);

        // 23:59:59 in Kyiv on Wednesday 18 November, then 00:00 on Thursday 19 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 21, 59, 59, TimeSpan.Zero));
        Assert.Equal(Thu19, (await OnlyItemAsync(user)).StartDate);
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 22, 0, 0, TimeSpan.Zero));
        Assert.Null((await OnlyItemAsync(user)).StartDate);
    }

    [Fact]
    public async Task US4_AS4_a_per_week_goal_starting_on_a_Wednesday_gives_the_next_Monday_until_that_week_begins()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoTimes(3, SchedulePeriod.Week), Wed18);

        var started = await OnlyItemAsync(user);
        Assert.Equal(Mon23, started.FirstWeekStarts);
        Assert.Null(started.StartDate);
        Assert.Null(started.NextCommitment);

        // 23:59:59 in Kyiv on Sunday 22 November, then 00:00 on Monday 23 November.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 22, 21, 59, 59, TimeSpan.Zero));
        Assert.Equal(Mon23, (await OnlyItemAsync(user)).FirstWeekStarts);
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 22, 22, 0, 0, TimeSpan.Zero));
        Assert.Null((await OnlyItemAsync(user)).FirstWeekStarts);
    }

    [Fact]
    public async Task US4_AS4_a_per_week_goal_starting_on_a_later_Monday_gives_that_Monday_as_its_first_week_and_start_date()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading, YesNoTimes(3, SchedulePeriod.Week), Mon30);

        var item = await OnlyItemAsync(user);

        Assert.Equal(Mon30, item.FirstWeekStarts);
        Assert.Equal(Mon30, item.StartDate);
    }

    [Fact]
    public async Task US2_AS5_a_user_with_no_goals_gets_an_empty_list()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        await CreateAsync(RegisteredUser(_world.Accounts), reading, FifteenMinutesOnWeekdays, Wed18);

        var result = await ListAsync(RegisteredUser(_world.Accounts));

        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Empty(result.Goals);
        Assert.Equal(Wed18, result.Today);
    }

    [Fact]
    public async Task FR_014_after_a_move_west_a_commitment_already_over_is_not_given_as_next()
    {
        // Kyiv's Wednesday ended at 22:00 UTC; New York's Thursday, the next version's first day, starts at 05:00 UTC.
        // At 23:00 UTC it is still Wednesday in New York, the account's zone, but the Wednesday commitment is over.
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, "America/New_York");
        AddGoal(user, reading, OnDays(AllDays), (Mon16, Zone(GoalInputs.Kyiv)), (Thu19, NewYork));
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 23, 0, 0, TimeSpan.Zero));

        var result = await ListAsync(user);

        Assert.Equal(Wed18, result.Today);
        Assert.Equal(Thu19, Assert.Single(result.Goals).NextCommitment);
    }

    [Fact]
    public async Task FR_014_after_a_move_east_a_commitment_in_progress_is_not_skipped()
    {
        // New York's Wednesday ends at 05:00 UTC on Thursday; Tokyo's Thursday, the next version's first day, started at
        // 15:00 UTC on Wednesday. At 01:00 UTC on Thursday it is Thursday in Tokyo, the account's zone, but the
        // Wednesday commitment is still in progress.
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = RegisteredUser(_world.Accounts, "Asia/Tokyo");
        AddGoal(user, reading, OnDays(AllDays), (Mon16, NewYork), (Thu19, Tokyo));
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 1, 0, 0, TimeSpan.Zero));

        var result = await ListAsync(user);

        Assert.Equal(Thu19, result.Today);
        Assert.Equal(Wed18, Assert.Single(result.Goals).NextCommitment);
    }

    [Theory]
    [InlineData(GoalInputs.Kyiv, 2026, 11, 19)]
    [InlineData(null, 2026, 11, 18)]
    public async Task Today_is_the_accounts_date_at_the_supplied_time(string? zone, int year, int month, int day)
    {
        // 00:30 in Kyiv on Thursday 19 November is 22:30 on Wednesday 18 November in UTC, the installation's default.
        _world.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 22, 30, 0, TimeSpan.Zero));
        var user = zone is null ? RegisteredUserWithDefaultZone(_world.Accounts) : RegisteredUser(_world.Accounts, zone);

        Assert.Equal(new DateOnly(year, month, day), (await ListAsync(user)).Today);
    }

    [Fact]
    public async Task Another_users_goals_are_not_listed()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var owner = RegisteredUser(_world.Accounts);
        var other = RegisteredUser(_world.Accounts);
        var mine = await CreateAsync(owner, reading, FifteenMinutesOnWeekdays, Wed18);
        await CreateAsync(other, reading, FifteenMinutesOnWeekdays, Wed18);

        Assert.Equal(mine, (await OnlyItemAsync(owner)).Id);
    }

    [Fact]
    public async Task FR_023_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await ListAsync(ActingUser.Anonymous);

        Assert.Equal(GoalListOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Empty(result.Goals);
    }

    [Fact]
    public async Task FR_023_an_administrator_is_refused_before_the_account_is_looked_up()
    {
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { Administrator(_world.Accounts), unstored })
        {
            var result = await ListAsync(administrator);
            Assert.Equal(GoalListOutcome.NotForAdministrators, result.Outcome);
            Assert.Equal("Goals are for registered users.", result.Message);
            Assert.Empty(result.Goals);
        }
    }

    [Fact]
    public async Task FR_023_a_registered_user_whose_account_is_not_stored_is_not_signed_in()
    {
        var result = await ListAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser));

        Assert.Equal(GoalListOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
    }

    private void AddGoal(ActingUser user, CatalogHabit habit, GoalTarget target, params (DateOnly From, NamedTimeZone Zone)[] versions) =>
        _world.AddGoal(user.AccountId!.Value, habit, target, versions);
}
