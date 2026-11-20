# Translation report — F005 / translate-002

## Build

success, 1 attempt, no remaining errors (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors). Not checked: whether the generated code-behind exists for the new file. The guard blocks searching `tests/HT.AcceptanceTests` outside the features folder.

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
| AC-037 | translated | A note given with a record is the note; no note leaves it unchanged |
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
| AC-070 | translated | Deleting the only record of a commitment keeps its note |
| AC-071 | translated | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it |
| AC-072 | translated | Changing the record of a commitment without a note, with a note, adds the note |

## Issues

No issues.

The items were checked against spec.md (including FR-007 as amended on 2026-11-20 and the slice-4 rules it restates), the constitution, SPEC.md §5 and §11, and ADR 0003. They were also recomputed by hand: instants, grace ends, Kyiv and New York offsets around the 2026-10-25 and 2027-03-28 clock changes and the 2026-11-01 US change, range lengths (AC-043), demo dates for R on Monday, Tuesday, Wednesday and Sunday, and the Sugar parity days (AC-053). No item contradicts another item or a spec input, and none lacks a value it needs. No item names a golden sample; `tests/HT.AcceptanceTests/Samples/F005/` does not exist and none is needed.

## Interpretations

- All items: each `### AC-NNN (refs) Title` heading became a `Scenario` (or `Scenario Outline` for items with an `Examples:` table). It is tagged `@AC-NNN` plus one tag per traceability reference, in the order the item gives them, and its title is the item's title. The `- ` list markers were dropped and steps were indented under the scenario. Step text, backticks, quotes and parentheticals are kept verbatim.
- All items with `Examples:`: the Markdown table became a Gherkin Examples table. The `|---|` separator row was dropped and the columns were aligned. Column names (including multi-word ones such as `created at`, `start date`, `time zone`, `first week`, `use case`, `demo password`, `run time`, `last date`) and the `<placeholder>` references in the steps are unchanged. Informational columns (`why` in AC-005 and AC-069) are kept as Examples columns with their text verbatim.
- AC-030, AC-040, AC-042, AC-052, AC-053, AC-054, AC-055: a `Then` step that ends in `:` ("it gives 4 entries, in this order:", etc.) is followed by its `And` lines as separate steps, exactly as the item lists them. No data table was introduced.
- AC-006, AC-027, AC-033, AC-038, AC-039, AC-043, AC-064, AC-069: the item's `Then <result>` takes whole-step text from the table. The cell text, including its embedded "and the history …" clauses, is kept verbatim as one step.
- AC-022, AC-027: the value `nothing` in the `records` / `before` column is substituted as written, giving a `Given … recorded nothing` step. The step wording is not changed.
