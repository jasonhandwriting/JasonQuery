using System;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal static class ColumnDdlSqlBuilder
    {
        public static string Build(ColumnDdlRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            if (request.DataSourceType == DataSourceType.Oracle && request.RequireOracleDdlTargetIdentity
                && !OracleDdlTargetIdentityPolicy.TryQuoteOwner(request.OracleTargetIdentity, out _))
            {
                return string.Empty;
            }

            switch (request.Operation)
            {
                case ColumnDdlOperation.Comment:
                    {
                        return BuildComment(request);
                    }
                case ColumnDdlOperation.Drop:
                    {
                        return BuildDrop(request);
                    }
                case ColumnDdlOperation.Rename:
                    {
                        return BuildRename(request);
                    }
                case ColumnDdlOperation.Add:
                    {
                        return BuildAdd(request);
                    }
                case ColumnDdlOperation.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        //Definition-free modern MySQL rename. Callers must first authorize this syntax with a verified server VERSION() on the same physical connection.
        //Do not replace the legacy Build(Rename) contract; older servers need CHANGE.
        internal static string BuildMySqlRenameColumnSql(ColumnDdlRequest request)
        {
            if (request == null || request.DataSourceType != DataSourceType.MySql
                || request.Operation != ColumnDdlOperation.Rename
                || DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(DataSourceType.MySql, request.ColumnName, request.NewColumnName))
            {
                return string.Empty;
            }

            var tableName = BuildQualifiedTableName(request);
            var oldName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);
            var newName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(DataSourceType.MySql, request.NewColumnName);

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                return string.Empty;
            }

            return $"ALTER TABLE {tableName} RENAME COLUMN {oldName} TO {newName};";
        }

        private static string BuildComment(ColumnDdlRequest request)
        {
            var comment = EscapeSqlString(request.Comment);

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        //D1-B3-F01: the DDL Owner must be the Owner matched by Metadata.
                        var owner = request.RequireOracleDdlTargetIdentity ? QuoteTrustedOracleOwner(request) : OracleCommentOwnerIdentityPolicy.QuoteDdlOwner(request.SchemaName);
                        var table = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.Oracle, request.TableName);
                        var column = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.Oracle, request.ColumnName);
                        var columnName = string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column) ? string.Empty : owner + "." + table + "." + column;

                        return string.IsNullOrEmpty(columnName) ? string.Empty : $"COMMENT ON COLUMN {columnName} IS '{comment}';";
                    }
                case DataSourceType.PostgreSql:
                    {
                        var columnName = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(request.DataSourceType, request.SchemaName, request.TableName, request.ColumnName);

                        return string.IsNullOrEmpty(columnName) ? string.Empty : $"COMMENT ON COLUMN {columnName} IS '{comment}';";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, request.HasExistingSqlServerComment ? "sp_updateextendedproperty" : "sp_addextendedproperty");
                        var schemaValue = GetIdentifierValue(DataSourceType.SqlServer, request.SchemaName);
                        var tableValue = GetIdentifierValue(DataSourceType.SqlServer, request.TableName);
                        var columnValue = GetIdentifierValue(DataSourceType.SqlServer, request.ColumnName);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(schemaValue) || string.IsNullOrEmpty(tableValue) || string.IsNullOrEmpty(columnValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@name=N'MS_Description', "
                               + $"@value=N'{comment}', "
                               + $"@level0type=N'SCHEMA', "
                               + $"@level0name=N'{EscapeSqlString(schemaValue)}', "
                               + $"@level1type=N'TABLE', "
                               + $"@level1name=N'{EscapeSqlString(tableValue)}', "
                               + $"@level2type=N'COLUMN', "
                               + $"@level2name=N'{EscapeSqlString(columnValue)}';";
                    }
                case DataSourceType.MySql:
                    {
                        if (string.IsNullOrWhiteSpace(request.ColumnType))
                        {
                            return string.Empty;
                        }

                        var tableName = BuildQualifiedTableName(request);
                        var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);

                        if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
                        {
                            return string.Empty;
                        }

                        var columnType = NormalizeMySqlColumnTypeForComment(request.ColumnType);

                        return $"ALTER TABLE {tableName} "
                               + $"CHANGE COLUMN {columnName} {columnName} "
                               + $"{columnType} COMMENT '{comment}';";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildDrop(ColumnDdlRequest request)
        {
            var tableName = BuildQualifiedTableName(request);
            var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(request.DataSourceType, request.ColumnName);

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            {
                return string.Empty;
            }

            return $"ALTER TABLE {tableName} DROP COLUMN {columnName};";
        }

        private static string BuildRename(ColumnDdlRequest request)
        {
            //Oracle and PostgreSQL preserve the exact new Column name.
            var preserveInputCase = request.DataSourceType == DataSourceType.Oracle || request.DataSourceType == DataSourceType.PostgreSql;

            var caseMode = preserveInputCase ? DdlNewIdentifierCaseMode.PreserveInput : DdlNewIdentifierCaseMode.NormalizeDatabaseDefault;

            if (DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(request.DataSourceType, request.ColumnName, request.NewColumnName, caseMode))
            {
                return string.Empty;
            }

            var tableName = BuildQualifiedTableName(request);
            var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(request.DataSourceType, request.ColumnName);
            var newColumnName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(request.DataSourceType, request.NewColumnName, caseMode);

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(newColumnName))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"ALTER TABLE {tableName} "
                               + $"RENAME COLUMN {columnName} TO {newColumnName};";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, "sp_rename");
                        var objectName = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.SqlServer, request.SchemaName, request.TableName, request.ColumnName);
                        var newNameValue = DdlIdentifierQuotingPolicy.GetNewIdentifierValue(DataSourceType.SqlServer, request.NewColumnName);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(newNameValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@objname=N'{EscapeSqlString(objectName)}', "
                               + $"@newname=N'{EscapeSqlString(newNameValue)}', "
                               + $"@objtype=N'COLUMN';";
                    }
                case DataSourceType.MySql:
                    {
                        if (string.IsNullOrWhiteSpace(request.ColumnType))
                        {
                            return string.Empty;
                        }

                        return $"ALTER TABLE {tableName} "
                               + $"CHANGE COLUMN {columnName} {newColumnName} "
                               + $"{request.ColumnType};";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildAdd(ColumnDdlRequest request)
        {
            var result = ColumnAddSqlBuilder.Build
            (
                new ColumnAddRequest
                {
                    DataSourceType = request.DataSourceType,
                    SchemaDatabase = request.SchemaDatabase,
                    SchemaName = request.SchemaName,
                    TableName = request.TableName,
                    Column = request.ColumnDefinition,
                    RequireOracleDdlTargetIdentity = request.RequireOracleDdlTargetIdentity,
                    OracleTargetIdentity = request.OracleTargetIdentity
                }
            );

            return result.Succeeded ? result.Sql : string.Empty;
        }

        private static string BuildQualifiedTableName(
            ColumnDdlRequest request)
        {
            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        if (request.RequireOracleDdlTargetIdentity)
                        {
                            var owner = QuoteTrustedOracleOwner(request);
                            var table = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.Oracle, request.TableName);

                            return string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(table) ? string.Empty : owner + "." + table;
                        }

                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.Oracle, request.SchemaName, request.TableName);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.PostgreSql, request.SchemaName, request.TableName);
                    }
                case DataSourceType.SqlServer:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.SqlServer, request.SchemaDatabase, request.SchemaName, request.TableName);
                    }
                case DataSourceType.MySql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, request.SchemaDatabase, request.TableName);
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string QuoteTrustedOracleOwner(ColumnDdlRequest request)
        {
            return OracleDdlTargetIdentityPolicy.TryQuoteOwner(request.OracleTargetIdentity, out string owner) ? owner : string.Empty;
        }

        private static string BuildSqlServerProcedureName(string databaseName, string procedureName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(procedureName))
            {
                return string.Empty;
            }

            return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName
            (
                DataSourceType.SqlServer,
                databaseName,
                "sys",
                procedureName
            );
        }

        private static string GetIdentifierValue(DataSourceType dataSourceType, string identifier)
        {
            return DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(dataSourceType, identifier);
        }

        private static string NormalizeMySqlColumnTypeForComment(string columnType)
        {
            return (columnType ?? string.Empty).Replace(" (", "(").Trim();
        }

        private static string EscapeSqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }

    //F7-U3-C4B3A: pure SQL formatting, NOT an Execute permit. Caller must supply a fresh Reader-owned definition receipt and separately pass the F7 last-mile gate.
    //The first qualification supports only a narrow, fully spelled-out grammar.
    internal static class MySqlCompleteColumnDefinitionSqlBuilder
    {
        private const string SafeLiteralContent = @"(?:[A-Za-z0-9 _.,!?:/\-]|'')*";
        private const string SafeLiteral = "'" + SafeLiteralContent + "'";
        private const string NumericType = @"(?:tinyint|smallint|mediumint|int|integer|bigint)(?:\([1-9][0-9]?\))?(?: unsigned)?";

        //Only explicitly spelled-out charset/collation and quoted ASCII DEFAULT are eligible for later metadata proof.
        //The Regex is SQL GRAMMAR ONLY; a matched definition is not proof that its attributes belong to a live MySQL column.
        private const string ExplicitCharacterCore =
                @"(?<characterType>(?:char|varchar)\([1-9][0-9]{0,4}\))"
                + @" CHARACTER SET (?<characterSet>[A-Za-z][A-Za-z0-9_]{0,63})"
                + @" COLLATE (?<collation>[A-Za-z][A-Za-z0-9_]{0,63})"
                + @" NOT NULL DEFAULT '(?<textDefault>" + SafeLiteralContent + @")'";

        //Require an explicit NOT NULL and DEFAULT. A missing DEFAULT cannot be proven equivalent from INFORMATION_SCHEMA.COLUMNS.COLUMN_DEFAULT alone.
        private static readonly System.Text.RegularExpressions.Regex AcceptedDefinition = new System.Text.RegularExpressions.Regex
        (
            @"\A(?<base>(?:" + NumericType + @" NOT NULL DEFAULT (?:-?(?:0|[1-9][0-9]*)|'-?(?:0|[1-9][0-9]*)')"
            + @"|varchar\([1-9][0-9]{0,4}\) NOT NULL DEFAULT " + SafeLiteral
            + @"|" + ExplicitCharacterCore + @"))"
            + @"(?: COMMENT '(?<oldComment>" + SafeLiteralContent + @")')?\z",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant
            | System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        internal static bool TryQualifyCompleteDefinition(string definition, out string withoutOldComment)
        {
            withoutOldComment = string.Empty;

            if (string.IsNullOrWhiteSpace(definition) || definition.Length > 2048
                || definition.IndexOf('\\') >= 0 || definition.IndexOf(';') >= 0
                || definition.IndexOf('\r') >= 0 || definition.IndexOf('\n') >= 0
                || definition.IndexOf('\0') >= 0)
            {
                return false;
            }

            var match = AcceptedDefinition.Match(definition);

            if (!match.Success)
            {
                return false;
            }

            //Enforce MySQL CHAR/VARCHAR length bounds before accepting the new explicit character grammar.
            if (match.Groups["characterType"].Success)
            {
                var characterType = match.Groups["characterType"].Value;

                var lengthMatch = System.Text.RegularExpressions.Regex.Match
                (
                    characterType, @"\((?<length>[1-9][0-9]{0,4})\)\z",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant
                );

                if (!lengthMatch.Success || !int.TryParse(lengthMatch.Groups["length"].Value, out var length)
                    || length > (characterType.StartsWith("char(", StringComparison.OrdinalIgnoreCase) ? 255 : 65535))
                {
                    return false;
                }
            }

            withoutOldComment = match.Groups["base"].Value;
            return !string.IsNullOrWhiteSpace(withoutOldComment);
        }

        //For pure proof only. A matching Regex is never sufficient to issue a Reader receipt or Execute permit.
        internal static bool TryMatchExplicitCharacterDefinition(string definition, out System.Text.RegularExpressions.Match match)
        {
            match = null;

            if (!TryQualifyCompleteDefinition(definition, out _))
            {
                return false;
            }

            var parsed = AcceptedDefinition.Match(definition);

            if (!parsed.Success || !parsed.Groups["characterType"].Success)
            {
                return false;
            }

            match = parsed;
            return true;
        }

        internal static bool TryBuildLegacyRename(ColumnDdlRequest request, string verifiedCompleteDefinition, out string sql)
        {
            sql = string.Empty;

            if (request == null || request.DataSourceType != DataSourceType.MySql
                || request.Operation != ColumnDdlOperation.Rename
                || DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(DataSourceType.MySql, request.ColumnName, request.NewColumnName)
                || !TryQualifyCompleteDefinition(verifiedCompleteDefinition, out _))
            {
                return false;
            }

            var table = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, request.SchemaDatabase, request.TableName);
            var oldColumn = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);
            var newColumn = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(DataSourceType.MySql, request.NewColumnName);

            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(oldColumn) || string.IsNullOrEmpty(newColumn))
            {
                return false;
            }

            sql = "ALTER TABLE " + table + " CHANGE COLUMN " + oldColumn + " " + newColumn + " " + verifiedCompleteDefinition + ";";
            return true;
        }

        internal static bool TryBuildComment(ColumnDdlRequest request, string verifiedCompleteDefinition, out string sql)
        {
            sql = string.Empty;

            if (request == null || request.DataSourceType != DataSourceType.MySql
                || request.Operation != ColumnDdlOperation.Comment
                || !TryQualifyCompleteDefinition(verifiedCompleteDefinition, out var originalWithoutComment)
                || request.Comment == null || request.Comment.Length > 512
                || !System.Text.RegularExpressions.Regex.IsMatch(request.Comment, @"\A[A-Za-z0-9 _.,!?:/\-]*\z", System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            {
                return false;
            }

            var table = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, request.SchemaDatabase, request.TableName);
            var column = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);

            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column))
            {
                return false;
            }

            //MODIFY, not the old CHANGE-COLUMN-with-type-only preview.
            sql = "ALTER TABLE " + table + " MODIFY COLUMN " + column + " " + originalWithoutComment
                  + " COMMENT '" + request.Comment.Replace("'", "''") + "';";

            return true;
        }
    }

    //F7-U3-C4C2-B: pure, fail-closed character-definition corroboration policy.
    //The caller MUST obtain every input from one verified Reader-owned connection/transaction.
    //verifiedCharsetCollationPair can only be true after a live INFORMATION_SCHEMA.COLLATIONS lookup.
    //This policy performs NO database reads, creates NO receipt and never grants Execute authorization.
    internal static class MySqlCharacterColumnDefinitionProofPolicy
    {
        internal static string Validate(string definition, string columnType, string isNullable, string columnDefault,
                                        string oldComment, string characterSet, string collation, string extra,
                                        string generationExpression, string columnKey, bool verifiedCharsetCollationPair)
        {
            if (!verifiedCharsetCollationPair)
            {
                return "F7U3C4C2B_CHARSET_COLLATION_PAIR_UNVERIFIED";
            }

            if (!string.Equals(isNullable, "NO", StringComparison.Ordinal)
                || columnDefault == null || oldComment == null
                || extra == null || generationExpression == null || columnKey == null
                || !string.IsNullOrEmpty(extra) || !string.IsNullOrEmpty(generationExpression)
                || !string.IsNullOrEmpty(columnKey))
            {
                return "F7U3C4C2B_ATTRIBUTES_OUTSIDE_SAFE_SUBSET";
            }

            if (!MySqlCompleteColumnDefinitionSqlBuilder.TryMatchExplicitCharacterDefinition(definition, out var match))
            {
                return "F7U3C4C2B_EXPLICIT_DEFINITION_REQUIRED";
            }

            //Exact type, character set, collation and default/old-comment corroboration.
            //Never infer a charset/collation pair merely from its name prefix.
            if (!string.Equals(match.Groups["characterType"].Value, columnType, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(match.Groups["characterSet"].Value, characterSet, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(match.Groups["collation"].Value, collation, StringComparison.OrdinalIgnoreCase))
            {
                return "F7U3C4C2B_CHARACTER_METADATA_MISMATCH";
            }

            var actualDefault = match.Groups["textDefault"].Value.Replace("''", "'");
            var commentGroup = match.Groups["oldComment"];
            var actualComment = commentGroup.Success ? commentGroup.Value.Replace("''", "'") : string.Empty;

            if (!string.Equals(actualDefault, columnDefault, StringComparison.Ordinal) || !string.Equals(actualComment, oldComment, StringComparison.Ordinal))
            {
                return "F7U3C4C2B_LITERAL_METADATA_MISMATCH";
            }

            return string.Empty;
        }
    }

    //Receipt is intentionally immutable and has NO public constructor.
    //It records a verified metadata read, not a general capability to execute.
    internal sealed class MySqlVerifiedColumnDefinitionSnapshot
    {
        internal MySqlVerifiedColumnDefinitionSnapshot(Guid connectionId, string databaseName, string tableName, string columnName, string serverVersion, string completeDefinition, long serverConnectionId)
        {
            ConnectionId = connectionId;
            ServerConnectionId = serverConnectionId;
            DatabaseName = databaseName;
            TableName = tableName;
            ColumnName = columnName;
            ServerVersion = serverVersion;
            CompleteDefinition = completeDefinition;
        }

        internal Guid ConnectionId { get; }
        internal long ServerConnectionId { get; }
        internal string DatabaseName { get; }
        internal string TableName { get; }
        internal string ColumnName { get; }
        internal string ServerVersion { get; }
        internal string CompleteDefinition { get; }
    }


    //F7-U3-C4B3B2A: pure preflight contract. A snapshot is evidence, NOT Execute permission.
    //Only the dedicated Reader may re-read and dispatch after its own transaction, identity and SQL checks.
    internal static class MySqlVerifiedColumnDefinitionExecutionPolicy
    {
        internal static string Validate(ColumnDdlOperation operation, MySqlVerifiedColumnDefinitionSnapshot preview, MySqlVerifiedColumnDefinitionSnapshot live, ColumnDdlRequest request, string displayedSql)
        {
            if (operation != ColumnDdlOperation.Rename && operation != ColumnDdlOperation.Comment)
            {
                return "F7U3C4B3B2_OPERATION_UNSUPPORTED";
            }

            if (preview == null || live == null || request == null
                || request.DataSourceType != DataSourceType.MySql || request.Operation != operation
                || preview.ConnectionId == Guid.Empty || live.ConnectionId == Guid.Empty
                || preview.ServerConnectionId <= 0 || live.ServerConnectionId <= 0)
            {
                return "F7U3C4B3B2_RECEIPT_UNVERIFIED";
            }

            if (preview.ConnectionId != live.ConnectionId || preview.ServerConnectionId != live.ServerConnectionId)
            {
                return "F7U3C4B3B2_PHYSICAL_CONNECTION_CHANGED";
            }

            if (string.IsNullOrWhiteSpace(preview.DatabaseName) || string.IsNullOrWhiteSpace(preview.TableName)
                || string.IsNullOrWhiteSpace(preview.ColumnName)
                || !string.Equals(preview.DatabaseName, live.DatabaseName, StringComparison.Ordinal)
                || !string.Equals(preview.TableName, live.TableName, StringComparison.Ordinal)
                || !string.Equals(preview.ColumnName, live.ColumnName, StringComparison.Ordinal)
                || !string.Equals(preview.DatabaseName, request.SchemaDatabase, StringComparison.Ordinal)
                || !string.Equals(preview.TableName, request.TableName, StringComparison.Ordinal)
                || !string.Equals(preview.ColumnName, request.ColumnName, StringComparison.Ordinal))
            {
                return "F7U3C4B3B2_COLUMN_IDENTITY_CHANGED";
            }

            if (string.IsNullOrWhiteSpace(preview.ServerVersion)
                || !string.Equals(preview.ServerVersion, live.ServerVersion, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(preview.CompleteDefinition)
                || !string.Equals(preview.CompleteDefinition, live.CompleteDefinition, StringComparison.Ordinal))
            {
                return "F7U3C4B3B2_DEFINITION_OR_VERSION_CHANGED";
            }

            var renameSyntax = MySqlColumnDdlVersionPolicy.SelectRenameSyntax(live.ServerVersion);

            if (renameSyntax == MySqlColumnRenameSyntax.Unknown
                || (operation == ColumnDdlOperation.Rename && renameSyntax != MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition))
            {
                //MySQL 8.0.3+ must continue to use its definition-free RENAME COLUMN path.
                return "F7U3C4B3B2_RENAME_SYNTAX_NOT_AUTHORIZED";
            }

            if (!MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(live.CompleteDefinition, out _))
            {
                return "F7U3C4B3B2_DEFINITION_NOT_QUALIFIED";
            }

            string rebuiltSql;

            var built = operation == ColumnDdlOperation.Rename
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, live.CompleteDefinition, out rebuiltSql)
                : MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, live.CompleteDefinition, out rebuiltSql);

            if (!built || string.IsNullOrWhiteSpace(displayedSql)
                || !string.Equals(displayedSql, rebuiltSql, StringComparison.Ordinal)
                || !NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, operation, rebuiltSql))
            {
                return "F7U3C4B3B2_SQL_NOT_EXACT";
            }

            return string.Empty;
        }
    }

    //F7-U3-C4C1-B: classification is diagnostic only. Never use this output as an Execute permit.
    //Never expose raw Reader exception text, connection strings or server error details.
    internal static class MySqlColumnDdlPreviewFeedbackPolicy
    {
        internal static string GetReasonKey(string failureStatus)
        {
            var code = (failureStatus ?? string.Empty).Split(':')[0].Trim();

            switch (code)
            {
                case "F7U2C2_TRANSACTION_UNSAFE":
                    {
                        return "MySqlColumnDdlPendingTransaction";
                    }
                case "F7U3C4B3_METADATA_OUTSIDE_SAFE_SUBSET":
                    {
                        return "MySqlColumnDdlUnsupportedDefinition";
                    }
                case "F7U3C4B3_METADATA_AMBIGUOUS":
                case "F7U3C4B3_ATTRIBUTES_NOT_PROVEN":
                case "F7U3C4B3_SHOW_CREATE_AMBIGUOUS":
                case "F7U3C4B3_MULTIPLE_COLUMN_DECLARATIONS":
                case "F7U3C4B3_SHOW_CREATE_UNAVAILABLE":
                case "F7U3C4B3_DEFINITION_NOT_QUALIFIED":
                    {
                        return "MySqlColumnDdlMetadataUnverified";
                    }
                case "F7U3C4B3_CONNECTION_UNVERIFIED":
                    {
                        return "MySqlColumnDdlConnectionUnverified";
                    }
                case "F7U3C4B3_CONNECTION_CHANGED":
                case "F7U3C4B3B2_PHYSICAL_CONNECTION_CHANGED":
                    {
                        return "MySqlColumnDdlConnectionChanged";
                    }
                case "F7U3C4B3_VERSION_UNVERIFIED":
                    {
                        return "MySqlColumnDdlVersionUnverified";
                    }
                case "F7U3C4B3_READ_FAILED":
                    {
                        return "MySqlColumnDdlMetadataReadFailed";
                    }
                default:
                    {
                        return "MySqlColumnDdlDefinitionUnverified";
                    }
            }
        }

        //Only default English strings. Localized zh-Hant/zh-Hans/en values are generated by the existing LocalizationGenerator workbook using the same stable IDs.
        internal static string GetDefaultMessage(string reasonKey)
        {
            switch (reasonKey)
            {
                case "MySqlColumnDdlPendingTransaction":
                    {
                        return "An unfinished transaction prevents this DDL operation. Resolve it before reopening the form.";
                    }
                case "MySqlColumnDdlUnsupportedDefinition":
                    {
                        return "Some column attributes cannot yet be safely preserved "
                               + "(e.g., CHARACTER SET, COLLATE, AUTO_INCREMENT, or generated columns). "
                               + "SQL generation and Execute are disabled to prevent unintended changes.";
                    }
                case "MySqlColumnDdlMetadataUnverified":
                    {
                        return "The original column definition could not be fully verified. No safe SQL was generated.";
                    }
                case "MySqlColumnDdlConnectionUnverified":
                    {
                        return "The MySQL connection could not be verified. Reconnect and reopen the operation from Schema Explorer.";
                    }
                case "MySqlColumnDdlConnectionChanged":
                    {
                        return "The MySQL physical connection changed. Close this form and reopen the operation.";
                    }
                case "MySqlColumnDdlVersionUnverified":
                    {
                        return "The MySQL server version could not be verified. SQL generation and execution are blocked.";
                    }
                case "MySqlColumnDdlMetadataReadFailed":
                    {
                        return "The column metadata could not be read safely. Check your connection and privileges, then retry.";
                    }
                default:
                    {
                        return "The column definition could not be verified. SQL generation and execution are blocked.";
                    }
            }
        }
    }
}
