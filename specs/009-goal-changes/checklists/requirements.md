# Specification Quality Checklist: Goal changes

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-22
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

- Items marked incomplete require spec updates before `/speckit-clarify` or `/speckit-plan`
- Iteration 1 (2026-11-22): two open questions remain as markers. D-1 (SPEC §11.7, second part, FR-001) is the
  roadmap's product row for this slice. The constitution's Decision Discipline requires it as a marker, so the
  owner answers it rather than the spec guessing. D-2 (which results belong to an ended attempt, FR-008)
  decides whether ending can create a failure (SPEC §5.4). Scenario figures that depend on D-1 or D-2 are tagged
  and show the recommended answer. D-3 (where the demo goal changes live) is a technical decision with a
  recommended answer (a sixth startup task and a new ADR, following ADR 0023), not a marker. It goes to
  `/speckit-clarify`.
- Iteration 2 (2026-11-22): the owner answered D-1 (A: a change keeps the schedule kind; the streak runs across
  versions) and D-2 (B: the commitment or period in progress leaves the ended attempt; Pending ones finalize as
  usual). Markers replaced in FR-001 and FR-008; answers recorded in `## Clarifications` and the decisions
  section, with the rejected options; both recorded in ADR 0025 (Decision Discipline: they reject real
  alternatives, and D-1 binds slice 9). `CONTEXT.md` gained Ended goal, Outcome and Settings change, and Attempt,
  Goal settings version, Streak, Report range and Dashboard were sharpened; the demo entries wait for D-3. The
  roadmap's SPEC §11.7 row is decided. All items pass. D-3 is left for `/speckit-clarify`.
- As in earlier slices, page and file names in HP requirements (`design/...`, routes, `tools/HT.SmokeCheck`) sit
  outside the AC surface and name the repo's design sources. They are not implementation choices.
- Demo figures of FR-031 and the scenario figures were recomputed by hand (counts, rounding down, streak runs,
  grace-period ends).
