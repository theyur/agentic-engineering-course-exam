# Critic pass 2: plan for 007-personal-reports

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Views cannot feed Home rows, Goals items, dialogs | Resolved | data-model `TodayEntry` now carries `ScheduleText`, `Goal` (`GoalHistory`) and `Entry` (`HistoryEntry`); `OverviewGoal.Recordable` is a `GoalHistory`. These are exactly the `Recordable`/`Row`/`Editing` and `History`/`Row` parameters of `RecordDialog.razor` (lines 140-161) and `NoteDialog.razor` (lines 70-83); `GoalName` comes from `TodayEntry.GoalName` / `Item.Name`. R-06 and R-07 name the extracted `EntryViews.Of`/`HistoryOf`, which match `RecordingService.EntryView`/`HistoryOf` (lines 418-462). |
| MAJOR-2 Trend kind cannot tell minutes from counts | Partly | The target line is fixed: `TrendKind` is `Minutes`/`Count`/`RecordCount` and `TargetLine(TrendKind, int)` gives "Target {n} min" only for `Minutes` (R-05, R-13), so Sugar reads "Target 1". The titles and sub lines are still not determined by the inputs the contract gives them. Carried forward as MINOR-1. |
| MAJOR-3 Smoke check host test before the pages | Resolved | plan.md Work order: step 5 says "The smoke check waits for step 7", step 7 comes after step 6 (Pages) with `SmokeCheckHostTests`, `RunnerBehaviourTests` and `CoverageTests` in the same change; R-11 "Order". |
| MAJOR-4 Goals-page Record mode changed without the owner | Resolved | R-07, R-12 and contracts/host.md "Goals page" now open the RecordDialog in its page-head mode with `Recordable` and the "For" select, which is the `Row is null` branch of `RecordDialog.razor` (lines 44, 63, 191). plan.md "Spec discrepancies" no longer contains the auto-chosen fixed mode. |
| MINOR-1 GoalListItem component name and GoalItem's fate | Resolved | R-12 "Shared components" and contracts/host.md "Goals page": the existing `GoalItem` is extended to the full item, keeps its name, and the reduced form is removed; `GoalsList` is its only user. |
| MINOR-2 StatusChip "Upcoming" word has no source | Resolved | R-13 and the contract add `ReportText.State`, which gives "Upcoming" and otherwise delegates to `RecordingText.State`, which stays unchanged (so `RecordingTextTests.Upcoming_has_no_text_in_this_slice` keeps passing). R-14 lists `StatusChipTests`. The modifier class named for it is a new problem (MINOR-2). |
| MINOR-3 `PendingLeft` missing and nothing to delegate to | Resolved | contracts/application-api.md now lists `PendingLeft` and a new public `RecordingText.HoursLeft`, which `ForLabel` calls (R-13). The rounding in `RecordingText.ForLabel` (lines 196-199) is the rule moved. |
| MINOR-4 Not every bar has the same target | Resolved | R-05 and plan.md "Spec discrepancies" 2 now state per-bar targets for commitment-based yes/no Build goals, keep `TrendBar.Target`, draw one line at the latest bar's target, and name each differing bar in the tooltip and `aria-label` (AC-025's 2, 3, 3). |
| MINOR-5 Visitor strip does not fit GoalStats/StripLabel | Resolved | `StripLabel(IReadOnlyList<EvaluationState>, StreakUnit)`; GoalStats takes a list of strip states, fed by `Recent.Results` states or `ExampleGoal.Strip` (R-12, data-model `ExampleGoal`). |
| MINOR-6 Where the smoke check matches its texts | Partly | R-11 and contracts/host.md now scope the texts to cards and state check 7's output line, but the named reader cannot find two of the three scopes. Carried forward as MAJOR-1. |
| MINOR-7 R-14 misdescribes two architecture tests | Resolved | R-14 now relies on the generic `DomainAndApplicationMustNotReadTheClockOrTheCurrentCulture` and on `NoPublicOrProtectedSignatureHasANodaTimeType`, matching `ClockReadTests.cs` (line 49) and `PublicSurfaceTests.cs` (line 21); plan.md Constitution Check II says the same. |
| MINOR-8 Query counts understated | Resolved | plan.md Performance Goals and R-08: about 8 round trips (account, goal list, records and notes per goal) and 4 for a goal report. This matches `GoalStore.Read` (`AsSingleQuery`, one query) and `RecordStore.ReadAsync` (two `ToListAsync`, lines 135-143). |
| MINOR-9 Quickstart figures hold only on the creation day | Resolved | quickstart.md §3 now says to start from an empty development database and explains the different figures on an older one. The instruction's position is a new nit (MINOR-4). |
| MINOR-10 `StreakUnit.Of` on an enum | Resolved | R-01 and data-model: `StreakUnits.Of(GoalTarget)` on a static class, with the reason stated. |
| MINOR-11 Demo report result types not member for member | Resolved | R-09 and data-model now give `Outcome` (`DemoAccountOutcome`) and `Accounts` as `IReadOnlyList<DemoReportAccountResult>`, matching `DemoHistoryAccountResult.cs` (lines 19-30) and `DemoHistoryLoadingResult.cs` (lines 24-30). |

