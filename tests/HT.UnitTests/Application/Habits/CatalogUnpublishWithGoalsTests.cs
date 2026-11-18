using System.Reflection;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Habits.CatalogInputs;

namespace HT.UnitTests.Application.Habits;

/// <summary>
/// US7-AS1 to AS5, FR-019, FR-023, FR-033 and the spec's edge case: unpublishing a catalog habit that goals use,
/// through <see cref="CatalogAdministrationService"/> over the goal fakes of <see cref="GoalWorld"/>. Goals are created
/// through <see cref="GoalService"/> with the start date today, by accounts the world's account store holds.
/// </summary>
public sealed class CatalogUnpublishWithGoalsTests
{
    private readonly GoalWorld _world = new();

    private CatalogAdministrationService Service => new(_world.CatalogHabits, _world.Goals);

    private async Task CreateGoalAsync(ActingUser user, CatalogHabit habit, string? name = null)
    {
        var result = await _world.Service.CreateAsync(
            user, GoalInputs.Input(GoalInputs.Catalog(habit), GoalWorld.Today, name: name), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
    }

    private ActingUser NewUser() => GoalInputs.RegisteredUser(_world.Accounts);

    /// <summary>Reading, Published, with a goal of each of two users.</summary>
    private async Task<CatalogHabit> ReadingWithTwoGoalsAsync()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        await CreateGoalAsync(NewUser(), reading, "Morning pages");
        await CreateGoalAsync(NewUser(), reading);
        return reading;
    }

    private CatalogHabitState StateOf(CatalogHabit habit) => _world.CatalogHabits.Get(habit.Id)!.State;

