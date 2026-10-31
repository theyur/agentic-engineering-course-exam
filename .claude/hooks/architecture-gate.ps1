# Claude Code PreToolUse hook: reject git commit attempts when ArchUnitNET fails.
# Input is the JSON tool event supplied by Claude Code on stdin (Bash or PowerShell tool).
# Tests every repository a commit in the command runs in (e.g. a git worktree), not the one the
# hook was loaded from. Anything that cannot be resolved blocks the commit (fail closed).
$ErrorActionPreference = 'Stop'

# git global options that take the next token as their value.
$gitOptionsWithValue = @('-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path', '--super-prefix', '--list-cmds', '--attr-source')

function Resolve-CommandPath([string]$base, [string]$path) {
    if ($path -eq '~' -or $path.StartsWith('~/')) { $path = $HOME + $path.Substring(1) }
    # Git Bash style /e/dir -> E:/dir
    if ($IsWindows -and $path -match '^/([a-zA-Z])(/.*)?$') { $path = "$($Matches[1]):$($Matches[2])" }
    if (-not [System.IO.Path]::IsPathRooted($path)) { $path = Join-Path $base $path }
    return [System.IO.Path]::GetFullPath($path)
}

# Splits a shell command into words (quotes removed) and operators: '(' and ')' for subshells,
# ';' for any command separator. Quoted text and $( ... ) never produce operators.
function Get-ShellTokens([string]$command, [char]$escape) {
    $tokens = [System.Collections.Generic.List[object]]::new()
    $word = [System.Text.StringBuilder]::new()
    $inWord = $false
    $quote = [char]0
    for ($i = 0; $i -lt $command.Length; $i++) {
        $c = $command[$i]
        if ($quote -ne [char]0) {
            if ($c -eq $quote) { $quote = [char]0 }
            elseif ($c -eq $escape -and $quote -eq '"' -and $i + 1 -lt $command.Length) { [void]$word.Append($command[++$i]) }
            else { [void]$word.Append($c) }
            continue
        }
        if ($c -eq "'" -or $c -eq '"') { $quote = $c; $inWord = $true; continue }
        if ($c -eq $escape -and $i + 1 -lt $command.Length) { [void]$word.Append($command[++$i]); $inWord = $true; continue }
        if ($c -eq '$' -and $i + 1 -lt $command.Length -and $command[$i + 1] -eq '(') {
            $depth = 0
            do {
                if ($command[$i] -eq '(') { $depth++ } elseif ($command[$i] -eq ')') { $depth-- }
                [void]$word.Append($command[$i]); $i++
            } while ($i -lt $command.Length -and $depth -gt 0)
            if ($depth -gt 0) { throw 'unbalanced $( in command' }
            $i--; $inWord = $true; continue
        }

        $op = $null
        if ($c -eq '(' -and -not $inWord) { $op = '(' }
        elseif ($c -eq ')') { $op = ')' }
        elseif ($c -in ';', '|', "`n", "`r") { $op = ';' }
        # '&' separates commands, except in redirections such as 2>&1 or &>file.
        elseif ($c -eq '&' -and -not ($inWord -and $word.ToString().EndsWith('>')) -and -not ($i + 1 -lt $command.Length -and $command[$i + 1] -eq '>')) { $op = ';' }

        if ($op -or [char]::IsWhiteSpace($c)) {
            if ($inWord) { $tokens.Add([pscustomobject]@{ Op = $null; Text = $word.ToString() }); [void]$word.Clear(); $inWord = $false }
            if ($op) { $tokens.Add([pscustomobject]@{ Op = $op; Text = $op }) }
            continue
        }
        [void]$word.Append($c); $inWord = $true
    }
    if ($quote -ne [char]0) { throw 'unterminated quote in command' }
    if ($inWord) { $tokens.Add([pscustomobject]@{ Op = $null; Text = $word.ToString() }) }
    $tokens.Add([pscustomobject]@{ Op = ';'; Text = ';' })
    return , $tokens
}

