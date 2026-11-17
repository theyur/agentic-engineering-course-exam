# Critic pass 2: spec for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/spec.md, specs/004-goals/checklists/requirements.md (with CONTEXT.md and docs/adr/0016-time-zones.md checked for consistency)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (spec not self-contained) | Resolved | spec.md lines 117-172 now restate the slice 2 FR-004 comparison, UTF-16 counting, every FR-007 field, range and message in slice 2 order, the owner answers (values not stored, empty yes/no unit, missing times), and the FR-017 text and kind rules. I compared them with specs/002-habit-catalog/spec.md lines 72-88 and 546-631 and found no divergence. |
| MAJOR-2 (duplicate FR-030) | Resolved | FR-030 now appears once (line 783). The catalog requirements are FR-031 to FR-035 (790-810), and FR-019 is used (707). Every FR-031 to FR-035 cross-reference I grepped points at the right topic (lines 52, 305, 692, 710-715, 825, 924-926, 945, 970, 1053, 1074). |
| MAJOR-3 (which version a read returns) | Partly | FR-016 (691-697) now says what a read returns: the version in effect today, and a pending later version. The invariant it relies on ("never more than one taking effect after today", FR-026 line 759) is false in reachable cases. See MAJOR-2. |
| MAJOR-4 (smoke check for demo goals) | Resolved | HP-018 (908-914) extends the smoke check and the `SmokeCheckCoverage` entries and acknowledges the widened port read. The change list (955-958) names slice 3 SC-005, the coverage rule and `CoverageTests`. This matches tests/HT.UiTests/SmokeChecks/CoverageTests.cs and the `IDemoDataStore` doc comment. |
| MAJOR-5 (changes to slice 1/2 contracts and locked F001/F002) | Partly | FR-019 (707-715) and the change list (945-954) now amend slice 2 FR-008/FR-010 and say that F001/F002 must keep passing. That claim is false for locked F002 AC-066. See MAJOR-1. |
| MAJOR-6 (CONTEXT.md deferred) | Resolved | CONTEXT.md (working tree, `updated_at: "2026-11-17"`) has all 12 terms HP-014 lists (Account name, Account time zone, Attempt, Commitment, Commitment-based goal, Evaluation calendar, Evaluation period, Goal settings version, Named time zone, Period-based goal, Personal goal, Target text), each with a NOT-boundary. The Demo account entry names "Demo One". They arrive in the same change as this revision. |
| MINOR-1 ("intent" for a goal) | Resolved | The Tag and Settings row are now "Build or Reduce" / "Build or reduce" (HP-003, HP-004, US2-AS6). The design conflict is listed (984-986). Every remaining "intent" (lines 218, 359, 369, 637-638) refers to the habit. |
| MINOR-2 (repeated time zone changes) | Partly | FR-026 (750-762) now covers replacement, a version already in effect, a not-yet-started goal and two changes on the same day. US10-AS10 (519-523) checks out. The rule still breaks when a second change follows a westward move. See MAJOR-2. |
| MINOR-3 (large-offset changes) | Resolved | FR-027 (763-768) and ADR 0016 now state the gap or overlap of up to 26 hours, and that a first new-zone commitment or period may already have begun or ended. |
| MINOR-4 ("named time zone", installation default) | Partly | FR-024 (733-740) defines the set, and FR-025 (744-746) says how the default reaches the use cases and that acceptance tests expect "UTC". The rule for link names is not decidable. See MINOR-4. |
| MINOR-5 ("first to read the current time") | Resolved | Line 113-115 now cites `TimeProvider` and slice 1's sign-in block. |
| MINOR-6 (checklist stale) | Resolved | checklists/requirements.md iterations 3 and 4 (lines 54-62). The remaining gap is about the pending owner confirmation, see MINOR-7. |
| MINOR-7 (earlier-slice items not listed) | Resolved | Slice 1 HP-015 (943-944), slice 2 HP-009 (949-951), and the TopNav/MobileNav/Profile avatar (HP-017 905-907, design list 989-990) are now listed. |
| MINOR-8 (page data no FR returns) | Resolved | FR-035 (808-809) returns the usage count, HP-002 (846-854) defines the sentence and the Evaluated values, and the design list (986-988) replaces the preview texts. |
| MINOR-9 (meta line / future goals) | Resolved | HP-003 (857-861) gives an order of precedence. FR-014 (678-680) gives the first scheduled day on or after the start date, and US4-AS4 (332-334) now says "while its first week has not begun". |
| MINOR-10 (demo states undemonstrated) | Resolved | FR-040 (814-835) adds "Demo One", "Morning yoga" and the Coffee goal, and states that "first week starts" depends on the weekday. SC-006 (1027-1030) was updated. |
| MINOR-11 (US11-AS6 not testable) | Resolved | US11-AS6 (551-554) is flagged, and the bullet on line 183-185 and HP-019 (915-917) cover it. |
| MINOR-12 (numbering) | Resolved | US4 is numbered 1-8, US7 1-8, US9 1-5 and US10 1-10. The `---` separator before US10 is at line 484. Line 186 states that FR-036 to FR-039 are unused. |

