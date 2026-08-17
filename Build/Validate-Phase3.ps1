[CmdletBinding()]
param
(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$ReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$results = New-Object System.Collections.Generic.List[object]
$startedAt = Get-Date

function Add-Phase3Result
{
    param
    (
        [ValidateSet("PASS", "FAIL")]
        [string]$Status,
        [string]$Name,
        [string]$Detail
    )

    $results.Add([PSCustomObject]@{
        Status = $Status
        Name = $Name
        Detail = $Detail
    })

    $color = if ($Status -eq "PASS") { "Green" } else { "Red" }
    Write-Host ("[{0}] {1} - {2}" -f $Status, $Name, $Detail) -ForegroundColor $color
}

function Test-TextContract
{
    param
    (
        [string]$Name,
        [string]$RelativePath,
        [string[]]$RequiredFragments,
        [string[]]$ForbiddenFragments = @()
    )

    $path = Join-Path $script:repositoryRootPath $RelativePath

    if (![System.IO.File]::Exists($path))
    {
        Add-Phase3Result "FAIL" $Name "Missing file: $RelativePath"
        return
    }

    $text = [System.IO.File]::ReadAllText($path)
    $contractText = [System.Text.RegularExpressions.Regex]::Replace(
        $text,
        '(?<action>[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+)@v[1-9][0-9]*(?![0-9A-Za-z_.-])',
        '${action}@vMAJOR'
    )
    $missingFragments = @(
        $RequiredFragments | Where-Object {
            $contractText.IndexOf($_, [System.StringComparison]::Ordinal) -lt 0
        }
    )
    $foundForbiddenFragments = @(
        $ForbiddenFragments | Where-Object {
            $text.IndexOf($_, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
    )

    if ($missingFragments.Count -eq 0 -and $foundForbiddenFragments.Count -eq 0)
    {
        Add-Phase3Result "PASS" $Name "$RelativePath satisfies the Phase 3 contract."
        return
    }

    $details = New-Object System.Collections.Generic.List[string]

    if ($missingFragments.Count -ne 0)
    {
        $details.Add("Missing: $($missingFragments -join ', ')")
    }

    if ($foundForbiddenFragments.Count -ne 0)
    {
        $details.Add("Forbidden: $($foundForbiddenFragments -join ', ')")
    }

    Add-Phase3Result "FAIL" $Name ($details -join "; ")
}

function Write-Phase3Report
{
    param([string]$Path)

    $directory = Split-Path -Parent $Path

    if (![System.IO.Directory]::Exists($directory))
    {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    $passCount = @($results | Where-Object { $_.Status -eq "PASS" }).Count
    $failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("JasonQuery Phase 3 Validation Report")
    $lines.Add("====================================")
    $lines.Add("Started: $($startedAt.ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Finished: $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Repository: $script:repositoryRootPath")
    $lines.Add("")

    foreach ($result in $results)
    {
        $lines.Add(("[{0}] {1} - {2}" -f $result.Status, $result.Name, $result.Detail))
    }

    $lines.Add("")
    $lines.Add("Phase 3 result: $(if ($failureCount -eq 0) { 'PASS' } else { 'FAIL' })")
    $lines.Add("PASS: $passCount; FAIL: $failureCount")

    [System.IO.File]::WriteAllLines(
        $Path,
        $lines,
        (New-Object System.Text.UTF8Encoding($false))
    )
}

$repositoryRootPath = (Resolve-Path -LiteralPath $RepositoryRoot).Path.TrimEnd([char[]]@("\", "/"))

if ([string]::IsNullOrWhiteSpace($ReportPath))
{
    $ReportPath = Join-Path $repositoryRootPath ("Build\ValidationReports\Phase3-{0}.txt" -f (Get-Date -Format "yyyyMMdd-HHmmss"))
}
elseif (![System.IO.Path]::IsPathRooted($ReportPath))
{
    $ReportPath = Join-Path $repositoryRootPath $ReportPath
}

$ReportPath = [System.IO.Path]::GetFullPath($ReportPath)

Test-TextContract "Dependabot" ".github\dependabot.yml" @(
    "version: 2",
    'package-ecosystem: "nuget"',
    'package-ecosystem: "github-actions"',
    'interval: "monthly"',
    'timezone: "Asia/Taipei"',
    "open-pull-requests-limit: 2",
    "routine-nuget-updates:",
    "major-nuget-updates:",
    "routine-action-updates:",
    "major-action-updates:"
)

Test-TextContract "Secret Scan workflow" ".github\workflows\secret-scan.yml" @(
    "name: Secret Scan",
    "pull_request:",
    "push:",
    "schedule:",
    "workflow_dispatch:",
    "contents: read",
    "pull-requests: read",
    "uses: actions/checkout@vMAJOR",
    "fetch-depth: 0",
    "uses: gitleaks/gitleaks-action@vMAJOR",
    'GITLEAKS_ENABLE_COMMENTS: "false"',
    'GITLEAKS_ENABLE_UPLOAD_ARTIFACT: "false"'
) @(
    "pull_request_target:",
    "contents: write"
)

Test-TextContract "Public Endpoint Monitor workflow" ".github\workflows\public-endpoint-monitor.yml" @(
    "name: Public Endpoint Monitor",
    "schedule:",
    "workflow_dispatch:",
    "contents: read",
    "vars.PUBLIC_ENDPOINT_MONITOR_ENABLED == 'true'",
    "Build/Test-PublicEndpoints.ps1",
    "MinimumCertificateDays 21",
    "uses: actions/upload-artifact@vMAJOR"
) @(
    "pull_request_target:",
    "pull_request:",
    "push:",
    "contents: write",
    "github.repository == 'jasonhandwriting/JasonQuery'",
    "secrets."
)

Test-TextContract "Draft Release workflow" ".github\workflows\draft-release.yml" @(
    "name: Draft Release",
    "workflow_dispatch:",
    "package_sha256:",
    "confirm_draft:",
    "contents: write",
    "inputs.confirm_draft == true",
    "Build\New-DraftRelease.ps1",
    "secrets.GITHUB_TOKEN"
) @(
    "pull_request_target:",
    "pull_request:",
    "push:",
    "schedule:"
)

Test-TextContract "Public endpoint monitor" "Build\Test-PublicEndpoints.ps1" @(
    "https://www.jasonquery.org/",
    "jasonquery-update.json",
    "MinimumCertificateDays",
    "TLS certificate expired on",
    'ValidateSet("PASS", "FAIL", "SKIP")',
    "Test-TlsCertificate",
    "Test-ZipEndpoint",
    "ConvertFrom-Json",
    "ZIP PK signature"
)

Test-TextContract "Draft release creator" "Build\New-DraftRelease.ps1" @(
    "PackageSha256",
    "Get-FileHash",
    "Package SHA-256 mismatch",
    '"--generate-notes"',
    '"--draft"',
    "This workflow never publishes the release"
)

Test-TextContract "Repository Guard integration" ".github\workflows\repository-guard.yml" @(
    "Validate Phase 3 automation",
    "Build\Validate-Phase3.ps1",
    "Build/ValidationReports/"
)

Test-TextContract "Pull request checklist" ".github\pull_request_template.md" @(
    'GitHub Actions `Repository Guard` passed',
    'GitHub Actions `Secret Scan` passed'
)

Test-TextContract "README Phase 3 documentation" "README.md" @(
    "## Maintainer automation",
    "Dependabot",
    "Secret Scan",
    "Public Endpoint Monitor",
    "Draft Release",
    "PUBLIC_ENDPOINT_MONITOR_ENABLED"
)

Write-Phase3Report $ReportPath
Write-Host ""
Write-Host "Phase 3 validation report: $ReportPath"

$failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count

if ($failureCount -ne 0)
{
    throw "JasonQuery Phase 3 validation failed with $failureCount failed check(s)."
}

Write-Host ""
Write-Host "JasonQuery Phase 3 validation passed." -ForegroundColor Green

$global:LASTEXITCODE = 0
