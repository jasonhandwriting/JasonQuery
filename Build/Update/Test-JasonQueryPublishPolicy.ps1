[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$validatorPath = Join-Path $PSScriptRoot "Confirm-JasonQueryPublishVersion.ps1"
$officialPublisherPath = Join-Path $repositoryRoot "Publish-JasonQuery-Release.bat"
$testPublisherPath = Join-Path $repositoryRoot "Publish-JasonQuery-Test.bat"
$draftReleasePath = Join-Path $repositoryRoot "Build\New-DraftRelease.ps1"
$draftWorkflowPath = Join-Path $repositoryRoot ".github\workflows\draft-release.yml"
$readmePath = Join-Path $PSScriptRoot "README.md"
$passed = 0

function Complete-Test
{
    param([Parameter(Mandatory = $true)][string]$Name)
    $script:passed++
    Write-Host "[PASS] $Name"
}

function Assert-True
{
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition)
    {
        throw $Message
    }
}

function Invoke-ExpectedPass
{
    param(
        [Parameter(Mandatory = $true)][string]$Channel,
        [Parameter(Mandatory = $true)][string]$Version,
        [Parameter(Mandatory = $true)][string]$Name
    )

    & $validatorPath -Channel $Channel -Version $Version
    Complete-Test $Name
}

function Invoke-ExpectedFailure
{
    param(
        [Parameter(Mandatory = $true)][string]$Channel,
        [Parameter(Mandatory = $true)][string]$Version,
        [Parameter(Mandatory = $true)][string]$ExpectedMessageFragment,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $failedAsExpected = $false

    try
    {
        & $validatorPath -Channel $Channel -Version $Version
    }
    catch
    {
        $failedAsExpected = $_.Exception.Message.IndexOf(
            $ExpectedMessageFragment,
            [System.StringComparison]::OrdinalIgnoreCase
        ) -ge 0
    }

    Assert-True $failedAsExpected "$Name did not reject the input as expected."
    Complete-Test $Name
}

if (-not [System.IO.File]::Exists($validatorPath))
{
    throw "Publish version validator was not found: $validatorPath"
}

Invoke-ExpectedPass -Channel Production -Version "0.97.0" -Name "Production version acceptance"
Invoke-ExpectedPass -Channel Test -Version "0.97.1" -Name "Test version acceptance"
Invoke-ExpectedPass -Channel Test -Version "12.34.56.0" -Name "Four-component zero revision acceptance"
Invoke-ExpectedFailure -Channel Production -Version "0.97.1" -ExpectedMessageFragment "belongs to the Test channel" -Name "Production rejects Test version"
Invoke-ExpectedFailure -Channel Test -Version "0.97.0" -ExpectedMessageFragment "belongs to the Production channel" -Name "Test rejects Production version"
Invoke-ExpectedFailure -Channel Production -Version "0.97.0.1" -ExpectedMessageFragment "fourth version component must be zero" -Name "Nonzero fourth component rejection"
Invoke-ExpectedFailure -Channel Production -Version "0.97" -ExpectedMessageFragment "must use major.minor.build" -Name "Two-component version rejection"

$officialPublisher = [System.IO.File]::ReadAllText($officialPublisherPath)
$testPublisher = [System.IO.File]::ReadAllText($testPublisherPath)
$draftRelease = [System.IO.File]::ReadAllText($draftReleasePath)
$draftWorkflow = [System.IO.File]::ReadAllText($draftWorkflowPath)
$readme = [System.IO.File]::ReadAllText($readmePath)

$officialPreflight = $officialPublisher.IndexOf('-Channel "Production"', [System.StringComparison]::Ordinal)
$officialRepository = $officialPublisher.IndexOf('echo [1/9] Validating the repository...', [System.StringComparison]::Ordinal)
Assert-True ($officialPreflight -ge 0 -and $officialPreflight -lt $officialRepository) "Production version preflight is not before repository/package work."
Complete-Test "Production early version preflight"

$testPreflight = $testPublisher.IndexOf('-Channel "Test"', [System.StringComparison]::Ordinal)
$testRepository = $testPublisher.IndexOf('echo [1/8] Validating the repository...', [System.StringComparison]::Ordinal)
Assert-True ($testPreflight -ge 0 -and $testPreflight -lt $testRepository) "Test version preflight is not before repository/package work."
Assert-True ($testPublisher.IndexOf('-RequireCleanWorkingTree -ExpectedBranch "main"', [System.StringComparison]::Ordinal) -ge 0) "Test publisher does not require clean main."
Complete-Test "Test early preflight and clean-main parity"

Assert-True ($draftRelease.IndexOf("'^v(\d+)[.](\d+)[.](\d+)$'", [System.StringComparison]::Ordinal) -ge 0) "Draft Release does not require an exact three-component tag."
Assert-True ($draftRelease.IndexOf('"JasonQuery64Test.zip"', [System.StringComparison]::Ordinal) -ge 0) "Draft Release does not derive the Test asset alias."
Assert-True ($draftRelease.IndexOf('"--verify-tag"', [System.StringComparison]::Ordinal) -ge 0) "Draft Release does not use --verify-tag."
Assert-True ($draftRelease.IndexOf('"--target"', [System.StringComparison]::Ordinal) -lt 0) "Draft Release still allows implicit tag creation through --target."
Assert-True ($draftRelease.IndexOf("[switch]`$Prerelease", [System.StringComparison]::Ordinal) -lt 0) "Draft Release still exposes manual prerelease selection."
Assert-True ($draftRelease.IndexOf("ExpectedCommitSha", [System.StringComparison]::Ordinal) -ge 0) "Draft Release does not verify the qualified commit SHA."
Complete-Test "Draft Release tag/channel/asset contract"

Assert-True ($draftWorkflow.IndexOf("qualified_commit_sha:", [System.StringComparison]::Ordinal) -ge 0) "Draft Release workflow does not request the qualified commit SHA."
Assert-True ($draftWorkflow.IndexOf("inputs.prerelease", [System.StringComparison]::Ordinal) -lt 0) "Draft Release workflow still exposes manual prerelease selection."
Assert-True ($draftWorkflow.IndexOf("-ExpectedCommitSha", [System.StringComparison]::Ordinal) -ge 0) "Draft Release workflow does not pass the qualified commit SHA."
Complete-Test "Draft Release workflow contract"

Assert-True ($readme.IndexOf("Historical tag exception", [System.StringComparison]::Ordinal) -ge 0) "README does not document the historical tag exception."
Assert-True ($readme.IndexOf("v0.94", [System.StringComparison]::Ordinal) -ge 0) "README does not name v0.94 as historical."
Assert-True ($readme.IndexOf("v0.95", [System.StringComparison]::Ordinal) -ge 0) "README does not name v0.95 as historical."
Assert-True ($readme.IndexOf("JasonQuery v0.97.1 Test", [System.StringComparison]::Ordinal) -ge 0) "README does not document the Test release title policy."
Complete-Test "Release/tag policy documentation"

Write-Host ""
Write-Host "P0-06 publish policy self-test passed: $passed tests." -ForegroundColor Green
$global:LASTEXITCODE = 0
