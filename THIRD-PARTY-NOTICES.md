# Third-Party Notices

JasonQuery is licensed under the MIT License as described in
[`LICENSE.md`](LICENSE.md).

JasonQuery also incorporates, depends on, or is built with third-party
software that remains subject to its own copyright and license terms.
The JasonQuery MIT License does not replace, override, or extend those
third-party terms.

This document provides a central inventory and attribution reference for
third-party components used by JasonQuery. Where a component has a dedicated
license or notice file in this repository, that file contains the applicable
terms and takes precedence over this summary.

## Source Code Included in This Repository

### Magic Library (MagicDock)

JasonQuery includes a vendored and locally maintained copy of Magic Library,
originally developed by Crownwood Consulting Ltd.

- Component: Magic Library / MagicDock
- Original developer: Crownwood Consulting Ltd.
- Original copyright: Copyright © 2003 Crownwood Consulting Ltd.
- License: Original Magic Library legacy license and usage terms
- Repository location: `MagicLibrary/`
- License: [`MagicLibrary/LICENSE.txt`](MagicLibrary/LICENSE.txt)
- Additional notice:
  [`MagicLibrary/THIRD-PARTY-NOTICES.md`](MagicLibrary/THIRD-PARTY-NOTICES.md)

Magic Library is not licensed under the JasonQuery MIT License. JasonQuery
contains local integration, maintenance, and compatibility changes, but those
changes do not alter ownership or the original licensing terms of Magic
Library.

JasonQuery acknowledges its use of Magic Library in the application's About
dialog.

### ScintillaNET

JasonQuery includes a vendored and locally maintained copy of ScintillaNET,
used as the Windows Forms SQL editor control.

- Component: ScintillaNET
- Original developer: Jacob Slusser and contributors
- License: MIT License
- Repository location: `ScintillaNET/`
- Additional notice:
  [`ScintillaNET/THIRD-PARTY-NOTICES.md`](ScintillaNET/THIRD-PARTY-NOTICES.md)

JasonQuery contains local integration and maintenance changes. The original
ScintillaNET copyright and license continue to apply to the corresponding
third-party source.

### Scintilla / SciLexer

ScintillaNET uses the native Scintilla / SciLexer editing component.

- Component: Scintilla / SciLexer
- Original developer: Neil Hodgson
- License: Scintilla and SciTE license
- JasonQuery architecture: x64 only
- Repository location: `ScintillaNET/x64/`
- License: [`ScintillaNET/x64/License.txt`](ScintillaNET/x64/License.txt)

The compressed native resource `ScintillaNET/x64/SciLexer.dll.gz` remains
subject to the Scintilla license.

### Be.HexEditor / HexBox

JasonLibrary contains vendored and locally modified source derived from
Be.HexEditor, including the `Be.Windows.Forms.HexBox` control and related bit
controls.

- Component: Be.HexEditor / HexBox
- Original developer: Bernhard Elbl
- License: MIT License
- Repository location: `JasonLibrary/UI/Controls/HexBox/`
- License:
  [`JasonLibrary/UI/Controls/HexBox/LICENSE.txt`](JasonLibrary/UI/Controls/HexBox/LICENSE.txt)
- Additional information:
  [`JasonLibrary/UI/Controls/HexBox/README.md`](JasonLibrary/UI/Controls/HexBox/README.md)

The original author retains copyright in the upstream source. JasonQuery and
JasonLibrary contain local integration and maintenance changes.

### ScintillaNET-FindReplaceDialog

Parts of JasonQuery's Find and Replace implementation are derived from the
ScintillaNET-FindReplaceDialog project by Steve Towner.

- Component: ScintillaNET-FindReplaceDialog
- Original developer: Steve Towner
- Original copyright: Copyright (c) 2017 Steve Towner
- License: MIT License
- Upstream project:
  https://github.com/Stumpii/ScintillaNET-FindReplaceDialog
- Derived and adapted code includes:
  - `JasonQuery/Editor/FindAndReplace/`
  - `JasonQuery/UI/Forms/FindAndReplaceForm.cs`
  - `JasonQuery/UI/Forms/FindAndReplaceForm.Designer.cs`
  - `JasonQuery/UI/Forms/FindAndReplaceForm.resx`
