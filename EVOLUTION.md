# JasonQuery Evolution

JasonQuery has been developed continuously since 2018.

It began as a practical Windows desktop database tool and, over years of use and refinement, grew into a multi-database application with dedicated query, schema, update, security, migration, diagnostics, and test infrastructure.

This document explains that evolution at a high level. It is not a replacement for [CHANGELOG.md](CHANGELOG.md), which records release-level changes from v0.94 onward, nor for [ARCHITECTURE.md](ARCHITECTURE.md), which documents the current technical design.

## Source boundaries for this history

Two kinds of history are intentionally distinguished here:

- **Maintainer history** covers the project's origin and long-running development before the current public repository documentation existed.
- **Repository-verifiable milestones** are tied to the current source tree, changelog, release tags, architecture, tests, and public repository structure.

Development began in 2018. Versions v0.27 through v0.93 predate the current repository changelog and were documented through historical JasonQuery Release Notes rather than through the present `CHANGELOG.md`.

Because those historical Release Notes are not part of the current public repository, this document does not invent a year-by-year reconstruction of those releases. The detailed, repository-verifiable timeline below therefore starts with v0.94.

## 2018 — the beginning

JasonQuery started in 2018 as a Windows desktop database query tool built to solve everyday database work directly and efficiently.

The project was not created as a short-lived prototype. It continued to evolve through real use, repeated maintenance, and compatibility work over multiple years.

That long development period shaped several characteristics that remain visible today:

- a native Windows desktop workflow;
- a strong SQL-editor focus;
- support for multiple database platforms;
- persistent connection and SQL-history features;
- extensive database-object browsing and scripting;
- localization;
- compatibility with long-lived user data;
- a preference for incremental evolution over disruptive rewrites.

Some users have used JasonQuery across multiple versions for years. That history is one reason backward compatibility, migration safety, and predictable upgrades are treated as engineering requirements rather than optional conveniences.

## Before the current public changelog

The current `CHANGELOG.md` begins at v0.94 and explicitly notes that versions v0.27 through v0.93 were covered by historical JasonQuery Release Notes.

Those earlier versions established much of the product surface that later releases continued to refine, including the database-query workflow, editor behavior, schema browsing, connection management, local settings/history storage, and multi-database support.

The public repository does not currently contain enough historical evidence to assign each of those capabilities to a specific old version or year. For that reason, this document treats the pre-v0.94 period as a continuous maturation phase rather than presenting an unsupported detailed timeline.

## 2026 — formalizing the engineering baseline

By 2026, JasonQuery had moved beyond feature accumulation into a broader engineering-hardening phase.

The work in this period increasingly focused on:

- making behavior easier to validate;
- separating runtime responsibilities;
- reducing implicit or ambiguous fallback behavior;
- strengthening release and update integrity;
- expanding regression coverage;
- documenting architecture and build boundaries;
- protecting long-lived user data through explicit migration and recovery flows; and
- preparing the project for public source distribution without publishing licensed or private dependencies.

The v0.94, v0.95, and v0.96 releases show that progression clearly.

## v0.94 — query behavior and formatter safety

**Release date: 2026-08-20**

v0.94 concentrated on day-to-day SQL editing and query-execution correctness.

The release improved SQL formatting across the supported database engines, including configurable list-item layout and stronger protections against semantic breakage around functions, `IN` lists, window functions, comments, literals, and quoted identifiers.

It also tightened query behavior in areas such as:

- query timeout handling for non-query commands;
- AutoComplete behavior in `UPDATE` statements;
- transaction-state handling when DML waiting on database locks is cancelled; and
- editor/menu interaction consistency.

This release represents a recurring JasonQuery theme: usability improvements are expected to preserve SQL semantics and transaction safety.

See [CHANGELOG.md](CHANGELOG.md) for the complete v0.94 release details.

## v0.95 — update-system and publication hardening

**Release date: 2026-08-30**

v0.95 substantially expanded the update and release infrastructure.

JasonQuery gained configurable update sources, including the official JasonQuery website and a Company Update Folder intended for controlled or offline environments. The update process moved toward structured metadata through `jasonquery-update.json` while retaining legacy `jq.txt` compatibility for already-deployed versions.

The release also introduced stronger update integrity and recovery behavior:

- release ZIP size and SHA-256 verification;
- safe ZIP extraction;
- rejection of traversal, unsafe paths, duplicate paths, symbolic links, and protected user-data paths;
- verified pre-install backups;
- transactional file replacement;
- verified restoration after failure;
- release metadata generation and publication automation;
- Production/Test channel separation; and
- an IT-administrator offline update package.

At the same time, AutoComplete error feedback and editor behavior continued to improve.

v0.95 marks an important transition: updating JasonQuery became an explicitly verified software-delivery workflow rather than a simple file-replacement operation.

See [CHANGELOG.md](CHANGELOG.md) for the complete v0.95 release details.

## v0.96 — security, storage, recovery, and compatibility

**Release date: 2026-09-29**

v0.96 was a major internal architecture and data-protection milestone.

### Storage V2 and runtime isolation

New `JasonQuery.db` storage uses Storage V2 with SQLCipher 4-compatible storage and an isolated Modern SQLite Runtime.

Historical Storage V1 remains a supported compatibility and migration source. The application uses an explicit persisted `storageFormatVersion` contract and fails closed for unknown storage versions rather than guessing a format through provider probing or exception-driven fallback.

Legacy SQLite and Modern SQLCipher responsibilities are deliberately isolated across separate runtime boundaries.

### Explicit migration

Storage V1 → V2 migration was designed as an explicit physical storage transition.

The migration path preserves the logical database key, security mode, schema, and saved connection information while using validated candidates, backups, transition journals, verification, and deterministic recovery behavior.

