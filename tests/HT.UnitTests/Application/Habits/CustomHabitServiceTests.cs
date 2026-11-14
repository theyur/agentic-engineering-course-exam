using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>US4 and SC-006: custom habits for their owner only; every other audience is refused and nothing changes.</summary>
public sealed class CustomHabitServiceTests
{
    private static readonly ActingUser UserA = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private static readonly ActingUser UserB = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private static readonly ActingUser Admin = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    private readonly FakeCustomHabitStore _store = new();
    private CustomHabitService Service => new(_store);

    private static HabitInput Input(
        string? name = "Cold shower", Category? category = Category.Body, Intent? intent = Intent.Build, string? icon = "droplet", string? description = null) =>
        new() { Name = name, Category = category, Intent = intent, Icon = icon, Description = description };

    private CustomHabit Own(ActingUser user, string name) => _store.Add(user.AccountId!.Value, Definition(name, Category.Body, Intent.Build, "droplet"));

    [Fact]
    public async Task US4_AS1_create_and_list_with_its_values()
    {
        var result = await Service.CreateAsync(UserA, Input(description: "At least 1 minute of cold water."), CancellationToken.None);

        Assert.Equal(CustomHabitCreateOutcome.Created, result.Outcome);
        Assert.Null(result.Message);
        var listed = Assert.Single((await Service.ListAsync(UserA, CancellationToken.None)).Habits);
        Assert.Equal(result.HabitId, listed.Id);
        Assert.Equal(("Cold shower", Category.Body, Intent.Build, "droplet", "At least 1 minute of cold water."),
            (listed.Name, listed.Category, listed.Intent, listed.Icon, listed.Description));
    }

    [Fact]
    public async Task US4_AS2_the_list_is_in_name_order_and_has_only_the_users_own_habits()
    {
        Own(UserA, "b habit");
        Own(UserA, "A habit");
        Own(UserB, "B's habit");

        var result = await Service.ListAsync(UserA, CancellationToken.None);

        Assert.Equal(CustomHabitListOutcome.Found, result.Outcome);
        Assert.Equal(["A habit", "b habit"], result.Habits.Select(h => h.Name));
    }

    [Fact]
    public async Task US4_AS3_an_edit_is_returned_by_the_next_read()
    {
        var habit = Own(UserA, "Cold shower");

        var edit = await Service.EditAsync(UserA, habit.Id, Input("Cold shower, 2 min", intent: Intent.Reduce), CancellationToken.None);

        Assert.Equal(CustomHabitEditOutcome.Saved, edit.Outcome);
        var read = (await Service.GetAsync(UserA, habit.Id, CancellationToken.None)).Habit!;
        Assert.Equal(("Cold shower, 2 min", Intent.Reduce), (read.Name, read.Intent));
    }

