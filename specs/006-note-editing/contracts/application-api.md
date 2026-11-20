# Contract: Application surface of note editing

**Feature**: specs/006-note-editing · **Plan**: [../plan.md](../plan.md)

This contract gives only what changes in
[slice 5's Application contract](../../005-recording-and-evaluation/contracts/application-api.md). Implementation
amends that document in place, with a dated line citing this feature and ADR 0019 (Work order step 2). Types are in
[../data-model.md](../data-model.md).

## Unchanged

- Every method, parameter, outcome, check order and text of `RecordingService` and `DemoHistoryService` (spec FR-007).
- The composition: `AddHtApplication` and `AddHtPersistence` register the same services and ports.
- The refusals and what they carry: no input value, so a result never echoes a note (HP-007).
- **Save note** (FR-002): the date names the entry, and any begun date of a week names that week. A note with text
  replaces the note, and one that is empty after trimming removes it. This works for any entry, with or without
  records, whatever its state. The page offers the NoteDialog only on rows without a record (HP-002), but the use
  case does not check for records (research R-08).

## Changed rules

Slice 5's "Record" bullets for the note ("a note with text adds or replaces the entry's note; no note, or one that
is empty after trimming, leaves the note unchanged") become:

- **Record** and **Change**, the note (FR-001, D-1):
  - no note given (`RecordInput.Note` null): the entry's note is unchanged;
  - a note that is empty after trimming (`""`, only spaces): the entry has no note after the write;
  - a note with text: that text, trimmed, is the entry's note after the write, whether it had none, the same one or
    another one;
  - the note is written in the same transaction as the record. So `NoteTooLong`, `RecordLimitReached`,
    `RecordNotFound` and every refusal before them leave the records and the note as they were (FR-003);
  - for a period, the note is the whole period's, whichever of its records is recorded or changed.
- **Delete** removes the record and never changes the note, also when it was the entry's only record (FR-004). An
  entry that had a note and loses its last record still has the note. If it is final, it is Missed with that note.
- `Entry` in the result is still the entry after the write, as `GetHistoryAsync` gives it at the same time. So its
  `Note` shows the outcome of the three cases above.

## Examples (these match F005's revised criteria)

| Before | Call | After |
|---|---|---|
| A week with the note "Knee was sore." | `RecordAsync`, Yes, `Note` null | "Knee was sore." (AC-037 row 01) |
| the same | `RecordAsync`, Yes, `Note` `"   "` or `""` | no note (AC-037 rows 02, 03) |
| the same | `RecordAsync`, Yes, `Note` "Better today." | "Better today." (AC-037 row 04) |
| A week record, the note "Knee was sore." | `ChangeRecordAsync`, `Note` `""` | no note (AC-037 row 07) |
| the same | `ChangeRecordAsync`, `Note` "Knee was sore." | "Knee was sore." (AC-037 row 08) |
| A commitment with one record and "Fell asleep early." | `DeleteRecordAsync` | no record, "Fell asleep early." (AC-070) |
| A Missed commitment with "Away for the weekend.", no record | `RecordAsync`, 20 min, `Note` "Away for the weekend.", `""`, `"   "` or null | Met, a correction; the note "Away for the weekend.", none, none, "Away for the weekend." (AC-071) |
| A commitment with one record and no note | `ChangeRecordAsync`, `Note` "Back was tired." | "Back was tired." (AC-072) |
