# Changelog

All notable changes to JasonQuery from version 0.94 onward are documented in this file. Versions 0.27 through 0.93 predate this changelog and are covered by the historical JasonQuery Release Notes.

## [0.96] - 2026-09-29

### New Features

1. Added a JasonQuery.db Recovery Key for disaster recovery
1.1. Windows Protected mode can now create a Recovery Key in advance. If the original Windows user profile, DPAPI binding, or operating-system environment is no longer available, the Recovery Key can be used to restore access to JasonQuery.db.
1.2. Recovery verifies the JasonQuery.db and Recovery Key pairing, preserves the existing logical database key, and rebinds it to the current Windows user protection. Recovery does not generate a new database key or re-key the database.
1.3. Incorrect, mismatched, or tampered Recovery Keys are rejected without incorrectly modifying the existing JasonQuery.db or its security metadata.
1.4. Recovery Keys can be regenerated or disabled. Regeneration invalidates the previous Recovery Key, and disabling recovery prevents the existing key from being used again.
1.5. Recovery Keys are specific to Windows Protected mode. Custom Password mode continues to use the user-defined password and does not use a Recovery Key.
1.6. Recovery also covers supported historical Storage V1 states. After successful recovery, JasonQuery can complete the Storage V1 → Storage V2 upgrade and use the new Storage V2 runtime on subsequent startups.
2. SQL History - Added automatic cleanup
2.1. Added SQL History automatic cleanup settings to the General tab in Options. The feature can be enabled independently for each database connection.
2.2. Automatic cleanup is disabled by default, so existing and newly created connections will not have SQL History deleted unless the user explicitly enables the feature.
2.3. When enabled, users can retain SQL History for the most recent 30, 60, 90, 180, or 365 days. Entries older than the selected retention period are automatically removed.
2.4. Automatic cleanup applies only to the current database connection and does not remove SQL History belonging to other connections.
2.5. Improved the Options settings persistence flow to significantly reduce the time required to apply large numbers of settings and ensure JasonQuery can close normally after SQL History automatic cleanup is enabled.

### Enhancements

