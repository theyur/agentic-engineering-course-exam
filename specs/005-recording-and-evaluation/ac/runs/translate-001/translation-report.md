# Translation report — F005 / translate-001

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). No remaining errors.

Feature file: `tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature` (one `Feature: Recording and evaluation`, tagged `@F005`).

## Items

| AC id | Status (translated / issue) | Scenario name |
|---|---|---|
| AC-001 | translated | A partial amount recorded for today is kept and the commitment is Due |
| AC-002 | translated | Recording again for a commitment replaces its one record |
| AC-003 | translated | A per-week period holds one record per recording, listed in date order |
| AC-004 | translated | Records on the same date of a per-day period keep the order in which they were stored |
| AC-005 | translated | A date that belongs to no commitment or period cannot be recorded |
| AC-006 | translated | A day that has not begun on the goal's timeline cannot be recorded |
| AC-007 | translated | A value that does not fit the goal's measurement is refused with its message |
| AC-008 | translated | The lowest and highest amounts are accepted and kept |
| AC-009 | translated | A value that does not belong to the goal's measurement is not stored |
| AC-010 | translated | Recording checks the acting user, the goal, the date, the value and the note in that order |
| AC-011 | translated | A note given with a record is trimmed |
| AC-012 | translated | A commitment is Upcoming before its start instant and Due from it |
| AC-013 | translated | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself |
| AC-014 | translated | A final Build duration commitment is Met at or above its target and Failed below it |
| AC-015 | translated | A final Reduce count commitment is Met at or below its limit and Failed above it |
| AC-016 | translated | A final yes/no commitment is Met on Yes for Build and on No for Reduce |
| AC-017 | translated | A final per-week Build yes/no period is Met with at least N Yes records |
| AC-018 | translated | A final per-week Build count period counts only the records that reach the amount |
| AC-019 | translated | A final per-day Reduce count period is Met when its total is within the limit |
| AC-020 | translated | A final per-day yes/no period counts its Yes records |
| AC-021 | translated | An ended commitment or period stays Pending during its grace period, even when its records reach the target |
| AC-022 | translated | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit |
| AC-023 | translated | Around a clock change the grace period lasts exactly 48 elapsed hours |
| AC-024 | translated | A record added to a Missed commitment is a correction that makes it Met |
| AC-025 | translated | Lowering the record of a Met commitment below its target is a correction that makes it Failed |
| AC-026 | translated | Deleting the only record of a final commitment is a correction that makes it Missed |
| AC-027 | translated | A change is a correction only from the instant its grace period ends |
| AC-028 | translated | A missing record added to a Failed week is a correction that makes it Met |
| AC-029 | translated | Results corrected after the grace period equal the same records entered on time |
| AC-030 | translated | After a time zone change, each commitment keeps the instants and grace period of its own settings version |
| AC-031 | translated | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period |
| AC-032 | translated | A record identifier that is not a record of the goal named in the call does not exist |
| AC-033 | translated | An evaluation period takes at most 50 records, but its records can still be changed or deleted |
| AC-034 | translated | A record belongs to the week of its date, not to the week of the instant it is saved |
| AC-035 | translated | A note can be added to a Missed commitment and leaves it Missed |
| AC-036 | translated | A note saved on its own replaces the note, and an empty one removes it |
| AC-037 | translated | A note given with a record replaces the note only when it has text |
| AC-038 | translated | A note has at most 500 characters |
| AC-039 | translated | Any begun date of a week names that week's one note |
| AC-040 | translated | The recent notes are the 5 with the latest dates, newest first, each with its state |
| AC-041 | translated | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those |
| AC-042 | translated | The history gives every commitment of the range in date order, each with its state at the supplied time |
| AC-043 | translated | The history's range has 1 to 366 days and is checked after the goal |
| AC-044 | translated | Progress is given at any state, as the goal's kind of target counts it |
| AC-045 | translated | What can be recorded now is every Due and every Pending commitment or period, newest first |
| AC-046 | translated | After a move east, the old and the new day are both Due while they overlap |
| AC-047 | translated | Another registered user gets "This goal does not exist." from every use case of this slice |
| AC-048 | translated | Another account's record identifier does not exist on one's own goal |
| AC-049 | translated | An administrator gets "Goals are for registered users." from every use case of this slice |
| AC-050 | translated | The anonymous visitor gets "You are not signed in." from every use case of this slice |
| AC-051 | translated | Demo history loading creates the demo account with history and its three demo goals |
| AC-052 | translated | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due |
| AC-053 | translated | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due |
| AC-054 | translated | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday |
| AC-055 | translated | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 |
| AC-056 | translated | The demo goals' recent notes give the demo notes with their states |
| AC-057 | translated | Demo history loading run again leaves the existing demo account with history unchanged |
| AC-058 | translated | An account that already has the demo email gets no demo goals and keeps its password |
| AC-059 | translated | Demo history loading's results name the demo email and no goal, habit, note, unit or amount |
| AC-060 | translated | A demo goal is left out when its catalog habit is not Published, and the results stay the same |
| AC-061 | translated | Demo history loading without a demo password creates no account |
| AC-062 | translated | Demo loading still names only the three slice-4 demo accounts, and their goals get no records |
| AC-063 | translated | The evaluation calendar still gives no state |
| AC-064 | translated | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement |
| AC-065 | translated | A goal whose catalog habit is Unpublished is still recorded and given its states |
| AC-066 | translated | The other kinds of target, just inside, at and just past their target |
| AC-067 | translated | After a move east, the old and the new day can both be recorded while they overlap |
| AC-068 | translated | The demo Yoga goal's weeks when R is a Sunday or a Tuesday |
| AC-069 | translated | Demo history loading takes a demo password only with at least 8 characters and a digit |

