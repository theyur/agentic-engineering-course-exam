# Implementation Plan: Goal changes

**Branch**: `claude/speckit-specify-slice-8-4f4bd7` | **Date**: 2026-11-22 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/009-goal-changes/spec.md`;
- the acceptance criteria [acceptance.md](acceptance.md) and
  `tests/HT.AcceptanceTests/Features/F009/goal-changes.feature`, both locked (2026-11-22);
- [ADR 0025](../../docs/adr/0025-goal-change-rules.md) (D-1, D-2 and its two confirmed consequences) and
  [ADR 0026](../../docs/adr/0026-demo-goal-change-loading.md) (D-3), with ADR 0016 (time zones), ADR 0018 and
  ADR 0020 (evaluation and figures on read), and ADR 0024 (excusals and pauses).

## Summary

A registered user can:

- change a goal's settings within its schedule kind, from the next boundary at the goal's current date, and rename it
  at once;
- end a goal with an outcome and an optional note, which takes the commitment or period in progress and everything
  after it out of the attempt, so ending never creates a failure;
- start a new attempt of an ended goal, with any settings and a start date after the ending date, and its own figures
  from "N/A" and 0.

Earlier attempts keep their figures and their correctable History in an attempt view. Ended goals leave the active
views but stay on the Goals page's Ended tab and in the Profile's Goal history. A sixth startup task, demo
goal-change loading, creates `demo7@example.com` with a changed target, an ended goal and a goal on attempt 2.

The work splits by layer:

- **`HT.Domain`**:
  - `Goal` holds its attempts, each with its own settings versions and an optional `GoalEnding`: the ending date, the
    `Outcome` and the `EndingNote` (research R-01, R-04);
  - `GoalTimeline` becomes attempt-aware, so every existing reader respects attempt boundaries, also for a calendar
    across attempts of two schedule kinds. It gains `LastDate`, `AttemptOf`, `EntriesThrough(goal, number, last)`,
    `VersionInEffect` (both overloads), `LatestAttemptHasBegun`, `SettingsChangeOf`, `EndingOf` and
    `NewAttemptEarliestStart`, with the result records `GoalSettingsChange` and `GoalEndingEffect` (R-02 to R-04);
  - `GoalText` gains `Name(goal, habitName, instant)` and `AttemptName` (R-08);
  - the trend draws an entry of another measurement by its result only (R-07).
- **`HT.Application`**:
  - `GoalChangeService` (change, end, starting values, start attempt) and `AttemptService` (attempts, one attempt,
    goal history), both owner-scoped and taking the acting user first (R-06);
  - `GoalEvaluation` evaluates one attempt with that attempt's own settings, with the latest attempt as the report
    range, and exposes the earlier attempts' open Pending entries (R-07);
  - every `HistoryEntry` carries its own `EntrySettings`, and `GoalHistory.SettingsOf`, `RecordPreview` and the
    `RecordingText` overloads on it give every view of one entry that entry's settings (R-07);
  - recording paths no longer throw for a record or note outside every attempt (R-07);
  - `CatalogService` gets a constructor with a `TimeProvider`, so it names goals at one time read (R-08);
  - every earlier read follows FR-016 to FR-021: reading a goal, the list, the overview, the dashboard, what can be
    recorded now, the History, recent notes, pausing and excusing (R-07);
  - demo goal-change loading adds `DemoGoalChangeService`, `DemoGoalChanges` and the port `IDemoGoalChangeStore`
    (R-09).
- **`HT.Persistence`**:
  - one migration, `GoalChanges`: the ending columns on `GoalAttempts`, and `AttemptNumber` in the key of
    `GoalSettingsVersions` (R-05);
  - three conditional writes on the account and goal locks with the goal revision, and one new read;
  - catalog counts without ended goals, a pause re-check after an ending, and the recent-notes overload;
  - `DemoGoalChangeStore` over slice 6's `DemoAccountWriter`.
- **`HT.Ui`**:
  - the goal page gets Edit settings, End goal, the ended Notice, the Settings card's "From …" row and attempt start
    row, the attempt line and the Earlier attempts card;
  - the History card, the RecordDialog, the NoteDialog and `CommitmentRow` take each row's own
    settings;
  - three new pages: Edit settings, Start new attempt and the attempt view;
  - a new EndGoalDialog;
  - the Goals page gets the Ended tab, the Profile the Goal history tab, and the Home the "Ended" and "Attempt {n}"
    Tags;
  - the host gets the seventh startup step, and the smoke check gets check 9.

  `design/` changes first (constitution IV, ADR 0008; R-10, R-11).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and
warnings as errors. Unchanged.

**Primary Dependencies**: unchanged. ASP.NET Core 10 (Blazor static SSR with Interactive Server islands), EF Core 10
with SQL Server, Identity's EF stores, and NodaTime 3.2.x inside `HT.Domain` only (ADR 0017). No new package.

**Storage**: SQL Server, the databases of slices 1 to 7, plus one migration, `GoalChanges`
([data-model.md](data-model.md#tables-migration-goalchanges)):

- `GoalAttempts` gains `EndingDate`, `Outcome` and `EndingNote`, all null for an attempt that is not ended;
- `GoalSettingsVersions` gains `AttemptNumber`, in its key and in a foreign key to `GoalAttempts`. Existing rows
  become attempt 1.

Only facts are stored: versions, attempts and endings. Last dates, states and figures are derived (FR-026).

**Testing**:

- xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests, and `FakeTimeProvider`
  (ADR 0006).
- Stryker for the new and changed Domain and Application code (ADR 0022).
- The acceptance tests go through the AC workflow (ADR 0003).
- Details: [research.md R-12](research.md#r-12-tests).

**Target Platform**: a browser-based web app over HTTPS. Windows developer machines, CI on Ubuntu, and the hosted
demo on Azure App Service, refreshed daily (ADR 0015, ADR 0018). Unchanged.

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**:

- A report still evaluates one attempt in memory from one record read.
- The dashboard and the overview evaluate an earlier attempt only when its last entry ended in the last 48 hours
  (R-07), so most goals cost what they cost in slice 7.
- The attempt list costs one record read per attempt, and the goal history one goal list read.
- Slice 6's SC-007 timing test (3 goals, one with 366 daily results, each read under 2 seconds) is re-run unchanged
  and must still pass. Everything else is demo scale.

**Constraints**:

- No clock, time-zone or culture read in Domain or Application (constitution II). Time enters as `TimeProvider`, read
  once per call.
- Nothing derived is stored. The last date is derived from the stored ending date (FR-026, ADR 0018, ADR 0020).
- Every new use case and store member is owner-scoped. An attempt is found only by its number among the attempts of
  the named goal of the acting user (constitution III, FR-022).
- No ending note, note, recorded value, goal, habit name or unit in any log, exception or error page (HP-010).
- **Locked AC code.** The constructors, method signatures, positional records and port members of slices 1 to 7 stay
  as they are, so the locked AC code of F001 to F008 compiles and passes unchanged (FR-024). Only these are added:
  - enum members;
  - init-only properties that are not `required`;
  - overloads, a second `Goal` constructor and new types;
  - new port members.

  Existing members keep their meaning for a goal with one attempt, no ending and no change (R-01, R-02).
- `design/` comes first, and the pages use tokens only (constitution IV).

**Scale/Scope**:

- 7 new use cases in 2 services, and 1 operator use case (`DemoGoalChangeService`).
- 4 new `IGoalStore` members, 1 new `IRecordStore` overload, 1 new port (`IDemoGoalChangeStore`), 4 new columns, a
  re-keyed table and 1 migration.
- 4 changed pages: goal page, Goals, Profile and signed-in Home. 3 new pages: Edit settings, Start new attempt and
  the attempt view.
- 2 new components, EndGoalDialog and EarlierAttemptsCard.
- Changed components:
  - `NewGoalForm` (edit and attempt modes) and `GoalSettingsCard` ("From …", the attempt's start row);
  - the shared `GoalItem` (ended form) and `CommitmentRow` (Tag);
  - the islands `GoalsList` (Ended tab) and `TodayPanel`;
  - `HistoryCard` (it also serves the attempt view), `RecordDialog`, `NoteDialog`, `CommitmentRow` and `GoalItem`,
    which take each row's own settings (R-07). `RecentNotesCard` keeps the goal-level schedule kind: recent notes cover only the latest attempt, whose kind never changes (D-1).
- 1 new startup task and 1 new smoke check.
- 3 demo goals for one new demo account.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass. No `[NEEDS CLARIFICATION]` marker is open:*

- *SPEC §11.7's second part is decided as D-1, and D-2 with its two consequences is confirmed by the owner. Both are
  in ADR 0025.*
- *D-3 is decided and in ADR 0026.*
- *No spec discrepancy blocks planning (see "Spec discrepancies").*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | **Names.** Types and members use the glossary: `Outcome` (`Achieved`, `Abandoned`, `NoLongerRelevant`), `GoalEnding`, `EndingNote`, `GoalChangeService`, `ChangeSettingsAsync`, `EndAsync`, `StartAttemptAsync`, `AttemptService`, `GetAttemptsAsync`, `GetGoalHistoryAsync`, `LastDate`, demo goal-change loading. No "archive", "close", "reset", "retry" or "edit history".<br>**Code groupings.** `GoalAttemptVersions` and the write records group code; they are not new domain terms.<br>**Homonym.** The glossary separates *Goal history* (a user's ended attempts) from a goal's *History* (one goal's dates). The existing type `GoalHistory` (slice 5) means the latter and cannot be renamed (locked AC code). So the new goal-history types avoid the word: `EndedAttemptView`, `GoalHistoryListResult`, and only the use case is called `GetGoalHistoryAsync`, as the spec names it. The `GoalHistory` doc comment names the distinction.<br>**Glossary.** The `CONTEXT.md` entries of HP-013 were added with the spec on 2026-11-22. |
| II. Deterministic, Reproducible Evaluation | met | **One function.** Each attempt's dates, current date, last date and versions are pure functions of its stored facts and the supplied instant, in `GoalTimeline` (R-02). Every reader goes through it, and every figure through the one `GoalEvaluation` (R-07).<br>**Versions.** Settings changes are versions. A change never alters the version of a begun date (FR-002, SC-002). Ending removes only versions that judged no date of the attempt (FR-009). Slice 4's `ReplacePending` already removes such a never-applied version. So every historical evaluation can still be recomputed against the rules that applied to it.<br>**Next read.** Change, end, restart and corrections show on the next read through the same functions (FR-025).<br>**Boundaries.** Unit tests cover both sides of each boundary of SC-005: a change at 23:59:59 and 00:00 local; before and after a per-week boundary; before a waiting version takes effect; an ending on, before and after the start date; per-week endings on a Sunday and a Monday; a restart on the ending date and the day after, also with the account's today a day behind the goal's timeline (R-12).<br>**Clock rule.** `ClockReadTests` covers every Domain and Application type, the new ones included. |
| III. Privacy Enforced Below the UI | met | **Access.** `GoalChangeService` and `AttemptService` take the acting user first and refuse the anonymous visitor and administrators before the goal (FR-023).<br>**Scoping.** Each new store member takes the owner and the goal, and acts only when the goal is the owner's, in the same statement. An attempt is found only by number within that goal (FR-022). The goal history reads only the owner's goals.<br>**Tests.** Each of the seven use cases has negative tests for another user, an administrator and an anonymous visitor (SC-006). Integration tests prove store scoping.<br>**Logs.** `EndingNote` has no value-printing `ToString`. Log capture proves HP-010.<br>**Demo data.** Demo goal-change loading writes only for the account it creates, through a port fenced to its service (ADR 0013). |
| IV. Design-System Fidelity and Accessible UI | met | **Design first.** `design/` changes first (Work order step 1).<br>**Components.** EndGoalDialog and EarlierAttemptsCard are each one Razor component. `NewGoalForm` is reused by Edit settings and Start new attempt. `HistoryCard`, `RecordDialog`, `NoteDialog` and `CommitmentRow` are reused with one change: each row takes its own `Settings` (R-07). `GoalStats`, `Tag`, `Tabs`, `DataTable` and `Notice` are reused as they are.<br>**Texts.** All texts come from `ReportText` and `GoalChangeTexts` (HP-008). Outcome Tags carry their word, never colour alone.<br>**Accessibility.** The dialog and the forms manage focus through `HtDialog` and the form conventions of slices 4 and 7 (HP-009). bUnit tests check labels, focus order, Esc and the focus return of every page and dialog of this slice (SC-008). |
| V. Verified, Not Asserted | met | **Verification.** Build and the full suite run before any report of done. Steps 2 to 4 of the Work order are verified and committed together.<br>**Architecture rules** (R-12, as slice 7 extended them):<br>• the `IGoalStore` rule admits `AttemptService`;<br>• `IDemoGoalChangeStore` is fenced to `DemoGoalChangeService` in the Application and from every HT.Ui type;<br>• `DemoGoalChangeService` joins the operator-services rule;<br>• `DemoGoalChangeStore` joins the internal demo stores;<br>• the new public types join `PublicSurfaceTests`;<br>• `UseCaseParameterTests` covers the new members. Stryker runs over the new and changed code (ADR 0022).<br>**Enforcing mechanism.** The preconditions that must hold under concurrency (one later version, one attempt that is not ended, no pause after an ending) are re-checked in the store transaction, not only in the use case (R-05).<br>**Unverified.** The hosted smoke check and the daily refresh are labelled unverified, each with a quickstart step.<br>**Gate.** The pre-commit gate stays on. |
| VI. Demonstrable Vertical Slices | met | **Slice.** Domain rules, use cases, storage, pages, tests and demo data ship together.<br>**`demo7`.** It shows, on every weekday:<br>• a target that changed, with a further change waiting;<br>• an ended goal on the Ended tab;<br>• a goal on attempt 2 with its earlier attempt;<br>• a filled Goal history.<br>Its figures span both settings versions and both attempts (FR-031, SC-007).<br>**Infrastructure.** Nothing beyond one startup task. The daily refresh re-creates it. |
| Decision Discipline | met | **Recorded decisions.** SPEC §11.7's second part is D-1. D-2 and its two consequences are in ADR 0025 and D-3 is in ADR 0026, all with their dates in `## Clarifications`.<br>**This plan**, against the constitution's three tests (spans features, hard to reverse, rejects real alternatives):<br>• The attempt model and its storage (R-01, R-05) meet all three. Slice 9 reads goals through it. Once a second attempt exists, the migration's `Down` is no longer valid (data-model.md). R-01 also rejects two real modelling alternatives. So it becomes **ADR 0027, "Attempts and per-attempt settings versions"**, written in Work order step 1, before any code. It records R-01's decision and alternatives, R-05's key and foreign keys, and the one-way migration.<br>• The other choices (R-02 to R-04, R-06 to R-13) apply ADR 0025 and ADR 0026 inside this slice. Each is recorded in research.md, as slices 4 to 7 recorded theirs, and none needs an ADR of its own. |

