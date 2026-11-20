# Critic pass 2: tasks for 006-note-editing

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: specs/006-note-editing/tasks.md (against spec.md, plan.md, research.md, data-model.md, quickstart.md, contracts/host.md, contracts/application-api.md, ADR 0019, the constitution, CLAUDE.md, docs/roadmap.md, slice 5's documents and acceptance.md, design/, and the existing code and tests)

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (T029 misses the `Action` helper, its four tests, line 296's count and the page focus test) | Resolved | T028's third bullet (line 245) keeps the record control as the first button of `td.ht-actions`, so Blazor keeps its element. T029 (lines 252-254) redefines `Action` and adds `NoteControl`. It also names the users of `Action`, the count on line 296 (12 plus the 10 rows without a record in `TwelveDaysAsync`, which is 22, counted by hand) and the theory's `Assert.Single` on "17 – 23 Aug" (`HistoryCardTests.cs:336`). T033's first bullet (line 263) covers `GoalDetailPageTests.cs:219-265`, whose `QuerySelector("td.ht-actions > button")` returns the first button. `HistoryCard.razor:65-78` already renders the record button in fixed branches, so the rule can be met. Some line and test references are imprecise (MINOR-5 below). |
| MINOR-1 (`History` parameter of RecordDialog) | Resolved | T025's third bullet (line 210) now reads `RecordingText.NoteHelp(Recordable)`, which matches `RecordDialog.razor:150`. |
| MINOR-2 (text propagation leaves out data-model.md and spec.md) | Partly | data-model.md is now in the rule (line 55). Leaving spec.md out conflicts with the spec itself and with the next convention bullet, and T037 still checks a narrower set of documents. Continued as MINOR-2. |
| MINOR-3 (Phase 2 claims to follow Work order 1-3 in full) | Resolved | Organization (lines 21-22) now says "with one move (see Notes)". Notes (lines 402-407) states and justifies the move of `NoteHelp` to T024. |
| MINOR-4 (US3 beside US2 versus the per-story commit gate) | Resolved | "Story dependencies" US3 (lines 324-326) and "Parallel opportunities" (line 350) now run the stories one after the other, and give the shared-tree reason. |
| MINOR-5 (two edge cases untested) | Resolved | T022's last bullet (line 201) tests a change with the same value and only a new note: the result is a correction, the state stays Met and the note is replaced. T034's last bullet (line 287) tests "Edit" in a 50-record week, with the week's note and help, and its save. |
| MINOR-6 (T020 under-specified; no rollback test for a failed `Remove`) | Partly | T020 now names both log tests and asks for a `GoalNotes`-trigger rollback check. As placed, the check cannot reach the path it claims to test. Continued as MINOR-1. |
| MINOR-7 (T027 misses AC-071's only-spaces row; a RecordDialogTests name keeps the old rule) | Resolved | T027 (line 237) now has `""`, `"   "`, null and the same note. T026 (line 215) renames `The_note_is_optional_empty_and_says_that_an_empty_field_keeps_the_existing_note`. MINOR-6 below reports the same class of problem in HistoryCardTests. |
| MINOR-8 (no commit for the planning work) | Resolved | T001 (line 94) commits the ADR, this feature's folder, slice 5's spec and acceptance.md, the F005 locks, `translate-002` and the `.feature` file. Its list matches `git status --porcelain` exactly. MINOR-3 below reports how this commit fits the file's own commit rule. |

No older note has regressed. The line references that pass 1 confirmed still hold: RecordTests 487, CorrectionTests 486-487, RecordPrivacyTests 129, RecordDialog.razor line 20, and NoteDialogTests 124, 140 and 156.

## Notes

### MINOR-1: T020's rollback check for a failed `Remove` never reaches the note delete, so the rollback goes untested
- **Where**: tasks.md T020, second bullet (line 176): "add a change with `NoteWrite.Remove` on an entry with a note, with the `GoalNotes` fault trigger on (it fires on delete) … so the record written before the note's delete is rolled back with it", in `DatabaseFaultsAsync`.
- **Against**: `tests/HT.IntegrationTests/Records/RecordLogTests.cs:454-455`. `DatabaseFaultsAsync` creates both triggers for every write in its list. `RecordsTrigger` (lines 496-506) is `AFTER UPDATE, DELETE` on the daily goal's `GoalRecords`. `src/HT.Persistence/Records/RecordStore.cs:94`: `ChangeAsync` first runs `SetValueAsync`, an `ExecuteUpdateAsync` (line 230), on the record row. T014 calls `ApplyNoteAsync` only after that. ADR 0019 Consequences and research R-03: a failed save changes neither the records nor the note.
- **Continues**: MINOR-6
- **Reasoning**:
  - The change's `UPDATE` of `GoalRecords` fires `TR_It_Rl_GoalRecords_Fault` and throws before `ApplyNoteAsync` runs. So the note's `DELETE` is never issued, and no record write exists to roll back.
  - The before/after row comparison (lines 491-492) would still pass. The test would look like it covers the Remove-after-record-write rollback, but it only repeats the record-trigger fault that line 472 already covers.
  - The file already exercises this path for `Replace` with an insert that the records trigger ignores: line 466, `RecordAsync` for `Tue17` with a note. T020 instead names a change, which always updates a record row first.
  - So the failure mode that research R-01 gives as its reason for one transaction stays unverified for `Remove`. The Notes (line 412) list it as settled.

### MINOR-2: The rule that spec.md is not edited conflicts with the spec and with the next convention, and T037's check is narrower than the rule
- **Where**: tasks.md "Conventions for every task", the design-text bullet (lines 52-57) and the bullet after it (lines 58-59): "Tests assert the literal texts of spec.md and contracts/host.md". T037 (line 302).
- **Against**:
  - spec.md "Slice 5 requirements and tests this feature changes" (lines 317-321): this feature's HP-001 to HP-004 "govern; slice 5's spec does not restate their texts, so a wording that `design/` changes is changed here only". Under that rule, a text that `design/` changes is changed in this spec.
  - spec.md HP-003 and HP-004 hold the help texts and the "Edit note · {goal name}" title.
  - The convention bullet itself lists research.md R-07 to R-09. T009 copies `RecordingText.NoteHelp`, with its texts, into slice 5's data-model.md.
  - Constitution, Governance: "never by silent divergence".
- **Continues**: MINOR-2
- **Reasoning**:
  - The stated reason ("its texts are proposals that `design/` confirms, and `design/` governs") fits HP-003's proposal wording. It does not fit the spec's own instruction that a changed wording is changed in this spec, because these HPs govern.
  - The next bullet tells tests to assert spec.md's literal texts. Suppose T003 to T006 change a text. The tests then follow either the unedited spec, which diverges from `design/`, or `design/`, which diverges from the spec they are said to follow.
  - T037, the polish check of shipped texts, leaves out research.md and slice 5's data-model.md, although the convention and T009 put the texts there. Drift in those files would pass the check.
  - All of this matters only if the design step changes a text, which keeps the note MINOR.

### MINOR-3: T001's commit breaks the file's own rule that commits happen only at a checkpoint with the full suite green
- **Where**: tasks.md T001 (line 94) commits right away. Conventions (line 85): "Commit only at a checkpoint, with the full suite green (constitution V)". The Phase 1 checkpoint (line 101) is only "`dotnet build HabitTracker.slnx` passes", and comes after T001.
- **Against**: tasks.md's own Conventions. Constitution V ("the build and the full test suite have passed in the tree being committed"), which the convention cites.
- **Continues**: new (comes from the MINOR-8 fix)
- **Reasoning**: T001 commits before any checkpoint and runs no build or test. That is defensible for a docs-only commit: the pre-commit hook runs the architecture tests, and the code tree is `master`'s. But the convention admits no exception, and T001 does not state one. An implementer following the conventions literally must either run the full suite before T001 or treat T001 as breaking a rule.

### MINOR-4: T036 compares a scoped Roslynator run, and a jb run, with baselines that do not describe them
- **Where**: tasks.md T036 (line 301): `roslynator analyze HabitTracker.slnx --projects HT.Domain HT.Application HT.Persistence` and `jb inspectcode …`, "Compare both with the `CLAUDE.md` baselines".
- **Against**: CLAUDE.md "C# code analysis". The baseline is dated "2026-11-18 (after slice 4)" and covers the full solution: 333 Roslynator diagnostics, 37 not from the compiler, almost all of the rest Razor phantoms in `HT.UiTests`; and 170 from `jb -e=WARNING`. `docs/roadmap.md` Shipped lists slice 5 (PR #38) after that date.
- **Reasoning**:
  - No documented baseline exists for a run scoped to the three production projects, so the Roslynator comparison the task asks for cannot be made.
  - Both baselines predate slice 5, so a difference from them cannot be attributed to this feature.
  - So "fix real findings only" has no reference set to compare against, beyond reading each hit.

### MINOR-5: T029 misnames one of the four tests that use `Action`, and some line numbers are off by one
- **Where**: tasks.md T029, the `Action` bullet (line 252): "… `The_row_controls_raise_the_row_and_for_Edit_the_record` (line 340) and its focus check on line 365"; "the helper `Action` (lines 249 and 250)"; the period test "(line 305)"; the theory bullet (line 254) "(line 335)".
- **Against**: `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`:
  - `Action` is declared on lines 250-251 (line 249 is blank);
  - the period test calls `Action` on lines 306-308 (line 305 is the states assertion);
  - the theory is declared on line 322, and its `Assert.Single` over "17 – 23 Aug" is on line 336 (line 335 is the record-button label assertion);
  - line 365 belongs to the separate test `FocusAsync_gives_the_focus_to_the_record_button_while_the_row_shows_it_and_otherwise_to_the_rows_action` (lines 360-374), not to `The_row_controls_raise_the_row_and_for_Edit_the_record` (lines 339-358).
- **Continues**: new (precision of the MAJOR-1 fix)
- **Reasoning**: the task says its list "keeps the four tests that use `Action` asserting what they asserted before". The fourth test is not named, and is described as part of another test. An implementer who checks the list against the file finds one test fewer than claimed, plus line pointers that land on neighbouring lines. The tests themselves are covered, so this is only imprecision.

### MINOR-6: HistoryCardTests keep test names that state the removed rule
- **Where**: tasks.md T029 (lines 255-257) rewrites the bodies of the Note-cell tests but renames none of them.
- **Against**: `HistoryCardTests.cs`:
  - `US4_AS7_every_row_offers_Add_note_without_a_note_and_the_note_as_a_button_with_one_whatever_its_state` (line 382);
  - `The_Note_cells_buttons_raise_their_row` (line 409);
  - `FocusNoteAsync_gives_the_focus_to_the_rows_Note_button_and_nothing_for_a_row_not_shown` (line 424);
  - the section comment "US4-AS7: the Note cell's actions" (line 376);
  - the `NoteButton` helper (lines 378-379), which looks for a button in the Note cell.

  T017 asks for names that cite the amended rule, and T026 renames the RecordDialogTests test for that reason. Constitution I says test names use the rule's vocabulary.
- **Continues**: new (same class as MINOR-7, which was fixed only for RecordDialogTests)
- **Reasoning**:
  - After T028, no row offers a note control on every row, the Note cell has no button, and `FocusNoteAsync` focuses a control in the actions column. The names above then state the opposite of what their bodies assert.
  - The tasks file treats this as a defect elsewhere: T026 renames the RecordDialogTests test, and T017 renames its three tests.

## Summary

0 MAJOR, 6 MINOR. Previous notes: 7 Resolved, 2 Partly, 0 Unresolved.

The revision resolves the blocking finding of pass 1. T028's first-button rule, T029's redefined `Action` and `NoteControl` helpers with the hand-counted literal for line 296, and T033's reading of the goal page's focus test now account for every existing test that the History card change would break, and the rule is achievable in the current markup. The other pass 1 notes are resolved: the commit of the planning work, the sequential stories, the two edge-case tests, the AC-071 row and the rename.

What remains is precision:
- the rollback check that T020 places in `DatabaseFaultsAsync` stops at the records trigger before it reaches the note delete;
- the rule against editing spec.md conflicts with the spec's own instruction and with the tests-assert-spec-literals bullet;
- T001's commit is an unstated exception to the file's commit rule;
- T036 compares against baselines that do not describe its scoped run;
- a few HistoryCardTests references and test names are stale.

None of them blocks `/speckit-analyze` or implementation.