1. Modernized JasonQuery.db security and storage architecture
1.1. New JasonQuery.db storage uses Storage V2 with a SQLCipher 4-compatible format and an isolated Modern SQLite Runtime, protected by an independently generated logical database key.
1.2. An isolated Legacy Storage V1 compatibility runtime is retained so supported historical JasonQuery.db files can migrate directly to Storage V2. Migration does not create a plaintext staging database, and legacy and modern SQLite runtimes remain separated into different processes.
1.3. Added an explicit persisted storageFormatVersion contract. JasonQuery selects the correct runtime from the stored format version, fails closed for unknown versions, and does not guess the database format through provider probing or exception fallback.
1.4. Storage V1 → V2 migration preserves the existing security mode, logical database key, application schema, and saved connection information. A physical storage-format upgrade does not silently change the user's security configuration.
1.5. Added direct upgrade support for historical Default Password and Custom Password JasonQuery.db files. Legacy Default migrates to Windows Protected mode; Legacy Custom Password preserves the same user password while migrating to the new Custom Password protection. Incorrect passwords or migration failures stop startup without modifying the original JasonQuery.db.
1.6. Strengthened protection-mode switching and Custom Password changes with validated candidate databases, backups, transition journals, verification, and deterministic recovery for interrupted or ambiguous states.
1.7. Improved startup errors and backup/transfer guidance when Windows Protected data cannot be unlocked. JasonQuery.db and JasonQuery.security.json should be treated as a matched pair when backing up or moving an installation.
1.8. Consolidated the JasonQuery.db security and startup-error interfaces, retired legacy security dialogs, and updated English, Traditional Chinese, and Simplified Chinese wording to refer explicitly to JasonQuery.db so the messages are not confused with connected Oracle, PostgreSQL, SQL Server, or MySQL databases.
2. Improved security for saved database connection passwords
2.1. Existing DBInfo.Password values are automatically migrated from the historical per-field storage format to the new credential-storage model. Saved Oracle, PostgreSQL, SQL Server, and MySQL credentials remain usable after upgrade without requiring users to re-enter their passwords.
2.2. Saved connection passwords are now protected by the secured JasonQuery.db itself instead of applying the historical TripleDES/DES, DomainUser binding, and additional encoding to DBInfo.Password. Password writes also use parameterized SQL.
2.3. Preserved compatibility with the existing .jqc connection import/export format while strengthening temporary-file cleanup on success, wrong-password, and exception paths to reduce the risk of sensitive connection data remaining in the system temporary directory.
3. Runtime Logging - Improved TraceLogger v2 diagnostics and log management
3.1. Replaced the single, repeatedly overwritten JasonQuery.log file with a separate UTF-8 BOM CSV file for each logging session, allowing logs to be opened, filtered, and compared directly in Excel.
3.2. Added a fixed structured schema that records operation start and end events, parent-child relationships, execution duration, memory usage, database and object context, and error details to help identify performance bottlenecks and failure locations.
3.3. Improved thread-safe logging, nested-operation tracing, and logging lifecycle management. JasonQuery records the session completion status and releases the log file during a normal application shutdown.
3.4. Added session timestamps and Process IDs to log filenames to prevent collisions between application instances. Matching JasonQuery log files older than seven days are automatically removed, while recent and unrelated files are preserved.
3.5. Runtime logs do not intentionally record passwords, complete connection strings, SQL parameter values, or returned data rows. Before attaching a log to a public issue, users should still review database names, connection names, object names, and error messages that may appear in diagnostic fields.
4. Schema Browser - Improved tab content loading with Lazy Load
4.1. SQL Pane, Table/View Structure, the first 100 View rows, and the first 500 Table rows are now loaded only when the corresponding tab is opened for the first time, avoiding unnecessary queries and data processing.
4.2. The currently selected tab is preserved when switching database objects. If the new object does not support that tab, such as selecting a Function while Table Data is active, JasonQuery automatically returns to SQL Pane.
4.3. A successfully loaded object/tab combination is not queried again. If loading fails, the same tab can be retried after the cause has been corrected.
4.4. A wait message and busy cursor are displayed while JasonQuery retrieves information and prepares the Grid, providing clear feedback that loading is in progress.
4.5. Selecting a large Table no longer automatically retrieves its first 500 rows, reducing unnecessary waiting time. The data query runs only when the user explicitly opens the Table Data tab.
4.6. Added separate timing stages for SQL execution, KeyInfo, DataPage, data arrangement, and Grid formatting, while ensuring that an already loaded SQL script remains intact after other content tabs are opened.
5. Improved Windows version information and error diagnostics
5.1. Added a concise Windows version name to message-box captions and automatically omitted unavailable database version fields to prevent unnecessary separators.
5.2. Enhanced TraceLogger session logs with the full Windows edition, version, and build information while retaining the CLR version and process architecture for easier diagnosis of user-reported compatibility issues.
6. Improved Updater diagnostics and localized layout
6.1. Added an independent version identifier to the Updater window title so the running Updater build can be identified more easily. The Company Update Folder path field is also positioned dynamically after its localized label to prevent text overlap.
7. SQL Formatting - Updated the Microsoft SQL ScriptDOM component
7.1. Updated Microsoft.SqlServer.TransactSql.ScriptDom from version 180.78.1 to 180.102.0 to incorporate maintenance fixes and improve the compatibility and stability of the Microsoft SQL ScriptDOM formatting engine.
8. Improved JasonQuery.db internal architecture and regression coverage
8.1. Renamed internal JasonQuery.db security, storage-migration, and recovery classes, enums, namespaces, files, and tests from generic Database* identifiers to the clearer JasonQueryDb* naming, reducing ambiguity with user-connected databases and improving maintainability.
8.2. Significantly expanded regression coverage for Storage V1/V2 migration, runtime routing, security transitions, Recovery, connection-credential migration, and compatibility. The current complete automated test baseline contains 2,987 tests.

### Bug Fixes

