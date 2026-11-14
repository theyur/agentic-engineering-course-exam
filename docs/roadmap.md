---
status: living
updated_at: "2026-11-14"
---

# Roadmap — Habit Tracker

> **Direction, not a promise.** The near slices are firm; later ones are directional and will
> change as decisions land. There are **no dates**. What each feature does is decided in its
> Spec Kit folder (`specs/NNN-<name>/`), not here. This file only orders the work, names what
> blocks each slice, and records where each one stands.

Sources: [SPEC.md](../SPEC.md) (what the app does), [ARCHITECTURE.md](../ARCHITECTURE.md) (how it is
built), the [constitution](../.specify/memory/constitution.md) (how features are specified and
verified), and the [habit-catalog idea brief](features/habit-catalog/idea-brief.md).

This file keeps the frontmatter and the no-dates rule of the course's roadmap template, but not its
Now / Next / Later tables. This repo specifies features with Spec Kit instead of the course's
`write-prd` / `ship-feature` stages, so one dependency-ordered table with a Horizon column replaces them.
Don't regenerate this file with `/sdlc-roadmap`.

## Delivery sequence

Thin vertical slices (constitution VI), in dependency order. *Needs* means the slice cannot be built
without that slice. *Better after* is an ordering preference. Only the habit catalog has a RICE score: 5.3
in its idea brief, which assumed the catalog also carried the account foundation (see R1).

| # | Slice | Outcome (the why) | SPEC | Needs | Better after | Horizon | Status | Spec |
|---|---|---|---|---|---|---|---|---|
| 1 | `accounts-and-access` | People can register, sign in and sign out. The app tells anonymous visitors apart from signed-in registered users and administrators. | §3; ARCH §4 | — | — | — | shipped | [001](../specs/001-accounts-and-access/spec.md) |
| 2 | `habit-catalog` | A new user does not start from a blank page. Anyone can browse curated catalog habits by category, with suggested starting goals; administrators curate them; users keep private custom habits. | §3, §4 (without Suggest for catalog), §8 (catalog) | 1 | — | Now | planned | [002](../specs/002-habit-catalog/spec.md) |
| 3 | `demo-hosting` | Reviewers open the app in a browser, with demonstration data, without installing anything. | §10; ARCH §7 | 1 | 2 | Next | not started | — |
| 4 | `goals` | A user turns a catalog or custom habit into a measurable Build or Reduce goal and sees it in the goal list and detail. | §5.1 | 2 | — | Next | not started | — |
| 5 | `recording-and-evaluation` | A user records what actually happened, including partial amounts and notes. Unrecorded results are Pending, become Missed after the 48-hour grace period, and corrections are evaluated with the original rules. | §5.2 | 4 | — | Later | not started | — |
| 6 | `personal-reports` | A user sees how consistent they are: a dashboard of upcoming, due and overdue commitments and Missed results, consistency with its count, strict streaks with their unit, trends. | §6 | 5 | — | Later | not started | — |
| 7 | `excusals-and-pauses` | Excused commitments and paused expectations are left out of consistency and neither extend nor break the streak. | §5.3 | 5, 6 | — | Later | not started | — |
| 8 | `goal-changes` | A user changes a goal from the next evaluation boundary, ends it with an outcome, and restarts it as a new attempt, with earlier attempts kept in history. | §5.4 | 5, 6 | 7 | Later | not started | — |
| 9 | `community` | People who opt in appear in Consistency Bands under a public display name, and catalog habits show aggregate performance. Anyone, including anonymous visitors, sees these views; no private data is exposed. | §7 | 2, 6 | 7 | Later | not started | — |
| 10 | `suggest-for-catalog` | A user can offer a custom habit to the catalog; an administrator approves, edits or rejects it. | §4, §8 | 2, 4 | — | Later | not started | — |
| 11 | `administration` | Administrators review user feedback and deactivate or reactivate accounts, still without access to private tracking data. | §8; ARCH §4 | 1 | 9 | Later | not started | — |

SPEC §10 (demonstration data) is cross-cutting: every user-facing slice extends the demo data
(constitution VI), and slice 3 makes it reachable for reviewers.

