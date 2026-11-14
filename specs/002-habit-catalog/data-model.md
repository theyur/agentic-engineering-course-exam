# Data model: Habit catalog

**Feature**: specs/002-habit-catalog · **Date**: 2026-11-14 · **Plan**: [plan.md](plan.md)

The domain types in `HT.Domain`, the input types of `HT.Application`, and the tables of `HT.Persistence`.
Slice 1's account types are unchanged ([../001-accounts-and-access/data-model.md](../001-accounts-and-access/data-model.md)).
Names follow `CONTEXT.md` (constitution I): catalog habit, custom habit, suggested starting goal, category,
intent, catalog habit state, seed catalog.

## Domain (`HT.Domain`, namespace `HT.Domain.Habits`)

### `HabitId`

- Value: a `Guid`. Readonly record struct. Identifies a catalog habit or a custom habit (research R-02).
  Created by the stores; a test may create one for "an unknown identifier".

### `Category`

- Enum, in FR-002's order: `Body`, `Mind`, `Health`, `Money`, `Home`, `Other`. The order of the enum values
  is the filter's order. The label of each is its name.

### `Intent`

- Enum: `Build`, `Reduce`. A catalog or custom habit's intent: the direction it is usually pursued in
  (`CONTEXT.md`).

### `BuildOrReduce`

- Enum: `Build`, `Reduce`. The Build or Reduce of a suggested starting goal, the goal-side concept of SPEC
  §2 "Build / Reduce" that slice 4's goals reuse; NOT a habit's `Intent` (research R-05).

### `HabitIcon`

- Value: one of the 24 Lucide names of FR-003, as a string. `HabitIcon.All` lists them in FR-003's order.
- `HabitIcon.TryParse(string? name, out HabitIcon icon)`: exact, case-sensitive match against the list; any
  other name (including `menu`, `trash-2`, `""`, `null`) fails.

### `HabitName`

- `Value`: the trimmed text, 1 to 60 UTF-16 code units (research R-03).
- `Key`: `Value.ToUpperInvariant()`. Equality of names is ordinal equality of keys.
- `HabitName.Order`: an `IComparer<HabitName>` that compares keys with `string.CompareOrdinal`.
- `Matches(string? search)`: true when the trimmed search is empty or missing, or when `Key` contains the
  trimmed search's `ToUpperInvariant()` form ordinally. Uniqueness, order and search share the one key
  (research R-03).
- Examples: `" ac17 reading "` and `"Ac17 Reading"` have the same key; `"Éclair"` sorts after `"Zest"`
  because `É` (U+00C9) is above `Z`; `"b habit"` sorts after `"A habit"`; `"ſun"` and `"Sun"` have the same
  key (`ſ`, U+017F, upper-cases to `S`).

### `HabitDefinition`

- Fields: `Name` (`HabitName`), `Description` (`string?`, trimmed, at most 500 code units, null when empty
  after trimming), `Category`, `Intent`, `Icon` (`HabitIcon`). Immutable.
- `HabitDefinition.TryCreate(string? name, string? description, Category? category, Intent? intent,
  string? icon, out HabitDefinition definition, out HabitFieldProblem problem)` checks in FR-005's order:

  | Order | Input | Problem |
  |---|---|---|
  | 1 | name empty after trimming | `NameMissing` |
  | 2 | name over 60 after trimming | `NameTooLong` |
  | 3 | description over 500 after trimming | `DescriptionTooLong` |
  | 4 | no category | `CategoryMissing` |
  | 5 | no intent | `IntentMissing` |
  | 6 | no icon, or not in `HabitIcon.All` | `IconMissing` |

  Boundaries tested on both sides: 60/61 and 500/501 code units.

### `SuggestedStartingGoal`

- Fields: `BuildOrReduce`; `Measurement` (`YesNo`, `Count`, `Duration`); `Amount` (`int?`, null for yes/no);
  `CountUnit` (`string?`, trimmed, only for count); `DurationUnit` (`Minutes`, `Hours`; only for duration);
  `Schedule`. Immutable.