## Project Structure

### Documentation (this feature)

```text
specs/009-goal-changes/
├── spec.md              # /speckit-specify, /speckit-clarify, critic revisions
├── acceptance.md        # /speckit-ac-translate (locked)
├── ac/                  # locks and AC run records
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── application-api.md
│   └── host.md
└── tasks.md             # /speckit-tasks (not created here)
```

### Source Code (repository root)

```text
src/
├── HT.Domain/
│   ├── Goals/                          # Outcome, EndingNote, GoalEnding, GoalAttemptVersions (new);
│   │                                   #   Goal (+ constructor, Attempts, IsEnded, VersionsOf);
│   │                                   #   GoalAttempt (+ Ending); GoalTarget.HasSameScheduleKind;
│   │                                   #   GoalTimeline (attempt-aware, calendar across kinds; LastDate, AttemptOf,
│   │                                   #   VersionInEffect, LatestAttemptHasBegun, SettingsChangeOf, EndingOf,
│   │                                   #   NewAttemptEarliestStart, EntriesThrough overload); GoalSettingsChange,
│   │                                   #   GoalEndingEffect (new); EvaluationCalendar (doc); GoalVersionChange
│   │                                   #   (latest attempt); GoalText.Name overload, AttemptName;
│   │                                   #   GoalDraft.CheckTarget, FieldOf; GoalProblem.StartDateNotAfterEnding
│   ├── Reports/TrendBuilder.cs         # entries of another measurement by their result only
│   └── Records/RecordLimits.cs         # MaxNoteLength shared with EndingNote
├── HT.Application/
│   ├── Goals/                          # GoalChangeService, inputs, results, outcomes, GoalChangeTexts;
│   │                                   #   IGoalStore + 4 members, OwnerGoal, write records, GoalChangeWriteOutcome;
│   │                                   #   GoalDetails + later settings (GoalSettingsView), ending fields,
│   │                                   #   ChangesApplyFrom, AttemptHasBegun; GoalListItem + IsEnded, EndingDate,
│   │                                   #   Outcome; GoalListItems; GoalService (attempt-aware reads); GoalCalendarView (doc)
│   ├── Habits/CatalogService.cs        # + constructor with TimeProvider; goal names at one time read
│   ├── Records/                        # HistoryEntry + attempt fields, Settings; EntrySettings (new);
│   │                                   #   GoalHistory.SettingsOf; RecordPreview, RecordingText (EntrySettings
│   │                                   #   overloads); EntryViews; RecordingService (history range, recordable,
│   │                                   #   bounded recent notes, no throw outside every attempt); IRecordStore +
│   │                                   #   ReadRecentNoteKeysAsync overload; ExemptionStoreWrite.GoalEnded
│   ├── Exemptions/                     # PauseService ("This goal has ended."); ExcusalService (the date's attempt)
│   ├── Reports/                        # AttemptService, AttemptView, AttemptDetails, EndedAttemptView, results;
│   │                                   #   GoalEvaluation attempt overload + EarlierPending; ReportService (overview,
│   │                                   #   dashboard); TodayEntry + GoalEnded; ReportText + texts
│   ├── Accounts/ProfileService.cs      # unchanged code; ChangeOf now attempt-aware
│   ├── Operator/                       # DemoGoalChangeService, DemoGoalChanges, DemoGoalChangeGoal, DemoAttempt,
│   │                                   #   IDemoGoalChangeStore, DemoGoalChangeLoadingResult, DemoGoalChangeAccountResult
│   └── ApplicationServiceCollectionExtensions.cs   # + GoalChangeService, AttemptService, DemoGoalChangeService
├── HT.Persistence/
│   ├── Goals/                          # GoalRows (+ ending columns, AttemptNumber); GoalMapping (attempts, endings);
│   │                                   #   GoalStore (+ ReadOwnerGoalAsync, TryChangeSettingsAsync, TryEndAsync,
│   │                                   #   TryStartAttemptAsync; latest-attempt time zone writes; catalog counts)
│   ├── Records/RecordStore.cs          # AddPauseAsync re-check; ReadRecentNoteKeysAsync overload
│   ├── Habits/DemoAccountWriter.cs     # + attempts, per-attempt versions, endings
│   ├── Habits/DemoGoalChangeStore.cs   # IDemoGoalChangeStore over DemoAccountWriter
│   ├── HtDbContext.cs                  # version key and foreign key, ending columns
│   ├── Migrations/                     # + <timestamp>_GoalChanges
│   └── PersistenceServiceCollectionExtensions.cs   # + IDemoGoalChangeStore
└── HT.Ui/
    ├── Operator/StartupTasks.cs        # + Demo goal-change loading step and its lines
    └── Components/
        ├── Goals/                      # EndGoalDialog, EarlierAttemptsCard (new); NewGoalForm (edit, attempt modes);
        │                               #   GoalSettingsCard ("From …", attempt start row)
        ├── Records/                    # GoalDetailBody (head actions, Notice, attempt line, cards); HistoryCard
        │                               #   (attempt view, row settings); RecordDialog, NoteDialog
        │                               #   (row settings)
        ├── Shared/                     # GoalItem (ended form); CommitmentRow (Tag, row settings)
        ├── Reports/                    # GoalsList (Ended tab); TodayPanel (Tags, counts)
        └── Pages/                      # EditGoalSettings, NewAttempt, AttemptDetail (new); Goals (?tab=ended),
                                        #   Profile (?tab=history), Home

tools/HT.SmokeCheck/                    # check 9 demo goal changes (demo7); SmokeCheckCoverage entries

tests/
├── HT.UnitTests/                       # Domain/Goals, Domain/Reports, Application/Goals, Application/Records,
│                                       #   Application/Reports, Application/Exemptions, Application/Operator;
│                                       #   Fakes/ (FakeGoalStore + members, FakeRecordStore overload,
│                                       #   FakeDemoGoalChangeStore)
├── HT.ArchitectureTests/               # R-12 rules
├── HT.IntegrationTests/                # Goals/ (writes, migration, counts), Records/ (pause re-check), Operator/,
│                                       #   log capture, SC-007 timing
├── HT.UiTests/                         # components, pages, startup and smoke check tests
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
specs/003-demo-hosting/contracts/smoke-check.md   # coverage table rows, check 9
docs/operations/hosted-demo.md, docs/roadmap.md, README.md   # demo7, status
```

