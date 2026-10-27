---
status: Confirmed
owner: "Yuri Derevianko"
reviewers: []
updated_at: "2026-10-27"
feature_size: TBD     # set by sdlc:classify-size, not here
stage: "01"
depth: easy   # interview depth used
ticket: "—"
value_score:
  rice: 5.3                 # computed by Claude
  state: confirmed
  confirmed_at: "2026-10-27"
feasibility_state: confirmed
---

<!-- Stage 01 → see SDLC/plugin/skills/interview/SKILL.md -->
<!-- Why: capture the idea before it's forgotten or retold incorrectly -->

# Idea Brief — Habit catalog

## 1. Raw idea
A public catalog of reusable habit ideas (e.g. Reading, Yoga) with optional categories and suggested starting goals, so people don't start from a blank page; users can also create private custom habits and suggest them for the catalog, which an admin approves, edits or rejects.

*(Scope was narrowed in Phase 2: the «suggest for catalog» flow and its admin review are out of this feature — see §5.)*

## 2. Problem
A newly registered user lands on an empty app and has to invent both the behavior and a realistic target from scratch; this blank-page moment is where a first goal is most likely never created. The product also has no shared vocabulary of habits yet, which later community comparison (SPEC §7.3) depends on.

## 3. Users
- **Primary — new registered user**: first session, no goals yet; needs ideas and a sensible starting target. Frequency: once per user at onboarding, then occasionally when adding a goal.
- **Secondary — anonymous visitor / demo reviewer**: browses the catalog without an account to understand what the product offers (SPEC §3, §10).
- **Secondary — administrator**: maintains catalog content (create, edit, publish, unpublish).

## 4. Why now
Course-exam timeline: the catalog is the first feature a reviewer meets, it is a prerequisite for goals based on catalog habits (future `goal-tracking`), and it carries the representative demo content required by SPEC §10. It has few open product questions, which makes it a good first pass through the SDLC pipeline.

## 5. Out of scope
- **«Suggest for catalog»** and the admin review of suggestions (approve / edit-before-publish / reject) — deferred to a separate later feature by decision in Phase 2.
- Creating a personal goal from a catalog habit or a suggested starting goal — belongs to `goal-tracking`; this feature only *shows* suggestions.
- Catalog Habit Performance and any community statistics (SPEC §7).
- Personalised recommendations, coaching or automatic goal generation (SPEC §9).
- Ratings, comments or social features on catalog entries.

