# Guard oracle

Tests `.claude/hooks/architecture-gate.ps1`, the Claude Code hook that stops Claude from skipping
the architecture pre-commit hook (see `docs/dependency-violation-demo.md`).

The guard decides from the **text** of a Bash or PowerShell command. Shells rewrite that text in
many ways (quotes, escapes, variables, `sh -c`, `eval`, heredocs …), so instead of reasoning about
what git will receive, the oracle **measures** it:

1. Ask the guard for its verdict on the command.
2. Run the command through the real shell (Git Bash or `pwsh`) inside a throwaway worktree, with a
   fake `git.exe` (`shim/`) first on `PATH`. The fake git records the exact arguments, working
   directory and `GIT_*` environment it receives, and does nothing else.
3. From that ground truth, decide whether a commit would skip the pre-commit hook (`-n`,
   `--no-verify`, hooks-path or include config overrides, `GIT_DIR` …) or commit outside this clone.
4. Compare.

| Result | Meaning |
|---|---|
| `BYPASS` | git would skip the hook, but the guard allowed the command. **Failure.** |
| `EXPECT_FAIL` | the guard's verdict differs from the case's `expect`. **Failure.** |
| `UNKNOWN` | the command mentions commit, the guard allowed it, and no git call was recorded (e.g. git run by absolute path, or a shell error). Investigate. |
| `FALSE_POSITIVE` | the guard blocked a command that turned out harmless. Fine for shapes the guard refuses on purpose; must not happen in `realistic.json`. |

## Run

```powershell
pwsh tools/guard-oracle/oracle.ps1
```

It prints one `SUMMARY` line per case file, details for failures, and `PASSED` (exit 0) or
`FAILED: n case(s)` (exit 1). Options: `-CasesFile <file>` for one file, `-ShowAll` to list every
case, `-GuardPath <file>` to test another version of the guard, `-KeepSandbox` to keep the
throwaway worktree for inspection.

Requirements: Windows with Git for Windows (Git Bash is located from `git` on `PATH`, or pass
`-Bash`), PowerShell 7, .NET 10 SDK (the fake git is built on first run into `shim/bin/`). The
clone's own `core.hooksPath` setting does not matter: the guard is judged as if the pre-commit hook
is enabled (`core.hooksPath=.githooks` is passed to it for the run only).

Safety: test commands run in `.claude/worktrees/guard-oracle-sandbox`, a plain folder (not a
repository; gitignored, removed after the run). They get `GIT_DIR` pointing to a folder that does
not exist and `GIT_CEILING_DIRECTORIES` above the sandbox, so even a real git they reach by absolute
path finds no repository. Before the cases, the run aborts unless the fake git intercepts `git` in
both shells and the real git cannot reach a repository; after every case it aborts if any branch
head of this clone moved. Still, do not put destructive commands in case files.

## Cases

`cases/*.json` are arrays of `{ "cmd": "...", "tool": "Bash" | "PowerShell", "expect": "allow" | "block" }`
(`tool` defaults to Bash; `expect` is optional). Placeholders: `{SANDBOX}`, `{SANDBOX_BASH}`
(Git Bash form `/e/...`), `{CLONE}`, `{CLONE_WIN}` (main checkout root).

- `realistic.json` — commit commands Claude actually writes; all `expect: allow`.
- `rules.json` — one or more cases per rule the guard enforces.
- `adversarial.json` — constructs from review findings and adversarial rounds.

When a review or a new idea finds a way around the guard, add the command here first, see it
reported as `BYPASS`, then fix the guard until the whole run passes again.

The evaluator in `oracle.ps1` is itself a model of git's option parsing (short-flag clusters,
option values, unique long-option prefixes). If a result looks wrong, check git's real behaviour
and fix the model.
