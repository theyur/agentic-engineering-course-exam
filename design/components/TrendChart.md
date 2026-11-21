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
