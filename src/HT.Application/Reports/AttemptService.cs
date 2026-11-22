using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;

namespace HT.Application.Reports;

/// <summary>
/// The attempts of a registered user's goal (slice 8 FR-014, FR-022, FR-023; contracts/application-api.md
/// "AttemptService", research R-06). Every use case takes the acting user first and checks in FR-023's order: signed in,
/// not an administrator, the acting user's stored account, then the goal among that account's goals. An attempt is
/// found only by number among that goal's attempts. Every store call passes the acting user's account as the owner and
/// the goal named in the call (constitution III). The time is read once per call, each attempt is evaluated once by the
/// shared <see cref="GoalEvaluation"/>, and nothing derived is stored (ADR 0018, ADR 0020). It logs nothing, and no
/// result names a value of another account (HP-010).
/// </summary>
public sealed class AttemptService
{
    /// <summary>The most dates an attempt's History covers (slice 5's longest range, FR-014).</summary>
    private const int HistoryDays = 366;

    private readonly IGoalStore _goals;
    private readonly IRecordStore _records;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;

    public AttemptService(IGoalStore goals, IRecordStore records, IAccountStore accounts, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        _goals = goals;
        _records = records;
        _accounts = accounts;
        _clock = clock;
    }

    /// <summary>
    /// Every attempt of one of the acting user's goals at the supplied time, the latest first (FR-014): each with its
    /// number, name (FR-004), start date, ending fields once ended, whether it has no date, and its consistency and
    /// streaks over that attempt up to the supplied time, from one record read per attempt (none for an attempt with no
    /// date). Another account's goal and no goal both give <see cref="AttemptListOutcome.NotFound"/>.
    /// </summary>
    public async Task<AttemptListResult> GetAttemptsAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return AttemptListResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? AttemptListOutcome.NotSignedIn : AttemptListOutcome.NotForAdministrators,
                access.Message!);
        }

        if (await _goals.FindAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false) is not { } stored)
        {
            return AttemptListResult.Refused(AttemptListOutcome.NotFound, GoalTexts.NotFound);
        }

        var now = _clock.GetUtcNow();
        var attempts = new List<AttemptView>(stored.Goal.Attempts.Count);
        foreach (var attempt in stored.Goal.Attempts.Reverse())
        {
            var evaluation = await GoalEvaluation.OfAsync(_records, access.Owner, stored, attempt.Attempt.Number, now, cancellationToken)
                .ConfigureAwait(false);
            attempts.Add(ViewOf(evaluation, attempt.Attempt));
        }

        return AttemptListResult.Found(attempts);
    }

    /// <summary>
    /// Attempt <paramref name="number"/> of one of the acting user's goals at the supplied time (FR-014): its
    /// <see cref="AttemptView"/>, its latest 30 ended results, and its History from its start date to its last date, or to
    /// the goal's current date while it is not ended, newest first, limited to the entries that overlap the newest 366
    /// dates of that range. A number that is not one of the goal's attempts gives
    /// <see cref="AttemptReadOutcome.AttemptNotFound"/>, "This attempt does not exist.". An attempt with no date has an
    /// empty History and "N/A" (AC-102).
    /// </summary>
    public async Task<AttemptReadResult> GetAttemptAsync(
        ActingUser actingUser, GoalId goal, int number, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await AccessAsync(actingUser, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return AttemptReadResult.Refused(
                refusal == AccessRefusal.NotSignedIn ? AttemptReadOutcome.NotSignedIn : AttemptReadOutcome.NotForAdministrators,
                access.Message!);
        }

        if (await _goals.FindAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false) is not { } stored)
        {
            return AttemptReadResult.Refused(AttemptReadOutcome.NotFound, GoalTexts.NotFound);
        }

        if (stored.Goal.Attempts.FirstOrDefault(attempt => attempt.Attempt.Number == number) is not { } found)
        {
            return AttemptReadResult.Refused(AttemptReadOutcome.AttemptNotFound, GoalChangeTexts.AttemptNotFound);
        }

        var now = _clock.GetUtcNow();
        var evaluation = await GoalEvaluation.OfAsync(_records, access.Owner, stored, number, now, cancellationToken).ConfigureAwait(false);
        return AttemptReadResult.Found(new AttemptDetails
        {
            Attempt = ViewOf(evaluation, found.Attempt),
            Recent = ReportService.RecentOf(evaluation, ReportService.GoalReportRecentCount),
            History = HistoryOf(evaluation),
        });
    }

    /// <summary>One attempt with its figures, from its evaluation (FR-014).</summary>
    private static AttemptView ViewOf(GoalEvaluation evaluation, GoalAttempt attempt)
    {
        var goal = evaluation.Goal;
        var ending = attempt.Ending;
        return new AttemptView
        {
            Number = attempt.Number,
            Name = evaluation.Name,
            StartDate = attempt.StartDate,
            IsLatest = evaluation.IsLatestAttempt,
            EndingDate = ending?.Date,
            LastDate = GoalTimeline.LastDate(goal, attempt.Number),
            Outcome = ending?.Outcome,
            EndingNote = ending?.Note?.Value,
            HasNoDate = AttemptEntries.HasNoDate(goal, attempt.Number),
            Stats = ReportService.StatsOf(evaluation),
        };
    }

    /// <summary>
    /// The attempt's History (FR-014): the evaluated entries, which run from its first entry through the earlier of its
    /// last date and the goal's current date, newest first, those that overlap the newest <see cref="HistoryDays"/> dates
    /// of that range; the goal-level settings are the attempt's settings in effect.
    /// </summary>
    private static GoalHistory HistoryOf(GoalEvaluation evaluation)
    {
        var goal = evaluation.Goal;
        var number = evaluation.AttemptNumber;
        IReadOnlyList<HistoryEntry> entries = [];
        if ((GoalTimeline.LastDate(goal, number) ?? evaluation.CurrentDate) is { } end)
        {
            var first = end.AddDays(1 - HistoryDays);
            entries = [.. evaluation.Entries.Where(entry => entry.LastDate >= first).Reverse()];
        }

        return EntryViews.HistoryOf(goal, GoalTimeline.VersionInEffect(goal, number, evaluation.Now), evaluation.CurrentDate, entries, evaluation.Now);
    }

    /// <summary>
    /// FR-023 steps 1 and 2, then the acting user's stored account: not signed in, an administrator, or an account that is
    /// not stored, which also gives "You are not signed in." and reveals nothing.
    /// </summary>
    private async Task<Access> AccessAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        if (actingUser.AccountId is not { } owner)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return Access.Refused(AccessRefusal.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        return await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false) is null
            ? Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn)
            : new Access(owner, null, null);
    }

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
    }

    /// <summary>The acting user's account, or why the use case refuses before the goal is looked at.</summary>
    private sealed record Access(AccountId Owner, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, refusal, message);
    }
}
