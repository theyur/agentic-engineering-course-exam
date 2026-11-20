# Critic pass 2: spec for Note editing

**Verdict**: APPROVE
**Date**: 2026-11-20
**Reviewed**: `specs/006-note-editing/spec.md`, `specs/006-note-editing/checklists/requirements.md`; as references written or changed for this feature: `docs/adr/0019-note-editing.md`, `docs/roadmap.md` (row 5a, "Status values")

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1: "exactly one place" contradicted by HP-003 | Resolved in the spec; a residue outside it is MINOR-5 | Clarifications, critic revision (spec l. 68-71) defines the term; HP-002 (l. 241-252) and SC-002 (l. 379-381) now count "controls that change the note without changing its records": none on a row with a record, exactly one on a row without. That can be tested, and it agrees with HP-003, US2-AS3 and US2-AS5. The definition does not weaken the owner's input: the input itself asks for both the note dialog on rows without a record (rule 4) and the note moving "into the prefilled record dialog" when the row gets a record (spec l. 19-20). The rationale is sound. |
| MAJOR-2: reversed decision and D-1, D-2 without an ADR | Resolved | `docs/adr/0019-note-editing.md` (Accepted, 2026-11-20) names the slice 5 decision it changes (slice 5 Clarifications, critic revision, last answer, l. 70-72: verified as the last answer of that session), lists the options of issue #37 and of D-1 and D-2 with the rejected ones, and its consequences. The spec links it in Clarifications (l. 43, 49, 52, 72-73) and in "Decisions due before acceptance criteria" (l. 406-414). |
| MINOR-1: AC states and order of the F005 steps | Resolved | Roadmap l. 50-52 adds the rule for a slice that amends a shipped slice's criteria. "Order of work" (l. 390-402) places F005's unlock and `/speckit-ac-translate` before `/speckit-plan`, and `/speckit-ac-tests-write` after `/speckit-implement`, as `speckit.ac.tests-write.md` step 4 needs. `ac_lock.py unlock` deletes the lock file, so `speckit.ac.translate.md` step 4 enters phase 1 and step 6 revises the existing `acceptance.md`, as step 2 of the order says. |
| MINOR-2: FR-004's criterion conditional | Resolved | l. 297 and l. 333-334: a new criterion for slice 5 US4-AS8, without a condition. The list of criteria that delete a record (AC-026, AC-027, AC-029, AC-032, AC-033, AC-047 to AC-050) was checked again against `acceptance.md`: none deletes a record of an entry with a note and reads the note after a successful deletion. |
| MINOR-3: "Not testable through the API" has no place | Resolved | l. 94-96 no longer promise that listing; the table "Where this feature's scenarios are tested" (l. 288-298) maps all 16 scenarios. Two of its rows are inaccurate: MINOR-2 below. |
| MINOR-4: identifiers carried "word for word" | Resolved | l. 105-106 and l. 304-313: slice 5's text uses slice 5's numbering, the rule text quoted for FR-007 carries no identifier, and the new rule is cited as "ADR 0019", not as D-1 or D-2. |
| MINOR-5: incomplete list of documents | Resolved, with one detail left: MINOR-6 | Now listed: quickstart steps 4 and 5 and their manual re-check (l. 324-327, l. 402), slice 5's "Design changes before build" (l. 317-319), `data-model.md`'s port rows (l. 322-323), `NoteDialog.html`'s comment (l. 359-361), `design/pages.md`'s RecordDialog and NoteDialog lines (l. 362-364), and that the slice 5 state blocks are amended in place (l. 351-353). The quoted texts match the files. Slice 5's `research.md` (l. 504) and `tasks.md` still state the old rule; they record finished work, so no note is raised. |
| MINOR-6: understated code change | Resolved | "Code and tests" (l. 338-347) and Assumptions (l. 419-421). Verified: `IRecordStore.AddAsync` / `ChangeAsync` take `NoteText? note` with null meaning "unchanged"; `RecordStore.cs` l. 66 and l. 99 and `FakeRecordStore.cs` l. 129 and l. 182 upsert only a non-null note; `NoteText.TryCreate` gives no note for null, empty and only spaces; `RecordInput.Note` is `string?`, so the use-case input can tell none from empty; `GoalDetailPageTests.cs` l. 286-317 is named. |
| MINOR-7: SC-003 against the two-tabs edge case | Resolved for that case; its new wording has another gap: MINOR-4 | SC-003 (l. 382-384) is limited to one opened dialog and excludes two tabs; the edge case (l. 208-210) and ADR 0019 (l. 75-76) say the same. |
| MINOR-8: SC-004 and "Show all N →" | Resolved | SC-004 (l. 385-386) counts from a row that is on the page and names "Show all N →" for later rows. |
| MINOR-9: "Edit note" title, second text on the note field | Resolved | HP-004 (l. 267-271): "Edit note · {goal name}". HP-003 (l. 259-263): one help text per kind, which a refusal replaces; this fits `TextArea` (one `Help`, replaced by `Error`). The design files that fix the old title and help are in "Design changes before build" (l. 357-361). |

