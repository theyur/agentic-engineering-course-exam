# Critic pass 1: features for 002-habit-catalog

**Verdict**: APPROVE
**Date**: 2026-11-12
**Reviewed**: tests/HT.AcceptanceTests/Features/F002/habit-catalog.feature (73 scenarios, 33 of them Scenario
Outlines; the only `.feature` file of F002).

Checked against: specs/002-habit-catalog/acceptance.md (73 items, AC-001 to AC-073),
specs/002-habit-catalog/spec.md, .specify/memory/constitution.md, docs/adr/0003-acceptance-tests.md,
specs/002-habit-catalog/ac/sources.lock, the translation report
(specs/002-habit-catalog/ac/runs/translate-001/translation-report.md), the rules the extension gives the
translator and the test writer (.specify/extensions/ac/agents/ac-feature-translator.md, ac-test-writer.md,
scripts/check_features.py; read, not run), and, for slice 1 facts, specs/001-accounts-and-access/spec.md
(FR-014, FR-016). Nothing was built or run, so the report's "build success" is not verified here.

Mechanical checks, done with a read-only script over the two files:

- Every item of acceptance.md (list markers removed, Markdown tables read as rows) was compared with the
  scenario that has the same `@AC-` id: tags, title, every step, every Examples cell and the data table of
  AC-064. Result: 73 of 73 identical, in the same order; no scenario without an item, no item without a
  scenario, so no item needs a skip record.
- The scenario tags agree with the Coverage table of acceptance.md for all 25 FR and all 53 covered spec
  scenarios; the 13 spec scenarios without a scenario are all marked *(page)* in spec.md.
- The rules of check_features.py were re-applied: one `@F002` tag on the Feature line, exactly one `@AC-` tag
  per scenario, no id used twice, a When and a Then in every scenario, no non-step line inside a scenario, no
  duplicate scenario title. Every `<placeholder>` of an outline is a column of its Examples table, every row
  has the column count of its header, and `n` runs 01, 02, … in every table.
- In every outline, each email and each catalog-habit name in the steps contains `<n>`, and each literal email
  or catalog-habit name inside a row carries that row's `n`; in every other scenario each email starts with
  `acNNN.` and each catalog-habit name with `ANNN ` of its own item (the demo emails appear only in scenarios
  that reset the database).
- Every refusal and result text in double quotes is word for word in spec.md. The 16 text and kind rows of
  AC-015, the three of AC-013, and the texts of AC-034 and AC-038 were recomputed by hand from FR-017; the
  name orders of AC-002, AC-011, AC-012, AC-040, AC-045, AC-048, AC-052 and AC-072 from FR-004; the check
  orders of AC-029, AC-035, AC-036, AC-039, AC-041, AC-046, AC-054 and AC-057 from FR-005 to FR-007 and
  FR-016; the 24 icons of AC-044 and the six demo custom habits of AC-064 against FR-003 and FR-031. No
  difference.
- sources.lock: the SHA-256 of acceptance.md and of spec.md in the working tree equal the locked values.
- The file is UTF-8 without BOM, LF line endings (as `.gitattributes` requires), no tabs, no trailing spaces.
  Its only non-ASCII characters are U+00B7 and U+2013 (the FR-017 kinds) and U+00C9 / U+00E9 (AC-045).

## Notes

### MINOR-1: The Conventions that give many steps their values are not in the feature file, and the translation report does not record that they were left out
- **Where**: habit-catalog.feature lines 1-2 (Feature line, no description or comment). Steps that depend on a
  convention, for example:
  - the fields a Given does not give: line 730 arranges "a Published `A058 <n> Reading`" and line 733 expects
    it "with the category Other and the intent Build"; line 8 gives `A001 <n> Yoga` three fields and line 12
    expects "no description and no suggested starting goal";
  - which suggested starting goal is marked: lines 7, 155, 213, 456, 476 and 516 give a single suggested
    starting goal without "(marked)" and expect success (line 518: "that suggested starting goal, marked"),
    while line 419 expects a refusal for one suggested starting goal "none marked";
  - the passwords behind "the registered user `x`" and "the administrator `a`" (every scenario, from line 6);
  - what "a Draft `N`", "a Published `N`" and "an Unpublished `N`" did before the scenario (every catalog
    Given);
  - "a text of 501 characters" (lines 303, 325, 326, 706), "a unit of 21 characters" (line 445), "a name of
    61 characters with the prefix …" (lines 302, 325, 705);
  - "browsing" and "the administrator's list" without an acting user or filters (for example lines 215-216),
    "an unknown identifier" (for example line 189), "the AC database is reset" (lines 95, 127, 753, …).
