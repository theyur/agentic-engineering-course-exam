# Critic pass 2: ac for Recording and evaluation

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: specs/005-recording-and-evaluation/acceptance.md (no golden samples: tests/HT.AcceptanceTests/Samples/F005 does not exist and no item names one)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 No item tests the refusals of the change-record use case | Resolved | AC-064 (lines 1150-1171) changes a final record (at `2026-11-22T10:00:00Z`) to no value, -1 and 10000 (count), 1441 (duration), no value (yes/no), and a 501-character note. It also checks an unknown record identifier together with no value (record checked before value, row 07), the upper bound 9999 (row 08), and values that don't belong to the measurement (rows 09/10). Each refusal checks that the stored record and note are unchanged. Coverage rows FR-004 to FR-007 list AC-064. One order gap is left, see MINOR-2. |
| MINOR-1 Weak demo password case excluded with an unsound reason | Resolved (rejection accepted) | The new reason holds. ADR 0003 says the AC test writers work "from the feature spec, its acceptance criteria and this ADR", and spec.md FR-030 (line 539) only names "slice 1 FR-002" without restating the rule, so no failing password can be justified from the allowed sources. The gap is stated in the FR-030 Coverage note (line 112) and the US7-AS6 row (line 165) and is routed to the spec owner. What is left belongs in spec.md, not acceptance.md. The same standard isn't applied to AC-065, see MINOR-3. |
| MINOR-2 Edge cases and SCs missing from coverage; two edge cases unaddressed | Resolved | The edge-case list (lines 167-184) maps every spec edge case. The Unpublished-habit case is covered by AC-065. The large-move-east case is listed as unreachable with a reason: a 7-hour move from `America/New_York` to `Europe/Kyiv`, correct for November 2026 (EST −5, EET +2). The SC paragraph (lines 186-190) marks SC-001, SC-006 and SC-007 and the hosted part of SC-005 as outside the AC surface. Some of its claims are overstated, see MINOR-1 and MINOR-5. |
| MINOR-3 Several kinds of target never evaluated to a final Met or Failed | Partly | AC-066 (lines 1183-1213) adds Build count fixed-days, Build count, duration per-day, Build duration per-week, Reduce duration fixed-days and per-day, Reduce count per-week, Reduce yes/no per-week, and Reduce yes/no with N = 2. Reduce duration per-week is still never evaluated, and several combinations are tested on one side only. Carried forward as MINOR-1. |
| MINOR-4 Trim-before-length and trimming of a note saved on its own not discriminated | Resolved | AC-038 rows 05/06 use 504 characters that trim to 500, which an implementation that measures before trimming would refuse. AC-036 row 04 reads back a note saved on its own as "New note." from "  New note.  ". The notation of rows 05/06 is ambiguous, see MINOR-6. |
| MINOR-5 Save-note check order not discriminated | Resolved | AC-039 rows 05/06 pair a 501-character note with a date that has not begun and a date before the start date. Each expects the date message. |
| MINOR-6 AC-046 claims more than it tests and acts in a Then step | Resolved | AC-046 (lines 902-915) is now read-only and its title is limited to "both Due while they overlap". Recording both days moved to AC-067 (lines 1215-1222), which records 2026-11-18 (old version) and 2026-11-19 (new version) in its When step. Recomputed: NY day 2026-11-18 is `05:00Z`–`05:00Z`, Kyiv day 2026-11-19 is `2026-11-18T22:00Z`–`2026-11-19T22:00Z`, so both are Due at `2026-11-18T23:00Z`. |
| MINOR-7 Progress notation adds undefined unit words | Resolved | The Progress convention (lines 69-75) now says a trailing word only helps the reader, and that the two numbers and the kind of progress are what is checked. |
| MINOR-8 FR-030 weekday cases (R Sunday, R Tuesday) not tested | Resolved | AC-068 rows 01/02. Recomputed: R = Sunday 2026-11-22 gives start 2026-10-25, current week 11-16..11-22 Due, week 1 Met, week 2 Failed, week 3 (10-26..11-01) Missed, and no earlier week. R = Tuesday 2026-11-24 gives start 2026-10-27 and first full week 11-02, and week 1 (11-16..11-22) is Pending until `2026-11-25T00:00Z`. Both match FR-030 lines 555-569. |

