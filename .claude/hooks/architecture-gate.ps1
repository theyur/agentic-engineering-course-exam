# Claude Code PreToolUse hook: keep Claude's git commits behind the architecture pre-commit hook.
# Input is the JSON tool event supplied by Claude Code on stdin (Bash or PowerShell tool).
# The ArchUnitNET tests run in .githooks/pre-commit; this guard only stops Claude from skipping that
# hook. It is an allowlist: in a command that mentions git or commit, every simple command must be one
# it can fully check (plain `git` with literal arguments, a directory change inside this clone, or a
# command that cannot run git). Anything else is blocked - blocking a harmless command is acceptable,
# missing a skipped hook is not.
$ErrorActionPreference = 'Stop'

function Block([string]$reason) {
    [Console]::Error.WriteLine("BLOCKED: $reason")
    exit 2
}

# Words (quotes kept) and separators; '(' and ')' also split, so unquoted $( ... ) is checked as its own command.
$tokenPattern = '(?:"(?:\\.|[^"\\])*"|''[^'']*''|[^\s"'';&|()])+|[;&|()\r\n]+'
$separatorPattern = '^[;&|()\r\n]+$'
$directoryCommands = @('cd', 'chdir', 'pushd', 'popd', 'sl', 'Set-Location', 'Push-Location', 'Pop-Location')
# Shells and evaluators can run git from a string, a heredoc or a script file: never allowed next to git.
$shellCommands = @('sh', 'bash', 'zsh', 'dash', 'ksh', 'fish', 'eval', 'exec', 'source', '.', 'script',
    'pwsh', 'powershell', 'cmd', 'iex', 'Invoke-Expression')
# Commands that can run git as another process: blocked when the command mentions git or commit.
$indirectCommands = @('xargs', 'env', 'command', 'builtin', 'nohup', 'time', 'timeout', 'nice', 'sudo', 'watch',
    'parallel', 'find', 'start', 'Start-Process', 'Invoke-Command', 'alias', 'function', 'Set-Alias', 'New-Alias',
    'sal', 'nal', 'python', 'python3', 'py', 'node', 'perl', 'ruby')
$gitGlobalFlags = @('--no-pager', '-P', '-p', '--paginate', '--no-optional-locks', '--literal-pathspecs')
# git commit options whose value is the next argument (S and u take only attached values).
$commitShortOptionsWithValue = @('m', 'F', 'C', 'c', 't')
$commitLongOptionsWithValue = @('message', 'file', 'reuse-message', 'reedit-message', 'fixup', 'squash',
    'author', 'date', 'template', 'cleanup', 'trailer', 'pathspec-from-file')

# One argument as the shell passes it: plain word, or a single fully quoted string. $null for anything
# else (mixed quoting, unbalanced). Expands = the shell may substitute something into it.
function Get-Word([string]$token) {
    if ($token -match '^''([^'']*)''$') { return [pscustomobject]@{ Value = $Matches[1]; Expands = $false } }
    if ($token -match '^"((?:\\.|[^"\\])*)"$') {
        $inner = $Matches[1]
        return [pscustomobject]@{ Value = $inner; Expands = $inner -match '[$`]' }
    }
    $plain = if ($script:tool -eq 'PowerShell') { '^[A-Za-z0-9_./\\:@%+=,~-]+$' } else { '^[A-Za-z0-9_./:@%+=,~-]+$' }
    if ($token -cmatch $plain -and -not $token.StartsWith('@')) { return [pscustomobject]@{ Value = $token; Expands = $false } }
    return $null
}

function Test-Path-Argument([string]$word, [string]$token) {
    $argument = Get-Word $token
    if ($null -eq $argument -or $argument.Expands -or $argument.Value -match '^~|^$') {
        Block "cannot check where '$word $token' leads; use an absolute path inside this clone."
    }
    $path = $argument.Value
    if ($IsWindows -and $path -match '^/([a-zA-Z])(/.*)?$') { $path = "$($Matches[1]):$($Matches[2])" }
    if ([System.IO.Path]::IsPathRooted($path)) {
        $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
        if (-not ($script:cloneRoots | Where-Object { $full.StartsWith($_, [StringComparison]::OrdinalIgnoreCase) })) {
            Block "'$word $token' leaves this clone, whose architecture hook setup is the only one checked."
        }
    }
    elseif ($path -split '[\\/]' -contains '..') {
        Block "'$word $token' may leave this clone; use an absolute path inside it."
    }
}

