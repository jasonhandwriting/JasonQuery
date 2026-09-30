# JasonQuery Architecture

JasonQuery is a Windows desktop database query and management application for Oracle, PostgreSQL, SQL Server, and MySQL. The codebase targets .NET Framework 4.8, is built as x64, and combines the main WinForms application with shared libraries, vendored UI components, a separate updater, isolated internal-database helpers, and three automated test projects.

This document describes the repository structure and the boundaries that are important when changing JasonQuery. Build prerequisites and local dependency setup belong in [BUILD.md](BUILD.md); security reporting belongs in [SECURITY.md](SECURITY.md).

## Architectural goals

The current architecture is organized around several practical constraints:

- keep the interactive WinForms application on .NET Framework 4.8 and x64;
- support Oracle, PostgreSQL, SQL Server, and MySQL through provider-specific database behavior while sharing common application workflows;
- keep proprietary or non-redistributable build dependencies outside source control;
- isolate historical and modern `JasonQuery.db` storage technologies instead of loading incompatible SQLite/SQLCipher stacks into one process;
- make internal-database storage routing explicit and versioned rather than provider-probing or exception-based;
- make storage migration durable, recoverable, and independently validatable;
- keep updater execution separate from the main application;
- separate deterministic regression tests from real-database integration tests.

## Solution overview

`JasonQuery.sln` contains 11 projects.

| Project | Type | Primary role |
| --- | --- | --- |
| `JasonQuery` | WinExe | Main WinForms application, database workflows, UI, internal settings/history database access, security/bootstrap logic, and application orchestration. |
| `JasonLibrary` | Library | Shared schema/type helpers, SQL formatting, update metadata logic, common controls, editor helpers, and reusable infrastructure. |
| `MagicLibrary` | Library | Vendored Crownwood Magic UI/docking code retained for JasonQuery compatibility. |
| `ScintillaNET` | Library | Vendored ScintillaNET editor wrapper and x64 Scintilla native payload used by the editor layer. |
| `Updater` | WinExe | Separate update process responsible for package verification, safe extraction, backup/transaction behavior, and file replacement. |
| `JasonQuery.LegacyDbMigration` | Exe | Isolated reader/streamer for historical Storage V1 `System.Data.SQLite` data during physical storage migration. |
| `JasonQuery.ModernDbMigration` | Exe | Isolated SQLCipher-based Storage V2 candidate writer and read-only validator used by migration. |
| `JasonQuery.ModernDbRuntime` | Exe | Isolated SQLCipher-based runtime process used by the modern internal-database runtime boundary. |
| `JasonLibrary.Tests` | Test | Unit/regression tests for shared library and updater behavior. |
| `JasonQuery.Tests` | Test | Unit/regression/safety tests for the main application, including internal database security, migration, recovery, and runtime contracts. |
| `JasonQuery.IntegrationTests` | Test | Real Oracle/PostgreSQL/SQL Server/MySQL integration tests. |

The supported solution configurations are `Debug | x64` and `Release | x64`.

## High-level dependency shape

```text
                         +----------------------+
                         |      JasonQuery      |
                         |   WinForms main app  |
                         +----------+-----------+
                                    |
                +-------------------+-------------------+
                |                   |                   |
                v                   v                   v
        +---------------+   +---------------+   +---------------+
        | JasonLibrary  |   | MagicLibrary  |   | ScintillaNET  |
        +---------------+   +---------------+   +---------------+
                |
                | shared update contracts / helpers
                v
        +---------------+
        |    Updater    |
        | separate exe  |
        +---------------+

JasonQuery also builds with and launches isolated internal-database helpers:

  JasonQuery.LegacyDbMigration
  JasonQuery.ModernDbMigration
  JasonQuery.ModernDbRuntime

Tests:

  JasonLibrary.Tests ----------> JasonLibrary + Updater
  JasonQuery.Tests ------------> JasonQuery
  JasonQuery.IntegrationTests -> JasonQuery + real database providers
```

The project references from `JasonQuery.csproj` ensure that the shared libraries and helper executables are part of the solution/build graph. The migration/runtime helpers still define process boundaries: the main application communicates with the modern internal-database runtime through a protocol client rather than loading the SQLCipher runtime into the main process.

