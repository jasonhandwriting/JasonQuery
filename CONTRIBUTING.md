# Contributing to JasonQuery

Thank you for your interest in improving JasonQuery.

JasonQuery is a Windows x64 desktop database tool with a deliberately conservative build, dependency, test, and security model. Contributions are welcome, but changes should preserve the architectural and safety boundaries documented in this repository.

Before making a substantial change, review:

- [BUILD.md](BUILD.md) — supported build environment and local dependency setup
- [ARCHITECTURE.md](ARCHITECTURE.md) — project, provider, runtime, migration, and dependency boundaries
- [Tests/README.md](Tests/README.md) — unit/regression and real-database integration-test policy
- [SECURITY.md](SECURITY.md) — vulnerability reporting and security scope
- [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) — third-party licensing and attribution

## Reporting a bug

Use the Bug Report issue form for reproducible application defects.

A useful bug report normally includes:

- the affected JasonQuery version or commit;
- the Windows version;
- whether the problem is specific to Oracle, PostgreSQL, SQL Server, MySQL, or is not database-specific;
- the behavior you observed;
- the behavior you expected;
- the smallest reliable reproduction steps; and
- relevant screenshots or sanitized diagnostic text when they materially help reproduce the problem.

Before filing a new issue, search existing issues for the same behavior.

Do not attach production databases, credentials, recovery keys, private keys, customer data, or unreviewed logs containing sensitive information.

## Security vulnerabilities

Do **not** report a security vulnerability in a public issue, discussion, or pull request.

Use GitHub Private Vulnerability Reporting as described in [SECURITY.md](SECURITY.md). From the repository's **Security** tab, choose **Report a vulnerability**.

Security reports should use synthetic or non-sensitive reproduction material whenever possible.

## Requesting a feature

Use the Feature Request issue form.

Describe the problem or workflow the change would improve, the behavior you would like JasonQuery to provide, and any important compatibility or database-specific constraints. A focused problem statement is more useful than a large implementation proposal.

Feature requests are evaluated against JasonQuery's current Windows x64, .NET Framework 4.8, C# 8.0, provider, licensing, runtime, and compatibility boundaries.

## Development environment

JasonQuery targets:

```text
Windows x64
Visual Studio 2022
.NET Framework 4.8
C# 8.0
Debug | x64
Release | x64
```

JasonQuery does not provide or support an x86 build.

Follow [BUILD.md](BUILD.md) to prepare the required `.local` dependency tree before rebuilding the solution. Do not work around missing dependencies by committing vendor binaries or copying arbitrary binaries into tracked repository paths.

## Private and licensed dependencies

A fresh clone intentionally does not contain every binary required for a full build.

Important non-repository inputs include:

- MESCIUS ComponentOne WinForms assemblies;
- Devart .NET Framework provider assemblies;
- the private JasonQuery `IconLibrary` binary;
- qualified SQLite / SQLCipher runtime binaries supplied through the documented local dependency boundary.

Contributors are responsible for obtaining proprietary dependencies under appropriate vendor licenses or trials.

Do not commit:

```text
.local\
private IconLibrary source
IconLibrary.dll
IconLibrary.pdb
vendor or trial activation keys
database passwords
integration-test credentials
recovery keys
private keys
local JasonQuery.db files
local JasonQuery.security.json files
DLLs or PDBs that are build outputs or private dependencies
release ZIP files
```

Do not put secrets or sensitive production data in source files, scripts, screenshots, logs, issues, pull requests, or test fixtures.

## Keep changes focused

Prefer a small pull request that solves one problem.

Avoid combining an unrelated refactor, formatting sweep, dependency change, documentation rewrite, and behavior change in the same pull request. A narrow scope makes review, regression analysis, and rollback substantially safer.

When changing an existing architectural boundary, explain why the boundary must change rather than silently bypassing it.

## Architecture boundaries

Use [ARCHITECTURE.md](ARCHITECTURE.md) as the source of truth for the current high-level design.

Important boundaries include:

- external Oracle, PostgreSQL, SQL Server, and MySQL behavior remains separate from the internal `JasonQuery.db` store;
- direct proprietary/provider references remain constrained to projects that actually require them;
- persisted `storageFormatVersion` determines internal storage routing;
- unknown internal storage formats fail closed;
- Legacy SQLite and Modern SQLCipher runtimes remain deliberately isolated;
- Storage V1 → V2 migration is explicit, validated, journaled, and recoverable;
- the Modern SQLCipher runtime is isolated in its dedicated process;
- private/licensed build inputs remain outside source control; and
- real database integration tests remain separated from deterministic unit/regression tests.

If a contribution intentionally changes one of these invariants, call it out explicitly in the pull request and include the qualification required to demonstrate that the new boundary is safe.

## `JasonQuery.db` security, recovery, and storage changes

Changes involving any of the following require a higher level of review and evidence:

