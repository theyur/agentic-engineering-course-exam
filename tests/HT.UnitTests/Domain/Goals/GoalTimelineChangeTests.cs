using HT.Domain.Goals;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// SC-003, US10, FR-026, FR-027: the current date and the next commitment across time-zone moves, and every case of
/// <see cref="GoalTimeline.ChangeOf"/>. In November 2026 Kyiv is UTC+2, New York UTC−5, Tokyo UTC+9, Pago Pago
/// UTC−11 and Kiritimati UTC+14.
/// </summary>
public class GoalTimelineChangeTests
{
    private static readonly DateOnly Monday = Date(2026, 11, 16);
    private static readonly DateOnly Wednesday = Date(2026, 11, 18);
    private static readonly DateOnly Thursday = Date(2026, 11, 19);
    private static readonly DateOnly Friday = Date(2026, 11, 20);
    private static readonly DateOnly NextMonday = Date(2026, 11, 23);
    private static readonly DateOnly FirstDecember = Date(2026, 12, 1);

    /// <summary>12:00 Kyiv on Wednesday 18 November 2026 (05:00 in New York, also Wednesday): US10-AS4.</summary>
    private static readonly DateTimeOffset FirstChange = Utc("2026-11-18T10:00:00Z");

    /// <summary>12:00 New York on Thursday 19 November 2026: US10-AS10.</summary>
    private static readonly DateTimeOffset SecondChange = Utc("2026-11-19T17:00:00Z");

    private static readonly NamedTimeZone PagoPago = Zone("Pacific/Pago_Pago");
    private static readonly NamedTimeZone Kiritimati = Zone("Pacific/Kiritimati");

    private static Goal Changed(Goal goal, DateTimeOffset at, NamedTimeZone zone) =>
        GoalTimeline.ChangeOf(goal, at, zone).ApplyTo(goal);

