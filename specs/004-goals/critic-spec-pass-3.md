# Critic pass 3: spec for 004-goals

**Verdict**: APPROVE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/spec.md, specs/004-goals/checklists/requirements.md (with CONTEXT.md, docs/adr/0016-time-zones.md and docs/roadmap.md checked for consistency)

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (demo goals on custom habits break F002 AC-066) | Resolved | FR-040 (spec.md 839-848) now puts demo1's custom goal on "No phone in bed" and demo2's second goal on catalog "Meditation"; line 854-855 states no demo goal uses "Cold shower", "Evening stretch" or "Plan tomorrow". AC-066 (habit-catalog.feature 827-836) deletes exactly those three and expects demo1 to list "No phone in bed", which a goal keeps from deletion anyway. AC-064/AC-065/AC-067/AC-068 list or create only custom habits, which FR-040 leaves unchanged. `SeedCatalogLoadingTests.US6_AS7_…` deletes the same three habits. I found no locked F001/F002 scenario that unpublishes a seed habit named by FR-040. |
| MAJOR-2 (FR-026 measures C in the old zone) | Resolved | FR-012 (678-681) defines the goal's current date on its own versions' days; FR-026 (770-779) measures D and E from it; FR-016 (711-715) reads the version of the current date and at most one later. Both pass-2 cases now hold: (a) per-day goal starting Thu 19, Kyiv→New York at 23:00 UTC Wed 18: D = Thu 19, one New York version from Fri 20, nothing else after D; (b) second change to Chicago at 23:30 UTC Wed 18: D2 = Thu 19, the pending Fri 20 version gets Chicago, Thu 19 stays Kyiv. I also showed that the current date never decreases, so a pending version's date always equals the new E. One summary sentence is still wrong, see MINOR-3. |
| MINOR-1 (US9-AS3 vs Coffee) | Resolved | US9-AS3 (487-489) now says "'Coffee' is a Draft", matching FR-040 (853-854) and US9-AS5. A new setup gap in the same scenario is MINOR-5. |
| MINOR-2 (reversed demo-loading order not listed) | Partly | FR-040 (836-838) and the change list (959-961) now state the reversal. The existing tests that assert the old order are not listed, see MINOR-2. |
| MINOR-3 (Reduce yes/no fixed-day text) | Partly | The goal text exception (169-172) fixes the goal's name and summary. A suggested starting goal keeps the old text, and it is shown next to the goal's new text, see MINOR-6. |
| MINOR-4 (link names undecidable) | Resolved | FR-024 (753-760) gives a decidable rule (exactly "UTC", or an IANA zone or link containing "/" and not starting with "Etc/") with examples on both sides, including three-part names and "US/Eastern". FR-025 (762-763) covers "FLE Standard Time". HP-015 (924-925) keeps a current link value visible. The Profile list's own rule raises a new problem, see MINOR-4. |
| MINOR-5 (US4-AS8 "starting today") | Resolved | US4-AS8 (351-354) now tests the starting values' start date and the acceptance or refusal of that Monday at 23:30 and at 00:30, matching FR-006 (641-642) and FR-007 (654). |
| MINOR-6 (name and start date in the settings version) | Resolved | FR-002 (614-619) says the habit, start date and name belong to the goal, not to a version. Key Entities (1025-1030) and CONTEXT.md "Goal settings version" (line 47) agree. FR-005 (632-633) keeps only the start date as the first version's effective date. |
| MINOR-7 (critic defaults recorded as settled) | Partly | The defaults sentences are gone from the owner's Clarifications bullets (27-34, 52-55). The "Critic revision" subsection (59-62), the "Open:" line (87-88), the decisions list (1086), checklist iteration 5 and the roadmap time-zone row (roadmap.md line 182) all say confirmation is pending. The constitution's marker is still missing, see MINOR-7. |

None of the pass-1 notes has regressed. I re-checked MAJOR-1/2/4/6 and MINOR-1, 8, 9, 10 and 12 of pass 1 against the current text. The FR-031 to FR-035 cross-references, the smoke-check extension (HP-018), the CONTEXT.md terms and the scenario numbering are intact. SC-006's coverage holds with the new demo set: Meditation is the future goal, "No phone in bed" and "Late snacks" are on custom habits, Coffee is on an Unpublished habit and Morning yoga has its own name.

## Notes

