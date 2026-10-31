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
Claude's own commits behind it. It does not run the tests; it blocks a commit
command when:

- it would skip the pre-commit hook: `--no-verify` (or an abbreviation such as
  `--no-veri`), a short flag cluster containing `n` among a commit's arguments
  (`git commit -n` is `--no-verify`), or commit arguments taken from variables;
- it overrides git config or the repository (`core.hooksPath`, `GIT_CONFIG_*`,
  `GIT_DIR`, `GIT_WORK_TREE`);
- it may commit outside this clone: every `cd` / `pushd` / `Set-Location` /
  `git -C` / `--git-dir` / `--work-tree` argument must be an absolute path inside
  one of the clone's worktrees, or a relative path without `..` (the hook setting
  is per clone, so other clones cannot be vouched for);
- the hook is not enabled (`core.hooksPath` is not `.githooks`).

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
- The Claude Code guard reads command text and errs towards blocking: any
  unquoted `-C <dir>` (also `make -C`) or `cd ..` in a commit command is checked,
  and `git commit -m $(date)` is blocked; use absolute paths and quote arguments.
  Git aliases (`git ci -n`) are not recognized as commits.

The gate is not a security boundary or a replacement for CI. Do not use
`--no-verify` or disable either hook as a supposed fix.

Run the commands locally to confirm behavior; the code was authored remotely
and was not executed against a .NET SDK during repository setup.
