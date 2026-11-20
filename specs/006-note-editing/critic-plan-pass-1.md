# Critic pass 1: plan for 006-note-editing

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Notes

### MINOR-1: The list of existing tests that break is incomplete, and research R-10 is given as the complete list
- **Where**: research.md R-10 ("The existing tests that encode slice 5's old rule change in the same commits", "Mechanical changes", the HistoryCardTests line list); plan.md "Spec discrepancies" (callers of the port the spec did not name, "Research R-10 adds them"); plan.md Work order step 3.
- **Against**: the current tests; `TreatWarningsAsErrors` and `Nullable` (CLAUDE.md, root `Directory.Build.props`); constitution V.
- **Reasoning**: these existing tests break under the planned change, and R-10 does not name them:
  - `tests/HT.UnitTests/Application/Records/CorrectionTests.cs:486`, `FR_007_a_change_without_a_note_or_with_only_spaces_leaves_the_entrys_note`, has `[InlineData("")]` and `[InlineData("   ")]` and asserts "Back was tired.". Under FR-001 those two cases remove the note. R-10 lists only the `NoteTests` and `RecordTests` tests as changing. The plan's tree says "CorrectionTests (as needed)".
  - `tests/HT.UiTests/Host/GoalsHostTests.cs:213-214` calls `IRecordStore.AddAsync(..., null, ...)`. With a non-nullable `NoteWrite note` parameter, the `null` literal raises CS8625, which `TreatWarningsAsErrors` turns into a build error. The plan says it lists every port caller the spec missed, but names only `GoalPrivacyHostTests` line 114.
  - `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`:
    - lines 173-178 assert `Assert.Single(actions.Children)` for every row, and rows without a record now get two controls;
    - line 392 asserts one button in the Note cell of every row;
    - lines 409-419 (`The_Note_cells_buttons_raise_their_row`) click the Note cell of "Tue 3 Nov", a row that has a record.

  R-10 names only lines 172, 387 to 391, 404, 405 and 424 to 430. The suite would expose these breaks. But the plan presents its inventory as complete, and the task list will be built from it.

### MINOR-2: The plan keeps roadmap row 5a at `features locked` through planning, tasks and implementation
- **Where**: plan.md Work order step 5 ("the roadmap row 5a stays `features locked` until the AC tests pass, then becomes `AC tests green`"); plan.md Risks ("roadmap row 5a stays `features locked` until then"); plan.md "Gates before the next step".
- **Against**: `docs/roadmap.md` status values (`features locked` · `planned` · `tasks ready` · `implementing` · `AC tests green`) and "Updating this file" ("the feature branch updates its row's Status after each step"); CLAUDE.md ("keep it current as its 'Updating this file' section says").
- **Reasoning**: the roadmap expects the row to move to `planned` after this step, then to `tasks ready` and `implementing`. The plan tells `/speckit-tasks` to skip those states. The row in the working tree still reads `features locked` after the plan was written.

### MINOR-3: The new delete path inside `AddAsync` and `ChangeAsync` gets no store-level privacy test
- **Where**: research.md R-10 (it lists `RecordPrivacyTests` under "Mechanical changes" only); R-03 (`Remove` runs `ExecuteDeleteAsync` on `GoalNotes`).
- **Against**: constitution III (negative tests for writes of private data); the existing precedent at `tests/HT.IntegrationTests/Records/RecordPrivacyTests.cs:129`.
- **Reasoning**: before this feature, `SaveNoteAsync` with null was the only store member that deleted a note. `RecordPrivacyTests.cs:129` checks that path for another owner. The plan adds a second destructive path, `Remove` through `AddAsync` and `ChangeAsync`, but its privacy tests only change mechanically (`null` to `NoteWrite.Keep`, a note to `Replace`). R-10's new integration cases are:
  - `Remove` meeting `LimitReached` or `RecordNotFound`;
  - `Remove` on an entry that has no note.

  No case covers `Remove` from another owner (`GoalNotFound`). The goal-lock query makes that path safe by construction. Still, the equivalent check for the old delete path exists, and nothing covers the new one.