1. SQL Editor - Fixed tab file names when dragging multiple SQL files
1.1. Fixed an issue where only the last tab displayed the correct file name when multiple SQL files were dragged from Windows File Explorer into the editor. Each newly opened tab now completes file loading and initialization correctly and displays its corresponding SQL file name.
2. Connection Export
2.1. Fixed an issue where connection export could fail to use the current selections shown in the connection Grid. JasonQuery now determines the exported connections from the live checkbox state, preventing selected connections from being omitted or an incorrect number of connections from being exported.


## [0.95] - 2026-08-30

### Enhancements

1. QueryForm - SQL Editor: Improved AutoComplete Error Feedback
1.1. When AutoComplete cannot retrieve suggestions because of an SQL error, a missing table, or another database error, a concise message directs users to SQL History for details.
1.2. The message is displayed only when the candidate query actually fails; no error is shown when no suggestions are available.
1.3. The message is cleared automatically after 15 seconds, or immediately after AutoComplete succeeds following an SQL correction.
2. Update System and Publishing Improvements
2.1. Added configurable update information sources
2.1.1. Added the JasonQuery Website, GitHub Releases, and Company Update Folder sources to the global Update Settings page.
2.1.2. Set the JasonQuery Website as the recommended update source.
2.1.3. The GitHub Releases source is temporarily unavailable and will be enabled after the official open-source releases are published.
2.1.4. Retained Company Update Folder support for enterprise, offline, and local-folder update environments.
2.1.5. Added validation for company update folders, metadata files, and saved update-source settings.
2.1.6. Expanded localized Company Update Folder help with IT administrator deployment steps, required files, local and UNC path guidance, and read-only access recommendations.
2.2. Improved the update-check workflow
2.2.1. JasonQuery now uses structured jasonquery-update.json metadata containing the version, release channel, download location, package size, and SHA-256 digest.
2.2.2. Added Production and Test release-channel selection rules to prevent incompatible updates and automatic downgrades.
2.2.3. Scheduled checks are marked as completed only after the metadata check succeeds; network, file, and JSON failures no longer incorrectly advance the schedule.
2.2.4. Improved localized update-source, error-reason, and metadata-location messages in English, Traditional Chinese, and Simplified Chinese.
2.2.5. Preserved the existing jq.txt format so previously deployed JasonQuery versions can continue using the legacy update-check mechanism.
2.3. Added a secure transactional update workflow
2.3.1. Update packages are verified using their expected size and SHA-256 digest before installation.
2.3.2. Added safe ZIP extraction that rejects directory traversal, absolute paths, symbolic links, duplicate paths, and unsafe Windows file names.
2.3.3. Updates are applied through a temporary Worker so Updater.exe can be safely replaced as part of the same transaction.
2.3.4. A verified backup is created before installation, with original file state, size, and SHA-256 information recorded in backup-manifest.json.
2.3.5. If an update fails, modified files are automatically restored and both backup and restored files are verified again using SHA-256.
2.3.6. User data, including JasonQuery.db, log, and backup content, is protected from update-package replacement.
2.3.7. The latest three fully verified update backups are retained.
2.4. Improved publishing and metadata automation
2.4.1. Official and test publishing workflows automatically read the version from JasonQuery.exe and classify it as a Production or Test release.
2.4.2. Publishing automatically calculates the release ZIP size and SHA-256 digest and generates jasonquery-update.json.
2.4.3. The legacy jq.txt file is generated at the same time to retain compatibility with existing installations.
2.4.4. Existing metadata is preserved if metadata generation or replacement fails, preventing a failed publication from damaging the currently available update information.
2.4.5. Remote metadata and package download URLs are required to use HTTPS.
2.4.6. Standardized official website, update metadata, download, release-note, and bug-report links on the canonical bare domain https://jasonquery.org/.
2.4.7. Added a Production-only IT administrator offline update package containing jasonquery-update.json, JasonQuery64.zip, and README-Company-Update.txt.
2.5. Improved Updater build and release integrity
2.5.1. Visual Studio builds now automatically copy Updater.exe and Updater.pdb to the corresponding JasonQuery Debug or Release output directory.
2.5.2. The release-package validator requires Updater.exe and Updater.pdb to be present in both the Release output and publication ZIP.
2.5.3. The Updater artifacts in the Release output and ZIP package are verified as byte-identical using SHA-256.
2.6. Completed full publishing and update regression testing
2.6.1. Added end-to-end coverage from an isolated old installation through metadata loading, ZIP payload resolution, package verification, safe extraction, transactional installation, verified backup, and restoration.
2.6.2. Verified that JasonQuery restarts successfully after an update and that Worker logs, the backup Manifest, and installed files are correct.
2.6.3. Verified that a tampered package is rejected for a SHA-256 mismatch before any installation content is modified.
2.6.4. Verified that a package containing protected paths such as JasonQuery.db is rejected before backup creation or installation changes.
2.6.5. Verified that JasonQuery.db and user content under log and backup remain unchanged throughout update and restoration operations.
2.6.6. Completed regression testing of the official publishing workflow, Release package validation, actual Updater execution, and the already-running-latest-version UI state.

