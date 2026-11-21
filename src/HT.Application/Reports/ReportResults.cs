namespace HT.Application.Reports;

/// <summary>The outcomes of reading a goal's report, in FR-007's check order (contracts/application-api.md).</summary>
public enum GoalReportOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>
/// A goal's report (FR-007), or the refusal. An unknown identifier and another account's goal give the same
/// <see cref="GoalReportOutcome.NotFound"/> (FR-015).
/// </summary>
public sealed class GoalReportResult
{
    private GoalReportResult(GoalReportOutcome outcome, string? message, GoalReport? report)
    {
        Outcome = outcome;
        Message = message;
        Report = report;
    }

    public GoalReportOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="GoalReportOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="GoalReportOutcome.Found"/>.</summary>
    public GoalReport? Report { get; }

    internal static GoalReportResult Refused(GoalReportOutcome outcome, string message) => new(outcome, message, null);

    internal static GoalReportResult Found(GoalReport report) => new(GoalReportOutcome.Found, null, report);
}

/// <summary>The outcomes of reading the goals overview, in FR-015's check order (contracts/application-api.md).</summary>
public enum GoalsOverviewOutcome
{
    NotSignedIn,
    NotForAdministrators,
    Found,
}

/// <summary>Every goal of the acting user with its figures (FR-011), or the refusal.</summary>
public sealed class GoalsOverviewResult
{
    private GoalsOverviewResult(GoalsOverviewOutcome outcome, string? message, GoalsOverview? overview)
    {
        Outcome = outcome;
        Message = message;
        Overview = overview;
    }

    public GoalsOverviewOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="GoalsOverviewOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="GoalsOverviewOutcome.Found"/>.</summary>
    public GoalsOverview? Overview { get; }

    internal static GoalsOverviewResult Refused(GoalsOverviewOutcome outcome, string message) => new(outcome, message, null);

    internal static GoalsOverviewResult Found(GoalsOverview overview) => new(GoalsOverviewOutcome.Found, null, overview);
}

/// <summary>The outcomes of reading the dashboard, in FR-015's check order (contracts/application-api.md).</summary>
public enum DashboardOutcome
{
    NotSignedIn,
    NotForAdministrators,
    Found,
}

/// <summary>The signed-in Home's figures (FR-012, FR-013), or the refusal. A user with no goals is found, never refused.</summary>
public sealed class DashboardResult
{
    private DashboardResult(DashboardOutcome outcome, string? message, Dashboard? dashboard)
    {
        Outcome = outcome;
        Message = message;
        Dashboard = dashboard;
    }

    public DashboardOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="DashboardOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="DashboardOutcome.Found"/>.</summary>
    public Dashboard? Dashboard { get; }

    internal static DashboardResult Refused(DashboardOutcome outcome, string message) => new(outcome, message, null);

    internal static DashboardResult Found(Dashboard dashboard) => new(DashboardOutcome.Found, null, dashboard);
}
