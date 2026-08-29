[CmdletBinding()]
param
(
    [string]$WebsiteUrl = "https://jasonquery.org/",

    [string]$MetadataUrl = "https://jasonquery.org/JasonQueryUpdate/jasonquery-update.json",

    [string]$ReportPath = "Build/ValidationReports/Public-Endpoint-Monitor.txt",

    [ValidateRange(1, 365)]
    [int]$MinimumCertificateDays = 21,

    [ValidateRange(5, 120)]
    [int]$TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Net.Http

$results = New-Object System.Collections.Generic.List[object]
$startedAt = Get-Date
$httpClient = $null

function Add-MonitorResult
{
    param
    (
        [ValidateSet("PASS", "FAIL", "SKIP")]
        [string]$Status,
        [string]$Name,
        [string]$Detail
    )

    $results.Add([PSCustomObject]@{
        Status = $Status
        Name = $Name
        Detail = $Detail
    })

    $color = if ($Status -eq "PASS")
    {
        "Green"
    }
    elseif ($Status -eq "SKIP")
    {
        "Yellow"
    }
    else
    {
        "Red"
    }
    Write-Host ("[{0}] {1} - {2}" -f $Status, $Name, $Detail) -ForegroundColor $color
}

function Assert-HttpsUri
{
    param
    (
        [string]$Value,
        [string]$Name
    )

    $uri = $null

    if (![System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$uri))
    {
        throw "$Name is not an absolute URI: $Value"
    }

    if (![string]::Equals($uri.Scheme, "https", [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "$Name must use HTTPS: $Value"
    }

    return $uri
}

function Test-TlsCertificate
{
    param([System.Uri]$Uri)

    # The first connection accepts the certificate only long enough to read its
    # identity and validity dates. No HTTP content is requested through it.
    # A second connection below performs normal Windows/.NET trust and hostname
    # validation before any public content is downloaded.
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $sslStream = $null
    $certificate = $null
    $validationTcpClient = $null
    $validationSslStream = $null

    try
    {
        $asyncResult = $tcpClient.BeginConnect($Uri.DnsSafeHost, 443, $null, $null)

        if (!$asyncResult.AsyncWaitHandle.WaitOne([TimeSpan]::FromSeconds($TimeoutSeconds)))
        {
            throw "Timed out while connecting to $($Uri.DnsSafeHost):443."
        }

        $tcpClient.EndConnect($asyncResult)
        $diagnosticCallback = [System.Net.Security.RemoteCertificateValidationCallback]{
            param($Sender, $RemoteCertificate, $Chain, $SslPolicyErrors)
            return $true
        }
        $sslStream = New-Object System.Net.Security.SslStream(
            $tcpClient.GetStream(),
            $false,
            $diagnosticCallback
        )
        $sslStream.ReadTimeout = $TimeoutSeconds * 1000
        $sslStream.WriteTimeout = $TimeoutSeconds * 1000
        $sslStream.AuthenticateAsClient($Uri.DnsSafeHost)

        if ($null -eq $sslStream.RemoteCertificate)
        {
            throw "The server did not provide a TLS certificate."
        }

        $certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($sslStream.RemoteCertificate)
        $remainingDays = [Math]::Floor(($certificate.NotAfter.ToUniversalTime() - [DateTime]::UtcNow).TotalDays)
        $certificateIdentity = "Subject=$($certificate.Subject); Issuer=$($certificate.Issuer)"

        if ($remainingDays -lt 0)
        {
            throw "TLS certificate expired on $($certificate.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')) UTC ($([Math]::Abs($remainingDays)) day(s) ago). $certificateIdentity"
        }

        if ($remainingDays -lt $MinimumCertificateDays)
        {
            throw "TLS certificate expires on $($certificate.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')) UTC ($remainingDays day(s) remaining). $certificateIdentity"
        }

        $validationTcpClient = New-Object System.Net.Sockets.TcpClient
        $validationAsyncResult = $validationTcpClient.BeginConnect(
            $Uri.DnsSafeHost,
            443,
            $null,
            $null
        )

        if (!$validationAsyncResult.AsyncWaitHandle.WaitOne([TimeSpan]::FromSeconds($TimeoutSeconds)))
        {
            throw "Timed out while validating TLS for $($Uri.DnsSafeHost):443."
        }

        $validationTcpClient.EndConnect($validationAsyncResult)
        $validationSslStream = New-Object System.Net.Security.SslStream(
            $validationTcpClient.GetStream(),
            $false
        )
        $validationSslStream.ReadTimeout = $TimeoutSeconds * 1000
        $validationSslStream.WriteTimeout = $TimeoutSeconds * 1000

        try
        {
            $validationSslStream.AuthenticateAsClient($Uri.DnsSafeHost)
        }
        catch
        {
            throw "TLS trust or hostname validation failed for $($Uri.DnsSafeHost). $certificateIdentity Error: $($_.Exception.Message)"
        }

        Add-MonitorResult "PASS" "TLS certificate" "$($Uri.DnsSafeHost); expires $($certificate.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd')); $remainingDays day(s) remaining; $certificateIdentity."
    }
    finally
    {
        if ($null -ne $validationSslStream)
        {
            $validationSslStream.Dispose()
        }

        if ($null -ne $validationTcpClient)
        {
            $validationTcpClient.Dispose()
        }

        if ($null -ne $certificate)
        {
            $certificate.Dispose()
        }

        if ($null -ne $sslStream)
        {
            $sslStream.Dispose()
        }

        $tcpClient.Dispose()
    }
}

function Get-HttpText
{
    param
    (
        [System.Uri]$Uri,
        [string]$Name
    )

    $response = $httpClient.GetAsync(
        $Uri,
        [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead
    ).GetAwaiter().GetResult()

    try
    {
        if (!$response.IsSuccessStatusCode)
        {
            throw "$Name returned HTTP $([int]$response.StatusCode) ($($response.ReasonPhrase))."
        }

        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

        if ([string]::IsNullOrWhiteSpace($text))
        {
            throw "$Name returned an empty response."
        }

        Add-MonitorResult "PASS" $Name "HTTP $([int]$response.StatusCode); $($text.Length) character(s)."
        return $text
    }
    finally
    {
        $response.Dispose()
    }
}

function Test-ZipEndpoint
{
    param
    (
        [System.Uri]$Uri,
        [string]$AssetName
    )

    $response = $httpClient.GetAsync(
        $Uri,
        [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead
    ).GetAwaiter().GetResult()

    $stream = $null

    try
    {
        if (!$response.IsSuccessStatusCode)
        {
            throw "$AssetName returned HTTP $([int]$response.StatusCode) ($($response.ReasonPhrase))."
        }

        if ($response.Content.Headers.ContentLength.HasValue -and $response.Content.Headers.ContentLength.Value -le 0)
        {
            throw "$AssetName returned an empty package."
        }

        $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $signature = New-Object byte[] 4
        $bytesRead = $stream.Read($signature, 0, $signature.Length)

        if ($bytesRead -lt 4 -or $signature[0] -ne 0x50 -or $signature[1] -ne 0x4B)
        {
            throw "$AssetName does not begin with a ZIP PK signature."
        }

        $contentLength = if ($response.Content.Headers.ContentLength.HasValue)
        {
            "$($response.Content.Headers.ContentLength.Value) byte(s)"
        }
        else
        {
            "content length not supplied"
        }

        Add-MonitorResult "PASS" "ZIP package: $AssetName" "HTTP $([int]$response.StatusCode); $contentLength; PK signature present."
    }
    finally
    {
        if ($null -ne $stream)
        {
            $stream.Dispose()
        }

        $response.Dispose()
    }
}

function Write-MonitorReport
{
    param([string]$Path)

    $fullPath = if ([System.IO.Path]::IsPathRooted($Path))
    {
        [System.IO.Path]::GetFullPath($Path)
    }
    else
    {
        [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
    }

    $directory = Split-Path -Parent $fullPath

    if (![System.IO.Directory]::Exists($directory))
    {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    $passCount = @($results | Where-Object { $_.Status -eq "PASS" }).Count
    $failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count
    $skipCount = @($results | Where-Object { $_.Status -eq "SKIP" }).Count
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("JasonQuery Public Endpoint Monitor")
    $lines.Add("==================================")
    $lines.Add("Started: $($startedAt.ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Finished: $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("")

    foreach ($result in $results)
    {
        $lines.Add(("[{0}] {1} - {2}" -f $result.Status, $result.Name, $result.Detail))
    }

    $lines.Add("")
    $lines.Add("Result: $(if ($failureCount -eq 0) { 'PASS' } else { 'FAIL' })")
    $lines.Add("PASS: $passCount; FAIL: $failureCount; SKIP: $skipCount")

    [System.IO.File]::WriteAllLines(
        $fullPath,
        $lines,
        (New-Object System.Text.UTF8Encoding($false))
    )

    Write-Host ""
    Write-Host "Monitoring report: $fullPath"
}

$websiteUri = Assert-HttpsUri $WebsiteUrl "WebsiteUrl"
$metadataUri = Assert-HttpsUri $MetadataUrl "MetadataUrl"
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.AllowAutoRedirect = $true
$httpClient = New-Object System.Net.Http.HttpClient($handler)
$httpClient.Timeout = [TimeSpan]::FromSeconds($TimeoutSeconds)
$httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JasonQuery-Endpoint-Monitor/1.0")

try
{
    $tlsIsValid = $false

    try
    {
        Test-TlsCertificate $websiteUri
        $tlsIsValid = $true
    }
    catch
    {
        Add-MonitorResult "FAIL" "TLS certificate" $_.Exception.Message
    }

    $metadataText = $null

    if ($tlsIsValid)
    {
        try
        {
            [void](Get-HttpText $websiteUri "Official website")
        }
        catch
        {
            Add-MonitorResult "FAIL" "Official website" $_.Exception.Message
        }

        try
        {
            $metadataText = Get-HttpText $metadataUri "Update metadata"
        }
        catch
        {
            Add-MonitorResult "FAIL" "Update metadata" $_.Exception.Message
        }
    }
    else
    {
        Add-MonitorResult "SKIP" "Official website" "Skipped because strict TLS validation failed."
        Add-MonitorResult "SKIP" "Update metadata and ZIP packages" "Skipped because strict TLS validation failed."
    }

    if ($null -ne $metadataText)
    {
        try
        {
            $metadata = $metadataText | ConvertFrom-Json
            $releases = @($metadata.releases)

            if ($metadata.schema_version -ne 1)
            {
                throw "Expected schema_version 1 but found '$($metadata.schema_version)'."
            }

            if (![string]::Equals($metadata.product, "JasonQuery", [System.StringComparison]::Ordinal))
            {
                throw "Expected product JasonQuery but found '$($metadata.product)'."
            }

            if ($releases.Count -eq 0)
            {
                throw "The releases array is empty."
            }

            Add-MonitorResult "PASS" "Update metadata schema" "schema_version=1; product=JasonQuery; $($releases.Count) release entry/entries."

            $zipAssets = @(
                $releases |
                    Where-Object { $_.draft -ne $true } |
                    ForEach-Object { @($_.assets) } |
                    Where-Object {
                        $_.state -eq "uploaded" -and
                        $_.name -like "*.zip" -and
                        ![string]::IsNullOrWhiteSpace($_.browser_download_url)
                    }
            )

            if ($zipAssets.Count -eq 0)
            {
                throw "No uploaded ZIP asset URL was found in a non-draft release."
            }

            foreach ($asset in $zipAssets)
            {
                try
                {
                    $assetUri = Assert-HttpsUri $asset.browser_download_url "Asset URL"
                    Test-ZipEndpoint $assetUri $asset.name
                }
                catch
                {
                    Add-MonitorResult "FAIL" "ZIP package: $($asset.name)" $_.Exception.Message
                }
            }
        }
        catch
        {
            Add-MonitorResult "FAIL" "Update metadata schema" $_.Exception.Message
        }
    }
}
finally
{
    if ($null -ne $httpClient)
    {
        $httpClient.Dispose()
    }

    Write-MonitorReport $ReportPath
}

$failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count

if ($failureCount -ne 0)
{
    throw "JasonQuery public endpoint monitoring failed with $failureCount failed check(s)."
}

Write-Host ""
Write-Host "JasonQuery public endpoint monitoring passed." -ForegroundColor Green

$global:LASTEXITCODE = 0
