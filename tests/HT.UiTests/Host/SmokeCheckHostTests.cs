using System.Text.RegularExpressions;
using HT.Application.Operator;
using HT.SmokeCheck;
using Microsoft.Data.SqlClient;

namespace HT.UiTests.Host;

/// <summary>
/// HP-017, SC-005 (research R-11): the smoke check in process against a real host on database <c>HT-UI-SMOKE</c>,
/// reset before each case, with check 2 skipped (no HTTP front end). A freshly seeded host passes every check; a
/// demo password that fails the password rule (no demo accounts, no seed catalog) fails <c>catalog</c>, every
/// <c>demo</c> check and <c>demo history</c>; a demo account that lost a custom habit or a goal fails that account's check
/// only. The output never contains a password, the administrator email, a demo custom-habit name, a demo goal name,
/// demo1's account name (slice 4 HP-018), or demo4's goal names, notes and amounts (slice 5 HP-008).
/// </summary>
[Collection(DerivedDatabaseHostCollection.Name)]
public sealed partial class SmokeCheckHostTests
{
    private const string DatabaseName = "HT-UI-SMOKE";
    private const string AdministratorEmail = "smoke-admin@example.com";
    private const string AdministratorPassword = "throwaway-admin-8";
    private const string DemoPassword = "throwaway-demo-8";

    /// <summary>Fails the password rule (no digit), and is distinctive enough to search the output for.</summary>
    private const string RuleBreakingDemoPassword = "nodigitsentinel";

