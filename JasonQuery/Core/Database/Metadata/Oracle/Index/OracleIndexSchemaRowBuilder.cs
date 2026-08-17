using JasonQuery.Core.Database.Metadata.Oracle.Common;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Index
{
    internal static class OracleIndexSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string indexSchemaType,
                                    string indexSchemaName, string columnInfo)
        {
            var row = OracleSimpleSchemaRowBuilder.Build(targetSchemaTable, connectionName, indexSchemaType, indexSchemaName);

            if (targetSchemaTable.Columns.Contains("ColumnInfo"))
            {
                row["ColumnInfo"] = columnInfo;
            }

            return row;
        }
    }
}