### MINOR-1: Listing compares with the account's "today", which a time zone change can put on the other side of the goal's current date
- **Where**: spec.md FR-014 (693-700): "the first commitment whose date is today or later", "when its start date is after today", "Dates are compared as dates in the acting user's current time zone (FR-012), not as instants". FR-012 (680-681): "The current date can differ from 'today' while a time zone change is on its way". HP-003 (882-886, "next today").
- **Against**: FR-026 (770-781), whose goal timeline decides which days have begun. FR-011 (672-673).
- **Continues**: new (a consequence of the pass-2 MAJOR-2 fix)
- **Reasoning**: Take a Mon/Wed/Fri goal running in Kyiv (UTC+2). The user moves to New York at 01:00 Kyiv on Thu 19 Nov (23:00 UTC Wed 18). Under FR-026, D = Thu 19 and E = Fri 20. Wednesday 18's Kyiv commitment ended at 22:00 UTC. Account "today" is now Wed 18 (18:00 New York), so FR-014 returns Wed 18 as the next commitment and HP-003 shows "next today" for a commitment that is already over. That lasts until New York midnight. The same applies to "starts …" for a goal whose start date's day has already begun in its own zone. The reverse happens after a move east. New York to Kyiv at 20:00 New York Wed 18: the goal's Wed 18 commitment runs until 05:00 UTC Thu 19, but today is Thu 19, so the listing skips the commitment in progress and shows Fri 20. FR-012 acknowledges that the two dates can differ but does not say which one FR-014 should use. The listing therefore gives wrong data in these windows, and no scenario covers them.

### MINOR-2: Existing tests that assert slice 2's demo-loading order are not listed as changing
- **Where**: spec.md FR-040 (836-838); change list (959-964), which lists only `StartupTasksDemoTests` ("gains the 'Demo goals' lines") and `SeedCatalogLoadingTests` ("keeps passing").
- **Against**: tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs lines 276-283 (`The_work_runs_in_order_each_account_with_its_custom_habits_then_the_seed_catalog`, which expects `[account demo1, account demo2, account demo3, catalog]`) and lines 285-293 (`An_existing_account_is_skipped_and_the_seed_catalog_still_comes_last`). tests/HT.UiTests/Operator/StartupTasksDemoTests.cs line 24 ("the catalog line comes once, last") and `AssertDemoLines` (118-127), which compares the exact sequence. The `DemoDataService.LoadAsync` doc comment.
- **Continues**: MINOR-2 (pass 2)
- **Reasoning**: Reversing the order makes both unit tests fail, and the change list does not mention them. FR-040 and HP-013 do not say whether the "Demo catalog" log line also moves before the account lines or stays last. `StartupTasksDemoTests` checks that exact order, so the planner has to guess. FR-040 also drops slice 2 FR-030's explicit "only when it has a valid demo password" from the moved catalog step, and locked AC-063 depends on that condition. FR-019 implies it but does not state it.

### MINOR-3: FR-026's closing sentence about two changes on one day is false after a large move east
- **Where**: spec.md FR-026 (782-783): "Two changes on the same day are handled the same way: the second replaces what the first added."
- **Against**: FR-026's own rule (774-779) and FR-027 (787-789).
- **Continues**: MAJOR-2 (pass 2), as a remaining inaccuracy
- **Reasoning**: Take a per-day goal in New York. The user moves to Kyiv at 01:00 UTC Thu 19 (20:00 New York Wed 18). D = Wed 18, and a Kyiv version is added from Thu 19, whose Kyiv day began at 22:00 UTC Wed 18. A second change at 01:30 UTC, the same day in both zones, finds the current date Thu 19, governed by the first change's version. That version therefore takes effect on the current date, not after it, so the rule adds a third version from Fri 20 instead of replacing the first. The rule's outcome is correct, because no begun day moves. The summary sentence contradicts it, and an acceptance criterion translated from that sentence would expect the wrong result.

### MINOR-4: The Profile list's "zones, not links" leaves out "UTC" and depends on the time zone data version
- **Where**: spec.md HP-015 (923-925): "Its list holds the named time zones of FR-024 that are zones, not links, plus the account's current value when that is a link". FR-024 (753-758) admits "UTC" by its exact name, apart from the zone or link rule. FR-025 (763-766): the default and the acceptance-test default is "UTC".
- **Against**: The IANA data (external; not in the repo) lists `UTC` as a link to `Etc/UTC` in its `backward` file. Its 2022b release turned several long-standing zones into links (for example `Europe/Amsterdam`). ADR 0015 (line 30, 60): the host is App Service on Linux, while local and CI test runs use Windows (CLAUDE.md, `.\COLL1`), so the two environments read different copies of the data.
- **Continues**: new
- **Reasoning**:
  - If "UTC" counts as a link, the Profile list offers it only to an account whose current value is UTC. A user who once picked another zone can never choose UTC again, although FR-024 makes it a named time zone and FR-025 makes it the default.
  - If "UTC" is meant to be in the list, HP-015 does not say so.
  - Whether an identifier counts as a zone or a link depends on the data version, so the list differs between the Windows test machines and the Linux host. A user whose browser gives such a former zone keeps it only as the current value.
  - The spec does not say which copy of the database defines the list, so a UI test cannot fix what it should contain.

