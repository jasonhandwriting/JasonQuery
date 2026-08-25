using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.SchemaExplorer
{
    public sealed class C1SchemaExplorerRowReader : ISchemaExplorerRowReader
    {
        private readonly C1TrueDBGrid _grid;

        public C1SchemaExplorerRowReader(C1TrueDBGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        public bool IsGroupRow(int displayRowIndex)
        {
            return GetGroupRow(displayRowIndex) != null;
        }

        public int GetGroupLevel(int displayRowIndex)
        {
            var groupRow = GetGroupRow(displayRowIndex);

            return groupRow == null ? -1 : groupRow.Level;
        }

        public string GetGroupedText(int displayRowIndex)
        {
            var groupRow = GetGroupRow(displayRowIndex);

            return groupRow == null ? string.Empty : groupRow.GroupedText;
        }

        public string GetSchemaTypeFromGroupStartIndex(int displayRowIndex)
        {
            var groupRow = GetGroupRow(displayRowIndex);

            if (groupRow == null)
            {
                return string.Empty;
            }

            try
            {
                return Convert.ToString(_grid.Columns["SchemaType"].CellValue(groupRow.StartIndex));
            }
            catch
            {
                return string.Empty;
            }
        }

        public string FindPreviousGroupText(int displayRowIndex, int groupLevel)
        {
            if (displayRowIndex <= 0)
            {
                return string.Empty;
            }

            if (_grid.Splits == null || _grid.Splits.Count == 0)
            {
                return string.Empty;
            }

            var rows = _grid.Splits[0].Rows;

            if (rows == null)
            {
                return string.Empty;
            }

            for (var i = displayRowIndex - 1; i >= 0; i--)
            {
                if (i >= rows.Count)
                {
                    continue;
                }

                if (!(rows[i] is GroupRow groupRow))
                {
                    continue;
                }

                if (groupRow.Level != groupLevel)
                {
                    continue;
                }

                return groupRow.GroupedText;
            }

            return string.Empty;
        }

        public DataRow GetDataRow(int displayRowIndex)
        {
            if (displayRowIndex < 0)
            {
                return null;
            }

            try
            {
                var dataRowView = _grid.GetDataBoundItem(displayRowIndex) as DataRowView;

                return dataRowView?.Row;
            }
            catch
            {
                return null;
            }
        }

        public DataTable GetSchemaDataTable()
        {
            var dataSource = _grid.DataSource;

            while (dataSource is BindingSource bindingSource)
            {
                dataSource = bindingSource.DataSource;
            }

            if (dataSource is DataTable dataTable)
            {
                return dataTable;
            }

            if (dataSource is DataView dataView)
            {
                return dataView.Table;
            }

            return null;
        }

        public void SetCurrentRow(int displayRowIndex)
        {
            if (displayRowIndex < 0)
            {
                return;
            }

            try
            {
                _grid.Row = displayRowIndex;
            }
            catch
            {
                //忽略 C1TrueDBGrid 在特殊列或資料重繫結期間可能發生的 Row 設定失敗
            }
        }

        private object GetSplitRow(int displayRowIndex)
        {
            if (displayRowIndex < 0)
            {
                return null;
            }

            if (_grid.Splits == null || _grid.Splits.Count == 0)
            {
                return null;
            }

            var rows = _grid.Splits[0].Rows;

            if (rows == null)
            {
                return null;
            }

            if (displayRowIndex >= rows.Count)
            {
                return null;
            }

            return rows[displayRowIndex];
        }

        private GroupRow GetGroupRow(int displayRowIndex)
        {
            return GetSplitRow(displayRowIndex) as GroupRow;
        }
    }
}