- `Schedule`: either `FixedDays` with `Days` (a non-empty `IReadOnlySet<DayOfWeek>`) or `Times` with `Count`
  (1 to 20) and `Period` (`SchedulePeriod.Day`, `SchedulePeriod.Week`).
- `SuggestedStartingGoalFields`: the unchecked values, one nullable value per control of the form —
  `BuildOrReduce?`, `Measurement?`, `Amount` (`int?`), `CountUnit` (`string?`), `DurationUnit?`, `ScheduleKind?`
  (`FixedDays`, `Times`), `Days` (`IReadOnlySet<DayOfWeek>`, may be empty), `Times` (`int?`), `Period`
  (`SchedulePeriod?`). Application's `SuggestedStartingGoalInput` carries the same values plus `IsMarked`.
- `SuggestedStartingGoal.TryCreate(SuggestedStartingGoalFields fields, out SuggestedStartingGoal goal, out
  SuggestedStartingGoalProblem problem)` checks in FR-007's order:

  | Order | Input | Problem |
  |---|---|---|
  | 1 | no Build or Reduce | `BuildOrReduceMissing` |
  | 2 | no measurement | `MeasurementMissing` |
  | 3 | yes/no with an amount, a count unit not empty after trimming, or a duration unit | `YesNoHasAmountOrUnit` |
  | 4 | count or duration, Build, amount missing or outside 1–999 | `BuildAmountOutOfRange` |
  | 4 | count or duration, Reduce, amount missing or outside 0–999 | `ReduceAmountOutOfRange` |
  | 5 | count with a unit empty after trimming | `CountUnitMissing` |
  | 6 | count with a unit over 20 after trimming | `CountUnitTooLong` |
  | 7 | duration with no unit | `DurationUnitMissing` |
  | 8 | no schedule kind | `ScheduleMissing` |
  | 9 | fixed days with no day | `DaysMissing` |
  | 10 | times missing or outside 1–20 (missing: plan, "Spec discrepancies" 5) | `TimesOutOfRange` |
  | 11 | times with no period | `PeriodMissing` |

  Values that do not belong to the measurement or schedule kind are not stored (research R-05).
  Boundaries tested on both sides: Build 0/1 and 999/1000, Reduce -1/0 and 999/1000, unit 20/21, times 0/1
  and 20/21.

### `SuggestedStartingGoals`

- An ordered list of 0 to 5 `SuggestedStartingGoal`, with `MarkedIndex` (`int?`, null exactly when the list
  is empty). `Marked` returns the marked suggested starting goal, the one the card shows.
- `SuggestedStartingGoals.TryCreate(IReadOnlyList<(SuggestedStartingGoalFields Fields, bool IsMarked)>
  items, …)`: first more than 5 items (`TooMany`), then a non-empty list whose number of marks is not
  exactly 1 (`MarkMissing`, for none or several), then each item in order with `SuggestedStartingGoal.TryCreate`;
  the first item problem is returned with its position counted from 1. Boundary tested on both sides: 5
  and 6 items.

### `SuggestedStartingGoalText`

- `Format(SuggestedStartingGoal goal)` returns `(string Text, string Kind)` by FR-017's rules (research
  R-06). Examples it must reproduce:

  | Goal | Text | Kind |
  |---|---|---|
  | Build, duration 15 minutes, fixed Mon–Fri | 15 min, 5 days a week | Duration · fixed days Mon–Fri |
  | Build, count 20 `pages`, 3 times per week | 20 pages, 3 times a week | Count · flexible |
  | Build, yes/no, 1 time per day | once a day | Yes/no · flexible |
  | Build, duration 30 minutes, all seven days | 30 min daily | Duration · fixed days every day |
  | Reduce, duration 2 hours, 1 time per day | at most 2 h a day | Duration · flexible |
  | Reduce, count 1 `sweet`, 1 time per day | at most 1 sweet a day | Count · flexible |
  | Build, yes/no, 1 time per week | once a week | Yes/no · flexible |
  | Build, duration 1 hour, fixed Mon, Wed, Fri | 1 h, 3 days a week | Duration · fixed days Mon, Wed, Fri |
  | Build, count 10 `pages`, fixed Sat | 10 pages, once a week | Count · fixed days Sat |
  | Reduce, yes/no, 1 time per day | at most once a day | Yes/no · flexible |
  | Build, yes/no, fixed Mon, Tue, Wed, Sat | 4 days a week | Yes/no · fixed days Mon–Wed, Sat |
  | Build, yes/no, fixed Sat, Sun | 2 days a week | Yes/no · fixed days Sat, Sun |
  | Reduce, count 0 `sweets`, 1 time per day | at most 0 sweets a day | Count · flexible |