**Structure Decision**: the four production projects stay, and no project is added.

- Goal changes belong to the goal: the Domain rules are in `HT.Domain.Goals`, and the writes in
  `HT.Application.Goals` through `IGoalStore`, on the goal store's existing account lock and revision (R-05).
- The attempt reads need the record store and the shared evaluation, so they live in `HT.Application.Reports`
  (R-06).
- Demo goal-change loading sits with the other operator capabilities. Its port is public for the AC reference
  export, fenced by architecture rules (ADR 0010, ADR 0013), and implemented internally in `HT.Persistence`.

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008). Make the changes in the spec's "Design changes before build" list:
   - `PageGoalDetail` (slice-8 state);
   - the Edit settings page, the Start new attempt page and the attempt view page;
   - `EndGoalDialog` (texts, no outcome chosen, refusal placement);
   - `PageGoals`, `GoalListItem`;
   - `PageHomeSignedIn`, `CommitmentRow`;
   - `PageProfile` (Goal history tab);
   - `design/pages.md` and `design/README.md`.

   Then copy any changed `design/css` file to `src/HT.Ui/wwwroot/css`. In the same step, write **ADR 0027**,
   "Attempts and per-attempt settings versions" (Constitution Check, Decision Discipline).

   **Roadmap status, after each step** (docs/roadmap.md "Updating this file"):
   - `planned`: set with this plan (the row skipped `acceptance locked` and `features locked`, which this plan's
     change also covers);
   - `tasks ready` after `/speckit-tasks`;
   - `implementing` with the first code commit;
   - `AC tests green` after `/speckit-ac-tests-analyze`;
   - `shipped` in the commit right after the PR opens.
