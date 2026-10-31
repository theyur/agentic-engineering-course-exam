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

function Get-Tokens([string]$segment) {
    foreach ($m in [regex]::Matches($segment, '"([^"]*)"|''([^'']*)''|(\S+)')) {
        if ($m.Groups[1].Success) { $m.Groups[1].Value } elseif ($m.Groups[2].Success) { $m.Groups[2].Value } else { $m.Groups[3].Value }
    }
}

# Walks the command in order, tracking the shell directory, and returns each commit's target
# directory. Throws when a target cannot be determined.
function Get-CommitTargets([string]$command, [string]$startDir) {
    # Heredoc / here-string bodies are data (e.g. commit messages), not commands.
    $command = [regex]::Replace($command, '(?ms)<<-?\s*([''"]?)(\w+)\1([^\n]*)\n.*?^\s*\2[ \t]*$', '$3')
    $command = [regex]::Replace($command, '(?ms)@([''"])\r?\n.*?^\1@', '')

    $dir = $startDir
    $stack = [System.Collections.Generic.Stack[string]]::new()
    $targets = @()
    foreach ($segment in [regex]::Split($command, '\|\||&&|[;|&\r\n]')) {
        $tokens = @(Get-Tokens $segment)
        if ($tokens.Count -eq 0) { continue }

        # Subshell "( ... )": directory changes inside do not outlive it.
        while ($tokens[0].StartsWith('(')) {
            $stack.Push($dir)
            $tokens[0] = $tokens[0].Substring(1)
            if ($tokens[0] -eq '') { $tokens = @($tokens | Select-Object -Skip 1); if ($tokens.Count -eq 0) { break } }
        }
        $last = if ($tokens.Count) { $tokens[-1] } else { '' }
        $closing = ([regex]::Matches($last, '\)')).Count - ([regex]::Matches($last, '\(')).Count
        if ($closing -gt 0) { $tokens[-1] = $last.Substring(0, $last.Length - $closing); $tokens = @($tokens | Where-Object { $_ -ne '' }) }

        # Skip env assignments (VAR=value) and the PowerShell call operator.
        $i = 0
        while ($i -lt $tokens.Count -and ($tokens[$i] -match '^\w+=' -or $tokens[$i] -eq '&')) { $i++ }
        if ($i -lt $tokens.Count) {
            $verb = $tokens[$i]
            $verbArgs = @($tokens | Select-Object -Skip ($i + 1) | Where-Object { $_ -notmatch '^-(Path|LiteralPath)$' })
            switch -Regex ($verb) {
                '^(cd|chdir|sl|Set-Location)$' { $dir = if ($verbArgs.Count) { Resolve-CommandPath $dir $verbArgs[0] } else { $HOME } }
                '^(pushd|Push-Location)$' { $stack.Push($dir); if ($verbArgs.Count) { $dir = Resolve-CommandPath $dir $verbArgs[0] } }
                '^(popd|Pop-Location)$' {
                    if ($stack.Count -eq 0) { throw "cannot resolve the directory after '$verb'" }
                    $dir = $stack.Pop()
                }
                '(^|[\\/])git(\.exe)?$' {
                    $target = $dir
                    $j = $i + 1
                    while ($j -lt $tokens.Count -and $tokens[$j].StartsWith('-')) {
                        $option = ($tokens[$j] -split '=', 2)[0]
                        if ($option -in '--git-dir', '--work-tree') { throw "'$option' is not supported; use 'git -C <dir>' or cd" }
                        if ($tokens[$j] -ceq '-C') { $target = Resolve-CommandPath $target $tokens[$j + 1] }
                        if ($tokens[$j] -cin $gitOptionsWithValue) { $j++ }
                        $j++
                    }
                    if ($j -lt $tokens.Count -and $tokens[$j] -eq 'commit') { $targets += $target }
                }
            }
        }
        for ($k = 0; $k -lt $closing -and $stack.Count -gt 0; $k++) { $dir = $stack.Pop() }
    }
    return $targets
}

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid expensive parsing and tests on unrelated invocations.
    if ($command -notmatch '(?i)\bgit\b[\s\S]*\bcommit\b') {
        exit 0
    }

    $startDir = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    $targets = @(Get-CommitTargets $command $startDir | Select-Object -Unique)

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
