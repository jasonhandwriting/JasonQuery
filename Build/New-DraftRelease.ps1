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

    [Parameter(Mandatory = $true)]
    [string]$ExpectedCommitSha
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-GhApiJson
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Endpoint,

        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $output = @(& $script:ghCommand.Source api $Endpoint 2>&1)
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0)
    {
        throw "$Context failed. $($output -join ' ')"
    }

    try
    {
        return (($output -join [Environment]::NewLine) | ConvertFrom-Json)
    }
    catch
    {
        throw "$Context returned invalid JSON. $($_.Exception.Message)"
    }
}

function Resolve-TagCommitSha
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryName,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseTag
    )

    $reference = Invoke-GhApiJson `
        -Endpoint "repos/$RepositoryName/git/ref/tags/$ReleaseTag" `
        -Context "Reading tag '$ReleaseTag'"

    $objectType = [string]$reference.object.type
    $objectSha = [string]$reference.object.sha
    $depth = 0

    while ($objectType -eq "tag")
    {
        $depth++

        if ($depth -gt 5)
        {
            throw "Tag '$ReleaseTag' has an unexpectedly deep annotated-tag chain."
        }

        $tagObject = Invoke-GhApiJson `
            -Endpoint "repos/$RepositoryName/git/tags/$objectSha" `
            -Context "Resolving annotated tag '$ReleaseTag'"

        $objectType = [string]$tagObject.object.type
        $objectSha = [string]$tagObject.object.sha
    }

    if ($objectType -ne "commit" -or $objectSha -notmatch '^[0-9a-fA-F]{40}$')
    {
        throw "Tag '$ReleaseTag' does not resolve to a Git commit."
    }

    return $objectSha.ToLowerInvariant()
}

if ($TagName -notmatch '^v(\d+)[.](\d+)[.](\d+)$')
{
    throw "TagName must use exactly v<major.minor.build>, for example v0.97.0 or v0.97.1."
}

$major = [int]$Matches[1]
$minor = [int]$Matches[2]
$build = [int]$Matches[3]
$versionText = "$major.$minor.$build"
$channel = if ($build -eq 0) { "Production" } else { "Test" }
$expectedPackageName = if ($channel -eq "Production") { "JasonQuery64.zip" } else { "JasonQuery64Test.zip" }
$releaseTitle = if ($channel -eq "Production")
{
    "JasonQuery v$major.$minor"
}
else
{
    "JasonQuery v$versionText Test"
}

$expectedCommit = $ExpectedCommitSha.Trim().ToLowerInvariant()

if ($expectedCommit -notmatch '^[0-9a-f]{40}$')
{
    throw "ExpectedCommitSha must contain exactly 40 hexadecimal characters."
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

$packageFileName = [System.IO.Path]::GetFileName($packageUri.AbsolutePath)

if (![string]::Equals($packageFileName, $expectedPackageName, [System.StringComparison]::Ordinal))
{
    throw "$channel tag $TagName requires package URL ending in $expectedPackageName. Actual file name: $packageFileName"
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

$script:ghCommand = $ghCommand

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN))
{
    throw "GH_TOKEN is not configured."
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("JasonQuery-DraftRelease-{0}" -f [Guid]::NewGuid().ToString("N"))
$packagePath = Join-Path $temporaryRoot $expectedPackageName
[System.IO.Directory]::CreateDirectory($temporaryRoot) | Out-Null

try
{
    Write-Host "Checking GitHub repository access..."
    [void](Invoke-GhApiJson -Endpoint "repos/$Repository" -Context "Repository access check")

    $tagCommitSha = Resolve-TagCommitSha -RepositoryName $Repository -ReleaseTag $TagName

    if (![string]::Equals($tagCommitSha, $expectedCommit, [System.StringComparison]::Ordinal))
    {
        throw "Tag '$TagName' resolves to $tagCommitSha, not the qualified commit $expectedCommit. The tag was not changed."
    }

    Write-Host "Verified existing tag: $TagName -> $tagCommitSha" -ForegroundColor Green
    Write-Host "Derived channel: $channel"
    Write-Host "Expected asset: $expectedPackageName"
    Write-Host "Release title: $releaseTitle"

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

    Write-Host "Downloading the already validated $channel package..."
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

    $releaseArguments = @(
        "release"
        "create"
        $TagName
        $packagePath
        "--repo"
        $Repository
        "--verify-tag"
        "--title"
        $releaseTitle
        "--generate-notes"
        "--draft"
    )

    if ($channel -eq "Test")
    {
        $releaseArguments += "--prerelease"
    }

    Write-Host "Creating a draft release from the verified existing tag. This workflow never publishes the release..."
    $createOutput = @(& $ghCommand.Source @releaseArguments 2>&1)
    $createExitCode = $LASTEXITCODE
    $createOutput | ForEach-Object { Write-Host $_ }

    if ($createExitCode -ne 0)
    {
        throw "GitHub CLI failed to create the draft release (exit code $createExitCode)."
    }

    Write-Host ""
    Write-Host "Draft release created. Review its notes, tag, asset, title, and prerelease state before publishing it manually." -ForegroundColor Green
}
finally
{
    if ([System.IO.Directory]::Exists($temporaryRoot))
    {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}

$global:LASTEXITCODE = 0
