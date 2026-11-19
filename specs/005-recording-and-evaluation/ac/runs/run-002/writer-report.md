# Writer report — F005 / run-002

## Build

success, 1 attempt (`dotnet build tests/HT.AcceptanceTests/HT.AcceptanceTests.csproj -p:AcRefMode=true -nologo -v q`: 0 warnings, 0 errors).

## Coverage

Step definition files are in `tests/HT.AcceptanceTests/StepDefinitions/F005/`: `GivenSteps.cs` (G), `WhenSteps.cs` (W),
`ResultSteps.cs` (R), `HistorySteps.cs` (H), `DemoSteps.cs` (D). They share the feature support in
`tests/HT.AcceptanceTests/Support/F005/` (`F005Actions.cs` new; `F005Checks.cs` extended; `F005Arrange.cs`,
`F005Entries.cs`, `F005Text.cs`, `F005Context.cs`, `F005Driver.cs`, `F005Database.cs`, `F005Hooks.cs` reused).

| AC tag | Scenario name | Step definition file(s) |
|---|---|---|
| AC-001 | A partial amount recorded for today is kept and the commitment is Due | G, W, R, H |
| AC-002 | Recording again for a commitment replaces its one record | G, W, R, H |
| AC-003 | A per-week period holds one record per recording, listed in date order | G, W, R, H |
| AC-004 | Records on the same date of a per-day period keep the order in which they were stored | G, W, R, H |
| AC-005 | A date that belongs to no commitment or period cannot be recorded (5 rows) | G, W, R, H |
| AC-006 | A day that has not begun on the goal's timeline cannot be recorded (8 rows) | G, W, R |
| AC-007 | A value that does not fit the goal's measurement is refused with its message (8 rows) | G, W, R |
| AC-008 | The lowest and highest amounts are accepted and kept (5 rows) | G, W, R, H |
| AC-009 | A value that does not belong to the goal's measurement is not stored (3 rows) | G, W, R, H |
| AC-010 | Recording checks the acting user, the goal, the date, the value and the note in that order (7 rows) | G, W, R |
| AC-011 | A note given with a record is trimmed | G, W, R, H |
| AC-012 | A commitment is Upcoming before its start instant and Due from it (3 rows) | G, W, H |
| AC-013 | An unrecorded commitment is Pending for 48 hours after its end and then Missed by itself (4 rows) | G, W, H |
| AC-014 | A final Build duration commitment is Met at or above its target and Failed below it (8 rows) | G, W, H |
| AC-015 | A final Reduce count commitment is Met at or below its limit and Failed above it (6 rows) | G, W, H |
| AC-016 | A final yes/no commitment is Met on Yes for Build and on No for Reduce (4 rows) | G, W, H |
| AC-017 | A final per-week Build yes/no period is Met with at least N Yes records (4 rows) | G, W, H |
| AC-018 | A final per-week Build count period counts only the records that reach the amount (5 rows) | G, W, H |
| AC-019 | A final per-day Reduce count period is Met when its total is within the limit (4 rows) | G, W, H |
| AC-020 | A final per-day yes/no period counts its Yes records (7 rows) | G, W, H |
| AC-021 | An ended commitment or period stays Pending during its grace period, even when its records reach the target (7 rows) | G, W, H |
| AC-022 | A Reduce commitment or period with no record is Missed, and Met only on an explicit No or an amount within the limit (9 rows) | G, W, H |
| AC-023 | Around a clock change the grace period lasts exactly 48 elapsed hours (4 rows) | G, W, H |
| AC-024 | A record added to a Missed commitment is a correction that makes it Met | G, W, R, H |
| AC-025 | Lowering the record of a Met commitment below its target is a correction that makes it Failed (2 rows) | G, W, R, H |
| AC-026 | Deleting the only record of a final commitment is a correction that makes it Missed | G, W, R, H |
| AC-027 | A change is a correction only from the instant its grace period ends (6 rows) | G, W, R, H |
| AC-028 | A missing record added to a Failed week is a correction that makes it Met | G, W, R, H |
| AC-029 | Results corrected after the grace period equal the same records entered on time | G, W, R, H |
| AC-030 | After a time zone change, each commitment keeps the instants and grace period of its own settings version | G, W, H |
| AC-031 | A commitment in the old time zone becomes final, and is corrected, at the end of its own grace period (2 rows) | G, W, R, H |
| AC-032 | A record identifier that is not a record of the goal named in the call does not exist (4 rows) | G, W, R, H |
| AC-033 | An evaluation period takes at most 50 records, but its records can still be changed or deleted (5 rows) | G, W, R, H |
| AC-034 | A record belongs to the week of its date, not to the week of the instant it is saved (3 rows) | G, W, R, H |
| AC-035 | A note can be added to a Missed commitment and leaves it Missed | G, W, R, H |
| AC-036 | A note saved on its own replaces the note, and an empty one removes it (4 rows) | G, W, R, H |
| AC-037 | A note given with a record replaces the note only when it has text (6 rows) | G, W, R, H |
| AC-038 | A note has at most 500 characters (6 rows) | G, W, R, H |
| AC-039 | Any begun date of a week names that week's one note (6 rows) | G, W, R, H |
| AC-040 | The recent notes are the 5 with the latest dates, newest first, each with its state | G, W, H |
| AC-041 | A per-week note gives its week's first and last date, and a goal with fewer notes gives only those | G, W, H |
| AC-042 | The history gives every commitment of the range in date order, each with its state at the supplied time | G, W, H |
| AC-043 | The history's range has 1 to 366 days and is checked after the goal (6 rows) | G, W, R, H |
| AC-044 | Progress is given at any state, as the goal's kind of target counts it (7 rows) | G, W, H |
| AC-045 | What can be recorded now is every Due and every Pending commitment or period, newest first (7 rows) | G, W, H |
| AC-046 | After a move east, the old and the new day are both Due while they overlap (2 rows) | G, W, H |
| AC-047 | Another registered user gets "This goal does not exist." from every use case of this slice (9 rows) | G, W, R, H |
| AC-048 | Another account's record identifier does not exist on one's own goal (2 rows) | G, W, R, H |
| AC-049 | An administrator gets "Goals are for registered users." from every use case of this slice (7 rows) | G, W, R, H |
| AC-050 | The anonymous visitor gets "You are not signed in." from every use case of this slice (7 rows) | G, W, R, H |
| AC-051 | Demo history loading creates the demo account with history and its three demo goals | D |
| AC-052 | The demo goal "Reading, 20 min daily" shows Met, Failed with a partial amount, Missed with a note, Pending and Due | D, W, H |
| AC-053 | The demo goal "Sugar, at most 1 sweet a day" shows Met, Failed with a note, Missed, Pending and Due | D, W, H |
| AC-054 | The demo goal "Yoga, 3 times a week" shows a Met, a Failed and a Missed week and a Due week when R is a Wednesday | D, W, H |
| AC-055 | When R is a Monday, the demo Yoga goal's last full week is Pending and it has a week before week 3 | D, W, H |
| AC-056 | The demo goals' recent notes give the demo notes with their states | D, W, H |
| AC-057 | Demo history loading run again leaves the existing demo account with history unchanged | D, H |
| AC-058 | An account that already has the demo email gets no demo goals and keeps its password | G, D |
| AC-059 | Demo history loading's results name the demo email and no goal, habit, note, unit or amount | D |
| AC-060 | A demo goal is left out when its catalog habit is not Published, and the results stay the same (3 rows) | G, D |
| AC-061 | Demo history loading without a demo password creates no account (3 rows) | D |
| AC-062 | Demo loading still names only the three slice-4 demo accounts, and their goals get no records | D, H |
| AC-063 | The evaluation calendar still gives no state | G, W, H |
| AC-064 | Changing a record checks the record, the value and the note as recording does, and stores only what belongs to the measurement (11 rows) | G, W, R, H |
| AC-065 | A goal whose catalog habit is Unpublished is still recorded and given its states | G, W, R, H |
| AC-066 | The other kinds of target, just inside, at and just past their target (33 rows) | G, W, H |
| AC-067 | After a move east, the old and the new day can both be recorded while they overlap | G, W, R, H |
| AC-068 | The demo Yoga goal's weeks when R is a Sunday or a Tuesday (2 rows) | D, W, H |
| AC-069 | Demo history loading takes a demo password only with at least 8 characters and a digit (4 rows) | D |

