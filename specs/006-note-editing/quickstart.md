# Quickstart: validate note editing

**Feature**: specs/006-note-editing · **Plan**: [plan.md](plan.md)

How to prove this feature works end to end. The rules are in [contracts/](contracts/) and the types in
[data-model.md](data-model.md); this guide only runs them. Prerequisites are slice 5's
([quickstart](../005-recording-and-evaluation/quickstart.md#prerequisites)): the .NET 10 SDK, SQL Server at `.\COLL1`
(or `HT_IT_CONNECTION` and `HT_UI_CONNECTION`), the User Secrets of `HT.Ui` set (never print their values), and
`git config core.hooksPath .githooks`.

## 1. Build and the solution's tests

```bash
dotnet build HabitTracker.slnx
```

```bash
dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"
```

Expected: no warning, no error, every test passes. This feature's tests (research R-10):

| What | Where | Proves |
|---|---|---|
| The note rule of a record's write | `HT.UnitTests/Application/Records` (`NoteTests`, `RecordTests`) | FR-001 (keep, remove, replace, for a record and a change), FR-003 (a refusal changes nothing), FR-004 (a delete keeps the note) |
| The help text | `HT.UnitTests/Application/Records/RecordingTextTests` | HP-003's three texts |
| The store's three cases in one transaction | `HT.IntegrationTests/Records/RecordStoreTests` | `Keep`, `Remove` and `Replace` for an add and a change; nothing written on `LimitReached` or `RecordNotFound` |
| No note in a log | `HT.IntegrationTests/Records/RecordLogTests` | HP-007, now with `Remove` |
| The History card | `HT.UiTests/Components/Records/HistoryCardTests` | HP-001, HP-002, SC-002 |
| The dialogs | `HT.UiTests/Components/Records/RecordDialogTests`, `NoteDialogTests` | HP-003, HP-004, SC-001, SC-003 |
| The goal page | `HT.UiTests/Components/Pages/GoalDetailPageTests` | the Missed row's "Add note", then "Edit note" with the note as text and the focus back on it; SC-004: a note changed or cleared in 3 actions |
| Architecture | `HT.ArchitectureTests` | `NoteWrite` in the public-surface list and fenced from `HT.Ui` |

To run one area on its own:

```bash
dotnet test HabitTracker.slnx --nologo -v q --filter "FullyQualifiedName~HT.UiTests.Components.Records"
```

## 2. Acceptance tests

After `/speckit-implement`, the owner runs `/speckit-ac-tests-write 005`, then `/speckit-ac-tests-analyze`. Expected:
every F005 scenario passes, the revised AC-037 and the new AC-070 to AC-072 among them, and F001, F002 and F004 pass
unchanged (SC-005).

## 3. The running app (slice 5 quickstart, steps 4 and 5 re-checked; issue #37)

```bash
dotnet run --project src/HT.Ui --launch-profile https
```

Open `https://localhost:7194` and sign in as `demo4@example.com` with the demo password. R is the date the demo
account was created (slice 5 quickstart, section 3).

1. "Reading, 20 min daily". The Note column is plain text: "Fell asleep early." on the Failed row R − 6, and "—" on
   rows without a note. The row R − 6 offers only "Edit". No row with a record has a note control. On every row the
   record control ("Add record" or "Edit") is the rightmost button.
2. "Edit" on R − 6: the note field holds "Fell asleep early.", with the help "Clear the field to remove the note.".
   Clear it and save: the row shows "—" and is still Failed. That is three actions: open, edit, save (SC-004).
   "Edit" again, type "Fell asleep early." and save: the note is back.
3. After "Show all", the Missed row R − 10 shows "Away for the weekend." as text, with "Edit note" and "Add record".
   (Amended 2026-11-20, owner's decision after implementation: the note control now comes before "Add record".)
   "Edit note" opens "Edit note · Reading, 20 min daily" with the note in the field. "Cancel" returns the focus to
   "Edit note".
4. Slice 5's step 4: "Add record" on that Missed row. The note field holds "Away for the weekend.". Save 20 minutes
   with the field unchanged: the row turns Met (a correction), still with the note, and offers "Edit" only. "Edit",
   then "Delete record": the row is Missed again, with "Away for the weekend." and "Edit note".
5. Slice 5's step 5, as it is now: on a Met row, "Edit", type "Test note." and save. The chip stays Met and the note
   column shows "Test note.". "Edit", clear the field and save: the note is gone. A Met row has no "Add note".
6. "Yoga, 3 times a week": "Add record" on the week with "Knee was sore." opens with that note and the help "This
   note is for the whole week. Clear the field to remove it.". The same holds for any of the week's record buttons
   in the Recorded cell (each "Yes"), which open "Edit" of that record. A period row has no "Edit" button.
7. "Sugar, at most 1 sweet a day": the record button "3 sweet" in the Recorded cell of the Failed day with "Birthday
   cake at work." opens "Edit" of that record with that note and the help "This note is for the whole day. Clear the
   field to remove it.".
8. The page head's "Record": choose another entry in "For" after typing in the note field. The field now shows that
   entry's note, or nothing.
9. Keyboard only: Tab reaches "Edit note", Enter opens the NoteDialog with focus in the field, Esc closes it, and the
   focus returns to "Edit note".

Steps 4 and 5 are slice 5's quickstart steps that issue #37 asks to re-check by hand (spec, Order of work step 6).
Until they are run, they are reported as unverified.

## 4. Hosted demo

Nothing changes. The smoke check still reads "Fell asleep early." in the History card, now as cell text, and passes
with `PASS demo history demo4@example.com (11 of 11)` after the next deployment.
