using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// US3, SC-002, FR-020 to FR-023: who may use which goal, for every <see cref="GoalService"/> method
/// (<c>StartNewGoalAsync</c>, <c>CreateAsync</c>, <c>ListAsync</c>, <c>GetAsync</c>, <c>GetCalendarAsync</c>). User A and
/// user B both have goals, A's on a catalog habit and on A's own custom habit. Each refusal is called with inputs that
/// would also fail every later check of FR-023's order, so the earlier refusal is shown to win. The later steps of that
/// order are tested with each method: the suggestion position after the habit in <c>GoalStartingValuesTests</c>, the
/// fields after the habit in <c>GoalCreateTests</c>, and the range after the goal in <c>GoalCalendarTests</c>.
/// </summary>
public sealed class GoalAccessTests
{
    private static readonly DateOnly Wed18 = GoalWorld.Today;

    private readonly GoalWorld _world = new();
    private readonly ActingUser _a;
    private readonly ActingUser _b;
    private readonly CatalogHabit _reading;
    private readonly CustomHabit _aCustom;
    private readonly CustomHabit _bCustom;

    public GoalAccessTests()
    {
        _reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _a = RegisteredUser(_world.Accounts);
        _b = RegisteredUser(_world.Accounts);
        _aCustom = _world.CustomHabits.Add(_a.AccountId!.Value, HabitSamples.Definition("Zq A stretch", Category.Body));
        _bCustom = _world.CustomHabits.Add(_b.AccountId!.Value, HabitSamples.Definition("Zq B stretch", Category.Body));
    }

    private static CancellationToken None => CancellationToken.None;