2. **Domain**:
   - `Outcome`, `EndingNote`, `GoalEnding`, `GoalAttemptVersions`;
   - the `Goal` constructor and members, and `GoalAttempt.Ending`;
   - attempt-aware `GoalTimeline` with its new members (`VersionInEffect`, `LatestAttemptHasBegun` among them), the
     calendar across attempts of both kinds, the per-attempt start dates in place of `goal.StartDate`, and
     `GoalVersionChange` on the latest attempt;
   - `GoalTarget.HasSameScheduleKind`, `GoalDraft.CheckTarget`, `GoalProblem.StartDateNotAfterEnding` with its
     `FieldOf` entry;
   - `GoalText.Name(goal, habitName, instant)` and `GoalText.AttemptName`;
   - the boundary and equivalence unit tests (R-12), and the two Domain tests that change on purpose
     (`GoalTests.Only_the_time_zone_differs_between_versions`, `GoalDraftTests`' field data).

   The Domain architecture rules already cover `HT.Domain.Goals`.
3. **Application** and 4. **Persistence**, verified and committed **together**. After step 3 alone,
   `AddHtApplication` registers `DemoGoalChangeService`, whose port has no implementation. That fails
   `ValidateOnBuild` in every integration and host test, as in slices 2 and 4 to 7. `IGoalStore`'s new members
   likewise need `GoalStore` to compile.
   - Step 3:
     - `GoalChangeService`, its inputs, results and `GoalChangeTexts`;
     - `AttemptService` and its views;
     - the `GoalEvaluation` attempt overload, `EarlierPending` and `Recordable`;
     - `EntrySettings` on every `HistoryEntry`, and the `RecordPreview` and `RecordingText` overloads on it;
     - the changes to `EntryViews`, `RecordingService` (no throw outside every attempt, the bounded recent notes),
       `ReportService` (overview and dashboard with earlier Pending entries), `GoalService`, `GoalListItems`,
       `CatalogService` (the `TimeProvider` constructor), `PauseService` and `ExcusalService`;
     - `DemoGoalChanges`, `DemoGoalChangeService` and `IDemoGoalChangeStore`;
     - the fakes' new members and the unit tests;
     - the architecture rules, in the same commit as the services (R-12): the `IGoalStore` rule widened to
       `AttemptService`, which otherwise fails the pre-commit gate; the `IDemoGoalChangeStore` fences in the
       Application and HT.Ui; `DemoGoalChangeService` in the operator-services rule; the new public types in
       `PublicSurfaceTests`; `UseCaseParameterTests`.

     The bUnit page services (`tests/HT.UiTests/Components/Pages/PageServices.cs`) register `GoalChangeService`,
     `AttemptService` and `FakeDemoGoalChangeStore` in the same commit.
   - Step 4:
     - the rows, the mapping, the `HtDbContext` keys and the `GoalChanges` migration;
     - `GoalStore`'s four members, its latest-attempt time zone writes and its catalog counts;
     - `RecordStore`'s pause re-check and the recent-notes overload;
     - `DemoAccountWriter`'s attempts and endings, `DemoGoalChangeStore` (internal, added to the internal demo stores
       rule) and its registration, and the host's `CatalogService` registration with its clock;
     - the integration tests: migration, conditional writes, cuts, scoping, demo goal-change loading, log capture, and
       the re-run SC-007 timing test.