A storage-format migration is therefore separate from a silent security-mode change.

### Recovery Key

v0.96 added a Recovery Key for Windows Protected mode.

The Recovery Key provides a supported disaster-recovery path when the original Windows profile, DPAPI binding, or operating-system environment is no longer available. Recovery validates the database/key pairing and rebinds the existing logical database key rather than generating an unrelated replacement key.

Wrong, mismatched, tampered, regenerated-old, or disabled Recovery Keys are expected to fail safely.

### Saved connection credentials

The release also moved saved connection passwords away from the historical per-field protection model.

Saved Oracle, PostgreSQL, SQL Server, and MySQL credentials are protected by the secured `JasonQuery.db`, while existing data is migrated for compatibility.

### Diagnostics and runtime behavior

v0.96 expanded structured runtime logging, improved Windows-version diagnostics, strengthened updater diagnostics, and introduced lazy loading for Schema Browser content to reduce unnecessary work when large database objects are selected.

### Regression coverage

Security, migration, runtime routing, recovery, credential migration, update behavior, and compatibility received significantly expanded automated coverage.

The release changelog records a complete automated test baseline of 2,987 tests at v0.96. That number describes the v0.96 release baseline; contributor documentation intentionally does not treat it as a permanent future test-count requirement.

See [CHANGELOG.md](CHANGELOG.md) for the complete v0.96 release details.

## From application code to explicit architecture

The same hardening period changed how JasonQuery's codebase is organized.

The current solution separates responsibilities across eleven projects, including:

- the main `JasonQuery` application;
- reusable `JasonLibrary` behavior;
- vendored UI/editor projects;
- unit/regression test projects;
- real-database integration tests;
- a separate updater executable;
- isolated Legacy Storage V1 migration support;
- isolated Modern Storage V2 migration support; and
- an isolated Modern SQLCipher runtime process.

The architecture also distinguishes external Oracle, PostgreSQL, SQL Server, and MySQL behavior from the internal `JasonQuery.db` store.

This separation makes it possible to reason about compatibility, security, migration, provider behavior, and build dependencies independently.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the current design and invariants.

## Testing evolved with the product

JasonQuery's test strategy grew from ordinary regression coverage into three distinct layers:

- `JasonLibrary.Tests` for reusable library behavior;
- `JasonQuery.Tests` for application/core regression and safety contracts; and
- `JasonQuery.IntegrationTests` for real Oracle, PostgreSQL, SQL Server, and MySQL behavior.

The separation is intentional.

Most deterministic tests do not require a real external database. Provider-specific integration tests use dedicated mutable database environments and are not intended to run against production systems.

Security- and migration-sensitive work goes further than a normal happy-path regression test. Negative paths, interrupted states, wrong credentials or recovery material, runtime mismatches, and fail-closed behavior are part of the qualification model.

See [Tests/README.md](Tests/README.md) and [CONTRIBUTING.md](CONTRIBUTING.md).

## Release engineering became part of the product

Over time, JasonQuery's release process also became more formal.

The repository now contains explicit build and publishing contracts for:

- x64-only Debug and Release configurations;
- proprietary/local dependency boundaries;
- release-package validation;
- update metadata generation;
- release ZIP digest verification;
- offline Company Update packaging;
- GitHub draft-release preparation;
- public endpoint monitoring;
- dependency maintenance;
- repository validation; and
- Git-history secret scanning.

The full application build still depends on licensed/private components that are not committed to the public repository, so the trusted Windows build environment remains part of release qualification.

See [BUILD.md](BUILD.md) for the supported build path.

## 2026 — transition to an open-source repository

After years of private development and distribution, JasonQuery was prepared for public source release under the MIT License.

That transition required more than changing repository visibility.

The open-source preparation work included:

- removing or documenting non-redistributable dependencies;
- formalizing `LICENSE.md`;
- expanding third-party attribution and notices;
- publishing a security policy and Private Vulnerability Reporting path;
- documenting the fresh-clone build process;
- defining release/tag conventions;
- adding repository validation, secret scanning, dependency maintenance, and public-endpoint monitoring;
- documenting the architecture;
- documenting contributor workflows and Issue Forms; and
- preserving compatibility with the existing released application and long-lived user data.

The result is a repository that exposes the JasonQuery source while keeping proprietary build inputs outside source control and making those boundaries explicit.

## What did not change

Open sourcing JasonQuery did not reset the product's history.

The public repository is a continuation of the same application rather than a new product that happens to share the name.

That means several long-standing priorities remain:

- Windows desktop usability;
- Oracle, PostgreSQL, SQL Server, and MySQL support;
- compatibility with existing JasonQuery data;
- conservative handling of transactions and database changes;
- safe upgrade and recovery behavior;
- localized UI and diagnostics;
- practical workflows for day-to-day database work; and
- incremental improvement without unnecessarily breaking established users.

The source is now public, but compatibility with the product's earlier life remains part of its engineering context.

## Documentation map

For the current project rather than its history:

- [README.md](README.md) — public project entry point
- [CHANGELOG.md](CHANGELOG.md) — release-level changes from v0.94 onward
- [ARCHITECTURE.md](ARCHITECTURE.md) — current system architecture and invariants
- [BUILD.md](BUILD.md) — supported build environment and dependency setup
- [CONTRIBUTING.md](CONTRIBUTING.md) — contributor workflow and validation expectations
- [SECURITY.md](SECURITY.md) — vulnerability reporting and security policy
- [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) — third-party attribution
- [LICENSE.md](LICENSE.md) — MIT license

JasonQuery continues to evolve, but this history explains why current changes are evaluated not only for what they add, but also for how safely they preserve what already exists.
