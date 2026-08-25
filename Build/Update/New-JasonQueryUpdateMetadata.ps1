[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Production", "Test")]
    [string]$Channel,

    [string]$ApplicationPath,

    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true)]
    [string]$MetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$LegacyVersionPath,

    [string]$WebsiteBaseUrl = "https://www.jasonquery.org/JasonQueryUpdate",

    [string]$ReleasePageUrl = "https://www.jasonquery.org/",

    [string]$ReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$productionPackageName = "JasonQuery64.zip"
$testPackageName = "JasonQuery64Test.zip"

function Get-FullPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetFullPath($Path)
}

function Get-NormalizedVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ($Value -notmatch '^\s*v?(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?\s*$') {
        throw "The version '$Value' must use major.minor.build or major.minor.build.0 format."
    }

    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $build = [int]$Matches[3]
    $revision = if ($Matches.ContainsKey(4) -and -not [string]::IsNullOrWhiteSpace($Matches[4])) {
        [int]$Matches[4]
    }
    else {
        0
    }

    if ($revision -ne 0) {
        throw "The fourth version component must be zero. The update contract uses three components."
    }

    $text = "$major.$minor.$build"
    $resolvedChannel = if ($build -eq 0) { "Production" } else { "Test" }

    return [pscustomobject]@{
        Text = $text
        Value = [System.Version]::Parse($text)
        Channel = $resolvedChannel
        LegacyProduction = "$major.$minor"
    }
}

function Get-RequiredProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Object,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        throw "$Context is missing '$Name'."
    }

    return $property.Value
}

function Test-HttpsUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,

        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $uri = $null
    if (-not [System.Uri]::TryCreate($Url, [System.UriKind]::Absolute, [ref]$uri) -or
        $uri.Scheme -ne [System.Uri]::UriSchemeHttps) {
        throw "$Context must be an absolute HTTPS URL."
    }
}

function ConvertTo-ValidatedRelease {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Release
    )

    $tagName = [string](Get-RequiredProperty -Object $Release -Name "tag_name" -Context "A release")
    $versionInfo = Get-NormalizedVersion -Value $tagName
    $expectedPackageName = if ($versionInfo.Channel -eq "Production") {
        $productionPackageName
    }
    else {
        $testPackageName
    }

    $draft = Get-RequiredProperty -Object $Release -Name "draft" -Context "Release $tagName"
    if ($draft -isnot [bool] -or $draft) {
        throw "Release $tagName must have draft=false."
    }

    $prerelease = Get-RequiredProperty -Object $Release -Name "prerelease" -Context "Release $tagName"
    if ($prerelease -isnot [bool] -or $prerelease -ne ($versionInfo.Channel -eq "Test")) {
        throw "Release $tagName has a prerelease value that does not match its version channel."
    }

    $assets = @(Get-RequiredProperty -Object $Release -Name "assets" -Context "Release $tagName")
    $matchingAssets = @($assets | Where-Object {
        [string](Get-RequiredProperty -Object $_ -Name "name" -Context "An asset in $tagName") -ceq $expectedPackageName
    })

    if ($matchingAssets.Count -ne 1) {
        throw "Release $tagName must contain exactly one $expectedPackageName asset."
    }

    $asset = $matchingAssets[0]
    $assetState = [string](Get-RequiredProperty -Object $asset -Name "state" -Context "Asset $expectedPackageName")
    if ($assetState -ne "uploaded") {
        throw "Asset $expectedPackageName must have state=uploaded."
    }

    $assetSize = [long](Get-RequiredProperty -Object $asset -Name "size" -Context "Asset $expectedPackageName")
    if ($assetSize -le 0) {
        throw "Asset $expectedPackageName must have a positive size."
    }

    $assetDigest = [string](Get-RequiredProperty -Object $asset -Name "digest" -Context "Asset $expectedPackageName")
    if ($assetDigest -notmatch '^sha256:[0-9a-fA-F]{64}$') {
        throw "Asset $expectedPackageName must have a sha256 digest."
    }

    $downloadUrl = [string](Get-RequiredProperty -Object $asset -Name "browser_download_url" -Context "Asset $expectedPackageName")
    Test-HttpsUrl -Url $downloadUrl -Context "Asset $expectedPackageName browser_download_url"

    $releaseUrl = [string](Get-RequiredProperty -Object $Release -Name "html_url" -Context "Release $tagName")
    Test-HttpsUrl -Url $releaseUrl -Context "Release $tagName html_url"

    $publishedAt = [string](Get-RequiredProperty -Object $Release -Name "published_at" -Context "Release $tagName")
    $publishedAtValue = [datetimeoffset]::MinValue
    if (-not [datetimeoffset]::TryParse($publishedAt, [ref]$publishedAtValue)) {
        throw "Release $tagName has an invalid published_at value."
    }

    $releaseName = [string](Get-RequiredProperty -Object $Release -Name "name" -Context "Release $tagName")
    $bodyProperty = $Release.PSObject.Properties["body"]
    $releaseBody = if ($null -eq $bodyProperty -or $null -eq $bodyProperty.Value) { "" } else { [string]$bodyProperty.Value }

    return [pscustomobject]@{
        VersionInfo = $versionInfo
        Document = [ordered]@{
            tag_name = "v$($versionInfo.Text)"
            name = $releaseName
            body = $releaseBody
            draft = $false
            prerelease = ($versionInfo.Channel -eq "Test")
            published_at = $publishedAtValue.ToUniversalTime().ToString("o")
            html_url = $releaseUrl
            assets = @(
                [ordered]@{
                    name = $expectedPackageName
                    state = "uploaded"
                    content_type = "application/zip"
                    size = $assetSize
                    digest = $assetDigest.ToLowerInvariant()
                    browser_download_url = $downloadUrl
                }
            )
        }
    }
}

