using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 1 US7, FR-016, SC-006: demo loading creates the three demo accounts once and never changes them. Slice 2
/// US6, FR-030, FR-031, SC-007: it also gives each account it creates its demo custom habits, and loads the seed
/// catalog into an empty catalog (research R-13).
/// </summary>
public sealed class DemoLoadingTests
{
    private static readonly string[] Emails = ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    private readonly FakeAccountStore _store = new();
    private readonly FakeDemoDataStore _demoData;
    private readonly DemoDataService _demo;
    private readonly AccountService _accounts;

    public DemoLoadingTests()
    {
        _demoData = new FakeDemoDataStore(_store);
        _demo = new DemoDataService(_store, _demoData);
        _accounts = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

    // Slice 1 (US7, FR-016, SC-006)

    [Fact]
    public async Task US7_AS1_each_demo_email_is_created_and_signs_in_as_a_registered_user()
    {
        var result = await LoadAsync("example2026");

        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(Emails, result.Accounts.Select(a => a.Email));
        Assert.All(result.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.Created, a.Outcome);
            Assert.Equal("created", a.Message);
        });
        foreach (var email in Emails)
        {
            var signIn = await _accounts.SignInAsync(email, "example2026", CancellationToken.None);
            Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
            Assert.Equal(Role.RegisteredUser, signIn.ActingUser!.Role);
        }
    }

    [Fact]
    public async Task US7_AS2_a_second_run_reports_already_exists_and_keeps_a_changed_password()
    {
        await LoadAsync("example2026");
        var demo3 = _store.Find("demo3@example.com")!;
        Assert.True(await _store.ChangePasswordAsync(demo3.Id, "example2026", "changed2026", CancellationToken.None));
        var createsBefore = _store.CreateCount;

        var result = await LoadAsync("example2026");

        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        Assert.Equal(Emails, result.Accounts.Select(a => a.Email));
        Assert.All(result.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.AlreadyExists, a.Outcome);
            Assert.Equal("already exists", a.Message);
        });
        Assert.Equal(createsBefore, _store.CreateCount);
        Assert.Equal(3, _store.Accounts.Count);
        Assert.Equal(SignInOutcome.SignedIn, (await _accounts.SignInAsync("demo3@example.com", "changed2026", CancellationToken.None)).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await _accounts.SignInAsync("demo3@example.com", "example2026", CancellationToken.None)).Outcome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("        ")]
    public async Task US7_AS3_no_demo_password_creates_nothing(string? password)
    {
        var result = await LoadAsync(password);

        Assert.Equal(DemoLoadingOutcome.NoPassword, result.Outcome);
        Assert.Equal("no demo password configured", result.Message);
        Assert.Empty(result.Accounts);
        Assert.Empty(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }

    [Fact]
    public async Task US7_AS4_a_password_that_fails_the_rule_creates_nothing()
    {
        var result = await LoadAsync("examplepass");

        Assert.Equal(DemoLoadingOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("demo password does not meet the password rule", result.Message);
        Assert.Empty(result.Accounts);
        Assert.Empty(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }

    [Theory]
    [InlineData(Role.RegisteredUser)]
    [InlineData(Role.Administrator)]
    public async Task An_existing_account_of_either_role_is_reported_and_left_unchanged(Role role)
    {
        var owner = _store.Add("demo2@example.com", "owner2026", role);

        var result = await LoadAsync("example2026");

        Assert.Equal(
            [DemoAccountOutcome.Created, DemoAccountOutcome.AlreadyExists, DemoAccountOutcome.Created],
            result.Accounts.Select(a => a.Outcome));
        Assert.Equal(role, _store.Find("demo2@example.com")!.Role);
        Assert.Equal("owner2026", _store.PasswordOf(owner.Id));
        Assert.Equal(0, _store.PasswordChangeCount);
    }

    [Fact]
    public async Task SC_006_a_second_run_leaves_the_same_accounts_and_passwords()
    {
        await LoadAsync("example2026");
        var once = Snapshot();

        await LoadAsync("example2026");

        Assert.Equal(once, Snapshot());
    }

    [Fact]
    public async Task A_lost_race_for_a_demo_email_is_reported_as_already_exists()
    {
        _store.TakeEmailOnNextCreate();

        var result = await LoadAsync("example2026");

        Assert.Equal(DemoAccountOutcome.AlreadyExists, result.Accounts[0].Outcome);
        Assert.Equal(DemoAccountOutcome.Created, result.Accounts[1].Outcome);
        Assert.Equal(DemoAccountOutcome.Created, result.Accounts[2].Outcome);
    }

    [Fact]
    public void US7_AS5_the_demo_emails_are_the_three_example_com_addresses_in_order()
    {
        Assert.Equal(Emails, DemoDataService.DemoEmails);
        Assert.All(DemoDataService.DemoEmails, e => Assert.EndsWith("@example.com", e, StringComparison.Ordinal));
    }

    // Slice 2: the seed catalog (US6-AS1 to US6-AS4, FR-030, SC-007)

    [Fact]
    public async Task US6_AS1_an_empty_catalog_gets_the_seed_catalog()
    {
        var result = await LoadAsync("example2026");

        Assert.NotNull(result.Catalog);
        Assert.Equal(DemoCatalogOutcome.SeedCatalogLoaded, result.Catalog.Outcome);
        Assert.Equal("seed catalog loaded", result.Catalog.Message);
        Assert.Equal(1, _demoData.SeedCount);
        Assert.Equal(SeedCatalog.Habits, _demoData.Catalog);
    }

    [Fact]
    public async Task US6_AS2_SC_007_a_second_run_reports_catalog_not_empty_and_adds_nothing()
    {
        await LoadAsync("example2026");

        var result = await LoadAsync("example2026");

        Assert.NotNull(result.Catalog);
        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, result.Catalog.Outcome);
        Assert.Equal("catalog not empty", result.Catalog.Message);
        Assert.Equal(1, _demoData.SeedCount);
        Assert.Equal(SeedCatalog.Habits, _demoData.Catalog);
    }

    [Fact]
    public async Task US6_AS3_a_catalog_with_any_habit_reports_catalog_not_empty_and_keeps_only_that_habit()
    {
        var rowing = new SeedCatalogHabit(
            HabitSamples.Definition("Ac29 Rowing", Category.Body, Intent.Build, "dumbbell"), SuggestedStartingGoals.None, CatalogHabitState.Draft);
        _demoData.AddCatalogHabit(rowing);

        var result = await LoadAsync("example2026");

        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, result.Catalog!.Outcome);
        Assert.Equal("catalog not empty", result.Catalog.Message);
        Assert.Equal(0, _demoData.SeedCount);
        Assert.Same(rowing, Assert.Single(_demoData.Catalog));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("examplepass")]
    public async Task US6_AS4_a_refusal_has_no_catalog_result_and_creates_no_account_catalog_or_custom_habit(string? password)
    {
        var result = await LoadAsync(password);

        Assert.Null(result.Catalog);
        Assert.Empty(_demoData.Calls);
        Assert.Empty(_demoData.Catalog);
        Assert.Empty(_demoData.AllCustomHabits);
        Assert.Empty(_store.Accounts);
    }

    // Slice 2: the demo custom habits (US6-AS5 to US6-AS8, FR-031)

    [Fact]
    public async Task US6_AS5_each_created_account_gets_its_two_demo_custom_habits()
    {
        var result = await LoadAsync("example2026");

        Assert.All(result.Accounts, a =>
        {
            Assert.NotNull(a.CustomHabits);
            Assert.Equal(DemoCustomHabitsOutcome.Added, a.CustomHabits.Outcome);
            Assert.Equal("demo custom habits added", a.CustomHabits.Message);
        });
        Assert.Equal(["Cold shower", "No phone in bed"], CustomHabitNamesOf("demo1@example.com"));
        Assert.Equal(["Evening stretch", "Plan tomorrow"], CustomHabitNamesOf("demo2@example.com"));
        Assert.Equal(["Late snacks", "Water the plants"], CustomHabitNamesOf("demo3@example.com"));
        foreach (var email in Emails)
        {
            Assert.Equal(DemoCustomHabits.For(email), _demoData.CustomHabitsOf(_store.Find(email)!.Id));
        }
    }

    [Fact]
    public async Task US6_AS6_a_second_run_has_no_custom_habit_result_and_adds_no_custom_habit()
    {
        await LoadAsync("example2026");
        var before = _demoData.AllCustomHabits;

        var result = await LoadAsync("example2026");

        Assert.All(result.Accounts, a => Assert.Null(a.CustomHabits));
        Assert.Equal(before, _demoData.AllCustomHabits);
        Assert.Equal(3, _demoData.Calls.Count(c => c.StartsWith("account ", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(Role.RegisteredUser)]
    [InlineData(Role.Administrator)]
    public async Task US6_AS8_an_existing_account_gets_no_custom_habit_result_and_no_custom_habit(Role role)
    {
        var owner = _store.Add("demo2@example.com", "owner2026", role);

        var result = await LoadAsync("example2026");

        Assert.Equal(DemoAccountOutcome.AlreadyExists, result.Accounts[1].Outcome);
        Assert.Null(result.Accounts[1].CustomHabits);
        Assert.NotNull(result.Accounts[0].CustomHabits);
        Assert.NotNull(result.Accounts[2].CustomHabits);
        Assert.Empty(_demoData.CustomHabitsOf(owner.Id));
        Assert.DoesNotContain("account demo2@example.com", _demoData.Calls);
    }

    [Fact]
    public async Task A_lost_race_gives_no_custom_habit_result_and_records_no_custom_habit_for_that_email()
    {
        _store.TakeEmailOnNextCreate();

        var result = await LoadAsync("example2026");

        Assert.Null(result.Accounts[0].CustomHabits);
        Assert.Empty(_demoData.CustomHabitsOf(_store.Find("demo1@example.com")!.Id));
        Assert.NotNull(result.Accounts[1].CustomHabits);
        Assert.NotNull(result.Accounts[2].CustomHabits);
    }

    [Fact]
    public async Task The_work_runs_in_order_each_account_with_its_custom_habits_then_the_seed_catalog()
    {
        await LoadAsync("example2026");

        Assert.Equal(
            ["account demo1@example.com", "account demo2@example.com", "account demo3@example.com", "catalog"],
            _demoData.Calls);
    }

    [Fact]
    public async Task An_existing_account_is_skipped_and_the_seed_catalog_still_comes_last()
    {
        _store.Add("demo2@example.com", "owner2026");

        await LoadAsync("example2026");

        Assert.Equal(["account demo1@example.com", "account demo3@example.com", "catalog"], _demoData.Calls);
    }

    [Fact]
    public async Task No_result_text_contains_a_demo_custom_habit_name()
    {
        var names = Emails.SelectMany(DemoCustomHabits.For).Select(h => h.Name.Value).ToList();

        var texts = new List<string?>();
        foreach (var password in new[] { "example2026", "example2026", null, "examplepass" })
        {
            var result = await LoadAsync(password);
            texts.Add(result.Message);
            texts.Add(result.Catalog?.Message);
            foreach (var account in result.Accounts)
            {
                texts.AddRange([account.Email, account.Message, account.CustomHabits?.Message]);
            }
        }

        foreach (var text in texts.OfType<string>())
        {
            Assert.DoesNotContain(names, name => text.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    private Task<DemoLoadingResult> LoadAsync(string? password) =>
        _demo.LoadAsync(password, CancellationToken.None);

    private List<string> CustomHabitNamesOf(string email) =>
        _demoData.CustomHabitsOf(_store.Find(email)!.Id).Select(h => h.Name.Value).ToList();

    private List<(string Email, Role Role, string? Password)> Snapshot() =>
        _store.Accounts
            .Select(a => (a.Email.Value, a.Role, _store.PasswordOf(a.Id)))
            .OrderBy(a => a.Value, StringComparer.Ordinal)
            .ToList();
}