### `CatalogHabitState`

- Enum: `Draft`, `Published`, `Unpublished` (FR-011). Transitions:

  | State | Publish | Unpublish |
  |---|---|---|
  | Draft | → Published | refused: not published, stays Draft |
  | Published | stays Published (success) | → Unpublished |
  | Unpublished | → Published | stays Unpublished (success) |

- `CatalogHabitStates.Publish(state)` and `CatalogHabitStates.Unpublish(state)` return the next state (which
  may equal the current one) or the refusal. The service decides with them and writes through the store's
  compare-and-swap; the store has no rule of its own (research R-08).

### `CatalogHabit`

- Fields: `Id` (`HabitId`), `Definition` (`HabitDefinition`), `SuggestedStartingGoals`, `State`. Immutable.

### `CustomHabit`

- Fields: `Id` (`HabitId`), `Owner` (`AccountId`), `Definition` (`HabitDefinition`). Immutable. No
  suggested starting goals (FR-006).

## Application inputs (`HT.Application.Habits`)

- `HabitInput`: `Name` (`string?`), `Description` (`string?`), `Category` (`Category?`), `Intent`
  (`Intent?`), `Icon` (`string?`). Used for custom habits and, with `SuggestedStartingGoals`, for catalog
  habits.
- `CatalogHabitInput`: the `HabitInput` fields plus `SuggestedStartingGoals`
  (`IReadOnlyList<SuggestedStartingGoalInput>`, may be empty).
- `SuggestedStartingGoalInput` (research R-05): `BuildOrReduce?`, `Measurement?`, `Amount` (`int?`), `CountUnit`
  (`string?`), `DurationUnit?`, `ScheduleKind?` (`FixedDays`, `Times`), `Days` (`IReadOnlySet<DayOfWeek>`,
  may be empty), `Times` (`int?`), `Period` (`SchedulePeriod?`), `IsMarked` (`bool`).
- `CatalogFilter`: `Category` (`Category?`, null = all), `Intent` (`Intent?`, null = both), `Search`
  (`string?`).

Result and view types are in [contracts/application-api.md](contracts/application-api.md).

## Demo data (`HT.Application.Operator`)

- `SeedCatalogHabit`: `Definition`, `SuggestedStartingGoals`, `State`. `SeedCatalog.Habits` is the fixed list
  (research R-10).
- `DemoCustomHabits.For(string demoEmail)`: the two `HabitDefinition`s of FR-031 for that email, no
  description:

  | Demo email | Custom habits (category, intent, icon) |
  |---|---|
  | `demo1@example.com` | Cold shower (Body, Build, `droplet`); No phone in bed (Home, Reduce, `smartphone`) |
  | `demo2@example.com` | Evening stretch (Body, Build, `person-standing`); Plan tomorrow (Mind, Build, `pen-line`) |
  | `demo3@example.com` | Late snacks (Health, Reduce, `candy`); Water the plants (Home, Build, `sprout`) |

## Ports

