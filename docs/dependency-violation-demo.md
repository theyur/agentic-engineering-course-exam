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
