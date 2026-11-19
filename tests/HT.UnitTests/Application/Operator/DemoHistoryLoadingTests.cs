using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 5 US7-AS1, AS3, AS4, AS6, FR-030, research R-08: demo history loading checks the demo password as demo
/// loading does, then creates <c>demo4@example.com</c> once, with the goals of <see cref="DemoHistory.For"/> for the
/// run date in the default zone, and never reads or changes an existing account.
/// </summary>
public sealed class DemoHistoryLoadingTests
{
    private const string Demo4 = "demo4@example.com";
    private const string DemoPassword = "example2026";

    /// <summary>Wednesday 18 November 2026, 09:00 UTC; with the UTC default, the run date is 18 November.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);

    private readonly FakeAccountStore _store = new();
    private readonly FakeDemoHistoryStore _demoHistory;
    private readonly DemoHistoryService _service;

    public DemoHistoryLoadingTests()
    {
        _demoHistory = new FakeDemoHistoryStore(_store);
        _service = new DemoHistoryService(_store, _demoHistory, new FakeTimeProvider(Now), InstallationTimeZone.From(null));
    }

    [Fact]
    public void The_demo_email_is_demo4_at_example_com()
    {
        Assert.Equal(Demo4, DemoHistoryService.DemoEmail);
        Assert.DoesNotContain(DemoHistoryService.DemoEmail, DemoDataService.DemoEmails);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("        ")]
    public async Task US7_AS6_no_demo_password_gives_its_refusal_and_calls_no_store(string? password)
    {
        var result = await _service.LoadAsync(password, CancellationToken.None);

        Assert.Equal(DemoHistoryLoadingOutcome.NoPassword, result.Outcome);
        Assert.Equal("no demo password configured", result.Message);
        AssertNothingDone(result);
    }

    [Theory]
    [InlineData("examplepass")]
    [InlineData("short1")]
    public async Task US7_AS6_a_password_that_fails_the_rule_gives_its_refusal_and_calls_no_store(string password)
    {
        var result = await _service.LoadAsync(password, CancellationToken.None);

        Assert.Equal(DemoHistoryLoadingOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("demo password does not meet the password rule", result.Message);
        AssertNothingDone(result);
    }

    [Fact]
    public async Task US7_AS1_a_valid_password_creates_demo4_with_created_demo_goals_added_and_demo_records_added()
    {
        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        var account = Assert.Single(result.Accounts);
        AssertCreated(account);
        Assert.Equal([$"account {Demo4}"], _demoHistory.Calls);

        var stored = _store.Find(Demo4);
        Assert.NotNull(stored);
        Assert.Equal(Role.RegisteredUser, stored.Role);
        Assert.Equal(DemoPassword, _store.PasswordOf(stored.Id));
        Assert.Null(stored.Name);
    }

    [Fact]
    public async Task US7_AS1_the_store_gets_the_goals_of_DemoHistory_for_the_run_date_and_the_profile()
    {
        await _service.LoadAsync(DemoPassword, CancellationToken.None);

        var id = _store.Find(Demo4)!.Id;
        AssertSameGoals(DemoHistory.For(new DateOnly(2026, 11, 18)), _demoHistory.GoalsOf(id));
        var profile = _demoHistory.ProfileOf(id)!;
        Assert.Null(profile.Name);
        Assert.Equal("UTC", profile.TimeZone.Id);
        Assert.Equal(Now, profile.CreatedAt);
    }

    [Fact]
    public async Task US7_AS1_the_results_are_the_same_when_the_store_leaves_goals_out()
    {
        _demoHistory.LeftOutHabits.UnionWith(["Reading", "Sugar", "Yoga"]);

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertCreated(Assert.Single(result.Accounts));
        Assert.Empty(_demoHistory.GoalsOf(_store.Find(Demo4)!.Id));
    }

    [Fact]
    public async Task US7_AS1_the_results_are_the_same_when_the_store_leaves_one_goal_out()
    {
        _demoHistory.LeftOutHabits.Add("Yoga");

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertCreated(Assert.Single(result.Accounts));
        Assert.Equal(["Reading", "Sugar"], _demoHistory.GoalsOf(_store.Find(Demo4)!.Id).Select(g => g.Goal.CatalogHabitName));
    }

    [Theory]
    [InlineData("demo4@example.com", Role.RegisteredUser)]
    [InlineData("DEMO4@Example.com", Role.RegisteredUser)]
    [InlineData("demo4@example.com", Role.Administrator)]
    public async Task US7_AS3_an_existing_account_gives_already_exists_with_no_goal_or_record_result_and_is_not_read(string email, Role role)
    {
        var owner = _store.Add(email, "owner2026", role, name: "A person", createdAt: Now.AddDays(-3));

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, result.Outcome);
        var account = Assert.Single(result.Accounts);
        AssertAlreadyExists(account);
        Assert.Empty(_demoHistory.Calls);
        Assert.Equal(0, _store.CreateCount);
        Assert.Equal(0, _store.PasswordCheckCount);
        Assert.Equal(0, _store.NameWriteCount);
        Assert.Equal("owner2026", _store.PasswordOf(owner.Id));
        Assert.Equal(owner, _store.Find(Demo4));
    }