    [Fact]
    public async Task A_freshly_seeded_host_passes_every_check()
    {
        var (result, _) = await RunAsync(DemoPassword);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.CheckLines));
        Assert.All(result.CheckLines, line => Assert.True(
            line.StartsWith("PASS ", StringComparison.Ordinal) || line == "SKIP https-redirect", line));
        Assert.Equal(
            ["home", "https-redirect", "catalog", "administrator", .. DemoDataService.DemoEmails.Select(e => "demo " + e), "demo history " + DemoHistoryService.DemoEmail],
            result.CheckLines.Select(CheckName));

        // Slice 4 HP-018: each demo line counts its custom habits, its goal names and, for demo1, its account name.
        Assert.Equal("PASS demo demo1@example.com (5 of 5)", Line(result, "demo demo1@example.com"));
        Assert.Equal("PASS demo demo2@example.com (4 of 4)", Line(result, "demo demo2@example.com"));
        Assert.Equal("PASS demo demo3@example.com (5 of 5)", Line(result, "demo demo3@example.com"));

        // Slice 5 HP-008, SC-005: demo4's 3 goal names, 6 texts on Reading's History card and 2 on Yoga's.
        Assert.Equal("PASS demo history demo4@example.com (11 of 11)", Line(result, "demo history " + DemoHistoryService.DemoEmail));
        Assert.Matches(LaterTiming(), result.TimingLine);
        Assert.True(result.LaterRequests >= 1);
    }

    [Fact]
    public async Task A_demo_account_that_lost_a_goal_fails_only_its_own_check()
    {
        const string affected = "demo3@example.com";

        var (result, _) = await RunAsync(DemoPassword, host => DeleteOneGoalAsync(host.ConnectionString, affected));

        Assert.False(result.Passed);
        var failed = Assert.Single(result.CheckLines, line => line.StartsWith(SmokeCheckRunner.FailPrefix, StringComparison.Ordinal));
        Assert.Equal($"FAIL demo {affected}: 2 of 3 goals shown", failed);
    }

    [Fact]
    public async Task A_demo_password_that_fails_the_password_rule_fails_the_catalog_and_every_demo_check()
    {
        var (result, _) = await RunAsync(RuleBreakingDemoPassword);

        Assert.False(result.Passed);
        Assert.StartsWith("FAIL catalog: ", Line(result, "catalog"), StringComparison.Ordinal);
        Assert.Equal("PASS administrator", Line(result, "administrator"));
        Assert.All(DemoDataService.DemoEmails, email =>
            Assert.StartsWith($"FAIL demo {email}: ", Line(result, "demo " + email), StringComparison.Ordinal));
        Assert.StartsWith("FAIL demo history demo4@example.com: ", Line(result, "demo history " + DemoHistoryService.DemoEmail), StringComparison.Ordinal);
        Assert.EndsWith(", later n/a", result.TimingLine, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_demo_account_that_lost_a_custom_habit_fails_only_its_own_check()
    {
        const string affected = "demo2@example.com";

        var (result, _) = await RunAsync(DemoPassword, host => DeleteOneCustomHabitAsync(host.ConnectionString, affected));

        Assert.False(result.Passed);
        var failed = Assert.Single(result.CheckLines, line => line.StartsWith(SmokeCheckRunner.FailPrefix, StringComparison.Ordinal));
        Assert.StartsWith($"FAIL demo {affected}: ", failed, StringComparison.Ordinal);
    }

    /// <summary>
    /// Resets the database, starts a host with these operator values, lets <paramref name="prepare"/> change the seeded
    /// data, runs the smoke check with the host's browser factory and asserts that the output holds no secret or
    /// custom-habit name (case (d) of T022).
    /// </summary>
    private static async Task<(SmokeCheckResult Result, string Output)> RunAsync(
        string demoPassword, Func<DerivedDatabaseHost, Task>? prepare = null)
    {
        await DerivedDatabaseHost.ResetAsync(DatabaseName);
        await using var host = new DerivedDatabaseHost(DatabaseName, new Dictionary<string, string?>
        {
            ["Operator:AdministratorEmail"] = AdministratorEmail,
            ["Operator:AdministratorPassword"] = AdministratorPassword,
            ["Operator:DemoPassword"] = demoPassword,
        });
        // Starts the host, so the startup tasks have seeded the database before it is changed.
        _ = host.Services;
        if (prepare is not null)
        {
            await prepare(host);
        }

        var options = new SmokeCheckOptions(new Uri("https://localhost/"), demoPassword, AdministratorEmail, AdministratorPassword)
        {
            SkipHttpRedirect = true,
            WarmUp = TimeSpan.FromSeconds(5),
        };
        await using var output = new StringWriter();

        var result = await new SmokeCheckRunner(host.CreateBrowser, options, output).RunAsync();

        var text = output.ToString();
        string[] forbidden =
        [
            demoPassword,
            AdministratorEmail,
            AdministratorPassword,
            .. DemoDataService.DemoEmails.SelectMany(DemoCustomHabits.For).Select(h => h.Name.Value),
            .. DemoDataService.DemoEmails.SelectMany(DemoGoals.DisplayNames),
            DemoAccounts.Demo1Name,

            // Slice 5 HP-008: demo4's goal names, the four demo notes of FR-030 and the amounts its History cards show.
            .. DemoHistory.GoalNames,
            "Fell asleep early.",
            "Away for the weekend.",
            "Birthday cake at work.",
            "Knee was sore.",
            "12 min",
            "25 min",
            "3 sweet",
        ];
        Assert.All(forbidden, value => Assert.DoesNotContain(value, text, StringComparison.OrdinalIgnoreCase));
        return (result, text);
    }

    private static string Line(SmokeCheckResult result, string name) =>
        Assert.Single(result.CheckLines, line => CheckName(line) == name);

    /// <summary>The check's name: the line without its outcome word, its count and its reason.</summary>
    private static string CheckName(string line)
    {
        var name = line[(line.IndexOf(' ', StringComparison.Ordinal) + 1)..];
        var end = name.IndexOfAny([':', '(']);
        return (end < 0 ? name : name[..end]).TrimEnd();
    }

    private static async Task DeleteOneCustomHabitAsync(string connectionString, string email)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            """
            DELETE TOP (1) habit
            FROM [CustomHabits] AS habit
            JOIN [AspNetUsers] AS owner ON owner.[Id] = habit.[OwnerId]
            WHERE owner.[UserName] = @email
            """,
            connection);
        command.Parameters.AddWithValue("@email", email);
        Assert.Equal(1, await command.ExecuteNonQueryAsync());
    }

    private static async Task DeleteOneGoalAsync(string connectionString, string email)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            """
            DELETE TOP (1) goal
            FROM [Goals] AS goal
            JOIN [AspNetUsers] AS owner ON owner.[Id] = goal.[OwnerId]
            WHERE owner.[UserName] = @email
            """,
            connection);
        command.Parameters.AddWithValue("@email", email);
        Assert.Equal(1, await command.ExecuteNonQueryAsync());
    }

    [GeneratedRegex(@", later \d+ ms \(\d+ requests\)$")]
    private static partial Regex LaterTiming();
}
