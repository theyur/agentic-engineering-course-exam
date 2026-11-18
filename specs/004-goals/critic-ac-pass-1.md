# Critic pass 1: ac for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/acceptance.md (AC-001 to AC-091 and the Coverage table); golden samples: none
(tests/HT.AcceptanceTests/Samples/F004 does not exist and no item names one)

Checked and found consistent, so not listed below: the Coverage table matches every item header's FR/US
references exactly (scripted cross-check, 91 items); every FR-001 to FR-035 and FR-040 and every US1 to US11
scenario has a row; every time-zone instant in AC-036 to AC-045 and AC-071 to AC-089 was recomputed with .NET
`TimeZoneInfo` (IANA ids) and matches; the date arithmetic (2026-11-16 is a Monday, 365 days to 2027-11-18,
53 Wednesdays in AC-043 row 03) and the OrdinalIgnoreCase order of AC-022 (Äpfel after zebra) are correct.

## Notes

### MAJOR-1: AC-052's example rows share all their data
- **Where**: acceptance.md AC-052 (lines 888-901): `ac052.admin@example.com`, `ac052.user@example.com`,
  `A052 Reading`, `A052 Yoga`, `A052 Water`, with an Examples table of three rows (n = 01, 02, 03).
- **Against**: acceptance.md conventions, "Examples" (lines 25-27): "Every email and catalog-habit name of such
  an item contains `<n>` ... so rows never share data"; spec "What acceptance tests can reach", Test data (one
  database per run, scenarios use their own accounts and habit names).
- **Reasoning**: AC-052 is the only Examples item whose emails and habit names have no `<n>` (scripted check
  over all items with an Examples table). Each row runs as its own scenario in the same database. So row 02
  repeats administrator creation and registration for emails that already exist. Under the "registered user"
  convention, "registration succeeded" is then false and the Given fails. If setup got past that, the habits
  would exist twice and the user would have 4 goals on "A052 Reading", not 2. Row 02 or row 03 fails against a
  correct implementation.

