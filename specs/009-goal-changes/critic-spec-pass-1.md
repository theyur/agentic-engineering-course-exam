# Critic pass 1: spec for 009-goal-changes

**Verdict**: REVISE
**Date**: 2026-11-22
**Reviewed**: specs/009-goal-changes/spec.md, specs/009-goal-changes/checklists/requirements.md

Figures I recomputed and found correct: US1-AS3/AS4 ("83% (10 of 12)", 0 / 10 commitments), US2-AS2 to AS4 ("85% (12 of 14)", "92% (13 of 14)"), US3-AS3/AS9/AS10, US3-AS6 (Sat 20 Nov 2027 is 367 days after Wed 18 Nov 2026), US4-AS1 to AS4, and every FR-031 demo figure at run date R (Reading "96% (25 of 26)", streaks 6 / 19; Meditation attempt 2 "100% (12 of 12)", attempt 1 last date R − 22 and "52% (11 of 21)"; Sugar "100% (23 of 23)", 23 days; dashboard 2 Due / 4 Pending / 0 Missed / 2 goals; goal-history order). The derived names match `GoalText` / `TargetTextParts` ("Sugar, at most 1 sweet a day", "Yoga, 3 times a week", "Reading, 15 min daily"). The target-check order restated in "Earlier behaviour" matches `TargetFieldCheck` and `CatalogTexts`. FR-024's list of locked features (F001, F002, F004, F005, F007, F008) matches the folders that have an `acceptance.md`.

## Notes