No older note has regressed. I recomputed every date, instant and count in the items: AC-012/013/021/023/030/031/034/040/042/043/045/046/052-055/057/065/066/067/068. All match spec.md.

## Notes

### MINOR-1: Reduce duration per-week is never evaluated, and the SC-002 claim says every combination is covered on three sides
- **Where**: acceptance.md, Success criteria paragraph (lines 186-187): "AC-012 to AC-023, AC-066 (every combination of Build or Reduce, measurement and schedule kind, at, just below and just above the target)". AC-066 (lines 1183-1213), whose own title is narrower: "Met at their target and Failed just past it".
- **Against**: spec.md SC-002 (line 716: "For every combination of Build or Reduce, measurement and schedule kind, the tests cover the target at, just below and just above its amount"). Constitution II ("at, below, and above a Build or Reduce target"). spec.md "What the target means" (lines 119-124).
- **Continues**: MINOR-3 (pass 1)
- **Reasoning**: I counted all 18 combinations against the items.
  - **Not covered at all**: Reduce, duration, per week. No item creates such a goal; AC-022 row 04 and AC-044 row 07 are per-day.
  - **One side missing**:
    - Reduce duration fixed-days (AC-066 rows 10/11: 30, 31), Reduce duration per-day (rows 12/13), Reduce count per-week (rows 14/15) and Reduce yes/no per-week (rows 16/17) have no value below the limit.
    - Build count per-day (rows 04/05) and Build duration per-day and per-week (rows 06-09) use only records exactly at, or one below, the amount. Build yes/no per-day (AC-020 rows 04-06) has no case above N.
  - **What would pass**: an implementation that tests `total == amount` on a Reduce per-week or Reduce duration path, or `value == amount` on a Build per-day or duration-per-week path, passes every row of these combinations. AC-018 row 01 (25 pages) discriminates this only for Build count per-week.
  - **Why it matters**: SC-002 may still be met by unit tests, but acceptance.md claims it is met here, and that claim does not match the items.

### MINOR-2: Changing a record does not discriminate the value check from the note check
- **Where**: AC-064 row 06 (line 1167) gives a valid value (3) with a 501-character note. No row of AC-064 gives an invalid value together with an invalid note.
- **Against**: spec.md FR-018 (lines 524-525: every use case "MUST check in the order of FR-005 (signed in, role, goal, record, date, value, note, limit)"). FR-005 (line 439) applies to "Recording or changing a record".
- **Continues**: new (the last part of the order gap of MAJOR-1, pass 1)
- **Reasoning**: For recording, AC-010 row 06 sends no value with a 501-character note and expects the value message. For changing, row 07 checks record before value, but nothing checks value before note. A change path that checks the note before the value returns "The note can have at most 500 characters." for such a call and still passes every item.

### MINOR-3: AC-065 depends on slice-4 unpublishing behaviour that the AC sources don't give
- **Where**: AC-065 (lines 1175-1178): "created a goal on the catalog habit `A5.065 Run`" and "unpublished `A5.065 Run`, confirming that goals are based on it, and it is Unpublished".
- **Against**:
  - spec.md "Slice 4 behaviour these tests use" (lines 98-134) is meant to give "the rules and texts its acceptance tests depend on". It restates goal creation on a Published catalog habit (line 102), but not unpublishing or any confirmation when goals exist. The edge case (line 412) only cites "slice 4 FR-034".
  - The acceptance.md Goals convention (lines 39-45) defines only "created a goal on their custom habit `C`".
  - The Habits convention (lines 34-38) defines "an Unpublished `N`" without a confirmation.
  - ADR 0003: test writers work from the spec, the acceptance criteria and the ADR.
- **Continues**: new
- **Reasoning**: To reject pass-1 MINOR-1, the author ruled that a rule which spec.md only names by slice and number cannot be used, because the AC sources don't give it. AC-065 relies on exactly such a rule: an unpublish step that needs confirming when goals are based on the habit (slice 4 FR-034). It also uses a goal-creation phrasing that no convention defines. The test writer cannot tell from the allowed sources what "confirming" means, or whether an unpublish without it is refused. The same source standard is applied unevenly.

### MINOR-4: Whether a history range returns a per-week period that only partly overlaps it is not stated
- **Where**:
  - AC-044 row 02 (line 876) and AC-066 rows 08/09 and 14-17 (lines 1202-1211) read the history "from 2026-11-18 to 2026-11-18" of a per-week goal and expect "exactly one entry" for the week 2026-11-16..11-22.
  - AC-054 (line 1046), AC-055 (line 1057) and AC-068 (line 1228) read up to R and expect the current week, which ends after R.
