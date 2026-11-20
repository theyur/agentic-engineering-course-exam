# Critic pass 1: tasks for 006-note-editing

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: specs/006-note-editing/tasks.md (against spec.md, plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md, ADR 0019, the constitution, CLAUDE.md, docs/roadmap.md, slice 5's documents and acceptance.md, design/, and the existing code and tests)

## Notes

### MAJOR-1: T029 leaves four existing HistoryCardTests and their shared `Action` helper out, and they break with T028
- **Where**: tasks.md T029 (lines 242-248), which gives the HistoryCardTests edits line by line (172; 173-178; 387-392; 404-405; 409-419; 424-430; a new theory). T028 (lines 236-241) puts "Add record", then "Add note" or "Edit note", in `td.ht-actions` of every row without a record.
- **Against**: `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`:
  - the helper `Action` (lines 249-250) is `Assert.Single(Row(cut, date).QuerySelectorAll("td.ht-actions > button"))`;
  - `US3_AS8_a_commitment_row_offers_Add_record_without_a_record_and_Edit_with_one_whatever_its_state` (lines 284-297) calls `Action` on the rows without a record "Fri 13 Nov", "Thu 12 Nov", "Tue 10 Nov" and "Wed 4 Nov". Line 296 asserts `Assert.Equal(12, cut.FindAll("td.ht-actions > button").Count)`;
  - `US3_AS8_a_period_row_offers_Add_record_and_each_record_as_a_button_that_opens_Edit_final_rows_included` (line 305) calls `Action(cut, "17 – 23 Aug")`, a week without a record;
  - the theory `US3_AS8_a_period_row_offers_Add_record_only_while_it_has_fewer_than_50_records` (line 335) runs `Assert.Single` over the actions of "17 – 23 Aug", which has no record;
  - `The_row_controls_raise_the_row_and_for_Edit_the_record` (line 345) calls `Action(commitments, "Wed 4 Nov")`, which has no record.
  - The plan's constitution check (V) and the Phase 2 checkpoint say the old rule's tests change in the same commits ("never loosen an assertion").
- **Reasoning**: after T028, every row without a record has two buttons in `td.ht-actions`. The helper's `Assert.Single` then fails, and so does the count of 12 on line 296. That makes four existing tests fail at the US2 checkpoint, and neither T029 nor research R-10 names them. The task's line-by-line list reads as complete, so the implementer meets these failures unannounced. Fixing them takes decisions no artifact makes: what `Action` picks on a row with two controls, and what the new count on line 296 is. The constitution's rule against loosening an assertion leaves no quick fallback. A related risk is also unlisted. `GoalDetailPageTests.US3_a_Missed_rows_Add_record_and_then_Edit_open_the_RecordDialog_for_that_row_and_give_the_focus_back_to_it` (lines 219-265) asserts that focus returns to the same element id across "Add record" → "Edit" → "Add record" (lines 235-237, 251-252, 262-263). Its comment says "The row keeps its one action button". That holds only if T028's markup keeps "Add record"/"Edit" as the same first element once a second button exists. T033 changes only lines 286-317 of that file.

### MINOR-1: T025 names a `History` parameter that RecordDialog does not have
- **Where**: tasks.md T025, third bullet: "the `TextArea`'s `Help` is `RecordingText.NoteHelp(History)`".
- **Against**: `src/HT.Ui/Components/Records/RecordDialog.razor`: the dialog's `GoalHistory` parameter is `Recordable` (the `[Parameter, EditorRequired] public GoalHistory Recordable` property). `History` is a parameter of HistoryCard and NoteDialog, not of RecordDialog. contracts/host.md says `RecordingText.NoteHelp(history)`.
- **Reasoning**: the task names a member that does not exist in the component it changes. It compiles only if the implementer works out that `Recordable` is meant, or adds a duplicate parameter.

### MINOR-2: The text-propagation rule leaves out data-model.md and the spec, which hold the same texts
- **Where**: tasks.md "Conventions for every task", the design-artifacts bullet (lines 51-55): a text that T003-T006 changes must also change in contracts/host.md, research.md R-07 to R-09 and quickstart.md §3.
- **Against**: data-model.md, "Application: `RecordingText`", restates the three help texts. spec.md HP-003 and HP-004 hold the help texts and the "Edit note · {goal name}" title, and plan.md Work order 1 points to them. T024 also takes "the texts as T004 confirmed them".
- **Reasoning**: suppose `design/` changes a help text or title. The list would then leave data-model.md and spec.md with the old wording, although T009 copies data-model.md into slice 5's data-model. T037 checks contracts/host.md and quickstart.md only, so the drift would also pass the polish check. The constitution's Governance clause forbids silent divergence.

### MINOR-3: Phase 2 does not follow plan Work order 1 to 3 "in full", as the Organization section says
- **Where**: tasks.md "Organization" (line 21: "Phase 2 follows the plan's Work order steps 1 to 3 in full"); T024 puts `RecordingText.NoteHelp` in Phase 3 (US1).
- **Against**: plan.md Work order step 3 lists `RecordingText.NoteHelp` among the Application changes "verified and committed together" with the port change.
- **Reasoning**: the claim and the task placement disagree. Delivering NoteHelp with US1 is defensible, but the deviation from the plan's step 3 is not acknowledged. The Notes section acknowledges only the split of step 4.

### MINOR-4: Running US3 beside US2 conflicts with the full-suite checkpoint and commit of each story
- **Where**: tasks.md "Story dependencies", US3 ("It can run beside US2"), and "Parallel opportunities" ("US3 beside US2, once US1 is done"). The same file, under "Within each story", says the story's checkpoint "runs the full suite before the next story, and is its commit point". The Conventions allow commits "only at a checkpoint, with the full suite green".
- **Against**: constitution V (done means the build and the full suite pass in the tree being committed).
- **Reasoning**: suppose US3 runs in the same working tree while US2 is partly done, for example with HistoryCard changed but its tests not yet (see MAJOR-1). US3's checkpoint then either fails on US2's red tests or commits US2's unfinished work. Only the disjoint files are considered, not the shared tree and commit gate.

### MINOR-5: Two spec edge cases have no test task, or have one that does not test them
- **Where**: tasks.md T022, T026, T034 ("The 50-record period row is covered by T029"), and T029.
- **Against**: spec.md "Edge Cases":
  - "A per-week period with 50 records offers no 'Add record', but each record's 'Edit' still edits the week's note";
  - "Changing only the note in 'Edit' of a final commitment saves the record with its unchanged value. The use case reports it as a correction … and the state does not change".
- **Reasoning**: T029 only counts the note controls on a 50-record row. It does not open "Edit" of one of its records or check that the field holds the week's note. T034 tests weeks with a few records. No task covers a change with the same value and only a new note on a final commitment, with the correction flag and the unchanged state. The review asks that every edge case be addressed.

### MINOR-6: T020 does not say which log test gets the `Remove` write, and nothing requires a failed `Remove` to roll back
- **Where**: tasks.md T020 ("Add a `Remove` write to the log-capture writes of … RecordLogTests.cs").
- **Against**: `tests/HT.IntegrationTests/Records/RecordLogTests.cs` has two candidates:
  - `HP_006_no_store_member_logs_a_note_or_value_when_it_writes_reads_refuses_or_is_cancelled`, with owner writes and no fault (lines 314-370);
  - the fault-trigger test around lines 455-500. It forces the `GoalNotes` trigger to fault and then asserts that the record rows and note rows are unchanged.

  ADR 0019 Consequences say "a refused or failed save changes neither the records nor the note". research R-03 says "a fault rolls back the record and the note together".
- **Reasoning**: added to the first test, the `Remove` write covers HP-007 only. Nothing then tests that a `Remove` faulting inside `InGoalTransactionAsync` rolls back the record written before it. That is the failure mode research R-01 gives as the reason for a single transaction. T018 covers refusals only (`LimitReached`, `RecordNotFound`).

### MINOR-7: T027 omits AC-071's only-spaces row, and a T026 test name keeps the old rule
- **Where**: tasks.md T027, first bullet (same note, `""`, null), and T026 (removes the constant `NoteHelp`, line 27).
- **Against**: acceptance.md AC-071 rows 01-04 (same note, `""`, `"   "`, none); contracts/application-api.md "Examples" (AC-071: `""`, `"   "` or null). `RecordDialogTests.The_note_is_optional_empty_and_says_that_an_empty_field_keeps_the_existing_note` (line 216) is the test that asserts the old help.
- **Reasoning**: the unit test of US2-AS3 leaves out one of the four rows of the criterion it mirrors. The page-head test whose name states the reversed rule is not named for a rename. That conflicts with the task's own rule that names cite the amended ids, and with constitution I (test names use the rule's vocabulary).

