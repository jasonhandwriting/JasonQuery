using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Text;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Data.DataTables
{
    internal static class DataTableSearchHelper
    {
        public static DataRow[] FindDataTableRows(DataTable dt, params (string columnName, string columnValue)[] conditions)
        {
            if (dt == null || conditions == null || conditions.Length == 0)
            {
                return Array.Empty<DataRow>();
            }

            return dt.AsEnumerable()
                     .Where
                      (
                          row => conditions.All
                                 (
                                     cond => string.Equals(row[cond.columnName]?.ToString(), cond.columnValue, StringComparison.OrdinalIgnoreCase)
                                 )
                      )
                     .ToArray();
        }

        public static DataRow FindDataTableFirstRow(DataTable dt, params (string columnName, string columnValue)[] conditions)
        {
            if (dt == null || conditions == null || conditions.Length == 0)
            {
                return null;
            }

            return dt.AsEnumerable()
                     .FirstOrDefault
                      (
                          row => conditions.All
                                 (
                                     cond => string.Equals(row[cond.columnName]?.ToString(), cond.columnValue, StringComparison.OrdinalIgnoreCase)
                                 )
                      );
        }

        /// <summary>
        /// 回傳符合指定條件的指定欄位的值
        /// </summary>
        /// <param name="dt">數據來源</param>
        /// <param name="resultColumnName">指定欄位名稱</param>
        /// <param name="conditions">指定條件(允許多組)</param>
        /// <returns>指定欄位的值</returns>
        public static string FindValueFromDataTable(DataTable dt, string resultColumnName, params (string columnName, string expectedValue)[] conditions)
        {
            if (dt == null || string.IsNullOrWhiteSpace(resultColumnName))
            {
                return string.Empty;
            }

            var row = FindDataTableFirstRow(dt, conditions);

            return row?[resultColumnName]?.ToString() ?? string.Empty;
        }

        public static void UpdateColumnComments(DataTable dtSchemaTable, DataTable dtColumnComments)
        {
            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                return;
            }

            if (dtColumnComments == null || dtColumnComments.Rows.Count == 0)
            {
                return;
            }

            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "Comment");

            //Create Comment Map : (Schema, Table, Column) => Comment
            var commentMap = dtColumnComments.AsEnumerable()
                .Where
                 (
                     r =>
                     !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName")) &&
                     !string.IsNullOrWhiteSpace(r.GetSafeString("TableName")) &&
                     !string.IsNullOrWhiteSpace(r.GetSafeString("ColumnName")) &&
                     !string.IsNullOrWhiteSpace(r.GetSafeString("Comments"))
                 )
                .GroupBy
                 (
                     r => (
                              Schema: r.GetSafeString("SchemaName"),
                              Table: r.GetSafeString("TableName"),
                              Column: r.GetSafeString("ColumnName")
                          )
                 )
                .ToDictionary
                 (
                     g => g.Key,
                     g => g.First().GetSafeString("Comments")
                 );

            if (commentMap.Count == 0)
            {
                return;
            }

            //套用 Comment (單次掃描 dtSchema)
            foreach (DataRow row in dtSchemaTable.Rows)
            {
                var schema = row.GetSafeString("BaseSchemaName");
                var table = row.GetSafeString("BaseTableName");
                var column = row.GetSafeString("BaseColumnName");

                if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(column))
                {
                    continue;
                }

                var key = (schema, table, column);

                if (commentMap.TryGetValue(key, out var comment))
                {
                    row["Comment"] = comment;
                }
            }
        }

        public static void UpdateColumnDefaultValues(DataTable dtSchemaTable, DataTable dtColumnDefaultValue, string nullDisplayText)
        {
            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                return;
            }

            if (dtColumnDefaultValue == null || dtColumnDefaultValue.Rows.Count == 0)
            {
                return;
            }

            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "DefaultValue");

            var nullText = nullDisplayText ?? string.Empty;

            //Create Default Value Map : ColumnName => (DefaultValue, Nullable)
            var defaultValueMap = dtColumnDefaultValue.AsEnumerable()
                .Where
                 (
                     r => !string.IsNullOrWhiteSpace(r.GetSafeString("ColumnName"))
                 )
                .GroupBy
                 (
                     r => r.GetSafeString("ColumnName").Trim()
                 )
                .ToDictionary
                 (
                     g => g.Key,
                     g =>
                     {
                         //理論上一張 Table 的 ColumnName 不應重複
                         //若 metadata 查詢意外重複，優先取有 DefaultValue 的那筆
                         var row = g.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.GetSafeString("DefaultValue"))) ?? g.First();

                         return
                         (
                             DefaultValue: row.GetSafeString("DefaultValue"),
                             Nullable: row.GetSafeString("Nullable")
                         );
                     }
                 );

            if (defaultValueMap.Count == 0)
            {
                return;
            }

            //套用 Default Value (單次掃描 dtSchemaTable)
            foreach (DataRow row in dtSchemaTable.Rows)
            {
                var column = row.GetSafeString("BaseColumnName").Trim();

                if (string.IsNullOrWhiteSpace(column))
                {
                    continue;
                }

                if (!defaultValueMap.TryGetValue(column, out var defaultInfo))
                {
                    continue;
                }

                var defaultValue = defaultInfo.DefaultValue;
                var nullable = defaultInfo.Nullable;

                //如果 dtColumnDefaultValue 的 DefaultValue 有值，直接填入 DefaultValue
                if (!string.IsNullOrWhiteSpace(defaultValue))
                {
                    //20260510 移除包住整個字串的外層成對小括號
                    defaultValue = TextHelper.RemoveEnclosingParentheses(defaultValue, trimOuterWhiteSpace: true);

                    row["DefaultValue"] = defaultValue;
                    continue;
                }

                //如果 DefaultValue 為空，且 Nullable = Y 開頭，填入 nullDisplayText
                if (nullable.StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                {
                    row["DefaultValue"] = nullText;
                    continue;
                }

                //如果 DefaultValue 為空，且 Nullable = N 開頭，填入 string.Empty
                if (nullable.StartsWith("N", StringComparison.OrdinalIgnoreCase))
                {
                    row["DefaultValue"] = string.Empty;
                    continue;
                }

                //Nullable 值異常或未知時，保守填空字串
                row["DefaultValue"] = string.Empty;
            }
        }
    }
}