5. **Host**:
   - the "Demo goal-change loading" startup step and its lines (HP-011);
   - `StartupLogTests` and `StartupTasksDemoTests` updated to expect `demo7@example.com` after `demo6@example.com`,
     with the new fault test.

   The smoke check waits for step 7.
6. **Pages**:
   - `NewGoalForm` modes and `GoalSettingsCard`;
   - EndGoalDialog and EarlierAttemptsCard;
   - the goal page (`GoalDetailBody`);
   - Edit settings, Start new attempt and the attempt view;
   - the Goals page's Ended tab and `GoalItem`'s ended form;
   - the Profile's Goal history tab;
   - the Home's `TodayPanel` and `CommitmentRow` Tags;
   - `HistoryCard`, `RecordDialog`, `NoteDialog`, `CommitmentRow` and `GoalItem` on each row's settings
     (`GoalHistory.SettingsOf` and the `RecordingText` overloads), with component tests for AC-020's History and
     AC-080's RecordDialog;
   - SC-001's action counts as host tests: changing the target from the goal page in three actions, ending in three,
     and starting a new attempt from the Ended tab in two (its "Start new attempt" opens the form with the starting
     values, then "Start attempt").

   Each comes with bUnit and host tests. The tests the spec's "Earlier requirements and tests this slice changes"
   names are updated in the step that changes their page: absences of Edit settings, End goal, earlier attempts, the
   Ended tab and the Goal history tab, and `GoalListItem` tests. The record and note dialog tests gain the attempt
   view as an opener.
