# Critic pass 1: ac for Recording and evaluation

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: specs/005-recording-and-evaluation/acceptance.md (no golden samples: tests/HT.AcceptanceTests/Samples/F005 does not exist and no item names one)

## Notes

### MAJOR-1: No item tests the refusals of the change-record use case
- **Where**: acceptance.md, Coverage row FR-005 (line 94), which maps it to AC-007, AC-008, AC-010, AC-032, AC-033 and AC-038. Every "change" in the file (AC-025 row 01, AC-027 rows 05/06, AC-029, AC-032, AC-033 row 04, AC-037 rows 05/06, AC-047/049/050 row 02, AC-048) uses a valid value and either no note or a valid one.
- **Against**: spec.md FR-005 (line 439): "Recording **or changing** a record MUST give one result, checking in this order", followed by (5) the value messages, (6) the note and the record check in (3). Also FR-004 (line 436): "a record never stores a value that does not belong to the goal's measurement". Constitution II says a correction must give the same result as an original record, so a change must be held to the same value rules.
- **Reasoning**: AC-007 (value messages), AC-009 (values that don't belong to the measurement), AC-010 (check order) and AC-038 rows 03/04 (note length) all use only the *record* use case. For *change* there is no item where the value is missing, -1, 10000 or 1441, none with a 501-character note, none with "Yes" plus an amount, and none that checks the order of record versus value (an unknown record identifier together with an invalid value). Suppose an implementation's change use case skips value checks, or checks the value before the record. It would store out-of-range amounts on finalized results and still pass every item. That is incorrect data from a requirement the coverage table counts as covered. The convention "changes the record of D to V" already allows "no value", so these cases can be written.

### MINOR-1: The excluded "demo password does not meet the password rule" case has a reason that doesn't hold
- **Where**: acceptance.md Coverage rows FR-030 (line 109) and US7-AS6 (line 162); AC-061.
- **Against**: spec.md US7-AS6 (line 385) and FR-030 (line 538: "with one that fails slice 1 FR-002 ... in both cases it creates nothing"). Slice 1's FR-002 (specs/001-accounts-and-access/spec.md line 459) is concrete: "at least 8 characters and contain at least one digit". The AC rule (review item 7) lets a scenario be left out only when it is "not testable through the API", with a reason.
- **Reasoning**: The case is reachable through the API: demo history loading takes the password directly. The reason given, "no literal password that breaks it can be given", is wrong, because FR-030 names the rule by number and that rule is a two-clause literal check. So half of US7-AS6, and the "creates nothing" half of FR-030 for a weak password, are untested. An implementation that skips the rule check and creates `demo4` with a weak password passes every item.

### MINOR-2: Edge cases and success criteria are missing from the coverage table, and two edge cases are not addressed
- **Where**: acceptance.md "## Coverage" (lines 86-162) lists FR and US rows only.
- **Against**: spec.md "### Edge Cases" (lines 391-414) and "## Success Criteria" SC-001 to SC-007 (lines 714-727).
- **Reasoning**: No edge case and no SC has a row, so you can't check from the table whether they are covered. Checked one by one, most edge cases are covered (AC-030, AC-034, AC-027 row 02, AC-005 row 04, AC-014 row 04, AC-015 rows 05/06, AC-014 rows 06-08, AC-020 rows 04-06, AC-027 rows 03/04, AC-033). Two are not:
  - "A goal whose catalog habit is Unpublished (slice 4 FR-034) keeps being recorded and evaluated as usual" (line 412). Every non-demo item uses a custom habit, and AC-060 is about demo history loading leaving goals out. An implementation that refuses to record once a goal's catalog habit is Unpublished would pass everything.
  - "After a large move east, the first commitment of the new version may already have ended when the change is made" (line 396). It is neither tested nor listed as not testable. With the time zones the spec allows acceptance tests (line 117: "Europe/Kyiv", "America/New_York", UTC), it looks unreachable, and the table should say so.
  - The page and host SCs (SC-001, SC-005 hosted part, SC-006, SC-007) are not marked as outside the AC surface either.

### MINOR-3: Several kinds of target are never evaluated to a final Met or Failed
- **Where**: AC-014 to AC-022 and AC-044.
- **Against**: spec.md SC-002 (line 716: "For every combination of Build or Reduce, measurement and schedule kind, the tests cover the target at, just below and just above its amount"), constitution II (both sides of each boundary), and spec "What the target means" (lines 119-124).
- **Reasoning**: A final Met or Failed is checked only for Build duration fixed-days (AC-014), Reduce count fixed-days (AC-015), yes/no fixed-days (AC-016), Build yes/no per-week (AC-017), Build count per-week (AC-018), Reduce count per-day (AC-019) and yes/no per-day (AC-020).
  - Reduce duration is never final Met or Failed. AC-022 row 04 checks only Missed, and AC-044 row 07 only Due progress.
  - Reduce count per-week ("the period's total is at most the amount" over a week) and Reduce yes/no per-week are never evaluated.
  - Reduce yes/no is evaluated only with N = 1 (AC-020 rows 01-03). An implementation that hard-codes "at most 1 Yes" instead of "at most N" passes.
  - Build count fixed-days and Build count or duration per-day have no final-state item.
  - Unit tests may cover these, but SC-002 is stated over "the tests", and the AC file doesn't say which combinations it leaves to unit tests.

### MINOR-4: Notes: trimming before the length check, and trimming of a note saved on its own, are not discriminated
- **Where**: AC-011, AC-036, AC-038.
- **Against**: spec.md FR-007 (line 455: "A note is trimmed; longer than 500 characters it gives ..."). The order is trim first, then measure.
- **Reasoning**: AC-038 uses exactly 500 and 501 untrimmed characters, so an implementation that measures before trimming gives the same results. There is no note of 500 characters plus surrounding spaces, which such an implementation would wrongly refuse. Trimming is checked only for a note given with a record (AC-011). A note saved on its own with surrounding spaces (as opposed to the all-space removal case in AC-036 rows 01/02) is never read back, so a save-note path that stores untrimmed text passes.

### MINOR-5: The check order of the save-note use case is not discriminated
- **Where**: AC-038 rows 01/02 (note length only), AC-039 rows 03/04 (date only), AC-047/049/050 row 04 (privacy only).
- **Against**: spec.md FR-018 (line 524): every use case "MUST check in the order of FR-005 (signed in, role, goal, record, date, value, note, limit)". Constitution III negative tests are covered, but the date-before-note order is not.
- **Reasoning**: No save-note item has both an invalid date and a 501-character note, so the order of the date and note checks for save note is unverified. The same order is tested for recording in AC-010.

### MINOR-6: AC-046 claims more than it tests, and puts an action in a Then step
- **Where**: AC-046 title (line 869: "the old and the new day can both be Due and both be recorded") and its step "And recording Yes for 2026-11-19 at <time> succeeds ..." (line 876).
- **Against**: acceptance.md Time convention (line 11: "A Then step, and every And or But step after it, checks the results of the item's When step"), the AC rule "each item tests one behaviour", and spec.md US5-AS3 / FR-011.
- **Reasoning**: Only the new day (2026-11-19) is recorded. Recording the old day (2026-11-18) while it is still Due in the overlap is never done, so "both be recorded" is not shown. The recording is a second state-changing call inside the Then block, which contradicts the convention that steps after Then only check the When step's results. It also merges an FR-011 read check and an FR-002 recording check into one item.

### MINOR-7: The progress notation adds unit words that the convention doesn't define
- **Where**: Conventions "Progress" (lines 68-72) define only "progress A of B", "of at most N" and "total A of at most B". The items write "progress 12 of 20 minutes" (AC-001, AC-014, AC-025, AC-042), "progress 2 of 3 times" (AC-003, AC-017, AC-018), "progress 1 of 1 times" (AC-021 rows 06/07), but "progress total 25 of at most 30" for a Reduce duration period (AC-044 row 07) and "total 1 of at most 1" for a count period (AC-019, AC-053), with no unit.
- **Against**: spec.md FR-014 (progress as numbers against a target). HP-011 puts the formatted texts ("12 of 20 min", "1 of at most 2 cups") in a UI formatter outside the AC surface.
- **Reasoning**: The translator and test writer can't tell whether "minutes"/"times" is part of the expected value. If it is, it would have to come from a formatter that isn't on the AC surface, and it is missing for Reduce periods. If it isn't, the convention should say the trailing word is only a gloss. As written, the notation is inconsistent across items.

### MINOR-8: The weekday cases that FR-030 defines specially are not tested
- **Where**: AC-054 (R is a Wednesday) and AC-055 (R is a Monday) are the only Yoga demo items.
- **Against**: spec.md FR-030 (line 556: "when R is a Sunday, the week ending on R"; line 568: "Yoga's week 1 is Pending when R is a Monday or Tuesday and Met otherwise") and HP-008/SC-005 ("whatever the weekday", "on any day").
- **Reasoning**: R = Sunday is the case the spec calls out explicitly, and it is where a Sunday-first week calculation (for example `DayOfWeek.Sunday == 0`) would put R in the following week, so week 1 to 3 would shift by one. No item covers it. R = Tuesday, the other side of the Pending/Met boundary for week 1, is not covered either: AC-055 covers Monday, AC-054 Wednesday. Neither wrong behaviour would fail any item.

## Summary
1 MAJOR, 8 MINOR.

The items are mostly accurate. Every date, weekday, DST instant (Kyiv 2026-10-25 and 2027-03-28), grace end, entry count (29, 364, 14) and the demo data for R = 2026-11-18 and R = 2026-11-23 match spec.md when recomputed. The privacy items cover all seven use cases for all three negative actors, and the grace and target boundaries for the cases that are tested hit just before, at and after the boundary. The file is not ready to lock, because FR-005's refusal contract for changing a record has no item at all. An implementation that skips value validation on the change path, and so stores invalid data, would pass. The other notes are smaller gaps: the weak-password exclusion has a reason that doesn't hold, two edge cases are not covered, some kinds of target are never evaluated, and the progress notation and AC-046 need tidying.
