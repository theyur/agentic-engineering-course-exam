# Claude Code architecture-gate demo

This branch intentionally has a **failing architectural test**. Do not merge the
violation into the production branch.

## Rule and deliberate violation

`HT.Ui` is the composition root and may reference `HT.Persistence` for
dependency-injection registration. **Blazor components** under
`HT.Ui.Components` must not depend on concrete Persistence types.

The Home page directly creates `HT.Persistence.DependencyViolationDemo`.
This compiles and renders, but breaks that rule.

## Test manually

From the repository root:

```powershell
dotnet build HabitTracker.slnx
dotnet test tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj -c Debug
```

Expected: **build succeeds; architecture test fails**. The failing test is
intentional, not a setup error.

## Try the Claude Code hook

1. Install .NET 10 SDK and PowerShell 7 (`pwsh`) on the machine running Claude Code.
2. Check out this branch and start a **new Claude Code session**, so it loads
   `.claude/settings.json` (approve the project hook if prompted).
3. Ask Claude to commit the current changes, or have it make a small edit and
   attempt `git commit` through its Bash tool.
4. The `PreToolUse` hook runs architecture tests first. When they fail, exit
   code **2** blocks Claude's Bash tool call, and stderr gives Claude feedback.
5. Fix the page to use an Application abstraction (or remove the offending
   component dependency), then retry the commit: the gate should pass.

The hook does **not** affect manual commits outside Claude Code. It checks the
working tree (not exclusively staged changes), and only recognizes conventional
`git commit` syntax in Bash commands. The demo is not a security boundary or
a replacement for CI. Do not use `--no-verify` or disable the Claude Code hook
as a supposed fix.

Run the commands locally to confirm behavior; the code was authored remotely
and was not executed against a .NET SDK during repository setup.
