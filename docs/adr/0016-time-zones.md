# 0016 — Time zones: the account's own, kept with each goal's settings version

**Status:** Proposed, 2026-11-17. Owner answer in slice 4, `specs/004-goals` (Clarifications, FR-011, FR-024 to
FR-027). Settles the roadmap row "Commitment deadlines and time zones" (SPEC §11.1, first part; ARCHITECTURE
§9). Becomes Accepted when the slice's plan is approved. The owner chose the account's own time zone and
that each goal settings version keeps it (Options 2a). The detailed rules in the Decision (the goal's current
date, the replacement of a pending version, the set of named time zones, the 26-hour gap) are the spec's
critic-revision defaults, which the owner confirmed on 2026-11-17 (spec, `## Clarifications`, "Critic
revision 2026-11-17"). The 48-hour grace period around clock changes (SPEC
§11.1, second part) stays with slice 5 (`recording-and-evaluation`).

## Context

- Every commitment and evaluation period starts and ends at a local midnight, and the 48-hour grace period
  (slice 5) counts from that end. Something must say whose midnight.
- Constitution II: evaluation is a deterministic function of stored facts, the rule version in effect for
  each evaluation, and a supplied current time. Domain and Application never read the clock or the machine's
  time zone. Correcting an old record must give the result it would have had then.
- Users move or travel. The reviewers of the hosted demo are in different places.
- Slice 4 does not let users edit goal settings (roadmap decision R4); slice 8 does.

## Options

1. **One installation time zone, set by the operator.** Cheapest; every user's days change at the
   installation's midnight, which is wrong for anyone elsewhere.
2. **The account's own time zone, set in the profile (chosen).** Taken from the browser at registration,
   changeable later. A change applies from each goal's next commitment or period.
3. **A time zone chosen per goal.** Most flexible, but one user's goals can disagree about what "today" is, and
   the New goal form gains a field nobody expects.

Where the history lives, for option 2:

- a. **Each goal settings version keeps the time zone in effect (chosen).** A change adds a version per goal,
  effective from that goal's next boundary; evaluation reads one version per day, as it will for every other
  setting from slice 8.
- b. A dated time-zone history on the account. One place to store it, but evaluation then combines two
  histories (the account's zones and the goal's settings) and must still work out each goal's next boundary.

## Decision

- Every account has one named time zone (standard time-zone identifiers, for example `Europe/Kyiv`). It is
  private account data: readable and changeable only by the account itself.
- Registration takes the browser's time zone when it is a named one. Otherwise, and for accounts that existed
  before slice 4, the configured administrator and the demo accounts, the account gets the installation's
  default time zone: an operator setting, UTC when absent or invalid.
- A goal is created with its owner's current time zone in its first settings version. A change of the
  account's time zone is measured on each goal's own timeline, from the goal's current date D: the latest
  date whose day, in its own version's time zone, has begun at the change. It adds a version to the goal,
  with only the time zone changed, effective from the day after D (commitment-based and per-day goals) or the
  Monday after D's week (per-week goals). A goal whose start date's day has not begun gets the new time zone in
  its first version. A goal never has more than one version taking effect after its current date: a change
  that finds one (an earlier change not yet in effect) gives that version the new time zone instead of adding
  another. Measuring from the account's date instead could move a day that had already begun after a move
  west (spec FR-012, FR-026).
- A named time zone is exactly `UTC`, or an IANA identifier, zone or link, that contains "/" and does not
  start with `Etc/` (`Europe/Kyiv`, `Europe/Kiev`); Windows names, offsets and abbreviations are not (spec
  FR-024).
- A day runs from midnight to midnight in its version's time zone (23 or 25 hours on a clock-change day); a
  week runs Monday to Sunday. At the switch, the old day's end and the new day's start may leave a gap or an
  overlap as long as the difference of the two offsets (up to 26 hours); each commitment or period keeps its
  own instants, none is merged, split or dropped. After a large move east, the new version's first commitment
  or period may already have begun when the change is made.
- "Today" is the date in the acting user's current time zone at the supplied time.

## Consequences

- No past commitment, period, deadline or corrected result ever moves when a user changes time zone
  (constitution II).
- Slice 4 already adds settings versions (for time-zone changes only), so slice 8's settings changes reuse the
  mechanism instead of introducing it.
- Registration, Profile and the account model change in slice 4: a migration gives existing accounts the
  default time zone, and the Register page sends the browser's zone.
- A gap or overlap at a switch can make one deadline up to 26 hours earlier or later than the old zone would
  have put it (usually a few hours); this is accepted, since records belong to a date (slice 5), not to an
  instant.
- The hosted demo's operator sets the default time zone in the App Service settings (ADR 0015).
