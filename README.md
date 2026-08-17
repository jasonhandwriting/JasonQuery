# JasonQuery

JasonQuery is a Windows desktop database query and management tool for Oracle, PostgreSQL, SQL Server, and MySQL.

The application targets .NET Framework 4.8, C# 8.0, and x64 Windows.

## Repository status

This repository is being prepared for a future public source release. Some proprietary or separately licensed build dependencies are intentionally not included. A fresh clone will not build until those dependencies have been installed or supplied by the developer.

## Build environment

- Windows x64
- Visual Studio 2022
- .NET Framework 4.8 Developer Pack
- .NET desktop development workload
- Properly licensed ComponentOne and Devart dependencies used by the solution
- Required private assemblies placed under the `.local` directory

The supported solution configurations are:

- `Debug | x64`
- `Release | x64`

JasonQuery does not provide or support an x86 build.

## Private IconLibrary dependency

The `IconLibrary` source project and its embedded icon assets are not included in this repository because they contain third-party licensed materials that are not licensed for source redistribution.

To build JasonQuery, place a legally obtained, compatible assembly at:

```text
.local\IconLibrary\IconLibrary.dll
```

For the maintainer's official packaging workflow, also place the matching program database file at:

```text
.local\IconLibrary\IconLibrary.pdb
```

`IconLibrary.dll` is required to build and run JasonQuery. The PDB is not required for compilation, but the official JasonQuery publishing scripts require it so release packages retain matching debugging symbols.

The private `IconLibrary` source directory, `.local` dependencies, DLL, and PDB must not be committed to this repository. Official JasonQuery release packages include the runtime `IconLibrary.dll` and its matching `IconLibrary.pdb`.

## Build

1. Install the required development tools and properly licensed third-party components.
2. Place all required private assemblies in their documented `.local` locations.
3. Open `JasonQuery.sln` in Visual Studio 2022.
4. Select `Debug | x64` or `Release | x64`.
5. Rebuild the solution.

## Tests

Automated tests can be run from Visual Studio Test Explorer. Database integration tests require separately configured Oracle, PostgreSQL, SQL Server, and MySQL test environments and may be skipped when those environments are unavailable.

## Automated repository guard

GitHub Actions runs a lightweight `Repository Guard` for every pull request targeting `main`, every push to `main`, and manual workflow runs. It checks repository structure, private and generated file guards, supported solution configurations, the private `IconLibrary` reference contract, whitespace, line endings, merge markers, and the workflow's read-only security contract.

The GitHub-hosted check intentionally does not build JasonQuery or run database tests because the required licensed and private dependencies are not stored in this repository. Full `Release | x64` builds, automated tests, database integration checks, and release-package validation remain part of the trusted Windows VM workflow.
## Maintainer automation

JasonQuery uses deliberately lightweight automation suited to a one-person maintenance workflow:

- **Dependabot** checks NuGet and GitHub Actions dependencies once a month. Routine minor/patch updates and major updates are grouped separately so that the maintainer normally receives no more than two PRs per ecosystem and can test risky major upgrades independently.
- **Secret Scan** runs Gitleaks for pull requests targeting `main`, pushes to `main`, a weekly full-history schedule, and manual runs. It uses read-only repository permission and neither posts PR comments nor uploads a potentially sensitive findings artifact.
- **Public Endpoint Monitor** checks the official website, TLS certificate lifetime, update metadata schema, and every published ZIP URL referenced by the metadata. Manual runs are always available. Scheduled runs remain dormant until the repository variable `PUBLIC_ENDPOINT_MONITOR_ENABLED` is explicitly set to `true` after HTTPS passes a manual run; this prevents known infrastructure work from creating repetitive false alarms.
- **Draft Release** is manual-only. It downloads the official ZIP, requires the SHA-256 produced by the trusted Windows VM, verifies the ZIP before upload, and creates a GitHub **draft** release. It never publishes a release; final notes and assets must be reviewed manually.

GitHub-hosted runners still do not build JasonQuery or run database integration tests because the licensed and private dependencies are not stored in this repository. Those checks remain in the trusted Windows VM publishing workflow. A GitHub-hosted Core Tests job should be added only after a test project can build without private dependencies.
## Maintainer publishing workflow

Official packages are built from a trusted Windows VM after a clean `Release | x64` rebuild, automated tests, database smoke tests, and a final manual startup test.

The publishing scripts verify that `IconLibrary.dll` and `IconLibrary.pdb` are present in both the Release output and the ZIP package and that the packaged files are byte-identical to the Release output.

## Third-party components

Third-party components remain subject to their respective licenses. Refer to the license and notice files included with the relevant projects and official release packages.
