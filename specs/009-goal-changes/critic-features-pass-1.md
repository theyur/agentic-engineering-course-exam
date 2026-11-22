# Critic pass 1: features for 009-goal-changes

**Verdict**: APPROVE
**Date**: 2026-11-22
**Reviewed**: tests/HT.AcceptanceTests/Features/F009/goal-changes.feature (against specs/009-goal-changes/acceptance.md, specs/009-goal-changes/spec.md and specs/009-goal-changes/ac/runs/translate-001/translation-report.md)

## Notes

No MAJOR or MINOR issues were found.

## Summary
0 MAJOR, 0 MINOR. I compared every scenario with the acceptance item that has the same `@AC-` id, using a script and checking the differences by hand. The script parsed both files and compared the tags, the scenario name with the item title, each step in order, and each Examples row with its cells trimmed.

- **Items and scenarios.** The file has 106 items and 106 scenarios (AC-001 to AC-106). No item is missing, no scenario is extra, and no id appears twice. Each scenario's tags are exactly `@AC-NNN` plus the references in the item's heading. Each scenario name equals the item title, and no two names collide, not even after punctuation is stripped.
- **Steps.** Every step is the item's bullet, word for word, with its keyword kept. Every scenario has exactly one Given, one When and one Then.
- **Outlines.** The 33 items with an `Examples:` table are the 33 `Scenario Outline`s, the same list the translation report gives. Every `<placeholder>` in a step names a column. The only unused column is `n` in AC-090 and AC-093, which is correct: those items reset the database, so by the Examples convention their demo emails carry no `<n>`. All rows have the same width, and no `n` value appears twice within an item. The outlines expand to 136 rows.
- **The one cell-value difference is not a defect.** In AC-084, AC-085, AC-086 and AC-104, each `action` cell has `<n>` replaced by that row's own `n` value. Gherkin does not replace placeholders inside Examples cells. The value used is exactly the email that the acceptance criteria's Examples convention defines, and the translation report records the change, so the meaning is unchanged. All other cells are verbatim, including the whitespace-sensitive quoted values in AC-012, AC-016 and AC-030, which keep their quotes.
- **File format.** The file is UTF-8 without a BOM, with LF line endings, as `.gitattributes` requires for `Features/**/*.feature`. It has no backslash escapes, comment lines or empty cells.
- **Hashes.** The hashes of `acceptance.md` and `spec.md` match `ac/sources.lock`, so the feature was generated from the locked sources.

Coverage of spec.md is the same as the acceptance criteria's. Every FR, every user-story scenario on the AC surface and every edge case listed in the acceptance Coverage table has its scenario. FR-028 and FR-029 are unused by the spec's own statement. The scenarios left out are page, host and accessibility outcomes outside the AC surface (HP-xxx). I also recomputed several figures against spec.md:

- AC-004: 83% (10 of 12)
- AC-025 and AC-026: 85% (12 of 14) and 92% (13 of 14)
- AC-088: 90% (18 of 20), streak 11
- AC-090 and AC-091: the demo7 dates and figures for R = 2026-11-18
- AC-099: the last date of a Sunday and a Monday ending

All of them agree.

The feature file is a faithful translation and is ready to be locked.
