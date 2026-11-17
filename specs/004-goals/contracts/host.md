# Contract: Host, pages and operator log of goals

**Feature**: specs/004-goals · **Plan**: [../plan.md](../plan.md)

This contract covers what the UI tests and the smoke check rely on: the routes, the navigation, the page states
and the log lines. The pages follow `design/` after the design step (plan.md, Work order step 1).

## Routes

| Route | Render mode | Access | Notes |
|---|---|---|---|
| `/goals` | static SSR | `[Authorize]`; Not found (404) for administrators | HP-003 |
| `/goals/{Id:guid}` | static SSR | `[Authorize]`; Not found for administrators and for another account's or an unknown goal | HP-004 |
| `/goals/new` | Interactive Server | `[Authorize]`; Not found for administrators | HP-001, HP-002 |
| `/register`, `/profile`, `/` | unchanged modes | unchanged | HP-010, HP-015 to HP-017 |
| `/catalog`, `/catalog/{Id}`, `/custom-habits/{Id}`, `/admin` | unchanged modes | unchanged | HP-005 to HP-008 |

`/goals/new` takes three query values:

- `habit={guid}`;
- `kind=catalog|custom`;
- `suggestion={n}`, counted from 1.

Each one is optional. A value the use case refuses shows the use case's text in the Habit section. The page still
opens, with no habit chosen. The catalog filter keeps "Only habits I track" in the address as `tracked=true`
(HP-005).

## Navigation (`SiteNav`)

| Audience | Links |
|---|---|
| Registered user | TopNav: Today, Goals, Catalog, "+ New goal", avatar menu. MobileNav menu: Goals among the main links, then "New goal" after the divider, before Profile. "+ New goal" in the bar is hidden on phones (`ht-hide-sm`), as `design/components/MobileNav.md` says (HP-009). |
| Administrator | Today, Catalog, Admin (unchanged). |
| Visitor | unchanged. |

The avatar letter is the first letter of the account name in upper case, or of the email when the account has no
name (HP-017). It comes from the claim `ht:name`, which `HtUserClaimsPrincipalFactory` adds. A name change
refreshes the cookie.

## Page behaviour the tests check

- **New goal**:
  - The sections, the wording and "Create goal" follow HP-001.
  - The summary card's sentence comes from `GoalSummaryText` (HP-002), as do Evaluated, "Met when", "Streak
    counted in", "Late records", the catalog suggestion line and the first-week line.
  - A refusal sits under its `Field`, one at a time.
  - Success navigates to `/goals/{id}`.
- **Goals**:
  - `ht-list` of GoalListItems.
  - The meta line is the schedule, then the first that applies of these:
    1. "first week starts Mon 23 Nov";
    2. "starts Mon 23 Nov";
    3. "next today" or "next Wed 18 Nov";
    4. nothing.
  - EmptyState "No goals yet", with "Browse the catalog" and "Create custom habit" (HP-003).
- **Goal page**:
  - "← Goals", then the name, then the tags.
  - The attempt line "Attempt 1 · started 14 Jul" or "Attempt 1 · starts 23 Nov".
  - The Settings card, with its sentence.
  - The Notice of FR-034 when `Habit.IsInCatalog` is false (HP-004).
- **Catalog and details** (HP-005, HP-006):
  - Cards: "N goals" and "Add another goal", or "Use this habit".
  - "· you track N" after the count.
  - The head button "Use this habit".
  - "You track this · N goals".
  - "Start with this" on each suggested starting goal.
  - "Your goals on this habit".
- **Custom habit** (HP-007):
  - "Save and create goal" goes to `/goals/new?habit={id}&kind=custom`.
  - "Delete" is shown only when `GoalCount` is 0.
- **Admin unpublish** (HP-008):
  - `ConfirmationNeeded` opens `HtDialog` with the text, a sentence that the goals keep working for their owners,
    and the confirm button.
  - The confirm button calls `UnpublishAsync(…, confirmed: true)`.
- **Home** (HP-010): `heading-1` is the greeting, followed by the date, for registered users and administrators.
  A registered user with no goals sees the slice 4 EmptyState.
- **Profile** (HP-015, HP-017):
  - The header shows the name (when there is one), the email, and "member since …" (when known).
  - The Account tab has the "Name" form, with the Notice "Your name is changed.".
  - The Account tab has the "Time zone" form, with the Notice "Your time zone is changed. Your goals use it from
    their next day or week; earlier days keep their time zone.".
- **Register** (HP-016, HP-017):
  - The optional "Name" field.
  - A hidden input marked `data-ht-browser-time-zone`.
  - `App.razor` loads `wwwroot/js/time-zone.js` once. A document-level `submit` listener fills the input from
    `Intl.DateTimeFormat().resolvedOptions().timeZone`, so this works after enhanced navigation too (research
    R-12).

## Operator log (category `HT.Ui.Operator.StartupTasks`)

The lines of slices 1 and 2 are unchanged in text. Their order changes for `Loaded` (HP-013):

1. `Demo catalog: {Result}`, once, first.
2. For each demo email, in `DemoEmails` order:
   - `Demo account {Email}: {Result}`;
   - `Demo custom habits {Email}: {Result}`, only with a custom-habit result;
   - `Demo goals {Email}: {Result}` (new), only with a goal result.

New, at start, only when `Operator:DefaultTimeZone` is present but not a named time zone:
`Default time zone: not a named time zone, using UTC` (Warning).

No line contains a goal name, habit name, account name, unit or amount (HP-011).

## Smoke check (`tools/HT.SmokeCheck`, slice 3 contract extended)

Check 5, `demo <email>`, also:

- reads `/goals` and expects every name of `DemoGoals.DisplayNames(email)`;
- for demo1, reads `/profile` and expects "Demo One".

`SmokeCheckCoverage` gains two entries. The coverage table in `specs/003-demo-hosting/contracts/smoke-check.md`
("Coverage rule") gains the same two rows:

- `CreateDemoAccountAsync`/`profile` (`DemoAccountProfile`), check "5 demo <email>: the account name on Profile";
- `CreateDemoAccountAsync`/`goals` (`IReadOnlyList<DemoGoal>`), check "5 demo <email>: the goal names on Goals".
