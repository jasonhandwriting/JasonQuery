[CmdletBinding()]
param
(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [string]$ReportPath,

    [switch]$RequireCleanWorkingTree,

    [string]$ExpectedBranch,

    [switch]$SkipGitleaks
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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

function Invoke-GitCommand
{
    param([string[]]$Arguments)

    # Windows PowerShell 5 turns native stderr records into PowerShell errors.
    # Git can legitimately write an advisory (for example, an autocrlf notice)
    # to stderr while still returning exit code 0. Capture that output without
    # allowing $ErrorActionPreference = "Stop" to abort the entire validator.
    $previousErrorActionPreference = $ErrorActionPreference

    try
    {
        $ErrorActionPreference = "Continue"
        $output = @(& git -C $script:repositoryRootPath @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    }
    finally
    {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    return [PSCustomObject]@{
        ExitCode = $exitCode
        Output = $output
    }
}

function Assert-GitSuccess
{
    param
    (
        [object]$Result,
        [string]$Operation
    )

    if ($Result.ExitCode -ne 0)
    {
        $message = ($Result.Output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        throw "$Operation failed with exit code $($Result.ExitCode). $message"
    }
}

function Test-PathIsIgnored
{
    param([string]$Path)

    $result = Invoke-GitCommand @("check-ignore", "--no-index", "--quiet", "--", $Path)
    return $result.ExitCode -eq 0
}

function Get-EolFilePath
{
    param([object]$Entry)

    $text = $Entry.ToString()
    $separatorIndex = $text.IndexOf([char]9)

    if ($separatorIndex -lt 0)
    {
        return $text
    }

    return $text.Substring($separatorIndex + 1).Replace("\", "/")
}

function Write-ValidationReport
{
    param
    (
        [string]$Path,
        [string]$Branch,
        [string]$Commit,
        [string]$WorkingTreeState
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
    $lines.Add("JasonQuery Validation Report")
    $lines.Add("============================")
    $lines.Add("Section: Repository validation")
    $lines.Add("Started: $($validationStartedAt.ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Finished: $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    $lines.Add("Repository: $script:repositoryRootPath")
    $lines.Add("Branch: $Branch")
    $lines.Add("Commit: $Commit")
    $lines.Add("Working tree: $WorkingTreeState")
    $lines.Add("")

    foreach ($result in $results)
    {
        $lines.Add(("[{0}] {1} - {2}" -f $result.Status, $result.Name, $result.Detail))
    }

    $lines.Add("")
    $lines.Add("Repository result: $overallStatus")
    $lines.Add("PASS: $passCount; WARN: $warningCount; FAIL: $failureCount")

    [System.IO.File]::WriteAllLines(
        $Path,
        $lines,
        (New-Object System.Text.UTF8Encoding($false))
    )
}

$repositoryRootPath = ""
$resolvedReportPath = ""
$branchName = "unknown"
$commitSha = "unknown"
$workingTreeState = "unknown"
$fatalError = $null

try
{
    $repositoryRootPath = (Resolve-Path -LiteralPath $RepositoryRoot).Path.TrimEnd([char[]]@("\", "/"))

    if ([string]::IsNullOrWhiteSpace($ReportPath))
    {
        $reportDirectory = Join-Path $repositoryRootPath "Build\ValidationReports"
        $ReportPath = Join-Path $reportDirectory ("Repository-{0}.txt" -f (Get-Date -Format "yyyyMMdd-HHmmss"))
    }
    elseif (![System.IO.Path]::IsPathRooted($ReportPath))
    {
        $ReportPath = Join-Path $repositoryRootPath $ReportPath
    }

    $resolvedReportPath = [System.IO.Path]::GetFullPath($ReportPath)

    if ($null -eq (Get-Command git -ErrorAction SilentlyContinue))
    {
        throw "Git was not found in PATH."
    }

    $insideWorkTree = Invoke-GitCommand @("rev-parse", "--is-inside-work-tree")
    Assert-GitSuccess $insideWorkTree "Resolving the Git work tree"

    if (($insideWorkTree.Output -join "").Trim() -ne "true")
    {
        throw "The selected directory is not a Git work tree."
    }

    Add-ValidationResult "PASS" "Git repository" $repositoryRootPath

    $topLevel = Invoke-GitCommand @("rev-parse", "--show-toplevel")
    Assert-GitSuccess $topLevel "Resolving the repository root"
    $gitTopLevel = [System.IO.Path]::GetFullPath(($topLevel.Output -join "").Trim()).TrimEnd([char[]]@("\", "/"))

    if (![string]::Equals($gitTopLevel, $repositoryRootPath, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "RepositoryRoot must be the Git top-level directory. Git reported: $gitTopLevel"
    }

    $branchResult = Invoke-GitCommand @("branch", "--show-current")
    Assert-GitSuccess $branchResult "Reading the current branch"
    $branchName = ($branchResult.Output -join "").Trim()

    if ([string]::IsNullOrWhiteSpace($branchName))
    {
        $branchName = "detached HEAD"
    }

    $commitResult = Invoke-GitCommand @("rev-parse", "HEAD")
    Assert-GitSuccess $commitResult "Reading HEAD"
    $commitSha = ($commitResult.Output -join "").Trim()

    if (![string]::IsNullOrWhiteSpace($ExpectedBranch))
    {
        if (![string]::Equals($branchName, $ExpectedBranch, [System.StringComparison]::OrdinalIgnoreCase))
        {
            Add-ValidationResult "FAIL" "Expected branch" "Expected '$ExpectedBranch' but found '$branchName'."
        }
        else
        {
            Add-ValidationResult "PASS" "Expected branch" $branchName
        }
    }
    else
    {
        Add-ValidationResult "PASS" "Current branch" $branchName
    }

    $statusResult = Invoke-GitCommand @("status", "--porcelain", "--untracked-files=all")
    Assert-GitSuccess $statusResult "Reading the working tree status"
    $statusLines = @($statusResult.Output | Where-Object { ![string]::IsNullOrWhiteSpace($_.ToString()) })
    $workingTreeState = if ($statusLines.Count -eq 0) { "clean" } else { "has changes ($($statusLines.Count) entries)" }

    if ($RequireCleanWorkingTree -and $statusLines.Count -ne 0)
    {
        Add-ValidationResult "FAIL" "Clean working tree" $workingTreeState
    }
    elseif ($statusLines.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Working tree" "clean"
    }
    else
    {
        Add-ValidationResult "WARN" "Working tree" "$workingTreeState; allowed for pull request validation."
    }

    $requiredRepositoryFiles = @(
        ".gitignore",
        ".editorconfig",
        "README.md",
        "JasonQuery.sln",
        "JasonQuery\JasonQuery.csproj",
        "Publish-JasonQuery-Release.bat",
        "Publish-JasonQuery-Test.bat",
        "Build\Validate-Repository.ps1",
        "Build\Normalize-FinalNewlines.ps1",
        "Build\Validate-Step3D-Release.ps1",
        "Build\Update\New-JasonQueryCompanyUpdatePackage.ps1",
        ".github\pull_request_template.md",
        ".github\workflows\repository-guard.yml"
    )

    $missingRepositoryFiles = @(
        $requiredRepositoryFiles | Where-Object {
            ![System.IO.File]::Exists((Join-Path $repositoryRootPath $_))
        }
    )

    if ($missingRepositoryFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Required repository files" "$($requiredRepositoryFiles.Count) files are present."
    }
    else
    {
        Add-ValidationResult "FAIL" "Required repository files" ("Missing: " + ($missingRepositoryFiles -join ", "))
    }

    $repositoryGuardPath = Join-Path $repositoryRootPath ".github\workflows\repository-guard.yml"

    if ([System.IO.File]::Exists($repositoryGuardPath))
    {
        $repositoryGuardText = [System.IO.File]::ReadAllText($repositoryGuardPath)
        $repositoryGuardContractText = [System.Text.RegularExpressions.Regex]::Replace(
            $repositoryGuardText,
            '(?<action>[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+)@v[1-9][0-9]*(?![0-9A-Za-z_.-])',
            '${action}@vMAJOR'
        )
        $requiredRepositoryGuardFragments = @(
            "name: Repository Guard",
            "pull_request:",
            "push:",
            "workflow_dispatch:",
            "contents: read",
            "runs-on: windows-latest",
            "uses: actions/checkout@vMAJOR",
            "Build\Validate-Repository.ps1",
            "-RequireCleanWorkingTree",
            "-SkipGitleaks",
            "if: always()",
            "uses: actions/upload-artifact@vMAJOR"
        )
        $missingRepositoryGuardFragments = @(
            $requiredRepositoryGuardFragments | Where-Object {
                $repositoryGuardContractText.IndexOf($_, [System.StringComparison]::Ordinal) -lt 0
            }
        )
        $forbiddenRepositoryGuardFragments = @(
            "pull_request_target:",
            "contents: write",
            "pull-requests: write",
            "secrets."
        )
        $foundForbiddenRepositoryGuardFragments = @(
            $forbiddenRepositoryGuardFragments | Where-Object {
                $repositoryGuardText.IndexOf($_, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
            }
        )

        if ($missingRepositoryGuardFragments.Count -eq 0 -and $foundForbiddenRepositoryGuardFragments.Count -eq 0)
        {
            Add-ValidationResult "PASS" "Repository Guard workflow" "PR, main-push, manual triggers, read-only permissions, validator execution, and report upload are configured."
        }
        else
        {
            $repositoryGuardDetail = New-Object System.Collections.Generic.List[string]

            if ($missingRepositoryGuardFragments.Count -ne 0)
            {
                $repositoryGuardDetail.Add("Missing: $($missingRepositoryGuardFragments -join ', ')")
            }

            if ($foundForbiddenRepositoryGuardFragments.Count -ne 0)
            {
                $repositoryGuardDetail.Add("Forbidden: $($foundForbiddenRepositoryGuardFragments -join ', ')")
            }

            Add-ValidationResult "FAIL" "Repository Guard workflow" ($repositoryGuardDetail -join "; ")
        }
    }
    $unreleasedDirectoryName = -join @(
        [char]0x672A,
        [char]0x4E0A,
        [char]0x7DDA
    )

    $trackedResult = Invoke-GitCommand @("ls-files")
    Assert-GitSuccess $trackedResult "Listing tracked files"
    $trackedFiles = @($trackedResult.Output | ForEach-Object { $_.ToString().Replace("\", "/") })

    $firstPartyEditableCSharpFiles = @(
        $trackedFiles | Where-Object {
            $_ -imatch "\.cs$" -and
            $_ -inotmatch "^(MagicLibrary|ScintillaNET)/" -and
            $_ -inotmatch "^JasonLibrary/UI/Controls/HexBox/" -and
            $_ -inotmatch "\.Designer\.cs$"
        }
    )
    $cSharpTabFiles = New-Object System.Collections.Generic.List[string]
    $cSharpTrailingWhitespaceFiles = New-Object System.Collections.Generic.List[string]
    $invalidUtf8CSharpFiles = New-Object System.Collections.Generic.List[string]
    $strictUtf8 = New-Object System.Text.UTF8Encoding($false, $true)

    foreach ($relativePath in $firstPartyEditableCSharpFiles)
    {
        $fullPath = Join-Path $repositoryRootPath $relativePath
        $bytes = [System.IO.File]::ReadAllBytes($fullPath)

        if ($bytes -contains [byte]0x09)
        {
            $cSharpTabFiles.Add($relativePath)
        }

        try
        {
            $text = $strictUtf8.GetString($bytes)
        }
        catch
        {
            $invalidUtf8CSharpFiles.Add($relativePath)
            continue
        }

        if (
            [System.Text.RegularExpressions.Regex]::IsMatch(
                $text,
                "[ `t]+(?=`r?$)",
                [System.Text.RegularExpressions.RegexOptions]::Multiline
            )
        )
        {
            $cSharpTrailingWhitespaceFiles.Add($relativePath)
        }
    }

    if ($cSharpTabFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "C# indentation" "$($firstPartyEditableCSharpFiles.Count) first-party editable C# files contain no tab characters."
    }
    else
    {
        Add-ValidationResult "FAIL" "C# indentation" ("Tab characters found: " + ($cSharpTabFiles -join ", "))
    }

    if ($cSharpTrailingWhitespaceFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "C# trailing whitespace" "$($firstPartyEditableCSharpFiles.Count) first-party editable C# files contain no trailing whitespace."
    }
    else
    {
        Add-ValidationResult "FAIL" "C# trailing whitespace" ("Trailing whitespace found: " + ($cSharpTrailingWhitespaceFiles -join ", "))
    }

    if ($invalidUtf8CSharpFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "C# UTF-8 encoding" "$($firstPartyEditableCSharpFiles.Count) first-party editable C# files are valid UTF-8, with or without BOM."
    }
    else
    {
        Add-ValidationResult "FAIL" "C# UTF-8 encoding" ("Invalid UTF-8 files: " + ($invalidUtf8CSharpFiles -join ", "))
    }

    $trackedPrivateFiles = @(
        $trackedFiles | Where-Object {
            $_ -imatch "^\.local/" -or $_ -imatch "^IconLibrary/"
        }
    )

    $trackedUnreleasedResult = Invoke-GitCommand @(
        "ls-files",
        "--",
        ":(glob)**/$unreleasedDirectoryName/**"
    )
    Assert-GitSuccess $trackedUnreleasedResult "Listing tracked unreleased files"
    $trackedUnreleasedFiles = @(
        $trackedUnreleasedResult.Output |
            ForEach-Object { $_.ToString().Replace("\", "/") }
    )

    $trackedPrivateFiles = @(
        $trackedPrivateFiles + $trackedUnreleasedFiles
    )

    if ($trackedPrivateFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Private dependency guard" "No .local, IconLibrary source, or unreleased private source files are tracked."
    }
    else
    {
        Add-ValidationResult "FAIL" "Private dependency guard" ("Tracked private or unreleased files: " + ($trackedPrivateFiles -join ", "))
    }

    $trackedGeneratedFiles = @(
        $trackedFiles | Where-Object {
            $_ -imatch "(^|/)(bin|obj|TestResults|ValidationReports)/" -or
            $_ -imatch "\.(dll|exe|pdb|zip|7z)$" -or
            $_ -imatch "\.bak$" -or
            $_ -imatch "^JasonQuery[.]db$" -or
            $_ -imatch "^JasonQuery/Files/(newSQLCipher|newSQLite3)[.]db$" -or
            $_ -imatch "^gitleaks-report.*[.]json$"
        }
    )

    if ($trackedGeneratedFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Generated file guard" "No forbidden build output, archive, backup, or local database file is tracked."
    }
    else
    {
        Add-ValidationResult "FAIL" "Generated file guard" ("Tracked forbidden files: " + ($trackedGeneratedFiles -join ", "))
    }

    $ignoreTestPaths = @(
        ".local/IconLibrary/IconLibrary.dll",
        "IconLibrary/IconLibrary.csproj",
        "JasonQuery/$unreleasedDirectoryName/sample.cs",
        "Build/ValidationReports/sample.txt",
        "Nested/WinMerge/OptionsForm.cs.bak",
        "JasonQuery.db",
        "JasonQuery/Files/newSQLCipher.db",
        "JasonQuery/Files/newSQLite3.db"
    )

    $notIgnoredPaths = @($ignoreTestPaths | Where-Object { !(Test-PathIsIgnored $_) })

    if ($notIgnoredPaths.Count -eq 0)
    {
        Add-ValidationResult "PASS" ".gitignore rules" "$($ignoreTestPaths.Count) private and generated path samples are ignored."
    }
    else
    {
        Add-ValidationResult "FAIL" ".gitignore rules" ("Not ignored: " + ($notIgnoredPaths -join ", "))
    }

    $solutionPath = Join-Path $repositoryRootPath "JasonQuery.sln"

    if ([System.IO.File]::Exists($solutionPath))
    {
        $solutionText = [System.IO.File]::ReadAllText($solutionPath)
        $requiredConfigurations = @("Debug|x64 = Debug|x64", "Release|x64 = Release|x64")
        $missingConfigurations = @($requiredConfigurations | Where-Object { !$solutionText.Contains($_) })
        $forbiddenSolutionPatterns = @("Any CPU", "|x86", "Signed", "StrongMagic", "IconLibrary.csproj")
        $foundForbiddenPatterns = @($forbiddenSolutionPatterns | Where-Object { $solutionText.IndexOf($_, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 })

        if ($missingConfigurations.Count -eq 0 -and $foundForbiddenPatterns.Count -eq 0)
        {
            Add-ValidationResult "PASS" "Solution configurations" "Only the supported Debug|x64 and Release|x64 configurations are declared."
        }
        else
        {
            $detailParts = New-Object System.Collections.Generic.List[string]

            if ($missingConfigurations.Count -ne 0)
            {
                $detailParts.Add("Missing: $($missingConfigurations -join ', ')")
            }

            if ($foundForbiddenPatterns.Count -ne 0)
            {
                $detailParts.Add("Forbidden: $($foundForbiddenPatterns -join ', ')")
            }

            Add-ValidationResult "FAIL" "Solution configurations" ($detailParts -join "; ")
        }
    }

    $projectPath = Join-Path $repositoryRootPath "JasonQuery\JasonQuery.csproj"

    if ([System.IO.File]::Exists($projectPath))
    {
        [xml]$projectXml = [System.IO.File]::ReadAllText($projectPath)
        $iconReferences = @(
            $projectXml.SelectNodes("//*[local-name()='Reference']") | Where-Object {
                $include = $_.GetAttribute("Include")
                $include -ieq "IconLibrary" -or $include -ilike "IconLibrary,*"
            }
        )
        $iconProjectReferences = @(
            $projectXml.SelectNodes("//*[local-name()='ProjectReference']") | Where-Object {
                $_.GetAttribute("Include") -imatch "IconLibrary" -or
                ($null -ne $_.SelectSingleNode("*[local-name()='Name']") -and $_.SelectSingleNode("*[local-name()='Name']").InnerText -ieq "IconLibrary")
            }
        )

        if ($iconReferences.Count -ne 1)
        {
            Add-ValidationResult "FAIL" "IconLibrary assembly reference" "Expected exactly one assembly Reference, found $($iconReferences.Count)."
        }
        else
        {
            $hintPathNode = $iconReferences[0].SelectSingleNode("*[local-name()='HintPath']")
            $privateNode = $iconReferences[0].SelectSingleNode("*[local-name()='Private']")
            $hintPath = if ($null -eq $hintPathNode) { "" } else { $hintPathNode.InnerText.Replace("/", "\") }
            $copyLocal = if ($null -eq $privateNode) { "" } else { $privateNode.InnerText }
            $expectedHintPath = "..\.local\IconLibrary\IconLibrary.dll"

            if (![string]::Equals($hintPath, $expectedHintPath, [System.StringComparison]::OrdinalIgnoreCase))
            {
                Add-ValidationResult "FAIL" "IconLibrary HintPath" "Expected '$expectedHintPath' but found '$hintPath'."
            }
            elseif (![string]::Equals($copyLocal, "True", [System.StringComparison]::OrdinalIgnoreCase))
            {
                Add-ValidationResult "FAIL" "IconLibrary Copy Local" "Expected Private=True but found '$copyLocal'."
            }
            else
            {
                Add-ValidationResult "PASS" "IconLibrary assembly reference" "$expectedHintPath with Private=True."
            }
        }

        if ($iconProjectReferences.Count -eq 0)
        {
            Add-ValidationResult "PASS" "IconLibrary project reference" "No private IconLibrary project reference exists."
        }
        else
        {
            Add-ValidationResult "FAIL" "IconLibrary project reference" "A private IconLibrary ProjectReference is still present."
        }
    }

    $unstagedWhitespace = Invoke-GitCommand @("diff", "--check")

    if ($unstagedWhitespace.ExitCode -eq 0)
    {
        Add-ValidationResult "PASS" "Unstaged whitespace check" "git diff --check passed."
    }
    else
    {
        Add-ValidationResult "FAIL" "Unstaged whitespace check" (($unstagedWhitespace.Output | ForEach-Object { $_.ToString() }) -join "; ")
    }

    $stagedWhitespace = Invoke-GitCommand @("diff", "--cached", "--check")

    if ($stagedWhitespace.ExitCode -eq 0)
    {
        Add-ValidationResult "PASS" "Staged whitespace check" "git diff --cached --check passed."
    }
    else
    {
        Add-ValidationResult "FAIL" "Staged whitespace check" (($stagedWhitespace.Output | ForEach-Object { $_.ToString() }) -join "; ")
    }

    $eolResult = Invoke-GitCommand @("ls-files", "--eol")
    Assert-GitSuccess $eolResult "Checking line endings"
    $indexMixedEolFiles = @(
        $eolResult.Output |
            Where-Object { $_.ToString() -match "(^|\s)i/mixed(\s|$)" } |
            ForEach-Object { Get-EolFilePath $_ }
    )

    if ($indexMixedEolFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Repository line endings" "No tracked file has mixed line endings in the Git index."
    }
    else
    {
        Add-ValidationResult "FAIL" "Repository line endings" ("Mixed in Git index: " + ($indexMixedEolFiles -join ", "))
    }

    $workingTreeMixedEolFiles = @(
        $eolResult.Output |
            Where-Object { $_.ToString() -match "(^|\s)w/mixed(\s|$)" } |
            ForEach-Object { Get-EolFilePath $_ }
    )

    $changedTrackedResult = Invoke-GitCommand @("diff", "HEAD", "--name-only", "--")
    Assert-GitSuccess $changedTrackedResult "Listing changed tracked files"
    $changedTrackedFiles = @(
        $changedTrackedResult.Output |
            Where-Object { ![string]::IsNullOrWhiteSpace($_.ToString()) } |
            ForEach-Object { $_.ToString().Replace("\", "/") }
    )
    $changedWorkingTreeMixedEolFiles = @(
        $workingTreeMixedEolFiles | Where-Object { $changedTrackedFiles -icontains $_ }
    )

    if ($workingTreeMixedEolFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "Working-tree line endings" "No tracked working-tree file has mixed line endings."
    }
    elseif ($changedWorkingTreeMixedEolFiles.Count -ne 0)
    {
        Add-ValidationResult "FAIL" "Working-tree line endings" ("Changed files with mixed line endings: " + ($changedWorkingTreeMixedEolFiles -join ", "))
    }
    else
    {
        Add-ValidationResult "WARN" "Working-tree line endings" "$($workingTreeMixedEolFiles.Count) unchanged tracked file(s) have legacy mixed working-tree line endings; the Git index is clean and no changed file is affected."
    }

    $nonCrLfFirstPartyCSharpFiles = @(
        foreach ($entry in $eolResult.Output)
        {
            $relativePath = Get-EolFilePath $entry

            if (
                $firstPartyEditableCSharpFiles -icontains $relativePath -and
                $entry.ToString() -notmatch "(^|\s)w/crlf(\s|$)"
            )
            {
                $relativePath
            }
        }
    )

    if ($nonCrLfFirstPartyCSharpFiles.Count -eq 0)
    {
        Add-ValidationResult "PASS" "C# line endings" "$($firstPartyEditableCSharpFiles.Count) first-party editable C# files use CRLF line endings."
    }
    else
    {
        Add-ValidationResult "FAIL" "C# line endings" ("Non-CRLF files: " + ($nonCrLfFirstPartyCSharpFiles -join ", "))
    }

    $finalNewlineScriptPath = Join-Path $repositoryRootPath "Build\Normalize-FinalNewlines.ps1"

    if ([System.IO.File]::Exists($finalNewlineScriptPath))
    {
        try
        {
            & $finalNewlineScriptPath `
                -RepositoryRoot $repositoryRootPath `
                -Check `
                -Quiet

            Add-ValidationResult "PASS" "Final newlines" "All canonical tracked text files end with exactly one CRLF or LF sequence."
        }
        catch
        {
            Add-ValidationResult "FAIL" "Final newlines" $_.Exception.Message
        }
    }
    else
    {
        Add-ValidationResult "FAIL" "Final newlines" "Build\Normalize-FinalNewlines.ps1 was not found."
    }

    $conflictMarkers = Invoke-GitCommand @("grep", "-n", "-I", "-E", "^(<<<<<<< |>>>>>>> )")

    if ($conflictMarkers.ExitCode -eq 1)
    {
        Add-ValidationResult "PASS" "Merge conflict markers" "No unresolved conflict markers were found."
    }
    elseif ($conflictMarkers.ExitCode -eq 0)
    {
        Add-ValidationResult "FAIL" "Merge conflict markers" (($conflictMarkers.Output | ForEach-Object { $_.ToString() }) -join "; ")
    }
    else
    {
        Add-ValidationResult "FAIL" "Merge conflict markers" "git grep failed with exit code $($conflictMarkers.ExitCode)."
    }

    $rootLicense = Join-Path $repositoryRootPath "LICENSE"

    if ([System.IO.File]::Exists($rootLicense) -or [System.IO.File]::Exists("$rootLicense.txt") -or [System.IO.File]::Exists("$rootLicense.md"))
    {
        Add-ValidationResult "PASS" "Root license" "A root license file is present."
    }
    else
    {
        Add-ValidationResult "WARN" "Root license" "Not present yet; add it before the final public source release."
    }

    if (!$SkipGitleaks)
    {
        $gitleaksCommand = Get-Command gitleaks -ErrorAction SilentlyContinue

        if ($null -eq $gitleaksCommand)
        {
            Add-ValidationResult "WARN" "Gitleaks" "gitleaks was not found in PATH; the scan was skipped."
        }
        else
        {
            $snapshotRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("JasonQuery-Gitleaks-{0}" -f [Guid]::NewGuid().ToString("N"))

            try
            {
                [System.IO.Directory]::CreateDirectory($snapshotRoot) | Out-Null
                $snapshotPrefix = $snapshotRoot.TrimEnd([char[]]@("\", "/")) + [System.IO.Path]::DirectorySeparatorChar
                $checkoutResult = Invoke-GitCommand @("checkout-index", "--all", "--force", "--prefix=$snapshotPrefix")
                Assert-GitSuccess $checkoutResult "Exporting the Git index for gitleaks"

                $gitleaksOutput = @(& $gitleaksCommand.Source dir --no-banner --redact $snapshotRoot 2>&1)
                $gitleaksExitCode = $LASTEXITCODE

                if ($gitleaksExitCode -eq 0)
                {
                    Add-ValidationResult "PASS" "Gitleaks" "No leaks were found in the staged/tracked snapshot."
                }
                else
                {
                    Add-ValidationResult "FAIL" "Gitleaks" "The scan returned exit code $gitleaksExitCode; review the redacted console output."
                    $gitleaksOutput | ForEach-Object { Write-Host $_ }
                }
            }
            finally
            {
                if ([System.IO.Directory]::Exists($snapshotRoot))
                {
                    Remove-Item -LiteralPath $snapshotRoot -Recurse -Force
                }
            }
        }
    }
    else
    {
        Add-ValidationResult "WARN" "Gitleaks" "Skipped by -SkipGitleaks."
    }
}
catch
{
    $fatalError = $_
    Add-ValidationResult "FAIL" "Repository validator" $_.Exception.Message
}
finally
{
    if (![string]::IsNullOrWhiteSpace($resolvedReportPath))
    {
        try
        {
            Write-ValidationReport $resolvedReportPath $branchName $commitSha $workingTreeState
            Write-Host ""
            Write-Host "Validation report: $resolvedReportPath"
        }
        catch
        {
            Write-Host "Unable to write the validation report: $($_.Exception.Message)" -ForegroundColor Red

            if ($null -eq $fatalError)
            {
                $fatalError = $_
            }
        }
    }
}

$failureCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count

if ($null -ne $fatalError -or $failureCount -ne 0)
{
    throw "JasonQuery repository validation failed with $failureCount failed check(s)."
}

Write-Host ""
Write-Host "JasonQuery repository validation passed." -ForegroundColor Green

$global:LASTEXITCODE = 0
