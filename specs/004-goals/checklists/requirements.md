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
- Iteration 3 (2026-11-17, `/speckit-clarify`): D-1 to D-5 are decided (attempt 1 from the start; count the
  times; the first full week, told with no confirmation; name, greeting and date; a Notice and no new goals on
  an Unpublished habit). The defaults the iteration 1 notes describe for FR-004, FR-013, FR-016 and HP-010 are
  replaced by those answers; no decision is open.
- Iteration 4 (2026-11-17, after [critic spec pass 1](../critic-spec-pass-1.md)): the duplicate FR-030 is
  resolved (the catalog requirements are now FR-031 to FR-035) and FR-019 is added. Before that fix, "All
  functional requirements have clear acceptance criteria" did not hold, although it was ticked. The slice 2
  rules the scenarios depend on are now restated, so the spec stands alone for `/speckit-ac-translate`.
  `CONTEXT.md` gains this slice's terms. All items pass.
- Iteration 5 (2026-11-17, after [critic spec pass 2](../critic-spec-pass-2.md)): a time zone change is
  measured on each goal's own timeline (FR-012, FR-016, FR-026). The demo goals no longer use the custom
  habits that slice 2's locked AC-066 deletes, and demo loading loads the seed catalog first (FR-040). The
  items pass, with one condition outside this list: the defaults under "Critic revision 2026-11-17" in
  `## Clarifications` await the owner's confirmation before `/speckit-ac-translate`. Until then they are open
  decisions.
- Iteration 6 (2026-11-17, MINOR notes of [critic spec pass 3](../critic-spec-pass-3.md), which approved): the
  open defaults now carry one `[NEEDS CLARIFICATION]` marker (constitution, Decision Discipline), so "No
  [NEEDS CLARIFICATION] markers remain" fails until the owner answers it. All other items pass.
- Iteration 7 (2026-11-17): the owner confirmed all defaults of "Critic revision 2026-11-17"; the answer is
  recorded in `## Clarifications` and the marker is removed. All items pass; no decision is open.
