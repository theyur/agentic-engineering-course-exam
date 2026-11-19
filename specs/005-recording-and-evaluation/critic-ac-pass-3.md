# Critic pass 3: ac for Recording and evaluation

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: specs/005-recording-and-evaluation/acceptance.md (no golden samples: tests/HT.AcceptanceTests/Samples/F005 does not exist and no item names one)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MINOR-1 Reduce duration per-week never evaluated; SC-002 claim overstated | Resolved | AC-066 rows 28-30 (lines 1233-1235) add Reduce duration 60 per week with totals 59 (Met), 60 (Met) and 61 (Failed). Rows 20-27 add the missing sides: Build count per-day above (6, 6), Build duration per-day above (21), Build duration per-week above (21 ×3), and below-limit rows for Reduce duration fixed-days (29), Reduce duration per-day (29), Reduce count per-week (2), Reduce yes/no per-week (1 Yes) and Reduce yes/no per-day N = 2 (1 Yes). AC-020 row 07 (line 517) adds 3 Yes for Build yes/no per-day N = 2. I recounted all 18 combinations. Each now has a final result at, inside and past the target, except the two yes/no commitment kinds, and the SC paragraph (lines 192-194) states that exception. Every row recomputed: the per-week grace end is `2026-11-25T00:00:00Z`, so each entry is final at the read time, and every total, count and state is right. A related gap in how Build periods are discriminated is new, see MINOR-1. |
| MINOR-2 Change-record value check not discriminated from the note check | Resolved | AC-064 row 11 (line 1182) changes to -1 with a 501-character note, expects "Enter a whole number from 0 to 9999." and leaves the record and "First note." unchanged. |
| MINOR-3 AC-065 depends on slice-4 unpublishing behaviour the AC sources don't give | Partly | The Goals convention (lines 40-42) now defines "on the catalog habit `H`" by citing spec "Slice 4 behaviour", Goals, so the phrasing gap is closed. The unpublish step (line 1189) dropped "confirming" and only says the habit "so its state is Unpublished". The rule that a habit with goals needs confirmation is still not restated in spec.md. Carried forward as MINOR-2. |
| MINOR-4 Partial-week overlap of a history range not stated | Resolved | The new Ranges convention (lines 81-83) takes the question out of scope. Every item whose result depends on a per-week entry now reads Monday-to-Sunday ranges, or ranges whose partial part belongs to no week of the goal: AC-044 (line 878), AC-066 (line 1199), AC-054 (10-21..11-22, where 10-21..10-25 is before the first week), AC-055, and AC-068 (new "last date" column: 10-25..11-22 and 10-27..11-29). The convention's general claim is not accurate for two items whose results don't depend on it, see MINOR-4. |
| MINOR-5 Coverage claims and table don't match the items | Resolved | A mechanical cross-check of every AC header tag against the Coverage rows shows no difference (FR-009 now lists AC-034, FR-011 lists AC-067, FR-014 lists AC-003/AC-004 and not AC-065, FR-010 lists AC-065). The SC-005 sentence (lines 195-198) now limits AC-055 and AC-068 to the Yoga goal's weeks for R on a Monday, Sunday and Tuesday. |
| MINOR-6 Undefined notation for padded notes in AC-038 rows 05/06 | Resolved | The Texts convention (lines 78-80) defines "a padded text of 500 characters" as one 504-character text that trims to 500. AC-038 rows 05/06 (lines 791-792) use that phrase. |

Older notes have not regressed. Pass-1 MAJOR-1 (AC-064) and pass-1 MINOR-2 to MINOR-8 still hold as pass 2 found them. Pass-1 MINOR-1, previously closed as "rejection accepted", is now covered by AC-069, after the spec.md change ("Passwords" bullet at spec lines 129-131, FR-030 lines 541-544). I recomputed every row changed or added in this iteration and found no error:
- AC-020 row 07 and AC-038 rows 05/06.
- AC-044 at `2026-11-18T10:00:00Z`: every entry containing 2026-11-18 is Due.
- AC-054: R = Wednesday, start 2026-10-21, weeks 10-26 Missed, 11-02 Failed, 11-09 Met (grace ended `2026-11-18T00:00:00Z`), 11-16 Due.
- AC-055: R = Monday, start 10-26, five weeks, week 1 Pending until `2026-11-25T00:00:00Z`.
- AC-064 row 11.
- AC-065: setup at `2026-11-16T08:00:00Z`. At `2026-11-18T10:00:00Z`, 11-16 is Pending until `2026-11-19T00:00:00Z`, 11-17 Pending until `2026-11-20T00:00:00Z`, 11-18 Due.
- AC-066 rows 01-30.
- AC-068 rows 01/02: R = Sunday 11-22 gives start 10-25 and 4 weeks. R = Tuesday 11-24 gives start 10-27, first full week 11-02, and 4 weeks.
- AC-069: `exampl1` is 7 characters, `examples` 8.

## Notes

