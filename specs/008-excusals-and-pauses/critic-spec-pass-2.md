# Critic pass 2: spec for 008-excusals-and-pauses

**Verdict**: APPROVE
**Date**: 2026-11-21
**Reviewed**: specs/008-excusals-and-pauses/spec.md, specs/008-excusals-and-pauses/checklists/requirements.md (cross-checked against CONTEXT.md and docs/roadmap.md as changed in this change, docs/adr/0023-demo-pause-loading.md and docs/adr/0024-excusal-and-pause-rules.md)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (D-1 has no ADR) | Resolved | ADR 0024 records D-1, D-2 and D-3 with options, decision and consequences, and names the community slice as bound (Context l. 17-19, Consequences l. 60-67). The spec's Clarifications intro (l. 22-24) and "Decisions due" intro (l. 734-738) link it. The roadmap's SPEC §11.2 and §11.3 rows are now "decided" and link ADR 0024. The Assumptions first bullet (l. 762-764) has slice 9 cite ADR 0024. |
| MINOR-1 (US3-AS4 trend omits 17, 18) | Resolved | US3-AS4 (l. 280-283) now lists "17 to 19 November (no record) have no value and no mark". Checked: 6 valued, 4 marked and 4 unmarked bars make 14 (6 to 19 November). |
| MINOR-2 (order of excused/paused refusals) | Resolved | FR-005 (l. 465-472) adds steps (4a) to (4c) between slice 5 FR-005 steps (4) and (5) and fixes their order: an excused and paused date gives (4a). Changing and deleting skip them, as they skip step (4). |
| MINOR-3 (upcoming pause pauses the week of a goal that is not paused) | Resolved | New edge case "A Paused week of a goal that is not paused" (l. 404-409) states that this is intended and describes the Active tab, the count, the Notice, the dashboard, the record refusal, Resume and cancel. FR-018 (l. 530-531) and the ADR 0024 Decision (l. 47-50) agree. |
| MINOR-4 (state/progress of exempt entries in earlier reads) | Resolved | (a) FR-015 (l. 516-518) gives progress per slice 5 FR-014, and FR-017 (l. 525-528) gives records, progress, note and correction "no". (b) FR-009 (l. 495-496) returns "Paused" after a change and never a correction. (c) FR-020 (l. 536-537) covers both recent-notes reads. (d) "Earlier requirements" (l. 675-677) now cites slice 5 FR-006, FR-008 and FR-009. |
| MINOR-5 (HP-001 Paused row vs FR-009) | Resolved | HP-001 History bullet (l. 605-608): a Paused row offers no "Add record", but a kept record keeps its edit action and both rows keep their note actions. |
| MINOR-6 (Paused commitment's records in the week value) | Resolved | FR-016 (l. 520-523): the week's value (Build and Reduce) counts only the "Yes" records of commitments that are neither Excused nor Paused. A follow-on gap is in MINOR-3 below. |
| MINOR-7 ("as if made at its own time" vs no stored time) | Resolved | FR-003 (l. 456-459) and FR-025 (l. 556-557) say there is "no time of its own" and "no time of making". FR-031 (l. 569-571) and the ADR 0023 Decision say "stored directly as facts", and the user rules do not apply to the task. |
| MINOR-8 (no Excusal entry; Evaluation state "exactly one of") | Resolved | CONTEXT.md adds an Excusal entry with NOT-boundaries. Evaluation state now reads "for one commitment or evaluation period that is not Excused or Paused ... (an Excused or Paused one has none of them)". HP-012 (l. 652-658) lists both. |
| MINOR-9 (checklist stale after clarify) | Resolved | The checklist Notes add Iteration 3 (the clarify answers, ADR 0023) and Iteration 4 (the pass-1 revisions). |
| MINOR-10 (scenarios missing inputs) | Resolved | US1 header (l. 148-149) names the measurement. US2-AS11 (l. 232-236) and US2-AS12 (l. 237-244) give the settings, start date and supplied times. US3 data (l. 266-270) gives the pause's last day. US3-AS6 and AS7 (l. 287-296) and US4 (l. 313-316, AS4 l. 327-330) define their goals. |

No older note has regressed. I rechecked the worked figures against slice 5 FR-016 (48 elapsed hours from the end instant) and slice 6 FR-001 to FR-006 and FR-012, and all hold:

- US1: grace ends of 9 and 11 November.
- US2-AS5, AS8, AS11 and AS12.
- US3: "90% (10 of 11)", streaks 2 and 8, 17 recent results, the 14 trend bars, "83% (10 of 12)" with a longest streak of 7, "N/A", "100% (2 of 2)" with 2 weeks.
- US4: the Upcoming window of 6 days excludes 23 November; Friday 20 November is the earliest Upcoming entry that is not Paused.
- FR-031: "95% (22 of 23)", 10/12, "94% (18 of 19)", 2/16, and 1 Due, 2 Pending, 0 Missed this week on every weekday.

## Notes

### MINOR-1: SC-001's "at most two actions" can't be met for a Due commitment or from the goal page
- **Where**: spec.md SC-001 (l. 714-715); HP-001 History bullet (l. 605-608); HP-003 (l. 619-621); HP-005 (l. 628-630).
- **Against**: the spec's own page requirements. HP-001 gives no "Excuse" action on a Due or Pending History row. HP-003 opens the Excuse dialog only from "Excuse" on the Home's **Pending** rows and from "Excuse instead" in the RecordDialog. HP-005 adds "Excuse" only to Pending rows. FR-002 step (8) and D-2 allow excusing a Due commitment.
- **Continues**: new
- **Reasoning**: SC-001 promises that a user excuses a commitment "in at most two actions from the page where they see it (the Home row or the goal page)". Under the HP section, a Due commitment on the Home (its row has "Record", not "Excuse") and any commitment on the goal page take at least three actions: "Record" or "Add record", then "Excuse instead", then "Excuse commitment". Only a Pending row on the Home meets the criterion. As written, the measurable outcome fails for part of what FR-002 allows, and a UI test of SC-001 has no two-action path to check there.

### MINOR-2: SPEC §5.3's "does not erase ... failures already recorded" is read only as "final", and the reading is not stated
- **Where**: spec.md FR-008 (l. 488-490); Edge Cases "A pause over today's begun entry" (l. 397-398) and "One paused day in a week" (l. 402-403); FR-007 (l. 484-487); ADR 0024 Options, partly paused week, option 1 cost (l. 26-27) and Consequences (l. 63-64).
- **Against**: SPEC §5.3: "A pause does not erase activity or failures already recorded and cannot retroactively convert an already finalized failure into an excusal". It has two clauses, and FR-008 maps only the second one ("a pause cannot turn a finalized failure into an exemption").
- **Continues**: new
- **Reasoning**: A pause may start on the current date (FR-006 step (5)). Under D-1, a pause starting later in the week exempts the whole current week. Take a Reduce goal whose Due day or week already has records over its limit, for example a per-week Reduce goal over its weekly limit by Wednesday. That failure is already certain from the records, but a pause from today, or from any later day of the week, turns the entry Paused and takes it out of consistency and streaks. Slice 5's vocabulary supports the spec's reading, since Failed exists only once the result is final. But the spec never says that SPEC's "failures already recorded" means final results only. ADR 0024 weighs only the opposite case ("also when its records already reach the target"), not records that already fail, so the owner's D-1 choice doesn't visibly cover it. CLAUDE.md requires a conflict with SPEC to be raised, not resolved silently. Here the interpretation is defensible but unrecorded, so `/speckit-ac-translate` and reviewers can't tell whether it is intended.

### MINOR-3: FR-016 doesn't say whether a yes/no week whose only records sit on exempt commitments has the value 0 or no value
- **Where**: spec.md FR-016 (l. 519-524).
- **Against**: slice 6 FR-006 (specs/007-personal-reports/spec.md l. 537-540): "A bar whose commitment, day or week has no record has no value (not 0) ... A bar with at least one record has its value, 0 included."
- **Continues**: new (follow-on of pass-1 MINOR-6)
- **Reasoning**: FR-016 now leaves a "Yes" kept on a Paused commitment out of the week's value. It doesn't say whether that record still makes the week "a week with at least one record". Example: a Mon/Wed/Fri yes/no goal whose Monday is Paused with a kept "Yes" (FR-009, US2-AS10), while Wednesday and Friday are unrecorded. Slice 6's rule taken literally gives the value 0, against a target of 2. If records on exempt commitments are also left out of "has a record", the bar has no value. For a Reduce goal (target 0) the two readings show "0, within the limit" versus "unknown", the very distinction slice 6 FR-006 draws. The value can be computed two ways.

### MINOR-4: "Earlier requirements and tests this slice changes" leaves out slice 5 FR-010 and FR-018 and the RecordDialog test
- **Where**: spec.md "Earlier requirements and tests this slice changes" (l. 660-677); terms (l. 437-439); FR-005 (l. 465-472).
- **Against**: slice 5 FR-010 (specs/005-recording-and-evaluation/spec.md l. 547: the state "MUST be exactly one of" Upcoming, Due, Pending, Met, Failed or Missed); slice 5 FR-018 (l. 593-596: "MUST check in the order of FR-005 (signed in, role, goal, record, date, value, note, limit)"); tests/HT.UiTests/Components/Records/RecordDialogTests.cs l. 131-141 (`US1_AS9_..._and_no_Excuse_instead` asserts the markup has no "Excuse").
- **Continues**: new
- **Reasoning**: Three earlier items change without being listed:
  - This spec states that an Excused or Paused commitment or period has none of the six states (l. 437-439). That contradicts slice 5 FR-010's "exactly one of", but the section lists FR-005, FR-006, FR-008, FR-009 and FR-011, not FR-010. CONTEXT.md's Evaluation state entry was sharpened, but its FR counterpart wasn't recorded as amended.
  - Slice 5 FR-018 fixes the full check order as eight named steps. FR-005's steps (4a) to (4c) extend that list, and FR-018 isn't listed either.
  - The amended slice 5 HP-002 (l. 671) names no test, although `RecordDialogTests` asserts that "Excuse" is absent and will fail once HP-003 adds "Excuse instead". The section names the other absence-asserting tests (l. 669-670), so readers take its list as the full test impact.

### MINOR-5: ADR 0024's provenance doesn't match its Decision
- **Where**: docs/adr/0024-excusal-and-pause-rules.md status line (l. 3-4: "Owner choices ... (spec D-1, D-2 and D-3, Clarifications 2026-11-21 (specify))") and Decision bullet 2 (l. 52-53: "An excusal can be removed only while the commitment's grace period is open"); spec.md Clarifications (clarify) Q3 (l. 48-50) and Q2 (l. 44-47); "Decisions due" intro (l. 734-738).
- **Against**: constitution, Decision Discipline (an ADR records decisions with the alternatives they reject); the spec's own stated reason for the ADR, "They reject real alternatives ... so all three are recorded in ADR 0024" (l. 22-24).
- **Continues**: new
- **Reasoning**:
  - The removal-window rule was decided in the clarify session, not the specify session. ADR 0024 still records it as a decision, but with no option or rejected alternative under "Options", and its status line attributes the ADR to the specify session only.
  - The early-resume answer of the same clarify session (Q2: today is expected again, and a pause that began today is removed) rejects an equally real alternative, today staying paused, and appears in no ADR. FR-010 relies on that answer, and ADR 0024's partly-paused-week rule depends on it for a mid-week resume.
  - The spec applies "rejects real alternatives, so an ADR" to D-1 to D-3 but doesn't say why the two clarify answers are treated differently.

  The ADR is therefore an incomplete and partly mis-attributed record of the slice's alternative-rejecting decisions. `/speckit-plan`'s Constitution Check will cite it as the record.

## Summary
0 MAJOR, 5 MINOR. All 11 notes of pass 1 are resolved, and none has regressed. ADR 0024 closes the Decision Discipline gap, the record-check order is fixed, the Paused week of a goal that is not paused is described, and the exempt states in recent results, history, recent notes and record results are specified. The worked figures of US1 to US4 and FR-031 all recheck correctly. What remains are clean-ups:
- SC-001 is wider than the page requirements allow for Due commitments and the goal page (MINOR-1).
- The reading of SPEC §5.3's "failures already recorded" isn't stated (MINOR-2).
- One edge of the yes/no trend value is open (MINOR-3).
- Earlier slice-5 requirements and one UI test are missing from the impact list (MINOR-4).
- ADR 0024's provenance is imprecise (MINOR-5).

None of these blocks `/speckit-ac-translate` or `/speckit-plan`, though MINOR-2 and MINOR-3 are better settled before the acceptance criteria are drafted.