    private async Task<GoalId> CreateAsync(ActingUser user, GoalHabit habit, string? name = null)
    {
        var created = await _world.Service.CreateAsync(user, Input(habit, Wed18, name: name), None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>
    /// An input that fails every check after the acting user: another account's custom habit, no target, a start date
    /// in the past and a name of 61 characters.
    /// </summary>
    private GoalInput FailingEverywhere() => new()
    {
        Habit = Custom(_bCustom), Target = new GoalTargetFields(), StartDate = Wed18.AddDays(-1), Name = new string('x', 61),
    };

    /// <summary>One call of each <see cref="GoalService"/> method: its outcome's name, its text and whether it returned goal data.</summary>
    private sealed record Call(string UseCase, string Outcome, string? Message, bool HasData);

    /// <summary>
    /// Every method, each with arguments that fail every later check: B's custom habit with a suggestion position it does
    /// not have, <see cref="FailingEverywhere"/>, A's goal, and a range whose last date is before its first.
    /// </summary>
    private async Task<IReadOnlyList<Call>> EveryUseCaseAsync(ActingUser user, GoalId goal)
    {
        var service = _world.Service;
        var started = await service.StartNewGoalAsync(user, Custom(_bCustom), 9, None);
        var created = await service.CreateAsync(user, FailingEverywhere(), None);
        var listed = await service.ListAsync(user, None);
        var read = await service.GetAsync(user, goal, None);
        var calendar = await service.GetCalendarAsync(user, goal, Wed18, Wed18.AddDays(-1), None);
        return
        [
            new(nameof(service.StartNewGoalAsync), started.Outcome.ToString(), started.Message, started.StartingValues is not null),
            new(nameof(service.CreateAsync), created.Outcome.ToString(), created.Message, created.GoalId is not null),
            new(nameof(service.ListAsync), listed.Outcome.ToString(), listed.Message, listed.Goals.Count > 0),
            new(nameof(service.GetAsync), read.Outcome.ToString(), read.Message, read.Goal is not null),
            new(nameof(service.GetCalendarAsync), calendar.Outcome.ToString(), calendar.Message, calendar.Calendar is not null),
        ];
    }

    private static void AssertEveryUseCaseRefused(IReadOnlyList<Call> calls, string outcome, string message)
    {
        Assert.Equal(
            ["StartNewGoalAsync", "CreateAsync", "ListAsync", "GetAsync", "GetCalendarAsync"],
            calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal(outcome, call.Outcome);
            Assert.Equal(message, call.Message);
            Assert.False(call.HasData);
        });
    }

    [Fact]
    public async Task US3_AS4_an_anonymous_visitor_is_not_signed_in_on_every_goal_use_case_before_any_other_check()
    {
        var aGoal = await CreateAsync(_a, Catalog(_reading));
        var goals = _world.Goals.Goals.Count;
        var creates = _world.Goals.CreateCount;

        var calls = await EveryUseCaseAsync(ActingUser.Anonymous, aGoal);

        AssertEveryUseCaseRefused(calls, "NotSignedIn", "You are not signed in.");
        Assert.Equal(goals, _world.Goals.Goals.Count);
        Assert.Equal(creates, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task US3_AS3_an_administrator_gets_the_role_refusal_and_no_goal_data_on_every_goal_use_case_while_other_accounts_have_goals()
    {
        var aGoal = await CreateAsync(_a, Catalog(_reading));
        await CreateAsync(_a, Custom(_aCustom));
        await CreateAsync(_b, Custom(_bCustom));
        var goals = _world.Goals.Goals.Count;
        var creates = _world.Goals.CreateCount;
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { Administrator(_world.Accounts), unstored })
        {
            var calls = await EveryUseCaseAsync(administrator, aGoal);

            AssertEveryUseCaseRefused(calls, "NotForAdministrators", "Goals are for registered users.");
        }

        Assert.Equal(goals, _world.Goals.Goals.Count);
        Assert.Equal(creates, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task FR_023_a_registered_user_whose_account_is_not_stored_is_not_signed_in_before_any_identifier_check()
    {
        var aGoal = await CreateAsync(_a, Catalog(_reading));
        var creates = _world.Goals.CreateCount;

        var calls = await EveryUseCaseAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), aGoal);

        AssertEveryUseCaseRefused(calls, "NotSignedIn", "You are not signed in.");
        Assert.Equal(creates, _world.Goals.CreateCount);
    }

    [Fact]
    public async Task US3_AS1_B_reading_As_goal_or_its_calendar_gets_the_same_result_as_for_an_unknown_identifier()
    {
        var aGoal = await CreateAsync(_a, Catalog(_reading), "Zq A private goal");
        var unknown = new GoalId(Guid.NewGuid());

        foreach (var id in new[] { aGoal, unknown })
        {
            var read = await _world.Service.GetAsync(_b, id, None);
            var calendar = await _world.Service.GetCalendarAsync(_b, id, Wed18, Wed18.AddDays(13), None);

            Assert.Equal(GoalReadOutcome.NotFound, read.Outcome);
            Assert.Equal("This goal does not exist.", read.Message);
            Assert.Null(read.Goal);
            Assert.Equal(GoalCalendarOutcome.NotFound, calendar.Outcome);
            Assert.Equal("This goal does not exist.", calendar.Message);
            Assert.Null(calendar.Calendar);
        }
    }

    [Fact]
    public async Task FR_023_the_goal_identifier_is_checked_before_the_calendar_range()
    {
        var aGoal = await CreateAsync(_a, Catalog(_reading));

        foreach (var last in new[] { Wed18.AddDays(-1), Wed18.AddDays(366) })
        {
            var calendar = await _world.Service.GetCalendarAsync(_b, aGoal, Wed18, last, None);

            Assert.Equal(GoalCalendarOutcome.NotFound, calendar.Outcome);
            Assert.Equal("This goal does not exist.", calendar.Message);
        }
    }

    [Fact]
    public async Task US3_AS2_Bs_list_holds_only_Bs_goals_and_none_of_As()
    {
        var aGoals = new[]
        {
            await CreateAsync(_a, Catalog(_reading)),
            await CreateAsync(_a, Custom(_aCustom), "Zq A private goal"),
        };
        var bGoal = await CreateAsync(_b, Catalog(_reading));

        var bList = await _world.Service.ListAsync(_b, None);
        var aList = await _world.Service.ListAsync(_a, None);

        Assert.Equal(GoalListOutcome.Found, bList.Outcome);
        Assert.Equal([bGoal], bList.Goals.Select(g => g.Id));
        Assert.DoesNotContain(bList.Goals, g => g.Name.Contains("Zq A", StringComparison.Ordinal) || g.HabitName.Contains("Zq A", StringComparison.Ordinal));
        Assert.Equal(aGoals.ToHashSet(), aList.Goals.Select(g => g.Id).ToHashSet());
    }

    [Fact]
    public async Task US3_AS5_A_starting_or_creating_a_goal_on_Bs_custom_habit_gets_the_same_refusal_as_for_an_unknown_one_and_nothing_is_created()
    {
        await CreateAsync(_b, Custom(_bCustom));
        var goals = _world.Goals.Goals.Count;

        foreach (var habit in new[] { Custom(_bCustom), new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Custom) })
        {
            // A position the habit does not have, and fields that all fail: the habit is checked first.
            var started = await _world.Service.StartNewGoalAsync(_a, habit, 9, None);
            var created = await _world.Service.CreateAsync(_a, FailingEverywhere() with { Habit = habit }, None);

            Assert.Equal(GoalStartOutcome.CustomHabitNotFound, started.Outcome);
            Assert.Equal("This custom habit does not exist.", started.Message);
            Assert.Null(started.StartingValues);
            Assert.Equal(GoalCreateOutcome.CustomHabitNotFound, created.Outcome);
            Assert.Equal("This custom habit does not exist.", created.Message);
            Assert.Equal(GoalField.Habit, created.Field);
            Assert.Null(created.GoalId);
        }

        Assert.Equal(goals, _world.Goals.Goals.Count);
        Assert.Empty((await _world.Service.ListAsync(_a, None)).Goals);
    }
}
