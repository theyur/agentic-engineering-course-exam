# Critic pass 1: ac for 009-goal-changes

**Verdict**: REVISE
**Date**: 2026-11-22
**Reviewed**: specs/009-goal-changes/acceptance.md (no golden samples: `tests/HT.AcceptanceTests/Samples/F009` does not exist and no item names one)

## Notes

### MAJOR-1: FR-016's trend and evaluation calendar are not covered, and the gap is not acknowledged
- **Where**: acceptance.md, Coverage table row "FR-016" (AC-023, AC-024, AC-041, AC-059, AC-067, AC-069, AC-079, AC-080, with an empty Note); the "Earlier reads" convention (lines 95-104), which defines no step for the trend or the evaluation calendar.
- **Against**: spec.md FR-016: "Reading a goal's History, recent results, trend and recent notes (slices 5 to 7) MUST cover only dates of its latest attempt" and "the evaluation calendar (slice 4 FR-013) MUST give no commitment or period outside an attempt". Both are use-case reads on the AC surface: specs/004-goals/spec.md FR-013 is the use case that reads a goal's evaluation calendar for a range of dates, and specs/007-personal-reports/spec.md FR-006 defines the trend values that the report use case returns.
- **Reasoning**: A grep of acceptance.md for "trend" and "calendar" finds nothing. AC-079 checks the History, recent results and recent notes after a restart, but no item reads the trend or the evaluation calendar of an ended goal, a restarted goal, or the dates between two attempts. Two plausible wrong implementations would pass every item: one whose trend still shows bars of an earlier attempt, and one whose calendar still lists commitments after the last date or between attempts, for example 2026-11-16 to 2026-11-22 in US3-AS4/AC-055. Where the file leaves other parts of an FR unchecked (FR-009, FR-015, FR-018, FR-026, FR-031), its Coverage notes say so and give a reason. FR-016 has no such note, so these two clauses are silently uncovered. The coverage note on US3-AS4 also checks "its next commitment is Monday 23 November" only indirectly, through refused records (AC-055, AC-069). The calendar is the read that FR-016 names for that fact.

### MINOR-1: The per-week rule for the last date is never tested on a Sunday or a Monday ending
- **Where**: AC-034, AC-038 row 02, AC-067 and AC-076. Every per-week (Yoga) ending in the file is on Wednesday 2026-11-18.
- **Against**: spec.md FR-008: "for a per-week goal the Sunday before E's week"; SC-003; the acceptance-criteria rule that boundaries must be discriminated (constitution II also asks for both sides of each time boundary).
- **Reasoning**: For a Wednesday ending (E = 2026-11-18), the correct rule gives 2026-11-15. A wrong "Sunday on or before E" gives the same date. So does a wrong "Sunday on or before E − 1". The correct rule and "Sunday on or before E" give different answers only when E is a Sunday: for E = 2026-11-22 the correct last date is 2026-11-15, and the wrong one is 2026-11-22. That wrong rule keeps the week in progress in the attempt, and the week becomes Failed or Missed after its grace period. Ending would then create a failure, which FR-008, SC-003 and SPEC §5.4 forbid, and no item would catch it. A Monday ending (E = 2026-11-23, last date 2026-11-22) is the other side of the same boundary and is also missing.

### MINOR-2: "settings unchanged" is compared with the latest version, but no item tells it apart from the version in effect
- **Where**: AC-013 and AC-016 row 02. Both use a goal with no waiting version.
- **Against**: spec.md FR-003: "settings and name equal to the goal's latest version and name give 'settings unchanged'". The convention at acceptance.md line 83 also defines "latest settings" as those of the latest settings version.
- **Reasoning**: With no waiting version, the latest version is the one in effect, so an implementation that compares with the version in effect passes AC-013. The two differ only while a later version is waiting. Example: after the change to 20 minutes (AC-001), the user submits 20 minutes again, or submits 15 minutes. No item covers either case, so the choice FR-003 makes ("latest") is not tested.

### MINOR-3: Ties in the goal history are broken in an order that matches creation order and ending time
- **Where**: AC-076.
- **Against**: spec.md FR-015 ("the latest ending date first, then by the attempt's name"); FR-026 (no time of making is stored); the rule that examples must fail for a plausible wrong ordering.
- **Reasoning**: `Meditation, 10 min daily` and `Reading, 15 min daily` share the ending date 2026-11-16. Meditation is created first and ended first (10:00 vs 11:00), and its name also sorts first. Several wrong implementations give the same order as the name rule: sorting the tie by goal identifier, by creation order or by insertion order. So AC-076 cannot tell the name tie-break apart from them.

