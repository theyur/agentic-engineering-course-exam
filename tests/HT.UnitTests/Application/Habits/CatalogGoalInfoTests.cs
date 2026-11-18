using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;

namespace HT.UnitTests.Application.Habits;

/// <summary>
/// US6-AS1 to AS4, US3-AS6, FR-019, FR-031, FR-032: the catalog's goal information through the acting-user overloads
/// of <see cref="CatalogService"/>, over the goal fakes of <see cref="GoalWorld"/>. Goals are created through
/// <see cref="GoalService"/> with the start date today (Wednesday 18 November 2026 in Kyiv at the world's clock).
/// </summary>
public sealed class CatalogGoalInfoTests
{
    private readonly GoalWorld _world = new();

    private CatalogService Service => new(_world.CatalogHabits, _world.Goals);

    private static readonly GoalTargetFields ThreeTimesAWeek =
        HT.UnitTests.Domain.Goals.GoalSamples.YesNoTimes(3, SchedulePeriod.Week);

    private async Task<GoalId> CreateAsync(ActingUser user, CatalogHabit habit, string? name = null, GoalTargetFields? target = null)
    {
        var result = await _world.Service.CreateAsync(
            user, GoalInputs.Input(GoalInputs.Catalog(habit), GoalWorld.Today, target, name), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private Task<CatalogBrowseResult> BrowseAsync(ActingUser user, CatalogFilter? filter = null, bool onlyTracked = false) =>
        Service.BrowseAsync(user, filter ?? new CatalogFilter(), onlyTracked, CancellationToken.None);

    private Task<CatalogHabitDetailsResult> DetailsAsync(ActingUser user, CatalogHabit habit) =>
        Service.GetDetailsAsync(user, habit.Id, CancellationToken.None);

    private static IReadOnlyList<(string Name, int? GoalCount)> Counts(CatalogBrowseResult result) =>
        result.Habits.Select(h => (h.Name, h.GoalCount)).ToList();

    /// <summary>Reading (Mind, Build), Running (Body, Build), Screen time (Mind, Reduce), Yoga (Body, Build).</summary>
    private (CatalogHabit Reading, CatalogHabit Running, CatalogHabit ScreenTime, CatalogHabit Yoga) FourHabits() =>
    (
        _world.CatalogHabits.Add(HabitSamples.Reading()),
        _world.CatalogHabits.Add(HabitSamples.Catalog("Running", category: Category.Body)),
        _world.CatalogHabits.Add(HabitSamples.Catalog("Screen time", intent: Intent.Reduce)),
        _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body)));