# Walks the command in order, tracking the shell directory, and returns each commit's target
# directory. Throws when a target cannot be determined.
function Get-CommitTargets([string]$command, [string]$startDir, [char]$escape) {
    # Heredoc / here-string bodies are data (e.g. commit messages), not commands.
    $command = [regex]::Replace($command, '(?ms)<<-?\s*([''"]?)(\w+)\1([^\n]*)\n.*?^\s*\2[ \t]*$', '$3')
    $command = [regex]::Replace($command, '(?ms)@([''"])\r?\n.*?^\1@', '')

    $dir = $startDir
    $dirStack = [System.Collections.Generic.Stack[string]]::new()
    # Subshell "( ... )": saves the directory and pushd stack, restored when it closes.
    $subshells = [System.Collections.Generic.Stack[object]]::new()
    $targets = @()
    $words = @()
    foreach ($token in (Get-ShellTokens $command $escape)) {
        if (-not $token.Op) { $words += $token.Text; continue }

        # Skip env assignments (VAR=value).
        $i = 0
        while ($i -lt $words.Count -and $words[$i] -match '^\w+=') { $i++ }
        if ($i -lt $words.Count) {
            $verb = $words[$i]
            $verbArgs = @($words | Select-Object -Skip ($i + 1) | Where-Object { $_ -notmatch '^-(Path|LiteralPath)$' })
            switch -Regex ($verb) {
                '^(cd|chdir|sl|Set-Location)$' { $dir = if ($verbArgs.Count) { Resolve-CommandPath $dir $verbArgs[0] } else { $HOME } }
                '^(pushd|Push-Location)$' { $dirStack.Push($dir); if ($verbArgs.Count) { $dir = Resolve-CommandPath $dir $verbArgs[0] } }
                '^(popd|Pop-Location)$' {
                    if ($dirStack.Count -eq 0) { throw "cannot resolve the directory after '$verb'" }
                    $dir = $dirStack.Pop()
                }
                '(^|[\\/])git(\.exe)?$' {
                    $target = $dir
                    $j = $i + 1
                    while ($j -lt $words.Count -and $words[$j].StartsWith('-')) {
                        $option = ($words[$j] -split '=', 2)[0]
                        if ($option -in '--git-dir', '--work-tree') { throw "'$option' is not supported; use 'git -C <dir>' or cd" }
                        if ($words[$j] -ceq '-C') { $target = Resolve-CommandPath $target $words[$j + 1] }
                        if ($words[$j] -cin $gitOptionsWithValue) { $j++ }
                        $j++
                    }
                    if ($j -lt $words.Count -and $words[$j] -eq 'commit') { $targets += $target }
                }
            }
        }
        $words = @()

        if ($token.Op -eq '(') {
            $subshells.Push([pscustomobject]@{ Dir = $dir; Stack = $dirStack.ToArray() })
        }
        elseif ($token.Op -eq ')') {
            if ($subshells.Count -eq 0) { throw "unbalanced ')' in command" }
            $saved = $subshells.Pop()
            $dir = $saved.Dir
            $entries = [string[]]$saved.Stack
            [array]::Reverse($entries)
            $dirStack = [System.Collections.Generic.Stack[string]]::new($entries)
        }
    }
    if ($subshells.Count -gt 0) { throw "unbalanced '(' in command" }
    return $targets
}

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid expensive parsing and tests on unrelated invocations.
    if ($command -notmatch '(?i)\bgit\b[\s\S]*\bcommit\b') {
        exit 0
    }

    $escape = if ($inputJson.tool_name -eq 'PowerShell') { [char]'`' } else { [char]'\' }
    $startDir = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    $targets = @(Get-CommitTargets $command $startDir $escape | Select-Object -Unique)

    $projectRoots = @()
    foreach ($dir in $targets) {
        if (-not (Test-Path -LiteralPath $dir -PathType Container)) {
            throw "commit directory '$dir' does not exist"
        }
        $root = & git -C $dir rev-parse --show-toplevel 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "could not resolve the git repository for '$dir': $root"
        }
        $projectRoots += [string]$root
    }

    foreach ($projectRoot in @($projectRoots | Select-Object -Unique)) {
        $testProject = Join-Path $projectRoot 'tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj'
        if (-not (Test-Path -LiteralPath $testProject)) {
            [Console]::Error.WriteLine("Architecture gate: no architecture tests in $projectRoot; skipping.")
            continue
        }

        [Console]::Error.WriteLine("Architecture gate: running ArchUnitNET tests in $projectRoot before git commit...")
        Push-Location $projectRoot
        try {
            & dotnet test $testProject --configuration Debug --nologo --verbosity minimal 2>&1 | ForEach-Object { [Console]::Error.WriteLine($_) }
            if ($LASTEXITCODE -ne 0) {
                [Console]::Error.WriteLine('BLOCKED: architectural tests failed. Fix the dependency before committing; do not bypass or disable the gate.')
                exit 2
            }
        }
        finally {
            Pop-Location
        }
    }
}
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