- Local license:
  `JasonQuery/Editor/FindAndReplace/LICENSE.txt`

The upstream implementation has been adapted and integrated into JasonQuery.
The original copyright and MIT License continue to apply to the portions
derived from ScintillaNET-FindReplaceDialog.

## Open-Source Package and Runtime Dependencies

### Cyotek Windows Forms Color Picker

JasonLibrary uses the Cyotek Windows Forms Color Picker NuGet package.

- Package: `Cyotek.Windows.Forms.ColorPicker`
- Version: 1.7.2
- Copyright: Copyright © 2013-2021 Cyotek Ltd.
- License: MIT License
- Upstream project:
  https://github.com/cyotek/Cyotek.Windows.Forms.ColorPicker
- Existing JasonLibrary notice:
  [`JasonLibrary/THIRD-PARTY-NOTICES.md`](JasonLibrary/THIRD-PARTY-NOTICES.md)

### Hogimn SQL Formatter

JasonLibrary uses Hogimn SQL Formatter as one of JasonQuery's SQL formatting
engines.

- Package: `Hogimn.Sql.Formatter`
- Version: 2.0.4
- Original developer: Hoki Min
- License: MIT License
- Upstream project:
  https://github.com/hogimn/sql-formatter

### Microsoft SQL Server Transact-SQL ScriptDom

JasonLibrary uses Microsoft SQL Server Transact-SQL ScriptDom as a SQL Server
parser and formatting engine.

- Package: `Microsoft.SqlServer.TransactSql.ScriptDom`
- Version: 180.102.0
- Copyright holder: Microsoft Corporation
- License: MIT License
- Upstream project:
  https://github.com/microsoft/SqlScriptDOM

### Newtonsoft.Json

JasonQuery uses Newtonsoft.Json for JSON processing.

- Package: `Newtonsoft.Json`
- Version: 13.0.4
- Original developer: James Newton-King
- License: MIT License
- Upstream project:
  https://github.com/JamesNK/Newtonsoft.Json

### SQLitePCLRaw

The modern JasonQuery.db migration and runtime processes use SQLitePCLRaw to
access the qualified SQLCipher native runtime.

- Packages:
  - `SQLitePCLRaw.core`
  - `SQLitePCLRaw.provider.sqlcipher`
- Version: 3.0.5
- Copyright holder: SourceGear, LLC and contributors
- License: Apache License 2.0
- Upstream project:
  https://github.com/ericsink/SQLitePCL.raw

SQLitePCLRaw includes its own upstream NOTICE information. Those notices and
the Apache License 2.0 continue to apply.

### System.Data.SQLite

JasonQuery uses qualified System.Data.SQLite runtimes for historical
JasonQuery.db compatibility and isolated legacy migration responsibilities.

The characterized JasonQuery legacy runtimes use System.Data.SQLite 1.0.112.x
and the corresponding SQLite native interop components.

- Component: System.Data.SQLite
- License status: Public Domain for the core System.Data.SQLite provider and
  its associated SQLite code
- Upstream project: System.Data.SQLite

Certain separate upstream System.Data.SQLite LINQ / SQL Generation source
files are distributed by the upstream project under the Microsoft Public
License. JasonQuery's qualified database runtime uses the core provider
runtime rather than those LINQ / Entity Framework components.

### SQLite

SQLite is used by JasonQuery through the qualified SQLite database runtimes.

- Legacy characterized SQLite engine: 3.30.1
- Modern SQLCipher SQLite base: 3.53.3
- License status: Public Domain
- Upstream project: SQLite

### SQLCipher

JasonQuery Storage V2 uses a qualified native SQLCipher runtime for encrypted
`JasonQuery.db` storage.

The current runtime contract validates the native runtime identity before use.

- Component: SQLCipher
- Qualified SQLCipher version: 4.17.0
- Copyright holder: ZETETIC LLC
- License: BSD 3-Clause-style license
- Upstream project:
  https://github.com/sqlcipher/sqlcipher
- Local build location:
  `.local/SQLite/Modern/sqlcipher.dll`

The qualified native binary is not committed to this repository.

