# StatusChip

A status chip shows the evaluation state of one commitment or period, always as a word plus a coloured dot.

## States (words from the product glossary)
| Class | Word on the chip | Meaning |
|---|---|---|
| `ht-status--met` | Met · Within limit (Reduce goals) | Finalized, target met |
| `ht-status--failed` | Failed | Recorded, but target not met |
| `ht-status--missed` | Missed | No record after the 48 h grace period (square dot) |
| `ht-status--pending` | Pending | Ended, still inside the grace period |
| `ht-status--excused` | Excused | Deliberately exempted (dashed border) |
| `ht-status--paused` | Paused | Goal is paused |
| `ht-status--due` | Due today · Due Mon | Upcoming or due now, nothing to judge yet |

Markup: `<span class="ht-chip ht-status ht-status--pending">Pending</span>`.

## Rules
- Never show the colour without the word. `met` and `failed` have the same lightness; the word tells them apart.
- Use exactly these words. Do not say "done", "skipped", "late" or "overdue" for these states.
- Do not use status chips for non-evaluation states (publish state, roles): use `Tag`.
