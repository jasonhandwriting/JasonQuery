using JasonLibrary.Core.Schema;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private readonly HashSet<Control> _gridDataColumnEditors = new HashSet<Control>();

        /// <summary>
        /// RawSchema 與 Grid 欄位順序不一致時，依 DataField 重新定位，避免直接以索引存取造成例外。
        /// </summary>
        private bool TryAlignGridDataColumnIndex(ref int columnIndex, string columnName)
        {
            if (columnIndex >= 0 && columnIndex < c1GridData.Columns.Count
                && string.Equals(c1GridData.Columns[columnIndex].DataField, columnName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            for (var candidateColumnIndex = 0; candidateColumnIndex < c1GridData.Columns.Count; candidateColumnIndex++)
            {
                if (!string.Equals(c1GridData.Columns[candidateColumnIndex].DataField, columnName, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                columnIndex = candidateColumnIndex;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 嘗試取得指定 Grid 欄位的正規化欄位資訊。
        /// 將 Collector 的 null 檢查與 out 參數初始化集中處理，避免各資料庫重複撰寫。
        /// </summary>
        private bool TryGetGridDataColumnInfo(string columnName, out ColumnInfo columnInfo)
        {
            columnInfo = null;

            return _columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out columnInfo);
        }

        /// <summary>
        /// 在重新設定欄位 Editor 前，先解除 Grid 參考並釋放上一批 Editor。
        /// </summary>
        private void DisposeGridDataColumnEditors()
        {
            foreach (C1.Win.C1TrueDBGrid.C1DataColumn column in c1GridData.Columns)
            {
                var editor = column.Editor as Control;

                if (editor != null && _gridDataColumnEditors.Contains(editor))
                {
                    column.Editor = null;
                }
            }

            foreach (var editor in _gridDataColumnEditors)
            {
                UnregisterGridDataColumnEditorEvents(editor);
                editor.Dispose();
            }

            _gridDataColumnEditors.Clear();
            _gridColumnEditorRegistry.Clear();
        }


        /// <summary>
        /// 依統一的欄位更新政策設定 Grid 欄位鎖定狀態。
        /// LargeText 即使資料庫 metadata 標記為可更新，也一律維持唯讀。
        /// </summary>
        private void ApplyGridDataColumnEditPolicy()
        {
            if (c1GridData.Splits.Count == 0)
            {
                return;
            }

            foreach (C1.Win.C1TrueDBGrid.C1DisplayColumn displayColumn in c1GridData.Splits[0].DisplayColumns)
            {
                var columnName = displayColumn.DataColumn?.DataField ?? string.Empty;
                var isEditable = TryGetGridDataColumnInfo(columnName, out var columnInfo)
                                 && IsTableDataColumnEditable(columnInfo);

                displayColumn.Locked = !isEditable;
            }
        }
    }
}
