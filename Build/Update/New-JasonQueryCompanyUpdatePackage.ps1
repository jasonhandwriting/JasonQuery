[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$MetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$ProductionPackagePath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [string]$ReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$expectedSchemaVersion = 1
$expectedProduct = "JasonQuery"
$expectedPackageName = "JasonQuery64.zip"
$minimumVersion = [System.Version]::Parse("0.95.0")
$officialBaseUrl = "https://jasonquery.org"
$officialPackageUrl = "$officialBaseUrl/JasonQueryUpdate/$expectedPackageName"
$companyPackagePrefix = "JasonQuery-Company-Update-v"
$readmeFileName = "README-Company-Update.txt"

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

function Get-NormalizedVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ($Value -notmatch '^\s*v?(\d+)\.(\d+)\.(\d+)(?:\.0)?\s*$') {
        throw "Invalid release version: '$Value'."
    }

    return [System.Version]::Parse("$($Matches[1]).$($Matches[2]).$($Matches[3])")
}

function Assert-CanonicalWebsiteUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,

        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $uri = $null
    if (-not [System.Uri]::TryCreate($Url, [System.UriKind]::Absolute, [ref]$uri) -or
        $uri.Scheme -ne [System.Uri]::UriSchemeHttps -or
        $uri.Host -cne "jasonquery.org") {
        throw "$Context must use the canonical https://jasonquery.org domain."
    }
}

