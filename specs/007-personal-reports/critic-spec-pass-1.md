# Critic pass 1: spec for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: specs/007-personal-reports/spec.md, specs/007-personal-reports/checklists/requirements.md

Verified first. The demo figures of FR-030 (demo4) and FR-032 (demo5) are correct for every weekday variant the
spec lists. They were recomputed from the stated records, the 48-hour grace period (the day R − 3 is final from
R 00:00, R − 2 and R − 1 are Pending), the per-week calendar (first Monday on or after the start date) and D-1, and
checked against `src/HT.Application/Operator/DemoHistory.cs` and slice 5 AC-051 to AC-056:

- Reading is 24 of 26, streak 3, longest 18. Sugar is 24 of 26, streak 1, longest 16.
- Yoga:
  - Wednesday to Sunday: 1 of 3, streak 1, longest 1.
  - Tuesday: 0 of 2, streak 0, longest 0.
  - Monday: 1 of 3, streak 0, longest 1.
- Screen time is 78 of 82, streak 27, longest 34.
- Strength training:
  - Wednesday to Sunday: 9 of 11, streak 3, longest 4.
  - Tuesday: 8 of 10, streak 2, longest 4.
  - Monday: 9 of 11, streak 2, longest 5.
- Walking, with R a Wednesday: 35 final commitments, R − 21, R − 23 and R − 26 Failed, R − 44, R − 47 and R − 49
  Missed. It reads 29 of 35, streak 7, longest 15.
- Bar counts are 14, 12 and 14.
- The goal names match `GoalText.TargetText`. The habits Walking, Strength training and Screen time are Published
  in `SeedCatalog.cs`.
- The HP-004 example strip gives 18 of 20, streak 6 and longest 11.

The notes below are the problems that remain.

## Notes

### MAJOR-1: No demo account ever shows a Missed result in the Today list or the "Missed this week" tile
- **Where**: the following parts of spec.md:
  - US8 narrative (lines 364–367): reviewers find "a Today list with Due, Pending and Missed results".
  - US8-AS4 (lines 384–386): "Missed results, when one became final this week".
  - FR-012 (lines 519–520) and the clarification "Missed this week" (lines 49–51).
  - FR-030 and FR-032 (lines 551–605).
  - SC-006 (lines 762–765).
- **Against**: constitution VI says every user-facing feature MUST extend the demo data "so a reviewer sees its
  states in the hosted app without waiting for real time to pass". This slice adds three new parts to the Home:
  - HP-003's Missed rows and their "Add note" / "Edit note" action;
  - the "Missed this week" tile;
  - US6-AS4's week boundary.
- **Reasoning**: "Missed this week" means the grace period ended at or after Monday 00:00 of R's week, and that
  Monday is never earlier than R − 6. Every Missed result in the demo data ends its grace period before it:
  - demo4:
    - Reading R − 10: grace ends R − 7 00:00.
    - Sugar R − 12: grace ends R − 9.
    - Yoga week 3: grace ends 12 days before the current Monday.
  - demo5:
    - Screen time R − 50: grace ends R − 47.
    - Walking R − 44 to R − 49: grace ends R − 41 or earlier.
    - Strength training week 7: weeks earlier.

  So on every weekday, both demo accounts show "Missed this week" as 0, and their Today list has no Missed row.
  The US8 narrative is false as written. US8-AS4's "when one became final this week" clause is never true, so the
  acceptance criteria cannot check it. On the hosted demo, the Missed row, its note action and a non-zero
  "Missed this week" tile cannot be shown. The spec states this coverage but does not deliver it, and it
  acknowledges no exception to constitution VI.

### MINOR-1: FR-006's "no value" rule conflicts with slice 5 progress for periods, and weeks are left undefined
- **Where**: spec.md FR-006 (lines 473–487): per-week bars use "the period's progress (slice 5 FR-014)", per-day
  bars use "the day's progress", and "A commitment or day without a record has no value (not 0)".
- **Against**: slice 5 progress as built. `src/HT.Domain/Evaluation/Progress.cs` says: "Value is null only for a
  commitment without a record; a period without records has 0." Also slice 5 FR-013: nothing recorded is never a
  success for Reduce.
