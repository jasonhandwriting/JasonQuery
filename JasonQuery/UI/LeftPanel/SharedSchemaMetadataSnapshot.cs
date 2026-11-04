using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C3B: independent, connection-scoped snapshot. Never rename global dtSchema columns.
    public sealed class SharedSchemaMetadataSnapshot
    {
        private readonly string[] _groupColumns;
        private readonly string[] _hiddenColumns;

        internal SharedSchemaMetadataSnapshot(DataSourceType sourceType, int connectionRevision, DataTable table, string[] groupColumns, string[] hiddenColumns, string oracleVerifiedSessionUser, Guid oracleConnectionId, string databaseScope = "")
        {
            SourceType = sourceType;
            ConnectionRevision = connectionRevision;
            OracleVerifiedSessionUser = oracleVerifiedSessionUser ?? string.Empty;
            OracleConnectionId = oracleConnectionId;
            DatabaseScope = databaseScope ?? string.Empty;
            Table = table;
            _groupColumns = (string[])groupColumns.Clone();
            _hiddenColumns = (string[])hiddenColumns.Clone();
        }

        public DataSourceType SourceType { get; }
        public int ConnectionRevision { get; }
        public string OracleVerifiedSessionUser { get; }
        public Guid OracleConnectionId { get; }
        public string DatabaseScope { get; }

        //Owned copy. A grid may modify this copy but never the caller's DataTable.
        public DataTable Table { get; }
        public string[] GroupColumns => (string[])_groupColumns.Clone();
        public string[] HiddenColumns => (string[])_hiddenColumns.Clone();
    }

    public static class SharedSchemaMetadataSnapshotPolicy
    {
        public static bool TryCapture(DataTable source, DataSourceType sourceType, bool includeColumnGroup, int connectionRevision, out SharedSchemaMetadataSnapshot snapshot)
        {
            //Preserve the legacy, source-neutral capture API for existing callers/tests.
            return TryCaptureCore(source, sourceType, includeColumnGroup, connectionRevision, out snapshot, string.Empty, Guid.Empty, false);
        }

        public static bool TryCapture(DataTable source, DataSourceType sourceType, bool includeColumnGroup, int connectionRevision, out SharedSchemaMetadataSnapshot snapshot, string oracleVerifiedSessionUser, Guid oracleConnectionId)
        {
            return TryCaptureCore(source, sourceType, includeColumnGroup, connectionRevision, out snapshot, oracleVerifiedSessionUser, oracleConnectionId, true);
        }

        private static bool TryCaptureCore(DataTable source, DataSourceType sourceType, bool includeColumnGroup, int connectionRevision, out SharedSchemaMetadataSnapshot snapshot, string oracleVerifiedSessionUser, Guid oracleConnectionId, bool requireVerifiedOracleSession, string databaseScope = "")
        {
            snapshot = null;

            if (requireVerifiedOracleSession && sourceType == DataSourceType.Oracle && (string.IsNullOrWhiteSpace(oracleVerifiedSessionUser) || oracleConnectionId == Guid.Empty || source == null || !source.Columns.Contains("OracleMetadataOwner")))
            {
                return false;
            }

            if (source == null || sourceType == DataSourceType.None || connectionRevision <= 0)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(DataSourceType), sourceType))
            {
                return false;
            }

            var groups = new List<string> { "SchemaObject" };

            if (sourceType != DataSourceType.Oracle)
            {
                groups.Add("SchemaNode");
            }

            groups.Add("SchemaType");

            if (includeColumnGroup)
            {
                groups.Add("SchemaName");
            }

            foreach (var name in groups)
            {
                if (!source.Columns.Contains(name))
                {
                    return false;
                }
            }

            //DataTable.Copy also clones rows. Rename only the clone for the browser view.
            var owned = source.Copy();

            if (owned.Columns.Contains("ColumnInfo") && !owned.Columns.Contains("Schema_Browser"))
            {
                owned.Columns["ColumnInfo"].ColumnName = "Schema_Browser";
            }

            string[] hidden = Array.Empty<string>();

            if (sourceType == DataSourceType.Oracle)
            {
                //Keep raw table/column identifiers in the owned DataTable for the next qualified Column bridge, but never display them as normal grid columns.
                var oracleHidden = new List<string>();

                foreach (var name in new[] { "OracleMetadataOwner", "OracleMetadataTableName", "OracleMetadataColumnName" })
                {
                    if (owned.Columns.Contains(name))
                    {
                        oracleHidden.Add(name);
                    }
                }

                hidden = oracleHidden.ToArray();
            }

            if (sourceType == DataSourceType.SqlServer)
            {
                hidden = new[] { "SchemaDbo", "ObjectID", "CreateDate", "ModifyDate" };
            }

            if (sourceType == DataSourceType.MySql)
            {
                hidden = new[] { "CreateDate", "ModifyDate" };
            }

            snapshot = new SharedSchemaMetadataSnapshot(sourceType, connectionRevision, owned, groups.ToArray(), hidden, oracleVerifiedSessionUser, oracleConnectionId, databaseScope);
            return true;
        }

        //Only source-neutral providers may use this API. Oracle needs a verified load record, never merely a caller-provided scope string.
        public static bool TryCaptureForScope(DataTable source, DataSourceType sourceType, bool includeColumnGroup, int connectionRevision, string databaseScope, out SharedSchemaMetadataSnapshot snapshot)
        {
            snapshot = null;

            if (sourceType == DataSourceType.Oracle || string.IsNullOrWhiteSpace(databaseScope))
            {
                return false;
            }

            return TryCaptureCore(source, sourceType, includeColumnGroup, connectionRevision, out snapshot, string.Empty, Guid.Empty, false, databaseScope);
        }

        //Separate, verified Oracle path. A scope string alone never qualifies Oracle data.
        internal static bool TryCaptureVerifiedOracleForScope(DataTable source, bool includeColumnGroup, int connectionRevision, string databaseScope, string verifiedSessionUser, Guid verifiedConnectionId, out SharedSchemaMetadataSnapshot snapshot)
        {
            snapshot = null;

            if (string.IsNullOrWhiteSpace(databaseScope))
            {
                return false;
            }

            return TryCaptureCore(source, DataSourceType.Oracle, includeColumnGroup, connectionRevision, out snapshot, verifiedSessionUser, verifiedConnectionId, true, databaseScope);
        }

        //B2c-3B2: no public caller-authored provenance can qualify Oracle.
        //The receipt proves a completed physical session batch, not a MainForm logical revision.
        //Fail closed until the trusted logical-revision correlation is connected in B2c-3C.
        internal static bool TryCaptureQualifiedOracle(DataTable source, bool includeColumnGroup, int connectionRevision, string databaseScope, OracleVerifiedSessionScope observedScope, SharedOracleMetadataCompletedLoadReceipt receipt, out SharedSchemaMetadataSnapshot snapshot)
        {
            snapshot = null;

            if (receipt == null || source == null || observedScope == null
                || !ReferenceEquals(receipt.SourceTable, source)
                || !ReferenceEquals(DatabaseSqlExecutor.dtSchema, source)
                || receipt.CompletedLoadId == Guid.Empty
                || !SharedOracleMetadataCompletedLoadReceipt.HasRawOracleIdentifiers(source)
                || string.IsNullOrWhiteSpace(databaseScope)
                || connectionRevision <= 0
                || receipt.TrustedLogicalConnectionRevision != connectionRevision
                || !OracleVerifiedSessionScopePolicy.CanBindMetadataToSession(observedScope, receipt.ConnectionId, receipt.SessionUser, receipt.DatabaseName, receipt.ContainerName))
            {
                return false;
            }

            return TryCaptureCore(source, DataSourceType.Oracle, includeColumnGroup, connectionRevision, out snapshot, receipt.SessionUser, receipt.ConnectionId, true, databaseScope);
        }

        public static bool CanBindForScope(SharedLeftPanelHostState state, SharedSchemaMetadataSnapshot snapshot, DataSourceType currentSourceType, string databaseScope)
        {
            return !string.IsNullOrWhiteSpace(databaseScope)
                   && snapshot != null
                   && !string.IsNullOrWhiteSpace(snapshot.DatabaseScope)
                   && string.Equals(snapshot.DatabaseScope, databaseScope, StringComparison.Ordinal)
                   && CanBind(state, snapshot, currentSourceType);
        }

        public static bool CanBind(SharedLeftPanelHostState state, SharedSchemaMetadataSnapshot snapshot, DataSourceType currentSourceType)
        {
            //Snapshot validity follows the connection, not the selected right-hand Tab.
            return state != null
                   && !state.IsDisposed
                   && state.IsConnected
                   && snapshot != null
                   && currentSourceType != DataSourceType.None
                   && snapshot.SourceType == currentSourceType
                   && snapshot.ConnectionRevision == state.ConnectionRevision;
        }
    }
}