    private static EvaluationCalendar Calendar(Goal goal, DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryCalendar(goal, first, last, out var calendar));
        return calendar;
    }

    private static IEnumerable<(DateOnly AppliesFrom, string Zone)> VersionsOf(Goal goal) =>
        goal.Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id));

    [Fact]
    public void US10_AS4_a_Mon_Wed_Fri_goal_keeps_Kyiv_through_Wednesday_and_uses_New_York_from_Friday()
    {
        var goal = Goal(OnDays(MonWedFri), Monday, Kyiv);

        Assert.Equal(GoalVersionChange.Add(Thursday, NewYork), GoalTimeline.ChangeOf(goal, FirstChange, NewYork));
        var changed = Changed(goal, FirstChange, NewYork);

        Assert.Equal(
            [
                new Commitment(Monday, Utc("2026-11-15T22:00:00Z"), Utc("2026-11-16T22:00:00Z")),
                new Commitment(Wednesday, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")),
                new Commitment(Friday, Utc("2026-11-20T05:00:00Z"), Utc("2026-11-21T05:00:00Z")),
                new Commitment(NextMonday, Utc("2026-11-23T05:00:00Z"), Utc("2026-11-24T05:00:00Z")),
            ],
            Calendar(changed, Monday, NextMonday).Commitments);
    }

    [Fact]
    public void US10_AS5_a_per_day_goal_uses_New_York_from_Thursday()
    {
        var goal = Goal(PerDay(), Monday, Kyiv);

        Assert.Equal(GoalVersionChange.Add(Thursday, NewYork), GoalTimeline.ChangeOf(goal, FirstChange, NewYork));
        var periods = Calendar(Changed(goal, FirstChange, NewYork), Wednesday, Thursday).Periods;

        Assert.Equal(
            [
                new EvaluationPeriod(Wednesday, Wednesday, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")),
                new EvaluationPeriod(Thursday, Thursday, Utc("2026-11-19T05:00:00Z"), Utc("2026-11-20T05:00:00Z")),
            ],
            periods);
    }

    [Fact]
    public void US10_AS5_a_per_week_goal_uses_New_York_from_the_next_Monday()
    {
        var goal = Goal(PerWeek(), Monday, Kyiv);

        Assert.Equal(GoalVersionChange.Add(NextMonday, NewYork), GoalTimeline.ChangeOf(goal, FirstChange, NewYork));
        var periods = Calendar(Changed(goal, FirstChange, NewYork), Monday, NextMonday).Periods;

        Assert.Equal(
            [
                new EvaluationPeriod(Monday, Date(2026, 11, 22), Utc("2026-11-15T22:00:00Z"), Utc("2026-11-22T22:00:00Z")),
                new EvaluationPeriod(NextMonday, Date(2026, 11, 29), Utc("2026-11-23T05:00:00Z"), Utc("2026-11-30T05:00:00Z")),
            ],
            periods);
    }

    [Fact]
    public void US10_AS5_a_goal_that_has_not_started_uses_New_York_from_its_start_date()
    {
        var goal = Goal(OnDays(MonWedFri), FirstDecember, Kyiv);

        Assert.Equal(GoalVersionChange.ReplaceFirst(NewYork), GoalTimeline.ChangeOf(goal, FirstChange, NewYork));
        var changed = Changed(goal, FirstChange, NewYork);

        Assert.Equal([(FirstDecember, "America/New_York")], VersionsOf(changed));
        Assert.Equal(
            new Commitment(Date(2026, 12, 2), Utc("2026-12-02T05:00:00Z"), Utc("2026-12-03T05:00:00Z")),
            Calendar(changed, FirstDecember, Date(2026, 12, 2)).Commitments.Single());
    }

    [Fact]
    public void US10_AS6_the_version_in_effect_and_the_later_one()
    {
        var goal = Changed(Goal(OnDays(MonWedFri), Monday, Kyiv), FirstChange, NewYork);

        var wednesday = GoalTimeline.CurrentDate(goal, FirstChange);
        Assert.Equal(Wednesday, wednesday);
        Assert.Equal((Monday, Kyiv), (GoalTimeline.VersionOf(goal, Wednesday).AppliesFrom, GoalTimeline.VersionOf(goal, Wednesday).Settings.TimeZone));
        Assert.Equal([(Thursday, "America/New_York")], VersionsOf(goal).Where(v => v.AppliesFrom > Wednesday));

        var fridayNoon = Utc("2026-11-20T17:00:00Z");
        Assert.Equal(Friday, GoalTimeline.CurrentDate(goal, fridayNoon));
        Assert.Equal((Thursday, NewYork), (GoalTimeline.VersionOf(goal, Friday).AppliesFrom, GoalTimeline.VersionOf(goal, Friday).Settings.TimeZone));
        Assert.DoesNotContain(goal.Versions, v => v.AppliesFrom > Friday);
        Assert.All(goal.Versions, v => Assert.Equal(goal.Versions[0].Settings.Target, v.Settings.Target));
    }

    [Fact]
    public void US10_AS10_a_second_change_replaces_a_pending_version_and_adds_after_one_in_effect()
    {
        var weekly = Changed(Goal(PerWeek(), Monday, Kyiv), FirstChange, NewYork);
        var daily = Changed(Goal(PerDay(), Monday, Kyiv), FirstChange, NewYork);
        var later = Changed(Goal(OnDays(MonWedFri), FirstDecember, Kyiv), FirstChange, NewYork);

        Assert.Equal(GoalVersionChange.ReplacePending(Tokyo, NextMonday), GoalTimeline.ChangeOf(weekly, SecondChange, Tokyo));
        Assert.Equal(GoalVersionChange.Add(Friday, Tokyo), GoalTimeline.ChangeOf(daily, SecondChange, Tokyo));
        Assert.Equal(GoalVersionChange.ReplaceFirst(Tokyo), GoalTimeline.ChangeOf(later, SecondChange, Tokyo));

        Assert.Equal([(Monday, "Europe/Kyiv"), (NextMonday, "Asia/Tokyo")], VersionsOf(Changed(weekly, SecondChange, Tokyo)));
        Assert.Equal(
            [(Monday, "Europe/Kyiv"), (Thursday, "America/New_York"), (Friday, "Asia/Tokyo")],
            VersionsOf(Changed(daily, SecondChange, Tokyo)));
        Assert.Equal([(FirstDecember, "Asia/Tokyo")], VersionsOf(Changed(later, SecondChange, Tokyo)));
    }

    [Fact]
    public void Moving_west_leaves_a_gap_in_which_the_old_day_is_current_and_the_new_day_is_next()
    {
        var goal = Changed(Goal(OnDays(AllDays), Monday, Kyiv), FirstChange, NewYork);

        // Wednesday ends at Kyiv midnight, 22:00 UTC; Thursday begins at New York midnight, 05:00 UTC.
        var inTheGap = Utc("2026-11-19T01:00:00Z");
        Assert.Equal(Wednesday, GoalTimeline.CurrentDate(goal, inTheGap));
        Assert.Equal(Thursday, GoalTimeline.NextCommitment(goal, inTheGap));
        Assert.Equal(Wednesday, GoalTimeline.CurrentDate(goal, Utc("2026-11-19T04:59:59Z")));
        Assert.Equal(Thursday, GoalTimeline.CurrentDate(goal, Utc("2026-11-19T05:00:00Z")));
    }

    [Fact]
    public void Moving_east_overlaps_and_the_day_in_progress_is_not_skipped()
    {
        var noonNewYork = Utc("2026-11-18T17:00:00Z");
        var goal = Goal(OnDays(AllDays), Monday, NewYork);
        Assert.Equal(GoalVersionChange.Add(Thursday, Kyiv), GoalTimeline.ChangeOf(goal, noonNewYork, Kyiv));
        goal = Changed(goal, noonNewYork, Kyiv);

        // Thursday begins at Kyiv midnight, 22:00 UTC on Wednesday; Wednesday ends at New York midnight, 05:00 UTC.
        var inTheOverlap = Utc("2026-11-18T23:00:00Z");
        Assert.Equal(Thursday, GoalTimeline.CurrentDate(goal, inTheOverlap));
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, inTheOverlap));
        Assert.Equal(Wednesday, GoalTimeline.CurrentDate(goal, Utc("2026-11-18T21:59:59Z")));
        Assert.Equal(Thursday, GoalTimeline.NextCommitment(goal, Utc("2026-11-19T05:00:00Z")));
    }

    [Fact]
    public void A_large_move_east_can_begin_the_new_version_at_once()
    {
        // 12:00 UTC on Wednesday 18 November is 01:00 Wednesday in Pago Pago and 02:00 Thursday in Kiritimati.
        var at = Utc("2026-11-18T12:00:00Z");
        var goal = Goal(OnDays(AllDays), Monday, PagoPago);
        Assert.Equal(Wednesday, GoalTimeline.CurrentDate(goal, at));
        Assert.Equal(GoalVersionChange.Add(Thursday, Kiritimati), GoalTimeline.ChangeOf(goal, at, Kiritimati));
        goal = Changed(goal, at, Kiritimati);

        // Thursday (Kiritimati) began at 10:00 UTC on Wednesday, before Wednesday (Pago Pago) began at 11:00 UTC.
        Assert.Equal(Thursday, GoalTimeline.CurrentDate(goal, at));
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, at));
        var thursday = Calendar(goal, Thursday, Thursday).Commitments.Single();
        Assert.Equal(new Commitment(Thursday, Utc("2026-11-18T10:00:00Z"), Utc("2026-11-19T10:00:00Z")), thursday);

        // Thursday ends at 10:00 UTC, while Wednesday runs until 11:00 UTC; then Friday is next.
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, Utc("2026-11-19T10:30:00Z")));
        Assert.Equal(Friday, GoalTimeline.NextCommitment(goal, Utc("2026-11-19T11:30:00Z")));
        Assert.Equal(Friday, GoalTimeline.CurrentDate(goal, Utc("2026-11-19T11:30:00Z")));

        // The Kiritimati version is in effect at once, so a second change adds a version from the next day.
        Assert.Equal(GoalVersionChange.Add(Friday, NamedTimeZone.Utc), GoalTimeline.ChangeOf(goal, at.AddMinutes(1), NamedTimeZone.Utc));
    }

    [Fact]
    public void A_goal_whose_start_date_has_not_begun_has_no_current_date()
    {
        var goal = Goal(PerDay(), Thursday, Kyiv);

        Assert.Null(GoalTimeline.CurrentDate(goal, Utc("2026-11-18T21:59:59Z")));
        Assert.Equal(Thursday, GoalTimeline.CurrentDate(goal, Utc("2026-11-18T22:00:00Z")));
        Assert.Null(GoalTimeline.CurrentDate(goal, Utc("2026-11-01T00:00:00Z")));
    }

    [Fact]
    public void None_when_the_zone_is_already_in_effect_from_the_current_date_on()
    {
        Assert.Equal(GoalVersionChange.None, GoalTimeline.ChangeOf(Goal(OnDays(MonWedFri), Monday, Kyiv), FirstChange, Kyiv));
        Assert.Equal(GoalVersionChange.None, GoalTimeline.ChangeOf(Goal(PerWeek(), FirstDecember, Kyiv), FirstChange, Kyiv));

        var changed = Changed(Goal(PerDay(), Monday, Kyiv), FirstChange, NewYork);
        Assert.Equal(GoalVersionChange.None, GoalTimeline.ChangeOf(changed, Utc("2026-11-20T17:00:00Z"), NewYork));

        var goal = Goal(PerDay(), Monday, Kyiv);
        Assert.Same(goal, GoalVersionChange.None.ApplyTo(goal));
    }

    [Fact]
    public void A_second_change_before_the_first_applies_replaces_it()
    {
        var goal = Changed(Goal(OnDays(MonWedFri), Monday, Kyiv), FirstChange, NewYork);
        var stillWednesday = Utc("2026-11-18T12:00:00Z");

        Assert.Equal(GoalVersionChange.ReplacePending(Tokyo, Thursday), GoalTimeline.ChangeOf(goal, stillWednesday, Tokyo));
        Assert.Equal([(Monday, "Europe/Kyiv"), (Thursday, "Asia/Tokyo")], VersionsOf(Changed(goal, stillWednesday, Tokyo)));
    }

    [Fact]
    public void A_change_back_while_a_version_is_pending_gives_that_version_the_old_zone()
    {
        var goal = Changed(Goal(PerDay(), Monday, Kyiv), FirstChange, NewYork);
        var stillWednesday = Utc("2026-11-18T12:00:00Z");

        Assert.Equal(GoalVersionChange.ReplacePending(Kyiv, Thursday), GoalTimeline.ChangeOf(goal, stillWednesday, Kyiv));
        Assert.Equal([(Monday, "Europe/Kyiv"), (Thursday, "Europe/Kyiv")], VersionsOf(Changed(goal, stillWednesday, Kyiv)));
    }

    public static TheoryData<string, DateOnly> PerWeekChanges => new()
    {
        { "2026-11-18T10:00:00Z", NextMonday }, // Wednesday 18
        { "2026-11-22T10:00:00Z", NextMonday }, // Sunday 22
        { "2026-11-23T10:00:00Z", Date(2026, 11, 30) }, // Monday 23
    };

    [Theory]
    [MemberData(nameof(PerWeekChanges))]
    public void For_a_per_week_goal_E_is_the_Monday_after_the_current_dates_week(string at, DateOnly appliesFrom)
    {
        Assert.Equal(GoalVersionChange.Add(appliesFrom, NewYork), GoalTimeline.ChangeOf(Goal(PerWeek(), Monday, Kyiv), Utc(at), NewYork));
    }

    [Fact]
    public void For_a_per_week_goal_before_its_first_week_E_is_its_first_Monday()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        Assert.Equal(GoalVersionChange.Add(NextMonday, NewYork), GoalTimeline.ChangeOf(goal, Utc("2026-11-19T10:00:00Z"), NewYork));
    }

    [Theory]
    [InlineData("2026-11-18T10:00:00Z")]
    [InlineData("2026-11-22T21:00:00Z")]
    public void For_commitment_based_and_per_day_goals_E_is_the_next_day(string at)
    {
        var instant = Utc(at);
        var next = Kyiv.DateAt(instant).AddDays(1);

        Assert.Equal(GoalVersionChange.Add(next, NewYork), GoalTimeline.ChangeOf(Goal(OnDays(DayOfWeek.Monday), Monday, Kyiv), instant, NewYork));
        Assert.Equal(GoalVersionChange.Add(next, NewYork), GoalTimeline.ChangeOf(Goal(PerDay(), Monday, Kyiv), instant, NewYork));
    }

    [Fact]
    public void Never_more_than_one_version_after_the_current_date_and_no_begun_day_changes_its_version()
    {
        NamedTimeZone[] zones = [NewYork, Tokyo, Kyiv, PagoPago, Kiritimati, NamedTimeZone.Utc, Zone("America/Havana")];
        Goal[] goals = [Goal(OnDays(MonWedFri), Monday, Kyiv), Goal(PerDay(), Monday, Kyiv), Goal(PerWeek(), Wednesday, Kyiv)];

        foreach (var start in goals)
        {
            var goal = start;
            var at = Utc("2026-11-16T03:00:00Z");
            for (var step = 0; step < 60; step++, at = at.AddHours(7))
            {
                var zone = zones[step % zones.Length];
                var before = goal;
                var begun = GoalTimeline.CurrentDate(before, at);
                goal = Changed(goal, at, zone);

                var current = GoalTimeline.CurrentDate(goal, at);
                Assert.True(goal.Versions.Count(v => current is null || v.AppliesFrom > current) <= 1);
                Assert.Equal(goal.Versions.Count, goal.Versions.Select(v => v.AppliesFrom).Distinct().Count());
                Assert.True(begun is null || current >= begun);
                for (var date = goal.StartDate; begun is not null && date <= begun; date = date.AddDays(1))
                {
                    Assert.Equal(GoalTimeline.VersionOf(before, date), GoalTimeline.VersionOf(goal, date));
                }
            }
        }
    }
}