### MINOR-8: No commit takes the work already made but not committed
- **Where**: tasks.md Phase 2 checkpoint ("Commit the Application, Persistence, fake, test and architecture changes together, with the design and slice 5 document changes"), and T001.
- **Against**: `git status` of the worktree. These are modified or untracked and not committed:
  - `docs/adr/0019-note-editing.md`;
  - `specs/006-note-editing/`;
  - `specs/005-recording-and-evaluation/spec.md` and `acceptance.md`;
  - `ac/sources.lock`, `ac/features.lock` and `ac/runs/translate-002/`;
  - `tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature`;
  - `docs/roadmap.md`.
- **Reasoning**: "slice 5 document changes" reads as T007-T010, so no task or checkpoint puts the ADR, this feature's documents, slice 5's amended spec or the locked F005 artifacts into a commit. They could be committed together with code by accident, or left out of the branch. The plan needs both the locked criteria and ADR 0019 on the branch for the PR.

## Summary

1 MAJOR, 8 MINOR. The task list follows the plan's order: design first, then slice 5's documents, then the port change as one compile unit. Its line references are accurate (RecordTests 487, RecordPrivacyTests 129, UiDependencyTests 80, GoalPrivacyHostTests 114, GoalsHostTests 213-214, RecordDialog.razor line 20, the NoteDialogTests lines). It covers FR-001 to FR-007 and HP-001 to HP-007 with unit, integration, architecture and bUnit tests. The blocking gap is MAJOR-1. The HistoryCard change breaks four existing tests through a shared `Assert.Single` helper and a hard-coded button count, and T029 does not list them. The implementer would hit unplanned failures at the US2 checkpoint, and the fix needs a decision no artifact makes. The minor notes are about precision: a wrong component member name, how design text changes propagate, the claim about the plan's order, the commit gate when stories run in parallel, two untested edge cases, an under-specified log or rollback test, one missing AC-071 row and a stale test name, and the commit of work already done.