- **Against**: acceptance.md Conventions, lines 5-43 (lines 8-13: the passwords `walking42` and `admin2026`
  and who acts; lines 20-22: how a Draft, a Published and an Unpublished habit are made; lines 23-24: the
  default fields; lines 27-28: a single suggested starting goal is the marked one; lines 29-35: "browsing",
  "the administrator's list", "an unknown identifier"; lines 36-38: the filler characters `x`, `t` and `u`;
  lines 39-43: the reset). FR-006 (spec.md lines 524-529: a non-empty list needs exactly one mark).
  Review item 7 (dropped meaning). ac-test-writer.md, "What you can see": "The `.feature` files are what you
  bind; the acceptance criteria are only context". translation-report.md "Interpretations" (lines 95-104).
  The same note was MINOR-1 of specs/001-accounts-and-access/critic-features-pass-1.md; constitution V, third
  bullet (a review finding of the same class that appears twice).
- **Reasoning**: The item texts were carried over word for word, but the block that defines their phrases was
  not, in any form. Read alone, the feature file is in places undetermined and in one place at odds with the
  spec: by FR-006 the edits of lines 456 and 476 (one suggested starting goal, no mark stated) would be
  refused with "Mark one suggested starting goal for the card.", yet lines 457 and 477 expect success; only
  the convention of acceptance.md lines 27-28 makes them consistent. The category and intent that line 733
  checks are arranged by nothing in the scenario. The report's Interpretations list every syntax change and
  say nothing about the Conventions (its only mention of them, line 93, is about the reset of AC-012 and
  AC-072), so the omission is not recorded as a decision. The test writer does receive acceptance.md as
  context, so the meaning is still reachable and this does not block the next step; it means the file that is
  bound and locked is not self-contained. Slice 2 leans on the Conventions for more values than slice 1 did
  (default fields, the mark, three filler characters, two passwords), and this is the second feature for
  which the same note is made.

### MINOR-2: The details of a Published habit are read only by the anonymous visitor
- **Where**: habit-catalog.feature lines 139 (AC-013), 148 (AC-014) and 156 (AC-015): every scenario that
  reads the details of a Published habit does so as "the anonymous visitor". Lines 187-195 (AC-016) and
  206-208 (AC-017): the registered user and the administrator read catalog details only where the expected
  result is the refusal.
- **Against**: spec.md User Story 2, line 203 ("Anyone opens a published catalog habit and sees its name,
  tags, description and suggested starting goals"); US2-AS1, lines 214-220 ("**When** anyone reads its
  details"); US2-AS2, lines 221-222 ("anyone browses the catalog or reads its details"); FR-010, lines
  564-568; Edge Cases, lines 482-483 ("An administrator browses the public catalog exactly like a registered
  user"). Review item 1 (partly covered).
- **Reasoning**: For browsing, AC-001 (lines 17-19) and AC-009 (lines 104-106) run as the anonymous visitor,
  a registered user and an administrator. For details, "anyone" of US2-AS1 and US2-AS2 became one audience.
  This slice has several results that depend on the role of the acting user (FR-014, FR-016, FR-025), so the
  case is not empty: an implementation that gives the public details of a Published habit only to a caller
  who is not signed in, or that gives a signed-in administrator or registered user a different content,
  passes all 73 scenarios. The narrowing is already in the items of acceptance.md (lines 288, 297, 305), which
  the feature file reproduces exactly; it is not a translation fault.

### MINOR-3: The translation report counts the Scenario Outlines and Scenarios wrongly
- **Where**: specs/002-habit-catalog/ac/runs/translate-001/translation-report.md line 7: "73 scenarios: 43
  `Scenario Outline` and 30 `Scenario`".
- **Against**: habit-catalog.feature: 33 lines start with `Scenario Outline:` and 40 with `Scenario:` (33 +
  40 = 73). The same report, line 98, lists the items that became a `Scenario Outline`: 33 ids. acceptance.md
  has 33 items with an `Examples:` table. Review items 3 and 4.
- **Reasoning**: The total is right and both parts are wrong by ten, in opposite directions; the report
  contradicts its own list in line 98. The feature file is not affected (all 33 items with an
  Examples table are outlines and no other scenario is). The report is the record that step 15 and step 17 of
  /speckit-ac-translate show to the owner before the features are approved and locked, so a count taken from
  it is wrong.

## Summary

0 MAJOR, 3 MINOR.

The feature file is a faithful translation: all 73 items of the locked acceptance.md have exactly one scenario
with the same id, tags, title, steps and values, the 33 items with Examples are Scenario Outlines with the
same rows, the empty text and the three-space text are kept distinct, and no scenario, step, value or
assertion was added or dropped. The structure meets the extension's rules for feature files, and the locked
source hashes match the files that were translated. Against spec.md, every FR and every non-page acceptance
scenario has a scenario, every expected text, kind, order and refusal that was recomputed agrees with FR-003
to FR-017, FR-030 and FR-031, and the scenarios that do not reset the database hold in any state of the shared
database because they search by their own name prefix or check by inclusion. The three notes do not concern
the translation of any item: the Conventions of acceptance.md are needed to read many steps and are absent
from the bound file, the public details of a Published habit are exercised for one audience only, and the
translation report states a wrong split of its 73 scenarios. None of them contradicts the spec or blocks
locking the feature file and the next step.
