using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 7 US6-AS1, AS3, AS4, FR-031, research R-08: demo pause loading checks the demo password as demo loading does,
/// then creates <c>demo6@example.com</c> once, with the goals of <see cref="DemoPauses.For"/> for the run date in the
/// default zone, and never reads or changes an existing account.
/// </summary>
public sealed class DemoPauseLoadingTests
{
    private const string Demo6 = "demo6@example.com";
    private const string DemoPassword = "example2026";

    /// <summary>Wednesday 18 November 2026, 10:00 UTC; with the UTC default, the run date is 18 November.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeAccountStore _store = new();
    private readonly FakeDemoPauseStore _demoPause;
    private readonly DemoPauseService _service;

    public DemoPauseLoadingTests()
    {
        _demoPause = new FakeDemoPauseStore(_store);
        _service = new DemoPauseService(_demoPause, _store, new FakeTimeProvider(Now), InstallationTimeZone.From(null));
    }

    [Fact]
    public void The_demo_email_is_demo6_at_example_com()
    {
        Assert.Equal(Demo6, DemoPauseService.DemoEmail);
        Assert.DoesNotContain(DemoPauseService.DemoEmail, DemoDataService.DemoEmails);
        Assert.NotEqual(DemoHistoryService.DemoEmail, DemoPauseService.DemoEmail);
        Assert.NotEqual(DemoReportService.DemoEmail, DemoPauseService.DemoEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("        ")]
    public async Task US6_AS4_no_demo_password_gives_its_refusal_and_calls_no_store(string? password)
    {
        var result = await _service.LoadAsync(password, CancellationToken.None);

        Assert.Equal(DemoPauseLoadingOutcome.NoPassword, result.Outcome);
        Assert.Equal("no demo password configured", result.Message);
        AssertNothingDone(result);
    }

    [Theory]
    [InlineData("examplepass")]
    [InlineData("short1")]
    [InlineData("abc")]
    public async Task US6_AS4_a_password_that_fails_the_rule_gives_its_refusal_and_calls_no_store(string password)
    {
        var result = await _service.LoadAsync(password, CancellationToken.None);

        Assert.Equal(DemoPauseLoadingOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("demo password does not meet the password rule", result.Message);
        AssertNothingDone(result);
    }

    [Fact]
    public async Task US6_AS1_a_valid_password_creates_demo6_with_created_demo_goals_added_and_demo_records_added()
    {
        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        Assert.Equal(DemoPauseLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        var account = Assert.Single(result.Accounts);
        AssertCreated(account);
        Assert.Equal([$"account {Demo6}"], _demoPause.Calls);

        var stored = _store.Find(Demo6);
        Assert.NotNull(stored);
        Assert.Equal(Role.RegisteredUser, stored.Role);
        Assert.Equal(DemoPassword, _store.PasswordOf(stored.Id));
        Assert.Null(stored.Name);
    }

    [Fact]
    public async Task US6_AS1_the_store_gets_the_goals_of_DemoPauses_for_the_run_date_and_the_profile()
    {
        await _service.LoadAsync(DemoPassword, CancellationToken.None);

        var id = _store.Find(Demo6)!.Id;
        AssertSameGoals(DemoPauses.For(new DateOnly(2026, 11, 18)), _demoPause.GoalsOf(id));
        var profile = _demoPause.ProfileOf(id)!;
        Assert.Null(profile.Name);
        Assert.Equal("UTC", profile.TimeZone.Id);
        Assert.Equal(Now, profile.CreatedAt);
    }

    [Fact]
    public async Task US6_AS1_the_results_are_the_same_when_the_store_leaves_both_goals_out()
    {
        _demoPause.LeftOutHabits.UnionWith(["Meditation", "Drinking water"]);

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertCreated(Assert.Single(result.Accounts));
        Assert.Empty(_demoPause.GoalsOf(_store.Find(Demo6)!.Id));
    }

    [Fact]
    public async Task US6_AS1_the_results_are_the_same_when_the_store_leaves_one_goal_out()
    {
        _demoPause.LeftOutHabits.Add("DRINKING WATER");

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertCreated(Assert.Single(result.Accounts));
        Assert.Equal(["Meditation"], _demoPause.GoalsOf(_store.Find(Demo6)!.Id).Select(g => g.History.Goal.CatalogHabitName));
    }

    [Theory]
    [InlineData("demo6@example.com", Role.RegisteredUser)]
    [InlineData("DEMO6@Example.com", Role.RegisteredUser)]
    [InlineData("demo6@example.com", Role.Administrator)]
    [InlineData("Demo6@EXAMPLE.COM", Role.Administrator)]
    public async Task US6_AS3_an_existing_account_gives_already_exists_with_no_goal_or_record_result_and_is_not_read(string email, Role role)
    {
        var owner = _store.Add(email, "owner2026", role, name: "A person", createdAt: Now.AddDays(-3));

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        Assert.Equal(DemoPauseLoadingOutcome.Loaded, result.Outcome);
        var account = Assert.Single(result.Accounts);
        AssertAlreadyExists(account);
        Assert.Empty(_demoPause.Calls);
        Assert.Equal(0, _store.CreateCount);
        Assert.Equal(0, _store.PasswordCheckCount);
        Assert.Equal(0, _store.NameWriteCount);
        Assert.Equal("owner2026", _store.PasswordOf(owner.Id));
        Assert.Equal(owner, _store.Find(Demo6));
    }

    [Fact]
    public async Task US6_AS3_a_second_run_gives_already_exists_and_passes_nothing_more()
    {
        await _service.LoadAsync(DemoPassword, CancellationToken.None);
        var before = _demoPause.AllGoals;

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertAlreadyExists(Assert.Single(result.Accounts));
        Assert.Equal([$"account {Demo6}"], _demoPause.Calls);
        Assert.Equal(before, _demoPause.AllGoals);
        Assert.Single(_store.Accounts);
    }

    [Fact]
    public async Task US6_AS3_a_lost_race_for_the_demo_email_is_reported_as_already_exists()
    {
        _store.TakeEmailOnNextCreate();

        var result = await _service.LoadAsync(DemoPassword, CancellationToken.None);

        AssertAlreadyExists(Assert.Single(result.Accounts));
        Assert.Empty(_demoPause.GoalsOf(_store.Find(Demo6)!.Id));
    }

    [Fact]
    public async Task The_run_date_is_the_default_zones_date_at_the_supplied_time()
    {
        // 22:30 UTC on Sunday 15 November 2026 is 00:30 on Monday 16 November in Kyiv (UTC+2), so the run date is the
        // Monday and the goals start on R − 28, Monday 19 October, while the UTC date would be the Sunday before.
        var now = new DateTimeOffset(2026, 11, 15, 22, 30, 0, TimeSpan.Zero);
        var service = new DemoPauseService(_demoPause, _store, new FakeTimeProvider(now), InstallationTimeZone.From("Europe/Kyiv"));

        await service.LoadAsync(DemoPassword, CancellationToken.None);

        var id = _store.Find(Demo6)!.Id;
        var goals = _demoPause.GoalsOf(id);
        AssertSameGoals(DemoPauses.For(new DateOnly(2026, 11, 16)), goals);
        Assert.All(goals, g => Assert.Equal(new DateOnly(2026, 10, 19), g.History.Goal.StartDate));
        Assert.Equal(new DateOnly(2026, 11, 21), goals[1].Pauses[1].LastDay);
        var profile = _demoPause.ProfileOf(id)!;
        Assert.Equal("Europe/Kyiv", profile.TimeZone.Id);
        Assert.Equal(now, profile.CreatedAt);
        Assert.Equal("Europe/Kyiv", _store.Find(Demo6)!.StoredTimeZone);
    }

    [Fact]
    public async Task US6_AS1_no_result_text_contains_a_goal_habit_note_reason_unit_or_amount()
    {
        string[] values =
        [
            .. DemoPauses.GoalNames, "Meditation", "Drinking water", "glasses", "min", DemoPauses.TravellingReason,
            DemoPauses.IllnessReason, DemoPauses.FamilyVisitReason, DemoPauses.OversleptNote, DemoPauses.BusyDayNote,
            "Travelling", "flu", "Family", "Overslept", "Busy", "6", "8", "10",
        ];

        var texts = new List<string?>();
        foreach (var password in new[] { DemoPassword, DemoPassword, null, "examplepass" })
        {
            var result = await _service.LoadAsync(password, CancellationToken.None);
            texts.Add(result.Message);
            foreach (var account in result.Accounts)
            {
                texts.AddRange([account.Message, account.Goals?.Message, account.Records?.Message]);
                Assert.Equal(Demo6, account.Email);
            }
        }

        Assert.All(texts.OfType<string>(), text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void The_service_refuses_a_null_dependency()
    {
        var clock = new FakeTimeProvider(Now);
        var zone = InstallationTimeZone.From(null);

        Assert.Throws<ArgumentNullException>(() => new DemoPauseService(null!, _store, clock, zone));
        Assert.Throws<ArgumentNullException>(() => new DemoPauseService(_demoPause, null!, clock, zone));
        Assert.Throws<ArgumentNullException>(() => new DemoPauseService(_demoPause, _store, null!, zone));
        Assert.Throws<ArgumentNullException>(() => new DemoPauseService(_demoPause, _store, clock, null!));
    }

    private static void AssertCreated(DemoPauseAccountResult account)
    {
        Assert.Equal(Demo6, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal(DemoGoalsOutcome.Added, account.Goals!.Outcome);
        Assert.Equal("demo goals added", account.Goals.Message);
        Assert.Equal("demo records added", account.Records!.Message);
    }

    private static void AssertAlreadyExists(DemoPauseAccountResult account)
    {
        Assert.Equal(Demo6, account.Email);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Equal("already exists", account.Message);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
    }

    private static void AssertSameGoals(IReadOnlyList<DemoPauseGoal> expected, IReadOnlyList<DemoPauseGoal> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        Assert.All(expected.Zip(actual), pair =>
        {
            var (first, second) = (pair.First.History, pair.Second.History);
            Assert.Equal(first.Goal.CatalogHabitName, second.Goal.CatalogHabitName);
            Assert.Equal(first.Goal.AcceptedStates, second.Goal.AcceptedStates);
            Assert.Equal(first.Goal.Target, second.Goal.Target);
            Assert.Equal(first.Goal.StartDate, second.Goal.StartDate);
            Assert.Equal(first.Records, second.Records);
            Assert.Equal(first.Notes, second.Notes);
            Assert.Equal(pair.First.Excusals, pair.Second.Excusals);
            Assert.Equal(pair.First.Pauses, pair.Second.Pauses);
        });
    }

    private void AssertNothingDone(DemoPauseLoadingResult result)
    {
        Assert.Empty(result.Accounts);
        Assert.Empty(_demoPause.Calls);
        Assert.Empty(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }
}
