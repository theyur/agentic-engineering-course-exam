# Claude Code PreToolUse hook: keep Claude's git commits behind the architecture pre-commit hook.
# Input is the JSON tool event supplied by Claude Code on stdin (Bash or PowerShell tool).
# The ArchUnitNET tests run in .githooks/pre-commit, inside the repository being committed; this hook
# only blocks commits that would skip it (--no-verify / -n, config overrides), commits it cannot
# confirm stay in this clone (whose worktrees share core.hooksPath), and commits while it is off.
$ErrorActionPreference = 'Stop'

function Block([string]$reason) {
    [Console]::Error.WriteLine("BLOCKED: $reason")
    exit 2
}

# Words (quotes kept, so quoted text never looks like a flag or keyword) and separators.
$tokenPattern = '(?:"(?:\\.|[^"\\])*"|''[^'']*''|[^\s"'';&|()])+|[;&|()\r\n]+'
$separatorPattern = '^[;&|()\r\n]+$'
$directoryCommands = @('cd', 'chdir', 'pushd', 'sl', 'Set-Location', 'Push-Location')
# git commit options whose value is the next argument (S and u take only attached values).
$commitShortOptionsWithValue = @('m', 'F', 'C', 'c', 't')
$commitLongOptionsWithValue = @('--message', '--file', '--reuse-message', '--reedit-message', '--fixup', '--squash',
    '--author', '--date', '--template', '--cleanup', '--trailer', '--pathspec-from-file')

function Get-Unquoted([string]$token) { $token -replace '["'']', '' }

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid work on unrelated invocations.
    if ($command -notmatch '(?i)\bgit\b[\s\S]*\bcommit\b') {
        exit 0
    }

    # --no-verify, or an abbreviation git accepts (--no-veri...).
    if ($command -match '--no-veri') {
        Block '--no-verify skips the architecture pre-commit hook. Fix the failing architecture test instead.'
    }
    if ($command -match '(?i)core\.hooksPath|GIT_CONFIG|GIT_DIR|GIT_WORK_TREE') {
        Block 'overriding git config or repository (core.hooksPath, GIT_CONFIG_*, GIT_DIR, GIT_WORK_TREE) in a commit command can bypass the architecture pre-commit hook.'
    }

    $tokens = @([regex]::Matches($command, $tokenPattern) | ForEach-Object { $_.Value })
    $commitIndexes = @(for ($i = 0; $i -lt $tokens.Count; $i++) { if ((Get-Unquoted $tokens[$i]) -ceq 'commit') { $i } })
    if ($commitIndexes.Count -eq 0) {
        exit 0
    }

    # Arguments of each commit, up to the next separator, as git receives them (quotes removed).
    # Option values (-m <msg>, --author <a> ...) are skipped so message text is not read as flags.
    foreach ($index in $commitIndexes) {
        $expectValue = $false
        for ($j = $index + 1; $j -lt $tokens.Count -and $tokens[$j] -notmatch $separatorPattern; $j++) {
            if ($expectValue) { $expectValue = $false; continue }
            $argument = Get-Unquoted $tokens[$j]
            # A variable could expand to a flag where git expects an option name.
            $variableFlag = "commit arguments from variables ($($tokens[$j])) cannot be checked for --no-verify; write them out."
            if ($argument.StartsWith('$')) { Block $variableFlag }
            if ($argument -ceq '--') { break }
            if ($argument.StartsWith('--')) {
                if (($argument -split '=', 2)[0].Contains('$')) { Block $variableFlag }
                if ($argument.StartsWith('--no-veri')) {
                    Block '--no-verify skips the architecture pre-commit hook. Fix the failing architecture test instead.'
                }
                $expectValue = $argument -notmatch '=' -and $argument -in $commitLongOptionsWithValue
                continue
            }
            if ($argument -cmatch '^-[a-zA-Z]') {
                # Short flag cluster, read like git: 'n' is --no-verify; a value-taking letter ends it.
                for ($k = 1; $k -lt $argument.Length; $k++) {
                    $letter = [string]$argument[$k]
                    if ($letter -eq '$') { Block $variableFlag }
                    if ($letter -ceq 'n') {
                        Block "-n (git commit -n = --no-verify) skips the architecture pre-commit hook. Spell out the flags you need."
                    }
                    if ($letter -cin $commitShortOptionsWithValue) { $expectValue = $k -eq $argument.Length - 1; break }
                    if ($letter -cin 'S', 'u') { break }
                }
            }
        }
    }

    # The hook setting lives in the clone's config, shared by all its worktrees.
    $cwd = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    $worktreeLines = & git -C $cwd worktree list --porcelain 2>$null
    if ($LASTEXITCODE -ne 0) {
        Block "cannot check the git setup of '$cwd'."
    }
    $cloneRoots = @($worktreeLines | Where-Object { $_ -like 'worktree *' } | ForEach-Object {
        [System.IO.Path]::GetFullPath($_.Substring(9)).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    })

    # Every directory a commit command could move to must stay inside this clone.
    for ($i = 0; $i -lt $tokens.Count; $i++) {
        $word = Get-Unquoted $tokens[$i]
        $argument = $null
        if ($word -in $directoryCommands) {
            $j = $i + 1
            while ($j -lt $tokens.Count -and (Get-Unquoted $tokens[$j]) -in '-Path', '-LiteralPath', '-P', '-L') { $j++ }
            $argument = if ($j -lt $tokens.Count -and $tokens[$j] -notmatch $separatorPattern) { $tokens[$j] } else { '~' }
            if ((Get-Unquoted $argument) -eq '-') { continue }
        }
        elseif ($word -ceq '-C' -or $word -in '--git-dir', '--work-tree') {
            $argument = if ($i + 1 -lt $tokens.Count) { $tokens[$i + 1] } else { '' }
        }
        elseif ($word -match '^--(git-dir|work-tree)=(?<path>.*)$') {
            $argument = $Matches['path']
        }
        if ($null -eq $argument) { continue }

        $path = Get-Unquoted $argument
        if ($path -match '[$`~]' -or $path -eq '') {
            Block "cannot check where '$word $argument' leads; use an absolute path inside this clone."
        }
        if ($IsWindows -and $path -match '^/([a-zA-Z])(/.*)?$') { $path = "$($Matches[1]):$($Matches[2])" }
        if ([System.IO.Path]::IsPathRooted($path)) {
            $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
            if (-not ($cloneRoots | Where-Object { $full.StartsWith($_, [StringComparison]::OrdinalIgnoreCase) })) {
                Block "'$word $argument' leaves this clone, whose architecture hook setup is the only one checked."
            }
        }
        elseif ($path -split '[\\/]' -contains '..') {
            Block "'$word $argument' may leave this clone; use an absolute path inside it."
        }
    }

    # The pre-commit hook only runs when this clone points core.hooksPath at it.
    $hooksPath = & git -C $cwd config --get core.hooksPath 2>$null
    if ($hooksPath -ne '.githooks') {
        Block "the architecture pre-commit hook is not enabled (core.hooksPath is '$hooksPath'). Run: git config core.hooksPath .githooks"
    }
}
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
