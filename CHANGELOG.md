# Changelog

All notable changes to JasonQuery from version 0.94 onward are documented in this file. Versions 0.27 through 0.93 predate this changelog and are covered by the historical JasonQuery Release Notes.

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
