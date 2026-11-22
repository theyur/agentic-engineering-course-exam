using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.UiTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HT.UiTests.Operator;

/// <summary>
/// Slice 1 HP-021: demo loading runs at every start, right after administrator creation, and never logs the password.
/// Slice 2 HP-017: its custom-habit and catalog results are logged too, never with a custom-habit name. Slice 4
/// HP-013: the catalog line comes first, and each created account's goal line follows its custom-habit line; no line
/// holds a goal, habit or account name. Slice 5 HP-007, research R-09: demo history loading runs right after demo
/// loading, whose lines stay exactly as they are; its lines for <c>demo4@example.com</c> follow them, and none holds a
/// goal, habit, note, unit or amount (HP-006). Slice 6 HP-011, research R-10: demo report loading runs right after demo
/// history loading, whose lines stay exactly as they are; its lines for <c>demo5@example.com</c> follow them, also
/// after a fault in demo history loading, and none holds a goal, habit, note, unit or amount (HP-007). Slice 7 HP-010:
/// demo pause loading runs right after demo report loading, whose lines stay exactly as they are; its lines for
/// <c>demo6@example.com</c> follow them, also after a fault in demo report loading, and none holds a goal, habit, note,
/// reason, unit or amount (HP-009). Slice 8 HP-011: demo goal-change loading runs right after demo pause loading, whose
/// lines stay exactly as they are; its lines for <c>demo7@example.com</c> follow them, also after a fault in demo pause
/// loading, and none holds a goal, habit, note, outcome, unit or amount (HP-010).
/// </summary>
[Collection(StartupTasksCollection.Name)]
public sealed class StartupTasksDemoTests
{
    private const string DemoPassword = "startup-demo-7";
    private const string AdministratorLine = "Administrator creation:";
    private const string Demo4 = "demo4@example.com";
    private const string Demo5 = "demo5@example.com";
    private const string Demo6 = "demo6@example.com";
    private const string Demo7 = "demo7@example.com";

    /// <summary>Demo report loading's lines for a created <c>demo5@example.com</c>.</summary>
    private static readonly string[] Demo5CreatedLines =
    [
        $"Demo account {Demo5}: created",
        $"Demo goals {Demo5}: demo goals added",
        $"Demo records {Demo5}: demo records added",
    ];

    /// <summary>Demo pause loading's lines for a created <c>demo6@example.com</c>.</summary>
    private static readonly string[] Demo6CreatedLines =
    [
        $"Demo account {Demo6}: created",
        $"Demo goals {Demo6}: demo goals added",
        $"Demo records {Demo6}: demo records added",
    ];

    /// <summary>Demo goal-change loading's lines for a created <c>demo7@example.com</c>.</summary>
    private static readonly string[] Demo7CreatedLines =
    [
        $"Demo account {Demo7}: created",
        $"Demo goals {Demo7}: demo goals added",
        $"Demo records {Demo7}: demo records added",
    ];

    private static readonly string[] Emails = ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    /// <summary>Demo loading's lines of a first start, exactly as slice 4 logs them.</summary>
    private static readonly string[] DemoLoadingCreatedLines =
    [
        "Demo catalog: seed catalog loaded",
        .. Emails.SelectMany(email => new[]
        {
            $"Demo account {email}: created",
            $"Demo custom habits {email}: demo custom habits added",
            $"Demo goals {email}: demo goals added",
        }),
    ];

