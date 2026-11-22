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
- Iteration 3 (2026-11-22, `/speckit-clarify`): the owner answered D-3 (A: a sixth startup task, demo goal-change
  loading, creating `demo7@example.com`; ADR 0026), the earlier attempt's History (B: an attempt view with figures,
  strip and correctable History; FR-014, HP-014), an ended goal's Pending results on Home (B: kept, marked "Ended",
  until their grace period ends; FR-019, HP-007) and the name of an earlier attempt (A: named by its own last
  settings; FR-004, FR-015). `CONTEXT.md` gained the demo entries, and Attempt, Dashboard and Ended goal were
  sharpened. No markers remain; all items pass.
- Iteration 4 (2026-11-22, `/speckit-ac-critic spec`, pass 1): 2 MAJOR and 13 MINOR notes, all verified and
  confirmed (MINOR-9 as an accepted consequence). A new attempt now starts after the ending date (new message
  "A new attempt starts after the day the goal ended."; FR-011, FR-012, ADR 0025), which removes the double date
  and the mismatched records of a same-day restart. `CONTEXT.md` gained Goal history and sharpened Attempt and
  Dashboard. Goal-level dates for several attempts, earlier attempts' Pending results, the inputs of reading one
  attempt, the goal history's "latest" flag, the catalog details' goal list, a waiting change in the demo, the
  page states and a restart on an Unpublished habit are now specified; US1-AS14 and US3-AS2 were fixed and
  US3-AS12 added (the page scenarios are now AS13 and AS14). All items still pass.
- Iteration 5 (2026-11-22, `/speckit-ac-critic spec`, pass 2): 2 MAJOR and 5 MINOR notes, verified by the author
  and all confirmed. Ending keeps the attempt's first version (FR-009, US2-AS12). The current date is extended
  for ended goals and waiting new attempts, so an earlier attempt's dates stay recordable ("Terms used below",
  US3-AS4); "a goal that has not started" keeps slice 4's meaning. US3-AS6 has its time, the page scenario
  gives FR-011's start date, attempts with no date get their own text, every ended attempt's Pending entries
  stay on the dashboard, ADR 0025 matches FR-009, and the author's restart rule is marked for the owner's
  review. A time-zone scenario was added (US3-AS13; the page scenarios are now AS14 and AS15). All items
  still pass.
- Iteration 6 (2026-11-22, `/speckit-ac-critic spec`, pass 3: APPROVE with 5 MINOR notes, applied on the owner's
  request after verification): a waiting attempt's dates are not recordable before their own day begins
  ("Current date"); FR-005 gives no later version for a waiting attempt; earlier attempts' dashboard and
  record-now entries carry their own name and Build or Reduce (FR-016, FR-019); US3-AS13 and US3-AS14 reworded;
  the two author consequences of D-2 are marked in the spec and ADR 0025 as awaiting the owner's confirmation
  before the acceptance criteria are locked (open item).
- Iteration 7 (2026-11-22): the owner confirmed both consequences of D-2. They are recorded in `## Clarifications`
  (Session 2026-11-22 (spec review)), the decisions section and ADR 0025. No open item remains.
- As in earlier slices, page and file names in HP requirements (`design/...`, routes, `tools/HT.SmokeCheck`) sit
  outside the AC surface and name the repo's design sources. They are not implementation choices.
- Demo figures of FR-031 and the scenario figures were recomputed by hand (counts, rounding down, streak runs,
  grace-period ends).
