# 0001 — PR quality gates: SonarQube Cloud + CodeQL, then manual Copilot review

**Status:** Accepted, 2026-11-04

## Context

PRs were analyzed by SonarQube Cloud, and `greptile-gate.yml` (a `workflow_run` workflow) posted
`@greptileai` once Sonar was green. The global `/pr-review` command drove the Greptile fix loop.
We want two independent static analyzers, no Greptile, and a human deciding when an AI reviewer
looks at the PR.

## Decision

- **Static gates, in parallel, on every PR revision and on pushes to `master`:**
  - SonarQube Cloud — `.github/workflows/sonar.yml`; authoritative signal is the
    `SonarCloud Code Analysis` check on the PR head SHA. The workflow job succeeding without that
    check (no `SONAR_TOKEN`) is *skipped*, not green.
  - CodeQL — GitHub code scanning **default setup** (C#, JavaScript/TypeScript, GitHub Actions; no
    build needed). Authoritative signal is the `CodeQL` results check, which fails when the PR adds
    alerts at or above the repository's failure threshold. A green `Analyze (…)` job alone is not.
    Default setup over an advanced workflow: nothing here needs custom queries or a build.
- SonarQube Cloud checks out the PR merge ref `refs/pull/N/merge`; CodeQL default setup analyzes
  `refs/pull/N/head` at the head SHA. GitHub posts both analyzers' check runs on the PR head SHA, so
  gate evaluation keys on the head SHA, and CodeQL alerts are read from `refs/pull/N/head` (merge
  ref as fallback for an advanced setup, tied back to the head via the merge commit's parent).
- The `CodeQL` results check aggregates every language's upload and is updated in place (its
  `completed_at` stays at the first upload), so the gate also requires every `Analyze (…)` job to
  have finished. Verified on demo PR #12: two C# alerts, uploaded after the check's
  `completed_at`, turned the check red.
- **Gate evaluation** lives in `tools/pr-gates/` (PowerShell, offline self-test). Only
  `GREEN` + `GREEN` on the current head counts as ready.
- **Copilot code review is requested manually** by the user once both gates are green. Nothing
  requests it automatically (no ruleset rule, no workflow).
- **Claude Code** runs the loop through the repo skill `/process-pr-feedback`: wait (bounded,
  background process), fetch findings, triage, fix, push; max two unattended rounds per stage;
  never merges, never requests reviews, never weakens a gate.
- **Greptile is retired here:** `greptile-gate.yml` and `.greptile/` removed; the global
  `/pr-review` is turned off for this project (`skillOverrides`), denied as a Skill, and blocked by
  a hook; the Greptile MCP review trigger is denied.

## Consequences

- No unattended automation: the loop advances while a Claude Code session is alive (the background
  wait wakes it). Unattended operation would need a runner with Claude Code (e.g.
  `anthropics/claude-code-action` on `workflow_run` after both gates), an `ANTHROPIC_API_KEY`
  secret, and a privileged job that only reads check results and never executes PR code — deferred.
- The branch ruleset does not require the gate checks; the human merging is the enforcement point.
  Adding `SonarCloud Code Analysis` and `CodeQL` as required checks is a one-step tightening.
- Coverage is collected for .NET only, so repo scripts (`tools/**`, `.claude/hooks/**`) are in
  `sonar.coverage.exclusions`; Sonar still reports issues in them, and their self-tests cover them.
- `master` currently fails the Sonar gate on `new_coverage` (0% vs 80%) because there are no unit
  tests yet; it clears as features land with tests. The threshold stays.
