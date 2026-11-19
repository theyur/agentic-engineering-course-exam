# Critic pass 1: features for Recording and evaluation

**Verdict**: APPROVE
**Date**: 2026-11-18
**Reviewed**: tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature (against specs/005-recording-and-evaluation/acceptance.md, spec.md and ac/runs/translate-001/translation-report.md)

Checks I ran (read-only):

- **Mechanical comparison.** I parsed every `### AC-NNN` item of acceptance.md (heading title, traceability references, steps, Examples rows) and every scenario of the feature file (tags, name, steps, Examples rows), then compared them.
  - Result: 69 items and 69 scenarios, the same ids, no missing or extra scenario.
  - Every scenario name equals its item's heading title.
  - Every tag set equals `@AC-NNN` plus the item's references.
  - Every step, with its keyword, is identical to the item's step.
  - Every Examples row is identical to the item's row, except the documented `<n>` substitution in the cells of AC-010 (`actor`), AC-043 (`actor`) and AC-047 (`goal`). There each cell differs only by `<n>` being replaced with that row's `n` value, which is what the acceptance.md "Examples" convention defines.
- **Gherkin structure.**
  - 42 `Scenario Outline`s, matching the 42 `Examples:` blocks of acceptance.md, on the same items.
  - Every placeholder in an outline's steps has a column. No plain `Scenario` holds a placeholder, and no cell still holds a `<…>`.
  - The only columns no step uses are `why` (AC-005, AC-069) and `n` (AC-061, AC-068, AC-069, the reset-database items whose emails are demo emails). They are kept verbatim and don't change any meaning.
  - Each scenario has exactly one Given, one When and one Then (69 each, plus 206 And).
  - No backslash escapes, `#` lines, tabs, non-ASCII characters or trailing whitespace. The file is LF only, as `.gitattributes` requires for `Features/**/*.feature`, and it ends with a newline.
  - Space-only texts (`"   "`, `"  New note.  "`, `"  Back was tired.  "`) survive Gherkin's trimming of a cell's outer whitespace, because the spaces sit inside the quotes.
- **Provenance.** The sha256 values of acceptance.md (`dc05f60b…`) and spec.md (`39f28f95…`) equal those in `specs/005-recording-and-evaluation/ac/sources.lock`. No `features.lock` exists yet, which is what I expect before this review.
- **Spot-checks of the translated values against spec.md.** I recomputed these and found no error:
  - instants and grace ends: AC-001, AC-012, AC-013, AC-021, AC-042, AC-045;
  - the Kyiv clock changes: AC-023, 2026-10-25 and 2027-03-28;
  - the time zone moves: AC-030, AC-031, AC-046 and AC-067, with Kyiv at UTC+2 and New York at UTC−5 in November;
  - the range count of AC-043: 366 and 367 days, 364 entries;
  - the recent-note states of AC-040 and AC-041;
  - the demo data of AC-052 to AC-057 and AC-068 against FR-030: R − k dates, even and odd k for Sugar, weeks 1 to 3 and the extra week for R on a Monday, Wednesday, Sunday and Tuesday.

## Notes

### MINOR-1: The translation report misstates how AC-042 was translated
- **Where**: `specs/005-recording-and-evaluation/ac/runs/translate-001/translation-report.md`, Interpretations, 6th bullet ("AC-030, AC-040, AC-042, AC-054, AC-055: the 'Then … in this order:' line, followed by 'And …' lines, is kept as one `Then` step ending in a colon").
- **Against**:
  - acceptance.md AC-042, line 847: "Then it gives 14 entries, one for each date from 2026-11-16 to 2026-11-29, in date order, each from 00:00:00Z of its date to 00:00:00Z of the next date". There is no "in this order:" and no trailing colon.
  - The feature file, line 591, has the same text.
  - The only `Then` steps of the feature file that end in a colon are lines 417 (AC-030), 566 (AC-040), 787 (AC-054) and 798 (AC-055).
- **Reasoning**: The feature file carries AC-042's step verbatim, so the scenario is correct. Only the report's description of it is wrong: it names AC-042 among the "in this order:" items, and it leaves out the other items that also have a `Then` followed by `And` lines without a colon (AC-051, AC-052, AC-053). Anyone who uses the report to trace a translation decision for AC-042 is misled. The translation itself is not affected.

## Summary
0 MAJOR, 1 MINOR.

The feature file is a faithful translation of the locked acceptance.md:
- every item has its scenario, with the same name, tags, steps and Examples rows;
- the only change of content is the `<n>` substitution in example cells, which the acceptance.md conventions require;
- the Gherkin is well formed, LF-only and free of escape or trimming hazards;
- the source hashes match `sources.lock`.

The coverage of the spec's FRs, user-story scenarios and edge cases is the acceptance.md coverage, carried over one to one. The only items without a scenario are the four *(page)* scenarios (US1-AS9, US3-AS8, US4-AS7, US5-AS5), and acceptance.md marks them "Not testable through the API". The single note concerns the translator's report, not the feature file. The feature file is ready to be locked.
