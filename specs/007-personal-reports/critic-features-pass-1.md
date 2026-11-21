# Critic pass 1: features for 007-personal-reports

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: tests/HT.AcceptanceTests/Features/F007/personal-reports.feature (67 scenarios), against specs/007-personal-reports/acceptance.md (AC-001 to AC-067, Conventions, Coverage) and specs/007-personal-reports/spec.md; translation report specs/007-personal-reports/ac/runs/translate-001/translation-report.md read as context only

What was checked, so the next pass does not repeat it:

- **Fidelity, mechanically.** Every item was compared with the scenario of the same `@AC-` id, step by step and cell by cell, after removing list markers and Markdown table separators. The result: 67 items and 67 scenarios, none missing or extra. All titles are verbatim. Every tag list equals the item's references, in the same order. The only differences are syntax normalisation:
  - one-column data tables with the headers `entry`, `goal` or `recent note` (AC-033, AC-035, AC-036, AC-045, AC-054, AC-066), whose rows are the nested bullets, verbatim and in order;
  - `<n>` written as the row's value inside the Examples cells of AC-039 and AC-041, which is the same meaning under the acceptance.md "Examples" convention.
- **Outlines.** Every placeholder has a column, and no Examples row is a duplicate. The only unused columns are `note` (AC-003) and `n` in the reset-database items. No plain `Scenario` contains a placeholder.
- **Provenance and file format.** `ac/sources.lock` hashes match acceptance.md and spec.md as they are. The file is UTF-8 with no BOM and LF line endings, which matches `.gitattributes` and F001 to F005. It has no tabs, and its only non-ASCII character is the en dash of week ranges.
- **Coverage.** The acceptance.md Coverage table and the scenario tags agree both ways: every listed item carries the tag, and every tag is listed. The FR-001 to FR-016 and FR-030 to FR-032 rows and the user-story scenarios of spec.md are covered or listed as page-only.
- **Expected values, recomputed independently from the Given steps, the conventions and spec.md.** Grace ends were computed in UTC, Europe/Kyiv (UTC+2) and Europe/Berlin across 2026-10-25 and 2026-03-29. The items checked are:
  - AC-001, AC-005, AC-007, AC-020 and AC-021 to AC-038;
  - AC-042 to AC-045, against the demo4 data;
  - AC-048, AC-049, AC-050, AC-053, AC-054 and AC-066, against the demo5 data with R a Monday, Tuesday, Wednesday or Sunday;
  - AC-056, AC-057, AC-061, AC-062, AC-064, AC-065 and AC-067.

  Every value follows. Every goal's start date is on or after its creation date in the account's time zone.

## Notes

### MINOR-1: Two scenarios supply times that go back between steps
- **Where**: personal-reports.feature, AC-027 (lines 316-318) and AC-040 (lines 474-477).
  - AC-027: a Given step records on the `Screen` goal "at 2026-11-18T09:00:00Z". The When step after it then reads the `Reading` and `Sugar` reports "at 2026-11-05T10:00:00Z".
  - AC-040: line 474, "`f7.ac040.owner@example.com` records 20 for 2026-11-16 and saves the note …", gives no time. Under the convention it runs at the When step's time, 2026-11-18T10:00:00Z. The next Givens are registration with no time, which runs at the earliest time the item gives (2026-11-09T08:00:00Z), and line 476, "at 2026-11-09T08:00:00Z … created a goal".
- **Against**: acceptance.md Conventions "Time" (lines 7-14: steps run in the order written, each at its own or a convention-given time); spec.md FR-008 (lines 555-558: figures are computed "from the records as they are at the supplied time"); critic-ac-pass-2.md MINOR-3, which raised the same pattern for AC-063. AC-063 was then reordered so that its When step no longer goes back.
- **Reasoning**: no expected value is wrong. In AC-027 the records entered at 11-18 belong to the `Screen` goal, which is read at 11-18T10:00:00Z, so the `Reading` and `Sugar` reports at 11-05 are not affected. In AC-040 the earlier-timed steps belong to another user. Still, these are the only scenarios whose supplied time goes backwards in step order, and AC-027 does it with explicit times. The conventions neither allow nor forbid this. The feature file does not flag it, and the translation report mentions AC-027's two times only as "kept as one When step". So the step definitions must supply each time per call and keep no clock that only moves forward. That is a feasibility risk for /speckit-ac-tests-write, not a requirement violation. It is also the same class of issue that the AC review treated as worth fixing in AC-063.

## Summary

0 MAJOR, 1 MINOR. The feature file is a faithful translation of the locked acceptance criteria. All 67 items have a scenario with the same title, tags, steps and values; the only changes are Gherkin syntax (data tables, Scenario Outlines, and `<n>` written out in the Examples cells, as the acceptance.md convention allows). The coverage tags match the Coverage table exactly, the source hashes match the lock, and the recomputed expected figures agree with spec.md, including the time-zone and clock-change boundaries and the demo4/demo5 data. The one note is a risk for the step-definition writer: supplied time goes backwards in AC-027 and, through the convention, in AC-040. It changes no expected outcome. The feature is ready to be locked.
