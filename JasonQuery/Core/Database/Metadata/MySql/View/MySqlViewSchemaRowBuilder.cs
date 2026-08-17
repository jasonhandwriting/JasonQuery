using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Formatters;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.View
{
    internal static class MySqlViewSchemaRowBuilder
    {
        public static List<DataRow> BuildRows(MySqlViewSchemaBuildContext context, string viewName)
        {
            var rows = new List<DataRow>();

            if (context?.SchemaTable == null)
            {
                return rows;
            }

            var key = (SchemaName: context.DatabaseName, TableName: viewName);

            if (!context.ViewColumnInfoMap.TryGetValue(key, out var cols))
            {
                cols = new List<DataRow>();
            }

            var columns = context.SchemaTable.Columns;

            foreach (var col in cols)
            {
                var row = context.SchemaTable.NewRow();

                row["SchemaObject"] = context.ConnectionName;
                row["SchemaNode"] = context.DatabaseName;
                row["SchemaType"] = $"Views{MyGlobal.Separator}({DataSizeFormatter.FormatInt(context.ViewRows.Count)})";

                if (columns.Contains("Schema_Browser"))
                {
                    row["Schema_Browser"] = viewName;
                }
                else
                {
                    row["SchemaName"] = viewName;
                }

                var columnInfo = col.GetSafeString("ColumnName");
                var columnType = col.GetSafeString("ColumnType");

                if (columns.Contains("ColumnInfo"))
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