No older note has regressed.

## Notes

### MAJOR-1: The smoke check's card scoping names a reader that cannot find the GoalStats card or any StatTile
- **Where**: research.md R-11 (check 6: "`PageReader.ReadCardText(html, "Consistency and streaks")`" and "'Due now', matched only in the Home's first StatTile"; check 7: "weeks" in the GoalStats card, "Missed this week" in its StatTile); contracts/host.md "Smoke check" table; plan.md Project Structure (`tools/HT.SmokeCheck/` lists only check 6, check 7 and the coverage entries).
- **Against**:
  - `tools/HT.SmokeCheck/PageReader.cs`: `ReadCardText` (lines 81-104) matches only `<section class="ht-card">` (`CardSectionPattern`, line 178) whose first `<h2 class="ht-card__title">` has the title (`CardTitlePattern`, line 181).
  - `design/pages/PageGoalDetail.html` (lines 20-22) and `design/components/GoalStats.html` (lines 9-11): the GoalStats card is a `<div class="ht-card">` with an `<h3 class="ht-card__title">`.
  - `design/components/StatTile.html` (lines 10-13) and StatTile.md: a tile is a `div.ht-card.ht-tile` with a value and a label, and no title.
  - spec HP-008 and SC-006; constitution IV (design fidelity); `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs` (the reader's own tests).
- **Continues**: MINOR-6 (pass 1).
- **Reasoning**:
  - If GoalStats follows the design, `ReadCardText(html, "Consistency and streaks")` returns null. Check 6 then finds neither "Consistency" nor "commitments", and check 7 does not find "weeks". `SmokeCheckHostTests` would fail at Work order step 7, and so would the hosted check after deployment (ADR 0015).
  - A StatTile has no `section` and no title, so no existing `PageReader` member can scope "Due now" or "Missed this week" to "the Home's first StatTile" or "its StatTile".
  - The fix needs one of two things the plan doesn't list. Either new `PageReader` members, with their `PageReaderTests`, or markup that departs from the design (`section` and `h2` for GoalStats). Neither appears in R-11, the Project Structure, or Work order step 7.
  - Even where `ReadCardText` does find the card, the title "Consistency and streaks" already contains "Consistency". The text check therefore proves only that the card is present.
  - As written, the tasks would either produce a failing check or have to invent the reader.

### MINOR-1: The trend title and sub-line functions still lack the input to tell some goal kinds apart
- **Where**: contracts/application-api.md `ReportText.TrendTitle(TrendKind kind, TrendSpan span, Measurement measurement, string? countUnit)` and `TrendSubLine(TrendSpan span, int bars)`; research.md R-13 (titles "Minutes per day", "Times per week", "Days done per week"; sub lines "last 14 days", "last 12 weeks"); data-model `TrendView` (no schedule kind or days).
- **Against**:
  - spec "Design changes before build", TrendChart: "Minutes per day · last 14 days", "Times per week · last 12 weeks", "Days done per week · last 12 weeks".
  - spec HP-001 ("the title and sub line per goal kind as the design gives them") and HP-006 ("Minutes per day, last 14 days; target 20").
  - R-05 / data-model `TrendKind`, which gives `RecordCount` to both per-week periods and commitment-based yes/no goals.
- **Continues**: MAJOR-2 (pass 1), second bullet.
- **Reasoning**:
  - A per-week yes/no goal (Strength training, Yoga) and a commitment-based yes/no goal (Mon · Wed · Fri) both reach `TrendTitle` as (`RecordCount`, `Week`, `YesNo`, null). One function cannot return "Times per week" for the first and "Days done per week" for the second.
  - "Minutes per day · last 14 days" can only describe a commitment-based duration goal on all seven days, such as demo4's Reading. A Build per-day duration period is `RecordCount`, not `Minutes`. `TrendSubLine(Commitment, 14)` gets the same input for Reading and for Walking (Mon, Wed, Fri). "last 14 days" is right for Reading and wrong for Walking, whose 14 bars span about 4.5 weeks.
  - As the contract stands, the design's example texts cannot all be produced.

### MINOR-2: The StatusChip's Upcoming modifier class is not in the design and breaks its naming pattern
- **Where**: research.md R-12 ("Its `ht-chip--upcoming` modifier comes from the design").
- **Against**:
  - `design/components/StatusChip.md`: every state class is `ht-status--{state}`, and the upcoming chip "Due Mon" uses `ht-status--due`.
  - `design/css/ht-components.css` and `src/HT.Ui/wwwroot/css`: no class contains "upcoming".
  - spec HP-006 ("tokens and `ht-` classes only"); constitution IV.
- **Continues**: new.
- **Reasoning**:
  - The class is said to come from the design, but neither the design nor the CSS has it.
  - It also uses the `ht-chip--` prefix, which the design keeps for chip variants such as `ht-chip--strong`, not for states.
  - Step 1's design list ("StatusChip 'Upcoming'") doesn't say a new class or CSS rule is added. `StatusChip.razor`'s `Modifier` switch (which throws for Upcoming) needs one.

### MINOR-3: How the Home and Goals islands get their data is unstated, and "the same result" cannot be passed to them
- **Where**: research.md R-12 "Signed-in Home" ("`GoalSummary` and the 'Recent notes' card are static components fed by the same `GetDashboardAsync` result during prerendering"), "Goals page"; contracts/host.md "Routes".
- **Against**:
  - `src/HT.Ui/Components/Records/GoalDetailBody.razor` header: an island is prerendered "with the goal identifier as its only parameter" and reads its own use cases through `UseCaseScope`.
  - `src/HT.Domain/Goals/GoalTarget.cs` line 15: a private constructor. It is carried by `GoalHistory.Target`, so by `TodayEntry.Goal` and `OverviewGoal.Recordable`.
  - ADR 0020 / HP-009: "within one read".
- **Reasoning**:
  - Parameters that a static SSR page passes to an interactive island are serialized for the circuit. A `Dashboard` or `GoalsOverview` that contains `GoalTarget` cannot be serialized that way. So the island must call the use case itself, as `GoalDetailBody` does.
  - Then the static cards and the island do not share "the same" result. A Home visit reads the dashboard at least three times: the static page, the island's prerender, and the island's interactive start.
  - The artifacts say neither which component calls `GetDashboardAsync` and `GetGoalsOverviewAsync`, nor what the islands take as parameters. The phrase "the same `GetDashboardAsync` result" suggests a hand-off that would fail at run time.

### MINOR-4: The quickstart says to drop the database after the command that starts the app
- **Where**: quickstart.md §3: the `dotnet run` block and "Open `https://localhost:7194`" come first. The paragraph "Start from an empty development database, by dropping the database …" follows them.
- **Against**: quickstart.md §3's own premise (the figures hold only when `demo4` and `demo5` were created today); ADR 0021 Consequences.
- **Continues**: MINOR-9 (pass 1), now resolved in substance.
- **Reasoning**: read top to bottom, the guide starts the app on the existing database, which creates nothing new. Only afterwards does it say to drop the database. The required order (drop, then start) is not the order written.

## Summary

1 MAJOR, 4 MINOR.

The revision resolves 13 of the 15 pass-1 notes:
- The dashboard and overview views now carry slice 5's own `GoalHistory` and `HistoryEntry`, so rows and dialogs need no extra read.
- The target line uses a three-way trend kind.
- The smoke check comes after the pages.
- The Goals-page dialog keeps slice 5's page-head mode.
- The architecture-test, query-count, result-type and naming notes are fixed and check out against the code.

One MAJOR blocks `/speckit-tasks`. R-11 scopes the smoke check's new texts with `PageReader.ReadCardText`, which matches only `section`/`h2` cards. The design's GoalStats card is a `div`/`h3`, and StatTiles have no title at all. Followed as written, checks 6 and 7 would fail in `SmokeCheckHostTests` and on the hosted demo, unless the tasks add reader members and tests that the plan doesn't list.

The MINOR notes:
- The trend title and sub-line signatures cannot produce every design text.
- An undefined chip class.
- The island data flow.
- A quickstart ordering nit.
