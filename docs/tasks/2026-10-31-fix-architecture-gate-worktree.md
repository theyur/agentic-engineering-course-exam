# Fix architecture gate to test the committing worktree

## Problem

`.claude/hooks/architecture-gate.ps1` (Claude Code `PreToolUse` hook, added in PR #3) ran the
ArchUnitNET tests before Claude commits. It found the test project via `$PSScriptRoot/../..`, and
`settings.json` loads it from `$CLAUDE_PROJECT_DIR`, so in a git-worktree session it built and
tested the **main checkout** instead of the tree being committed. During PR #5 this blocked a
clean worktree commit (main checkout still had the violation demo) and would equally have let a
violating worktree commit through.

Also: `git -C <dir> commit` bypassed both the `if: "Bash(git commit *)"` prefilter and the
script's regex, commits through the PowerShell tool were not gated, and a leftover `TEMP` marker
logged every invocation.

## First approach: resolve the target from the command (abandoned)

The hook resolved each commit's repository from the command text: hook `cwd`, then `cd` / `pushd`
/ `popd`, subshells and `git -C`. Four review rounds on PR #6 each found another shell construct
where the hook's reading diverged from the shell (popd, chained commits, quoted `;` and `)`,
subshell stack sharing, Bash backslash rules, `$( … )` boundaries, comments). Each was fixable,
but the class of bug is open-ended (`eval`, aliases, backticks, variables …).

## Decision

Run the tests in a **git `pre-commit` hook** (`.githooks/pre-commit`, enabled per clone with
`git config core.hooksPath .githooks`). Git runs it inside the repository or worktree being
committed, so there is nothing to parse, and it covers every commit path (Bash, PowerShell,
terminal, IDE). It blocks when the test project is missing.

The Claude Code hook becomes a guard that only stops Claude from skipping the pre-commit hook.
Its first versions recognized bad shapes (a denylist over command text); each review round found
another shell construct it misread. It is now an **allowlist**: in a command that mentions git or
commit, every simple command must be one it can fully check — plain `git` with literal arguments
(no `-c` / `--config-env` / `--git-dir` / `--work-tree` / aliases; commit arguments read like git
reads them, nothing that could expand into a flag), a directory change that stays inside this
clone, or a command that cannot run git (shells, `eval`, `iex` are never allowed next to git).
Heredoc bodies are message data unless they feed a shell or expand. Anything else is blocked.

Verification: a differential oracle (scratch tooling, not committed) ran each test command through
real Git Bash / pwsh with a fake `git.exe` first on PATH that records the exact argv, cwd and
`GIT_*` environment, and compared that ground truth with the guard's verdict: 215 commands
(realistic commit commands, one case per rule, and three fresh adversarial rounds), no bypass;
realistic commands raise no false positives.

`settings.json`: both the Bash and PowerShell entries run the guard for every command, without an
`if` prefilter — an `if` such as `Bash(git *)` would not fire for `sh -c '…'`. The script exits in
~0.25 s when a command does not mention git. `.gitattributes` pins `.githooks/*` to LF (the repo
uses `core.autocrlf=true`; a CRLF shebang breaks `sh`).

Trade-offs accepted: each clone needs the one-time `core.hooksPath` setting; outside Claude Code
`git commit --no-verify` still skips the hook (CI remains the authoritative check).

## Done when

- A commit in a worktree with a Blazor → Persistence dependency is rejected by the pre-commit
  hook; a clean worktree commits; a missing test project is rejected.
- Claude's commits with `--no-verify` / `-n` / hooksPath overrides, or with the hook disabled,
  are blocked; non-commit commands are not.
- `docs/dependency-violation-demo.md` describes setup, behavior and limitations.
