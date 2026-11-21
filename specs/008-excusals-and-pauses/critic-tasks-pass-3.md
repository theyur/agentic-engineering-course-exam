# Critic pass 3: tasks for Excusals and pauses

**Verdict**: APPROVE
**Date**: 2026-11-21
**Reviewed**: specs/008-excusals-and-pauses/tasks.md

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (T042's legend change breaks three page and component tests at the Phase 2 checkpoint) | Resolved | T042 (tasks.md:86-92) now changes `GoalStatsTests.cs:72` and `HomeVisitorTests.cs:116` (both today `["Met", "Failed", "Missed", "Pending"]`, confirmed) to the six entries. It also changes `GoalDetailPageTests.cs:116`'s substring absent list: "Pause" and "Excuse" leave it, and button checks take their place. The Checkpoint (shared) (:94-97) names these edits. T052 (:129), T064 (:363) and T069 (:385) follow the change consistently. Verified against the code: no other full strip exists (`GoalItem.razor:29` renders `Mini="true"`, and `ConsistencyStrip.razor` shows the legend only when `!Mini`), so `GoalsPageTests.cs:107` (absent "Paused") is unaffected. `NewGoalPageTests.cs:198` and `RecordDialogTests.cs:141` render no strip. No host test asserts the legend: a grep of `tests/HT.UiTests/Host` for `ht-legend` and legend HTML finds nothing. T042's own grep (`ht-legend\|"Pause"\|"Excuse"`) returns exactly the sites it names, plus the empty-legend checks of the mini strip (`ConsistencyStripTests.cs:93`, `HabitComponentTests.cs:430`), which stay valid. |
| MINOR-1 (T034 misses `RecordTests.cs` and leaves the place of the exemption read open) | Resolved | T034 (:67-69) now says where the read goes: once per call, after access and goal, and only when there is an entry or date to evaluate. In `RecordAsync` it goes right after "not begun". It names `RecordTests.cs`' note-too-long list (`RecordTests.cs:336`, which becomes `[ReadExemptionsAsync]`). Checked against every `Assert.Empty(Calls)` in the code. In `RecordTests.cs`, lines 160, 172 and 184 are refusals for nothing scheduled and line 196 for not begun, all before the read; lines 241, 252 and 263 are access refusals. The no-entry cases are `HistoryTests.cs:336/349/362` and `RecordableTests.cs:126/176`. The access refusals are `HistoryTests.cs:313` and `RecordableTests.cs:215`, and `HistoryTests.cs:266` is a range refusal. Also unaffected: `CorrectionTests.cs:335`, `NoteTests.cs:381/393`, `RecentNotesTests.cs:189` and `ReportAccessTests.cs:94/148`. All keep empty lists under the stated rule. `RecordingAccessTests.cs:205` (record not found) also keeps its list. |
| MINOR-2 (`CoverageTests.Ports` lacks `IDemoPauseStore`) | Resolved | T093 (:466) adds `IDemoPauseStore` to `Ports` (`CoverageTests.cs:20`, confirmed) and to the class's doc comment. It names both coverage theories, the count 14 to 18 (`CoverageTests.cs:114`) and the `Assert.Equal(10, …)` at `CoverageTests.cs:96`, with an explicit choice of 14 or keeping 10. |
| MINOR-3 (T088 does not reach the internal-type rule for `DemoPauseStore`) | Resolved | T088 (:455) adds `[InlineData("HT.Persistence.Habits.DemoPauseStore")]` to `TheDemoAccountWriterAndTheDemoReportStoreAreInternal` (`PersistenceDependencyTests.cs:47-50`, confirmed) and renames it and its doc comment. T087 (:454) makes `DemoPauseStore` `internal sealed`, like `DemoReportStore.cs:15`. |
| MINOR-4 (research.md R-06 still says `CurrentWeekUpcoming` leaves out Paused entries) | Resolved (in tasks.md) | T036 (:72) tells the implementer to correct that bullet of R-06 in the same task, citing R-07 and critic pass-1 MAJOR-4. The bullet is still at research.md:222 until T036 runs, which the Conventions (:59-61) allow. |
| MINOR-5 (two factual slips in T016 and T088) | Resolved | T016 (:20) now makes the grep's output the list. Its parenthetical list matches the grep's 17 files exactly (re-run: `EntryViews`, `RecordingService`, `RecordingText`, `ExampleGoal`, `GoalEvaluation`, `ReportService`, `ReportText`, `Evaluator`, `Consistency`, `Streaks`, `HistoryCard`, `TodayPanel`, `CommitmentRow`, `ConsistencyStrip`, `GoalItem`, `StatusChip`, `SmokeCheckRunner`). T088 no longer contains the false exclusion sentence. |

No older note (pass 1) has regressed:
- `RecordingText.State`'s arms are in T016.
- `FaultingRecordStore` is in T028 and the commit group.
- `RecordStoreWrite.Exempt` and `DemoPauseLoadingOutcome` are on the allowed list in the Conventions.
- T036 still passes the current week's Paused Upcoming commitments.
- T100 keeps `implementing`.

## Notes

### MINOR-1: The visitor Home's six-entry legend is recorded only as a note inside PageHomeVisitor.md's slice-6 state, so its preview and the spec still give four entries
- **Where**: tasks.md T009 (:7: "so the visitor Home's example card shows the same six: note this in `design/pages/PageHomeVisitor.md`'s slice-6 state"); T011 (:9, which updates `pages.md` and `README.md` only for the goal page, the Goals page and the signed-in Home); the task list's own "Design first" rule (Phase 2 intro: a slice-7 state is added "as a 'Slice 7 state' block after the slice-6 one").
- **Against**:
  - constitution IV ("Pages MUST follow the structure and wording of their previews in `design/pages/`"; a visual change is made in `design/` first);
  - `design/pages/PageHomeVisitor.html:130`, the slice-6 preview block, whose legend lists Met, Failed, Missed, Pending;
  - `design/pages/PageHomeVisitor.md:27`, the slice-6 state ("with its legend (Met, Failed, Missed, Pending)");
  - slice 6 spec HP-004 (`specs/007-personal-reports/spec.md:726`, same four-entry legend);
  - slice 7 spec "Earlier requirements and tests this slice changes" (spec.md:680-701) and "Design changes before build" (spec.md:703-721), neither of which names the visitor Home.
- **Continues**: new. It follows from the T042 fix of pass-2 MAJOR-1.
- **Reasoning**:
  - T042 changes the visitor Home's example card for every visitor. The test edit for that change is now in place (`HomeVisitorTests.cs:116`).
  - The design record of the change is a note put inside the *slice-6* state of the .md. That goes against the tasks' own rule of adding a separate slice-7 state block. It also leaves the slice-6 description (line 27) contradicting the note.
  - The HTML preview that the built page must follow (constitution IV) keeps its four-entry legend at line 130. T011's structure update leaves the visitor Home out.
  - The slice-7 spec lists every earlier slice-6 requirement it amends (HP-001 to HP-003 of slice 6, the startup and smoke-check lines), but not slice 6 HP-004's visitor legend.
  - So, after implementation, the visitor Home differs from its preview and from slice 6's spec. The only record is a note in a block that describes an earlier slice. This is a residue of "silent divergence" (constitution Governance). It does not break a slice-7 requirement.

### MINOR-2: T084 says `DemoPauses.For` builds the goals "as `DemoHistoryGoal`s", while data-model.md and research R-08 return `DemoPauseGoal`s
- **Where**: tasks.md T084 (:452: "`DemoPauses.For(DateOnly runDate)` builds FR-031's two goals as `DemoHistoryGoal`s (see `DemoReport.cs` and `DemoHistory.cs` for the shapes) with their records, notes, excusals and pauses").
- **Against**: data-model.md:179 (`DemoPauseGoal` is a sealed class with `DemoHistoryGoal History`, `IReadOnlyList<DemoExcusal> Excusals` and `IReadOnlyList<DemoGoalPause> Pauses`); data-model.md:180 (`IDemoPauseStore` takes `IReadOnlyList<DemoPauseGoal>`); research.md:279 ("builds FR-031's two goals as `DemoPauseGoal`s, each a slice 6 `DemoHistoryGoal` with its `DemoExcusal`s and `DemoGoalPause`s").
- **Continues**: new.
- **Reasoning**:
  - A `DemoHistoryGoal` has no excusals or pauses, so "as `DemoHistoryGoal`s … with their … excusals and pauses" cannot be built literally.
  - The same task creates `DemoPauseGoal.cs`, and the Conventions make data-model.md authoritative, so a careful implementer will probably pick the right type. The task text still contradicts the contract it cites about the return type of a public member, and T097 compares public signatures against data-model.md.

## Summary
0 MAJOR, 2 MINOR. Every note of pass 2 is resolved and confirmed against the code:
- T042 now changes the three page and component tests that the full strip's legend reaches. No other full strip, legend assertion or substring check exists in the UI or host tests, so the Phase 2 shared checkpoint can be green as written.
- T034 now places the exemption read so that slice 5's "no store read" and access-refusal tests keep their empty call lists. It names the one `RecordTests` list that changes.
- T093 and T088 add the missing fences for the demo pause port and store.
- T036 carries the R-06 correction, and T016's and T088's slips are gone.

The two remaining notes are documentation residue. The visitor Home's legend change is recorded only in a slice-6 design note, not in its preview or the spec. T084 names the wrong demo goal type. Neither blocks `/speckit-implement`.