No pass 1 note has regressed.

## Notes

### MINOR-1: HP-004 gives a focus rule that is not slice 5's, while saying the focus handling stays as in slice 5
- **Where**: spec.md HP-004 (l. 269-271), HP-006 (l. 275: "dialogs keep slice 5's focus handling"); the contract list (l. 320-321) and the NoteDialog design bullet (l. 359-361).
- **Against**: `specs/005-recording-and-evaluation/contracts/host.md` l. 58-61 and l. 138-139 (after a save "the focus goes back to the row's Note button ..., which now shows the note or 'Add note'"); `src/HT.Ui/Components/Records/HistoryCard.razor`, `FocusNoteAsync` ("'Add note' or the note, which a save may have swapped"); `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs` l. 287 and l. 306-308 ("The row keeps its one Note button, so the focus goes back to the same element after the save"); `design/components/NoteDialog.md` l. 19 and `Dialog.md` l. 14 ("focus returns to the button that opened it"); constitution IV ("focus returns to the opener").
- **Continues**: new
- **Reasoning**: HP-004 says in one sentence that the focus handling stays "as in slice 5" and in the next that, when the note control "changed from one to the other", the focus goes to the row's "Add record". In slice 5 the focus goes to the row's note control as the row shows it after the save, also when the save swapped it. Under HP-004 the most common use of the dialog, "Add note" saved with text on a Missed row, ends with the focus on "Add record", a control for another action, although "Edit note" now stands where the opener was. The spec gives no reason for the difference, and the two statements cannot both be tested. The change is also missing from the lists of what is amended: l. 320-321 names "the NoteDialog's openers and title" of `contracts/host.md`, and the design bullet names openers and titles; neither names the focus.

### MINOR-2: Two rows of "Where this feature's scenarios are tested" name criteria that do not exercise the scenario
- **Where**: spec.md l. 291 (US1-AS2, US1-AS4 → "AC-037 (rows that change a record with a note that has text)"), l. 293 (US1-AS5 → "AC-011, AC-037"); "AC-037 changes" (l. 329-332); checklist l. 27 ("All functional requirements have clear acceptance criteria", ticked).
- **Against**: `specs/005-recording-and-evaluation/acceptance.md` AC-011 (l. 352-358), AC-037 (l. 757-775), AC-064 (l. 1162-1184).
- **Continues**: MINOR-3 of pass 1 (the mapping it asked for)
- **Reasoning**:
  - US1-AS4 (a change with the same note leaves the note unchanged): AC-037 has one row that changes a record with a note that has text, row 06, and it gives a different note ("Better today." over "Knee was sore."). The spec adds to AC-037 only "a row for a change with an empty note". No existing or announced criterion changes a record with the note the entry already has. This is the call the page makes on every "Edit" in which the user does not touch the note (HP-003: "an unchanged field keeps the note").
  - US1-AS5 (an entry with a record and no note; a change with a note adds it): AC-011 records, it does not change, and its commitment has no record before; every row of AC-037 starts from an entry that has the note "Knee was sore."; AC-064 starts with "First note.". No criterion changes a record with a note on an entry without one.

  The table therefore states coverage that F005 will not have after the revision listed in the spec.

