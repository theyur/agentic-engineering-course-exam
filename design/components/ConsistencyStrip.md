# ConsistencyStrip

A grid of small squares, one per commitment or period, oldest first, showing how each was evaluated.

| Cell class | Shape | Meaning |
|---|---|---|
| `ht-cell--met` | solid `met` | Met |
| `ht-cell--failed` | solid `failed` | Failed |
| `ht-cell--missed` | hatched `missed` with border | Missed |
| `ht-cell--excused` | dashed outline | Excused |
| `ht-cell--paused` | light `paused` fill | Paused |
| `ht-cell--pending` | dotted outline on `pending-soft` | Pending |

- 15 columns; 30 cells = last 30 commitments or periods. Always show the legend under it.
- Give the grid `role="img"` and an `aria-label` that counts each state.
- Each cell can have a `title` with the date and the recorded amount.
