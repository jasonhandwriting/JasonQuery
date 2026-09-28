# Building JasonQuery

JasonQuery is a Windows x64 application targeting .NET Framework 4.8 and C# 8.0.

A fresh source clone intentionally does **not** contain every binary dependency required to build the solution. Some dependencies are proprietary, privately maintained, or supplied as qualified local runtime binaries. Prepare the `.local` dependency tree before rebuilding the solution.

This document describes the build path qualified on a clean Windows x64 virtual machine.

## 1. Supported build environment

Use:

- Windows 10 or Windows 11, x64.
- Visual Studio 2022 with the **.NET desktop development** workload.
- .NET Framework 4.8 Developer Pack.
- Git for Windows.
- ComponentOne WinForms `2020v1 (416)` / file version `4.5.20201.416`.
- Active Devart trial installations or appropriately licensed Devart .NET Framework providers.
- The local dependencies described below.

Supported solution configurations are:

```text
Debug | x64
Release | x64
```

JasonQuery does not provide or support an x86 build.

The clean-VM qualification used Visual Studio 2022 17.14. Later Visual Studio 2022 servicing updates are expected to be compatible, but the supported workflow remains Visual Studio 2022 with .NET Framework 4.8.

## 2. Local dependency model

The repository ignores `.local/`. Do not commit files from this directory.

The default dependency layout is:

```text
.local\
├─ ComponentOne\
│  ├─ C1.C1Excel.4.5.2.dll
│  ├─ C1.C1Zip.4.5.2.dll
│  ├─ C1.Win.4.5.2.dll
│  ├─ C1.Win.Bitmap.4.5.2.dll
│  ├─ C1.Win.C1Command.4.5.2.dll
│  ├─ C1.Win.C1DX.4.5.2.dll
│  ├─ C1.Win.C1Input.4.5.2.dll
│  ├─ C1.Win.C1Ribbon.4.5.2.dll
│  ├─ C1.Win.C1SuperTooltip.4.5.2.dll
│  ├─ C1.Win.C1Themes.4.5.2.dll
│  ├─ C1.Win.C1Themes.Extended.4.5.2.dll
│  ├─ C1.Win.C1TrueDBGrid.4.5.2.dll
│  ├─ C1.Win.C1TrueDBGrid.Excel.4.5.2.dll
│  ├─ C1.Win.Calendar.4.5.2.dll
│  └─ C1.Win.Ribbon.4.5.2.dll
│
├─ Devart\
│  ├─ Devart.Data.dll
│  ├─ Devart.Data.Oracle.dll
│  ├─ Devart.Data.PostgreSql.dll
│  ├─ Devart.Data.SqlServer.dll
│  ├─ Devart.Data.MySql.dll
│  └─ Devart.Data.SQLite.dll
│
├─ IconLibrary\
│  ├─ IconLibrary.dll
│  └─ IconLibrary.pdb
│
└─ SQLite\
   ├─ System.Data.SQLite.dll
   ├─ System.Threading.Tasks.Extensions.dll
   ├─ SQLite.Interop.dll
   │
   ├─ Legacy\
   │  ├─ System.Data.SQLite.dll
   │  ├─ System.Threading.Tasks.Extensions.dll
   │  └─ SQLite.Interop.dll
   │
   └─ Modern\
      └─ sqlcipher.dll
```

`IconLibrary.pdb` is not required for compilation, but the maintainer publishing workflow requires the matching PDB when producing official packages.

## 3. ComponentOne

### 3.1 Install the qualified WinForms version

Install ComponentOne through the current MESCIUS ComponentOne Control Panel.

Select:

```text
Desktop
→ WinForms Controls
→ 2020v1 (416)
→ .NET Framework 4.5.2 Controls
```

Do not use the current default ComponentOne release merely because it is newer. JasonQuery currently references the `4.5.2` assembly family and the clean-VM qualification used file version:

```text
4.5.20201.416
```

The vendor installation path is **not** part of the JasonQuery build contract. MESCIUS may change that path in future installers.

For example, the clean-VM qualification installed the files under:

```text
C:\Program Files (x86)\MESCIUS\ComponentOne\WinForms\bin\v4.5.2
```

Older installations may use a ComponentOne-branded path instead. Locate the 15 DLLs listed in the `.local\ComponentOne` tree above and copy them into:

```text
.local\ComponentOne
```

All 15 files must identify themselves as file version `4.5.20201.416`.

JasonQuery uses explicit MSBuild references with `Private=true` for all 15 files so the Release output does not depend on the vendor installation path or on assemblies being resolved from another machine-wide location.

### 3.2 Optional ComponentOne path override

Instead of `.local\ComponentOne`, set:

```text
JASONQUERY_COMPONENTONE_DIR
```

to a directory containing the same 15 qualified DLLs.

Restart Visual Studio after changing a user- or machine-level environment variable.

## 4. Devart dotConnect providers

