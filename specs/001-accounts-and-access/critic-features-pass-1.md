# Critic pass 1: features for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: tests/HT.AcceptanceTests/Features/F001/accounts-and-access.feature (61 scenarios, 10 of them
Scenario Outlines; the only `.feature` file of F001).

Checked against: specs/001-accounts-and-access/acceptance.md (61 items, AC-001 to AC-061),
specs/001-accounts-and-access/spec.md, .specify/memory/constitution.md, docs/adr/0003-acceptance-tests.md,
specs/001-accounts-and-access/ac/sources.lock, the translation report
(specs/001-accounts-and-access/ac/runs/translate-001/translation-report.md), and the rules the extension gives
the translator and the test writer (.specify/extensions/ac/agents/ac-feature-translator.md,
ac-test-writer.md, scripts/check_features.py; read, not run).

Mechanical checks, done with a read-only script over the two files (nothing was built or run):

- Every item of acceptance.md was normalised (list markers removed, backtick values put in double quotes,
  Markdown tables read as rows) and compared with the scenario that has the same `@AC-` id: tags, title, every
  step and every Examples cell. Result: 61 of 61 identical, in the same order; no scenario without an item,
  no item without a scenario.
- The scenario tags agree with the Coverage table of acceptance.md for all 16 FR and all 39 covered spec
  scenarios.
- The rules of check_features.py were re-applied by hand: one `@F001` tag on the Feature line, exactly one
  `@AC-` tag per scenario, no id used twice, a When and a Then in every scenario, no non-step line inside a
  scenario, no duplicate scenario title, no duplicate Examples row.
- sources.lock: the SHA-256 of acceptance.md and of spec.md in the working tree equal the locked values.
- The file is ASCII with LF line endings (as `.gitattributes` requires), no tabs, no trailing spaces.
- Every password literal was counted against its "why" cell or parenthesis (length and digit); every block
  time was recomputed against FR-008. No difference.

## Notes

### MINOR-1: The Conventions that give several step phrases their meaning are not in the feature file, and the translation report does not mention them
- **Where**: accounts-and-access.feature lines 1-2 (Feature line, no description or comment). Steps that
  depend on a convention: "sign-in ... fails" (lines 42, 93, 270, 287, 360, 513, 524) and the Given lines
  "sign-ins ... failed at ..." (for example lines 137, 144, 221); the bare word `absent` (lines 377-379, 418,
  436, 443, 472) beside `""` and `"   "`; "the AC database is reset" (lines 465, 478, 490, 499, 506, 519);
  "`x` registered with the password `p`" and "`x` is signed in with `p`" (who acts afterwards); every step
  without a time.
- **Against**: acceptance.md Conventions, lines 5-27 (in particular lines 15-16: a failed sign-in "always
  means the result is "Email or password is not correct."", also in Given lines; lines 17-18: steps without a
  time may use any current time; lines 19-20: `absent` versus `""` versus `"   "`; lines 21-25: what the
  reset is; lines 10-14: who the acting user is). Review item 7 (dropped meaning). ac-test-writer.md, "What
  you can see": "The `.feature` files are what you bind; the acceptance criteria are only context", and
  "Rules": "Expected values must be literals from the Gherkin or the content of a named golden sample file".
  translation-report.md "Interpretations" (lines 77-97).
- **Reasoning**: The item texts were carried over word for word, but the block that defines their phrases
  was not, in any form. Inside the feature file a step such as line 42 (`sign-in with <email> and "walking42"
  fails`) names no expected result; the text it must be checked against exists only in the Conventions of
  acceptance.md (and, by inference, in FR-007). The same holds for the difference between `absent` and `""`,
  for what "reset" does, and for which account acts after a "registered" or "signed in" Given. The report's
  Interpretations list every syntax change but say nothing about the Conventions, so the omission is not
  recorded as a decision. The test writer does receive acceptance.md as context, so the meaning is still
  reachable and this does not block the next step; it means the file that is bound and locked is not
  self-contained, and the expected value of seven "fails" steps is not a literal of the Gherkin.

