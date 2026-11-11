# Critic pass 2: ac for 001-accounts-and-access

**Verdict**: APPROVE
**Date**: 2026-11-11
**Reviewed**: specs/001-accounts-and-access/acceptance.md (60 items, AC-001 to AC-060; Conventions, Coverage
table, "Open points for the spec"). Golden samples: none (no item names one;
`tests/HT.AcceptanceTests/Samples/F001` does not exist).

Checked against: specs/001-accounts-and-access/spec.md, .specify/memory/constitution.md, SPEC.md,
docs/adr/0003-acceptance-tests.md, and the format rules of
.specify/extensions/ac/templates/acceptance-template.md and
.specify/extensions/ac/scripts/check_acceptance.py (read, not run). Mechanical checks done by reading the two
files with a read-only script: item headings against the Coverage table, spec scenarios against Coverage rows,
email reuse across items, and the length and digit content of every password literal.

## Previous notes: resolution status

| Previous note | Status (Resolved / Partly / Unresolved) | Evidence |
|---|---|---|
| MAJOR-1: demo edge case has no item; per-run versus per-email result not told apart | Resolved | AC-060 (lines 655-664) starts from a reset database, registers ` Demo2@Example.com ` with `owner2026`, and expects "created" / "already exists" / "created" with the owner's password kept. A once-per-run rule fails it. It also covers the demo-loading half of the normalised-email edge case (spec.md lines 425-426) and the edge case of spec.md lines 429-430. |
| MINOR-1: "same result" checked only as equal text | Resolved | AC-014 (lines 259-265) compares the three causes in one item and requires "the same named outcome, with the text". Matches FR-007 and SC-004 (spec.md lines 465-466, 619-620) and the named-outcome answer (spec.md lines 83-87). |
| MINOR-2 (a): no item with failures far apart | Resolved | AC-021 (lines 313-318): five failures one hour apart, still blocked at 12:04:59. A time-window rule fails it. |
| MINOR-2 (b): count across a block not discriminated | Resolved | AC-022 (lines 320-327): one failure during the block plus four after it, sign-in at 10:06:00 succeeds. AC-023 (lines 329-335): five failures during the block, sign-in at 10:05:00 succeeds. An implementation that counts attempts during the block fails both. All times were recomputed against FR-008 (spec.md lines 467-471). |
| MINOR-3 (a): two readings of "two non-empty parts" not separated | Partly | Added rows `ana@.example` and `ana@example.` (lines 184-185) close the "one non-empty part plus one empty part" gap. Domains with an empty part next to two non-empty parts are not covered; the author lists them under "Open points for the spec" (lines 123-125). The rationale is sound, see MINOR-1 below. |
| MINOR-3 (b): empty and spaces-only email in registration | Resolved | AC-004 rows `""` and `"   "` (lines 189-190), with the Convention on lines 19-20 that separates them from "absent" in administrator creation. |
| MINOR-3 (c): the email that account details return is never observed for an account registered with spaces or mixed case | Partly | Not covered; listed under "Open points for the spec" (lines 126-128). The rationale holds for letter case and not for surrounding spaces, see MINOR-2 below. |
| MINOR-4: FR-002 bounds uneven across the four places | Resolved | 8 characters with a digit accepted in password change (AC-032, `running4`), administrator creation (AC-040, `curator1`) and demo loading (AC-058, `example1`); 7 characters with a digit refused in AC-034 (`runnin4`), AC-046 (`curat42`) and AC-056 (`exampl1`). Lengths verified by count. |
| MINOR-5 (a): anonymous visitor with a weak new password | Resolved | AC-036 Examples (lines 443-448) add the new password `run`; the result stays "You are not signed in." |
| MINOR-5 (b): missing password before an existing account | Resolved | AC-051 (existing administrator, lines 567-572) and AC-052 (existing registered user, lines 574-579) expect "no administrator configured". Every pair of FR-014 steps that can both apply to one input now has an item (1/2 AC-048, 1/3 AC-051, 1/4 AC-052, 1/5 AC-043, 2/5 AC-049, 3/5 AC-047, 4/5 AC-050). |
| MINOR-6: one item with six actions and five results | Resolved | Split into AC-055 to AC-060, each with one When. Each starts with "the AC database is reset"; ADR 0003 (lines 38-39) lets step definitions arrange through the two `HT.Persistence` entry points, and the reset entry point is the one of ADR 0003 lines 44-47. The successful runs in AC-057 to AC-060 all carry "names no other email". |
| MINOR-7: Coverage table and headings disagree | Resolved | All 60 headings were compared with the 84 Coverage rows (16 FR, 68 scenarios): no difference. 39 scenarios have items; the 29 without items are exactly the spec's *(page)* scenarios. |
| MINOR-8: undefined step phrasings | Resolved | Conventions lines 10-16 define the three sign-in phrasings, the two account-details phrasings, and failed sign-ins in Given lines. No item has an untimed sign-in on an account that already has timed failures, so "Steps without a time may use any current time" (lines 17-18) changes no result. |