    [Fact]
    public async Task US4_AS4_delete_removes_it_for_good()
    {
        var habit = Own(UserA, "Cold shower");

        var result = await Service.DeleteAsync(UserA, habit.Id, CancellationToken.None);

        Assert.Equal(CustomHabitDeleteOutcome.Deleted, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(CustomHabitOutcome.NotFound, (await Service.GetAsync(UserA, habit.Id, CancellationToken.None)).Outcome);
        Assert.Empty((await Service.ListAsync(UserA, CancellationToken.None)).Habits);
    }

    [Fact]
    public async Task US4_AS5_the_users_own_name_is_taken_on_create_and_rename()
    {
        Own(UserA, "Cold shower");
        var other = Own(UserA, "Warm bath");

        var create = await Service.CreateAsync(UserA, Input(" cold SHOWER "), CancellationToken.None);
        var rename = await Service.EditAsync(UserA, other.Id, Input(" cold SHOWER "), CancellationToken.None);

        Assert.Equal(CustomHabitCreateOutcome.NameTaken, create.Outcome);
        Assert.Equal("You already have a custom habit with this name.", create.Message);
        Assert.Equal(CustomHabitEditOutcome.NameTaken, rename.Outcome);
        Assert.Equal("You already have a custom habit with this name.", rename.Message);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task US4_AS6_another_users_name_is_free()
    {
        Own(UserA, "Cold shower");

        Assert.Equal(CustomHabitCreateOutcome.Created, (await Service.CreateAsync(UserB, Input("Cold shower"), CancellationToken.None)).Outcome);
    }

    [Fact]
    public async Task A_lost_uniqueness_race_gives_name_taken()
    {
        _store.RefuseNameOnNextWrite();

        var result = await Service.CreateAsync(UserA, Input(), CancellationToken.None);

        Assert.Equal(CustomHabitCreateOutcome.NameTaken, result.Outcome);
        Assert.Empty(_store.Habits);
    }

    /// <summary>Each of the five use cases on user A's habit as <paramref name="user"/>: (outcome name, message, carries a habit).</summary>
    private async Task<IReadOnlyList<(string Outcome, string? Message, bool Carries)>> AllAsync(ActingUser user, HabitId id)
    {
        var service = Service;
        var list = await service.ListAsync(user, CancellationToken.None);
        var get = await service.GetAsync(user, id, CancellationToken.None);
        var create = await service.CreateAsync(user, Input("New one"), CancellationToken.None);
        var edit = await service.EditAsync(user, id, Input("Renamed"), CancellationToken.None);
        var delete = await service.DeleteAsync(user, id, CancellationToken.None);
        return
        [
            (list.Outcome.ToString(), list.Message, list.Habits.Count > 0),
            (get.Outcome.ToString(), get.Message, get.Habit is not null),
            (create.Outcome.ToString(), create.Message, false),
            (edit.Outcome.ToString(), edit.Message, false),
            (delete.Outcome.ToString(), delete.Message, false),
        ];
    }

    [Fact]
    public async Task US4_AS7_another_registered_user_finds_no_habit_and_changes_nothing()
    {
        var secret = Own(UserA, "Ac19 Secret");
        var service = Service;

        var get = await service.GetAsync(UserB, secret.Id, CancellationToken.None);
        var edit = await service.EditAsync(UserB, secret.Id, Input("Taken over"), CancellationToken.None);
        var delete = await service.DeleteAsync(UserB, secret.Id, CancellationToken.None);
        var list = await service.ListAsync(UserB, CancellationToken.None);

        Assert.All(new[] { get.Message, edit.Message, delete.Message }, m => Assert.Equal("This custom habit does not exist.", m));
        Assert.Equal(CustomHabitOutcome.NotFound, get.Outcome);
        Assert.Equal(CustomHabitEditOutcome.NotFound, edit.Outcome);
        Assert.Equal(CustomHabitDeleteOutcome.NotFound, delete.Outcome);
        Assert.Null(get.Habit);
        Assert.Empty(list.Habits);
        Assert.Equal("Ac19 Secret", _store.Get(secret.Id)!.Definition.Name.Value);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task US4_AS8_an_administrator_is_refused_by_every_use_case_and_sees_nothing()
    {
        var secret = Own(UserA, "Ac20 Secret");

        var results = await AllAsync(Admin, secret.Id);

        Assert.All(results, r =>
        {
            Assert.Equal("NotForAdministrators", r.Outcome);
            Assert.Equal("Custom habits are for registered users.", r.Message);
            Assert.False(r.Carries);
        });
        Assert.Single(_store.Habits);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task US4_AS9_an_anonymous_visitor_is_refused_by_every_use_case()
    {
        var secret = Own(UserA, "Ac21 Secret");

        var results = await AllAsync(ActingUser.Anonymous, secret.Id);

        Assert.All(results, r =>
        {
            Assert.Equal("NotSignedIn", r.Outcome);
            Assert.Equal("You are not signed in.", r.Message);
            Assert.False(r.Carries);
        });
        Assert.Equal(0, _store.WriteCount);
    }

    public static TheoryData<HabitInput, CustomHabitCreateOutcome, string> FieldRefusals => new()
    {
        { Input("   "), CustomHabitCreateOutcome.NameMissing, "Enter a name." },
        { Input(new string('x', 61)), CustomHabitCreateOutcome.NameTooLong, "The name can have at most 60 characters." },
        { Input(description: new string('t', 501)), CustomHabitCreateOutcome.DescriptionTooLong, "The description can have at most 500 characters." },
        { Input(category: null), CustomHabitCreateOutcome.CategoryMissing, "Choose a category." },
        { Input(intent: null), CustomHabitCreateOutcome.IntentMissing, "Choose Build or Reduce." },
        { Input(icon: null), CustomHabitCreateOutcome.IconMissing, "Choose an icon." },
    };

    [Theory]
    [MemberData(nameof(FieldRefusals))]
    public async Task US4_AS10_the_field_refusals_of_FR_005_and_nothing_is_created(HabitInput input, CustomHabitCreateOutcome outcome, string message)
    {
        var result = await Service.CreateAsync(UserA, input, CancellationToken.None);

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Empty(_store.Habits);
    }

    [Fact]
    public async Task US4_AS11_FR_016_order_across_steps()
    {
        var secret = Own(UserA, "Ac21b Secret");

        var anonymous = await Service.CreateAsync(ActingUser.Anonymous, Input("   "), CancellationToken.None);
        var other = await Service.EditAsync(UserB, secret.Id, Input("   "), CancellationToken.None);
        var admin = await Service.EditAsync(Admin, new HabitId(Guid.NewGuid()), Input("   "), CancellationToken.None);

        Assert.Equal("You are not signed in.", anonymous.Message);
        Assert.Equal("This custom habit does not exist.", other.Message);
        Assert.Equal("Custom habits are for registered users.", admin.Message);
    }
}
