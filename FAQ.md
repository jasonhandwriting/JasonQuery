# Frequently Asked Questions

This FAQ answers common questions about using, downloading, building, updating, and contributing to JasonQuery.

For a general introduction, see [README.md](README.md). For detailed build instructions, architecture, contribution rules, security reporting, and release history, use the linked documents throughout this page.

## General

### What is JasonQuery?

JasonQuery is a Windows x64 desktop database query and management tool for **Oracle**, **PostgreSQL**, **SQL Server**, and **MySQL**.

It combines SQL editing and execution with database-aware AutoComplete, SQL formatting, Schema Browser workflows, SQL History, connection management, diagnostics, update handling, and protected local application data.

### Which database platforms does JasonQuery support?

JasonQuery supports:

- Oracle
- PostgreSQL
- SQL Server
- MySQL

Provider-specific behavior is kept separate where the platforms differ in SQL syntax, metadata, transactions, locking, or data types.

### Is JasonQuery Windows-only?

Yes. JasonQuery is currently a Windows x64 desktop application.

The public source-build workflow is qualified on Windows 10 or Windows 11 x64 with Visual Studio 2022 and .NET Framework 4.8.

JasonQuery does not provide or support an x86 build, and the repository does not define a supported Linux or macOS build.

### Where should I download JasonQuery?

Use an official distribution channel:

