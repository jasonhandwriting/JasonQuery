# JasonQuery update publishing metadata

This directory contains the update metadata contract and the maintainer tools used by the official and test publishing workflows.

## Version and channel policy

JasonQuery uses a three-component version number:

- `major.minor.0` is a Production release, for example `0.95.0`.
- `major.minor.nonzero` is a Test release, for example `0.95.1` or `0.95.2`.
- Production installations receive Production updates only.
- Test installations may receive a newer Production or Test update.
- The updater never performs an automatic downgrade.

The package names are stable client contracts:

- Production: `JasonQuery64.zip`
- Test: `JasonQuery64Test.zip`

Do not add a version to these asset names without first changing and deploying the client-side update contract.

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
2. Run `Publish-JasonQuery-Release.bat` from a clean `main` branch.
3. Complete and confirm the manual smoke test.
4. Review the generated validation report.
5. Review the Desktop outputs:
   - `JasonQuery64.zip`
   - `jasonquery-update.json`
   - `jq.txt`

The generated release tag is `v<major.minor.build>`, for example `v0.95.0`.

## Test release workflow

1. Keep the current official `jasonquery-update.json` on the Desktop.
2. Rebuild `Release | x64` with a nonzero third version component.
3. Run `Publish-JasonQuery-Test.bat`.
4. Complete and confirm the manual smoke test.
5. Review the generated validation report.
6. Review the Desktop outputs:
   - `JasonQuery64Test.zip`
   - updated `jasonquery-update.json`
   - updated `jq.txt`

The generated release tag is `v<major.minor.build>`, for example `v0.95.1`.

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

Create the GitHub release using the generated `tag_name` value and upload the same stable package alias for that channel. Mark a Test release as a prerelease. The GitHub asset name must remain exactly `JasonQuery64.zip` or `JasonQuery64Test.zip`, because the client release selector uses those names.

The website metadata intentionally contains website download URLs. GitHub API metadata is read directly from GitHub when the user selects GitHub Releases.

## Offline self-test

Run the generator self-test before committing publishing changes:

```powershell
& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -NoProfile `
    -ExecutionPolicy Bypass `
    -File ".\Build\Update\Test-JasonQueryUpdateMetadata.ps1"
```

The self-test does not use release binaries and does not change repository or Desktop files.

SHA-256 verifies package integrity against the selected metadata. It does not replace HTTPS or independently prove publisher identity if an attacker can replace both the metadata and package.


## IT administrator offline update package

The official Production workflow also creates:

- `JasonQuery-Company-Update-v<version>.zip`

This archive is intended for an IT administrator who maintains an internal update share for computers that cannot access external update websites. It contains exactly:

- `jasonquery-update.json` with the latest Production release only.
- `JasonQuery64.zip`.
- `README-Company-Update.txt`.

The archive does not contain `JasonQuery64Test.zip`; company packages are Production-only. Extract the three files into one local folder or UNC share, grant users read-only access, and select that folder under **Options > Update Settings > Company Update Folder**.

The generator validates the metadata schema, product, Production channel, minimum supported company-package version, package size, SHA-256 digest, and canonical `https://jasonquery.org/...` URLs before it writes the archive.
