# Claude Code PreToolUse hook: reject Bash git commit attempts when ArchUnitNET fails.
# Input is the JSON tool event supplied by Claude Code on stdin.
# Tests the repository the commit runs in (e.g. a git worktree), not the one the hook was loaded from.
$ErrorActionPreference = 'Stop'

$quotedOrBare = '"[^"]*"|''[^'']*''|[^\s;&|()]+'
# git, then optional global options (-C <dir>, -c <k=v>, --flag[=v]), then the commit subcommand.
$commitPattern = "(?i)(?:^|[;&|(]\s*|\s)git(?:\.exe)?(?<opts>(?:\s+(?:-[Cc]\s+(?:$quotedOrBare)|--?[\w-]+(?:=\S+)?))*)\s+commit\b"
$cdPattern = "(?i)(?:^|[;&|(]\s*)(?:cd|pushd|sl|Set-Location|Push-Location)\s+(?:-Path\s+|-LiteralPath\s+)?(?<dir>$quotedOrBare)"
$gitDirOptionPattern = "-C\s+(?<dir>$quotedOrBare)"

function Resolve-CommandPath([string]$base, [string]$path) {
    $path = $path.Trim('"', "'")
    if ($path -eq '~' -or $path.StartsWith('~/')) { $path = $HOME + $path.Substring(1) }
    # Git Bash style /e/dir -> E:/dir
    if ($IsWindows -and $path -match '^/([a-zA-Z])(/.*)?$') { $path = "$($Matches[1]):$($Matches[2])" }
    if (-not [System.IO.Path]::IsPathRooted($path)) { $path = Join-Path $base $path }
    return [System.IO.Path]::GetFullPath($path)
}

try {
    $inputJson = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $command = [string]$inputJson.tool_input.command

    # Avoid expensive tests on unrelated Bash invocations.
    $commit = [regex]::Match($command, $commitPattern)
    if (-not $commit.Success) {
        exit 0
    }

    # Directory the commit runs in: hook cwd, then each cd/pushd before the commit, then each git -C.
    $dir = if ($inputJson.cwd) { [string]$inputJson.cwd } else { (Get-Location).Path }
    foreach ($cd in [regex]::Matches($command.Substring(0, $commit.Index), $cdPattern)) {
        $dir = Resolve-CommandPath $dir $cd.Groups['dir'].Value
    }
    foreach ($opt in [regex]::Matches($commit.Groups['opts'].Value, $gitDirOptionPattern)) {
        $dir = Resolve-CommandPath $dir $opt.Groups['dir'].Value
    }

    if (-not (Test-Path -LiteralPath $dir -PathType Container)) {
        [Console]::Error.WriteLine("BLOCKED: architecture gate could not find commit directory '$dir'.")
        exit 2
    }
    $projectRoot = & git -C $dir rev-parse --show-toplevel 2>&1
    if ($LASTEXITCODE -ne 0) {
        [Console]::Error.WriteLine("BLOCKED: architecture gate could not resolve the git repository for '$dir': $projectRoot")
        exit 2
    }

    $testProject = Join-Path $projectRoot 'tests/HT.ArchitectureTests/HT.ArchitectureTests.csproj'
    if (-not (Test-Path -LiteralPath $testProject)) {
        [Console]::Error.WriteLine("Architecture gate: no architecture tests in $projectRoot; skipping.")
        exit 0
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
catch {
    [Console]::Error.WriteLine("BLOCKED: architecture gate could not run: $($_.Exception.Message)")
    exit 2
}

exit 0
