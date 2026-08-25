using JasonQuery.UI.Helpers;

namespace JasonQuery.UI.Forms
{
    public partial class SingleRecordViewerForm
    {
        private void CellViewer()
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

            if (!SingleRecordCellViewerContextBuilder.TryBuild(c1Grid, c1Grid.Row, gridColIndex, out var context))
            {
                return;
            }

            using (var form = new CellViewerForm())
            {
                form.ApplyContext(context);
                form.TotalQty = visibleRowIndexes.Count;
                form.CurrentRow = currentVisibleIndex;
                form.IsFromSingleRecordForm = true;

                form.CellValueLoader = visibleIndex =>
                {
                    if (visibleIndex < 0 || visibleIndex >= visibleRowIndexes.Count)
                    {
                        return null;
                    }

                    var gridRowIndex = visibleRowIndexes[visibleIndex];

                    return SingleRecordCellViewerContextBuilder.TryBuild(c1Grid, gridRowIndex, gridColIndex, out var nextContext) ? nextContext : null;
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
