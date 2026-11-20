# Contract: Application surface of recording and evaluation

**Feature**: specs/005-recording-and-evaluation · **Plan**: [../plan.md](../plan.md)

This is the public API that `HT.Ui`, the integration tests and the acceptance tests call (ADR 0003, ADR 0005).
Types are in [../data-model.md](../data-model.md).

- The names below are the contract; parameter names may differ.
- Every method is `async` and takes a `CancellationToken` last.
- Every result has `Outcome` (an enum) and `Message`: the literal text below, or null on success.
- Methods for signed-in users take `ActingUser` first.
- The current time is `TimeProvider.GetUtcNow()` from DI, read once per call. Acceptance tests register their own
  `TimeProvider` and supply no `InstallationTimeZone`, so the default is `UTC` (slice 4 research R-03).

The surface of slices 1 to 4 is unchanged (FR-019). No existing constructor, method or port member changes.

## Composition

`AddHtApplication` also registers `RecordingService` and `DemoHistoryService`, both scoped. `AddHtPersistence`
also registers `IRecordStore` and `IDemoHistoryStore`, both scoped. `HtDatabase.ResetAsync` applies the migration
`Records` together with the others.

## `RecordingService` (registered users)

Every method checks, in this order, and stops at the first refusal (FR-005, FR-018):

1. `NotSignedIn`: no account, or an account that is not stored;
2. `NotForAdministrators`;
3. `NotFound`: the goal is not one of the acting user's own (FR-017);
4. `RecordNotFound`: change and delete only; the record is not one of that goal's records;
5. the date (FR-002): `NothingScheduled`, then `DayNotBegun`;
6. the value (FR-005): `AnswerMissing`, `CountOutOfRange` or `DurationOutOfRange`, by measurement;
7. the note (FR-007): `NoteTooLong`;
8. the limit: `RecordLimitReached`, when adding to a period that has 50 records.

A method skips the steps that do not apply to it:

| Method | Outcomes, in check order | Requirement |
|---|---|---|
| `RecordAsync(ActingUser, GoalId goal, DateOnly date, RecordInput input)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `NothingScheduled` · `DayNotBegun` · `AnswerMissing` / `CountOutOfRange` / `DurationOutOfRange` · `NoteTooLong` · `RecordLimitReached` · `Recorded` with `RecordId`, `Entry`, `IsCorrection` | FR-001 to FR-007 |
| `ChangeRecordAsync(ActingUser, GoalId goal, RecordId record, RecordInput input)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `RecordNotFound` · value outcomes · `NoteTooLong` · `Changed` with `RecordId`, `Entry`, `IsCorrection` | FR-003 to FR-007 |
| `DeleteRecordAsync(ActingUser, GoalId goal, RecordId record)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `RecordNotFound` · `Deleted` with `Entry`, `IsCorrection` | FR-003, FR-006 |
| `SaveNoteAsync(ActingUser, GoalId goal, DateOnly date, string? note)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `NothingScheduled` · `DayNotBegun` · `NoteTooLong` · `Saved` with `Entry` | FR-007 |
| `GetHistoryAsync(ActingUser, GoalId goal, DateOnly first, DateOnly last)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `RangeOutOfBounds` · `Found` with `History` | FR-009, FR-010, FR-014 |
| `GetRecordableAsync(ActingUser, GoalId goal)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `Found` with `History` (its Due and Pending entries, newest first) | FR-011 |
| `GetRecentNotesAsync(ActingUser, GoalId goal)` | `NotSignedIn` · `NotForAdministrators` · `NotFound` · `Found` with `Notes` (at most 5, newest first) | FR-008 |

The rules behind these outcomes:

- **Record**:
  - for a commitment, an existing record's value is replaced, and it keeps its identifier;
  - for a period, a record is added;
  - the note (FR-003, FR-007):
    - no note given (`RecordInput.Note` null): the entry's note is unchanged;
    - a note that is empty after trimming (`""`, only spaces): the entry has no note after the write;
    - a note with text: that text, trimmed, is the entry's note after the write, whether it had none, the same one
      or another one;
    - the note is written in the same transaction as the record. So `NoteTooLong`, `RecordLimitReached`,
      `RecordNotFound` and every refusal before them leave the records and the note as they were;
    - for a period, the note is the whole period's, whichever of its records is recorded or changed.

  Amended 2026-11-20, specs/006-note-editing, ADR 0019: no note, or one that was empty after trimming, left the note
  unchanged.
- **Change** replaces the value of that one record. Its date never changes (FR-003). There is no date step: see
  plan "Resolved in the spec" 1. The note follows the record rule above.
- **Delete** removes the record and never changes the note, also when it was the entry's only record (FR-007).
  Deleting the only record of a final entry makes it Missed, with its note if it had one. Amended 2026-11-20,
  specs/006-note-editing, ADR 0019: what a delete does to the note was not stated.
- **Save note**:
  - the date names the entry, and any begun date of a week names that week;
  - a note with text replaces the note, and one that is empty after trimming removes it;
  - the entry's state does not change (FR-007);
  - it works for any entry, with or without records. The page offers the NoteDialog only on rows without a record,
    but the use case does not check for records (amended 2026-11-20, specs/006-note-editing, ADR 0019).
- **`IsCorrection`** is true when the entry was final at the supplied time, that is when its grace end was on or
  before it (FR-006, FR-016). It is false for a refusal.
- **`Entry`** is the entry after the write, as `GetHistoryAsync` gives it at the same time. So its `Note` shows the
  outcome of the note rules above.
- **History**:
  - every entry of the goal's evaluation calendar in the range (slice 4 FR-013), in date order;
  - a per-week entry gives all of its records, also those with dates outside the range;
  - the range is checked after the goal (FR-018).
- **Recordable**: the entries that are Due or Pending at the supplied time, by first date descending. None when
  the goal has no current date. Two are Due only during the overlap after a move east (FR-011).
- **Recent notes**:
  - the 5 notes with the latest entry keys;
  - each note with its first and last date and its state at the supplied time.
- **What a refusal carries**: no input value, so the result never echoes a note, a value or a date (HP-006).

## `DemoHistoryService` (operator)

| Method | Outcomes | Requirement |
|---|---|---|
| `LoadAsync(string? demoPassword)` | `NoPassword` ("no demo password configured") · `PasswordRuleNotMet` ("demo password does not meet the password rule") · `Loaded` with `Accounts`: one `DemoHistoryAccountResult` for `demo4@example.com`. It is `Created` ("created", with `Goals` "demo goals added" and `Records` "demo records added"), or `AlreadyExists` ("already exists", with `Goals` and `Records` null) | FR-030 |

The run date R is the date of the supplied time in the installation's default time zone. The demo data is FR-030's,
as research R-08 lists it. An existing account of that email is neither read nor changed.

`DemoDataService.LoadAsync` is unchanged and never names `demo4@example.com` (FR-031).

## Texts

New texts, in `RecordTexts`:

| Constant | Text |
|---|---|
| `NothingScheduled` | Nothing is scheduled on this date. |
| `DayNotBegun` | You cannot record a day that has not begun. |
| `AnswerMissing` | Choose Yes or No. |
| `CountOutOfRange` | Enter a whole number from 0 to 9999. |
| `DurationOutOfRange` | Enter a whole number of minutes from 0 to 1440. |
| `NoteTooLong` | The note can have at most 500 characters. |
| `RecordLimitReached` | This evaluation period already has 50 records. |
| `RecordNotFound` | This record does not exist. |

Reused texts:

- "You are not signed in.": `AccountTexts.NotSignedIn`;
- "Goals are for registered users.": `GoalTexts.NotForAdministrators`;
- "This goal does not exist.": `GoalTexts.NotFound`;
- "Choose a range of 1 to 366 days.": `GoalTexts.RangeOutOfBounds`;
- demo history loading's refusals and account results: `OperatorTexts`, plus the new `DemoRecordsAdded`, "demo
  records added".
