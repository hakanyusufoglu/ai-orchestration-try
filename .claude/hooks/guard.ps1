# PreToolUse hook: blocks destructive or outward-facing commands. Exit code 2 = block, stderr goes to Claude.
$ErrorActionPreference = 'Stop'

$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$command = [string]$payload.tool_input.command
if ([string]::IsNullOrWhiteSpace($command)) { exit 0 }

$blocked = @(
    @{ Pattern = '\bgit\s+push\b';                          Reason = 'git push is done by the user.' }
    @{ Pattern = '\bgit\s+reset\s+--hard\b';                Reason = 'git reset --hard discards work.' }
    @{ Pattern = '\bgit\s+clean\s+-\w*f';                   Reason = 'git clean -f deletes untracked files.' }
    @{ Pattern = '\brm\s+-\w*r\w*f|\brm\s+-\w*f\w*r';       Reason = 'Recursive forced delete.' }
    @{ Pattern = 'Remove-Item\b(?=.*-Recurse)(?=.*-Force)'; Reason = 'Recursive forced delete.' }
)

foreach ($rule in $blocked) {
    if ($command -match $rule.Pattern) {
        [Console]::Error.WriteLine("Blocked by project guard hook: $($rule.Reason) Ask the user to run it themselves if it is really needed.")
        exit 2
    }
}

if ($command -match '\bgit\s+merge\b') {
    $branch = (git branch --show-current 2>$null)
    if ($branch -in @('main', 'master')) {
        [Console]::Error.WriteLine("Blocked by project guard hook: merging into '$branch' is done by the user through a merge request.")
        exit 2
    }
}

exit 0