- **Reasoning**:
  - **Per-day bars.** These are defined as the day's progress, which is 0 for an unrecorded day. FR-006 says they
    have no value instead, so the two sentences of the same requirement disagree.
  - **Per-week bars.** FR-006 says nothing about an unrecorded week. By "the period's progress" it gets 0, which
    breaks the rule just applied to days. Unrecorded weeks in the demo data: demo4 Yoga week 3 and its current
    week, demo5 week 7 and its current week.
  - **Reduce per-week count or duration goals.** A bar of 0 against the limit reads as within the limit, the
    reading FR-013 rules out.
  - **Commitment-based yes/no weeks.** It is not stated whether a week with no record is 0 or has no value. It is
    also not stated whether its target counts Upcoming commitments of the current week and commitments before the
    start date.

  Acceptance criteria for trend values cannot be drafted without an answer to these points.

### MINOR-2: The value of a Reduce yes/no bar counts "No" for one goal kind and "Yes" for the others
- **Where**: spec.md FR-006 (lines 477–483).
- **Against**: the same requirement's "the trend shows amounts, not verdicts" (line 487), and slice 5 FR-014
  ("a Reduce yes/no period's number of 'Yes' records against N").
- **Reasoning**:
  - **Commitment-based Reduce yes/no weeks:** the bar counts the commitments recorded "No", that is the successes,
    against the number of commitments.
  - **Per-week and per-day Reduce yes/no goals:** the bar is the progress, which counts "Yes" against the limit N.

  The same measurement gives bars of opposite meaning depending on the schedule kind. Counting "No" for Reduce is
  a verdict, not an amount. No acceptance scenario covers the commitment-based Reduce yes/no bar.

### MINOR-3: The trend target is in minutes in the FR and in hours on the page
- **Where**: the following parts of spec.md:
  - HP-001 (lines 615–617): the dashed target line "Target {n} h" for a duration goal in hours.
  - FR-006 (line 486): durations in minutes, a goal in hours is the amount × 60.
  - Edge Cases (line 424): "trend values and the target are in minutes (a target of 1 h is 60)".
- **Reasoning**: on the page, the bars would be in minutes and the target label in hours, or the page would have
  to convert the values itself. That contradicts HP-005's aim that pages never build figures. The two statements
  conflict and do not say which one governs.

### MINOR-4: The Today list and the recent notes have no tie-break, and demo4 has exact ties
- **Where**: spec.md FR-012 (lines 518–531):
  - Pending: "the one whose grace period ends soonest first".
  - Missed: "the latest grace end first".
  - Recent notes: "with the latest first dates, newest first".
- **Against**: FR-012 gives a tie-break to Due rows ("by goal in the order of FR-011") and to Upcoming rows
  ("then in the order of FR-011"). The spec's testing rule checks lists "by … order" (line 146).
- **Reasoning**: demo4's Reading and Sugar both have Pending results on R − 2 and on R − 1. Each pair ends its
  grace period at the same instant (R + 1 00:00 and R + 2 00:00). Two goals' notes on the same date tie in the same
  way. Neither the order of US6-AS2 nor that of the demo dashboard is defined, so an acceptance test of the order
  would depend on the implementation.

### MINOR-5: The clarification on "Missed this week" leaves out Friday, which its own rule includes
- **Where**: spec.md Clarifications (clarify), lines 49–51: grace period ended "at or after Monday 00:00 … The
  previous week's Saturday, Sunday and per-week period therefore appear early in the week".
- **Against**: slice 5 FR-016: the grace period is exactly 48 h and the result is final "that instant included".
- **Reasoning**: Friday's commitment ends at Saturday 00:00, so its grace period ends at exactly Monday 00:00
  (with no clock change). That is "at or after Monday 00:00", so the previous Friday also counts as Missed this
  week. The "therefore" list can lead to a wrong boundary test, at exactly the boundary that constitution II
  requires tests on both sides of. Around a clock change, Friday's grace end moves to Sunday 23:00 or Monday 01:00,
  and the clarification does not mention that either.

### MINOR-6: SC-006 says the smoke check confirms figures that HP-008 never checks
- **Where**: spec.md SC-006 (lines 762–765) and HP-008 (lines 666–670).
- **Reasoning**: SC-006 says "the smoke check confirms" four things:
  - a non-zero current streak;
  - a longest streak above 10;
  - a Today list with Due and Pending results;
  - every demo5 trend "with its full number of bars (14 or 12)".

  HP-008 checks only these texts:
  - for demo4: the word "Consistency" and the unit "commitments" on Reading's page, and "Due today" on the Home;
  - for demo5: the goal names on its Goals page and the unit "weeks" on Strength training's page.

  "Due today" is a tile label that is always shown, so it does not prove a Due row exists. None of SC-006's
  conditions is checked by the requirement that is meant to verify it.

### MINOR-7: SC-007 cannot be measured where it says it is measured
- **Where**: spec.md SC-007 (lines 766–767): "a goal with a year of daily results in under 2 seconds on the
  hosted demo". ADR 0020's Consequences repeat that SC-007 "checks it on the hosted demo".