7. **Smoke check** (HP-012), after the pages:
   - check 9 and the coverage entries;
   - in the same change:
     - `RunnerBehaviourTests.GoodSite()`;
     - `SmokeCheckHostTests`' exact lines and its "(N of N)";
     - `CoverageTests`;
     - the coverage table and check 9 in `specs/003-demo-hosting/contracts/smoke-check.md`.
8. **Docs**:
   - the README status line (the roadmap row follows each step, step 1);
   - `docs/operations/hosted-demo.md` (`demo7`, check 9);
   - `CONTEXT.md`, only if implementation sharpens a term (HP-013 is already done).
9. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which the owner starts after
   `/speckit-implement`.

## Gates before the next step

No gate is open:

- The spec's decisions are all recorded, and the owner confirmed D-2's two consequences.
- `sources` and `features` are locked (2026-11-22).
- The features critic approved the translation with no notes.
- No roadmap decision is due before this slice's `/speckit-tasks`.

## Spec discrepancies

None blocks planning. Five readings are recorded so the next steps apply them the same way:

1. **HP-011, "the deployment's success check also requires its result"**. As in slices 6 and 7, ADR 0015's success
   check is the smoke check. Check 9 needs `demo7`'s goals, so a deployment succeeds only when demo goal-change
   loading created them (R-10).
