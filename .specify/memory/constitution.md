<!--
Sync Impact Report
- Version change: template (unversioned) → 1.0.0 (initial ratification)
- Principles defined:
  - template slot 1 → I. Ubiquitous Domain Language
  - template slot 2 → II. Deterministic, Reproducible Evaluation
  - template slot 3 → III. Privacy Enforced Below the UI
  - template slot 4 → IV. Design-System Fidelity and Accessible UI
  - template slot 5 → V. Verified, Not Asserted
  - added → VI. Demonstrable Vertical Slices
- Added sections: Decision Discipline (slot 2), Workflow and Quality Gates (slot 3), Governance
- Removed sections: none
- Templates: none modified (plan/spec/tasks templates read this file at runtime; the plan template's
  generic "Constitution Check" gate resolves against principles I–VI)
- Deliberately not restated here: everything in CLAUDE.md, SPEC.md, ARCHITECTURE.md
  (stack, layer rules, admin no-bypass, test types, build/test commands, branch/PR workflow)
- Resolved before ratification: feature artifacts live in Spec Kit folders (specs/NNN-<name>/);
  CLAUDE.md, README.md and ARCHITECTURE.md §8 updated, docs/tasks/ is legacy. The Spec Kit install
  (.specify/, .claude/skills/speckit-*) is committed with this file.
- Follow-up TODOs: none
-->

# Habit Tracker Constitution

This constitution adds engineering principles on top of `CLAUDE.md`, `SPEC.md`, and
`ARCHITECTURE.md`. Those files stay binding and are not restated here; nothing in this document
overrides them.

## Core Principles

### I. Ubiquitous Domain Language

- Domain type and member names, UI labels, spec text, test names, and commit messages MUST use the
  vocabulary of SPEC §2 and the `CONTEXT.md` glossary verbatim: catalog habit, custom habit, goal,
  attempt, commitment, evaluation period, Build / Reduce, Pending, Met, Failed, Missed, Excused,
  Paused, consistency, streak, Consistency Bands.
- Synonyms for defined terms MUST NOT appear (e.g. "task" for commitment, "skipped" for Excused,
  "score" or "rank" for a band, "habit" when a goal is meant).
- A change that introduces a new domain term, or sharpens the meaning of an existing one, MUST add
  or update its `CONTEXT.md` entry (one-sentence definition plus a NOT-boundary) in the same change.
  `CONTEXT.md` stays free of implementation detail.

Rationale: the domain is full of near-homonyms (Pending / Missed / Failed / Excused, pause vs.
excusal, habit vs. goal). In this product a wrong word is a wrong rule.

### II. Deterministic, Reproducible Evaluation

- Evaluation, consistency, and streak logic MUST be a deterministic function of recorded activity,
  the goal-rule version in effect for each evaluation, pauses, excusals, and an explicitly supplied
  current time. Domain and Application code MUST NOT read the system clock, local time zone, or
  current culture directly; time enters through `TimeProvider` or a parameter. An architecture test
  SHOULD enforce this.
- Goal-setting changes MUST be retained as versions so that any historical evaluation can be
  recomputed against the rules that applied to it.
- Every derived figure (consistency, current and longest streak, band membership, catalog
  aggregates) MUST be reproducible from stored facts. Correcting a historical record MUST produce
  the same results as if the corrected value had been recorded originally; any stored derivation
  is invalidated by the correction.
- Unit tests for a time- or threshold-based rule MUST cover both sides of each boundary (e.g. just
  inside and just after the 48-hour grace period; at, below, and above a Build or Reduce target).

Rationale: grace periods, corrections, and prospective rule changes make "what was true on that
day" the core of the product. Hidden clock reads and unrecomputable totals make it untestable.

### III. Privacy Enforced Below the UI

- Owner-only rules MUST be enforced in Application use cases. Hiding a control or route in Blazor is
  never the protection. Every use case that reads or changes private data MUST take the acting user
  as input and scope its queries to that owner; lookups of private data by identifier alone MUST NOT
  exist.
- Each such use case MUST have negative tests: another registered user, an administrator, and an
  anonymous visitor are all denied.
- Community views MUST be built from a projection that carries only public display names, band
  membership, aggregate values, and contributor counts. Consent, the 10-evaluation qualification,
  and the 5-contributor minimum MUST be applied in Domain or Application, and below-threshold
  results MUST not leak the hidden value in any form.
- Consent withdrawal and account deactivation MUST take effect on the next read of any community
  view.
- Logs, exception messages, and error pages MUST NOT contain notes, custom-habit names, or recorded
  amounts.

Rationale: SPEC treats privacy as a product promise, not a setting. Controls that live only in
the UI or in reviewer memory fail silently.

### IV. Design-System Fidelity and Accessible UI

- `design/` is the source of truth for the UI. `HT.Ui` MUST use only the `css/tokens.css` variables
  and `ht-` classes: no hard-coded colors, sizes, or shadows, no Bootstrap, no new colors. A needed
  visual change MUST be made in `design/` first (tokens via `tokens.json`), then applied in the app.