### MINOR-2: "no account has the email ..." is a precondition that the AC surface cannot establish or check
- **Where**: accounts-and-access.feature lines 6, 75, 82, 123, 135, 220, 342, 349, 450 (AC-001, AC-006,
  AC-061, AC-012, AC-014, AC-025, AC-039, AC-040, AC-053).
- **Against**: spec.md "What acceptance tests can reach", lines 110-117 (the use cases on the AC surface:
  registration, sign-in, password change, account details, administrator creation, demo loading) and
  lines 124-125 ("nothing in this slice removes an account"); FR-007 (lines 472-473: an unknown email and a
  wrong password give the same result); ADR 0003 "Through the use cases only" (docs/adr/0003, lines 38-39);
  ac-test-writer.md "Rules" ("Not allowed: empty step bodies"; a step that cannot be expressed through the
  public API becomes `PendingStepException("UNEXPRESSIBLE: ...")`); the extension's reqnroll.json template
  (`missingOrPendingStepsOutcome: Error`). Review items 5 and 6.
- **Reasoning**: None of the six use cases reports whether an email has an account without changing state:
  registration would create it, and sign-in is required by FR-007 to answer the same for "no account" and
  "wrong password". The step is true only by construction: the run starts with an empty schema (ADR 0003,
  lines 30-31) and each valid email belongs to one item (acceptance.md lines 26-27, a convention that is not
  in the feature file, see MINOR-1). The step text does not say that it is a statement about the test data
  and not an action or a check, so its binding is left to the writer, whose rules offer a body that verifies
  nothing or a pending step; with pending steps reported as errors the second choice would fail nine
  scenarios, among them the main success paths AC-001 and AC-039. This was checked at the level of the spec
  and ADR 0003 only; the public API listings are outside this review. The wording comes unchanged from the
  locked acceptance.md, so it is not a translation fault.

### MINOR-3: Administrator creation with an email in mixed case or with spaces is exercised only up to "administrator already exists"
- **Where**: accounts-and-access.feature line 459 (AC-054), the only administrator-creation step whose email
  is not already trimmed and lower-case; lines 241, 257, 333, 343, 350, 356, 411, 435, 449, 451, 458 (every
  run that gives "administrator created" uses a lower-case email without spaces); lines 365, 429, 443 (every
  run against a registered user's email uses the exact stored form).
- **Against**: spec.md FR-003, lines 457-463 ("An account keeps its email in that trimmed, lower-case form,
  and account details (FR-012) return it"); Edge Cases, lines 431-432 (the same email "for registration,
  sign-in, administrator creation and demo loading"); FR-014 steps 4 and 6 (lines 505-508). Review item 1
  (partly covered).
- **Reasoning**: For registration the stored form is pinned by AC-006 and AC-061. For administrator creation
  no scenario creates an administrator from an email such as ` Case.Admin@Example.com ` and then reads the
  account details or signs in with the lower-case form, and no scenario reaches step 4 ("email belongs to a
  registered user") with a differently cased or spaced email. An implementation that normalises the email
  only for the administrator lookup of step 3, stores a new administrator's email as configured, or compares
  the registered-user email of step 4 as typed, passes all 61 scenarios and breaks FR-003 and the edge case.
  The gap is in the item set of acceptance.md, which the feature file reproduces exactly; the feature file
  adds nothing to it and removes nothing from it.

## Summary

0 MAJOR, 3 MINOR.

The feature file is a faithful translation: all 61 items of the locked acceptance.md have exactly one
scenario with the same id, tags, title, steps and values, the 10 items with Examples are Scenario Outlines
with the same rows, the empty text, the three-space text and `absent` are kept distinct, and no scenario,
step, value or assertion was added. The structure meets the extension's rules for feature files, and the
locked source hashes match the files that were translated. Against spec.md, every FR, every non-page
acceptance scenario and every application-level edge case has a scenario; the 29 scenarios without one are
the spec's *(page)* scenarios, listed as not testable in acceptance.md. The three notes do not concern the
translation itself: the Conventions of acceptance.md are needed to read several steps and are absent from the
bound file, one Given phrase cannot be verified through the AC surface and leaves its binding to the test
writer, and one corner of FR-003 for administrator creation is not pinned by the item set. None of them
contradicts the spec or blocks locking the feature file and writing the tests.
