# Judge report — F008 / run-002 / analyze-1

## Summary

| Feature/AC | Scope (current / regression) | Verdict | Confidence | One-line reason |
|---|---|---|---|---|
| F008/AC-061 | current | SPEC_AMBIGUITY | high | The reviewed item expects amount bars (value 8, target 8) for a Build count per-day goal. Slice 6 FR-006, which F008's spec adopts, says those bars count the records that reach the amount against N (here 1). |

Totals: current 149 scenarios, 148 passed, 1 failed. Regression 1060 scenarios, all passed.

## Details (for the user)

### F008/AC-061 The trend of a per-day goal marks its Paused days

- Failing step: `Then the trend is, with the target 8: (2026-11-06, 8), … (2026-11-15, 8), (2026-11-16, Paused), … (2026-11-19, Paused)`. It fails on the first bar, (2026-11-06, 8), at the value check. The bar dates matched the 14 written dates, since the list check runs first.
- What the test does: it registers the user and creates "Drinking water, 8 glasses a day" as the acceptance conventions define it (Build, count 8 `glasses`, 1 time per day; `F008Text.cs` line 142). It pauses the goal at 2026-11-16T09:00:00Z from 2026-11-16 to 2026-11-22 and records 8 for each date from 2026-11-02 to 2026-11-15, at the When step's time as the conventions require. It then reads the goal report at 2026-11-19T10:00:00Z. `F008Checks.Trend`/`Bar` checks the bars as an exact ordered list: each bar's dates, its value against the Gherkin literal (8), its mark, and then the target (8). The expected values are literals from the Gherkin, the API calls match the steps, and the check is neither stronger nor weaker than the step.
- What the AC item / scenario / spec requires:
  - The `.feature` scenario matches acceptance.md AC-061 word for word, so there is no translation fault.
  - AC-061 expects the value 8 and the target 8 for every recorded day.
  - F008 spec, "Earlier behaviour these tests use", says the trend works "as slice 6 gives them, extended by FR-015 to FR-019". FR-016 only adds the Paused/Excused marks and the yes/no week rules. It does not change how a per-day count bar gets its value.
  - Slice 6 (F007) FR-006 says that for a per-day goal the bar value is "the day's progress (slice 5 FR-014), the target as for a per-week goal" (N for Build). It also says: "Everywhere else the value is a *number of records* against a number: … for a Build count or duration period the records that each reach the amount, against N". Bars show amounts only for commitment-based count/duration goals and for Reduce count/duration periods.
  - "Drinking water, 8 glasses a day" is a Build count goal scheduled 1 time per day, so it is a per-day period, not a commitment. Under FR-006, a day with one record of 8 has the value 1 against the target 1. F007's locked AC-065 confirms this reading: a Build count 10 `pages`, 2 times per day goal has bars such as (2026-11-01, 1) against the target 2. That scenario passes in this run's regression set.
- Verdict and reasoning: SPEC_AMBIGUITY. The test faithfully does what the scenario says, and the scenario faithfully says what the reviewed item says. The item itself contradicts the spec it is meant to check: it treats this per-day Build count goal as if its bars were amounts. The goal's name ("8 glasses a day") and F008 spec's wording "count at least 8 glasses, once per day" make that misreading easy. The failure is consistent with the system following F007 FR-006. Calling it an implementation fault would mean the reviewed item overrides slice 6 FR-006, which F008's spec says is unchanged. The user must decide which side is right. The spec text is explicit, so the most likely fix is to the acceptance criterion.
- Suggested action: unlock F008's acceptance sources and revise AC-061. Keep the Paused marks for 2026-11-16 to 2026-11-19, but write the recorded days by FR-006's rule for a Build count per-day goal: the number of records reaching the amount, against the target N. Or, if amount bars are wanted, change the goal to a commitment-based count goal (for example "Reading, 20 pages daily" on fixed days, with matching values). Then lock again, translate, and run `/speckit-ac-tests-write` for F008. If the user decides instead that per-day count bars should show amounts, that changes slice 6 FR-006 and has to be specified (it would also contradict F007 AC-065).

<!-- WRITER-FEEDBACK:BEGIN -->
## Feedback for the test writer
No feedback.
<!-- WRITER-FEEDBACK:END -->
