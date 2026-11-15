# Specification Quality Checklist: Demo hosting

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-15
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

- [ ] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- Iteration 1 (`/speckit-specify`, 2026-11-15): "EF Core migrations" in US3 and HP-004 replaced with "schema
  migrations". Remaining technology references are project facts the spec cites, not choices it makes: Azure
  (fixed by SPEC §10 and ARCHITECTURE §2), ADR numbers, `master`, `example.com`.
- Iteration 2 (`/speckit-specify`, 2026-11-15): three markers (demo sign-in channel, reviewer administrator
  access, demo refresh timing) answered by the owner and recorded in `## Clarifications`.
- Iteration 3 (`/speckit-clarify`, 2026-11-15): five more answers (Operator-started deployments, reachable
  until 2026-12-31, slow first page after idling, R2 hosting at the end, latest start date) recorded.
- Iteration 4 (critic pass 1, 2026-11-15): spec revised; decisions listed under "Critic revision 2026-11-15".
  Three items are unchecked on purpose:
  - Markers: four `[NEEDS CLARIFICATION: ARCHITECTURE §9]` markers in "Decisions due before `/speckit-plan`"
    are required by the constitution (Decision Discipline) for the deferred hosting choices. They are decided
    in the hosting ADR when the slice resumes; the plan's Constitution Check fails until then.
  - Measurable, and meets measurable outcomes: SC-007 has no number until the hosting ADR records the budget.
  - Acceptance criteria: HP-012, HP-013 and HP-014 have no acceptance scenario; they are checked by review of
    the deployment definitions and configuration and by SC-004 and SC-007, not by a scenario.
- Iteration 5 (critic pass 2 MINOR notes, 2026-11-15): the author's decisions are marked as pending owner
  confirmation and listed first under "Decisions due before `/speckit-plan`"; the success rule covers every
  shipped slice's demo data; failure wording aligned; US4-AS5 and an edge case cover a failed refresh; US3-AS3
  covers the operator log not being exposed; "Deployment" added to `CONTEXT.md` and Operator sharpened; HP-015
  keeps the administrator email out of the repository; HP-018 cites slice 1's session rules; the sign-in block
  is named and lasts 5 minutes; roadmap rows for hosting and refresh updated.
- Iteration 6 (`/speckit-plan`, 2026-11-15): the owner confirmed the critic-revision decisions and decided the
  four ARCHITECTURE §9 items (ADR 0015); the markers are gone and SC-007 has its number (USD 25 a month). Only
  "All functional requirements have clear acceptance criteria" stays unchecked, for the reason in iteration 4.
- Requirements use HP-NNN, not FR-NNN: the slice adds no Application behaviour, so nothing is on the
  acceptance-test surface (ADR 0003, spec "What acceptance tests can reach").
