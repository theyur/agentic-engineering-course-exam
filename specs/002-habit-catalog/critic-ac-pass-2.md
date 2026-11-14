# Critic pass 2: ac for 002-habit-catalog

**Verdict**: APPROVE
**Date**: 2026-11-12
**Reviewed**: specs/002-habit-catalog/acceptance.md (72 items, AC-001 to AC-072; 34 items with an `Examples:` table; no golden samples)

Checked against: specs/002-habit-catalog/spec.md (working tree, with the new section "Slice 1 behaviour these
tests use"), .specify/memory/constitution.md, SPEC.md, docs/adr/0003-acceptance-tests.md. For facts about
slice 1 and the item format, also read: specs/001-accounts-and-access/spec.md (FR-014, FR-016),
specs/001-accounts-and-access/acceptance.md (conventions, emails, AC-055 to AC-060),
.specify/extensions/ac/templates/acceptance-template.md and
.specify/extensions/ac/scripts/check_acceptance.py (read, not run). Line numbers are those of the files in the
working tree.

Mechanical checks done on the file (read-only script): ids AC-001 to AC-072 without a gap or duplicate; every
FR of spec.md has a Coverage row and at least one item; every spec scenario has an item or a "Not testable"
row with a reason; every Examples table has the column `n` with the values 01, 02, … in order; in every
Examples item each email and each catalog-habit name in the Given/When/Then lines contains `<n>`, and each
literal email or catalog-habit name inside a row carries that row's `n`; in every other item each email
starts with `acNNN.` and each catalog-habit name with `ANNN ` of its own item; no email is shared with slice
1's acceptance.md; every refusal and result text is word for word in spec.md; `É` in AC-045 is the single
character U+00C9; the file has LF line endings and no BOM (`.gitattributes`: `specs/*/acceptance.md text
eol=lf`).

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1 (Examples rows repeat accounts and habits) | Resolved | Conventions lines 16–19 state that each row is its own scenario in the same database and that every email and catalog-habit name of such an item contains `<n>`. All 34 Examples items follow it (script check above); custom habits belong to the row's own accounts; AC-009 and AC-063 reset the database in every row. |
| MAJOR-2 (category filter and Home preview not checked for custom habits; filter not for Unpublished) | Resolved | AC-009 (lines 240–255): reset database, a Published habit in Body, a Draft in Home, an Unpublished in Money, a custom habit in Mind, filter "exactly "All", "Body"". AC-012 (lines 274–282): custom habit `A012 Aaa private`, Draft `A012 Aardvark` and Unpublished `A012 Abacus` would all sort before `A012 Alpha`. Coverage row FR-024 (line 70) lists both. |
| MINOR-1 (Draft/Unpublished kept out of browsing only for the anonymous visitor) | Resolved | AC-001 (lines 143–160) browses as anonymous visitor, registered user and administrator with a Draft and an Unpublished habit; AC-009 asks for the category filter as each of the three. |
| MINOR-2 (order examples pass with a case-sensitive order; administrator's list order never asserted) | Resolved | AC-002: `A002 apple`, `A002 Breathing`, `A002 walking` (case-sensitive gives Breathing first). AC-048: `a habit`, `B habit` (case-sensitive gives `B habit` first). AC-040 (lines 670–676): `A040 rowing` before `A040 Running` in the administrator's list (case-sensitive gives `Running` first). |
| MINOR-3 (AC-045 fixed the direction of case folding) | Resolved | AC-045 (lines 755–760) now uses `A045 Éclair`; U+00C9 and U+00E9 are above `Z` and `z`, so the expected order `Apple`, `zebra`, `Éclair` holds for upper and lower folding, and differs from a culture order (`Apple`, `Éclair`, `zebra`). |
| MINOR-4 (AC-005 did not show trimming of the search) | Resolved | AC-005 row 01 (line 202): `"  a005 01 morn  "`; the name has no two spaces before `A005` and none after `Morn`, so the row fails without trimming. |
| MINOR-5 (AC-010 passed with the category alone) | Resolved | AC-010 (lines 257–265): `Night snacks` fails only the intent, `Day walk` only the search, `Night reading` only the category. |
| MINOR-6 (AC-064 put a name rule on the seed catalog) | Resolved | AC-064 lines 1010–1011 compare identifiers, not names. |
| MINOR-7 (total of the administrator's list never exact) | Resolved | AC-062 line 975: "exactly `A062 Rowing`, with the total 1" in a reset database. See MINOR-5 below for the clause that follows it. |
| MINOR-8 (trimming of descriptions, blank description, trimming before the length check; bounds for custom habits) | Resolved | AC-070 rows 01–04 (lines 1073–1076): trimmed name and description, `"   "` gives no description, 66 characters trimmed to 60 and 504 trimmed to 500 are accepted. AC-059: 60 and 500 characters for a custom habit. See MINOR-4 below for custom habits. |
| MINOR-9 (amount boundaries for count only; Reduce below 0; unit with surrounding spaces) | Partly | AC-036 rows 08–10 (duration missing, 0, 1000), rows 12–14 (Reduce -1, missing, duration 1000); AC-037 rows 08–09 (unit with surrounding spaces, limit after trimming). What is kept for such a unit is still not stated: continued as MINOR-1. |
| MINOR-10 (check orders without example) | Resolved | AC-029 row 02 (name length before description), row 06 (icon before the list of suggested starting goals), row 08 (list check before the check of each goal), row 07 (list before name taken); AC-057 rows 04–05 (custom habit: field before name taken). |
| MINOR-11 (few fields edited; no custom-habit edit that keeps its name; AC-035 without "unchanged") | Resolved | AC-071 (description, intent, icon of a catalog habit); AC-049 (second edit keeps the name and changes category, icon, description); AC-035 column `after`. |
| MINOR-12 (values that are not literals) | Resolved | Conventions lines 36–38 define "a text of N characters" and "a unit of N characters"; lines 23–24 apply the default fields to custom habits; AC-029 rows 06–08 give their suggested starting goals; AC-021 line 393; AC-054 line 856; AC-033 lines 549 and 554 give the fields. |
| MINOR-13 (second behaviour and second action inside a Then line: AC-028, AC-012) | Resolved | AC-028 has only the bounds, trimming is AC-070; "fewer when fewer exist" is the new AC-072. The same kind of line remains in two other items: MINOR-6 below. |
| MINOR-14 (US6-AS6, FR-030, FR-031 covered less than shown) | Resolved | AC-065 lines 1020–1022 (all three accounts); AC-060 line 952 (every habit has a description); AC-064 line 998 (no result text has a custom-habit name); Coverage row FR-031 (line 73) has its note. |
| MINOR-15 (AC-001 heading, missing AC-059, reason for the reset items) | Resolved | AC-001 heading is "(FR-008; US1-AS1)"; AC-059 exists; conventions lines 39–43 name AC-012 and AC-072 with their own reason. A new mismatch of the same kind: MINOR-7 below. |

Only one earlier pass exists for this stage, so there is no older note to check for regression.

## Notes

### MINOR-1: AC-037 accepts a unit with surrounding spaces but no line says which unit is kept

- **Where**: acceptance.md, AC-037, line 631 (Then) and rows 08–09 (lines 644–645).
- **Against**: FR-007 (spec.md lines 532–534: "a unit of 1 to 20 characters after trimming, kept as entered");
  FR-017 (lines 599–600: "count "N unit" with the unit as entered"); AC rule on discriminating examples.
- **Continues**: MINOR-9 of pass 1.
- **Reasoning**: Rows 08 and 09 give the units `"  pages  "` and two spaces + 20 characters + two spaces. Row
  09 shows that the limit of 20 applies after trimming. The Then line lists what reading gives back: "Build or
  Reduce, the measurement, the amount and the schedule given above". The unit is not in that list, for any of
  the ten rows. So an implementation that keeps `pages` and one that keeps `"  pages  "` both pass, and the
  FR-017 text of such a goal ("5 pages a day" or the same with the spaces) is not stated either. Row 08 (9
  characters before trimming) then separates nothing that row 01 does not. spec.md has both phrases, "after
  trimming" and "kept as entered", and the item neither picks a reading nor says that it leaves the point open.

### MINOR-2: Editing with a catalog habit's identifier through the custom-habit use case has no example

- **Where**: acceptance.md, AC-058, rows 04–05 (lines 937–938); AC-053; AC-057 row 03.
- **Against**: FR-022 (spec.md lines 619–621: "Reading, editing or deleting with an identifier that is not one
  of the acting user's custom habits (another user's, a catalog habit's, or none) MUST give "This custom habit
  does not exist." and change nothing").
- **Continues**: new.
- **Reasoning**: FR-022 has three actions and three kinds of identifier. Another user's identifier has all
  three actions (AC-053), "none" has all three (AC-058 rows 01–03, AC-057 row 03). A catalog habit's
  identifier has read (row 04) and delete (row 05) only. The missing case is the one in which a registered user
  could change public catalog content: an edit that finds the habit by identifier without checking that it is
  the acting user's custom habit passes every item. The Then line of AC-058 checks only that the catalog habit
  is still found by its name.

### MINOR-3: No example shows that an administrator is refused before the field checks of a custom habit

- **Where**: acceptance.md, AC-054 rows 03 and 05 (lines 865, 867); AC-057 (lines 905–920); AC-039 (lines
  655–668).
- **Against**: FR-016 (spec.md lines 591–596: step 2, "a custom-habit use case for an administrator "Custom
  habits are for registered users."", before step 4, the field checks).
- **Continues**: new.
- **Reasoning**: AC-039 rows 01 and 04 show role before fields for the catalog use cases (a registered user
  with the name `"   "`). For custom habits, AC-057 row 01 shows it for the anonymous visitor only. The
  administrator's rows in AC-054 all give fields that pass: row 03 edits with the name `Seen`, row 05 creates
  `Admin habit`. An implementation that checks the fields of a custom habit before the role gives an
  administrator "Enter a name." for a blank name and passes every item. AC-057's title, "the acting user and
  ownership come first", names two of the steps of FR-016 and not the role.

### MINOR-4: Stored trimmed names and descriptions are shown for catalog habits only

- **Where**: acceptance.md, AC-070 (lines 1062–1076, catalog habits only); AC-047, AC-049, AC-051, AC-059
  (custom habits).
- **Against**: FR-004 (spec.md line 511: "The system MUST trim names and descriptions", for both kinds of
  habit); FR-005 (line 521: "An empty description after trimming means no description"); FR-001.
- **Continues**: new (the catalog side of pass 1 MINOR-8 is resolved by AC-070).
- **Reasoning**: Every successful creation or edit of a custom habit gives a name and a description without
  surrounding spaces (AC-047, AC-049, AC-059). AC-051 shows only that the name comparison ignores surrounding
  spaces, in a refusal. So no item fails for a custom habit that is stored with its spaces, for a custom-habit
  description of only spaces that is kept as text, or for a custom-habit length check before trimming.

### MINOR-5: AC-062 states that six custom habits exist, and no line checks it

- **Where**: acceptance.md, AC-062, line 975 ("with the total 1, although demo loading also created six custom
  habits"); Coverage row FR-024 (line 70).
- **Against**: FR-024 (spec.md lines 623–624); FR-031 (lines 640–651: custom habits for each account the run
  creates); acceptance-template.md ("Then [observable outcome with concrete values]").
- **Continues**: new (it concerns the item that resolves pass 1 MINOR-7).
- **Reasoning**: AC-062 is the only item that fixes the total of the administrator's list as a number, and the
  Coverage table lists it under FR-024. "Total 1" is evidence for FR-024 only when custom habits exist in the
  database. The item says so in a clause of the Then line, but has no line with an account result, a
  custom-habit result or a list of a demo account. AC-064 to AC-068 check the demo custom habits only for runs
  with an empty catalog; for the run with a catalog that is not empty nothing is checked.

### MINOR-6: AC-060 and AC-064 create an administrator inside a Then line

- **Where**: acceptance.md, AC-060 line 955 and AC-064 line 1011 ("for the administrator `ac060.admin@example.com`,
  the administrator's list has …"); conventions lines 11–13; AC-061 line 964 and AC-062 line 972.
- **Against**: acceptance-template.md (lines are Given [state] / When [action] / Then [outcome]).
- **Continues**: new; same kind as pass 1 MINOR-13.
- **Reasoning**: By the conventions, "the administrator `a`" means that administrator creation ran, gave
  "administrator created", and sign-in succeeded. In AC-060 and AC-064 this phrase first appears in a Then line,
  so that line runs two use cases that change state before it checks anything. AC-061 and AC-062 put the same
  setup in a Given line. The file is not consistent here, and nothing says whether the administrator of AC-060
  and AC-064 is meant to be created after demo loading.

### MINOR-7: AC-040 cites FR-004 and the Coverage row of FR-004 does not list it

- **Where**: acceptance.md, AC-040 heading (line 670: "(FR-004; FR-012; US3-AS19)"); Coverage row FR-004
  (line 52).
- **Against**: internal consistency of the Coverage table with the item headings.
- **Continues**: new; same kind as pass 1 MINOR-15.
- **Reasoning**: The Coverage row of FR-004 lists AC-002, AC-026, AC-045, AC-048, AC-049, AC-051, AC-052 and
  AC-070. AC-040's heading also cites FR-004. It is the only difference between the headings and the Coverage
  table (all other FR and scenario rows match the headings in both directions).

## Summary

0 MAJOR, 7 MINOR.

Both MAJOR notes of pass 1 are resolved: every Examples row now has data of its own, and the category filter
and the Home preview are checked against custom habits, Drafts and Unpublished habits. Fourteen of the fifteen
MINOR notes are resolved and one is partly resolved (the unit with surrounding spaces, MINOR-1). The refusal and
result texts match spec.md word for word, the 16 text and kind rows of AC-015 and the three of AC-013 follow
from FR-017, and the items that do not reset the database still hold when slice 1's demo scenarios have loaded
the seed catalog, because they search by their own name prefix or check by inclusion. The remaining notes are
single cases without an example (FR-022 edit with a catalog habit's identifier, the role check before the
fields of a custom habit, trimmed storage of custom habits), one unchecked clause, and two small
inconsistencies. None of them blocks locking the file or the translation to `.feature` files.
