# Specification Quality Checklist: Goals

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-11-17
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

- Iteration 1 (2026-11-17): 3 `[NEEDS CLARIFICATION]` markers remain: FR-011 time zones (SPEC §11.1,
  ARCHITECTURE §9; the constitution requires the marker), FR-017 roadmap decision R4 (goal changes here or in
  slice 8), FR-021 administrators' own goals. They are the roadmap's "do not guess" rows with the largest
  scope and privacy impact, and are put to the owner.
- Five further open decisions (D-1 first attempt, D-2 period-based target meaning, D-3 first week of a weekly
  goal, D-4 account name and Home date, D-5 goal on an Unpublished habit) carry a stated default in "Decisions
  due before acceptance criteria" and go to `/speckit-clarify`.
- References to `design/` files, page addresses, `ht-` classes, ADRs, "use cases" and the AC surface follow
  this repo's convention from slices 1 and 2: they name the project's own sources of truth and test boundary,
  not a technology choice.
- "Requirements are testable": FR-004 (period-based), FR-013 (first week), FR-016 (attempt) and HP-010 state
  their default and name the open decision; acceptance criteria are drafted only after those decisions are
  recorded.
- Iteration 2 (2026-11-17): the owner answered Q2 A (goal changes go to slice 8) and Q3 A (no goals for
  administrators). On Q1 the owner asked why the time zone is bound to a goal; the answer separated where the
  zone is chosen (the profile) from what a goal keeps for its history (the zone in effect, in its settings
  version). The owner then chose B1: the account's own time zone, taken from the browser at registration and
  editable in Profile. The markers are replaced (FR-011, FR-017, FR-021); FR-024 to FR-027, US10, HP-015 and
  HP-016 are added; the answers are recorded in `## Clarifications` with ADR 0016 (Proposed). All items pass.
  D-1 to D-5 remain for `/speckit-clarify`.
