# Critic pass 1: plan for Recording and evaluation

**Verdict**: REVISE
**Date**: 2026-11-18
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Notes

### MAJOR-1: `RecordValue.FromStored` is internal to HT.Domain, so HT.Persistence cannot call it
- **Where**: data-model.md, "Domain: records", `RecordValue` row (line 14: "The internal factory `FromStored` rebuilds a stored value"); data-model.md "Tables" `GoalRecords` (line 135-137: the mapping enforces exactly one of `Answer`/`Amount` and turns a bad row into a fault); research R-05 and R-06.
- **Against**: `tests/HT.ArchitectureTests/AcSurfaceTests.cs` (`SurfaceAssemblyGrantsNoInternals` and `SurfaceSourcesAndRootBuildFilesDoNotMentionInternalsVisibleTo`). These tests forbid `InternalsVisibleTo` on HT.Domain, HT.Application and HT.Persistence, and no project has one today (only `HT.Ui` grants to `HT.UiTests`). The data-model's own header (line 5) says "Names are the contract; members not listed are private".
- **Reasoning**: `RecordStore`, `RecordMapping` and `DemoHistoryStore` live in HT.Persistence and must rebuild `GoalRecord` values from `GoalRecordRow` for `StoredEntry` and `StoredEntries`. An `internal` member of HT.Domain is not visible to them, and the architecture gate forbids the usual workaround. The only public factory is `RecordValue.TryCreate(Measurement, …)`. It needs the goal's measurement, but no `IRecordStore` read member receives one (`ReadAsync(owner, goal, spans)`, data-model line 108). It also drops a wrong-kind value instead of turning it into the fault that line 135-137 promises. As written, the persistence mapping cannot compile. The fix has to change the public Domain surface or a port signature, and the AC reference export makes both public. That decision is not made in the artifacts, so implementation is blocked.

### MAJOR-2: The planned smoke check 6 does not confirm what SC-005 requires, but the plan says it does
- **Where**: plan.md Constitution Check, row VI (line 105: "smoke check 6 confirms them after every refresh (HP-008)", where "them" means Met, Failed, Missed, Pending and Due, and notes); research R-10 (lines 358-369); contracts/host.md "Smoke check" (lines 106-113).
- **Against**: spec SC-005 ("`demo4@example.com` shows at least one Met, Failed, Missed, Pending and Due result, a partial amount and a note; the smoke check confirms it after every deployment"); slice 3 SC-005 ("finds every shipped slice's demo data").
- **Reasoning**: check 6 looks only for "Met", "Failed" and "Pending" on the Reading page and "Failed" and "Missed" on the Yoga page. It never checks a Due result ("Due today" / "Due this week"), a partial amount ("12 min" or "3 sweets") or any demo note. A deployment that loses demo4's notes or partial records, or that renders no Due row, still passes. So SC-005 is not met as the plan specifies the check. HP-008 lists only these words, so the spec itself conflicts here. The plan follows HP-008, claims SC-005 and constitution VI as met, and does not raise the conflict.

### MAJOR-3: A scheduled run after 2027-01-31 still enters the `demo` environment, against ADR 0018 and HP-009
- **Where**: research R-11, Decision step 2 (lines 388-391: "Every later step is conditional on `skip != 'true'` … The run succeeds and touches neither Azure nor the `demo` environment"); contracts/host.md "Hosting workflow" (lines 129-131).
- **Against**: ADR 0018 Decision, "Daily refresh" ("a scheduled run on a date after 2027-01-31 does nothing and succeeds, so the trigger left in the workflow file after the teardown … never touches them or the `demo` environment"); spec HP-009 ("does nothing and succeeds"); `.github/workflows/deploy-demo.yml` line 49 (`environment: demo` at job level); `docs/operations/hosted-demo.md` lines 236-238 (teardown deletes the GitHub environment `demo`).
- **Reasoning**: the date gate is designed as step 0.1 inside the existing job, and that job declares `environment: demo`. GitHub creates the deployment against the environment, and evaluates its rules, when the job starts, before any step runs. Step-level `if:` conditions cannot prevent that. After the teardown has deleted the environment, a job that references it makes GitHub create it again. So the case ADR 0018 is written for (the trigger still in the file after teardown) recreates and touches `demo`, which contradicts the ADR. The plan presents this design as compliant, and the workflow has no offline test that would catch it (plan "Risks").

