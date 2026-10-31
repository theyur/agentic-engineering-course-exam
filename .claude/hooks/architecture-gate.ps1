# Claude Code PreToolUse hook: keep Claude's git commits behind the architecture pre-commit hook.
# Input is the JSON tool event supplied by Claude Code on stdin (Bash or PowerShell tool).
# The ArchUnitNET tests run in .githooks/pre-commit, inside the repository being committed; this hook
# only blocks commits that would skip it (--no-verify / -n, hooksPath overrides) or while it is off.
$ErrorActionPreference = 'Stop'

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid work on unrelated invocations.
    if ($command -notmatch '(?i)\bgit\b[\s\S]*\bcommit\b') {
        exit 0
    }

    if ($command -match '--no-verify') {
        [Console]::Error.WriteLine('BLOCKED: --no-verify skips the architecture pre-commit hook. Fix the failing architecture test instead.')
        exit 2
    }
    if ($command -match '(?i)core\.hooksPath|GIT_CONFIG') {
        [Console]::Error.WriteLine('BLOCKED: overriding git config (core.hooksPath / GIT_CONFIG_*) in a commit command can disable the architecture pre-commit hook.')
        exit 2
    }

    # git [global options] commit <rest>
    $commitPattern = '(?i)(?:^|[\s(&])git(?:\.exe)?(?:\s+(?:-[Cc]\s+\S+|--(?:git-dir|work-tree|namespace)\s+\S+|--?[\w-]+(?:=\S+)?))*\s+commit\b(?<rest>.*)'
    $commitRests = @(foreach ($segment in [regex]::Split($command, '&&|\|\||[;|\r\n]')) {
        if ($segment -match $commitPattern) { $Matches['rest'] }
    })
    if ($commitRests.Count -eq 0) {
        exit 0
    }
    # -n is commit's short form of --no-verify, alone or in a flag cluster such as -nm.
    if ($commitRests -cmatch '(?<![\w-])-[a-zA-Z]*n[a-zA-Z]*(?![\w-])') {
        [Console]::Error.WriteLine("BLOCKED: a short flag containing 'n' (git commit -n = --no-verify) skips the architecture pre-commit hook. Spell out the flags you need.")
        exit 2
    }

    # The pre-commit hook only runs when this clone points core.hooksPath at it.
    $cwd = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    $hooksPath = & git -C $cwd config --get core.hooksPath 2>$null
    if ($LASTEXITCODE -le 1 -and $hooksPath -ne '.githooks') {
        [Console]::Error.WriteLine("BLOCKED: the architecture pre-commit hook is not enabled (core.hooksPath is '$hooksPath'). Run: git config core.hooksPath .githooks")
        exit 2
    }
}
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
