# PageAttemptDetail

The attempt view: one earlier attempt of a goal, with its own figures, strip and correctable History (FR-014). New page, designed in slice 8 from `PageGoalDetail` (HP-014): the goal page's head, GoalStats and History only.

- Opened from "Attempt n · …" in the goal page's Earlier attempts card (`pages/PageGoalDetail.md`, "Slice 8 state") and from the Goal column of the Profile's Goal history (`pages/PageProfile.md`, "Slice 8 state").
- It shows an attempt that is not the goal's latest. The latest attempt is the goal page itself.

## Slice 8 state

The preview shows the page as slice 8 builds it (HP-014, US3-AS11, US3-AS15, contracts/host.md "Attempt view", ADR 0008); the full preview and the "Slice 8 state" block show the same page. The full preview: attempt 1 of "Meditation, 5 min daily" (ended Mon 16 Nov with "Abandoned" and the note "Too busy this month.", restarted with 5 min from Mon 23 Nov, spec US3-AS4), read on Mon 23 Nov 2026, 12:00.

- Address: `/goals/{id}/attempts/{n}`, for registered users. The latest attempt's number, an unknown number, another account's goal or an unknown goal shows the Not found page. An administrator gets the Not found page.
- BackLink "← {goal name}" to the goal page, with the goal's current name ("← Meditation, 5 min daily").
- Head (`ht-page-head ht-detail-head`):
  - `heading-1`: "Attempt 1 · 2 Nov – 15 Nov" (the attempt's start date to its last date, `ReportText`), or "Attempt 1 · ended before it began" for an attempt with no date (its last date is before its first commitment or period; plan "Spec discrepancies" 3);
  - under it, one `ht-inline ht-head-tags` line: the attempt's name (FR-004: as it was at its last date, "Meditation, 10 min daily"; `span.ht-attempt`), then the outcome Tag (`ht-chip`, "Abandoned", always with its word). The name is always shown here, also when it equals the goal's name (settled here, T006: HP-014 lists it without a condition);
  - then the ending note as plain text in `ht-muted` (a `div.ht-muted.ht-head-tags`), only when it has one.

  No action in the head: no "Record", "Edit settings", "Pause" or "End goal".
- One column (`ht-stack`), because no right-column card remains (settled here, T006):
  1. the GoalStats card (`components/GoalStats.md`, "Slice 7 state") with the eyebrow "Attempt 1" (in place of "This attempt"), the title "Consistency and streaks", "Consistency" ("85% (12 of 14)"), "Streak, commitments" ("0") and "Longest, commitments" ("12") in the attempt's own unit, then the strip of the attempt's last 30 ended results, oldest first ("Last 14 commitments: 12 met, 2 missed"), with the six-entry legend. The figures cover the attempt from its start date to its last date, as computed at the read's time;
  2. the History card of the goal page (`pages/PageGoalDetail.md`, "Slice 5 state" and "Slice 7 state") over the attempt's dates, newest first, the first 10, then "Show all 14 →": the same cells, StatusChips, row actions and accessible names. "Add record", "Edit", "Add note", "Edit note" and a period's record buttons open the RecordDialog or the NoteDialog fixed to their row, with the attempt's own settings for that row (never the latest attempt's): "Add record" on Sat 14 Nov opens the RecordDialog for that date (US3-AS15). An Excused row keeps "Remove excusal" while its grace period is open (FR-010).
- An attempt with no date: the heading "Attempt 1 · ended before it began", the name, the Tag and the note when it has one; the GoalStats card with "N/A", "0" and "0" and an empty strip ("Last 0 commitments") with its legend; no History card, as its History is empty and the History card is absent without rows (slice 5; settled here, T006).
- No TrendChart, Settings, Pauses, Recent notes or Earlier attempts card (HP-014).
- After a record, a change, a delete, a note or an excusal is saved in a dialog or on a row, the page reads the attempt again, so the History, the figures and the strip agree. Focus returns as on the goal page (`components/RecordDialog.md`, `components/NoteDialog.md`).
- Every text comes from `ReportText`, `GoalChangeTexts` and the shared texts (HP-008); the page builds none.
