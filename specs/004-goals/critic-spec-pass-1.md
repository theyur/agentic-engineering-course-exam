# Critic pass 1: spec for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/spec.md, specs/004-goals/checklists/requirements.md

## Notes

### MAJOR-1: The spec is not self-contained for the rules its acceptance criteria depend on
- **Where**: spec.md "Slice 1 and slice 2 behaviour these tests use" (lines 93-112), especially the bullet "Suggested starting goal fields and text (slice 2 FR-007, FR-017): as cited above"; FR-006 (lines 545-553), FR-010 (572-574), FR-015 (604-605); US1-AS6, US1-AS7, US1-AS9, US2-AS1, US5-AS2.
- **Against**: `.workflow/config.json` `specInputs` = `SPEC.md` and ADR 0003 only. docs/roadmap.md, "Decisions each slice must settle": acceptance criteria are drafted only from the spec, the constitution and the `specInputs`. CLAUDE.md, "Acceptance tests": "Every domain rule the acceptance criteria depend on belongs in the feature's `spec.md` or in the constitution". The section's own claim (line 95): "so that this spec alone gives the rules and texts its acceptance tests depend on". Slice 2 restated slice 1 the same way (specs/002-habit-catalog/spec.md line 114).
- **Reasoning**: The section says the FR-007/FR-017 rules are "as cited above", but the cited bullet (lines 73-77) only names the fields. Several things are missing from the spec: the FR-007 refusal messages and their order (for example "Choose how it is measured.", "Enter a whole number from 1 to 999.", "Enter a unit, for example pages.", "Choose per day or per week."), the amount ranges, and the FR-017 text-building rules ("a day" / "once a day" / "daily", joined by a space or by ", ", the "at most " prefix). Scenarios depend on all of them: US1-AS7 asks for "one scenario per message", in FR-006 order, and FR-006 says that order is slice 2 FR-007's. The derived names of FR-010, US1-AS9 and US5-AS2, and the target text of US1-AS6, need the FR-017 rules. Also missing: the slice 2 FR-004 name comparison (ordinal, case-insensitive) used by FR-015/US2-AS1, and the slice 2 owner answers that FR-006 cites ("owner answer 2026-11-14"). Those answers include the UTF-16 counting and the empty yes/no unit rule. The AC translator sees none of these sources, so it cannot write those scenarios from the spec. That blocks `/speckit-ac-translate`.

### MAJOR-2: Duplicate requirement identifier FR-030
- **Where**: spec.md lines 668-671 (FR-030 "member since") and 675-679 (FR-030 "Only habits I track"); Clarifications line 55 ("FR-028 to FR-030"); also FR-019 is skipped between FR-018 and FR-020.
- **Against**: The FR-NNN convention used for AC traceability. The locked features of earlier slices tag every scenario with `@FR-NNN` (tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature, for example `@AC-001 @FR-008 @US1-AS1`).
- **Reasoning**: Two different requirements share the number FR-030. One covers the account creation time and "member since", the other the catalog goal counts and the "Only habits I track" filter. Any scenario, task or cross-reference tagged FR-030 is ambiguous. US11-AS6 and US6-AS1/AS2 would both trace to "FR-030", and the ambiguity would be locked into the `.feature` files and lock files, which only an unlock can change. Checklist item "All functional requirements have clear acceptance criteria" is checked even so.