Only one earlier pass exists for this stage, so there is no older note that could have regressed.

## Notes

### MINOR-1: FR-003's verdict for a domain with an empty part beside two non-empty ones stays unpinned and is recorded only in acceptance.md
- **Where**: acceptance.md "Open points for the spec", lines 123-125; AC-004 Examples (lines 178-190); AC-005
  (lines 199-204).
- **Against**: spec.md FR-003 (lines 451-453) and the clarification on lines 59-61 ("a domain of at least two
  non-empty parts separated by dots"); constitution "Decision Discipline" (lines 138-140: the owner's answer
  is recorded in the spec's `## Clarifications` with its date); review rule 7 (boundaries are discriminated).
- **Continues**: pass-1 MINOR-3 (a).
- **Reasoning**: The author's rationale is sound. The phrase can be read as "every part is non-empty" or as
  "at least two of the parts are non-empty"; the spec's own examples (`ana@`, `ana@example`, `ana@.`) are
  invalid under both, and the acceptance criteria may not choose between them. The claim "Items test only
  domains whose verdict is the same under both readings" was checked row by row for AC-004 and AC-005 and is
  true. What remains: `ana@example..com`, `ana@.example.com` and `ana@example.com.` have no expected result,
  so two implementations with opposite verdicts both pass all 60 items, and the implementation will have to
  take one of the verdicts. The open point is written only in acceptance.md; spec.md is unchanged and has no
  clarification entry or marker for it, so nothing in the spec shows that this part of FR-003 is undecided.

### MINOR-2: The second open point treats the surrounding spaces as undecided, although FR-003 decides them; the returned email of an account registered with spaces is checked by no item
- **Where**: acceptance.md "Open points for the spec", lines 126-128; AC-006 (lines 206-211); AC-060 (lines
  655-664).
- **Against**: spec.md FR-003 (lines 451-453): "treat an email as its trimmed, case-insensitive form and
  accept it only if it has no spaces"; FR-012 (lines 482-483): account details are "email and role"; HP-014
  (lines 555-556): the avatar shows "the first letter of the email"; Edge Cases (lines 425-426).
- **Continues**: pass-1 MINOR-3 (c).
- **Reasoning**: The rationale is sound for letter case: the spec says a mixed-case email "is the same email
  as" its lower-case form, which is a statement about identity, and nothing says whether account details
  show `Mixed.Case@Example.com` or `mixed.case@example.com`. It is not sound for the surrounding spaces. Under
  FR-003 an accepted email has no spaces and is treated as its trimmed form, so the spaces typed around it
  are not part of the account's email, and FR-012 returns that email; HP-014 also takes the first letter of
  the email, which presumes no leading space. The open point's sentence "FR-012 does not say which form
  account details return for an account registered with spaces" is therefore inaccurate for that half. The
  effect: the only accounts registered with surrounding spaces are those of AC-006 and AC-060, and both
  items only sign in afterwards. An implementation that stores and returns the email as typed, spaces
  included, passes all 60 items and breaks FR-003 together with FR-012.

## Summary

0 MAJOR, 2 MINOR.

The revision resolves the blocking note of pass 1 and all but two sub-points of its MINOR notes. The document
is complete in form: 60 items, every one of the 16 FR has items, all 68 spec scenarios are covered (39) or
listed as not testable with a reason that matches the spec's *(page)* marks (29), the Coverage table agrees
with the headings, every result text matches spec.md word for word, every valid email outside the demo items
is used by one item and one Examples row only, and the demo items are independent of scenario order through
the reset entry point that ADR 0003 allows. The new block items, check-order items and lower-bound items
were recomputed and each fails for the wrong implementation it targets. The two remaining notes both concern
the "Open points for the spec" section: the first open point is a real gap in spec.md that the acceptance
criteria correctly do not decide, but it is tracked nowhere in the spec; the second mixes an undecided
question (letter case of the returned email) with one that FR-003 already answers (surrounding spaces), which
leaves that answer untested. Neither contradicts the spec or blocks locking and translation.