## 6. Competitive analysis
| # | Product · URL | Features | Value per feature (1-5) | Gap |
|---|---|---|---|---|
| 1 | Habitify · [habitify.me](https://habitify.me/blog/habitica-alternatives) | User-defined habits; daily/weekly/monthly streaks; challenges | Custom habits 4 · Streaks 4 · Preset catalog — | No preset catalog of habit ideas found; user starts from a blank form¹ |
| 2 | Loop Habit Tracker · [App Store](https://apps.apple.com/is/app/habit-tracker-loop/id6478388488) | User sets all habits; minimal tracking | Custom habits 4 · Preset catalog — | «You set the habits» — no starting ideas or targets¹ |
| 3 | Fabulous · [overview](https://softpicker.com/best-habit-tracker-apps/) | Guided journeys, coaching-style routines | Guided start 4 · Freedom to customise 2 | Prescriptive coaching — the opposite of SPEC's «example, not advice» stance¹ |
| 4 | Habitica · [Wikipedia](https://en.wikipedia.org/wiki/Habitica), [Wiki](https://habitica.fandom.com/wiki/Habitica_Wiki) | Positive/negative habits; example habits in a player-maintained wiki; challenges add habit sets | Build/Reduce polarity 4 · Example habits 2 | Examples live outside the product (fan wiki), not a curated in-app catalog² |

Ratings are Claude's estimates from the sources, not measurements. Source quality is weak (several SEO listicles); in-app catalogs may exist but were not confirmed.

¹ 2026-10-27, query: «habit tracker app habit library catalog of preset habits suggested goals (Habitify, Streaks, Loop, Fabulous)».
² 2026-10-27, query: «Habitica OR "Way of Life" OR HabitShare habit templates predefined habits categories community suggested habits».

## 7. Strategic approaches

### Approach A — Static curated catalog
- **Thesis**: Admins curate a list of habit ideas (name, description, category) that anyone can browse; no starting-goal suggestions yet.
- **For whom**: demo reviewer, administrator.
- **Outcome metric**: 3 end-to-end demo scenarios (§11 basis) pass: 0 → 3.
- **Key trade-off**: fastest to build, but the new user still has to invent a target.
- **Effort signal**: S
- **Recommended?** ◯

### Approach B — Catalog with goal presets
- **Thesis**: Each habit offers several ready starting goals across measurement types (yes/no, count, duration) and Build/Reduce, with search and a preview of what the goal would look like.
- **For whom**: new registered user.
- **Outcome metric**: share of new users whose first goal starts from a preset: 0 → ≥60% (measurable only after `goal-tracking`).
- **Key trade-off**: strongest onboarding, but overlaps heavily with `goal-tracking` and drifts toward coaching.
- **Effort signal**: L
- **Recommended?** ◯

### Approach C — Curated catalog, one example goal
- **Thesis**: Admins curate categorised habit ideas, each with at most one optional, editable suggested starting goal; visitors filter by category.
- **For whom**: new registered user (primary), demo reviewer.
- **Outcome metric**: 3 end-to-end demo scenarios pass: 0 → 3 — (1) anonymous visitor browses ≥20 published habits in ≥4 categories and filters by category; (2) a user creates, edits and deletes a private custom habit that no one else — including admins — can see; (3) an admin creates, edits, publishes and unpublishes a habit and the public view reflects it.
- **Key trade-off**: removes the blank-page barrier without building goal creation; the suggestion format must stay compatible with future goals.
- **Effort signal**: M
- **Recommended?** ●

## 8. Multi-perspective feedback

*(Easy depth: perspectives written by Claude in-session, not by independent reviewers.)*

### Engineer
- All approaches need foundations the repo lacks: sign-in, an admin role, stored data.
- Privacy boundary for custom habits must be enforced and tested, not assumed.
- B's presets duplicate the goal model before it exists — high rework risk.
- Unpublish/edit of an entry already used by goals needs a defined behavior.

### Executive
- Catalog is the first thing a reviewer sees — strong demo value per effort.
- It unlocks `goal-tracking` and later community statistics (only catalog habits count).
- B's effort competes with the core value (consistency tracking) for limited exam time.

### UX-researcher
- A blank page is the main onboarding drop-off; one concrete example helps most.
- Many presets (B) risk choice overload and read as advice.
- Categories plus one filter are enough at ~20–50 entries; search can wait.

### Synthesis matrix
|         | Engineer | Executive | UX |
|---------|:--------:|:---------:|:--:|
| App. A  | + smallest surface, nothing to rework | 0 good demo, weak onboarding value | − still leaves blank-target problem |
| App. B  | − duplicates goal model before it exists | − effort competes with core tracking | 0 strong start, risks choice overload |
| App. C  | 0 small extra; format must align | + demo value and onboarding, moderate cost | + one example removes blank page |

## 9. Trade-offs and edge cases

### Trade-offs per approach
| Approach | Pros | Cons |
|---|---|---|
| A | Smallest scope; no rework risk | Doesn't address the target-setting half of the problem |
| B | Best onboarding; differentiating | Large; pre-empts `goal-tracking` design; coaching tone |
| C | Addresses the problem at moderate cost | Suggestion shape must be agreed with future goals |

### Edge cases
- Unpublishing a catalog habit that existing goals are based on — goals must keep working; the entry just leaves the public catalog.
- Editing a catalog habit's name/description after goals use it — does the change show in those goals?
- A custom habit with the same name as a catalog habit, or two catalog entries with near-identical names.
- Uncategorised habits and empty categories in the public view.
- Drafts / unpublished entries must never appear to anonymous visitors or regular users.
- A deactivated account's custom habits are retained but unreachable.
- A Reduce-type suggested starting goal (e.g. alcohol) must read as an example, not medical advice.

## 10. Risks
- **Top (devil's advocate): foundation drag.** The repo has no sign-in, admin role or stored data yet; the catalog pulls all of them in, and effort balloons past M. Signal: early tasks exceed one day, the first PR touches all four projects.
- **Privacy leak of custom habits** to admins or public listings, violating the owner-only rule. Signal: an admin page or public query lists custom habits; no test covers it.
- **Unpublish/edit breaks future goals** that reference catalog entries. Signal: dangling or silently renamed habits in `goal-tracking`.
- **Suggestions read as prescriptive advice**, contradicting SPEC §4. Signal: reviewer feedback; Reduce suggestions with health targets.
- **Thin seed content** makes the demo look empty. Signal: fewer than ~20 entries at review time.

## 11. RICE — Claude proposed
- **Reach (R)**: 20 — every demo visitor per quarter (reviewers + demo accounts) meets the catalog first (§3).
- **Impact (I)**: 1 — medium: prerequisite for goals and community stats, but not the core consistency value itself (§2, §8 Executive).
- **Confidence (C)**: 0.8 — problem and scope are clear; 2 product questions open (§15).
- **Effort (E)**: 3 person-weeks — M signal for Approach C (§7), including minimal sign-in/admin/storage foundation.
- **RICE = R × I × C / E = 20 × 1 × 0.8 / 3 ≈ 5.3**
- **State**: confirmed

## 12. Feasibility — Claude proposed

- [☑] **Tech**: stack is fixed in ARCHITECTURE.md and standard for this kind of feature; repo scan (2026-10-27) found only the scaffold — empty Domain/Application, starter UI page, one architecture test — so there is no adjacent shipped feature to compare against.
- [☑] **Skills**: owner is a senior .NET developer; role-gated content management is familiar work.
- [☐] **Time**: TBD — no feature has shipped in this repo yet (scaffold only, 2026-10-11), so there is no delivery history to base an estimate on.
- **State**: confirmed

## 13. Recommendation
**Selected: Approach C** — A curated, categorised catalog with at most one optional suggested starting goal per habit. It targets the primary user's blank-page problem (UX cell for C: «one example removes blank page», §8), and fills a gap competitors leave open: Habitify and Loop offer no starting ideas, while Fabulous answers it with prescriptive coaching that SPEC excludes (§6). Its RICE ≈ 5.3 (§11) reflects moderate value at M effort, including the missing foundation. Feasibility is Tech ☑, Skills ☑, Time TBD (§12) — the recommendation stands, with the warning that the schedule is unverified.

**Locked-in pointer**: write-prd covers public browsing with category filter, admin create/edit/publish/unpublish, private custom habits (owner-only, invisible to admins), and display-only suggested starting goals; the suggestion's shape must be agreed with `goal-tracking`.

## 14. Parked & rejected approaches
| # | Approach | Status | Reason | Revisit trigger |
|---|---|:---:|---|---|
| A | Static curated catalog | parked | Leaves target-setting problem unsolved | Time check (§12) shows M doesn't fit |
| B | Catalog with goal presets | parked | L effort; duplicates goal design before it exists | After `goal-tracking` ships and presets can map onto real goals |
| - | Suggest-for-catalog flow | parked | Descoped in Phase 2 | After this feature ships; needs its own brief |

## 15. Open questions
- [ ] What happens to goals when their catalog habit is edited or unpublished? — owner: Yuri Derevianko, due: before write-prd sign-off
- [ ] Shape of a suggested starting goal (measurement type, target, schedule) and its fit with `goal-tracking` — owner: Yuri Derevianko, due: write-prd
- [ ] Realistic delivery time against the exam deadline (Feasibility Time TBD) — owner: Yuri Derevianko, due: break-tasks
- [ ] Is sign-in / admin role a separate prerequisite feature or part of this one? — owner: Yuri Derevianko, due: write-prd
- [ ] Seed content: who writes the ~20+ demo entries and in which categories? — owner: Yuri Derevianko, due: implement-tasks

## Related
- [CONTEXT.md](../../../CONTEXT.md) — glossary: Catalog habit, Custom habit, Suggested starting goal
- [SPEC.md](../../../SPEC.md) §3, §4, §7.3, §9, §10
- [ARCHITECTURE.md](../../../ARCHITECTURE.md) §2, §4
- Future features: `goal-tracking`, suggest-for-catalog, community comparison

## DoD self-check
- [x] 15 sections present
- [x] No anti-pattern terms (Postgres/Redis/etc.)
- [x] Length ≤ 5 pages (~2200 words)
- [x] Frontmatter status: Confirmed
- [x] RICE confirmed (state: confirmed)
- [x] Feasibility confirmed (state: confirmed)
- [x] Recommendation present with rationale citing 4 upstream sections (§6, §8, §11, §12)
