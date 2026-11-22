using HT.Application.Accounts;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// Slice 8 (T053; FR-009, FR-010, FR-014, FR-016, FR-027, research R-07): <see cref="RecordingService"/> over goals with
/// attempts and endings, on <see cref="AttemptWorld"/>. Unless a test says otherwise the user has no stored zone, so
/// everything is in UTC: a day runs from 00:00Z to 00:00Z and its grace period ends 48 hours after it. "Reading, 10 min
/// daily" starts on Monday 2 November 2026 and is ended at 2026-11-16T10:00:00Z, so its ending date is Monday 16 and its
/// last date Sunday 15 November; 14 November's grace period ends at 2026-11-17T00:00:00Z and 15 November's at
/// 2026-11-18T00:00:00Z. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class AttemptRecordingTests
{
    private const string NothingScheduled = "Nothing is scheduled on this date.";
    private const string DayNotBegun = "You cannot record a day that has not begun.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields TenMinutesDaily => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields AtMostOneSweetADay => CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    /// <summary>"Reading, 10 min daily" from Monday 2 November, ended at <see cref="EndedAt"/> (ending date 16, last date 15 November).</summary>
    private async Task<GoalId> EndedReadingAsync(string? endingNote = null)
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(EndedAt);
        Assert.Equal((Mon16, Sun15), await _world.EndAsync(goal, Outcome.Abandoned, endingNote));
        return goal;
    }

    [Fact]
    public async Task The_History_of_a_restarted_goal_covers_only_the_latest_attempt()
    {
        var goal = await EndedReadingAsync();
        _world.SeedAmount(goal, Sun15, 10);
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, AtMostOneSweetADay, Wed18);
        _world.At(Utc("2026-11-20T10:00:00Z"));

        var history = await _world.HistoryAsync(goal, Date(2026, 11, 1), Fri20);

        Assert.Equal([Wed18, Thu19, Fri20], history.Entries.Select(e => e.FirstDate));
        Assert.All(history.Entries, e => Assert.Equal((2, true, (string?)null), (e.AttemptNumber, e.IsLatestAttempt, e.AttemptName)));
        Assert.All(history.Entries, e => Assert.Equal(Measurement.Count, history.SettingsOf(e).Measurement));
        Assert.Equal(BuildOrReduce.Reduce, history.BuildOrReduce);
        Assert.Equal(Fri20, history.CurrentDate);
    }

    [Fact]
    public async Task What_can_be_recorded_now_adds_an_earlier_attempts_open_Pending_entry_with_its_own_fields_and_drops_it_at_its_grace_end()
    {
        var goal = await EndedReadingAsync();
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, AtMostOneSweetADay, Tue17);

        _world.At(Utc("2026-11-17T10:00:00Z"));
        var recordable = await _world.RecordableAsync(goal);

        Assert.Equal([(Tue17, EvaluationState.Due, 2), (Sun15, EvaluationState.Pending, 1)], recordable.Entries.Select(e => (e.FirstDate, e.State, e.AttemptNumber)));
        var earlier = recordable.Entries[1];
        Assert.False(earlier.IsLatestAttempt);
        Assert.Equal("Reading, 10 min daily", earlier.AttemptName);
        Assert.Equal((BuildOrReduce.Build, Measurement.Duration, GoalScheduleKind.CommitmentBased), (recordable.SettingsOf(earlier).BuildOrReduce, recordable.SettingsOf(earlier).Measurement, recordable.SettingsOf(earlier).GoalScheduleKind));
        Assert.Equal((BuildOrReduce.Reduce, Measurement.Count), (recordable.BuildOrReduce, recordable.Measurement));
        Assert.True(recordable.Entries[0].IsLatestAttempt);

        // 15 November's grace period ends at 2026-11-18T00:00:00Z.
        _world.At(Utc("2026-11-17T23:59:59Z"));
        Assert.Contains(Sun15, (await _world.RecordableAsync(goal)).Entries.Select(e => e.FirstDate));
        _world.At(Utc("2026-11-18T00:00:00Z"));
        Assert.Equal([Wed18, Tue17], (await _world.RecordableAsync(goal)).Entries.Select(e => e.FirstDate));
    }

    [Fact]
    public async Task A_record_and_a_note_on_the_ending_date_of_a_commitment_based_goal_are_kept_but_never_mapped_to_an_entry()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(Utc("2026-11-16T09:00:00Z"));
        var kept = await _world.RecordAsync(goal, Mon16, Minutes(10, "Morning session."));
        Assert.Equal(RecordWriteOutcome.Recorded, kept.Outcome);
        _world.At(EndedAt);
        await _world.EndAsync(goal);

        _world.At(Utc("2026-11-16T11:00:00Z"));
        Assert.DoesNotContain(Mon16, (await _world.HistoryAsync(goal, Mon2, Mon16)).Entries.Select(e => e.FirstDate));
        Assert.Empty(await _world.RecentNotesAsync(goal));
        await AssertStrayRecordIsKeptAsync(goal, kept.RecordId!.Value);
        Assert.Contains(Mon16, _world.Records.NotesOfGoal(goal).Keys);
    }

    [Fact]
    public async Task A_record_and_a_note_in_the_week_in_progress_of_a_per_week_goal_are_kept_but_never_mapped_to_an_entry()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Created);
        _world.At(Utc("2026-11-17T09:00:00Z"));
        var kept = await _world.RecordAsync(goal, Tue17, Yes("Week started well."));
        Assert.Equal(RecordWriteOutcome.Recorded, kept.Outcome);

        // Ended on Wednesday 18 November: the last date is Sunday 15 November, so the week of 16 November is in no attempt.
        _world.At(Utc("2026-11-18T10:00:00Z"));
        Assert.Equal((Wed18, Sun15), await _world.EndAsync(goal));

        Assert.DoesNotContain(Mon16, (await _world.HistoryAsync(goal, Mon2, Wed18)).Entries.Select(e => e.FirstDate));
        Assert.Empty(await _world.RecentNotesAsync(goal));
        await AssertStrayRecordIsKeptAsync(goal, kept.RecordId!.Value);
        Assert.Contains(Mon16, _world.Records.NotesOfGoal(goal).Keys);
    }

    [Fact]
    public async Task Adding_a_record_or_a_note_between_two_attempts_gives_Nothing_is_scheduled_on_this_date()
    {
        var goal = await EndedReadingAsync();
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Wed18);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        foreach (var day in new[] { Mon16, Tue17 })
        {
            var recorded = await _world.RecordAsync(goal, day, Minutes(10));
            Assert.Equal((RecordWriteOutcome.NothingScheduled, NothingScheduled), (recorded.Outcome, recorded.Message));
            var saved = await _world.Recording.SaveNoteAsync(_world.User, goal, day, "A note.", CancellationToken.None);
            Assert.Equal((NoteSaveOutcome.NothingScheduled, NothingScheduled), (saved.Outcome, saved.Message));
        }

        Assert.Empty(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task A_date_of_a_waiting_attempt_in_a_zone_west_of_the_earlier_one_has_not_begun_until_its_own_day_begins()
    {
        var user = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created, user);

        // Kyiv is UTC+2: 2026-11-16T10:00:00Z is Monday 16 November 12:00 there.
        _world.At(EndedAt);
        Assert.Equal((Mon16, Sun15), await _world.EndAsync(goal, user: user));
        _world.At(Utc("2026-11-16T10:05:00Z"));
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await _world.Profiles.ChangeTimeZoneAsync(user, "America/New_York", CancellationToken.None)).Outcome);
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Wed18, user);

        // 2026-11-18T04:59:59Z is Wednesday 18 November 06:59:59 in Kyiv but Tuesday 17 November 23:59:59 in New York.
        _world.At(Utc("2026-11-18T04:59:59Z"));
        var early = await _world.RecordAsync(goal, Wed18, Minutes(10), user);
        Assert.Equal((RecordWriteOutcome.DayNotBegun, DayNotBegun), (early.Outcome, early.Message));
        var earlyNote = await _world.Recording.SaveNoteAsync(user, goal, Wed18, "Too early.", CancellationToken.None);
        Assert.Equal((NoteSaveOutcome.DayNotBegun, DayNotBegun), (earlyNote.Outcome, earlyNote.Message));

        _world.At(Utc("2026-11-18T05:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Wed18, Minutes(10), user)).Outcome);
    }

    [Fact]
    public async Task A_record_of_an_earlier_attempts_date_is_accepted_and_checked_against_its_own_measurement()
    {
        var goal = await EndedReadingAsync();
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, AtMostOneSweetADay, Tue17);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        // 1441 is a valid count for attempt 2, but not a valid duration for attempt 1's 15 November.
        var refused = await _world.RecordAsync(goal, Sun15, Amount(1441));
        Assert.Equal((RecordWriteOutcome.DurationOutOfRange, "Enter a whole number of minutes from 0 to 1440."), (refused.Outcome, refused.Message));

        var recorded = await _world.RecordAsync(goal, Sun15, Minutes(10));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        Assert.Equal((1, false, EvaluationState.Pending), (recorded.Entry!.AttemptNumber, recorded.Entry.IsLatestAttempt, recorded.Entry.State));
        Assert.Equal(Measurement.Duration, recorded.Entry.Settings!.Measurement);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 10, 10), recorded.Entry.Progress);
        var earlier = Assert.Single((await _world.RecordableAsync(goal)).Entries, e => e.FirstDate == Sun15);
        Assert.Equal([10], earlier.Records.Select(r => r.Amount));

        var changed = await _world.Recording.ChangeRecordAsync(_world.User, goal, recorded.RecordId!.Value, Amount(1441), CancellationToken.None);
        Assert.Equal(RecordWriteOutcome.DurationOutOfRange, changed.Outcome);
        Assert.Equal(RecordWriteOutcome.Deleted, (await _world.Recording.DeleteRecordAsync(_world.User, goal, recorded.RecordId!.Value, CancellationToken.None)).Outcome);
    }

    [Fact]
    public async Task The_recent_notes_of_a_latest_attempt_with_no_date_read_nothing()
    {
        var goal = await EndedReadingAsync();
        _world.SeedNote(goal, Sun15, "Attempt 1's note.");
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Mon23);

        // Ended on Wednesday 18 November, the goal's current date from attempt 1: the last date 17 November is before 23 November.
        _world.At(Utc("2026-11-18T10:00:00Z"));
        Assert.Equal((Wed18, Tue17), await _world.EndAsync(goal));
        var calls = _world.Records.Calls.Count;

        Assert.Empty(await _world.RecentNotesAsync(goal));
        Assert.Equal(calls, _world.Records.Calls.Count);
    }

    [Fact]
    public async Task The_recent_notes_of_a_restarted_goal_cover_only_the_latest_attempt()
    {
        var goal = await EndedReadingAsync();
        _world.SeedNote(goal, Sat14, "Attempt 1's note.");
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Tue17);
        _world.SeedNote(goal, Wed18, "Attempt 2's note.");
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var notes = await _world.RecentNotesAsync(goal);

        Assert.Equal([(Wed18, "Attempt 2's note.")], notes.Select(n => (n.FirstDate, n.Note)));
    }

    [Fact]
    public async Task FR_027_an_ending_note_never_appears_in_the_recent_notes_or_the_History()
    {
        const string endingNote = "Knee injury, stopping for now.";
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.SeedNote(goal, Sat14, "A note of the commitment.");
        _world.At(EndedAt);
        await _world.EndAsync(goal, Outcome.NoLongerRelevant, endingNote);

        var notes = await _world.RecentNotesAsync(goal);
        var history = await _world.HistoryAsync(goal, Mon2, Mon16);

        Assert.Equal(["A note of the commitment."], notes.Select(n => n.Note));
        Assert.DoesNotContain(history.Entries, e => e.Note == endingNote);
        Assert.Equal(endingNote, (await _world.DetailsAsync(goal)).EndingNote);
    }

    /// <summary>Changing and deleting a record kept outside every attempt give "Nothing is scheduled on this date." and change nothing.</summary>
    private async Task AssertStrayRecordIsKeptAsync(GoalId goal, RecordId record)
    {
        var before = _world.Records.RecordsOfGoal(goal).ToList();
        var changed = await _world.Recording.ChangeRecordAsync(_world.User, goal, record, Minutes(20, "Changed."), CancellationToken.None);
        var deleted = await _world.Recording.DeleteRecordAsync(_world.User, goal, record, CancellationToken.None);

        Assert.Equal((RecordWriteOutcome.NothingScheduled, NothingScheduled), (changed.Outcome, changed.Message));
        Assert.Equal((RecordWriteOutcome.NothingScheduled, NothingScheduled), (deleted.Outcome, deleted.Message));
        Assert.Null(changed.Entry);
        Assert.Equal(before, _world.Records.RecordsOfGoal(goal));
    }
}
