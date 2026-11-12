# Critic pass 1: ac for 001-accounts-and-access

**Verdict**: REVISE
**Date**: 2026-11-11
**Reviewed**: specs/001-accounts-and-access/acceptance.md (47 items, AC-001 to AC-047). Golden samples: none
(no item names one; `tests/HT.AcceptanceTests/Samples/F001` does not exist).

Checked against: specs/001-accounts-and-access/spec.md, .specify/memory/constitution.md, SPEC.md,
docs/adr/0003-acceptance-tests.md, and the format rules of
.specify/extensions/ac/templates/acceptance-template.md.

## Notes

### MAJOR-1: The spec's demo edge case (a demo email that already has an account before demo loading) has no item, and AC-047 cannot tell a per-email result from a per-run result
- **Where**: acceptance.md AC-047 (lines 509-532); Coverage table (lines 23-108) has no row for it.
- **Against**: spec.md Edge Cases, line 429-430: "A demo email that someone registered before demo loading ran
  gets "already exists" and keeps its owner's password (FR-016)."; spec.md line 425-426 (an email in mixed case
  or with spaces is the same email "for registration, sign-in, administrator creation and demo loading");
  FR-016 (lines 510-515): "for each of the demo emails ... the result is "created" ... or "already exists"".
- **Reasoning**: In AC-047 the three demo emails always share one state: none exists before the first
  successful run (line 514, 523-524), all three exist before the second (line 527-528). No step has one demo
  email with an account while the others have none. So the per-email rule of FR-016 is never exercised with
  a mixed outcome. An implementation that decides once per run ("if any demo account exists, report all as
  already existing and create nothing", the common seed-once pattern) passes every step of AC-047 and breaks
  FR-016 and the edge case: with one demo email registered earlier, the other two accounts would never be
  created. The edge case is application behaviour (it cites FR-016, and registration and demo loading are both
  on the AC surface per spec.md lines 108-111), so it is not excused as a page scenario, and the document does
  not list it as not testable. The demo-loading half of the normalised-email edge case (line 425-426: a demo
  email registered earlier in another case counts as that demo account) is not exercised either. The
  convention on line 17-18 ("the demo emails are used only by AC-047") means no other item can cover it.

### MINOR-1: FR-007 / SC-004 "same result" is checked only as equal message text
- **Where**: acceptance.md AC-012 (line 228), AC-013 (line 234), AC-014 (line 241), AC-021 (line 292).
- **Against**: spec.md FR-007 (lines 465-466): "Every failed sign-in MUST give the same result"; SC-004
  (lines 619-620): "the result is identical for an unknown email, a wrong password and a blocked account";
  spec.md Clarifications lines 83-87: every refusal "has a named outcome and carries the literal text".
- **Reasoning**: A result has two parts, a named outcome and its text. Each of the four items compares only
  the text "Email or password is not correct." in its own scenario; no item states that the three causes give
  the same result. An implementation with three named outcomes (unknown email, wrong password, blocked) that
  all carry this text satisfies every item and still breaks FR-007 and SC-004, and it exposes on the
  application surface exactly what the block must not reveal (spec.md lines 49-52).

### MINOR-2: FR-008 examples leave two plausible wrong block rules undetected
- **Where**: acceptance.md AC-014 to AC-021 (lines 236-295), in particular AC-016 (lines 250-256) and AC-017
  (lines 258-264).
- **Against**: spec.md FR-008 (lines 467-471): "After 5 failed sign-ins in a row ... attempts do not extend the
  block. A successful sign-in, and the end of a block, reset the count to 0."; review rule 7 (boundaries and
  edge cases are discriminated).
- **Reasoning**: (a) Every series of five failures lies within 60 seconds (09:59:00 to 10:00:00, lines 239,
  246, 253, 261, 285). FR-008 has no time window: five failures in a row block, however far apart. A rule
  that counts only failures inside a window (for example the last 5 minutes) passes all items. (b) AC-016 has
  one failed attempt during the block and no failures after it; AC-017 has four failures after the block and
  none during it. An implementation that sets the count to 0 when the block starts and keeps counting failed
  attempts during the block passes both (count 1 in AC-016, count 4 in AC-017). It breaks FR-008 in two ways
  that no item reaches: one failure during the block plus four after it would block again, and five failures
  during the block would start a new block, which extends it.

### MINOR-3: FR-003 email examples do not separate the two readings of "two non-empty parts", omit the empty email, and never observe the stored form
- **Where**: acceptance.md AC-004 Examples (lines 157-167), AC-005 (lines 176-181), AC-006 (lines 183-188).
- **Against**: spec.md FR-003 (lines 451-453): "a domain of at least two non-empty parts separated by dots",
  "treat an email as its trimmed, case-insensitive form"; spec.md Edge Cases line 425-426 ("the same email as
  its trimmed, lower-case form"); FR-012 (line 482): account details are "email and role".
- **Reasoning**: (a) The only invalid example with an empty domain part is `ana@.` (no non-empty part). The
  phrase can mean "every part is non-empty and there are at least two" or "at least two of the parts are
  non-empty". The readings differ for a domain with an empty part next to two non-empty ones (a doubled dot,
  or a leading or trailing dot beside two other parts), and no example has that shape. A domain with exactly
  one non-empty part and one empty part (dot first or dot last) is also absent, so a check as weak as
  "the domain contains a dot and is not only a dot" passes all seven rows. (b) No row registers with an empty
  or spaces-only email. FR-003 makes that "Enter a valid email address." for registration, while FR-014 makes
  the same input "no administrator configured" (AC-037); only the second is tested. (c) AC-006 registers
  `  Mixed.Case@Example.com  ` and then only signs in with the lower-case form. No item reads the account
  details of an account registered with spaces or mixed case, so the email that FR-012 returns for it (with
  or without the spaces, as typed or lower-case) is pinned by nothing; AC-010 and AC-011 read details only for
  accounts registered in lower case without spaces.

