# Critic pass 3: plan for 007-personal-reports

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: plan.md, research.md, data-model.md, quickstart.md, contracts/application-api.md, contracts/host.md

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Card scoping names a reader that cannot find GoalStats or a StatTile | Resolved | R-11 adds `PageReader.ReadStatLabels` (`.ht-stat__label`), `ReadTileLabels` (`.ht-tile__label`) and `ReadStatusChips` (`.ht-status`) with `PageReaderTests` cases. The same three members appear in plan.md Project Structure (tools line), Work order step 7 and contracts/host.md "Smoke check". The classes match the design: `design/components/GoalStats.html` (`ht-stat__label`: "Consistency", "Streak, commitments"), `StatTile.html` and `PageHomeSignedIn.html` lines 13-16 (`ht-tile__label`), and `StatusChip.html` / `CommitmentRow.html` (`ht-chip ht-status`). The Home's GoalSummary and Recent notes cards carry no `ht-status` (`PageHomeSignedIn.html` lines 25-32), so R-11's "only Today rows carry chips" holds. No `ht-stat__label` exists elsewhere in `src/HT.Ui` today, so the goal page has no other stat label that could match. Matching against labels instead of the card title also removes the old "title contains Consistency" weakness. One matching detail is left open (MINOR-2). |
| MINOR-1 Trend title and sub-line functions lack the input to tell goal kinds apart | Partly | The contract is fixed. `ReportText.TrendTitle(GoalTarget, TrendKind, TrendSpan)` and `TrendSubLine(GoalTarget, TrendSpan, int maxBars)` (application-api.md lines 100-101, R-13) now get the schedule kind, the fixed days, the measurement and the count unit from `GoalTarget` (`src/HT.Domain/Goals/GoalTarget.cs` lines 26-46). That separates "Times per week" from "Days done per week", and "last 14 days" from "last 14 commitments". The exact wording is deferred to the step-1 TrendChart design, as HP-001 ("as the design gives them") allows. R-05 and data-model.md still describe the old inputs (MINOR-1). |
| MINOR-2 StatusChip Upcoming modifier class not in the design | Resolved | R-12 now uses `ht-status--due`, which `design/components/StatusChip.md` (States table) gives the upcoming chip ("Due Mon", "nothing to judge yet"). The word "Upcoming" separates it from Due. |
| MINOR-3 Island data flow unstated; "the same result" cannot be passed | Resolved | R-12 "Each island reads its own data" and contracts/host.md (Goals page, TodayPanel) say the islands call `GetGoalsOverviewAsync` / `GetDashboardAsync` in `OnInitializedAsync`, with the acting user from the authentication state. That matches `ActingUserAccessor` (`AuthenticationStateProvider`) and `GoalDetailBody.razor` (lines 11-12, 112-144). The reason given is the non-public `GoalTarget` constructor. The three dashboard reads per Home visit are stated and fit HP-009 / ADR 0020 ("within one read"). |
| MINOR-4 Quickstart drops the database after starting the app | Resolved | quickstart.md §3 now says to drop the database first, then shows "Then start the app" and the `dotnet run` block. |

No older note (pass 1) has regressed. These still hold:
- the work order puts the smoke check after the pages (plan step 7);
- the Goals-page dialog keeps slice 5's page-head mode (R-07, host.md);
- `TodayEntry` and `OverviewGoal` carry slice 5's `GoalHistory` and `HistoryEntry`;
- the coverage count rises from 10 to 14, which matches `SmokeCheckCoverage.cs` (10 entries) and `CoverageTests.cs` line 95.

## Notes

### MINOR-1: R-05 and data-model.md still name the old inputs of the trend texts
- **Where**: research.md R-05, last bullet: "The title and sub line come from `ReportText` with the kind, the span, the measurement and the count unit, which `TrendView` carries (R-13)". data-model.md `TrendView`: "`GetGoalReportAsync` fills the texts with `ReportText`'s functions of the kind, span, measurement, unit and target". It also still lists `Measurement` and `CountUnit` as members kept for that purpose.
- **Against**:
  - research.md R-13 and contracts/application-api.md lines 100-101: the functions take the goal's `GoalTarget`, and `TrendSubLine` also takes `maxBars`.
  - spec "Design changes before build", TrendChart ("Times per week" and "Days done per week").
- **Continues**: MINOR-1 (pass 2).
- **Reasoning**:
  - R-05 and data-model.md describe the input set that pass 2 showed to be insufficient. Kind, span, measurement and count unit cannot tell a per-week yes/no goal from a commitment-based yes/no goal. R-13 and the contract now use `GoalTarget`.
  - `TrendView` carries no `GoalTarget`, no schedule and no days. So R-05's claim that `TrendView` carries what the texts need is false under the new signatures.
  - The texts are filled inside `GetGoalReportAsync` (R-13), so nothing breaks. But three artifacts describe one function's inputs in two incompatible ways, and a task written from R-05 or data-model.md would rebuild the pass-2 defect.

### MINOR-2: Whether a smoke-check text matches a whole label or part of one is unstated
- **Where**: research.md R-11: "'Consistency' and 'commitments', matched only among the stat labels"; "'weeks' among the stat labels only ('Streak, weeks')"; "'Missed this week' among the tile labels". contracts/host.md "Smoke check" table.
- **Against**:
  - `tools/HT.SmokeCheck/PageReader.cs` lines 134-141: `ContainsAll`, the existing helper for list readers, compares whole strings (ordinal set membership).
  - The design labels themselves (`GoalStats.html`): "Consistency", "Streak, commitments", "Longest, commitments".
- **Continues**: new.
- **Reasoning**:
  - "Consistency", "Due now", "Missed this week" and "Missed" are whole label or chip texts. "commitments" and "weeks" are only parts of the labels "Streak, commitments" and "Streak, weeks".
  - The new readers return lists. The plan doesn't say whether each text is compared with a whole element (as `ContainsAll` does for goal names) or found within one (as check 6's existing History-card texts are, inside `ReadCardText`'s string).
  - A whole-element match fails "commitments" and "weeks" on every page. `SmokeCheckHostTests` would catch that, so this costs a rework loop rather than a shipped defect. The rule belongs in R-11 and in the slice 3 contract's check rows that step 7 updates.

## Summary

0 MAJOR, 2 MINOR.

The revision resolves the blocking pass-2 note. The smoke check now names three leaf readers that match the design's real markup:
- `ht-stat__label` in GoalStats;
- `ht-tile__label` in StatTile;
- `ht-status` in StatusChip.

They are listed consistently in research R-11, contracts/host.md, the plan's Project Structure and Work order step 7, with their `PageReaderTests`. The StatusChip class, the island data flow and the quickstart order are fixed, and they check out against the design and the existing code (`GoalDetailBody`, `ActingUserAccessor`, `UseCaseScope`). The trend-text signatures now take the goal's `GoalTarget`, which gives them the schedule kind and days they lacked.

Two clean-ups remain:
- R-05 and data-model.md still describe the old trend-text inputs.
- R-11 doesn't say whether a text must match a whole label or may be part of one.

Neither blocks `/speckit-tasks`.
