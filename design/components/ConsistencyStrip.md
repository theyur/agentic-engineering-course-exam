# ConsistencyStrip

A grid of small squares, one per commitment or period, oldest first, showing how each was evaluated.

| Cell class | Shape | Meaning |
|---|---|---|
| `ht-cell--met` | solid `met` | Met |
| `ht-cell--failed` | solid `failed` with a light diagonal slash | Failed |
| `ht-cell--missed` | hatched `missed` with border | Missed |
| `ht-cell--excused` | dashed outline | Excused |
| `ht-cell--paused` | light `paused` fill | Paused |
| `ht-cell--pending` | dotted outline on `pending-soft` | Pending |

- Every state must differ by shape, not only colour (Met and Failed have near-identical luminance).
- 15 columns; 30 cells = last 30 commitments or periods. Always show the legend under it.
- Give the grid `role="img"` and an `aria-label` that counts each state.
- Each cell can have a `title` with the date and the recorded amount.

## Slice 6 state

The last part of the preview shows the strip as slice 6 builds it (HP-001, HP-002, HP-006, ADR 0008). One Razor component, `ConsistencyStrip`, renders both sizes.

- Cells and legend entries: Met (`ht-cell--met`), Failed (`ht-cell--failed`), Missed (`ht-cell--missed`) and Pending (`ht-cell--pending`) only, each with its shape. Excused and Paused come with slice 7.
- The cells are the goal's latest ended results (Met, Failed, Missed or Pending; a Due or Upcoming commitment or period has no cell), oldest first: the last 30 on the goal page and the visitor Home, the last 15 on the Goals page. A goal with fewer ended results has fewer cells; with none it has no cell.
- Full strip (`ht-strip`): 15 columns, then the legend (`ht-legend`) with the four entries Met, Failed, Missed, Pending, in that order.
- Mini strip (`ht-strip ht-strip--mini`): 15 columns, the last 15 results, no legend.
- Both have `role="img"` and an `aria-label` that counts each state: "Last {n} {unit}: {a} met, {b} failed, {c} missed, {d} pending".
  - {n} is the number of cells and {unit} the goal's streak unit, singular for exactly 1: "Last 28 commitments", "Last 1 day", "Last 11 weeks".
  - The states come in the order met, failed, missed, pending; a state with 0 is left out ("Last 15 commitments: 12 met, 1 missed, 2 pending").
  - With no cell, the label is "Last 0 {unit}" with nothing after it ("Last 0 commitments").
  - The mini strip's label replaces the full preview's "Last 15 results".
- No cell has a `title` in this slice: the History card gives each date with its state and recorded value.
- The label comes from `ReportText.StripLabel` (HP-005).

## Slice 7 state

The last part of the preview shows the strip as slice 7 builds it (HP-001, HP-004, HP-006, US3-AS8, ADR 0008). One Razor component, `ConsistencyStrip`, still renders both sizes.

- Cells and legend entries: slice 6's four plus Excused and Paused, each with its shape from the table above, so no state is shown by colour alone:
  - Excused (`ht-cell--excused`): a dashed outline with no fill;
  - Paused (`ht-cell--paused`): a light `paused` fill with no outline, unlike Met's dark solid fill and Pending's dotted outline.
- The cells are the goal's latest ended results, oldest first, now including the exempt results: an Excused or Paused commitment or period whose end is at or before now (FR-015). An Excused or Paused entry that has not ended (today's, or the current week) has no cell, like a Due one. The last 30 on the goal page and the visitor Home, the last 15 on the Goals page, as in slice 6.
- Full strip (`ht-strip`): the legend (`ht-legend`) always lists all six entries, Met, Failed, Missed, Excused, Paused, Pending, in that order, also when the strip has no Excused or Paused cell: it is a key, not a count.
- Mini strip (`ht-strip ht-strip--mini`): unchanged, no legend.
- The `aria-label` counts each state in the order met, failed, missed, excused, paused, pending: "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending" (spec US3-AS3). A state with 0 is left out, as in slice 6, so a goal without exemptions keeps slice 6's label.
- The label comes from `ReportText.StripLabel` (HP-006, HP-008).
- The previews use spec US3's "Meditation, 10 min daily" read on Thu 19 Nov 2026: Met × 7, Excused, Met, Missed, Met, Paused × 3, Met, Pending × 2 (the full strip), and its last 15 (the mini strip).