### MINOR-1: Existing tests that this plan breaks and does not acknowledge
- **Where**: research R-13 (rules added, none amended); plan.md Work order steps 3, 5 and 6 (lines 206-240), which list only the startup-log tests, `RunnerBehaviourTests.GoodSite()`, the coverage test and "the existing goal-page UI tests of slice 4".
- **Against**:
  - `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs:50-60` (`OnlyTheGoalTypesTheThreeHabitServicesAndTheProfileServiceMayDependOnTheGoalStorePort`). `RecordingService` in `HT.Application.Records` takes `IGoalStore` (research R-04 constructor), so this privacy fence fails.
  - `tests/HT.UiTests/Host/SmokeCheckHostTests.cs:33-35`, which asserts the exact list of check names, ending with the demo1 to demo3 lines. Check 6 adds a line.
  - `tests/HT.UiTests/Components/Pages/NewGoalPageTests.cs:193` (`Assert.DoesNotContain("partial", …)`), which HP-005's help line breaks.
- **Reasoning**: the pre-commit gate runs the architecture tests, and step 3/4's "verified and committed together" requires the full suite. All three fail without a change the plan does not name. The first one widens a constitution III fence (which types may use the goal store), and the plan should state that choice explicitly, not leave it to be found at commit time.

### MINOR-2: History read: one span or one span per entry is ambiguous
- **Where**: research R-07 step 2 (line 254-256: "reads the records and notes once for the span from the first entry's `FirstDate` to the last entry's `LastDate`") against data-model.md `IRecordStore.ReadAsync(owner, goal, IReadOnlyList<EntrySpan> entries)` (line 108: records "with dates in any span" and notes "whose key is one of the spans' keys").
- **Against**: FR-009 (each entry's note); internal consistency.
- **Reasoning**: if the service passes one covering span, as R-07 describes, the port returns only the note keyed at that span's `Key`, and every other entry's note is lost. If it passes one span per entry (up to 366), the store has to translate up to 366 date ranges, and R-07's "once for the span" does not describe that. The two artifacts describe different calls, and the choice affects the integration tests and the query shape.

### MINOR-3: Check 6's output line, its counts and the source of its expected names are not specified
- **Where**: contracts/host.md "Smoke check" (lines 106-113: "A missing word fails the check with its page and word, and no other value"); research R-10; data-model.md (no names helper for demo4).
- **Against**: `specs/003-demo-hosting/contracts/smoke-check.md` "Output" (`PASS <name> (<found> of <expected>)`, the ordered `FAIL` reasons, and "No … goal name … or page content is printed") and "Invocation" (expected values come from the deployed commit's `HT.Application`, as `DemoGoals.DisplayNames` does for demo1 to demo3).
- **Reasoning**: "its page" is not defined. If the check names the page by its goal, it prints a goal name, which slice 3 forbids. The `PASS` line format and the found/expected counts for check 6 are missing, so `SmokeCheckHostTests` and `RunnerBehaviourTests` have nothing exact to assert. No HT.Application member gives demo4's display names (`DemoHistory.For(R)` returns goals, not names), so the check would hard-code them in the tool, against slice 3's rule.

### MINOR-4: The host contract adds UI classes and wording that `design/` does not have
- **Where**: contracts/host.md "StatusChip" (tones success, danger, neutral-strong, warning, info: "The tones are the `design/components/StatusChip` classes"); "Dialogs", NoteDialog ("Edit note · {goal name}" with a note); "Goal page", Date ("16–22 Nov" for a week, via `DateTexts`); Recorded (each record's `aria-label` "Edit record of Tue 17 Nov").
- **Against**: `design/components/StatusChip.md` and `design/css/ht-components.css:64-72` (the classes are `ht-status--met`, `--failed`, `--missed`, `--pending` and `--due`; no tone names exist); `design/components/NoteDialog.html` (only "Add note · …"); `src/HT.Application/Accounts/DateTexts.cs` (no week-range formatter); constitution IV (design first; icon-only and repeated controls need usable names); the spec's "Design changes before build" list, which has no "Edit note" title.
- **Reasoning**: an implementer who follows the tone names invents classes or colors, which constitution IV forbids. "Edit note" is new wording that the design-first step does not cover. The week format for a week that spans two months ("30 Nov – 6 Dec"?) is undefined, and no formatter exists yet for HP-011 to unit-test. A per-day period with several records on one date gives several buttons with the same accessible name, so a screen-reader user cannot tell them apart.

### MINOR-5: The type `Evaluation` sits in a namespace also named `Evaluation`
- **Where**: research R-01 (`HT.Domain.Evaluation` with the static class `Evaluation`); data-model.md "Domain: evaluation"; plan.md Project Structure and R-14 (`HT.UnitTests/Domain/Evaluation`).
- **Against**: .NET naming guidance (CA1724, "type names should not match namespaces"); C# name lookup.
- **Reasoning**: inside any namespace under `HT.Domain`, or in the planned test namespace `HT.UnitTests.Domain.Evaluation`, the simple name `Evaluation` resolves to the namespace first, so `Evaluation.Evaluate(…)` does not compile. Every caller there needs a fully qualified name or an alias. The boundary tests of SC-002, the largest test set of the slice, live exactly there.

### MINOR-6: The 60-day schedule-disable limit is dismissed without checking the dates
- **Where**: research R-11 "Known limits" (lines 404-407: "The hosted demo ends before that could happen").
- **Against**: HP-009 (daily data until 2027-01-31); GitHub's rule that disables scheduled workflows in a public repository after 60 days without repository activity.
- **Reasoning**: the claim holds only if the repository has activity after 2026-12-02 (2027-01-31 minus 60 days). If the last push or merge falls before that date (for example, this slice is the last change), the schedule turns off before 2027-01-31 and HP-009 fails silently. The Operator guide's manual refresh is the fallback, but the plan states the risk as impossible instead of naming its condition.

### MINOR-7: The quickstart places the Missed Reading row on the first History page
- **Where**: quickstart.md section 3, step 2 (lines 70-73: "History shows, newest first: … Failed with 12 min …, and Missed with 'Away for the weekend.'"; then "'Show all N →' lists the rest").
- **Against**: FR-030 (R − 10 has no record) and HP-001 (the first 10 rows, newest first).
- **Reasoning**: the rows are R, R − 1, …, R − 9 (10 rows), so R − 10 is the 11th row. It appears only after "Show all N →". A manual check that follows the quickstart finds no Missed row on the first page and may report a defect that is not there.

### MINOR-8: Counts and orderings differ between the artifacts
- **Where**:
  - plan.md Scale/Scope (lines 87-88: "5 new components") against the Project Structure (lines 154-158: StatusChip, GoalDetailBody, HistoryCard, RecentNotesCard, RecordDialog, RecordFields and NoteDialog, which is 7);
  - research R-03 (line 96: "`GoalTimeline` gains two members", then three listed: `EntryOf`, `HasBegun`, `TryEntries`);
  - data-model.md `GoalHistory.Entries` "date order" (line 56) against `RecordableResult.History`, "whose `Entries` are the Due and Pending ones, newest first" (line 70).
- **Against**: internal consistency (review item 4).
- **Reasoning**: `/speckit-tasks` derives tasks and counts from these lists. One type, `GoalHistory.Entries`, documented with two opposite orders invites a page or test to sort it wrongly. The component and member counts understate the work.

## Summary

3 MAJOR and 8 MINOR notes. The plan covers the spec and the locked acceptance criteria thoroughly. I checked the evaluation rules, check orders, correction flag, FR-011 window, demo data for every weekday and the clock-change instants against AC-001 to AC-069, and found no contradiction there. Three points block a clean implementation:

- the persistence mapping depends on an internal Domain factory that the AC-surface architecture tests make unreachable;
- the smoke check as planned does not confirm SC-005's Due, partial amount and note, although the Constitution Check says it does;
- the date gate of the scheduled workflow still enters the `demo` environment, which ADR 0018 and HP-009 rule out.

The minor notes are three existing tests the plan breaks without saying so (one is a constitution III fence), two underspecified contracts (the history read and smoke check 6's output), and wording, naming and count inconsistencies to clean up before `/speckit-tasks`.