2. **Slice 7's excusal step 4** ("the goal is period-based", whose contract says every version has the same schedule
   kind) is read per attempt. The schedule kind is that of the attempt that contains the date. A date in no attempt
   gets step 5's "Nothing is scheduled on this date.", which FR-010 gives (R-07). Before this slice every version of
   a goal had one schedule kind, so slice 7's results are unchanged (FR-024).
3. **"No date"** (FR-008, HP-001, HP-014). The spec writes it as "a last date before its start date". A per-week
   attempt that starts mid-week and ends before its first full week is a case the spec doesn't name. Example: it starts
   Wed 18 Nov and is ended Tue 24 Nov, so its last date is Sun 22 Nov. Its last date is not before its start date,
   yet it has no period. The spec's own consequence is "the attempt has no commitment or period". The plan reads "no
   date" as exactly that: the last date is before the attempt's first entry. Such an attempt shows "ended before it
   began", an empty History and "N/A" (R-02). Every case the spec names gives the same result under both readings.
4. **A record kept outside every attempt** (FR-009). The spec says such records are kept and "not shown or counted",
   and that adding one gives "Nothing is scheduled on this date." (FR-010). It doesn't say what changing or deleting
   one gives. The pages never show one, but a stale page could still hold its identifier. The plan gives the same
   text, "Nothing is scheduled on this date.", and changes nothing, so the stored fact stays (R-07).
