# Data model: Note editing

**Feature**: specs/006-note-editing · **Plan**: [plan.md](plan.md) · **Research**: [research.md](research.md)

This feature changes slice 5's model in four places:

- the new port types `NoteWrite` and `NoteWriteKind`;
- the meaning of `RecordInput.Note`;
- two `IRecordStore` members;
- one new `RecordingText` member.

Every type not named here stays as
[slice 5's data model](../005-recording-and-evaluation/data-model.md) gives it. Implementation amends that document
in place, with a dated line citing this feature and ADR 0019 (spec D-2, Work order step 2).

## Unchanged

- **Domain**: every type. `NoteText` stays "trimmed, 1 to 500 characters", and `TryCreate` still gives no note for
  null, empty and only spaces (research R-01).
- **Tables**: `GoalRecords` and `GoalNotes`. No migration: a note is still one row under (`GoalId`, `EntryDate`).
- **Use cases**: every signature and result type of `RecordingService`, `DemoHistoryService` and slices 1 to 4
  (spec FR-007).
- **Ports**: `IRecordStore.FindRecordDateAsync`, `DeleteAsync`, `SaveNoteAsync`, `ReadAsync`,
  `ReadRecentNoteKeysAsync`, and `IDemoHistoryStore`.
- **Views**: `HistoryEntry.Note` already gives each entry's one note, in `GetHistoryAsync` and in `GetRecordableAsync`.
  The record dialog fills its field from it (research R-05).

## Application: `NoteWrite` (`HT.Application.Records`, new port type)

What a record's write does to its entry's note (FR-001, D-1; research R-01).

| Member | Rule |
|---|---|
| `Kind` | `NoteWriteKind`: `Keep`, `Remove`, `Replace`. |
| `Text` | `NoteText?`: the note to store; set only when `Kind` is `Replace`. |
| `Keep` | static property: leave the note as it is. |
| `Remove` | static property: delete the note, if there is one. |
| `Replace(NoteText text)` | static factory: store `text` as the note, adding or replacing it. Throws `ArgumentNullException` for null. |

It is a sealed class with a private constructor, like `RecordStoreWrite`. `ToString` is not overridden, so a note never
reaches a log or a message (HP-007). `RecordingService` builds it from `RecordInput.Note`:

| `RecordInput.Note` | `NoteWrite` |
|---|---|
| null (no note given) | `Keep` |
| `""` or only spaces | `Remove` |
| text of 1 to 500 characters after trimming | `Replace` with the trimmed text |
| more than 500 characters after trimming | none: the use case refuses with `NoteTooLong` and writes nothing |

## Application: `RecordInput.Note` (meaning changed, type unchanged)

`string?`. Null keeps the entry's note. A text that is empty after trimming removes it. Any other text, trimmed,
becomes the note. The page always sends the field's content, so it never sends null (research R-06).

## Ports: `IRecordStore` (two members changed)

| Member | Change |
|---|---|
| `AddAsync(owner, goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteWrite note, int limit)` | `note` was `NoteText?`. In the same transaction as the record, under the goal row's update lock: `Keep` leaves the note, `Remove` deletes it, and `Replace` adds or replaces it under `entry.Key`. `GoalNotFound` and `LimitReached` write nothing, the note included (FR-003). |
| `ChangeAsync(owner, goal, RecordId record, EntrySpan entry, RecordValue value, NoteWrite note)` | `note` was `NoteText?`. The same three cases as `AddAsync`. `GoalNotFound` and `RecordNotFound` write nothing, the note included. |

`DeleteAsync` already leaves the note: deleting a record, the only one included, never changes it (FR-004). This
feature adds tests for it and no code. `SaveNoteAsync` keeps `NoteText?`, where null deletes the note (FR-002,
unchanged).

Implementations: `RecordStore` (`HT.Persistence`, internal) and `FakeRecordStore` (unit-test fake, also used by the
bUnit page services). There is also the fault-injecting wrapper in `tests/HT.IntegrationTests/Records/RecordLogTests.cs`
(research R-03, R-04).

## Application: `RecordingText` (one member added)

| Member | Result |
|---|---|
| `NoteHelp(GoalHistory history)` | The note field's help (HP-003): "Clear the field to remove the note." for a commitment-based goal; "This note is for the whole day. Clear the field to remove it." for a per-day period; "This note is for the whole week. Clear the field to remove it." for a per-week period. The texts are `design/`'s (Work order step 1). |

## State transitions

Unchanged: a note never changes an entry's state (slice 5 FR-007). What changes is what a record's write does to the
note:

```text
record or change, no note given   -> note unchanged
record or change, empty note      -> no note
record or change, note with text  -> that note (trimmed)
delete a record (any, the last one included) -> note unchanged
save the note on its own          -> that note, or no note when empty (slice 5, unchanged)
refused write (any step)          -> records and note unchanged
```
