# TrendChart

A simple bar chart of recorded progress per commitment, day or week, with the target as a dashed line.

- Bars are `accent`; paused periods are dashed empty bars; the target line is `ink-muted`, labelled "Target 3".
- Bars show recorded progress, an amount or a number of records, not verdicts: do not colour bars by status (the ConsistencyStrip does that).
- Use weeks for per-week goals and commitment-based yes/no goals; commitments or days otherwise; up to 12–14 bars.
- Give the chart `role="img"` and an `aria-label`; offer the History table for exact values.
- Pure CSS (height in % per bar). Replace with a chart library only if interaction is needed.

## Slice 6 state

The last part of the preview shows one card per bar kind as slice 6 builds it (FR-006, HP-001, HP-006, ADR 0008), read on Wed 18 Nov 2026. One Razor component, `TrendChart`, renders every kind; the goal page shows it under the GoalStats card (`pages/PageGoalDetail.md`, "Slice 6 state").

### Bars

| Goal | One bar per | Last | Value | Target |
|---|---|---|---|---|
| counted per week | week (its period) | 12 | the week's progress: "Yes" records, or records that reach the amount (Build count or duration); the amount recorded (Reduce count or duration) | N (Build, Reduce yes/no), or the amount (Reduce count or duration) |
| counted per day | day | 14 | as per week, for the day | as per week |
| commitment-based, yes/no | Monday-to-Sunday week with at least one begun commitment | 12 | the week's commitments recorded "Yes" | Build: the week's commitments on or after the start date, Upcoming ones included; Reduce: 0 |
| commitment-based, count or duration | commitment | 14 | its recorded amount | the goal's amount |

- Amounts are a count or minutes; a goal in hours gives minutes (the amount times 60), the values and the target alike.
- Only bars that have begun are drawn, oldest first. A week of a commitment-based yes/no goal begins with its first commitment.
- A bar with no record has no value: it is drawn as `ht-bar ht-bar--empty`, a full-height dotted outline, never as a zero-height bar. A bar with records has its value, 0 included (a 2px bar, the `ht-bar` minimum).
- Values above the target are drawn as recorded. A bar's height is its value's share of the largest of the values and the target, rounded to a whole percentage (`style="height:67%"`); the target line's `bottom` is the target's share of the same.
- The value counts the records of every state, Due and Pending included.
- No paused bars until slice 7. Slice 7's paused bar (`ht-bar--paused`, dashed) and the empty bar stay apart in their `title` and in the chart's `aria-label`.

### Title and sub line

The card's title (`ht-card__title`) and sub line (`ht-card__sub`) per goal. `ReportText.TrendTitle` and `TrendSubLine` give exactly this table (HP-005).

| Goal | Measure, Build or Reduce | Bar value | Title | Sub line |
|---|---|---|---|---|
| commitment-based, any days | yes/no, Build or Reduce | records | Days done per week | Last 12 weeks |
| commitment-based, all seven days | count, Build or Reduce | amount | {Count unit} per day ("Pages per day") | Last 14 days |
| commitment-based, fewer than seven days | count, Build or Reduce | amount | {Count unit} per day | Last 14 commitments |
| commitment-based, all seven days | duration (minutes or hours), Build or Reduce | amount | Minutes per day | Last 14 days |
| commitment-based, fewer than seven days | duration (minutes or hours), Build or Reduce | amount | Minutes per day | Last 14 commitments |
| counted per day | yes/no, Build or Reduce | records | Times per day | Last 14 days |
| counted per day | count or duration, Build | records | Times per day | Last 14 days |
| counted per day | count, Reduce | amount | {Count unit} per day ("Sweets per day") | Last 14 days |
| counted per day | duration (minutes or hours), Reduce | amount | Minutes per day | Last 14 days |
| counted per week | yes/no, Build or Reduce | records | Times per week | Last 12 weeks |
| counted per week | count or duration, Build | records | Times per week | Last 12 weeks |
| counted per week | count, Reduce | amount | {Count unit} per week | Last 12 weeks |
| counted per week | duration (minutes or hours), Reduce | amount | Minutes per week | Last 12 weeks |

- {Count unit} is the goal's count unit as stored, never inflected, with its first letter upper-case: "pages" gives "Pages per day", "sweet" gives "Sweet per day".
- "All seven days" is a commitment-based goal on Monday to Sunday ("Every day"); any other set of days is "fewer than seven days".
- The sub line names the most bars the card can show (14 or 12), also while fewer have begun.
- The goal settings version of the latest bar decides the row.

### Target line, tooltips and accessible name