## Unexpressible steps

None.

## Cross-feature changes

None. No file outside `StepDefinitions/F005/` and `Support/F005/` was changed; nothing was added to `Support/Shared/`.

## Missing packages

None.

## Interpretations and assumptions

- All items: the times follow acceptance.md's "Time" convention (`F005Arrange`): account and habit setup without a
  time runs at `2026-11-18T10:00:00Z` or at the item's first, earlier time; other Given/When steps without a time
  run at that default or the latest earlier time; every Then step runs at the When step's time.
- All items: the progress kind expected by "progress A of B" follows acceptance.md's "Progress": "total A of at
  most B" is a total, "A of at most B" a count of Yes records, and plain "A of B" the recorded value for a
  commitment-based goal and the count of records that reach the amount for a period-based one. Which kind of goal it
  is comes from the settings the Gherkin writes (fixed days: commitment-based); for the demo goals from AC-051's
  Gherkin ("Reading, 20 min daily" Commitment-based, the other two Period-based).
- AC-005, AC-006, AC-007, AC-010: "nothing is recorded" is acceptance.md's definition: the owner's history over the
  item's dates (the start date and every date the item's steps record or note for) has exactly the records and notes
  the Given steps made.
- AC-013: "no other use case is called for the goal after it was created" is checked with the driver's per-goal call
  counter: zero calls after creation when the step runs, and still none just before the When step's read.
