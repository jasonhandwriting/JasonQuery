# JasonQuery Philosophy

JasonQuery is shaped by a set of deliberate engineering choices that have accumulated through continuous development since 2018.

This document explains **why** those choices exist. It complements [ARCHITECTURE.md](ARCHITECTURE.md), which describes the current technical structure, and [CONTRIBUTING.md](CONTRIBUTING.md), which describes how changes should be prepared and validated.

These decisions are not intended to make the project permanently static. They are durable defaults: changing one should require a concrete user-facing benefit, a clear understanding of the compatibility and maintenance cost, and qualification appropriate to the risk.

## 1. Why Windows only

JasonQuery intentionally targets **Windows x64**.

The application is built around a Windows desktop workflow and currently depends on WinForms, .NET Framework 4.8, Windows-specific application behavior, Windows user protection for local security state, and Windows-oriented deployment and update paths. The qualified build and release process is also Windows-based.

Adding another operating system would therefore be more than making the executable start elsewhere. It would create additional UI, security, packaging, filesystem, provider, installation, and regression-test matrices that would need to remain reliable over time.

For a single-maintainer project, keeping one qualified operating-system family allows engineering effort to stay focused on database behavior, data safety, compatibility, and the workflows that existing users rely on.

Cross-platform support is not currently planned. A future proposal would need to show enough user value to justify a new long-term compatibility and qualification surface.

## 2. Why WinForms

JasonQuery intentionally remains a **WinForms** application.

WinForms is not used because newer UI technologies do not exist. It is used because JasonQuery already has a mature desktop interface, established keyboard and editor workflows, ComponentOne integration, localization behavior, window and docking behavior, and years of regression experience built around the current UI stack.

A UI-framework rewrite would touch a very large part of the application while providing no automatic improvement to SQL execution, metadata accuracy, transaction safety, migration, recovery, or database compatibility. It would also create a long period in which old and new behavior would need to be compared and re-qualified.

For JasonQuery, technology replacement is not a goal by itself. A UI-platform change would need a specific user-facing benefit large enough to justify the migration cost and the risk of regressions in mature workflows.

## 3. Why explicit Commit/Rollback

JasonQuery treats transaction completion as an explicit database action.

Where JasonQuery controls a transaction-capable workflow, the application does not treat automatic commit as a universal convenience default. Commit and Rollback remain visible decisions, and pending transaction state is surfaced to the user, including periodic reminders when a transaction remains unresolved.

This design favors awareness over speed. A database tool can execute statements that affect far more data than is immediately visible in the result grid. Silently completing a transaction can therefore turn an accidental statement into a durable change before the user has had a chance to review it.

Explicit Commit/Rollback also makes transaction state easier to reason about during troubleshooting and qualification. Provider-specific behavior still matters—Oracle, PostgreSQL, SQL Server, and MySQL do not have identical transaction semantics—but JasonQuery's own UI should not hide an unresolved transaction from the user.

Changes that make transaction completion more automatic must demonstrate that they do not weaken visibility, recovery options, or provider-specific correctness.

## 4. Why a focused connection model

JasonQuery organizes the main work area around a **focused active database connection context**.

The selected connection determines the active database provider and related execution, metadata, schema, SQL History, version, and transaction context used by the working interface. Query execution is routed through the reader/provider that belongs to that active database type, while the main application keeps transaction status visible at the application level.

This model deliberately avoids pretending that every editor surface is an isolated connection universe. Keeping a clear active context reduces ambiguity about which database a command, schema lookup, history entry, or transaction state belongs to.

That simplicity has trade-offs. A design in which every editor tab owns an independently pooled connection could enable different workflows, but it would also require explicit isolation of transaction state, provider state, schema metadata, history, cancellation, reconnect behavior, UI status, and failure handling for every tab.

A future move toward more independent concurrent connection contexts would therefore be an architectural change, not a small UI enhancement. It would require dedicated design and regression qualification.

## 5. Why only Oracle, PostgreSQL, SQL Server, and MySQL

JasonQuery intentionally limits its supported **external database platforms** to Oracle, PostgreSQL, SQL Server, and MySQL.

Supporting a database means more than accepting a connection string. JasonQuery contains provider-specific behavior for SQL execution, metadata and schema discovery, data types, error interpretation, transaction handling, locking, object scripting, editor assistance, and real-database integration testing.

Every additional external database platform expands that permanent compatibility matrix. A shallow implementation that can connect but cannot be qualified across those behaviors would weaken the meaning of “supported.”