- The dashed line `ht-bars__target` is drawn at the latest bar's target, labelled "Target {n} min" when the bar values are minutes and "Target {n}" otherwise (counts and numbers of records): "Target 20 min", "Target 60 min" for a goal of 1 h, "Target 1" for "Sugar, at most 1 sweet a day", "Target 3" for "Cycling, 30 min, 3 times a week" (its bars count rides of at least 30 minutes). `ReportText.TargetLine` gives it.
- Each bar keeps its own target. Only a commitment-based yes/no Build goal's weeks can differ in this slice (its first week can have fewer commitments); every other goal's bars share one target.
- Each bar's `title` is "{date}: {value} of {target}", with " min" after the target for minutes: "Wed 4 Nov: 20 of 20 min", "week of 2 Nov: 2 of 2"; a bar with no record is "{date}: no record". {date} is "Wed 4 Nov" for a commitment or a day and "week of 2 Nov" (its Monday) for a week.
- The date labels under the bars (`ht-bars__labels`) are the day and month, "4 Nov" (for a week, its Monday).
- The chart (`ht-bars`) has `role="img"` and the `aria-label` "{title}, {sub line, first letter lower-case}; target {n}" ("Minutes per day, last 14 days; target 20"), then, for each bar whose own target differs from the line's, "; {date}: {value} of {target}" ("Days done per week, last 12 weeks; target 3; week of 2 Nov: 2 of 2"), or "; {date}: no record of {target}" for such a bar with no record. `ReportText.TrendLabel` gives it.
- The History card under the chart gives every exact value.

## Slice 7 state

The last part of the preview shows the chart as slice 7 builds it (FR-016, HP-001, HP-006, research R-07, ADR 0008). Everything of slice 6 stays: the bar kinds, titles, sub lines, heights, date labels and the empty bar. Slice 7 adds marked bars.

### Marked bars

| Bar | Marked when | Drawn as | `title` |
|---|---|---|---|
| a commitment or a day (per-day goals, commitment-based count and duration goals) | it is Paused, or (a commitment) Excused | `ht-bar ht-bar--paused` or `ht-bar ht-bar--excused` | "{date}: Paused", "{date}: Excused" ("Mon 9 Nov: Excused") |
| a week of a per-week goal | it is Paused (any of its dates is in a pause, D-1) | `ht-bar ht-bar--paused` | "week of 21 Sep: Paused" |
| a week of a commitment-based yes/no goal | every one of its commitments, the current week's Upcoming ones included, is Excused or Paused | `ht-bar--paused` when one of them is Paused, else `ht-bar--excused` | "week of 21 Sep: Paused", "week of 21 Sep: Excused" |

- A marked bar has no value, also when a record is kept on its commitment or day (FR-009): it is drawn full height with a dashed outline and no `style` height, like the empty bar.
- The paused bar (`ht-bar--paused`) keeps its existing dashed `line-strong` outline. The Excused bar (`ht-bar--excused`, new in `css/ht-components.css`, tokens only) is the same dashed outline in `excused`, the colour of the Excused StatusChip and strip cell. The two are told apart in their `title` and in the chart's `aria-label`, never by colour alone; the empty bar has a dotted outline and the `title` "{date}: no record".
- A commitment-based yes/no week that is not all exempt keeps an unmarked bar: its Build target counts only its commitments that are neither Excused nor Paused (Upcoming ones of the current week included), and its value counts only the "Yes" records of those commitments. A "Yes" kept on a Paused commitment is not counted, and a week whose only records sit on Paused commitments has no value (the empty bar), not 0. A Reduce week's target stays 0. In the preview, the week of 26 Oct (one Excused commitment of three) is "2 of 2" and the week of 9 Nov (one Excused and one Paused) is "1 of 1".
- A marked bar keeps its own target by the same rules (0 for an all-exempt yes/no week); it is not drawn.

### Target line and accessible name

- The dashed target line is drawn at the target of the latest bar that is not marked ("Target 1" in the third example, whose latest bar is Paused), or at 0 when every bar is marked or there is none. A goal without exemptions keeps slice 6's line.
- The chart's `aria-label` is slice 6's, with the marked bars counted after the target: "{title}, {sub line}; target {n}; {a} excused, {b} paused", then slice 6's list of bars whose own target differs ("Days done per week, last 12 weeks; target 3; 2 paused; week of 26 Oct: 2 of 2; week of 9 Nov: 1 of 1"). A count of 0 is left out ("Minutes per day, last 14 days; target 10; 1 excused, 3 paused", "Times per day, last 14 days; target 1; 1 paused"); with no marked bar nothing is added, so slice 6's label is unchanged. Bars with no record are not counted, so they stay apart from marked bars. Marked bars are never listed among the bars whose own target differs. Settled here in slice 7 (T010): the spec and the contract ask that the label count marked bars apart from empty ones without fixing the words.
- `ReportText` gives the bar titles and the label (HP-008); the chart computes nothing.
- The previews: Meditation, 10 min daily (spec US3-AS4: 9 November Excused, 13 to 15 November Paused, 11 and 17 to 19 November with no record); a commitment-based yes/no goal with two all-exempt Paused weeks and two weeks with exempt commitments; and Drinking water, 8 glasses a day, paused from today with a record kept on that day (spec US2-AS10), whose line stays at the latest unmarked bar's target.
