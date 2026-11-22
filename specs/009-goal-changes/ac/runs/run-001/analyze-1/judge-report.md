# Judge report — F009 / run-001 / analyze-1

## Summary

| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F009/AC-092 | current | SPEC_AMBIGUITY | high | The AC item expects the goal name `Reading, 20 min daily` on 2026-11-19. Under FR-031 (version 3 with 25 minutes from R + 1) and FR-004 (the name follows the version in effect), that day's name is the 25-minute one. |

Totals: F009 has 209 scenarios: 208 passed and 1 failed. Regression has 1209 scenarios and all passed.

## Details (for the user)

### F009/AC-092 Demo goal-change loading leaves an existing `demo7@example.com` unchanged

- Failing step: `And at 2026-11-19T10:00:00Z the goals overview of `demo7@example.com` lists exactly `Reading, 20 min daily`, `Meditation, 5 min daily` and `Sugar, at most 1 sweet a day`` (goal-changes.feature line 1023). The goal-history step after it was skipped.
- What the test does: `ReportThenSteps.ThenTheGoalsOverviewListsExactly` sets the check time to 2026-11-19T10:00:00Z. It reads the goals overview as `demo7@example.com` and compares the set of goal names with the three backticked literals from the Gherkin (sorted, exact). It computes nothing from spec data. The setup steps match the item: earlier demo tasks run, then demo goal-change loading at 2026-11-18T10:00:00Z, then a second loading at 2026-11-19T10:00:00Z.
- What the AC item / scenario / spec requires:
  - The `.feature` scenario is word for word the same as AC-092 in `specs/009-goal-changes/acceptance.md`, so this is not a translation fault.
  - The item asks for the name `Reading, 20 min daily` on 2026-11-19, which is the day after the run date R = 2026-11-18. The second loading leaves the account unchanged ("already exists"), so R stays 2026-11-18.
  - The spec says otherwise:
    - FR-031: the demo "Reading" goal has "version 3 from R + 1 with 25 minutes, a change still waiting for the next boundary". It also says "its name follows FR-004". The label "Reading, 20 min daily" is given only for reading "at the run time".
    - FR-004: a goal without its own name is named by "the target text of its settings version in effect on its current date".
    - AC-091 confirms this: the later settings are "25 minutes … from 2026-11-19".
  - So on 2026-11-19 the version in effect is the 25-minute version, and the spec's name for the goal is the 25-minute name. AC-090 rows 04 to 07 pass because each row loads the demo on the same day it reads, so R is the reading day.
- Verdict and reasoning: SPEC_AMBIGUITY.
  - The test faithfully expresses the scenario, and the scenario faithfully expresses the reviewed item.
  - The reviewed item contradicts FR-031 together with FR-004 and FR-005: it reads the overview on R + 1 but still expects the name of R. What the system does matches the spec, not the item.
  - The item probably meant either to check the overview at the first run's time or to expect the name of the version that takes effect on R + 1. Only the user can decide which.
- Suggested action: revise AC-092 in `specs/009-goal-changes/acceptance.md` (unlock it, edit it, lock it again), then translate the feature and write its tests again. Two possible edits:
  - (a) Read the goals overview and the goal history at 2026-11-18T10:00:00Z. This is the run time, so the item still shows that the second run changed nothing.
  - (b) Keep 2026-11-19T10:00:00Z and expect the 25-minute name for the Reading goal.

  Check the goal-history step after the edit as well. It did not run here.

<!-- WRITER-FEEDBACK:BEGIN -->
## Feedback for the test writer
No feedback.
<!-- WRITER-FEEDBACK:END -->