### MAJOR-1: An allowed restart can give one date to two attempts, against FR-013
- **Where**: spec.md "Terms used below", *Ending date* (lines 494–495); FR-008 (lines 561–563); FR-011 (lines 580–581); FR-012 step 6 (lines 588–589); FR-013 (line 597); FR-021 (line 637); Edge case "Ending and restarting on the same day" (lines 472–473).
- **Against**: FR-013 "No date belongs to two attempts"; slice 4 FR-012 ("Today" is the date in the account's *current* time zone, and "the current date can differ from 'today' while a time zone change is on its way"); slice 4 FR-006 (the start-date checks compare with that "today"); ADR 0025 Decision ("A new attempt starts on a chosen date from today on").
- **Reasoning**: The ending date E is the goal's current date on the goal's own timeline. The last date is E − 1. The new attempt's start date is only checked against the *account's* today, and FR-011 even proposes that date as the default. Those two dates differ whenever the goal's version time zone and the account's time zone differ. Two cases allowed by the spec produce this. In the first, an ended goal gets no new version on a time zone change (FR-021). In the second, the user ends the goal while a time-zone version is still waiting, and FR-009 then removes that version. Example: the goal is on Europe/Kyiv. At 16 Nov 23:30 UTC (17 Nov 01:30 Kyiv) the user moves the account to America/New_York (16 Nov 18:30). The user then ends the goal, so E is 17 Nov and the last date is 16 Nov. The user next starts a new attempt with FR-011's default start date, which is the account's today, 16 Nov. That date passes "The start date cannot be in the past.", so 16 Nov now belongs to both attempt 1 and attempt 2. Records on that date would count in both attempts' figures. No check, edge case or scenario covers this, and SC-005 lists midnight and boundary cases but not this one. The spec allows a state that its own invariant forbids, and an implementer has to invent the rule.

### MAJOR-2: "Goal history" is a new domain term with no CONTEXT.md entry, and it clashes with a goal's "history"
- **Where**: spec.md Key Entities "Goal history" (line 857); FR-015 "Reading the goal history" (line 617) next to FR-016 "Reading a goal's history" (line 620); US4-AS3, US5-AS2/AS4; HP-013's list of required CONTEXT.md entries (lines 786–799).
- **Against**: Constitution I ("A change that introduces a new domain term … MUST add or update its CONTEXT.md entry … in the same change"; "In this product a wrong word is a wrong rule"). CONTEXT.md has no "Goal history" entry and no "History" entry. A grep for "history" finds only the Attempt, Correction, Demo … and Ended goal lines.
- **Reasoning**: The spec makes "Goal history" a Key Entity, meaning a user's ended attempts across all goals. It then puts it in adjacent requirements beside slice 5's "a goal's history", meaning one goal's commitments and periods with their states. FR-014 adds a third meaning: an earlier attempt's "history". These are exactly the near-homonyms that constitution I asks CONTEXT.md to separate with a NOT-boundary. HP-013 lists every other new term (Outcome, Ended goal, Settings change) but leaves this one out. The checklist still claims "All items pass".

### MINOR-1: Records kept on the ending date can fall into a new attempt with another measurement, and their evaluation is undefined
- **Where**: spec.md Edge cases "Ending and restarting on the same day … Records kept on that date then belong to the new attempt" (lines 472–473) and "Build to Reduce, or a change of measurement" (lines 460–461); FR-009 (lines 570–571); FR-012 ("settings of any schedule kind", line 584).
- **Against**: slice 5 FR-004 (a record never stores a value that does not belong to the goal's measurement); slice 5 FR-014 (Met or Failed against the target of the version of the date); `HT.Domain/Records/RecordValue.cs` (a record holds either an answer or an amount).
- **Reasoning**: For a settings change, the spec argues that records under the old measurement "stay with their dates", because a change only applies to dates that have not begun. A same-day restart breaks that argument. A duration record kept on E becomes a record of a yes/no or count commitment of attempt 2, and a count amount is read as minutes. Neither the spec nor slice 5 says how such a record is evaluated or shown.

### MINOR-2: Goal-level "start date", "current date", "has not started" and "settings in effect" are undefined once a goal has several attempts or has ended
- **Where**: spec.md FR-001 ("the start date … cannot be changed", line 506); FR-005 ("the settings version in effect on its current date", lines 540–541); FR-007 ("for a goal that has not started, the account's today", lines 494–495, 559–560); FR-017 (lines 625–626); FR-016 (lines 622–624).
- **Against**: slice 4 FR-012 (the current date exists once "its start date's day" has begun) and FR-016 (reading a goal returns "start date" and its attempt). The code: `GoalDetails` has both `StartDate` and `AttemptStartDate`, and its `HasStarted` is the goal's start date. `GoalDetailBody.razor:480-482` builds the attempt line from `goal.HasStarted` plus `AttemptStartDate`. Slice 7 FR-006 step (5) uses "the goal's current date (for a goal that has not started, the account's today)". Slice 6 FR-012's Upcoming window counts from the goal's current date.
- **Reasoning**: Take US3-AS4, where attempt 2 starts on 23 Nov and is read on 18 Nov. It is unclear whether the goal "has started" (attempt 1 did) and what its current date is (no version covers 16–22 Nov). It is also unclear which start date reading the goal returns, and what the "settings in effect" are for an ended goal, which has no version for its current date. Several decisions depend on the answer: the attempt line ("started" or "starts"), slice 7's pause start check, slice 6's Upcoming window and the ending date. FR-004 sets the name rule for these cases, but FR-005 and the goal-level date fields have no matching rule.

### MINOR-3: The earlier attempt's Pending and Missed results after a restart are unspecified on the dashboard and in "what can be recorded now"
- **Where**: spec.md FR-016 (lines 620–624), FR-019 (lines 630–634), Edge case "Ending and restarting on the same day" (lines 472–473), Clarification 3 (lines 48–51).
- **Against**: FR-019's stated purpose ("so that the user can still record them"); SPEC §6 (the dashboard shows overdue and finalized missed results).
- **Reasoning**: FR-019 keeps Pending entries only for an *ended* goal. If the user ends and restarts on the same day, attempt 1's Pending dates (E − 2, E − 1) are no longer the latest attempt. By FR-016 they leave "what can be recorded now", the goal page and, by implication, the Home Today list and Pending count. They then turn Missed without ever being shown. No requirement or scenario says whether a restarted goal's earlier-attempt Pending or Missed-this-week entries stay on the dashboard. The US4 data avoids the case, because Sugar's attempt 1 has no open grace period when it is read.

### MINOR-4: The inputs of "read one attempt" and its history range are unclear
- **Where**: spec.md FR-014 second paragraph (lines 608–611); US3-AS11 (lines 334–337).
- **Against**: slice 5 FR-009 (history takes a caller-supplied range of 1 to 366 days).
- **Reasoning**: FR-014 says the history is "over its own dates" and also that its range "is limited to the attempt's dates and to slice 5's 1 to 366 days". It does not say whether the caller supplies a range, or which 366 days are returned for an attempt longer than that. US3-AS11 reads without a range. Acceptance translation needs the call's inputs.

### MINOR-5: US3-AS2 asserts the attempt-line text, which no FR returns
- **Where**: spec.md US3-AS2 ("its attempt line is 'Attempt 2 · started 18 Nov'", lines 305–306), which is not marked *(page)*; FR-031 ("attempt line 'Attempt 2'", line 703).
- **Against**: FR-017 (returns only the latest attempt's number and start date); HP-008 (the attempt line comes from the page formatter); "What acceptance tests can reach" (ADR 0003). Today the line is built in `HT.Ui` (`GoalDetailBody.razor:482`).
- **Reasoning**: An FR-surface scenario asserts a formatted text that the spec assigns to the host or page side. An acceptance test cannot reach it as specified.

### MINOR-6: US1-AS14's Given cannot be built from the US1 background
- **Where**: spec.md US1 background ("15 minutes recorded on each date from 2 to 15 November", lines 179–181); US1-AS14 ("paused from Wednesday 11 until Friday 13 November", lines 222–223).
- **Against**: slice 7 FR-006 step (5) (a pause cannot start before the goal's current date); slice 7 FR-005 / FR-009 (a record for a paused date is refused with "This goal is paused on this date.").
- **Reasoning**: The pause must exist by 11 Nov. Records for 12 and 13 Nov can only be added once those days begin, and by then the pause refuses them. The scenario as written combines facts that cannot coexist, so translation has to guess which one to drop.

### MINOR-7: HP-006's link rule needs data that FR-015 does not return
- **Where**: spec.md HP-006 (Goal column links "to the attempt view, or to the goal page when the goal's latest attempt is the one ended", lines 757–758); FR-015 fields (lines 617–619).
- **Against**: HP-014 (the latest attempt's number shows Not found on the attempt view).
- **Reasoning**: FR-015 returns the goal identifier, name, number, ending date, outcome and note, but not whether the attempt is the goal's latest. The Profile cannot choose the link target without extra reads per row, and a wrong choice leads to Not found.

### MINOR-8: The catalog details' goal list for ended goals is unspecified
- **Where**: spec.md FR-020 (lines 635–636); US4-AS5 (lines 383–385).
- **Against**: slice 4 FR-031 (details give "the user's goal count and their goals on that habit").
- **Reasoning**: FR-020 covers the counts and "Only habits I track", but not the details' list of goals. The details could then report a goal count of 0 while still listing the ended goal.

### MINOR-9: Ending removes a version already in effect on the ending date, and the consequence is not acknowledged
- **Where**: spec.md FR-009 ("removes … every settings version that takes effect after it", line 570); FR-011 ("the settings of the ended attempt's last version", line 580); FR-004 (an ended goal is named by its latest attempt's last version, lines 535–536); Edge case "Ending a goal with a later version waiting" (lines 480–481).
- **Against**: FR-002 ("A change never alters the version of a date whose day has begun", line 514).
- **Reasoning**: Suppose a change made on E − 1 takes effect on E (or on the Monday of E's week for a per-week goal), and the goal is ended on E. That version is in effect at the ending but starts after the last date, so FR-009 deletes it. The ended goal's name, its Settings card and FR-011's starting values then fall back to the older settings. The user's latest change is lost from "Start new attempt". The edge cases only discuss a version that is still waiting.

### MINOR-10: The demo hardly shows "a goal whose target changed"
- **Where**: spec.md FR-031 Reading ("version 2 from R − 13"; "the later settings: none", lines 677–679, 701); US6 summary (line 424); SC-007 (lines 880–881); HP-001 Settings card (lines 723–724); HP-012 (lines 778–782).
- **Against**: Constitution VI (a reviewer sees the slice's states without waiting).
- **Reasoning**: At run time the change has already taken effect. The goal page shows only the settings in effect, and the "From …" row appears only for a waiting version. No page lists past versions, and the smoke check does not cover Reading. A reviewer sees "Reading, 20 min daily". The only trace of the change is History rows with 15 minutes marked Met, so the slice's main state (US1) is barely visible.

### MINOR-11: An assumption pre-decides the community slice's scope without a marker
- **Where**: spec.md Assumptions ("A later community slice uses each goal's latest attempt, and leaves out ended goals unless its own spec decides otherwise", lines 917–918).
- **Against**: Constitution Decision Discipline; SPEC §7.2 (eligible evaluations "in that window across applicable goals"); SPEC §11.5 / §11.6.
- **Reasoning**: Dropping earlier attempts and ended goals from a rolling 30-day window changes which evaluations qualify. Those are community decisions tied to SPEC §11.5 and §11.6. Even as a soft default, they are neither marked nor recorded in ADR 0025.

### MINOR-12: Page states left open by the HP requirements
- **Where**: spec.md HP-006 (lines 756–760); HP-004 (lines 748–751); HP-003 (lines 739–747) with FR-007 step 5 (line 556); HP-005 (lines 752–755).
- **Against**: `design/pages/PageProfile.md` (an administrator's Profile has no Tabs); HP-002 (which does give the unknown, foreign and ended cases for its page); `design/components/EndGoalDialog.html` (the "Achieved" radio is `checked`).
- **Reasoning**: Four page states are left open:
  - HP-006 does not say whether an administrator's Profile shows the Goal history tab, and US5-AS2 refuses the read.
  - HP-004 gives no behaviour for a goal that is not ended, unknown or another account's. HP-002 does.
  - The design preselects "Achieved", so the dialog can never produce FR-007's "Choose how the goal ended.", and HP-003 does not decide the default.
  - HP-005 does not say whether an ended goal with Pending results shows the Pending chip or "Record" on the Ended tab.

### MINOR-13: Restarting a goal whose catalog habit is Unpublished is unspecified
- **Where**: spec.md FR-012 (lines 584–594); Edge case "A habit unpublished …" (lines 482–483).
- **Against**: slice 4 FR-008 and FR-034 (no new goal on an Unpublished habit; existing goals keep working).
- **Reasoning**: A new attempt is neither a new goal nor plainly "keeps working". FR-012 has no habit check and no scenario covers this, so it is unclear whether the restart is accepted.

## Summary
2 MAJOR, 13 MINOR. Every scenario and demo figure I recomputed is correct, and the spec covers SPEC §5.4 and the D-1 / D-2 decisions well. It is not ready for `/speckit-ac-translate` yet, for two reasons. First, the rules for ending and restarting allow one date to belong to two attempts whenever the goal's time zone and the account's differ, which breaks FR-013's own invariant. Second, "Goal history" is added as a domain term with no CONTEXT.md entry, right beside slice 5's "a goal's history", against constitution I. The MINOR notes are mostly gaps a translator or implementer would otherwise fill by guessing:
- goal-level dates on multi-attempt goals;
- earlier-attempt Pending results after a restart;
- the inputs of "read one attempt";
- one Given that cannot be built (US1-AS14);
- an FR-surface assertion of page text (US3-AS2).
