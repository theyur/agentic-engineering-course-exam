# GoalStats

The goal summary card: consistency, current streak and longest streak, with the history strip below.

## Rules from the product spec
- Consistency is always shown with its count: **90% (18 of 20)**. With no eligible finalized evaluations show **N/A**.
- Streaks always show their unit: commitments, days or weeks ("6 commitments", "3 weeks").
- Pending, excused and paused evaluations are not counted; say so in help text where people ask.
- Each goal has its own stats; never combine goals into one habit score.

Markup: `ht-stats` with three `ht-stat` boxes (`ht-stat__value`, `ht-stat__label`), then `ConsistencyStrip`.

## Slice 6 state

The last part of the preview shows the card as slice 6 builds it (HP-001, HP-004, HP-005, HP-006, ADR 0008). One Razor component, `GoalStats`, renders all of its variants.

- Goal page variant: the eyebrow "This attempt", the title "Consistency and streaks" (`ht-card__title`, an `h2` on the pages; the preview shows the card alone with an `h3`), then `ht-stats` with three `ht-stat` boxes, then the ConsistencyStrip of the last 30 ended results (`components/ConsistencyStrip.md`, "Slice 6 state") and its legend.
- Visitor Home variant: the eyebrow "Example goal" and the goal's title "Yoga, 3 days a week" in place of the eyebrow and title above; the rest is the same component with fixed example figures that belong to no account (`pages/PageHomeVisitor.md`, "Slice 6 state").
- The three stats, in this order:

  | Label (`ht-stat__label`) | Value (`ht-stat__value`) |
  |---|---|
  | Consistency | the consistency as one whole text: "92% (24 of 26)", or "N/A" with no finalized result (FR-002) |
  | Streak, {unit} | the current streak as a number: "7" |
  | Longest, {unit} | the longest streak as a number: "9" |

  {unit} is the goal's streak unit in its plural form: "commitments" (commitment-based), "days" (counted per day) or "weeks" (counted per week), also when the number is 1 or 0. The label carries the unit, so the value is the number alone.
- The consistency value is one whole text in `ht-stat__value ht-stat__value--text` ("92% (24 of 26)", "N/A"), with no `<small>` part: it is one `ReportText` text, and no component splits it (tasks, Conventions). `ht-stat__value--text` sets it at 20px and widens the first box (`ht-stats` becomes 2fr 1fr 1fr), so the whole text stays on one line in a 460px card. The full preview above keeps its split value for the slices that come later.
- With no finalized result: "N/A", and both streaks "0".
- No Excused or Paused cell or legend entry (slice 7). The strip's `aria-label` counts each state shown ("Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending").
- All texts come from `ReportText` (HP-005); the card computes nothing.

## Slice 7 state

The last part of the preview shows the card as slice 7 builds it (HP-001, HP-004, HP-006, US3-AS8, ADR 0008). The one `GoalStats` component still renders every variant.

- Everything of slice 6 stays: the eyebrow, the title, the three stats and their texts, and the strip of the last 30 ended results.
- The strip (`components/ConsistencyStrip.md`, "Slice 7 state") shows Excused (`ht-cell--excused`) and Paused (`ht-cell--paused`) cells with their shapes, and its legend always lists all six entries, Met, Failed, Missed, Excused, Paused, Pending, in that order (a key, not a count). Its `aria-label` counts Excused and Paused too: "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending".
- The figures leave Excused and Paused results out (FR-013, FR-014): spec US3's goal reads "90% (10 of 11)", streak 2 and longest 8 commitments; a goal whose ended results are all exempt reads "N/A", 0 and 0, with a strip of only Excused and Paused cells ("Last 7 commitments: 1 excused, 6 paused").
- Visitor Home variant: the same component, so its legend lists the same six entries; its 21 fixed results, figures and `aria-label` are slice 6's, unchanged (`pages/PageHomeVisitor.md`, "Slice 7 state").
- All texts still come from `ReportText` (HP-008); the card computes nothing.