### MAJOR-3: "Its time zone" and "the date from which its settings apply" are undefined once a goal has two versions
- **Where**: FR-016 (lines 608-612), FR-005 (540-544), FR-026 (644-650), US2-AS4 (205-207), US10-AS6 (440-441).
- **Against**: FR-026 adds a second settings version that takes effect on a later date (the day after C, the Monday after C's week, or the start date). FR-005 says the first version applies from the start date.
- **Reasoning**: After a time zone change on date C, a goal has a version in effect today (old zone) and a pending version (new zone) that takes effect later. FR-016 requires reading a goal to return "its time zone" and "the date from which its settings apply", but it does not say which version they come from. US10-AS6 ("all its settings except the time zone are unchanged") implies that reading on day C already returns the new zone, which is the pending version, not the one in effect. FR-005 implies the "applies from" date is the start date. These cannot both hold for one read, and the spec gives no rule for what a read returns on C, between C and the switch, or after the switch. US2-AS4 and US10-AS6 cannot be turned into deterministic acceptance criteria until this is settled.

### MAJOR-4: Demo goals are not added to the hosted-demo smoke check, which slice 3 requires
- **Where**: spec.md FR-040 (698-712), HP-013 (763-764), "Slice 1 and slice 2 requirements and tests this slice changes" (778-800).
- **Against**: specs/003-demo-hosting/spec.md SC-005: "the smoke check ... finds every shipped slice's demo data". specs/003-demo-hosting/contracts/smoke-check.md "Coverage rule": every data parameter of `IDemoDataStore` has a `SmokeCheckCoverage` entry. tests/HT.UiTests/SmokeChecks/CoverageTests.cs fails when the port gains a parameter without one. The `IDemoDataStore` doc comment (src/HT.Application/Operator/IDemoDataStore.cs) says "Its only read is whether the catalog is empty".
- **Reasoning**: FR-040 adds demo goals, which have to be written through the demo data port (ADR 0013), so the port changes. The spec has no host requirement for extending `tools/HT.SmokeCheck` with a check for the demo goals. Its list of changed earlier requirements and tests leaves out slice 3, SC-005, the coverage rule and `CoverageTests`. Two consequences follow:
  - The existing coverage test fails, or, if the port is changed some other way, slice 3's SC-005 goes unmet.
  - The deployment's success check, the smoke check, would no longer verify every shipped slice's demo data.

  FR-040 also needs to know whether named catalog habits are Published, which is a new read. That changes the port's documented contract ("only read is whether the catalog is empty"), and the spec does not acknowledge it.

### MAJOR-5: Unacknowledged change to slice 2 "same result for every audience" and to use cases called by the locked F001/F002 acceptance tests
- **Where**: FR-030 (catalog, lines 675-679), FR-031 (680-682), FR-025 (639-643), FR-028 (657-662), FR-032 (683-687); the section "Slice 1 and slice 2 requirements and tests this slice changes" (778-800).
- **Against**: specs/002-habit-catalog/spec.md FR-008: "The same result is given to every audience." FR-010 says the same, for every audience including administrators. src/HT.Application/Habits/CatalogService.cs: "The use cases take no acting user: every audience gets the same result (ADR 0005, research R-07)". The locked tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature AC-001 browses "to every audience". The F001 feature exercises registration. CLAUDE.md: step definitions, `.feature` and lock files must never be edited by the main agent.
- **Reasoning**: The spec makes three changes to earlier behaviour:
  - Catalog browsing and details become audience-dependent: goal counts and "your goals" for registered users, and a filter refused for visitors and administrators. That reverses slice 2 FR-008/FR-010 and the design decision of research R-07.
  - Registration gains an optional time zone and an optional name.
  - Unpublishing gains a confirmation input.

  The change list names only UI and integration tests. It does not list slice 2 FR-008/FR-010 as amended, and it never mentions the locked F001/F002 acceptance criteria or their step definitions. Those call `CatalogService.BrowseAsync(filter)`, `GetDetailsAsync(id)`, `AccountService.RegisterAsync(email, password)` and `UnpublishAsync(actingUser, id)`. If any of these change shape, the existing AC tests break, and nobody may edit them without an unlock. The spec neither marks this as spec drift for the earlier slices nor states that their contracts must stay callable.

### MAJOR-6: CONTEXT.md is deferred past the change that introduces and settles the new terms
- **Where**: HP-014 (765-767): "`CONTEXT.md` MUST gain, in the change that settles the open decisions: ..."; Clarifications line 59 and "Decisions due before acceptance criteria" line 887: "No decision of this slice is open."
- **Against**: Constitution I: "A change that introduces a new domain term, or sharpens the meaning of an existing one, MUST add or update its `CONTEXT.md` entry ... in the same change." docs/roadmap.md "Every slice": "New domain terms go into `CONTEXT.md` in the same change". Precedent: `git log -- CONTEXT.md` shows the spec commits of slices 1, 2 and 3 updating it (054b16d, 3c479bb, ada3fcf).
- **Reasoning**: Commit 23822d2 adds the spec, records every decision as settled, and introduces terms such as evaluation calendar, settings version, account time zone, account name and target text. It also sharpens Attempt, Commitment and Evaluation period with boundaries and versions. CONTEXT.md is unchanged in that commit (still `updated_at: "2026-11-15"`, with no Personal goal, Attempt, Commitment, Evaluation period or time zone entries). The trigger HP-014 names, "the change that settles the open decisions", has therefore already passed without the update, which breaks constitution I. The AC translator and later slices then work from a glossary that lacks the terms these scenarios use.

### MINOR-1: The goal's Build or Reduce is called "Intent", against the CONTEXT.md boundary
- **Where**: HP-003 ("intent Tag", line 731), US2-AS6 ("the intent Tag", 211), HP-004 Settings card row "Intent" (737), "Design changes before build" (GoalListItem "intent Tag", 810).
- **Against**: CONTEXT.md, Intent: "NOT a personal goal's Build or Reduce, which the goal itself defines (SPEC §5.1)". Constitution I forbids synonyms for defined terms.
- **Reasoning**: The Tag and Settings row show the goal's own Build or Reduce, which can differ from the habit's intent (US5-AS5 says so explicitly). Calling it "intent" is exactly the confusion the glossary's NOT-boundary rules out. The design previews do the same, but the spec lists other wording conflicts with the design under "Design changes before build" and not this one.

### MINOR-2: Rules for repeated time zone changes are only partly stated
- **Where**: Edge Cases lines 488-490; FR-026 (644-650); docs/adr/0016-time-zones.md Decision ("A second change before that date replaces the first").
- **Reasoning**: The replacement rule appears only as an edge case, with a per-week example, and not in FR-026. Because each goal has its own effective date, a second change can come after one goal's pending version has already taken effect but before another goal's has, and FR-026 does not say what happens then. A goal whose start date is after C also ends up with two versions effective on the same date, its first version and the new one. The edge case "only one version starts on that date" contradicts that, but FR-005/FR-026 do not say which version wins. Two changes on the same day C are not covered either.

### MINOR-3: Large-offset time zone changes break the "a few hours" assumption and the "never moves" promise
- **Where**: FR-011 (581: "never moves a commitment or evaluation period that has already begun or ended"), FR-026, FR-027, FR-014 ("next commitment on or after today", 598-599); ADR 0016 ("a gap or an overlap of a few hours").
- **Reasoning**: C is "today in the old time zone", but "today" for the user is decided in the new zone (FR-012). Take a move from UTC-11 to UTC+14, made late on C. The first new-zone day, C+1, can already have begun, or even ended, at the moment of the change. So the change creates a commitment that is already in the past, and C+1 in the new zone ends before C in the old zone does. The gap or overlap is up to about 25 hours, not "a few". FR-014 can also return as the "next commitment" a commitment (today, in the user's new zone) whose old-zone instants have already ended. None of these cases is stated, so the acceptance criteria and unit tests (constitution II, both sides of each boundary) have no defined result for them.

### MINOR-4: "Named time zone" and the installation default are not defined for testing
- **Where**: FR-024 (636-638), FR-025 (639-643), FR-026; US10-AS1, AS2, AS7.
- **Reasoning**: The spec does not say which identifier set counts as "named":
  - IANA identifiers, Windows identifiers such as "FLE Standard Time", or both;
  - links and aliases such as "Europe/Kiev" next to "Europe/Kyiv", and whether a read returns the value as given or a canonical form (US10-AS1 expects "Europe/Kyiv" verbatim);
  - fixed-offset names such as "Etc/GMT+3".

  "Choose a time zone from the list." implies a list that the spec never defines. US10-AS2 expects "the installation's default", an operator setting. The spec does not say how that setting reaches the Application use cases that acceptance tests compose without `HT.Ui`. Without that, a test cannot know the expected value, except by assuming UTC.

### MINOR-5: The claim that this slice is the first to read the current time is wrong
- **Where**: "Decisions this slice cites", lines 90-91: "This slice is the first to read the current time".
- **Against**: src/HT.Application/Accounts/AccountService.cs line 61, `SignInAsync`: `var now = _timeProvider.GetUtcNow();` (slice 1 sign-in block); tests/HT.ArchitectureTests/ClockReadTests.cs.
- **Reasoning**: The current time already enters Application through `TimeProvider` (ADR 0005 "Time"). The statement is factually incorrect and could mislead the plan into adding a second time mechanism.

### MINOR-6: The checklist does not reflect the clarify session
- **Where**: checklists/requirements.md Notes, lines 38-53 ("D-1 to D-5 remain for `/speckit-clarify`"; "FR-004 (period-based), FR-013 (first week), FR-016 (attempt) and HP-010 state their default and name the open decision").
- **Against**: spec.md Clarifications (lines 44-57) and "Decisions due before acceptance criteria" (876-887): all of D-1 to D-5 are decided.
- **Reasoning**: The checklist has no iteration for the clarify answers. It still describes open decisions and defaults that the spec has since replaced. Every box is ticked even so, including "All functional requirements have clear acceptance criteria", despite the duplicate FR-030 (MAJOR-2).

### MINOR-7: Earlier-slice items that this slice affects but does not list
- **Where**: "Slice 1 and slice 2 requirements and tests this slice changes" (778-800); HP-017 (773-776); "Design changes before build" (802-826).
- **Against**: specs/001-accounts-and-access/spec.md HP-015 ("Pages of this slice MUST NOT show any date or time of day that depends on the user's time zone") and HP-014 (the avatar letter in "the navigation, the narrow-screen menu and the Profile header"); specs/002-habit-catalog/spec.md HP-009 ("The table has no 'Updated' column until time zones are decided (slice 4)"); design/components/TopNav.md "Avatar menu" and MobileNav.md (avatar = first letter of the email).
- **Reasoning**:
  - Slice 1 HP-015 is not listed as amended, although HP-010 and HP-017 now show time-zone-dependent dates.
  - Slice 2 HP-009 explicitly deferred an "Updated" column to this slice, and the spec neither adds it nor says it stays out.
  - HP-017 changes the avatar letter as part of the Profile sentence, but leaves open whether the TopNav and MobileNav avatars, which slice 1 HP-014 also covers, change too. The design-change list does not include the TopNav/MobileNav avatar rule.

### MINOR-8: Page requirements need data that no FR returns
- **Where**: HP-007 ("show 'Delete' only when no goal uses the habit", 750), HP-002 (723-728), HP-004 Settings "Evaluated" (737-738).
- **Against**: FR-034 (692-694) defines only the delete refusal. Slice 2's custom habit read (`CustomHabitService.GetAsync`) returns no usage information. design/pages/PageGoalDetail shows "Evaluated: Each commitment", while HP-002 says "Each scheduled day". design/pages/PageNewGoal's summary sentence is "Read at least 15 minutes on Mon, Tue, Wed, Thu and Fri".
- **Reasoning**:
  - The custom habit page cannot know whether to hide "Delete": no FR makes reading a custom habit report whether goals use it.
  - HP-002 requires "one sentence", and the preview uses a habit-specific verb ("Read") that none of the goal's fields provides. No rule produces it.
  - The Settings "Evaluated" value is unspecified and differs from the summary card's wording.

  All of these are left to the design change without a stated rule, although HP-002 requires "one shared formatter".

### MINOR-9: The meta line and listing information are ambiguous for some future goals
- **Where**: FR-014 (597-600), HP-003 (729-734), US4-AS8 (268-270).
- **Reasoning**: Take a per-week goal whose start date is after today and is not a Monday. Both "starts on that date" and "first week starts" apply, but HP-003 offers them as alternatives ("starts Mon 23 Nov" or "first week starts Mon 23 Nov") without saying which is shown. For a commitment-based goal whose future start date is not a scheduled day, FR-014 gives only the start date, although its first commitment comes later (Edge Cases, line 491). US4-AS8's trigger, "before that week's end", is a different condition from FR-014's "first week has not begun". They happen to coincide, but the wording leaves the condition open to reading.

### MINOR-10: Demo data leaves several new states undemonstrated
- **Where**: FR-040 (698-712), SC-006 (861-862).
- **Against**: Constitution VI: "Every user-facing feature MUST extend the representative demo data so a reviewer sees its states".
- **Reasoning**: No demo account has a name, so the greeting with a name (HP-010) and the Profile name header (HP-017) are not visible in the hosted demo. No demo goal uses an Unpublished catalog habit: FR-040 leaves such goals out, and the seed's Unpublished "Coffee" is never used. So the FR-033/HP-004 Notice cannot be seen. No demo goal has a name of its own. Whether the "first week starts" state appears depends on the weekday of the deployment. The spec does not say that these states are left out deliberately.

### MINOR-11: Scenarios that the AC surface cannot set up are not marked
- **Where**: US11-AS6 ("given an account created before this slice, no 'member since'", 476), FR-025/FR-030 (accounts created before this slice).
- **Against**: ADR 0003: AC runs start from a freshly migrated, empty schema and arrange data only through use cases. "What acceptance tests can reach" (114-126).
- **Reasoning**: No use case can create an account "before this slice" (that state comes only from a migration of older rows). The second half of US11-AS6 is therefore not testable through the AC surface, but it is not marked *(page)* or otherwise flagged as "not testable through the API".

### MINOR-12: Scenario numbering and structure are irregular
- **Where**: US4 acceptance scenarios numbered 1, 2, 3, 8, 4, 5, 6, 7 (lines 259-279); US7 numbered 1-6, 8, 7 (352-369); no `---` separator between US9 and US10 (414-416); FR-019 and FR-035 to FR-039 absent.
- **Reasoning**: The `@USn-ASm` tags come from these numbers. Out-of-order numbering makes review and judge reports harder to follow and invites mistakes during translation. This is a structural issue only and changes no rule.

## Summary

6 MAJOR, 12 MINOR. The spec is detailed, and most of its examples check out against the data:

- The seed catalog's Reading, Yoga and Sugar suggestions, and the fact that every seed Reduce count or duration suggestion has 1 time per period.
- The demo custom habits of slice 2 FR-031.
- The FR-017 text examples.
- The 2026 weekdays used in the scenarios.

It is not ready for `/speckit-ac-translate` for four reasons:

- It depends on slice 2 rules and texts it does not restate, and the AC translator cannot see them.
- It reuses FR-030 for two requirements.
- It leaves undefined what a goal read returns once a time zone change has added a second settings version.
- It skips CONTEXT.md, which constitution I requires in the same change.

Two cross-slice impacts are also unacknowledged: slice 3's smoke-check coverage rule for new demo data, and the change to slice 2's "same result for every audience" catalog contract, which the locked F001/F002 acceptance tests exercise. The minor notes concern edge cases of time zone changes, an undefined set of time zone identifiers, terminology ("intent" for a goal), a stale checklist, and page requirements that need data no FR provides.