function Get-Sha256Digest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return "sha256:" + (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

$metadataFullPath = [System.IO.Path]::GetFullPath($MetadataPath)
$packageFullPath = [System.IO.Path]::GetFullPath($ProductionPackagePath)
$outputDirectoryFullPath = [System.IO.Path]::GetFullPath($OutputDirectory)

if (-not [System.IO.File]::Exists($metadataFullPath)) {
    throw "Update metadata was not found: $metadataFullPath"
}

if (-not [System.IO.File]::Exists($packageFullPath)) {
    throw "Production package was not found: $packageFullPath"
}

if ([System.IO.Path]::GetFileName($packageFullPath) -cne $expectedPackageName) {
    throw "The Production package must be named '$expectedPackageName'."
}

try {
    $metadata = [System.IO.File]::ReadAllText($metadataFullPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
}
catch {
    throw "Update metadata is not valid JSON: $($_.Exception.Message)"
}

$schemaVersion = [int](Get-RequiredProperty -Object $metadata -Name "schema_version" -Context "Update metadata")
$product = [string](Get-RequiredProperty -Object $metadata -Name "product" -Context "Update metadata")
$generatedAt = [string](Get-RequiredProperty -Object $metadata -Name "generated_at" -Context "Update metadata")

if ($schemaVersion -ne $expectedSchemaVersion) {
    throw "Unsupported metadata schema_version: $schemaVersion"
}

if ($product -cne $expectedProduct) {
    throw "Unexpected metadata product: '$product'."
}

$productionCandidates = @()
foreach ($release in @(Get-RequiredProperty -Object $metadata -Name "releases" -Context "Update metadata")) {
    if ($null -eq $release) {
        continue
    }

    $tagName = [string](Get-RequiredProperty -Object $release -Name "tag_name" -Context "Release")
    $version = Get-NormalizedVersion -Value $tagName
    $htmlUrl = [string](Get-RequiredProperty -Object $release -Name "html_url" -Context "Release $tagName")
    Assert-CanonicalWebsiteUrl -Url $htmlUrl -Context "Release $tagName html_url"

    foreach ($assetItem in @($release.assets)) {
        if ($null -eq $assetItem) {
            continue
        }

        $assetUrl = [string](Get-RequiredProperty -Object $assetItem -Name "browser_download_url" -Context "Release $tagName asset")
        Assert-CanonicalWebsiteUrl -Url $assetUrl -Context "Release $tagName asset browser_download_url"
    }

    $isDraft = [bool](Get-RequiredProperty -Object $release -Name "draft" -Context "Release $tagName")
    $isPrerelease = [bool](Get-RequiredProperty -Object $release -Name "prerelease" -Context "Release $tagName")

    if (-not $isDraft -and -not $isPrerelease -and $version.Build -eq 0) {
        $productionCandidates += [pscustomobject]@{
            Version = $version
            Release = $release
        }
    }
}

$selected = @($productionCandidates | Sort-Object Version -Descending | Select-Object -First 1)
if ($selected.Count -ne 1) {
    throw "The metadata does not contain a publishable Production release."
}

$productionVersion = $selected[0].Version
$productionRelease = $selected[0].Release

if ($productionVersion -lt $minimumVersion) {
    throw "Company update packages require Production version $minimumVersion or later."
}

$productionTag = "v$($productionVersion.ToString(3))"
if ([string]$productionRelease.tag_name -cne $productionTag) {
    throw "The Production tag must use '$productionTag'."
}

if ([string]$productionRelease.html_url -cne "$officialBaseUrl/") {
    throw "The Production release html_url must be '$officialBaseUrl/'."
}

$assets = @($productionRelease.assets | Where-Object { $null -ne $_ -and [string]$_.name -ceq $expectedPackageName })
if ($assets.Count -ne 1) {
    throw "The Production release must contain exactly one '$expectedPackageName' asset."
}

$asset = $assets[0]
$actualSize = [long](Get-Item -LiteralPath $packageFullPath).Length
$expectedSize = [long](Get-RequiredProperty -Object $asset -Name "size" -Context "Production asset")
$actualDigest = Get-Sha256Digest -Path $packageFullPath
$expectedDigest = [string](Get-RequiredProperty -Object $asset -Name "digest" -Context "Production asset")
$downloadUrl = [string](Get-RequiredProperty -Object $asset -Name "browser_download_url" -Context "Production asset")

if ($expectedSize -ne $actualSize) {
    throw "Production package size mismatch. Metadata: $expectedSize; actual: $actualSize."
}

if ($expectedDigest -cne $actualDigest) {
    throw "Production package SHA-256 mismatch. Metadata: '$expectedDigest'; actual: '$actualDigest'."
}

if ($downloadUrl -cne $officialPackageUrl) {
    throw "The Production package URL must be '$officialPackageUrl'."
}

[System.IO.Directory]::CreateDirectory($outputDirectoryFullPath) | Out-Null
$versionText = $productionVersion.ToString(3)
$outputName = "$companyPackagePrefix$versionText.zip"
$outputPath = Join-Path $outputDirectoryFullPath $outputName
$workingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("JasonQuery-Company-Update-" + [guid]::NewGuid().ToString("N"))
$stagingDirectory = Join-Path $workingRoot "content"
$temporaryZip = Join-Path $workingRoot $outputName

try {
    [System.IO.Directory]::CreateDirectory($stagingDirectory) | Out-Null
    Copy-Item -LiteralPath $packageFullPath -Destination (Join-Path $stagingDirectory $expectedPackageName)

    $companyMetadata = [ordered]@{
        schema_version = $expectedSchemaVersion
        product = $expectedProduct
        generated_at = $generatedAt
        releases = @($productionRelease)
    }

    $companyMetadataJson = $companyMetadata | ConvertTo-Json -Depth 10
    $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText(
        (Join-Path $stagingDirectory "jasonquery-update.json"),
        $companyMetadataJson + [Environment]::NewLine,
        $utf8WithoutBom
    )

    $readme = @"
JasonQuery Company Update Package
==================================

Version: $versionText
Channel: Production
Package: $expectedPackageName
SHA-256: $($actualDigest.Substring(7))

Contents
--------
- jasonquery-update.json
- JasonQuery64.zip
- README-Company-Update.txt

Deployment
----------
1. Extract all three files to one internal folder.
2. The folder may be a local disk path or a UNC path such as \\server\share\JasonQueryUpdate.
3. Grant JasonQuery users read-only access.
4. In JasonQuery, select Options > Update Settings > Company Update Folder.
5. Select the extracted folder.

Keep the three files together. Do not rename them.
This package contains Production updates only.

Traditional Chinese
-------------------
1. 將三個檔案解壓縮至同一個公司內部資料夾。
2. 可使用本機路徑或 \\server\share\JasonQueryUpdate 形式的 UNC 路徑。
3. 授予 JasonQuery 使用者唯讀權限。
4. 在 JasonQuery 選擇「選項 > 更新設定 > 公司內部更新資料夾」。
5. 選取解壓縮後的資料夾。

請勿重新命名或分開存放這三個檔案。本更新包僅包含正式版本。

Simplified Chinese
------------------
1. 将三个文件解压缩至同一个公司内部文件夹。
2. 可使用本地路径或 \\server\share\JasonQueryUpdate 形式的 UNC 路径。
3. 授予 JasonQuery 用户只读权限。
4. 在 JasonQuery 选择“选项 > 更新设置 > 公司内部更新文件夹”。
5. 选择解压缩后的文件夹。

请勿重命名或分开存放这三个文件。本更新包仅包含正式版本。
"@

    [System.IO.File]::WriteAllText((Join-Path $stagingDirectory $readmeFileName), $readme, $utf8WithoutBom)

    Compress-Archive -Path (Join-Path $stagingDirectory "*") -DestinationPath $temporaryZip -CompressionLevel Optimal

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($temporaryZip)
    try {
        $entryNames = @($archive.Entries | ForEach-Object { $_.FullName } | Sort-Object)
        $expectedEntries = @("JasonQuery64.zip", "README-Company-Update.txt", "jasonquery-update.json") | Sort-Object
        if (($entryNames -join "|") -cne ($expectedEntries -join "|")) {
            throw "Unexpected company package contents: $($entryNames -join ', ')"
        }
    }
    finally {
        $archive.Dispose()
    }

    Copy-Item -LiteralPath $temporaryZip -Destination $outputPath -Force

    if (-not [string]::IsNullOrWhiteSpace($ReportPath)) {
        Add-Content -LiteralPath $ReportPath -Encoding UTF8 -Value @(
            "",
            "Company Update Package",
            "======================",
            "[PASS] Version - $versionText",
            "[PASS] Channel - Production",
            "[PASS] Output - $outputPath",
            "[PASS] Package SHA-256 - $actualDigest",
            "[PASS] Contents - jasonquery-update.json, JasonQuery64.zip, README-Company-Update.txt"
        )
    }

    Write-Host "Company update package created: $outputPath"
    Write-Output $outputPath
}
finally {
    if ([System.IO.Directory]::Exists($workingRoot)) {
        [System.IO.Directory]::Delete($workingRoot, $true)
    }
}
