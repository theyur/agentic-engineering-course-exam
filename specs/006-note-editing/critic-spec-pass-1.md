# Critic pass 1: spec for Note editing

**Verdict**: REVISE
**Date**: 2026-11-20
**Reviewed**: `specs/006-note-editing/spec.md`, `specs/006-note-editing/checklists/requirements.md`

## Notes

### MAJOR-1: "Exactly one place" (HP-002, SC-002) is contradicted by HP-003 for rows without a record
- **Where**: spec.md HP-002 (l. 225-233), SC-002 (l. 314-315), against HP-003 (l. 234-245), US2-AS3 (l. 141-143), US2-AS5 (l. 146-147) and Clarifications (l. 44-47). Checklist "Requirements are testable and unambiguous" (l. 17) is ticked.
- **Against**: the owner's input, rule (4) and "Each row has exactly one place to edit its note" (spec.md l. 17-19); issue #37, option 3a ("Each row then has exactly one place to edit its note").
- **Reasoning**: HP-002 says a row without a record edits its note through "Add note" / "Edit note" and the NoteDialog. The same row also has "Add record" (slice 5 HP-001, kept). HP-003 requires the RecordDialog's note field to open "in every mode (page head, a row's 'Add record', 'Edit')" with the current note, and "Save record" always sends the field's content. US2-AS5 states it for exactly this row state ("a row without a record and with a note ... 'Add record' ... the note field contains that note"), and US2-AS3 and the clarification say that clearing the field there removes the note. So a row without a record has two controls and two dialogs that show, change and remove its note. A Due or Pending row has a third: the page head's "Record" (HP-003, "page head"). SC-002 demands "exactly one control or dialog that edits its note, in 100% of the row states (with or without records ...)". As written it cannot be met by a page that satisfies HP-003, and a bUnit test for SC-002 cannot be written without first deciding what "place" means. The spec defines the term nowhere. For a period row with records, "the record dialog only" is also several controls (each record button and "Add record"), and SC-002's "one control or dialog" does not say which of the two is counted.

### MAJOR-2: A recorded decision is changed, and two decisions with rejected alternatives are taken, without an ADR
- **Where**: spec.md Clarifications (l. 48-54), "Decisions due before acceptance criteria" D-1 and D-2 (l. 322-333), "Decisions this feature cites" (l. 64-77), FR-001 (l. 199-203: "reverses"/"replaces the second bullet of slice 5 FR-007").
- **Against**: constitution, Decision Discipline (l. 139-143): "A decision that spans features, is hard to reverse, or rejects real alternatives MUST also become an ADR in `docs/adr/`" and "Changing a recorded decision requires a new or superseding ADR." Slice 5 spec.md Clarifications, Session 2026-11-18 (critic revision), l. 70-72, and its "Decisions due before acceptance criteria" (l. 769-770: "plus an ADR where it spans features").
- **Reasoning**: The slice 5 answer "A note given with a record adds or replaces the note only when it has text; a note is removed only by saving it empty on its own" is an owner decision recorded with its date in slice 5's Clarifications. This feature reverses it. `docs/adr/` (0001 to 0018) holds no ADR on the note rule (0018 mentions notes only as demo data), and the spec announces none: its only ADR references are citations of 0003, 0005, 0006 and 0008. In addition, D-1 and D-2 each record a rejected real alternative ("Rejected: treating none like empty ...", "Rejected: a separate `acceptance.md` ..."), and D-2 spans features 005 and 006: it moves this feature's rule and acceptance criteria into a shipped slice's spec and locked criteria. Both conditions of the constitution's ADR rule apply, and neither the spec nor the checklist marks an exception.

### MINOR-1: Skipping the AC states does not match the roadmap's rule, and F005's second pass has no place in the flow
- **Where**: spec.md "What acceptance tests can reach" (l. 85-90), D-2 (l. 329-333).
- **Against**: `docs/roadmap.md` l. 49-50 ("A slice whose spec finds no behavior on the AC surface (ADR 0003) records that in its spec and skips the three AC states"), row 5 (`shipped`) and row 5a (`clarified`); `CLAUDE.md`, "Flow per feature" (`/speckit-ac-translate` before `/speckit-plan`, `/speckit-ac-tests-write` after `/speckit-implement`).
- **Reasoning**: The spec itself finds behaviour on the AC surface (FR-001, FR-004) and still skips the three states. The roadmap allows the skip only for a slice with no such behaviour, and the pending roadmap change (row 5a added) leaves that sentence as it is. "F005 goes through them again instead" has no status to show it: row 5 is `shipped`. D-2 orders the AC steps among themselves but not against this feature's `/speckit-plan`, `/speckit-tasks` and `/speckit-implement`. It lists `/speckit-ac-tests-write` before "then `/speckit-ac-tests-analyze` after implementation", while the project flow and the command (`speckit.ac.tests-write.md` step 4: "The implementation step is not finished") place the writer after implementation.

