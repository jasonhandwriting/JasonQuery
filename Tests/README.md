# JasonQuery Tests

The `Tests` directory contains the automated regression and real-database integration tests for JasonQuery. The suite currently contains more than 2,000 automated test cases. The exact count is intentionally not maintained in this document; Visual Studio Test Explorer and the latest test results are the source of truth.

## Test Projects

| Project | Purpose | Database required |
| --- | --- | --- |
| `JasonLibrary.Tests` | Tests shared library behavior, including SQL formatting, settings, and update metadata handling. | No |
| `JasonQuery.Tests` | Tests JasonQuery application and core behavior, including SQL generation, diagnostics, transaction policies, autocomplete, value formatting, and safety rules. | No |
| `JasonQuery.IntegrationTests` | Exercises the real Oracle, PostgreSQL, SQL Server, and MySQL providers, readers, schema metadata, special data types, transactions, and locking behavior. | Yes |

All three projects use MSTest, target .NET Framework 4.8, and run as x64 test projects.

## Prerequisites

- Visual Studio 2022 with the **.NET desktop development** workload.
- .NET Framework 4.8 Developer Pack.
- The NuGet packages and local build dependencies required by the main JasonQuery solution.
- The `Release | x64` solution configuration for release verification.
- Dedicated test databases only when running `JasonQuery.IntegrationTests`.

## Running Unit Tests

1. Open `JasonQuery.sln` in Visual Studio 2022.
2. Select the required solution configuration and the `x64` platform.
3. Open **Test > Test Explorer**.
4. Build the solution and run `JasonLibrary.Tests` and `JasonQuery.Tests`.

These two projects do not require a database and form the normal regression suite. Ordinary unit tests should be deterministic, isolated, and independent of developer-specific files or services.

## Running Integration Tests

Integration tests are disabled by default. When a database family is disabled or its required settings are missing, its tests are reported as Inconclusive/Skipped rather than Failed.

The integration suite creates and removes database objects, performs DDL and DML, commits and rolls back transactions, and verifies real locking behavior. Run it only against dedicated test databases and accounts that are safe to modify.

See [JasonQuery.IntegrationTests/README.md](JasonQuery.IntegrationTests/README.md) for configuration, safety rules, supported categories, and execution instructions.

## Test Categories

Tests use MSTest `TestCategory` attributes to support filtering in Test Explorer. Common categories include:

- Test scope: `Unit`, `Integration`, `Regression`, `Safety`
- Database: `Oracle`, `PostgreSql`, `SqlServer`, `MySql`
- Functional area: `AutoComplete`, `DdlPreview`, `Transaction`, `Locking`, `Formatter`, `Schema`, `SpecialTypes`, `Update`

The source code is the authoritative list of categories. Add a category only when it provides a useful and stable filtering boundary.

## Skipped and Inconclusive Tests

- Real-database integration tests are intentionally Inconclusive/Skipped until their database family is enabled and configured.
- Unit tests should not be skipped merely because a developer machine is missing optional local data.
- Any other skipped test should have a clear reason and should not silently weaken the release gate.

## Minimum Release Verification

Before a formal release:

1. Build the solution successfully with `Release | x64`.
2. Run all tests in `JasonLibrary.Tests` and `JasonQuery.Tests`; every discovered unit test must pass.
3. Enable and run `JasonQuery.IntegrationTests` against Oracle, PostgreSQL, SQL Server, and MySQL in the prepared release environment.
4. Confirm that no test failed and that no database family was unintentionally disabled or left unconfigured.
5. Preserve the exact test totals and results in the release evidence or test report, not in this README.

## Adding or Updating Tests

- Place tests in the project and folder that match the production component being protected.
- Follow the existing descriptive naming style, such as `Method_Condition_ExpectedResult`.
- Use `DataTestMethod` and `DataRow` when the same behavior should be verified with multiple inputs.
- Add `Unit` or `Integration` and any relevant provider or feature categories.
- Keep unit tests free of real database, network, machine, and credential dependencies.
- Keep integration test objects under the reserved `JQ_IT_` prefix and preserve cleanup in failure paths.
- Never commit passwords, real connection strings, or the local `JasonQuery.IntegrationTests.runsettings` file.
- Add regression coverage for bug fixes and safety-sensitive changes whenever practical.

This README does not need to change for every new test method. Update it only when the project structure, prerequisites, execution workflow, test categories, safety rules, or release verification policy changes.