JasonQuery uses Devart .NET Framework providers for Oracle, PostgreSQL, SQL Server, MySQL, and SQLite.

Install the required dotConnect products from Devart and activate an active trial or an appropriate paid license. The clean-VM qualification used the normal installer path with GAC installation enabled; do not select **Do not install assemblies in the GAC** when reproducing that baseline.

Current Devart installers may start as time-limited trials. JasonQuery's full development baseline is **not** qualified against an Express-only feature set. Some JasonQuery behavior depends on Devart features that are not available in every Express edition, including Oracle-specific functionality used by the application. After a trial expires, use an appropriate Devart license if you need to continue full development and testing.

Contributors are responsible for obtaining and using Devart software in accordance with Devart's licensing terms.

### 4.1 Qualified Devart baseline

The clean-VM qualification used:

| File | Qualified file version |
| --- | --- |
| `Devart.Data.dll` | `7.0.192.0` |
| `Devart.Data.Oracle.dll` | `11.2.192.0` |
| `Devart.Data.PostgreSql.dll` | `9.2.192.0` |
| `Devart.Data.SqlServer.dll` | `7.0.192.0` |
| `Devart.Data.MySql.dll` | `10.2.192.0` |
| `Devart.Data.SQLite.dll` | `7.2.192.0` |

Newer compatible Devart versions may work, but they are outside this qualified baseline until they are tested.

All provider assemblies must resolve against a compatible `Devart.Data.dll`. During the clean-VM qualification, the canonical common `Devart.Data.dll` was the assembly installed into the GAC, and each provider was verified to reference the same `Devart.Data` assembly version.

Copy the six required files into:

```text
.local\Devart
```

### 4.2 Optional Devart path override

Instead of `.local\Devart`, set:

```text
JASONQUERY_DEVART_DIR
```

to a directory containing the required Devart assemblies.

Restart Visual Studio after changing the environment variable.

## 5. IconLibrary

`IconLibrary` is a private JasonQuery dependency whose source and embedded third-party icon assets are intentionally not included in the public source repository.

Place a compatible binary at:

```text
.local\IconLibrary\IconLibrary.dll
```

For official maintainer packaging, also provide the matching:

```text
.local\IconLibrary\IconLibrary.pdb
```

The clean-VM qualification used `IconLibrary.dll` file version `1.0.0.0`.

Do not commit the private IconLibrary source, DLL, or PDB to this repository.

## 6. SQLite and SQLCipher local runtimes

JasonQuery uses three separate SQLite-related runtime roles. Do not mix their binaries.

### 6.1 Current JasonQuery SQLite runtime

Place these files directly under `.local\SQLite`:

| File | Qualified file version | Qualified SHA-256 |
| --- | --- | --- |
| `System.Data.SQLite.dll` | `1.0.112.0` | `2229240874CB86363B01BFB117E75FCB5E1A3884F59D7ECAC013DAD1D36CB730` |
| `System.Threading.Tasks.Extensions.dll` | `4.6.24705.01` | `2804E53913A5F6B8663F27A902EE8E939559C0991CE02399FF10595392F2340A` |
| `SQLite.Interop.dll` | `1.0.112.0` | `2ABB21507A37592DD97C11A035E3450D4D4E3438BEC7B62C27D096C3952DD2DB` |

This runtime uses SQLite `3.30.1`.

### 6.2 Legacy JasonQuery.db migration runtime

Place these files under `.local\SQLite\Legacy`:

| File | Qualified file version | Qualified SHA-256 |
| --- | --- | --- |
| `System.Data.SQLite.dll` | `1.0.112.1` | `19AD18AD0A128F690667C7239DBAF89629ABE43A6BB365BAC295B72A8CC26318` |
| `System.Threading.Tasks.Extensions.dll` | `4.6.24705.01` | `2804E53913A5F6B8663F27A902EE8E939559C0991CE02399FF10595392F2340A` |
| `SQLite.Interop.dll` | `1.0.112.1` | `96C952EFA25720EEC63437DF20E20B8959DDE5230C6F1D5C30BE68CF72665532` |

This isolated migration runtime uses SQLite `3.31.1`.

It exists only to read and migrate historical JasonQuery.db storage. It must not replace the normal JasonQuery SQLite runtime.

### 6.3 Modern SQLCipher native runtime

Place the qualified x64 native library at:

```text
.local\SQLite\Modern\sqlcipher.dll
```

Qualified binary:

```text
FileVersion: 3.53.3
SHA-256:     25852CE7A4067CC79E73309D26C1AD9B5706E876BBDF48BE9A25379180FB9A07
```

The qualified modern runtime is based on:

```text
SQLite:    3.53.3
SQLCipher: 4.17.0
OpenSSL:   3.5.8
```

The normal source-build workflow does not require rebuilding SQLCipher from source. It requires a compatible qualified `sqlcipher.dll` at the path above.

### 6.4 Optional SQLite path overrides

