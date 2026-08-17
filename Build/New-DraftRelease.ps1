[CmdletBinding()]
param
(
    [Parameter(Mandatory = $true)]
    [string]$TagName,

    [Parameter(Mandatory = $true)]
    [string]$PackageUrl,

    [Parameter(Mandatory = $true)]
    [string]$PackageSha256,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [string]$TargetCommitish = "main",

    [switch]$Prerelease
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($TagName -notmatch '^v[0-9]+[.][0-9]+(?:[.][0-9]+)?(?:-[0-9A-Za-z.-]+)?$')
{
    throw "TagName must look like v0.94.0 or v0.94.0-preview.1."
}

if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')
{
    throw "Repository must use the owner/name format."
}

$packageUri = $null

if (![System.Uri]::TryCreate($PackageUrl, [System.UriKind]::Absolute, [ref]$packageUri) -or
    ![string]::Equals($packageUri.Scheme, "https", [System.StringComparison]::OrdinalIgnoreCase))
{
    throw "PackageUrl must be an absolute HTTPS URL."
}

$expectedSha256 = $PackageSha256.Trim().ToUpperInvariant()

if ($expectedSha256 -notmatch '^[0-9A-F]{64}$')
{
    throw "PackageSha256 must contain exactly 64 hexadecimal characters."
}

$ghCommand = Get-Command gh -ErrorAction SilentlyContinue

if ($null -eq $ghCommand)
{
    throw "GitHub CLI (gh) was not found in PATH."
}

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN))
{
    throw "GH_TOKEN is not configured."
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("JasonQuery-DraftRelease-{0}" -f [Guid]::NewGuid().ToString("N"))
$packagePath = Join-Path $temporaryRoot "JasonQuery64.zip"
[System.IO.Directory]::CreateDirectory($temporaryRoot) | Out-Null

try
{
    Write-Host "Checking GitHub repository access..."
    $repositoryCheckOutput = @(& $ghCommand.Source api "repos/$Repository" --silent 2>&1)

    if ($LASTEXITCODE -ne 0)
    {
        throw "Unable to access GitHub repository '$Repository'. $($repositoryCheckOutput -join ' ')"
    }

    $previousErrorActionPreference = $ErrorActionPreference

    try
    {
        $ErrorActionPreference = "Continue"
        $existingReleaseOutput = @(& $ghCommand.Source release view $TagName --repo $Repository 2>&1)
        $existingReleaseExitCode = $LASTEXITCODE
    }
    finally
    {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($existingReleaseExitCode -eq 0)
    {
        throw "A GitHub release already exists for tag '$TagName'. No existing release was changed."
    }

    Write-Host "Downloading the already validated official package..."
    Add-Type -AssemblyName System.Net.Http
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.AllowAutoRedirect = $true
    $httpClient = New-Object System.Net.Http.HttpClient($handler)
    $httpClient.Timeout = [TimeSpan]::FromMinutes(5)
    $httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JasonQuery-Draft-Release/1.0")
    $response = $null
    $inputStream = $null
    $outputStream = $null

    try
    {
        $response = $httpClient.GetAsync(
            $packageUri,
            [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead
        ).GetAwaiter().GetResult()

        if (!$response.IsSuccessStatusCode)
        {
            throw "Package download returned HTTP $([int]$response.StatusCode) ($($response.ReasonPhrase))."
        }

        $inputStream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $outputStream = New-Object System.IO.FileStream(
            $packagePath,
            [System.IO.FileMode]::CreateNew,
            [System.IO.FileAccess]::Write,
            [System.IO.FileShare]::None
        )
        $inputStream.CopyTo($outputStream)
    }
    finally
    {
        if ($null -ne $outputStream)
        {
            $outputStream.Dispose()
        }

        if ($null -ne $inputStream)
        {
            $inputStream.Dispose()
        }

        if ($null -ne $response)
        {
            $response.Dispose()
        }

        if ($null -ne $httpClient)
        {
            $httpClient.Dispose()
        }
    }

    $packageInfo = Get-Item -LiteralPath $packagePath

    if ($packageInfo.Length -lt 4)
    {
        throw "The downloaded package is too small to be a ZIP file."
    }

    $signature = New-Object byte[] 4
    $signatureStream = [System.IO.File]::OpenRead($packagePath)

    try
    {
        [void]$signatureStream.Read($signature, 0, $signature.Length)
    }
    finally
    {
        $signatureStream.Dispose()
    }

    if ($signature[0] -ne 0x50 -or $signature[1] -ne 0x4B)
    {
        throw "The downloaded package does not begin with a ZIP PK signature."
    }

    $actualSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToUpperInvariant()

    if (![string]::Equals($actualSha256, $expectedSha256, [System.StringComparison]::Ordinal))
    {
        throw "Package SHA-256 mismatch. Expected $expectedSha256 but downloaded $actualSha256."
    }

    Write-Host "Package SHA-256 verified: $actualSha256" -ForegroundColor Green

    $releaseTitle = "JasonQuery " + $TagName.Substring(1)
    $assetArgument = $packagePath
    $releaseArguments = @(
        "release"
        "create"
        $TagName
        $assetArgument
        "--repo"
        $Repository
        "--target"
        $TargetCommitish
        "--title"
        $releaseTitle
        "--generate-notes"
        "--draft"
    )

    if ($Prerelease)
    {
        $releaseArguments += "--prerelease"
    }

    Write-Host "Creating a draft release. This workflow never publishes the release..."
    $createOutput = @(& $ghCommand.Source @releaseArguments 2>&1)
    $createExitCode = $LASTEXITCODE
    $createOutput | ForEach-Object { Write-Host $_ }

    if ($createExitCode -ne 0)
    {
        throw "GitHub CLI failed to create the draft release (exit code $createExitCode)."
    }

    Write-Host ""
    Write-Host "Draft release created. Review its notes, tag, and asset before publishing it manually." -ForegroundColor Green
}
finally
{
    if ([System.IO.Directory]::Exists($temporaryRoot))
    {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}

$global:LASTEXITCODE = 0