### MINOR-2: FR-004's acceptance criterion is left conditional, on a check that D-2's own order never runs
- **Where**: spec.md l. 283-284 ("FR-004 ... gets a criterion in F005, unless an existing one already deletes a record of an entry with a note and reads the note; `/speckit-ac-translate` for F005 checks that"). Checklist "All functional requirements have clear acceptance criteria" (l. 27) is ticked.
- **Against**: `specs/005-recording-and-evaluation/acceptance.md`; `.specify/extensions/ac/commands/speckit.ac.translate.md` steps 3-4 and phase 2; D-2 (spec.md l. 330-332).
- **Reasoning**: The condition is decidable now. The criteria that delete a record are AC-026, AC-027 rows 03 and 04, AC-029, AC-032, AC-033 row 05 and AC-047 to AC-050; in none of them does the entry have a note that is read after the deletion. So no existing criterion covers FR-004, yet the spec leaves it open. The step it delegates to cannot decide it in D-2's order: there `acceptance.md` is revised and `sources` locked before `/speckit-ac-translate` runs, and with `sources` locked the command goes to phase 2, which translates and does not draft or review criteria.

### MINOR-3: "Not testable through the API" has nowhere to be listed
- **Where**: spec.md l. 81-83 ("Scenarios marked *(page)* are expected to be listed as 'Not testable through the API'") against l. 87-89 and l. 53-54 ("no `acceptance.md` of its own").
- **Against**: `.specify/extensions/ac/commands/speckit.ac.translate.md` step 6 (the Coverage table of a feature's `acceptance.md` holds that listing); `specs/005-recording-and-evaluation/acceptance.md` Coverage, keyed on slice 5's FR and US ids.
- **Reasoning**: With D-2 this feature has no Coverage table. Its nine *(page)* scenarios and its API scenarios (US1-AS2 to AS6, US2-AS1, AS3, AS4, US3-AS3) are therefore mapped to no acceptance item anywhere. F005's Coverage table lists only slice 5's ids, and the spec amends only US4-AS3 there. The sentence is carried over from slices 1 to 5 and contradicts D-2.

### MINOR-4: "Word for word" carries identifiers that mean something else in slice 5
- **Where**: spec.md l. 89-90 ("FR-001 to FR-007 ... are carried into slice 5's spec word for word where they change it"), FR-001 (l. 199-203), l. 262-263, l. 277-284.
- **Against**: slice 5 spec.md l. 773-776 (its own D-1: the grace period around clock changes; D-2: when a result becomes final), FR-004 (l. 466-470: amounts kept as entered).
- **Reasoning**: FR-001's text contains "This replaces the second bullet of slice 5 FR-007" and "(D-1)". Carried verbatim into slice 5's FR-007 it refers to itself and cites slice 5's D-1, a different decision. "FR-007 adds FR-004" and "FR-004 ... gets a criterion in F005" name this feature's FR-004, while F005's criteria cite slice 5's FR numbers, where FR-004 is another rule. The spec does not say which identifiers the amended slice 5 text and the revised criteria use.

### MINOR-5: The list of slice 5 and design documents to change is incomplete
- **Where**: spec.md "Slice 5 requirements and tests this feature changes" (l. 260-289) and "Design changes before build" (l. 291-300).
- **Against**: issue #37 ("Re-check steps 4 and 5 of the manual quickstart"); `specs/005-recording-and-evaluation/quickstart.md` l. 79; slice 5 spec.md l. 719-726; `specs/005-recording-and-evaluation/data-model.md` l. 136-138; `design/components/NoteDialog.html` l. 15; `design/pages.md` l. 151-152; constitution IV (design changes first).
- **Reasoning**: Not listed, although each states the behaviour this feature replaces:
  - quickstart step 5, "'Add note' on a Met row", which no longer exists once a row with a record has no note control; the issue's re-check of steps 4 and 5 appears nowhere in the spec;
  - slice 5's own "Design changes before build" ("the note help 'An empty field keeps the existing note.'", "'Add note'" as a row action for every row, "the 'Add note' entry from the History row"), which would contradict the amended HP-001 to HP-003 in the same file;
  - `data-model.md`'s store port rows (note only "with text is upserted");
  - `NoteDialog.html`'s slice-5 comment ("or from the row's note (filled)"), while the spec lists only `NoteDialog.md`;
  - `design/pages.md` l. 151 (RecordDialog: "empty, with 'An empty field keeps the existing note.'") and l. 152 (NoteDialog "opened from ... its note"), while the spec names only "the changed History row structure" for that file.

  The spec also does not say whether the design files' "Slice 5 state" blocks are edited or a new state block is added (`design/README.md` l. 17: "Build from the latest slice block that exists").

### MINOR-6: The assumption "only the note rule changes" understates the code change
- **Where**: spec.md Assumptions (l. 338-340), FR-003 (l. 207-208), l. 288-289.
- **Against**: `src/HT.Application/Records/IRecordStore.cs` (`AddAsync`, `ChangeAsync`: "`note` is added or replaced ...; null leaves the note unchanged"), `src/HT.Persistence/Records/RecordStore.cs` l. 66 and l. 99, `src/HT.Domain/Records/NoteText.cs` (`TryCreate`: null, empty and only spaces all give no note), `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` l. 129, `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs` l. 286-317.
- **Reasoning**: FR-001 with D-1 needs three cases for a note given with a record: none (keep), empty (remove), text (replace). The public store port has two: `NoteText? note`, where null keeps and a value replaces, and `NoteText.TryCreate` folds none and empty into one. Removing the note inside the record's write, which FR-003 needs (a refused save, the limit included, changes neither the records nor the note; the port promises one transaction per write), therefore changes the contract of a public port member, its implementation and its fake. The spec speaks only of use-case names, inputs and outcomes. The affected tests it names are those "of the note rule, the History card and the RecordDialog"; the goal page's bUnit tests, which open the NoteDialog from the note button and assert "…, edit note of Sat 14 Nov", are not among them.

### MINOR-7: SC-003 is contradicted by an edge case the spec accepts
- **Where**: spec.md SC-003 (l. 316-317) against Edge Cases (l. 192-193) and Assumptions (l. 337).
- **Against**: the spec's own rationale (l. 33-35: "the field always shows the note, so what the user sees is what is stored").
- **Reasoning**: SC-003 says no save removes or replaces a note "without the note having been shown in the field or dialog that saved it". The two-tabs edge case accepts exactly that: the later save overwrites a note written in the other tab, which the saving field never showed. The same holds for any dialog opened before another write. The criterion's second half narrows it to "in the tests", so the outcome stated first is not what is measured.

### MINOR-8: SC-004's three actions do not hold for rows behind "Show all N →"
- **Where**: spec.md SC-004 (l. 318), HP-008 (l. 255-256).
- **Against**: slice 5 HP-001 (the first 10 rows, then "Show all N →"); `specs/005-recording-and-evaluation/quickstart.md` l. 73 ("Missed with 'Away for the weekend.' (R − 10, the 11th row)").
- **Reasoning**: A row after the tenth is reached only after "Show all N →", so changing its note takes four actions. The spec's own example of a note on a row without a record, `Reading` R − 10, is such a row at the run time.

### MINOR-9: "Edit note" opens a dialog titled "Add note", and the note field's second text has no defined place
- **Where**: spec.md HP-002 (l. 228-230), HP-004 (l. 246-248: "otherwise unchanged: its title"), HP-003 (l. 240-242), HP-006 (l. 251-252).
- **Against**: `design/components/Dialog.md` l. 11 ("Title says the action and the object"); `design/components/NoteDialog.md` ("Title: 'Add note · ' ... There is no other title"); `design/components/TextField.md` ("A label, an input, and help or error text, stacked"); `src/HT.Ui/Components/Shared` `TextArea` (one `Help`, replaced by `Error`); slice 5 `contracts/host.md` l. 121-122 (the note refusal sits under the note).
- **Reasoning**: The new visible control "Edit note" opens a dialog that HP-004 fixes as "Add note · {goal name}", so the title names another action than the control. For a period, HP-003 puts two texts on the note field, the help and the week or day hint, where the design component has one help line that the error replaces. The spec does not say where the hint sits, or whether the 500-character refusal replaces the help, the hint or both, and `TextArea` / `TextField` are not in "Design changes before build".

## Summary

2 MAJOR, 9 MINOR. The rule change itself (FR-001 to FR-004, D-1) is consistent with issue #37, the owner's input and F005's criteria: the rows of AC-011, AC-035 to AC-039 and AC-064 named in the spec were checked and behave as it says, the demo data and the smoke check claims of HP-008 hold, and the use-case inputs can express D-1. The spec is not ready for the next step for two reasons. Its central page requirement and success criterion ("exactly one place") contradict HP-003 on rows without a record, so SC-002 cannot be met or tested as written. And it reverses a recorded slice 5 decision, and takes two decisions with rejected alternatives, one of them spanning two features, without the ADR the constitution requires. The minor notes concern the AC bookkeeping that D-2 leaves open (roadmap states, order of the F005 steps, FR-004's criterion, coverage), incomplete lists of documents to amend, and three success-criterion and design details.
