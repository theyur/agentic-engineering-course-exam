# Critic pass 2: spec for 002-habit-catalog

**Verdict**: APPROVE
**Date**: 2026-11-12
**Reviewed**: `specs/002-habit-catalog/spec.md`, `specs/002-habit-catalog/checklists/requirements.md`

Checked against: `.specify/memory/constitution.md`, `SPEC.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `CONTEXT.md` (with
its diff), `docs/roadmap.md` (with its diff), `docs/features/habit-catalog/idea-brief.md`, ADR 0003, 0005, 0007,
0008, 0011, 0012, `design/` (pages.md, README.md, brand-book.md, the named page and component files,
`design/icons/`), `specs/001-accounts-and-access/spec.md`, its `acceptance.md` and `contracts/`,
`.workflow/config.json`, and the code and tests under `src/` and `tests/` except `tests/HT.AcceptanceTests`,
which was not opened. Nothing was built or run.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 Demo loading writes custom habits with no acting user, behaviour undefined | Resolved | FR-031 (lines 600–614) now names the six demo custom habits with their fields, gives three literal results per account, runs only with a valid demo password (US6-AS4), covers a demo email held by an administrator (US6-AS8, "not a registered user"), replaces "does not have by name" with "owns no custom habit", and routes every read and write through the owner-scoped use cases of FR-020. HP-017 defines the log lines without custom-habit names. What is left is narrower and listed as MINOR-2. |
| MAJOR-2 The set of valid icon names is not defined | Resolved | FR-003 lists 24 names. Verified: the first 10 exist in `design/icons/`, the other 14 do not and are exactly the 14 that "Design changes before build" (lines 695–698) adds; 10 + 14 = 24. Every icon used in a scenario or in FR-031 (`person-standing`, `droplet`, `smartphone`, `pen-line`, `candy`, `sprout`) is in the list, and `menu` (US3-AS10) is not. `design/components/Icons`, `design/icons/`, brand-book Iconography and the `PageCustomHabit` picker are in the design-change list. A leftover wording point is in MINOR-4. |
| MINOR-1 Catalog seeding as a new startup task departs from ADR 0007 | Resolved; the rationale is sound | The separate task is gone. FR-030 puts the seed catalog into demo loading, which is what ADR 0007 prescribes ("Each later slice extends demo loading with its own demo data"; opt-in through the demo password; "existing data is reported and left unchanged"). SPEC §10 and constitution VI treat the catalog content as demonstration data, so no new or superseding ADR is needed. "Decisions this slice cites" (lines 82–85) now matches ADR 0007, HP-017 keeps slice 1's "right after administrator creation" and states the fault handling, and the consequence (no demo password, no seed catalog) is recorded in Assumptions (lines 798–801) and in SC-002's condition. |
| MINOR-2 Unpublishing an Unpublished habit has no defined result | Resolved | FR-011 (lines 533–535): success, stays Unpublished; US3-AS8. |
| MINOR-3 The test-data rule is broken by US1-AS8 and the demo scenarios | Partly | The old US1-AS8 is split into AS8 (inclusion and order only) and AS9, and three exceptions are listed (lines 108–117). The exceptions contradict each other and do not match how slice 1's locked criteria run. Continued as MINOR-1. |
| MINOR-4 Parts of the Admin and Home previews dropped or reworded | Resolved | FR-012 takes a search text and returns the total; US3-AS19, US3-AS20 and HP-009 keep "Search catalog" and "Showing N of M"; the `PageAdmin` design entry says they stay. US5-AS2 and HP-012 use "See all N →", as `PageHomeVisitor.html` ("See all 24 →"). |
| MINOR-5 The order of role, existence and field checks is not stated | Resolved | FR-016 gives one order; FR-006 puts the length check before the mark check; US3-AS15, US3-AS18 and US4-AS11 show it. FR-005 and FR-016 agree that name uniqueness is last. |
| MINOR-6 Administrator and visitor variants of the custom-habit pages | Resolved | HP-008 (administrator Profile keeps Account only), HP-013 and US5-AS3 (administrator Home keeps its slice-1 state), US4-AS15 and HP-007 (addresses for a visitor, an administrator and a habit that is not the user's). One leftover inconsistency is in MINOR-8. |
| MINOR-7 Wording of a suggested starting goal defined by two places that point at each other | Resolved | HP-019 is removed (no reference to it remains) and FR-017 defines text and kind. All ten examples of US2-AS1, US2-AS3 and US3-AS17 were derived from the FR-017 rules and match. Every card text in the previews ("15 min, 5 days a week", "3 times a week", "at most 2 h a day", "30 min daily", "once a week", "at most 1 sweet a day") can be produced by the rules. One rule is still ambiguous: MINOR-3. |
| MINOR-8 Scenario gaps for FR-006, FR-007, FR-012, FR-023 | Resolved | US3-AS16 now has the missing Build or Reduce and the missing amount; US2-AS1 and US3-AS14 state Build or Reduce; FR-007 says the position is a separate value counted from 1; US3-AS11 includes list and read; US4-AS9 includes create; "it is marked private" is gone from US4-AS1. |
| MINOR-9 "Suggested start" beside "suggested starting goal"; CONTEXT.md | Resolved in the spec and in CONTEXT.md | The spec uses "suggested starting goal" only (the one remaining "Suggested start" is the quoted old label, line 699). CONTEXT.md now sharpens Suggested starting goal and Custom habit and adds Seed catalog; "catalog seeding" is gone. The design-change list that carries the new label is incomplete: MINOR-6. |
| MINOR-10 The seed requirements do not ensure the demo shows the slice's states | Resolved | FR-030: all six categories, at least 4 Reduce, all three measurements, both schedule kinds, one Published habit with no suggested starting goal and one with three or more, one Draft, one Unpublished. |
| MINOR-11 Slice 1 requirements and tests this slice overrides are not acknowledged | Partly | The new section (lines 672–688) names slice 1's HP-010, HP-011, HP-013, HP-014, FR-016 and HP-021 and five test files; each description was checked against the test code and is accurate. One further test that asserts the slice-1 demo log is not listed: MINOR-5. |
| MINOR-12 The checklist is out of date | Resolved | The Notes record `/speckit-clarify`, critic pass 1 and the FR-015 exception (lines 45–55); the counts "2 MAJOR and 13 MINOR" are right. |
| MINOR-13 "Name order, ignoring case" does not say which comparison | Resolved | FR-004: ordinal, ignoring case, never the current culture; FR-009 and HP-018 refer to it. |

No older pass exists, so nothing could regress.

## Notes

### MINOR-1: The test-data exceptions contradict each other and ignore slice 1's locked scenarios in the same run
- **Where**: spec.md "What acceptance tests can reach", Test data (lines 105–117); US1-AS9 (lines 159–160); US6-AS1,
  US6-AS4, US6-AS8 (lines 405–408, 413–414, 426–428).
- **Against**: ADR 0003 (one shared database, one run at a time); `.workflow/config.json` (`acTestCommand` runs the
  whole AC project, so both features' scenarios are in one run); `specs/001-accounts-and-access/acceptance.md`
  lines 21–25 and AC-055 to AC-060 (locked).
- **Continues**: pass 1 MINOR-3.
- **Reasoning**:
  - The first exception says US1-AS9 runs before US6-AS1, because the seed catalog publishes habits in Home. The
    second says US6-AS1 and US6-AS4 "run before every scenario that creates" a catalog habit. US1-AS9 creates one
    (the draft "Ac27 Tidying"). The two rules cannot both hold in one run.
  - The third exception says US6-AS8 "cannot share a run" with slice 1's demo scenarios or with the other US6
    scenarios. The section gives acceptance criteria only two tools, "order or combine into one sequence", and
    neither gives a scenario a run of its own. US6-AS8 is an FR-031 behaviour on the AC surface.
  - The description of slice 1 does not match its locked criteria. AC-055 to AC-060 each start with "the AC
    database is reset" and then run demo loading with a valid demo password. After this slice, each of them also
    loads the seed catalog (with Published habits in Home) and the demo custom habits, at a point in the run
    that this feature's criteria do not control. "No other scenario of this spec publishes a habit in Home" is
    therefore not enough for US1-AS9, and "run before every scenario that creates one" is not enough for
    US6-AS1 and US6-AS4. The section does not mention these resets at all, although they change the state every
    exception relies on.
  - Several catalog scenarios give no category (for example US1-AS2, US1-AS5, US3-AS2), so the claim that none of
    them publishes a habit in Home depends on values the spec does not fix.

### MINOR-2: Demo loading "acting as" a demo account conflicts with the glossary and ADR 0005, and reaches accounts it did not create
- **Where**: spec.md FR-031 (lines 609–614), Clarifications, critic revision (lines 61–62), Assumptions (lines
  802–803).
- **Against**: CONTEXT.md "Acting user" ("it is established only by signing in") and "Demo account" ("created by
  demo loading"); ADR 0005 ("Operator capabilities (administrator creation, demo loading) take no acting user");
  `src/HT.Domain/Accounts/ActingUser.cs` lines 3–6 ("Built by the host from the session and by tests from a
  successful sign-in result"); constitution I (a change that sharpens a term updates CONTEXT.md in the same
  change) and Decision Discipline (a decision that spans features becomes an ADR); slice 1 spec Edge Cases
  (lines 440–441) and FR-016; slice 1 `acceptance.md` AC-060.
- **Continues**: new (left over from pass 1 MAJOR-1).
- **Reasoning**:
  - FR-031 makes an operator task the acting user of an account without a sign-in. It cannot sign in: slice 1
    lets the demo password differ from the account's password (US7-AS2, AC-060). The glossary says an acting
    user is established only by signing in, and this change edits CONTEXT.md without touching that entry. ADR
    0005 says demo loading takes no acting user and does not describe an operator capability that builds one.
  - The pattern will apply to every later slice's private demo data (goals, records, notes), so it spans
    features. It is recorded only as an author decision in the spec; ADR 0007 and ADR 0005 are unchanged.
  - FR-031 applies to "the account" at each demo email. Slice 1 states that such an account can belong to
    someone who registered it before demo loading ran and "keeps its owner's password"; AC-060 arranges exactly
    that (`Demo2@Example.com` registered with `owner2026`). `DemoDataService.LoadAccountAsync` reports "already
    exists" for it. Under FR-031 this account, which demo loading did not create, gets two private custom
    habits written on its owner's behalf. The spec does not address the case; US6-AS5 only covers "the demo
    accounts that demo loading created", while US6-AS8 shows that FR-031 is meant to go by email.

### MINOR-3: FR-017's kind rule is ambiguous for mixed day sets, and FR-007 leaves a yes/no amount undefined
- **Where**: spec.md FR-017, Kind (lines 568–571); FR-007 (line 494); US2-AS1, US2-AS3.
- **Against**: FR-008 and FR-010 (text and kind are part of use-case results, so acceptance tests check them
  literally); ADR 0005 (results carry literal text).
- **Continues**: new (left over from pass 1 MINOR-7).
- **Reasoning**: The rule reads "a run of three or more consecutive days Monday to Sunday as 'Mon–Fri', otherwise
  the days in week order, comma-separated". For a selection that contains a run and other days (Monday, Tuesday,
  Wednesday and Saturday), it can be read as "Mon–Wed, Sat" or as "Mon, Tue, Wed, Sat". The examples cover only a
  whole run (Mon–Fri), no run (Mon, Wed, Fri), one day (Sat) and all seven days. FR-007 states that "Yes/no
  carries no amount and no unit" but lists no result for a yes/no suggested starting goal that is given one,
  where every other field has a refusal.

### MINOR-4: FR-030 has no rule for a seed that is loaded only in part, and "its own icon" can be read two ways
- **Where**: spec.md FR-030 (lines 592–599), HP-017 (lines 664–668), SC-002, SC-007, FR-031 (lines 609–612).
- **Against**: SC-002 (at least 20 Published habits in all six categories); FR-003 ("Several habits may use the
  same icon"); `src/HT.Ui/Operator/StartupTasks.cs` lines 85–95 (a fault in a step is logged and the start goes
  on).
- **Continues**: new.
- **Reasoning**:
  - The seed is loaded only "when no catalog habit exists", and a fault during demo loading does not stop the
    start. Nothing requires the seed to be created as a whole. If a start fails after some seed habits exist,
    every later start reports "catalog not empty" and adds nothing, so that database never reaches SC-002. The
    same holds, with less effect, for the two demo custom habits of an account ("custom habits left unchanged"
    once one exists).
  - "Each with a neutral description and its own icon" can mean a different icon for every seed habit, or simply
    that each has one. FR-003 allows several habits to share an icon, and the seed needs at least 22 habits out
    of 24 icons. US6-AS1 does not check either reading.

### MINOR-5: A second test of the slice-1 demo log is not listed
- **Where**: spec.md "Slice 1 requirements and tests this slice changes" (lines 686–688), HP-017 (lines 664–668).
- **Against**: `tests/HT.UiTests/Host/StartupLogTests.cs` lines 22–36; slice 1 `contracts/host.md` line 84 (`Demo
  account {Email}: {Result}`).
- **Continues**: pass 1 MINOR-11.
- **Reasoning**: The section lists `StartupTasksDemoTests.cs` only. `StartupLogTests` takes every line of the
  started host that begins with "Demo account " after the administrator line, requires exactly three, and reads
  the email from each. HP-017 adds "one per demo account for the custom-habit result, naming the demo email"
  without giving the line's text. A line in the existing form for a demo account fails this test, so the test
  either changes or limits the wording, and the spec acknowledges neither.

### MINOR-6: The design-change list omits places that prescribe "Suggested start"
- **Where**: spec.md "Design changes before build" (lines 695–722, in particular 698–700 and 718–719), HP-001
  (lines 618–622), HP-014 (lines 657–658).
- **Against**: `design/brand-book.md` line 13 (Content fundamentals: `"Suggested start: 15 min, 5 days a week"`);
  `design/pages/PageHomeVisitor.html` lines 44–62; `design/pages/PageCatalogVisitor.md`,
  `PageCatalogSignedIn.md`, `PageNewGoal.md` and `design/README.md` ("the suggested start"); constitution I (no
  synonyms) and IV (`design/` changes first; text follows the brand book).
- **Continues**: pass 1 MINOR-9.
- **Reasoning**: The spec changes the card label to "Suggested starting goal: …" for constitution I and lists
  `HabitCard` and the two catalog pages for it. `brand-book.md` is listed for Iconography only, yet its content
  rule gives the old label as the wording to use, and HP-014 requires all text to follow the brand book. After
  the listed changes HP-001 and HP-014 point to different labels. The visitor Home preview, which this slice
  builds, shows the old label on its four cards and its entry names other changes only. The page notes above
  keep the synonym in prose.

### MINOR-7: The author decisions are both "awaiting confirmation" and "not open", and no step is named for the confirmation
- **Where**: spec.md "Decisions due before acceptance criteria" (lines 778–780); Clarifications, critic revision
  (lines 56–57); checklists/requirements.md lines 48–52.
- **Against**: constitution, Decision Discipline (answers come from the owner and are recorded with their date);
  `docs/roadmap.md`, "Decisions each slice must settle" (settled before `/speckit-ac-translate`; acceptance
  criteria are drafted from the spec; an answer after the lock shows as spec drift).
- **Continues**: new.
- **Reasoning**: Line 778 says the author decisions "await the owner's confirmation in review"; line 780 says "No
  decision of this slice is open". Several of them fix literal results that acceptance criteria will lock: the
  demo loading results and the six demo custom-habit names (FR-031), the FR-017 texts, success for unpublishing
  an Unpublished habit, the 24 icons, and the new card label. "In review" names no step. If it means the PR, the
  confirmation comes after the criteria are locked. The checklist records the revision without saying that the
  confirmation is pending.

### MINOR-8: Two scenario details cannot be observed or disagree with each other
- **Where**: spec.md US6-AS8 (lines 426–428); Edge Cases (lines 451–452); US4-AS15 (lines 358–361); HP-007 (lines
  636–638).
- **Against**: FR-021, FR-025 and "What acceptance tests can reach" (FR scenarios are verified through use cases);
  FR-022.
- **Continues**: new (the second point is left over from pass 1 MINOR-6).
- **Reasoning**:
  - US6-AS8 ends with "and it gets no custom habit" for an administrator account. Every custom-habit use case
    refuses an administrator (FR-025) and none reads another account's custom habits (FR-021), so no use case
    can show this; only the result text "not a registered user" can be checked.
  - The Edge Cases say a user who "opens" a custom habit deleted in another browser "gets 'This custom habit
    does not exist.'". US4-AS15 says the address of a custom habit that no longer exists shows the Not found
    page. HP-007 puts refusals "under their field", and this refusal belongs to no field, so what the open edit
    form shows when Save meets a habit deleted elsewhere is not stated.

## Summary

0 MAJOR, 8 MINOR.

The revision settles both MAJOR notes of pass 1 and eleven of its thirteen MINOR notes. The decision to carry
the seed catalog in demo loading instead of writing a new ADR is sound: it is what ADR 0007 prescribes, and its
consequences are recorded. The FR-017 rules reproduce every example in the spec and every card text in the
previews, the icon list matches `design/icons/`, and the listed slice-1 tests match the code. The spec is ready
for `/speckit-ac-translate` provided the drafting does not take the test-data section literally: its ordering
rules are circular, and slice 1's locked demo items reset the shared database and will load the seed catalog in
the same run (MINOR-1). The other notes are a glossary and ADR mismatch for the operator acting as a demo
account, one ambiguous formatter rule, a missing all-or-nothing rule for the seed, one unlisted slice-1 test,
gaps in the design-change list, and the unclear status of the author decisions.
