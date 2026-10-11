# Habit Tracker — Architecture Foundation

**Status:** Initial architectural baseline for the Agentic Engineering course exam  
**Scope:** Technology choices and boundaries only; no solution implementation plan

## 1. Architectural objectives

The application is an intentionally bounded **agentic software engineering exercise** built with Claude Code. Architecture should make business rules explicit, preserve separation of concerns, and permit meaningful automated verification—without adding infrastructure solely for hypothetical future clients or scale.

Key priorities, in order: understandable business behavior; enforceable architectural boundaries; automated correctness checks; reliable browser-based demonstration; low-cost hosting.

`SPEC.md` is the source of truth for **what the application does**. This document describes **how its major responsibilities are separated and which technologies are chosen**. Detailed domain models, interfaces, tables, UI component designs, endpoint contracts, and delivery plans are deferred.

## 2. Technology decisions

| Concern | Decision | Rationale |
| --- | --- | --- |
| Language / runtime | **C# / .NET 10** | Familiar ecosystem and focus on engineering with agents rather than learning a new stack. |
| UI | **Blazor Interactive Server** | Browser-based, interactive UI; suitable for a single .NET web application. |
| API boundary | **No separate REST API in v1** | Blazor invokes application use cases directly; no second client is planned. |
| Persistence | **Entity Framework Core + Microsoft SQL Server** | Relational persistence with a familiar ORM and schema migration support. |
| Authentication | **ASP.NET Core Identity**, local email/password | Use established framework mechanisms rather than custom password/session management. |
| Hosting | **Azure**, low-cost public demo | Browser access for reviewers on Windows/macOS without local setup. |
| Architectural style | **Clean / Onion Architecture principles** | Clear dependency direction; domain independent of frameworks and infrastructure. |

EF Core migrations are the agreed mechanism for database schema evolution. This document does not select Azure resource types, pricing tiers, deployment tooling, or CI/CD strategy.

## 3. Solution boundaries

Use four production projects:

| Project | Responsibility |
| --- | --- |
| `HT.Domain` | Business terminology, invariants, and domain-level rules. No dependencies on UI, persistence, or framework-specific infrastructure. |
| `HT.Application` | Application use cases, workflow coordination, and inward-facing abstractions for external capabilities. Depends inward on Domain. |
| `HT.Persistence` | EF Core / SQL Server integration and implementation of persistence contracts established by inward layers. |
| `HT.Ui` | Blazor interactive server UI, presentation, authentication/authorization integration, and application composition/startup. |

### Dependency principles

- `HT.Domain` is the innermost layer; it must not reference the other three projects.
- `HT.Application` may depend on `HT.Domain`, but not on `HT.Persistence` or `HT.Ui`.
- `HT.Persistence` depends inward as necessary to implement persistence capabilities; it must not define business behavior that belongs to Domain or application workflow decisions that belong to Application.
- `HT.Ui` depends on `HT.Application` and participates in composing the application's infrastructure, but UI components should not access persistence implementations directly.
- The runtime call flow may invoke infrastructure through abstractions defined inward; **runtime flow and source-code dependency direction are not identical**.
- Keep business rules out of Blazor components and SQL-specific details out of Domain and Application.

These boundaries must be enforced through tests, not only prose, using ArchUnitNET (see §6).

## 4. Web and security boundary

- Render the product as a web application; no native mobile or desktop client.
- Use Blazor **Interactive Server**; UI interactions invoke Application functionality through dependency injection, not through an unnecessary local HTTP round trip.
- ASP.NET Core Identity supports local email/password registration, sign-in, and established authentication mechanisms.
- Authorization distinguishes anonymous visitors, registered users, and administrators.
- Access to a user's private goals, activity, notes, and reports must be controlled by ownership rules. Administrator privileges **must not bypass** the product's private-tracking boundary.
- Deactivation must prevent continuing account access; session invalidation/revalidation requirements need to be covered in implementation planning and tests.
- Avoid designing custom cryptographic, password-storage, or session-management protocols.

## 5. Persistence boundary

- SQL Server is the sole selected database provider for v1; SQLite is not included as a parallel development provider.
- EF Core and database-specific concerns are contained in `HT.Persistence`, using inward-defined contracts where appropriate.
- Use EF Core migrations for schema changes.
- Local execution may require SQL Server or access to an existing SQL Server instance; reviewers should interact with the Azure-hosted app from a browser.
- Storage schema, entity mappings, transaction boundaries, aggregation optimizations, and migration rollout procedures are **future design work**, not part of this foundation.

## 6. Testing and quality constraints

Testing is a part of feature completion, not an afterthought.

| Test scope | Required intent |
| --- | --- |
| **Unit** | Verify domain behavior, including targets, grace periods, corrections, pauses, consistency, and streak semantics. |
| **Integration** | Verify application workflows and real persistence behavior, including EF Core + SQL Server integration. |
| **Architecture** | Verify layer dependency rules and prevent business logic from crossing structural boundaries. |
| **Selective UI** | Verify important Blazor interactions and authorization-sensitive experiences when useful. |

Selected testing libraries:

- **xUnit** — test framework for unit, integration, and architecture tests.
- **ArchUnitNET** (`TngTech.ArchUnitNET` + `TngTech.ArchUnitNET.xUnit`) — architecture tests: layer dependency rules and cross-boundary constraints (e.g., UI must not depend on persistence types; inward-defined abstractions are implemented only in outer layers).

Do not prescribe individual test cases, test-project count, or end-to-end browser infrastructure at this stage.

## 7. Hosting and operation

- Target **Azure-hosted, browser-accessible demonstration** with cost efficiency prioritized over scale.
- A reviewer on macOS or Windows should be able to access and explore the demo without installing the application or database locally.
- Include representative sample/demo data to make personal reports, catalog content, and community statistics demonstrable; generation and provisioning mechanisms are deferred.
- Do not require autoscaling, high availability, multi-region deployment, or production-grade disaster recovery for v1.
- Hosting service selection, infrastructure-as-code, secrets management mechanics, observability stack, pipelines, and operational runbooks belong to subsequent plans. Use secure defaults and do not embed secrets in source control.

## 8. Documentation and change control

Only **two foundation files** are created now, both at the repository root:

- `SPEC.md` — product scope, behavior, roles, rules, non-goals, and open product questions.
- `ARCHITECTURE.md` — stack, architecture boundaries, rationale, quality and deployment principles.

Future documentation conventions (not files to create now):

- `docs/adr/` — significant architectural decisions, numbered, e.g. `0001-initial-setup.md`.
- `docs/tasks/` — feature and bug-fix task specifications, date-prefixed, e.g. `2026-10-11-implement-auth.md`.

Initial rationale stays in this document. **Do not create ADRs, tasks, domain-model plans, database designs, or scaffolding as part of this foundation step.** Future agents should resolve relevant open questions explicitly during feature planning and document material architectural changes in ADRs when warranted.

## 9. Explicitly deferred technical choices

- Domain and persistence models, schema and tables.
- Concrete use-case interfaces, services, component hierarchies, or endpoint contracts.
- Authentication UX particulars, email verification, password recovery, and outbound email infrastructure decisions.
- Timezone rules, scheduled evaluation mechanics, background processing, and caching.
- Exact organization of test projects; libraries for integration-test databases, mocking, and UI testing.
- Azure service SKUs, deployment infrastructure, pipelines, and monitoring.
- Production optimization and scaling strategy.

These are **deliberate unknowns**, not invitations for an implementation agent to silently choose them without feature-level deliberation.