    [Fact]
    public async Task US7_AS3_a_second_run_gives_already_exists_and_passes_nothing_more()
    {
        await _service.LoadAsync(DemoPassword, CancellationToken.None);
        var before = _demoHistory.AllGoals;

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertAlreadyExists(Assert.Single(result.Accounts));
        Assert.Equal([$"account {Demo4}"], _demoHistory.Calls);
        Assert.Equal(before, _demoHistory.AllGoals);
        Assert.Single(_store.Accounts);
    }

    [Fact]
    public async Task A_lost_race_for_the_demo_email_is_reported_as_already_exists()
    {
        _store.TakeEmailOnNextCreate();

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertAlreadyExists(Assert.Single(result.Accounts));
        Assert.Empty(_demoHistory.GoalsOf(_store.Find(Demo4)!.Id));
    }

    [Fact]
    public async Task The_run_date_is_the_default_zones_date_at_the_supplied_time()
    {
        // 22:30 UTC on Sunday 15 November 2026 is 00:30 on Monday 16 November in Kyiv (UTC+2), so the run date is a
        // Monday and Yoga gets the week before week 3, while the UTC date would be the Sunday before.
        var now = new DateTimeOffset(2026, 11, 15, 22, 30, 0, TimeSpan.Zero);
        var service = new DemoHistoryService(_store, _demoHistory, new FakeTimeProvider(now), InstallationTimeZone.From("Europe/Kyiv"));

        await service.LoadAsync(DemoPassword, CancellationToken.None);

        var id = _store.Find(Demo4)!.Id;
        var goals = _demoHistory.GoalsOf(id);
        AssertSameGoals(DemoHistory.For(new DateOnly(2026, 11, 16)), goals);
        Assert.All(goals, g => Assert.Equal(new DateOnly(2026, 10, 19), g.Goal.StartDate));
        Assert.Equal(8, goals[2].Records.Count);
        var profile = _demoHistory.ProfileOf(id)!;
        Assert.Equal("Europe/Kyiv", profile.TimeZone.Id);
        Assert.Equal(now, profile.CreatedAt);
        Assert.Equal("Europe/Kyiv", _store.Find(Demo4)!.StoredTimeZone);
    }

    [Fact]
    public async Task US7_AS4_no_result_text_contains_a_goal_habit_note_unit_or_amount()
    {
        string[] values =
        [
            .. DemoHistory.GoalNames, "Reading", "Sugar", "Yoga", "sweet", "min", DemoHistory.ReadingNote,
            "Away for the weekend.", "Birthday cake at work.", "Knee was sore.", "12", "20", "25",
        ];

        var texts = new List<string?>();
        foreach (var password in new[] { DemoPassword, DemoPassword, null, "examplepass" })
        {
            var result = await _service.LoadAsync(password, CancellationToken.None);
            texts.Add(result.Message);
            foreach (var account in result.Accounts)
            {
                texts.AddRange([account.Message, account.Goals?.Message, account.Records?.Message]);
                Assert.Equal(Demo4, account.Email);
            }
        }

        Assert.All(texts.OfType<string>(), text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
    }

    private static void AssertCreated(DemoHistoryAccountResult account)
    {
        Assert.Equal(Demo4, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal(DemoGoalsOutcome.Added, account.Goals!.Outcome);
        Assert.Equal("demo goals added", account.Goals.Message);
        Assert.Equal("demo records added", account.Records!.Message);
    }

    private static void AssertAlreadyExists(DemoHistoryAccountResult account)
    {
        Assert.Equal(Demo4, account.Email);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Equal("already exists", account.Message);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
    }

    private static void AssertSameGoals(IReadOnlyList<DemoHistoryGoal> expected, IReadOnlyList<DemoHistoryGoal> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        Assert.All(expected.Zip(actual), pair =>
        {
            Assert.Equal(pair.First.Goal.CatalogHabitName, pair.Second.Goal.CatalogHabitName);
            Assert.Equal(pair.First.Goal.AcceptedStates, pair.Second.Goal.AcceptedStates);
            Assert.Equal(pair.First.Goal.Target, pair.Second.Goal.Target);
            Assert.Equal(pair.First.Goal.StartDate, pair.Second.Goal.StartDate);
            Assert.Equal(pair.First.Records, pair.Second.Records);
            Assert.Equal(pair.First.Notes, pair.Second.Notes);
        });
    }

    private void AssertNothingDone(DemoHistoryLoadingResult result)
    {
        Assert.Empty(result.Accounts);
        Assert.Empty(_demoHistory.Calls);
        Assert.Empty(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }
}
