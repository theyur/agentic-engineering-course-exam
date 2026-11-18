# Critic pass 1: spec for 005-recording-and-evaluation

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: specs/005-recording-and-evaluation/spec.md, specs/005-recording-and-evaluation/checklists/requirements.md

## Notes

### MAJOR-1: Adding `demo4@example.com` breaks the locked F001 acceptance criteria, and the spec does not acknowledge it
- **Where**: spec.md FR-030 (l. 493-498), FR-019 (l. 487-489), D-6 (l. 645-649), "Slice 4 requirements and tests this slice changes" (l. 567-578).
- **Against**: slice 1 FR-016 (specs/001-accounts-and-access/spec.md l. 522-527), which names exactly the three demo emails; locked F001 AC-057, AC-058, AC-059 and AC-060 (specs/001-accounts-and-access/acceptance.md l. 630-664; tests/HT.AcceptanceTests/Features/F001/accounts-and-access.feature l. 496, 505, 515, 526). Each says demo loading's result is "created" or "already exists" for demo1 to demo3 "and names no other email". Existing tests that pin three demo emails are affected too: tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs:18, :76 (`Assert.Equal(3, _store.Accounts.Count)`) and :152; tests/HT.UnitTests/Application/Operator/SeedCatalogTests.cs:130 (every demo email has 2 demo custom habits, but FR-030 gives demo4 none); tests/HT.UiTests/Host/StartupLogTests.cs:44; tests/HT.UiTests/Operator/StartupTasksDemoTests.cs:19.
- **Reasoning**: FR-030 makes demo loading return an account result for a fourth email. That contradicts the literal text of four locked F001 criteria, which forbid any other email. It does not depend on how the step definitions are written. FR-019 promises that F001 passes unchanged. D-6 names only a possible F004 step-definition risk, and it says F004 "is not unlocked" because of the locked demo1 to demo3 lists, but it overlooks the F001 lock, which this design breaks for certain. The "requirements and tests this slice changes" section does not list slice 1 FR-016, F001, or the unit and host tests above. As written, FR-019 and FR-030 cannot both hold. The design also needs an F001 unlock that the spec never mentions, and the AC workflow (CLAUDE.md, "Acceptance tests") allows that only when the owner asks for it.

### MAJOR-2: The technical rows D-4 and D-5 are open and unmarked, have no consistent deadline, and FR-030 already assumes D-5's answer
- **Where**: spec.md HP-009 (l. 554-556: "decided in the plan"), HP-010 (l. 557-558: "decided in the plan"), "Decisions due before acceptance criteria" D-4 and D-5 (l. 637-642: decided "before `/speckit-ac-translate`"), FR-030 (l. 493-517), US7-AS3 (l. 348-349); checklists/requirements.md l. 16 and l. 46-48.
- **Against**: constitution, Decision Discipline ("A spec that touches ... an ARCHITECTURE §9 deferred choice MUST mark it `[NEEDS CLARIFICATION: ...]`"); ARCHITECTURE §9 ("scheduled evaluation mechanics, background processing"); the CLAUDE.md flow (clarify → ac-translate → plan); docs/roadmap.md "Decisions each slice must settle" (technical rows are decided with the owner before `/speckit-ac-translate`, so that `research.md` does not choose them); ADR 0007 l. 51, ADR 0013 l. 48, ADR 0015 l. 129.
- **Reasoning**: In this repo the plan runs after `/speckit-ac-translate`. HP-009 and HP-010 ("decided in the plan") and the checklist note ("left to the plan ... before `/speckit-ac-translate`") give a deadline that falls after the step it is supposed to come before, and that contradicts the spec's own Decisions section. Both rows touch ARCHITECTURE §9 or ARCH §7 items, yet the spec carries no marker, and the checklist ticks "No [NEEDS CLARIFICATION] markers remain". D-5 also decides behaviour on the AC surface: FR-030 fixes the demo data relative to the run date and writes it only once, with the account; US7-AS3 says a re-run leaves records unchanged. A D-5 answer such as re-basing records at each start, or a scheduled refresh that changes what demo loading does, would change FR-030 and US7. Acceptance criteria drafted now would show spec drift as soon as D-5 is decided.

### MAJOR-3: Yoga's demo records are specified for the current week and future weeks, and "week 1" is ambiguous
- **Where**: spec.md FR-030, Yoga bullet (l. 506-510): "every other week of the goal has 'Yes' on its Monday, Wednesday and Friday"; "the most recent week that ended on or before R (week 1)".
- **Against**: FR-002 (l. 391-395: no record for a day that has not begun); FR-030's own Reading and Sugar bullets, which stop at bounded dates (R − 3 and R); US7-AS2 (l. 346-347).
- **Reasoning**: The goal has no end, so "every other week of the goal" includes the week that contains R and every later week. Read literally, demo loading writes "Yes" records on dates that have not begun. If R is a Monday, those are that week's Wednesday and Friday and every week after it. That is data FR-002 forbids, and it would make the current week's progress and state wrong. The week-1 definition is also unclear when R is a Sunday. The week whose last date is R ends at the instant after R, so it is unclear whether that week "ended on or before R" (date-wise) or the week before is meant. The two readings move weeks 1 to 3 by seven days and change which week is Pending, Failed or Missed at the run time. Acceptance criteria for FR-030 cannot be pinned down until both points are settled.

