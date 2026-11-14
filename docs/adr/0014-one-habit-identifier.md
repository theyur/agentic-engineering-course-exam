# 0014 — One identifier type for catalog habits and custom habits

**Status:** Accepted, 2026-11-14. Owner answer in slice 2, `specs/002-habit-catalog` (plan, "Gates before the
next step" 2; research R-02), after critic plan passes 1 and 2. Slice 4 (`goals`) cites it.

## Context

- Catalog habits and custom habits are two kinds of habit with their own tables, stores and use cases.
- The spec treats a custom habit's identifier given to a catalog use case, and a catalog habit's identifier
  given to a custom-habit use case, as ordinary inputs with a fixed result: "This habit is not in the
  catalog.", "This catalog habit does not exist." or "This custom habit does not exist." (US2-AS5, US3-AS13,
  FR-013, FR-022). The locked acceptance criteria call the use cases that way.
- SPEC §2 and §5.1: a personal goal is based on "a catalog or custom habit". Slice 4 stores which habit a goal
  is based on.

## Options

1. **One `HabitId` for both kinds (chosen).** A readonly record struct over a `Guid`, in `HT.Domain.Habits`.
   Each table has its own `Guid` key, created by `Guid.NewGuid()` in the store, so an identifier names at most
   one habit of either kind.
2. **Two types, `CatalogHabitId` and `CustomHabitId`.** The compiler keeps the kinds apart, but every
   cross-kind case of the spec needs a conversion in the caller, which the spec does not describe, and a goal
   needs a union of the two.

## Decision

- `HabitId` identifies catalog habits and custom habits alike. Every use case that takes a habit identifier
  takes a `HabitId`.
- Which kind an identifier belongs to is never inferred from the value: each use case looks it up only among
  its own kind (and, for custom habits, only among the acting user's own), so a wrong kind gives the same
  result as an unknown identifier (constitution III).

## Consequences

- The spec's cross-kind scenarios are written as the spec states them, with no conversion.
- The compiler does not stop a caller from passing a catalog habit's identifier to a custom-habit use case;
  the use cases' "not found" results and their negative tests cover that instead.
- Slice 4 can reference the habit of a goal with one `HabitId` plus the kind it was based on; it decides how a
  goal records the kind. If a later slice needs compile-time separation, it needs a superseding ADR and a
  migration of the stored references.
