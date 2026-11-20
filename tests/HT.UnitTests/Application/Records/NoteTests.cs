using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// Notes (US4-AS1 to AS5, AS8, AS9, FR-007): <c>SaveNoteAsync</c>, the note given with <c>RecordAsync</c>, and the note
/// a <c>DeleteRecordAsync</c> leaves, over the fakes of
/// <see cref="RecordWorld"/>. The user has no stored time zone, so the goals are in UTC and start on Monday 16 November
/// 2026, created at 08:00Z that day. A day runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours
/// later, so Monday 16's ends at 2026-11-19T00:00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class NoteTests
{
    private const string NoteTooLong = "The note can have at most 500 characters.";
    private const string DayNotBegun = "You cannot record a day that has not begun.";
    private const string NothingScheduled = "Nothing is scheduled on this date.";
    private const string NotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";

    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");

    /// <summary>Wednesday 18 November, 10:00Z: Wednesday 18 is the current date, Thursday 19 has not begun.</summary>
    private static readonly DateTimeOffset Wed18TenOClock = Utc("2026-11-18T10:00:00Z");

    /// <summary>Friday 20 November, 10:00Z: Monday 16's grace period ended at 2026-11-19T00:00:00Z, so it is final.</summary>
    private static readonly DateTimeOffset Fri20TenOClock = Utc("2026-11-20T10:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private static GoalTargetFields YesNoOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private Task<GoalId> GoalAsync(GoalTargetFields target, ActingUser? user = null) =>
        _world.CreateGoalAsync(target, Mon16, Mon16Morning, user);

    private Task<NoteSaveResult> SaveNoteAsync(GoalId goal, DateOnly date, string? note, ActingUser? user = null) =>
        _world.Service.SaveNoteAsync(user ?? _world.User, goal, date, note, CancellationToken.None);

    private static HistoryEntry Saved(NoteSaveResult result)
    {
        Assert.Equal(NoteSaveOutcome.Saved, result.Outcome);
        Assert.Null(result.Message);
        return result.Entry!;
    }

    private static void AssertRefused(NoteSaveResult result, NoteSaveOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Entry);
    }

    private void AssertNoNoteWritten() => Assert.DoesNotContain(nameof(IRecordStore.SaveNoteAsync), _world.Records.Calls);

    // ---- AS1: a note never changes the state ----

    [Fact]
    public async Task US4_AS1_a_note_on_a_Missed_commitment_is_kept_and_the_commitment_stays_Missed()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Assert.Equal(EvaluationState.Missed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);

        var entry = Saved(await SaveNoteAsync(goal, Mon16, "Away for the weekend."));

        Assert.Equal((Mon16, Mon16), (entry.FirstDate, entry.LastDate));
        Assert.Equal(EvaluationState.Missed, entry.State);
        Assert.Equal("Away for the weekend.", entry.Note);
        Assert.Empty(entry.Records);
        Assert.True(entry.IsCorrection);
        var read = Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16));
        Assert.Equal(EvaluationState.Missed, read.State);
        Assert.Equal("Away for the weekend.", read.Note);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task US4_AS1_a_note_on_a_Due_commitment_keeps_it_Due_with_its_grace_end_and_records()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        await _world.RecordAsync(goal, Mon16, Minutes(12));

        var entry = Saved(await SaveNoteAsync(goal, Mon16, "Fell asleep early."));

        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Equal(Utc("2026-11-19T00:00:00Z"), entry.GraceEnds);
        Assert.Equal(12, Assert.Single(entry.Records).Amount);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), entry.Progress);
        Assert.Equal("Fell asleep early.", entry.Note);
        Assert.False(entry.IsCorrection);
    }

    [Fact]
    public async Task US4_AS1_a_note_saved_on_its_own_replaces_the_existing_note_and_is_trimmed()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Saved(await SaveNoteAsync(goal, Mon16, "Away for the weekend."));

        var entry = Saved(await SaveNoteAsync(goal, Mon16, "  Visiting family.  "));

        Assert.Equal("Visiting family.", entry.Note);
        Assert.Equal("Visiting family.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    // ---- AS2: an empty note saved on its own removes the note ----

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task US4_AS2_an_empty_note_or_one_of_only_spaces_saved_on_its_own_removes_the_note(string? note)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Saved(await SaveNoteAsync(goal, Mon16, "Away for the weekend."));

        var entry = Saved(await SaveNoteAsync(goal, Mon16, note));

        Assert.Null(entry.Note);
        Assert.Equal(EvaluationState.Missed, entry.State);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
        Assert.Null(Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).Note);
    }

    // ---- AS3: a note given with a record adds or replaces, an empty one removes, and no note leaves it ----
    // (amended by specs/006-note-editing D-1: an empty note or one of only spaces now removes the note)

    [Fact]
    public async Task US4_AS3_a_Yes_with_no_note_keeps_the_weeks_note()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);
        Saved(await SaveNoteAsync(goal, Tue17, "Knee was sore."));

        var result = await _world.RecordAsync(goal, Wed18, Yes());

        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Equal("Knee was sore.", result.Entry!.Note);
        Assert.Equal("Knee was sore.", Assert.Single(await _world.HistoryAsync(goal, Mon16, Sun22)).Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task US4_AS3_a_Yes_with_an_empty_note_or_one_of_only_spaces_removes_the_weeks_note(string note)
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);
        Saved(await SaveNoteAsync(goal, Tue17, "Knee was sore."));

        var result = await _world.RecordAsync(goal, Wed18, Yes(note));

        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Single(result.Entry!.Records);
        Assert.Null(result.Entry.Note);
        var week = Assert.Single(await _world.HistoryAsync(goal, Mon16, Sun22));
        Assert.Null(week.Note);
        Assert.Single(week.Records);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task US4_AS3_a_Yes_with_the_note_Better_today_replaces_the_weeks_note()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);
        Saved(await SaveNoteAsync(goal, Tue17, "Knee was sore."));

        var result = await _world.RecordAsync(goal, Wed18, Yes("Better today."));

        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Equal("Better today.", result.Entry!.Note);
        var week = Assert.Single(await _world.HistoryAsync(goal, Mon16, Sun22));
        Assert.Equal("Better today.", week.Note);
        Assert.Equal(Mon16, Assert.Single(_world.Records.NotesOfGoal(goal)).Key);
    }

    // ---- AS8 and AS9 (added by specs/006-note-editing US2-AS3 and US2-AS4): the note of a commitment without a record
    // moves into its first record, and stays when its only record is deleted ----

    [Theory]
    [InlineData("Away for the weekend.", "Away for the weekend.")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, "Away for the weekend.")]
    public async Task US4_AS9_the_first_record_of_a_Missed_commitment_keeps_its_note_with_that_note_or_none_and_an_empty_note_removes_it(
        string? given, string? expected)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Saved(await SaveNoteAsync(goal, Mon16, "Away for the weekend."));
        Assert.Equal(EvaluationState.Missed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(20, given));

        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Equal(20, Assert.Single(result.Entry!.Records).Amount);
        Assert.Equal(expected, result.Entry.Note);
        var read = Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16));
        Assert.Single(read.Records);
        Assert.Equal(expected, read.Note);
        Assert.Single(_world.Records.RecordsOfGoal(goal));
        Assert.Equal(expected, _world.Records.NotesOfGoal(goal).Values.SingleOrDefault()?.Value);
    }

    /// <summary>
    /// Monday 16 is recorded at 10:00Z that day with 12 min and "Fell asleep early.". At 10:00Z on Wednesday 18 its grace
    /// period (to 2026-11-19T00:00:00Z) has not ended, so without a record it is Pending; at 10:00Z on Friday 20 it has,
    /// so it is Missed.
    /// </summary>
    [Theory]
    [InlineData("2026-11-18T10:00:00Z", EvaluationState.Pending)]
    [InlineData("2026-11-20T10:00:00Z", EvaluationState.Missed)]
    public async Task US4_AS8_deleting_the_only_record_of_a_commitment_with_a_note_leaves_no_record_and_the_note(
        string deletedAt, EvaluationState state)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        var recorded = await _world.RecordAsync(goal, Mon16, Minutes(12, "Fell asleep early."));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        _world.At(Utc(deletedAt));

        var result = await _world.DeleteAsync(goal, recorded.RecordId!.Value);

        Assert.Equal(RecordWriteOutcome.Deleted, result.Outcome);
        Assert.Empty(result.Entry!.Records);
        Assert.Equal(state, result.Entry.State);
        Assert.Equal("Fell asleep early.", result.Entry.Note);
        var read = Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16));
        Assert.Empty(read.Records);
        Assert.Equal("Fell asleep early.", read.Note);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
        Assert.Equal("Fell asleep early.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    // ---- AS4: at most 500 characters ----

    [Fact]
    public async Task US4_AS4_a_note_of_501_characters_is_refused_and_changes_nothing()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Saved(await SaveNoteAsync(goal, Mon16, "Away for the weekend."));

        var result = await SaveNoteAsync(goal, Mon16, new string('a', 501));

        AssertRefused(result, NoteSaveOutcome.NoteTooLong, NoteTooLong);
        Assert.Single(_world.Records.Calls, nameof(IRecordStore.SaveNoteAsync));
        Assert.Equal("Away for the weekend.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    [Fact]
    public async Task US4_AS4_a_note_of_500_characters_is_accepted_also_with_spaces_around_it()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        var note = new string('a', 500);

        var entry = Saved(await SaveNoteAsync(goal, Mon16, "  " + note + "  "));

        Assert.Equal(note, entry.Note);
        Assert.Equal(note, Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    // ---- AS5: any begun date of a week names the week ----

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    public async Task US4_AS5_a_note_for_any_begun_date_of_a_week_is_the_weeks_one_note(int day)
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);

        var entry = Saved(await SaveNoteAsync(goal, Date(2026, 11, day), "Knee was sore."));

        Assert.Equal((Mon16, Sun22), (entry.FirstDate, entry.LastDate));
        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Equal("Knee was sore.", entry.Note);
        var note = Assert.Single(_world.Records.NotesOfGoal(goal));
        Assert.Equal(Mon16, note.Key);
    }

    [Fact]
    public async Task US4_AS5_a_second_date_of_the_week_replaces_the_weeks_note()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);
        Saved(await SaveNoteAsync(goal, Mon16, "Knee was sore."));

        var entry = Saved(await SaveNoteAsync(goal, Wed18, "Better today."));

        Assert.Equal("Better today.", entry.Note);
        Assert.Equal("Better today.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    [Fact]
    public async Task US4_AS5_a_date_of_the_current_week_that_has_not_begun_is_refused()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18TenOClock);

        var result = await SaveNoteAsync(goal, Thu19, "Knee was sore.");

        AssertRefused(result, NoteSaveOutcome.DayNotBegun, DayNotBegun);
        AssertNoNoteWritten();
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    // ---- FR-002 and FR-005: the date, then the note ----

    [Fact]
    public async Task An_unscheduled_date_is_refused_with_Nothing_is_scheduled_on_this_date()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Wed18TenOClock);

        var result = await SaveNoteAsync(goal, Tue17, "Rest day.");

        AssertRefused(result, NoteSaveOutcome.NothingScheduled, NothingScheduled);
        AssertNoNoteWritten();
    }

    [Fact]
    public async Task A_date_before_the_start_date_is_refused_with_Nothing_is_scheduled_on_this_date()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Wed18TenOClock);

        var result = await SaveNoteAsync(goal, Sun15, "Before the start.");

        AssertRefused(result, NoteSaveOutcome.NothingScheduled, NothingScheduled);
        AssertNoNoteWritten();
    }

    [Fact]
    public async Task The_date_is_checked_before_the_note()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Wed18TenOClock);
        var tooLong = new string('a', 501);

        AssertRefused(await SaveNoteAsync(goal, Tue17, tooLong), NoteSaveOutcome.NothingScheduled, NothingScheduled);
        AssertRefused(await SaveNoteAsync(goal, Date(2026, 11, 20), tooLong), NoteSaveOutcome.DayNotBegun, DayNotBegun);
        AssertNoNoteWritten();
    }

    // ---- FR-005 and FR-017: access, before anything else ----

    [Fact]
    public async Task Another_accounts_goal_gives_This_goal_does_not_exist_and_no_note_is_written()
    {
        var goal = await GoalAsync(Reading20MinDaily, _world.OtherUser);
        _world.At(Fri20TenOClock);

        var result = await SaveNoteAsync(goal, Mon16, "Away for the weekend.");

        AssertRefused(result, NoteSaveOutcome.NotFound, NotFound);
        Assert.Empty(_world.Records.Calls);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task An_administrator_and_an_anonymous_user_are_refused_before_the_goal()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);

        AssertRefused(await SaveNoteAsync(goal, Mon16, "Away.", _world.Administrator), NoteSaveOutcome.NotForAdministrators, NotForAdministrators);
        AssertRefused(await SaveNoteAsync(goal, Mon16, "Away.", ActingUser.Anonymous), NoteSaveOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
    }
}