- AC-032, AC-047, AC-064: "a record identifier that belongs to no record" and "a goal identifier that belongs to no
  goal" are new random GUIDs.
- AC-047, AC-049, AC-050: the table's use cases use acceptance.md's fixed arguments (records 12 for 2026-11-18; the
  owner's record of 2026-11-18, read from the owner's history, changed to 12 or deleted; the note "Changed." for
  2026-11-18; history 2026-11-18 to 2026-11-18). "it gives no record, note or state" is: no entry and no record
  identifier for writes, no entry for note saves, no history for history and recordable-now reads, and an empty list
  for recent notes.
- All "the result is "M"" steps: the When step's one use case was refused (its outcome is not the success outcome)
  and its message is exactly M.
- AC-029: "for the first goal and for the second goal alike: L" checks each goal's history against exactly the
  entries of L, in order.
- AC-051, AC-060, AC-069: "its results are, in this order, A, B and C, each for `e`" is checked as exactly those
  three result lines (email, text), in order, also where the step does not add "and no other result".
- AC-051: "goal list has exactly three goals: …" is checked as three goals, each found by its name with the facts in
  parentheses; the list's order is not checked (the step does not say "in this order"). "reading each demo goal" and
  "each demo goal" mean the goals of `demo4@example.com`, the demo account of the item. "no account name" is the
  profile's name being absent.
- AC-057: "each with the start date 2026-10-21" is read from each listed goal's details (the goal read's start date),
  since a list item's start date is optional in the API.
- AC-051 to AC-069: "the AC database is reset, so no account and no habit exists" runs the reset entry point; the
  "so …" part is not checked separately. The parentheses of "demo loading and then demo history loading ran (R is …;
  the goal starts …)" explain the dates and are not checked by that Given step; the Then steps check the dates.
  Demo loading and demo history loading in Given steps must give the outcome Loaded.
- AC-059: "no result contains any of …" checks each result line's text and email with ordinal comparison.
- AC-062: "none names `demo4@example.com`" checks demo loading's message, catalog message, and every account's email,
  message, custom-habit message and goal message, ignoring case.
- AC-063: "no entry has a status" is checked by reflection: neither the calendar view nor its commitment or period
  types have a public property whose name contains "Status" or "State" (as F004's step for the same text does, with
  "State" added because this slice's word is "state").
- AC-065: "unpublished … with confirmation" calls unpublish with the confirmation given; "its state is Unpublished"
  reads the catalog habit as the administrator.
- AC-060: "an Unpublished `N`" is created, published and unpublished (no goal is based on it, so no confirmation is
  needed); "Draft" is created only.
- AC-068: "reading the goal gives the start date D" reads the goal the When step read the history of.

## Feedback addressed

No feedback file was given for this run.
