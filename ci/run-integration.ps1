param([string]$Dotnet = 'dotnet', [string]$Results = 'TestResults/ci')
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    New-Item -ItemType Directory -Force -Path $Results | Out-Null
    $trx = Join-Path $Results 'integration.trx'
    if (Test-Path -LiteralPath $trx) { Remove-Item -LiteralPath $trx }
    # Preserve native test failures as an exit code, including in Windows PowerShell 5.
    $ErrorActionPreference = 'Continue'
    & $Dotnet test tests/HybridTests/HybridTests.csproj --configuration Release --logger 'trx;LogFileName=integration.trx' --results-directory $Results --nologo *> (Join-Path $Results 'run.log')
    $testExit = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    Get-Content (Join-Path $Results 'run.log') -Tail 25
    if (!(Test-Path -LiteralPath $trx)) { throw "No TRX generated. Test exit code: $testExit. See run.log." }
    [xml]$xml = Get-Content -LiteralPath $trx -Raw
    $tests = @($xml.SelectNodes("//*[local-name()='UnitTestResult']"))
    if ($tests.Count -eq 0) { throw 'No tests executed.' }
    $passed = @($tests | Where-Object outcome -eq 'Passed').Count
    $failed = @($tests | Where-Object outcome -eq 'Failed').Count
    $summary = "Tests: $($tests.Count); passed: $passed; failed: $failed; exit: $testExit"
    $summary | Set-Content (Join-Path $Results 'summary.txt') -Encoding UTF8
    if ($env:GITHUB_STEP_SUMMARY) { $summary | Add-Content $env:GITHUB_STEP_SUMMARY }
    Write-Host $summary
    if ($testExit -ne 0 -or $failed -gt 0) { exit 1 }
} finally { Pop-Location }
