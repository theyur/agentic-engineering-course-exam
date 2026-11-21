# Critic pass 1: ac for 008-excusals-and-pauses

**Verdict**: APPROVE
**Date**: 2026-11-21
**Reviewed**: specs/008-excusals-and-pauses/acceptance.md (no golden samples: `tests/HT.AcceptanceTests/Samples/F008` does not exist and acceptance.md names none)

All figures were recomputed against the reference material: the dates and weekdays of November 2026, the grace
ends (48 h after the end of the date), the consistency and streaks of AC-036, AC-053 to AC-059 and AC-075, the
recent results and trend of AC-055, AC-056 and AC-060 to AC-065, the dashboard counts of AC-067, AC-068 and
AC-077, and the demo figures and dates of AC-077 and AC-078 (R − k mapped for R = 2026-11-18 and checked for all
seven rows). No wrong figure was found. Every FR-NNN and every user-story scenario appears in the Coverage table;
the five *(page)* scenarios and US5-AS5 are listed as not testable with a reason.

## Notes

### MINOR-1: FR-016's lower target for an Excused commitment is never exercised
- **Where**: acceptance.md AC-062 to AC-065 (the FR-016 items); Coverage row FR-016.
- **Against**: spec.md FR-016: "a week's Build target counts only its commitments that are neither Excused nor
  Paused, and the week's value ... counts only the 'Yes' records of those commitments".
- **Reasoning**: AC-062 and AC-063 lower the target only through a Paused commitment ("target 2"). AC-064 has all
  three commitments Excused, so the bar is marked and shows no target. No item has a week with one or two
  Excused commitments and at least one counted one. An implementation that leaves out only Paused commitments
  from the target and value would pass every FR-016 item. That is a plausible wrong implementation, since the
  Paused path and the Excused path are separate code.

### MINOR-2: AC-022's "after the date and before the value" order rests on rules spec.md does not give
- **Where**: acceptance.md AC-022, rows 01 to 03, and its title.
- **Against**: spec.md "Earlier behaviour these tests use" (line 81: "this spec alone gives the rules and texts its
  acceptance tests depend on"); FR-005 ("after its step (4) (the date) and before its step (5) (the value)").
- **Reasoning**: rows 01 and 02 show that the excused and paused checks come before the value check only if
  `10000` pages is an invalid value. spec.md gives no value limit for records, so it never says that `10000` is
  refused. If it is a valid value, both rows pass whatever the order, and the title's claim goes untested. Row 03
  expects "You cannot record a day that has not begun." for 2026-11-17, a date that is also in the pause. So the
  row assumes that the not-begun check belongs to slice 5's step (4) and comes before (4a). spec.md lists the two
  date refusals in one sentence (line 109) but never says that both belong to step (4). The test writer sees only
  this spec (ADR 0003), so it can't verify either assumption.

### MINOR-3: Some check-order pairs are never told apart
- **Where**: acceptance.md AC-028 row 03; AC-011 and AC-012; AC-051.
- **Against**: spec.md FR-006 (steps 6 and 7), FR-002 (steps 5 and 6), FR-011 (steps 4 and 5). These are
  fixed-order result lists ("checking in this order").
- **Reasoning**: AC-028 row 03 (from 2026-11-20 until 2026-11-19) doesn't overlap the existing pause of
  2026-11-23 to 2026-11-25. So it tells step 6 apart from step 8 (the reason), but not from step 7 (the overlap).
  An implementation that checks the overlap before the reversed range passes it. FR-002's step 5 (not a
  commitment) before step 6 (in a pause) is never tested, for example with a Tuesday inside a pause of
  `Yoga, 3 days a week`. AC-011 and AC-012 use only commitment dates. AC-051 cancels an *upcoming* pause through
  the wrong goal, so an implementation that rejects current or past pauses (step 5) before it looks up the pause
  (step 4) gives the same text. The other orders are told apart: FR-002 steps 4/5, 6/7, 6/8, 7/8, 8/9, 8/10 and
  9/10, FR-004 steps 4/5, and FR-006 steps 4/5, 5/6 and 7/8.

### MINOR-4: Several FR clauses are only partly covered
- **Where**: acceptance.md, the Coverage rows FR-010, FR-015, FR-017, FR-018 and FR-019, and the edge case "A goal that
  has not started: AC-029".
