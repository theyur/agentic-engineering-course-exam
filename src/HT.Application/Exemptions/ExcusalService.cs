using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;

namespace HT.Application.Exemptions;

/// <summary>
/// Excusing a commitment and removing its excusal, for registered users (FR-001 to FR-004, FR-021, FR-022; research
/// R-05). Every use case takes the acting user first and checks in the order: signed in, not an administrator, the
/// acting user's account, the goal among the acting user's own, then the steps of contracts/application-api.md. Every
/// store call passes the acting user's account as the owner and the goal named in the call (constitution III). The time
/// is <see cref="TimeProvider.GetUtcNow"/>, read once per call. A refusal carries no input value, and nothing is logged
/// (HP-009).
/// </summary>
public sealed class ExcusalService
{
    private readonly IRecordStore _records;
    private readonly IGoalStore _goals;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;

    public ExcusalService(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(clock);
        _records = records;
        _goals = goals;
        _accounts = accounts;
        _clock = clock;
    }

    /// <summary>
    /// Excuses the commitment of <paramref name="date"/> of one of the acting user's goals (FR-001 to FR-003), checking
    /// in FR-002's order after access: (4) a period-based goal; (5) a date that is not a commitment; (6) a date in a
    /// pause; (7) an excusal of the date; (8) the commitment's state without exemption, which must be Due or Pending
    /// (D-2); (9) a record of the commitment (D-3); (10) a reason over 200 characters after trimming. The store
    /// re-checks steps 6, 7 and 9 under the goal-row lock, and a concurrent write there gives that step's refusal.
    /// <para>
    /// Slice 8 (plan "Spec discrepancies" 2, FR-010): step 4 judges "period-based" by the schedule kind of the attempt
    /// that contains the date (every version of one attempt has its schedule kind). A date between two attempts, or after
    /// an ended latest attempt, is in no attempt and goes on to step 5's "Nothing is scheduled on this date.".
    /// </para>
    /// </summary>
    public async Task<ExcuseResult> ExcuseAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, string? reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return ExcuseResult.Refused(ExcuseOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!.Goal;

        // VersionOf gives the version of the attempt that contains the date, so its schedule kind is that attempt's.
        if (!IsAfterAnAttempt(found, date)
            && GoalTimeline.VersionOf(found, date).Settings.Target.ScheduleKindOf != GoalScheduleKind.CommitmentBased)
        {
            return ExcuseResult.Refused(ExcuseOutcome.NotACommitment, ExcusalTexts.NotACommitment);
        }

        if (GoalTimeline.EntryOf(found, date) is not { } entry)
        {
            return ExcuseResult.Refused(ExcuseOutcome.NothingScheduled, RecordTexts.NothingScheduled);
        }

        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
        if (exemptions.IsPaused(date))
        {
            return ExcuseResult.Refused(ExcuseOutcome.DatePaused, PauseTexts.DatePaused);
        }

        if (exemptions.IsExcused(date))
        {
            return ExcuseResult.Refused(ExcuseOutcome.AlreadyExcused, ExcusalTexts.AlreadyExcused);
        }

        var stored = await _records.ReadAsync(access.Owner, goal, entry.FirstDate, entry.LastDate, cancellationToken).ConfigureAwait(false);
        var records = EntryViews.RecordsIn(stored, entry);
        var target = EntryViews.TargetOf(found, entry);
        var state = Evaluator.Evaluate(target, entry.Starts, entry.Ends, records.Select(record => record.Value).ToList(), now).State;
        if (state == EvaluationState.Upcoming)
        {
            return ExcuseResult.Refused(ExcuseOutcome.NotBegun, ExcusalTexts.NotBegun);
        }

        if (state is not (EvaluationState.Due or EvaluationState.Pending))
        {
            return ExcuseResult.Refused(ExcuseOutcome.Final, ExcusalTexts.Final);
        }

        if (records.Count > 0)
        {
            return ExcuseResult.Refused(ExcuseOutcome.HasRecord, ExcusalTexts.HasRecord);
        }

        if (!ExemptionReason.TryCreate(reason, out var text, out _))
        {
            return ExcuseResult.Refused(ExcuseOutcome.ReasonTooLong, ExcusalTexts.ReasonTooLong);
        }

        var write = await _records.ExcuseAsync(access.Owner, goal, date, text, cancellationToken).ConfigureAwait(false);
        return write.Outcome switch
        {
            ExemptionStoreWriteOutcome.Written => ExcuseResult.Excused(date),

            // A concurrent excusal, record or pause between the checks above and the write (D-3, research R-04).
            ExemptionStoreWriteOutcome.AlreadyExcused => ExcuseResult.Refused(ExcuseOutcome.AlreadyExcused, ExcusalTexts.AlreadyExcused),
            ExemptionStoreWriteOutcome.HasRecord => ExcuseResult.Refused(ExcuseOutcome.HasRecord, ExcusalTexts.HasRecord),
            ExemptionStoreWriteOutcome.Paused => ExcuseResult.Refused(ExcuseOutcome.DatePaused, PauseTexts.DatePaused),

            // The goal was removed between the lookup and the write.
            ExemptionStoreWriteOutcome.GoalNotFound => ExcuseResult.Refused(ExcuseOutcome.NotFound, GoalTexts.NotFound),
            _ => throw new InvalidOperationException("The record store gave an outcome that excusing does not have."),
        };
    }

