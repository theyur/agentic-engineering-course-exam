# Critic pass 2: tasks for 005-recording-and-evaluation

**Verdict**: REVISE
**Date**: 2026-11-19
**Reviewed**: specs/005-recording-and-evaluation/tasks.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (Phase 2 checkpoint fails with a `RecordingService` that has no public use case) | Resolved | Organization (lines 31-33) and T029 (line 276) no longer add `RecordingService` in Phase 2. T042 (lines 343-355) creates it with `RecordAsync`, its registrations and its `UseCaseParameterTests` row in one task. Its explicit constructor and fields are all read by `RecordAsync`, so neither CS9113 nor `Assert.NotEmpty` can fail. The Phase 2 checkpoint (line 304) says "No `RecordingService` exists yet". |
| MAJOR-2 (live progress has no source that respects the layer rules) | Partly | The layer problem is solved: `Evaluator.ProgressOf` (T015, line 220), `GoalHistory.Target` (T022) and `RecordPreview.Progress` (T028) move the counting into Domain and Application. The new preview gives wrong results for a commitment opened from "Record", see MAJOR-1. |
| MINOR-1 (entry range with no entries) | Resolved | T043 (line 356) and T053 (line 400) give `Found` with no entries and no store read. T040 (line 332) and T051 (line 395) test it, the per-week first partial week included. |
| MINOR-2 (dialog modes not assigned) | Resolved | T063 (line 464) adds the row mode. T073 (line 498) opens the NoteDialog with the existing note. The shared-files list (line 605) has T045 and T063 for `RecordDialog.razor`. |
| MINOR-3 (RecentNotesCard StatusChip inputs) | Resolved | T074 (line 499) passes `ScheduleKind` and `Period` from `GoalDetailBody`'s `GoalHistory`. T076 tests "Due this week". |
| MINOR-4 (`RecordingText` texts undefined; smoke check cannot reach the formatter) | Partly | T004 (lines 173-177) now settles the per-week "For" label, the yes/no commitment progress, the Reduce yes/no period text, the partial sentence and the sub line. T024 adds `Amount`, which T090 calls. The sub-line rule contradicts its own example (MINOR-2), and other dialog texts are still undefined (MINOR-3). |
| MINOR-5 (History card differs from the design) | Resolved in tasks.md | T003 (line 159), T054 (line 401) and T056 (line 403) follow `design/pages/PageGoalDetail.html` lines 33-35: the sentence in the card head, a fifth actions column, and a raw `section.ht-card` instead of `HtCard`. The rationale is sound: constitution IV says "Pages MUST follow the structure and wording of their previews". contracts/host.md and research R-12 still say otherwise (MINOR-1). |
| MINOR-6 ("Within limit" misses `tokens.json`) | Resolved | T005 (line 179) includes `design/tokens.json`. `grep -rln "Within limit" design src` lists only the five files T005 names. |
| MINOR-7 (SC-002 grace boundaries per combination) | Resolved | T017 (line 234) runs the seven state instants for every T016 combination, with no record, a record below the target and one at it. |
| MINOR-8 (HP-006 for demo history loading) | Resolved | T079 (line 519) is scoped to `RecordingService` and `IRecordStore`. T083 (line 539) adds `LogCapture` and the exception-message check for the forced rollback. |
| MINOR-9 (edge cases without tests) | Resolved | The Unpublished habit is in T041 (line 339). The move east and two Pending commitments are in T058 (line 426). The Sunday/Monday case is now tested at `RecordAsync` in T039 (line 329), and T019 tests dates only. |
| MINOR-10 (smoke-check contract and host test updated only in part) | Partly | T091 (line 559) now rewrites the whole slice 3 contract: check 6, "checks 2–6", both ports, the Port column. The forbidden-text list it adds is miscounted and incomplete (MINOR-8). |
| MINOR-11 (constructor convention conflicts with T090) | Resolved | Conventions (lines 108-111) scope the rule to `HT.Domain`, `HT.Application` and `HT.Persistence` and exempt `tools/HT.SmokeCheck`. T090 (line 553) calls `CoverageEntry` the top-level record, which matches `tools/HT.SmokeCheck/SmokeCheckCoverage.cs` line 15. |
| MINOR-12 (T093 asks for roadmap edits already made) | Resolved | T093 (line 576) now says to check the roadmap rows and change nothing unless the implementation differs. |

No older note has regressed.

## Notes

### MAJOR-1: The RecordDialog's preview adds the new value as a second record on a commitment opened from "Record"

