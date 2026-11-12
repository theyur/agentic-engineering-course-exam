# Critic pass 1: ac for 002-habit-catalog

**Verdict**: REVISE
**Date**: 2026-11-12
**Reviewed**: specs/002-habit-catalog/acceptance.md (68 items, AC-001 to AC-069 without AC-059; no golden samples)

Checked against: specs/002-habit-catalog/spec.md, .specify/memory/constitution.md, SPEC.md,
docs/adr/0003-acceptance-tests.md. For the facts acceptance.md states about slice 1 and about the item format,
also read: specs/001-accounts-and-access/spec.md (FR-014, FR-016), specs/001-accounts-and-access/acceptance.md
(conventions), .specify/extensions/ac/templates/acceptance-template.md. Line numbers are those of the files
in the working tree.

## Notes

### MAJOR-1: 28 of the 29 Examples items repeat the same accounts and habits in every row of a database that is not reset between rows

- **Where**: acceptance.md, conventions lines 8–21 and line 14 ("Every email belongs to one item only"), and
  every item with an `Examples:` table except AC-063: AC-004, AC-005, AC-006, AC-015, AC-016, AC-017, AC-026,
  AC-027, AC-029, AC-030, AC-031, AC-032, AC-033, AC-035, AC-036, AC-037, AC-039, AC-041, AC-043, AC-044,
  AC-046, AC-051, AC-053, AC-054, AC-055, AC-056, AC-057, AC-058.
