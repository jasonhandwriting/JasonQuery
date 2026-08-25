[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$generatorPath = Join-Path $PSScriptRoot "New-JasonQueryUpdateMetadata.ps1"
$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("JasonQuery-Step5-" + [guid]::NewGuid().ToString("N"))
$metadataPath = Join-Path $testRoot "jasonquery-update.json"
$legacyPath = Join-Path $testRoot "jq.txt"
$productionPackagePath = Join-Path $testRoot "JasonQuery64.zip"
$testPackagePath = Join-Path $testRoot "JasonQuery64Test.zip"
$script:passed = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Assert-Equal {
    param(
        [AllowNull()]
        [object]$Expected,

        [AllowNull()]
        [object]$Actual,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if ($Expected -cne $Actual) {
        throw "$Message Expected: '$Expected'. Actual: '$Actual'."
    }
}

function Complete-Test {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $script:passed++
    Write-Host "[PASS] $Name"
}

function Read-Metadata {
    return [System.IO.File]::ReadAllText($metadataPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
}

function Get-Release {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Metadata,

        [Parameter(Mandatory = $true)]
        [string]$TagName
    )

    return @($Metadata.releases | Where-Object { [string]$_.tag_name -eq $TagName })
}

function Invoke-Generator {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("Production", "Test")]
        [string]$Channel,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$PackagePath,

        [string]$TargetMetadataPath = $metadataPath,

        [string]$TargetLegacyPath = $legacyPath
    )

    & $generatorPath `
        -Channel $Channel `
        -Version $Version `
        -PackagePath $PackagePath `
        -MetadataPath $TargetMetadataPath `
        -LegacyVersionPath $TargetLegacyPath
}

try {
    if (-not [System.IO.File]::Exists($generatorPath)) {
        throw "Generator was not found: $generatorPath"
    }

    [System.IO.Directory]::CreateDirectory($testRoot) | Out-Null
    [System.IO.File]::WriteAllBytes($productionPackagePath, [byte[]](1..64))
    [System.IO.File]::WriteAllBytes($testPackagePath, [byte[]](65..128))

    Invoke-Generator -Channel Production -Version "0.95.0" -PackagePath $productionPackagePath
    $metadata = Read-Metadata
    $productionRelease = @(Get-Release -Metadata $metadata -TagName "v0.95.0")
    Assert-Equal -Expected 1 -Actual (@($metadata.releases).Count) -Message "A first production publish must create one release."
    Assert-Equal -Expected 1 -Actual $productionRelease.Count -Message "The production release is missing."
    Assert-Equal -Expected $false -Actual ([bool]$productionRelease[0].prerelease) -Message "The production release must not be a prerelease."
    Assert-Equal -Expected '```0.95```0.95.0' -Actual ([System.IO.File]::ReadAllText($legacyPath)) -Message "The initial jq.txt value is incorrect."
    Complete-Test -Name "Production metadata initialization"

    Invoke-Generator -Channel Test -Version "0.95.1" -PackagePath $testPackagePath
    $metadata = Read-Metadata
    $testRelease = @(Get-Release -Metadata $metadata -TagName "v0.95.1")
    Assert-Equal -Expected 2 -Actual (@($metadata.releases).Count) -Message "Production and Test releases must both be retained."
    Assert-Equal -Expected 1 -Actual (@(Get-Release -Metadata $metadata -TagName "v0.95.0").Count) -Message "The production release was not preserved."
    Assert-Equal -Expected 1 -Actual $testRelease.Count -Message "The test release is missing."
    Assert-Equal -Expected $true -Actual ([bool]$testRelease[0].prerelease) -Message "The test release must be a prerelease."
    Assert-Equal -Expected '```0.95```0.95.1' -Actual ([System.IO.File]::ReadAllText($legacyPath)) -Message "The test jq.txt value is incorrect."
    Complete-Test -Name "Test metadata preserves Production"

    $testPackageHash = (Get-FileHash -LiteralPath $testPackagePath -Algorithm SHA256).Hash.ToLowerInvariant()
    $testAsset = @($testRelease[0].assets)[0]
    Assert-Equal -Expected ([long](Get-Item -LiteralPath $testPackagePath).Length) -Actual ([long]$testAsset.size) -Message "The test package size is incorrect."
    Assert-Equal -Expected "sha256:$testPackageHash" -Actual ([string]$testAsset.digest) -Message "The test package digest is incorrect."
    Assert-Equal -Expected "JasonQuery64Test.zip" -Actual ([string]$testAsset.name) -Message "The test asset name is incorrect."
    Complete-Test -Name "Package size and SHA-256 generation"

    [System.IO.File]::WriteAllBytes($testPackagePath, [byte[]](129..224))
    Invoke-Generator -Channel Test -Version "0.95.2" -PackagePath $testPackagePath
    $metadata = Read-Metadata
    Assert-Equal -Expected 2 -Actual (@($metadata.releases).Count) -Message "Only the latest release in each channel should be kept."
    Assert-Equal -Expected 0 -Actual (@(Get-Release -Metadata $metadata -TagName "v0.95.1").Count) -Message "The previous test release should be replaced."
    Assert-Equal -Expected 1 -Actual (@(Get-Release -Metadata $metadata -TagName "v0.95.2").Count) -Message "The latest test release is missing."
    Assert-Equal -Expected '```0.95```0.95.2' -Actual ([System.IO.File]::ReadAllText($legacyPath)) -Message "The updated test jq.txt value is incorrect."
    Complete-Test -Name "Latest Test release replacement"

    [System.IO.File]::WriteAllBytes($productionPackagePath, [byte[]](225..255))
    Invoke-Generator -Channel Production -Version "0.96.0" -PackagePath $productionPackagePath
    $metadata = Read-Metadata
    Assert-Equal -Expected 2 -Actual (@($metadata.releases).Count) -Message "Only the latest release in each channel should be kept after a production publish."
    Assert-Equal -Expected 1 -Actual (@(Get-Release -Metadata $metadata -TagName "v0.96.0").Count) -Message "The latest production release is missing."
    Assert-Equal -Expected 1 -Actual (@(Get-Release -Metadata $metadata -TagName "v0.95.2").Count) -Message "The test release should be preserved."
    Assert-Equal -Expected '```0.96```0.96.0' -Actual ([System.IO.File]::ReadAllText($legacyPath)) -Message "jq.txt should advertise the numerically highest release."
    Complete-Test -Name "Latest Production release replacement"

    $beforeMetadata = [System.IO.File]::ReadAllText($metadataPath)
    $beforeLegacy = [System.IO.File]::ReadAllText($legacyPath)
    $channelMismatchRejected = $false
    try {
        Invoke-Generator -Channel Production -Version "0.96.1" -PackagePath $productionPackagePath
    }
    catch {
        $channelMismatchRejected = $_.Exception.Message -like "*belongs to the Test channel*"
    }

    Assert-True -Condition $channelMismatchRejected -Message "A version/channel mismatch was not rejected."
    Assert-Equal -Expected $beforeMetadata -Actual ([System.IO.File]::ReadAllText($metadataPath)) -Message "Metadata changed after a rejected publish."
    Assert-Equal -Expected $beforeLegacy -Actual ([System.IO.File]::ReadAllText($legacyPath)) -Message "jq.txt changed after a rejected publish."
    Complete-Test -Name "Channel mismatch rejection is non-destructive"

    $testOnlyDirectory = Join-Path $testRoot "test-only"
    [System.IO.Directory]::CreateDirectory($testOnlyDirectory) | Out-Null
    $testOnlyMetadata = Join-Path $testOnlyDirectory "jasonquery-update.json"
    $testOnlyLegacy = Join-Path $testOnlyDirectory "jq.txt"
    $testWithoutProductionRejected = $false
    try {
        Invoke-Generator `
            -Channel Test `
            -Version "1.0.1" `
            -PackagePath $testPackagePath `
            -TargetMetadataPath $testOnlyMetadata `
            -TargetLegacyPath $testOnlyLegacy
    }
    catch {
        $testWithoutProductionRejected = $_.Exception.Message -like "*requires an existing Production release*"
    }

    Assert-True -Condition $testWithoutProductionRejected -Message "A Test-first publication was not rejected."
    Assert-True -Condition (-not [System.IO.File]::Exists($testOnlyMetadata)) -Message "Rejected Test-first metadata should not be created."
    Assert-True -Condition (-not [System.IO.File]::Exists($testOnlyLegacy)) -Message "Rejected Test-first jq.txt should not be created."
    Complete-Test -Name "Test-first publication rejection"

    $corruptDirectory = Join-Path $testRoot "corrupt"
    [System.IO.Directory]::CreateDirectory($corruptDirectory) | Out-Null
    $corruptMetadata = Join-Path $corruptDirectory "jasonquery-update.json"
    $corruptLegacy = Join-Path $corruptDirectory "jq.txt"
    [System.IO.File]::WriteAllText($corruptMetadata, "{not-json", (New-Object System.Text.UTF8Encoding($false)))
    [System.IO.File]::WriteAllText($corruptLegacy, "keep-this", (New-Object System.Text.UTF8Encoding($false)))
    $corruptRejected = $false
    try {
        Invoke-Generator `
            -Channel Production `
            -Version "1.0.0" `
            -PackagePath $productionPackagePath `
            -TargetMetadataPath $corruptMetadata `
            -TargetLegacyPath $corruptLegacy
    }
    catch {
        $corruptRejected = $_.Exception.Message -like "*not valid JSON*"
    }

    Assert-True -Condition $corruptRejected -Message "Corrupt existing metadata was not rejected."
    Assert-Equal -Expected "{not-json" -Actual ([System.IO.File]::ReadAllText($corruptMetadata)) -Message "Corrupt metadata was overwritten after rejection."
    Assert-Equal -Expected "keep-this" -Actual ([System.IO.File]::ReadAllText($corruptLegacy)) -Message "jq.txt changed after corrupt metadata rejection."
    Complete-Test -Name "Corrupt existing metadata rejection is non-destructive"

    $reportPath = Join-Path $testRoot "publish-report.txt"
    [System.IO.File]::WriteAllText($reportPath, "Existing report" + [Environment]::NewLine, (New-Object System.Text.UTF8Encoding($false)))

    & $generatorPath `
        -Channel Production `
        -Version "0.96.0" `
        -PackagePath $productionPackagePath `
        -MetadataPath $metadataPath `
        -LegacyVersionPath $legacyPath `
        -ReportPath $reportPath

    $reportText = [System.IO.File]::ReadAllText($reportPath, [System.Text.Encoding]::UTF8)
    Assert-True -Condition ($reportText.StartsWith("Existing report")) -Message "The existing publish report content was not preserved."
    Assert-True -Condition ($reportText -like "*Update Metadata*") -Message "The update metadata report section is missing."
    Assert-True -Condition ($reportText.Contains("[PASS] Channel - Production")) -Message "The report channel result is missing."
    Assert-True -Condition ($reportText.Contains("[PASS] Version - 0.96.0")) -Message "The report version result is missing."
    Assert-True -Condition ($reportText.Contains("[PASS] Package SHA-256 -")) -Message "The report SHA-256 result is missing."
    Complete-Test -Name "Windows PowerShell 5.1 report append compatibility"

    $utf8Bytes = [System.IO.File]::ReadAllBytes($metadataPath)
    $hasUtf8Bom = $utf8Bytes.Length -ge 3 -and $utf8Bytes[0] -eq 0xEF -and $utf8Bytes[1] -eq 0xBB -and $utf8Bytes[2] -eq 0xBF
    Assert-True -Condition (-not $hasUtf8Bom) -Message "Generated metadata must use UTF-8 without BOM."
    $legacyBytes = [System.IO.File]::ReadAllBytes($legacyPath)
    $legacyHasNewLine = $legacyBytes -contains 0x0A -or $legacyBytes -contains 0x0D
    Assert-True -Condition (-not $legacyHasNewLine) -Message "jq.txt must contain only the legacy contract value."
    Complete-Test -Name "Output encoding and legacy formatting"

    Write-Host ""
    Write-Host "Step 5 update metadata self-test passed: $script:passed tests."
}
finally {
    if ([System.IO.Directory]::Exists($testRoot)) {
        [System.IO.Directory]::Delete($testRoot, $true)
    }
}
