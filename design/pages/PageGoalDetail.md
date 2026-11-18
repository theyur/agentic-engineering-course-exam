# PageGoalDetail

The goal page: everything about one attempt of one goal — stats, trend, history with corrections, settings, pauses, notes and earlier attempts.

- Page head: BackLink "← Goals", `heading-1` goal name, tags (category, Build or Reduce, Commitment-based or Period-based), attempt line. Actions right: ghost Edit settings, secondary Pause, ghost End goal, primary Record.
- Left column: GoalStats (consistency + streaks + strip), TrendChart, History DataTable (date, recorded amount, StatusChip, note, action). Missed and finalized rows can be corrected ("Add record", "Edit").
- Right column: Settings (KeyValue) with "changes apply from the next commitment", Pauses, Recent notes, Earlier attempts (each with its own consistency and outcome).
- Pause and End goal open a Dialog that explains the effect; End goal asks for the outcome (Achieved, Abandoned, No longer relevant) and an optional note.

## Slice 4 state

The second part of the preview shows the page as slice 4 builds it (HP-004, ADR 0008). Today is Tue 17 Nov 2026 in the preview.

- `/goals/{id}` for the goal's owner; another account's goal, an unknown goal and any goal address for an administrator show the Not found page.
- "← Goals", the goal's name as `heading-1`, then the tags: category, Build or Reduce, and "Commitment-based" or "Period-based", and the attempt line: "Attempt 1 · started 14 Jul" once the goal has started, "Attempt 1 · starts 25 Nov" before.
- A goal on a catalog habit that is no longer in the catalog: the neutral Notice "This habit is no longer in the public catalog. Your goal keeps working." under the page head. The goal list shows nothing extra.
- The Settings card (KeyValue), in this order:
  - Habit: the habit's current name, " · " and "catalog" or "custom" ("Yoga · catalog");
  - Build or reduce: "Build" or "Reduce";
  - Measure: "Yes / no", "Count" or "Duration";
  - Target: the goal's target text ("3 days a week", "15 min, 5 days a week", "at most 2 cups a day", "avoided every day");
  - Schedule: the schedule as on the Goals page ("Mon · Wed · Fri", "Every day", "Flexible · 3 times a week");
  - Evaluated: "Each scheduled day", "Each day" or "Each week", as on the New goal summary card;
  - First week, for a per-week goal only: the date its first week starts ("30 Nov 2026");
  - Started: the start date ("14 Jul 2026"); the row is "Starts" while the goal has not started.
- Under the list: "Changes apply from the next commitment. Past results keep their rules."
- No actions, GoalStats, trend, history, pauses, notes or earlier attempts (slices 5 to 8).
- The full preview's texts follow slice 4 too: the "Build or reduce" row (not "Intent"), the Target row, "Each scheduled day" (not "Each commitment") and the streak unit "commitments" for a commitment-based goal (not "sessions").
