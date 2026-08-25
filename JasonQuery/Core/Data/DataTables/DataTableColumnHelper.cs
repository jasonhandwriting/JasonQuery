using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Data.DataTables
{
    internal static class DataTableColumnHelper
    {
        public static void SafeAddColumn(DataTable dtTable, string sColumnName)
        {
            if (!dtTable.Columns.Contains(sColumnName))
            {
                dtTable.Columns.Add(sColumnName);
            }
        }

        public static void SafeRemoveColumn(DataTable dtTable, string sColumnName)
        {
            if (dtTable.Columns.Contains(sColumnName))
            {
                dtTable.Columns.Remove(sColumnName);
            }
        }

        /// <summary>
        /// 如果有重複，重複出現的，欄位名稱的尾綴加上數字流水號，以免 SchemaTable 與 查詢結果(DataTable) 無法匹配
        /// (原因：當查詢結果的欄位名稱有重複的情況出現時，SchemaTable 的欄位名稱不會加數字流水號，但 DataTable 會！)
        /// </summary>
        /// <param name="dtData"></param>
        /// <returns></returns>
        public static DataTable EnsureUniqueFirstColumnNames(DataTable dtData)
        {
            if (dtData == null || dtData.Rows.Count == 0)
            {
                return dtData;
            }

            var dtResult = new DataTable();
            var columnCount = dtData.Columns.Count;

            for (var i = 0; i < columnCount; i++)
            {
                var columnName = dtData.Columns[i].ColumnName;

                dtResult.Columns.Add(columnName, typeof(string)); //欄位型態改為 string，方便後續處理
            }

            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow srcRow in dtData.Rows)
            {
                var newRow = dtResult.NewRow();

                //第一欄是 ColumnName
                var originalName = srcRow[0]?.ToString() ?? string.Empty;
                var finalName = originalName;

                if (!usedNames.Add(finalName))
                {
                    for (var i = 1; i < 100; i++)
                    {
                        var candidate = $"{originalName}{i}";

                        if (usedNames.Add(candidate))
                        {
                            finalName = candidate;
                            break;
                        }
                    }
                }

                newRow[0] = finalName;

                //其餘欄位按原樣複製
                for (var i = 1; i < dtData.Columns.Count; i++)
                {
                    newRow[i] = srcRow[i];
                }

                dtResult.Rows.Add(newRow);
            }

            return dtResult;
        }
    }
}
