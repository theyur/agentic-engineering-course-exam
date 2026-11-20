# Contract: smoke check `tools/HT.SmokeCheck`

Research R-06. Decides whether a deployment or refresh succeeded (HP-017, SC-005) and is the check of SC-006
(workflow mode `check`).

> **Slice 5 (ADR 0018).** [ADR 0018](../../../docs/adr/0018-evaluation-on-read-and-demo-history.md) adds demo
> history loading (`demo4@example.com`) and the daily scheduled refresh. This contract gains check 6
> `demo history demo4@example.com`, the second demo data port `IDemoHistoryStore` in the coverage rule, and a
> `Port` member in `CoverageEntry` (slice 5 HP-008, SC-005, research R-10,
> `specs/005-recording-and-evaluation/contracts/host.md` "Smoke check"). Checks 1 to 5 are unchanged.

> **Slice 6 (ADR 0021).** [ADR 0021](../../../docs/adr/0021-demo-report-loading.md) adds demo report loading
> (`demo5@example.com`). This contract gains three texts in check 6 (two stat labels of "Reading"'s page and the
> Home's "Due now" tile label), a check 7 `demo report demo5@example.com`, the third demo data port
> `IDemoReportStore` in the coverage rule, and the `PageReader` readers `ReadStatLabels`, `ReadTileLabels` and
> `ReadStatusChips` (slice 6 HP-008, research R-11, `specs/007-personal-reports/contracts/host.md` "Smoke check").
> Checks 1 to 5 are unchanged.

## Invocation

```text
dotnet run --project tools/HT.SmokeCheck -- <base-url> [--skip-http-redirect] [--warm-up-seconds <n>]
```

- `<base-url>`: the published `https://` address. Required.
- `--skip-http-redirect`: skip check 2 (local run without an HTTP front end).
- `--warm-up-seconds`: how long check 1 retries; default 300.
- Environment variables, required and non-empty: `HT_SMOKE_DEMO_PASSWORD`, `HT_SMOKE_ADMIN_EMAIL`,
  `HT_SMOKE_ADMIN_PASSWORD`. Secret values are never accepted as arguments. The workflow fails at its step 3
  before it calls the tool when a value is missing, so exit code 2 means a broken invocation, not missing
  configuration.
- Build the tool from the commit that runs on the hosted app (`Deployment__Commit`): its expected values come
  from that commit's `HT.Application`. The workflow does this in every mode.

## Checks, in order

Each check that signs in uses a new client with an empty cookie container. The checks class `SmokeCheckRunner`
takes a client factory (`Func<HttpClient>`), not one client: the tool's factory creates an `HttpClient` with its
own `CookieContainer`; the in-process host test passes the factory's `CreateBrowser`, which returns a new client
with its own cookies on every call.

| # | Name | Passes when |
|---|---|---|
| 1 | `home` | `GET /` returns 200 and the visitor Home within the warm-up time |
| 2 | `https-redirect` | `GET http://<host>/` returns 301, 302, 307 or 308 to `https://<host>/` |
| 3 | `catalog` | `GET /catalog` shows the name of every Published habit of `SeedCatalog.Habits` |
| 4 | `administrator` | the configured administrator signs in and `GET /admin` returns 200 |
| 5 | `demo <email>`, one per `DemoDataService.DemoEmails` | the account signs in with the demo password, `GET /custom-habits` shows the name of every habit of `DemoCustomHabits.For(<email>)`, `GET /goals` shows every name of `DemoGoals.DisplayNames(<email>)` (slice 4 HP-018), and for an account with a name (`DemoAccounts.NameFor(<email>)`: demo1, "Demo One") `GET /profile` has that name as its first `h1` |
| 6 | `demo history demo4@example.com` (slice 5) | `DemoHistoryService.DemoEmail` signs in with the demo password, `GET /goals` shows the 3 names of `DemoHistory.GoalNames`, the History card of "Reading, 20 min daily" contains "Met", "Failed", "Pending", "Due today", "12 min" and "Fell asleep early.", and the History card of "Yoga, 3 times a week" contains "Failed" and "Missed": 11 expected texts. Slice 6 adds 3: the stat labels "Consistency" and "Streak, commitments" among the stat labels of "Reading, 20 min daily"'s page, and "Due now" among the tile labels of `GET /`: 14 expected texts |
| 7 | `demo report demo5@example.com` (slice 6) | `DemoReportService.DemoEmail` signs in with the demo password, `GET /goals` shows the 3 names of `DemoReport.GoalNames`, the page of "Strength training, 2 times a week" has the stat label "Streak, weeks", and `GET /` has "Missed this week" among its tile labels and "Missed" among its status chips: 6 expected texts |

- Sign-in: `GET /sign-in`, read the antiforgery token and the `sign-in` handler, `POST /sign-in` with
  `Input.Email` and `Input.Password`, and expect a redirect that is not back to `/sign-in`.
- "Shows every": the check passes when all expected names are on the page, whatever else is there. Data that
  reviewers add does not fail it (SC-005, SC-006); missing demo data does. Names are compared in memory after
  HTML decoding and never printed.
- Check 6 (slice 5 research R-10):
  - Its expected texts all come from `HT.Application`: the goal names from `DemoHistory.GoalNames`, the state
    words from `RecordingText.State`, "12 min" from `RecordingText.Amount(Measurement.Duration, null,
    DemoHistory.PartialMinutes)` and the note from `DemoHistory.ReadingNote`. None is typed in the tool.
  - The goal page links come from `PageReader.ReadGoalLinks` on the same `GET /goals`: the `a.ht-goalitem__name`
    anchors with their `href`. A goal page is reached through the first anchor with the goal's name, and only
    when its `href` is a path on the same site (`/goals/…`), never an absolute or protocol-relative address.
  - The texts are matched only in the History card: `PageReader.ReadCardText(html, "History")` takes the first
    `section.ht-card` whose `h2.ht-card__title` is "History", up to that section's `</section>`, with tags
    removed, entities decoded and white space runs made one space. Each expected text must be an ordinal
    substring of it, so the Settings card's words cannot satisfy them.
  - On any weekday the texts lie in the History card's first 10 rows, so the check needs no "Show all":
    "Reading" shows Due today (R), Pending (R − 1, R − 2), Met (R − 3) and Failed with 12 minutes and the note
    (R − 6); "Yoga" shows Failed (week 2) and Missed (week 3).
  - A goal page is named by its position n in `DemoHistory.GoalNames` ("Reading" is 1, "Yoga" is 3), never by its
    name or its path, which holds an identifier read from the page.
  - Slice 6 (HP-008, research R-11): "Reading"'s page also has the stat labels "Consistency" and "Streak,
    commitments" (`PageReader.ReadStatLabels`: the text of every `.ht-stat__label`), and the Home has "Due now"
    (`PageReader.ReadTileLabels`: the text of every `.ht-tile__label`). Each must equal a whole label
    (`PageReader.ContainsAll`, ordinal), so a part of a label, the History card or a trend's sub line cannot satisfy
    it. "Streak, commitments" comes from `ReportText.StreakLabel`; "Consistency" and "Due now" are the labels of the
    GoalStats card and the StatTile, which `HT.Application` does not hold, so the tool names them once.
- Check 7 (slice 6 HP-008, research R-11):
  - The goal names come from `DemoReport.GoalNames`, "Streak, weeks" from `ReportText.StreakLabel` and "Missed" from
    `ReportText.State`; "Missed this week" is the StatTile's label, named once in the tool.
  - The page of "Strength training, 2 times a week" (position 2 in `DemoReport.GoalNames`) is reached as check 6
    reaches its goal pages: the first `a.ht-goalitem__name` with the name, whose `href` is a path on the same site.
    "Streak, weeks" must equal one of its stat labels, so the trend's sub line "last 12 weeks" does not count.
  - On the Home, "Missed this week" must equal a tile label and "Missed" a status chip's word
    (`PageReader.ReadStatusChips`: the text of every element with the class `ht-status`). On the Home only the
    Today rows carry status chips; GoalSummary and Recent notes have none.
  - Every expected text is matched whole and ordinally, as in checks 1 to 6.
- Timings, reported for SC-001 and never failing a check (one sample is not SC-001's 9 of 10):
  - `first`: the duration of the first request of check 1, from sending to the full response, whether it
    succeeded or not; when it did not succeed and a retry was needed, the line says `first <ms> ms (retried)`
    and the sample does not count for SC-001. Meaningful in `check` runs after the app has had no requests;
    after a deployment's start it measures the cold start instead.
  - `later`: for the first demo account that signs in, after its sign-in, the duration of `GET /custom-habits` together with
    every stylesheet and script the page links (`<link rel="stylesheet" href>`, `<script src>`), fetched one
    after another with that account's cookie, as a signed-in browser without a cache does (slice 1 research
    R-09: no-store, one security-stamp lookup per request). It does not include opening the interactive
    circuit. Checks 6 and 7 do not measure it.

## Coverage rule

All demo data is written through the demo data ports `IDemoDataStore` (ADR 0013, slice 2 research R-09),
`IDemoHistoryStore` (slice 5, ADR 0018) and `IDemoReportStore` (slice 6, ADR 0021). Every data parameter of every method of each port (all parameters but
the `CancellationToken`) has an entry in the smoke check's public coverage map `SmokeCheckCoverage`, keyed by
port, method name, parameter name and parameter type, with the check that verifies it:

| Port | Method | Parameter | Type | Check |
|---|---|---|---|---|
| `IDemoDataStore` | `CreateDemoAccountAsync` | `email` | `Email` | 5, the sign-in |
| `IDemoDataStore` | `CreateDemoAccountAsync` | `password` | `string` | 5, the sign-in |
| `IDemoDataStore` | `CreateDemoAccountAsync` | `customHabits` | `IReadOnlyList<HabitDefinition>` | 5, the custom habit names |
| `IDemoDataStore` | `CreateDemoAccountAsync` | `profile` | `DemoAccountProfile` | 5, the account name on Profile (slice 4 HP-018) |
| `IDemoDataStore` | `CreateDemoAccountAsync` | `goals` | `IReadOnlyList<DemoGoal>` | 5, the goal names on Goals (slice 4 HP-018) |
| `IDemoDataStore` | `TryCreateSeedCatalogAsync` | `habits` | `IReadOnlyList<SeedCatalogHabit>` | 3 |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `email` | `Email` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `password` | `string` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `profile` | `DemoAccountProfile` | 6 demo history: the sign-in |
| `IDemoHistoryStore` | `CreateDemoHistoryAccountAsync` | `goals` | `IReadOnlyList<DemoHistoryGoal>` | 6 demo history: the goal names, and the states, partial amount and note on the goal pages |
| `IDemoReportStore` | `CreateDemoReportAccountAsync` | `email` | `Email` | 7 demo report: the sign-in |
| `IDemoReportStore` | `CreateDemoReportAccountAsync` | `password` | `string` | 7 demo report: the sign-in |
| `IDemoReportStore` | `CreateDemoReportAccountAsync` | `profile` | `DemoAccountProfile` | 7 demo report: the sign-in |
| `IDemoReportStore` | `CreateDemoReportAccountAsync` | `goals` | `IReadOnlyList<DemoHistoryGoal>` | 7 demo report: the goal names on Goals, the weeks streak label on a goal page, and a Missed result this week on the Home |

That is 14 entries (10 until slice 5). A test (research R-11) reflects over every port and fails when a method or a data parameter has no entry, or an
entry no longer matches a parameter of its port. A slice that adds demo data adds a method or a parameter to a
port, adds a port, or changes a parameter's type, so its build of this test fails until it extends the check (a
new port is also added to the test's list of ports). A slice that ships before this slice is implemented is
covered by the tasks gate instead (plan, Gates): the test does not exist yet then.

## Output

One line per check, in order. Every check runs, also after an earlier one failed (a failed `home` does not stop
checks 2–7):

- a check with a list of expected names (`catalog`, `demo <email>`): `PASS <name> (<found> of <expected>)` or
  `FAIL <name>: <reason>`, where `<found>` counts the expected names present on the page, not the cards shown;
- `demo <email>` (slice 4 HP-018) stays one line per account over its pages. `<expected>` counts every expected
  name of its pages together: the custom habits, the goal display names and, for demo1, the account name.
  `<found>` counts those present on their own page, so a healthy account's line has `<found>` equal to
  `<expected>` (demo1 `5 of 5`, demo2 `4 of 4`, demo3 `5 of 5` today). Every page is still read; a `FAIL` names
  the first part that misses, in this order: `<n> of <m> custom habits shown`, `<n> of <m> goals shown`, or
  `account name not shown` (a page that does not return 200 gives `GET <path> gave status <code>` for its part).
  No name is printed;
- `demo history demo4@example.com` (slice 5), in check 5's form: `PASS demo history demo4@example.com (14 of 14)`
  (`11 of 11` until slice 6) when every expected text is found, otherwise `FAIL demo history demo4@example.com:
  <reason>`. Every page that can be reached is still read; the reason is the first failing part's, in this order:
  - `the sign-in was refused`, as in check 5;
  - `GET /goals gave status <code>`;
  - `<found> of 3 goal names shown`;
  - then for goal page 1 ("Reading"), and after it goal page 3 ("Yoga"): `no link for goal page <n>`,
    `GET goal page <n> gave status <code>`, or `<found> of <expected> texts on goal page <n> shown`; for goal
    page 1 then `<found> of 2 stat labels on goal page 1 shown` (slice 6);
  - `GET / gave status <code>` or `0 of 1 tile labels on the Home shown` (slice 6).

  No goal name, note, amount, goal path or other page content is printed;
- `demo report demo5@example.com` (slice 6), in check 6's form: `PASS demo report demo5@example.com (6 of 6)` when
  every expected text is found, otherwise `FAIL demo report demo5@example.com: <reason>`. Every page that can be
  reached is still read; the reason is the first failing part's, in this order:
  - `the sign-in was refused`;
  - `GET /goals gave status <code>`, or `<found> of 3 goal names shown`;
  - for goal page 2 ("Strength training"): `no link for goal page 2`, `GET goal page 2 gave status <code>`, or
    `0 of 1 stat labels on goal page 2 shown`;
  - `GET / gave status <code>`, `0 of 1 tile labels on the Home shown`, or `0 of 1 status chips on the Home shown`.

  No goal name, note, figure, goal path, label read from a page or other page content is printed;
- a check without a list (`home`, `https-redirect`, `administrator`): `PASS <name>` or `FAIL <name>: <reason>`;
- check 2 with `--skip-http-redirect`: `SKIP https-redirect`;
- a check that signs in and gets no response fails with `no response (<error>)` or `no response in time`.

Then `TIMING first <ms> ms[ (retried)], later <ms> ms (<n> requests)`, or `TIMING first <ms> ms[ (retried)], later n/a`
when no demo account signed in, then `RESULT PASS` or `RESULT FAIL`. "Every check line" means the lines before
`TIMING`. `<name>` contains a demo email or the word `administrator`, never the administrator email. No
password, habit name, goal name, account name, note, recorded amount, goal path or page content is printed.

## Exit codes

`0` all checks passed · `1` at least one check failed · `2` usage error (missing argument or environment
variable).

## Public surface

`Program.cs` only calls `SmokeCheckCommand.RunAsync(args, environment, output)`, which parses the arguments,
reads the three variables from the environment dictionary it is given, runs the checks and returns the exit
code. Its optional fourth parameter `createHandler` (`Func<HttpMessageHandler>?`) gives each new client its
handler, so tests run without a network; by default each client gets a new `HttpClientHandler` with its own
cookies that does not follow redirects.

Public types, so `HT.UiTests` tests the invocation, the checks, the page reading and the coverage without
starting a process:

- `SmokeCheckCommand`: `RunAsync(string[] args, IDictionary environment, TextWriter output,
  Func<HttpMessageHandler>? createHandler = null)` and `Parse(string[] args, IDictionary environment)`, which
  returns a `SmokeCheckParseResult` (options or a usage error that names no value);
- `SmokeCheckOptions`: the base URL, the three values, `SkipHttpRedirect` and `WarmUp`; its text form leaves
  out the passwords and the administrator email;
- `SmokeCheckRunner(Func<HttpClient> createClient, SmokeCheckOptions options, TextWriter output)` with
  `RunAsync()`, which writes the output lines and returns a `SmokeCheckResult` (the check lines, the `TIMING`
  line, the timings and `Passed`);
- `PageReader`, with `ReadGoalLinks(html)`, which returns `GoalLink` records (name, `href` or null), and
  `ReadCardText(html, title)` (slice 5); `ReadStatLabels(html)`, `ReadTileLabels(html)` and `ReadStatusChips(html)`
  (slice 6), each the decoded texts of its leaf elements in document order;
- `SmokeCheckCoverage`, whose `Entries` are `CoverageEntry` records (port type, method, parameter, parameter
  type, check).

The tool's `Program` stays internal.
