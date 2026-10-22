using JasonQuery.Core.Database.Connection;
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

        internal SharedSchemaMetadataSnapshot(DataSourceType sourceType, int connectionRevision, DataTable table, string[] groupColumns, string[] hiddenColumns)
        {
            SourceType = sourceType;
            ConnectionRevision = connectionRevision;
            Table = table;
            _groupColumns = (string[])groupColumns.Clone();
            _hiddenColumns = (string[])hiddenColumns.Clone();
        }

        public DataSourceType SourceType { get; }
        public int ConnectionRevision { get; }

        //Owned copy. A grid may modify this copy but never the caller's DataTable.
        public DataTable Table { get; }
        public string[] GroupColumns => (string[])_groupColumns.Clone();
        public string[] HiddenColumns => (string[])_hiddenColumns.Clone();
    }

    public static class SharedSchemaMetadataSnapshotPolicy
    {
        public static bool TryCapture(DataTable source, DataSourceType sourceType, bool includeColumnGroup, int connectionRevision, out SharedSchemaMetadataSnapshot snapshot)
        {
            snapshot = null;

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

            if (sourceType == DataSourceType.SqlServer)
            {
                hidden = new[] { "SchemaDbo", "ObjectID", "CreateDate", "ModifyDate" };
            }

            if (sourceType == DataSourceType.MySql)
            {
                hidden = new[] { "CreateDate", "ModifyDate" };
            }

            snapshot = new SharedSchemaMetadataSnapshot(sourceType, connectionRevision, owned, groups.ToArray(), hidden);
            return true;
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
