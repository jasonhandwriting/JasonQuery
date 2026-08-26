[CmdletBinding()]
param
(
    [string]$ReleaseDirectory = (Join-Path (Split-Path -Parent $PSScriptRoot) "JasonQuery\bin\Release"),

    [Parameter(Mandatory = $true)]
    [string]$ReleaseZip,

    [string]$ReportPath,

    [string]$PackageKind = "Release",

    [switch]$AppendReport
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$results = New-Object System.Collections.Generic.List[object]
$validationStartedAt = Get-Date

function Add-ValidationResult
{
    param
    (
        [ValidateSet("PASS", "WARN", "FAIL")]
        [string]$Status,
        [string]$Name,
        [string]$Detail
    )

    $result = [PSCustomObject]@{
        Status = $Status
        Name = $Name
        Detail = $Detail
    }

    $results.Add($result)

    $color = switch ($Status)
    {
        "PASS" { "Green" }
        "WARN" { "Yellow" }
        "FAIL" { "Red" }
    }

    Write-Host ("[{0}] {1} - {2}" -f $Status, $Name, $Detail) -ForegroundColor $color
}

function Get-NormalizedZipPath
{
    param([string]$Path)

    return ($Path -replace "\\", "/").TrimStart([char[]]@("/"))
}

function Get-RelativeReleasePath
{
    param
    (
        [string]$Root,
        [string]$FullName
    )

    return ($FullName.Substring($Root.Length).TrimStart([char[]]@("\", "/")) -replace "\\", "/")
}

function Get-StreamSha256
{
    param([System.IO.Stream]$Stream)

    $sha256 = [System.Security.Cryptography.SHA256]::Create()

    try
    {
        $hash = $sha256.ComputeHash($Stream)
        return ([System.BitConverter]::ToString($hash)).Replace("-", "")
    }
    finally
    {
        $sha256.Dispose()
    }
}

function Assert-SingleZipEntry
{
    param
    (
        [object[]]$Entries,
        [string]$ExpectedPath
    )

    $matches = @(
        $Entries | Where-Object {
            [string]::Equals(
                (Get-NormalizedZipPath $_.FullName),
                $ExpectedPath,
                [System.StringComparison]::OrdinalIgnoreCase
            )
        }
    )

    if ($matches.Count -ne 1)
    {
        throw "Expected exactly one ZIP entry '$ExpectedPath', but found $($matches.Count)."
    }

    return $matches[0]
}

function Write-PackageValidationReport
{
    param
    (
        [string]$Path,
        [string]$ReleaseRoot,
        [string]$ZipPath,
        [string]$ZipSha256,
        [long]$ZipLength
    )

    $reportDirectory = Split-Path -Parent $Path

    if (![string]::IsNullOrWhiteSpace($reportDirectory) -and ![System.IO.Directory]::Exists($reportDirectory))
    {
        [System.IO.Directory]::CreateDirectory($reportDirectory) | Out-Null
    }

    $passCount = @($results | Where-Object { $_.Status -eq "PASS" }).Count
    $warningCount = @($results | Where-Object { $_.Status -eq "WARN" }).Count
    $failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count
    $overallStatus = if ($failureCount -eq 0) { "PASS" } else { "FAIL" }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("")
    $lines.Add("Package Validation")
    $lines.Add("==================")
    $lines.Add("Package kind: $PackageKind")
    $lines.Add("Started: $($validationStartedAt.ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Finished: $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Release directory: $ReleaseRoot")
    $lines.Add("ZIP: $ZipPath")
    $lines.Add("ZIP size: $ZipLength bytes")
    $lines.Add("ZIP SHA256: $ZipSha256")
    $lines.Add("")

    foreach ($result in $results)
    {
        $lines.Add(("[{0}] {1} - {2}" -f $result.Status, $result.Name, $result.Detail))
    }

    $lines.Add("")
    $lines.Add("Package result: $overallStatus")
    $lines.Add("PASS: $passCount; WARN: $warningCount; FAIL: $failureCount")

    if ($AppendReport -and [System.IO.File]::Exists($Path))
    {
        [System.IO.File]::AppendAllLines(
            $Path,
            $lines,
            (New-Object System.Text.UTF8Encoding($false))
        )
    }
    else
    {
        [System.IO.File]::WriteAllLines(
            $Path,
            $lines,
            (New-Object System.Text.UTF8Encoding($false))
        )
    }
}

$releaseRoot = "unresolved"
$releaseZipPath = "unresolved"
$releaseZipSha256 = "unavailable"
$releaseZipLength = 0
$validationError = $null

try
{
    if ([string]::IsNullOrWhiteSpace($ReportPath))
    {
        $reportDirectory = Join-Path (Split-Path -Parent $PSScriptRoot) "Build\ValidationReports"
        $ReportPath = Join-Path $reportDirectory ("Package-{0}.txt" -f (Get-Date -Format "yyyyMMdd-HHmmss"))
    }
    elseif (![System.IO.Path]::IsPathRooted($ReportPath))
    {
        $ReportPath = Join-Path (Split-Path -Parent $PSScriptRoot) $ReportPath
    }

    $ReportPath = [System.IO.Path]::GetFullPath($ReportPath)

    $releaseRoot = (Resolve-Path -LiteralPath $ReleaseDirectory).Path.TrimEnd([char[]]@("\", "/"))
    $releaseZipPath = (Resolve-Path -LiteralPath $ReleaseZip).Path
    $releaseZipInfo = Get-Item -LiteralPath $releaseZipPath
    $releaseZipLength = $releaseZipInfo.Length
    $releaseZipSha256 = (Get-FileHash -LiteralPath $releaseZipPath -Algorithm SHA256).Hash

    $requiredFiles = @(
        "JasonQuery.exe",
        "Updater.exe",
        "Updater.pdb",
        "IconLibrary.dll",
        "IconLibrary.pdb",
        "SQL.Formatter.dll",
        "Microsoft.SqlServer.TransactSql.ScriptDom.dll",
        "System.Data.SQLite.dll",
        "System.Threading.Tasks.Extensions.dll",
        "SQLite.Interop.dll",
        "zh-Hans\Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll",
        "zh-Hant\Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll"
    )

    $allowedScriptDomResources = @(
        "zh-Hans/Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll",
        "zh-Hant/Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll"
    )

    Write-Host "Validating Release directory: $releaseRoot"

    foreach ($relativePath in $requiredFiles)
    {
        $fullPath = Join-Path $releaseRoot $relativePath

        if (!([System.IO.File]::Exists($fullPath)))
        {
            throw "Missing required Release file: $fullPath"
        }
    }

    $legacyReleaseFiles = @(
        Get-ChildItem -LiteralPath $releaseRoot -Recurse -File |
            Where-Object { $_.Name -ieq "PoorMansTSqlFormatterLib.dll" }
    )

    if ($legacyReleaseFiles.Count -ne 0)
    {
        throw "Legacy formatter found in Release directory: $($legacyReleaseFiles.FullName -join ', ')"
    }

    Add-ValidationResult "PASS" "Legacy formatter in Release" "PoorMansTSqlFormatterLib.dll is absent."

    $unexpectedReleaseResources = @(
        Get-ChildItem -LiteralPath $releaseRoot -Recurse -File |
            Where-Object { $_.Name -ieq "Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll" } |
            Where-Object {
                $relativePath = Get-RelativeReleasePath $releaseRoot $_.FullName
                $allowedScriptDomResources -notcontains $relativePath
            }
    )

    if ($unexpectedReleaseResources.Count -ne 0)
    {
        throw "Unexpected ScriptDOM resource in Release directory: $($unexpectedReleaseResources.FullName -join ', ')"
    }

    Add-ValidationResult "PASS" "ScriptDOM resources in Release" "Only zh-Hans and zh-Hant resources are present."

    $archive = [System.IO.Compression.ZipFile]::OpenRead($releaseZipPath)

    try
    {
        $fileEntries = @($archive.Entries | Where-Object { ![string]::IsNullOrEmpty($_.Name) })
        $applicationEntries = @(
            $fileEntries | Where-Object {
                (Get-NormalizedZipPath $_.FullName) -imatch "(^|/)JasonQuery[.]exe$"
            }
        )

        if ($applicationEntries.Count -ne 1)
        {
            throw "Expected exactly one JasonQuery.exe in ZIP, but found $($applicationEntries.Count)."
        }

        $applicationPath = Get-NormalizedZipPath $applicationEntries[0].FullName
        $separatorIndex = $applicationPath.LastIndexOf("/")
        $zipRoot = if ($separatorIndex -ge 0) { $applicationPath.Substring(0, $separatorIndex + 1) } else { "" }

        $legacyZipEntries = @(
            $fileEntries | Where-Object {
                (Get-NormalizedZipPath $_.FullName) -imatch "(^|/)PoorMansTSqlFormatterLib[.]dll$"
            }
        )

        if ($legacyZipEntries.Count -ne 0)
        {
            throw "Legacy formatter found in ZIP: $($legacyZipEntries.FullName -join ', ')"
        }

        Add-ValidationResult "PASS" "Legacy formatter in ZIP" "PoorMansTSqlFormatterLib.dll is absent."

        $unexpectedZipResources = @(
            $fileEntries | Where-Object { $_.Name -ieq "Microsoft.SqlServer.TransactSql.ScriptDom.resources.dll" } |
                Where-Object {
                    $entryPath = Get-NormalizedZipPath $_.FullName
                    $relativePath = if ($entryPath.StartsWith($zipRoot, [System.StringComparison]::OrdinalIgnoreCase))
                    {
                        $entryPath.Substring($zipRoot.Length)
                    }
                    else
                    {
                        $entryPath
                    }

                    $allowedScriptDomResources -notcontains $relativePath
                }
        )

        if ($unexpectedZipResources.Count -ne 0)
        {
            throw "Unexpected ScriptDOM resource in ZIP: $($unexpectedZipResources.FullName -join ', ')"
        }

        Add-ValidationResult "PASS" "ScriptDOM resources in ZIP" "Only zh-Hans and zh-Hant resources are present."

        foreach ($relativePath in $requiredFiles)
        {
            $releasePath = Join-Path $releaseRoot $relativePath
            $expectedZipPath = $zipRoot + ($relativePath -replace "\\", "/")
            $zipEntry = Assert-SingleZipEntry $fileEntries $expectedZipPath

            $releaseHash = (Get-FileHash -LiteralPath $releasePath -Algorithm SHA256).Hash
            $zipStream = $zipEntry.Open()

            try
            {
                $zipHash = Get-StreamSha256 $zipStream
            }
            finally
            {
                $zipStream.Dispose()
            }

            if (![string]::Equals($releaseHash, $zipHash, [System.StringComparison]::OrdinalIgnoreCase))
            {
                throw "Release/ZIP hash mismatch: $relativePath"
            }

            Add-ValidationResult "PASS" $relativePath "Present and byte-identical in Release and ZIP."
        }
    }
    finally
    {
        $archive.Dispose()
    }

    Add-ValidationResult "PASS" "Package archive" ("{0} bytes; SHA256 {1}" -f $releaseZipLength, $releaseZipSha256)
}
catch
{
    $validationError = $_
    Add-ValidationResult "FAIL" "Package validator" $_.Exception.Message
}
finally
{
    if (![string]::IsNullOrWhiteSpace($ReportPath))
    {
        try
        {
            Write-PackageValidationReport $ReportPath $releaseRoot $releaseZipPath $releaseZipSha256 $releaseZipLength
            Write-Host ""
            Write-Host "Validation report: $ReportPath"
        }
        catch
        {
            Write-Host "Unable to write the package validation report: $($_.Exception.Message)" -ForegroundColor Red

            if ($null -eq $validationError)
            {
                $validationError = $_
            }
        }
    }
}

if ($null -ne $validationError)
{
    throw $validationError
}

Write-Host ""
Write-Host "Step 3D Release validation passed." -ForegroundColor Green
Write-Host "- IconLibrary.dll and IconLibrary.pdb are present."
Write-Host "- Required formatter and SQLite dependencies are present."
Write-Host "- PoorMansTSqlFormatterLib.dll is absent."
Write-Host "- Only zh-Hans and zh-Hant ScriptDOM resources are present."
Write-Host "- Required files in Release and the publication ZIP are byte-identical."
