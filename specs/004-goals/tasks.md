---

description: "Task list for slice 4, goals"
---

# Tasks: Goals

**Input**: Design documents from `specs/004-goals/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0016](../../docs/adr/0016-time-zones.md),
[ADR 0017](../../docs/adr/0017-embedded-time-zone-data.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F004/` are locked. No task edits them,
the lock files, the run records or anything under `tests/HT.AcceptanceTests`.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 8).

**Organization**: Phase 2 follows the plan's Work order steps 1 to 5 for what every story needs:

- design;
- the whole of `HT.Domain.Time` and `HT.Domain.Goals`;
- the shared Application types;
- the account columns and the new account-store signature;
- the migration and the goal store's core;
- the host's default time zone.

Each story phase then adds its use cases, its store members, its tests and its pages. The story phases follow
the spec's priorities (P1, then P2, then P3). Within P1, US4 comes before US3, because US3's privacy tests cover
every goal use case, the evaluation calendar included.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US11).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every Application method and every port method is `async` and takes a `CancellationToken` last.
- Take message texts from `GoalTexts`, `AccountTexts`, `CatalogTexts`, `CustomHabitTexts` and `OperatorTexts`.
  Never retype one. Tests assert the literal text from spec.md, so a typo in a constant fails a test.
- Time and culture stay out of `HT.Domain` and `HT.Application` (constitution II; research R-01, R-11, R-13):
  - Time enters only as `TimeProvider` in the services, and as an instant parameter in Domain.
  - Time zones go only through `NamedTimeZone`, backed by NodaTime's `DateTimeZoneProviders.Tzdb`.
  - Date and month texts come only from `DateTexts`.
  - Names compare through `HabitName.KeyOf`, never through `ToUpper()`, `ToLower()`, `string.Compare(a, b)`,
    `CompareTo` or a culture comparer.
- No NodaTime type appears in any public or protected signature of `HT.Domain` or `HT.Application` (ADR 0017).
  Public members use `DateOnly`, `DateTimeOffset` in UTC and `NamedTimeZone`.
- Goals are private (constitution III):
  - They are reached only through `GoalService` and owner-scoped `IGoalStore` members. Every call passes the acting
    user's `AccountId` as the owner.
  - The one member without an owner is `CountUsingCatalogHabitAsync`, which returns an `int`.
  - Administrators get no goal of any account.
  - No goal name, custom-habit name, account name, unit or amount goes into a log template, an exception message
    or an error page (HP-011, research R-08).
- Test instants are UTC literals worked out by hand from the spec's local times, never computed by the code under
  test. For example, 12:00 in Kyiv on Wednesday 18 November 2026 is `2026-11-18T10:00:00Z`.
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their `design/pages/`
  preview after the design tasks of Phase 2 (constitution IV, HP-012). Parts of slices 5 to 11 are absent, not
  disabled (ADR 0008).
- Interactive pages call use cases only through `UseCaseScope` (slice 2 research R-15). Static pages inject the
  services.
- Never read, print or embed a secret. That includes the `Operator:*` User Secrets values. `Operator:DefaultTimeZone`
  is not a secret, but tests still use literal zone values and never the developer's configuration.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests` enforces
  it). Tests use public members only, and arrange or inspect rows with plain SQL (`tests/HT.IntegrationTests/Sql.cs`)
  or through the public ports. The one reflection is the HP-019 test's lookup of `HtDbContext` by type name
  (research R-14).
- Databases:
  - integration tests use `HT-IT` (`DatabaseFixture`), `HT-IT-SEED` (`SeedDatabaseFixture`) and `HT-IT-MIGRATE`
    (T004);
  - host tests use `HT-UI` and its derived databases;
  - no test touches `HT` or `HT-AC`.

  Tests in shared databases use account emails, habit names and goal names with their own prefix. They check by
  inclusion, exclusion and relative order, never by totals.
- Commit only at a checkpoint, with the full suite green (constitution V). These land in the same commit, so
  `ValidateOnBuild` never meets an unregistered port (plan, Work order 3 and 4):
  - a port member, its `HT.Persistence` implementation, its `FakeGoalStore` (or other fake) member and its
    registration;
  - a service and the store it resolves.
- A changed service constructor breaks its hand-built callers. Many are target-typed
  (`private CatalogService Service => new(_store);`), so search for the type name, not for `new <Service>(`:
  `grep -rnw "<Service>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`). Update every hit in the
  same task, and let the build confirm that none is left. The bUnit page tests resolve the services through
  `tests/HT.UiTests/Components/Pages/PageServices.cs`, so a service that gains a port needs that port registered
  there in the same task (T040). The locked F001 AC support code also builds `AccountService`, `OperatorService` and
  `DemoDataService` by hand, and it will stop compiling. That is expected (plan, Risks). Nobody reads or fixes it
  here: the F004 run of `/speckit-ac-tests-write` does.
- The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state, a baseline of the public API, the NodaTime package, and the migration test
database.

- [X] T001 Set slice 4's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`
- [X] T002 [P] Save the public API baseline before any code change (quickstart §1, research R-07):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member` and the same for `HT.Domain` into two files in the session scratchpad, never in the repo. T133 compares them.

  The scratchpad does not survive a reboot or a later session. If T133 finds the files missing, it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`, the state T002 captured; `master` itself may have gained API from other merges since) in a temporary `git worktree` under the scratchpad (restore, then the same two commands), and removes that worktree afterwards.
- [X] T003 Add NodaTime to `src/HT.Domain/HT.Domain.csproj` (ADR 0017, research R-01):
  - look up the latest `NodaTime` 3.2.x patch on nuget.org;
  - **ask the owner first**, naming the package, the exact version, the source (nuget.org) and its approximate size, and restore only after an explicit yes;
  - add `<PackageReference Include="NodaTime" Version="3.2.N" />` with that exact version. No other project references it directly;
  - record the version in ADR 0017's Decision section ("pinned to one patch version");
  - run `dotnet restore HabitTracker.slnx` again, because Roslynator needs it after a new package.