**Status values**, following the feature flow in `CLAUDE.md`: `not started` · `specified` · `clarified`
· `acceptance locked` (`acceptance.md`) · `features locked` (`.feature` files) · `planned` ·
`tasks ready` · `implementing` · `AC tests green` · `shipped` (set when the PR opens; see
[Updating this file](#updating-this-file)). A slice whose spec
finds no behavior on the AC surface (ADR 0003) records that in its spec and skips the three AC states.
Idea briefs are not part of the flow; whether other slices get one is decision R6.

**Horizons:** *Now* = a Spec Kit folder exists and the slice is being built; *Next* = the following
slices, close enough to plan; *Later* = directional. A shipped slice keeps its row with Horizon `—`.

### Slice notes

Only what the order depends on. Scope is decided in each spec.

1. **`accounts-and-access`** carries the foundation the catalog would otherwise drag in. The idea brief's
   top risk (§10) is foundation drag. The slice brings:
   - ASP.NET Core Identity on SQL Server, sign-in and registration (`design/pages/PageSignIn`, `PageRegister`);
   - the first migration, and the two `HT.Persistence` entry points ADR 0003 requires;
   - the ArchUnitNET rules for every layer that gets its first production types (constitution V);
   - the shared shell: TopNav, MobileNav and Footer (see [Pages across slices](#pages-across-slices));
   - the first demo data: synthetic accounts on `example.com`;
   - for anonymous visitors, the parts of the visitor Home that need no catalog (hero with "Register free", how it works).

   The spec settles how much of this slice AC tests can reach: account behaviour is Application use cases
   (spec `## Clarifications`). Where the account model lives and how the acting user reaches those use cases
   are decided in ADR 0004 and ADR 0005 (see the decision table).
2. **`habit-catalog`** keeps the confirmed idea brief (Approach C and its three demo scenarios), minus the
   foundation slice 1 now carries. The brief, SPEC §4 and the design disagree on details (see the decision
   table). Seed content: at least 20 published habits in at least 4 categories.
3. **`demo-hosting`** is infrastructure, not a feature: an ADR for hosting and deployment, then the
   deployment itself and hosted demo data. No AC tests. If it comes right after the catalog, every later
   slice can be shown in the hosted app once it is deployed.
4. **`goals`** defines goals and fixes the time model the rest depends on: schedules, evaluation
   boundaries and time zones. Goal settings are stored as rule versions from the start (constitution II),
   even though changing them comes in slice 8. Whether a goal starts as its first attempt is decided here,
   for the same reason.
5. **`recording-and-evaluation`** is the core of the product: evaluation as a deterministic function of
   stored facts and a supplied time (`TimeProvider`). Demo data has to show Pending, Met, Failed and Missed
   results relative to the current time. A correction's effect on reports is verified in slice 6.
6. **`personal-reports`** comes before excusals and pauses so the core value is demonstrable earlier
   (decision R3). Consistency and streak formatting come from one shared formatter (constitution IV).
7. **`excusals-and-pauses`** extends the evaluation of slice 5 and the reports of slice 6. SPEC §11.2 can
   change how period-based goals are evaluated, so this slice may rework slice 5's period logic, not only
   add two excluded states. That stays cheap only while no derived figure is stored (constitution II).
8. **`goal-changes`** needs evaluations (slice 5) and streaks (slice 6) to verify its rules: completed
   evaluations keep their rules, ending creates no failure, and a new attempt's streak starts at zero. It
   also owns the Profile "Goal history" tab and the Goals page "Ended" tab.
9. **`community`** uses consistency from slice 6. If it lands before slice 7, slice 7 keeps the community
   figures correct when it adds the excluded states. Consent and the public display name live on the
   Profile "Community" tab. Demo data needs enough consenting demo users to fill bands, and catalog habits
   both above and below the 5-contributor minimum.
10. **`suggest-for-catalog`** is the one place where a user's private custom habit reaches an
    administrator. SPEC §4 rules that publishing does not expose the submitter's identity or private data,
    and does not reconnect their goals. The second rule needs goals (slice 4).
11. **`administration`**: deactivation must end access (session revalidation, ARCH §4) and suspend
    community participation. A deactivated account sees a notice on sign-in (`design/pages/PageSignIn`).
    Whichever of slices 9 and 11 lands second covers the suspension.

### Pages across slices

Several design pages combine sections from different slices. Each slice's spec confirms its part. How a
partly built page or navigation meets constitution IV ("pages MUST follow … their previews") is
decision R7.

| Page or shell (`design/`) | Created in | Extended in |
|---|---|---|
| Shell: `TopNav` (visitor, signed-in, admin), `MobileNav`, `Footer` | 1 (Today, avatar menu, "Privacy") | each slice adds its links (Catalog and Admin 2, Goals and "+ New goal" 4, Community 9); "Send feedback" 11 |
| `PageSignIn`, `PageRegister` | 1 | 11 (deactivated notice) |
| `PagePrivacy` (new, designed in 1) | 1 | — |
| `PageHomeVisitor` | 1 (hero with "Register free", how it works) | 2 ("Browse the catalog", catalog preview), 6 (GoalStats card from demo data), 9 (BandSummary) |
| `PageHomeSignedIn` | 1 ("Today" heading; the greeting and date wait for the time-zone decision of slice 4) | 2 (empty state pointing to the catalog), 6 (stat tiles, Today list, GoalSummary, recent notes), 7 ("Excuse" on Pending rows) |
| `PageProfile` | 1 (Account tab) | 2 (Custom habits), 8 (Goal history), 9 (Community) |
| `PageCatalogVisitor` | 2 | — |
| `PageCatalogSignedIn` | 2 | 4 ("Use this habit", "N goals", "Add another goal", "Only habits I track"), 10 ("Suggest for catalog" on custom-habit cards) |
| `PageCustomHabit` | 2 (including "Delete") | 4 ("Save and create goal"; "Delete" only when no goals use it), 10 ("Suggest for catalog") |
| `PageHabitDetail` | 2 | 4 ("Use this habit", "You track this · N goals", "Start with this", your goals), 9 (community performance) |
| `PageNewGoal`, `PageGoals`, `PageGoalDetail` | 4 | 5 (Record, history, notes), 6 (GoalStats, trend), 7 (Pause, Excuse, Paused tab), 8 (Edit settings, End goal, Ended tab, earlier attempts) |
| `PageCommunity` | 9 | — |
| `PageAdmin` | 2 (Catalog tab) | 10 (Suggestions), 11 (Feedback, Accounts) |
| `PageAdminSuggestions` | 10 | — |
| `PageAdminFeedback` | 11 | — |
| `PageAdminAccounts` | 11 | 9 (public display name and community state, if 9 lands after 11) |

## Decisions each slice must settle

Constitution, Decision Discipline: the owner answers each of these, and the answer is recorded in the
spec's `## Clarifications`, plus an ADR when it spans features. Each decision is settled once, in the slice
named here, before that slice's acceptance criteria are drafted (`/speckit-ac-translate`), or before
`/speckit-plan` for a slice with no AC stage. Later slices cite it.

How this works with the Spec Kit commands:

- **Product rows:** pass the slice's open rows in the `/speckit-specify` input as "do not guess". The
  skill keeps at most 3 `[NEEDS CLARIFICATION]` markers and guesses the rest, and `/speckit-clarify` asks
  at most 5 questions. Rows beyond those limits are asked in conversation and recorded the same way.
  Answers given inside `/speckit-specify` replace the marker inline; copy them to `## Clarifications`
  with the date.
- **Technical rows** (with or without an ADR): `/speckit-plan` would otherwise choose them itself in
  `research.md`. Decide them with the owner and write an ADR where the row says so or the constitution's
  test applies (the decision spans features, is hard to reverse, or rejects real alternatives). Before
  `/speckit-ac-translate`, add one dated line to the spec's `## Clarifications` that names the decision
  and links the ADR; the detail stays in the ADR, since the spec holds no implementation detail.
  Acceptance criteria are drafted only from the spec, the constitution and the `specInputs` of
  `.workflow/config.json` (today `SPEC.md` and ADR 0003), and they must be testable through the AC
  surface. An answer added after they are locked shows as spec drift: the user confirms the criteria
  (`ac_lock.py confirm-spec`) or, if they change, unlocks them. For a slice with no AC stage, decide
  before `/speckit-plan`. `research.md` then cites the answer instead of choosing.
- **Mocking and UI-test libraries** have no owning slice: when `/speckit-plan` first needs one, stop, ask
  the owner and record the answer. The spec change is confirmed as spec drift; no unlock is needed.
- *Status* is `open` or `decided`, with a note where `CONTEXT.md` already suggests an answer. *Recorded
  in* links the spec section or ADR.

| Decision | Source | Settled in | Status | Recorded in |
|---|---|---|---|---|
| Where the account model lives, and how the acting user, the administrator role and the deactivated state reach Application use cases (ADR) | ARCH §3, §4, §9 | `accounts-and-access` | decided | [ADR 0004](adr/0004-account-model.md) |
| Use-case convention: shape of the Application surface that UI and AC tests call (ADR) | ARCH §9; ADR 0003 | `accounts-and-access` | decided | [ADR 0005](adr/0005-use-case-convention.md) |
| How the first administrator account comes to exist | new | `accounts-and-access` | decided | [spec](../specs/001-accounts-and-access/spec.md#clarifications); [ADR 0007](adr/0007-startup-operator-tasks.md) |
| Authentication UX: email verification, password recovery, outbound email, "keep me signed in". `design/pages/PageSignIn` already shows "Forgot password?" and "Keep me signed in" and says "Built on ASP.NET Core Identity pages"; if the decision differs, `design/` changes first. | ARCH §9; design | `accounts-and-access` | decided | [spec](../specs/001-accounts-and-access/spec.md#clarifications); [ADR 0009](adr/0009-no-outbound-email.md) |
| Integration-test database approach (ADR) | ARCH §9; ADR 0003 | `accounts-and-access` | decided | [ADR 0006](adr/0006-integration-tests.md) |
| Test-project layout | ARCH §9 | `accounts-and-access` | decided | [ADR 0006](adr/0006-integration-tests.md) |
| Re-runnable demo-data seeding | ARCH §7 | `accounts-and-access` | decided | [ADR 0007](adr/0007-startup-operator-tasks.md) |
| Privacy page: `design/components/Footer` links to it, but no page design exists | new; design | `accounts-and-access` | decided | [spec](../specs/001-accounts-and-access/spec.md#clarifications) |
| Whether an account has a name, and the time-dependent text of the signed-in Home greeting and the Profile "member since". Slice 1 shows neither ([spec](../specs/001-accounts-and-access/spec.md#clarifications)). | new; design | `goals` | open | — |
| Whether an administrator account can also track its own goals. `design/components/TopNav` gives administrators no Goals link. | SPEC §3; design | `goals` | open | — |
| One or several suggested starting goals per catalog habit, and whether the catalog page has search, an Intent filter and sort. The idea brief says "at most one" and "search can wait". SPEC §4 says "suggested starting goals" while SPEC §2 is singular, so SPEC is ambiguous. `design/pages/PageHabitDetail` shows several; `PageCatalogVisitor` and `HabitCard` show one per card, plus search, an Intent filter and sort. If the brief wins, `design/` changes first (constitution IV). | idea brief §7, §8; SPEC §2, §4; design | `habit-catalog` | decided: several suggested starting goals, one marked for the card; search, Intent filter and A–Z order as the design shows | [spec](../specs/002-habit-catalog/spec.md#clarifications); [ADR 0011](adr/0011-suggested-starting-goals.md) |
| Shape of a suggested starting goal, compatible with goals | idea brief §15 | `habit-catalog` | decided: structured fields like the New goal form | [spec](../specs/002-habit-catalog/spec.md#clarifications); [ADR 0011](adr/0011-suggested-starting-goals.md) |
| How goals behave when their catalog habit is edited or unpublished; `goals` cites the answer | idea brief §15 | `habit-catalog` | decided: goals keep working with the current name; unpublishing a used habit asks for confirmation with the goal count | [spec](../specs/002-habit-catalog/spec.md#clarifications); [ADR 0012](adr/0012-goals-on-changed-catalog-habit.md) |
| Who writes the seed catalog content | idea brief §15 | `habit-catalog` | decided: Claude drafts it, the owner reviews it in the PR | [spec](../specs/002-habit-catalog/spec.md#clarifications) |
| Hosting services, deployment, pipelines, monitoring, secrets mechanics (ADR). Scheduled evaluation (slice 5) may need more; changing this then needs a superseding ADR. | ARCH §7, §9 | `demo-hosting` | open | — |
| Schema migration rollout on each deployment | ARCH §5 | `demo-hosting` | open; locally the host applies migrations at start in Development ([ADR 0007](adr/0007-startup-operator-tasks.md)) | — |
| When and how hosted demo data is refreshed. Time-relative generation (slice 5) may reopen it with a new or superseding ADR. | ARCH §7 | `demo-hosting` | open | — |
| How reviewers get demo sign-ins, with no secret in source control | ARCH §7; CLAUDE.md | `demo-hosting` | open | — |
| Commitment deadlines and time zones (ADR) | SPEC §11.1 (first part); ARCH §9 | `goals` | open | — |
| Whether a goal is created as its first attempt from the start | SPEC §2, §5.4 | `goals` | open | — |
| The 48-hour grace period around local clock changes | SPEC §11.1 (second part) | `recording-and-evaluation` | open | — |
| When a recorded result becomes final: on recording, or when the grace period ends. SPEC §5.2 only says a missing record is Pending; `CONTEXT.md` (Pending) says Pending applies "whether or not any activity has been recorded so far". Confirm and record it, or change the glossary. | SPEC §5.2; CONTEXT.md | `recording-and-evaluation` | open | — |
| An unrecorded Reduce period | SPEC §11.4 | `recording-and-evaluation` | open; `CONTEXT.md` (Missed) already treats any unrecorded period as Missed. Confirm and record it, or change the glossary. | — |
| Time-relative demo-data generation (results relative to the current time) | ARCH §7; constitution VI | `recording-and-evaluation` | open | — |
| Scheduled evaluation and background processing | ARCH §9 | `recording-and-evaluation` | open | — |
| Caching of derived figures | ARCH §9 | `personal-reports` | open | — |
| A Pending evaluation in the current streak | SPEC §11.7 (first part) | `personal-reports` | open; `CONTEXT.md` (Pending) already says it "never resets a streak". Confirm and record it, or change the glossary. | — |
| Partially overlapping pauses in multi-day evaluation periods | SPEC §11.2 | `excusals-and-pauses` | open | — |
| Excusing a commitment after it is finalized | SPEC §11.3 | `excusals-and-pauses` | open; `CONTEXT.md` (Excused) already says it "can be set only while the commitment is upcoming or Pending". Confirm and record it, or change the glossary. | — |
| What a streak means after goal settings change | SPEC §11.7 (second part) | `goal-changes` | open | — |
| Aggregating Catalog Habit Performance | SPEC §11.5 | `community` | open | — |
| Whether community contributions are limited to catalog-habit goals | SPEC §11.6 | `community` | open | — |
| Where and how a user sends feedback. SPEC §8 has administrators review it, but §3 does not list sending it; `design/components/Footer` and `FeedbackDialog` show a "Send feedback" form. | new; design | `administration` | open | — |
| Mocking library | ARCH §9 | the first slice that needs one (`accounts-and-access`) | decided: none, hand-written fakes | [ADR 0006](adr/0006-integration-tests.md) |
| UI-test library | ARCH §9 | the first slice that needs one (`accounts-and-access`) | decided: bUnit and host tests, no browser automation yet | [ADR 0006](adr/0006-integration-tests.md) |

Domain and persistence models and the schema (ARCH §9) are decided in each slice's plan, for its own part,
and are not tracked here.

## Decisions on this roadmap

For the owner. *Before* says which slice cannot start until the decision is made.

| # | Decision | Before | Recommended | Alternative | Owner answer |
|---|---|---|---|---|---|
| R1 | Accounts as their own first slice | slice 1 | Yes: it isolates the idea brief's top risk, foundation drag, from the catalog. The catalog's 3 person-week estimate and RICE 5.3 included that foundation, so both need re-estimating after the split. README's example folder `001-habit-catalog/` changes too. | Fold minimal sign-in into `habit-catalog`, as the brief's estimate assumed. The catalog then becomes `001`. | Yes (2026-11-11) |
| R2 | When to host the demo | slice 3 | Right after `habit-catalog` | At the end, which risks a late deployment surprise | — |
| R3 | Reports before excusals and pauses | slice 6 | Yes: the core value is demonstrable sooner. The cost: SPEC §11.2 may rework period evaluation in slice 7 (slice note 7). | After them, with less rework | — |
| R4 | Splitting goal changes (§5.4) out of `goals` | slice 4 | Yes, as slice 8: its rules can only be verified once evaluations and streaks exist. The cost: between slices 4 and 8 a goal cannot be edited or ended, and SPEC has no delete, so a mistaken demo goal stays. | Keep them in `goals` and name, per rule, the later slice that verifies it | — |
| R5 | Cutting scope if time runs short | after R8 | No cut now; decide after R8. Any cut needs a SPEC amendment by the owner (constitution, Governance). Stopping after slice 6 would remove §5.3, §5.4 changes, §7, Suggest for catalog (§4) and §8 feedback and deactivation. SPEC §10 requires the demo to show community views, and design pages already show Pause, Excuse, Ended and Community. | Name a cut line now | No cut (2026-11-11): slices ship in roadmap order; what has shipped by 2026-11-23 is what the exam shows. SPEC stays unchanged. |
| R6 | Idea briefs for other slices | slice 1 | Optional. Idea brief §14 asks for one for Suggest for catalog. | Required for every slice | Optional (2026-11-11) |
| R7 | Partly built pages and navigation (see [Pages across slices](#pages-across-slices)) | slice 1 | A page or the navigation shows only the sections and links whose slice has shipped, with no placeholder content. The full preview is reached when the last extending slice ships. | Build each page in full in one slice, which moves work earlier | As recommended: only shipped sections and links (2026-11-11); [ADR 0008](adr/0008-partly-built-pages.md) |
| R8 | Delivery time against the exam deadline | slice 1's `/speckit-tasks` (idea brief §15 set it for the course's break-tasks stage) | — (Feasibility "Time TBD") | — | Out of scope for planning (2026-11-11): the exam is on 2026-11-23, and whatever has shipped by then is presented, together with the speckit-ac extension this repo uses. No slice waits on a time estimate. |
| R9 | Point to this file from `CLAUDE.md` so every session keeps it current | slice 1 | Yes, one line under "Docs and git workflow" | Leave it to the user to ask for updates | Yes, one line (2026-11-11) |

## Every slice

These come from `CLAUDE.md` and the constitution; they are listed here only as a checklist.

- Flow: `/speckit-specify` → `/speckit-clarify` → `/speckit-ac-translate` → `/speckit-plan` →
  `/speckit-tasks` → `/speckit-analyze` → `/speckit-implement` → `/speckit-ac-tests-write` →
  `/speckit-ac-tests-analyze`, then a PR with `/process-pr-feedback`. The user starts every
  `/speckit-ac-*` command.
- The constitution asks for a `speckit-critic` pass on the spec and plan of slices that touch evaluation
  rules or privacy: 1, 2, 4–11. The repo's `/speckit-ac-critic` reviews the stages `spec`, `plan`, `tasks`,
  `ac` and `features`, and the user starts it. Whether it satisfies the constitution's pass is the
  owner's call.
- Unit, integration and architecture tests are part of done. Privacy use cases have negative tests for
  another registered user, an administrator and an anonymous visitor (constitution III).
- Every user-facing slice extends the demo data (constitution VI).
- New domain terms go into `CONTEXT.md` in the same change (constitution I).

## Updating this file

Every change to this file also sets `updated_at`.

- **Starting a slice:** write `SPECIFY_FEATURE_DIRECTORY=specs/NNN-<slice>` (the next free number) in
  the `/speckit-specify` input text, so the folder carries the slice's name; Spec Kit generates its own
  short name otherwise. Don't set it as an environment variable: the Spec Kit scripts prefer it over
  `.specify/feature.json`, so a value left over from the previous slice would reuse the old folder.
  Set the Horizon to Now and fill the Spec column. Move the next Later slices to Next so that Next holds
  the two or three slices after Now.
- **During the slice:** the feature branch updates its row's Status after each step, and the decision
  rows it settles. The feature branch shows the current state; `master` shows it only after a merge. With
  one PR per slice, `master` therefore goes straight from `not started` to `shipped`.
- **Opening the PR:** right after opening it, before the first `/process-pr-feedback` run, one commit
  sets the row to `shipped` and Horizon `—`, and adds it to Shipped with the PR link. Every later commit
  is reviewed, so no status-only commit moves the reviewed head. The row counts on `master` only once the
  user merges. If the PR is closed unmerged, the next change to this file resets the row to its status
  before `shipped` and removes the Shipped entry.
- **A slice with several PRs:** earlier PRs keep the row `implementing`; the last PR sets `shipped` and
  lists all of the slice's PRs in Shipped.
- Rows for decisions R1–R9 are filled in when the owner answers, in the next change to this file.

## Shipped

| Outcome | Slice | Spec | PR |
|---|---|---|---|
| People can register, sign in and sign out; the app tells visitors, registered users and administrators apart. | `accounts-and-access` | [001](../specs/001-accounts-and-access/spec.md) | [#21](https://github.com/theyur/agentic-engineering-course-exam/pull/21) |
