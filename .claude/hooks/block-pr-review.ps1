# Claude Code hook: the global /pr-review command (a Greptile review loop) must not run in this repo.
# Wired twice in .claude/settings.json, as a backstop to "skillOverrides": {"pr-review": "off"}:
#   - UserPromptExpansion, matcher "pr-review": a user-typed /pr-review.
#   - PreToolUse, matcher "Skill": Claude invoking it through the Skill tool.
# Blocks only when the invoked name is exactly pr-review; every other prompt, skill or tool passes.
# Use /process-pr-feedback instead (see README "PR review flow").
$ErrorActionPreference = 'Stop'

try { $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json -AsHashtable }
catch { exit 0 }   # unreadable input: not ours to judge; the skillOverrides entry still applies

$names = @()
switch ($hookInput['hook_event_name']) {
    'UserPromptExpansion' {
        $names += $hookInput['command_name']
        if ($hookInput['prompt'] -match '^\s*/([^\s]+)') { $names += $Matches[1] }
    }
    'PreToolUse' {
        # The Skill tool's input field name is not documented; check the likely ones.
        $tool = $hookInput['tool_input']
        if ($tool -is [hashtable]) { foreach ($key in 'skill', 'name', 'command', 'skill_name') { $names += $tool[$key] } }
    }
}

foreach ($name in $names) {
    if ($name -is [string] -and ($name.Trim().TrimStart('/') -replace '^.*:', '') -ieq 'pr-review') {
        [Console]::Error.WriteLine('BLOCKED: /pr-review (Greptile review loop) is disabled in this repository. Use /process-pr-feedback; see README "PR review flow".')
        exit 2
    }
}
exit 0