## Main application structure

The `JasonQuery` project is the application composition root. Its source is divided by responsibility rather than by one monolithic form hierarchy.

Important top-level areas include:

| Area | Responsibility |
| --- | --- |
| `Core/` | Application logic such as connection state, query behavior, formatting, schema operations, logging, localization, export, security, and internal contracts. |
| `Database/` | Internal application storage and external database provider implementations. |
| `UI/` | Forms, UI helpers, services, and presentation-level behavior. |
| `Editor/` | Editor integration and editor-specific behavior. |
| `Display/` | Display-oriented helpers and presentation logic. |
| `Infrastructure/` | Cross-cutting infrastructure used by application features. |
| `Services/` | Application services that coordinate reusable operations. |
| `Files/` | Source-controlled templates or static files used by the application. |
| `localization/` | Localization resources shipped with the application. |

`Program.cs` is the WinForms entry point. It initializes logging, application-wide exception handling, the low-priority idle memory check, and then launches `MainForm`.

The codebase continues to contain substantial WinForms-era code, but newer work is progressively factored into `Core`, `Database`, `Services`, and helper classes so behavior can be tested without requiring every path to remain embedded in a form.

## External database architecture

JasonQuery supports four external database families:

```text
Oracle
PostgreSQL
SQL Server
MySQL
```

`Core/Database/Connection/DataSourceType.cs` is the common application-level database-family identity.

External database work is split between shared contracts/workflows and provider-specific behavior:

- `Core/Database/Connection/` owns common connection state, validation, server-version information, and transaction-oriented helpers.
- `Core/Database/CreateScript/` contains provider-specific DDL/script generation paths.
- `Core/Arrange/` contains provider-specific result/schema formatting behavior.
- `Database/Providers/Readers/` contains provider readers for Oracle, PostgreSQL, SQL Server, and MySQL.
- `Database/Providers/Executor/` contains provider-specific executor behavior where the implementation requires a separate executor.
- `Core/Schema`, `Core/SchemaExplorer`, and related UI/services build higher-level schema browsing and editing behavior on top of the provider layer.

The application and integration-test project import the Devart provider assemblies through the centralized build configuration. Provider-specific behavior should remain behind the database-family boundaries instead of spreading raw provider assumptions through unrelated UI code.

### External database dependency boundary

`Directory.Build.props` centralizes Devart paths and restricts direct Devart references to:

- `JasonQuery.csproj`
- `JasonQuery.IntegrationTests.csproj`

This is intentional. A new shared library should not acquire a Devart reference merely for convenience; doing so widens the licensed dependency surface and makes isolated testing harder.

## Internal `JasonQuery.db` architecture

`JasonQuery.db` is the application's own settings/history/internal-data store. It is architecturally separate from the Oracle/PostgreSQL/SQL Server/MySQL connections that users open.

The main repository facade is:

`JasonQuery/Database/Internal/Repositories/JasonQueryRepository.cs`

Application code calls this repository for internal data rather than treating the internal SQLite implementation as another user-selectable database provider.

The repository delegates physical database work through:

`IJasonQueryDatabaseRuntime`

The runtime contract exposes a small set of storage operations:

- query;
- non-query;
- batch non-query;
- open/credential validation;
- password/key change.

This boundary allows the internal store to have different physical runtime implementations without changing every repository call site.

### Legacy runtime

`LegacySystemDataSQLiteDatabaseRuntime` is the in-process historical runtime based on `System.Data.SQLite`.

It opens the internal database directly in the main process and validates the expected `SystemConfig` schema before treating the database as usable.

The legacy SQLite assemblies are intentionally treated as a distinct dependency family from the modern SQLCipher stack.

### Modern runtime

`ModernSqlCipherDatabaseRuntime` implements the same `IJasonQueryDatabaseRuntime` abstraction, but it does not host SQLCipher directly inside the JasonQuery process.

Instead it uses:

`ModernDatabaseRuntimeProcessClient`

to start and communicate with:

`JasonQuery.ModernDbRuntime.exe`

The client uses a framed binary protocol over redirected standard input/output. It validates the runtime identity, including the protocol/runtime identity and the qualified SQLite/SQLCipher/native-library identity, before using the process.