- **Where**:
  - tasks.md T028 (line 275): `RecordPreview.Progress(history, entry, RecordId? replacing, answer, amount)` "rebuilds the entry's other records … (leaving out `replacing`, the record being edited), adds the candidate".
  - T045 (line 360): from "Record", the preview is computed "for the value being entered", and no `replacing` is passed. Only T063's "Edit" mode (line 465) passes one.
  - T015 (lines 218 and 220): "a commitment-based target with more than one record is a programming error and throws", and `ProgressOf` "gives the same `Progress`".
  - T028's test list: "a commitment" (one case only, with no existing record).
- **Against**:
  - FR-003: "recording for a commitment that has one replaces its value".
  - US1-AS2: 25 minutes replaces today's 12.
  - FR-011 and HP-002: "For" lists every Due and Pending entry, whether or not it has a record.
  - T042 (line 350): `RecordAsync` replaces in place through `AddAsync`.
  - HP-002: "the progress under the field".
- **Continues**: MAJOR-2 of pass 1.
- **Reasoning**: Take a user who opens "Record" for today on "Reading, 20 min daily" after already saving 12 minutes. The selected `HistoryEntry` has one record, `replacing` is null, and the candidate is 25. `RecordPreview` then hands two values for a commitment-based target to `Evaluator.ProgressOf`:
  - if `ProgressOf` shares `Evaluate`'s rule, it throws, and the exception surfaces in the interactive component on every keystroke;
  - if it doesn't, `RecordedValue` with two values is undefined, and the dialog shows something other than "25 of 20 min".

  The save itself replaces the record (FR-003), so the preview contradicts what is saved, on the core path of US1-AS2. No task or test covers a commitment with a record opened from the page head, and no rule says that a commitment's existing record counts as `replacing` there.

### MINOR-1: Decisions settled in tasks.md leave the design artifacts contradicting it

- **Where**:
  - New public members: tasks.md T015 (`Evaluator.ProgressOf`, public), T022 (`GoalHistory.Target`), T028 (`RecordPreview`), T024 (`RecordingText.Amount`, `TargetLine`, `PartialNote`).
  - T054: the History card sentence in the card head and a fifth column.
  - Notes (lines 717-727): these were settled "within the design artifacts".
- **Against**:
  - data-model.md line 5: "Names are the contract; members not listed are private". Its `Evaluator` row lists only `Evaluate`, and its `GoalHistory` row (line 57) has no `Target`.
  - contracts/application-api.md: "This is the public API that `HT.Ui`, the integration tests and the acceptance tests call". It doesn't list them.
  - contracts/host.md lines 26 and 47 ("a DataTable with Date, Recorded, Result and Note", "Under the table: …"), research R-12 line 497 (same), and contracts/host.md line 150 (`SmokeCheckCoverage.CoverageEntry`, a nested name that doesn't exist).
  - The tasks.md Conventions (line 57): "Use the names in contracts/application-api.md and data-model.md".
  - Constitution Governance: conflicts are resolved "by amending the right document, never by silent divergence".
- **Continues**: new. It is related to the rationale behind pass-1 MAJOR-2 and MINOR-5.
- **Reasoning**: The design decisions are sound, but no task updates data-model.md, the contracts or research.md. After implementation, the public surface of `HT.Domain` and `HT.Application` holds members that data-model.md says are private. The host contract also describes a History card and a coverage type name that the code won't have. The AC test writer and later slices read these documents as the contract. The tasks contradict them and record the divergence only in their own Notes.

### MINOR-2: The sub-line rule T004 settles cannot produce its own example

- **Where**: tasks.md T004 (line 175): the sub line is "'Build' or 'Reduce', ' · ', and slice 4's target text (`GoalDetails.TargetText`), as 'Build · at least 20 min · every day' shows it". T024 `TargetLine(GoalHistory, string targetText)` (line 267) takes the texts "as T004 words them".
- **Against**: `GoalDetails.TargetText` is `GoalText.TargetText`. For Reading it gives "20 min daily": `TargetTextParts.Text` joins "daily" with a space, and the goal name "Reading, 20 min daily" is the habit name plus `TargetText` (`src/HT.Domain/Goals/GoalText.cs` lines 19-31 and 56-60). The "at least" prefix is added only in `GoalSummaryText.Sentence`. `design/components/RecordDialog.html` shows "Build · at least 20 min · every day".
- **Continues**: MINOR-4 of pass 1.
- **Reasoning**: The rule gives "Build · 20 min daily", and the design example has a different structure: "at least", and " · every day" as a separate schedule part. The design step is told to settle the text, and T024 to copy it "as written", but the instruction holds two answers. Whichever one T003 or T004 picks, the other part of the instruction is broken, and T025 has no fixed expected value.

### MINOR-3: Several RecordDialog texts and inputs are still assigned to no task or formatter

- **Where**: tasks.md T004 (lines 173-177), T024, T044 (line 357) and T045 (lines 358-366).
- **Against**:
  - `design/components/RecordDialog.html` and `RecordForms.html`:
    - the dialog title "Record · Reading" / "Record · Yoga" / "Record · Coffee" (the `aria-labelledby` heading);
    - the field labels "Time spent" and "Cups today";
    - the progress bar `ht-progress` with its width;
    - the Reduce help "At most 2. Enter 0 …", built from the goal's amount.
  - Constitution IV ("Pages MUST follow the structure and wording of their previews").
  - HP-002: "Pending, N h left" counts from "the supplied time".
- **Continues**: MINOR-4 of pass 1.
- **Reasoning**:
  - No task says what the RecordDialog's title is, whether it names the habit or the goal (the NoteDialog's names the goal, HP-003), or what the count and duration field labels are. "Cups today" also reads wrong for a Pending day or a week.
  - The Reduce help's "At most N" has no formatter. For a Reduce yes/no commitment ("avoided every day") there is no N at all, and T004 only says "with 'No' in place of 0".
  - `ForLabel` takes `now` (T024), but `GetRecordableAsync`'s `GoalHistory` carries no instant, and T045 doesn't say where the component gets the time for "N h left". It would have to read a clock in the component that is separate from the use case's.

  An implementer has to invent these texts and data flows, and T049 can't assert them.

