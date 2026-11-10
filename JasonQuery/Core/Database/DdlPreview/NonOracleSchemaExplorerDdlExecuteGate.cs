using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    //F7-U2-C3A: testable policy only. This class neither opens connections nor dispatches SQL.
    //The caller must supply live, trustworthy scope and target evidence at Execute time.
    internal static class NonOracleSchemaExplorerDdlExecuteGate
    {
        public static string ValidateTarget(DataSourceType provider, string selectedDatabase, string selectedSchema, string selectedTable, int? selectedObjectId, string liveDatabase, string liveSchema, string liveTable, int? liveObjectId, bool qualifiedMetadata, bool sameConnection, string displayedPreview, string rebuiltPreview)
        {
            if (!IsSupportedProvider(provider))
            {
                return "F7U2C2_INVALID_PROVIDER";
            }

            if (!qualifiedMetadata)
            {
                return "F7U2C2_METADATA_UNQUALIFIED";
            }

            if (!sameConnection)
            {
                return "F7U2C2_CONNECTION_CHANGED";
            }

            if (string.IsNullOrWhiteSpace(selectedDatabase) || string.IsNullOrWhiteSpace(liveDatabase) || string.IsNullOrWhiteSpace(selectedTable) || string.IsNullOrWhiteSpace(liveTable))
            {
                return "F7U2C2_TARGET_INCOMPLETE";
            }

            if (provider != DataSourceType.MySql && (string.IsNullOrWhiteSpace(selectedSchema) || string.IsNullOrWhiteSpace(liveSchema)))
            {
                return "F7U2C2_TARGET_INCOMPLETE";
            }

            if (provider == DataSourceType.SqlServer && (!selectedObjectId.HasValue || selectedObjectId.Value <= 0 || !liveObjectId.HasValue || liveObjectId.Value <= 0))
            {
                return "F7U2C2_TARGET_INCOMPLETE";
            }

            if (string.IsNullOrWhiteSpace(displayedPreview) || string.IsNullOrWhiteSpace(rebuiltPreview))
            {
                return "F7U2C2_SQL_EMPTY";
            }

            //Never normalize SQL: the executed statement must match the rebuilt SQL exactly.
            if (!string.Equals(displayedPreview, rebuiltPreview, StringComparison.Ordinal))
            {
                return "F7U2C2_SQL_CHANGED";
            }

            //Conservative comparison: reject uncertain identifier casing rather than guess the database/server collation or MySQL lower_case_table_names setting.
            if (!string.Equals(selectedDatabase, liveDatabase, StringComparison.Ordinal))
            {
                return "F7U2C2_DATABASE_MISMATCH";
            }

            if (provider != DataSourceType.MySql && !string.Equals(selectedSchema, liveSchema, StringComparison.Ordinal))
            {
                return "F7U2C2_SCHEMA_MISMATCH";
            }

            if (!string.Equals(selectedTable, liveTable, StringComparison.Ordinal))
            {
                return "F7U2C2_TABLE_MISMATCH";
            }

            if (provider == DataSourceType.SqlServer && selectedObjectId.Value != liveObjectId.Value)
            {
                return "F7U2C2_OBJECT_ID_MISMATCH";
            }

            return string.Empty;
        }

        //F7-U2-C98: verify the immutable F7 selection against the form and exact SQL.
        //Only PostgreSQL additionally requires the live database from the open Reader.
        //SQL Server and MySQL use database-qualified builder SQL; the database engine is responsible for checking object existence and DDL permissions.
        public static string ValidateFormIntent(DataSourceType provider, string selectedDatabase, string selectedSchema, string selectedTable, string formDatabase, string formSchema, string formTable, string livePostgreSqlDatabase, string displayedSql, string rebuiltSql)
        {
            if (!IsSupportedProvider(provider))
            {
                return "F7U2C2_INVALID_PROVIDER";
            }

            if (string.IsNullOrWhiteSpace(selectedDatabase) || string.IsNullOrWhiteSpace(selectedTable) || string.IsNullOrWhiteSpace(formTable) || (provider != DataSourceType.MySql && string.IsNullOrWhiteSpace(selectedSchema)))
            {
                return "F7U2C2_TARGET_INCOMPLETE";
            }

            if (provider != DataSourceType.PostgreSql && !string.Equals(selectedDatabase, formDatabase, StringComparison.Ordinal))
            {
                return "F7U2C2_DATABASE_MISMATCH";
            }

            if (provider == DataSourceType.PostgreSql && !string.Equals(selectedDatabase, livePostgreSqlDatabase, StringComparison.Ordinal))
            {
                return "F7U2C2_DATABASE_MISMATCH";
            }

            if (provider != DataSourceType.MySql && !string.Equals(selectedSchema, formSchema, StringComparison.Ordinal))
            {
                return "F7U2C2_SCHEMA_MISMATCH";
            }

            if (!string.Equals(selectedTable, formTable, StringComparison.Ordinal))
            {
                return "F7U2C2_TABLE_MISMATCH";
            }

            if (string.IsNullOrWhiteSpace(displayedSql) || string.IsNullOrWhiteSpace(rebuiltSql))
            {
                return "F7U2C2_SQL_EMPTY";
            }

            return string.Equals(displayedSql, rebuiltSql, StringComparison.Ordinal) ? string.Empty : "F7U2C2_SQL_CHANGED";
        }

        public static string TryExecuteF7(DataSourceType provider, bool pendingOrUnknownTransaction, bool mysqlImplicitCommitAcknowledged, Func<string> ensureConnection, Func<string> verifyLiveScopeTargetAndSql, Func<Guid> getCurrentConnectionId, Func<Guid, string> guardedDispatch)
        {
            if (!IsSupportedProvider(provider))
            {
                return "F7U2C2_INVALID_PROVIDER";
            }

            //Do not even reconnect while a transaction could be lost or committed.
            if (pendingOrUnknownTransaction)
            {
                return "F7U2C2_TRANSACTION_UNSAFE";
            }

            //MySQL ALTER TABLE may implicitly commit; acknowledgment is not authorization.
            if (provider == DataSourceType.MySql && !mysqlImplicitCommitAcknowledged)
            {
                return "F7U2C2_MYSQL_IMPLICIT_COMMIT_CONFIRMATION_REQUIRED";
            }

            if (ensureConnection == null || verifyLiveScopeTargetAndSql == null || getCurrentConnectionId == null || guardedDispatch == null)
            {
                return "F7U2C2_INVALID_GATE";
            }

            string reason;

            try
            {
                reason = ensureConnection();
            }
            catch (Exception)
            {
                return "F7U2C2_CONNECTION_FAILED";
            }

            if (reason == null)
            {
                return "F7U2C2_CONNECTION_FAILED";
            }

            if (reason.Length != 0)
            {
                return reason;
            }

            Guid verifiedConnectionId;

            try
            {
                verifiedConnectionId = getCurrentConnectionId();
            }
            catch (Exception)
            {
                return "F7U2C2_CONNECTION_UNVERIFIED";
            }

            if (verifiedConnectionId == Guid.Empty)
            {
                return "F7U2C2_CONNECTION_UNVERIFIED";
            }

            try
            {
                reason = verifyLiveScopeTargetAndSql();
            }
            catch (Exception)
            {
                return "F7U2C2_VERIFICATION_FAILED";
            }

            if (reason == null)
            {
                return "F7U2C2_VERIFICATION_FAILED";
            }

            if (reason.Length != 0)
            {
                return reason;
            }

            Guid currentConnectionId;

            try
            {
                currentConnectionId = getCurrentConnectionId();
            }
            catch (Exception)
            {
                return "F7U2C2_CONNECTION_CHANGED";
            }

            if (currentConnectionId == Guid.Empty || currentConnectionId != verifiedConnectionId)
            {
                return "F7U2C2_CONNECTION_CHANGED";
            }

            //The C3-B Reader entry must independently recheck the same connection ID, must not reconnect, and must not retry after a possibly dispatched DDL.
            try
            {
                reason = guardedDispatch(verifiedConnectionId);
            }
            catch (Exception)
            {
                return "F7U2C2_EXECUTION_OUTCOME_UNKNOWN";
            }

            return reason ?? "F7U2C2_EXECUTION_OUTCOME_UNKNOWN";
        }

        //U3-B1: operation-specific SQL-shape gate. Exact target and SQL equality are independently checked at Execute time.
        //The provider Reader also requires one parsed statement on the same verified physical connection.
        public static bool IsQualifiedTableCommentSql(DataSourceType provider, string sql)
        {
            if (!IsSupportedProvider(provider) || string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.Singleline;

            switch (provider)
            {
                case DataSourceType.PostgreSql:
                    {
                        return System.Text.RegularExpressions.Regex.IsMatch(sql, @"^\s*COMMENT\s+ON\s+TABLE\s+.+?\s+IS\s+'(?:''|[^'])*';?\s*$", options);
                    }
                case DataSourceType.SqlServer:
                    {
                        return System.Text.RegularExpressions.Regex.IsMatch(sql, @"^\s*EXEC\s+\[[^\r\n]+\]\.\[sys\]\.\[sp_(?:add|update)extendedproperty\]\s+@name=N'MS_Description',.*@level1type=N'TABLE',.*@level1name=N'(?:''|[^'])*';?\s*$", options);
                    }
                case DataSourceType.MySql:
                    {
                        return System.Text.RegularExpressions.Regex.IsMatch(sql, @"^\s*ALTER\s+TABLE\s+.+?\s+COMMENT\s*=\s*'(?:''|[^'])*';?\s*$", options);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        //U3-B2: this is only the SQL-shape whitelist. Caller must also verify captured target, exact form SQL, connection and transaction; Reader rejects multi-statement scripts.
        public static bool IsQualifiedTableDdlSql(DataSourceType provider, TableDdlOperation operation, string sql)
        {
            if (!IsSupportedProvider(provider) || string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            if (operation == TableDdlOperation.Comment)
            {
                return IsQualifiedTableCommentSql(provider, sql);
            }

            const System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.CultureInvariant;
            const string pgIdentifier = "\"(?:\"\"|[^\";\\r\\n])+\"";
            const string sqlIdentifier = @"\[(?:\]\]|[^\];\r\n])+\]";
            const string myIdentifier = @"`(?:``|[^`;\r\n])+`";

            var pgTable = pgIdentifier + @"\." + pgIdentifier;
            var sqlTable = sqlIdentifier + @"\." + sqlIdentifier + @"\." + sqlIdentifier;
            var myTable = myIdentifier + @"\." + myIdentifier;
            string pattern;

            switch (provider)
            {
                case DataSourceType.PostgreSql:
                    {
                        switch (operation)
                        {
                            case TableDdlOperation.Rename:
                                {
                                    pattern = @"^\s*ALTER\s+TABLE\s+" + pgTable + @"\s+RENAME\s+TO\s+" + pgIdentifier + @";?\s*$";
                                    break;
                                }
                            case TableDdlOperation.Drop:
                                {
                                    pattern = @"^\s*DROP\s+TABLE\s+" + pgTable + @"(?:\s+CASCADE)?;\s*$";
                                    break;
                                }
                            case TableDdlOperation.Truncate:
                                {
                                    pattern = @"^\s*TRUNCATE\s+TABLE\s+(?:ONLY\s+)?" + pgTable + @"(?:\s+RESTART\s+IDENTITY)?(?:\s+CASCADE)?;\s*$";
                                    break;
                                }
                            default:
                                {
                                    return false;
                                }
                        }
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        switch (operation)
                        {
                            case TableDdlOperation.Rename:
                                {
                                    pattern = @"^\s*EXEC\s+" + sqlIdentifier + @"\.\[sys\]\.\[sp_rename\]\s+@objname=N'(?:''|[^';\r\n])*',\s+@newname=N'(?:''|[^';\r\n])*';\s*$";
                                    break;
                                }
                            case TableDdlOperation.Drop:
                                {
                                    pattern = @"^\s*DROP\s+TABLE\s+" + sqlTable + @";?\s*$";
                                    break;
                                }
                            case TableDdlOperation.Truncate:
                                {
                                    pattern = @"^\s*TRUNCATE\s+TABLE\s+" + sqlTable + @";?\s*$";
                                    break;
                                }
                            default:
                                {
                                    return false;
                                }
                        }
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        switch (operation)
                        {
                            case TableDdlOperation.Rename:
                                {
                                    pattern = @"^\s*RENAME\s+TABLE\s+" + myTable + @"\s+TO\s+" + myTable + @";?\s*$";
                                    break;
                                }
                            case TableDdlOperation.Drop:
                                {
                                    pattern = @"^\s*DROP\s+TABLE\s+" + myTable + @";?\s*$";
                                    break;
                                }
                            case TableDdlOperation.Truncate:
                                {
                                    pattern = @"^\s*TRUNCATE\s+TABLE\s+" + myTable + @";?\s*$";
                                    break;
                                }
                            default:
                                {
                                    return false;
                                }
                        }
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            return System.Text.RegularExpressions.Regex.IsMatch(sql, pattern, options | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private static bool IsSupportedProvider(DataSourceType provider)
        {
            return provider == DataSourceType.PostgreSql || provider == DataSourceType.SqlServer || provider == DataSourceType.MySql;
        }
    }
}