SQLite is the only additional external database platform that may be considered in the future, and that is not a committed roadmap item. JasonQuery's internal use of SQLite/SQLCipher for `JasonQuery.db` is a separate storage concern and does not imply user-facing SQLite database support.

New database-platform proposals should be evaluated against their long-term provider, UI, metadata, transaction, testing, and maintenance cost—not only the effort required to make an initial connection succeed.

## 6. Why JasonQuery uses Devart

JasonQuery's external database provider layer is currently built and qualified around **Devart .NET Framework providers**.

That choice is embedded in years of application behavior. Provider APIs participate in connection management, commands, readers, metadata, paging, errors, transactions, database-specific features, and the regression surface around all four supported database platforms.

Replacing a provider is therefore not equivalent to changing a package reference. Even when two providers implement familiar ADO.NET abstractions, differences in behavior, metadata, exceptions, type mapping, paging, transaction semantics, deployment, and edge cases can affect user-visible results.

Devart is a proprietary dependency and remains outside the public source repository under its own licensing terms. JasonQuery's MIT license does not grant rights to it, and the technical choice to use Devart is not a statement that alternative providers are unsuitable.

A future provider change is possible, but it should be treated as a compatibility migration. The expected benefit must justify the implementation and re-qualification cost across the affected databases and workflows.

## 7. Why safety is prioritized over convenience

JasonQuery gives **safety and inspectability** a higher priority than removing every confirmation, boundary, or extra step.

Examples include explicit transaction completion, visible pending-transaction state, inspectable SQL History, integrity-checked update packages, validated backups and rollback, explicit internal storage versions, fail-closed behavior for unknown storage states, and recovery procedures that verify the database/recovery-material relationship before changing protection state.

These mechanisms can require more code and sometimes more user interaction. Their purpose is to make consequential state changes visible, reviewable, and recoverable.

Database tools operate close to durable data. Convenience features are valuable when their failure mode is small; they require more scrutiny when a shortcut can silently commit data, overwrite files, select the wrong storage runtime, weaken recovery, or make an unexpected database operation difficult to explain.

The preferred JasonQuery design is therefore not “add friction everywhere.” It is: remove friction where the state transition is safe and well understood, and keep explicit controls where mistakes are expensive.

## 8. Why compatibility is treated as a contract

JasonQuery treats important compatibility behavior as an engineering contract.

Users can accumulate saved connections, settings, SQL History, editor preferences, local security state, and `JasonQuery.db` data over years of upgrades. The project also has historical storage formats and migration paths that must remain understandable even after the current runtime has moved forward.

For that reason, an internal refactor is not automatically safe simply because the new code is cleaner. Changes that affect persisted data, protection modes, provider behavior, updates, transactions, file formats, or established workflows must either preserve the existing contract or provide an explicit and qualified migration boundary.

Compatibility does not mean preserving every historical implementation detail or every bug. It means avoiding accidental breakage of data and supported workflows, documenting intentional boundaries, and providing a controlled path when a durable contract truly has to change.

This is also why historical migration code can remain isolated in the repository even when it is not part of the normal current runtime path: compatibility work sometimes requires retaining a narrowly scoped bridge to older supported states.

## 9. Why changes are qualified before they become defaults

JasonQuery prefers **evidence before default behavior changes**.

A change can compile, pass a happy-path test, and still be unsafe when interrupted, given the wrong input, run against another supported database, or applied to historical user state. Higher-risk changes therefore require validation that matches their failure modes.

The project uses deterministic regression tests, real-database integration tests, repository guards, negative-path testing, manual smoke checks, package and integrity validation, and focused qualification for areas such as security, migration, recovery, transactions, locking, updates, and releases.

Not every change requires every gate. Documentation-only work should not pretend to need a database integration test, while a storage migration should not be considered qualified merely because the solution rebuilds.

The principle is proportional evidence: identify what could break, test the important positive and negative paths, preserve a known baseline, and make the resulting evidence strong enough to justify changing the default for users.

## Applying these principles

These principles are intended to help maintainers and contributors make consistent trade-offs.

A proposal that challenges one of them is not automatically rejected. It should, however, explain:

- what user problem requires the boundary to change;
- why the benefit cannot be achieved safely within the current design;
- which compatibility, security, licensing, provider, or maintenance costs are introduced;
- how existing users and persisted state remain protected; and
- what qualification demonstrates that the new default is safe.

The goal is not to prevent JasonQuery from evolving. The goal is to make significant evolution deliberate, reviewable, and compatible with the level of trust expected from a database tool.
