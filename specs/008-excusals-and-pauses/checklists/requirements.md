# Specification Quality Checklist: Excusals and pauses

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-21
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

- Iteration 1 (2026-11-21): three open questions remain as markers: D-1 (SPEC §11.2, FR-007), D-2 (SPEC §11.3,
  FR-002 step 8) and D-3 (excusing a commitment with a record, FR-002 step 9). The constitution's Decision
  Discipline requires the two SPEC §11 markers; they are answered by the owner, not guessed. D-4 (where the demo
  excusals and pauses live) is a technical decision with a recommended answer, not a marker; it needs an ADR.
- Iteration 2 (2026-11-21): the owner answered D-1 (A: a partly paused week is Paused as a whole), D-2 (A: excuse
  only while Due or Pending) and D-3 (A: a commitment with a record is refused). Markers replaced in FR-002,
  FR-007, FR-009, FR-010, US1-AS10, US2-AS12 and two edge cases; answers recorded in `## Clarifications`;
  `CONTEXT.md` (Excused sharpened; Pause and Paused added; Consistency, Streak, Recent results, Trend, Dashboard)
  and the roadmap's two SPEC §11 rows updated. All items pass. D-4 was left for `/speckit-clarify` (iteration 3).
- Iteration 3 (2026-11-21, `/speckit-clarify`): the owner answered D-4 (A: a fifth startup task, demo pause loading,
  creating `demo6@example.com`; ADR 0023), early resume (A: the current date is expected again; FR-010) and the
  excusal-removal window (A: only while the grace period is open; FR-004). No markers remain; all items pass.
- Iteration 4 (2026-11-21, after critic spec pass 1): D-1 to D-3 recorded in ADR 0024 (Decision Discipline);
  the record checks (4a) to (4c) ordered in FR-005; the Paused week of a goal that is not paused described (Edge
  Cases, FR-018); exempt results in recent results, history, recent notes (FR-020) and the record result's state
  fixed; the yes/no week value (FR-016); HP-001's History actions; "no time of its own" for excusals and pauses
  (FR-003, FR-025, FR-031, ADR 0023); `CONTEXT.md` Excusal added and Evaluation state sharpened; the missing
  inputs of US1, US2-AS11, US2-AS12, US3, US3-AS6, US3-AS7 and US4 supplied; US3-AS4's bars listed in full. All
  items pass.
- Iteration 5 (2026-11-21, after critic spec pass 2, APPROVE with 5 MINOR notes, applied at the owner's request):
  SC-001 matched to the page actions; SPEC §5.3's "failures already recorded" read as final results (FR-008, ADR
  0024); a yes/no week whose only records sit on Paused commitments has no value (FR-016); slice 5 FR-010, FR-018
  and `RecordDialogTests` added to the impact list; ADR 0024 also records the early-resume and excusal-removal
  answers with their options. All items pass.
- Iteration 6 (2026-11-21, while drafting `acceptance.md`): the password rule, the seed catalog and the
  unpublish-with-confirmation rule restated in "Earlier behaviour these tests use", worded as in slice 6's spec,
  so that the demo acceptance criteria follow from this spec alone. All items pass.
- Implementation details: the FR section and Success Criteria stay technology-agnostic. As in slices 1 to 6, the
  "Host, page and integration requirements", "Earlier requirements and tests this slice changes" and "Design
  changes before build" sections name design classes, routes, test files and ADRs on purpose: the project keeps
  page fidelity (constitution IV) and test impact in the spec. This is the repo's accepted convention, not a leak.
- Date arithmetic of every worked example (US1 to US4, FR-031) was checked against slice 5 FR-016 (grace period
  of 48 elapsed hours from the end instant) and slice 6's consistency and streak rules.
- Items marked incomplete require spec updates before `/speckit-clarify` or `/speckit-plan`.
