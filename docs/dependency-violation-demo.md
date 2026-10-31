# Claude Code architecture-gate demo

The deliberate violation that demonstrated the gate (Home page creating
`HT.Persistence.DependencyViolationDemo`, introduced in PR #3) has been
removed; the architecture test now passes. To re-run the demo, reintroduce a
direct Persistence dependency in a Blazor component on a throwaway branch.

## Rule

`HT.Ui` is the composition root and may reference `HT.Persistence` for
dependency-injection registration. **Blazor components** under
`HT.Ui.Components` must not depend on concrete Persistence types.

## Test manually

From the repository root:

```powershell
dotnet build HabitTracker.slnx
dotnet test tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj -c Debug
```

Expected: **build succeeds; architecture test passes**. With a violation
reintroduced, the build still succeeds but the test fails.

## How the gate works

The architecture tests run in a **git `pre-commit` hook**, `.githooks/pre-commit`.
Git runs it inside the repository or worktree being committed, so it always tests
that tree, however the commit was started (Claude's Bash or PowerShell tool, a
terminal, an IDE).

Enable it once per clone (the setting is shared by all of the clone's worktrees):

```bash
git config core.hooksPath .githooks
```

A Claude Code `PreToolUse` hook (`.claude/hooks/architecture-gate.ps1`) keeps
Claude's own commits behind it. It does not run the tests; it only stops Claude
from skipping the hook. It is an **allowlist**: in any Bash or PowerShell command
that mentions git or commit, every simple command must be one it can fully check,
and anything else is blocked with a message saying how to rephrase:

- `git` written plainly, with literal arguments. Allowed git options before the
  subcommand: `-C <dir>`, `--no-pager`, `-P`/`-p`; `-c`, `--config-env`,
  `--git-dir`, `--work-tree` and git aliases are blocked. A commit's arguments are
  read like git reads them: `--no-verify` (or `--no-veri…`), `-n` in a short flag
  cluster, or an argument that could expand into a flag (variables, globs,
  escapes, mixed quoting, PowerShell expressions) is blocked.
- `cd` / `pushd` / `Set-Location` / `git -C` paths must stay inside this clone
  (`core.hooksPath` is per clone): an absolute path in one of its worktrees, or a
  relative path without `..`.
- Shells and evaluators (`sh`, `bash`, `eval`, `source`, `pwsh`, `iex`, …) are not
  allowed next to git; `xargs`, `env`, `find`, `Start-Process` and the like are
  blocked when they could run git. Heredoc and `@'…'@` bodies are message data,
  unless they feed a shell, or expand `$( )` / backticks.
- `core.hooksPath`, `include.path`, `GIT_*` environment overrides and `PATH`
  changes are blocked, and so is any commit while the hook is not enabled.

The guard was verified with a differential oracle: each test command was run
through the real shell against a fake `git.exe` that records the exact arguments,
working directory and environment git would receive, and that ground truth was
compared with the guard's verdict (215 commands, no bypass found).

## Try it

1. Install .NET 10 SDK and PowerShell 7 (`pwsh`), and enable the hook as above.
2. Start a **new Claude Code session**, so it loads `.claude/settings.json`
   (approve the project hook if prompted).
3. Add a direct Persistence dependency to a Blazor component and ask Claude to
   commit. The pre-commit hook runs the tests and rejects the commit; Claude sees
   the failing rule in the git output.
4. Fix the component to use an Application abstraction, then retry: the commit
   goes through.

Limitations:

- The tests check the working tree, not exclusively staged changes.
- Outside Claude Code, `git commit --no-verify` still skips the hook, and a clone
  without `core.hooksPath` set has no gate. Branches created before the hook was
  added have no `.githooks/pre-commit`.
- The guard errs towards blocking: unusual but harmless shapes (`xargs git`,
  `cd ..`, git by absolute path, `git commit -m $(date)` without quotes, variables
  in PowerShell git commands) are refused with a message. A script file that runs
  git internally, started by a separate command that doesn't mention git, is out
  of its sight.

The gate is not a security boundary or a replacement for CI. Do not use
`--no-verify` or disable either hook as a supposed fix.

Run the commands locally to confirm behavior; the code was authored remotely
and was not executed against a .NET SDK during repository setup.