function Read-ExistingReleases {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not [System.IO.File]::Exists($Path)) {
        return @()
    }

    try {
        $metadata = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    }
    catch {
        throw "The existing update metadata is not valid JSON: $($_.Exception.Message)"
    }

    $schemaVersion = [int](Get-RequiredProperty -Object $metadata -Name "schema_version" -Context "Update metadata")
    if ($schemaVersion -ne 1) {
        throw "The existing update metadata schema_version must be 1."
    }

    $product = [string](Get-RequiredProperty -Object $metadata -Name "product" -Context "Update metadata")
    if ($product -ne "JasonQuery") {
        throw "The existing update metadata product must be JasonQuery."
    }

    $releases = @(Get-RequiredProperty -Object $metadata -Name "releases" -Context "Update metadata")
    $validated = @()
    foreach ($release in $releases) {
        $validated += ConvertTo-ValidatedRelease -Release $release
    }

    return $validated
}

function Get-LatestReleaseForChannel {
    param(
        [object[]]$Releases,
        [Parameter(Mandatory = $true)]
        [ValidateSet("Production", "Test")]
        [string]$ExpectedChannel
    )

    return @($Releases | Where-Object {
        $_.VersionInfo.Channel -eq $ExpectedChannel
    } | Sort-Object { $_.VersionInfo.Value } -Descending | Select-Object -First 1)
}

