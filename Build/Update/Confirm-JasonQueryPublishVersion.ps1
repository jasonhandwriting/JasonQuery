[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Production", "Test")]
    [string]$Channel,

    [string]$ApplicationPath,

    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-NormalizedPublishVersion
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    if ($Value -notmatch '^\s*v?(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?\s*$')
    {
        throw "The version '$Value' must use major.minor.build or major.minor.build.0 format."
    }

    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $build = [int]$Matches[3]
    $revision = if ($Matches.ContainsKey(4) -and -not [string]::IsNullOrWhiteSpace($Matches[4]))
    {
        [int]$Matches[4]
    }
    else
    {
        0
    }

    if ($revision -ne 0)
    {
        throw "The fourth version component must be zero. JasonQuery publishing uses major.minor.build."
    }

    $text = "$major.$minor.$build"
    $resolvedChannel = if ($build -eq 0) { "Production" } else { "Test" }

    return [pscustomobject]@{
        Text = $text
        Channel = $resolvedChannel
    }
}

if ([string]::IsNullOrWhiteSpace($ApplicationPath) -eq [string]::IsNullOrWhiteSpace($Version))
{
    throw "Specify exactly one of ApplicationPath or Version."
}

$resolvedVersionText = $Version

if (-not [string]::IsNullOrWhiteSpace($ApplicationPath))
{
    $fullPath = [System.IO.Path]::GetFullPath($ApplicationPath)

    if (-not [System.IO.File]::Exists($fullPath))
    {
        throw "JasonQuery application was not found: $fullPath"
    }

    $resolvedVersionText = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fullPath).FileVersion
}

$versionInfo = Get-NormalizedPublishVersion -Value $resolvedVersionText

if ($versionInfo.Channel -ne $Channel)
{
    throw "Version $($versionInfo.Text) belongs to the $($versionInfo.Channel) channel, not $Channel."
}

Write-Host ("[PASS] Publish version preflight - {0} is a valid {1} version." -f $versionInfo.Text, $Channel) -ForegroundColor Green
$global:LASTEXITCODE = 0