- database-key protection;
- Windows-current-user protection;
- custom-password protection;
- recovery keys or recovery flows;
- `JasonQuery.security.json`;
- Storage V1 / Storage V2 routing;
- Legacy SQLite / Modern SQLCipher isolation;
- migration journals, backups, candidates, replacement, or recovery;
- runtime identity or protocol validation; or
- fail-closed behavior.

For these areas:

1. preserve existing security and storage boundaries unless the change explicitly redesigns them;
2. add or update targeted automated regression tests;
3. test negative and interrupted states, not only the happy path;
4. avoid provider probing or exception-driven fallback for internal storage-format detection;
5. do not introduce plaintext staging for protected `JasonQuery.db` migration;
6. keep secrets, database keys, passwords, recovery material, and production databases out of evidence and Git history; and
7. describe the validation evidence in the pull request.

A security-sensitive change may require additional maintainer qualification beyond the normal contributor test gate.

## Build and validation expectations

The repository's pull request template is the checklist for every pull request.

At minimum, run the repository validation applicable to the change:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Build\Validate-Repository.ps1"
```

After the branch is pushed, the GitHub Actions **Repository Guard** and **Secret Scan** checks must pass.

Use the following test scope as a guide:

| Change type | Expected validation |
| --- | --- |
| Documentation / issue-template-only change | Repository validation; build/tests may be marked not applicable when no build or runtime input changed |
| Normal application or library code | `Release | x64` rebuild plus relevant `JasonLibrary.Tests` / `JasonQuery.Tests` |
| External Oracle / PostgreSQL / SQL Server / MySQL behavior | Relevant unit/regression coverage plus the applicable real-database integration tests |
| UI, runtime, packaging, updater, or release behavior | Relevant automated tests plus an appropriate manual smoke test |
| `JasonQuery.db` security/storage/migration/recovery | Targeted regression and negative-path qualification in addition to the normal applicable build/test gates |

Do not hard-code an expected unit-test count in contribution instructions. The tests discovered by Visual Studio Test Explorer and the current test results are the source of truth.

## Real-database integration tests

`JasonQuery.IntegrationTests` runs against real Oracle, PostgreSQL, SQL Server, and MySQL environments.

These tests are intentionally disabled unless dedicated integration-test environments and settings are configured. Do not point them at production databases.

If your change affects provider-specific SQL, metadata, transaction behavior, locking, special data types, schema behavior, or another database-specific path, run the applicable integration coverage when a qualified environment is available and report the result in the pull request.

See:

- [Tests/README.md](Tests/README.md)
- [Tests/JasonQuery.IntegrationTests/README.md](Tests/JasonQuery.IntegrationTests/README.md)

## Pull requests

Before opening a pull request:

1. start from the current `main`;
2. keep the branch and commit scope focused;
3. review `git status` and the final diff;
4. make sure no private, licensed, generated, credential, database, DLL, PDB, or release-package file is included;
5. run the validation appropriate to the change;
6. update tests and documentation when behavior or contracts change; and
7. include release-note wording when the change is user-visible and release notes are applicable.

When the pull request opens, complete the existing `.github/pull_request_template.md` rather than replacing its validation checklist with a custom format.

A documentation-only pull request may mark rebuild, unit-test, integration-test, and smoke-test items as not applicable when the change truly does not affect those areas. Explain the basis for any non-applicable gate.

## C# compatibility

Production projects are built with C# 8.0 and .NET Framework 4.8.

Do not introduce syntax or APIs that require a newer C# language version, a newer .NET runtime, or an x86-only path unless the project intentionally changes and re-qualifies those platform requirements.

Follow the style of the surrounding code and avoid unrelated formatting churn.

## Third-party code and licensing

Do not copy third-party source, binaries, icons, data, or other assets into JasonQuery without confirming that redistribution is compatible with the repository and documenting any required license or attribution.

If a contribution adds or changes a redistributable third-party component, update the relevant notice/license files and review [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

Proprietary build dependencies such as ComponentOne and Devart remain local dependencies and must not be committed to the public repository.

## Documentation

Update public documentation when a change modifies:

- supported build or dependency requirements;
- architecture or trust boundaries;
- security or vulnerability-reporting behavior;
- release or update behavior;
- contributor workflows; or
- user-visible behavior that would otherwise leave the documentation inaccurate.

Keep documentation statements tied to actual current behavior. When documenting a compatibility constant, historical path, or migration source format, distinguish it from the current production runtime behavior.

## Maintainer qualification

The maintainer may require additional evidence before merging changes that affect security, data migration, release packaging, updater behavior, database locking/transactions, or compatibility with historical JasonQuery data.

This is separate from the normal contributor build path described above.

## License

By contributing to JasonQuery, you agree that your contribution will be licensed under the repository's [LICENSE.md](LICENSE.md).
