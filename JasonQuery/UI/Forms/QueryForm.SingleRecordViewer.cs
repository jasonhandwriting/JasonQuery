using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.UI.Helpers;
using System.Data;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void SingleRecordViewer()
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
            var currentRecordIndex = visibleRowIndexes.IndexOf(c1Grid.Row);

            if (currentRecordIndex < 0)
            {
                return;
            }

            var currentCol = c1Grid.Col;

            DataTable LoadSingleRecord(int recordIndex)
            {
                if (recordIndex < 0 || recordIndex >= visibleRowIndexes.Count)
                {
                    return null;
                }

                var gridRowIndex = visibleRowIndexes[recordIndex];

                return GetDataRowForSingleRecordViewer(c1Grid, gridRowIndex);
            }

            var initialData = LoadSingleRecord(currentRecordIndex);

            if (initialData == null)
            {
                return;
            }

            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SingleRecordViewerFormWidth", "SingleRecordViewerFormHeight", defaultWidth: 360, defaultHeight: 520);

            using (var form = new SingleRecordViewerForm())
            {
                form.Initialize(initialData, visibleRowIndexes.Count, currentRecordIndex, currentCol);
                form.RecordLoader = LoadSingleRecord;

                form.CurrentRecordChanged = (recordIndex, sourceColumnIndex) =>
                {
                    if (recordIndex < 0 || recordIndex >= visibleRowIndexes.Count)
                    {
                        return;
                    }

                    var gridRowIndex = visibleRowIndexes[recordIndex];

                    c1Grid.Row = gridRowIndex;
                    c1Grid.Col = sourceColumnIndex;
                    c1Grid.Select();
                };

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                form.ShowDialog();
            }
        }

        private DataTable GetDataRowForSingleRecordViewer(C1TrueDBGrid c1Grid, int currentRow)
        {
            var dtNew = new DataTable();

            dtNew.Columns.Add("ColumnName");
            dtNew.Columns.Add("Value");
            dtNew.Columns.Add("Blob", typeof(byte[]));

            var vr = c1Grid.Splits[_splitsIndex].Rows[currentRow];

            foreach (C1DataColumn column in c1Grid.Columns)
            {
                var columnName = column.Caption;
                var text = column.CellText(vr.DataRowIndex);
                var row = dtNew.NewRow();
                string[] splitters = { "\r\n", "\r", "\n" };
                var parts = columnName.Split(splitters, 2, System.StringSplitOptions.None);

                columnName = parts[0];
                row["ColumnName"] = columnName;
                row["Value"] = text;

                var dataField = column.DataField;

                if (!string.IsNullOrWhiteSpace(dataField))
                {
                    object rawValue = c1Grid[currentRow, dataField];

                    if (rawValue is LargeBinaryDataType binary && !binary.IsNull)
                    {
                        row["Blob"] = binary.LoadContent();
                    }
                    else if (rawValue is byte[] bytes)
                    {
                        row["Blob"] = bytes;
                    }
                }

                dtNew.Rows.Add(row);
            }

            return dtNew;
        }
    }
}