- Each design component MUST be implemented once as a Razor component and reused. Pages MUST
  follow the structure and wording of their previews in `design/pages/`.
- User-facing text MUST follow `design/brand-book.md`: plain sentence-case English, buttons that
  start with a verb, no emoji, exclamation marks, or gamified praise. Consistency MUST always render
  with its count ("90% (18 of 20)") or as "N/A", and streaks MUST carry their unit; this formatting
  MUST come from one shared, unit-tested formatter.
- A status MUST never be conveyed by color alone. Text contrast MUST be at least 4.5:1, every
  focusable element MUST show the focus ring, dialogs MUST manage focus (first field on open, Esc
  and × close, focus returns to the opener), and icon-only buttons MUST have an `aria-label`.

Rationale: reviewers judge the product in the browser. A single design source keeps the
agent-built UI coherent across many independent feature sessions.

### V. Verified, Not Asserted

- A change MUST NOT be reported as done until the build and the full test suite have passed in the
  tree being committed, run in the current session. Anything not executed MUST be labelled
  unverified, and failures MUST be reported as they occurred.
- The architecture gate (git pre-commit hook and the Claude Code guard) MUST NOT be bypassed or
  disabled (`--no-verify`, hooks-path or config overrides). A failing gate is fixed at its cause.
- When the same class of defect or review finding appears twice, the fix MUST add an enforcing
  mechanism (architecture test, analyzer rule, oracle case, or shared test fixture) instead of
  another point patch.
- The first change that adds production types to a layer MUST add the ArchUnitNET rules that
  cover that layer. The current suite only checks Blazor components against Persistence.

Rationale: agents produce plausible output quickly. Only executed checks and enforcing mechanisms
make that output trustworthy across sessions.

### VI. Demonstrable Vertical Slices

- Features MUST be delivered as thin vertical slices (domain rule, use case, persistence, UI,
  tests), each demonstrable on its own. Work MUST NOT add infrastructure for hypothetical clients
  or scale.
- Every user-facing feature MUST extend the representative demo data so a reviewer sees its states
  in the hosted app without waiting for real time to pass (e.g. Pending, Met, Failed, Missed,
  Excused, and Paused results, non-trivial streaks, bands and catalog aggregates above their
  thresholds).
- Demo data MUST be visibly synthetic (no real people, reserved domains such as `example.com`) and
  MUST obey the same consent and privacy rules as real data.

Rationale: SPEC §10 requires a browsable demo, and the exam values a coherent, working increment
over breadth.

## Decision Discipline

- A spec that touches a SPEC §11 open question or an ARCHITECTURE §9 deferred choice MUST mark it
  `[NEEDS CLARIFICATION: SPEC §11.n]` (or `ARCHITECTURE §9`) instead of assuming an answer.
- The answer comes from the project owner (via `/speckit-clarify` or conversation) and is recorded
  in the spec's `## Clarifications` section with its date. A decision that spans features, is hard
  to reverse, or rejects real alternatives MUST also become an ADR in `docs/adr/`.
- A plan MUST NOT pass its Constitution Check while a marker affecting it remains open.
- Later features MUST cite a recorded decision rather than decide it again. Changing a recorded
  decision requires a new or superseding ADR.

## Workflow and Quality Gates

- Features follow the Spec Kit sequence: specify → clarify → plan → tasks → analyze → implement.
  Features that touch evaluation rules or privacy SHOULD get a `speckit-critic` pass on the spec
  and plan before implementation.
- The plan's Constitution Check MUST list principles I–VI, each marked as met or as a justified
  exception. Each exception goes in Complexity Tracking with the simpler alternative and why it was
  rejected.
- A pull request description MUST link the feature spec, name the principles it touches, and give
  the verification evidence (test counts and anything left unverified). Review findings are fixed
  or answered, never silently dismissed.

## Governance

- Precedence: SPEC.md defines what the product does, ARCHITECTURE.md how it is built, CLAUDE.md
  how agents work in the repo. This constitution governs how features are specified, verified, and
  accepted. A conflict between any of them MUST be raised with the project owner and resolved by
  amending the right document, never by silent divergence.
- Amendments are made in a pull request that edits this file, updates the Sync Impact Report, and
  is approved by the project owner. Agents may propose amendments but MUST NOT ratify them.
- Versioning follows semantic versioning: MAJOR for removing or redefining a principle, MINOR for
  adding a principle or section or materially expanding guidance, PATCH for wording and
  clarifications.
- Compliance: `/speckit-plan` gates on the Constitution Check, `/speckit-analyze` treats violations
  as CRITICAL, and PR review checks the principles named in the description. The constitution is
  re-checked whenever an ADR is accepted or a SPEC §11 / ARCHITECTURE §9 item is decided.

**Version**: 1.0.0 | **Ratified**: 2026-11-03 | **Last Amended**: 2026-11-03
