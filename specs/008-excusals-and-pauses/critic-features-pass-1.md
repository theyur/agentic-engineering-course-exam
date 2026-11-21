# Critic pass 1: features for 008-excusals-and-pauses

**Verdict**: APPROVE
**Date**: 2026-11-21
**Reviewed**: tests/HT.AcceptanceTests/Features/F008/excusals-and-pauses.feature (against specs/008-excusals-and-pauses/acceptance.md, spec.md and ac/runs/translate-001/translation-report.md)

Verification done for this pass (read-only):

- A mechanical comparison of every `### AC-NNN` item in acceptance.md with the scenario tagged `@AC-NNN`: 88 items, 88 scenarios, none missing or extra. In every scenario the reference tags, title, step text (keyword included) and Examples rows (column names and cell values) are identical to the item. The outline/plain choice matches whether the item has an Examples table. No meaning was added, changed or dropped.
- The scenario tags per FR / US reference are exactly the sets of the acceptance.md Coverage table, and every FR in spec.md appears there.
- `sources.lock` hashes match the current acceptance.md (`9ee6a503…`) and spec.md (`53cd904c…`).
- The file is UTF-8 without a BOM, LF only (as `.gitattributes` requires for `Features/**/*.feature`), ends with a newline, uses no tabs and has no duplicate scenario titles.
- Checked the dates, states, grace ends, check orders and figures of every scenario against spec.md FR-001 to FR-031 and "Earlier behaviour" (for example AC-007/AC-018 grace instants, AC-028 check order, AC-030 Kyiv offset, AC-053 to AC-058 figures, AC-077/AC-078 demo figures for R = 2026-11-16 to 2026-11-22). No wrong value found.

## Notes

### MINOR-1: FR-016's Reduce half has no scenario and is not listed as unchecked
- **Where**: feature, trend scenarios AC-056, AC-060 to AC-065, AC-083 (lines 591-682, 894-900). The acceptance.md Coverage row for FR-016 has no note.
- **Against**: spec.md FR-016: "the week's value (Build and Reduce) counts only the "Yes" records of those commitments, so a "Yes" kept on a Paused commitment (FR-009) is not counted". slice 6 FR-006 (specs/007-personal-reports/spec.md:525-529) gives a Reduce commitment-based yes/no goal the target 0, so its value rule is a separate case.
- **Reasoning**: every trend scenario uses a Build goal (`Yoga, 3 days a week`, `Yoga, 3 times a week`, `Meditation…`, `Drinking water…`). The acceptance conventions define no Reduce yes/no commitment-based goal. So nothing checks that a Reduce week's value leaves out "Yes" records on Paused or Excused commitments, or what a Reduce week whose only records sit on Paused commitments shows. The Coverage table lists FR-016 as covered with no caveat, while other partial covers (FR-015, FR-017, FR-018, FR-025, FR-031) are noted.

### MINOR-2: FR-005's "before step (5), the value" ordering is untested
- **Where**: feature AC-022 (lines 234-246), AC-021 (226-232), AC-034 (361-367), AC-040 (416-422).
- **Against**: spec.md FR-005: the steps (4a) to (4c) come "after its step (4) (the date) and before its step (5) (the value)". slice 5 FR-005 step (5) gives value refusals such as "Enter a whole number from 0 to 9999." (specs/005-recording-and-evaluation/spec.md:506-509).
- **Reasoning**: every record attempt on a paused, Paused-week or excused date uses a valid value (`Yes` or `8`). AC-022 checks only the "after the date check" half, as its title says. An implementation that checked the value before (4a)/(4b)/(4c) would pass all F008 scenarios. The Coverage row for FR-005 (AC-021, AC-022, AC-034) does not say this ordering is left out.

### MINOR-3: The long-pause edge case contradicts FR-013, and the feature leaves the conflict open
- **Where**: feature AC-088 (lines 939-946). acceptance.md, edge-case list "Long pauses".
- **Against**: spec.md Edge Cases: "a goal paused for months shows "N/A" and its old streak unchanged when it resumes". spec.md FR-013 and "Earlier behaviour", Consistency: consistency counts the final results of the current attempt.
- **Reasoning**: AC-088's goal has five Met final results (2026-08-03 to 2026-08-07) before its pause. Under FR-013 its consistency at 2026-11-16 would read "100% (5 of 5)", not "N/A". The scenario checks only the streaks and the pause, so it neither supports nor contradicts the edge case. acceptance.md says why it does not check it, but spec.md still states both rules. Planning and implementation inherit two incompatible statements, and no acceptance test decides between them.

### MINOR-4: Some expected values depend on slice 6 rules that spec.md only cites
- **Where**: feature AC-060 (lines 633-640): four week bars including the begun week 2026-11-23 – 2026-11-29 with "no value". AC-062 (650-657): the current week's target 3, which counts the Upcoming commitments of 25 and 27 Nov. AC-070 and AC-077 (724-730, 825-841): the Missed count, which counts only grace ends in "this week".
- **Against**: spec.md "Earlier behaviour these tests use" says it restates the earlier rules "so that this spec alone gives the rules and texts its acceptance tests depend on". For the trend and dashboard it says only "as slice 6 gives them" (spec.md:120-121). The rules themselves are in slice 6 FR-006 (bars "that have begun", the last 12, the target "Upcoming ones of the current week included") and FR-012 ("Missed results whose grace period ended this week"), at specs/007-personal-reports/spec.md:521-583.
- **Reasoning**: the values are correct against slice 6. But they cannot be derived from spec.md alone, which the AC test writer and the fail judge rely on. acceptance.md also applies its "not restated in spec.md, so not checked" standard unevenly: it applies it to FR-015's progress and FR-018's order, but not to these trend and dashboard rules. A failure in these scenarios would have to be triaged against a spec that does not hold the rule.

## Summary
0 MAJOR, 4 MINOR. The feature file is a faithful, verbatim translation of the locked acceptance.md: all 88 items have a scenario, the steps, tags and Examples rows match exactly, the tags agree with the Coverage table, and the file meets the line-ending and encoding rules. The values checked against spec.md are correct. The minor notes are coverage gaps or spec-level tensions inherited from the inputs (FR-016 Reduce, FR-005 order against the value step, the long-pause "N/A", and unrestated slice 6 rules). None of them blocks locking the feature or the next step.
