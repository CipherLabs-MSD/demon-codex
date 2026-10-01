param(
    [Parameter(Mandatory = $true)][string]$UnityEditor,
    [ValidateSet('EditMode', 'PlayMode')][string]$TestPlatform = 'EditMode',
    [switch]$CaptureScreenshots
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectDirectory = Join-Path $repositoryRoot 'game/DemonCodex.Unity'
# Each invocation gets a fresh result path so an old success cannot mask failure.
$runDirectory = Join-Path $repositoryRoot ('artifacts/unity/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
$resultPath = Join-Path $runDirectory 'results.xml'
$logPath = Join-Path $runDirectory 'editor.log'
$arguments = @('-batchmode', '-projectPath', ('"' + $projectDirectory + '"'),
    '-runTests', '-testPlatform', $TestPlatform, '-testResults', ('"' + $resultPath + '"'),
    '-logFile', ('"' + $logPath + '"'))
if ($CaptureScreenshots) {
    # Editor batch mode does not repaint the Game view for ScreenCapture.
    $arguments = @($arguments | Where-Object { $_ -ne '-batchmode' })
    $env:DEMON_CODEX_CAPTURE_DIR = Join-Path $runDirectory 'screenshots'
} else {
    $arguments += '-nographics'
    $env:DEMON_CODEX_CAPTURE_DIR = $null
}
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
Write-Output "Unity PID $($process.Id); log: $logPath"
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity exited $($process.ExitCode). Inspect $logPath; tests are not assumed passed." }
if (!(Test-Path -LiteralPath $resultPath)) { throw "Unity returned no test results. Inspect $logPath." }
[xml]$results = Get-Content -LiteralPath $resultPath -Raw
$run = $results.'test-run'
if ($run.result -ne 'Passed' -or [int]$run.total -eq 0 -or [int]$run.failed -ne 0) {
    throw "Unity tests did not all pass: $resultPath"
}
Write-Output "Unity passed $($run.passed) tests; results: $resultPath"
