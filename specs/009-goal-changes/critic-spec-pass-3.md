# Critic pass 3: spec for 009-goal-changes

**Verdict**: APPROVE
**Date**: 2026-11-22
**Reviewed**: specs/009-goal-changes/spec.md, specs/009-goal-changes/checklists/requirements.md (with the changed reference material CONTEXT.md, docs/adr/0025-goal-change-rules.md, docs/adr/0026-demo-goal-change-loading.md)

I recomputed the following figures and found them correct:

- US2-AS12: ended on 17 Nov, before the start date 23 Nov. The ending date is the account's today, 17 Nov, so the last date is 16 Nov, which is before the start date. The attempt has no commitment and its consistency is "N/A". FR-009 keeps its only version, so the name and the starting values hold.
- US3-AS4: on 19 Nov the goal has started (attempt 1 began), so its current date is 19 Nov in UTC. 15 Nov is final from 18 Nov 00:00, so recording it is a correction to Met, and attempt 1 is 13 of 14 = 92.86%, shown as "92% (13 of 14)".
- US3-AS6 at 18 Nov 08:00: 17 Nov is before today, and that check comes first. 20 Nov 2027 is a Saturday and 367 days ahead (the year from 18 Nov 2026 to 18 Nov 2027 has 365 days). On 16 Nov at 11:00, 16 Nov passes the "past" check and then fails "on or before the ending date".
- US3-AS13: Kyiv is UTC+2 in November 2026 (its summer time ended on 25 Oct). The change to UTC happens at 16 Nov 23:30 UTC, when the goal's current date is 17 Nov, so a waiting version from 18 Nov is added (slice 4 FR-026). Ending at 23:40 UTC gives E = 17 Nov and L = 16 Nov, and the waiting version (after L, not the first version) is removed. The starting values are max(16 Nov, 18 Nov) = 18 Nov, and 17 Nov is refused with "A new attempt starts after the day the goal ended.".
- US3-AS12, US3-AS3, US3-AS9, US2-AS3, US2-AS10, US2-AS11, US4-AS2 to AS4 and AS8, and US3-AS15 ("2 Nov – 15 Nov"): unchanged and correct.
- FR-031: Reading "96% (25 of 26)" with streaks 6 and 19, Meditation attempt 2 "100% (12 of 12)", attempt 1 "52% (11 of 21)" (last date R − 22), Sugar "100% (23 of 23)", the dashboard (2 Due, 4 Pending, 0 Missed, 2 goals) and the goal history order (Sugar first). All unchanged and correct.

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (ending removes all versions of a not-begun attempt) | Resolved | FR-009 (lines 625–631) now removes later versions "except the attempt's first version", also "when the attempt ends on or before its start date". The new term *Last version* (lines 552–553) and FR-004, FR-005 and FR-011 take their values from that version. US2-AS12 (lines 278–283) asserts the kept name and starting values. The edge case "Ending on the day a change took effect" (lines 518–523) is aligned. ADR 0025 Decision says the same. |
| MAJOR-2 (has-not-started takes away the current date for an earlier attempt's dates) | Resolved | *A goal that has not started* (lines 543–545) keeps slice 4's meaning: no attempt has begun. *Current date* (lines 546–551) gives a goal whose new attempt has not begun a current date, so slice 5 FR-002 accepts the dates of an earlier attempt. US3-AS4 (lines 320–322) asserts this, and "Earlier requirements" (lines 904–906) lists the extension of slice 4 FR-012. The new definition opens two smaller gaps, which MINOR-1 and MINOR-2 report as new notes. |
| MINOR-1 (page start date "today") | Resolved | US3-AS14 (lines 361–365) now says "the start date of FR-011". Its parenthetical paraphrase is incomplete (MINOR-4). |
| MINOR-2 (US3-AS6 without a supplied time) | Resolved | The scenario now says "on Wednesday 18 November at 08:00" (line 326). Recomputed above. |
| MINOR-3 (attempts with no date; Pending entries of an ended goal's earlier attempt) | Resolved | HP-001 (lines 804–805) and HP-014 (lines 889–890) add "Attempt {n} · ended before it began". FR-019 (lines 702–705) covers "any ended attempt of any of the user's goals, whether the goal is ended or has restarted", with the flags for the latest attempt and for an ended goal. HP-007 (lines 843–844) chooses the Tag by "not the goal's latest". FR-016 carries the same flag. |
| MINOR-4 (ADR 0025 removal scope and provenance) | Partly | Removal scope: ADR 0025 Decision now matches FR-009, the in-effect version and the kept first version included. Provenance: ADR Status and spec D-2 (lines 1002–1006) now name the author's restart rule "for the owner's review". A residual mismatch remains (MINOR-5). |
| MINOR-5 (edge case cites scenarios that never make today and the ending date differ) | Resolved | The new US3-AS13 (lines 353–360) sets up a Kyiv-to-UTC change before the ending, with the account's today on 16 Nov and the ending date on 17 Nov. The edge case (lines 501–504) cites it. |

No pass-1 note has regressed. Pass-1 MAJOR-1 (one date in two attempts) still holds under the new current date: the start date is checked against both the account's today and the ending date. The FR-012 argument behind pass-1 MINOR-1 also still holds: at the ending, no day after E has begun on the goal's timeline. The pass-1 figures I rechecked are unchanged.

## Notes

### MINOR-1: While a new attempt waits in a time zone west of the earlier attempt, the current date runs ahead of the new attempt's own days
- **Where**: spec.md "Terms used below", *Current date* (lines 546–551: "the date of the supplied time in the time zone of the last version of the latest attempt that has begun … No commitment or period lies on a current date outside an attempt"). The setup is US3-AS13 (lines 353–360) followed by a restart. FR-012 (the new attempt uses "the account's current time zone").
- **Against**: slice 5 FR-002 (specs/005-recording-and-evaluation/spec.md lines 487–491: a record is accepted only when "that date's day has begun on the goal's timeline (the goal's current date or earlier, slice 4 FR-012)", otherwise "You cannot record a day that has not begun."). Slice 4 FR-012 (a goal's current date "is the latest date whose day of the goal has begun at that instant, each day in the time zone of its own settings version"). In the code, `GoalTimeline.HasBegun` is `CurrentDate(goal, instant) is { } current && date <= current` (src/HT.Domain/Goals/GoalTimeline.cs:119–120).
- **Continues**: new (opened by the fix of pass-2 MAJOR-2)
- **Reasoning**: Continue US3-AS13. Attempt 1's last version is in Europe/Kyiv. The user starts attempt 2 with the start date 18 Nov in the account's time zone, now UTC. At 17 Nov 23:00 UTC:
  - Attempt 2 has not begun, because its 18 Nov starts at 18 Nov 00:00 UTC.
  - So the new term takes the current date from attempt 1's last version, in Kyiv, which gives 18 Nov.
  - 18 Nov is a commitment of attempt 2, so slice 5 FR-002 accepts a record for it, although its day has not begun on its own timeline. By its own instants, 18 Nov is still Upcoming (slice 5 FR-010). "What can be recorded now" does not offer it, but the record use case accepts it.
  - The window lasts as long as the time-zone difference (2 hours here), and it opens whenever the new attempt's time zone is west of the earlier attempt's.

  The definition breaks the property of slice 4 FR-012 it claims to extend: the current date is a date whose day has begun. "Earlier requirements" (lines 904–906) does not acknowledge this. It is a narrow window, and the record lands in the right attempt with that attempt's measurement, so it is MINOR. It is still a time-boundary rule that constitution II's tests (SC-005) are meant to pin, and no FR or scenario decides it.

### MINOR-2: FR-005's "version that takes effect after its current date" now catches a waiting attempt's first version
- **Where**: spec.md FR-005 (lines 595–599: reading returns the version in effect "and the version that takes effect after its current date, if any … For a goal whose latest attempt has not begun, they are that attempt's first version"). HP-001 Settings card (lines 798–799: the row "From {date}" "when a later version exists").
- **Against**: slice 4 FR-016 (the later version is given only "when the goal has a version that takes effect after its current date"; a goal with no current date has none). The new *Current date* (lines 546–551) gives a goal whose new attempt waits a current date in the earlier attempt.
- **Continues**: new (a consequence of the pass-2 MAJOR-2 fix; the same area as pass-1 MINOR-2)
- **Reasoning**: Take US3-AS4's goal on 19 Nov. Its current date is 19 Nov, and attempt 2's first version applies from 23 Nov, which is after the current date. FR-005 sets the settings "in effect" to that version ("they are that attempt's first version"). Its first sentence also returns "the version that takes effect after its current date, if any", and the same version meets that condition.
  - Under slice 4 such a goal had no current date, so there was no later version. Now it has one.
  - The spec does not say whether reading gives that version once, as the settings in effect, or twice, as both.
  - The goal page's Settings card follows from this: does it show a "From 23 Nov" row?
  - No scenario reads the settings of a goal whose new attempt waits, so the acceptance criteria do not decide it.

### MINOR-3: An earlier attempt's dashboard and "record now" entries carry no rule for their name or Build/Reduce
- **Where**: spec.md FR-016 (lines 693–694) and FR-019 (lines 702–705): entries of an ended attempt give "its attempt number, whether that attempt is the goal's latest, and whether its goal is ended". HP-007 (lines 843–844: the Tag "after the goal name").
- **Against**: Clarifications 2026-11-22 (clarify), the fourth answer ("Each attempt is named as it was at its last date … the latest attempt keeps the goal's current name"). FR-004 (an earlier attempt "MUST be named as it was at its last date"). Slice 6 FR-012 (specs/007-personal-reports/spec.md lines 582–583: each Today entry has "its goal (identifier, name, Build or Reduce)"). FR-012 and US3-AS5 (a new attempt may have any settings, so Build or Reduce and the measurement can change).
- **Continues**: new (FR-019 widened in this revision to every earlier attempt)
- **Reasoning**: FR-019 now keeps the Pending entries of a restarted goal's earlier attempt. Under slice 6 FR-012 each entry carries the goal's name and Build or Reduce, which are the latest attempt's.
  - Example: attempt 1 is Build "Meditation, 10 min daily" and attempt 2 is Reduce "Meditation, at most 1 …". Attempt 1's Pending 15 Nov would be listed under attempt 2's name and with Reduce.
  - The clarification answer and FR-004 name an earlier attempt by its own last settings, but neither FR-016 nor FR-019 says that this applies to these entries.
  - US3-AS12 restarts with unchanged settings, so it cannot tell the two readings apart.

### MINOR-4: Two new scenario texts can be translated wrongly
- **Where**: spec.md US3-AS13 (lines 353–356) and US3-AS14 (lines 362–364).
- **Against**: User Story 3's background ("the goal 'Meditation, 10 min daily' of User Story 2, ended on Monday 16 November", line 303). FR-007 step 5 ("no outcome 'Choose how the goal ended.'"). FR-011 (the start date is "the later of the account's today and the day after the ending date"). US3-AS13's own result (the start date 18 Nov while the account's today is 16 Nov).
- **Continues**: new (pass-2 MINOR-1 and MINOR-5 led to these texts)
- **Reasoning**:
  - US3-AS13 says "a goal like the background's". In User Story 3 the background is an ended goal, which cannot then be ended at 23:40 UTC (FR-007 step 4). US2-AS12 says "with the background's settings", but this scenario does not.
  - US3-AS13 also says "the user ends the goal" with no outcome. Read literally, FR-007 step 5 refuses that call with "Choose how the goal ended.".
  - US3-AS14's parenthetical reads "the day after the ending date when it is opened on the day of the ending, otherwise today". In US3-AS13's state the page opens on the account's 16 Nov, which is not the ending date. "Otherwise today" then gives 16 Nov, but FR-011 gives 18 Nov, and FR-012 would refuse 16 Nov. A UI test written from the parenthetical would assert the wrong default for exactly the case the time-zone scenario adds.

### MINOR-5: The provenance of the first-version rule differs between ADR 0025 and the spec, and the ADR is "Accepted" with parts awaiting the owner
- **Where**: docs/adr/0025-goal-change-rules.md Status (lines 3–6: "The rule that a new attempt starts after the ending date, and the attempt's first version staying at an ending, are consequences of D-2 added by the author after the spec review … for the owner's review"). spec.md D-2 (lines 1002–1006), which marks only the restart rule as the author's consequence. FR-009 (lines 627–630), which states the first-version rule with no provenance. Checklist iteration 5 ("all items still pass").
- **Against**: constitution, Decision Discipline (an answer "comes from the project owner … and is recorded in the spec's `## Clarifications` section with its date"; "Changing a recorded decision requires a new or superseding ADR").
- **Continues**: MINOR-4 (pass 2)
- **Reasoning**: The removal scope now matches, and the restart rule's provenance is honest in both documents. Two gaps remain:
  - **Which rules await review.** The ADR marks two rules as awaiting the owner's review, and the spec marks one. A reviewer who checks D-2 and Clarifications would not see that the first-version rule (FR-009) is also unconfirmed.
  - **Status.** The ADR's Status still reads "Accepted, 2026-11-22" for content that it says awaits review. Once ADR 0025 is accepted, the constitution requires a superseding ADR to change it. If the owner rejects either consequence after the acceptance criteria are locked, the cost rises, and no marker or open item in the spec or checklist tracks the pending review.

## Summary
0 MAJOR, 5 MINOR. The revision resolves both pass-2 MAJOR notes:
- Ending keeps the attempt's first version.
- The current date is extended so that an earlier attempt's dates stay recordable while "a goal that has not started" keeps slice 4's meaning.

Four of the five pass-2 MINOR notes are resolved, and ADR 0025's provenance is partly fixed. Every figure I recomputed is correct, the new time-zone scenario US3-AS13 included. The remaining notes are small and do not block `/speckit-ac-translate`:
- the new current date runs a few hours ahead of a waiting new attempt in a western time zone;
- FR-005's "later version" is ambiguous for that waiting attempt;
- an earlier attempt's dashboard entries have no rule for their name or Build/Reduce;
- three wording slips in US3-AS13 and US3-AS14;
- a residual provenance mismatch in ADR 0025.

The author can clean them up before the acceptance criteria are drafted.
