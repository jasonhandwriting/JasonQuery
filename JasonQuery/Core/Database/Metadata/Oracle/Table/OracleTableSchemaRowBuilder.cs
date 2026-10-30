using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Table
{
    internal static class OracleTableSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string tableSchemaType, string tableSchemaName, string columnInfo, string oracleMetadataOwner = null)
        {
            return BuildWithRawIdentifiers(targetSchemaTable, connectionName, tableSchemaType, tableSchemaName, columnInfo, oracleMetadataOwner, null, null);
        }

        //B3-B2-C1: raw Oracle dictionary identifiers must never be reconstructed from ColumnInfo.
        public static DataRow BuildWithRawIdentifiers(DataTable targetSchemaTable, string connectionName, string tableSchemaType, string tableSchemaName, string columnInfo, string oracleMetadataOwner, string oracleMetadataTableName, string oracleMetadataColumnName)
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

            if (targetSchemaTable.Columns.Contains("OracleMetadataOwner"))
            {
                row["OracleMetadataOwner"] = oracleMetadataOwner ?? string.Empty;
            }

            if (targetSchemaTable.Columns.Contains("OracleMetadataTableName"))
            {
                row["OracleMetadataTableName"] = oracleMetadataTableName ?? string.Empty;
            }

            if (targetSchemaTable.Columns.Contains("OracleMetadataColumnName"))
            {
                row["OracleMetadataColumnName"] = oracleMetadataColumnName ?? string.Empty;
            }

            return row;
        }
    }
}
