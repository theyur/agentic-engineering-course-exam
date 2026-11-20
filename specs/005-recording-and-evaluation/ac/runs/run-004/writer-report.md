# Writer report — F005 / run-004

## Build
success, 1 attempt, no remaining errors (0 warnings, 0 errors).

The step definitions and support code that earlier runs wrote already bind every step of the current
`recording-and-evaluation.feature`. That includes the scenarios the 2026-11-20 note-editing amendment changed or
added: AC-037 rows 02, 03 and 07 now expect "no note", and AC-070, AC-071 and AC-072 are new. I traced each step
of those scenarios through the existing regexes and parsers (`F005Actions`, `F005Checks`, `F005Entries`,
`F005Text`). Each one reaches a binding that calls the right use case and makes a real assertion, so no code
change was needed in this run. The public API (`RecordInput.Note`: null for "no note", `""` / `"   "` for an
empty note) still fits the way the code already passes notes.

## Coverage
Step definition files are in `tests/HT.AcceptanceTests/StepDefinitions/F005/`. Support code is in
`tests/HT.AcceptanceTests/Support/F005/` (F005Actions, F005Arrange, F005Checks, F005Context, F005Database,
F005Driver, F005Entries, F005Hooks, F005Text).

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A partial amount recorded for today is kept and the commitment is Due | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-002 | Recording again for a commitment replaces its one record | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-003 | A per-week period holds one record per recording, listed in date order | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-004 | Records on the same date of a per-day period keep the order in which they were stored | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-005 | A date that belongs to no commitment or period cannot be recorded | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-006 | A day that has not begun on the goal's timeline cannot be recorded | GivenSteps, WhenSteps, ResultSteps |
| AC-007 | A value that does not fit the goal's measurement is refused with its message | GivenSteps, WhenSteps, ResultSteps |
| AC-008 | The lowest and highest amounts are accepted and kept | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-009 | A value that does not belong to the goal's measurement is not stored | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-010 | Recording checks the acting user, the goal, the date, the value and the note in that order | GivenSteps, WhenSteps, ResultSteps |
| AC-011 | A note given with a record is trimmed | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-012 | A commitment is Upcoming before its start instant and Due from it | GivenSteps, WhenSteps, HistorySteps |
| AC-013 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself | GivenSteps, WhenSteps, HistorySteps |
| AC-014 | A final Build duration commitment is Met at or above its target and Failed below it | GivenSteps, WhenSteps, HistorySteps |
| AC-015 | A final Reduce count commitment is Met at or below its limit and Failed above it | GivenSteps, WhenSteps, HistorySteps |
| AC-016 | A final yes/no commitment is Met on Yes for Build and on No for Reduce | GivenSteps, WhenSteps, HistorySteps |
| AC-017 | A final per-week Build yes/no period is Met with at least N Yes records | GivenSteps, WhenSteps, HistorySteps |
| AC-018 | A final per-week Build count period counts only the records that reach the amount | GivenSteps, WhenSteps, HistorySteps |
| AC-019 | A final per-day Reduce count period is Met when its total is within the limit | GivenSteps, WhenSteps, HistorySteps |
| AC-020 | A final per-day yes/no period counts its Yes records | GivenSteps, WhenSteps, HistorySteps |
| AC-021 | An ended commitment or period stays Pending during its grace period, even when its records reach the target | GivenSteps, WhenSteps, HistorySteps |
| AC-022 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit | GivenSteps, WhenSteps, HistorySteps |
| AC-023 | Around a clock change the grace period lasts exactly 48 elapsed hours | GivenSteps, WhenSteps, HistorySteps |
| AC-024 | A record added to a Missed commitment is a correction that makes it Met | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-025 | Lowering the record of a Met commitment below its target is a correction that makes it Failed | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-026 | Deleting the only record of a final commitment is a correction that makes it Missed | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-027 | A change is a correction only from the instant its grace period ends | GivenSteps, WhenSteps, ResultSteps |
| AC-028 | A missing record added to a Failed week is a correction that makes it Met | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-029 | Results corrected after the grace period equal the same records entered on time | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-030 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | GivenSteps, WhenSteps, HistorySteps |
| AC-031 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-032 | A record identifier that is not a record of the goal named in the call does not exist | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-033 | An evaluation period takes at most 50 records, but its records can still be changed or deleted | GivenSteps, WhenSteps, ResultSteps |
| AC-034 | A record belongs to the week of its date, not to the week of the instant it is saved | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-035 | A note can be added to a Missed commitment and leaves it Missed | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-036 | A note saved on its own replaces the note, and an empty one removes it | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-037 | A note given with a record is the note; no note leaves it unchanged | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-038 | A note has at most 500 characters | GivenSteps, WhenSteps, ResultSteps |
| AC-039 | Any begun date of a week names that week's one note | GivenSteps, WhenSteps, ResultSteps |
| AC-040 | The recent notes are the 5 with the latest dates, newest first, each with its state | GivenSteps, WhenSteps, HistorySteps |
| AC-041 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | GivenSteps, WhenSteps, HistorySteps |
| AC-042 | The history gives every commitment of the range in date order, each with its state at the supplied time | GivenSteps, WhenSteps, HistorySteps |
| AC-043 | The history's range has 1 to 366 days and is checked after the goal | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-044 | Progress is given at any state, as the goal's kind of target counts it | GivenSteps, WhenSteps, HistorySteps |
| AC-045 | What can be recorded now is every Due and every Pending commitment or period, newest first | GivenSteps, WhenSteps, HistorySteps |
| AC-046 | After a move east, the old and the new day are both Due while they overlap | GivenSteps, WhenSteps, HistorySteps |
| AC-047 | Another registered user gets "This goal does not exist." from every use case of this slice | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-048 | Another account's record identifier does not exist on one's own goal | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-049 | An administrator gets "Goals are for registered users." from every use case of this slice | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-050 | The anonymous visitor gets "You are not signed in." from every use case of this slice | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-051 | Demo history loading creates the demo account with history and its three demo goals | DemoSteps |
| AC-052 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | DemoSteps, WhenSteps, HistorySteps |
| AC-053 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | DemoSteps, WhenSteps, HistorySteps |
| AC-054 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | DemoSteps, WhenSteps, HistorySteps |
| AC-055 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | DemoSteps, WhenSteps, HistorySteps |
| AC-056 | The demo goals' recent notes give the demo notes with their states | DemoSteps, WhenSteps, HistorySteps |
| AC-057 | Demo history loading run again leaves the existing demo account with history unchanged | DemoSteps, HistorySteps |
| AC-058 | An account that already has the demo email gets no demo goals and keeps its password | DemoSteps, GivenSteps |
| AC-059 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | DemoSteps |
| AC-060 | A demo goal is left out when its catalog habit is not Published, and the results stay the same | DemoSteps, GivenSteps |
| AC-061 | Demo history loading without a demo password creates no account | DemoSteps |
| AC-062 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | DemoSteps, HistorySteps |
| AC-063 | The evaluation calendar still gives no state | GivenSteps, WhenSteps, HistorySteps |
| AC-064 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement | GivenSteps, WhenSteps, ResultSteps |
| AC-065 | A goal whose catalog habit is Unpublished is still recorded and given its states | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-066 | The other kinds of target, just inside, at and just past their target | GivenSteps, WhenSteps, HistorySteps |
| AC-067 | After a move east, the old and the new day can both be recorded while they overlap | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-068 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday | DemoSteps, WhenSteps, HistorySteps |
| AC-069 | Demo history loading takes a demo password only with at least 8 characters and a digit | DemoSteps |
| AC-070 | Deleting the only record of a commitment keeps its note | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-071 | The first record of a Missed commitment that has a note: the note given is the note, no note keeps it | GivenSteps, WhenSteps, ResultSteps, HistorySteps |
| AC-072 | Changing the record of a commitment without a note, with a note, adds the note | GivenSteps, WhenSteps, ResultSteps, HistorySteps |