### MINOR-4: FR-002 bounds are tested unevenly across the four places that set a password
- **Where**: acceptance.md AC-003 (lines 136-148), AC-027 (line 338), AC-029 (lines 360-365), AC-034 (line 403),
  AC-040 (lines 462-467), AC-047 (lines 521-524).
- **Against**: spec.md FR-002 (lines 448-450): "Every password the system sets (registration, password change,
  administrator, demo accounts) MUST have at least 8 characters and contain at least one digit."; review
  rule 7 (off-by-one).
- **Reasoning**: A password of exactly 8 characters with a digit is accepted only in registration (AC-003).
  Every accepted password in the other three places is longer: `running42` (9) for password change,
  `curator2026` (11) for administrator creation, `example2026` (11) for demo loading. An off-by-one in any of
  those three paths (more than 8 instead of at least 8) passes all items. Demo loading has no length example
  at all: its only refused password is `examplepass` (11 characters, no digit), so a demo-loading check that
  tests only for a digit passes AC-047.

### MINOR-5: Two check orders are not discriminated, although every other order is
- **Where**: acceptance.md AC-031 (lines 374-378); AC-037 (lines 424-440) and AC-041 to AC-044 (lines 469-491).
- **Against**: spec.md FR-011 (lines 480-481) with FR-013 (lines 484-487); FR-014 order, steps 1, 3 and 4
  (lines 491-498); review rule 7 (wrong ordering).
- **Reasoning**: (a) AC-031 gives the anonymous visitor a new password that meets the rule (`running42`). An
  implementation that checks the new password's rule before it checks who is acting passes AC-031 and answers
  an anonymous visitor with "At least 8 characters, with a number." for a weak new password, against FR-011.
  (b) For FR-014, AC-042 separates step 1 from step 2 and AC-041, AC-043 and AC-044 separate steps 2, 3 and 4
  from step 5, but every row with a missing password (AC-037, AC-038, AC-042) uses an email that has no
  account. An implementation that looks up an existing administrator or registered user before it checks for
  missing values passes all items and returns "administrator already exists" instead of "no administrator
  configured" for an existing administrator's email with an absent password.

### MINOR-6: AC-047 is one item with six actions and five different results
- **Where**: acceptance.md AC-047 (lines 509-532).
- **Against**: review rule 7 ("each item tests one behaviour"); acceptance-template.md ("One item per
  behaviour", "When [one action with concrete values]"). spec.md lines 118-122 allows the demo scenarios to
  be combined into one sequence, so this is not a contradiction of the spec.
- **Reasoning**: The item carries US7-AS1 to US7-AS4, the FR-002 refusal and all of FR-016, and it is the only
  item for FR-016 (Coverage line 40). A failure in an early step (for example the `"   "` demo password on
  line 519) stops the scenario, so creation, the second run and the kept password are then not checked and
  the result gives one verdict for four spec scenarios. The first run's result (line 524) also lacks the
  "names no other email" condition that the second run has (line 528), so the two runs are checked with
  different strictness.

### MINOR-7: Coverage table and item headings disagree in two places
- **Where**: acceptance.md Coverage line 27 (FR-003) and line 76 (US4-AS5); headings on line 450 (AC-039) and
  line 424 (AC-037).
- **Against**: internal consistency of the artifact; acceptance-template.md (the Coverage table lists the items
  of each requirement and scenario).
- **Reasoning**: AC-039's heading cites FR-003, but the FR-003 row lists AC-004 to AC-009 and AC-046 without
  AC-039. AC-037's heading cites US4-AS5, but the US4-AS5 row lists only AC-038. `check_acceptance.py` reads
  coverage from the headings, not from the table, so it does not catch either mismatch.

### MINOR-8: Step phrasings that the Conventions do not define
- **Where**: acceptance.md Conventions (lines 8-12); AC-001 (lines 117-118), AC-010 (lines 214-215), AC-034
  (lines 405-406), AC-036, AC-038; AC-040 (line 460), AC-045 (lines 499-500), AC-047 (line 525); Given lines
  239, 246, 253-254, 261-262, 269-271, 278, 285 compared with line 292.
- **Against**: review item 6 (underspecified points for the next step); the Conventions' own definitions.
- **Reasoning**: (a) The Conventions make `x` the acting user only for the phrase "`x` is signed in with `p`".
  Several items instead write "sign-in with `x` and `p` succeeds" followed by "account details give ...", and
  others use a third form, "account details for `x` signed in with `p` give ...". Three phrasings stand for
  one action, and two of them are not defined. (b) In the Given lines of AC-014 to AC-020 sign-ins "failed"
  with no message, while the same kind of line in AC-021 adds "each with "Email or password is not
  correct."". Whether a failed sign-in in a Given is checked against the FR-007 message is therefore stated
  for one item and left open for seven.

## Summary

1 MAJOR, 8 MINOR.

The document is complete in form: all 16 FR have items, all 68 spec scenarios are either covered (39) or
listed as not testable with a reason (29) that matches the spec's *(page)* marks, every value is a literal,
the result texts match spec.md word for word, every valid email is used by one item only, and the block
times and counts are placed on both sides of their boundaries. The one blocking gap is in demo loading: the
spec's edge case of a demo email that already has an account is not covered, and AC-047 as written accepts
an implementation that decides "created" or "already exists" once per run instead of per email. The MINOR
notes are discrimination gaps (same result versus same text, the block count across a block, the email rule's
empty parts, password bounds outside registration, two check orders) and small inconsistencies in the
Coverage table and step wording; none of them contradicts the spec.
