using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //The source table is replaced on each QueryForm navigator refresh.
    //The display table is an independent copy, never the QueryForm working table.
    public sealed class SharedSqlNavigatorSnapshot
    {
        private readonly DataTable _source;
        private readonly string _sourceEditorText;

        internal SharedSqlNavigatorSnapshot(string sourceIdentity, DataTable source, string sourceEditorText, DataTable table)
        {
            SourceIdentity = sourceIdentity;
            _source = source;
            _sourceEditorText = sourceEditorText;
            Table = table;
        }

        public string SourceIdentity { get; }
        public DataTable Table { get; }

        public bool IsCurrent(LeftPanelContext context, string activeIdentity, DataTable currentSource, string currentEditorText)
        {
            return context != null
                   && LeftPanelAvailabilityPolicy.Resolve(context).SqlList
                   && !string.IsNullOrWhiteSpace(SourceIdentity)
                   && string.Equals(SourceIdentity, activeIdentity, StringComparison.Ordinal)
                   && _source != null
                   && ReferenceEquals(_source, currentSource)
                   && _sourceEditorText != null
                   && string.Equals(_sourceEditorText, currentEditorText, StringComparison.Ordinal)
                   && Table.Rows.Count > 0;
        }

        public bool TryResolvePosition(LeftPanelContext context, string activeIdentity, DataTable currentSource, string currentEditorText, int oneBasedPosition, out int zeroBasedPosition)
        {
            zeroBasedPosition = -1;

            if (!IsCurrent(context, activeIdentity, currentSource, currentEditorText)
                || oneBasedPosition <= 0
                || oneBasedPosition > currentEditorText.Length
                || !SharedSqlNavigatorSnapshotPolicy.ContainsPosition(Table, oneBasedPosition)
                || !SharedSqlNavigatorSnapshotPolicy.ContainsPosition(currentSource, oneBasedPosition))
            {
                return false;
            }

            zeroBasedPosition = oneBasedPosition - 1;
            return true;
        }
    }

    public static class SharedSqlNavigatorSnapshotPolicy
    {
        public static SharedSqlNavigatorSnapshot Create(string sourceIdentity, DataTable source, string sourceEditorText)
        {
            var table = new DataTable();

            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Sql", typeof(string));
            table.Columns.Add("Position", typeof(int));

            if (!string.IsNullOrWhiteSpace(sourceIdentity) && sourceEditorText != null && HasCanonicalColumns(source))
            {
                foreach (DataRow row in source.Rows)
                {
                    if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached
                        || row.IsNull("Type") || row.IsNull("Sql") || row.IsNull("Position"))
                    {
                        continue;
                    }

                    var position = (int)row["Position"];
                    var sql = (string)row["Sql"];

                    if (position <= 0 || position > sourceEditorText.Length || string.IsNullOrWhiteSpace(sql))
                    {
                        continue;
                    }

                    table.Rows.Add(row["Type"], sql, position);
                }
            }

            return new SharedSqlNavigatorSnapshot(sourceIdentity, source, sourceEditorText, table);
        }

        internal static bool ContainsPosition(DataTable table, int position)
        {
            if (!HasCanonicalColumns(table))
            {
                return false;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached
                    && !row.IsNull("Position") && (int)row["Position"] == position)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCanonicalColumns(DataTable table)
        {
            return table != null
                   && table.Columns.Contains("Type") && table.Columns["Type"].DataType == typeof(string)
                   && table.Columns.Contains("Sql") && table.Columns["Sql"].DataType == typeof(string)
                   && table.Columns.Contains("Position") && table.Columns["Position"].DataType == typeof(int);
        }
    }
}