5. **When an ended attempt "has begun"**, and the attempt line of one that never did (spec "Terms used below",
   HP-001). The spec's terms time "begun" by the attempt's own day. The time of an ending is not stored (FR-026), so
   an ended attempt is judged from its stored dates: it has begun when it has a date (R-02). The rule never calls an
   attempt begun when it wasn't. A waiting attempt in a west zone ended on its start date has no date, so it counts as
   not begun. An attempt that ended with no date counts as not begun even if its start day had begun. That changes
   only which zone `CurrentDate` uses afterwards, and such an attempt has nothing to record. HP-001 gives the
   attempt line two forms, "started" and "starts". The plan adds a third for an ended latest attempt that never
   began, "Attempt n · ended before it began", reusing the Earlier attempts card's text, and leaves out the Settings
   card's start row then. Either of HP-001's forms would announce a start that never happens (AC-035's goal). The
   design page gets this state in Work order step 1.

## Risks

- **The locked AC code of F001 to F008 must keep compiling and passing, and it may not be read.** The plan changes
  only what is compile-safe:
  - The second `Goal` constructor, init-only properties, enum members, overloads, port members and new types.
    `GoalAttempt` gains an init-only `Ending`, which keeps its value equality.
  - `GoalProblem.StartDateNotAfterEnding` is never produced by creation, so `GoalCreateOutcome`'s
    `Enum.Parse<GoalCreateOutcome>(problem.ToString())` never meets it. A unit test pins that creation's problems all
    map.
  - New port members break every implementer of the port. Inside the solution those are the production stores and
    the test doubles that R-12 names (`RacingGoalStore`, `BeforeFirstCreate`, the two `FaultingRecordStore`s, the
    shared `FakeGoalStore` and `FakeRecordStore` that the unit and bUnit tests both register), each updated in the step that adds the members. The AC code composes the real
    stores (ADR 0003), so no implementer there is expected.
  - `HistoryEntry.Settings` is nullable and not `required`, so existing object initializers compile, and
    `GoalHistory.SettingsOf` falls back to the goal-level settings for an entry built without it (R-07).
  - `/speckit-ac-tests-analyze` runs all seven features with acceptance tests: F001, F002, F004, F005, F007, F008 and
    F009 (F003 demo hosting and F006 note editing have none).
- **Meaning drift of existing members.** `Goal.Versions`, `Goal.Attempt`, `VersionOf` and `CurrentDate` keep their
  results for a goal with one attempt. Equivalence tests run the slice 4 to 7 timeline cases through both
  constructors (R-12). Before this slice, four assumptions held for every goal, and this slice breaks each on purpose.
  The places that relied on them are each changed and tested:
  - **Only the time zone differs between versions:**
    - `Goal.Target` (removed);
    - `GoalVersionChange.ApplyTo`;
    - the excuse step 4;
    - `GoalText.Name` (R-08);
    - the goal-level `GoalHistory` read for each entry by `RecordPreview`, every `RecordingText` member that takes a `GoalHistory`,
      `ReportText.ProgressText`, `RecordDialog`, `HistoryCard`, `NoteDialog`, `CommitmentRow`, `GoalItem` and
      `TodayEntry.ScheduleText` (R-07);
    - the trend's kind (R-07).
  - **Every stored record and note belongs to an entry:** `RecordingService.StoredEntryOf` and `NoteEntryOf` (R-07),
    and `ReadRecentNoteKeysAsync`'s unbounded range (R-05).
  - **One schedule kind per goal:** `EvaluationCalendar`, `GoalCalendarView` and `TryEntries` (R-02).
  - **The goal's start date is its first date:** `CurrentDate`, `EntriesThrough`, `NextCommitment`,
    `FirstWeekStart`, `EntryViews.HistoryOf`, `GoalListItems.Of`, `GoalEvaluation.OfAsync` and
    `GoalService.DetailsOf` (R-01).
- **Migration on populated databases.** The hosted demo and developer databases have versions keyed by (goal, date).
  The migration adds `AttemptNumber` with the default 1 before it rebuilds the key and adds the foreign key, so every
  existing row satisfies both. The new foreign key has no delete action, because a second cascade to
  `GoalSettingsVersions` would be refused by SQL Server (R-05). An integration test migrates a slice 7 database with
  demo data, and another deletes a goal with two attempts. `Down` is valid only while no second attempt exists
  (ADR 0027).
- **Concurrency.**
  - A settings change and a time zone change could both add a later version. An ending could race a pause or a
    restart.
  - The new writes take the account lock and check `GoalsRevision`, as creation and time zone changes do. They then
    take the goal lock, which the record and exemption writes take, and re-check their preconditions there (R-05).
  - `AddPauseAsync` re-checks that the goal is not ended.
  - Integration tests run both orders of each pair.
- **F005's, F007's and F008's demo scenarios** reset the database and run their own demo tasks. Demo goal-change
  loading is a separate call that they never make, so their results never name `demo7` (FR-030). Only the host runs
  all five demo tasks (demo loading, demo history, demo report, demo pause and demo goal-change loading).
- **Clock changes.** Last dates and boundaries are dates, so they are unaffected. Grace ends are 48 elapsed hours from
  each entry's own end (slice 5 FR-016). Unit tests cover a restart and an ending in Kyiv around the time zone change
  of US3-AS13 and AC-061 to AC-063.
- **The hosted demo** gains a seventh account with 3 goals and 4 attempts: 74 records, 1 note, 6 versions and 2
  endings, in one transaction, which adds well under a second to the start.

## Complexity Tracking

No exception to the constitution. The table stays empty.

| Exception | Why needed | Simpler alternative rejected because |
|---|---|---|
| — | — | — |
