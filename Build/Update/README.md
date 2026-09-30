# JasonQuery update publishing metadata

This directory contains the update metadata contract and the maintainer tools used by the official and test publishing workflows.

## Version and channel policy

JasonQuery uses a three-component release version, `major.minor.build`. The
Windows assembly/file version uses the same values plus a zero revision:
`major.minor.build.0`.

- `major.minor.0` is a Production release, for example `0.97.0`.
- `major.minor.nonzero` is a Test release, for example `0.97.1` or `0.97.2`.
- Production installations receive Production updates only.
- Test installations may receive a newer Production or Test update.
- The updater never performs an automatic downgrade.

For all future releases, the canonical Git tag is the complete three-component
version prefixed with `v`:

- Production example: internal/update version `0.97.0`, Git tag `v0.97.0`,
  user-facing GitHub Release title `JasonQuery v0.97`.
- Test example: internal/update version `0.97.1`, Git tag `v0.97.1`,
  user-facing GitHub Release title `JasonQuery v0.97.1 Test`.

The Release title is presentation text only. Update selection and package
metadata use the three-component version and canonical tag.

### Historical tag exception

The already-published `v0.94` and `v0.95` two-component tags are grandfathered
historical identifiers. Do not rename, delete, recreate, or move them. Starting
with `v0.96.0`, new release tags follow `v<major.minor.build>`.

The package names are stable client contracts:

- Production: `JasonQuery64.zip`
- Test: `JasonQuery64Test.zip`

Do not add a version to these updater asset names without first changing and
deploying the client-side update contract. The IT administrator Company Update
archive remains versioned as `JasonQuery-Company-Update-v<version>.zip`.

## Generated files

Both publishing batch files call `New-JasonQueryUpdateMetadata.ps1` after the ZIP has passed package validation. The generator writes the following files to the Windows Desktop:

- `jasonquery-update.json` for current JasonQuery clients.
- `jq.txt` for existing installations that still use the legacy update parser.

The generator obtains the version from `JasonQuery.exe`, verifies that it matches the selected channel, calculates the actual ZIP size and SHA-256 digest, and updates both files as one local transaction. Existing output files remain unchanged when validation fails.

`jasonquery-update.json` keeps at most the latest Production release and the latest Test release. A Test publish requires an existing valid Production entry, so run the official workflow before the first Test publish in a new publishing workspace.

The legacy `jq.txt` format remains:

````text
```0.95```0.95.2
````

The first value is the latest Production `major.minor` version. The second value is the numerically highest published three-component version. This means legacy clients can still see a Test release, matching their existing behavior. New clients use the channel policy described above.

## Official release workflow

1. Rebuild `Release | x64` in Visual Studio.
2. Ensure the intended Production version uses a zero third component, for
   example `0.97.0`.
3. Run `Publish-JasonQuery-Release.bat` from a clean `main` branch.
4. The publisher performs the Production version/channel preflight before
   repository validation, staging, smoke testing, or package creation.
5. Complete and confirm the manual smoke test.
6. Review the generated validation report.
7. Review the Desktop outputs:
   - `JasonQuery64.zip`
   - `jasonquery-update.json`
   - `jq.txt`
   - `JasonQuery-Company-Update-v<version>.zip`
8. After the release commit has completed qualification, create and push the
   canonical tag `v<major.minor.build>` on that exact commit before using the
   Draft Release workflow.

The metadata generator repeats the version/channel validation later in the
workflow as a defense-in-depth check.

## Test release workflow

1. Keep the current official `jasonquery-update.json` on the Desktop.
2. Rebuild `Release | x64` with a nonzero third version component, for example
   `0.97.1`.
3. Ensure `main` is clean and synchronized with the commit intended for the
   formal Test release. Ad-hoc branch builds do not use the formal Test
   publisher.
4. Run `Publish-JasonQuery-Test.bat`.
5. The publisher performs the Test version/channel preflight before repository
   validation, staging, smoke testing, or package creation.
6. Complete and confirm the manual smoke test.
7. Review the generated validation report.
8. Review the Desktop outputs:
   - `JasonQuery64Test.zip`
   - updated `jasonquery-update.json`
   - updated `jq.txt`
9. After qualification, create and push the canonical tag
   `v<major.minor.build>` on that exact commit before using the Draft Release
   workflow.

The formal Production and Test publishers both require a clean `main` branch.
The metadata generator repeats the channel check before metadata is committed.

## Website deployment order

Publish files in this order so metadata never points to a package that is not yet available:

1. Upload the new `JasonQuery64.zip` or `JasonQuery64Test.zip`.
2. Upload `jasonquery-update.json`.
3. Upload `jq.txt` last.

The official website paths are:

- `https://jasonquery.org/JasonQueryUpdate/JasonQuery64.zip`
- `https://jasonquery.org/JasonQueryUpdate/JasonQuery64Test.zip`
- `https://jasonquery.org/JasonQueryUpdate/jasonquery-update.json`
- `https://jasonquery.org/JasonQueryUpdate/jq.txt`

For a company update folder, copy `jasonquery-update.json` and the package or packages named in its `assets` entries into the same folder. Do not hand-edit `size` or `digest`.

## GitHub Releases alignment

Create the canonical three-component Git tag on the exact qualified commit and
push that tag before running the manual **Draft Release** workflow. The Draft
Release workflow requires both the existing tag and the exact 40-character
qualified commit SHA. It resolves the remote tag and refuses to continue if the
tag does not point to that commit.

The Draft Release workflow never creates or moves a tag. It calls GitHub CLI
with `--verify-tag`, never `--target`.

Channel behavior is derived from the third version component:

- `v<major.minor.0>` is Production, uploads `JasonQuery64.zip`, is not marked
  prerelease, and uses the title `JasonQuery v<major.minor>`.
- `v<major.minor.nonzero>` is Test, uploads `JasonQuery64Test.zip`, is marked
  prerelease automatically, and uses the title
  `JasonQuery v<major.minor.build> Test`.

The package URL supplied to Draft Release must end in the correct stable asset
alias for the derived channel, and the downloaded file must match the SHA-256
produced by the trusted Windows VM.

The website metadata intentionally contains website download URLs. GitHub API
metadata is read directly from GitHub when the user selects GitHub Releases.

## Offline self-test

Run both publishing-policy self-tests before committing publishing changes:

```powershell
& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -NoProfile `
    -ExecutionPolicy Bypass `
    -File ".\Build\Update\Test-JasonQueryPublishPolicy.ps1"

& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -NoProfile `
    -ExecutionPolicy Bypass `
    -File ".\Build\Update\Test-JasonQueryUpdateMetadata.ps1"
```

These self-tests do not publish a release, create or move a tag, or modify the
Windows Desktop publication files.

SHA-256 verifies package integrity against the selected metadata. It does not
replace HTTPS or independently prove publisher identity if an attacker can
replace both the metadata and package.

## IT administrator offline update package

The official Production workflow also creates:

- `JasonQuery-Company-Update-v<version>.zip`

This archive is intended for an IT administrator who maintains an internal update share for computers that cannot access external update websites. It contains exactly:

- `jasonquery-update.json` with the latest Production release only.
- `JasonQuery64.zip`.
- `README-Company-Update.txt`.

The archive does not contain `JasonQuery64Test.zip`; company packages are Production-only. Extract the three files into one local folder or UNC share, grant users read-only access, and select that folder under **Options > Update Settings > Company Update Folder**.

The generator validates the metadata schema, product, Production channel, minimum supported company-package version, package size, SHA-256 digest, and canonical `https://jasonquery.org/...` URLs before it writes the archive.