function Test-CommitArguments([string[]]$arguments) {
    $expectValue = $false
    foreach ($token in $arguments) {
        if ($expectValue) {
            # The value of -m / --author ...: must stay one argument (an unquoted $X could split into flags).
            $expectValue = $false
            $value = Get-Word $token
            if ($null -eq $value -or ($value.Expands -and $token -notmatch '^"')) { Block "commit option value $token must be a single plain or quoted word." }
            continue
        }
        $attached = $false
        $word = Get-Word $token
        if ($null -eq $word) {
            # -m"msg" / --message="msg": plain option name, quoted value.
            if ($token -notmatch '^(?<flag>-[A-Za-z]+|--[A-Za-z-]+=)(?:''[^'']*''|"(?:\\.|[^"\\])*")$') { Block "cannot check commit argument $token; write flags as plain words." }
            $flag = $Matches['flag']; $attached = $true
        }
        elseif ($word.Expands) { Block "commit argument $token could expand into a flag; write it out." }
        else { $flag = $word.Value }

        if ($flag -ceq '--') { break }
        if ($flag.StartsWith('--')) {
            $name = ($flag.Substring(2) -split '=', 2)[0]
            # --no-verify, or an abbreviation git accepts (--no-veri...).
            if ($name.Length -ge 7 -and 'no-verify'.StartsWith($name)) { Block '--no-verify skips the architecture pre-commit hook. Fix the failing architecture test instead.' }
            $expectValue = -not $attached -and $flag -notmatch '=' -and $name -in $commitLongOptionsWithValue
            continue
        }
        if ($flag -cmatch '^-.') {
            # Short flag cluster, read like git: 'n' is --no-verify; a value-taking letter ends it.
            for ($k = 1; $k -lt $flag.Length; $k++) {
                $letter = [string]$flag[$k]
                if ($letter -ceq 'n') { Block '-n (git commit -n = --no-verify) skips the architecture pre-commit hook. Spell out the flags you need.' }
                if ($letter -cin $commitShortOptionsWithValue) { $expectValue = -not $attached -and $k -eq $flag.Length - 1; break }
                if ($letter -cin 'S', 'u') { break }
            }
        }
    }
}

function Test-Git([string[]]$words) {
    $j = 1
    while ($j -lt $words.Count -and $words[$j].StartsWith('-')) {
        $option = $words[$j]
        if ($option -ceq '-C') { Test-Path-Argument '-C' $words[$j + 1]; $j += 2; continue }
        if ($option -cin $gitGlobalFlags) { $j++; continue }
        Block "git option '$option' is not allowed in a command that commits or mentions commit (config overrides can disable the hook)."
    }
    if ($j -ge $words.Count) { return }
    $subcommand = $words[$j]
    if ($subcommand -cnotmatch '^[a-z][a-z0-9-]*$') { Block "git subcommand $subcommand must be a plain word." }
    if ($subcommand -ne 'commit') {
        $alias = & git -C $script:cwd config --get "alias.$subcommand" 2>$null
        if ($alias) { Block "git alias '$subcommand' cannot be checked for --no-verify; use the full command." }
        return
    }
    $script:commits++
    Test-CommitArguments @($words | Select-Object -Skip ($j + 1))
}

function Test-Command([string]$command) {
    # Heredoc / here-string bodies are message data. Unquoted delimiters still expand $( ) and `...`.
    # A heredoc fed to a shell or interpreter is a script, not data.
    $script:expandingBody = $false
    $script:scriptBody = $false
    $command = [regex]::Replace($command, '(?ms)<<-?\s*([''"]?)(\w+)\1([^\n]*)\n(.*?)^\s*\2[ \t]*$', {
            param($m)
            $body = $m.Groups[4].Value
            if ($m.Groups[1].Value -eq '' -and $body -match '[$`]') { $script:expandingBody = $true }
            $line = $m.Result('$`')
            $line = ($line.Substring($line.LastIndexOf("`n") + 1) -split '[;&|()]')[-1]
            $receiver = if ($line -match '^\s*(?:\w+=\S*\s+)*(\S+)') { (($Matches[1] -replace '[''"]', '') -split '[\\/]')[-1] -replace '\.exe$', '' } else { '' }
            if ($receiver -in ($shellCommands + $indirectCommands) -and ($body -replace '[''"\\`]', '') -match '(?i)git|commit') { $script:scriptBody = $true }
            $m.Groups[3].Value
        })
    $command = [regex]::Replace($command, '(?ms)@([''"])\r?\n(.*?)^\1@', {
            param($m)
            if ($m.Groups[1].Value -eq '"' -and $m.Groups[2].Value -match '[$`]') { $script:expandingBody = $true }
            ''
        })
    if ($script:expandingBody) { Block "a heredoc or here-string that expands `$( ) or backticks can run commands; use a quoted delimiter (<<'EOF') or @'...'@." }
    if ($script:scriptBody) { Block 'a heredoc that feeds git commands to a shell or interpreter cannot be checked; run git directly.' }
    if ($command -match '(?i)(^|[\s;&|(])(export\s+)?PATH=|\$env:PATH\s*=') { Block 'changing PATH next to git could run a different git; run git directly.' }
    if ($script:tool -eq 'PowerShell' -and $command -match '--%') { Block "PowerShell's --% passes arguments unchecked (and expands %VAR%); write them out." }

    if ($command -match '(?i)core\.hooksPath|include\.path|includeIf\.|--config-env|\bGIT_(DIR|WORK_TREE|COMMON_DIR|CONFIG\w*|EXEC_PATH|NAMESPACE|INDEX_FILE|OBJECT_DIRECTORY)\b') {
        Block 'git config or repository overrides (core.hooksPath, include, --config-env, GIT_* environment) can bypass the architecture pre-commit hook.'
    }

    $matched = [regex]::Matches($command, $tokenPattern)
    if (([regex]::Replace($command, $tokenPattern, '')).Trim() -ne '') { Block 'unbalanced quotes in command.' }
    $tokens = @($matched | ForEach-Object { $_.Value })

    # Command substitutions inside double quotes run too: check them as commands of their own.
    foreach ($token in $tokens) {
        $unquotedOrDouble = [regex]::Replace($token, '''[^'']*''', '')
        foreach ($substitution in [regex]::Matches($unquotedOrDouble, '\$\(')) {
            $start = $substitution.Index + 2; $depth = 1; $i = $start
            while ($i -lt $unquotedOrDouble.Length -and $depth -gt 0) {
                if ($unquotedOrDouble[$i] -eq '(') { $depth++ } elseif ($unquotedOrDouble[$i] -eq ')') { $depth-- }
                $i++
            }
            if ($depth -gt 0) { Block 'unbalanced $( in command.' }
            Test-Command $unquotedOrDouble.Substring($start, $i - 1 - $start)
        }
        if ($script:tool -ne 'PowerShell') {
            foreach ($backtick in [regex]::Matches($unquotedOrDouble, '`([^`]*)`')) { Test-Command $backtick.Groups[1].Value }
        }
    }

    $words = @()
    foreach ($token in @($tokens) + ';') {
        if ($token -notmatch $separatorPattern) { $words += $token; continue }
        # PowerShell `git commit (expr)` passes the expression's value as an argument.
        if ($token.StartsWith('(') -and ($words | Where-Object { $_ -notmatch '^[A-Za-z_]\w*=' } | Select-Object -First 1) -ceq 'git') {
            Block 'git arguments computed from an expression cannot be checked; write them out.'
        }
        if ($words.Count) { Test-SimpleCommand $words }
        $words = @()
    }
}

