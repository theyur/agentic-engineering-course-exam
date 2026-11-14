# PageHomeVisitor

The visitor Home in layout A: hero with an example goal, How it works, catalog preview and community bands.

- Hero: `display` headline, one sentence, primary "Register free" + secondary "Browse the catalog", privacy line. Right side: a real `GoalStats` card from demo data, not a screenshot.
- Sections in order: How it works (3 steps), Popular in the catalog (4 `HabitCard`s), Community (`BandSummary`). 40px (`space-7`) between sections.
- Built from: TopNav (visitor), GoalStats, ConsistencyStrip, HabitCard, BandSummary, Footer.

## Slice 2 state

The second part of the preview shows the visitor Home as slice 2 builds it (HP-012, US5-AS2, ADR 0008).

- TopNav (visitor) with "Catalog".
- Hero: the headline, the sentence, primary "Register free" and secondary "Browse the catalog" (→ `/catalog`), the privacy line. No GoalStats card yet (it needs demo goals, slice 6).
- "How it works" as in slice 1.
- "Popular in the catalog": the first four catalog habits of the catalog in name order, as visitor HabitCards ("Suggested starting goal: …" when the habit has one, "See details"), and "See all N →" (→ `/catalog`), where N is the number of all Published catalog habits. With fewer than four Published habits it shows those; with none the whole section is absent.
- No Community section (slice 9).