## Issues

No issues.

Checked without finding a conflict: the instants, grace ends and Kyiv clock changes (AC-001, AC-012, AC-013, AC-023, AC-030, AC-031, AC-045, AC-046, AC-067); the per-week boundaries (AC-017, AC-018, AC-034, AC-066); the range counts of AC-043 (366 / 367 days, 364 entries); the recent-note states of AC-040; and the demo data of AC-051 to AC-057 and AC-068 against FR-030 (R − k dates, even/odd k for Sugar, weeks 1 to 3 and the week before week 3 for R on Monday, Tuesday, Wednesday and Sunday). No item names a golden sample (and `tests/HT.AcceptanceTests/Samples/F005/` does not exist).

## Interpretations

- All items: removed the markdown list markers (`- `) and indented the steps under `Scenario` / `Scenario Outline`. Kept each step's text verbatim, including backticks, double quotes and parenthetical explanations such as "(R is 2026-11-18)", "(the date is checked before the note)" and "(23:00 in Kyiv on Monday 26 October 2026)".
- All items: each scenario is named by the item's heading title without the id and traceability. Tags are the item id plus one tag per traceability reference (the `;` separators became separate tags), e.g. `AC-013 (FR-010; FR-016; US2-AS2; US2-AS3)` → `@AC-013 @FR-010 @FR-016 @US2-AS2 @US2-AS3`.
- Items with an `Examples:` table (AC-005 to AC-010, AC-012 to AC-023, AC-025, AC-027, AC-031 to AC-034, AC-036 to AC-039, AC-043 to AC-050, AC-060, AC-061, AC-064, AC-066, AC-068, AC-069): written as `Scenario Outline` with `Examples:`. The markdown header-separator row (`|---|---|`) was dropped because Gherkin would read it as a data row. Columns that no step uses (`why` in AC-005 and AC-069) were kept as they are.
- AC-010 (column `actor`), AC-043 (column `actor`), AC-047 (column `goal`): Gherkin does not replace placeholders inside Examples cells, so `<n>` in the emails of these cells was written out as the row's `n` value, as the AC conventions define it ("Every email of such an item contains `<n>`, the row's value in the column `n`"). For example, AC-010 row 02 `the administrator `f5.ac010.admin.<n>@example.com`` became `the administrator `f5.ac010.admin.02@example.com``. Placeholders in step text stay `<n>`.
- AC-006, AC-027, AC-033, AC-038, AC-039, AC-043, AC-064, AC-069: the item's `Then <result>` step, whose cell holds a whole outcome phrase (sometimes with "and the history … gives …"), is kept as a single `Then <result>` step with the cell text verbatim.
- AC-030, AC-040, AC-042, AC-054, AC-055: the "Then … in this order:" line, followed by "And …" lines, is kept as one `Then` step ending in a colon, followed by `And` steps.
- AC-036, AC-037, AC-061: the empty text and space-only texts are kept in their cells as `` `""` ``, `` `"   "` `` (three spaces) and `"  New note.  "`. Gherkin trims only the cell's outer whitespace, so the spaces inside the quotes are preserved.
