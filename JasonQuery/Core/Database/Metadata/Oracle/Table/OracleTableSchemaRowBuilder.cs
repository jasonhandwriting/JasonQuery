using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Table
{
    internal static class OracleTableSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string tableSchemaType,
                                    string tableSchemaName, string columnInfo)
        {
            if (targetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(targetSchemaTable));
            }

            var row = targetSchemaTable.NewRow();

            row["SchemaObject"] = connectionName;
            row["SchemaType"] = tableSchemaType;
            row["SchemaName"] = tableSchemaName;
            row["ColumnInfo"] = columnInfo;

            return row;
        }
    }
}
