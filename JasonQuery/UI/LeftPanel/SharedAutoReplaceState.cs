using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace JasonQuery.UI.LeftPanel
{
    //One instance belongs to MainForm for the lifetime of the workspace.
    //Only committed working-table edits are published to the editor consumers.
    public sealed class SharedAutoReplaceState
    {
        public const string KeywordColumn = "Keyword";
        public const string ReplacementColumn = "Replacement";

        private const int InitialBlankRowCount = 10;
        private static readonly IDictionary<string, string> EmptyMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        private IDictionary<string, string> _publishedMap = EmptyMap;
        private List<KeyValuePair<string, string>> _permanentRows;

        public DataTable WorkingTable { get; private set; }

        public bool EnsureLoaded(bool enabled, Func<DataTable> loadRules)
        {
            if (!enabled)
            {
                return false;
            }

            if (WorkingTable != null)
            {
                return true;
            }

            return RefreshPermanentRules(true, loadRules);
        }

        //Compare with the last permanent source, never with the editable working table.
        public bool RefreshPermanentRules(bool enabled, Func<DataTable> loadRules)
        {
            if (!enabled)
            {
                return false;
            }

            if (loadRules == null)
            {
                throw new ArgumentNullException(nameof(loadRules));
            }

            var source = loadRules();

            if (source == null || !source.Columns.Contains(KeywordColumn) || !source.Columns.Contains(ReplacementColumn))
            {
                throw new InvalidOperationException("Auto Replace rules must contain Keyword and Replacement columns.");
            }

            var permanentRows = new List<KeyValuePair<string, string>>();

            foreach (DataRow row in source.Rows)
            {
                if (row.RowState == DataRowState.Deleted || !row.HasVersion(DataRowVersion.Current))
                {
                    continue;
                }

                permanentRows.Add(new KeyValuePair<string, string>
                (
                    row[KeywordColumn, DataRowVersion.Current]?.ToString(),
                    row[ReplacementColumn, DataRowVersion.Current]?.ToString())
                );
            }

            if (WorkingTable != null && _permanentRows.SequenceEqual(permanentRows))
            {
                return true;
            }

            var table = new DataTable();

            table.Columns.Add(KeywordColumn, typeof(string));
            table.Columns.Add(ReplacementColumn, typeof(string));

            foreach (DataRow row in source.Rows)
            {
                if (row.RowState == DataRowState.Deleted || !row.HasVersion(DataRowVersion.Current))
                {
                    continue;
                }

                table.Rows.Add(row[KeywordColumn, DataRowVersion.Current], row[ReplacementColumn, DataRowVersion.Current]);
            }

            for (var i = 0; i < InitialBlankRowCount; i++)
            {
                table.Rows.Add(string.Empty, string.Empty);
            }

            var map = BuildMap(table);

            _permanentRows = permanentRows;
            WorkingTable = table;
            _publishedMap = map;

            return true;
        }

        public void PublishEdits(bool enabled)
        {
            if (!enabled || WorkingTable == null)
            {
                return;
            }

            _publishedMap = BuildMap(WorkingTable);
        }

        public IDictionary<string, string> GetMap(bool enabled)
        {
            return enabled ? _publishedMap : EmptyMap;
        }

        private static IDictionary<string, string> BuildMap(DataTable table)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                //Use the committed value, not a pending BeginEdit/Proposed value.
                if (!row.HasVersion(DataRowVersion.Current))
                {
                    continue;
                }

                var keyword = row[KeywordColumn, DataRowVersion.Current]?.ToString();
                var replacement = row[ReplacementColumn, DataRowVersion.Current]?.ToString();

                if (!string.IsNullOrWhiteSpace(keyword) && !string.IsNullOrWhiteSpace(replacement))
                {
                    //Preserve the legacy case-insensitive, last-valid-row-wins contract.
                    map[keyword] = replacement;
                }
            }

            return new ReadOnlyDictionary<string, string>(map);
        }
    }
}