## Unexpressible steps
None.

## Cross-feature changes
None.

## Missing packages
None.

## Interpretations and assumptions
- All items: time. Account and habit setup that gives no time runs at `2026-11-18T10:00:00Z`, or at the earliest time the item gives when that is earlier. It is held until the first other step fixes that time. Other steps that give no time run at `2026-11-18T10:00:00Z` or at the latest time given so far, whichever is later. Then steps run at the When step's time. This follows acceptance.md, "Time". The time is supplied through `FakeTimeProvider`; a step at an earlier time gets a new provider started at that time.
- All items: "the record of D" is the identifier of the only record dated D that the owner's history read gives at that time; "the first record of D" is the first of several in the history's order (acceptance.md, "Recording use cases").
- All items: "nothing is recorded" reads the owner's history over every date the item names, at the When time. It checks that the number of records and notes equals what the Given steps made (acceptance.md).
- AC-013: "no other use case is called for the goal after it was created" counts the use-case calls the driver makes with the goal's identifier. It asserts that the count is zero at that step and unchanged when the When step reads.
- AC-047, AC-049, AC-050: the use cases named in tables use acceptance.md's fixed arguments: record 12 for 2026-11-18, change to 12 with the owner's record of 2026-11-18, the note "Changed." for 2026-11-18, and history from 2026-11-18 to 2026-11-18. "it gives no record, note or state" is checked as a null entry, record id or history, and as an empty notes list for recent notes.
- AC-063: "no entry has a status" is checked by reflection. No public property of the calendar view, its commitments or its periods has a name that contains "Status" or "State".
- AC-037, AC-071: `""` and `"   "` are passed as the note text (empty after trimming). A step with no "with the note" passes a null note, as FR-007 (amended 2026-11-20) distinguishes "no note at all" from an empty note.
- Progress phrases: "progress A of B" is read by goal kind. For a commitment-based goal it is the recorded value; for a period-based goal it is the records that reach the amount. "of at most" is the Yes-records kind and "total A of at most B" the total kind. Trailing words such as "minutes" and "times" are not checked (acceptance.md, "Progress").
- Demo items: demo goals are looked up by name in `demo4@example.com`'s goal list. Their schedule kind for progress checks comes from AC-051's goal list.