### MINOR-1: The Build period rule "N records that each reach the amount" is never told apart from "the total reaches N × the amount"
- **Where**: AC-018 rows 01-04 (lines 477-480), AC-066 rows 04, 05, 20 (Build count, 2 times per day), 06, 07, 21 (Build duration, 1 time per day) and 08, 09, 22 (Build duration, 3 times per week) (lines 1209-1214, 1225-1227). The SC-002 paragraph (lines 192-194).
- **Against**: spec.md "What the target means" (lines 122-123: "Build is met when the period has at least N records that each reach the amount") and US2-AS8 (lines 230-231). The review rule says boundaries must fail for a plausible wrong implementation.
- **Continues**: new
- **Reasoning**: I checked every final Build count and Build duration period row against the rule "Met when the sum of the records is at least N × amount". It gives the same state in every row:
  - AC-018: totals 57, 87, 57 and 60 against 60.
  - AC-066: 10, 9 and 12 against 10; 19, 20 and 21 against 20; 59, 60 and 63 against 60.

  No row has one large record that makes up the total while fewer than N records reach the amount (for example 60, 0, 0 for "20 pages, 3 times a week"). So an implementation that sums the period, which is a natural reading of "60 pages a week", gets every state right. The progress values (count of reaching records) catch it only if progress and state are computed from the same rule. The yes/no and Reduce paths aren't affected.

### MINOR-2: AC-065 still relies on the slice-4 confirmation rule for unpublishing a habit that goals use
- **Where**: AC-065, line 1189: "`f5.ac065.admin@example.com` unpublished `A5.065 Run`, so its state is Unpublished". This happens after the goal on it was created (line 1188).
- **Against**:
  - spec.md "Slice 4 behaviour these tests use" (lines 98-137) restates goal creation but not unpublishing. The edge case (line 415) only names "slice 4 FR-034".
  - Slice 4 FR-033 (specs/004-goals/spec.md line 872): "Unpublishing a Published catalog habit that at least one goal uses MUST, without confirmation, give 'confirmation needed' ... and leave the habit Published".
  - The acceptance.md Habits convention (lines 34-38) defines unpublishing without any confirmation.
  - ADR 0003 line 12: writers work "from the feature spec, its acceptance criteria and this ADR".
- **Continues**: MINOR-3 (pass 2)
- **Reasoning**: Removing "confirming" makes the dependency harder to see, but it is still there. A writer who unpublishes as the Habits convention describes, with no confirmation, gets "confirmation needed". The habit then stays Published, and the Given's own claim "so its state is Unpublished" fails. Only the public API's shape, or slice 4's spec, which the AC sources don't include, tells the writer to confirm. This is the same source standard the author used to keep pass-1 MINOR-1 out until spec.md restated the password rule. Here it is still applied unevenly.

### MINOR-3: AC-069 tests only the refusal side of the password length boundary
- **Where**: AC-069 Examples (lines 1271-1275): `exampl1` (7 characters with a digit), `examples` (8, no digit), `abc`. Every accepting run uses `example2026` (11 characters), for example AC-051 and AC-069 line 1267.
- **Against**: spec.md "Passwords" (lines 129-131: "at least 8 characters and at least one digit") and FR-030 (lines 541-544). The review rule says boundaries must catch an off-by-one.
- **Continues**: new (it follows from the coverage that pass-1 MINOR-1 asked for)
- **Reasoning**: Suppose demo history loading requires 9 or more characters, or more than 8. It refuses all three rows, as expected, and accepts `example2026`, so it passes every item. No item accepts a demo password of exactly 8 characters with a digit. So the "at least 8" boundary is shown only from below.

### MINOR-4: The Ranges convention states a property that two items don't have, and misdescribes FR-009
- **Where**:
  - Ranges convention, lines 81-83: "every history range in these items starts on a Monday and ends on a Sunday, or covers no week of the goal partly ... (FR-009 does not say)".
  - AC-062 line 1149 reads "every goal" of `demo1` to `demo3` "from 2026-11-18 to 2026-11-25".
  - AC-006 row 08 (line 272) checks "nothing is recorded" for a 3-times-per-week goal. The definition (lines 63-64) reads the history "over the item's dates", which are 2026-11-16 and 2026-11-20.
- **Against**:
  - Slice 4 FR-040 (specs/004-goals/spec.md, `demo2@example.com`: "Yoga ... 3 times per week", starting on the day of the run). So in AC-062 its first week 2026-11-23..29 is only partly inside 11-18..11-25.
  - spec.md FR-009 (lines 474-475) defines the entries as those "of the goal's evaluation calendar in that range (slice 4 FR-013)". Slice 4 FR-013 (specs/004-goals/spec.md lines 738-739) says "one evaluation period per week that overlaps the range".
- **Continues**: new (it follows from the fix for pass-2 MINOR-4)
- **Reasoning**: Both items give the same result whether or not a partly covered week is returned ("no record and no note"), so neither fails in either case. But the convention says the question never arises in "these items", and it does arise in AC-062 and AC-006 row 08. A translator who relies on the convention to treat ranges as week-aligned would be misled. The parenthetical is also imprecise: FR-009 defers the rule to slice 4 FR-013, which does state it. What is true is only that spec.md does not restate it.

## Summary
0 MAJOR, 4 MINOR. Previous notes: 5 Resolved, 1 Partly, 0 Unresolved.

This revision closes the target-combination gap. All 18 combinations of Build or Reduce, measurement and schedule kind now have a final result at, inside and past the target, apart from the two yes/no commitment kinds that the SC paragraph names. It also closes the value-before-note order for changing a record, the coverage mapping errors and the padded-note notation, and the new AC-069 tests the password rule that spec.md now restates. I recomputed every date, weekday, total, state and grace-period instant in the changed and new rows, and all of them are correct. The four remaining notes are small:
- The Build period rule is never told apart from a sum-based rule.
- AC-065 still needs a slice-4 confirmation rule that the AC sources don't give.
- The password length boundary is tested only from the refusal side.
- The Ranges convention claims more than the items show.

None of them blocks translation. The file can be locked once the author has decided whether to tidy them.
