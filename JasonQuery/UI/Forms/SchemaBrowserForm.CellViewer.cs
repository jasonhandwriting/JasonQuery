using C1.Win.C1TrueDBGrid;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private sealed class CellViewerGridHighlightState
        {
            public MarqueeEnum MarqueeStyle { get; set; }

            public List<bool> OwnerDrawStates { get; } = new List<bool>();
        }

        private void CellViewer(C1TrueDBGrid c1Grid)
        {
            if (c1Grid == null || c1Grid.Row < 0 || c1Grid.Col < 0)
            {
                return;
            }

            if (!CellViewerGridHelper.IsDataRow(c1Grid, c1Grid.Row))
            {
                return;
            }

            var visibleRowIndexes = CellViewerGridHelper.GetVisibleDataRowIndexes(c1Grid);
            var currentVisibleIndex = visibleRowIndexes.IndexOf(c1Grid.Row);

            if (currentVisibleIndex < 0)
            {
                return;
            }

            var gridColIndex = c1Grid.Col;

            if (!CellViewerContextBuilder.TryBuildFromColumnInfoCollector(c1Grid, c1Grid.Row, gridColIndex, _columnInfoCollector, out var context))
            {
                return;
            }

            var currentDataRow = GetDataRowFromGridDisplayRow(c1Grid, c1Grid.Row);
            var currentColumnName = c1Grid.Columns[gridColIndex].DataField;

            ApplyDirectBinaryChangeToViewerContext(currentDataRow, currentColumnName, context);

            var highlightState = BeginCellViewerGridHighlight(c1Grid);

            try
            {
                using (var form = new CellViewerForm())
                {
                    form.ApplyContext(context);
                    form.TotalQty = visibleRowIndexes.Count;
                    form.CurrentRow = currentVisibleIndex;

                    form.CellValueLoader = visibleIndex =>
                    {
                        if (visibleIndex < 0 || visibleIndex >= visibleRowIndexes.Count)
                        {
                            return null;
                        }

                        var gridRowIndex = visibleRowIndexes[visibleIndex];

                        if (!CellViewerContextBuilder.TryBuildFromColumnInfoCollector(c1Grid, gridRowIndex, gridColIndex, _columnInfoCollector, out var nextContext))
                        {
                            return null;
                        }

                        var nextDataRow = GetDataRowFromGridDisplayRow(c1Grid, gridRowIndex);

                        ApplyDirectBinaryChangeToViewerContext(nextDataRow, currentColumnName, nextContext);

                        return nextContext;
                    };

                    form.CurrentRowChanged = visibleIndex =>
                    {
                        if (visibleIndex < 0 || visibleIndex >= visibleRowIndexes.Count)
                        {
                            return;
                        }

                        var gridRowIndex = visibleRowIndexes[visibleIndex];

                        UpdateCellViewerGridCurrentCell(c1Grid, gridRowIndex, gridColIndex);
                    };

                    CellViewerGridHelper.ApplyCellViewerFormSize(form);
                    form.ShowDialog();
                }
            }
            finally
            {
                EndCellViewerGridHighlight(c1Grid, highlightState);
            }
        }

        private CellViewerGridHighlightState BeginCellViewerGridHighlight(C1TrueDBGrid grid)
        {
            var state = new CellViewerGridHighlightState
            {
                MarqueeStyle = grid.MarqueeStyle
            };

            var displayColumns = grid.Splits[0].DisplayColumns;

            for (var index = 0; index < displayColumns.Count; index++)
            {
                state.OwnerDrawStates.Add(displayColumns[index].OwnerDraw);
                displayColumns[index].OwnerDraw = true;
            }

            grid.MarqueeStyle = MarqueeEnum.HighlightCell;
            grid.OwnerDrawCell += CellViewerGrid_OwnerDrawCell;
            grid.Invalidate();
            grid.Refresh();

            return state;
        }

        private void EndCellViewerGridHighlight(
            C1TrueDBGrid grid,
            CellViewerGridHighlightState state)
        {
            if (grid == null || state == null)
            {
                return;
            }

            grid.OwnerDrawCell -= CellViewerGrid_OwnerDrawCell;
            grid.MarqueeStyle = state.MarqueeStyle;

            var displayColumns = grid.Splits[0].DisplayColumns;
            var restoreCount = Math.Min(displayColumns.Count, state.OwnerDrawStates.Count);

            for (var index = 0; index < restoreCount; index++)
            {
                displayColumns[index].OwnerDraw = state.OwnerDrawStates[index];
            }

            grid.Invalidate();
            grid.Refresh();
        }

        private void UpdateCellViewerGridCurrentCell(
            C1TrueDBGrid grid,
            int rowIndex,
            int columnIndex)
        {
            if (grid == null || rowIndex < 0 || columnIndex < 0)
            {
                return;
            }

            grid.Row = rowIndex;
            grid.Col = columnIndex;

            //CellViewerForm 是 Modal 視窗，背後 Grid 無法取得真正鍵盤焦點；
            //改由 OwnerDraw 顯示目前 Cell，避免呼叫 Select() 後仍看不到選取位置
            grid.Invalidate();
            grid.Refresh();
        }

        private void CellViewerGrid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
        {
            if (!(sender is C1TrueDBGrid grid))
            {
                return;
            }

            if (e.Row != grid.Row || e.Col != grid.Col)
            {
                return;
            }

            e.Style.BackColor = grid.SelectedStyle.BackColor;
            e.Style.ForeColor = grid.SelectedStyle.ForeColor;
        }
    }
}