### MAJOR-4: A record saved without a note may delete the commitment's or period's note
- **Where**: spec.md FR-007 (l. 418-423): "empty after trimming means no note, and saving it removes an existing note ... A note can be saved with a record or on its own"; FR-005 step (6) (l. 409); HP-002 "Note (optional)" (l. 535).
- **Against**: D-9 / Clarifications (l. 45-47: one note per commitment or period, not per record); US4-AS1 and US4-AS4.
- **Reasoning**: A period holds up to 50 records but only one note. The spec does not say whether a record call with no note, or with an empty one, leaves the existing note alone or counts as "saving an empty note" that removes it. Read literally, recording Wednesday's "Yes" in a week whose Monday record carried "Knee was sore." would silently delete that note. This happens whenever the RecordDialog's optional note field is left empty. The demo data (a week note plus several records in that week) and the AC scenarios both depend on the answer, and the spec gives none. One reasonable reading loses user data without warning.

### MAJOR-5: The goal page cannot add a record to a final period that already has records, so US3-AS4 cannot be done in the UI
- **Where**: spec.md HP-001 (l. 525-527: one action per row, "Add record" only for an unrecorded Pending or Missed row, "Edit" for a row with a record); HP-002 (l. 531-532, 536-537: the "For" select offers only Due and Pending; "Edit" opens the dialog "for that record"); US3-AS4 (l. 240-241).
- **Against**: SPEC §5.2 ("Users can correct finalized historical records"); FR-003 (a period holds several records, and recording adds one); design/components/RecordDialog.md ("Finalized dates are corrected from the goal page History table").
- **Reasoning**: A final Failed week with 2 of 3 "Yes" records, or a final Failed "2 times a day" day with one record, shows only "Edit" in its History row. The RecordDialog's "For" excludes final periods. So no page path adds the missing record that US3-AS4 describes, and the only correction left is to change an existing record. That is not the same fact: it turns a "No" into a "Yes" instead of adding a session. "Edit ... for that record" is also undefined for a period row that holds several records, because the row does not say which record it opens. The UI therefore cannot do a SPEC §5.2 correction that FR-003 allows.

### MINOR-1: The smoke check's "Missed" on the Reading page depends on a detail the spec does not state
- **Where**: spec.md HP-008 (l. 550-553), HP-001 (l. 526-528: newest first, the first 10 rows), FR-030 Reading bullet (l. 499-502).
- **Against**: FR-008 (l. 424-426); design/pages/PageGoalDetail.html l. 46-48 (the Recent notes meta shows the state for "Excused" but not for "Wed 28 Oct").
- **Reasoning**: At the run time, the first 10 History rows of "Reading, 20 min daily" are R (Due), R − 1 and R − 2 (Pending), and R − 3 to R − 9 (Met, with R − 6 Failed). The only Missed commitment, R − 10, is the 11th row. "Missed" then appears on the page only if the Recent notes card shows the state of R − 10's note. HP-001 does not require that, and the design shows the state only on some entries. HP-008, and so SC-005's smoke-check confirmation, can fail right after a deployment.

### MINOR-2: The order of the date messages is undefined, and US1-AS6 contradicts FR-002
- **Where**: spec.md US1-AS6 (l. 165-166: "Given any goal ... records for tomorrow ... 'You cannot record a day that has not begun.'"); FR-002 (l. 391-395); FR-005 step (4) (l. 407); FR-018 (l. 481-483).
- **Against**: US1-AS5 (l. 162-164).
- **Reasoning**: FR-005 treats "the date (FR-002)" as one step with two messages, in no stated order. For a Mon/Wed/Fri goal whose tomorrow is a Tuesday, or a goal whose start date is tomorrow or later, both messages apply. "Any goal" in US1-AS6 implies one precedence, while FR-002 lists "Nothing is scheduled on this date." first. FR-018's order also leaves out where the history range check ("Choose a range of 1 to 366 days.") falls. Slice 4 FR-023 stated that position explicitly.

