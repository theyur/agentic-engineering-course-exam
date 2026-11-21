using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// <c>GetRecentNotesAsync</c> (US4-AS6, FR-008; research R-04) over the fakes of <see cref="RecordWorld"/>. The user has
/// no stored time zone, so the goals are in UTC and start on Monday 16 November 2026, created at 08:00Z that day. A day
/// runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later; a week runs from Monday 00:00Z to
/// the next Monday 00:00Z and its grace period ends on that Wednesday 00:00Z. Records and notes are seeded straight
/// into the fake store, so the only store calls a test sees are the use case's own reads. Every instant is worked out
/// by hand.
/// </summary>
public sealed class RecentNotesTests
{
    private const string NotFound = "This goal does not exist.";

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sat21 = Date(2026, 11, 21);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Tue24 = Date(2026, 11, 24);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);
    private static readonly DateOnly Mon30 = Date(2026, 11, 30);
    private static readonly DateOnly Sun6 = Date(2026, 12, 6);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    /// <summary>
    /// Tuesday 24 November, 10:00Z: Tuesday 24 is Due; Monday 23 (grace end 2026-11-26T00:00:00Z) and Sunday 22 (grace
    /// end 2026-11-25T00:00:00Z) are Pending; Saturday 21 (grace end 2026-11-24T00:00:00Z) and earlier are final.
    /// </summary>
    private static readonly DateTimeOffset Tue24TenOClock = Utc("2026-11-24T10:00:00Z");

    /// <summary>
    /// Wednesday 2 December, 10:00Z: the week of Monday 30 November is Due; the week of Monday 23 ended at
    /// 2026-11-30T00:00:00Z and its grace period at 2026-12-02T00:00:00Z, so it is final, as is the week of Monday 16.
    /// </summary>
    private static readonly DateTimeOffset Wed2TenOClock = Utc("2026-12-02T10:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private Task<RecentNotesResult> RecentNotesAsync(GoalId goal) =>
        _world.Service.GetRecentNotesAsync(_world.User, goal, CancellationToken.None);

    private void Note(GoalId goal, DateOnly key, string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        _world.Records.SeedNote(goal, key, note!);
    }

    private static IReadOnlyList<RecentNote> Found(RecentNotesResult result)
    {
        Assert.Equal(RecentNotesOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Notes;
    }

    private static RecentNote Recent(DateOnly first, DateOnly last, string note, EvaluationState state) =>
        new() { FirstDate = first, LastDate = last, Note = note, State = state };

    [Fact]
    public async Task US4_AS6_notes_on_7_commitments_give_the_5_with_the_latest_dates_newest_first_with_their_states()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Thu19, RecordValue.FromAmount(20));
        _world.Records.Seed(goal, Fri20, RecordValue.FromAmount(12));
        _world.Records.Seed(goal, Sat21, RecordValue.FromAmount(25));
        _world.Records.Seed(goal, Sun22, RecordValue.FromAmount(20));
        Note(goal, Wed18, "Note of Wed 18.");
        Note(goal, Thu19, "Note of Thu 19.");
        Note(goal, Fri20, "Fell asleep early.");
        Note(goal, Sat21, "Long train ride.");
        Note(goal, Sun22, "Read in the park.");
        Note(goal, Mon23, "Away for the weekend.");
        Note(goal, Tue24, "Started a new book.");
        _world.At(Tue24TenOClock);

        var notes = Found(await RecentNotesAsync(goal));

        Assert.Equal(
            [
                Recent(Tue24, Tue24, "Started a new book.", EvaluationState.Due),
                Recent(Mon23, Mon23, "Away for the weekend.", EvaluationState.Pending),
                Recent(Sun22, Sun22, "Read in the park.", EvaluationState.Pending),
                Recent(Sat21, Sat21, "Long train ride.", EvaluationState.Met),
                Recent(Fri20, Fri20, "Fell asleep early.", EvaluationState.Failed),
            ],
            notes);
    }

    [Fact]
    public async Task US4_AS6_the_recent_notes_take_at_most_6_store_reads_each_over_one_entry()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        foreach (var day in new[] { Wed18, Thu19, Fri20, Sat21, Sun22, Mon23, Tue24 })
        {
            Note(goal, day, "A note.");
        }

        _world.At(Tue24TenOClock);

        Assert.Equal(5, Found(await RecentNotesAsync(goal)).Count);

        Assert.Equal(
            [
                nameof(IRecordStore.ReadRecentNoteKeysAsync),
                nameof(IRecordStore.ReadExemptionsAsync),
                nameof(IRecordStore.ReadAsync),
                nameof(IRecordStore.ReadAsync),
                nameof(IRecordStore.ReadAsync),
                nameof(IRecordStore.ReadAsync),
                nameof(IRecordStore.ReadAsync),
            ],
            _world.Records.Calls);
    }

    [Fact]
    public async Task US4_AS6_a_weeks_note_is_ordered_by_its_Monday_and_carries_its_first_and_last_date()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Mon16, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.Seed(goal, Wed18, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.Seed(goal, Fri20, RecordValue.FromAnswer(YesNoAnswer.Yes));
        Note(goal, Mon16, "Good week.");
        Note(goal, Mon23, "Knee was sore.");
        Note(goal, Mon30, "Better today.");
        _world.At(Wed2TenOClock);

        var notes = Found(await RecentNotesAsync(goal));

        Assert.Equal(
            [
                Recent(Mon30, Sun6, "Better today.", EvaluationState.Due),
                Recent(Mon23, Sun29, "Knee was sore.", EvaluationState.Missed),
                Recent(Mon16, Sun22, "Good week.", EvaluationState.Met),
            ],
            notes);
    }

    [Fact]
    public async Task A_goal_with_fewer_than_5_notes_gives_them_all()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        Note(goal, Fri20, "Fell asleep early.");
        Note(goal, Sat21, "Long train ride.");
        _world.At(Tue24TenOClock);

        var notes = Found(await RecentNotesAsync(goal));

        Assert.Equal([Sat21, Fri20], notes.Select(n => n.FirstDate));
        Assert.All(notes, n => Assert.Equal(EvaluationState.Missed, n.State));
    }

    [Fact]
    public async Task A_goal_without_notes_gives_none_after_one_store_read()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Thu19, RecordValue.FromAmount(20));
        _world.At(Tue24TenOClock);

        Assert.Empty(Found(await RecentNotesAsync(goal)));
        Assert.Equal([nameof(IRecordStore.ReadRecentNoteKeysAsync)], _world.Records.Calls);
    }

    [Fact]
    public async Task Another_accounts_goal_gives_This_goal_does_not_exist_and_no_note()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning, _world.OtherUser);
        Note(goal, Fri20, "Fell asleep early.");
        _world.At(Tue24TenOClock);

        var result = await RecentNotesAsync(goal);

        Assert.Equal(RecentNotesOutcome.NotFound, result.Outcome);
        Assert.Equal(NotFound, result.Message);
        Assert.Empty(result.Notes);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task Slice_7_FR_020_a_note_of_an_Excused_or_Paused_entry_has_that_state_after_one_exemption_read()
    {
        // Tuesday 24 at 10:00Z: Saturday 21 and Friday 20 would be final; Friday 20 is excused and Saturday 21 paused,
        // with a record kept on it; Sunday 22 is Pending.
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Sat21, RecordValue.FromAmount(20));
        _world.Records.SeedExcusal(goal, Fri20);
        _world.Records.SeedPause(goal, Sat21, Sat21);
        Note(goal, Fri20, "Ill.");
        Note(goal, Sat21, "Travelling.");
        Note(goal, Sun22, "Read in the park.");
        _world.At(Tue24TenOClock);

        var notes = Found(await RecentNotesAsync(goal));

        Assert.Equal(
            [
                Recent(Sun22, Sun22, "Read in the park.", EvaluationState.Pending),
                Recent(Sat21, Sat21, "Travelling.", EvaluationState.Paused),
                Recent(Fri20, Fri20, "Ill.", EvaluationState.Excused),
            ],
            notes);
        Assert.Single(_world.Records.Calls, nameof(IRecordStore.ReadExemptionsAsync));
    }

    [Fact]
    public async Task Slice_7_FR_020_a_note_of_a_Paused_week_has_the_state_Paused()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Wed18, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.SeedPause(goal, Sun22, Sun22);
        Note(goal, Mon16, "Good week.");
        _world.At(Wed2TenOClock);

        var note = Assert.Single(Found(await RecentNotesAsync(goal)));

        Assert.Equal(Recent(Mon16, Sun22, "Good week.", EvaluationState.Paused), note);
    }
}