- **Against**: ADR 0003 lines 30–33 (schema rebuilt once per run; scenarios share one database, "Each scenario
  arranges the data it needs"); spec.md lines 136–146 ("all acceptance scenarios of every feature share one
  database in one run … nothing in this slice removes a catalog habit"); spec.md lines 102–112 (the email is
  unique; administrator creation gives "administrator created" only "for an email that has no account");
  slice 1 FR-014 step 3 ("administrator already exists" when an administrator has that email); FR-004/FR-005
  (a taken name gives "A catalog habit with this name already exists." / "You already have a custom habit with
  this name."); slice 1 acceptance.md conventions ("Every valid email … belongs to one item only, and to one
  Examples row only").
- **Reasoning**: The conventions define the Givens as actions that succeeded: "the registered user `x`" means
  registration of `x` succeeded; "the administrator `a`" means administrator creation "gave 'administrator
  created'"; "a Draft `N`" means the administrator "created the catalog habit `N` and nothing else happened to
  it". An Examples table becomes a Scenario Outline, so each row runs the Given lines again. Only the items
  that say "the AC database is reset" start empty; the 28 items listed do not. In the second and later rows the
  same email already has an account (registration cannot succeed, administrator creation gives "administrator
  already exists") and the same habit names are taken. Examples: AC-004 creates `ac004.admin@example.com`,
  `A004 Sugar` and `A004 Water` in each of 3 rows; AC-016 creates one administrator, two habits and one
  registered user in each of 9 rows; AC-044 creates `ac044.admin@example.com` in each of 28 rows; AC-051
  registers `ac051.user@example.com` and creates `Cold shower` and `Evening walk` in both rows. As written, the
  Givens of every row after the first cannot hold. The file is also not consistent with itself here: AC-015,
  AC-026, AC-035, AC-036, AC-037 and AC-041 make the habit name differ per row with `<n>`, but keep one
  administrator email for all rows, and the other items make nothing differ per row. Slice 1's locked criteria
  state the per-row rule for emails explicitly; this file's line 14 drops it. Whether a Given may instead mean
  "exists already" is not stated anywhere, so the translator and the test writer have no rule to follow.

### MAJOR-2: FR-024 and SC-005 name the category filter and the Home preview, and no item checks them for custom habits (nor the category filter for an Unpublished habit)

- **Where**: acceptance.md, Coverage row FR-024 (line 64: AC-006, AC-017, AC-033, AC-064); AC-008 and AC-009
  (the only category-filter items besides AC-060); AC-012 (the only Home-preview item).
- **Against**: FR-024 (spec.md lines 623–624: "browsing, search, the category filter, details, the Home preview
  or the administrator's catalog list"); SC-005 (lines 800–801: Draft, Unpublished and custom habits appear in 0
  public results, "the category filter … and the Home preview" included); constitution III (privacy enforced in
  the use cases).
- **Reasoning**: AC-006 covers browsing and search, AC-017 details, AC-033 and AC-064 the administrator's list.
  No item has a custom habit in a category without a Published habit and then asks for the category filter:
  AC-009 arranges only a Published habit in Body and a Draft in Home; in AC-060 the seed already covers all six
  categories, so the demo custom habits cannot change the filter. No item has a custom habit when the Home
  preview is asked for: AC-012 arranges only catalog habits. An implementation whose category filter or Home
  preview also reads custom habits passes every item, although FR-024 forbids it and the leak is of private
  data. The same holds for an Unpublished habit in the category filter: AC-009 has a Draft only, so a filter
  that counts Unpublished habits passes, against SC-005. The Coverage table presents FR-024 as covered without
  a note on these surfaces.

### MINOR-1: Draft and Unpublished habits are kept out of browsing only for the anonymous visitor

- **Where**: acceptance.md, conventions line 26–28 ("browsing" is run by the anonymous visitor unless the item
  names another acting user); AC-001, AC-018, AC-021, AC-025, AC-069; AC-006.
- **Against**: FR-008 ("The same result is given to every audience"); spec.md Edge Cases lines 480–481 ("An
  administrator browses the public catalog exactly like a registered user; drafts appear only in the
  administrator's own catalog list").
- **Reasoning**: The only item in which a registered user or an administrator browses is AC-006, and it has no
  Draft or Unpublished habit. The category filter and the Home preview are asked for only by the anonymous
  visitor. An implementation that shows Drafts to an administrator who browses passes every item. AC-016 covers
  this edge case for details only.

### MINOR-2: The examples for "name order, ignoring case" pass with a case-sensitive order, and the order of the administrator's list is never asserted

- **Where**: acceptance.md, AC-002 (lines 148–153), AC-048 (lines 749–755), AC-045 (lines 719–724); every item
  that uses "the administrator's list".
- **Against**: FR-004 (order compares names ordinally, ignoring case); FR-012 ("list all catalog habits in every
  state, in name order"); FR-020 ("in name order, FR-004"); AC rule "the examples would fail for a plausible
  wrong implementation".
- **Reasoning**: AC-002 expects `A002 Breathing`, `A002 Cycling`, `A002 walking`. A case-sensitive ordinal
  order gives the same sequence ('B' 0x42, 'C' 0x43, 'w' 0x77), so the item separates name order only from
  creation order, not "ignoring case" as its title says. AC-048 expects `A habit`, `b habit`; case-sensitive
  ordinal gives the same ('A' 0x41 before 'b' 0x62). The lists of AC-051 and AC-052 also start with upper-case
  letters only. So no item fails for a custom-habit list ordered case-sensitively. AC-045 does separate the two
  orders, but only for browsing. For the administrator's list no item states any order: the items check
  "contains", "exactly one habit named", "the same names with the same states" or a one-habit list.

### MINOR-3: AC-045 fixes a rule that FR-004 does not state: which way case is folded

- **Where**: acceptance.md, AC-045, line 724 ("the upper-case `Z` sorts before `_` ordinally").
- **Against**: FR-004 (spec.md lines 511–514: "compare names ordinally, ignoring case (culture-invariant)");
  constitution, Decision Discipline (decisions are recorded in the spec's Clarifications).
- **Reasoning**: The expected order `A045 Apple`, `A045 zebra`, `A045 _note` holds only when names are
  compared in upper case ('Z' 0x5A before '_' 0x5F). Comparing in lower case is also ordinal, case-ignoring and
  culture-invariant, and gives `A045 _note`, `A045 Apple`, `A045 zebra` ('_' 0x5F before 'a' 0x61). FR-004 does
  not say which. The item therefore decides a rule that exists only in its parenthesis, for the six characters
  between 'Z' and 'a'.

### MINOR-4: AC-005 does not show that a search text is trimmed

- **Where**: acceptance.md, AC-005, lines 177–193, row `" morning "`.
- **Against**: FR-009 ("name contains the trimmed text"); US1-AS5; AC rule on discriminating examples.
- **Reasoning**: `A005 Morning pages` has a space before and after "Morning", so it contains `" morning "`
  untrimmed (ignoring case). The row passes without trimming. The rows `""` and `"   "` pass for an
  implementation that treats a blank text as "match all" and uses any other text as typed. No row has leading
  or trailing spaces that the name does not also have.

### MINOR-5: AC-010 passes when only the category is applied

- **Where**: acceptance.md, AC-010, lines 234–242.
- **Against**: FR-009 ("All given filters apply together"); AC rule on discriminating examples.
- **Reasoning**: The two habits that must be absent are in Health and Mind, so the category Body alone excludes
  both; all three names contain "night". No habit matches the category and fails only the intent, or matches
  category and intent and fails only the search. An implementation that ignores intent and search once a
  category is given passes the item. AC-007 combines category and search but expects an empty result that the
  category alone would give for its own data.

### MINOR-6: AC-064 puts a rule on the seed catalog that the spec does not have

- **Where**: acceptance.md, AC-064, lines 963–964 (browsing with each of the six names "contains no habit of
  that name"; the administrator's list "contains no habit of those six names").
- **Against**: FR-004 ("A catalog habit and a custom habit may share a name"); FR-030 (content rules of the seed
  catalog; no rule on names); FR-031.
- **Reasoning**: AC-064 runs demo loading on a reset database, so the seed catalog is present. The item
  identifies a leaked custom habit by its name alone. A seed catalog habit named "Evening stretch" or "Water
  the plants" is allowed by FR-004 and FR-030, and would make the item fail with no custom habit leaked. The
  item thus depends on "no seed habit has one of these six names", which neither spec.md nor the item states.

### MINOR-7: The total of the administrator's list is never checked as an exact number

- **Where**: acceptance.md, AC-040, lines 639–640 ("at least 2"; "the same total").
- **Against**: FR-012 ("together with the total number of catalog habits"); FR-024 (custom habits in no catalog
  result).
- **Reasoning**: "At least 2" and "the same with and without search" hold for a total that also counts custom
  habits, or anything else that grows. The file has items that start from a reset database with a known catalog
  (AC-009, AC-012, AC-062), but none of them states the total. No example would fail for a wrong total that is
  2 or more.

### MINOR-8: Trimming of descriptions, "empty after trimming means no description", and trimming before the length check have no example

- **Where**: acceptance.md, AC-027, AC-028 (line 450), AC-056.
- **Against**: FR-004 ("trim names and descriptions"); FR-005 ("An empty description after trimming means no
  description"; the length limits).
- **Reasoning**: AC-028 trims a name of 12 characters with 3 spaces on each side: 18 characters in all, under
  60 either way, so it does not show whether the limit of 60 applies before or after trimming. No item gives a
  description with surrounding spaces, or a description of only spaces. The accepted bounds (60 and 500) are
  shown for catalog habits only; AC-056 has only refusals for custom habits.

### MINOR-9: Amount boundaries of FR-007 are covered for count only, and not below 0 for Reduce

- **Where**: acceptance.md, AC-036 rows 05–08 (lines 581–584), AC-037.
- **Against**: FR-007 ("for count and duration an amount missing or out of range"; Reduce "from 0 to 999"; "a
  unit of 1 to 20 characters after trimming, kept as entered"); AC rule on boundaries.
- **Reasoning**: All four amount refusals use count. No row has a duration with the amount 0, 1000 or missing,
  so a check that exists for count only passes. For Reduce the only refusal is 1000; no row is below 0 and no
  row has a missing amount, so the Reduce message is tied to one case. For the unit, no row has surrounding
  spaces, so neither "after trimming" for the limit nor what "kept as entered" means for such a unit is shown.

### MINOR-10: Several orders of checks that FR-005 to FR-007 and FR-016 state have no example

- **Where**: acceptance.md, AC-029 (lines 452–468), AC-035, AC-036, AC-041, AC-057.
- **Against**: FR-005 (name length, then description, …, icon, then suggested starting goals, then name
  taken); FR-007 ("checked in list order, after the list checks of FR-006"); FR-016 steps 4 and 5.
- **Reasoning**: No row combines a name of 61 characters with a description of 501 characters, or an icon that
  is not a habit icon with a wrong list of suggested starting goals. Every item with an invalid suggested
  starting goal (AC-036, AC-041) has a correct list with one mark, and every item with a wrong list (AC-035,
  AC-029 row 6) has only valid goals, so "list checks before the checks of each goal" is not shown. For custom
  habits no row has a field refusal together with a taken name.

### MINOR-11: "Edit every field" is exercised for few fields, and a custom-habit edit that keeps its name never

- **Where**: acceptance.md, AC-020, AC-034, AC-035, AC-037, AC-038, AC-069 (catalog); AC-049 (custom); AC-035.
- **Against**: FR-012 ("edit every field of FR-001 except the state"); FR-020 ("edit every field of it");
  FR-004 (uniqueness among the user's custom habits); US3-AS15 ("and nothing changes").
- **Reasoning**: Successful catalog edits change only the name, the category and the suggested starting goals;
  none changes the description, the intent or the icon. AC-049 is the only successful custom-habit edit; it
  changes the name and the intent, so category, icon and description are never edited, and no custom-habit edit
  keeps its own name (an edit refused because the habit's own name counts as taken would pass every item).
  AC-035 states the refusals of US3-AS15 but, unlike AC-036, has no line that the habit's suggested starting
  goals are unchanged afterwards.

### MINOR-12: Some values are not concrete literals

- **Where**: acceptance.md, line 37 (conventions); AC-027 and AC-056 ("a text of 501 characters"); AC-028 ("a
  description of 500 characters"); AC-029 row 2 and row 6 (line 467, "suggested starting goals with none
  marked"); AC-036 row 10 ("a unit of 21 characters"); AC-037 row 5 ("a unit of 20 characters"); AC-021 line 370
  ("with no confirmation step"); AC-054 line 819 ("contains no detail of `Secret`"); AC-033 line 529 ("with its
  fields unchanged").
- **Against**: acceptance-template.md ("Every value is a concrete literal"); AC rule 7.
- **Reasoning**: The conventions define the three-space text and the name of 61 characters, but not the texts
  of 500 and 501 characters or the units of 20 and 21 characters. AC-029 row 6 gives neither the number nor the
  fields of its suggested starting goals. "No confirmation step" and "no detail of `Secret`" name no value of
  the result that a test can compare. AC-033 says "fields unchanged" for a custom habit whose fields the item
  never gives; the default fields are stated in the bullet about catalog habits (lines 18–21, with "no
  suggested starting goal"), and it is not stated that they also apply to custom habits, which ten items create
  with a name only.

### MINOR-13: Some items hold more than one behaviour, with a second action inside a Then line

- **Where**: acceptance.md, AC-028 line 450; AC-012 line 258.
- **Against**: acceptance-template.md ("One item per behaviour"; lines are Given / When / Then).
- **Reasoning**: AC-028 is about the accepted bounds (60 and 500 characters); its last line adds a different
  behaviour, trimming of a stored name (FR-004), through a second creation that no When line states. AC-012
  checks the first four habits and then, inside an And line, three unpublish actions and a second preview
  ("fewer when fewer exist").

### MINOR-14: Parts of US6-AS6, FR-030 and FR-031 are covered less than the Coverage table shows

- **Where**: acceptance.md, AC-065 line 973; AC-060; Coverage rows FR-030 and FR-031 (lines 66–67).
- **Against**: US6-AS6 ("each account lists the same custom habits"); SC-007; FR-030 ("each with a neutral
  description"); FR-031 ("as a whole with the account"; "The results name the demo email but never a
  custom-habit name").
- **Reasoning**: AC-065 checks the list of `demo1@example.com` only, not of `demo2` and `demo3`. AC-060 checks
  icons, categories, intents and suggested starting goals of the seed, but not that each Published seed habit
  has a description. No item checks that the results of demo loading contain no custom-habit name. The FR-030
  row notes that "as a whole or not at all" cannot be arranged; the FR-031 row has no such note for "as a
  whole with the account", which is the same kind of rule.

### MINOR-15: Small inconsistencies inside the file and with the spec section it cites

- **Where**: acceptance.md, AC-001 heading (line 137); Coverage row FR-024 (line 64); item numbering;
  conventions lines 33–36.
- **Against**: FR-024; FR-008; spec.md lines 144–146 (the scenarios that start from a reset database).
- **Reasoning**: AC-001's heading cites FR-024, but the item has no custom habit (its excluded habits are a
  Draft and an Unpublished one, which is FR-008), and the Coverage row of FR-024 does not list AC-001. The ids
  go from AC-058 to AC-060; there is no AC-059 and nothing says why. The conventions give spec.md's "What
  acceptance tests can reach" as the reason for the reset items "AC-009, AC-012 and AC-060 to AC-068", but that
  section lists only US1-AS9 and US6-AS1 to US6-AS8; AC-012 (Home preview) and AC-068 are not in it.

## Summary

2 MAJOR, 15 MINOR.

The file covers every FR and every spec scenario by id, its refusal texts match spec.md word for word, the 24
icons and the six demo custom habits match FR-003 and FR-031, and all 19 text and kind examples of AC-013 and
AC-015 follow from the rules of FR-017. It is not ready to lock. MAJOR-1 affects 28 items: under ADR 0003 and
the file's own conventions the Given lines of every Examples row after the first cannot hold, and the file
does not say what a repeated Given means. MAJOR-2 leaves two surfaces that FR-024 and SC-005 name without any
check for private custom habits. The MINOR notes are mostly examples that a plausible wrong implementation
would pass (case-insensitive order, trimmed search, combined filters, amount boundaries, check orders) and
values that are not literals.