- **Against**: spec.md FR-015 ("their progress as slice 5 FR-014 gives it"), FR-017 ("its records, progress and
  note as before"), FR-018 ("Its order is unchanged"), FR-019 ("MUST NOT count or list Excused or Paused entries
  as Due"), and the Edge Cases entry "A goal that has not started ... 'today' for its pauses is the account's
  today".
- **Reasoning**:
  - No item checks the progress of an exempt result that has a record, in recent results or in the history.
  - No item checks the order of the goals overview once a goal is paused.
  - No item checks that an Excused *Due* commitment (excused today) is left out of the dashboard's Due count or
    Today list. AC-002 checks only what can be recorded now.
  - For a goal that has not started, only pausing is checked (AC-029). Whether its pause is read as current,
    upcoming or past, and whether it can be resumed, both follow the account's today and are never checked.

  Each FR has at least one item, so this is partial coverage, not missing coverage.

### MINOR-5: FR-030 is checked only for `demo4` and `demo5`
- **Where**: acceptance.md AC-082; Coverage row FR-030; US6-AS5.
- **Against**: spec.md FR-030 ("Demo loading, demo history loading and demo report loading MUST stay exactly as
  slices 1 to 6 specify them, with no excusal or pause"); US6-AS5 ("their results and data are unchanged"); D-4
  (line 783), which shows that `demo1` has goals ("only a current pause on `demo1`").
- **Reasoning**: AC-082 reads the pauses and history only for `demo4@example.com` and `demo5@example.com`. The
  accounts created by demo loading (`demo1`, which D-4 shows has goals) are never checked for excusals or pauses,
  and the results of the earlier tasks aren't checked either. The Coverage table's FR-030 row gives no note that
  relies on the locked F001, F004 and F005 scenarios for these.

### MINOR-6: Some edge cases and part of SC-003 are left unchecked without a "not testable" reason
- **Where**: acceptance.md, the edge-case list ("Time zone changes: not checked", "Long pauses: not checked") and the
  success-criteria paragraph (SC-003).
- **Against**: spec.md Edge Cases ("a time zone change (slice 4 FR-026) never moves them, and a moved commitment
  of the same date keeps its excusal"; "a goal paused for months shows 'N/A' and its old streak unchanged when it
  resumes"); SC-003 ("excusing and removing an excusal just before, at and just after a grace period's end").
- **Reasoning**: both edge cases are rules about use-case results and look reachable through the API. The file
  marks them "not checked" without saying why, unlike the clock-change case, which gets a reason. The SC-003
  paragraph lists only "just before and at" for excusing (AC-007) and for removing (AC-018). It drops "just after"
  without saying so.

### MINOR-7: The timing convention is ambiguous for setup when an item uses the default goal-creation time
- **Where**: acceptance.md Conventions, "Time" ("Setup that gives no time ... runs at the earliest time the item
  gives") and "Habits and goals" ("Unless an item says otherwise, a goal is created at `2026-11-02T08:00:00Z`").
- **Against**: spec.md "What acceptance tests can reach" ("The current time is supplied to every use case").
- **Reasoning**: in most items (e.g. AC-001) the earliest time written is the When time (2026-11-12T10:00:00Z),
  while the goal is created at the default 2026-11-02T08:00:00Z. Read literally, registration, sign-in and the
  custom habit run at 2026-11-12, after the goal that is created on that habit. Read the other way, the default
  counts as "given" and they run at 2026-11-02. The convention doesn't say which, so the test writer has to
  guess.

### MINOR-8: Notation is inconsistent with the file's own conventions
- **Where**: acceptance.md AC-046, AC-067 to AC-071 (Today-list and dashboard-note tuples); AC-080; AC-081; AC-006
  row 01.
- **Against**: acceptance.md Conventions, "Results and figures" ("(goal, date, state)"), "Demo data" ("A result
  of a demo task is written (email, text)") and "Examples" ("Every email of such an item contains `<n>`"; the
  exception covers only "demo emails").
- **Reasoning**:
  - Goal names contain a comma ("Drinking water, 8 glasses a day"), but they're written unquoted inside
    comma-separated tuples, e.g. "(Drinking water, 8 glasses a day, 2026-11-14, Pending)". Elsewhere goal names
    are in backticks. This makes the tuples ambiguous to parse.
  - AC-080 writes its results as "(no demo password configured)", without the email the convention requires.
  - AC-081 has an Examples table, but its administrator email `f8.ac081.admin@example.com` has no `<n>` and isn't
    a demo email.
  - AC-006 row 01's setup "records nothing" isn't a phrase the Recording convention defines.

### MINOR-9: The Coverage table doesn't match the item headers
- **Where**: acceptance.md, the Coverage rows FR-007 and FR-018; the headers of AC-039 and AC-042.
- **Against**: internal consistency of acceptance.md.
- **Reasoning**: the FR-007 row lists AC-039, but AC-039's header is "(FR-008; FR-009)". AC-042's header names
  FR-018, but the FR-018 row doesn't list AC-042. These were found by comparing every header tag with the table
  mechanically. No other row differs.

## Summary
0 MAJOR, 9 MINOR. The acceptance criteria are factually sound. Every expected figure, state, grace end and demo
date recomputes correctly from spec.md, and the boundary pairs SC-003 names are mostly present: the grace end at
23:59:59 and at 00:00, the start of the day, the pause's first day, and 200 or 201 characters. Every FR and
scenario is mapped, and the excluded page scenarios have reasons. The most useful fixes are discrimination gaps,
where a plausible wrong implementation would still pass:
- the Excused target reduction of FR-016;
- a few check-order pairs;
- the value and not-begun assumptions of AC-022.

The rest is partial coverage of secondary FR clauses, the unchecked earlier demo accounts, an ambiguous timing
convention, and notation and table inconsistencies. None of them blocks translation to `.feature` files.
