# Contract: smoke check `tools/HT.SmokeCheck`

Research R-06. Decides whether a deployment or refresh succeeded (HP-017, SC-005) and is the check of SC-006
(workflow mode `check`).

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
| 5 | `demo <email>`, one per `DemoDataService.DemoEmails` | the account signs in with the demo password and `GET /custom-habits` shows the name of every habit of `DemoCustomHabits.For(<email>)` |

- Sign-in: `GET /sign-in`, read the antiforgery token and the `sign-in` handler, `POST /sign-in` with
  `Input.Email` and `Input.Password`, and expect a redirect that is not back to `/sign-in`.
- "Shows every": the check passes when all expected names are on the page, whatever else is there. Data that
  reviewers add does not fail it (SC-005, SC-006); missing demo data does. Names are compared in memory after
  HTML decoding and never printed.
- Timings, reported for SC-001 and never failing a check (one sample is not SC-001's 9 of 10):
  - `first`: the duration of the first request of check 1, from sending to the full response, whether it
    succeeded or not; when it did not succeed and a retry was needed, the line says `first <ms> ms (retried)`
    and the sample does not count for SC-001. Meaningful in `check` runs after the app has had no requests;
    after a deployment's start it measures the cold start instead.
  - `later`: for the first demo account that signs in, after its sign-in, the duration of `GET /custom-habits` together with
    every stylesheet and script the page links (`<link rel="stylesheet" href>`, `<script src>`), fetched one
    after another with that account's cookie, as a signed-in browser without a cache does (slice 1 research
    R-09: no-store, one security-stamp lookup per request). It does not include opening the interactive
    circuit.

## Coverage rule

All demo data is written through the demo data port `IDemoDataStore` (ADR 0013, slice 2 research R-09). Every
data parameter of every method of that port (all parameters but the `CancellationToken`) has an entry in the
smoke check's public coverage map `SmokeCheckCoverage`, keyed by method name, parameter name and parameter
type, with the check that verifies it:

| Port method | Parameter | Check |
|---|---|---|
| `CreateDemoAccountAsync` | `email` (`Email`), `password` (`string`) | 5, the sign-in |
| `CreateDemoAccountAsync` | `customHabits` (`IReadOnlyList<HabitDefinition>`) | 5, the custom habit names |
| `TryCreateSeedCatalogAsync` | `habits` (`IReadOnlyList<SeedCatalogHabit>`) | 3 |

A test (research R-11) reflects over `IDemoDataStore` and fails when a method or a data parameter has no
entry, or an entry no longer matches. A slice that adds demo data adds a method or a parameter to the port, or
changes a parameter's type, so its build of this test fails until it extends the check. A slice that ships
before this slice is implemented is covered by the tasks gate instead (plan, Gates): the test does not exist
yet then.

## Output

One line per check, in order. Every check runs, also after an earlier one failed (a failed `home` does not stop
checks 2–5):

- a check with a list of expected names (`catalog`, `demo <email>`): `PASS <name> (<found> of <expected>)` or
  `FAIL <name>: <reason>`, where `<found>` counts the expected names present on the page, not the cards shown;
- a check without a list (`home`, `https-redirect`, `administrator`): `PASS <name>` or `FAIL <name>: <reason>`;
- check 2 with `--skip-http-redirect`: `SKIP https-redirect`.

Then `TIMING first <ms> ms[ (retried)], later <ms> ms (<n> requests)`, or `TIMING first <ms> ms[ (retried)], later n/a`
when no demo account signed in, then `RESULT PASS` or `RESULT FAIL`. "Every check line" means the lines before
`TIMING`. `<name>` contains a demo email or the word `administrator`, never the administrator email. No
password, habit name, note or page content is printed.

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
- `PageReader`;
- `SmokeCheckCoverage`, whose `Entries` are `CoverageEntry` records (method, parameter, parameter type, check).

The tool's `Program` stays internal.