The normal SQLite directory can be overridden with:

```text
JASONQUERY_SQLITE_DIR
```

The isolated legacy SQLite directory can be overridden with:

```text
JASONQUERY_LEGACY_SQLITE_DIR
```

The modern helper/runtime projects default directly to:

```text
.local\SQLite\Modern\sqlcipher.dll
```

and also support the `ModernSqlCipherNativePath` MSBuild property when a different native path is required.

## 7. Restore NuGet packages

Open:

```text
JasonQuery.sln
```

in Visual Studio 2022.

Allow Visual Studio to restore NuGet packages. If necessary, use the Visual Studio **Restore NuGet Packages** command for the solution.

The repository intentionally does not track the restored `packages` directory.

## 8. Rebuild the solution

The qualified build path is the Visual Studio 2022 solution build.

In Visual Studio:

```text
1. Open JasonQuery.sln.
2. Select Release | x64 for release verification.
3. Choose Build > Rebuild Solution.
```

`Debug | x64` is also supported for development.

A parallel command-line solution rebuild is **not** the qualified JasonQuery build path. Some legacy projects still use file references whose ordering is handled reliably by the Visual Studio solution workflow but can race during a parallel command-line rebuild.

Do not work around build failures by copying DLLs directly into `bin\Release`. Fix the corresponding `.local` dependency instead and rebuild the solution.

## 9. Expected Release output

A successful `Release | x64` rebuild produces:

```text
JasonQuery\bin\Release\JasonQuery.exe
```

and the required runtime dependencies.

The build also produces isolated database-storage runtime components, including:

```text
JasonQuery\bin\Release\StorageMigration\Legacy\JasonQuery.LegacyDbMigration.exe
JasonQuery\bin\Release\StorageMigration\Modern\JasonQuery.ModernDbMigration.exe
JasonQuery\bin\Release\ModernRuntime\JasonQuery.ModernDbRuntime.exe
```

The 15 ComponentOne assemblies and six Devart assemblies must be copied locally into the main Release output from the configured local dependency paths.

If stale binaries remain from an older build, clean the solution, remove the affected `bin` / `obj` output, and rebuild.

## 10. Run automated tests

Use Visual Studio Test Explorer.

For the normal source-build regression gate, run:

```text
JasonLibrary.Tests
JasonQuery.Tests
```

Every test discovered in these two projects must pass. Do not hard-code an expected test count; Visual Studio Test Explorer and the current test results are the source of truth.

`JasonQuery.IntegrationTests` uses real Oracle, PostgreSQL, SQL Server, and MySQL databases. It is disabled unless dedicated integration-test environments and settings are configured.

For the complete testing policy and integration-test setup, see:

```text
Tests\README.md
Tests\JasonQuery.IntegrationTests\README.md
```

## 11. Fresh-install smoke test

For an additional runtime check after a successful `Release | x64` rebuild, start:

```text
JasonQuery\bin\Release\JasonQuery.exe
```

On a fresh output with no existing user database/security state, the qualified behavior is:

- JasonQuery starts without a missing-dependency dialog.
- `ConnectionForm` appears normally.
- A recovery prompt is not shown for a new installation.
- A custom-password prompt is not shown for a new installation.
- No database-security startup error is displayed.
- `JasonQuery.db` and `JasonQuery.security.json` are created as the new local security state.
- JasonQuery exits normally.

A source build is not considered fully reproduced if the solution compiles but the Release executable cannot pass this basic startup smoke test.

## 12. Licensing and repository hygiene

ComponentOne and Devart are proprietary third-party dependencies. They are not included in the source repository. Obtain them directly from their vendors under licenses or trials appropriate for your use.

`IconLibrary` is also intentionally excluded from source distribution.

The `.local` directory is ignored by Git and must remain untracked.

Do not commit:

```text
.local\
IconLibrary\
vendor activation keys
trial activation keys
database passwords
integration-test credentials
local JasonQuery.db files
local security metadata
```

Never place vendor activation keys in scripts, screenshots, logs, issues, pull requests, or repository files.

For third-party license and attribution information, see:

```text
THIRD-PARTY-NOTICES.md
Build\Legal\RuntimeLicenses\
```

## 13. Maintainer release verification

Before producing an official JasonQuery release, the maintainer should additionally:

1. Rebuild `Release | x64` successfully in the trusted release VM.
2. Run every discovered test in `JasonLibrary.Tests` and `JasonQuery.Tests`.
3. Run the configured Oracle, PostgreSQL, SQL Server, and MySQL integration-test gate.
4. Perform the final startup/runtime smoke checks.
5. Run the repository and package validators used by the publishing workflow.
6. Confirm the official package contains the required license and notice files.
7. Confirm packaged runtime files are byte-identical to the validated Release output where required by the release validator.

The maintainer publishing workflow may impose stricter requirements than an ordinary contributor source build, including the presence of `IconLibrary.pdb`, release evidence, and package-level legal validation.
