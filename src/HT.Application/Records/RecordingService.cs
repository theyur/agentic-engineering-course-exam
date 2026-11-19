using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// Recording and evaluation for registered users (FR-001 to FR-011, FR-017, FR-018; research R-04). Every use case
/// takes the acting user first and checks in FR-005's order: signed in, not an administrator, the acting user's
/// account, the goal among the acting user's own, then (where they apply) the date, the value, the note and the limit.
/// Every store call passes the acting user's account as the owner and the goal named in the call, so a use case acts
/// only on that user's own records and notes (constitution III). The time is <see cref="TimeProvider.GetUtcNow"/>, read
/// once per call; every state of the call is evaluated at that instant. Results carry texts and views only, and a
/// refusal never carries an input value (HP-006).
/// </summary>
public sealed class RecordingService
{
    /// <summary>
    /// How many days before the current date "what can be recorded now" looks (research R-04): the current week, the
    /// previous week, which is Pending until its Wednesday 00:00, and one day for a gap or overlap (FR-027).
    /// </summary>
    private const int RecordableWindowDays = 9;

    /// <summary>How many notes the recent notes give at most (FR-008).</summary>
    private const int RecentNotesCount = 5;

    private readonly IRecordStore _records;
    private readonly IGoalStore _goals;
    private readonly IAccountStore _accounts;
    private readonly TimeProvider _clock;