    [Fact]
    public async Task A_demo_password_logs_created_for_each_demo_email_after_administrator_creation()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        // Slice 4 HP-013: the catalog line once, first; then each account line, its custom-habit line and its goal line.
        // Slice 5 HP-007: then demo4's account line, its goal line and its record line; no custom-habit line.
        // Slice 6 HP-011: then demo5's, in the same form. Slice 7 HP-010: then demo6's, in the same form. Slice 8 HP-011:
        // then demo7's, in the same form.
        AssertDemoLines(
            log,
            [
                .. DemoLoadingCreatedLines,
                $"Demo account {Demo4}: created",
                $"Demo goals {Demo4}: demo goals added",
                $"Demo records {Demo4}: demo records added",
                .. Demo5CreatedLines,
                .. Demo6CreatedLines,
                .. Demo7CreatedLines,
            ]);
    }

    [Fact]
    public async Task A_second_start_logs_already_exists_for_each_demo_email()
    {
        await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        var log = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));

        // An existing account has no custom-habit line, no goal line and no record line; the catalog is not empty any
        // more and comes first; demo4's account line, then demo5's, demo6's and demo7's, come last.
        AssertDemoLines(
            log,
            [
                "Demo catalog: catalog not empty",
                .. Emails.Select(email => $"Demo account {email}: already exists"),
                $"Demo account {Demo4}: already exists",
                $"Demo account {Demo5}: already exists",
                $"Demo account {Demo6}: already exists",
                $"Demo account {Demo7}: already exists",
            ]);
    }

    [Fact]
    public async Task Every_demo_records_line_holds_the_demo_email_and_the_result_text_only()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        var lines = log.Where(e => e.Message.StartsWith("Demo records ", StringComparison.Ordinal)).Select(e => e.Message).ToList();
        Assert.Equal(
            [
                $"Demo records {Demo4}: {OperatorTexts.DemoRecordsAdded}",
                $"Demo records {Demo5}: {OperatorTexts.DemoRecordsAdded}",
                $"Demo records {Demo6}: {OperatorTexts.DemoRecordsAdded}",
                $"Demo records {Demo7}: {OperatorTexts.DemoRecordsAdded}",
            ],
            lines);
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo custom habits {Demo4}", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo custom habits {Demo5}", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo custom habits {Demo6}", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo custom habits {Demo7}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task No_log_entry_contains_a_demo_custom_habit_goal_or_account_name()
    {
        var first = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));
        var second = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));

        // "Reading" alone is left out: framework messages may contain the word; the goal names that hold it are in.
        // Slice 5 (HP-006): demo history loading's goal names and the four demo notes of FR-030 too.
        var names = Emails.SelectMany(DemoCustomHabits.For).Select(h => h.Name.Value)
            .Concat(Emails.SelectMany(DemoGoals.DisplayNames))
            .Concat(DemoHistory.GoalNames)
            .Concat([DemoHistory.ReadingNote, "Away for the weekend.", "Birthday cake at work.", "Knee was sore."])
            // Slice 6 (HP-007): demo report loading's goal names and the three demo notes of FR-032 too.
            .Concat(DemoReport.GoalNames)
            .Concat([DemoReport.WalkingNote, DemoReport.StrengthNote, DemoReport.ScreenTimeNote])
            // Slice 7 (HP-009): demo pause loading's goal names, reasons and notes of FR-031 too.
            .Concat(DemoPauses.GoalNames)
            .Concat([DemoPauses.TravellingReason, DemoPauses.IllnessReason, DemoPauses.FamilyVisitReason])
            .Concat([DemoPauses.OversleptNote, DemoPauses.BusyDayNote, "Drinking water", "glasses"])
            // Slice 8 (HP-010): demo goal-change loading's goal names, the earlier attempt's name, the later target, the
            // note, the ending notes and the outcomes of FR-031 too.
            .Concat(DemoGoalChanges.GoalNames)
            .Concat([DemoGoalChanges.EarlierAttemptName, DemoGoalChanges.WaitingTargetText])
            .Concat([DemoGoalChanges.ShortOnTimeNote, DemoGoalChanges.TooBusyEndingNote, DemoGoalChanges.SugarEndingNote])
            .Concat(["Achieved", "Abandoned", "No longer relevant"])
            .Concat(["Yoga", "Meditation", "Sugar", "Coffee", DemoAccounts.Demo1Name, "sweet", "cups"])
            .ToList();

        Assert.All(first.Concat(second), e => Assert.DoesNotContain(
            names, name => (e.Message + e.Exception).Contains(name, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task Every_demo_goals_line_holds_the_demo_email_and_the_result_text_only()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        var lines = log.Where(e => e.Message.StartsWith("Demo goals ", StringComparison.Ordinal)).Select(e => e.Message).ToList();
        Assert.Equal([.. Emails.Append(Demo4).Append(Demo5).Append(Demo6).Append(Demo7).Select(email => $"Demo goals {email}: {OperatorTexts.DemoGoalsAdded}")], lines);
    }

    [Fact]
    public async Task No_demo_password_logs_no_demo_password_configured()
    {
        var log = await StartupTasksHarness.RunAsync("Development", new Dictionary<string, string?>());

        // Demo loading's one line, then demo history loading's, demo report loading's, demo pause loading's and demo
        // goal-change loading's one line each.
        var lines = log.Where(e => e.Message.StartsWith("Demo ", StringComparison.Ordinal)).ToList();
        Assert.Equal(
            [
                "Demo loading: no demo password configured",
                "Demo history loading: no demo password configured",
                "Demo report loading: no demo password configured",
                "Demo pause loading: no demo password configured",
                "Demo goal-change loading: no demo password configured",
            ],
            lines.Select(e => e.Message));
        Assert.All(lines, line => Assert.Equal(LogLevel.Information, line.Level));
    }

    [Fact]
    public async Task A_password_that_fails_the_rule_logs_its_refusal_and_creates_nothing()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured("startup-demo"));

        Assert.Contains(log, e => e.Message == "Demo loading: demo password does not meet the password rule");
        Assert.Single(log, e => e.Message.StartsWith("Demo history loading", StringComparison.Ordinal));
        Assert.Contains(log, e => e.Message == "Demo history loading: demo password does not meet the password rule");
        Assert.Single(log, e => e.Message.StartsWith("Demo report loading", StringComparison.Ordinal));
        Assert.Contains(log, e => e.Message == "Demo report loading: demo password does not meet the password rule");
        Assert.Single(log, e => e.Message.StartsWith("Demo pause loading", StringComparison.Ordinal));
        Assert.Contains(log, e => e.Message == "Demo pause loading: demo password does not meet the password rule");
        Assert.Single(log, e => e.Message.StartsWith("Demo goal-change loading", StringComparison.Ordinal));
        Assert.Contains(log, e => e.Message == "Demo goal-change loading: demo password does not meet the password rule");
        Assert.Equal(
            [
                "Demo loading: demo password does not meet the password rule",
                "Demo history loading: demo password does not meet the password rule",
                "Demo report loading: demo password does not meet the password rule",
                "Demo pause loading: demo password does not meet the password rule",
                "Demo goal-change loading: demo password does not meet the password rule",
            ],
            log.Where(e => e.Message.StartsWith("Demo ", StringComparison.Ordinal)).Select(e => e.Message));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo account ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo custom habits ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo goals ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo records ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo catalog", StringComparison.Ordinal));
        foreach (var email in Emails.Append(Demo4).Append(Demo5).Append(Demo6).Append(Demo7))
        {
            Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, email));
        }
    }

    [Fact]
    public async Task No_log_entry_contains_the_demo_password()
    {
        var first = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));
        var second = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));
        var refused = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured("startup-demo"));

        Assert.All(first.Concat(second).Concat(refused), e =>
        {
            Assert.DoesNotContain(DemoPassword, e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("startup-demo", e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("startup-demo", e.Exception?.ToString() ?? string.Empty, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task A_fault_in_demo_loading_is_logged_as_an_error_and_the_start_goes_on()
    {
        const string unreachable =
            "Server=ht-no-such-server.invalid;Database=HT-UI-MIGRATE;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=2";

        // Testing applies no migration and, with no administrator values, administrator creation never touches
        // the store, so the demo step is the one that meets the unreachable database.
        var log = await StartupTasksHarness.RunWithoutDropAsync("Testing", Configured(DemoPassword), unreachable);

        Assert.Contains(log, e => e.Message == "Administrator creation: no administrator configured");
        var error = Assert.Single(log, e => e.Message == "Demo loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.NotNull(error.Exception);
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));

        // Slice 5 HP-007: demo history loading still runs, after it, and meets the same database.
        var historyError = Assert.Single(log, e => e.Message == "Demo history loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, historyError.Level);
        Assert.True(IndexOf(log, error) < IndexOf(log, historyError));

        // Slice 6 HP-011: demo report loading still runs, after both, and meets the same database.
        var reportError = Assert.Single(log, e => e.Message == "Demo report loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, reportError.Level);
        Assert.True(IndexOf(log, historyError) < IndexOf(log, reportError));

        // Slice 7 HP-010: demo pause loading still runs, after all three, and meets the same database.
        var pauseError = Assert.Single(log, e => e.Message == "Demo pause loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, pauseError.Level);
        Assert.True(IndexOf(log, reportError) < IndexOf(log, pauseError));

        // Slice 8 HP-011: demo goal-change loading still runs, after all four, and meets the same database.
        var goalChangeError = Assert.Single(log, e => e.Message == "Demo goal-change loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, goalChangeError.Level);
        Assert.True(IndexOf(log, pauseError) < IndexOf(log, goalChangeError));
    }

    [Fact]
    public async Task A_fault_in_demo_history_loading_is_logged_as_an_error_after_demo_loadings_lines_and_the_start_goes_on()
    {
        var log = await StartupTasksHarness.RunAsync(
            "Development",
            Configured(DemoPassword),
            configure: services => services.Replace(ServiceDescriptor.Scoped<IDemoHistoryStore, FaultingDemoHistoryStore>()));

        // Demo loading's lines are unchanged; the fault's line follows them, and demo5's, demo6's and demo7's lines follow
        // the fault: the start went on past it (slice 6 HP-011, slice 7 HP-010, slice 8 HP-011).
        AssertDemoLines(
            log, [.. DemoLoadingCreatedLines, .. Demo5CreatedLines, .. Demo6CreatedLines, .. Demo7CreatedLines], allInformation: false);
        var error = Assert.Single(log, e => e.Message == "Demo history loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.IsType<InvalidOperationException>(error.Exception);
        var demo5 = log.First(e => e.Message == Demo5CreatedLines[0]);
        Assert.True(IndexOf(log, log.Last(e => e.Message == DemoLoadingCreatedLines[^1])) < IndexOf(log, error));
        Assert.True(IndexOf(log, error) < IndexOf(log, demo5));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo account {Demo4}", StringComparison.Ordinal));
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));
        Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo4));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo5));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo6));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo7));
    }

    [Fact]
    public async Task A_fault_in_demo_report_loading_is_logged_as_an_error_after_demo_history_loadings_lines_and_the_start_goes_on()
    {
        var log = await StartupTasksHarness.RunAsync(
            "Development",
            Configured(DemoPassword),
            configure: services => services.Replace(ServiceDescriptor.Scoped<IDemoReportStore, FaultingDemoReportStore>()));

        // Demo loading's and demo history loading's lines are unchanged; the fault's line follows them, and demo6's and
        // demo7's lines follow the fault: the start went on past it (slice 7 HP-010, slice 8 HP-011).
        AssertDemoLines(
            log,
            [
                .. DemoLoadingCreatedLines,
                $"Demo account {Demo4}: created",
                $"Demo goals {Demo4}: demo goals added",
                $"Demo records {Demo4}: demo records added",
                .. Demo6CreatedLines,
                .. Demo7CreatedLines,
            ],
            allInformation: false);
        var error = Assert.Single(log, e => e.Message == "Demo report loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.IsType<InvalidOperationException>(error.Exception);
        Assert.True(IndexOf(log, log.Last(e => e.Message == $"Demo records {Demo4}: demo records added")) < IndexOf(log, error));
        Assert.True(IndexOf(log, error) < IndexOf(log, log.First(e => e.Message == Demo6CreatedLines[0])));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo account {Demo5}", StringComparison.Ordinal));
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));
        Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo5));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo6));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo7));
    }

    [Fact]
    public async Task A_fault_in_demo_pause_loading_is_logged_as_an_error_after_demo_report_loadings_lines_and_the_start_goes_on()
    {
        var log = await StartupTasksHarness.RunAsync(
            "Development",
            Configured(DemoPassword),
            configure: services => services.Replace(ServiceDescriptor.Scoped<IDemoPauseStore, FaultingDemoPauseStore>()));

        // The earlier tasks' lines are unchanged; the fault's line, with no secret or value, follows them, and demo7's lines
        // follow the fault: the start went on past it (slice 8 HP-011).
        AssertDemoLines(
            log,
            [
                .. DemoLoadingCreatedLines,
                $"Demo account {Demo4}: created",
                $"Demo goals {Demo4}: demo goals added",
                $"Demo records {Demo4}: demo records added",
                .. Demo5CreatedLines,
                .. Demo7CreatedLines,
            ],
            allInformation: false);
        var error = Assert.Single(log, e => e.Message == "Demo pause loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.IsType<InvalidOperationException>(error.Exception);
        Assert.True(IndexOf(log, log.Last(e => e.Message == Demo5CreatedLines[^1])) < IndexOf(log, error));
        Assert.True(IndexOf(log, error) < IndexOf(log, log.First(e => e.Message == Demo7CreatedLines[0])));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo account {Demo6}", StringComparison.Ordinal));
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo5));
        Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo6));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo7));
    }

    [Fact]
    public async Task A_fault_in_demo_goal_change_loading_is_logged_as_an_error_after_demo_pause_loadings_lines_and_the_start_goes_on()
    {
        var log = await StartupTasksHarness.RunAsync(
            "Development",
            Configured(DemoPassword),
            configure: services => services.Replace(ServiceDescriptor.Scoped<IDemoGoalChangeStore, FaultingDemoGoalChangeStore>()));

        // The earlier tasks' lines are unchanged, and the fault's line, with no secret or value, is the last demo line.
        AssertDemoLines(
            log,
            [
                .. DemoLoadingCreatedLines,
                $"Demo account {Demo4}: created",
                $"Demo goals {Demo4}: demo goals added",
                $"Demo records {Demo4}: demo records added",
                .. Demo5CreatedLines,
                .. Demo6CreatedLines,
            ],
            allInformation: false);
        var error = Assert.Single(log, e => e.Message == "Demo goal-change loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.IsType<InvalidOperationException>(error.Exception);
        Assert.Equal(error, log.Last(e => e.Message.StartsWith("Demo ", StringComparison.Ordinal)));
        Assert.DoesNotContain(log, e => e.Message.StartsWith($"Demo account {Demo7}", StringComparison.Ordinal));
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));
        Assert.Equal(1, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo6));
        Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, Demo7));
    }

    private static int IndexOf(IReadOnlyList<LogEntry> log, LogEntry entry) =>
        log.Select((e, i) => (e, i)).First(pair => ReferenceEquals(pair.e, entry)).i;

    /// <summary>
    /// The demo lines after administrator creation are <paramref name="expected"/>, in order, all at Information level;
    /// with <paramref name="allInformation"/> false, they start with <paramref name="expected"/> and may go on with an
    /// error line.
    /// </summary>
    private static void AssertDemoLines(IReadOnlyList<LogEntry> log, IReadOnlyList<string> expected, bool allInformation = true)
    {
        var messages = log.Select(e => e.Message).ToList();
        var administrator = messages.FindIndex(m => m.StartsWith(AdministratorLine, StringComparison.Ordinal));
        Assert.True(administrator >= 0, "No administrator creation line.");

        var demoLines = log.Skip(administrator + 1).Where(e => e.Message.StartsWith("Demo ", StringComparison.Ordinal)).ToList();
        var information = allInformation ? demoLines : [.. demoLines.Where(e => e.Level == LogLevel.Information)];
        Assert.Equal(expected, information.Select(e => e.Message));
        Assert.All(information, e => Assert.Equal(LogLevel.Information, e.Level));
        Assert.DoesNotContain(log.Take(administrator), e => e.Message.StartsWith("Demo ", StringComparison.Ordinal));
    }

    /// <summary>A demo history port that fails, as an unreachable or broken database would (ADR 0006: no mocking library).</summary>
    private sealed class FaultingDemoHistoryStore : IDemoHistoryStore
    {
        public Task<CreateDemoAccountOutcome> CreateDemoHistoryAccountAsync(
            Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoHistoryGoal> goals, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The demo history store failed.");
    }

    /// <summary>A demo report port that fails, as an unreachable or broken database would (ADR 0006: no mocking library).</summary>
    private sealed class FaultingDemoReportStore : IDemoReportStore
    {
        public Task<CreateDemoAccountOutcome> CreateDemoReportAccountAsync(
            Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoHistoryGoal> goals, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The demo report store failed.");
    }

    /// <summary>A demo pause port that fails, as an unreachable or broken database would (ADR 0006: no mocking library).</summary>
    private sealed class FaultingDemoPauseStore : IDemoPauseStore
    {
        public Task<CreateDemoAccountOutcome> CreateDemoPauseAccountAsync(
            Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoPauseGoal> goals, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The demo pause store failed.");
    }

    /// <summary>A demo goal-change port that fails, as an unreachable or broken database would (ADR 0006: no mocking library).</summary>
    private sealed class FaultingDemoGoalChangeStore : IDemoGoalChangeStore
    {
        public Task<CreateDemoAccountOutcome> CreateDemoGoalChangeAccountAsync(
            Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoGoalChangeGoal> goals, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The demo goal-change store failed.");
    }

    private static Dictionary<string, string?> Configured(string demoPassword) => new()
    {
        ["Operator:DemoPassword"] = demoPassword,
    };
}
