using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema;
using JasonQuery.Core.Schema;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Helpers
{
    public static class GridHeadingCellTipHelper
    {
        private const int ToolTipOffsetX = -20;
        private const int ToolTipOffsetY = 16;
        private const int ToolTipDuration = 8000;

        private sealed class GridHeadingCellTipContext
        {
            public ColumnInfoCollector ColumnInfoCollector { get; set; }

            public GridHeadingCellTipOptions Options { get; set; }

            public ToolTip ToolTip { get; set; }

            public bool IsMouseEventBound { get; set; }

            public string LastColumnName { get; set; }

            public string LastCellTipText { get; set; }
        }

        public sealed class GridHeadingCellTipOptions
        {
            public bool ShowPrimaryKey { get; set; } = true;

            public bool ShowNotNull { get; set; } = true;

            public bool ShowDataType { get; set; }

            public bool ShowComment { get; set; }
        }

        private static readonly ConditionalWeakTable<C1TrueDBGrid, GridHeadingCellTipContext> _contexts
                                = new ConditionalWeakTable<C1TrueDBGrid, GridHeadingCellTipContext>();

        public static void Apply(ColumnInfoCollector columnInfoCollector, C1TrueDBGrid c1Grid, GridHeadingCellTipOptions options = null)
        {
            if (c1Grid == null)
            {
                return;
            }

            if (columnInfoCollector == null)
            {
                Clear(c1Grid);
                return;
            }

            var context = _contexts.GetOrCreateValue(c1Grid);

            context.ColumnInfoCollector = columnInfoCollector;
            context.Options = options ?? new GridHeadingCellTipOptions();
            context.LastColumnName = null;
            context.LastCellTipText = null;

            if (context.ToolTip == null)
            {
                context.ToolTip = CreateToolTip();
            }

            c1Grid.CellTips = CellTipEnum.NoCellTips;

            if (!context.IsMouseEventBound)
            {
                c1Grid.MouseMove += C1Grid_MouseMove;
                c1Grid.MouseLeave += C1Grid_MouseLeave;
                c1Grid.Disposed += C1Grid_Disposed;
                context.IsMouseEventBound = true;
            }

            ShowCellTipAfterGridReady(c1Grid);
        }

        public static void Clear(C1TrueDBGrid c1Grid)
        {
            if (c1Grid == null)
            {
                return;
            }

            if (_contexts.TryGetValue(c1Grid, out var context))
            {
                if (context.IsMouseEventBound)
                {
                    c1Grid.MouseMove -= C1Grid_MouseMove;
                    c1Grid.MouseLeave -= C1Grid_MouseLeave;
                    c1Grid.Disposed -= C1Grid_Disposed;
                    context.IsMouseEventBound = false;
                }

                if (context.ToolTip != null)
                {
                    context.ToolTip.Hide(c1Grid);
                    context.ToolTip.Dispose();
                    context.ToolTip = null;
                }

                context.ColumnInfoCollector = null;
                context.LastColumnName = null;
                context.LastCellTipText = null;
            }

            _contexts.Remove(c1Grid);

            if (!c1Grid.IsDisposed)
            {
                c1Grid.CellTips = CellTipEnum.NoCellTips;
            }
        }

        private static ToolTip CreateToolTip()
        {
            return new ToolTip
            {
                AutoPopDelay = ToolTipDuration,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true,
                UseAnimation = false,
                UseFading = false
            };
        }

        private static void ShowCellTipAfterGridReady(C1TrueDBGrid c1Grid)
        {
            if (c1Grid == null || c1Grid.IsDisposed)
            {
                return;
            }

            if (!c1Grid.IsHandleCreated)
            {
                c1Grid.HandleCreated -= C1Grid_HandleCreated;
                c1Grid.HandleCreated += C1Grid_HandleCreated;
                return;
            }

            c1Grid.BeginInvoke((MethodInvoker)(() =>
            {
                if (c1Grid.IsDisposed)
                {
                    return;
                }

                var mousePoint = c1Grid.PointToClient(Cursor.Position);

                if (!c1Grid.ClientRectangle.Contains(mousePoint))
                {
                    HideCellTip(c1Grid);
                    return;
                }

                ShowCellTipAtPoint(c1Grid, mousePoint);
            }));
        }

        private static void C1Grid_HandleCreated(object sender, EventArgs e)
        {
            if (!(sender is C1TrueDBGrid c1Grid))
            {
                return;
            }

            c1Grid.HandleCreated -= C1Grid_HandleCreated;
            ShowCellTipAfterGridReady(c1Grid);
        }

        private static void C1Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is C1TrueDBGrid c1Grid))
            {
                return;
            }

            ShowCellTipAtPoint(c1Grid, e.Location);
        }

        private static void C1Grid_MouseLeave(object sender, EventArgs e)
        {
            if (!(sender is C1TrueDBGrid c1Grid))
            {
                return;
            }

            HideCellTip(c1Grid);
        }

        private static void C1Grid_Disposed(object sender, EventArgs e)
        {
            if (!(sender is C1TrueDBGrid c1Grid))
            {
                return;
            }

            Clear(c1Grid);
        }

        private static void ShowCellTipAtPoint(C1TrueDBGrid c1Grid, Point mousePoint)
        {
            if (!_contexts.TryGetValue(c1Grid, out var context))
            {
                return;
            }

            if (context.ColumnInfoCollector == null || context.ToolTip == null)
            {
                HideCellTip(c1Grid);
                return;
            }

            if (!TryGetHeadingColumnName(c1Grid, mousePoint, out var columnName))
            {
                HideCellTip(c1Grid);
                return;
            }

            if (!context.ColumnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                HideCellTip(c1Grid);
                return;
            }

            var options = context.Options ?? new GridHeadingCellTipOptions();

            if (!ShouldShowCellTip(columnInfo, options))
            {
                HideCellTip(c1Grid);
                return;
            }

            var cellTipText = BuildCellTipText(columnInfo, options);

            if (string.IsNullOrWhiteSpace(cellTipText))
            {
                HideCellTip(c1Grid);
                return;
            }

            if (string.Equals(context.LastColumnName, columnName, StringComparison.Ordinal) && string.Equals(context.LastCellTipText, cellTipText, StringComparison.Ordinal))
            {
                return;
            }

            context.LastColumnName = columnName;
            context.LastCellTipText = cellTipText;
            context.ToolTip.Show(cellTipText, c1Grid, mousePoint.X + ToolTipOffsetX, mousePoint.Y + ToolTipOffsetY, ToolTipDuration);
        }

        private static void HideCellTip(C1TrueDBGrid c1Grid)
        {
            if (!_contexts.TryGetValue(c1Grid, out var context))
            {
                return;
            }

            if (context.ToolTip != null)
            {
                context.ToolTip.Hide(c1Grid);
            }

            context.LastColumnName = null;
            context.LastCellTipText = null;
        }

        private static bool TryGetHeadingColumnName(C1TrueDBGrid c1Grid, Point mousePoint, out string columnName)
        {
            columnName = string.Empty;

            if (!IsHeadingArea(c1Grid, mousePoint))
            {
                return false;
            }

            var colIndex = c1Grid.ColContaining(mousePoint.X);

            if (colIndex < 0)
            {
                return false;
            }

            if (TryGetColumnNameFromDisplayColumn(c1Grid, colIndex, out columnName))
            {
                return true;
            }

            if (TryGetColumnNameFromDataColumn(c1Grid, colIndex, out columnName))
            {
                return true;
            }

            return false;
        }

        private static bool IsHeadingArea(C1TrueDBGrid c1Grid, Point mousePoint)
        {
            var rowIndex = c1Grid.RowContaining(mousePoint.Y);

            return rowIndex < 0;
        }

        private static bool TryGetColumnNameFromDisplayColumn(C1TrueDBGrid c1Grid, int colIndex, out string columnName)
        {
            columnName = string.Empty;

            try
            {
                if (c1Grid.Splits.Count == 0)
                {
                    return false;
                }

                var displayColumns = c1Grid.Splits[0].DisplayColumns;

                if (colIndex < 0 || colIndex >= displayColumns.Count)
                {
                    return false;
                }

                var displayColumn = displayColumns[colIndex];

                if (displayColumn == null || displayColumn.DataColumn == null)
                {
                    return false;
                }

                columnName = displayColumn.DataColumn.DataField;

                return !string.IsNullOrWhiteSpace(columnName);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetColumnNameFromDataColumn(C1TrueDBGrid c1Grid, int colIndex, out string columnName)
        {
            columnName = string.Empty;

            if (colIndex < 0 || colIndex >= c1Grid.Columns.Count)
            {
                return false;
            }

            var dataColumn = c1Grid.Columns[colIndex];

            if (dataColumn == null)
            {
                return false;
            }

            columnName = dataColumn.DataField;

            return !string.IsNullOrWhiteSpace(columnName);
        }

        private static bool ShouldShowCellTip(ColumnInfo columnInfo, GridHeadingCellTipOptions options)
        {
            if (columnInfo == null)
            {
                return false;
            }

            options = options ?? new GridHeadingCellTipOptions();

            if (options.ShowPrimaryKey && columnInfo.IsPrimaryKey)
            {
                return true;
            }

            if (options.ShowNotNull && !columnInfo.IsNullable)
            {
                return true;
            }

            if (options.ShowDataType && !string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
            {
                return true;
            }

            if (options.ShowComment && !string.IsNullOrWhiteSpace(columnInfo.ColumnComment))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(BuildSourceName(columnInfo)))
            {
                return true;
            }

            return false;
        }

        private static string BuildCellTipText(ColumnInfo columnInfo, GridHeadingCellTipOptions options)
        {
            var sb = new StringBuilder();

            options = options ?? new GridHeadingCellTipOptions();

            if (!string.IsNullOrWhiteSpace(columnInfo.ColumnName))
            {
                sb.AppendLine("Column: " + columnInfo.ColumnName);
            }

            var sourceName = BuildSourceName(columnInfo);

            if (!string.IsNullOrWhiteSpace(sourceName))
            {
                sb.AppendLine("Source: " + sourceName);
            }

            if (options.ShowDataType && !string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
            {
                sb.AppendLine("Type: " + columnInfo.BaseDataType);
            }

            if (options.ShowComment && !string.IsNullOrWhiteSpace(columnInfo.ColumnComment))
            {
                sb.AppendLine("Comment: " + columnInfo.ColumnComment);
            }

            if (options.ShowPrimaryKey && columnInfo.IsPrimaryKey)
            {
                sb.AppendLine("Primary Key");
            }

            if (options.ShowNotNull && !columnInfo.IsNullable)
            {
                sb.AppendLine("Not Null");
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildSourceName(ColumnInfo columnInfo)
        {
            var baseSchemaName = columnInfo.BaseSchemaName?.Trim() ?? string.Empty;
            var baseTableName = columnInfo.BaseTableName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseTableName))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(baseSchemaName))
            {
                return baseTableName;
            }

            return $"{baseSchemaName}.{baseTableName}";
        }
    }
}