- **Against**: spec.md FR-009 (lines 470-473: "each commitment or evaluation period of the goal's evaluation calendar in that range (slice 4 FR-013)"). US5-AS1 (line 323). Neither defines when a multi-day period is "in" a range, and the acceptance.md conventions don't either.
- **Continues**: new
- **Reasoning**: These items only pass if a period that overlaps the range at all is returned. Two other readings are plausible: return a period when its first date falls in the range, or only when the whole period lies inside the range. Under either one, AC-044 row 02 and AC-066 rows 08/09/14-17 return no entry, and AC-054/055/068 lose the current week. The rule may exist in slice 4 FR-013, but the AC sources restate the other slice-4 rules the tests depend on and leave this one out. So the translator and the test writer have no stated basis for the expected entry counts.

### MINOR-5: Coverage claims and the Coverage table don't match the items
- **Where**: acceptance.md Coverage table (lines 91-165) and SC paragraph (lines 188-189). The AC headers were cross-checked mechanically against the table rows.
- **Against**: review rule "every FR-NNN has at least one AC item"; internal consistency of the AC file.
- **Continues**: new
- **Reasoning**:
  - **SC-005 claim**: "AC-052 to AC-055 and AC-068 show every state for R on four weekdays" is overstated. AC-068 reads only the Yoga goal: row 01 (R = Sunday) shows no Pending, and row 02 (R = Tuesday) shows no Met. Only Wednesday (AC-052 to AC-054) and, for Yoga only, Monday (AC-055) show the states the sentence claims.
  - **FR-014 row**: the row lists AC-065, but AC-065 checks no progress and no Met or Failed state, only Pending and Due.
  - **Header tags missing from the table**:
    - AC-067 is tagged FR-011, but the FR-011 row (line 103) lists only AC-045 and AC-046.
    - AC-003 and AC-004 are tagged FR-014, but the FR-014 row (line 106) omits them.
    - AC-034 is tagged FR-009, but the FR-009 row (line 101) omits it.

### MINOR-6: The notation for a note with surrounding spaces in AC-038 rows 05/06 is undefined and reads like several calls
- **Where**: AC-038 row 05 (line 781): "saves the note `"  "`, then a text of 500 characters, then `"  "` (504 characters in all, 500 after trimming)". Row 06 (line 782) has the same wording without the parenthetical.
- **Against**:
  - The Texts convention (lines 76-77) defines only `""`, `"   "` (three spaces) and "a text of N characters". It defines neither two spaces nor joining texts with "then".
  - Elsewhere "then" means successive calls: AC-004 line 223 ("records 1 …, then records 2 …, then records 0"), AC-039 row 02 ("saves the note … and then the note …"), AC-061 line 1124.
- **Continues**: new
- **Reasoning**: Read with the file's own use of "then", row 05 describes three saves, the last of which (only spaces) would remove the note. That contradicts the expected result, so a reader has to infer that the texts are joined. Row 06 lacks even the "504 characters in all" hint. The translator must guess one value that the convention doesn't define, in the only items that discriminate trim-before-length (pass-1 MINOR-4).

## Summary
0 MAJOR, 6 MINOR. Previous notes: 8 Resolved, 1 Partly, 0 Unresolved.

The revision resolves the blocking gap from pass 1. AC-064 holds the change-record use case to FR-005's value, note and record checks and to FR-004's storage rule. The other pass-1 notes are resolved, apart from part of the target-combination coverage. I recomputed every date, weekday, DST instant, grace end, entry count and demo record against spec.md, including the new R = Sunday and R = Tuesday cases and the move-east overlap, and all of them are correct. The remaining notes are smaller:
- One target combination (Reduce duration per-week) is never evaluated, and the SC-002 claim overstates what the items cover.
- Changing a record does not test the value check against the note check.
- AC-065 relies on a slice-4 rule that the AC sources don't restate, against the source standard the author used to reject pass-1 MINOR-1.
- No source says how a per-week period that only partly overlaps a history range is treated.
- Some coverage mappings and the SC-005 claim are inaccurate.
- AC-038 rows 05/06 use an undefined notation.

None of these breaks a requirement by itself. The file can be locked once the author has decided whether to tidy them.