### MINOR-4: The claims about the changed AC surface and the size of the model change understate them
- **Where**:
  - plan.md Constraints: "The only change to the exported AC surface is the two port members (research R-11)";
  - R-11: "Its two changed members are the only change to the exported surface";
  - plan.md Scale/Scope: "1 new port type";
  - data-model.md introduction: "changes slice 5's model in three places and adds nothing else".
- **Against**: the same artifacts. R-01 and data-model.md define two new public types, `NoteWrite` and `NoteWriteKind`. R-07 and data-model.md add the public member `RecordingText.NoteHelp`. ADR 0003 and ADR 0010 make `HT.Application` an exported AC surface project.
- **Reasoning**: the exported reference surface also gains `NoteWrite`, `NoteWriteKind` and `RecordingText.NoteHelp`. data-model.md has four change sections (`NoteWrite`, `RecordInput.Note`, `IRecordStore`, `RecordingText`), and two of them add something. The risk analysis for `/speckit-ac-tests-write 005` and the refs export depends on these claims.

### MINOR-5: SC-004 is not mapped to any test or check
- **Where**: quickstart.md section 1 table (it maps HP-001 to HP-004, SC-001, SC-002 and SC-003) and section 2 (SC-005); research.md R-10.
- **Against**: spec.md SC-004 (a row's note is changed or cleared in at most 3 actions, and a row after the first 10 needs "Show all N →" first).
- **Reasoning**: every other success criterion is tied to a test or to a quickstart step. SC-004 appears nowhere in the plan artifacts, so neither `/speckit-analyze` nor implementation has a verification step for it.

### MINOR-6: The AC risk lists the revised AC-037 rows incompletely
- **Where**: plan.md Risks, "F005 before `/speckit-ac-tests-write`" ("the revised AC-037 rows 02, 03 and 07 and the new AC-070 to AC-072").
- **Against**: `specs/005-recording-and-evaluation/acceptance.md` AC-037 (locked).
- **Reasoning**: compared with the committed version, AC-037 adds both row 07 and row 08 ("changes the record of 2026-11-16 to No with the note 'Knee was sore.'"). It also rewrites the `note` column of every row, so rows 01, 04, 05 and 06 now read `the note "…"` where they read `"…"`. The risk omits row 08 and the rephrased rows, so its list of steps that may not bind is understated.

### MINOR-7: The quickstart tells the reviewer to press an "Edit" button that period rows do not have
- **Where**: quickstart.md section 3:
  - step 6: "The same holds for 'Edit' of any of its records" (Yoga, per week);
  - step 7: "'Edit' on the Failed day with 'Birthday cake at work.'" (Sugar).
- **Against**: `src/HT.Application/Operator/DemoGoals.cs:38` (Sugar is `TimesA(1, SchedulePeriod.Day)`, a per-day period); `HistoryCard.razor` and contracts/host.md ("A period's records stay buttons in the Recorded cell that open 'Edit' of that record").
- **Reasoning**: on period rows the visible control is the record's own button, such as "3 sweet" or "Yes". "Edit" is the dialog mode and appears only in the accessible name ("…, edit record 1 of …"). A reviewer following steps 6 and 7 finds no button labelled "Edit" on those rows.

## Summary

0 MAJOR, 7 MINOR. The plan and its artifacts hold up against the spec, ADR 0019, slice 5's revised and locked criteria, and the code:
- The F005 lock hashes match the files.
- `NoteWrite` gives the three cases that D-1 needs, applied in the goal's existing transaction. `LimitReached` and `RecordNotFound` already return before the note write.
- The dialogs can fill the note from `HistoryEntry.Note` in all three modes.
- The design, slice 5 documents and work order follow the spec's order of work.

The notes are mostly about completeness: the inventory of existing tests that break, the roadmap status flow, one missing privacy test for the new delete path, an understated description of the changed AC surface, an unmapped SC-004, an incomplete AC-037 risk list, and quickstart wording. None of them blocks `/speckit-tasks`, but fixing MINOR-1 and MINOR-2 first would keep the task list from carrying the gaps forward.
