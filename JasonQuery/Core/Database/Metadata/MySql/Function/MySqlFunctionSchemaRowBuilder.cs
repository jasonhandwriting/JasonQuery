using JasonQuery.Core.Config;
using JasonQuery.Core.Data.Formatters;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Function
{
    internal static class MySqlFunctionSchemaRowBuilder
    {
        public static DataRow BuildRow(MySqlFunctionSchemaBuildContext context, string functionName)
        {
            if (context?.SchemaTable == null)
            {
                return null;
            }

            var schemaType = $"Functions{MyGlobal.Separator}({DataSizeFormatter.FormatInt(context.FunctionRows.Count)})";
            var row = context.SchemaTable.NewRow();

            row["SchemaObject"] = context.ConnectionName;
            row["SchemaNode"] = context.DatabaseName;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = functionName;

            return row;
        }
    }
}