SQLCipher incorporates SQLite and uses a cryptographic provider. Their
respective licenses remain applicable independently of the SQLCipher license.

### OpenSSL

The qualified JasonQuery SQLCipher runtime uses OpenSSL as its cryptographic
provider.

- Component: OpenSSL
- Qualified runtime version: OpenSSL 3.5.8
- License: Apache License 2.0
- Upstream project:
  https://github.com/openssl/openssl

The OpenSSL license remains applicable wherever OpenSSL code is incorporated
into or distributed with the qualified SQLCipher runtime.

### System.Threading.Tasks.Extensions

Qualified local SQLite runtime dependencies used by JasonQuery may include
`System.Threading.Tasks.Extensions`.

- Component: System.Threading.Tasks.Extensions
- Copyright holder: Microsoft Corporation and .NET contributors
- License: MIT License
- Distribution source: supplied as part of the qualified local SQLite runtime
  dependency set

## Development and Test Dependencies

The following packages are used by JasonQuery's automated test projects and
are not part of JasonQuery's own source license.

### Microsoft.NET.Test.Sdk

- Package: `Microsoft.NET.Test.Sdk`
- Version: 17.14.1
- Copyright holder: Microsoft Corporation
- License: MIT License
- Upstream project:
  https://github.com/microsoft/vstest

### MSTest

- Packages:
  - `MSTest.TestAdapter`
  - `MSTest.TestFramework`
- Version: 3.11.1
- Copyright holder: Microsoft Corporation
- License: MIT License
- Upstream project:
  https://github.com/microsoft/testfx

## Proprietary and Separately Licensed Dependencies

The following dependencies are intentionally not licensed under the JasonQuery
MIT License and are not committed to this repository as redistributable source
or general-purpose binary dependencies.

### ComponentOne WinForms

JasonQuery uses ComponentOne WinForms controls and related runtime components.

ComponentOne is proprietary commercial software. A developer who builds
JasonQuery from source must obtain and use ComponentOne under an appropriate
license from its vendor.

JasonQuery's MIT License grants no rights to ComponentOne software.

### Devart Database Providers

JasonQuery uses separately licensed Devart data-provider assemblies for its
supported database connections.

The build currently references Devart provider components for Oracle,
PostgreSQL, SQL Server, MySQL, and SQLite-related functionality.

These assemblies are proprietary commercial software and are intentionally
not committed to the repository. Developers must obtain and use them under
the applicable Devart license.

JasonQuery's MIT License grants no rights to Devart software.

### IconLibrary

JasonQuery uses a private `IconLibrary` assembly containing icon resources
that include third-party licensed materials.

The IconLibrary source project and its licensed icon assets are intentionally
not included in the public JasonQuery source repository.

A compatible, legally obtained `IconLibrary.dll` is required by the current
build. The official JasonQuery binary distribution may include the compiled
runtime assembly only in accordance with the applicable licenses for its
contents.

JasonQuery's MIT License grants no independent rights to the underlying
third-party icon assets.

## NuGet and Transitive Dependencies

NuGet packages may themselves depend on additional packages. Those transitive
dependencies remain subject to their respective licenses even when they are
restored automatically during a JasonQuery build.

The package versions declared by the JasonQuery solution and the licenses
shipped with the corresponding NuGet packages are authoritative for a
particular build.

Before publishing an official JasonQuery binary release, the actual release
output should be checked against this notice so that newly introduced runtime
or transitive dependencies are not omitted.

## Local Modifications

Where JasonQuery contains modified third-party source code, the original
authors retain copyright in their contributions.

JasonQuery-specific modifications do not remove or replace the original
copyright notices, license requirements, attribution requirements, or
redistribution conditions that apply to the third-party portions.

## Trademarks

Third-party product names, project names, company names, and trademarks are
used only to identify the corresponding software and its origin.

Their inclusion in JasonQuery does not imply sponsorship, endorsement, or
affiliation with the JasonQuery project.

## License Precedence

The root [`LICENSE.md`](LICENSE.md) applies to JasonQuery-owned source and
materials except where another license or notice applies.

For third-party software, the applicable upstream license and any local copy
of that license take precedence over the JasonQuery MIT License.
