using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Formatters;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal static class MySqlTableSchemaRowBuilder
    {
        public static List<DataRow> BuildRows(MySqlTableSchemaBuildContext context, DataRow tableRow, long rowCount)
        {
            var rows = new List<DataRow>();

            if (context?.SchemaTable == null || tableRow == null)
            {
                return rows;
            }

            var tableName = tableRow.GetSafeString("TableName");
            var key = (SchemaName: context.DatabaseName, TableName: tableName);

            if (!context.TableColumnInfoMap.TryGetValue(key, out var cols))
            {
                cols = new List<DataRow>();
            }

            foreach (var col in cols)
            {
                var row = context.SchemaTable.NewRow();

                row["SchemaObject"] = context.ConnectionName;
                row["SchemaNode"] = context.DatabaseName;
                row["SchemaType"] = $"Tables{MyGlobal.Separator}({DataSizeFormatter.FormatInt(context.TableRows.Count)})";
                row["SchemaName"] = $"{tableName}{MyGlobal.Separator}({DataSizeFormatter.FormatInt(rowCount)})";

                if (context.SchemaTable.Columns.Contains("CreateDate") && !tableRow.IsNull("Create_Date"))
                {
                    row["CreateDate"] = tableRow["Create_Date"];
                }

                if (context.SchemaTable.Columns.Contains("ModifyDate") && !tableRow.IsNull("Modify_Date"))
                {
                    row["ModifyDate"] = tableRow["Modify_Date"];
                }

                var columnInfo = col.GetSafeString("ColumnName");
                var columnType = col.GetSafeString("ColumnType");

                if (context.SchemaTable.Columns.Contains("ColumnInfo"))
                {
                    row["ColumnInfo"] = $"{columnInfo}, {columnType}";
                }
                else
                {
                    row["SchemaName"] = $"{columnInfo}, {columnType}";
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}