### MINOR-5: US9-AS3's starting catalog is underspecified, and FR-040 does not say how a named catalog habit is matched
- **Where**: spec.md US9-AS3 (487-489): "Given a catalog in which 'Reading' is not Published, or 'Coffee' is a Draft, when the account is created … that one demo goal is left out and the others are created". FR-040 (836-838, 852-854).
- **Against**: FR-040's order: the seed catalog is loaded only into an empty catalog (slice 2 FR-030; locked AC-062 shows a non-empty catalog gets no seed).
- **Continues**: new
- **Reasoning**:
  - With the seed now loaded first, "Reading" can only be non-Published at account creation if the catalog was already non-empty. In that case no seed is loaded, and the catalog holds only what the scenario created.
  - "The others are created" is then true only if the scenario also created Published "Yoga", "Meditation" and "Sugar" and a non-Draft "Coffee". The Given does not say so.
  - Read minimally, as AC-062 sets up its catalog, the scenario's Then fails, because the other catalog goals are left out too.
  - FR-040 also does not say how a demo goal finds "its" habit in a catalog the seed did not create: by exact name, or by slice 2 FR-004's case-insensitive comparison. A translator needs that to set up AS3.

### MINOR-6: A Reduce yes/no fixed-days suggestion still reads the opposite of the goal made from it, side by side
- **Where**: spec.md "Target text and kind" (169-172: "a suggested starting goal keeps slice 2's text"); FR-007 (654-655: the catalog suggestion text is the suggestion's slice 2 text); HP-002 (871-877: the summary card shows the goal sentence and "Catalog suggestion: …"). CONTEXT.md "Target text" (line 60: one text for "a goal's or a suggested starting goal's target").
- **Against**: FR-004 (624-625: a Reduce yes/no commitment is met when not done).
- **Continues**: MINOR-3 (pass 2)
- **Reasoning**: Keeping slice 2's suggestion text is sound, since slice 2 is shipped and its texts are locked. Nothing in the seed or in F002 uses this combination; I grepped for "at most daily", "at most once a week" and "at most N days a week" and found none. An administrator can still create such a suggestion. Starting from it, the New goal summary card shows "X: avoided on Mon–Fri, starting …" next to "Catalog suggestion: at most 5 days a week", which is the misleading text pass 2 described, now on the same card. The glossary entry still describes a single shared text, so the exception is in the spec but not in CONTEXT.md (HP-014: an entry is updated "in the same change" when a term is sharpened).

### MINOR-7: The unconfirmed time zone defaults carry no constitution marker, and ADR 0016 states them as its decision
- **Where**: spec.md "Critic revision 2026-11-17" (59-74) and "Open:" (87-88; 1086); checklists/requirements.md iteration 5 ("they carry no `[NEEDS CLARIFICATION]` marker") together with the ticked "No [NEEDS CLARIFICATION] markers remain" and "Requirements are testable and unambiguous"; docs/adr/0016-time-zones.md Decision bullets 3-4 (current-date rule, named-time-zone set), with no mention that they await confirmation.
- **Against**: Constitution, Decision Discipline: a spec touching SPEC §11.1 or ARCHITECTURE §9 (time zone rules) "MUST mark it `[NEEDS CLARIFICATION: …]` … instead of assuming an answer".
- **Continues**: MINOR-7 (pass 2)
- **Reasoning**: The spec, the checklist and the roadmap now agree that these defaults are open. The deviation from the constitution's mandated form is acknowledged in the checklist but not justified. The marker is what the checklist item and `/speckit-clarify` look for, so the item passes while time-zone decisions are open. ADR 0016 is Proposed, but its Decision section gives the current-date rule and the named-time-zone set without the caveat the roadmap row carries. A reader of the ADR alone would take them as the owner's decision.

## Summary
0 MAJOR, 7 MINOR. Previous notes: 6 Resolved, 3 Partly, 0 Unresolved.

Both pass-2 majors are fixed:
- The demo goals no longer touch the custom habits that locked AC-066 deletes.
- FR-026 now measures every change on the goal's own timeline. Both pass-2 counterexamples hold, and US10-AS4 to AS10 recompute correctly under the rewritten rule.

The remaining notes:
- the listing's use of the account's "today" against the goal's current date;
- unit and host tests that assert the old demo-loading order;
- one false summary sentence in FR-026;
- the Profile list's link rule, which leaves out "UTC";
- US9-AS3's underspecified starting catalog;
- the suggestion and goal texts that contradict each other for Reduce yes/no fixed days;
- the missing constitution marker on the unconfirmed defaults.

None of these blocks `/speckit-ac-translate` by itself. The spec gates that step on the owner confirming the "Critic revision 2026-11-17" defaults, so that confirmation is still the next step.
