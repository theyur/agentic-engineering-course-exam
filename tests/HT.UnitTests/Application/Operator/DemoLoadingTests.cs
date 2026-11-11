using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Operator;

/// <summary>US7, FR-016, SC-006: demo loading creates the three demo accounts once and never changes them.</summary>
public sealed class DemoLoadingTests
{
    private static readonly string[] Emails = ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    private readonly FakeAccountStore _store = new();
    private readonly DemoDataService _demo;
    private readonly AccountService _accounts;

    public DemoLoadingTests()
    {
        _demo = new DemoDataService(_store);
        _accounts = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

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

    private Task<DemoLoadingResult> LoadAsync(string? password) =>
        _demo.LoadAsync(password, CancellationToken.None);

    private List<(string Email, Role Role, string? Password)> Snapshot() =>
        _store.Accounts
            .Select(a => (a.Email.Value, a.Role, _store.PasswordOf(a.Id)))
            .OrderBy(a => a.Value, StringComparer.Ordinal)
            .ToList();
}
