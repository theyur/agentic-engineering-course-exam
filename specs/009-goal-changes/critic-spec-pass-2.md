# Critic pass 2: spec for 009-goal-changes

**Verdict**: REVISE
**Date**: 2026-11-22
**Reviewed**: specs/009-goal-changes/spec.md, specs/009-goal-changes/checklists/requirements.md (with the changed reference material CONTEXT.md, docs/adr/0025-goal-change-rules.md, docs/adr/0026-demo-goal-change-loading.md)

I recomputed the following figures and found them correct:

- US3-AS1: start date 18 Nov when asked on 18 Nov, and 17 Nov when asked on 16 Nov at 11:00.
- US3-AS6: the ending-date refusal, with "past" checked before "on or before the ending date".
- US3-AS12: at 17 Nov 10:00, 14 Nov is Missed (its grace ended 17 Nov 00:00), 15 Nov is Pending (until 18 Nov 00:00) and 17 Nov is Due.
- US4-AS2 to AS5: Sugar's attempt 1 has its last date on 13 Nov and is final on 16 Nov, its restart start date 16 Nov is after its ending date 14 Nov, and the goal-history order is right.
- US1-AS14: the pause starts on the current date 11 Nov with no record on or after 11 Nov, so slice 7 FR-006 step (5) and FR-009 allow it.
- FR-031 with the new Reading version 3: "96% (25 of 26)" and streaks 6 / 19 are unchanged, the settings in effect are 20 minutes from R − 13, and the later settings are 25 minutes from R + 1. Meditation, Sugar, the dashboard (2 Due / 4 Pending / 0 Missed / 2 goals) and the goal-history order are unchanged.

