# 0005 — Use-case convention: services by area, explicit acting user, results with text

**Status:** Accepted, 2026-11-11. Settles the use-case-convention row of the roadmap (ARCHITECTURE §9,
ADR 0003) in slice 1, `specs/001-accounts-and-access`. Applies to every later slice.

## Context

ARCHITECTURE §2 has no REST API: Blazor calls Application use cases through dependency injection. ADR 0003
makes those use cases the acceptance-test surface. The isolated test writer sees only their public API, in
reference assemblies, plus the spec. So the shape of that surface decides how directly an acceptance
scenario maps to a call.

Constraints:

- Constitution III: every use case that reads or changes private data takes the acting user as input and acts
  only on that owner; lookups of private data by identifier alone must not exist.
- Constitution II: no clock reads in Domain or Application; time enters through `TimeProvider`.
- The spec gives each refusal and each operator result as literal text, with one result per call in a stated
  order (FR-003, FR-013, FR-014, FR-016).

## Options

1. **Application services grouped by area (chosen).** For example, one accounts service for slice 1, with one
   method per use case.
2. **One class per use case.** One type per use case with a single method. Easier to map one scenario to one
   type, but many small types.
3. **A mediator.** Commands and queries go through a mediator library, and the acting user comes from an
   injected current-user accessor. This adds a dependency, and the acting user is hidden from the call.

For results: carry the literal text (chosen), or carry only a named outcome and let the UI map it to text.
With outcome only, acceptance tests could not check the wording the spec promises.

## Decision

- **Services by area.** `HT.Application` exposes public services grouped by area. Each use case is one public
  async method; its last parameter is a `CancellationToken`. When a service grows too large, it is split by
  sub-area rather than mixed across areas. There is no mediator.
- **Explicit acting user.** Use cases for signed-in users take the acting user as their first parameter. The
  acting user is a `HT.Domain` type (the glossary term "Acting user"): either the anonymous visitor, or an
  account's identifier with its role. These use cases:
  - refuse the anonymous visitor with "You are not signed in." (FR-011);
  - take no other account's identifier (FR-010); they act on the acting user's own account or, in later
    slices, on data scoped to that owner.

  Use cases for anonymous visitors (registration, the sign-in check) take no acting user. Operator
  capabilities (administrator creation, demo loading) take no acting user and are called only by the host and
  by tests.
- **Results.** Every use case returns a result type with named outcomes:
  - success carries its value (for example, the account details);
  - each expected refusal or operator result is a named outcome that also carries the spec's literal text
    (for example `EmailTaken` with "An account with this email already exists.");
  - expected refusals are never exceptions. Exceptions are for faults only.

  The texts are defined once, in `HT.Application`, with the wording of the spec and `design/brand-book.md`.
  Pages show the text as given and use the named outcome only to decide where to show it (for example, next
  to the email field).
- **Time.** Services that need the current time take a `TimeProvider` through dependency injection.
- **Composition.** `HT.Application` has one public registration entry point for its services. The host, the
  integration tests and the acceptance tests call it together with the `HT.Persistence` composition entry
  point (ADR 0003).

## Consequences

- Acceptance scenarios call a service method and check its named outcome and its text, so the exact results of
  the spec can be verified.
- User-facing text lives in `HT.Application`. Version 1 is English only. Localization would need a superseding
  ADR.
- Whether a method for signed-in users takes another account's identifier is checked by its negative tests
  (constitution III) and in review, since a dependency rule cannot see what a parameter means.
- Each later slice adds methods to its own area's service, or a new service for a new area.