The modern runtime process is disposed after database operations and is explicitly shut down or terminated if protocol/process integrity is lost. Sensitive credential byte arrays are cleared by the calling/runtime code where the relevant operations require them.

This process boundary prevents the modern SQLCipher native/runtime stack from being mixed casually with the historical `System.Data.SQLite` stack in the main process.

## Storage-format routing

Physical `JasonQuery.db` format is a versioned contract.

`JasonQueryDbStorageRuntimeRoutingContract` resolves the allowed runtime from persisted `storageFormatVersion`:

```text
missing / explicit V1
    -> LegacySystemDataSQLite

explicit V2
    -> ModernSqlCipher

unknown version
    -> fail closed
```

The routing contract explicitly does **not**:

- probe multiple providers;
- inspect a database and guess which provider can open it;
- fall back from one provider to another after an exception;
- perform migration as a side effect of routing.

A storage-format/runtime pair is valid only when it matches the defined contract.

### Current production-format marker

At the documented baseline, `JasonQueryDbStorageFormatContract.CurrentVersion` is still:

`LegacyVersion`

Storage V2 (`SqlCipherCompatibility4`) is defined and recognized, and the repository contains migration, validation, routing, and modern-runtime infrastructure for it. However, the source-level `CurrentVersion` marker means contributors must not describe Storage V2 as the default production storage format until that contract is intentionally changed and qualified.

This distinction is important: **having a qualified V2 migration/runtime implementation is not the same as declaring V2 to be the current production format.**

## `JasonQuery.db` security boundary

Database-file security metadata is separate from the physical storage-format identity.

The `Core/Security/JasonQueryDb/` area contains the contracts for:

- Windows-current-user protection;
- custom-password protection;
- key derivation and key generation;
- recovery-key enrollment and recovery;
- security metadata persistence;
- security transitions;
- physical storage migration;
- migration journaling and recovery;
- runtime routing/cutover.

`JasonQueryDbSecurityBootstrapper` resolves startup security state from the database file plus security metadata.

For Windows-current-user mode, the protected database key is unlocked through the configured Windows key protector. If that Windows-protected key is unavailable and recovery information exists, startup moves to the recovery path rather than silently generating a replacement key.

For custom-password mode, startup derives the database password from the supplied password and persisted KDF metadata.

Security mode, recovery state, and physical storage format are related but distinct contracts. Changes in one layer should not bypass validation in another.

## Physical storage migration boundary

Physical Storage V1 → V2 migration is intentionally separated from normal runtime routing.

The migration path uses two isolated helpers:

```text
Legacy Storage V1
    |
    | read-only logical stream
    v
JasonQuery.LegacyDbMigration.exe
    |
    | versioned logical/wire protocol
    v
JasonQuery.ModernDbMigration.exe
    |
    | write + validate candidate
    v
Storage V2 candidate
```

The main application coordinates this through `JasonQueryDbStorageMigrationCoordinator` and related migration/recovery classes.

Important properties of this design include:

- migration helpers are separate executables;
- legacy and modern helpers must be different executable paths;
- migration uses an explicit logical/wire protocol shared through linked contract source;
- the modern helper writes the V2 candidate using the qualified SQLCipher runtime;
- the candidate is read-only validated before replacement is allowed;
- the migration has durable stages and a journal;
- file hashes are checked at durable boundaries;
- backup/candidate/metadata artifacts have explicit paths and lifecycle rules;
- an execution lock protects the migration state machine;
- recovery decisions are explicit rather than restarting a new migration blindly;
- completion requires the production database and metadata to match the committed identities and temporary migration artifacts to be gone.

`JasonQueryDbStorageRuntimeCutoverCoordinator` is the boundary that joins persisted routing with an explicit V1 → V2 migration. A new migration may start only from the persisted legacy route, and successful completion must resolve to the modern route.

Migration is not supposed to happen merely because a normal database open failed.

## Modern SQLCipher isolation

The two modern helper projects use:

- `SQLitePCLRaw.core`
- `SQLitePCLRaw.provider.sqlcipher`
- an externally supplied, qualified `sqlcipher.dll`