- **Against**: on the hosted demo:
  - slice 4: a goal's start date cannot be in the past;
  - the oldest demo goal starts 84 days back (FR-032);
  - the demo is refreshed daily (slice 5 HP-009).
- **Reasoning**: the hosted demo never has a goal with a year of results, so the success criterion cannot be run
  as written. The checklist still marks "Success criteria are measurable" as passing.

### MINOR-8: Existing tests that break are not acknowledged, and one log text is not given
- **Where**: spec.md "Slice 4 and 5 requirements and tests this slice changes" (lines 689–707) and HP-011
  (lines 681–687).
- **Against**: the following tests:
  - `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs:67-81` asserts that "Example goal", `.ht-card`,
    `.ht-stats` and `.ht-strip` are absent from the visitor Home (slice 2 US5-AS8). HP-004 shows that card to
    every visitor.
  - `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs:299` asserts that the GoalListItem has no
    `ht-strip`, "Record" or "%".
  - `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs:117-120` expects exactly two "Demo " lines when there is no
    password: "Demo loading: …" and "Demo history loading: …".
- **Reasoning**:
  - The visitor Home row names no test, and the two UI tests above break without being listed.
  - The section only says the startup tests change "to expect demo5@example.com's lines after demo4@example.com's",
    so the cases with no password and with a weak password are left out.
  - HP-011 never gives the text of demo report loading's refusal line. Slice 5 fixed "Demo history loading: …"
    for its task. The startup tests and the success check therefore have no text to expect.
  - It is also not stated whether slice 2 US5-AS8 ("with no Published habit nothing else from the preview is
    shown") is amended now that the card is always shown.

### MINOR-9: "Due today" counts a per-week period that is due this week
- **Where**: spec.md US6-AS3 (lines 320–321): a per-week Due week "is counted among the Due results". HP-003
  (lines 631–633): the tile "Due today" shows that count.
- **Against**: slice 5 HP-004: a per-week Due period is "Due this week", not "Due today". Constitution I says
  labels must use the defined terms precisely.
- **Reasoning**: every user with a per-week goal sees "Due today" count that goal's week on every day of it. The
  label and the counted state disagree, and the spec does not acknowledge this or choose between them.

### MINOR-10: CONTEXT.md was not changed with the decision that sharpens Pending, and the spec sets no time for it
- **Where**: spec.md D-1 (lines 25–28) and HP-010 (lines 675–680). Commits 280b6b9 and f2377ec change only the
  spec, the checklist, the roadmap and ADRs 0020 and 0021.
- **Against**: constitution I says a change that "sharpens the meaning of an existing one, MUST add or update its
  `CONTEXT.md` entry … in the same change". `CONTEXT.md:60` still reads "never resets a streak", and
  `CONTEXT.md:41` lists demo1 to demo4 only.
- **Reasoning**: D-1 is recorded as decided in the spec and the roadmap, but the glossary still has the old
  meaning. Slice 5 HP-012 stated when its glossary change would land ("in the change that revises this spec after
  its critic review"). HP-010 states no time, so the deviation is neither scheduled nor acknowledged.

### MINOR-11: The checklist and the decisions summary predate the clarify session
- **Where**: the following places:
  - checklists/requirements.md, Notes (lines 33–45): only the two iterations of `/speckit-specify` and "FR-030's
    demo figures follow D-1";
  - spec.md "Decisions due before acceptance criteria" (lines 772–773): "this spec added two of its own. All four
    were answered".
- **Reasoning**:
  - The checklist says nothing about the five clarify answers, D-5, ADR 0021, demo report loading or FR-032. Its
    items were not re-checked for that content, for example the measurability of SC-007.
  - The decisions section goes on to list D-1 to D-5 and R3, so "two of its own" and "all four" are now wrong.

## Summary

1 MAJOR and 11 MINOR notes.

The core rules of the spec are internally sound: consistency, its rounding, the strict streaks with D-1, the
report range and privacy. Every stated demo figure for demo4 and demo5 recomputes correctly.

The blocking issue is demo coverage. Neither demo account can ever show a Missed result in the Today list or a
non-zero "Missed this week", although the spec says reviewers will see them and constitution VI requires them.

Before `/speckit-ac-translate`, these points should also be settled:
- the trend-value rules of FR-006 (an unrecorded period, and a Reduce yes/no bar);
- the order of tied Pending, Missed and recent-note entries;
- whether the "Missed this week" boundary includes the previous Friday.

Without them, the acceptance criteria for those scenarios would have to guess.
