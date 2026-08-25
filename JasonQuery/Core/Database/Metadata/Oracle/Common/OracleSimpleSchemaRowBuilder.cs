using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Common
{
    internal static class OracleSimpleSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string schemaType, string schemaName)
        {
            if (targetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(targetSchemaTable));
            }

            var row = targetSchemaTable.NewRow();

            row["SchemaObject"] = connectionName;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = schemaName;

            return row;
        }
    }
}