function Write-Utf8WithoutBom {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

function Install-GeneratedFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MetadataTemporaryPath,

        [Parameter(Mandatory = $true)]
        [string]$LegacyTemporaryPath,

        [Parameter(Mandatory = $true)]
        [string]$MetadataDestination,

        [Parameter(Mandatory = $true)]
        [string]$LegacyDestination
    )

    $identifier = [guid]::NewGuid().ToString("N")
    $metadataBackup = "$MetadataDestination.backup-$identifier"
    $legacyBackup = "$LegacyDestination.backup-$identifier"
    $metadataInstalled = $false
    $legacyInstalled = $false

    try {
        if ([System.IO.File]::Exists($MetadataDestination)) {
            [System.IO.File]::Move($MetadataDestination, $metadataBackup)
        }

        if ([System.IO.File]::Exists($LegacyDestination)) {
            [System.IO.File]::Move($LegacyDestination, $legacyBackup)
        }

        [System.IO.File]::Move($MetadataTemporaryPath, $MetadataDestination)
        $metadataInstalled = $true
        [System.IO.File]::Move($LegacyTemporaryPath, $LegacyDestination)
        $legacyInstalled = $true
    }
    catch {
        if ($metadataInstalled -and [System.IO.File]::Exists($MetadataDestination)) {
            [System.IO.File]::Delete($MetadataDestination)
        }

        if ($legacyInstalled -and [System.IO.File]::Exists($LegacyDestination)) {
            [System.IO.File]::Delete($LegacyDestination)
        }

        if ([System.IO.File]::Exists($metadataBackup)) {
            [System.IO.File]::Move($metadataBackup, $MetadataDestination)
        }

        if ([System.IO.File]::Exists($legacyBackup)) {
            [System.IO.File]::Move($legacyBackup, $LegacyDestination)
        }

        throw
    }
    finally {
        foreach ($path in @($MetadataTemporaryPath, $LegacyTemporaryPath, $metadataBackup, $legacyBackup)) {
            if ([System.IO.File]::Exists($path)) {
                [System.IO.File]::Delete($path)
            }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($ApplicationPath) -eq [string]::IsNullOrWhiteSpace($Version)) {
    throw "Specify exactly one of ApplicationPath or Version. Version is intended only for the offline generator self-test."
}

$packageFullPath = Get-FullPath -Path $PackagePath
$metadataFullPath = Get-FullPath -Path $MetadataPath
$legacyFullPath = Get-FullPath -Path $LegacyVersionPath

if ($metadataFullPath -eq $legacyFullPath) {
    throw "MetadataPath and LegacyVersionPath must be different files."
}

if (-not [System.IO.File]::Exists($packageFullPath)) {
    throw "The update package was not found: $packageFullPath"
}

$resolvedVersionText = $Version
if (-not [string]::IsNullOrWhiteSpace($ApplicationPath)) {
    $applicationFullPath = Get-FullPath -Path $ApplicationPath
    if (-not [System.IO.File]::Exists($applicationFullPath)) {
        throw "JasonQuery.exe was not found: $applicationFullPath"
    }

    $resolvedVersionText = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($applicationFullPath).FileVersion
    if ([string]::IsNullOrWhiteSpace($resolvedVersionText)) {
        throw "JasonQuery.exe does not contain a file version."
    }
}

$currentVersion = Get-NormalizedVersion -Value $resolvedVersionText
if ($currentVersion.Channel -ne $Channel) {
    throw "Version $($currentVersion.Text) belongs to the $($currentVersion.Channel) channel, not $Channel."
}

$expectedPackageName = if ($Channel -eq "Production") { $productionPackageName } else { $testPackageName }
$actualPackageName = [System.IO.Path]::GetFileName($packageFullPath)
if ($actualPackageName -cne $expectedPackageName) {
    throw "The $Channel package must be named $expectedPackageName. Actual name: $actualPackageName"
}

Test-HttpsUrl -Url $WebsiteBaseUrl -Context "WebsiteBaseUrl"
Test-HttpsUrl -Url $ReleasePageUrl -Context "ReleasePageUrl"

$existingReleases = @(Read-ExistingReleases -Path $metadataFullPath)
$existingSameChannel = @(Get-LatestReleaseForChannel -Releases $existingReleases -ExpectedChannel $Channel)
if ($existingSameChannel.Count -eq 1 -and $existingSameChannel[0].VersionInfo.Value -gt $currentVersion.Value) {
    throw "The existing $Channel release $($existingSameChannel[0].VersionInfo.Text) is newer than $($currentVersion.Text). Metadata rollback is not allowed."
}

$oppositeChannel = if ($Channel -eq "Production") { "Test" } else { "Production" }
$oppositeRelease = @(Get-LatestReleaseForChannel -Releases $existingReleases -ExpectedChannel $oppositeChannel)

if ($Channel -eq "Test" -and $oppositeRelease.Count -eq 0) {
    throw "Test metadata requires an existing Production release. Run the official publish workflow first."
}

$packageInfo = New-Object System.IO.FileInfo($packageFullPath)
if ($packageInfo.Length -le 0) {
    throw "The update package is empty: $packageFullPath"
}

$packageHash = (Get-FileHash -LiteralPath $packageFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
$publishedAt = [datetimeoffset]::UtcNow.ToString("o")
$downloadUrl = $WebsiteBaseUrl.TrimEnd('/') + "/" + [System.Uri]::EscapeDataString($expectedPackageName)
$releaseName = if ($Channel -eq "Production") {
    "JasonQuery $($currentVersion.Text)"
}
else {
    "JasonQuery $($currentVersion.Text) Test"
}

$currentRelease = [pscustomobject]@{
    VersionInfo = $currentVersion
    Document = [ordered]@{
        tag_name = "v$($currentVersion.Text)"
        name = $releaseName
        body = ""
        draft = $false
        prerelease = ($Channel -eq "Test")
        published_at = $publishedAt
        html_url = $ReleasePageUrl
        assets = @(
            [ordered]@{
                name = $expectedPackageName
                state = "uploaded"
                content_type = "application/zip"
                size = $packageInfo.Length
                digest = "sha256:$packageHash"
                browser_download_url = $downloadUrl
            }
        )
    }
}

$selectedReleases = @($currentRelease)
if ($oppositeRelease.Count -eq 1) {
    $selectedReleases += $oppositeRelease[0]
}

$selectedReleases = @($selectedReleases | Sort-Object { $_.VersionInfo.Value } -Descending)
$productionRelease = @(Get-LatestReleaseForChannel -Releases $selectedReleases -ExpectedChannel "Production")
if ($productionRelease.Count -ne 1) {
    throw "The generated metadata must contain one Production release."
}

$highestRelease = @($selectedReleases | Sort-Object { $_.VersionInfo.Value } -Descending | Select-Object -First 1)[0]
$legacyText = '```' + $productionRelease[0].VersionInfo.LegacyProduction + '```' + $highestRelease.VersionInfo.Text

$metadataDocument = [ordered]@{
    schema_version = 1
    product = "JasonQuery"
    generated_at = $publishedAt
    releases = @($selectedReleases | ForEach-Object { $_.Document })
}

$jsonText = $metadataDocument | ConvertTo-Json -Depth 8
$metadataDirectory = [System.IO.Path]::GetDirectoryName($metadataFullPath)
$legacyDirectory = [System.IO.Path]::GetDirectoryName($legacyFullPath)
[System.IO.Directory]::CreateDirectory($metadataDirectory) | Out-Null
[System.IO.Directory]::CreateDirectory($legacyDirectory) | Out-Null

$identifier = [guid]::NewGuid().ToString("N")
$metadataTemporaryPath = "$metadataFullPath.tmp-$identifier"
$legacyTemporaryPath = "$legacyFullPath.tmp-$identifier"

try {
    Write-Utf8WithoutBom -Path $metadataTemporaryPath -Text $jsonText
    Write-Utf8WithoutBom -Path $legacyTemporaryPath -Text $legacyText

    $verifiedMetadata = [System.IO.File]::ReadAllText($metadataTemporaryPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    if ([int]$verifiedMetadata.schema_version -ne 1 -or [string]$verifiedMetadata.product -ne "JasonQuery") {
        throw "Generated metadata failed schema verification."
    }

    $verifiedCurrentRelease = @($verifiedMetadata.releases | Where-Object { [string]$_.tag_name -eq "v$($currentVersion.Text)" })
    if ($verifiedCurrentRelease.Count -ne 1) {
        throw "Generated metadata does not contain exactly one current release."
    }

    $verifiedAsset = @($verifiedCurrentRelease[0].assets | Where-Object { [string]$_.name -ceq $expectedPackageName })
    if ($verifiedAsset.Count -ne 1 -or
        [long]$verifiedAsset[0].size -ne $packageInfo.Length -or
        [string]$verifiedAsset[0].digest -ne "sha256:$packageHash" -or
        [string]$verifiedAsset[0].browser_download_url -ne $downloadUrl) {
        throw "Generated metadata failed package identity verification."
    }

    $verifiedLegacyText = [System.IO.File]::ReadAllText($legacyTemporaryPath, [System.Text.Encoding]::UTF8)
    if ($verifiedLegacyText -cne $legacyText) {
        throw "Generated jq.txt failed compatibility verification."
    }

    Install-GeneratedFiles `
        -MetadataTemporaryPath $metadataTemporaryPath `
        -LegacyTemporaryPath $legacyTemporaryPath `
        -MetadataDestination $metadataFullPath `
        -LegacyDestination $legacyFullPath
}
finally {
    foreach ($path in @($metadataTemporaryPath, $legacyTemporaryPath)) {
        if ([System.IO.File]::Exists($path)) {
            [System.IO.File]::Delete($path)
        }
    }
}

if (-not [string]::IsNullOrWhiteSpace($ReportPath)) {
    try {
        $reportFullPath = Get-FullPath -Path $ReportPath
        $reportDirectory = [System.IO.Path]::GetDirectoryName($reportFullPath)
        [System.IO.Directory]::CreateDirectory($reportDirectory) | Out-Null
        $reportLines = @(
            "",
            "Update Metadata",
            "===============",
            "[PASS] Channel - $Channel",
            "[PASS] Version - $($currentVersion.Text)",
            "[PASS] Package - $packageFullPath",
            "[PASS] Package size - $($packageInfo.Length) bytes",
            "[PASS] Package SHA-256 - $packageHash",
            "[PASS] Metadata - $metadataFullPath",
            "[PASS] Legacy metadata - $legacyFullPath"
        )
        [System.IO.File]::AppendAllLines($reportFullPath, $reportLines, (New-Object System.Text.UTF8Encoding($false)))
    }
    catch {
        Write-Warning "Update metadata was generated, but the validation report could not be updated: $($_.Exception.Message)"
    }
}

Write-Host "Update metadata generated successfully."
Write-Host "Channel:  $Channel"
Write-Host "Version:  $($currentVersion.Text)"
Write-Host "Package:  $packageFullPath"
Write-Host "Size:     $($packageInfo.Length) bytes"
Write-Host "SHA-256:  $packageHash"
Write-Host "Metadata: $metadataFullPath"
Write-Host "Legacy:   $legacyFullPath"
