using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using System;

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

    //F7-S1-C3C-B: parse an already-read SESSION.sql_mode; not connection evidence or Execute permission.
    internal static class MySqlColumnCommentSessionModePolicy
    {
        internal static bool TryParse(string sessionSqlMode, out bool noBackslashEscapes)
        {
            noBackslashEscapes = false;

            if (sessionSqlMode == null || sessionSqlMode.IndexOf('\0') >= 0)
            {
                return false;
            }

            if (sessionSqlMode.Length == 0)
            {
                return true;
            }

            var modes = sessionSqlMode.Split(',');

            foreach (var raw in modes)
            {
                var mode = raw.Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(mode, @"\A[A-Za-z][A-Za-z0-9_]*\z", System.Text.RegularExpressions.RegexOptions.CultureInvariant))
                {
                    return false;
                }

                if (string.Equals(mode, "NO_BACKSLASH_ESCAPES", StringComparison.OrdinalIgnoreCase))
                {
                    noBackslashEscapes = true;
                }
            }

            return true;
        }
    }

    //F7-S1-C3-B4-C3B: pure MySQL column COMMENT string-literal quoting; NO Execute permission.
    //The noBackslashEscapes flag must be supplied by a verified Reader SESSION.sql_mode.
    internal static class MySqlColumnCommentLiteralPolicy
    {
        internal static bool HasModeSensitiveCharacters(string value)
        {
            return value != null && (value.IndexOf('\\') >= 0 || value.IndexOf('\r') >= 0
                                     || value.IndexOf('\n') >= 0 || value.IndexOf('\t') >= 0
                                     || value.IndexOf('\b') >= 0 || value.IndexOf('\u001a') >= 0);
        }

        internal static bool TryQuote(string value, bool noBackslashEscapes, out string quotedLiteral)
        {
            quotedLiteral = string.Empty;

            if (value == null)
            {
                return false;
            }

            var scalarCount = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];

                if (ch == '\0')
                {
                    return false;
                }

                if (char.IsHighSurrogate(ch))
                {
                    if (i + 1 >= value.Length || !char.IsLowSurrogate(value[i + 1]))
                    {
                        return false;
                    }

                    i++;
                }
                else if (char.IsLowSurrogate(ch))
                {
                    return false;
                }

                scalarCount++;

                if (scalarCount > 1024)
                {
                    return false;
                }
            }

            //Escaped control characters cannot be round-tripped with NO_BACKSLASH_ESCAPES through a single-line quoted literal. Defer these cases to a future safe path.
            if (noBackslashEscapes && (value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\t') >= 0 || value.IndexOf('\b') >= 0 || value.IndexOf('\u001a') >= 0))
            {
                return false;
            }

            var builder = new System.Text.StringBuilder(value.Length + 2);

            builder.Append('\'');

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\'':
                        {
                            builder.Append("''");
                            break;
                        }
                    case '\\':
                        {
                            builder.Append(noBackslashEscapes ? "\\" : "\\\\");
                            break;
                        }
                    case '\r':
                        {
                            builder.Append("\\r");
                            break;
                        }
                    case '\n':
                        {
                            builder.Append("\\n");
                            break;
                        }
                    case '\t':
                        {
                            builder.Append("\\t");
                            break;
                        }
                    case '\b':
                        {
                            builder.Append("\\b");
                            break;
                        }
                    case '\u001a':
                        {
                            builder.Append("\\Z");
                            break;
                        }
                    default:
                        {
                            builder.Append(ch);
                            break;
                        }
                }
            }

            builder.Append('\'');
            quotedLiteral = builder.ToString();

            return true;
        }

        //Accept only canonical literals that this Builder can generate, never arbitrary SQL text.
        //SQL-mode-dependent backslash sequences are decoded then re-quoted for exact comparison.
        internal static bool TryUnquote(string quotedLiteral, bool noBackslashEscapes, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrEmpty(quotedLiteral) || quotedLiteral.Length < 2
                || quotedLiteral[0] != '\'' || quotedLiteral[quotedLiteral.Length - 1] != '\'')
            {
                return false;
            }

            var result = new System.Text.StringBuilder();

            for (var i = 1; i < quotedLiteral.Length - 1; i++)
            {
                var ch = quotedLiteral[i];

                if (ch == '\'')
                {
                    if (i + 1 >= quotedLiteral.Length - 1 || quotedLiteral[i + 1] != '\'')
                    {
                        return false;
                    }

                    result.Append('\'');
                    i++;

                    continue;
                }

                if (ch == '\\' && !noBackslashEscapes)
                {
                    if (i + 1 >= quotedLiteral.Length - 1)
                    {
                        return false;
                    }

                    i++;

                    switch (quotedLiteral[i])
                    {
                        case '\\':
                            {
                                result.Append('\\');
                                break;
                            }
                        case 'n':
                            {
                                result.Append('\n');
                                break;
                            }
                        case 'r':
                            {
                                result.Append('\r');
                                break;
                            }
                        case 't':
                            {
                                result.Append('\t');
                                break;
                            }
                        case 'b':
                            {
                                result.Append('\b');
                                break;
                            }
                        case 'Z':
                            {
                                result.Append('\u001a');
                                break;
                            }
                        default:
                            {
                                return false;
                            }
                    }

                    continue;
                }

                if (ch == '\0' || ch == '\r' || ch == '\n' || ch == '\t' || ch == '\b' || ch == '\u001a')
                {
                    return false;
                }

                result.Append(ch);
            }

            var decoded = result.ToString();

            if (!TryQuote(decoded, noBackslashEscapes, out var canonical) || !string.Equals(canonical, quotedLiteral, StringComparison.Ordinal))
            {
                return false;
            }

            value = decoded;
            return true;
        }
    }

    //Only an exactly reconstructed Builder statement can be decoded into a COMMENT intent.
    //A SQL string, even when canonical, is never DDL Execute authorization.
    internal static class MySqlColumnCommentSqlIntentPolicy
    {
        internal static bool TryExtractCanonicalComment(string sql, string databaseName, string tableName, string columnName, string verifiedDefinition, bool noBackslashEscapes, out string comment)
        {
            comment = string.Empty;

            if (string.IsNullOrWhiteSpace(sql)
                || !MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(verifiedDefinition, noBackslashEscapes, out var withoutOldComment))
            {
                return false;
            }

            var table = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, databaseName, tableName);
            var column = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, columnName);

            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column))
            {
                return false;
            }

            var prefix = "ALTER TABLE " + table + " MODIFY COLUMN " + column + " " + withoutOldComment + " COMMENT ";

            if (!sql.StartsWith(prefix, StringComparison.Ordinal) || !sql.EndsWith(";", StringComparison.Ordinal))
            {
                return false;
            }

            var literal = sql.Substring(prefix.Length, sql.Length - prefix.Length - 1);

            if (!MySqlColumnCommentLiteralPolicy.TryUnquote(literal, noBackslashEscapes, out var decoded))
            {
                return false;
            }

            var request = new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = databaseName,
                TableName = tableName,
                ColumnName = columnName,
                Comment = decoded
            };

            if (!MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, verifiedDefinition, noBackslashEscapes, out var rebuilt) || !string.Equals(sql, rebuilt, StringComparison.Ordinal))
            {
                return false;
            }

            comment = decoded;
            return true;
        }
    }

    //F7-U3-C4B3A: pure SQL formatting, NOT an Execute permit. Caller must supply a fresh Reader-owned definition receipt and separately pass the F7 last-mile gate.
    //The first qualification supports only a narrow, fully spelled-out grammar.
    internal static class MySqlCompleteColumnDefinitionSqlBuilder
    {
        private const string SafeLiteralContent = @"(?:[A-Za-z0-9 _.,!?:/\-]|'')*";
        private const string SafeLiteral = "'" + SafeLiteralContent + "'";
        private const string CommentLiteralContent = @"(?:''|[^'\\\r\n\0])*";
        private const string NumericType = @"(?:tinyint|smallint|mediumint|int|integer|bigint)(?:\([1-9][0-9]?\))?(?: unsigned)?";
        private const string IntegerDefaultLiteral = @"(?:-?(?:0|[1-9][0-9]*)|'-?(?:0|[1-9][0-9]*)')";
        private const string FractionalDefaultLiteral = @"(?:-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?|'-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?')";

        //Syntax only: DEFAULT NULL is a nullable-column shape, not evidence of the original live DEFAULT clause.
        //Reader metadata proof and the Execute-Time Gate remain mandatory.
        private const string IntegerDefinition = NumericType + @"(?: NOT NULL DEFAULT " + IntegerDefaultLiteral + @"|(?: NULL)? DEFAULT (?:NULL|" + IntegerDefaultLiteral + @"))";

        private const string FractionalType = @"(?:double|decimal\((?<decimalPrecision>[1-9][0-9]?)," + @"(?<decimalScale>(?:0|[1-9][0-9]?))\))";
        private const string FractionalDefinition = FractionalType + @"(?: NOT NULL DEFAULT " + FractionalDefaultLiteral + @"|(?: NULL)? DEFAULT (?:NULL|" + FractionalDefaultLiteral + @"))";

        //CHAR/VARCHAR without explicit CHARACTER SET/COLLATE is only a grammar match.
        //B3 must prove that a generated MODIFY/CHANGE preserves the effective charset and collation.
        private const string InheritedCharacterDefinition = @"(?<inheritedCharacterType>(?:char|varchar)\([1-9][0-9]{0,4}\))" + @"(?: NOT NULL DEFAULT " + SafeLiteral + @"|(?: NULL)? DEFAULT (?:NULL|" + SafeLiteral + @"))";

        //Retain the existing explicit CHARACTER SET/COLLATE grammar unchanged.
        private const string ExplicitCharacterCore =
                @"(?<characterType>(?:char|varchar)\([1-9][0-9]{0,4}\))"
                + @" CHARACTER SET (?<characterSet>[A-Za-z][A-Za-z0-9_]{0,63})"
                + @" COLLATE (?<collation>[A-Za-z][A-Za-z0-9_]{0,63})"
                + @" NOT NULL DEFAULT '(?<textDefault>" + SafeLiteralContent + @")'";

        private static readonly System.Text.RegularExpressions.Regex AcceptedDefinition = new System.Text.RegularExpressions.Regex
        (
            @"\A(?<base>(?:" + IntegerDefinition
            + @"|" + FractionalDefinition
            + @"|" + InheritedCharacterDefinition
            + @"|" + ExplicitCharacterCore + @"))"
            + @"(?: COMMENT '(?<oldComment>" + CommentLiteralContent + @")')?\z",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        internal static bool TryQualifyCompleteDefinition(string definition, out string withoutOldComment)
        {
            withoutOldComment = string.Empty;

            if (string.IsNullOrWhiteSpace(definition) || definition.Length > 8192
                || definition.IndexOf('\\') >= 0
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

            //The syntax range is deliberately small; reject DECIMAL formats that MySQL cannot represent.
            if (match.Groups["decimalPrecision"].Success)
            {
                if (!int.TryParse(match.Groups["decimalPrecision"].Value, out var precision)
                    || !int.TryParse(match.Groups["decimalScale"].Value, out var scale)
                    || precision > 65 || scale > 30 || scale > precision)
                {
                    return false;
                }
            }

            //Apply the same MySQL CHAR/VARCHAR length check to explicit and inherited definitions.
            var characterType = match.Groups["characterType"].Success ? match.Groups["characterType"].Value : match.Groups["inheritedCharacterType"].Value;

            if (!string.IsNullOrEmpty(characterType))
            {
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

        //Mode-aware overload for a verified SHOW CREATE result. The legacy mode-neutral grammar stays unchanged for all existing callers and older Golden contracts.
        internal static bool TryQualifyCompleteDefinition(string definition, bool noBackslashEscapes, out string withoutOldComment)
        {
            withoutOldComment = string.Empty;

            if (TryQualifyCompleteDefinition(definition, out withoutOldComment))
            {
                return true;
            }

            withoutOldComment = string.Empty;

            if (string.IsNullOrWhiteSpace(definition) || definition.Length > 8192
                || definition.IndexOf('\0') >= 0 || definition.IndexOf('\r') >= 0
                || definition.IndexOf('\n') >= 0)
            {
                return false;
            }

            const string marker = " COMMENT '";

            var at = definition.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            while (at >= 0)
            {
                var originalWithoutComment = definition.Substring(0, at);
                var originalLiteral = definition.Substring(at + " COMMENT ".Length);

                //D3C-02: SHOW CREATE on MySQL 8.0.34 escapes stored backslashes even in NBE mode.
                //This is stored-definition decoding, not quoting a new SQL statement.
                if (TryQualifyCompleteDefinition(originalWithoutComment, out var qualifiedBase)
                    && string.Equals(originalWithoutComment, qualifiedBase, StringComparison.Ordinal)
                    && MySqlColumnCommentLiteralPolicy.TryUnquote(originalLiteral, false, out _))
                {
                    withoutOldComment = originalWithoutComment;
                    return true;
                }

                at = definition.IndexOf(marker, at + marker.Length, StringComparison.OrdinalIgnoreCase);
            }

            return false;
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

        //Mode-neutral overload: only comments that do not depend on SESSION.sql_mode can produce Preview SQL without verified Reader mode evidence.
        internal static bool TryBuildComment(ColumnDdlRequest request, string verifiedCompleteDefinition, out string sql)
        {
            return TryBuildCommentCore(request, verifiedCompleteDefinition, null, out sql);
        }

        //The caller must supply noBackslashEscapes from a verified Reader session.
        //SQL mode must be re-checked before an actual Execute. This overload is not an Execute permit.
        internal static bool TryBuildComment(ColumnDdlRequest request, string verifiedCompleteDefinition, bool noBackslashEscapes, out string sql)
        {
            return TryBuildCommentCore(request, verifiedCompleteDefinition, noBackslashEscapes, out sql);
        }

        private static bool TryBuildCommentCore(ColumnDdlRequest request, string verifiedCompleteDefinition, bool? noBackslashEscapes, out string sql)
        {
            sql = string.Empty;

            if (request == null || request.DataSourceType != DataSourceType.MySql
                || request.Operation != ColumnDdlOperation.Comment
                || !(noBackslashEscapes.HasValue
                     ? TryQualifyCompleteDefinition(verifiedCompleteDefinition, noBackslashEscapes.Value, out _)
                     : TryQualifyCompleteDefinition(verifiedCompleteDefinition, out _))
                || request.Comment == null
                || (!noBackslashEscapes.HasValue && MySqlColumnCommentLiteralPolicy.HasModeSensitiveCharacters(request.Comment))
                || !MySqlColumnCommentLiteralPolicy.TryQuote(request.Comment, noBackslashEscapes ?? false, out var literal))
            {
                return false;
            }

            string originalWithoutComment;

            var qualifiedBase = noBackslashEscapes.HasValue
                ? TryQualifyCompleteDefinition(verifiedCompleteDefinition, noBackslashEscapes.Value, out originalWithoutComment)
                : TryQualifyCompleteDefinition(verifiedCompleteDefinition, out originalWithoutComment);

            var table = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, request.SchemaDatabase, request.TableName);
            var column = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);

            if (!qualifiedBase || string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column))
            {
                return false;
            }

            sql = "ALTER TABLE " + table + " MODIFY COLUMN " + column + " " + originalWithoutComment
                  + " COMMENT " + literal + ";";
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

    //F7-S1-C3-B4-C1B: pure corroboration of a simple, single-column PRIMARY KEY.
    //Input must be the verified Reader's SHOW CREATE TABLE result plus matching COLUMN metadata.
    //No database access, no receipt, and no Execute permission is issued here.
    internal static class MySqlPrimaryKeyColumnProofPolicy
    {
        internal static bool TryQualifySingleColumnPrimaryKey(string showCreateTable, string columnName, string columnKey, string isNullable, out string verifiedKeyClause)
        {
            verifiedKeyClause = string.Empty;

            if (string.IsNullOrWhiteSpace(showCreateTable) || showCreateTable.Length > 1048576
                || string.IsNullOrWhiteSpace(columnName)
                || !System.Text.RegularExpressions.Regex.IsMatch(columnName, @"\A[A-Za-z_][A-Za-z0-9_]*\z", System.Text.RegularExpressions.RegexOptions.CultureInvariant)
                || !string.Equals(columnKey, "PRI", StringComparison.Ordinal)
                || !string.Equals(isNullable, "NO", StringComparison.Ordinal))
            {
                return false;
            }

            //SHOW CREATE TABLE uses an ordinary line for the PRIMARY KEY; a column COMMENT or a UNIQUE KEY that merely yields COLUMN_KEY=PRI is not sufficient proof.
            var normalized = showCreateTable.Replace("\r\n", "\n");

            if (!normalized.StartsWith("CREATE TABLE `", StringComparison.Ordinal) || normalized.IndexOf('\r') >= 0 || normalized.IndexOf('\0') >= 0)
            {
                return false;
            }

            var declarationPrefix = "  `" + columnName + "` ";
            var expectedClause = "PRIMARY KEY (`" + columnName + "`)";
            var columnCount = 0;
            var primaryKeyCount = 0;

            foreach (var rawLine in normalized.Split('\n'))
            {
                if (rawLine.StartsWith(declarationPrefix, StringComparison.Ordinal))
                {
                    columnCount++;
                }

                var clause = rawLine.TrimStart(' ', '\t');

                if (!clause.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                primaryKeyCount++;

                //Reject composite keys, index options, unknown syntax and extra PK clauses.
                if (!string.Equals(clause, expectedClause, StringComparison.Ordinal) && !string.Equals(clause, expectedClause + ",", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (columnCount != 1 || primaryKeyCount != 1)
            {
                return false;
            }

            verifiedKeyClause = expectedClause;
            return true;
        }
    }

    //F7-S1-C3-B3B: pure, fail-closed metadata proof. A grammar match is never a Reader receipt or Execute permit.
    //definition MUST be the unique SHOW CREATE TABLE column declaration; metadata strings are Reader-sourced.
    //verifiedCharsetCollationPair is true only after one exact INFORMATION_SCHEMA.COLLATIONS row on the verified connection.
    internal static class MySqlCommonColumnDefinitionProofPolicy
    {
        //Legacy call is deliberately fail-closed for every indexed column.
        //Only a Reader-verified single-column primary key may use the overload below.
        internal static string Validate(string definition, string columnType, string isNullable, string columnDefault, string oldComment, string characterSet, string collation,
                                        string tableCharacterSet, string tableCollation, string extra, string generationExpression, string columnKey, bool verifiedCharsetCollationPair)
        {
            return Validate
            (
                definition, columnType, isNullable, columnDefault, oldComment, characterSet, collation,
                tableCharacterSet, tableCollation, extra, generationExpression, columnKey, verifiedCharsetCollationPair, false
            );
        }

        internal static string Validate(string definition, string columnType, string isNullable, string columnDefault, string oldComment, string characterSet, string collation,
                                        string tableCharacterSet, string tableCollation, string extra, string generationExpression, string columnKey, bool verifiedCharsetCollationPair, bool verifiedSingleColumnPrimaryKey)
        {
            return Validate
            (
                definition, columnType, isNullable, columnDefault, oldComment, characterSet, collation,
                tableCharacterSet, tableCollation, extra, generationExpression, columnKey,
                verifiedCharsetCollationPair, verifiedSingleColumnPrimaryKey, (bool?)null
            );
        }

        //New mode-aware entry. The last bool must come from the verified Reader session.
        internal static string Validate(string definition, string columnType, string isNullable, string columnDefault, string oldComment, string characterSet, string collation,
                                        string tableCharacterSet, string tableCollation, string extra, string generationExpression, string columnKey,
                                        bool verifiedCharsetCollationPair, bool verifiedSingleColumnPrimaryKey, bool noBackslashEscapes)
        {
            return Validate
            (
                definition, columnType, isNullable, columnDefault, oldComment, characterSet, collation,
                tableCharacterSet, tableCollation, extra, generationExpression, columnKey,
                verifiedCharsetCollationPair, verifiedSingleColumnPrimaryKey, (bool?)noBackslashEscapes
            );
        }

        private static string Validate(string definition, string columnType, string isNullable, string columnDefault, string oldComment, string characterSet, string collation,
                                       string tableCharacterSet, string tableCollation, string extra, string generationExpression, string columnKey,
                                       bool verifiedCharsetCollationPair, bool verifiedSingleColumnPrimaryKey, bool? noBackslashEscapes)
        {
            string withoutComment;
            var qualified = noBackslashEscapes.HasValue
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(definition, noBackslashEscapes.Value, out withoutComment)
                : MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(definition, out withoutComment);

            if (!qualified
                || string.IsNullOrWhiteSpace(columnType)
                || (isNullable != "YES" && isNullable != "NO")
                || oldComment == null || extra == null || generationExpression == null || columnKey == null)
            {
                return "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
            }

            var qualifiedPrimaryKey = verifiedSingleColumnPrimaryKey && string.Equals(columnKey, "PRI", StringComparison.Ordinal);

            if (extra.Length != 0 || generationExpression.Length != 0
                || (!string.IsNullOrEmpty(columnKey) && !qualifiedPrimaryKey)
                || (verifiedSingleColumnPrimaryKey && !qualifiedPrimaryKey))
            {
                return "F7U3C4B3_METADATA_OUTSIDE_SAFE_SUBSET";
            }

            //Grammar already rejects SQL separators, unsupported extra clauses and expression DEFAULTs.
            //Reconstruct the exact original Comment before comparing it with INFORMATION_SCHEMA.
            var commentSuffix = definition.Substring(withoutComment.Length);
            var actualComment = string.Empty;

            if (commentSuffix.Length != 0)
            {
                const string commentPrefix = " COMMENT '";

                if (!commentSuffix.StartsWith(commentPrefix, StringComparison.OrdinalIgnoreCase)
                    || !commentSuffix.EndsWith("'", StringComparison.Ordinal)
                    || commentSuffix.Length < commentPrefix.Length + 1)
                {
                    return "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
                }

                var quotedComment = commentSuffix.Substring(" COMMENT ".Length);

                if (noBackslashEscapes.HasValue)
                {
                    //SHOW CREATE text must be decoded before comparing with COLUMN_COMMENT.
                    if (!MySqlColumnCommentLiteralPolicy.TryUnquote(quotedComment, false, out actualComment))
                    {
                        return "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
                    }
                }
                else
                {
                    actualComment = commentSuffix.Substring(commentPrefix.Length, commentSuffix.Length - commentPrefix.Length - 1).Replace("''", "'");
                }
            }

            if (!string.Equals(actualComment, oldComment, StringComparison.Ordinal))
            {
                return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
            }

            const string defaultMarker = " DEFAULT ";

            var defaultIndex = withoutComment.LastIndexOf(defaultMarker, StringComparison.OrdinalIgnoreCase);

            if (defaultIndex <= 0)
            {
                return "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
            }

            var typeAndAttributes = withoutComment.Substring(0, defaultIndex);
            var originalDefault = withoutComment.Substring(defaultIndex + defaultMarker.Length);
            var expectedNullable = "YES";

            if (typeAndAttributes.EndsWith(" NOT NULL", StringComparison.OrdinalIgnoreCase))
            {
                typeAndAttributes = typeAndAttributes.Substring(0, typeAndAttributes.Length - " NOT NULL".Length);
                expectedNullable = "NO";
            }
            else if (typeAndAttributes.EndsWith(" NULL", StringComparison.OrdinalIgnoreCase))
            {
                typeAndAttributes = typeAndAttributes.Substring(0, typeAndAttributes.Length - " NULL".Length);
            }

            if (!string.Equals(expectedNullable, isNullable, StringComparison.Ordinal))
            {
                return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
            }

            if (string.Equals(originalDefault, "NULL", StringComparison.OrdinalIgnoreCase))
            {
                //COLUMN_DEFAULT = SQL NULL alone is ambiguous. An explicit SHOW CREATE DEFAULT NULL and a matching INFORMATION_SCHEMA NULL are BOTH required.
                if (columnDefault != null)
                {
                    return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
                }
            }
            else
            {
                if (columnDefault == null)
                {
                    return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
                }

                var unescapedDefault = originalDefault;

                if (originalDefault.Length >= 2 && originalDefault.StartsWith("'", StringComparison.Ordinal) && originalDefault.EndsWith("'", StringComparison.Ordinal))
                {
                    unescapedDefault = originalDefault.Substring(1, originalDefault.Length - 2).Replace("''", "'");
                }

                //Strict lexical corroboration: ambiguous provider formatting fails closed.
                if (!string.Equals(unescapedDefault, columnDefault, StringComparison.Ordinal))
                {
                    return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
                }
            }

            var originalCharacterSet = string.Empty;
            var originalCollation = string.Empty;

            const string characterMarker = " CHARACTER SET ";

            var characterIndex = typeAndAttributes.IndexOf(characterMarker, StringComparison.OrdinalIgnoreCase);
            var hasExplicitCharacterAttributes = characterIndex >= 0;
            var originalType = typeAndAttributes;

            if (hasExplicitCharacterAttributes)
            {
                originalType = typeAndAttributes.Substring(0, characterIndex);

                var attributeClause = typeAndAttributes.Substring(characterIndex);

                var attributeMatch = System.Text.RegularExpressions.Regex.Match
                (
                    attributeClause,
                    @"\A CHARACTER SET (?<charset>[A-Za-z][A-Za-z0-9_]{0,63}) COLLATE (?<collation>[A-Za-z][A-Za-z0-9_]{0,63})\z",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant
                    | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                if (!attributeMatch.Success)
                {
                    return "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
                }

                originalCharacterSet = attributeMatch.Groups["charset"].Value;
                originalCollation = attributeMatch.Groups["collation"].Value;
            }

            if (!string.Equals(originalType, columnType, StringComparison.OrdinalIgnoreCase))
            {
                return "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
            }

            var isCharacterColumn = originalType.StartsWith("char(", StringComparison.OrdinalIgnoreCase) || originalType.StartsWith("varchar(", StringComparison.OrdinalIgnoreCase);

            if (!isCharacterColumn)
            {
                //Numeric columns have no effective charset or collation metadata.
                return string.IsNullOrEmpty(characterSet) && string.IsNullOrEmpty(collation) ? string.Empty : "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
            }

            if (!verifiedCharsetCollationPair || string.IsNullOrWhiteSpace(characterSet) || string.IsNullOrWhiteSpace(collation))
            {
                return "F7U3C4C2B_CHARSET_COLLATION_PAIR_UNVERIFIED";
            }

            if (hasExplicitCharacterAttributes)
            {
                return string.Equals(originalCharacterSet, characterSet, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(originalCollation, collation, StringComparison.OrdinalIgnoreCase) ? string.Empty : "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
            }

            //No explicit attributes in SHOW CREATE: MODIFY/CHANGE inherits the current table defaults.
            //Only admit this when those defaults are catalog-verified and exactly equal to the live column.
            return !string.IsNullOrEmpty(tableCharacterSet) && !string.IsNullOrEmpty(tableCollation)
                   && string.Equals(tableCharacterSet, characterSet, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(tableCollation, collation, StringComparison.OrdinalIgnoreCase) ? string.Empty : "F7U3C4B3_ATTRIBUTES_NOT_PROVEN";
        }

        //A length-delimited comparison signature records effective metadata that may change without changing the raw SHOW CREATE column definition (for example table collation).
        //Not a cryptographic credential; only Reader-created immutable receipts may carry it.
        internal static string BuildVerifiedAttributesSignature(params string[] attributes)
        {
            var signature = new System.Text.StringBuilder();

            foreach (var attribute in attributes)
            {
                if (attribute == null)
                {
                    signature.Append("-1:");
                    continue;
                }

                signature.Append(attribute.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
                signature.Append(':');
                signature.Append(attribute);
            }

            return signature.ToString();
        }
    }

    //Receipt is intentionally immutable and has NO public constructor.
    //It records a verified metadata read, not a general capability to execute.
    internal sealed class MySqlVerifiedColumnDefinitionSnapshot
    {
        //Retain the existing constructor for published Golden contracts.
        internal MySqlVerifiedColumnDefinitionSnapshot(Guid connectionId, string databaseName, string tableName, string columnName, string serverVersion, string completeDefinition, long serverConnectionId) : this(connectionId, databaseName, tableName, columnName, serverVersion, completeDefinition, serverConnectionId, string.Empty, null)
        {
        }

        //Reader-owned receipts carry a corroborated effective-attributes signature.
        internal MySqlVerifiedColumnDefinitionSnapshot(Guid connectionId, string databaseName, string tableName, string columnName, string serverVersion, string completeDefinition, long serverConnectionId, string verifiedAttributesSignature) : this(connectionId, databaseName, tableName, columnName, serverVersion, completeDefinition, serverConnectionId, verifiedAttributesSignature, null)
        {
        }

        //Only the Reader's verified query may populate this evidence; never a standalone Execute permit.
        internal MySqlVerifiedColumnDefinitionSnapshot(Guid connectionId, string databaseName, string tableName, string columnName, string serverVersion, string completeDefinition, long serverConnectionId, string verifiedAttributesSignature, string verifiedSessionSqlMode)
        {
            ConnectionId = connectionId;
            ServerConnectionId = serverConnectionId;
            DatabaseName = databaseName;
            TableName = tableName;
            ColumnName = columnName;
            ServerVersion = serverVersion;
            CompleteDefinition = completeDefinition;
            VerifiedAttributesSignature = verifiedAttributesSignature ?? string.Empty;
            VerifiedSessionSqlMode = verifiedSessionSqlMode ?? string.Empty;
            HasVerifiedSessionSqlMode = verifiedSessionSqlMode != null;
        }

        internal Guid ConnectionId { get; }
        internal long ServerConnectionId { get; }
        internal string DatabaseName { get; }
        internal string TableName { get; }
        internal string ColumnName { get; }
        internal string ServerVersion { get; }
        internal string CompleteDefinition { get; }
        internal string VerifiedAttributesSignature { get; }
        internal string VerifiedSessionSqlMode { get; }
        internal bool HasVerifiedSessionSqlMode { get; }
    }


    //F7-U3-C4B3B2A: pure preflight contract. A snapshot is evidence, NOT Execute permission.
    //Only the dedicated Reader may re-read and dispatch after its own transaction, identity and SQL checks.
    internal static class MySqlVerifiedColumnDefinitionExecutionPolicy
    {
        //This is a pure comparison, never an Execute authorization.
        //Mode-neutral comments do not depend on the server's backslash parsing rules.
        internal static string ValidateSessionModeForComment(string previewMode, string liveMode, string comment)
        {
            if (!MySqlColumnCommentLiteralPolicy.HasModeSensitiveCharacters(comment))
            {
                return string.Empty;
            }

            if (!MySqlColumnCommentSessionModePolicy.TryParse(previewMode, out var previewNoBackslash)
                || !MySqlColumnCommentSessionModePolicy.TryParse(liveMode, out var liveNoBackslash))
            {
                return "F7S1_C3C_SESSION_MODE_UNVERIFIED";
            }

            if (previewNoBackslash != liveNoBackslash
                || !string.Equals(previewMode, liveMode, StringComparison.OrdinalIgnoreCase))
            {
                return "F7S1_C3C_SESSION_MODE_CHANGED";
            }

            return string.Empty;
        }

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

            //Preview and live receipts may come from different physical connections.
            //The Reader independently validates its CURRENT connection before DDL dispatch.

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
                || !string.Equals(preview.CompleteDefinition, live.CompleteDefinition, StringComparison.Ordinal)
                || !string.Equals(preview.VerifiedAttributesSignature, live.VerifiedAttributesSignature, StringComparison.Ordinal))
            {
                return "F7U3C4B3B2_DEFINITION_OR_VERSION_CHANGED";
            }

            //A backslash in the original SHOW CREATE COMMENT is mode-sensitive too, even if the requested new Comment contains only ordinary characters.
            var modeSensitiveComment = operation == ColumnDdlOperation.Comment
                                       && (MySqlColumnCommentLiteralPolicy.HasModeSensitiveCharacters(request.Comment)
                                           || (live.CompleteDefinition != null && live.CompleteDefinition.IndexOf('\\') >= 0)
                                           || (preview.CompleteDefinition != null && preview.CompleteDefinition.IndexOf('\\') >= 0));
            var noBackslashEscapes = false;

            if (modeSensitiveComment)
            {
                if (!preview.HasVerifiedSessionSqlMode || !live.HasVerifiedSessionSqlMode
                    || !MySqlColumnCommentSessionModePolicy.TryParse(preview.VerifiedSessionSqlMode, out var previewNoBackslash)
                    || !MySqlColumnCommentSessionModePolicy.TryParse(live.VerifiedSessionSqlMode, out noBackslashEscapes))
                {
                    return "F7S1_C3C_SESSION_MODE_UNVERIFIED";
                }

                if (previewNoBackslash != noBackslashEscapes || !string.Equals(preview.VerifiedSessionSqlMode, live.VerifiedSessionSqlMode, StringComparison.OrdinalIgnoreCase))
                {
                    return "F7S1_C3C_SESSION_MODE_CHANGED";
                }
            }

            var renameSyntax = MySqlColumnDdlVersionPolicy.SelectRenameSyntax(live.ServerVersion);

            if (renameSyntax == MySqlColumnRenameSyntax.Unknown
                || (operation == ColumnDdlOperation.Rename && renameSyntax != MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition))
            {
                //MySQL 8.0.3+ must continue to use its definition-free RENAME COLUMN path.
                return "F7U3C4B3B2_RENAME_SYNTAX_NOT_AUTHORIZED";
            }

            if (!(modeSensitiveComment
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(live.CompleteDefinition, noBackslashEscapes, out _)
                : MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(live.CompleteDefinition, out _)))
            {
                return "F7U3C4B3B2_DEFINITION_NOT_QUALIFIED";
            }

            string rebuiltSql;

            var built = operation == ColumnDdlOperation.Rename
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, live.CompleteDefinition, out rebuiltSql)
                : modeSensitiveComment
                    ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, live.CompleteDefinition, noBackslashEscapes, out rebuiltSql)
                    : MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, live.CompleteDefinition, out rebuiltSql);

            var qualifiedShape = modeSensitiveComment
                ? NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(rebuiltSql, noBackslashEscapes)
                : NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, operation, rebuiltSql);

            if (!built || string.IsNullOrWhiteSpace(displayedSql)
                || !string.Equals(displayedSql, rebuiltSql, StringComparison.Ordinal)
                || !qualifiedShape)
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
        //D3C-04B: classify an unsupported NBE comment input.
        //Feedback only. Never creates SQL or authorizes DDL execution.
        internal static string GetInputRejectionReasonKey(string originalComment, string currentComment, string verifiedSessionSqlMode, bool hasGeneratedSql)
        {
            if (hasGeneratedSql || currentComment == null
                || string.Equals(originalComment ?? string.Empty, currentComment, StringComparison.Ordinal)
                || !MySqlColumnCommentSessionModePolicy.TryParse(verifiedSessionSqlMode, out var noBackslashEscapes)
                || !noBackslashEscapes)
            {
                return string.Empty;
            }

            if (currentComment.IndexOf('\r') < 0 && currentComment.IndexOf('\n') < 0 && currentComment.IndexOf('\t') < 0 && currentComment.IndexOf('\b') < 0 && currentComment.IndexOf('\u001a') < 0)
            {
                return string.Empty;
            }

            return "MySqlColumnCommentNbeControlCharactersUnsupported";
        }

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
                        return "A transaction is unfinished. MySQL DDL may commit unconfirmed changes, so Execute is disabled. "
                               + "In the Query Editor, review the changes and choose Commit or Rollback; then close and reopen this form.";
                    }
                case "MySqlColumnDdlUnsupportedDefinition":
                    {
                        return "JasonQuery cannot yet verify all original column attributes. A change may alter NULL, DEFAULT, "
                               + "CHARACTER SET, COLLATE, or other settings. SQL generation and Execute are disabled. "
                               + "Check SHOW CREATE TABLE before preparing SQL in the Query Editor.";
                    }
                case "MySqlColumnDdlMetadataUnverified":
                    {
                        return "The original column definition could not be verified, so no SQL was generated. "
                               + "Check SHOW CREATE TABLE. After reviewing the SQL and transaction state, you may use the Query Editor.";
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
                        return "The MySQL server version could not be verified. To avoid guessing the DDL syntax, "
                               + "SQL generation and Execute are disabled. Check the connection, reconnect if needed, and reopen this form.";
                    }
                case "MySqlColumnDdlMetadataReadFailed":
                    {
                        return "The column metadata could not be read safely. Check your connection and privileges, then retry.";
                    }
                case "MySqlColumnCommentNbeControlCharactersUnsupported":
                    {
                        return "The MySQL session uses NO_BACKSLASH_ESCAPES. "
                               + "The column comment contains a line break or an unsupported "
                               + "control character. SQL Preview cannot be generated and "
                               + "Execute is disabled. Remove the line break or control "
                               + "character, then review SQL Preview again. "
                               + "Your input will not be changed automatically.";
                    }
                default:
                    {
                        return "The full column definition could not be verified, so SQL generation and Execute are disabled. "
                               + "Check SHOW CREATE TABLE before reviewing the SQL in the Query Editor.";
                    }
            }
        }
    }
}
