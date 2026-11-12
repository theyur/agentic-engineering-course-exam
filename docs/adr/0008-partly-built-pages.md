# 0008 — Partly built pages: only shipped sections and links

**Status:** Accepted, 2026-11-11. Records roadmap decision R7 (owner answer, 2026-11-11) and its scope,
clarified in slice 1, `specs/001-accounts-and-access`. Applies to every slice until the last extending slice
of each page ships.

## Context

Constitution IV says pages follow the structure and wording of their previews in `design/pages/`. Several
previews combine sections from different slices (roadmap, "Pages across slices"). For example,
`PageHomeVisitor` has a catalog preview (slice 2), a goal card (slice 6) and community bands (slice 9), and
`TopNav` has Catalog, Goals and Community links. Slices ship one at a time, so a page is often built before
everything its preview shows exists.

## Options

1. **Only shipped sections and links (chosen).** A page or the navigation shows only what has shipped, with no
   placeholder.
2. **Build each page in full in one slice.** The page matches its preview from the start, but that moves later
   slices' work earlier and breaks thin vertical slices (constitution VI).
3. **Placeholders or disabled controls.** The preview's layout stays visible, but the app shows things that do
   nothing, and reviewers judge the product in the browser.

## Decision

- A page or the navigation shows only the sections and links whose slice has shipped. Sections and links owned
  by later slices are absent: not disabled, hidden behind a flag or shown as placeholders.
- What a page shows follows its preview's structure and wording for the parts that have shipped. The full
  preview is reached when the last slice that extends the page ships.
- The scope is sections and links only. Sentences keep the wording of the previews, even when they mention a
  control that has not shipped yet (for example "You can opt in later in your profile." on Register, before
  slice 9).
- Each slice's spec states which parts of each page it builds. `docs/roadmap.md` ("Pages across slices")
  tracks which slice adds what to each page.
- A slice may need a page state that no preview shows (for example, Profile with a single section). That state
  is designed in `design/` first (constitution IV), then built.

## Consequences

- During the build-out the app is consistent with the design, but not identical to its previews. Reviewers of
  an intermediate slice see fewer sections, never broken ones.
- Every slice that extends a page edits that page again. Its spec and the roadmap's page tracker name the
  sections it adds.
- Some sentences can point to controls that arrive later. This is accepted to keep the previews' wording.
- No feature flags are needed for unshipped sections.
