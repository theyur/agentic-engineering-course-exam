using HT.Domain.Goals;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// Slice 8 US3-AS5, research R-02: a calendar over a range that spans a commitment-based attempt and a per-week one.
/// Attempt 1 is daily (every day, commitment-based) from Monday 2 November 2026, ended on Monday 16 November (last date
/// 15 November); attempt 2 is 3 times a week from Wednesday 18 November, so its first week starts on Monday 23 November.
/// </summary>
public class GoalTimelineMixedCalendarTests
{
    private static readonly DateOnly Nov2 = Date(2026, 11, 2);
    private static readonly DateOnly Nov15 = Date(2026, 11, 15);
    private static readonly DateOnly Nov16 = Date(2026, 11, 16);
    private static readonly DateOnly Nov18 = Date(2026, 11, 18);
    private static readonly DateOnly Nov23 = Date(2026, 11, 23);
    private static readonly DateOnly Nov29 = Date(2026, 11, 29);

    private static readonly Goal Restarted = GoalOf(
        AttemptFrom(1, Nov2, EndedOn(Nov16), SettingsFrom(OnDays(AllDays), Nov2, NamedTimeZone.Utc)),
        AttemptFrom(2, Nov18, null, SettingsFrom(PerWeek(), Nov18, NamedTimeZone.Utc)));

    private static EvaluationCalendar Calendar(DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryCalendar(Restarted, first, last, out var calendar));
        return calendar;
    }

    [Fact]
    public void A_range_across_both_attempts_fills_both_lists_and_takes_the_latest_attempts_kind()
    {
        var calendar = Calendar(Nov2, Nov29);

        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Equal(Enumerable.Range(0, 14).Select(i => Nov2.AddDays(i)), calendar.Commitments.Select(c => c.Date));
        Assert.Equal(
            [new EvaluationPeriod(Nov23, Nov29, Utc("2026-11-23T00:00:00Z"), Utc("2026-11-30T00:00:00Z"))],
            calendar.Periods);
    }

    [Fact]
    public void TryEntries_merges_both_lists_in_date_order()
    {
        Assert.True(GoalTimeline.TryEntries(Restarted, Nov2, Nov29, out var entries));

        Assert.Equal(15, entries.Count);
        Assert.Equal(entries.OrderBy(entry => entry.FirstDate), entries);
        Assert.Equal(Nov15, entries[13].LastDate);
        Assert.Equal((Nov23, Nov29), (entries[14].FirstDate, entries[14].LastDate));
        Assert.Equal(Nov23, GoalTimeline.EntryOf(Restarted, Date(2026, 11, 25))!.FirstDate);
        Assert.Equal(Nov15, GoalTimeline.EntryOf(Restarted, Nov15)!.FirstDate);
    }

    [Fact]
    public void A_range_inside_attempt_1_is_commitment_based_with_no_periods()
    {
        var calendar = Calendar(Date(2026, 11, 3), Date(2026, 11, 10));

        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Equal(8, calendar.Commitments.Count);
        Assert.Empty(calendar.Periods);
    }

    [Fact]
    public void A_range_between_the_attempts_has_no_entry_and_the_kind_of_the_latest_attempt_started_by_its_end()
    {
        var afterAttempt2Starts = Calendar(Nov16, Date(2026, 11, 22));
        var beforeAttempt2Starts = Calendar(Nov16, Date(2026, 11, 17));

        Assert.Empty(afterAttempt2Starts.Commitments);
        Assert.Empty(afterAttempt2Starts.Periods);
        Assert.Equal(GoalScheduleKind.PeriodBased, afterAttempt2Starts.Kind);
        Assert.Empty(beforeAttempt2Starts.Commitments);
        Assert.Empty(beforeAttempt2Starts.Periods);
        Assert.Equal(GoalScheduleKind.CommitmentBased, beforeAttempt2Starts.Kind);
        Assert.Null(GoalTimeline.EntryOf(Restarted, Date(2026, 11, 20)));
    }

    [Fact]
    public void A_range_before_every_attempt_takes_attempt_1s_kind()
    {
        var calendar = Calendar(Date(2026, 10, 1), Date(2026, 10, 31));

        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Empty(calendar.Commitments);
        Assert.Empty(calendar.Periods);
    }
}
