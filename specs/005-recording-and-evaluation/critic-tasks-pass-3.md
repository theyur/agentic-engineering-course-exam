# Critic pass 3: tasks for 005-recording-and-evaluation

**Verdict**: APPROVE
**Date**: 2026-11-19
**Reviewed**: specs/005-recording-and-evaluation/tasks.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (the RecordDialog's preview adds the new value as a second record on a commitment) | Resolved | T028 (line 290) now leaves out "for a commitment entry, its existing record in every case, because saving replaces it (FR-003). So a commitment never gets more than one value". Its test list adds "a commitment with 12 minutes and a candidate of 25 with no `replacing` (US1-AS2), which gives 25 of 20 and does not throw". A period opened from "Record" still keeps its other records and adds the candidate, which matches `RecordAsync`'s add (T042). |
| MINOR-1 (decisions settled in tasks.md leave the design artifacts contradicting it) | Partly | The new Conventions item (lines 59-66) names each addition and the document that records it. T046 (line 383), T054 (line 417) and T090 (line 569) repeat the instruction. The rationale is sound. Governance forbids *silent* divergence, and each amendment is now an explicit deliverable of the task that creates the member. The AC test writer runs only after `/speckit-implement`, so the documents are updated before anyone reads them as the contract. The list is incomplete, though: see MINOR-2. |
| MINOR-2 (the sub-line rule cannot produce its own example) | Partly | The rule is rewritten (T004 line 186), and Reading's example now follows from it. It still misstates `GoalText.AmountText`, and its lower-casing step breaks the other schedule kinds. MINOR-1 carries this forward. |
| MINOR-3 (RecordDialog texts and inputs assigned to no task) | Resolved | T004 now settles each item: the title "Record · {goal name}" (line 184), the field labels "Did you do it?", "Time spent" and "Amount" (line 187), the Reduce help for count, duration and yes/no (line 189), and the progress bar (line 190). T024 (lines 280-281) adds `Title`, `FieldLabel`, `ReduceHelp` and `ProgressFill`. `GoalHistory.ReadAt` (T022, line 273) gives T045 (line 376) the instant for "N h left", so no component reads a clock. |
| MINOR-4 (count-unit plural rule undefined) | Partly | T004 (line 191), T024 (line 278) and T025 (line 284) now say "with the unit as stored, never inflected" ("3 sweet", "1 cups"). The rationale is sound. `GoalText.AmountText` (`src/HT.Domain/Goals/GoalText.cs` lines 33-43) and `TargetTextParts.AmountPart` (`src/HT.Domain/Habits/TargetTextParts.cs` lines 36-42) already show the unit as stored. HP-011's own example "1 of at most 2 cups" uses a stored plural, and the spec's "3 sweets" (FR-030) is narrative, not a UI text. Three design documents still show the inflected form, and the recording list doesn't cover them: see MINOR-2. |
| MINOR-5 (a note given with a change neither specified for the store nor tested) | Resolved | T033 (line 304): `ChangeAsync` "like `AddAsync`, upserts a note with text under `entry.Key` and leaves the note alone without one (FR-007)". T061 (line 473) and T062 (line 475) test both cases. |
| MINOR-6 (column layout unspecified; T003 edits the full preview) | Resolved | T003 (line 169) gives the full preview's layout: History in the left column, then Settings and Recent notes in the right. T046 (line 383) builds `GoalDetailBody` in that layout and records it. T003 (line 168) keeps "Record" in the full preview and puts "Add record" only in the slice-5 block. This matches `design/pages/PageGoalDetail.html` lines 18-51. |
| MINOR-7 (smoke check 6 depends on the note cell, which only US4 renders) | Resolved | T054 (line 417) shows "the note's text, as plain text; US4 makes it a button", and T056 (line 419) tests "the note text in the Note cell". The US7 dependency (line 618) now gives this reason. |
| MINOR-8 (forbidden-text lists miscounted, amounts left out) | Resolved | T091 (line 574) lists the four FR-030 notes by text, plus "12 min", "25 min" and "3 sweet". T084 (line 556) adds `DemoHistory.GoalNames` and the four notes to `No_log_entry_contains_a_demo_custom_habit_goal_or_account_name`. |
| MINOR-9 (History card for a begun goal with no entries) | Resolved | T003 (line 170), T054 (line 417), T055 (line 418) and T056 (line 419) render no History card without a begun entry, the per-week first partial week included. T003 and T075 (line 516) do the same for Recent notes. Notes need a begun entry (T071's `HasBegun`), so the two rules agree. |

No pass-1 note has regressed:

- `RecordingService` is still created in T042.
- `RecordPreview` and `Evaluator.ProgressOf` remain (T015, T028).
- The empty-range rules remain (T043, T053).
- The dialog modes remain (T063, T073), and so does the RecentNotesCard's schedule input (T074).
- Each remaining pass-1 note is still covered by its fix:
  - `tokens.json` (T005);
  - the per-combination boundaries (T017);
  - the log capture (T079, T083);
  - the edge cases (T039, T041, T058);
  - the smoke-check contract (T091);
  - the constructor convention (Conventions lines 116-119);
  - the roadmap check (T093).

## Notes

### MINOR-1: The rewritten sub-line rule misreads `GoalText.AmountText` and breaks for fixed days and per-week schedules

- **Where**:
  - tasks.md T004 (line 186), the sub-line rule:
    - "for a count or duration goal 'at least ' (Build only) and slice 4's `GoalText.AmountText` ('20 min', 'at most 1 sweet')";
    - "slice 4's `GoalText.ScheduleText` with its first letter in lower case ('every day', '3 times a week')".
  - T004 (line 189), the Reduce help: "At most {AmountText without 'at most '}".
  - T024 `TargetLine` (line 282).
  - T025 (line 284) pins only "Build · at least 20 min · every day" and "Reduce · every day".
- **Against**:
  - `GoalText.AmountText` gives "N unit" with no prefix, e.g. "1 sweet". Only `TargetTextParts.Text` adds "at most " (`src/HT.Domain/Habits/TargetTextParts.cs` lines 32 and 36-42; `src/HT.Domain/Goals/GoalText.cs` lines 37-43).
  - `GoalText.ScheduleText` gives:
    - "Mon · Wed · Fri" for fixed days (`GoalText.cs` line 77);
    - "Flexible · 3 times a week" for a per-week goal (line 86).
  - `design/components/RecordForms.html` line 10 shows Yoga's sub line as "Build · Mon · Wed · Fri", and line 14 shows Coffee's as "Reduce · at most 2 cups a day".
  - Constitution IV: pages follow the wording of their previews.
- **Continues**: MINOR-2 of pass 2.
- **Reasoning**: Only Reading's case comes out as the example claims. Applied literally, the rule gives:
  - a Reduce count goal: "Reduce · 2 cups · every day". "at least " is Build only, and `AmountText` has no "at most ", so nothing marks the amount as a limit. The parenthetical "'at most 1 sweet'" suggests the opposite.
  - a fixed-days goal: "Build · mon · Wed · Fri". Lower-casing the first letter of a day name is wrong text.
  - a per-week goal: "flexible · 3 times a week", not the "3 times a week" the rule gives as its example.

  The Reduce help's "without 'at most '" depends on the same misreading. It happens to give the right text only because there is nothing to remove. The two cases T025 pins both work, so the suite would not catch the fixed-days, per-week or Reduce-amount sub lines. The design step (T004) and `RecordingText.TargetLine` (T024) are both told to copy the text "as written", so the defect spreads into both.

### MINOR-2: The recording list leaves the inflected "3 sweets" in three design documents

- **Where**:
  - tasks.md Conventions (lines 59-66): the items recorded in contracts/host.md and research.md R-12 are the History card's head sentence, its actions column, its note text, its absence, the two columns, and the RecordDialog's title, labels and help texts.
  - T004 (line 191) and T024 (line 278): "3 sweet", with the unit as stored.
  - Notes (line 737): "contracts/host.md says 'under the table'; the design governs". T054 now corrects that text, but the note still describes it in the present tense.
- **Against**:
  - contracts/host.md line 31: Recorded "for example '12 min', 'Yes' or '3 sweets'".
  - research.md line 534: the Recorded cell is tested with "Yes", "No", "12 min", "3 sweets".
  - quickstart.md line 82: "Failed with 3 sweets", a step that T097 runs by hand.
  - Constitution Governance: "never by silent divergence".
- **Continues**: MINOR-1 and MINOR-4 of pass 2.
- **Reasoning**: The plural decision is sound (see the table above), but it changes a text these three documents specify. The task that settles it (T004/T024) isn't told to record it, and no Conventions item lists it.
  - After implementation, the host contract's Recorded example and research R-12's test list both say "3 sweets", while the code shows "3 sweet".
  - The quickstart step T097 runs expects a text the page doesn't show.

  The list exists to catch exactly this kind of divergence, and it misses this one.

### MINOR-3: The progress text of a count or duration commitment with no value is undefined

- **Where**:
  - tasks.md T024 `Progress(GoalHistory, Progress)` (line 277): examples with a value only; "null for a yes/no commitment".
  - T004 (line 190): the bar "filled to `Value / Of`".
  - T028 (line 290): "An invalid or missing candidate gives the progress without it", which for a commitment is `Value` null.
  - T044 (line 372): the initial value of the count Stepper and the minutes field is not stated.
- **Against**:
  - data-model.md line 27: "`Value` is null only for a commitment without a record".
  - plan.md "Resolved in the spec" 10 (lines 307-312): "with no record, the value is absent and the target is given".
  - HP-002: "the progress under the field".
- **Continues**: new. It is the remaining text gap of pass-2 MINOR-3.
- **Reasoning**: The dialog has this state every time it opens from "Record" on a count or duration commitment with no record yet, before anything valid is typed, and whenever the field holds an out-of-range value. `RecordPreview` then gives `Progress(RecordedValue, null, 20)`. No task says what `RecordingText.Progress` and `ProgressFill` return for it:
  - "of 20 min";
  - "— of 20 min";
  - "0 of 20 min";
  - nothing at all.

  T025's "every text above" therefore has no expected value for it, and T049's "the live progress" can't assert the first paint. This leaves wording to the implementer that constitution IV expects to come from the design step.

### MINOR-4: T061 and T062 are both marked [P] but T062 edits T061's file, for cases T061 already has

- **Where**:
  - tasks.md T061 `[P]` (lines 464-474) creates `CorrectionTests.cs`, which already includes "a change with a note with text replaces the note, and one without leaves it (FR-007)" (line 473).
  - T062 `[P]` (line 475) ends: "Add the same note cases for `ChangeRecordAsync` to T061".
- **Against**:
  - Format (lines 49-50): "[P]: the task can run in parallel with the other [P] tasks of its group (they touch different files)".
  - The "Shared files" list (lines 619-632) doesn't list `CorrectionTests.cs`.
- **Continues**: new.
- **Reasoning**: Run in parallel as marked, T061 and T062 both write `tests/HT.UnitTests/Application/Records/CorrectionTests.cs`. Because the cases are already in T061, T062's instruction is either a duplicate or a conflicting second edit of the same file. Either way, the [P] marks contradict the Format rule.

## Summary

0 MAJOR, 4 MINOR. Previous notes: 7 Resolved, 3 Partly, 0 Unresolved.

The pass-2 blocker is fixed. `RecordPreview` now drops a commitment's existing record before it adds the candidate, and US1-AS2's path is tested directly. The rewritten items are consistent with the spec and the code:

- the column layout (T003, T046);
- the empty History card (T054, T055);
- the store's note handling on a change (T033, T062);
- the US7 reason (line 618);
- the forbidden-text lists (T084, T091);
- `GoalHistory.ReadAt` (T022).

The two adapted rationales hold. Each task records its own amendment of the design documents, and units are shown as stored, as slice 4's formatter already does.

What remains is wording and bookkeeping:

- a sub-line rule that misreads two slice-4 formatters and gives wrong text for fixed-days, per-week and Reduce-amount goals;
- a "3 sweets" example that the recording list leaves in three documents;
- an undefined progress text for a commitment with no value yet;
- one [P] pair that edits the same file.

None of these blocks implementation, but MINOR-1 would produce visibly wrong dialog text if it is followed as written.
