# PageHomeVisitor

The visitor Home in layout A: hero with an example goal, How it works, catalog preview and community bands.

- Hero: `display` headline, one sentence, primary "Register free" + secondary "Browse the catalog", privacy line. Right side: a `GoalStats` card labelled "Example goal" with fixed example figures that belong to no account, rendered by the real component, not a screenshot.
- Sections in order: How it works (3 steps), Popular in the catalog (4 `HabitCard`s), Community (`BandSummary`). 40px (`space-7`) between sections.
- Built from: TopNav (visitor), GoalStats, ConsistencyStrip, HabitCard, BandSummary, Footer.

## Slice 2 state

The second part of the preview shows the visitor Home as slice 2 builds it (HP-012, US5-AS2, ADR 0008).

- TopNav (visitor) with "Catalog".
- Hero: the headline, the sentence, primary "Register free" and secondary "Browse the catalog" (→ `/catalog`), the privacy line. No GoalStats card yet (slice 6 adds it, with fixed example figures rather than demo goals: D-3).
- "How it works" as in slice 1.
- "Popular in the catalog": the first four catalog habits of the catalog in name order, as visitor HabitCards ("Suggested starting goal: …" when the habit has one, "See details"), and "See all N →" (→ `/catalog`), where N is the number of all Published catalog habits. With fewer than four Published habits it shows those; with none the whole section is absent.
- No Community section (slice 9).

## Slice 6 state

The last part of the preview shows the visitor Home as slice 6 builds it (HP-004, D-3, ADR 0008).

- Everything of slice 2 stays: TopNav (visitor) with "Catalog", the hero's headline, sentence, "Register free", "Browse the catalog" and privacy line, "How it works", and "Popular in the catalog" (absent with no Published habit, as in the preview).
- The hero gains, on its right, the GoalStats card (`components/GoalStats.md`, "Slice 6 state", visitor variant):
  - the eyebrow "Example goal" and the title "Yoga, 3 days a week" (`h2.ht-card__title`);
  - "Consistency" with "90% (18 of 20)" as one whole text (`ht-stat__value ht-stat__value--text`; no `<small>` part, tasks, Conventions), "Streak, commitments" with "6" and "Longest, commitments" with "11";
  - the strip of 21 results, oldest first: Met × 11, Missed, Met, Failed, Met × 6, Pending, with its legend (Met, Failed, Missed, Pending) and the `aria-label` "Last 21 commitments: 18 met, 1 failed, 1 missed, 1 pending".
- The figures are fixed examples that belong to no account: the card reads no stored data, so the page is the same for every visitor and shows no account's data (D-3). The texts come from the shared formatter applied to these fixed results (HP-004, HP-005), so the card follows the rules of consistency and streaks.
- The card is part of the hero, not of the catalog preview, so it shows with or without Published habits (amended slice 2 US5-AS8).
- No Community section (slice 9).
