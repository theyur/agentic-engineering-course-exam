# 0017: Embedded time-zone data: NodaTime inside HT.Domain

**Status:** Accepted, 2026-11-18, by the owner. Comes from the plan of slice 4, `specs/004-goals` (research R-01;
plan, "Gates before the next step"). It applies [ADR 0016](0016-time-zones.md)
(time zones) and settles the technical half of ARCHITECTURE §9's "Timezone rules".

## Context

- ADR 0016 makes every commitment and evaluation period start and end at a local midnight, in a named time zone
  (an IANA identifier or `UTC`). Slice 5's 48-hour grace period counts from those ends.
- Constitution II requires evaluation to be a deterministic function of stored facts, the rule version and a
  supplied time. Recomputing an old result must give the same instants on any machine.
- The app runs on three kinds of machine:
  - Windows developer machines, where .NET maps IANA identifiers through Windows' ICU;
  - Ubuntu CI;
  - the Azure App Service host (ADR 0015).

  The last two use their own tzdata package. Each of the three updates its time-zone rules on its own schedule.
- Spec FR-024 and HP-015 need to know whether an identifier is a zone or a link. They also need the list of
  zones.
- Some zones change their clocks at midnight, so that day has no 00:00.

## Options

1. **NodaTime, with its embedded tzdb, used only inside `HT.Domain` (chosen).** It brings one package, and every
   machine uses the same rules: the version the package ships.
   - It provides zone and link data (`TzdbDateTimeZoneSource`).
   - `AtStartOfDay` gives the first instant of a day where midnight does not exist.
   - `LocalDate` arithmetic carries no hidden time.
2. **BCL `TimeZoneInfo` with IANA identifiers.** It adds no package, but has three costs:
   - the rules differ by machine;
   - there is no public way to tell a link from a zone, and on Windows no way to list IANA identifiers. So the
     Profile list of spec HP-015 (zones, not links) would be an identifier list kept by hand in source;
   - missing midnights need hand-written handling.
3. **NodaTime on the public surface** (`LocalDate`, `Instant` in use-case results). This is more precise. But the
   acceptance-test project (ADR 0003) and every page would depend on NodaTime, and the spec speaks in dates and
   instants.

## Decision

- `HT.Domain` references NodaTime 3.2.x, pinned to one patch version in `HT.Domain.csproj`. No other project
  references it directly.
- Every time-zone conversion and every date calculation of goals goes through `DateTimeZoneProviders.Tzdb`.
  Architecture rules forbid the following in Domain and Application:
  - `SystemClock`, the BCL provider and `TimeZoneInfo` lookups or conversions;
  - a NodaTime type in any public signature of Domain or Application.

  The public types are `DateOnly`, `DateTimeOffset` (UTC) and `NamedTimeZone`, which holds the identifier string.
- A NodaTime upgrade that changes the tzdb version is a reviewed dependency change. Its pull request names the
  tzdb version, as for any package update.

## Consequences

- The developer machine, CI and the hosted demo compute the same midnights for the same stored versions. This
  holds after the hosts update their OS time-zone data, too.
- Rule changes reach the app only through a package update. The hosted demo may lag a real-world rule change
  until then, which demo scale accepts.
- The domain layer has its first external package. `DomainDependencyTests` admits exactly this one.
- The acceptance-test surface stays BCL-only, so the AC project needs no new package.