- `ICatalogHabitStore` (`HT.Application.Habits`): `ListPublishedAsync()`, `ListAllAsync()`,
  `FindAsync(HabitId)`, `FindPublishedAsync(HabitId)`, `CreateAsync(HabitDefinition,
  SuggestedStartingGoals)` (Draft; name taken or created), `UpdateAsync(HabitId, HabitDefinition,
  SuggestedStartingGoals)` (not found, name taken or saved; a unique-index violation is name taken),
  `TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next)` (true when the row had
  `expected` and now has `next`), `NameKeyExistsAsync(string key, HabitId? except)`.
- `ICustomHabitStore` (`HT.Application.Habits`): every member takes the owner `AccountId` first —
  `ListAsync(owner)`, `FindAsync(owner, HabitId)`, `CreateAsync(owner, HabitDefinition)` (name taken or
  created), `UpdateAsync(owner, HabitId, HabitDefinition)` (not found, name taken or saved), `DeleteAsync(owner,
  HabitId)` (deleted or not found), `NameKeyExistsAsync(owner, string key, HabitId? except)`. Create and
  update are the conditional statements of research R-14, which refuse a taken name without a database
  error.
- `IDemoDataStore` (`HT.Application.Operator`): `CreateDemoAccountAsync(Email, string password,
  IReadOnlyList<HabitDefinition> customHabits)`, `TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit>)`
  (research R-09).

All members are `async` and take a `CancellationToken` last. All are public and fenced by architecture rules
(research R-12, ADR 0010).

## Tables (`HT.Persistence`, migration `HabitCatalog`)

### `CatalogHabits`

| Column | Type | Rule |
|---|---|---|
| `Id` | `uniqueidentifier` | primary key |
| `Name` | `nvarchar(60)` | not null; trimmed |
| `NameKey` | `nvarchar(60)` `COLLATE Latin1_General_100_BIN2` | not null; unique index `IX_CatalogHabits_NameKey` |
| `Description` | `nvarchar(500)` | null when none |
| `Category` | `varchar(16)` | not null; enum name |
| `Intent` | `varchar(16)` | not null; enum name |
| `Icon` | `varchar(32)` | not null; a `HabitIcon` name |
| `State` | `varchar(16)` | not null; enum name |

### `CatalogHabitSuggestedStartingGoals` (owned by `CatalogHabits`)

| Column | Type | Rule |
|---|---|---|
| `CatalogHabitId` | `uniqueidentifier` | primary key part 1; foreign key, cascade delete |
| `Position` | `int` | primary key part 2; 1 to 5 in list order |
| `IsMarked` | `bit` | exactly one row per habit when it has any (checked in Domain) |
| `BuildOrReduce` | `varchar(16)` | not null |
| `Measurement` | `varchar(16)` | not null |
| `Amount` | `int` | null for yes/no |
| `CountUnit` | `nvarchar(20)` | only for count |
| `DurationUnit` | `varchar(16)` | only for duration |
| `ScheduleKind` | `varchar(16)` | not null: `FixedDays` or `Times` |
| `Days` | `int` | bit mask, Monday 1 … Sunday 64; only for fixed days |
| `Times` | `int` | only for times |
| `Period` | `varchar(16)` | only for times |

### `CustomHabits`

| Column | Type | Rule |
|---|---|---|
| `Id` | `uniqueidentifier` | primary key |
| `OwnerId` | `uniqueidentifier` | not null; foreign key to `AspNetUsers.Id`, cascade delete |
| `Name` | `nvarchar(60)` | not null |
| `NameKey` | `nvarchar(60)` `COLLATE Latin1_General_100_BIN2` | not null; unique index `IX_CustomHabits_OwnerId_NameKey` on (`OwnerId`, `NameKey`) |
| `Description` | `nvarchar(500)` | null when none |
| `Category`, `Intent`, `Icon` | as in `CatalogHabits` | not null |

No check constraints and no stored text or kind: rules stay in Domain, the text is produced by
`SuggestedStartingGoalText` (ADR 0011). The mapping between rows and domain types lives only in the
Persistence stores; a stored row that fails the Domain checks is a fault (`InvalidOperationException` with no
field value in its message, HP-015).
