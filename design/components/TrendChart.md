# TrendChart

A simple bar chart of recorded amounts per day or week, with the target as a dashed line.

- Bars are `accent`; paused periods are dashed empty bars; the target line is `ink-muted`, labelled "Target 3".
- Bars show amounts, not verdicts: do not colour bars by status (the ConsistencyStrip does that).
- Use weeks for weekly and commitment-based goals, days for daily count/duration goals; up to 12–14 bars.
- Give the chart `role="img"` and an `aria-label`; offer the History table for exact values.
- Pure CSS (height in % per bar). Replace with a chart library only if interaction is needed.
