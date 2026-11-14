---
status: Living
updated_at: "2026-11-12"
---

# Domain Context — Habit Tracker

<!--
CONTEXT.md is the domain glossary — not a PRD and not a scratch pad. NO implementation
detail here (no datastore/broker/framework names, no API contracts) — only domain words
and the boundaries between them. Implementation choices live in the SAD and ADRs;
behaviour lives in PRD.md.

Multi-context repos: each bounded context has its own CONTEXT.md at its root path
(registered in CONTEXT-MAP.md). System-wide terms that span all contexts live in the
repo-root CONTEXT.md. Never duplicate a term across files — pick one owner.

Terms get fixed inline, the moment they surface in an interview / PRD / review — never
batched «I'll consolidate later». Empty H2 → prune before commit; keep only the sections
that carry real content. ## Glossary is mandatory; the other two are optional.
-->

## Glossary

<!-- One line per term: name · one-sentence canonical definition · one-sentence boundary
     (what it is NOT / the concept it gets confused with). Alphabetical once there are a few. -->
- Account — one person's way into the app, identified by a unique email address and holding exactly one role: registered user or administrator. NOT a public identity (the email is never shown publicly; a public display name exists only after community opt-in), and NOT a personal goal or any of its tracking data.
- Acting user — the account, with its role, on whose behalf the application performs an action, or the anonymous visitor when nobody is signed in; it is established only by signing in. NOT the owner of the data being asked for (an administrator who asks about another account is still only the acting user, and gains no access by asking).
- Administrator — an account with the administrator role, which maintains the public catalog, reviews suggestions and feedback, and deactivates or reactivates accounts. NOT a super-user: the role never grants access to another person's private goals, activity, notes or personal reports.
- Anonymous visitor — someone using the app without being signed in, who can browse public pages only; a registered user who is signed out in a browser is an anonymous visitor there. NOT an account (the visitor's account, if they have one, still exists but plays no part until they sign in), and never the owner of any private data.
- Catalog habit — a public, reusable description of a behavior (e.g. Reading, Yoga), maintained by administrators and visible to anyone once published. NOT a personal goal (one user's measurable commitment based on a habit), and NOT a signal that anyone's participation is public.
- Catalog habit state — where a catalog habit stands in curation: Draft (created, never published), Published (visible to everyone) or Unpublished (taken out of the public catalog after being published); only an administrator sees Draft and Unpublished habits. NOT a goal's status (Pending, Met, Failed, Missed belong to evaluations), and NOT deletion (an unpublished habit still exists and can be published again).
- Category — one grouping from a fixed list (Body, Mind, Health, Money, Home, Other) that every catalog habit and custom habit belongs to, used to filter the catalog. NOT a tag a user invents, and NOT the habit's intent.
- Custom habit — a behavior created privately by one registered user for their own goals, visible only to its owner; administrators have none. NOT a catalog habit (never public, never in community statistics, not visible to administrators).
- Demo account — a synthetic registered-user account in the reserved `example.com` domain (`demo1@example.com` to `demo3@example.com`), created by demo loading at start with the operator's demo password, so that reviewers can sign in without registering and later slices have users to attach their demo data to; its private demo data (such as custom habits) is created only together with the account. NOT a real person's account (it carries no real name or address), and NOT an administrator (the demo data has no administrator of its own).
- Excused — the user-chosen state of an individual scheduled commitment in a commitment-based goal that exempts it from evaluation, requiring no approval and optionally carrying a reason, and that can be set only while the commitment is upcoming or Pending, never after it has been finalized as Missed or Failed; it is excluded from consistency and preserves but does not extend the streak. NOT Pause (a pause suspends a whole goal over a date interval and applies to period-based goals too; excused covers one commitment of a commitment-based goal), and NOT Pending (pending is temporary and resolves on its own; excused is the user's deliberate, lasting choice).
- Failed — the finalized outcome of a commitment or evaluation period whose recorded activity did not meet the applicable target: below the required amount for a Build goal, or above the permitted amount for a Reduce goal. NOT Missed (a missed outcome has no record at all), and NOT partial progress (the recorded amount is preserved and reported; Failed is only the evaluation verdict on it).
- Intent — Build or Reduce as the direction a catalog habit or custom habit is usually pursued in, shown with the habit and used as a catalog filter. NOT a personal goal's Build or Reduce, which the goal itself defines (SPEC §5.1).
- Missed — the finalized outcome of a commitment or evaluation period for which no activity was recorded by the end of the 48-hour grace period; it counts as unsuccessful in consistency and resets the streak. NOT Failed (a failure has a record that fell short of the target), and NOT Pending (pending is still inside the grace period and not yet final).
- Operator — the person who runs a deployment and holds its secret configuration (administrator credentials, demo password), and reads its operator log. NOT an administrator (an operator is not an account and has no role in the app), and NOT one of the three audiences.
- Pending — the not-yet-final state of a commitment or evaluation period that has ended but whose 48-hour grace period is still open, whether or not any activity has been recorded so far; it is excluded from consistency and never resets a streak. NOT Missed (missed is final once the grace period has elapsed with no record), and NOT Excused (excused is a deliberate, lasting exemption by the user, while pending is temporary and resolves on its own).
- Registered user — an account with the registered-user role, which owns its private goals, activity, notes, custom habits and personal reports. NOT an administrator (registration never grants that role), and NOT a community participant (participation is off until the user opts in).
- Seed catalog — the catalog habits that demo loading creates when the catalog is empty, so that a new deployment shows a full catalog. NOT a protected or system-owned part of the catalog (administrators edit and unpublish seed habits like any other), and NOT demo users' private data.
- Sign-in block — a temporary state of an account, after 5 failed sign-ins in a row, in which sign-in fails for 5 minutes even with the correct password. NOT deactivation (an administrator's lasting decision that ends all access), and never shown as such to the person signing in.
- Suggested starting goal — an optional, fully customizable example target shown with a catalog habit (up to five per habit, one of them shown on the catalog card), described by the same fields a goal starts from: Build or Reduce, measurement, amount and unit, and schedule. NOT a prescribed target or coaching advice, and NOT a personal goal until a user creates one from it.
