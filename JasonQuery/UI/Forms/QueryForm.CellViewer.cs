using JasonQuery.UI.Helpers;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void CellViewer()
        {
            var c1Grid = GetWhichGrid();

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

                    return CellViewerContextBuilder.TryBuildFromColumnInfoCollector(c1Grid, gridRowIndex, gridColIndex, _columnInfoCollector, out var nextContext) ? nextContext : null;
                };

                form.CurrentRowChanged = visibleIndex =>
                {
                    if (visibleIndex < 0 || visibleIndex >= visibleRowIndexes.Count)
                    {
                        return;
                    }

                    var gridRowIndex = visibleRowIndexes[visibleIndex];

                    c1Grid.Row = gridRowIndex;
                    c1Grid.Col = gridColIndex;
                    c1Grid.Select();
                };

                CellViewerGridHelper.ApplyCellViewerFormSize(form);
                form.ShowDialog();
            }
        }
    }
}