### MINOR-4: Reading one attempt is checked only for an ended earlier attempt
- **Where**: AC-058 and AC-068. Both read attempt 1 after it ended. The Coverage row for FR-014.
- **Against**: spec.md FR-014: the History of one attempt covers its dates "to its last date, or to the current date while it is not ended". HP-001/HP-014 also cover an attempt with no date (ended before it began).
- **Reasoning**: No item reads the latest attempt while it is not ended. No item reads an attempt that ended before it began (AC-035, AC-042) through the single-attempt read either, where its History should be empty. The "or to the current date while it is not ended" branch of FR-014 is therefore untested, and the Coverage row does not say so.

### MINOR-5: The name of an earlier attempt is never checked after its habit is renamed
- **Where**: AC-042, AC-048, AC-070 and AC-075 check names of earlier and ended attempts, but only with the habit's name unchanged.
- **Against**: spec.md FR-004: an earlier attempt is named by "the habit's current name and the target text of that attempt's last settings version"; FR-015 uses the same name in the goal history.
- **Reasoning**: An implementation that stores the attempt's name when the attempt ends, and so keeps the old habit name, passes every item. The difference shows only when the habit is renamed after the ending, through the custom habit edit or the administrator's catalog edit. No item does that, and the Coverage row for FR-004 has no note.

### MINOR-6: For another user, the ownership check is not tested before "Only an ended goal can start a new attempt."
- **Where**: AC-084 rows 03 and 04. Both use user A's ended goal.
- **Against**: spec.md FR-011 and FR-012 (steps 1 to 3 before step 4); FR-022; constitution III (lookups scoped to the owner; negative tests for another user).
- **Reasoning**: Step 4 refuses only a goal that is not ended, so with A's ended goal it can never fire. An implementation that checks whether the goal is ended before checking its owner still passes AC-084. That implementation would answer user B with "Only an ended goal can start a new attempt." for A's active goal, which reveals that the goal exists. For changing and ending, the same ordering is told apart by using an ended goal, because step 4 there is "This goal has ended." / "This goal has already ended.". For the two restart use cases, no item tells it apart.

### MINOR-7: The edge case on "used by goals" is left out for a reason that is a spec gap, not an API limit
- **Where**: acceptance.md, edge case line "A habit unpublished, or a custom habit used only by ended goals, is still 'used by goals': not checked; the results ... are not restated in spec.md".
- **Against**: spec.md Edge Cases ("still 'used by goals' (slice 4 FR-033, FR-035). Ended goals keep their habit."); the AC rule that every scenario is covered or listed as not testable through the API with a reason.
- **Reasoning**: The behaviour can be reached through the API: unpublishing a habit, or deleting a custom habit, used only by an ended goal. The reason given is that spec.md does not restate those use cases' result texts. That is a gap in "Earlier behaviour these tests use", not an API limit. The risk is real. FR-020 tells the implementation to leave ended goals out of goal counts, and code that reuses that filter for "used by goals" would let a user delete a custom habit that an ended goal still uses. No item would detect it. AC-064 unpublishes "with confirmation", but asserts nothing about whether the habit counts as used.

### MINOR-8: AC-089 does not check that the demo task's results name no amount, and the Coverage note does not say so
- **Where**: AC-089, the Then step "no result contains ...". The Coverage row for FR-031.
- **Against**: spec.md FR-031 and US6-AS1: results "never ... a goal, habit, note, outcome, unit or amount".
- **Reasoning**: The list covers habit names, notes, two outcomes and the units "sweet" and "min". It has no amounts (for example 15, 20, 25, 10 or 5). The Coverage note lists the parts of FR-031 that are not checked (account name, custom habits, time zone, case-insensitive look-ups), but not the amount clause.

## Summary
1 MAJOR, 8 MINOR. The file is careful and mostly correct. I recomputed every figure, date and boundary in the items: the consistency and streak values, the grace-period instants, the 365-day and 366-entry limits (411 dates in AC-068), the next-boundary dates, the Kyiv/UTC timeline items, and every demo value for R = 2026-11-16 to 2026-11-22. All of them agree with spec.md and with slice 6's dashboard rule ("Missed results whose grace period ended this week"), which makes the Missed checks in AC-059 and AC-074 discriminating. Every FR and every user-story scenario is mapped, and the page scenarios are listed with reasons. The blocking problem is coverage: two of the reads FR-016 constrains, the trend and the evaluation calendar, are never exercised and not acknowledged. The minor notes are mostly boundaries and orderings that a plausible wrong implementation would pass, most importantly a per-week ending on a Sunday or a Monday.