### Bug Fixes

1. QueryForm - SQL Editor: AutoComplete
1.1. Fixed an issue where, when a derived table or subquery selected identically named columns from different source tables, the candidate list triggered by an alias followed by a period—for example, demo.—could incorrectly reuse the metadata of the last matching column for every item. Each candidate now preserves its correct Source Table, data type, and nullability metadata.
2. Fixed the UTF-8 BOM encoding status display
2.1. Fixed an issue where UTF-8 files containing a byte order mark were shown as “UTF-8” in the status bar. They are now correctly displayed as “UTF-8 BOM”, while UTF-8 files without a BOM continue to display as “UTF-8”, allowing the file encoding format to be identified accurately.


## [0.94] - 2026-08-20

### Enhancements

1. Improved SQL formatting and list layout
1.1. When Oracle uses Hogimn SQL Formatter, SELECT, FROM, WHERE, GROUP BY, HAVING, ORDER BY, JOIN, ON, AND, and OR now follow a more conventional Oracle-aligned layout.
1.2. Added a shared List Items Per Line setting with values from 1 to 10 and a default of 3. It applies to Hogimn SQL Formatter for Oracle, PostgreSQL, SQL Server, and MySQL, as well as Microsoft SQL ScriptDOM Formatter for SQL Server.
1.3. The List Items Per Line setting applies to SELECT, GROUP BY, ORDER BY, and nested queries. Both SQL Server formatter engines now support the same setting.
1.4. PostgreSQL, SQL Server, and MySQL retain the existing clause layout of their respective formatter engines while redistributing list items according to the configured value.
1.5. Strengthened semantic safety for list formatting to prevent incorrect splitting of function arguments, IN lists, STRING_AGG, window functions, comments, string literals, and quoted identifiers while continuing to honor the Inline Block Max Length setting.
1.6. Improved spacing between multiple SQL statements so that both Hogimn SQL Formatter and Microsoft SQL ScriptDOM Formatter correctly apply the Blank Lines Between Statements setting.
2. Main Form and SQL Editor Operations
2.1. Simplified the Main Form menu by removing the Edit menu, which duplicated the SQL editor’s context menu. Save and Save As on the File menu now operate directly on the active SQL editor tab, with consistent Ctrl+S and F12 shortcut behavior.

### Bug Fixes

1. Query Form - Query Execution
1.1. Fixed an issue where the query timeout setting was not fully applied to non-query commands. Single and batch data modification commands for Oracle, PostgreSQL, SQL Server, and MySQL now honor the configured query timeout.
1.2. Fixed an issue where pressing Space after WHERE, AND, or OR in an UPDATE statement did not trigger SPACE AutoComplete or display column suggestions for the target table.
1.3. Fixed an issue where cancelling a DML statement waiting for a database lock could incorrectly display the transaction as not yet committed or rolled back. If no transaction was pending before execution, the cancelled transaction is ended; any transaction that was already pending is preserved.
2. Minor bug fixes and improvements
