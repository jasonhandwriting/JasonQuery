# JasonQuery

**JasonQuery can do more than you think.**

JasonQuery is a Windows x64 desktop database query and management tool for **Oracle**, **PostgreSQL**, **SQL Server**, and **MySQL**.

Developed continuously since 2018, JasonQuery combines a practical SQL-centered desktop workflow with multi-database support, schema browsing, history, formatting, update integrity, local data protection, diagnostics, and a growing regression-test baseline.

[Website](https://jasonquery.org/) · [GitHub Releases](https://github.com/jasonhandwriting/JasonQuery/releases) · [Changelog](CHANGELOG.md) · [Build from source](BUILD.md) · [Contributing](CONTRIBUTING.md)

[![Repository Guard](https://github.com/jasonhandwriting/JasonQuery/actions/workflows/repository-guard.yml/badge.svg)](https://github.com/jasonhandwriting/JasonQuery/actions/workflows/repository-guard.yml)
[![Secret Scan](https://github.com/jasonhandwriting/JasonQuery/actions/workflows/secret-scan.yml/badge.svg)](https://github.com/jasonhandwriting/JasonQuery/actions/workflows/secret-scan.yml)

## What JasonQuery provides

JasonQuery is built for day-to-day database work where direct SQL access, clear feedback, and compatibility matter.

Highlights include:

- one Windows desktop application for Oracle, PostgreSQL, SQL Server, and MySQL;
- a SQL editor with AutoComplete, syntax-aware workflows, file handling, and configurable formatting;
- SQL formatting through Hogimn SQL Formatter and Microsoft SQL ScriptDOM where applicable;
- schema browsing and database-object scripting;
- connection management, import/export, and per-connection SQL History;
- SQL History retention controls;
- structured runtime diagnostics for troubleshooting and performance analysis;
- localized English, Traditional Chinese, and Simplified Chinese UI/messages;
- a transactional update workflow with package size/SHA-256 verification, safe ZIP extraction, verified backup, rollback, and protected user-data paths;
- enterprise/offline update support through the Company Update Folder workflow; and
- protected local application data with explicit migration and disaster-recovery behavior.

JasonQuery is intentionally conservative around database transactions, upgrades, internal storage, and long-lived user data. Changes in those areas are expected to preserve compatibility or provide an explicit, qualified migration path.

## Supported database platforms

| Database | JasonQuery support |
| --- | --- |
| Oracle | Query, metadata/schema, editor and provider-specific workflows |
| PostgreSQL | Query, metadata/schema, editor and provider-specific workflows |
| SQL Server | Query, metadata/schema, editor and provider-specific workflows |
| MySQL | Query, metadata/schema, editor and provider-specific workflows |

The application and its automated integration coverage keep database-specific behavior separate where the platforms differ in SQL, metadata, transactions, locking, and special data types.

## Download and releases

Official binary releases are published through:

- [JasonQuery website](https://jasonquery.org/)
- [GitHub Releases](https://github.com/jasonhandwriting/JasonQuery/releases)

The normal production package is `JasonQuery64.zip`.

Production releases may also provide a Company Update package for IT administrators who distribute updates through an internal folder or UNC path.

JasonQuery is **x64 only**. There is no supported x86 build.

Release notes and release-package SHA-256 values are published with the corresponding release. See [CHANGELOG.md](CHANGELOG.md) for repository release history from v0.94 onward.

## SQL editor and query workflow

The SQL editor is a central part of JasonQuery.

Current functionality includes database-aware AutoComplete, configurable SQL formatting, multiple SQL files/tabs, query execution, SQL History, and database-specific behavior for the four supported platforms.

Formatter behavior is designed to preserve SQL meaning rather than simply reflow text. Regression coverage includes cases involving nested queries, function arguments, `IN` lists, window functions, comments, string literals, and quoted identifiers.

Query and transaction behavior is also tested separately from presentation features so that cancellation, timeout, locking, and pending-transaction states can be handled deliberately.

## Schema Browser

Schema Browser provides object-oriented navigation and scripting around connected databases.

To reduce unnecessary work, content such as SQL panes, object structure, view rows, and table rows can be loaded lazily when the corresponding tab is opened instead of automatically querying every available view when an object is selected.

Provider-specific metadata and schema behavior is qualified against real Oracle, PostgreSQL, SQL Server, and MySQL test environments.

## Secure local data

JasonQuery stores its own application data separately from the Oracle, PostgreSQL, SQL Server, and MySQL databases to which it connects.

Current `JasonQuery.db` storage uses **Storage V2**, a SQLCipher 4-compatible format accessed through an isolated Modern SQLite Runtime.

Important properties of this design include:

- a separately generated logical database key;
- Windows Protected and Custom Password protection modes;
- saved connection credentials protected by the secured `JasonQuery.db`;
- explicit persisted storage-format routing;
- fail-closed behavior for unknown storage versions;
- isolated Legacy Storage V1 and Modern Storage V2 runtime responsibilities;
- explicit Storage V1 → V2 migration for supported historical databases; and
- a Recovery Key for Windows Protected mode to support disaster recovery when the original Windows profile/DPAPI environment is no longer available.

Migration and recovery preserve the existing logical database identity instead of silently replacing it with unrelated new credentials.

For the current architecture, trust boundaries, and migration model, see [ARCHITECTURE.md](ARCHITECTURE.md).

## Updates and release integrity

JasonQuery's update process verifies release metadata and package integrity before modifying application files.

The update workflow includes:

- structured `jasonquery-update.json` metadata;
- Production and Test release channels;
- expected package size and SHA-256 verification;
- safe ZIP extraction;
- rejection of traversal, absolute paths, symbolic links, duplicate paths, unsafe Windows names, and protected user-data paths;
- verified backup before installation;
- transactional file replacement;
- verified rollback after failure; and
- retention of recent verified backups.

The separate `Updater` executable performs the file-system mutation side of an update so the running main application is not required to overwrite itself.

## Source availability

JasonQuery-owned source is licensed under the [MIT License](LICENSE.md).

The public repository intentionally does **not** contain every binary required for a full build. Some dependencies are proprietary, separately licensed, private, or locally qualified and therefore remain outside source control.

Important examples include:

- MESCIUS ComponentOne WinForms;
- Devart .NET Framework database providers;
- the private JasonQuery `IconLibrary` assembly; and
- qualified local SQLite / SQLCipher runtime binaries.

The JasonQuery MIT License does not grant rights to those third-party or proprietary components.

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for attribution and licensing boundaries.

## Build from source

A fresh clone requires local dependency setup before the solution can be rebuilt.

Supported development baseline:

```text
Windows x64
Visual Studio 2022
.NET Framework 4.8
C# 8.0
Debug | x64
Release | x64
```

The repository uses a documented `.local` dependency boundary for private/licensed inputs and qualified SQLite/SQLCipher runtime files.

Do not work around missing dependencies by committing vendor DLLs, private assets, activation material, or arbitrary binaries into tracked repository paths.

For the complete fresh-clone procedure, use [BUILD.md](BUILD.md).

## Testing and qualification

JasonQuery separates deterministic regression coverage from real-database integration tests.

| Test project | Purpose |
| --- | --- |
| `JasonLibrary.Tests` | Shared-library and update-related regression coverage |
| `JasonQuery.Tests` | Main application/core regression, security, migration, recovery, formatting, query, and failure-path coverage |
| `JasonQuery.IntegrationTests` | Real Oracle, PostgreSQL, SQL Server, and MySQL behavior |

The normal unit/regression suites do not require real external databases.

Integration tests require dedicated mutable database environments and are intentionally kept separate from production databases.

Security-, migration-, recovery-, updater-, release-, transaction-, and locking-sensitive changes may require additional negative-path or manual qualification beyond the normal automated test gate.

See [Tests/README.md](Tests/README.md) and [CONTRIBUTING.md](CONTRIBUTING.md).

## Maintainer automation

GitHub-hosted automation protects the public repository without pretending to replace the trusted Windows release environment.

Current automation includes:

- **Repository Guard** — repository structure, private/generated file guards, solution/configuration contracts, whitespace/line-ending checks, and automation policy;
- **Secret Scan** — Gitleaks scanning for pull requests, pushes, scheduled history checks, and manual runs;
- **Public Endpoint Monitor** — official website/TLS/update-metadata/download endpoint checks. Manual runs are always available; scheduled runs execute only when the repository variable `PUBLIC_ENDPOINT_MONITOR_ENABLED` is set to `true`;
- **Dependabot** — dependency maintenance; and
- **Draft Release** — manual preparation of a GitHub draft release from an already qualified production ZIP and SHA-256.

The full application build is not performed on GitHub-hosted runners because required licensed/private build inputs are intentionally absent from the repository.

Official release qualification remains a trusted Windows workflow that includes the applicable rebuild, tests, database checks, packaging validation, and manual smoke verification.

## Project architecture

The current solution contains eleven projects covering the main application, shared library code, vendored UI/editor components, tests, updater behavior, legacy migration, modern migration, and the isolated Modern SQLCipher runtime.

Major design boundaries include:

- external database providers vs. internal `JasonQuery.db`;
- application behavior vs. reusable `JasonLibrary` code;
- deterministic tests vs. real-database integration tests;
- Legacy Storage V1 vs. Modern Storage V2 runtimes;
- migration processes vs. normal startup/open behavior; and
- the running application vs. the separate updater process.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the complete project map and architectural invariants.

## Project history

JasonQuery has been developed continuously since 2018.

The current repository changelog begins at v0.94. Earlier versions v0.27 through v0.93 were documented through historical JasonQuery Release Notes rather than the present `CHANGELOG.md`.

[EVOLUTION.md](EVOLUTION.md) explains the project's longer development history and the transition from years of private development to the current public repository.

## Documentation

| Document | Purpose |
| --- | --- |
| [CHANGELOG.md](CHANGELOG.md) | Release-level changes from v0.94 onward |
| [EVOLUTION.md](EVOLUTION.md) | Project history and major evolution milestones |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Current architecture, runtime boundaries, projects, and invariants |
| [BUILD.md](BUILD.md) | Fresh-clone build environment and local dependency setup |
| [Tests/README.md](Tests/README.md) | Test organization, execution, and safety rules |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution workflow and validation expectations |
| [SECURITY.md](SECURITY.md) | Vulnerability reporting and security policy |
| [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) | Third-party attribution and redistribution boundaries |
| [LICENSE.md](LICENSE.md) | MIT license for JasonQuery-owned source |

## Contributing

Contributions are welcome when they preserve the documented build, compatibility, security, and architecture boundaries.

Before opening a pull request:

1. read [CONTRIBUTING.md](CONTRIBUTING.md);
2. prepare the supported build environment from [BUILD.md](BUILD.md);
3. keep the change focused;
4. run the validation appropriate to the change; and
5. make sure no private/licensed binaries, credentials, production databases, recovery keys, or other sensitive material are included.

Bug reports and feature requests use the repository Issue Forms.

Security vulnerabilities should **not** be reported through a public issue. Use GitHub Private Vulnerability Reporting as described in [SECURITY.md](SECURITY.md).

## License

JasonQuery-owned source and materials are licensed under the [MIT License](LICENSE.md).

Third-party components and proprietary build dependencies remain subject to their own licenses and terms. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