The native library is expected under the documented `.local/SQLite/Modern` dependency boundary and is copied into the helper output by the helper projects' build targets.

`JasonQuery.ModernDbMigration` owns migration-time SQLCipher operations.

`JasonQuery.ModernDbRuntime` owns the isolated normal-runtime process for modern storage.

The main application owns orchestration and protocol clients, not the embedded SQLCipher provider implementation.

## Legacy SQLite isolation

Historical Storage V1 migration uses:

`JasonQuery.LegacyDbMigration`

and a separately configured legacy SQLite assembly location.

`Directory.Build.props` restricts the historical legacy SQLite references to that migration project.

The normal JasonQuery application also has its own current `System.Data.SQLite` reference set for the legacy internal runtime and unit-test validation. These are deliberately separate from the legacy-migration helper dependency path and from the modern SQLCipher path.

Contributors should not collapse these dependency families into one shared provider folder without re-qualifying migration/runtime compatibility.

## Shared library boundary

`JasonLibrary` contains reusable behavior that is not specific to one application form. Major areas include:

- database/schema type resolution;
- SQL formatter coordination;
- SQL formatter engines and formatter capability metadata;
- update metadata parsing and release selection;
- update digest validation;
- update-source resolution;
- common editor/Scintilla helpers;
- HexBox-based binary viewing controls;
- Win32/common utility code.

`JasonLibrary` references the vendored `ScintillaNET` output and selected NuGet packages used by shared formatting/UI behavior.

Code should move into `JasonLibrary` only when it is genuinely reusable and does not force application-only licensed/provider dependencies into the shared library.

## Vendored UI components

Two source projects are retained inside the repository because JasonQuery depends on their behavior:

### `MagicLibrary`

A vendored Crownwood Magic codebase used for docking, tabbed groups, menus, and related classic WinForms UI behavior.

### `ScintillaNET`

A vendored ScintillaNET project used by JasonQuery's editor layer. The x64 native Scintilla payload is embedded by this project.

These projects are third-party/vendored code, not architectural locations for new JasonQuery domain logic. Licensing and attribution are documented in the repository's notice files.

## Update architecture

Update responsibilities are split between shared metadata logic and a separate executable.

`JasonLibrary` contains update contracts and logic such as:

- update channels;
- metadata parsing;
- release selection;
- version comparison;
- digest validation;
- source resolution.

`Updater` is a separate WinForms executable that performs the file-system mutation side of an update. Its `Core` layer includes:

- package verification;
- SHA-256 digest handling;
- safe ZIP extraction;
- update workspace management;
- backup manifest handling;
- file-update transaction behavior;
- update worker/runtime orchestration.

Keeping file replacement in a separate process avoids requiring the running main executable to overwrite itself.

Official release packaging and update publication are separate maintainer workflows; see [BUILD.md](BUILD.md) and the repository publishing scripts.

## Build dependency boundaries

JasonQuery intentionally does not store every build dependency in Git.

`Directory.Build.props` centralizes the main local dependency roots and limits which projects may import them.

| Dependency family | Direct consumers |
| --- | --- |
| ComponentOne | `JasonQuery` |
| Devart | `JasonQuery`, `JasonQuery.IntegrationTests` |
| current `System.Data.SQLite` set | `JasonQuery`, `JasonQuery.Tests` |
| legacy `System.Data.SQLite` set | `JasonQuery.LegacyDbMigration` |
| modern SQLCipher native/runtime | modern migration/runtime helper projects |
| private `IconLibrary.dll` | `JasonQuery` |

The default local dependency locations are under `.local`, with documented environment/property overrides for supported build workflows.

`Directory.Build.targets` also validates important output invariants, including formatter assemblies, selected ScriptDOM satellite resources, absence of the retired legacy formatter assembly, and the expected x64 SQLite native output.

For exact setup instructions, use [BUILD.md](BUILD.md). Architecture documentation should not duplicate local installation procedures.

## Test architecture

The test suite is split into three projects.

### `JasonLibrary.Tests`

Covers shared-library behavior and also references `Updater` for update-related regression coverage.

No real database is required.

### `JasonQuery.Tests`

References the main application project and covers application/core behavior. This project includes extensive regression and safety coverage for areas such as:

