# PageGoalDetail

The goal page: everything about one attempt of one goal — stats, trend, history with corrections, settings, pauses, notes and earlier attempts.

- Page head: BackLink "← Goals", `heading-1` goal name, tags (category, intent, Commitment-based or Period-based), attempt line. Actions right: ghost Edit settings, secondary Pause, ghost End goal, primary Record.
- Left column: GoalStats (consistency + streaks + strip), TrendChart, History DataTable (date, recorded amount, StatusChip, note, action). Missed and finalized rows can be corrected ("Add record", "Edit").
- Right column: Settings (KeyValue) with "changes apply from the next commitment", Pauses, Recent notes, Earlier attempts (each with its own consistency and outcome).
- Pause and End goal open a Dialog that explains the effect; End goal asks for the outcome (Achieved, Abandoned, No longer relevant) and an optional note.
