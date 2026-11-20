# Specification Quality Checklist: Personal reports

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- Iteration 1 (2026-11-20): three open questions are marked [NEEDS CLARIFICATION] (D-1 Pending in the current
  streak, SPEC §11.7; D-2 upcoming commitments on the dashboard, SPEC §6; D-3 the visitor Home's example card,
  privacy). D-4 (caching, ARCHITECTURE §9) and roadmap decision R3 are asked in conversation, as the roadmap's
  "Decisions each slice must settle" allows beyond the 3-marker limit.
- "No implementation details": the spec names pages, design components and `ht-` classes in its HP section, as
  slices 1 to 5 do, because constitution IV makes `design/` the source of truth for the UI. The FR section, which
  the acceptance criteria are drafted from, names no technology.
- Iteration 2 (2026-11-20): the owner answered D-1 to D-4 during `/speckit-specify`, all as recommended. The
  markers are replaced and the answers are recorded in `## Clarifications`. All items pass. D-4 (no caching) is
  recorded in ADR 0020. Roadmap decision R3 was answered "Yes" on 2026-11-20 and recorded in the spec and the
  roadmap.
- FR-030's demo figures follow D-1 (Pending outside every streak).
- Iteration 3 (2026-11-20, after `/speckit-clarify`): five answers recorded in `## Clarifications` (rounding down,
  "Missed this week", the report range, one trend bar per commitment for count and duration goals, and D-5: demo
  report loading with `demo5@example.com`, FR-032, HP-011, ADR 0021). All items re-checked against that content;
  all pass.
- Iteration 4 (2026-11-20, after critic pass 1, `critic-spec-pass-1.md`): `demo5`'s Screen time has no record on
  R − 3, so a result is Missed this week on every weekday (MAJOR-1; figures "93% (77 of 82)", streak 0, longest
  34); FR-006 defines unrecorded periods, Reduce yes/no bars and minutes for the target; FR-012 gives tie-breaks;
  the Friday boundary of "Missed this week" is stated; the first tile reads "Due now"; SC-006 and SC-007 name how
  they are measured (SC-007 by an integration test, not on the hosted demo); the earlier tests that change are
  listed; HP-011 gives the refusal log line; `CONTEXT.md` gets the terms of HP-010 in this change. All items
  pass.
- Iteration 5 (2026-11-20, after critic pass 2, `critic-spec-pass-2.md`): FR-006 separates bars that are amounts
  (minutes apply) from bars that count records (a Build count or duration period keeps slice 5's progress against
  N, "Target 3"), and says when a bar begins; HP-001's target label follows; SC-006 says what the smoke check
  confirms and what it does not; the smoke-check and startup-fault tests that change are listed; FR-032 states
  both clock-change cases. All items pass.
- Iteration 6 (2026-11-20, after critic pass 3, `critic-spec-pass-3.md`, APPROVE with 2 MINOR notes, applied at
  the owner's request): `CoverageTests` is listed among the tests that change; `CONTEXT.md`'s Recent results entry
  and the TrendChart design change say "recorded progress" instead of "amounts". All items pass.
