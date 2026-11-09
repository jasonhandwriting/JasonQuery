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

        private static bool IsSupportedProvider(DataSourceType provider)
        {
            return provider == DataSourceType.PostgreSql || provider == DataSourceType.SqlServer || provider == DataSourceType.MySql;
        }
    }
}
