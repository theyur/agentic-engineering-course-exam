# Critic pass 3: ac for 004-goals

**Verdict**: APPROVE
**Date**: 2026-11-17
**Reviewed**: specs/004-goals/acceptance.md (AC-001 to AC-094, the Coverage table and the "Success criteria and
edge cases" map); golden samples: none (tests/HT.AcceptanceTests/Samples/F004 does not exist and no item names
one)

Checked again and found consistent, so not listed below:

- The Coverage table matches every item header's FR/US references exactly. This was a scripted cross-check over
  94 items, with AC-091 now under FR-023 and FR-030, and it found no mismatch.
- The rename is complete. A script expanded `<n>` per Examples row and found 470 distinct emails and 203
  distinct backticked catalog-habit names. Every non-demo email starts with its own item's `f4.acNNN.` and every
  catalog-habit name with its own `A4.NNN`. No email or name is used by two items. No old-scheme `acNNN.` or
  `ANNN ` name is left.
- Across features, the only emails shared with specs/001 and specs/002 acceptance.md are `demo1@example.com` to
  `demo3@example.com`, and no catalog-habit name is shared with specs/002. Every F004 search (`A4.034`,
  `A4.052 <n>`, `A4.053 <n>`, `A4.055 <n>`, `A4.056`, `A4.057 <n>`, `A4.058`) matches only names of its own item
  under the restated contains-ignoring-case rule (spec lines 141-144). No F002 search text (`A002`, `A010 night`,
  `"MORNING"`, `"ning pa"`, `"a040 r"`, …) selects an F004 name in a way that changes an F002 exact-result
  assertion: the F002 items that browse without a search assert inclusion and exclusion of their own names only.
  This also satisfies slice 2 FR-004 (catalog-habit names unique in any state).
- The name orders still hold after the rename. With .NET comparers, OrdinalIgnoreCase gives AC-021's order
  "A4.021 Reading, …", "A4.021 Yoga, …", "Cold shower, daily". For AC-054 it gives "A4.054 Reading, …",
  "evening pages", "Morning pages", "Äpfel pages". Ordinal gives "Morning" before "evening", and
  InvariantCultureIgnoreCase, de-DE and uk-UA all put "Äpfel" second, so AC-054 now tells the required order
  apart from both. The demo orders of AC-064 and AC-067 and the AC-022 order are unchanged and correct.
- AC-089 and AC-065 now have forward timelines under the new Time convention. In AC-089 the profile read runs at
  the registration time `2026-11-30T22:30:00Z`. In AC-065 the reads run at `2026-11-19T10:00:00Z`.

## Previous notes: resolution status
| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (pass 2: F004 emails and habit names collide with F002's) | Resolved | The convention "Names in the database" (lines 25-29) now uses `f4.acNNN.` and `A4.NNN ` and cites ADR 0003. The scripted comparison above finds no shared non-demo email with F001 or F002 and no shared catalog-habit name with F002. |
| MINOR-1 (pass 2: steps without a time run before steps given a later time) | Partly | The new rule (lines 11-12) makes AC-089 rows 02 and 03 and AC-065 run forward. Its fixed floor of `2026-11-18T10:00:00Z` now sends some steps back in time: see MINOR-1 below. |
| MINOR-2 (pass 2: AC-091 row 03 not traced) | Resolved | The AC-091 header (line 1401) is "FR-023; FR-028; FR-029; FR-030; US11-AS8". The Coverage rows FR-023 (line 103) and FR-030 (line 110) list AC-091. |
| MINOR-3 (pass 2: AC-054 does not rule out a culture or collation sort) | Resolved | Line 940 adds "Äpfel pages", and line 943 expects it last. A culture comparer or an accent-sensitive, case-insensitive SQL collation puts it second, so it no longer passes (comparer check above). |
| MINOR-4 (pass 2: AC-053 asserts an unrestated browse order) | Resolved | AC-053 row 01 (line 933) is now "`A4.053 01 Reading` and `A4.053 01 Sugar`, and nothing else", with no order. |

No older note has regressed:
- Pass 1 MAJOR-1: AC-052 still uses `<n>` everywhere.
- Pass 1 MAJOR-2: AC-064's list lines 1060-1062 give FR-015 fields only, plus the FR-014 start date of the
  not-yet-started "Meditation".
- AC-022 still uses distinct creation instants.
- AC-092, AC-093 and AC-094 and the success-criteria and edge-case map (lines 197-208) are unchanged.

## Notes

### MINOR-1: The fixed `2026-11-18T10:00:00Z` floor runs some steps later than the step they follow or check
- **Where**: acceptance.md Time convention (lines 11-12: "Any other step that gives no time, a Then step
  included, runs at `2026-11-18T10:00:00Z` or at the latest time of the steps before it, whichever is later").
  It applies to:
  - AC-088 (lines 1366-1369): setup at the earliest given time `2026-11-16T23:30:00Z`; then "the current culture
    of the test is …", with no time, at `2026-11-18T10:00:00Z`; then the When back at `2026-11-16T23:30:00Z`.
  - The Then steps of AC-023 rows 01 and 02 (lines 562-570), AC-044 (lines 833-844) and AC-086 rows 01 to 03
    (lines 1343-1352). Each runs at `2026-11-18T10:00:00Z`, after a When at an earlier instant.
- **Against**: acceptance.md Time convention line 7 ("Steps run in the order they are written"); constitution II
  and spec FR-012 (the supplied time is part of the input); spec FR-014 (the next commitment depends on "now").
- **Continues**: MINOR-1 (pass 2), MINOR-1 (pass 1)
- **Reasoning**: when every time an item gives is earlier than the default, the floor still moves its untimed
  non-setup steps to `2026-11-18T10:00:00Z`.
  - In AC-088 this produces a backwards timeline (step 2 two days after step 3). The culture step uses no time,
    so nothing changes.
  - For the Then steps, the convention now says explicitly that they run at an instant other than the When
    whose result they check. Most Then steps only inspect the When result, and then no asserted value changes.
  - But the convention invites a test writer to call the use case again at the Then step's own time. In AC-023
    row 01, listing at `2026-11-18T10:00:00Z` gives next commitment 2026-11-18, not the expected 2026-11-16. In
    AC-044, starting at that time gives the start date 2026-11-18, not 2026-11-16 or 2026-11-17.
  - The convention doesn't say that a Then step checking a When result uses that result. So the expected values
    of these rows depend on a reading that the convention's wording makes less likely.

## Summary
0 MAJOR, 1 MINOR. The revision resolves the pass-2 MAJOR. The `f4.acNNN.` / `A4.NNN ` scheme leaves no email or
catalog-habit name shared with F001 or F002 except the reset-only demo emails. The prefixed searches select only
their own item's habits. Every name-order expectation (AC-021, AC-022, AC-054, AC-064, AC-067) still holds and
is now discriminated against ordinal and culture sorts. The traceability gap for AC-091 and the unrestated
AC-053 order are gone, and the Coverage table still matches every item header. The one remaining note is that
the Time convention's fixed floor puts some steps at a later instant than the step they follow or check. Against
a correct implementation no asserted value changes, as long as Then steps only check the When result. The items
are ready to lock and translate.