## Notes

### MAJOR-1: Demo goals on demo custom habits break the locked F002 acceptance test AC-066 and an existing integration test
- **Where**: spec.md FR-040 (816-821: demo1's goal on custom habit "Cold shower", demo2's goal on custom habit "Evening stretch"); FR-035 (807-808); FR-019 (707-715, especially "deleting a custom habit that no goal uses deletes it"); "Slice 1 and slice 2 requirements and tests this slice changes" (952-954: "The locked acceptance criteria ... are not changed and must keep passing").
- **Against**: tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature AC-066 (`@FR-031 @US6-AS7`, lines 827-836). After demo loading, it has `demo1@example.com` delete `Cold shower` and `demo2@example.com` delete `Evening stretch` and `Plan tomorrow`, then expects `demo1@example.com lists exactly No phone in bed` and `demo2@example.com lists no custom habit`. tests/HT.IntegrationTests/Operator/SeedCatalogLoadingTests.cs `US6_AS7_deleted_demo_custom_habits_are_never_created_again` (lines 192-203) deletes the same habits and asserts `CustomHabitDeleteOutcome.Deleted` (line 323). CLAUDE.md: `.feature` files and lock files are never edited without an unlock.
- **Continues**: MAJOR-5
- **Reasoning**: Under FR-040, every run that creates demo1 and demo2 gives them goals on "Cold shower" and "Evening stretch". These goals do not depend on the catalog, so even AC-062's catalog with only a Draft gets them. Under FR-035, deleting a custom habit that a goal uses gives "Goals use this custom habit, so it cannot be deleted." and changes nothing. The AC-066 deletions in its Given steps therefore fail, and both of its list assertions fail. The integration test fails the same way. FR-019's promise only covers custom habits "that no goal uses", so it does not protect this scenario. The change list asserts that F001/F002 keep passing, but it does not mention this conflict, AC-066, slice 2 US6-AS7 or the integration test. As specified, the slice cannot ship with the locked F002 suite green, and only an unlock can change that suite.

