# StatusChip

A status chip shows the evaluation state of one commitment or period, always as a word plus a coloured dot.

## States (words from the product glossary)
| Class | Word on the chip | Meaning |
|---|---|---|
| `ht-status--met` | Met (Build and Reduce goals alike) | Finalized, target met |
| `ht-status--failed` | Failed | Recorded, but target not met |
| `ht-status--missed` | Missed | No record after the 48 h grace period (square dot) |
| `ht-status--pending` | Pending | Ended, still inside the grace period |
| `ht-status--excused` | Excused | Deliberately exempted (dashed border) |
| `ht-status--paused` | Paused | Goal is paused |
| `ht-status--due` | Due today · Due this week · Upcoming | Due now (today: a commitment or a day; this week: a week) or upcoming (Upcoming; its date is in the row's meta line), nothing to judge yet |

Markup: `<span class="ht-chip ht-status ht-status--pending">Pending</span>`.

## Rules
- Never show the colour without the word. `met` and `failed` have the same lightness; the word tells them apart.
- Use exactly these words. Do not say "done", "skipped", "late" or "overdue" for these states. A Reduce goal's Met is "Met" too (constitution I).
- Do not use status chips for non-evaluation states (publish state, roles): use `Tag`.

## Slice 5 state

The last part of the preview shows the chips slice 5 uses (HP-004, ADR 0008), on the goal page's History and Recent notes cards:

| State (FR-010) | Text | Classes |
|---|---|---|
| Met | Met | `ht-chip ht-status ht-status--met` |
| Failed | Failed | `ht-chip ht-status ht-status--failed` |
| Missed | Missed | `ht-chip ht-status ht-status--missed` |
| Pending | Pending | `ht-chip ht-status ht-status--pending` |
| Due, a commitment or a per-day period | Due today | `ht-chip ht-status ht-status--due` |
| Due, a per-week period | Due this week | `ht-chip ht-status ht-status--due` |

- The text is always shown with the colour, so colour is never the only signal.
- Upcoming is not shown in slice 5: the goal page lists only what has begun. The chip stays in the full design for a later slice that shows Upcoming.
- Excused and Paused come with slice 7.
- No new class: one Razor component `StatusChip` renders these classes and texts.

## Slice 6 state

The last part of the preview shows the chips slice 6 uses (HP-002, HP-003, constitution I, ADR 0008): on the goal page's History and Recent notes cards, the Goals page's Pending chip and the Home's Today rows.

| State (FR-010) | Text | Classes |
|---|---|---|
| Met | Met | `ht-chip ht-status ht-status--met` |
| Failed | Failed | `ht-chip ht-status ht-status--failed` |
| Missed | Missed | `ht-chip ht-status ht-status--missed` |
| Pending | Pending | `ht-chip ht-status ht-status--pending` |
| Due, a commitment or a per-day period | Due today | `ht-chip ht-status ht-status--due` |
| Due, a per-week period | Due this week | `ht-chip ht-status ht-status--due` |
| Upcoming | Upcoming | `ht-chip ht-status ht-status--due` |

- The upcoming chip reads "Upcoming", never "Due Mon": the date belongs to the row's meta line ("Wed 18 Nov", "23 – 29 Nov"; `components/CommitmentRow.md`, "Slice 6 state"). The word tells it apart from Due, which shares its colour, so no state relies on colour alone.
- Only the Home's Today list shows Upcoming; the goal page and the Goals page list only what has begun.
- Excused and Paused come with slice 7.
- The word comes from `ReportText.State` ("Upcoming" for Upcoming, slice 5's word for every other state). No new class.
