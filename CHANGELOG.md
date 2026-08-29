# Changelog

All notable changes to JasonQuery from version 0.94 onward are documented in this file. Versions 0.27 through 0.93 predate this changelog and are covered by the historical JasonQuery Release Notes.

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
