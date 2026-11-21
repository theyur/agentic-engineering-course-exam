# 0022 — Mutation testing: Stryker.NET on each PR's changes, report only

**Status:** Accepted, 2026-11-21. Adds a mutation-testing tool, which ARCHITECTURE §6 and §9 do not choose,
and a CI workflow next to the PR gates of ADR 0001, which stay unchanged.

## Context

Line coverage (SonarQube Cloud, ADR 0001) shows which code the tests run, not whether they would notice that
code being wrong. The domain rules (grace period, strict streaks, consistency, time zones) are where that
difference matters, and most tests here are written by agents, which tend to run code without asserting much.
Mutation testing measures it: it makes small changes to the production code (mutants) and checks that some
test fails for each one.

A spike on 2026-11-21 ran Stryker.NET 5.1.0 on `master` at `8344d63` (slice 6 merged), unit tests only, on a
developer machine:

| Run | Time | Mutants tested | Score |
|---|---|---|---|
| `HT.Domain`, whole project | 1 min 56 s | 806 | 86.11% |
| `HT.Application`, whole project | 2 min 15 s | 1,362 | 74.84% |
| `HT.Domain`, PR #40's changes, noise filtered | 1 min 13 s | 374 | 93.32% |
| `HT.Application`, PR #40's changes, noise filtered | 1 min 21 s | 652 | 87.80% |

Findings:

- **It works on .NET 10** with `TreatWarningsAsErrors`. Stryker drops the mutants that don't compile (13% in
  Domain, 23% in Application) by itself. The 2,144 unit tests run in under a second, which keeps a run short.
- **Most undetected mutants were noise.** Of 473 in the whole-project runs, 173 removed a `ThrowIfNull` /
  `ThrowIf…` guard and 87 emptied an exception message; Stryker's `ignore-methods` filters both out. In the
  diff run, 43 of Application's 80 survivors flipped `ConfigureAwait(false)` to `true`, and 13 removed a
  service registration in `ApplicationServiceCollectionExtensions` (wiring the host tests cover).
- **`ignore-methods` also drops mutants around the call.** With `*ConfigureAwait` in the list, Stryker also
  stopped testing 6 real mutants, such as the negated condition of
  `if (await _store.NameKeyExistsAsync(…).ConfigureAwait(false))` (the duplicate-name checks). So
  `ConfigureAwait` is not filtered that way.
- **The remaining survivors point at real gaps**, for example boundary and `&&`→`||` mutants in
  `GoalTimeline` (time zones and the report window).
- **Application's score undercounts.** Many use cases are tested mainly by the integration tests, which the
  spike did not run.
- **`--since` works per file, not per line.** A changed file is mutated in full. A changed test file also brings
  in every mutant its tests cover, so PR #40's run included `Email.cs`, which the PR did not touch. A changed
  non-C# file in the test project (its `.csproj`, or `stryker-config.json`) makes Stryker test every mutant.
- **`--since` finds nothing in a git worktree.** In `.claude/worktrees/…` it filtered out every mutant without
  an error. In a plain clone, also with a detached HEAD as in CI, it worked. Stryker probably opens the main
  checkout's repository, so the changed paths never match the worktree's files.
- **`--since` needs a branch name or a full SHA.** It rejected a short SHA.

## Options

1. **Diff only, report only, own workflow (chosen).** Each PR mutates what it changed; the score is shown, not
   enforced.
2. **Whole projects on every PR.** A complete picture, but every PR pays about 4–8 minutes of runner time
   whatever it changes, and its own survivors are lost among the old ones.
3. **Diff plus a `break` threshold from the start.** It enforces something, but on a small diff the score
   swings: 2 of 3 mutants killed is 67%. A threshold should be set from real PR numbers, not guessed.
4. **A step in `sonar.yml`.** One workflow fewer, but the Sonar gate would wait for mutation testing, and Sonar
   cannot import Stryker's results anyway.
