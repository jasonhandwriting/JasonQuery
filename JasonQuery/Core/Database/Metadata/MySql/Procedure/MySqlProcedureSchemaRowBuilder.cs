using JasonQuery.Core.Config;
using JasonQuery.Core.Data.Formatters;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Procedure
{
    internal static class MySqlProcedureSchemaRowBuilder
    {
        public static DataRow BuildRow(MySqlProcedureSchemaBuildContext context, string procedureName)
        {
            if (context?.SchemaTable == null)
            {
                return null;
            }

            var schemaType = $"Procedures{MyGlobal.Separator}({DataSizeFormatter.FormatInt(context.ProcedureRows.Count)})";
            var row = context.SchemaTable.NewRow();

            row["SchemaObject"] = context.ConnectionName;
            row["SchemaNode"] = context.DatabaseName;
            row["SchemaType"] = schemaType;

            if (context.SchemaTable.Columns.Contains("Schema_Browser"))
            {
                row["Schema_Browser"] = procedureName;
            }
            else
            {
                row["SchemaName"] = procedureName;
            }

            return row;
        }
    }
}
