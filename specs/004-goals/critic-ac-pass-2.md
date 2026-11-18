# Critic pass 2: ac for 004-goals

**Verdict**: REVISE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/acceptance.md (AC-001 to AC-094, the Coverage table and the "Success criteria and
edge cases" map); golden samples: none (tests/HT.AcceptanceTests/Samples/F004 does not exist and no item names
one)

Checked again and found consistent, so not listed below: the Coverage table matches every item header's FR/US
references exactly (scripted cross-check, 94 items, no mismatch); every email of an item starts with its own
`acNNN.` and every catalog-habit name with its own `ANNN`, and every Examples item carries `<n>` or a row
number in each email and catalog-habit name (scripted); the instants of the new or rewritten items AC-071 to
AC-083 and AC-089 (Kyiv UTC+2 in November, New York UTC-5, Tokyo UTC+9, Pago Pago UTC-11, Kiritimati UTC+14),
the current-date and next-version logic of AC-073, AC-081 and AC-082 against FR-012 and FR-026, the
ignore-case order of AC-022 and AC-054, and the list orders of AC-064 and AC-067.

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (AC-052 rows share data) | Resolved | AC-052 (lines 905-916) now uses `ac052.admin.<n>`, `ac052.user.<n>` and `A052 <n> …`; the searches `A052 01`, `A052 02 R`, `A052 03 Y` select only the row's own habits, and row 02 still checks that the tracked count counts returned habits only (1, not 2). |
| MAJOR-2 (AC-064/AC-065 assert list fields a started goal must not give) | Resolved | AC-064 lines 1057-1059 give only FR-015 fields plus the FR-014 start date of the not-yet-started "Meditation" (2026-11-25, no current date at the run). Amounts, days, start dates and the time zone moved to "reading each demo goal" (lines 1060-1061). AC-065 line 1070 checks start dates by reading too. |
| MINOR-1 (step times run backwards) | Partly | The new Time convention (lines 7-12) moves setup to the earliest given time, which fixes AC-023, AC-044, AC-071 to AC-078 and AC-081 to AC-083 (each checked). Steps that are not setup still run at the default after steps given a later time: see MINOR-1 below. |
| MINOR-2 (creation-order ties) | Resolved | AC-022 (line 551) creates the five goals at distinct instants `10:00:01Z` to `10:00:05Z`, so "Alpha" before "alpha" follows from the supplied creation time. AC-017 (line 482) asserts no order among its two same-named goals. I agree with the author that no other item depends on creation order. |
| MINOR-3 (two edge cases have no item; no SC/edge-case map) | Resolved | AC-092 (suggestion changed after starting and removed after creating), AC-093 (confirmed unpublish after the count changed) and the map at lines 194-205, which names an item or a reason for SC-001 to SC-007 and for all 14 bullets of spec "Edge Cases" (lines 604-631). |
| MINOR-4 (order of the name check at registration not discriminated) | Resolved | AC-094 (lines 1431-1435) registers with the failing password `short` and a 61-character name. A name check placed before slice 1's checks returns the name message and fails the item. The re-registration step shows that no account was created. The author's position is sound: the spec restates the password rule ("Accounts", line 130) but not its refusal text, and "not the name message" is the strongest literal assertion the reference material supports. |
| MINOR-5 (no anonymous negative test for reading the profile) | Resolved | AC-091 row 03 (line 1409). Its tracing is imprecise: see MINOR-2 below. |
| MINOR-6 (AC-054 does not tell ignore-case from case-sensitive order) | Partly | "evening pages" vs "Morning pages" (line 940) now fails a case-sensitive ordinal sort ('M' 0x4D < 'e' 0x65). The pass-1 note also said a culture comparison gives the same order, and it still does: see MINOR-3 below. |
| MINOR-7 (AC-020 narrows the seed suggestions silently) | Resolved | The Coverage row FR-007 (line 84) records that SC-004 and FR-007's last sentence are checked for the seed's Published habits only, with the FR-008 reason. |

No older note has regressed. The coverage, the example isolation inside the feature and the time-zone
instants verified in pass 1 still hold.

## Notes