function Test-SimpleCommand([string[]]$words) {
    # Leading VAR=value assignments.
    $i = 0
    while ($i -lt $words.Count -and $words[$i] -match '^[A-Za-z_]\w*=') { $i++ }
    if ($i -ge $words.Count) { return }
    $words = @($words | Select-Object -Skip $i)
    $first = $words[0]

    if ($first -in $directoryCommands) {
        $j = 1
        while ($j -lt $words.Count -and $words[$j] -in '-Path', '-LiteralPath', '-P', '-L') { $j++ }
        if ($first -in 'popd', 'Pop-Location') { return }
        if ($j -ge $words.Count) { Block "'$first' without a path leaves this clone." }
        if ($words[$j] -ne '-') { Test-Path-Argument $first $words[$j] }
        return
    }
    if ($first -ceq 'git') { Test-Git $words; return }

    $firstWord = Get-Word $first
    if ($null -eq $firstWord -or $firstWord.Expands -or $first -match '^["'']') {
        Block "cannot check what '$first' runs in a command that mentions git; write the command name plainly."
    }
    if ($first -in $shellCommands) { Block "'$first' can run git out of sight of this guard; run git directly." }
    $text = ($words -join ' ') -replace '[''"\\`]', ''
    if ($first -in $indirectCommands -and $text -match '(?i)git|commit') {
        Block "'$first' can run git out of sight of this guard; run git directly."
    }
    foreach ($word in $words) {
        $value = $word -replace '[''"]', ''
        if ($value -match '(?i)(^|[\\/])git(\.exe)?$') { Block "git run through '$first' cannot be checked; run git directly." }
    }
}

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command
    $script:tool = [string]$inputJson.tool_name

    # Only commands that mention git or commit (even split by quotes or escapes) are checked.
    if (($command -replace '[''"\\`]', '') -notmatch '(?i)git|commit') {
        exit 0
    }

    # The hook setting lives in the clone's config, shared by all its worktrees.
    $script:cwd = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    $worktreeLines = & git -C $script:cwd worktree list --porcelain 2>$null
    if ($LASTEXITCODE -ne 0) {
        Block "cannot check the git setup of '$($script:cwd)'."
    }
    $script:cloneRoots = @($worktreeLines | Where-Object { $_ -like 'worktree *' } | ForEach-Object {
        [System.IO.Path]::GetFullPath($_.Substring(9)).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    })

    $script:commits = 0
    Test-Command $command

    # The pre-commit hook only runs when this clone points core.hooksPath at it.
    if ($script:commits -gt 0) {
        $hooksPath = & git -C $script:cwd config --get core.hooksPath 2>$null
        if ($hooksPath -ne '.githooks') {
            Block "the architecture pre-commit hook is not enabled (core.hooksPath is '$hooksPath'). Run: git config core.hooksPath .githooks"
        }
    }
}
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