    [Fact]
    public async Task US7_AS1_two_goals_need_confirmation_with_the_count_and_its_text_and_the_habit_stays_Published()
    {
        var reading = await ReadingWithTwoGoalsAsync();

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: false, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, result.Outcome);
        Assert.Equal(2, result.GoalCount);
        Assert.Equal("2 goals use this habit.", result.Message);
        Assert.Equal(CatalogHabitState.Published, StateOf(reading));
        Assert.Equal(0, _world.CatalogHabits.SwapCount);
        Assert.Equal(0, _world.CatalogHabits.WriteCount);
    }

    [Fact]
    public async Task US7_AS2_with_confirmation_the_habit_is_Unpublished_and_the_goals_stay()
    {
        var reading = await ReadingWithTwoGoalsAsync();

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: true, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Null(result.Message);
        Assert.Null(result.GoalCount);
        Assert.Equal(CatalogHabitState.Unpublished, StateOf(reading));
        Assert.Equal(2, _world.Goals.Goals.Count);
    }

    [Fact]
    public async Task US7_AS3_one_goal_gives_the_singular_text()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        await CreateGoalAsync(NewUser(), reading);

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: false, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, result.Outcome);
        Assert.Equal(1, result.GoalCount);
        Assert.Equal("1 goal uses this habit.", result.Message);
        Assert.Equal(CatalogHabitState.Published, StateOf(reading));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task US7_AS4_a_habit_no_goal_uses_is_unpublished_at_once_with_or_without_confirmation(bool confirmed)
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var yoga = _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var user = NewUser();
        await CreateGoalAsync(user, yoga);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Reading", Category.Mind));
        var onCustom = await _world.Service.CreateAsync(
            user, GoalInputs.Input(GoalInputs.Custom(custom), GoalWorld.Today), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, onCustom.Outcome);

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Null(result.Message);
        Assert.Null(result.GoalCount);
        Assert.Equal(CatalogHabitState.Unpublished, StateOf(reading));
        Assert.Equal(CatalogHabitState.Published, StateOf(yoga));
    }

    [Fact]
    public void US7_AS5_the_result_has_only_the_outcome_the_message_and_the_goal_count()
    {
        var members = typeof(CatalogHabitUnpublishResult)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m is not MethodInfo { IsSpecialName: true })
            .Select(m => m.Name)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(["GoalCount", "Message", "Outcome"], members);
        Assert.Equal(
            ["GoalCount", "Message", "Outcome"],
            typeof(CatalogHabitUnpublishResult).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.All(typeof(CatalogHabitUnpublishResult).GetProperties(), p => Assert.False(p.SetMethod?.IsPublic ?? false));
    }

    [Fact]
    public async Task US7_AS5_the_confirmation_carries_no_owner_email_or_goal_name()
    {
        var reading = await ReadingWithTwoGoalsAsync();

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: false, CancellationToken.None);

        Assert.Equal("2 goals use this habit.", result.Message);
        Assert.DoesNotContain("Morning pages", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("@", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FR_019_the_slice_2_overload_means_not_confirmed()
    {
        var reading = await ReadingWithTwoGoalsAsync();

        var result = await Service.UnpublishAsync(Administrator, reading.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, result.Outcome);
        Assert.Equal(2, result.GoalCount);
        Assert.Equal("2 goals use this habit.", result.Message);
        Assert.Equal(CatalogHabitState.Published, StateOf(reading));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FR_023_the_acting_user_comes_before_the_count(bool confirmed)
    {
        var reading = await ReadingWithTwoGoalsAsync();

        var anonymous = await Service.UnpublishAsync(ActingUser.Anonymous, reading.Id, confirmed, CancellationToken.None);
        var registered = await Service.UnpublishAsync(RegisteredUser, reading.Id, confirmed, CancellationToken.None);

        Assert.Equal(
            [("NotSignedIn", "You are not signed in.", (int?)null), ("NotAdministrator", "Only administrators can manage the catalog.", null)],
            new[] { anonymous, registered }.Select(r => (r.Outcome.ToString(), r.Message, r.GoalCount)));
        Assert.Equal(CatalogHabitState.Published, StateOf(reading));
        Assert.Equal(0, _world.CatalogHabits.SwapCount);
    }

    [Fact]
    public async Task FR_023_an_unknown_identifier_does_not_exist_before_any_count()
    {
        var result = await Service.UnpublishAsync(Administrator, new HabitId(Guid.NewGuid()), confirmed: false, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.NotFound, result.Outcome);
        Assert.Equal("This catalog habit does not exist.", result.Message);
        Assert.Null(result.GoalCount);
    }

    [Fact]
    public async Task FR_023_the_state_rule_comes_before_the_count_for_a_Draft_and_an_Unpublished_habit()
    {
        var draft = _world.CatalogHabits.Add(HabitSamples.Catalog("Stretching", CatalogHabitState.Draft));
        var unpublished = _world.CatalogHabits.Add(HabitSamples.Catalog("Sugar", CatalogHabitState.Unpublished, intent: Intent.Reduce));
        var owner = NewUser().AccountId!.Value;
        var zone = HT.UnitTests.Domain.Goals.GoalSamples.Kyiv;
        var target = HT.UnitTests.Domain.Goals.GoalSamples.PerDay();
        _world.AddGoal(owner, draft, target, (GoalWorld.Today, zone));
        _world.AddGoal(owner, unpublished, target, (GoalWorld.Today, zone));

        var onDraft = await Service.UnpublishAsync(Administrator, draft.Id, confirmed: false, CancellationToken.None);
        var onUnpublished = await Service.UnpublishAsync(Administrator, unpublished.Id, confirmed: false, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.NotPublished, onDraft.Outcome);
        Assert.Equal("This catalog habit is not published.", onDraft.Message);
        Assert.Null(onDraft.GoalCount);
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, onUnpublished.Outcome);
        Assert.Null(onUnpublished.GoalCount);
        Assert.Equal(CatalogHabitState.Draft, StateOf(draft));
        Assert.Equal(CatalogHabitState.Unpublished, StateOf(unpublished));
        Assert.Equal(0, _world.CatalogHabits.SwapCount);
    }

    [Fact]
    public async Task A_goal_created_between_the_count_and_the_confirmation_does_not_stop_the_confirmed_unpublish()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        await CreateGoalAsync(NewUser(), reading);
        var shown = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: false, CancellationToken.None);
        Assert.Equal(1, shown.GoalCount);

        await CreateGoalAsync(NewUser(), reading);
        var confirmed = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: true, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, confirmed.Outcome);
        Assert.Equal(CatalogHabitState.Unpublished, StateOf(reading));
        Assert.Equal(2, _world.Goals.Goals.Count);
    }

    [Fact]
    public async Task A_confirmed_unpublish_keeps_slice_2s_compare_and_swap_through_lost_races()
    {
        var reading = await ReadingWithTwoGoalsAsync();
        _world.CatalogHabits.FailNextStateSwaps(9);

        var result = await Service.UnpublishAsync(Administrator, reading.Id, confirmed: true, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Equal(10, _world.CatalogHabits.SwapCount);
        Assert.Equal(CatalogHabitState.Unpublished, StateOf(reading));
    }
}