The "Goal history" entry in CONTEXT.md matches FR-015 and Key Entities.

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (one date in two attempts) | Resolved | FR-012 step 6 adds "A new attempt starts after the day the goal ended." (a start date on or before the ending date). FR-011 defaults to the later of today and E + 1. Attempt 1 ends at E − 1 at the latest and attempt 2 starts at E + 1 at the earliest, so no date is shared. ADR 0025 and CONTEXT.md Attempt match this. |
| MAJOR-2 ("Goal history" has no CONTEXT entry) | Resolved | CONTEXT.md now has a "Goal history" entry with NOT-boundaries against a goal's History and the attempt view. HP-013 lists it. FR-015 and Key Entities state the distinction. |
| MINOR-1 (ending-date records in a new attempt with another measurement) | Resolved | A restart now starts after the ending date. FR-012's argument holds: at the ending no day after E has begun, and FR-010 refuses later dates. The edge case "Ending and restarting on the same day" was rewritten to match. |
| MINOR-2 (goal-level dates for multi-attempt or ended goals) | Partly | New terms: *Goal's start date*, *A goal that has not started* and *Current date* of an ended goal, plus an FR-005 addition. However, the new definition of "has not started" takes away the current date that slice 5 FR-002 needs to record an earlier attempt's dates. MAJOR-2 continues this note. |
| MINOR-3 (earlier attempt's Pending after a restart) | Resolved | FR-016 and FR-019 now include an ended attempt's Pending entries, with the attempt number and whether the attempt is ended. US3-AS12 and HP-007 ("Attempt {n}") cover this. The recording gap is carried by MAJOR-2, and one leftover case by MINOR-3. |
| MINOR-4 (inputs of "read one attempt") | Resolved | FR-014 now names the inputs (acting user, goal, number, time, no range) and the History span, and gives the newest 366 dates when there are more. |
| MINOR-5 (US3-AS2 asserted page text) | Resolved | US3-AS2 now asserts the number 2, "started 18 November", "N/A" and "0 commitments". FR-031 says "latest attempt number 2". |
| MINOR-6 (US1-AS14 Given impossible) | Resolved | Records now run only from 2 to 10 Nov, and the pause is set on 11 Nov at 09:00 starting that day. Verified against slice 7 FR-006 and FR-009. |
| MINOR-7 (HP-006 link needs a "latest" flag) | Resolved | FR-015 returns whether the attempt is the goal's latest. HP-006 uses it, and US4-AS3 asserts it. |
| MINOR-8 (catalog details' goal list) | Resolved | FR-020 now covers the details' list. US4-AS5 asserts that the list is empty. |
| MINOR-9 (ending removes a version in effect on E) | Resolved as accepted consequence (rationale sound, with two caveats) | The edge case "Ending on the day a change took effect" states the consequence. The rationale ("the removed settings never applied to a result") holds for a version from E or from the Monday of E's week. It fails when the removed version is the attempt's only version (MAJOR-1). ADR 0025 still describes a narrower removal (MINOR-4). |
| MINOR-10 (demo hardly shows a changed target) | Resolved | Reading now has version 3 from R + 1, so the Settings card's "From …" row shows. HP-012 checks "25 min daily". The figures are unchanged, as I recomputed. |
| MINOR-11 (community scope pre-decided) | Resolved | Assumptions now leave contributions of earlier attempts and ended goals to slice 9, with SPEC §11.5 and §11.6. |
| MINOR-12 (open page states) | Resolved | HP-003: no outcome is preselected, and the design change is listed. HP-004: Not found and return cases. HP-005: no "Record" on an ended item. HP-006: an administrator has no Goal history. |
| MINOR-13 (restart on an Unpublished habit) | Resolved | FR-012 and the new edge case allow it and cite slice 4 FR-034. |

No older note has regressed. Every pass-1 figure I rechecked is unchanged.

## Notes

### MAJOR-1: Ending an attempt that has not begun removes all of its settings versions, leaving the ended goal with none
- **Where**: spec.md FR-009 ("removes … every settings version that takes effect after it", line 603), with FR-007 ("A goal that has not started, or whose current attempt has not begun, can be ended."), FR-008 ("A last date before the attempt's start date means the attempt has no commitment or period"), US2-AS12 (line 278) and the edge case "Ending on the start date, or before it" (line 471).
- **Against**: FR-004 (an ended goal is named by "its latest attempt's last version"); FR-005 ("For an ended goal, the settings in effect are its latest attempt's last version"); FR-011 (the starting values are "the settings of the ended attempt's last version"); the term *Current date* of an ended goal (line 530, the time zone of "its latest attempt's last version"); the edge case "Ending on the day a change took effect" (line 503, the values "then come from the version of the attempt's last date"). In the code, `GoalTimeline.VersionOf` falls back to `versions[0]` and assumes at least one version (src/HT.Domain/Goals/GoalTimeline.cs).
- **Continues**: new (it widens the case behind pass-1 MINOR-9)
- **Reasoning**: An attempt's first version applies from its start date S, and FR-002 keeps that date. When the attempt is ended on or before S, its last date is before S, so every version of the attempt takes effect after the last date. FR-009 then removes all of them. Two cases show this:
  - US2-AS12: the goal is created on 16 Nov with S = 23 Nov and ended on 17 Nov. The ending date is today, 17 Nov, so the last date is 16 Nov, and the only version (from 23 Nov) is removed.
  - The same happens to a restarted goal whose attempt 2 is ended before it begins, and to a per-week goal ended within its first week (its last date is the Sunday before S's week).

  The goal then has no version, so the spec leaves several things undefined:
  - its name (FR-004);
  - the settings returned by reading the goal (FR-005);
  - the starting values of a new attempt (FR-011);
  - the time zone of its current date.

  The new edge case says these values "come from the version of the attempt's last date", and no version covers that date. The accepted MINOR-9 rationale also does not reach this case: the setting being removed is the goal's only definition of what it measures. An implementer must either break FR-004, FR-005 and FR-011, or invent an exception to FR-009. US2-AS12 is an acceptance scenario that runs straight into this.

### MAJOR-2: The new meaning of "a goal that has not started" takes away the current date that recording an earlier attempt's dates needs
- **Where**: spec.md "Terms used below", *A goal that has not started* (lines 527–529: "a goal whose latest attempt has not begun … Where those slices use the account's today for such a goal, they do so for this goal too, also when an earlier attempt exists"); FR-005 ("as for a goal that has not started (slice 4 FR-016)"); FR-010; FR-014 (last sentence); FR-016 and FR-019 (an ended attempt's Pending entries are kept "so that the user can still record them"); the edge case "Earlier attempt's Pending results after a restart".
- **Against**: slice 4 FR-012 ("a goal whose start date's day has not begun has no current date"); slice 6 FR-012 ("for a goal that has not started, which has no current date"); slice 5 FR-002 (a record is accepted "only when that date's day has begun on the goal's timeline (the goal's current date or earlier, slice 4 FR-012), otherwise 'You cannot record a day that has not begun.'"). The code: `GoalTimeline.HasBegun` is `CurrentDate(goal, instant) is { } current && date <= current`.
- **Continues**: MINOR-2 (pass 1)
- **Reasoning**: Take a goal ended on 16 Nov and restarted on 16 Nov at 11:00 with S = 23 Nov. Its latest attempt has not begun, so by the new term it "has not started". The earlier slices that the term cites give such a goal no current date. Slice 5 FR-002 then refuses every record for attempt 1's dates with "You cannot record a day that has not begun." until 23 Nov.

  That refusal contradicts FR-010 and FR-014, which require recording and correcting the ended attempt's dates to keep working. It also defeats FR-016 and FR-019: the dashboard and "what can be recorded now" offer attempt 1's Pending 14 and 15 Nov, these cannot be recorded, and they turn Missed. The gap can last up to 365 days.

  The spec also contradicts itself on this. Under the new *Goal's start date* (attempt 1's start date), slice 4 FR-012 would give the goal a current date. Under *A goal that has not started*, which cites the same FR-012, it has none. Neither reading says which date or time zone decides "day has begun" for an earlier attempt's date in this state. "Earlier requirements and tests this slice changes" does not list slice 5 FR-002. US3-AS12 avoids the case only because attempt 2 starts on the day of the restart.

### MINOR-1: US3-AS13 opens the Start new attempt page with "the start date today", against FR-011
- **Where**: spec.md US3-AS13 (line 347, "with the earlier settings filled in and the start date today"); HP-004 ("It opens with FR-011's starting values").
- **Against**: FR-011 (the start date is "the later of the account's today and the day after the ending date"); US3-AS1 (asked on the day of the ending, the start date is Tuesday 17 November).
- **Continues**: new
- **Reasoning**: The most common restart happens on the day of the ending (end, then "Start new attempt"). There the page must show tomorrow, not today, or HP-004 and FR-011 are broken. The page scenario still describes the pass-1 rule, so a UI test written from it would assert the wrong default.

### MINOR-2: US3-AS6's first refusals have no supplied time, and the time stated next to them makes one of them pass
- **Where**: spec.md US3-AS6 (lines 321–326).
- **Against**: slice 4 FR-006, restated in "Earlier behaviour these tests use" (a start date is "in the past" only relative to the account's today); "What acceptance tests can reach" ("The current time is supplied to every use case").
- **Continues**: new (the revision added a second time to this scenario)
- **Reasoning**: The first sentence gives no time. "Tuesday 17 November (in the past)" is refused only when today is 18 Nov or later. The scenario's only stated time, added in this revision, is "On Monday 16 November at 11:00". At that time 17 Nov is a valid start date (after the ending date and not in the past), so the attempt would start. Pass 1 inferred Wed 18 Nov only from the 367-day figure. A translator now has two plausible times, and one of them reverses the expected result.

### MINOR-3: Attempts that end before they begin are not handled on pages and in the dashboard rules
- **Where**: spec.md HP-001 Earlier attempts card ("Attempt {n} · {2 Nov} – {15 Nov}" (start date to last date), line 779); HP-014 heading (line 863); HP-006; FR-019 ("those of an ended goal's latest attempt, and those of a restarted goal's earlier attempt", line 677); HP-007 (the Tags "Ended" or "Attempt {n}", line 816).
- **Against**: FR-008 ("A last date before the attempt's start date means the attempt has no commitment or period"); US2-AS12; the edge case "Ending on the start date, or before it".
- **Continues**: new
- **Reasoning**: Two cases are left open:
  - **Inverted date range.** An attempt with S = 23 Nov ended on 17 Nov has the last date 16 Nov. Once it is an earlier attempt, HP-001 and HP-014 render it as "Attempt 2 · 23 Nov – 16 Nov". No rule covers an attempt with no dates.
  - **Pending entries of an ended goal's earlier attempt.** Attempt 1 is ended on 16 Nov (its 15 Nov is Pending until 18 Nov), restarted with S = 17 Nov, and attempt 2 is ended on 17 Nov. The goal is now ended and attempt 1 is neither "an ended goal's latest attempt" nor "a restarted goal's earlier attempt". FR-019 does not say whether its Pending 15 Nov stays on the dashboard, and HP-007 does not say which Tag the row gets.

### MINOR-4: ADR 0025 does not match FR-009 on version removal, and credits the new restart rule to owner choices that were not recorded
- **Where**: docs/adr/0025-goal-change-rules.md Decision ("removes excusals and the waiting settings version after it"), Status ("Owner choices … spec D-1 and D-2 (Clarifications 2026-11-22 (specify))") and the new Decision and Consequence bullets on restarting after the ending date. spec.md D-2 ("Consequence settled after the first spec review"), Clarifications, and checklist iteration 4 ("all verified and confirmed").
- **Against**: spec.md FR-009 ("every settings version that takes effect after it") and the edge case "Ending on the day a change took effect" (a version already in effect on E is removed too); constitution Decision Discipline (an answer is recorded in `## Clarifications` with its date; "Changing a recorded decision requires a new or superseding ADR"); the precedent in the roadmap row "Commitment deadlines and time zones", where critic-revision defaults are recorded as "the owner confirmed".
- **Continues**: MINOR-9 (pass 1)
- **Reasoning**: The two documents conflict in two ways:
  - **Removal scope.** The ADR says ending removes only "the waiting" version. The spec also removes a version already in effect on the ending date (the accepted MINOR-9 consequence) and, by MAJOR-1, a not-begun attempt's only version. A later slice told to "cite these rules" (ADR 0025) would read the narrower rule.
  - **Provenance.** The restart-after-ending rule rejects a real alternative ("Rejected: a new attempt on the ending date"). It now sits in an ADR whose Status credits its Decision to owner answers in Clarifications 2026-11-22 (specify). Neither the Clarifications section nor the checklist records that the owner chose it. The checklist says only "confirmed", without saying by whom.

### MINOR-5: The edge case for "today and the ending date differ" cites scenarios that never make them differ
- **Where**: spec.md Edge case "The account's today and the ending date differ" (lines 486–489, citing "US3-AS1, US3-AS6"); SC-005 ("also with the account's today a day behind the goal's timeline").
- **Against**: "What acceptance tests can reach" (dates are in UTC unless a scenario says otherwise); US3-AS1 and US3-AS6 (no time zone change, all UTC).
- **Continues**: MAJOR-1 (pass 1), as a coverage remainder
- **Reasoning**: Pass-1 MAJOR-1 came from an ending date on the goal's timeline that differs from the account's today. Both cited scenarios run in a single UTC timeline, where today and the ending date coincide. The checks hold there, but they never exercise the time-zone case that the edge case claims to cover. No acceptance scenario sets up a time zone change before or after the ending, unlike US1-AS13 for changes. So the case that motivated the new FR-012 step is pinned only by SC-005's unit and integration tests, not by the locked acceptance criteria.

## Summary
2 MAJOR, 5 MINOR. The revision resolves both pass-1 MAJOR notes and 12 of the 13 MINOR notes, and every figure I recomputed is still correct, the new US3-AS12 and the Reading version 3 included. Two new contradictions block `/speckit-ac-translate`, both in the multi-attempt and not-begun states that the revision tightened:
- FR-009's "every settings version after the last date" deletes all of a not-begun attempt's settings, which leaves US2-AS12's goal with no name, no settings and no restart values.
- The new meaning of "a goal that has not started", read through slice 5 FR-002, refuses records for an earlier attempt's dates while a future-dated new attempt waits, which contradicts FR-010, FR-014, FR-016 and FR-019.

The MINOR notes are smaller inconsistencies:
- a page scenario that still shows the old default start date;
- one scenario with two candidate supplied times;
- page and dashboard rules for attempts that end before they begin;
- ADR 0025 not matching FR-009 and not recording who chose the restart rule;
- an edge case that cites scenarios which do not exercise it.