### MAJOR-2: FR-026 measures the change date C in the old time zone but "today" in the new one, so its invariant fails and a second change can move a day that has already begun
- **Where**: FR-026 (750-762: C is "today in the account's time zone before the change"; E is "the day after C" for commitment-based and per-day goals; "never more than one taking effect after today"); FR-016 (694-697); FR-012 (663-664: "today" is the date in the acting user's *current* time zone); FR-011 (659-660: a change "never moves a commitment or evaluation period that has already begun or ended"); Clarifications line 65-67; ADR 0016 Decision, third bullet.
- **Against**: Constitution II (deterministic evaluation against the rule version in effect; nothing past may move). FR-011's own promise.
- **Continues**: MAJOR-3, MINOR-2
- **Reasoning**: E is computed from C, the old-zone date. It does not use the zone of the version that actually governs the next day, which can differ from the account's current zone after an earlier change. Two reachable cases show the problem, checked with the spec's own zones (Kyiv UTC+2, New York UTC-5 in November):
  - **Two versions after today.** A per-day goal starts on Thu 19 Nov. The user moves Kyiv → New York at 01:00 Kyiv on Thu 19 Nov, which is 18:00 New York on Wed 18. Then C = Thu 19. The first version applies from Thu 19 = C, so it is not "after C", and FR-026 adds a New York version from E = Fri 20. FR-012 now makes today Wed 18. Both versions (Thu 19 and Fri 20) take effect after today. That contradicts "never more than one taking effect after today". FR-016 then has two candidates for "the version that takes effect after today".
  - **A begun day moves.** A per-day goal has been running in Kyiv. The same change at 01:00 Kyiv on Thu 19 adds a New York version from Fri 20. At 18:30 New York on Wed 18 the user changes to, for example, America/Chicago. Now C2 = Wed 18 (New York), and E2 = Thu 19. The goal "already has a version taking effect after C2" (Fri 20), so that version gets Chicago and the date Thu 19. Thursday 19 was governed by the Kyiv version and began at 17:00 New York on Wed 18, before the second change. It now runs on Chicago midnights. That is exactly the move FR-011 forbids, and FR-027's "kept as it is" does not cover it, because the day was already under way in its own version.

  Neither case is in an acceptance scenario, but FR-026 and ADR 0016 state the invariant as unconditional. The unit tests of constitution II (both sides of each boundary) and the implementation would have no consistent rule to follow.

### MINOR-1: US9-AS3 contradicts FR-040 and US9-AS5 for the Coffee goal
- **Where**: US9-AS3 (476-477): "a catalog habit named in FR-040 is not Published when the account is created ... that one demo goal is left out"; FR-040 (829-831): the Coffee goal is left out only "when 'Coffee' does not exist or is a Draft"; US9-AS5 (480-482): demo3's goal on the Unpublished "Coffee" exists.
- **Continues**: new
- **Reasoning**: "Coffee" is a catalog habit named in FR-040, and in the seed catalog it is not Published (src/HT.Application/Operator/SeedCatalog.cs line 112, `CatalogHabitState.Unpublished`). Taken literally, US9-AS3 removes the Coffee goal, which AS5 and FR-040 require. The AC translator either reports the conflict or has to guess which habit AS3 means.

### MINOR-2: FR-040 reverses slice 2's order of demo loading without listing it as a change
- **Where**: FR-040 (814-815: goals created "as a whole with the account ... after the seed catalog and the demo custom habits"); US9-AS3 ("when the account is created"); change list line 933-934 ("extended by FR-040").
- **Against**: specs/002-habit-catalog/spec.md FR-030 (line 650): "After its account step ... it creates the seed catalog". src/HT.Application/Operator/DemoDataService.cs `LoadAsync` creates each account with its custom habits first, then calls `TryCreateSeedCatalogAsync`.
- **Continues**: new
- **Reasoning**: FR-040 decides whether a catalog goal is left out from the habit's state at account creation. On an empty database, the current order creates the accounts before any seed habit exists. Every catalog demo goal would then be left out, and US9-AS1, US9-AS5 and SC-006 would fail. FR-040 therefore needs the seed catalog before the account step, which reverses slice 2 FR-030's stated order. The change list only says "extended" and does not record that slice 2 FR-030's order changes.

### MINOR-3: A Reduce yes/no target text on fixed days says the opposite of what FR-004 evaluates
- **Where**: FR-004 (611-612: a commitment-based Reduce yes/no commitment "is met when ... not done"); "Target text and kind" (157-161: fixed days give "daily", "once a week" or "N days a week", and a Reduce text starts with "at most "); FR-010 (651-653); HP-002 (846-849).
- **Continues**: new
- **Reasoning**: A goal of Reduce, Yes/no, fixed days Mon–Fri gets the target text "at most 5 days a week". All seven days gives "at most daily". These words suggest an allowance of up to 5 days, or up to every day. Under FR-004, each scheduled day is met only when the behaviour is *not* done. The same text becomes the goal's derived name (FR-010) and the summary sentence ("…: at most 5 days a week, starting …"), which HP-002 presents as the plain-words meaning of the goal. The seed has no such suggestion, but FR-006 accepts the combination, so a user can create it. The spec does not acknowledge the mismatch.

### MINOR-4: Which link names count as named time zones is not decidable
- **Where**: FR-024 (736-740: "A link name of that database that browsers still give ('Europe/Kiev') is accepted"); FR-025 (742-743: "an absent or unknown value"); HP-015 (898-899: the Profile field "with the named time zones"); CONTEXT.md "Named time zone".
- **Continues**: MINOR-4
- **Reasoning**:
  - "That browsers still give" is not a fixed set. It changes by browser and version, and nothing in the spec or repo lists it. A test cannot say whether "Asia/Calcutta", "US/Eastern" or "Etc/UTC" (all IANA names) is accepted.
  - Three-part identifiers such as "America/Argentina/Buenos_Aires" are not clearly "Area/Location".
  - FR-025 says "unknown", while FR-024's excluded set also covers *known* Windows and "Etc/…" names. It is therefore unclear whether "FLE Standard Time" at registration falls back to the default.
  - HP-015 does not say whether the Profile list includes link names, so an account registered as "Europe/Kiev" may hold a value its own Profile list cannot show.

### MINOR-5: US4-AS8 creates a goal "starting 'today'", which is not an input of any use case
- **Where**: US4-AS8 (342-343); FR-006 (628-630: the start date is a supplied date, checked against today); FR-007 (641: starting values give today).
- **Continues**: new
- **Reasoning**: FR-006 has no "today" value. A creation is given a date, so "its start date is that Monday" follows trivially from passing Monday. The scenario does not say which behaviour it tests at 23:30 and 00:30. It could be the start date in the starting values (FR-007), or that Monday is accepted at 23:30 and refused as "in the past" at 00:30 (FR-006). Each reading gives a different acceptance criterion.

### MINOR-6: FR-002/FR-005 put the name and start date into the settings version, which Key Entities and CONTEXT.md exclude
- **Where**: FR-002 (602-604: the settings include "the start date; an optional name"); FR-005 (619: "The goal's settings MUST be kept as its first settings version"); Key Entities "Goal settings (version)" (997-998); CONTEXT.md "Goal settings version" ("the goal keeps its identity, name and history across versions").
- **Continues**: new
- **Reasoning**: Read together, FR-002 and FR-005 make the name and start date part of each settings version. The Key Entities and the glossary list the version's contents without them and keep the name on the goal. Slice 8 edits settings by adding versions, and FR-026 copies "all other settings" into a new version. Whether a name change, or the start date, is versioned is therefore left contradictory.

### MINOR-7: The critic-revision defaults are recorded as open in the spec but as settled elsewhere
- **Where**: spec.md Clarifications line 33-34 (inside the owner's dated answer: "What a named time zone is, and what repeated or large changes do, are this spec's defaults"); "Critic revision 2026-11-17" (60-83); "Decisions due before acceptance criteria" line 1055 ("Open: the owner's confirmation ..."); checklists/requirements.md iteration 3 ("no decision is open") and iteration 4 ("All items pass"); docs/roadmap.md time-zone row ("decided").
- **Against**: Constitution, Decision Discipline: a spec touching SPEC §11 or ARCHITECTURE §9 (time zone rules) marks it instead of assuming, and the owner's answer is what is recorded in `## Clarifications`.
- **Continues**: new
- **Reasoning**: The named-time-zone set, the 26-hour rule, FR-019, the demo names and the "Updated" column are author defaults awaiting confirmation. The spec says so on lines 82 and 1055. The first Clarifications bullet, however, embeds some of them in the owner's dated answer. The checklist's latest iteration passes every item without noting that a decision is still open before `/speckit-ac-translate`, and the time zone rules carry no marker. A reader of the checklist or the roadmap would take the slice as fully decided.

## Summary
2 MAJOR, 7 MINOR. Previous notes: 14 Resolved, 4 Partly, 0 Unresolved.

The revision fixes most of pass 1:
- the spec now restates the slice 2 rules it depends on, and the restated messages and texts match slice 2 exactly;
- the FR numbering is clean;
- the smoke check and coverage rule are extended;
- CONTEXT.md carries the new terms;
- I checked the date, weekday and time zone arithmetic of US10 and US11 and found it correct.

It is not ready for `/speckit-ac-translate` for two reasons:
- The new demo goals on "Cold shower" and "Evening stretch", together with the new delete refusal, make the locked F002 AC-066 and its integration test fail. This contradicts FR-019's promise that F001/F002 pass unchanged.
- FR-026 computes the change date in the old time zone while reads and "today" use the new one. As a result, its "never more than one version after today" invariant fails, and a second change after a westward move can move a day that has already begun, which FR-011 and constitution II forbid.

The minor notes cover:
- a contradiction between US9-AS3 and FR-040 for Coffee;
- the unacknowledged reversal of slice 2's demo-loading order;
- misleading Reduce yes/no fixed-day texts;
- the undecidable link-name rule;
- an ambiguous US4-AS8;
- an inconsistency about what a settings version contains;
- the pending owner confirmation, which the checklist does not reflect.
