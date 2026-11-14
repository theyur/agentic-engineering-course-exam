using HT.Application.Habits;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Habits.CatalogInputs;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>US3: creating, editing, listing, names and every field refusal with its text and position.</summary>
public sealed class CatalogHabitFieldTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private CatalogAdministrationService Service => new(_store);
    private CatalogService Browse => new(_store);

    private static readonly SuggestedStartingGoalFields Valid = Count(20, "pages", 3, SchedulePeriod.Week);

    private async Task<IReadOnlyList<string>> PublicNamesAsync() =>
        (await Browse.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits.Select(h => h.Name).ToList();

    [Fact]
    public async Task US3_AS1_create_gives_a_Draft_listed_by_the_administrator_and_not_browsed()
    {
        var result = await Service.CreateAsync(Administrator,
            Input("Ac10 Stretching", Category.Body, Intent.Build, "person-standing", "Gentle stretching.", (YesNo(1, SchedulePeriod.Day), true)),
            CancellationToken.None);

        Assert.Equal(CatalogHabitCreateOutcome.Created, result.Outcome);
        Assert.Null(result.Message);
        Assert.Null(result.Position);
        var id = result.HabitId!.Value;
        Assert.Equal(CatalogHabitState.Draft, _store.Get(id)!.State);
        var item = Assert.Single((await Service.ListAsync(Administrator, null, CancellationToken.None)).Habits);
        Assert.Equal(("Ac10 Stretching", CatalogHabitState.Draft, Category.Body, Intent.Build), (item.Name, item.State, item.Category, item.Intent));
        Assert.Empty(await PublicNamesAsync());
        var read = (await Service.GetAsync(Administrator, id, CancellationToken.None)).Habit!;
        Assert.Equal("Gentle stretching.", read.Description);
        Assert.Equal("person-standing", read.Icon);
        Assert.Equal("once a day", Assert.Single(read.SuggestedStartingGoals).Text);
    }

    [Fact]
    public async Task US3_AS3_an_edit_of_a_Published_habit_is_visible_on_the_next_read()
    {
        var habit = _store.Add(Catalog("Ac12 Stretching", category: Category.Body));

        var result = await Service.EditAsync(Administrator, habit.Id, Input("Ac12 Morning stretch", Category.Health), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.Saved, result.Outcome);
        Assert.Null(result.Message);
        var browsed = Assert.Single((await Browse.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits);
        Assert.Equal(("Ac12 Morning stretch", Category.Health), (browsed.Name, browsed.Category));
        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
    }

    [Fact]
    public async Task US3_AS14_two_suggested_starting_goals_the_second_marked_and_shown_after_publish()
    {
        var habit = _store.Add(Catalog("Ac22 Reading", CatalogHabitState.Draft));

        var edit = await Service.EditAsync(Administrator, habit.Id, Input("Ac22 Reading",
            goals: [(Count(20, "pages", 3, SchedulePeriod.Week), false), (DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce), true)]),
            CancellationToken.None);
        Assert.Equal(CatalogHabitEditOutcome.Saved, edit.Outcome);

        var read = (await Service.GetAsync(Administrator, habit.Id, CancellationToken.None)).Habit!;
        Assert.Equal(["20 pages, 3 times a week", "at most 2 h a day"], read.SuggestedStartingGoals.Select(g => g.Text));
        Assert.Equal([false, true], read.SuggestedStartingGoals.Select(g => g.IsMarked));

        await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None);
        var browsed = Assert.Single((await Browse.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits);
        Assert.Equal("at most 2 h a day", browsed.MarkedSuggestedStartingGoal!.Text);
    }

    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Published)]
    [InlineData(CatalogHabitState.Unpublished)]
    public async Task US3_AS9_a_name_taken_in_any_state_is_refused_on_create_and_rename(CatalogHabitState state)
    {
        _store.Add(Catalog("Ac17 Reading", state));
        var other = _store.Add(Catalog("Ac17 Other", CatalogHabitState.Draft));

        var create = await Service.CreateAsync(Administrator, Input(" ac17 reading "), CancellationToken.None);
        var rename = await Service.EditAsync(Administrator, other.Id, Input(" ac17 reading "), CancellationToken.None);

        Assert.Equal(CatalogHabitCreateOutcome.NameTaken, create.Outcome);
        Assert.Equal("A catalog habit with this name already exists.", create.Message);
        Assert.Equal(CatalogHabitEditOutcome.NameTaken, rename.Outcome);
        Assert.Equal("A catalog habit with this name already exists.", rename.Message);
        Assert.Equal(0, _store.WriteCount);
        Assert.Equal("Ac17 Other", _store.Get(other.Id)!.Definition.Name.Value);
    }

    [Fact]
    public async Task US3_AS9_renaming_a_habit_to_its_own_name_in_other_case_is_saved()
    {
        var habit = _store.Add(Catalog("Ac17 Reading"));

        var result = await Service.EditAsync(Administrator, habit.Id, Input("AC17 READING"), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.Saved, result.Outcome);
        Assert.Equal("AC17 READING", _store.Get(habit.Id)!.Definition.Name.Value);
    }

    public static TheoryData<CatalogHabitInput, CatalogHabitCreateOutcome, string> FieldRefusals => new()
    {
        { Input("   "), CatalogHabitCreateOutcome.NameMissing, "Enter a name." },
        { Input(new string('x', 61)), CatalogHabitCreateOutcome.NameTooLong, "The name can have at most 60 characters." },
        { Input(description: new string('t', 501)), CatalogHabitCreateOutcome.DescriptionTooLong, "The description can have at most 500 characters." },
        { Input(category: null), CatalogHabitCreateOutcome.CategoryMissing, "Choose a category." },
        { Input(intent: null), CatalogHabitCreateOutcome.IntentMissing, "Choose Build or Reduce." },
        { Input(icon: null), CatalogHabitCreateOutcome.IconMissing, "Choose an icon." },
        { Input(icon: "menu"), CatalogHabitCreateOutcome.IconMissing, "Choose an icon." },
    };

    [Theory]
    [MemberData(nameof(FieldRefusals))]
    public async Task US3_AS10_each_field_refusal_and_nothing_is_created(CatalogHabitInput input, CatalogHabitCreateOutcome outcome, string message)
    {
        var result = await Service.CreateAsync(Administrator, input, CancellationToken.None);

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Position);
        Assert.Null(result.HabitId);
        Assert.Empty(_store.Habits);
    }

    [Fact]
    public async Task US3_AS10_a_name_of_60_and_a_description_of_500_are_accepted()
    {
        var result = await Service.CreateAsync(Administrator, Input(new string('x', 60), description: new string('t', 500)), CancellationToken.None);

        Assert.Equal(CatalogHabitCreateOutcome.Created, result.Outcome);
    }

    [Fact]
    public async Task US3_AS15_the_list_refusals_count_first_then_the_mark()
    {
        var habit = _store.Add(Catalog("Ac23 Reading"));
        async Task<CatalogHabitEditResult> EditAsync(params (SuggestedStartingGoalFields, bool)[] goals) =>
            await Service.EditAsync(Administrator, habit.Id, Input("Ac23 Reading", goals: goals), CancellationToken.None);

        var none = await EditAsync((Valid, false), (Valid, false));
        var both = await EditAsync((Valid, true), (Valid, true));
        var six = await EditAsync(Enumerable.Repeat((Valid, false), 6).ToArray());
        var five = await EditAsync(Enumerable.Range(0, 5).Select(i => (Valid, i == 2)).ToArray());

        Assert.Equal(CatalogHabitEditOutcome.SuggestedStartingGoalNotMarked, none.Outcome);
        Assert.Equal("Mark one suggested starting goal for the card.", none.Message);
        Assert.Null(none.Position);
        Assert.Equal(CatalogHabitEditOutcome.SuggestedStartingGoalNotMarked, both.Outcome);
        Assert.Equal(CatalogHabitEditOutcome.TooManySuggestedStartingGoals, six.Outcome);
        Assert.Equal("A habit can have at most 5 suggested starting goals.", six.Message);
        Assert.Equal(CatalogHabitEditOutcome.Saved, five.Outcome);
        Assert.Equal(1, _store.WriteCount);
    }

    public static TheoryData<SuggestedStartingGoalFields, CatalogHabitEditOutcome, string> ItemRefusals => new()
    {
        { Valid with { BuildOrReduce = null }, CatalogHabitEditOutcome.BuildOrReduceMissing, "Choose Build or Reduce." },
        { Valid with { Measurement = null }, CatalogHabitEditOutcome.MeasurementMissing, "Choose how it is measured." },
        { YesNo(1, SchedulePeriod.Day) with { Amount = 1 }, CatalogHabitEditOutcome.YesNoHasAmountOrUnit, "A yes/no suggested starting goal has no amount or unit." },
        { YesNo(1, SchedulePeriod.Day) with { CountUnit = "x" }, CatalogHabitEditOutcome.YesNoHasAmountOrUnit, "A yes/no suggested starting goal has no amount or unit." },
        { Valid with { Amount = null }, CatalogHabitEditOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999." },
        { Valid with { Amount = 0 }, CatalogHabitEditOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999." },
        { Valid with { Amount = 1000 }, CatalogHabitEditOutcome.BuildAmountOutOfRange, "Enter a whole number from 1 to 999." },
        { Valid with { BuildOrReduce = BuildOrReduce.Reduce, Amount = 1000 }, CatalogHabitEditOutcome.ReduceAmountOutOfRange, "Enter a whole number from 0 to 999." },
        { Valid with { CountUnit = "  " }, CatalogHabitEditOutcome.CountUnitMissing, "Enter a unit, for example pages." },
        { Valid with { CountUnit = new string('u', 21) }, CatalogHabitEditOutcome.CountUnitTooLong, "The unit can have at most 20 characters." },
        { DurationTimes(15, DurationUnit.Minutes, 1, SchedulePeriod.Day) with { DurationUnit = null }, CatalogHabitEditOutcome.DurationUnitMissing, "Choose minutes or hours." },
        { Valid with { ScheduleKind = null }, CatalogHabitEditOutcome.ScheduleMissing, "Choose fixed days or how many times." },
        { Valid with { ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek>() }, CatalogHabitEditOutcome.DaysMissing, "Choose at least one day." },
        { Valid with { Times = 0 }, CatalogHabitEditOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20." },
        { Valid with { Times = 21 }, CatalogHabitEditOutcome.TimesOutOfRange, "Enter a number of times from 1 to 20." },
        { Valid with { Period = null }, CatalogHabitEditOutcome.PeriodMissing, "Choose per day or per week." },
    };

    [Theory]
    [MemberData(nameof(ItemRefusals))]
    public async Task US3_AS16_each_refusal_of_the_second_suggested_starting_goal_has_position_2(
        SuggestedStartingGoalFields second, CatalogHabitEditOutcome outcome, string message)
    {
        var habit = _store.Add(Catalog("Ac24 Reading"));

        var result = await Service.EditAsync(Administrator, habit.Id, Input("Ac24 Reading", goals: [(Valid, true), (second, false)]), CancellationToken.None);

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Equal(2, result.Position);
        Assert.Equal(0, _store.WriteCount);
    }

    [Theory]
    [InlineData(1, 1, "pages")]
    [InlineData(999, 20, "uuuuuuuuuuuuuuuuuuuu")]
    public async Task US3_AS16_the_accepted_boundaries(int amount, int times, string unit)
    {
        var habit = _store.Add(Catalog("Ac24 Bounds"));

        var result = await Service.EditAsync(Administrator, habit.Id,
            Input("Ac24 Bounds", goals: [(Count(amount, unit, times, SchedulePeriod.Day), true)]), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.Saved, result.Outcome);
    }

    [Fact]
    public async Task US3_AS17_a_Reduce_amount_of_0_is_accepted()
    {
        var habit = _store.Add(Catalog("Ac25 Sugar", intent: Intent.Reduce));

        var result = await Service.EditAsync(Administrator, habit.Id,
            Input("Ac25 Sugar", intent: Intent.Reduce, goals: [(Count(0, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), true)]), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.Saved, result.Outcome);
        var browsed = Assert.Single((await Browse.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits);
        Assert.Equal("at most 0 sweets a day", browsed.MarkedSuggestedStartingGoal!.Text);
    }

    [Fact]
    public async Task A_lost_uniqueness_race_gives_name_taken()
    {
        _store.RefuseNameOnNextWrite();

        var result = await Service.CreateAsync(Administrator, Input("Ac17 Race"), CancellationToken.None);

        Assert.Equal(CatalogHabitCreateOutcome.NameTaken, result.Outcome);
        Assert.Equal("A catalog habit with this name already exists.", result.Message);
        Assert.Empty(_store.Habits);
    }

    [Fact]
    public async Task A_lost_uniqueness_race_on_edit_gives_name_taken()
    {
        var habit = _store.Add(Catalog("Ac17 Edit race"));
        _store.RefuseNameOnNextWrite();

        var result = await Service.EditAsync(Administrator, habit.Id, Input("Ac17 Edit raced"), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.NameTaken, result.Outcome);
    }

    [Fact]
    public async Task US3_AS19_the_search_lists_matching_habits_with_their_state_and_the_total()
    {
        _store.Add(Catalog("Ac28 Rowing", CatalogHabitState.Draft));
        _store.Add(Catalog("Ac28 Running"));

        var result = await Service.ListAsync(Administrator, "ac28 ro", CancellationToken.None);

        Assert.Equal(CatalogHabitListOutcome.Found, result.Outcome);
        var item = Assert.Single(result.Habits);
        Assert.Equal(("Ac28 Rowing", CatalogHabitState.Draft), (item.Name, item.State));
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task FR_012_the_list_holds_every_state_in_name_order()
    {
        _store.Add(Catalog("Zest", CatalogHabitState.Unpublished));
        _store.Add(Catalog("b Draft", CatalogHabitState.Draft));
        _store.Add(Catalog("Éclair", CatalogHabitState.Published));
        _store.Add(Catalog("A Published", CatalogHabitState.Published));

        var result = await Service.ListAsync(Administrator, null, CancellationToken.None);

        Assert.Equal(["A Published", "b Draft", "Zest", "Éclair"], result.Habits.Select(h => h.Name));
        Assert.Equal([CatalogHabitState.Published, CatalogHabitState.Draft, CatalogHabitState.Unpublished, CatalogHabitState.Published],
            result.Habits.Select(h => h.State));
        Assert.Equal(4, result.Total);
    }

    [Theory]
    [InlineData(CatalogHabitState.Draft)]
    [InlineData(CatalogHabitState.Unpublished)]
    public async Task FR_012_an_edit_keeps_the_state(CatalogHabitState state)
    {
        var habit = _store.Add(Catalog("Ac29 Keep", state));

        var result = await Service.EditAsync(Administrator, habit.Id, Input("Ac29 Kept"), CancellationToken.None);

        Assert.Equal(CatalogHabitEditOutcome.Saved, result.Outcome);
        Assert.Equal(state, _store.Get(habit.Id)!.State);
        Assert.Empty(await PublicNamesAsync());
    }
}