### MINOR-4: The count-unit plural rule is undefined

- **Where**: tasks.md T024 (line 264): `Amount(Measurement, string? countUnit, int amount)` gives "1 sweet", "3 sweets". T025 (line 269): "singular and plural units". T024 `Progress`: "1 of at most 2 cups", "0 of at most 1 sweet".
- **Against**:
  - `GoalTarget.CountUnit`: "The trimmed unit for count, with its case and spelling unchanged".
  - `GoalText.AmountText`: "the unit as stored ('20 pages', '1 sweet')".
  - No plural logic exists in `src/HT.Domain` or `src/HT.Application`.
  - Units are typed by users: the demo stores "sweet" for Sugar and "cups" for Coffee.
- **Continues**: new.
- **Reasoning**: "3 sweets" from the stored unit "sweet" needs a plural rule, while the progress examples use the unit as stored ("of at most 2 cups"). For a unit stored in the plural ("cups"), the rule must also say what 1 reads ("1 cups"?), and what 0 reads ("0 sweet" or "0 sweets", which Sugar's demo history records on every even day). Nothing defines the rule, so T024 and T025 can't agree on expected values.

### MINOR-5: A note given with a change of record is neither specified for the store nor tested

- **Where**: tasks.md T033 (lines 288-290): only `AddAsync` says "a note with text is upserted under `entry.Key`". `ChangeAsync` is described only by its `WHERE` clause and `RecordNotFound`. T062 (line 459) tests change and delete without a note. T061 (line 457) checks only that a change "checks the note after the value".
- **Against**:
  - FR-007: "A note can be saved on its own, or given with a record (recording or changing one)".
  - contracts/application-api.md line 55: "Change … The note follows the record rule above".
  - data-model.md: `ChangeAsync(…, RecordValue value, NoteText? note)`.
- **Continues**: new.
- **Reasoning**: The `RecordStore` task never says that `ChangeAsync` upserts a note with text and leaves the note alone without one, and no unit or integration test changes a record with a note. A `ChangeAsync` that ignores its note parameter would pass every planned test. The rule would then be checked only by the acceptance tests the owner runs later.

### MINOR-6: The slice-5 goal page's column layout is unspecified, and T003 edits the full preview it says stays whole

- **Where**:
  - tasks.md T003 (lines 157-162): "slice 4's tags, Notice and Settings card, unchanged"; the History card; "the Recent notes card in the right column".
  - T046 (line 367): "Move slice 4's goal-page markup … into it unchanged".
  - T055 (line 402): "render the HistoryCard", with no position.
  - T075 (line 500): Recent notes "in the right column".
  - T003 line 160: "The full preview's 'Record' on the Due row becomes 'Add record'". The design preamble (lines 152-154) says "The full previews stay whole …; it never deletes anything from the full preview", and limits the in-place replacements to texts that disagree with FR-010, HP-004 or `CONTEXT.md`.
- **Against**:
  - `design/pages/PageGoalDetail.html` lines 25-48: the left `ht-stack` holds the History card, and the right `ht-stack` holds Settings and Recent notes.
  - `src/HT.Ui/Components/Pages/GoalDetail.razor`: slice 4 puts the Settings card alone in `ht-grid-2`, which is its first (left) cell.
  - Constitution IV.
- **Continues**: new.
- **Reasoning**:
  - Moving slice 4's markup "unchanged" keeps Settings in the left column. The design puts Settings in the right column with Recent notes, and History in the left. No task says which layout the slice-5 page has, so T003's preview and T046/T055's page can differ, and T050's slice-4 assertions don't decide it.
  - The "Record" to "Add record" edit is not one of the replacement kinds the preamble allows, so it is unclear whether it goes into the full preview or only into the slice-5 block.

### MINOR-7: Smoke check 6 depends on the note cell, which only US4 renders, but US7 is declared runnable after US5

- **Where**: tasks.md "Story dependencies" (line 602): US7 "after US5 … it can run beside them [US2, US3, US4, US6] once US5 is done". The US7 checkpoint (line 568) has the in-process smoke check passing check 6.
- **Against**:
  - T090 (line 551): check 6 expects `DemoHistory.ReadingNote` ("Fell asleep early.") in the History card.
  - T054 (line 401) lists the Note column but doesn't say it shows the note text, and T056 (line 403) doesn't test it.
  - Only T075 (line 500, US4) puts "the note as a button" in the Note cell.
- **Continues**: new.
- **Reasoning**: If US7 is done before US4, as the dependency section allows, the History card may have no note text. Check 6 then fails with "5 of 6 texts on goal page 1 shown", and US7's checkpoint can't pass. Either the History card of US5 must show the note text, or US7 depends on US4. The tasks state neither.

### MINOR-8: The new forbidden-text lists are miscounted and leave out amounts

- **Where**: tasks.md T091 (line 558): "the forbidden-text list gains `DemoHistory.GoalNames` and the three demo notes of FR-030". T084 (line 540) says nothing about the HP-006 name list of `StartupTasksDemoTests`.
- **Against**:
  - FR-030 gives four notes: "Fell asleep early.", "Away for the weekend.", "Birthday cake at work.", "Knee was sore.".
  - contracts/host.md "Smoke check": "No goal name, note, amount, goal path or other page content is ever printed".
  - `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` `No_log_entry_contains_a_demo_custom_habit_goal_or_account_name` builds its names from demo1 to demo3 only.
- **Continues**: MINOR-10 of pass 1.
- **Reasoning**:
  - "Three" leaves one of FR-030's notes unchecked.
  - Neither list gains the amount texts the History card shows ("12 min", "3 sweets"), which check 6 reads and must never print.
  - The startup-log test, which T084 changes anyway, gains none of demo4's notes, so HP-006 for the new "Demo records …" lines is checked only through the names that happen to overlap ("Sugar", "Yoga", "sweet").

### MINOR-9: The History card for a begun goal with no entries is undefined

- **Where**: tasks.md T055 (line 402): "a goal with no current date shows no History card". T054 and T056 (lines 401 and 403) define rows, "Show all" and "no 'Show all' with 10 rows or fewer", but not zero rows. T046: no "Record" without an entry.
- **Against**: `GoalTimeline.TryCalendar` (`src/HT.Domain/Goals/GoalTimeline.cs` lines 77-98 and the `Weeks` helper) gives no entries while `CurrentDate` is set, in these cases:
  - a per-week goal in its first partial week (T040's case);
  - a fixed-days goal that started on an unscheduled day.

  `design/pages/PageGoalDetail.html` has no empty History state.
- **Continues**: new. It is the page-level side of pass-1 MINOR-1.
- **Reasoning**: In these cases `GoalDetailBody` has a current date, so it renders the History card, with an empty table, and no "Record". No task says whether the card is shown empty, hidden, or given a message, and no test pins it. Constitution IV requires following the preview, and the preview has no such state.

## Summary

1 MAJOR, 9 MINOR. Previous notes: 10 Resolved, 4 Partly, 0 Unresolved.

The revision fixes the pass-1 blockers:

- `RecordingService` now appears with its first use case.
- The live progress no longer needs evaluation logic in a component.

One defect remains that blocks implementation as written. `RecordPreview` treats the value entered for a commitment that already has a record as a second record. On the US1-AS2 path ("Record" for today after recording once), the dialog's preview then throws, or shows progress that disagrees with what is saved.

The MINOR notes are about consistency and gaps:

- design artifacts that contradict the new public members and the History card layout;
- a sub-line rule that contradicts its example;
- dialog texts and a plural rule that are not defined;
- the store's note handling on a change;
- the page's column layout;
- a US7 dependency on US4 that the dependency section doesn't state;
- incomplete forbidden-text lists;
- the empty History card.