    /// <summary>
    /// Removes the excusal of the commitment of <paramref name="date"/> of one of the acting user's goals (FR-004),
    /// checking after access: (4) no excusal of the date; (5) the commitment's grace period has ended, its end instant
    /// included. <see cref="RemoveExcusalOutcome.Removed"/> carries the commitment's state after the change at the same
    /// supplied time, evaluated with the remaining exemptions: Due, Pending, or Paused under a pause.
    /// </summary>
    public async Task<RemoveExcusalResult> RemoveExcusalAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RemoveExcusalResult.Refused(RemoveOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        var exemptions = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);

        // An excusal is stored only for a commitment's date, and while versions differ only in their time zone that date
        // stays one; a date with no commitment therefore has no excusal to remove.
        if (!exemptions.IsExcused(date) || GoalTimeline.EntryOf(found.Goal, date) is not { } entry)
        {
            return RemoveExcusalResult.Refused(RemoveExcusalOutcome.NotExcused, ExcusalTexts.NotExcused);
        }

        if (now >= GracePeriod.EndOf(entry.Ends))
        {
            return RemoveExcusalResult.Refused(RemoveExcusalOutcome.TooLate, ExcusalTexts.TooLateToRemove);
        }

        var write = await _records.RemoveExcusalAsync(access.Owner, goal, date, cancellationToken).ConfigureAwait(false);
        switch (write.Outcome)
        {
            case ExemptionStoreWriteOutcome.Written:
                var remaining = await _records.ReadExemptionsAsync(access.Owner, goal, cancellationToken).ConfigureAwait(false);
                var stored = await _records.ReadAsync(access.Owner, goal, entry.FirstDate, entry.LastDate, cancellationToken)
                    .ConfigureAwait(false);
                var view = EntryViews.Of(
                    found, entry, EntryViews.RecordsIn(stored, entry), stored.Notes.GetValueOrDefault(entry.Key), remaining, now);
                return RemoveExcusalResult.Removed(view.State);
            case ExemptionStoreWriteOutcome.NotExcused:
                // The excusal was removed between the check above and the write.
                return RemoveExcusalResult.Refused(RemoveExcusalOutcome.NotExcused, ExcusalTexts.NotExcused);
            case ExemptionStoreWriteOutcome.GoalNotFound:
                // The goal was removed between the lookup and the write.
                return RemoveExcusalResult.Refused(RemoveExcusalOutcome.NotFound, GoalTexts.NotFound);
            default:
                throw new InvalidOperationException("The record store gave an outcome that removing an excusal does not have.");
        }
    }

    /// <summary>
    /// FR-002 steps 1 to 3: not signed in, an administrator, an account that is not stored (which also gives "You are not
    /// signed in." and reveals nothing), then the goal through the owner-scoped <see cref="IGoalStore.FindAsync"/>, so
    /// another account's goal and no goal look the same (FR-021, FR-022).
    /// </summary>
    private async Task<Access> CheckAccessAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        if (actingUser.AccountId is not { } owner)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return Access.Refused(AccessRefusal.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        if (await _accounts.FindByIdAsync(owner, cancellationToken).ConfigureAwait(false) is null)
        {
            return Access.Refused(AccessRefusal.NotSignedIn, AccountTexts.NotSignedIn);
        }

        return await _goals.FindAsync(owner, goal, cancellationToken).ConfigureAwait(false) is { } stored
            ? new Access(owner, stored, null, null)
            : Access.Refused(AccessRefusal.NotFound, GoalTexts.NotFound);
    }

    /// <summary>
    /// Slice 8 (plan "Spec discrepancies" 2): whether <paramref name="date"/> lies after an ended attempt's last date and
    /// before the next attempt's start date, or after the last date of an ended latest attempt, so it is in no attempt.
    /// </summary>
    private static bool IsAfterAnAttempt(Goal goal, DateOnly date)
    {
        var attempts = goal.Attempts;
        for (var i = 0; i < attempts.Count; i++)
        {
            if (GoalTimeline.LastDate(goal, attempts[i].Attempt.Number) is { } last
                && last < date
                && (i == attempts.Count - 1 || date < attempts[i + 1].Attempt.StartDate))
            {
                return true;
            }
        }

        return false;
    }

    private static ExcuseOutcome ExcuseOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => ExcuseOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => ExcuseOutcome.NotForAdministrators,
        _ => ExcuseOutcome.NotFound,
    };

    private static RemoveExcusalOutcome RemoveOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => RemoveExcusalOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => RemoveExcusalOutcome.NotForAdministrators,
        _ => RemoveExcusalOutcome.NotFound,
    };

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
        NotFound,
    }

    /// <summary>The acting user's account and goal, or why the use case refuses before anything else is looked at.</summary>
    private sealed record Access(AccountId Owner, StoredGoal? Goal, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, null, refusal, message);
    }
}