### MINOR-3: The note's date rule for per-week periods contradicts the FR-002 check it cites
- **Where**: spec.md FR-007 (l. 421-422): "for any commitment or period whose first day has begun (FR-002 date checks) ... any date in the week names the period".
- **Against**: FR-002 (l. 393-395: the given date's own day must have begun).
- **Reasoning**: A note for the current week given with Friday's date, while today is Wednesday, passes "first day has begun" but fails FR-002's check on the given date. The cited check and the stated rule give different results.

### MINOR-4: "The Due commitment" is singular, and the RecordDialog's "today" differs from FR-011
- **Where**: spec.md FR-011 (l. 446-447), US5-AS3 (l. 299-300), HP-002 (l. 531-532: "today and every Pending commitment or period").
- **Against**: slice 4 FR-027 (after a move east, the old and new days overlap, so two commitments can both be in progress); the spec's own Edge Cases (l. 358-362); FR-002.
- **Reasoning**: During an overlap, two commitments can be Due at once, but FR-011 returns "the" Due commitment. HP-002 offers "today" in "For" even on a day with no commitment (an unscheduled day of a fixed-days goal), and FR-002 refuses that date. FR-011 returns periods, but HP-002 asks for the dates of a Pending week.

### MINOR-5: demo4's result set and log lines are underspecified
- **Where**: spec.md FR-030 (l. 495-496, 512-513), HP-007 (l. 548-549).
- **Against**: slice 4 HP-013 (each account line, custom-habit line, then goal line); src/HT.Application/Operator/DemoAccountResult.cs `Created` (always sets `CustomHabits = DemoCustomHabitsResult.Added`).
- **Reasoning**: demo4 has no custom habits, yet the current result shape and HP-007's "as the other demo accounts' lines" give it a "demo custom habits added" line. FR-030 lists only "created", "demo goals added" and "demo records added". The spec also does not say what "demo records added" means when all three goals are left out because their habits are not Published, as in F004 AC-066's catalog setup.

### MINOR-6: HP-005 gives the wrong place and wording for the partial-amount line
- **Where**: spec.md HP-005 (l. 544-545), "Design changes before build" (l. 592).
- **Against**: design/pages/PageNewGoal.html l. 26 ("You can record partial amounts; the target decides the result." as the help line of "How do you measure it?"); design/pages/PageNewGoal.md l. 29; constitution IV (pages follow the wording of their previews).
- **Reasoning**: The design puts the sentence, with a second clause, in the form section, not on the summary card. HP-005 requires a shorter sentence on the summary card.

### MINOR-7: Upcoming is labelled "Due", which clashes with FR-010's states
- **Where**: spec.md HP-004 (l. 541-543: "'Due today' or 'Due <day>' for Due and Upcoming").
- **Against**: FR-010 (l. 437-442: Upcoming and Due are separate states); constitution I (a wrong word is a wrong rule); design/components/StatusChip.md.
- **Reasoning**: Showing an Upcoming commitment as "Due Mon" uses the word of one state for another. For a per-week period that is Due for seven days, "Due today" is also unclear.

### MINOR-8: The glossary plan leaves a conflicting Missed entry and an unlisted new term
- **Where**: spec.md HP-012 (l. 562-565), Key Entities "Evaluation state" (l. 601-602), US3-AS1 and US3-AS3 (l. 234-239).
- **Against**: CONTEXT.md Missed ("no activity was recorded by the end of the 48-hour grace period") and Catalog habit state ("NOT a goal's status"); constitution I (an entry that is sharpened or added must be updated in the same change).
- **Reasoning**: Under FR-006 and FR-010 the state comes from the records as they are now. A Missed commitment corrected later is Met, and a final commitment whose record is deleted becomes Missed. Both contradict a Missed definition tied to "by the end of the grace period", yet HP-012 keeps that entry and adds only the Reduce point. "Evaluation state" is a new domain term, and HP-012 does not list it, although CONTEXT.md calls these values a "status".

### MINOR-9: The checklist is stale, and SC-006 is narrower than the privacy rule
- **Where**: checklists/requirements.md l. 49 ("Spec defaults D-6 to D-9 ... are listed for `/speckit-clarify`"), with no iteration for the clarify session; spec.md SC-006 (l. 622-623).
- **Against**: spec.md Clarifications (l. 35-49: D-6 to D-9 were decided in clarify); HP-006 (l. 546-547); constitution III (logs must not contain notes, custom-habit names or recorded amounts).
- **Reasoning**: The checklist does not reflect the clarify session. SC-006 checks only notes, recorded values and goal names, and leaves out the units and habit names that HP-006 and constitution III cover.

## Summary
5 MAJOR, 9 MINOR. The evaluation core is consistent and checks out against the code and the slice 4 rules: the Kyiv clock-change example, the target rules per schedule kind, the target texts, and the Reading and Sugar demo arithmetic. The spec is not ready for `/speckit-ac-translate`, for these reasons:
- The demo4 design breaks four locked F001 criteria, and the spec claims they stay green.
- The open D-5 decision can still change FR-030, and the spec gives no consistent point at which D-4 and D-5 are decided.
- Yoga's demo records reach into dates that have not begun.
- Saving a record without a note may delete a period's note.
- The goal page cannot do one of the corrections the spec requires.

The minor notes are about precision: message order, wording that differs from the design, and glossary upkeep.