5. **Integration tests as well.** A fairer Application score, but they share one database (ADR 0006) while
   Stryker runs several test processes at once. That needs `--concurrency 1`, the SQL Server container and
   its secret, and much longer runs.

## Decision

- **Tool.** Stryker.NET (`dotnet-stryker`), pinned in the repository's tool manifest `dotnet-tools.json`.
  CI installs only that tool, at the pinned version, instead of `dotnet tool restore`, which fails on the
  .NET 10 SDK's runner for a manifest with several tools (as in `deploy-demo.yml`).
- **Scope.** `HT.Domain` and `HT.Application`, tested by `tests/HT.UnitTests` only. Not `HT.Persistence`
  (mappings and migrations give mostly equivalent mutants), not `HT.Ui` (Stryker cannot mutate `.razor`), not
  `tools/HT.SmokeCheck`.
- **Configuration.** `tests/HT.UnitTests/stryker-config.json`, which Stryker finds when it runs from that
  folder:
  - `ignore-methods` drops mutants inside argument guards (`ThrowIfNull` and the other `ThrowIf…` helpers)
    and inside exception constructors (the message);
  - `mutate` leaves out `*ServiceCollectionExtensions.cs`, the DI wiring;
  - thresholds high 80, low 60, break 0.

  `ConfigureAwait(false)` → `true` mutants are still tested and count in the score, but the job summary only
  counts them instead of listing them, since no test can detect them.
- **Workflow.** `.github/workflows/mutation.yml`, separate from `sonar.yml`:
  - on a pull request that changes Domain, Application, the unit tests or the mutation setup, it runs
    `--since:<the PR's base SHA>`, with full history;
  - a manual run (`workflow_dispatch`) mutates both projects in full;
  - one matrix job per project, in parallel; no secret and no database;
  - the job summary lists every undetected mutant with file, line, mutator and source line
    (`tools/mutation/Write-MutationSummary.ps1`); the HTML and JSON reports are artifacts for 14 days.
- **Report only.** The job fails only when Stryker fails, never on a score. It is not a gate of ADR 0001, and
  `tools/pr-gates` does not read it.
- **Undetected mutants are fixed with tests.** A surviving mutant is fixed by a test that fails for it. A
  `// Stryker disable once <mutator> : <reason>` comment is allowed only for an equivalent mutant, one no test
  can tell apart from the original, and it must give that reason. Production code is not changed only to get rid
  of a mutant. This applies to agents as well: it is the mutation-testing form of "never loosen an assertion".

## Consequences

- GitHub's runner is about 4× slower than the developer machine. Whole-project runs on PR #41 took 7.5–8.5
  minutes for Domain and 8.5–10 minutes for Application, the two in parallel. A diff run is estimated at about
  4–6 minutes per project (the spike's diff runs took about 75 seconds locally). Other PRs don't start the
  workflow. A PR that changes `HT.UnitTests.csproj` or `stryker-config.json` gets a whole-project run.
- The ignored and left-out mutants make the score higher than an unfiltered one: the spike's whole-project
  Application score rose from 74.84% to 88.03% with this configuration. A score is only comparable with one from
  the same configuration.
- Local run, from a plain clone, not a worktree:

  ```bash
  dotnet tool restore
  cd tests/HT.UnitTests
  dotnet stryker --project HT.Domain.csproj --since:master
  ```

  In a worktree, run without `--since` (the whole project) or use a clone.
- `tools/mutation/` is a repository script like `tools/pr-gates/`, so it is in Sonar's
  `sonar.coverage.exclusions` (ADR 0001).
- Later steps, each its own decision:
  - a `break` threshold once a few PRs have shown what a realistic diff score is;
  - then adding the check to `tools/pr-gates` and ADR 0001;
  - `/process-pr-feedback` reading the job summary as a third source of findings;
  - the integration tests with `--concurrency 1`, if Application's score is wanted in full.
- If a later Stryker version changes `--since`, `ignore-methods` or the JSON report, the pinned version in
  `dotnet-tools.json` keeps CI stable until it is updated on purpose.
