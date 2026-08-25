[CmdletBinding()]
param
(
    [string]$RepositoryRoot,
    [switch]$Apply,
    [switch]$Check,
    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Apply -and $Check)
{
    throw "-Apply and -Check cannot be used together."
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot))
{
    if ([string]::IsNullOrWhiteSpace($PSScriptRoot))
    {
        throw "RepositoryRoot is required when PSScriptRoot is unavailable."
    }

    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}

$repositoryRootPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
$gitDirectoryPath = Join-Path $repositoryRootPath ".git"

if (![System.IO.Directory]::Exists($gitDirectoryPath))
{
    throw "Git repository not found: $repositoryRootPath"
}

$textExtensions = @(
    ".cs",
    ".csproj",
    ".sln",
    ".props",
    ".targets",
    ".config",
    ".xml",
    ".md",
    ".txt",
    ".ps1",
    ".bat",
    ".cmd",
    ".yml",
    ".yaml",
    ".json",
    ".sh",
    ".sql",
    ".example"
)

$specialTextNames = @(
    ".gitattributes",
    ".gitignore",
    ".editorconfig"
)

function Test-IsCanonicalRepositoryTextFile
{
    param([string]$RelativePath)

    $normalizedPath = $RelativePath.Replace("\", "/")
    $fileName = [System.IO.Path]::GetFileName($normalizedPath)
    $extension = [System.IO.Path]::GetExtension($fileName)

    # Preserve third-party source trees exactly as received upstream.
    if ($normalizedPath -imatch "^(MagicLibrary|ScintillaNET)/")
    {
        return $false
    }

    if ($normalizedPath -imatch "^JasonLibrary/UI/Controls/HexBox/")
    {
        return $false
    }

    # Preserve files maintained by WinForms and resource generators.
    if ($normalizedPath -imatch "\.Designer\.cs$")
    {
        return $false
    }

    if ($normalizedPath -imatch "\.(resx|settings|licx)$")
    {
        return $false
    }

    return `
        ($specialTextNames -icontains $fileName) -or
        ($textExtensions -icontains $extension)
}

function Get-UInt16Value
{
    param
    (
        [byte[]]$Bytes,
        [int]$Index,
        [bool]$BigEndian
    )

    if ($BigEndian)
    {
        return ([int]$Bytes[$Index] -shl 8) -bor [int]$Bytes[$Index + 1]
    }

    return [int]$Bytes[$Index] -bor ([int]$Bytes[$Index + 1] -shl 8)
}

function Get-SingleByteNewline
{
    param
    (
        [byte[]]$Bytes,
        [int]$Offset,
        [string]$RelativePath
    )

    for ($index = $Offset; $index -lt $Bytes.Length; $index++)
    {
        if ($Bytes[$index] -eq 0x0D)
        {
            return ,([byte[]]@(0x0D, 0x0A))
        }

        if ($Bytes[$index] -eq 0x0A)
        {
            return ,([byte[]]@(0x0A))
        }
    }

    if ([System.IO.Path]::GetExtension($RelativePath) -ieq ".sh")
    {
        return ,([byte[]]@(0x0A))
    }

    return ,([byte[]]@(0x0D, 0x0A))
}

function Get-Utf16Newline
{
    param
    (
        [byte[]]$Bytes,
        [int]$Offset,
        [bool]$BigEndian
    )

    for ($index = $Offset; $index -le $Bytes.Length - 2; $index += 2)
    {
        $value = Get-UInt16Value $Bytes $index $BigEndian

        if ($value -eq 0x000D)
        {
            if ($BigEndian)
            {
                return ,([byte[]]@(0x00, 0x0D, 0x00, 0x0A))
            }

            return ,([byte[]]@(0x0D, 0x00, 0x0A, 0x00))
        }

        if ($value -eq 0x000A)
        {
            if ($BigEndian)
            {
                return ,([byte[]]@(0x00, 0x0A))
            }

            return ,([byte[]]@(0x0A, 0x00))
        }
    }

    if ($BigEndian)
    {
        return ,([byte[]]@(0x00, 0x0D, 0x00, 0x0A))
    }

    return ,([byte[]]@(0x0D, 0x00, 0x0A, 0x00))
}

function Test-ByteRangeEquals
{
    param
    (
        [byte[]]$Bytes,
        [int]$Offset,
        [byte[]]$Expected
    )

    if ($Bytes.Length - $Offset -ne $Expected.Length)
    {
        return $false
    }

    for ($index = 0; $index -lt $Expected.Length; $index++)
    {
        if ($Bytes[$Offset + $index] -ne $Expected[$index])
        {
            return $false
        }
    }

    return $true
}

function Get-FinalNewlinePlan
{
    param
    (
        [string]$FullPath,
        [string]$RelativePath
    )

    $bytes = [System.IO.File]::ReadAllBytes($FullPath)

    if ($bytes.Length -eq 0)
    {
        return [pscustomobject]@{
            State = "Empty"
            Bytes = $bytes
            ContentLength = 0
            Newline = [byte[]]@()
        }
    }

    if (
        $bytes.Length -ge 4 -and
        (
            (
                $bytes[0] -eq 0xFF -and
                $bytes[1] -eq 0xFE -and
                $bytes[2] -eq 0x00 -and
                $bytes[3] -eq 0x00
            ) -or
            (
                $bytes[0] -eq 0x00 -and
                $bytes[1] -eq 0x00 -and
                $bytes[2] -eq 0xFE -and
                $bytes[3] -eq 0xFF
            )
        )
    )
    {
        return [pscustomobject]@{
            State = "UnsupportedUtf32"
            Bytes = $bytes
            ContentLength = $bytes.Length
            Newline = [byte[]]@()
        }
    }

    $isUtf16LittleEndian = `
        $bytes.Length -ge 2 -and
        $bytes[0] -eq 0xFF -and
        $bytes[1] -eq 0xFE
    $isUtf16BigEndian = `
        $bytes.Length -ge 2 -and
        $bytes[0] -eq 0xFE -and
        $bytes[1] -eq 0xFF

    if ($isUtf16LittleEndian -or $isUtf16BigEndian)
    {
        $offset = 2

        if ($bytes.Length -eq $offset)
        {
            return [pscustomobject]@{
                State = "Empty"
                Bytes = $bytes
                ContentLength = $bytes.Length
                Newline = [byte[]]@()
            }
        }

        if (($bytes.Length - $offset) % 2 -ne 0)
        {
            return [pscustomobject]@{
                State = "InvalidUtf16Length"
                Bytes = $bytes
                ContentLength = $bytes.Length
                Newline = [byte[]]@()
            }
        }

        $newline = Get-Utf16Newline $bytes $offset $isUtf16BigEndian
        $contentLength = $bytes.Length

        while ($contentLength -ge $offset + 2)
        {
            $value = Get-UInt16Value $bytes ($contentLength - 2) $isUtf16BigEndian

            if ($value -ne 0x000D -and $value -ne 0x000A)
            {
                break
            }

            $contentLength -= 2
        }

        $isValid = Test-ByteRangeEquals $bytes $contentLength $newline

        return [pscustomobject]@{
            State = if ($isValid) { "Valid" } else { "Normalize" }
            Bytes = $bytes
            ContentLength = $contentLength
            Newline = $newline
        }
    }

    $offset = 0

    if (
        $bytes.Length -ge 3 -and
        $bytes[0] -eq 0xEF -and
        $bytes[1] -eq 0xBB -and
        $bytes[2] -eq 0xBF
    )
    {
        $offset = 3
    }

    if ($bytes.Length -eq $offset)
    {
        return [pscustomobject]@{
            State = "Empty"
            Bytes = $bytes
            ContentLength = $bytes.Length
            Newline = [byte[]]@()
        }
    }

    $newline = Get-SingleByteNewline $bytes $offset $RelativePath
    $contentLength = $bytes.Length

    while (
        $contentLength -gt $offset -and
        (
            $bytes[$contentLength - 1] -eq 0x0D -or
            $bytes[$contentLength - 1] -eq 0x0A
        )
    )
    {
        $contentLength--
    }

    $isValid = Test-ByteRangeEquals $bytes $contentLength $newline

    return [pscustomobject]@{
        State = if ($isValid) { "Valid" } else { "Normalize" }
        Bytes = $bytes
        ContentLength = $contentLength
        Newline = $newline
    }
}

function Write-NormalizedFile
{
    param
    (
        [string]$FullPath,
        [object]$Plan
    )

    $normalizedBytes = New-Object byte[] ($Plan.ContentLength + $Plan.Newline.Length)
    [System.Array]::Copy($Plan.Bytes, 0, $normalizedBytes, 0, $Plan.ContentLength)
    [System.Array]::Copy($Plan.Newline, 0, $normalizedBytes, $Plan.ContentLength, $Plan.Newline.Length)
    [System.IO.File]::WriteAllBytes($FullPath, $normalizedBytes)
}

function Get-TrackedCanonicalTextFiles
{
    $gitOutput = @(& git -C $repositoryRootPath ls-files)
    $gitExitCode = $LASTEXITCODE

    if ($gitExitCode -ne 0)
    {
        throw "git ls-files failed with exit code $gitExitCode."
    }

    return @(
        $gitOutput |
            Where-Object { Test-IsCanonicalRepositoryTextFile $_ } |
            ForEach-Object { $_.ToString().Replace("\", "/") } |
            Sort-Object
    )
}

$trackedFiles = Get-TrackedCanonicalTextFiles
$results = New-Object System.Collections.Generic.List[object]

foreach ($relativePath in $trackedFiles)
{
    $fullPath = Join-Path $repositoryRootPath $relativePath
    $plan = Get-FinalNewlinePlan $fullPath $relativePath

    $results.Add([pscustomobject]@{
        Path = $relativePath
        FullPath = $fullPath
        Plan = $plan
    })
}

$unsupportedFiles = @(
    $results | Where-Object {
        $_.Plan.State -in @("UnsupportedUtf32", "InvalidUtf16Length")
    }
)
$filesToNormalize = @(
    $results | Where-Object { $_.Plan.State -eq "Normalize" }
)

if ($Apply)
{
    if ($unsupportedFiles.Count -ne 0)
    {
        throw "Unsupported canonical text files must be reviewed before applying normalization: $($unsupportedFiles.Path -join ', ')"
    }

    foreach ($item in $filesToNormalize)
    {
        Write-NormalizedFile $item.FullPath $item.Plan

        if (!$Quiet)
        {
            Write-Host "[UPDATED] $($item.Path)"
        }
    }

    $remainingFiles = New-Object System.Collections.Generic.List[string]

    foreach ($item in $filesToNormalize)
    {
        $verificationPlan = Get-FinalNewlinePlan $item.FullPath $item.Path

        if ($verificationPlan.State -ne "Valid")
        {
            $remainingFiles.Add($item.Path)
        }
    }

    if ($remainingFiles.Count -ne 0)
    {
        throw "Final-newline verification failed: $($remainingFiles -join ', ')"
    }
}

if (!$Quiet)
{
    Write-Host ""
    Write-Host "Final-newline summary:"
    Write-Host "- Canonical tracked text files: $($trackedFiles.Count)"

    if ($Apply)
    {
        Write-Host "- Files normalized: $($filesToNormalize.Count)"
    }
    else
    {
        Write-Host "- Files requiring normalization: $($filesToNormalize.Count)"
    }

    Write-Host "- Unsupported files: $($unsupportedFiles.Count)"

    if (!$Apply -and $filesToNormalize.Count -ne 0)
    {
        Write-Host ""

        $filesToNormalize |
            ForEach-Object { Write-Host "[NORMALIZE] $($_.Path)" }
    }

    if ($unsupportedFiles.Count -ne 0)
    {
        Write-Host ""

        $unsupportedFiles |
            ForEach-Object { Write-Host "[$($_.Plan.State)] $($_.Path)" }
    }
}

if ($Check -and ($filesToNormalize.Count -ne 0 -or $unsupportedFiles.Count -ne 0))
{
    $problemFiles = @(
        @($filesToNormalize + $unsupportedFiles) |
            ForEach-Object { $_.Path } |
            Select-Object -First 10
    )
    $moreCount = $filesToNormalize.Count + $unsupportedFiles.Count - $problemFiles.Count
    $detail = $problemFiles -join ", "

    if ($moreCount -gt 0)
    {
        $detail += ", and $moreCount more"
    }

    throw "Canonical tracked text files do not end with exactly one newline: $detail"
}

$global:LASTEXITCODE = 0