- [X] T004 [P] Create `tests/HT.IntegrationTests/MigrationDatabaseFixture.cs` for HP-019 (research R-14), on the pattern of `SeedDatabaseFixture`:
  - its connection string is `HT_IT_CONNECTION` (or `DatabaseFixture`'s local default), with the database name replaced by `HT-IT-MIGRATE` through `SqlConnectionStringBuilder`, so CI needs no extra variable;
  - the same guard as `DatabaseFixture` against `HT` and `HT-AC`;
  - a `FakeTimeProvider` clock;
  - a collection `MigrationDatabase` with `DisableParallelization = true`.

  It builds no service provider. The HP-019 test (T049) composes its own, because it migrates in two steps.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: everything every story needs:

- the design changes;
- `HT.Domain.Time` and `HT.Domain.Goals`, with their boundary tests;
- the texts, views and the `IGoalStore` port with its core members;
- the account profile columns and the new `IAccountStore.CreateAsync` with its callers;
- the migration `Goals`;
- the host's `InstallationTimeZone`;
- the KeyValue component.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slices 1 and 2:

- Texts that disagree with FR-010, HP-002 or `CONTEXT.md` are **replaced** everywhere.
- A **slice-4 state** is **added** beside the full preview, as a "Slice 4 state" block after it. The full previews
  stay whole, because they are the design source of slices 5 to 11. "No …" in a task below describes the slice-4
  state; it never deletes anything from the full preview.

- [X] T005 [P] Update `design/pages/PageNewGoal.html` and `.md` (HP-001, HP-002):
  - the head line for a catalog habit ("Based on the catalog habit Reading. Change anything you like."), for a custom habit, and with no habit;
  - the Habit section with Change, and the habit chooser: a Dialog with a SearchField over Published catalog habits and the user's custom habits;
  - "How do you measure it?" and "When?" as ChoiceCards (`design/components/ChoiceCard`);
  - the refusal states of FR-006, one at a time under their field;
  - a start date in the future;
  - the summary card for each schedule kind (Each scheduled day, Each day, Each week). It shows:
    - the habit-independent sentence of HP-002, replacing "Read at least 15 minutes on Mon, Tue, Wed, Thu and Fri";
    - Evaluated, "Met when" (per FR-004), "Streak counted in" (commitments, days or weeks) and "Late records";
    - the "Catalog suggestion: … An example to adjust, not a prescribed target." line;
    - the line "Your first week starts on Mon 23 Nov. Days before it are not evaluated.";
  - "Create goal" with no confirmation;
  - "Streak counted in Days" for a commitment-based goal becomes "commitments" (constitution I).

  The "Met when" values and the card's incomplete-form state are settled here, and `GoalSummaryText` (T059) uses them as written.
- [X] T006 [P] Add the slice-4 state to `design/pages/PageGoals.html` and `.md`, and to `design/components/GoalListItem.html` and `.md` (HP-003):
  - "Goals" with no Tabs;
  - one `ht-list` of GoalListItems, each with the name link, the Build or Reduce Tag (never "intent", `CONTEXT.md`) and the meta line;
  - the meta line: the schedule text of T026 ("Mon · Wed · Fri", "Mon–Fri", "Every day", "2 times a day", "Flexible · 3 times a week"), then " · " and the four variants in their order of precedence: "first week starts Mon 23 Nov", "starts Mon 23 Nov", "next today" or "next Wed 18 Nov", and nothing;
  - no consistency, streak, strip or action;
  - the EmptyState "No goals yet" with "Browse the catalog" and "Create custom habit".

  Fix the goal names the previews get wrong: "Yoga, 3 times a week" for fixed days Mon/Wed/Fri becomes "Yoga, 3 days a week", and "Coffee, at most 2 cups" becomes "Coffee, at most 2 cups a day".
- [X] T007 [P] Add the slice-4 state to `design/pages/PageGoalDetail.html` and `.md` (HP-004):
  - "← Goals", the name, and the tags: category, Build or Reduce, Commitment-based or Period-based;
  - the attempt line "Attempt 1 · started 14 Jul" or "Attempt 1 · starts 23 Nov";
  - the Settings card as KeyValue rows: Habit with "catalog" or "custom", "Build or reduce", Measure, Target, Schedule, Evaluated and Started, with the sentence "Changes apply from the next commitment. Past results keep their rules.";
  - the Notice "This habit is no longer in the public catalog. Your goal keeps working." under the page head;
  - no actions, GoalStats, trend, history, pauses, notes or earlier attempts.

  In the full preview too:
  - the "Intent" row becomes "Build or reduce";
  - "Evaluated: Each commitment" becomes "Each scheduled day";
  - "Streak, sessions" for a commitment-based goal becomes commitments.
- [X] T008 [P] Add the slice-4 state to `design/pages/PageCatalogSignedIn.html` and `.md`, and to `design/components/HabitCard.html` and `.md` (HP-005):
  - the "N goals" Tag and "Add another goal" on a tracked habit's card;
  - "Use this habit" on the others;
  - "Show: Only habits I track" in the FilterPanel;
  - "· you track N" after the count.
- [X] T009 [P] Add the slice-4 state to `design/pages/PageHabitDetail.html` and `.md` (HP-006), for registered users:
  - the head button "Use this habit";
  - the Tag "You track this · N goals" when N is at least 1;
  - "Start with this" on each suggested starting goal;
  - "Your goals on this habit", each name linking to its goal page;
  - no consistency, streak or community card.
- [X] T010 [P] Add the slice-4 state to `design/pages/PageCustomHabit.html` and `.md` (HP-007): "Save and create goal" beside "Save habit", and the edit state of a habit that goals use, without "Delete"
- [X] T011 [P] Add the unpublish confirmation Dialog to `design/pages/PageAdmin.html` and `.md` (HP-008, `design/components/Dialog`): "N goals use this habit.", a sentence that the goals keep working for their owners, Cancel and the confirm button. The table keeps having no "Updated" column (slice 2 HP-009)
- [X] T012 [P] Add the slice-4 state to `design/pages/PageHomeSignedIn.html` and `.md` (HP-010, FR-029):
  - the page head with the greeting as `heading-1` and today's date, with and without a name, for registered users and administrators. It replaces the "Today" heading;
  - a registered user's slice-4 EmptyState, which replaces "Goals come in a later version.";
  - nothing more with goals (slice 6).

  Write the EmptyState's title, text and actions here. T109 tests them as written.
- [X] T013 [P] Update `design/pages/PageProfile.html` and `.md` (HP-015, HP-017):
  - the Account tab's "Name" field with Save and the Notice "Your name is changed.";
  - the "Time zone" field (a select) with Save and the Notice "Your time zone is changed. Your goals use it from their next day or week; earlier days keep their time zone.";
  - the header with the name as its `h1`, then the email and "member since November 2026", plus its states without a name (the email as the `h1`, as now) and without "member since";
  - the avatar letter from the account name.
- [X] T014 [P] Add the optional "Name" field to `design/pages/PageRegister.html` and `.md` (HP-017). Say in the notes that the browser's time zone is sent with no visible field (HP-016)
- [X] T015 [P] Update `design/components/TopNav.html` and `.md` and `design/components/MobileNav.html` and `.md` (HP-009, HP-017):
  - "Goals" and "+ New goal" for registered users only;
  - in MobileNav, Goals among the main links, and "New goal" after the divider, before Profile;
  - the bar's "+ New goal" hidden on phones (`ht-hide-sm`);
  - the avatar letter from the account name, or from the email without one.
- [X] T016 Add the styles this slice needs to `design/css/ht-components.css` from existing tokens: the reduced GoalListItem, ChoiceCard, KeyValue as a component, the summary card, and any rule the changed previews need and the file lacks. Change `design/tokens.json` and `design/css/tokens.css` only if a new token is needed (depends on T005–T015)
- [X] T017 Update `design/pages.md` and `design/README.md` with the slice-4 structure of New goal, Goals, the goal page, the catalog, details, custom habit, Admin, both Homes, Profile, Register and the navigation. Then:
  - grep `design/` for the texts the spec lists as wrong, and fix any T005–T015 missed: an "intent" Tag or row for a goal's own Build or Reduce, "Streak counted in Days", "Streak, sessions", "Each commitment", "3 times a week" on fixed days, and "2 cups" without "a day";
  - in `docs/roadmap.md` ("Pages across slices"), adjust the slice-4 entries only where they now differ from what the previews show.

  Depends on T005–T016.
- [X] T018 Copy every `design/css` file that T016 changed to `src/HT.Ui/wwwroot/css/` and run `dotnet test tests/HT.UiTests --filter "FullyQualifiedName~DesignSyncTests"` (depends on T016)

### Domain: time (`src/HT.Domain/Time/`, namespace `HT.Domain.Time`; research R-01, R-02)

- [X] T019 [P] Create `NamedTimeZone.cs`, a readonly record struct (depends on T003):
  - `Id`, as given, and `static Utc`;
  - `static bool TryParse(string? text, out NamedTimeZone zone)`, following FR-024: exactly `UTC`, or an identifier in `DateTimeZoneProviders.Tzdb.Ids` that contains `/` and does not start with `Etc/`. It is ordinal and case-sensitive, and it does not trim;
  - `DateAt(DateTimeOffset) → DateOnly`, `TimeAt(DateTimeOffset) → TimeOnly`, and `StartOfDay(DateOnly) → DateTimeOffset` in UTC through `DateTimeZone.AtStartOfDay`, so a day whose midnight does not exist starts at its first instant.

  A link keeps its identifier and uses its zone's rules. NodaTime stays internal.
- [X] T020 [P] Create `NamedTimeZones.cs`: `Choices` is `UTC` first, then every canonical tzdb zone (`CanonicalIdMap` value equal to its key) that `NamedTimeZone.TryParse` accepts, in ordinal order (HP-015) (depends on T003)
- [X] T021 [P] Create `tests/HT.UnitTests/Domain/Time/NamedTimeZoneTests.cs` and `NamedTimeZonesTests.cs` (depends on T019, T020):
  - every FR-024 example:
    - accepted: `UTC`, `Europe/Kyiv`, `America/Argentina/Buenos_Aires`, and the links `Europe/Kiev`, `Asia/Calcutta` and `US/Eastern`, each kept as given;
    - refused: `FLE Standard Time`, `Etc/GMT+3`, `Etc/UTC`, `EST`, `CET`, `GMT`, `+02:00` and `Mars/Olympus`;
    - also refused: `" Europe/Kyiv"` (not trimmed), `europe/kyiv` (case), null and empty;
  - `DateAt` and `TimeAt` at midnight ± 1 second in Europe/Kyiv, America/New_York and UTC;
  - `StartOfDay` in Kyiv on 2027-03-28 and 2027-10-31, and on a date whose midnight the embedded tzdb skips (America/Havana in March, for example). Confirm that zone's transition date before fixing the literal;
  - `Choices`:
    - starts with `UTC`;
    - contains `Europe/Kyiv` and `America/New_York`;
    - contains no link (`Europe/Kiev`) and no `Etc/` identifier;
    - is in ordinal order;
    - every entry passes `TryParse`;
  - one test run inside `CultureScope` with a non-English culture, to show no culture is read.

### Domain: goals (`src/HT.Domain/Goals/`, namespace `HT.Domain.Goals`; [data-model.md](data-model.md#domain-goals-htdomaingoals))

- [X] T022 Factor the shared parts out of slice 2's types, without changing their output (research R-05):
  - an internal `TargetFieldCheck` in `src/HT.Domain/Habits/TargetFieldCheck.cs`, the FR-007 target checks in their order, which `SuggestedStartingGoal.TryCreate` now calls;
  - internal amount, schedule and day-list text parts in `src/HT.Domain/Habits/TargetTextParts.cs`, which `SuggestedStartingGoalText` now calls.

  `SuggestedStartingGoal`, `SuggestedStartingGoalProblem` and `SuggestedStartingGoalText` keep their public members and results exactly. Run `dotnet test tests/HT.UnitTests --filter "FullyQualifiedName~SuggestedStartingGoal"`: it must pass unchanged.
- [X] T023 [P] Create the value types, one per file:
  - `GoalId` (`Guid Value`);
  - `HabitKind` (`Catalog`, `Custom`);
  - `GoalHabit` (`HabitId Id`, `HabitKind Kind`);
  - `GoalScheduleKind` (`CommitmentBased`, `PeriodBased`);
  - `GoalField` (the eleven values of plan "Spec discrepancies" 3);
  - `GoalProblem` (the FR-006 order of data-model.md);
  - `GoalTargetFields` (one nullable value per form control);
  - `GoalName`: trimmed, 1 to 60 UTF-16 code units, empty means none, and `TryCreate(string?, out GoalName?, out bool tooLong)`.
- [X] T024 Create `GoalTarget.cs`, `GoalSettings.cs`, `GoalSettingsVersion.cs`, `GoalAttempt.cs` and `Goal.cs` as data-model.md describes (depends on T022, T023):
  - `GoalTarget` reuses slice 2's `Measurement`, `DurationUnit`, `Schedule`, `ScheduleKind`, `SchedulePeriod` and `BuildOrReduce`, stores no value that does not belong to the measurement or schedule, and gives `ScheduleKindOf` and `PeriodLength`;
  - `Goal`'s constructor enforces the invariants of data-model.md. `Versions` is non-empty, the first applies from `StartDate`, and the dates strictly increase.
- [X] T025 Create `GoalDraft.cs` (depends on T024):
  - `CheckHabit(GoalHabit?)`;
  - `TryCreate(GoalTargetFields, DateOnly? startDate, DateOnly today, string? name, out GoalTarget, out DateOnly, out GoalName?) → GoalProblem`, which checks:
    1. the target checks through `TargetFieldCheck`, with the goal's yes/no problem;
    2. `ReduceAmountNeedsOneTime`;
    3. `StartDateMissing`, `StartDateInPast` (before `today`) and `StartDateTooFar` (after `today` + 365);
    4. `NameTooLong`;
  - `FieldOf(GoalProblem) → GoalField` for every problem except `None`.
- [X] T026 Create `GoalText.cs` (depends on T022, T024):
  - `TargetText(GoalTarget)` and `Kind(GoalTarget)`, through `TargetTextParts`;
  - the one exception: Reduce, yes/no, fixed days gives "avoided on " followed by the days as `Kind` writes them, or "avoided every day";
  - `Name(Goal goal, string habitName)`: the own name, or the habit name, ", " and the target text;
  - `ScheduleText(GoalTarget)`, the schedule of the Goals page's meta line and of the Settings card's "Schedule" row (HP-003, HP-004, US2-AS6). The spec gives only examples, so this task fixes the rule:
    - fixed days: "Every day" for all seven; otherwise the days in week order as `Kind` groups them (a run of three or more as first–last), joined by " · " instead of ", ": "Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat";
    - N times per day: "Every day" for 1 time and "N times a day" otherwise, as the full `PageGoals` preview writes its daily goals ("Every day" for "Coffee, at most 2 cups a day"), so the slice-4 state and the full preview agree;
    - N times per week: "Flexible · once a week" or "Flexible · N times a week", as the preview's "Flexible · …".

    "Every day" is then the text of both a seven-day fixed-days goal and a once-a-day goal. The goal page's tags (Commitment-based or Period-based) tell them apart.

    Record `ScheduleText` in data-model.md's `GoalText` row in the same task.
- [X] T027 [P] Create `Commitment.cs`, `EvaluationPeriod.cs`, `EvaluationCalendar.cs` and `GoalVersionChange.cs` (`None`, `ReplaceFirst(zone)`, `ReplacePending(zone, appliesFrom)`, `Add(appliesFrom, zone)`), with UTC `DateTimeOffset` instants (depends on T019, T023)
- [X] T028 Create `GoalTimeline.cs`, exactly as research R-06 describes (depends on T024, T027):
  - `MaxCalendarDays` = 366;
  - `VersionOf`;
  - day and week boundaries through `NamedTimeZone.StartOfDay` of each date's own version;
  - `CurrentDate`, whose candidates are the local date of the instant in each version's zone, ± 2 days, filtered by "start ≤ instant", with the maximum taken;
  - `TryCalendar`;
  - `NextCommitment`, which scans from max(start date, current date − 2) for at most 15 days;
  - `FirstWeekStart`;
  - `ChangeOf`, where E is D + 1 for commitment-based and per-day goals and the Monday after D's week for per-week goals.

  It takes no clock, no culture and no machine time zone.
- [X] T029 [P] Extend `src/HT.Domain/Accounts/Account.cs` with the `init` properties `string? StoredTimeZone`, `string? Name` and `DateTimeOffset? CreatedAt`, keeping the positional constructor. Create `src/HT.Domain/Accounts/AccountName.cs`: trimmed, 1 to 60 code units, and empty means none (FR-028)
- [X] T030 [P] Create `tests/HT.UnitTests/Domain/Goals/GoalDraftTests.cs` (depends on T025):
  - one test per FR-006 refusal, in order, with every earlier field valid;
  - both sides of every limit:
    - amount 0, 1, 999 and 1000, for Build and for Reduce;
    - unit of 20 and 21 code units, with `"  pages  "` stored as "pages";
    - times 0, 1, 20 and 21;
    - `ReduceAmountNeedsOneTime` for count and for duration with 2 times, and its absence for a Reduce yes/no goal of 2 times a day;
    - start date yesterday, today, today + 365 and today + 366;
    - name of 60 and 61 code units after trimming, and spaces only as no name;
  - values that do not belong to the measurement or schedule are neither refused nor stored;
  - `FieldOf` for every problem.
- [X] T031 [P] Create `tests/HT.UnitTests/Domain/Goals/GoalTextTests.cs` (depends on T026):
  - every target-text and kind example of spec "Target text and kind";
  - "avoided on Mon–Fri", "avoided on Sat, Sun" and "avoided every day";
  - the derived names "Reading, 15 min, 5 days a week", "Cold shower, daily", "Sugar, at most 1 sweet a day", "No phone in bed, avoided every day", "Coffee, at most 2 cups a day" and "Meditation, 10 min, 3 days a week";
  - an own name wins over the derived one;
  - `ScheduleText`: "Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat", "Sat · Sun", "Every day" for seven fixed days and for 1 time a day, "2 times a day", "Flexible · once a week" and "Flexible · 3 times a week", for every measurement and both intents;
  - `SuggestedStartingGoalText` still gives "at most 5 days a week" for a Reduce yes/no suggestion on five days.
- [X] T032 [P] Create `tests/HT.UnitTests/Domain/Goals/GoalTimelineCalendarTests.cs`, part of SC-003 (depends on T028). For each schedule kind:
  - US4-AS1: a Mon/Wed/Fri goal starting on a Wednesday, read for 14 days from that Monday, gives 5 commitments, the first on the start date;
  - US4-AS2: 7 per-day periods;
  - US4-AS3: three weeks of a per-week goal starting on a Wednesday, the first period being the following Monday to Sunday;
  - US4-AS6: a range that ends before the start date gives an empty calendar;
  - US4-AS7: `last` before `first` and 367 days are refused; 1 and 366 days are accepted;
  - midnight ± 1 second in Europe/Kyiv, America/New_York and UTC;
  - the clock-change days: Kyiv 2027-03-28 (23 hours) and 2027-10-31 (25 hours), New York 2027-03-14 and 2027-11-07;
  - a day whose midnight does not exist;
  - `FirstWeekStart` from a Wednesday (the next Monday) and from a Monday (itself).
- [X] T033 [P] Create `tests/HT.UnitTests/Domain/Goals/GoalTimelineChangeTests.cs`, the rest of SC-003 (depends on T028):
  - `CurrentDate` and `NextCommitment` across moves west and east: Kyiv to New York, New York to Kyiv, and Pacific/Pago_Pago to Pacific/Kiritimati;
  - a goal whose start date's day has not begun has no current date;
  - every `ChangeOf` case: `None`, `ReplaceFirst`, `ReplacePending` and `Add`, with E for commitment-based, per-day and per-week goals;
  - a second change before the first applies replaces it, and a second change after the first has begun adds a version from the next day;
  - never more than one version after the current date, and never two on one date;
  - the dates of US10-AS4, AS5, AS6 and AS10: the change at `2026-11-18T10:00:00Z` (12:00 Kyiv) and the second change at `2026-11-19T17:00:00Z` (12:00 New York).
- [X] T034 [P] Create `tests/HT.UnitTests/Domain/Goals/GoalTests.cs` (`Goal`'s invariants, `GoalTarget`'s schedule kind and period length) and `tests/HT.UnitTests/Domain/Accounts/AccountNameTests.cs` (`"  Yuri  "` → "Yuri", spaces only → none, 60 accepted, 61 too long) (depends on T024, T029)
- [X] T035 Extend the architecture rules for Domain and Application (research R-13; depends on T019–T029):
  - `tests/HT.ArchitectureTests/DomainDependencyTests.cs`: NodaTime is Domain's only non-BCL assembly, and Microsoft types stay forbidden;
  - `tests/HT.ArchitectureTests/ClockReadTests.cs`: add to `ForbiddenMembers` the member calls `NodaTime.SystemClock::get_Instance(`, `NodaTime.DateTimeZoneProviders::get_Bcl(`, `System.TimeZoneInfo::FindSystemTimeZoneById(`, `System.TimeZoneInfo::GetSystemTimeZones(` and `System.TimeZoneInfo::ConvertTime` (every overload; `TimeZoneInfo.Local` is already listed):
    - `DateTimeZoneProviders.Serialization` is not added. It is `[Obsolete]` in NodaTime 3.2.x, so any use in Domain or Application is already CS0618, which `TreatWarningsAsErrors` turns into a build error, and the canary could not call it without suppressing that warning. Check the attribute in the pinned version (T003); if it is not obsolete there, add the entry and its canary call like the others;
    - the `Canary` calls each new entry once, so `EveryForbiddenMemberIsRecognisedInACall` keeps passing. NodaTime reaches the architecture test project through its `HT.Ui` reference, so no package reference is added;
    - `BclDateTimeZone` is a type, which the call matcher cannot see. Forbid it with a separate ArchUnitNET rule in the same file: no type of Domain or Application depends on `NodaTime.TimeZones.BclDateTimeZone`;
  - `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs`: no type of `HT.Application` depends on a type in a `NodaTime` namespace, so NodaTime stays inside `HT.Domain` (research R-01);
  - new `tests/HT.ArchitectureTests/PublicSurfaceTests.cs`: no public or protected member of `HT.Domain` or `HT.Application` has a NodaTime type in its signature, including generic arguments.

### Application: texts, dates, the default time zone and goal views

- [X] T036 Add the texts of contracts/application-api.md "Texts", each defined once:
  - `src/HT.Application/Goals/GoalTexts.cs` (new), with the goal texts, "Goals are for registered users." and "Choose a range of 1 to 366 days.". The goal's `NameTooLong` has no constant of its own: `GoalService` uses `AccountTexts.NameTooLong`, the one definition of "The name can have at most 60 characters.";
  - in `src/HT.Application/Accounts/AccountTexts.cs`: "The name can have at most 60 characters.", "Choose a time zone from the list.", the three greetings with `", {name}"`, and `MemberSince(int year, int month)` (research R-11), which is "member since " followed by `DateTexts.MonthYear` of the first of that month and has no month names of its own;
  - in `src/HT.Application/Habits/CatalogTexts.cs`: `GoalsUseThisHabit(int count)`, giving "1 goal uses this habit." or "{N} goals use this habit.";
  - in `src/HT.Application/Habits/CustomHabitTexts.cs`: "Goals use this custom habit, so it cannot be deleted.";
  - in `src/HT.Application/Operator/OperatorTexts.cs`: "demo goals added".

  Depends on T037, because `MemberSince` calls `DateTexts`.
- [X] T037 [P] Create `src/HT.Application/Accounts/DateTexts.cs` (research R-11): `Weekday`, `LongDate` ("Tuesday, 17 November"), `ShortDate` ("Tue 17 Nov"), `DayMonth` ("23 Nov"), `DayMonthYear` ("14 Jul 2026", the Settings card's "Started" row, HP-004) and `MonthYear` ("November 2026"), all from fixed English arrays. `DayMonthYear` is not in research R-11's list: record it there in the same task. Create `tests/HT.UnitTests/Application/Accounts/DateTextsTests.cs`, which checks every format for every weekday and month inside a non-English `CultureScope`
- [X] T038 [P] Create `src/HT.Application/Accounts/InstallationTimeZone.cs` (research R-03):
  - `NamedTimeZone Value`;
  - `From(string?)`, which gives UTC when the value is absent or not a named time zone.

  Register `InstallationTimeZone.From(null)` with `TryAddSingleton` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs`. Create `tests/HT.UnitTests/Application/Accounts/InstallationTimeZoneTests.cs`: null, "", `Mars/Olympus` and `Etc/UTC` give UTC, and `Europe/Kyiv` is kept. Depends on T019.
- [X] T039 [P] Create the goal views and store types in `src/HT.Application/Goals/`, as data-model.md "Application inputs and views" describes: `GoalInput`, `GoalHabitView`, `GoalStartingValues`, `GoalListItem`, `GoalDetails`, `GoalCalendarView`, `StoredGoal`, `NewGoal` and `GoalWriteOutcome` (`Created(GoalId)`, `HabitGone`, `TimeZoneChanged`). Depends on T024, T026 and T027.

  `GoalListItem` and `GoalDetails` also get `string ScheduleText` (`GoalText.ScheduleText`). Without it the Goals page cannot write "Mon · Wed · Fri", because the list has no days (HP-003, US2-AS6). Record the member in data-model.md and contracts/application-api.md in the same task
- [X] T040 Create the goal port and its fake (depends on T039, T029):
  - `src/HT.Application/Goals/IGoalStore.cs` with its core members: `CreateAsync(owner, NewGoal, expectedZone, defaultZone, …) → GoalWriteOutcome`, `ListAsync(owner, …)` in creation order, and `FindAsync(owner, GoalId, …)`. Its XML doc states the owner-first rule and the single exception that US7 adds;
  - `tests/HT.UnitTests/Fakes/FakeGoalStore.cs`, which gives the same outcomes as research R-08:
    - it reads the account's zone from `FakeAccountStore`, with null as `defaultZone`, and returns `TimeZoneChanged` when that zone is not `expectedZone`;
    - it returns `HabitGone` when `FakeCatalogHabitStore` has the habit in another state than Published, or `FakeCustomHabitStore` lacks the owner's habit;
    - otherwise it writes the account's null zone, increments a per-account revision, and keeps version 1 and attempt 1;
    - it can be scripted to return `TimeZoneChanged` N times;
    - it has a test-only `Add(owner, Goal, habit)` that stores a goal directly, so tests that only need goals to exist (the habit services' counts, T081, T088, T094) arrange them without `GoalService` and without an account;
    - its constructor takes the `FakeAccountStore`, `FakeCatalogHabitStore` and `FakeCustomHabitStore` it reads. The custom-habit fake needs the goal store back for `UsedByGoals` (T084), so it gets an assignable `Goals` property, which `FakeGoalStore`'s constructor sets. That gives one construction order and no cycle;
  - `FakeAccountStore` gets an opt-in `KnowsEverySignedInUser` (off by default). When on, `FindByIdAsync` of an unknown identifier answers a registered user with no stored zone, no name and no `CreatedAt`. The bUnit `PageServices.SignIn` signs in with a fresh identifier that no store holds, so without this the goal and profile use cases would answer "You are not signed in." (T058) on every page test. Unit tests keep it off, so the missing-account rule stays tested (T053, T100);
  - the `<Compile Include>` link for `FakeGoalStore.cs` in `tests/HT.UiTests/HT.UiTests.csproj`;
  - in `tests/HT.UiTests/Components/Pages/PageServices.cs`, one set of goal fakes that every page test resolves:
    - a new `AddGoals(...)` registers, each with `TryAdd`, a `FakeCustomHabitStore` as `ICustomHabitStore`, a `FakeAccountStore` with `KnowsEverySignedInUser` as `IAccountStore`, a `FakeGoalStore` built from the registered fakes as `IGoalStore`, a `FakeTimeProvider` as `TimeProvider` and `InstallationTimeZone.From(null)`. T058 adds `GoalService` to it, and T103 adds `ProfileService`;
    - `AddCatalog` calls `AddGoals` after registering its catalog store, so even the Home tests, which call only `AddCatalog`, can resolve `GoalService`;
    - `AddCustomHabits` no longer creates a store of its own. It returns the `FakeCustomHabitStore` that `AddGoals` registered (calling `AddGoals` first if needed) and registers `CustomHabitService`. No existing test passes it a store;
    - every existing page test must keep resolving once the habit services take `IGoalStore` (T085, T091, T097) and Home injects `GoalService` and `ProfileService` (T111, T119). That covers `HomeVisitorTests`, `HomeSignedInTests`, `AdminCatalogHabitPageTests`, `AdminPageTests`, `CatalogPageTests`, `CatalogHabitDetailTests`, `CustomHabitPageTests` and `CustomHabitsPageTests`;
  - `tests/HT.UnitTests/Application/Goals/GoalInputs.cs`, with sample `GoalInput` builders on the pattern of `CatalogInputs.cs`. Its acting users are registered users that exist in the `FakeAccountStore` (with a stored zone, or with none for the default), because every goal use case reads the account (T058). A habit unit test that creates goals through `GoalService` uses these users, not `CatalogInputs`' random ones. Add the catalog habit "Reading" to `tests/HT.UnitTests/Fakes/HabitSamples.cs`, with its marked suggestion "15 min, 5 days a week" (Duration, Mon–Fri, Build) and its second suggestion "20 pages, 3 times a week", unless they are already there.

### Accounts: profile columns and the new account-store signature (research R-04, R-07)

- [X] T041 Change the account port and every creator (depends on T029, T038):
  - create `src/HT.Application/Accounts/NewAccountProfile.cs` (`AccountName? Name`, `NamedTimeZone TimeZone`, `DateTimeOffset CreatedAt`);
  - change `IAccountStore.CreateAsync` to `CreateAsync(Email, Role, string password, NewAccountProfile profile, …)`, and update `tests/HT.UnitTests/Fakes/FakeAccountStore.cs`;
  - `AccountService` gains `InstallationTimeZone` in its constructor. The new overload `RegisterAsync(string? email, string? password, string? timeZone, string? name, …)`:
    - checks `InvalidEmail`, then `PasswordRuleNotMet`, then `EmailTaken`, then `NameTooLong`, appended to `RegistrationOutcome`;
    - looks up the email before refusing a long name, so a taken email with a long name gives `EmailTaken`, and nothing is created;
    - uses a time zone that `NamedTimeZone.TryParse` accepts, and otherwise the default, which is not refused (FR-025);
    - records `CreatedAt` as the supplied time.

    The old overload is `RegisterAsync(email, password, null, null)`;
  - `OperatorService` gains `TimeProvider` and `InstallationTimeZone`. The configured administrator gets the default zone, no name and `CreatedAt` (FR-025, FR-030);
  - update every other caller and implementer research R-07 lists:
    - `tests/HT.UiTests/Host/HostClient.cs`;
    - `tests/HT.IntegrationTests/Habits/HabitAccounts.cs`;
    - `tests/HT.IntegrationTests/Accounts/ClaimsPrincipalFactoryTests.cs`, `PasswordChangeTests.cs`, `AccountDetailsTests.cs` and `AccountStoreTests.cs`, with its `PasswordChangingStore`;
    - the hand-built `AccountService` and `OperatorService` in `tests/HT.UnitTests/Application/Accounts/{AccountDetails,ChangePassword,Register,SignIn}Tests.cs`, `tests/HT.UnitTests/Application/Operator/AdministratorCreationTests.cs` and `tests/HT.UiTests/Components/Account/{ChangePassword,Register,SignIn}FormTests.cs`;
    - any further hand-built instance that the grep convention finds.
- [X] T042 [P] Create `tests/HT.UnitTests/Application/Accounts/RegisterProfileTests.cs` (depends on T041):
  - US10-AS1: registration with `Europe/Kyiv` stores that zone;
  - US10-AS2: no zone, `Mars/Olympus` and `FLE Standard Time` store the default, which is `UTC` with no configured default and `Europe/Kyiv` with `InstallationTimeZone.From("Europe/Kyiv")`;
  - US11-AS1: the name `"  Yuri  "` is stored as "Yuri", and spaces only store none;
  - US11-AS2: 61 characters give "The name can have at most 60 characters." and create nothing, while 60 are accepted;
  - `CreatedAt` is the supplied time;
  - the old overload gives slice 1's outcomes unchanged (FR-019).

  Extend `AdministratorCreationTests.cs`: the administrator gets the default zone and `CreatedAt`.

### Persistence: tables, migration, account store and the goal store's core (`src/HT.Persistence/`; research R-04, R-08)

- [X] T043 Map the new data (depends on T024, T029):
  - create `src/HT.Persistence/Goals/GoalRows.cs` (`GoalRow`, `GoalSettingsVersionRow`, `GoalAttemptRow`) and `GoalMapping.cs`. The mapping goes row ↔ Domain, with `Days` as a bit mask from Monday 1 to Sunday 64, as `HabitMapping` does;
  - add `TimeZone`, `Name`, `CreatedAt` and `GoalsRevision` to `src/HT.Persistence/Identity/HtUser.cs`;
  - in `src/HT.Persistence/HtDbContext.cs`:
    - the three tables with the keys, indexes and foreign keys of data-model.md "Tables";
    - `CreationSequence` as a `bigint` identity column with a unique index;
    - enums as `varchar(16)`, and binary collation for keys;
    - `GoalsRevision` defaulting to 0;
    - `SetAfterSaveBehavior(PropertySaveBehavior.Ignore)` on `TimeZone`, `Name`, `CreatedAt` and `GoalsRevision`, as slice 1 does for `FailedSignInCount`.
- [X] T044 Add the migration `Goals` with the EF tool from the repo's tool manifest (`dotnet tool restore` first), into `src/HT.Persistence/Migrations/` (depends on T043). Read the generated code before keeping it:
  - the four nullable or defaulted `AspNetUsers` columns, with `TimeZone` nullable;
  - the three tables;
  - no other default and no check constraint.

  `HtDatabase.ResetAsync` and `MigrateAsync` must apply it.
- [X] T045 Extend `src/HT.Persistence/Identity/AccountStore.cs` (depends on T041, T043):
  - `CreateAsync` writes the profile's name, zone and `CreatedAt` into the new `HtUser` before `UserManager.CreateAsync`;
  - `FindByIdAsync` and `FindByEmailAsync` fill `StoredTimeZone`, `Name` and `CreatedAt`.
- [X] T046 Create `src/HT.Persistence/Goals/GoalStore.cs`, internal sealed, with the core members of T040, and register it as `IGoalStore` in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs` (depends on T040, T044):
  - `CreateAsync` is one transaction, following research R-08 steps 2 to 6:
    1. read the account row `WITH (UPDLOCK, HOLDLOCK)`;
    2. compare its zone, with NULL read as `defaultZone`, to `expectedZone`; on a difference, write nothing and return `TimeZoneChanged`;
    3. insert only when the catalog habit is Published under the same locks, or when the owner's custom habit exists, and otherwise return `HabitGone`;
    4. set a NULL account zone to `expectedZone`, and increment `GoalsRevision`;
    5. insert the goal, version 1 with `expectedZone`, and attempt 1;
  - `ListAsync` reads in `CreationSequence` order, joined in one query with each habit's current name, category and state;
  - `FindAsync` is scoped by owner.

  No statement may raise a database error whose message carries a value.
- [X] T047 [P] Create `tests/HT.IntegrationTests/Goals/GoalStoreTests.cs` on `HT-IT` (depends on T046):
  - every field round-trips through create, find and list: each measurement, each schedule, both duration units, a count unit, and zones `UTC`, `Europe/Kyiv` and the link `Europe/Kiev`;
  - another owner's `FindAsync` gives null, and `ListAsync` leaves the goal out;
  - creation order follows insertion for equal names and equal supplied times;
  - version 1 has `expectedZone` and applies from the start date, and attempt 1 starts on the start date;
  - `TimeZoneChanged` writes nothing;
  - a NULL account zone becomes `expectedZone` with the first goal, and `GoalsRevision` increments with every goal;
  - `HabitGone` for a Draft catalog habit, an Unpublished one, a missing custom habit and another owner's custom habit.
- [X] T048 [P] Create `tests/HT.IntegrationTests/Accounts/AccountProfileStoreTests.cs` (depends on T045, T046):
  - `CreateAsync` stores the name, the zone and `CreatedAt`, and `FindByIdAsync` and `FindByEmailAsync` return them;
  - registration through `AccountService` stores `Europe/Kyiv`, and stores the default for `FLE Standard Time`;
  - a password change whose `HtUser` was loaded before a goal creation committed keeps the account's zone, which went from NULL to concrete, and `GoalsRevision` (research R-04).

  US10 and US11 add the time-zone and name variants.
- [X] T049 Create `tests/HT.IntegrationTests/Goals/PreSliceAccountMigrationTests.cs` on `HT-IT-MIGRATE` (T004), following research R-14 steps 1 to 6 (depends on T044, T046):
  1. compose with `AddHtPersistence`;
  2. resolve the internal `HtDbContext` by its type name, through reflection over `DbContext`;
  3. delete the database, then migrate to `HabitCatalog` with EF's public `IMigrator`;
  4. insert an account row with slice 1's columns only, by SQL;
  5. apply the remaining migrations with `HtDatabase.MigrateAsync`;
  6. check that `StoredTimeZone`, `Name` and `CreatedAt` are null, that `GoalsRevision` is 0, and that a first goal created through `IGoalStore.CreateAsync` with the default `Europe/Kyiv` writes `Europe/Kyiv` into the row.

  US10 (T107) and US11 (T114) add the use-case checks.
- [X] T050 Extend the architecture rules for the goal port (research R-13; depends on T046):
  - `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`: every `IGoalStore` member takes the owner first, except one named `CountUsingCatalogHabitAsync`;
  - `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs`: `IGoalStore` is used only by `HT.Application.Goals`, the three services of `HT.Application.Habits` and `HT.Application.Accounts.ProfileService`, and not by `HT.Application.Operator`;
  - `tests/HT.ArchitectureTests/UiDependencyTests.cs`: no `HT.Ui` type depends on `IGoalStore`.

  No architecture rule requires a store to be internal. `GoalStore` is internal sealed because T046 declares it so, as the other stores are. No test checks it, and T133 compares only `HT.Application` and `HT.Domain`, so code review is the only safeguard.

### Host and shared components (`src/HT.Ui/`)

- [X] T051 Wire the installation's default time zone in the host (research R-03, contracts/host.md "Operator log"; depends on T038):
  - in `src/HT.Ui/Program.cs`, register `InstallationTimeZone.From(builder.Configuration["Operator:DefaultTimeZone"])` before `AddHtApplication`;
  - in `src/HT.Ui/Operator/StartupTasks.cs`, log the Warning `Default time zone: not a named time zone, using UTC` under category `HT.Ui.Operator.StartupTasks`, only when the value is present but not a named time zone. Never log the value.

  Add the tests to `tests/HT.UiTests/Host/StartupLogTests.cs`: an invalid value logs the line without the value, while an absent value and `Europe/Kyiv` log nothing.
- [X] T052 [P] Create `src/HT.Ui/Components/Shared/KeyValue.razor`, the `.ht-kv` rows of `design/components/KeyValue`, and its bUnit tests in `tests/HT.UiTests/Components/Shared/SharedComponentTests.cs` (depends on T018)

**Checkpoint**: `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx` pass. Commit design, Domain, the
Application and Persistence foundation, and the host change together (plan, Work order 3 and 4).

---

## Phase 3: User Story 1 - Create a goal from a catalog habit (Priority: P1) 🎯 MVP

**Goal**: a registered user starts a new goal from a catalog habit, with or without a suggested starting goal,
adjusts it, and creates it. The New goal page's summary card says what the goal means.

**Independent Test**: create a goal from "Reading" with its marked suggested starting goal, and read it back
through the store with the same settings (US1-AS1, AS5).

### Tests for User Story 1

- [X] T053 [P] [US1] Create `tests/HT.UnitTests/Application/Goals/GoalStartingValuesTests.cs` for `StartNewGoalAsync` (depends on T057, T058):
  - US1-AS1: Reading without a position gives its marked suggestion's values, today's start date, no name, and "15 min, 5 days a week" as the catalog suggestion text;
  - US1-AS2: position 2 gives Count, 20 "pages", 3 times per week, and "20 pages, 3 times a week";
  - US1-AS3: a Reduce habit with no suggestion gives only the habit, Reduce and the start date;
  - US1-AS4: position 4 of 3 gives "This suggested starting goal does not exist.";
  - the catalog suggestion text follows the goal rules, so a Reduce yes/no suggestion on fixed days gives "avoided on …" (FR-007);
  - US1-AS8: a Draft habit, an Unpublished habit, a custom habit's identifier given as a catalog habit, and an unknown identifier each give "This habit is not in the catalog.";
  - another user's custom habit gives "This custom habit does not exist.";
  - US8-AS4: no habit gives only today's start date;
  - US4-AS8: at 23:30 on a Monday in the user's zone the start date is that Monday, and at 00:30 the next day it is Tuesday;
  - "today" uses the account's stored zone, or the installation's default when it is null;
  - a registered acting user with no account in the store gets "You are not signed in.";
  - the FR-023 order: an administrator with an unknown habit gets the role refusal, and an anonymous visitor gets "You are not signed in.".
- [X] T054 [P] [US1] Create `tests/HT.UnitTests/Application/Goals/GoalCreateTests.cs` for `CreateAsync` (depends on T057, T058):
  - US1-AS5: unchanged starting values create a goal that the store holds with exactly those settings, version 1 with the account's zone, and attempt 1 on the start date (FR-018);
  - US1-AS6: Reduce, Count, at most 2 "cups", 1 time per day, with the target text "at most 2 cups a day";
  - US1-AS7: one test per FR-006 message, in order, each with its `Field`;
  - US1-AS8: the not-in-catalog cases;
  - US1-AS10: a second goal on Reading;
  - US8-AS4: no habit gives "Choose a habit.";
  - US4-AS8: creating with Monday's date at 23:30 on that Monday gives `Created`, and at 00:30 on Tuesday gives "The start date cannot be in the past.";
  - an empty name gives no name;
  - one `TimeZoneChanged` from the store makes the service re-read the account and check the start date again in the new zone. Ten in a row throw a fault exception whose message contains no value;
  - a refusal echoes no input value (HP-011).
- [X] T055 [P] [US1] Add a test to `tests/HT.UnitTests/Application/Operator/SeedCatalogTests.cs` for SC-004 and FR-007: for every Published seed catalog habit (the ones `CatalogService.BrowseAsync` returns, so not the Draft "Music practice" or the Unpublished "Coffee") and every suggestion position, `StartNewGoalAsync` followed by `CreateAsync` with the starting values unchanged gives `Created` (depends on T058)
- [X] T056 [P] [US1] Create `tests/HT.IntegrationTests/Goals/GoalCreationTests.cs` on `HT-IT` (depends on T058, T046):
  - creation through `GoalService` on a Published catalog habit, read back through `IGoalStore.FindAsync`;
  - the race of a goal created against an unpublish, forced with two contexts: no goal exists on a habit that was Unpublished at commit, and creation gives "This habit is not in the catalog.";
  - log capture for HP-011 (`LogCapture`): a creation and each kind of refusal, with a distinctive goal name, unit and amount, leave none of them in any log entry or exception text.

### Implementation for User Story 1

- [X] T057 [US1] Create `src/HT.Application/Goals/GoalResults.cs`:
  - `GoalStartOutcome` and `GoalStartResult`, with `StartingValues`;
  - `GoalCreateOutcome`, in the check order of data-model.md;
  - `GoalCreateResult`, with `Message`, `Field` (`GoalField?`) and `GoalId?`.

  Depends on T039 and T036.
- [X] T058 [US1] Create `src/HT.Application/Goals/GoalService.cs` (depends on T040, T041, T057):
  - its constructor takes `IGoalStore`, `IAccountStore`, `ICatalogHabitStore`, `ICustomHabitStore`, `TimeProvider` and `InstallationTimeZone`;
  - `StartNewGoalAsync` and `CreateAsync` follow contracts/application-api.md and FR-023's order;
  - one shared habit lookup, through `ICatalogHabitStore.FindPublishedAsync` or `ICustomHabitStore.FindAsync(owner, …)`;
  - "today" is `(StoredTimeZone ?? InstallationTimeZone).DateAt(now)`;
  - an acting user whose account `FindByIdAsync` does not find (deleted after sign-in, or a stale cookie) gets `NotSignedIn`, "You are not signed in.", after the role check. The spec defines no other outcome for it (FR-023), and it reveals nothing. `ProfileService` (T103, T116) does the same. Record the rule in contracts/application-api.md (the `NotSignedIn` row of "Texts") in this task;
  - `GoalDraft` runs in two steps around the lookup;
  - the expected-zone retry runs at most 10 times;
  - `HabitGone` maps to FR-008's texts.

  Register it as scoped in `ApplicationServiceCollectionExtensions.cs`, add it to `AddGoals` in `PageServices.cs`, and add `GoalService` to `SignedInServices` in `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`.
- [X] T059 [P] [US1] Create `src/HT.Application/Goals/GoalSummaryText.cs` and `tests/HT.UnitTests/Application/Goals/GoalSummaryTextTests.cs` (HP-002, research R-11; depends on T026, T037, T005):
  - the sentence: the habit name, ": ", "at least " for a Build count or duration goal only, the target text, ", starting " and the start date as `ShortDate`, then ".". The tests use HP-002's three examples;
  - Evaluated, "Met when", "Streak counted in" and "Late records" for each schedule kind, with the values T005 wrote;
  - the catalog suggestion line only when a suggestion was used;
  - the first-week line for a per-week goal whose start date is not a Monday, and none for a Monday;
  - the incomplete-form state of T005 while the target does not pass the checks.
- [X] T060 [P] [US1] Create `src/HT.Ui/Components/Shared/ChoiceCard.razor`, following `design/components/ChoiceCard`, with its bUnit tests (selected state as text and not by color alone, keyboard focus) in `tests/HT.UiTests/Components/Shared/SharedComponentTests.cs` (depends on T018)
- [X] T061 [US1] Build the New goal page (HP-001, HP-002, contracts/host.md; depends on T058, T059, T060):
  - `src/HT.Ui/Components/Goals/NewGoalForm.razor`: the sections of HP-001, with SegmentedControl, ChoiceCard, Stepper (minimum 0 for Reduce and 1 for Build, maximum 999) and DayPicker. "At least" switches to "At most" for Reduce;
  - `HabitChooser.razor`: an `HtDialog` with a SearchField over `CatalogService.BrowseAsync(CatalogFilter)` and `CustomHabitService.ListAsync`;
  - `GoalSummaryCard.razor`, which calls `GoalSummaryText` on every change;
  - `src/HT.Ui/Components/Pages/NewGoal.razor` at `/goals/new`:
    - Interactive Server and `[Authorize]`; an administrator gets Not found;
    - it reads `habit`, `kind` and `suggestion` from the address, and calls the use cases through `UseCaseScope`;
    - a refused address value shows the use case's text in the Habit section, and the page opens with no habit chosen;
    - each refusal shows under its `Field`;
    - success navigates to `/goals/{id}`;
    - the BackLink goes to the habit's details, the custom habit page, or `/goals` when the address names no habit.
- [X] T062 [P] [US1] Create `tests/HT.UiTests/Components/Pages/NewGoalPageTests.cs` (depends on T061):
  - the head line for a catalog habit, a custom habit and no habit (US8-AS3);
  - the sections and their wording;
  - US1-AS11: the summary card rewrites on every change, and Reduce switches the target word to "At most";
  - a refusal sits under its field;
  - the habit chooser opens, moves focus into the Dialog, searches, and closes back to the Change button;
  - a refused address value;
  - success navigates to the goal page.
- [X] T063 [P] [US1] Create `tests/HT.UiTests/Host/GoalsHostTests.cs` with the `/goals/new` cases: a signed-out visitor is sent to Sign in, an administrator gets the Not found page (404), and a registered user gets the page (depends on T061)

**Checkpoint**: full suite green. A goal can be created on `/goals/new`; its page arrives with US2. Commit.

---

## Phase 4: User Story 2 - See my goals (Priority: P1)

**Goal**: a registered user lists their goals in name order with their next commitment or start information, and
opens one to see its settings, attempt line and evaluation.

**Independent Test**: a user with two goals lists them in name order and opens one (US2-AS1, AS4).

### Tests for User Story 2

- [X] T064 [P] [US2] Create `tests/HT.UnitTests/Application/Goals/GoalListTests.cs` (depends on T067, T068):
  - US2-AS1: name order through `HabitName.KeyOf`, then creation order for equal names, with every FR-015 field and the `ScheduleText` ("Mon · Wed · Fri", "Flexible · 3 times a week");
  - US2-AS2: a started Mon/Wed/Fri goal read on Wednesday 18 November 2026 gives that Wednesday as its next commitment, and read on Thursday 19 November gives Friday 20 November;
  - US2-AS3: a per-day goal starting later gives its start date and no next commitment, and a Mon/Wed/Fri goal starting on a later Tuesday gives that start date and the following Wednesday;
  - US4-AS4 for the list: a per-week goal starting on a Wednesday gives the next Monday as its first week, while that week has not begun;
  - US2-AS5: an empty list;
  - FR-014 across a time-zone switch, with a goal of two versions in `FakeGoalStore`: a commitment already over is not given as next, and one in progress is not skipped;
  - the result's `Today` is the account's date.
- [X] T065 [P] [US2] Create `tests/HT.UnitTests/Application/Goals/GoalReadTests.cs` (depends on T067, T068):
  - US2-AS4: every FR-016 field, and the `ScheduleText`;
  - US1-AS9: the derived name "Reading, 15 min, 5 days a week", and the own name "Morning pages" with `HasOwnName`;
  - US7-AS7: the habit's current name after a rename;
  - `IsInCatalog`: false for an Unpublished catalog habit, true for a Published one, null for a custom habit;
  - `TimeZone`, `TimeZoneAppliesFrom`, `LaterTimeZone` and `LaterTimeZoneAppliesFrom`;
  - `FirstWeekStarts` for a per-week goal, given always;
  - attempt 1 and its start date, and `HasStarted` before and after the start date's day begins;
  - an unknown identifier and another user's goal both give "This goal does not exist.".
- [X] T066 [P] [US2] Create `tests/HT.IntegrationTests/Goals/GoalReadingTests.cs` on `HT-IT`: list and read through `GoalService` over SQL, with the habit renamed by an administrator so the goal shows the current name, and owner scoping (depends on T068)

### Implementation for User Story 2

- [X] T067 [US2] Add the list and read results to `src/HT.Application/Goals/GoalResults.cs`:
  - `GoalListOutcome` and `GoalListResult`, with `Goals` and `DateOnly Today`, the acting user's date at the supplied time;
  - `GoalReadOutcome` and `GoalReadResult`.

  Add `bool HasStarted` (the goal has a current date) to `GoalDetails` in `src/HT.Application/Goals/`. These two members let the pages say "next today", "started" and "starts" without reading a clock. Record both in data-model.md and contracts/application-api.md in the same task.
- [X] T068 [US2] Add `ListAsync` and `GetAsync` to `src/HT.Application/Goals/GoalService.cs` (FR-014 to FR-016, FR-018, FR-020; depends on T067):
  - FR-023's order;
  - `GoalTimeline.CurrentDate`, `NextCommitment` and `FirstWeekStart` at the supplied time;
  - `GoalText.Name` with the habit's current name;
  - a stable sort by `HabitName.KeyOf(name)` over the store's creation order;
  - the version of the current date and the one later version.
- [X] T069 [P] [US2] Create `src/HT.Ui/Components/Shared/GoalItem.razor`, the design's GoalListItem (HP-003). It is not named `GoalListItem`, because the view `HT.Application.Goals.GoalListItem` would then be ambiguous in `Goals.razor` (CS0104). It shows the name link, the Build or Reduce Tag and the meta line. The meta line is the item's `ScheduleText` (T039), then " · " and the first that applies of "first week starts Mon 23 Nov", "starts Mon 23 Nov" (exactly when the item has `StartDate`, plan "Spec discrepancies" 5), "next today" (`NextCommitment` equals `Today`), "next Wed 18 Nov", or nothing, in which case the line is the schedule text alone. All dates come from `DateTexts.ShortDate`. Add its bUnit tests for every variant to `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs` (depends on T018, T067)
- [X] T070 [US2] Build the Goals page and the goal page (HP-003, HP-004; depends on T068, T069, T052):
  - `src/HT.Ui/Components/Goals/GoalSettingsCard.razor`, made of KeyValue rows, with the sentence of HP-004. Schedule is `GoalDetails.ScheduleText`, Evaluated is "Each scheduled day", "Each day" or "Each week", and Started is the start date as `DateTexts.DayMonthYear` ("14 Jul 2026");
  - `src/HT.Ui/Components/Pages/Goals.razor` at `/goals`: static SSR and `[Authorize]`, with Not found for an administrator, an `ht-list` of `GoalItem`s, and the EmptyState;
  - `src/HT.Ui/Components/Pages/GoalDetail.razor` at `/goals/{Id:guid}`:
    - static SSR and `[Authorize]`;
    - Not found (404) for an administrator, an unknown goal and another account's goal;
    - "← Goals", the name and the tags;
    - the attempt line "Attempt 1 · started {DayMonth}" or "· starts {DayMonth}", from `HasStarted`;
    - the Settings card;
    - the FR-034 Notice when `Habit.IsInCatalog` is false.
- [X] T071 [P] [US2] Create `tests/HT.UiTests/Components/Pages/GoalsPageTests.cs` (US2-AS6, the EmptyState) and `GoalDetailPageTests.cs` (US2-AS7, both attempt lines, the Settings rows and sentence, and US7-AS8: the Notice for an Unpublished habit and none on the list) (depends on T070)
- [X] T072 [US2] Extend `tests/HT.UiTests/Host/GoalsHostTests.cs` with `/goals` and `/goals/{id}`: a signed-out visitor is sent to Sign in, an administrator gets 404, the owner gets 200, and an unknown identifier gets 404 (depends on T070, T063)

**Checkpoint**: full suite green. Create, list and open goals in the browser. Commit.

---

## Phase 5: User Story 4 - Commitments and evaluation periods (Priority: P1)

**Goal**: a goal's evaluation calendar lists its commitments or evaluation periods with start and end instants, at
local midnights in each day's own time zone.

**Independent Test**: for a fixed "now" and time zone, read the calendar of one goal of each schedule kind over two
weeks, including a clock-change day (US4-AS1 to AS5).

### Tests for User Story 4

- [X] T073 [P] [US4] Create `tests/HT.UnitTests/Application/Goals/GoalCalendarTests.cs` for `GetCalendarAsync`, through `FakeGoalStore` (depends on T075):
  - US4-AS1 to AS3 with UTC instant literals;
  - US4-AS5: a clock-change day lasts 23 or 25 hours, with a goal starting within 365 days of a fixed "now" in November 2026;
  - US4-AS6: an empty calendar;
  - US4-AS7: "Choose a range of 1 to 366 days." for `last` before `first` and for 367 days;
  - FR-013's "weeks that overlap the range": a per-week goal read from a Wednesday to the Tuesday after gives both weeks, each whole from Monday to Sunday, and a range inside one week gives that one week;
  - `Commitments` filled for a commitment-based goal and `Periods` for a period-based one;
  - US4-AS4 for reading: `FirstWeekStarts` is the Monday after a Wednesday start, or the start date itself when it is a Monday.
- [X] T074 [P] [US4] Create `tests/HT.IntegrationTests/Goals/GoalCalendarTests.cs` on `HT-IT` (US10-AS3): a goal of an account registered with `Europe/Kyiv`, created through `GoalService`, whose calendar gives Kyiv midnights as UTC literals over a range that includes 2027-03-28 (depends on T075)

### Implementation for User Story 4

- [X] T075 [US4] Add `GoalCalendarOutcome` and `GoalCalendarResult` to `src/HT.Application/Goals/GoalResults.cs`, and `GetCalendarAsync` to `GoalService.cs`. It follows FR-023's order (not signed in, role, goal identifier, range), then maps `GoalTimeline.TryCalendar` into `GoalCalendarView` (FR-012, FR-013, FR-027) (depends on T068)

**Checkpoint**: full suite green, with SC-003's boundary tests (T032, T033, T073). Commit.

---

## Phase 6: User Story 3 - Goal privacy (Priority: P1)

**Goal**: only a goal's owner can list, read or use it. Another registered user, an administrator and an anonymous
visitor get nothing, and no result reveals that someone else's goal exists.

**Independent Test**: user A creates a goal; user B, an administrator and an anonymous visitor each try to read it,
list it and read its calendar (US3-AS1 to AS4).

### Tests for User Story 3

- [X] T076 [P] [US3] Create `tests/HT.UnitTests/Application/Goals/GoalAccessTests.cs`, covering every `GoalService` method (`StartNewGoalAsync`, `CreateAsync`, `ListAsync`, `GetAsync` and `GetCalendarAsync`) (SC-002):
  - US3-AS4: an anonymous visitor gets "You are not signed in.";
  - US3-AS3: an administrator gets "Goals are for registered users." with no goal data, also when other accounts have goals;
  - US3-AS1: user B reading user A's goal, or its calendar, gets "This goal does not exist.", the same result and text as for an unknown identifier;
  - US3-AS2: B's list holds none of A's goals;
  - US3-AS5: A starting or creating a goal on B's custom habit gets "This custom habit does not exist.", and nothing is created;
  - FR-023: a refusal earlier in the order wins over a later one, for each method.
- [X] T077 [P] [US3] Create `tests/HT.IntegrationTests/Goals/GoalPrivacyTests.cs` on `HT-IT`: the same cases as T076 over SQL, with two registered users and an administrator created through `AccountService` and `OperatorService`
- [X] T078 [P] [US3] Create `tests/HT.IntegrationTests/Goals/GoalLogTests.cs` for HP-011, with `LogCapture`:
  - every goal use case runs: creation and each kind of refusal, list, read, calendar, and a read of another user's goal;
  - the goal name, custom-habit name, unit and amount are distinctive;
  - `AssertContainsNone` finds none of them in any log entry or exception text.
- [X] T079 [P] [US3] Create `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs`: a registered user who opens another account's `/goals/{id}` gets the Not found page with 404, and the response contains neither the goal's name nor its habit's name

**Checkpoint**: full suite green. The MVP (US1, US2, US4 and US3) is demonstrable: goals are created, listed, opened
and private. Commit.

---

## Phase 7: User Story 5 - Goals on custom habits (Priority: P2)

**Goal**: goals on one's own custom habits, "Save and create goal", and a custom habit that goals use cannot be
deleted.

**Independent Test**: create the custom habit "Cold shower", create a yes/no daily goal on it, then try to delete the
habit (US5-AS1 to AS3).

### Tests for User Story 5

- [X] T080 [P] [US5] Create `tests/HT.UnitTests/Application/Goals/GoalOnCustomHabitTests.cs` (depends on T085):
  - US5-AS1: the starting values are the custom habit, Build and today, and nothing else;
  - US5-AS2: the name "Cold shower, daily";
  - US5-AS5: after a rename to "Cold water" the name is "Cold water, daily" with the settings unchanged, and a change of the habit's usual intent leaves the goal's Build or Reduce as it was.
- [X] T081 [P] [US5] Extend `tests/HT.UnitTests/Application/Habits/CustomHabitServiceTests.cs` (depends on T085):
  - US5-AS3: deleting a habit that a goal uses gives "Goals use this custom habit, so it cannot be deleted." and changes nothing;
  - US5-AS4: a habit no goal uses is deleted, as in slice 2;
  - another user's or an unknown habit still gives `NotFound` first;
  - `GetAsync` gives `GoalCount`.
- [X] T082 [P] [US5] Extend the custom-habit integration tests (depends on T084, T085):
  - in `tests/HT.IntegrationTests/Habits/CustomHabitStoreTests.cs`, the conditional delete: `UsedByGoals` when a goal uses the habit, `NotFound` for none, and a delete when unused;
  - in `CustomHabitRaceTests.cs`, a goal created against a delete, forced with two contexts in both orders. Either the delete wins and creation gives "This custom habit does not exist.", or the goal wins and the delete gives `UsedByGoals`. No goal is ever left on a deleted habit.

### Implementation for User Story 5

- [X] T083 [US5] Add `CountUsingCustomHabitAsync(owner, habit, …)` to `IGoalStore`, `GoalStore` and `FakeGoalStore`
- [X] T084 [US5] Make the custom-habit delete conditional (research R-08):
  - append `UsedByGoals` to `DeleteCustomHabitOutcome` in `src/HT.Application/Habits/ICustomHabitStore.cs`;
  - in `src/HT.Persistence/Habits/CustomHabitStore.cs`, `DELETE … WHERE NOT EXISTS` (a goal on the habit, read `WITH (UPDLOCK, HOLDLOCK)`). Zero rows means a re-read, which tells `UsedByGoals` from `NotFound`;
  - `tests/HT.UnitTests/Fakes/FakeCustomHabitStore.cs` gets the same rule through a `FakeGoalStore` reference.
- [X] T085 [US5] Extend `src/HT.Application/Habits/CustomHabitService.cs` (FR-035; depends on T083, T084):
  - its constructor gains `IGoalStore`;
  - `DeleteAsync` gives `UsedByGoals`, appended to `CustomHabitDeleteOutcome`, after `NotFound`;
  - `GetAsync` fills `CustomHabitView.GoalCount`, a new `init` property.

  Update the hand-built instances: at least `tests/HT.UnitTests/Application/Habits/CustomHabitServiceTests.cs` (`Service => new(_store)`), plus whatever the grep convention finds.
- [X] T086 [US5] Update the custom habit page (HP-007; depends on T085):
  - `src/HT.Ui/Components/Pages/CustomHabit.razor` and `src/HT.Ui/Components/Habits/CustomHabitForm.razor` get "Save and create goal", which saves the habit and navigates to `/goals/new?habit={id}&kind=custom`;
  - "Delete" is shown only when `GoalCount` is 0.
- [X] T087 [P] [US5] Update `tests/HT.UiTests/Components/Pages/CustomHabitPageTests.cs`, whose slice 2 HP-007 expectations change: US5-AS6 "Save and create goal", which saves and then navigates to `/goals/new?habit={id}&kind=custom` (bUnit's `NavigationManager`, because the page is Interactive Server and the save never reaches the host as an HTTP redirect), and "Delete" absent with goals and present without. No host test: `CustomHabitHostTests.cs` sees only prerendered GETs (depends on T086)

**Checkpoint**: full suite green. Commit.

---

## Phase 8: User Story 6 - My goals in the catalog (Priority: P2)

**Goal**: in the catalog, a registered user sees their goal count per habit, can show only tracked habits, and sees
their goals on a habit's details.

**Independent Test**: a user with two goals on "Reading" browses with and without "Only habits I track" (US6-AS1,
AS2).

### Tests for User Story 6

- [X] T088 [P] [US6] Create `tests/HT.UnitTests/Application/Habits/CatalogGoalInfoTests.cs` (depends on T091):
  - US6-AS1: the counts 2 and 0, and `TrackedCount`;
  - US6-AS2: `onlyTracked` together with category, intent and search;
  - US6-AS3: the details give `GoalCount` 2 and `Goals` (identifier and derived name) in name order, then creation order;
  - US6-AS4: the visitor and the administrator get the slice 2 content with null goal information, and `onlyTracked` gives "You are not signed in." or "Goals are for registered users.";
  - US3-AS6: user B's count for "Reading" is 0, and nothing of A's goals is returned;
  - FR-019: the old `BrowseAsync(CatalogFilter)` and `GetDetailsAsync(HabitId)` give results equal to slice 2's.
- [X] T089 [P] [US6] Create `tests/HT.IntegrationTests/Habits/CatalogGoalInfoTests.cs` on `HT-IT`: two registered users with goals on shared catalog habits, checked by inclusion over SQL (depends on T091)

### Implementation for User Story 6

- [X] T090 [US6] Add `CountByCatalogHabitAsync(owner, …) → IReadOnlyDictionary<HabitId, int>` and `ListOnCatalogHabitAsync(owner, habit, …)` to `IGoalStore`, `GoalStore` and `FakeGoalStore`
- [X] T091 [US6] Extend `src/HT.Application/Habits/CatalogService.cs` (FR-031, FR-032, research R-09; depends on T090, T068):
  - its constructor gains `IGoalStore`;
  - the new overloads `BrowseAsync(ActingUser, CatalogFilter, bool onlyTracked, …)` and `GetDetailsAsync(ActingUser, HabitId, …)`;
  - the `init` properties `CatalogHabitSummary.GoalCount`, `CatalogBrowseResult.TrackedCount`, `CatalogHabitDetails.GoalCount` and `Goals` in `CatalogViews.cs` and `CatalogResults.cs`;
  - `GoalOnHabit` in `src/HT.Application/Habits/`;
  - `NotSignedIn` and `NotForAdministrators` appended to `CatalogBrowseOutcome`;
  - the old overloads unchanged.

  In `UseCaseParameterTests.cs`, each `CatalogService` overload takes either no acting user or the acting user first. Update the hand-built instances: the target-typed `Service => new(_store)` in `tests/HT.UnitTests/Application/Habits/CatalogBrowseTests.cs`, `CatalogDetailsTests.cs` and `HomePreviewTests.cs`, and `Browse => new(_store)` in `CatalogHabitFieldTests.cs`, the two `new CatalogService(_store)` in `CatalogHabitStateTests.cs`, plus whatever the grep convention finds.
- [X] T092 [US6] Update the catalog pages for registered users (HP-005, HP-006; depends on T091):
  - `src/HT.Ui/Components/Pages/Catalog.razor`: "Show: Only habits I track", kept in the address as `tracked=true`, and "· you track N" after the count;
  - `src/HT.Ui/Components/Shared/FilterPanel.razor`: the "Show" option;
  - `src/HT.Ui/Components/Shared/HabitCard.razor`: the "N goals" Tag and "Add another goal", or "Use this habit", linking to `/goals/new?habit={id}&kind=catalog`;
  - `src/HT.Ui/Components/Pages/CatalogHabitDetail.razor`:
    - the head button "Use this habit";
    - "You track this · N goals" when N is at least 1;
    - "Start with this" on each suggested starting goal, adding `&suggestion={n}`;
    - "Your goals on this habit", each name linking to its goal page.

  Visitors and administrators keep slice 2's pages. They have no "Show" option (HP-005), so their catalog page ignores a `tracked=true` in the address and shows slice 2's list through the old `BrowseAsync(CatalogFilter)`. The use case's refusal of `onlyTracked` (US6-AS4) is for direct calls; the page never passes it for them. contracts/host.md does not say this; record it there in the same task.
- [X] T093 [P] [US6] Update `tests/HT.UiTests/Components/Pages/CatalogPageTests.cs`, `CatalogHabitDetailTests.cs` and `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs`, whose slice 2 HP-002 and HP-005 expectations change, for US6-AS5. Extend `tests/HT.UiTests/Host/CatalogHostTests.cs`: `tracked=true` for a registered user shows only tracked habits, and for a visitor gives 200 with slice 2's list, unfiltered, and no "Show" option (depends on T092)

**Checkpoint**: full suite green. SC-001: from a habit's details, "Start with this" and then "Create goal" create a
goal with two actions. Commit.

---

## Phase 9: User Story 7 - Unpublishing a habit that goals use (Priority: P2)

**Goal**: an administrator who unpublishes a catalog habit that goals use first sees the number of goals and
confirms. The goals keep working.

**Independent Test**: two users each create a goal on a habit; the administrator unpublishes it without, then with,
confirmation (US7-AS1, AS2).

### Tests for User Story 7

- [X] T094 [P] [US7] Create `tests/HT.UnitTests/Application/Habits/CatalogUnpublishWithGoalsTests.cs` (depends on T097):
  - US7-AS1: 2 goals give `ConfirmationNeeded` with `GoalCount` 2 and "2 goals use this habit.", and the habit stays Published;
  - US7-AS2: with confirmation, the habit is Unpublished;
  - US7-AS3: one goal gives "1 goal uses this habit.";
  - US7-AS4: no goals unpublishes at once, with or without confirmation;
  - US7-AS5: a reflection check that the result's public members are only `Outcome`, `Message` and `GoalCount` (SC-005);
  - the old overload means "not confirmed";
  - FR-023's order.
- [X] T095 [P] [US7] Create `tests/HT.IntegrationTests/Goals/GoalsOnChangedCatalogHabitTests.cs` on `HT-IT` (depends on T097):
  - US7-AS6: after a confirmed unpublish, the owner lists and reads the goal with `IsInCatalog` false and the settings unchanged, and a new goal on the habit gives "This habit is not in the catalog." to the owner and to another user;
  - US7-AS7: a rename by the administrator shows in the goal and its derived name;
  - FR-034: publishing again makes new goals possible again;
  - the spec's edge case: a goal created between the count and a confirmed unpublish does not stop it.

### Implementation for User Story 7

- [X] T096 [US7] Add `CountUsingCatalogHabitAsync(HabitId, …) → int`, the one member without an owner, to `IGoalStore`, `GoalStore` and `FakeGoalStore`. Its XML doc names ADR 0012. Confirm that T050's rule accepts it under exactly this name
- [X] T097 [US7] Extend `src/HT.Application/Habits/CatalogAdministrationService.cs` (FR-033; depends on T096):
  - its constructor gains `IGoalStore`;
  - the new overload `UnpublishAsync(ActingUser, HabitId, bool confirmed, …)` counts first without confirmation, then runs slice 2's compare-and-swap;
  - `ConfirmationNeeded` is appended to `CatalogHabitUnpublishOutcome`;
  - `CatalogHabitUnpublishResult.GoalCount` is a new `init` property, and the text comes from `CatalogTexts.GoalsUseThisHabit`;
  - the old overload means `confirmed: false`.

  Update the hand-built instances: the target-typed `Service => new(_store)` in `tests/HT.UnitTests/Application/Habits/CatalogHabitStateTests.cs`, `CatalogHabitFieldTests.cs` and `CatalogAdministrationAccessTests.cs`, plus whatever the grep convention finds.
- [X] T098 [US7] In `src/HT.Ui/Components/Pages/Admin.razor`, `ConfirmationNeeded` opens an `HtDialog` with the text, the sentence that the goals keep working for their owners, and the confirm button. Confirming calls `UnpublishAsync(…, confirmed: true)` (HP-008; depends on T097)
- [X] T099 [P] [US7] Update `tests/HT.UiTests/Components/Pages/AdminPageTests.cs`: the Dialog's text and sentence, confirm and cancel, an immediate unpublish with no goals, and no "Updated" column (depends on T098)

**Checkpoint**: full suite green. Commit.

---

## Phase 10: User Story 10 - My time zone (Priority: P2)

**Goal**: each account has its own time zone, taken from the browser at registration and changeable in Profile.
Goals follow it from their next day or week.

**Independent Test**: register with "Europe/Kyiv", create goals of each schedule kind, change the time zone to
"America/New_York" on a Wednesday, and read the calendars across the change (US10-AS4 to AS6).

### Tests for User Story 10

- [X] T100 [P] [US10] Create `tests/HT.UnitTests/Application/Accounts/TimeZoneTests.cs` for `ProfileService` (depends on T103):
  - `GetTimeZoneAsync` gives the stored zone, or the default when it is null;
  - US10-AS7: a value that is not a named time zone gives "Choose a time zone from the list." and changes nothing;
  - the same zone gives `Unchanged` and writes nothing;
  - US10-AS4 to AS6 and AS10, with a Mon/Wed/Fri goal, a per-day goal, a per-week goal and a goal starting on 1 December:
    - the versions written;
    - what `GoalService.GetAsync` and `GetCalendarAsync` then give;
  - "today" is decided in the new zone at once;
  - US10-AS8: an administrator reads and changes their own zone, and an anonymous visitor gets "You are not signed in.", as does a registered acting user with no account in the store;
  - a conflicting write makes the service re-read and recompute, and ten conflicts throw a fault exception that names no value;
  - US10-AS9: no method takes another account's identifier, which `UseCaseParameterTests` enforces structurally.
- [X] T101 [P] [US10] Create `tests/HT.IntegrationTests/Goals/TimeZoneChangeTests.cs` on `HT-IT` (research R-08; depends on T102, T103):
  - a change through `ProfileService` over SQL writes the account's zone, each goal's versions and `GoalsRevision + 1`;
  - `ReadOwnerGoalsAsync` returns the revision it read together with the goals;
  - `TryChangeTimeZoneAsync` with a stale `expectedRevision`, or a stale stored zone, writes nothing and returns false;
  - a write that names another account's goal, together with one of the owner's, writes nothing, leaves both goals' versions as they were and returns false;
  - a goal created against a time-zone change, forced with two contexts: the goal ends either with the old zone and a later version in the new one, or with the new zone from its start. It never ends with neither;
  - two concurrent changes: one retries, and the final state equals the later change applied to the earlier one.

  Extend `tests/HT.IntegrationTests/Accounts/AccountProfileStoreTests.cs`: a password change whose `HtUser` was loaded before a time-zone change committed keeps the new zone and `GoalsRevision` (research R-04).

### Implementation for User Story 10

- [X] T102 [US10] Add the time-zone write to the goal port (research R-08):
  - `OwnerGoals` and `GoalVersionWrite` in `src/HT.Application/Goals/`;
  - `ReadOwnerGoalsAsync(owner, …)` and `TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, …)` in `IGoalStore` and `FakeGoalStore`;
  - in `GoalStore`, the read takes the account row first, then the goals, and documents that order. The write is one transaction under the account row's update lock, conditioned on the stored zone and the revision:
    - `ReplaceFirst` updates version 1's zone;
    - `ReplacePending` replaces the pending version's row with one of the new zone and date;
    - `Add` inserts a version;
    - the account's zone is written, and `GoalsRevision` is incremented;
    - every version row it updates, deletes or inserts belongs to a goal of `owner`. A `GoalVersionWrite` that names another account's goal, or an unknown one, makes the whole write roll back and return false (constitution III). `FakeGoalStore` applies the same rule.
- [X] T103 [US10] Create `src/HT.Application/Accounts/ProfileService.cs` with `GetTimeZoneAsync` and `ChangeTimeZoneAsync`, and its results in `src/HT.Application/Accounts/ProfileResults.cs` (FR-024 to FR-027; depends on T102):
  - its constructor takes `IAccountStore`, `IGoalStore`, `TimeProvider` and `InstallationTimeZone`;
  - an acting user whose account is not found gets "You are not signed in.", as in `GoalService` (T058);
  - the change returns `Unchanged` before computing anything when the zone is the same;
  - otherwise it calls `GoalTimeline.ChangeOf` per goal, then the conditional write, retrying up to 10 times.

  Register it as scoped, add it to `AddGoals` in `PageServices.cs`, and add it to `SignedInServices` in `UseCaseParameterTests.cs`.
- [X] T104 [US10] Send the browser's time zone with the registration (HP-016, research R-12; depends on T041):
  - `src/HT.Ui/Components/Account/RegisterForm.razor` gets a hidden input marked `data-ht-browser-time-zone`, and calls the four-argument `RegisterAsync`;
  - new `src/HT.Ui/wwwroot/js/time-zone.js`: one document-level `submit` listener in the capture phase, which fills every marked input of the submitted form from `Intl.DateTimeFormat().resolvedOptions().timeZone`;
  - `src/HT.Ui/Components/App.razor` loads the script once, after `blazor.web.js`.
- [X] T105 [US10] Create `src/HT.Ui/Components/Account/TimeZoneForm.razor`, a static form on the Profile Account tab in `src/HT.Ui/Components/Pages/Profile.razor` (HP-015; depends on T103):
  - a select of `NamedTimeZones.Choices`, plus the current value when it is not among them;
  - Save;
  - the Notice "Your time zone is changed. Your goals use it from their next day or week; earlier days keep their time zone.";
  - a refusal's text under the field.
- [X] T106 [P] [US10] Add the UI tests (depends on T104, T105):
  - `tests/HT.UiTests/Components/Account/RegisterFormTests.cs`: the marked hidden input, and no visible time-zone field;
  - `tests/HT.UiTests/Host/ShellTests.cs`: `App.razor`'s output references `js/time-zone.js` once;
  - `tests/HT.UiTests/Host/RegisterPageTests.cs`: a posted `Europe/Kyiv` is stored, and an empty value stores the default;
  - `tests/HT.UiTests/Host/ProfilePageTests.cs`: the list has "UTC", "Europe/Kyiv" and "America/New_York", plus a stored "Europe/Kiev" when that is the current value, and a change shows the Notice.

  The script itself runs only in a real browser, so it stays unverified here (T136).
- [X] T107 [US10] Extend `tests/HT.IntegrationTests/Goals/PreSliceAccountMigrationTests.cs`: with `InstallationTimeZone.From("Europe/Kyiv")` registered, `ProfileService.GetTimeZoneAsync` gives `Europe/Kyiv` for the pre-slice account, before and after its first goal (HP-019) (depends on T103, T049)

**Checkpoint**: full suite green. Commit.

---

## Phase 11: User Story 8 - Goals in the navigation and on Home (Priority: P3)

**Goal**: registered users reach Goals and "+ New goal" from the navigation, and Home no longer says that goals come
later.

**Independent Test**: UI tests of TopNav, MobileNav and Home for each audience.

### Tests for User Story 8

- [X] T108 [P] [US8] Update `tests/HT.UiTests/Components/Layout/SiteNavTests.cs`, whose slice 2 HP-011 expectations change (depends on T110):
  - US8-AS1: a registered user's TopNav has "Goals" and "+ New goal", the bar's "+ New goal" carries `ht-hide-sm`, and MobileNav lists Goals, with "New goal" after the divider and before Profile;
  - US8-AS2: neither link for an administrator or a visitor.
- [X] T109 [P] [US8] Update `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs`, whose slice 2 HP-013 expectations change (depends on T111):
  - US8-AS5: a registered user with no goals sees T012's EmptyState;
  - "Goals come in a later version." is gone;
  - with goals, nothing more.

### Implementation for User Story 8

- [X] T110 [US8] Add "Goals" and "+ New goal" for registered users to `src/HT.Ui/Components/Layout/SiteNav.razor`, as T015 designed them (HP-009)
- [X] T111 [US8] In `src/HT.Ui/Components/Pages/Home.razor`, a registered user with no goals (`GoalService.ListAsync`) sees T012's EmptyState (HP-010). An administrator's Home is unchanged here (depends on T068)

**Checkpoint**: full suite green. Commit.

---

## Phase 12: User Story 11 - My name and the Home greeting (Priority: P3)

**Goal**: an optional private account name, a Home greeting by local time with today's date, and Profile's "member
since".

**Independent Test**: register with and without a name, then read the Home heading at fixed times on both sides of
each greeting boundary (US11-AS1, AS3).

### Tests for User Story 11

- [X] T112 [P] [US11] Create `tests/HT.UnitTests/Application/Accounts/ProfileNameTests.cs` (depends on T116):
  - US11-AS1 and AS2 for `ChangeNameAsync`: trimmed, spaces only remove the name, 60 accepted, and 61 gives the refusal with nothing changed;
  - `GetProfileAsync` gives the name, the email and `MemberSince`:
    - US11-AS6: "member since November 2026" for an account created on 17 November 2026 in Kyiv;
    - `CreatedAt` `2026-11-30T23:30:00Z` read in Kyiv gives "member since December 2026";
    - a null `CreatedAt` gives none;
  - US11-AS7: an administrator sets and reads their own name;
  - US11-AS8: an anonymous visitor gets "You are not signed in.".
- [X] T113 [P] [US11] Create `tests/HT.UnitTests/Application/Accounts/HomeHeadingTests.cs` for `GetHomeHeadingAsync` (depends on T116):
  - US11-AS3: "Yuri" in Kyiv at 04:59, 05:00, 11:59, 12:00, 17:59 and 18:00, as UTC literals from `2026-11-18T02:59:00Z` on;
  - US11-AS4: "Good evening" without a name;
  - US11-AS5: at `2026-11-16T23:30:00Z` in Kyiv, "Tuesday, 17 November" and "Good evening";
  - the same results inside a non-English `CultureScope`;
  - an anonymous visitor's refusal.
- [X] T114 [P] [US11] Add the integration tests (depends on T115, T116, T117):
  - extend `tests/HT.IntegrationTests/Accounts/AccountProfileStoreTests.cs`: `TrySetNameAsync` round-trips, gives false for an unknown account, and a password change with a stale `HtUser` keeps the name;
  - extend `tests/HT.IntegrationTests/Accounts/ClaimsPrincipalFactoryTests.cs`: `ht:name` is present with a name and absent without one;
  - extend `tests/HT.IntegrationTests/Goals/PreSliceAccountMigrationTests.cs`: the pre-slice account's `GetProfileAsync` gives no name and no "member since" (HP-019, the API-untestable half of US11-AS6).

### Implementation for User Story 11

- [X] T115 [US11] Add `TrySetNameAsync(AccountId, AccountName?, …) → bool` to `IAccountStore`, to `AccountStore` (one `ExecuteUpdate` scoped by account) and to `FakeAccountStore`
- [X] T116 [US11] Add `ChangeNameAsync`, `GetProfileAsync` and `GetHomeHeadingAsync` to `src/HT.Application/Accounts/ProfileService.cs`, with `ProfileView` and `HomeHeading` (FR-028 to FR-030, research R-11; depends on T115, T103, T037):
  - the greeting hour comes from `NamedTimeZone.TimeAt`, with the bands 05:00 to 11:59, 12:00 to 17:59 and the rest;
  - the date comes from `DateTexts.LongDate`, and "member since" from `MonthYear` of `CreatedAt` in the account's zone.
- [X] T117 [US11] In `src/HT.Persistence/Identity/HtUserClaimsPrincipalFactory.cs`, add the claim `ht:name` when the account has a name (depends on T045)
- [X] T118 [US11] Add `Avatar.LetterOf(string? name, string email)` to `src/HT.Ui/Components/Shared/Avatar.razor`: the first letter of the name, upper-cased with `char.ToUpperInvariant`, or of the email. `SiteNav.razor` and the Profile header read the name from `ht:name` (HP-017; depends on T117, T110)
- [X] T119 [US11] In `src/HT.Ui/Components/Pages/Home.razor`, replace the "Today" heading with the greeting as `heading-1` and the date from `GetHomeHeadingAsync`, for registered users and administrators (HP-010; depends on T116, T111)
- [X] T120 [US11] Add the name to the pages (HP-017; depends on T116, T118, T105):
  - `src/HT.Ui/Components/Account/NameForm.razor` on the Profile Account tab, with the Notice "Your name is changed.";
  - after a name change, `Profile.razor` refreshes the session cookie as the password change does (`RefreshSessionAsync`), so TopNav shows the new letter;
  - the Profile header shows the name when set as its `h1`, then the email, and "member since …" when known; without a name the email stays the `h1`, as in T013's preview. KeyValue replaces the rows the page wrote as `.ht-kv` by hand. The smoke check reads the name from this `h1` (T131);
  - `RegisterForm.razor` gets the optional "Name" field.
- [X] T121 [P] [US11] Add the UI tests (depends on T118–T120):
  - `LetterOf` cases in `tests/HT.UiTests/Components/Shared/SharedComponentTests.cs`;
  - `SiteNavTests.cs`: the avatar letter comes from the name, or from the email without one;
  - `HomeSignedInTests.cs`: the greeting is `heading-1`, followed by the date, for a registered user and an administrator, whose slice 1 "Today" expectation changes;
  - `tests/HT.UiTests/Host/ProfilePageTests.cs`: the header with and without a name and "member since", the name form's Notice, and the letter after the cookie refresh;
  - `RegisterFormTests.cs`: the optional Name field.

**Checkpoint**: full suite green. Commit.

---

## Phase 13: User Story 9 - Demo goals (Priority: P3)

**Goal**: demo loading gives each demo account it creates the goals of FR-040, after loading the seed catalog
first. The host logs them, and the smoke check finds them.

**Independent Test**: reset the database, run demo loading with a valid demo password, and list each demo account's
goals (US9-AS1, AS5).

### Tests for User Story 9

- [ ] T122 [P] [US9] Create `tests/HT.UnitTests/Application/Operator/DemoGoalsTests.cs` (depends on T127):
  - `DemoGoals.For(email, today)` gives exactly the FR-040 goals per account, with `today` and `today + 7` for Meditation;
  - every target passes `GoalDraft`;
  - `DisplayNames(email)` gives "Reading, 15 min, 5 days a week", "No phone in bed, avoided every day", "Morning yoga", "Meditation, 10 min, 3 days a week", "Sugar, at most 1 sweet a day", "Late snacks, at most once a day" and "Coffee, at most 2 cups a day", each under its account;
  - the profile of demo1 has the name "Demo One", and demo2 and demo3 have none;
  - no demo goal uses "Cold shower", "Evening stretch" or "Plan tomorrow".
- [ ] T123 [P] [US9] Rewrite `tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs` for the new order (depends on T129):
  - rename and invert `The_work_runs_in_order_each_account_with_its_custom_habits_then_the_seed_catalog` and `An_existing_account_is_skipped_and_the_seed_catalog_still_comes_last`: the password checks, then the seed catalog, then each account;
  - a created account gets `Goals` "demo goals added", and an existing account gets none and nothing of its data is read;
  - "the day of the run" is `InstallationTimeZone`'s date at the supplied time, with a non-UTC default and a time just after its midnight;
  - the profile passes the default zone and the supplied `CreatedAt`.
- [ ] T124 [P] [US9] Create `tests/HT.IntegrationTests/Operator/DemoGoalsTests.cs` on `HT-IT-SEED` (depends on T128, T129):
  - US9-AS1 and AS5 on an empty database:
    - each demo account's goals, through `GoalService.ListAsync` as that account;
    - demo3's "Coffee" goal with `Habit.IsInCatalog` false, read through `GoalService.GetAsync` (`GoalListItem` has no `IsInCatalog`);
    - demo2's "Morning yoga";
    - demo1's name "Demo One", read through `IAccountStore`;
  - US9-AS2: a second run gives no goal result and leaves the goals unchanged;
  - US9-AS3: with an administrator-made catalog of Published "Yoga", "Meditation" and "Sugar" and Draft "Reading" and "Coffee", demo1 gets only "No phone in bed", demo2 both goals, and demo3 "Sugar" and "Late snacks" but not "Coffee";
  - US9-AS4: `LogCapture` finds no goal name, habit name, account name or setting.

  Update `tests/HT.IntegrationTests/Operator/SeedCatalogLoadingTests.cs` only where its `CreateDemoAccountAsync` call gains the profile and an empty goal list. Its assertions stay.
- [ ] T125 [P] [US9] Update `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` for HP-013: "Demo catalog" first, then for each demo email its account line, its custom-habit line and its new `Demo goals {Email}: {Result}` line. No line holds a goal, habit or account name (depends on T130)
- [ ] T126 [P] [US9] Update the smoke check's tests (HP-018; depends on T131):
  - `tests/HT.UiTests/SmokeChecks/RunnerBehaviourTests.cs`: `GoodSite()` scripts `GET /goals` with every display name in `ht-goalitem__name` links and `GET /profile` with "Demo One" as the first heading, plus failing cases for a missing goal name and a missing name;
  - `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`: `ReadGoalNames`, with entities, nested markup and other classes on the same element, as the `ReadHabitNames` tests do;
  - `tests/HT.UiTests/SmokeChecks/CoverageTests.cs`: its two new entries;
  - `tests/HT.UiTests/Host/SmokeCheckHostTests.cs` runs the real runner against a seeded in-process host and must stay green: a fresh host passes every check, with the goals and demo1's name, and a demo account that lost a custom habit still fails only its own check. This needs US11's Profile header (now earlier, Phase 12), which is why US9 comes after US11.

### Implementation for User Story 9

- [ ] T127 [US9] Create the demo goal types in `src/HT.Application/Operator/` (FR-040, research R-10):
  - `DemoGoal`;
  - `DemoGoals`, with `For(email, today)` and `DisplayNames(email)` derived with `GoalText` and the seed habit names;
  - `DemoAccountProfile`;
  - `DemoGoalsResult`;
  - `DemoAccountResult.Goals`, a new `init` property;
  - `DemoAccounts.ProfileFor(email)`, a new static class `DemoAccounts` as research R-10 names it, which gives the account name ("Demo One" for demo1, none for demo2 and demo3). The emails stay in `DemoDataService.DemoEmails`.
- [ ] T128 [US9] Extend the demo data port (research R-10, HP-018; depends on T127, T046):
  - `IDemoDataStore.CreateDemoAccountAsync` gains `DemoAccountProfile profile` and `IReadOnlyList<DemoGoal> goals`. Its XML doc widens "the only read is whether the catalog is empty" to "the state of the named catalog habits";
  - `src/HT.Persistence/Habits/DemoDataStore.cs`, in the account's one transaction:
    - sets `HtUser`'s name, zone and `CreatedAt`;
    - looks each named catalog habit up by name key and accepted state;
    - uses the identifiers it has just generated for the custom-habit positions, without reading them;
    - writes the qualifying goals, each with version 1 and attempt 1, and skips the others with no error;
  - `tests/HT.UnitTests/Fakes/FakeDemoDataStore.cs`.
- [ ] T129 [US9] Extend `src/HT.Application/Operator/DemoDataService.cs` (FR-040; depends on T128):
  - its constructor gains `TimeProvider` and `InstallationTimeZone`;
  - `LoadAsync` runs the password checks, then the seed catalog, then each account;
  - today is computed once in the default zone;
  - each created account's profile and goals are passed to the store;
  - goal results are set.

  Update the hand-built instances.
- [ ] T130 [US9] In `src/HT.Ui/Operator/StartupTasks.cs`, log the lines in HP-013's order: `Demo catalog: {Result}` first, then per demo email its account line, its custom-habit line and `Demo goals {Email}: {Result}` (depends on T129)
- [ ] T131 [US9] Extend the smoke check (HP-018; depends on T127, T069 and T120):
  - `tools/HT.SmokeCheck/SmokeCheckCoverage.cs` gets the entries `CreateDemoAccountAsync`/`profile` and `CreateDemoAccountAsync`/`goals`;
  - `tools/HT.SmokeCheck/PageReader.cs` gets `ReadGoalNames(html)`, which reads the text of every `<a class="ht-goalitem__name">` (`GoalItem.razor`, T069) the way `ReadHabitNames` reads `ht-habit__name`;
  - `tools/HT.SmokeCheck/SmokeCheckRunner.cs`'s check 5, after the custom habits, reads `GET /goals` for every `DemoGoals.DisplayNames(email)` with `ReadGoalNames`, and on demo1 `GET /profile` for "Demo One" with the existing `ReadFirstHeading` (the Profile header's `h1` is the name when one is set, T120). It stays one line per account:
    - `<expected>` counts every expected name of the three pages together: the custom habits, the goal display names, and on demo1 the account name. `<found>` counts those present on their own page, so a healthy account's line has `<found>` equal to `<expected>`;
    - a `FAIL` names the first part that misses, in that order, as `NamesLine` does now: `{n} of {m} custom habits shown`, `{n} of {m} goals shown`, or `account name not shown`. Every page is still read, and no name is printed;
  - in `specs/003-demo-hosting/contracts/smoke-check.md`, extend check 5's row of the checks table with the goals and the name, write the counting and `FAIL` rule above into "Output", and add the two rows to the coverage table ("Coverage rule").

**Checkpoint**: full suite green. A start against a dropped local `HT` database logs the HP-013 lines and shows the
demo goals (quickstart §3). Commit.

---

## Phase 14: Polish and cross-cutting concerns

**Purpose**: documentation, the API and analyzer comparisons, the accessibility sweep and the manual quickstart.

- [ ] T132 [P] Update the slice 4 status line of `README.md` ("Status"). Check `CONTEXT.md`: change an entry only if the implementation sharpened one of the terms HP-014 lists, in the same change
- [ ] T133 Compare the public API of `HT.Application` and `HT.Domain` with T002's baseline (quickstart §1, research R-07). The only differences allowed are:
  - additions;
  - the documented constructor changes;
  - the port changes of `IAccountStore`, `ICustomHabitStore` and `IDemoDataStore`.

  Report any other difference as a defect.
- [ ] T134 [P] Run `roslynator analyze HabitTracker.slnx --projects HT.Domain HT.Application HT.Persistence`, and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING` with its cache in the session scratchpad. Compare both with the `CLAUDE.md` baselines, and fix real findings only; the documented false positives stay. Never point either tool at `tests/HT.AcceptanceTests`
- [ ] T135 [P] Sweep the new and changed pages for constitution IV and HP-012 (SC-007):
  - tokens and `ht-` classes only, with no literal colors;
  - visible focus;
  - status never shown by color alone;
  - the habit chooser's focus handling;
  - sentence case per `design/brand-book.md`.

  Then rerun `DesignSyncTests`.
- [ ] T136 Run the quickstart's §3 in the running app after dropping the local `HT` database. Report each row's result. The HP-016 row (registering from a browser in another time zone) needs a real browser, so it is reported as unverified unless the owner checks it by hand
- [ ] T137 Run the full build and `dotnet test HabitTracker.slnx`, and report `PASS` with the counts. Leave slice 4's roadmap Status at `implementing`: `AC tests green` comes after the owner's `/speckit-ac-tests-analyze`. Commit

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies. T003 waits for the owner's yes.
- **Foundational (Phase 2)**: after Setup. It blocks every story.
- **User stories (Phases 3 to 13)**: after Phase 2, in this order of priority:
  - P1: US1, US2, US4, then US3;
  - P2: US5, US6, US7 and US10;
  - P3: US8, US11, then US9. US11 comes before US9, because US9's smoke check reads the name that US11 puts on
    the Profile page.
- **Polish (Phase 14)**: after the stories that are to ship.

### Story dependencies

- **US1**: none beyond Phase 2.
- **US2**: after US1. Its tests create goals through `CreateAsync`, and its page is where `/goals/new` navigates.
- **US4**: after US2. T075 adds to `GoalService.cs` and `GoalResults.cs` after T068.
- **US3**: after US4. Its tests cover every goal use case, the calendar included.
- **US5**: after US2. T086 links to US1's `/goals/new`, and T080 reads goals through US2's `GetAsync`.
- **US6**: after US2. T091 derives goal names as `GetAsync` does, and T092 links to US1's page.
- **US7**: after US2. T095 reads goals on an Unpublished habit, and T071 already tests the Notice.
- **US10**: after US4. T100 reads calendars across the change.
- **US8**: after US2. T111 uses `ListAsync`.
- **US9**: after US2, to read the demo goals, and after US11: check 5 reads demo1's name from the Profile
  header (T120), and `SmokeCheckHostTests` runs that check against an in-process host, so it fails until the
  header shows the name.
- **US11**: after US10, whose `ProfileService`, `Profile.razor` and `RegisterForm.razor` it extends, and after US8, whose `SiteNav.razor` and `Home.razor` it extends.
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `GoalService.cs`: T058, T068, T075;
  - `GoalResults.cs`: T057, T067, T075;
  - `IGoalStore.cs`, `GoalStore.cs` and `FakeGoalStore.cs`: T040 and T046, T083, T090, T096, T102;
  - `ApplicationServiceCollectionExtensions.cs`: T038, T058, T103;
  - `UseCaseParameterTests.cs`: T050, T058, T091, T103;
  - `ProfileService.cs`: T103, T116;
  - `Profile.razor`: T105, T120; `tests/HT.UiTests/Host/ProfilePageTests.cs`: T106, T121;
  - `RegisterForm.razor`: T104, T120; `RegisterFormTests.cs`: T106, T121;
  - `Home.razor`: T111, T119; `HomeSignedInTests.cs`: T109, T121;
  - `SiteNav.razor`: T110, T118; `SiteNavTests.cs`: T108, T121;
  - `StartupTasks.cs`: T051, T130;
  - `AccountProfileStoreTests.cs`: T048, T101, T114;
  - `PreSliceAccountMigrationTests.cs`: T049, T107, T114;
  - `GoalsHostTests.cs`: T063, T072;
  - `SharedComponentTests.cs`: T052, T060, T121;
  - `HabitComponentTests.cs`: T069, T093;
  - `FakeAccountStore.cs`: T041, T115;
  - `tests/HT.UiTests/Components/Pages/PageServices.cs`: T040, T058, T103;
  - `tests/HT.UnitTests/Application/Habits/CatalogHabitFieldTests.cs` and `CatalogHabitStateTests.cs`: T091, T097.

### Within each story

- A test task starts once the tasks it names as dependencies are done. These are usually the use case it calls,
  because a test that does not compile cannot fail meaningfully. It then runs in parallel with the story's
  remaining work (components, page, host tests). The order of the implementation is: the results, the use case, the store member and its
  registration together, the components, the page, and the host tests. Nothing here requires a test to be written
  before its use case.
- The story's checkpoint runs the full suite before the next story, and is its commit point.

### Parallel opportunities

- Phase 1: T002 and T004, while T003 waits for the owner.
- Design: T005 to T015, then T016, T017 and T018.
- Domain: T019, T020, T023 and T029; then T022, T024, T027; then T025, T026, T028; then the tests T021 and T030 to
  T034.
- Application foundation: T037, T038 and T039, then T036 (after T037), T040 and T041.
- Persistence: T047, T048 and T049 once T046 is done.
- In each story, every task marked [P].
- After US2, US5, US6, US7 and US8 touch different files, apart from the shared files above, so two of them can
  run side by side. US9 waits for US11.

---

## Parallel example: Phase 2 Domain

```text
Task: "T019 NamedTimeZone in src/HT.Domain/Time/NamedTimeZone.cs"
Task: "T020 NamedTimeZones in src/HT.Domain/Time/NamedTimeZones.cs"
Task: "T023 Goal value types in src/HT.Domain/Goals/"
Task: "T029 Account init properties and AccountName in src/HT.Domain/Accounts/"

# after T028:
Task: "T032 Calendar boundary tests in tests/HT.UnitTests/Domain/Goals/GoalTimelineCalendarTests.cs"
Task: "T033 Change tests in tests/HT.UnitTests/Domain/Goals/GoalTimelineChangeTests.cs"
```

## Parallel example: User Story 1

```text
# after T058:
Task: "T053 [US1] Starting values tests in tests/HT.UnitTests/Application/Goals/GoalStartingValuesTests.cs"
Task: "T054 [US1] Creation tests in tests/HT.UnitTests/Application/Goals/GoalCreateTests.cs"
Task: "T055 [US1] SC-004 test in tests/HT.UnitTests/Application/Operator/SeedCatalogTests.cs"
Task: "T056 [US1] Creation and race tests in tests/HT.IntegrationTests/Goals/GoalCreationTests.cs"
Task: "T059 [US1] GoalSummaryText in src/HT.Application/Goals/GoalSummaryText.cs"
Task: "T060 [US1] ChoiceCard in src/HT.Ui/Components/Shared/ChoiceCard.razor"
```

## Parallel example: User Story 3

```text
Task: "T076 [US3] Access tests in tests/HT.UnitTests/Application/Goals/GoalAccessTests.cs"
Task: "T077 [US3] Privacy tests in tests/HT.IntegrationTests/Goals/GoalPrivacyTests.cs"
Task: "T078 [US3] HP-011 log tests in tests/HT.IntegrationTests/Goals/GoalLogTests.cs"
Task: "T079 [US3] Host privacy tests in tests/HT.UiTests/Host/GoalPrivacyHostTests.cs"
```

## Parallel example: User Story 9

```text
# after T127 to T131:
Task: "T122 [US9] Demo goal tests in tests/HT.UnitTests/Application/Operator/DemoGoalsTests.cs"
Task: "T123 [US9] Demo loading order in tests/HT.UnitTests/Application/Operator/DemoLoadingTests.cs"
Task: "T124 [US9] Demo goals on HT-IT-SEED in tests/HT.IntegrationTests/Operator/DemoGoalsTests.cs"
Task: "T125 [US9] Log order in tests/HT.UiTests/Operator/StartupTasksDemoTests.cs"
Task: "T126 [US9] Smoke check tests in tests/HT.UiTests/SmokeChecks/"
```

---

## Implementation strategy

### MVP first (the P1 stories: US1, US2, US4, US3)

1. Phase 1 and Phase 2: design first, then Domain with its boundary tests, the Application and Persistence
   foundation, and the host's default time zone.
2. US1: goals are created on `/goals/new`, with the summary card.
3. US2: the Goals page and the goal page.
4. US4: the evaluation calendar, the time model the later slices build on.
5. US3: privacy proven for every goal use case (SC-002).
6. **Stop and validate**: run the full suite, then check in the browser that goals are created, listed and opened.

### Incremental delivery

1. US5, US6 and US7: goals on custom habits, the catalog with goal information, and the unpublish confirmation.
2. US10: the account's time zone and its changes.
3. US8, US11 and US9: the navigation and Home, the name and the greeting, then the demo goals and the smoke check.
4. Polish. Then the owner runs `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, and expects the F001
   support code to need its cross-feature change (plan, Risks).

One PR for the slice. Each checkpoint is a commit on the feature branch.

---

## Notes

- [P] tasks touch different files, and start once the tasks they name as dependencies are done (Format).
- The AC project, its features, the locks and the run records are never edited.
- The plan's positions ("Spec discrepancies" 1 to 5) and ADRs 0016 and 0017 are accepted; the tasks implement them
  as stated.
- These additions to the design artifacts were found while writing and reviewing these tasks. The task named
  records each in data-model.md, the contract or research.md:
  - `GoalListResult.Today` (T067), so the Goals page says "next today" without reading a clock;
  - `GoalDetails.HasStarted` (T067), so the attempt line chooses "started" or "starts" by the goal's own current
    date (FR-012), as the list does (plan "Spec discrepancies" 5);
  - `GoalText.ScheduleText` (T026), with `ScheduleText` on `GoalListItem` and `GoalDetails` (T039): the list has
    no days, so the Goals page's "Mon · Wed · Fri" needs it (HP-003);
  - `DateTexts.DayMonthYear` (T037), for the Settings card's "Started 14 Jul 2026" (HP-004);
  - a visitor's or administrator's `/catalog?tracked=true` shows slice 2's list unfiltered (T092, contracts/host.md).
- A goal or profile use case whose acting user has no stored account answers "You are not signed in." (T058,
  T103; recorded in the contract by T058). The bUnit page tests opt into `FakeAccountStore.KnowsEverySignedInUser`
  (T040), the unit tests do not.
- The design step settles three texts the spec leaves open: the summary card's "Met when" values and its
  incomplete-form state (T005), and the slice-4 Home EmptyState (T012). The code and the tests take them from
  there.
- The registration overload, with its zone and name, is in Phase 2, because `IAccountStore.CreateAsync` changes
  there for every creator. US10 and US11 add the pages that use it.
- Stop at any checkpoint to validate a story on its own.