    [Fact]
    public async Task US6_AS1_each_habit_carries_the_users_goal_count_and_the_result_says_how_many_they_track()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading);
        await CreateAsync(user, reading, "Morning pages");

        var result = await BrowseAsync(user);

        Assert.Equal(CatalogBrowseOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal([("Reading", (int?)2), ("Yoga", 0)], Counts(result));
        Assert.Equal(1, result.TrackedCount);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task US6_AS1_a_goal_on_a_custom_habit_counts_for_no_catalog_habit()
    {
        _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        var custom = _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body));
        var created = await _world.Service.CreateAsync(
            user, GoalInputs.Input(GoalInputs.Custom(custom), GoalWorld.Today, ThreeTimesAWeek), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);

        var result = await BrowseAsync(user);

        Assert.Equal([("Reading", (int?)0)], Counts(result));
        Assert.Equal(0, result.TrackedCount);
    }

    [Fact]
    public async Task US6_AS2_only_habits_I_track_keeps_the_other_filters()
    {
        var (reading, running, screenTime, _) = FourHabits();
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading);
        await CreateAsync(user, reading, "Morning pages");
        await CreateAsync(user, running, target: ThreeTimesAWeek);
        await CreateAsync(user, screenTime, target: ThreeTimesAWeek);

        var all = await BrowseAsync(user, onlyTracked: true);
        var mind = await BrowseAsync(user, new CatalogFilter { Category = Category.Mind }, onlyTracked: true);
        var build = await BrowseAsync(user, new CatalogFilter { Intent = Intent.Build }, onlyTracked: true);
        var searched = await BrowseAsync(user, new CatalogFilter { Search = "run" }, onlyTracked: true);
        var nothing = await BrowseAsync(user, new CatalogFilter { Search = "yoga" }, onlyTracked: true);

        Assert.Equal(CatalogBrowseOutcome.Found, all.Outcome);
        Assert.Equal([("Reading", (int?)2), ("Running", 1), ("Screen time", 1)], Counts(all));
        Assert.Equal(3, all.TrackedCount);
        Assert.Equal([("Reading", (int?)2), ("Screen time", 1)], Counts(mind));
        Assert.Equal([("Reading", (int?)2), ("Running", 1)], Counts(build));
        Assert.Equal([("Running", (int?)1)], Counts(searched));
        Assert.Equal(1, searched.TrackedCount);
        Assert.Empty(nothing.Habits);
        Assert.Equal(0, nothing.TrackedCount);
    }

    [Fact]
    public async Task US6_AS2_only_Published_habits_are_returned_with_only_habits_I_track()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var stretching = _world.CatalogHabits.Add(HabitSamples.Catalog("Stretching", CatalogHabitState.Unpublished));
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading);
        _world.AddGoal(user.AccountId!.Value, stretching, HT.UnitTests.Domain.Goals.GoalSamples.PerWeek(),
            (GoalWorld.Today, HT.UnitTests.Domain.Goals.GoalSamples.Kyiv));

        var result = await BrowseAsync(user, onlyTracked: true);

        Assert.Equal([("Reading", (int?)1)], Counts(result));
        Assert.Equal(1, result.TrackedCount);
    }

    [Fact]
    public async Task US6_AS3_the_details_give_the_users_goal_count_and_goals_in_name_order()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var yoga = _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        var derived = await CreateAsync(user, reading);
        var named = await CreateAsync(user, reading, "Morning pages");
        await CreateAsync(user, yoga, target: ThreeTimesAWeek);

        var result = await DetailsAsync(user, reading);

        Assert.Equal(CatalogHabitDetailsOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        var details = result.Details!;
        Assert.Equal("Reading", details.Name);
        Assert.Equal(2, details.GoalCount);
        Assert.Equal([new GoalOnHabit(named, "Morning pages"), new GoalOnHabit(derived, "Reading, 15 min, 5 days a week")], details.Goals);
    }

    [Fact]
    public async Task US6_AS3_goals_with_the_same_name_come_in_creation_order_and_the_name_key_ignores_case()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        var first = await CreateAsync(user, reading);
        var lower = await CreateAsync(user, reading, "morning pages");
        var second = await CreateAsync(user, reading);

        var details = (await DetailsAsync(user, reading)).Details!;

        Assert.Equal(3, details.GoalCount);
        Assert.Equal([lower, first, second], details.Goals!.Select(g => g.Id));
    }

    [Fact]
    public async Task US6_AS3_a_habit_without_goals_of_the_user_gives_0_and_no_goals()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var user = GoalInputs.RegisteredUser(_world.Accounts);

        var details = (await DetailsAsync(user, reading)).Details!;

        Assert.Equal(0, details.GoalCount);
        Assert.NotNull(details.Goals);
        Assert.Empty(details.Goals);
    }

    [Fact]
    public async Task US6_AS3_a_habit_not_in_the_catalog_gives_the_slice_2_refusal_to_a_registered_user()
    {
        var stretching = _world.CatalogHabits.Add(HabitSamples.Catalog("Stretching", CatalogHabitState.Unpublished));
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        _world.AddGoal(user.AccountId!.Value, stretching, HT.UnitTests.Domain.Goals.GoalSamples.PerWeek(),
            (GoalWorld.Today, HT.UnitTests.Domain.Goals.GoalSamples.Kyiv));

        var result = await DetailsAsync(user, stretching);

        Assert.Equal(CatalogHabitDetailsOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
        Assert.Null(result.Details);
    }

    public static TheoryData<string> OtherAudiences => new("visitor", "administrator");

    private ActingUser Audience(string audience) =>
        audience == "visitor" ? ActingUser.Anonymous : GoalInputs.Administrator(_world.Accounts);

    [Theory]
    [MemberData(nameof(OtherAudiences))]
    public async Task US6_AS4_a_visitor_and_an_administrator_get_the_slice_2_content_with_no_goal_information(string audience)
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _world.CatalogHabits.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var owner = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(owner, reading);
        var user = Audience(audience);

        var browsed = await BrowseAsync(user);
        var details = await DetailsAsync(user, reading);

        Assert.Equal(CatalogBrowseOutcome.Found, browsed.Outcome);
        Assert.Null(browsed.Message);
        Assert.Null(browsed.TrackedCount);
        Assert.Equal([("Reading", (int?)null), ("Yoga", null)], Counts(browsed));
        Assert.Equal((await Service.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits, browsed.Habits);
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        Assert.Null(details.Details!.GoalCount);
        Assert.Null(details.Details.Goals);
        Assert.Equivalent(await Service.GetDetailsAsync(reading.Id, CancellationToken.None), details, strict: true);
    }

    [Fact]
    public async Task US6_AS4_only_habits_I_track_is_refused_for_a_visitor()
    {
        _world.CatalogHabits.Add(HabitSamples.Reading());

        var result = await BrowseAsync(ActingUser.Anonymous, onlyTracked: true);

        Assert.Equal(CatalogBrowseOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("NotSignedIn", result.Outcome.ToString());
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Empty(result.Habits);
        Assert.Null(result.TrackedCount);
    }

    [Fact]
    public async Task US6_AS4_only_habits_I_track_is_refused_for_an_administrator()
    {
        _world.CatalogHabits.Add(HabitSamples.Reading());

        var result = await BrowseAsync(GoalInputs.Administrator(_world.Accounts), new CatalogFilter { Search = "nothing matches" }, onlyTracked: true);

        Assert.Equal(CatalogBrowseOutcome.NotForAdministrators, result.Outcome);
        Assert.Equal("NotForAdministrators", result.Outcome.ToString());
        Assert.Equal("Goals are for registered users.", result.Message);
        Assert.Empty(result.Habits);
        Assert.Null(result.TrackedCount);
    }

    [Fact]
    public async Task US3_AS6_another_users_goals_count_for_nothing_and_are_never_returned()
    {
        var reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        var a = GoalInputs.RegisteredUser(_world.Accounts);
        var b = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(a, reading);
        await CreateAsync(a, reading, "Morning pages");

        var browsed = await BrowseAsync(b);
        var tracked = await BrowseAsync(b, onlyTracked: true);
        var details = (await DetailsAsync(b, reading)).Details!;

        Assert.Equal([("Reading", (int?)0)], Counts(browsed));
        Assert.Equal(0, browsed.TrackedCount);
        Assert.Empty(tracked.Habits);
        Assert.Equal(0, details.GoalCount);
        Assert.Empty(details.Goals!);
        Assert.Equal(2, (await DetailsAsync(a, reading)).Details!.GoalCount);
    }

    [Fact]
    public async Task FR_019_the_slice_2_overloads_give_the_slice_2_results_whoever_has_goals()
    {
        var (reading, running, _, _) = FourHabits();
        var user = GoalInputs.RegisteredUser(_world.Accounts);
        await CreateAsync(user, reading);
        await CreateAsync(user, running, target: ThreeTimesAWeek);
        var filter = new CatalogFilter { Category = Category.Body };

        var browsed = await Service.BrowseAsync(filter, CancellationToken.None);
        var details = await Service.GetDetailsAsync(reading.Id, CancellationToken.None);
        var asUser = await BrowseAsync(user, filter);

        Assert.Equal(CatalogBrowseOutcome.Found, browsed.Outcome);
        Assert.Null(browsed.Message);
        Assert.Null(browsed.TrackedCount);
        Assert.Equal(["Running", "Yoga"], browsed.Habits.Select(h => h.Name));
        Assert.All(browsed.Habits, h => Assert.Null(h.GoalCount));
        Assert.Equal(browsed.Habits, asUser.Habits.Select(h => h with { GoalCount = null }));
        Assert.Null(details.Details!.GoalCount);
        Assert.Null(details.Details.Goals);
        Assert.Equivalent(await Service.GetDetailsAsync(ActingUser.Anonymous, reading.Id, CancellationToken.None), details, strict: true);
    }
}
