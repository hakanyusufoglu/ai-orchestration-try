# Stop / SubagentStop hook: when C# code changed since the last successful check, build and test must pass
# before the agent may stop. Exit code 2 = keep working, stderr goes to Claude.
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_UI_LANGUAGE = 'en'
$env:DOTNET_NOLOGO = '1'

$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (git rev-parse --show-toplevel) }
Set-Location $root

$paths = @('*.cs', '*.csproj', '*.slnx')
$tracked = (git diff HEAD -- $paths) -join "`n"
$untracked = @(git ls-files --others --exclude-standard -- $paths)
if ([string]::IsNullOrEmpty($tracked) -and $untracked.Count -eq 0) { exit 0 }

$fingerprintSource = $tracked + (($untracked | ForEach-Object { $_ + (Get-Content -Raw -LiteralPath $_) }) -join "`n")
$sha = [System.Security.Cryptography.SHA256]::Create()
$fingerprint = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($fingerprintSource)))

$stateFile = Join-Path $root '.claude/.verify-state'
if ((Test-Path $stateFile) -and ((Get-Content -Raw $stateFile).Trim() -eq $fingerprint)) { exit 0 }

$failure = $null
$buildOutput = dotnet build OrderService.slnx -v q -clp:ErrorsOnly 2>&1
if ($LASTEXITCODE -ne 0) {
    $failure = "Build failed:`n" + (($buildOutput | Select-Object -Last 40) -join "`n")
} else {
    $testOutput = dotnet test OrderService.slnx --no-build -v q 2>&1
    if ($LASTEXITCODE -ne 0) {
        $failure = "Tests failed:`n" + (($testOutput | Select-Object -Last 60) -join "`n")
    }
}

if ($null -eq $failure) {
    Set-Content -LiteralPath $stateFile -Value $fingerprint -NoNewline
    exit 0
}

if ($payload.stop_hook_active) {
    @{ systemMessage = "verify hook: build/test still failing after a fix attempt. Check the output before accepting this work." } | ConvertTo-Json -Compress
    exit 0
}

[Console]::Error.WriteLine("verify hook: the work is not done yet. $failure")
exit 2