### MAJOR-2: AC-064 and AC-065 assert list fields that a started goal's list entry must not give
- **Where**: acceptance.md AC-064 (lines 1042-1044): "`demo1@example.com`'s goal list is ... (custom, Reduce,
  yes/no, fixed days on all seven days, Commitment-based, start date 2026-11-18)", and the same for demo1's
  Reading goal, demo2's "Morning yoga" and demo3's three goals ("each Period-based with the start date
  2026-11-18"). AC-065 (line 1053): "with the same names and the same start dates".
- **Against**: spec FR-014 (a list entry gives the start date only "when the goal has no current date"); spec
  FR-015 (a list entry has the measurement and target text, not the amount, unit or days); acceptance.md
  conventions line 50 ("In list results, ... 'start date D' ... are the FR-014 information"); AC-021 line 528,
  which asserts "no start date" for a goal that started on 2026-11-18.
- **Reasoning**: demo goals that start "on the day of the run" (FR-040) run at the default
  `2026-11-18T10:00:00Z` in `UTC`, so they have a current date. Under FR-014 and the item set's own convention,
  their list entries give no start date. AC-064 requires one, which contradicts AC-021. The list also carries
  no amount (15 minutes, 2 `cups`, 1 `sweet`) and no days, which AC-064 states as list content. A test written
  as stated fails against a correct implementation. Otherwise the test writer has to guess that "goal list"
  means reading each goal, which breaks the literal-value rule. AC-065's "same start dates" has the same
  problem for every demo goal except "Meditation".

### MINOR-1: Step times run backwards in many items
- **Where**: acceptance.md conventions lines 7-9 ("Every step runs at the time the item gives for it, otherwise
  at `2026-11-18T10:00:00Z`. Registration and sign-in run at the earliest time the item gives."), applied to
  AC-023, AC-044, AC-071 to AC-078 and AC-081 to AC-083.
- **Against**: spec FR-012 and constitution II (time is a supplied value, so the scenario's timeline is part
  of the input); spec FR-030 (account creation time is recorded).
- **Reasoning**: "who created the custom habit `Yoga`" has no time of its own, so it runs at
  `2026-11-18T10:00:00Z`. In AC-023 and AC-071 the goal on that habit is then created at
  `2026-11-16T08:00:00Z`, two days before the habit. In AC-082 the goal comes at `06:00:00Z` and the habit at
  `10:00:00Z`. In AC-076 registration runs at `23:30:00Z`, so the habit (10:00Z) is created before its account
  exists. The Given clause binds the habit to the registration sentence, so a test writer could also run it at
  the registration time. The two readings give different timelines, and the convention doesn't say which is
  meant.

### MINOR-2: "Creation order" ties under a shared supplied time
- **Where**: acceptance.md AC-022 (lines 533-538; "Alpha" before "alpha"), AC-017 (two goals with the same
  derived name).
- **Against**: spec FR-015 ("in name order ... and then by creation order"); acceptance.md conventions line 8
  (all steps without a time run at the same instant).
- **Reasoning**: all five goals of AC-022 are created at the same supplied instant. "Alpha" and "alpha" are
  equal under the FR-004 comparison, so only creation order separates them. If an implementation stores the
  creation time from the supplied time (constitution II forbids reading the clock), it has no way to order
  them. The result then depends on database order, and the test becomes flaky. Neither the spec nor the items
  say that creation order is independent of the supplied time. That is an implicit implementation constraint.

### MINOR-3: Two edge cases have no item, and the Coverage table lists no success criteria or edge cases
- **Where**: acceptance.md Coverage table (lines 74-190): it has rows only for FR and US scenarios.
- **Against**: spec Edge Cases (lines 620-623): a suggestion the administrator changes or removes after
  starting, and the goal never following later changes to the suggestion. Spec Edge Cases (lines 630-631): a
  confirmed unpublish after the count changed ("the count is informative, not a lock"). Spec SC-001 to SC-007.
- **Reasoning**: no item edits or removes a suggested starting goal between starting and creating, or after
  creating, and then checks the goal. AC-058 confirms the unpublish without first getting a "confirmation
  needed" count and then creating another goal. Both edge cases are reachable through the use cases. None of
  this is listed as "Not testable" either. The missing SC rows (for example SC-004 → AC-020, SC-005 → AC-057)
  make the success-criteria coverage impossible to check from the table.

### MINOR-4: The order of the name check at registration is not discriminated
- **Where**: acceptance.md AC-085 (lines 1311-1318).
- **Against**: spec FR-028 ("longer than 60 characters gives ... (at registration, after slice 1's checks)").
- **Reasoning**: every registration with a 61-character name in the items uses a valid email and the password
  `walking42`. An implementation that checks the name before the password rule passes all items, even though
  FR-028 fixes the order.

### MINOR-5: No anonymous negative test for reading the profile
- **Where**: acceptance.md AC-091 (lines 1381-1391) covers the Home heading and the name change. AC-079 covers
  the time zone. No item reads the profile as the anonymous visitor.
- **Against**: constitution III ("Each such use case MUST have negative tests: ... an anonymous visitor");
  spec FR-023 (1); spec FR-030 (the profile now gives the private name and "member since").
- **Reasoning**: the profile read returns private account data added by this slice (FR-028, FR-030), but its
  anonymous refusal is never exercised. The other private account use cases each have one.

### MINOR-6: AC-054's order does not tell ignore-case comparison apart from case-sensitive comparison
- **Where**: acceptance.md AC-054 (lines 919-925): "A054 Reading, 15 min, 5 days a week", "Evening pages",
  "morning pages".
- **Against**: spec FR-031 (details give the goals "in name order", the slice 2 FR-004 comparison: ordinal,
  ignoring case).
- **Reasoning**: case-sensitive ordinal comparison gives the same order ('A' 0x41 < 'E' 0x45 < 'm' 0x6D), and
  so does a culture comparison. A details query that sorts case-sensitively passes. AC-022 discriminates this
  for the list, but nothing does it for the details.

### MINOR-7: AC-020 narrows "every suggested starting goal of the seed catalog" without saying so
- **Where**: acceptance.md AC-020 (lines 512-519): "for every catalog habit that browsing the catalog without
  an acting user returns".
- **Against**: spec FR-007 last sentence and SC-004 ("Every suggested starting goal of the seed catalog");
  spec "Demo loading" (the seed catalog includes the Unpublished "Coffee").
- **Reasoning**: browsing returns only Published habits, so the suggestions of the seed's Unpublished "Coffee"
  are left out. That is the only reading compatible with FR-008. Still, the item and its Coverage row don't
  note that the requirement is covered only for Published seed habits, so the gap between SC-004's wording and
  what is tested isn't recorded.

## Summary
2 MAJOR, 7 MINOR. The item set is thorough. FR and scenario coverage is complete and matches the Coverage
table, and the time-zone instants, clock-change durations, boundaries and order discriminators were verified
correct. Two items would fail against a correct implementation: AC-052 because its example rows collide in
the shared database, and AC-064/AC-065 because they assert list fields that FR-014/FR-015 and the item set's
own convention rule out for started goals. Those two must be resolved before the items are locked and
translated. The MINOR notes are timeline and ordering ambiguities and small coverage gaps that don't block
translation by themselves.