- [JasonQuery website](https://jasonquery.org/)
- [GitHub Releases](https://github.com/jasonhandwriting/JasonQuery/releases)

The normal production package is `JasonQuery64.zip`.

Release notes and published SHA-256 values should be checked against the corresponding release when you need package-integrity information.

### How is JasonQuery licensed?

JasonQuery-owned source and materials are licensed under the [MIT License](LICENSE.md).

That license does not replace the licenses of third-party or proprietary components used by the application or build process. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for attribution and licensing boundaries.

## Releases and updates

### What is the difference between Production and Test releases?

JasonQuery uses **Production** and **Test** release channels.

The latest Production release is the normally supported end-user release. Test releases are intended for earlier validation and may be replaced more quickly; security support for Test releases is best effort.

For security-support policy, see [SECURITY.md](SECURITY.md).

### Can an organization distribute JasonQuery updates internally?

Yes. Production releases may include a **Company Update** package for administrators who distribute updates through an internal folder or UNC path.

This allows an organization to use JasonQuery's Company Update Folder workflow instead of requiring every workstation to retrieve the production package directly from the public download location.

### How does JasonQuery protect the update process?

JasonQuery's update workflow validates release metadata and package integrity before replacing application files.

The current workflow includes package size and SHA-256 verification, safe ZIP extraction, rejection of unsafe paths, verified backup, transactional file replacement, and rollback verification.

The separate `Updater` executable performs the file-system mutation portion of an update so the running main application does not need to overwrite itself.

## Local data and recovery

### What is `JasonQuery.db`?

`JasonQuery.db` is JasonQuery's own local application database. It is separate from the Oracle, PostgreSQL, SQL Server, or MySQL databases to which JasonQuery connects.

It stores JasonQuery application data such as saved connection information and other persistent application state.

Current storage uses **Storage V2**, a SQLCipher 4-compatible format accessed through an isolated Modern SQLite Runtime.

### What is the difference between Windows Protected and Custom Password protection?

JasonQuery supports two protection modes for `JasonQuery.db`:

- **Windows Protected** — protection is tied to the applicable Windows user protection environment.
- **Custom Password** — access is protected by a password chosen by the user.

A Recovery Key applies to Windows Protected mode. Custom Password mode continues to use the user-defined password and does not use a Recovery Key.

For architectural details, see [ARCHITECTURE.md](ARCHITECTURE.md).

### What should I do before reinstalling Windows or if a Windows user profile may be lost?

If you use **Windows Protected** mode, create and securely store a **Recovery Key in advance**.

The Recovery Key is designed for disaster-recovery cases where the original Windows profile or protection environment is no longer available. Recovery validates the `JasonQuery.db` / Recovery Key pairing and rebinds the existing logical database key to the current Windows user protection.

Recovery does not silently generate an unrelated replacement database key.

Treat `JasonQuery.db`, `JasonQuery.security.json`, and any Recovery Key as security-sensitive material. Do not post them in public issues or logs.

### What happens to older JasonQuery.db files?

Supported historical Storage V1 databases can migrate explicitly to Storage V2.

The migration path is designed to preserve the applicable security mode, logical database key, application schema, and saved connection information. Unknown storage versions fail closed instead of being guessed through provider probing or exception fallback.

See [ARCHITECTURE.md](ARCHITECTURE.md) and [CHANGELOG.md](CHANGELOG.md) for the current storage and migration model.

### How are saved database connection passwords protected?

Current saved Oracle, PostgreSQL, SQL Server, and MySQL credentials are protected by the secured `JasonQuery.db`.

Existing supported connection data is migrated from the historical credential-storage model as part of the compatibility path.

Do not include real connection passwords, exported credentials, or production `JasonQuery.db` files in public bug reports.

## Building from source

### Why does a fresh clone not build immediately?

The public repository intentionally does **not** contain every binary dependency required for a full build.

Some inputs are proprietary, separately licensed, privately maintained, or supplied as qualified local runtime binaries. Examples include MESCIUS ComponentOne WinForms, Devart .NET Framework providers, the private `IconLibrary` assembly, and qualified SQLite / SQLCipher runtime files.

Follow [BUILD.md](BUILD.md) to prepare the required `.local` dependency tree before rebuilding the solution.

### What development environment is supported?

The qualified source-build baseline is:

```text
Windows 10 or Windows 11, x64
Visual Studio 2022
.NET Framework 4.8
C# 8.0
Debug | x64
Release | x64
```

For the complete dependency versions and setup procedure, see [BUILD.md](BUILD.md).

### Do the normal automated tests require real database servers?

No.

The normal deterministic regression gate uses:

- `JasonLibrary.Tests`
- `JasonQuery.Tests`

Real Oracle, PostgreSQL, SQL Server, and MySQL environments are used by `JasonQuery.IntegrationTests`, which is intentionally separate and requires dedicated mutable integration-test databases and configuration.

Do not run integration tests against production databases.

### Why does GitHub Actions not build the complete application?

The complete application build depends on licensed or private inputs that are intentionally not stored in the public repository.

GitHub-hosted automation therefore focuses on repository validation, secret scanning, public endpoint monitoring, dependency maintenance, and release-preparation checks.

Official full release qualification remains a trusted Windows workflow with the applicable rebuild, tests, database checks, package validation, and smoke verification.

## Reporting problems and contributing

### How should I report a normal bug or request a feature?

Use the repository Issue Forms for bug reports and feature requests.

For a useful bug report, include the JasonQuery version or commit, Windows environment, affected database platform/version when relevant, clear reproduction steps, expected behavior, and actual behavior.

Remove credentials, Recovery Keys, private database contents, and other sensitive information before posting.

See [CONTRIBUTING.md](CONTRIBUTING.md) for contributor expectations.

### How should I report a security vulnerability?

Do **not** open a public issue containing security-sensitive details.

Use GitHub **Private Vulnerability Reporting** from the repository's **Security** tab and choose **Report a vulnerability**.

See [SECURITY.md](SECURITY.md) for scope, reporting guidance, and response targets.

### What sensitive information should I avoid sharing publicly?

Do not publicly post material such as:

- database passwords or complete connection strings;
- Recovery Keys or other recovery material;
- private keys or vendor activation material;
- production `JasonQuery.db` or `JasonQuery.security.json` files;
- production database contents;
- SQL History containing sensitive SQL or identifiers; or
- logs that have not been reviewed for database names, object names, error details, or other environment-specific information.

When reporting a problem, prefer synthetic data and the minimum information needed to reproduce the issue.

### Can I contribute code?

Yes.

Before opening a pull request, read [CONTRIBUTING.md](CONTRIBUTING.md) and [BUILD.md](BUILD.md), keep the change focused, run the validation appropriate to the change, and do not commit private/licensed dependencies or sensitive material.

Some changes require more than normal unit/regression tests. Security-, migration-, recovery-, updater-, transaction-, locking-, release-, and provider-sensitive changes may require additional integration, negative-path, or manual qualification.

## Documentation and history

### Where can I learn more about the project?

Use the document that matches the question:

| Document | Use it for |
| --- | --- |
| [README.md](README.md) | Product overview and public entry point |
| [CHANGELOG.md](CHANGELOG.md) | Release-level changes from v0.94 onward |
| [EVOLUTION.md](EVOLUTION.md) | Project history and major evolution milestones |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Architecture, runtime boundaries, storage, and migration model |
| [BUILD.md](BUILD.md) | Fresh-clone build environment and dependency setup |
| [Tests/README.md](Tests/README.md) | Test organization and execution |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution workflow and validation expectations |
| [SECURITY.md](SECURITY.md) | Vulnerability reporting and security policy |
| [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) | Third-party attribution and redistribution boundaries |
| [LICENSE.md](LICENSE.md) | MIT license for JasonQuery-owned source |

The current repository changelog begins at v0.94. Earlier versions were documented through historical JasonQuery Release Notes rather than the present `CHANGELOG.md`.