    public RecordingService(IRecordStore records, IGoalStore goals, IAccountStore accounts, TimeProvider clock)
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
    /// Records what happened on <paramref name="date"/> for one of the acting user's goals (FR-001 to FR-007), checking
    /// in contracts/application-api.md's order: access; the date belongs to a commitment or period of the goal
    /// (<see cref="RecordWriteOutcome.NothingScheduled"/>); its day has begun (<see cref="RecordWriteOutcome.DayNotBegun"/>);
    /// the value for the measurement of the version in effect for the entry (FR-004, FR-015); the note (FR-007); then the
    /// store adds the record, or for a commitment replaces its one record in place, and refuses a period that already has
    /// <see cref="RecordLimits.MaxRecordsPerPeriod"/> records. A note with text adds or replaces the entry's note; none
    /// leaves it unchanged. <see cref="RecordWriteOutcome.Recorded"/> carries the record's identifier, the entry after
    /// the write and whether the write was a correction (FR-006).
    /// </summary>
    public async Task<RecordWriteResult> RecordAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, RecordInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RecordWriteResult.Refused(WriteOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        if (GoalTimeline.EntryOf(found, date) is not { } entry)
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.NothingScheduled, RecordTexts.NothingScheduled);
        }

        if (!GoalTimeline.HasBegun(found, date, now))
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.DayNotBegun, RecordTexts.DayNotBegun);
        }

        var target = TargetOf(found, entry);
        if (!RecordValue.TryCreate(target.Measurement, input.Answer, input.Amount, out var value, out var problem))
        {
            return ValueRefused(problem);
        }

        if (!NoteText.TryCreate(input.Note, out var note, out _))
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.NoteTooLong, RecordTexts.NoteTooLong);
        }

        var write = await _records.AddAsync(
            access.Owner,
            goal,
            SpanOf(entry),
            target.ScheduleKindOf == GoalScheduleKind.CommitmentBased,
            date,
            value,
            note,
            RecordLimits.MaxRecordsPerPeriod,
            cancellationToken).ConfigureAwait(false);
        switch (write.Outcome)
        {
            case RecordStoreWriteOutcome.Written:
                var view = EntryView(found, entry, write.Entry!.Records, write.Entry.Note, now);
                return RecordWriteResult.Recorded(write.RecordId!.Value, view, view.IsCorrection);
            case RecordStoreWriteOutcome.LimitReached:
                return RecordWriteResult.Refused(RecordWriteOutcome.RecordLimitReached, RecordTexts.RecordLimitReached);
            case RecordStoreWriteOutcome.GoalNotFound:
                // The goal was removed between the lookup and the write.
                return RecordWriteResult.Refused(RecordWriteOutcome.NotFound, GoalTexts.NotFound);
            default:
                throw new InvalidOperationException("The record store gave an outcome that adding a record does not have.");
        }
    }

    /// <summary>
    /// Changes the value of one record of one of the acting user's goals (FR-003 to FR-007), at any time, also after the
    /// entry is final (US3), checking in contracts/application-api.md's order: access; the record among that goal's
    /// records (<see cref="RecordWriteOutcome.RecordNotFound"/> for a record of another goal, of another account or of
    /// nothing); the value for the measurement of the version in effect for the record's entry (FR-004, FR-015); then
    /// the note (FR-007). There is no date step: the record's date never changes (FR-003), and while versions differ
    /// only in their time zone it always passes FR-002 again (plan "Resolved in the spec" 1). The record keeps its
    /// identifier, date and place in the stored order. A note with text adds or replaces the entry's note; none leaves
    /// it unchanged. <see cref="RecordWriteOutcome.Changed"/> carries the record's identifier, the entry after the write
    /// and whether the change was a correction (FR-006).
    /// </summary>
    public async Task<RecordWriteResult> ChangeRecordAsync(
        ActingUser actingUser, GoalId goal, RecordId record, RecordInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RecordWriteResult.Refused(WriteOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        if (await _records.FindRecordDateAsync(access.Owner, goal, record, cancellationToken).ConfigureAwait(false) is not { } date)
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.RecordNotFound, RecordTexts.RecordNotFound);
        }

        var entry = StoredEntryOf(found, date);
        var target = TargetOf(found, entry);
        if (!RecordValue.TryCreate(target.Measurement, input.Answer, input.Amount, out var value, out var problem))
        {
            return ValueRefused(problem);
        }

        if (!NoteText.TryCreate(input.Note, out var note, out _))
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.NoteTooLong, RecordTexts.NoteTooLong);
        }

        var write = await _records.ChangeAsync(access.Owner, goal, record, SpanOf(entry), value, note, cancellationToken)
            .ConfigureAwait(false);
        switch (write.Outcome)
        {
            case RecordStoreWriteOutcome.Written:
                var view = EntryView(found, entry, write.Entry!.Records, write.Entry.Note, now);
                return RecordWriteResult.Changed(write.RecordId!.Value, view, view.IsCorrection);
            case RecordStoreWriteOutcome.RecordNotFound:
                // The record was deleted between the lookup and the write.
                return RecordWriteResult.Refused(RecordWriteOutcome.RecordNotFound, RecordTexts.RecordNotFound);
            case RecordStoreWriteOutcome.GoalNotFound:
                // The goal was removed between the lookup and the write.
                return RecordWriteResult.Refused(RecordWriteOutcome.NotFound, GoalTexts.NotFound);
            default:
                throw new InvalidOperationException("The record store gave an outcome that changing a record does not have.");
        }
    }

    /// <summary>
    /// Deletes one record of one of the acting user's goals (FR-003, FR-006), at any time, also after the entry is final
    /// (US3-AS3), checking in contracts/application-api.md's order: access, then the record among that goal's records
    /// (<see cref="RecordWriteOutcome.RecordNotFound"/>); nothing after the record. Deleting the only record of a final
    /// entry makes it Missed. <see cref="RecordWriteOutcome.Deleted"/> carries the entry after the write and whether the
    /// deletion was a correction (FR-006): deleting the only record of a Due or Pending entry is not one.
    /// </summary>
    public async Task<RecordWriteResult> DeleteRecordAsync(ActingUser actingUser, GoalId goal, RecordId record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RecordWriteResult.Refused(WriteOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        if (await _records.FindRecordDateAsync(access.Owner, goal, record, cancellationToken).ConfigureAwait(false) is not { } date)
        {
            return RecordWriteResult.Refused(RecordWriteOutcome.RecordNotFound, RecordTexts.RecordNotFound);
        }

        var entry = StoredEntryOf(found, date);
        var write = await _records.DeleteAsync(access.Owner, goal, record, SpanOf(entry), cancellationToken).ConfigureAwait(false);
        switch (write.Outcome)
        {
            case RecordStoreWriteOutcome.Written:
                var view = EntryView(found, entry, write.Entry!.Records, write.Entry.Note, now);
                return RecordWriteResult.Deleted(view, view.IsCorrection);
            case RecordStoreWriteOutcome.RecordNotFound:
                // The record was deleted between the lookup and the write.
                return RecordWriteResult.Refused(RecordWriteOutcome.RecordNotFound, RecordTexts.RecordNotFound);
            case RecordStoreWriteOutcome.GoalNotFound:
                // The goal was removed between the lookup and the write.
                return RecordWriteResult.Refused(RecordWriteOutcome.NotFound, GoalTexts.NotFound);
            default:
                throw new InvalidOperationException("The record store gave an outcome that deleting a record does not have.");
        }
    }

    /// <summary>
    /// Saves the note of the entry of <paramref name="date"/> on its own, for one of the acting user's goals (FR-007),
    /// checking in contracts/application-api.md's order: access; the date belongs to a commitment or period of the goal
    /// (<see cref="NoteSaveOutcome.NothingScheduled"/>); its day has begun (<see cref="NoteSaveOutcome.DayNotBegun"/>);
    /// then the note (<see cref="NoteSaveOutcome.NoteTooLong"/>). Any begun date of a week names that week. A note with
    /// text adds or replaces the entry's note, and one that is empty after trimming removes it, whatever the entry's
    /// state, final included. The note never changes the state. <see cref="NoteSaveOutcome.Saved"/> carries the entry
    /// after the write.
    /// </summary>
    public async Task<NoteSaveResult> SaveNoteAsync(
        ActingUser actingUser, GoalId goal, DateOnly date, string? note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return NoteSaveResult.Refused(NoteOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        if (GoalTimeline.EntryOf(found, date) is not { } entry)
        {
            return NoteSaveResult.Refused(NoteSaveOutcome.NothingScheduled, RecordTexts.NothingScheduled);
        }

        if (!GoalTimeline.HasBegun(found, date, now))
        {
            return NoteSaveResult.Refused(NoteSaveOutcome.DayNotBegun, RecordTexts.DayNotBegun);
        }

        if (!NoteText.TryCreate(note, out var text, out _))
        {
            return NoteSaveResult.Refused(NoteSaveOutcome.NoteTooLong, RecordTexts.NoteTooLong);
        }

        var write = await _records.SaveNoteAsync(access.Owner, goal, SpanOf(entry), text, cancellationToken).ConfigureAwait(false);
        switch (write.Outcome)
        {
            case RecordStoreWriteOutcome.Written:
                return NoteSaveResult.Saved(EntryView(found, entry, write.Entry!.Records, write.Entry.Note, now));
            case RecordStoreWriteOutcome.GoalNotFound:
                // The goal was removed between the lookup and the write.
                return NoteSaveResult.Refused(NoteSaveOutcome.NotFound, GoalTexts.NotFound);
            default:
                throw new InvalidOperationException("The record store gave an outcome that saving a note does not have.");
        }
    }

    /// <summary>
    /// The recent notes of one of the acting user's goals (FR-008; research R-04): the notes of the
    /// <see cref="RecentNotesCount"/> entries with the latest first dates (a per-week period: its Monday), newest first,
    /// each with its entry's first and last date, the note and the entry's state at the supplied time. The store gives the
    /// latest note keys; then one read per key, over that entry's dates only, gives its records and note, so the call
    /// makes at most 6 store reads and none spans the dates between two notes. A note removed between the two reads is
    /// left out.
    /// </summary>
    public async Task<RecentNotesResult> GetRecentNotesAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RecentNotesResult.Refused(RecentNotesOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        var keys = await _records.ReadRecentNoteKeysAsync(access.Owner, goal, RecentNotesCount, cancellationToken).ConfigureAwait(false);
        var notes = new List<RecentNote>(keys.Count);
        foreach (var key in keys)
        {
            var entry = NoteEntryOf(found, key);
            var stored = await _records.ReadAsync(access.Owner, goal, entry.FirstDate, entry.LastDate, cancellationToken)
                .ConfigureAwait(false);
            if (stored.Notes.GetValueOrDefault(entry.Key) is not { } note)
            {
                continue;
            }

            var view = EntryView(found, entry, RecordsIn(stored, entry), note, now);
            notes.Add(new RecentNote { FirstDate = view.FirstDate, LastDate = view.LastDate, Note = note.Value, State = view.State });
        }

        return RecentNotesResult.Found(notes);
    }

    /// <summary>
    /// The history of one of the acting user's goals from <paramref name="first"/> to <paramref name="last"/>, both
    /// included (FR-009, FR-010, FR-014; research R-07): every commitment or period of the goal's evaluation calendar in
    /// the range, in date order, each as <c>EntryView</c> gives it at the supplied time. The range is checked after the
    /// goal (FR-018): 1 to 366 days, otherwise <see cref="HistoryOutcome.RangeOutOfBounds"/>. A per-week period that
    /// overlaps the range is given whole, with all its records, also those dated outside the range, so one store read
    /// covers the first entry's first date to the last entry's last date. A range with no entry (unscheduled days only,
    /// days before the start date, or a per-week goal's days before its first week) gives no entries, and then no
    /// record is read. <see cref="GoalHistory.CurrentDate"/> is the goal's current date at the supplied time.
    /// </summary>
    public async Task<HistoryResult> GetHistoryAsync(
        ActingUser actingUser, GoalId goal, DateOnly first, DateOnly last, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return HistoryResult.Refused(HistoryOutcomeOf(refusal), access.Message!);
        }

        var found = access.Goal!;
        if (!GoalTimeline.TryEntries(found, first, last, out var entries))
        {
            return HistoryResult.Refused(HistoryOutcome.RangeOutOfBounds, GoalTexts.RangeOutOfBounds);
        }

        var now = _clock.GetUtcNow();
        var current = GoalTimeline.CurrentDate(found, now);
        if (entries.Count == 0)
        {
            return HistoryResult.Found(HistoryOf(found, current, [], now));
        }

        var stored = await _records.ReadAsync(access.Owner, goal, entries[0].FirstDate, entries[^1].LastDate, cancellationToken)
            .ConfigureAwait(false);
        var views = entries
            .Select(entry => EntryView(found, entry, RecordsIn(stored, entry), stored.Notes.GetValueOrDefault(entry.Key), now))
            .ToList();
        return HistoryResult.Found(HistoryOf(found, current, views, now));
    }

    /// <summary>
    /// What can be recorded now for one of the acting user's goals (FR-011, research R-04): the entries of the goal's
    /// calendar from 9 days before its current date to the current date that are Due or Pending at the supplied time,
    /// newest first by first date, each as the history gives it. A goal with no current date, or with no entry in that
    /// window (a per-week goal before its first week), has none, and then no record is read.
    /// <see cref="GoalHistory.CurrentDate"/> is the goal's current date at the supplied time.
    /// </summary>
    public async Task<RecordableResult> GetRecordableAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var access = await CheckAccessAsync(actingUser, goal, cancellationToken).ConfigureAwait(false);
        if (access.Refusal is { } refusal)
        {
            return RecordableResult.Refused(RecordableOutcomeOf(refusal), access.Message!);
        }

        var now = _clock.GetUtcNow();
        var found = access.Goal!;
        if (GoalTimeline.CurrentDate(found, now) is not { } current)
        {
            return RecordableResult.Found(HistoryOf(found, null, [], now));
        }

        if (!GoalTimeline.TryEntries(found, current.AddDays(-RecordableWindowDays), current, out var entries) || entries.Count == 0)
        {
            return RecordableResult.Found(HistoryOf(found, current, [], now));
        }

        var stored = await _records.ReadAsync(access.Owner, goal, entries[0].FirstDate, entries[^1].LastDate, cancellationToken)
            .ConfigureAwait(false);
        var recordable = entries
            .Select(entry => EntryView(found, entry, RecordsIn(stored, entry), stored.Notes.GetValueOrDefault(entry.Key), now))
            .Where(view => view.State is EvaluationState.Due or EvaluationState.Pending)
            .OrderByDescending(view => view.FirstDate)
            .ToList();
        return RecordableResult.Found(HistoryOf(found, current, recordable, now));
    }

    /// <summary>
    /// FR-005 steps 1 to 3: not signed in, an administrator, an account that is not stored (deleted after sign-in, or a
    /// stale cookie, which also gives "You are not signed in." and reveals nothing), then the goal through the
    /// owner-scoped <see cref="IGoalStore.FindAsync"/>, so another account's goal and no goal look the same (FR-017).
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
            ? new Access(owner, stored.Goal, null, null)
            : Access.Refused(AccessRefusal.NotFound, GoalTexts.NotFound);
    }

    /// <summary>
    /// One entry as the history gives it at <paramref name="now"/> (FR-009, FR-010, FR-014, FR-015): evaluated with the
    /// target of the settings version in effect for its first date, its grace end only while it is Due or Pending, its
    /// records as views, its note, and whether a change now is a correction (its grace period has ended, FR-006).
    /// <paramref name="records"/> are the entry's records, in date order and then in the stored order.
    /// </summary>
    private static HistoryEntry EntryView(Goal goal, CalendarEntry entry, IReadOnlyList<GoalRecord> records, NoteText? note, DateTimeOffset now)
    {
        var evaluation = Evaluator.Evaluate(TargetOf(goal, entry), entry.Starts, entry.Ends, records.Select(record => record.Value).ToList(), now);
        return new HistoryEntry
        {
            FirstDate = entry.FirstDate,
            LastDate = entry.LastDate,
            Starts = entry.Starts,
            Ends = entry.Ends,
            State = evaluation.State,
            GraceEnds = evaluation.State is EvaluationState.Due or EvaluationState.Pending ? evaluation.GraceEnds : null,
            Records = records.Select(record => new RecordView(record.Id, record.Date, record.Value.Answer, record.Value.Amount)).ToList(),
            Progress = evaluation.Progress,
            Note = note?.Value,
            IsCorrection = now >= GracePeriod.EndOf(entry.Ends),
        };
    }

    /// <summary>
    /// The goal's settings as the pages need them to show <paramref name="entries"/> (FR-009, FR-011): those of the
    /// version of <paramref name="currentDate"/>, or of the start date when there is none, and the instant the clock was
    /// read. No goal or habit name.
    /// </summary>
    private static GoalHistory HistoryOf(Goal goal, DateOnly? currentDate, IReadOnlyList<HistoryEntry> entries, DateTimeOffset now)
    {
        var target = GoalTimeline.VersionOf(goal, currentDate ?? goal.StartDate).Settings.Target;
        return new GoalHistory
        {
            GoalId = goal.Id,
            GoalScheduleKind = target.ScheduleKindOf,
            Period = target.PeriodLength,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            Times = target.ScheduleKindOf == GoalScheduleKind.PeriodBased ? target.Schedule.Times : null,
            CurrentDate = currentDate,
            Target = target,
            ReadAt = now,
            Entries = entries,
        };
    }

    /// <summary>
    /// The entry of a stored record's date. A record is stored only for a date of an entry, and while versions differ
    /// only in their time zone that date stays one (FR-015, plan "Resolved in the spec" 1); the message names no date.
    /// </summary>
    private static CalendarEntry StoredEntryOf(Goal goal, DateOnly date) =>
        GoalTimeline.EntryOf(goal, date)
        ?? throw new InvalidOperationException("A stored record's date belongs to no commitment or period of its goal.");

    /// <summary>
    /// The entry of a stored note's key (the commitment's date, the day, or the week's Monday). A note is stored only under
    /// an entry's key, and while versions differ only in their time zone that key stays one (FR-015); the message names no
    /// date.
    /// </summary>
    private static CalendarEntry NoteEntryOf(Goal goal, DateOnly key) =>
        GoalTimeline.EntryOf(goal, key)
        ?? throw new InvalidOperationException("A stored note's key belongs to no commitment or period of its goal.");

    /// <summary>The entry as the record store sees it: its note key and its first and last date (research R-05).</summary>
    private static EntrySpan SpanOf(CalendarEntry entry) => new(entry.Key, entry.FirstDate, entry.LastDate);

    /// <summary>The target of the settings version in effect for the entry's date (FR-015).</summary>
    private static GoalTarget TargetOf(Goal goal, CalendarEntry entry) => GoalTimeline.VersionOf(goal, entry.FirstDate).Settings.Target;

    /// <summary>The records read with <paramref name="stored"/> that are dated in <paramref name="entry"/>, in their order.</summary>
    private static List<GoalRecord> RecordsIn(StoredEntries stored, CalendarEntry entry) =>
        stored.Records.Where(record => record.Date >= entry.FirstDate && record.Date <= entry.LastDate).ToList();

    private static RecordWriteResult ValueRefused(RecordProblem problem) => problem switch
    {
        RecordProblem.AnswerMissing => RecordWriteResult.Refused(RecordWriteOutcome.AnswerMissing, RecordTexts.AnswerMissing),
        RecordProblem.CountOutOfRange => RecordWriteResult.Refused(RecordWriteOutcome.CountOutOfRange, RecordTexts.CountOutOfRange),
        RecordProblem.DurationOutOfRange => RecordWriteResult.Refused(RecordWriteOutcome.DurationOutOfRange, RecordTexts.DurationOutOfRange),
        _ => throw new ArgumentOutOfRangeException(nameof(problem), "Not a value refusal."),
    };

    private static RecordWriteOutcome WriteOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => RecordWriteOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => RecordWriteOutcome.NotForAdministrators,
        _ => RecordWriteOutcome.NotFound,
    };

    private static HistoryOutcome HistoryOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => HistoryOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => HistoryOutcome.NotForAdministrators,
        _ => HistoryOutcome.NotFound,
    };

    private static NoteSaveOutcome NoteOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => NoteSaveOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => NoteSaveOutcome.NotForAdministrators,
        _ => NoteSaveOutcome.NotFound,
    };

    private static RecentNotesOutcome RecentNotesOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => RecentNotesOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => RecentNotesOutcome.NotForAdministrators,
        _ => RecentNotesOutcome.NotFound,
    };

    private static RecordableOutcome RecordableOutcomeOf(AccessRefusal refusal) => refusal switch
    {
        AccessRefusal.NotSignedIn => RecordableOutcome.NotSignedIn,
        AccessRefusal.NotForAdministrators => RecordableOutcome.NotForAdministrators,
        _ => RecordableOutcome.NotFound,
    };

    private enum AccessRefusal
    {
        NotSignedIn,
        NotForAdministrators,
        NotFound,
    }

    /// <summary>The acting user's account and goal, or why the use case refuses before anything else is looked at.</summary>
    private sealed record Access(AccountId Owner, Goal? Goal, AccessRefusal? Refusal, string? Message)
    {
        public static Access Refused(AccessRefusal refusal, string message) => new(default, null, refusal, message);
    }
}