### MINOR-3: "Order of work" amends slice 5's documents before their content is settled, and leaves out `/speckit-analyze`
- **Where**: spec.md "Order of work" steps 1, 3 and 4 (l. 394-399); l. 320-323 (contracts, data model); l. 314-316 (slice 5 HP-001 to HP-003 "become" this feature's HP-001 to HP-004); l. 338 ("constraints for the plan, not a design"); l. 266 and l. 424 (the help texts and the "Edit note" title are proposals that `design/` confirms before build).
- **Against**: constitution, Workflow and Quality Gates (l. 147: "specify → clarify → plan → tasks → analyze → implement"); `docs/roadmap.md` l. 232-234 ("Every slice", with `/speckit-analyze`); spec Clarifications l. 53-54 and ADR 0019 l. 57 ("`specs/006-note-editing` holds the page requirements").
- **Continues**: MINOR-1 of pass 1 (ordering)
- **Reasoning**:
  - Step 1 amends `data-model.md`'s store port rows of `AddAsync` and `ChangeAsync` before step 3. How the port expresses keep, remove and replace is left to the plan (l. 338-343), so the amended rows cannot be written in step 1.
  - Step 1 also writes the help texts and the "Edit note" title into slice 5's HP-002, HP-003 and `contracts/host.md`, while the spec calls them proposals that `design/` confirms, and the design changes come with the tasks of step 3. A different wording in `design/` then needs a second amendment of slice 5.
  - It is not stated what step 1 writes into slice 5's HP-001 to HP-003: the text of this feature's HP-001 to HP-004, or a reference to them. The spec says both that slice 5 is amended in place and that this feature "keeps the page requirements"; after step 1 the same page rules stand in slice 5's spec, in its `contracts/host.md` and here, with no statement of which one governs.
  - The numbered order goes from `/speckit-tasks` (step 3) to `/speckit-implement` (step 4). `/speckit-analyze`, which the constitution's sequence and the roadmap's flow place between them, is not in it.

### MINOR-4: SC-003's wording does not hold when "For" is changed in the page-head dialog
- **Where**: spec.md SC-003 (l. 382-384: "a save removes or replaces the note only when that dialog showed the note when it opened").
- **Against**: HP-003 (l. 255-256), US3-AS4 (l. 190-191) and the edge case at l. 206-207.
- **Continues**: MINOR-7 of pass 1 (wording of SC-003)
- **Reasoning**: Opened from the page head, the dialog shows the note of the first "For" entry. After the owner chooses another entry, the field shows that entry's note, and "Save record" replaces or removes it. That note was not shown "when it opened". The first clause of SC-003 is false for a flow the spec requires, while its second clause ("every removal comes from a field or dialog that showed the note and was cleared") holds. A test written to the first clause would fail on US3-AS4's flow.

### MINOR-5: Roadmap row 5a still states the claim that pass 1 found contradicted; the ADR and the spec name different citations
- **Where**: `docs/roadmap.md` l. 35 (row 5a, Outcome); `docs/adr/0019-note-editing.md` l. 1 (title) and l. 68-70; spec.md l. 302 and l. 351-352.
- **Against**: spec.md Clarifications l. 68-71, HP-002 (l. 245-248), HP-003 (l. 253-256), US2-AS5 (l. 162-163).
- **Continues**: MAJOR-1 of pass 1 (residue outside the spec)
- **Reasoning**: The spec now defines "one place" as at most one control that changes the note without changing the records, and says that the RecordDialog also changes the note of a row without a record (its "Add record", and the page head's "Record"). Row 5a, added for this feature, still says "Each History row has one place to edit its note ...: the record dialog for a row with a record, the note dialog for a row without one", which is the reading HP-003 contradicts. ADR 0019's title keeps "One place per History row" without the definition; its Decision text agrees with the spec. Separately, ADR 0019 says the amended slice 5 documents and design previews each get "a dated line citing this ADR", while the spec says twice "a dated line citing this feature" (and "ADR 0019" only for slice 5's Clarifications), so it is open what the dated lines cite.

### MINOR-6: The NoteDialog design shows a state in which the dialog can no longer open
- **Where**: spec.md "Design changes before build", NoteDialog bullet (l. 359-361), and HP-004 (l. 267: "MUST open only from a row without a record").
- **Against**: `design/components/NoteDialog.html` l. 24-29 (slice 5 state block "from the note of a Failed week", "27 Jul – 2 Aug · Failed"); `design/components/NoteDialog.md` l. 15 (sub line example "27 Jul – 2 Aug · Failed"); slice 5 FR-010 (Failed only "when it has at least one record").
- **Continues**: MINOR-5 of pass 1
- **Reasoning**: A Failed commitment or period has a record, so under HP-004 the NoteDialog never opens for it. For `NoteDialog.html` the bullet names only the comment "or from the row's note (filled)" as changing; the second slice 5 preview and the guide's sub line example show the dialog on a Failed week, a state the amended design would still present as built.

## Summary

0 MAJOR, 6 MINOR. Both MAJOR notes of pass 1 are resolved: "one place" is defined in a way that HP-002, HP-003 and SC-002 can meet and test, and ADR 0019 records the reversed slice 5 decision, D-1 and D-2 with their rejected alternatives. The claims about F005's criteria (AC-011, AC-035 to AC-039, AC-064, the deletion criteria, the next free id AC-070), the record store port, `NoteText`, `RecordInput`, the demo data, the smoke check (it reads the History card's text, not the note button), the quickstart steps and the quoted design and contract texts were checked against the files and hold. The order of the AC steps for F005 fits the commands (`unlock` removes the lock file, phase 1 revises an existing `acceptance.md`, the test writer runs after implementation). The spec can go to its next step. The remaining notes are a focus rule in HP-004 that contradicts the slice 5 handling it says it keeps, two mapping rows that overstate F005's coverage, ordering details of "Order of work" (including the missing `/speckit-analyze`), one success-criterion wording, and leftovers in the roadmap row, the ADR and the NoteDialog design list.