### MAJOR-1: F004's emails and catalog-habit names collide with F002's in the shared AC database
- **Where**: acceptance.md conventions "Names in the database" (lines 23-26: "Every email and every
  catalog-habit name belongs to one item only: emails start with `acNNN.` and catalog-habit names with
  `ANNN `"). This applies to every non-demo item, for example AC-002 line 225 (`ac002.admin@example.com`),
  AC-001 line 212 (`A001 <n> Reading`) and AC-034 line 696 (`ac034.admin@example.com`, `A034 Reading`).
- **Against**: docs/adr/0003-acceptance-tests.md "Decision" (lines 30-33: the schema is dropped and rebuilt
  "at the start of every AC test run", and AC tests "share one database"; "Each scenario arranges the data it
  needs"); spec "What acceptance tests can reach", Test data (lines 226-228: "all acceptance scenarios share
  one database in one run … Scenarios therefore use their own accounts, habit names and goal names"); spec
  FR-019 and "Slice 1 and slice 2 requirements and tests this slice changes" (line 1026: the locked F001 and
  F002 scenarios "must keep passing", so they run in the same AC test project). As data evidence,
  specs/002-habit-catalog/acceptance.md line 14 uses the same scheme (`acNNN.` emails, `ANNN ` habit names)
  with the same item numbers.
- **Continues**: new
- **Reasoning**: a scripted comparison of the two acceptance files, with `<n>` expanded per Examples row,
  finds 76 identical non-demo emails across 27 item pairs (F002/F004 AC-001, 002, 003, 004, 009, 015, 016,
  019, 021, 025, 028, 029, 031, 032, 033, 034, 035, 039, 046, 047, 048, 049, 050, 057, 061, 062, 073). It also
  finds three identical catalog-habit names: `A001 01 Reading`, `A001 02 Reading` (F002 line 146 / F004 line
  212) and `A034 Reading` (F002 line 567 / F004 line 696). A reset happens only when an item starts with
  one, and neither the reference material nor the items fix the order of scenarios across features. So
  whenever an F002 scenario runs and no reset follows before the F004 scenario with the same number, the F004
  Given fails against a correct implementation. Under the items' own conventions (lines 15-22), "the
  administrator `a`" requires that administrator creation "created the administrator", and "the registered
  user `x`" requires that "registration succeeded". For an email that already exists, neither holds. AC-034
  would also have a second `A034 Reading` from F002, so its search `A034` returns two habits. The tests then
  fail or not depending on run order. The convention's claim that each name "belongs to one item only" is
  true inside F004 but false for the database the tests actually share.

### MINOR-1: Steps without a time still run before steps given a later time
- **Where**: acceptance.md AC-089 rows 02 and 03 (lines 1378-1388): registration at `2026-11-30T22:30:00Z`,
  then "reads their profile" with no time, so at `2026-11-18T10:00:00Z`. AC-065 (lines 1066-1070): "at
  `2026-11-19T10:00:00Z` demo loading runs again", then the goal-list and read steps with no time, so at
  `2026-11-18T10:00:00Z`.
- **Against**: acceptance.md Time convention (lines 7-11). It moves only the setup steps, and steps "run in
  the order they are written". Constitution II and spec FR-012 make the supplied time part of the input, and
  spec FR-030 records the account's creation time.
- **Continues**: MINOR-1 (pass 1)
- **Reasoning**: under the convention as written, AC-089 reads the profile of an account 12 days before the
  account was created. AC-065 reads the goals one day before the run that it checks they survived. The
  asserted values don't depend on "now" ("member since" uses the creation time, the names and start dates are
  fixed), so a correct implementation still passes. But the timeline is not one a real user can produce, and
  a test writer may run the Then steps at the When time instead. The convention doesn't say which is meant.

### MINOR-2: AC-091 row 03 (anonymous profile read) is not traced to the requirement it tests
- **Where**: acceptance.md AC-091 header (line 1398: "FR-028; FR-029; US11-AS8") and row 03 (line 1409); the
  Coverage row FR-030 (line 107) lists only AC-089.
- **Against**: spec FR-030 (line 838: "Reading one's own profile gives the name, the email and 'member
  since'"); spec FR-023 (1) (line 777: every use case of this slice refuses the anonymous visitor first);
  spec US11-AS8 (lines 597-598), which names only the Home heading and changing a name; constitution III
  (negative tests per use case).
- **Continues**: MINOR-5 (pass 1)
- **Reasoning**: the profile read is the FR-030 use case. Its anonymous refusal comes from FR-023, not from
  FR-028 (the name) or FR-029 (the Home heading), and US11-AS8 doesn't include it. So the Coverage table
  doesn't show that FR-030's use case has its anonymous negative test, which is what pass 1 asked for. The
  test exists, but checking constitution III coverage from the table gives the wrong answer.

### MINOR-3: The details order of AC-054 does not rule out a culture-based (or SQL-collation) sort
- **Where**: acceptance.md AC-054 (lines 937-940): "A054 Reading, 15 min, 5 days a week", "evening pages",
  "Morning pages".
- **Against**: spec FR-031 (line 848, details give the goals "in name order"); spec "Names" (lines 149-150:
  "ordinally, ignoring case (culture-invariant), never by the current culture").
- **Continues**: MINOR-6 (pass 1)
- **Reasoning**: a current-culture or invariant-culture ignore-case comparison gives the same order (A < e <
  M), and so does a SQL Server case-insensitive collation used in an `ORDER BY`. For the goal list, AC-022
  catches this with "Äpfel" after "zebra". Nothing does it for the details query, which is a separate use
  case and plausibly sorts in the database. An implementation that breaks the "never by the current culture"
  rule for details passes every item.

### MINOR-4: AC-053 asserts a browse order that the spec doesn't restate
- **Where**: acceptance.md AC-053 row 01 (line 930): "`A053 01 Reading` and `A053 01 Sugar`, in this order".
- **Against**: spec "Slice 1 and slice 2 behaviour these tests use" (line 128: "Restated here … so that this
  spec alone gives the rules and texts its acceptance tests depend on"). Its "Catalog habits" and "Browsing
  filters" bullets (lines 133-144) give no order of browse results. FR-031 (lines 844-848) gives a name order
  only for the details' goals.
- **Continues**: new
- **Reasoning**: the asserted order is right under slice 2 FR-008 ("in name order", specs/002-habit-catalog/
  spec.md line 571). But the reference set of this feature doesn't contain that rule, so nothing in the
  reference material supports the order assertion. An order by category (FR-002 lists Health before Mind) would
  put Sugar first, and nothing in spec.md rules that out.

## Summary
1 MAJOR, 4 MINOR. The revision fixed both pass-1 MAJOR notes and five of the seven MINOR notes. The rewritten
items (AC-022, AC-052, AC-054, AC-064, AC-065, AC-091) and the new AC-092 to AC-094 are correct against the spec,
and the success-criteria and edge-case map is complete. The new Time convention removes the backwards setup
timelines. One problem blocks locking: the item set reuses F002's `acNNN.` emails and `ANNN ` habit names with
the same numbers, in the database that ADR 0003 has all features share in one run. So many F004 Givens fail
against a correct implementation whenever the matching F002 scenario's data hasn't been wiped by a reset. The
MINOR notes are a remaining timeline inconsistency, one traceability gap, one undiscriminated culture sort for
details, and one order assertion that relies on an unrestated slice 2 rule.
