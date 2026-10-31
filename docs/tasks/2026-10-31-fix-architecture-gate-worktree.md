# Fix architecture gate to test the committing worktree

## Problem

`.claude/hooks/architecture-gate.ps1` (Claude Code `PreToolUse` hook, added in PR #3) runs the
ArchUnitNET tests before Claude commits. It found the test project via `$PSScriptRoot/../..`, and
`settings.json` loads it from `$CLAUDE_PROJECT_DIR`, so in a git-worktree session it built and
tested the **main checkout** instead of the tree being committed. During PR #5 this blocked a
clean worktree commit (main checkout still had the violation demo) and would equally have let a
violating worktree commit through.

Also:
- Its commit regex `git(?:\.exe)?\s+commit` misses git global options before the subcommand
  (`git -C <dir> commit`), and the `if: "Bash(git commit *)"` prefilter in `settings.json` never
  invokes the hook for that form — the gate is silently skipped.
- A leftover `TEMP` marker appended to `$env:TEMP/architecture-gate.log` on every invocation.

## Decision

Resolve the repositories from the command itself: start at the hook input's `cwd` and walk the
command's segments in order, tracking `cd` / `pushd` / `popd` (and PowerShell equivalents) and
subshells, skipping heredoc / here-string bodies; for **every** `git … commit`, apply its
`-C <dir>` options and test the `git rev-parse --show-toplevel` of that directory. Fail closed
(block) on anything unresolvable — missing directory, non-repo, `popd` with an empty stack,
`--git-dir` / `--work-tree`; skip (allow) only a resolved repository without the architecture test
project, i.e. another repo.

Review (PR #6): attached `-C<path>` was reported as bypassing the gate; git 2.56 rejects it
(`unknown option: -C.`, exit 129), so the commit fails before the gate matters. `popd` tracking and
multiple commit targets were real gaps and are fixed, as is `--git-dir <path>` / `--work-tree
<path>` (separate-argument form previously skipped the gate).

Hook registration (`settings.json`): a per-hook `if` holds exactly one permission rule and does
not normalize git global options, so the Bash prefilter widens to `Bash(git *)` (the script drops
non-commit git commands). A `PowerShell` matcher is added without `if` (subcommand splitting is
only documented for Bash); the script exits in ~0.25 s for non-commit calls.

## Done when

- Commit from a worktree with a Blazor → Persistence dependency is blocked, while the main
  checkout is clean.
- Commit from a clean worktree passes regardless of the main checkout's state.
- `cd <dir> && git commit` and `git -C <dir> commit` test `<dir>`'s repository.
- Non-commit Bash commands are not gated.
- `docs/dependency-violation-demo.md` reflects the behavior and known limitations.

## Known limitations (not fixed here)

- `PreToolUse` runs once before the whole Bash command, so `git apply x && git commit` is tested
  against the tree before `git apply`. Run such steps as separate commands.
- Directory changes via variables (`cd "$DIR"`, `cd $(…)`) are not evaluated; the commit is
  blocked instead.
- No full shell parser: a separator inside a quoted argument can create pseudo-commands, which
  can only add tests or block.