- SQL generation and formatting;
- diagnostics and transaction policy;
- autocomplete;
- value formatting;
- internal database security;
- storage migration and recovery;
- runtime boundaries and failure handling.

It also links the legacy logical-streamer source where needed to validate migration contracts without changing production assembly boundaries.

No real external database is required for the normal unit/regression suite.

### `JasonQuery.IntegrationTests`

References the main application and directly receives the qualified Devart provider dependencies.

It exercises real:

- Oracle;
- PostgreSQL;
- SQL Server;
- MySQL

environments, including schema metadata, special data types, transaction behavior, and locking.

Integration tests are intentionally separated from deterministic unit tests because they require dedicated mutable database environments.

See [Tests/README.md](Tests/README.md) for execution and safety rules.

## Repository and automation boundary

GitHub-hosted automation is intentionally lightweight because the full application build depends on licensed/private components that are not committed to the repository.

Repository automation includes:

- Repository Guard;
- Secret Scan;
- Public Endpoint Monitor;
- manual Draft Release workflow;
- Dependabot.

These workflows protect repository structure, secrets, public endpoints, dependency maintenance, and release preparation, but they do not replace the trusted Windows VM release build and real-database integration qualification.

## Architectural invariants

Changes should preserve these invariants unless an intentional architecture change is separately designed and qualified:

1. The supported application build remains x64; do not reintroduce an accidental x86 path.
2. External user database providers and the internal `JasonQuery.db` store remain separate concerns.
3. Direct proprietary/provider dependencies remain constrained to the projects that require them.
4. Internal storage routing is determined by persisted format contracts, not provider probing or fallback.
5. Unknown internal storage-format versions fail closed.
6. Legacy SQLite and modern SQLCipher implementations remain isolated across deliberate project/process boundaries.
7. A physical storage migration is explicit, journaled, validated, and recoverable; normal open failure must not silently start migration.
8. The modern runtime process identity/protocol must be validated before it is trusted.
9. Security metadata, security mode, recovery state, and physical storage format remain separate versioned concerns.
10. The updater remains a separate process for application file replacement.
11. Unit/regression tests remain independent of real database infrastructure; real provider behavior belongs in integration tests.
12. Private/licensed build inputs remain outside source control and are supplied through the documented `.local`/environment boundaries.

## Where to make a change

A useful first approximation is:

| Change | Start here |
| --- | --- |
| WinForms workflow or form behavior | `JasonQuery/UI`, relevant `Services`, then `Core` |
| Query/editor behavior | `JasonQuery/Core/QueryEngine`, `JasonQuery/Editor`, `JasonLibrary/Core/Text/Formatting` |
| External DB connection/provider behavior | `JasonQuery/Core/Database`, `JasonQuery/Database/Providers` |
| Schema/create-script behavior | `JasonQuery/Core/Database/CreateScript`, schema/provider areas |
| Internal settings/history persistence | `JasonQuery/Database/Internal/Repositories` |
| `JasonQuery.db` security/recovery | `JasonQuery/Core/Security/JasonQueryDb` |
| Internal DB runtime boundary | `JasonQuery/Database/Internal/Runtime` |
| Storage V1 → V2 migration | `JasonQuery/Core/Security/JasonQueryDb`, `Migration/LegacySQLite`, `Migration/ModernSQLite` |
| Modern SQLCipher runtime process | `Runtime/ModernSQLite` and the main-process protocol client |
| Shared formatter/type/update logic | `JasonLibrary` |
| Application updater/file replacement | `Updater` |
| Unit/regression coverage | `Tests/JasonLibrary.Tests`, `Tests/JasonQuery.Tests` |
| Real provider qualification | `Tests/JasonQuery.IntegrationTests` |
| Build/dependency wiring | `Directory.Build.props`, `Directory.Build.targets`, `Build/` |

## Related documentation

- [README.md](README.md) — project overview and public entry point
- [BUILD.md](BUILD.md) — build environment and dependency setup
- [Tests/README.md](Tests/README.md) — test organization and execution
- [SECURITY.md](SECURITY.md) — vulnerability reporting and security policy
- [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) — third-party attribution and redistribution notes
- [LICENSE.md](LICENSE.md) — JasonQuery license
