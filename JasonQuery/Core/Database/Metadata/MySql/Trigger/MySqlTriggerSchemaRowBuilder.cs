using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Formatters;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Trigger
{
    internal static class MySqlTriggerSchemaRowBuilder
    {
        public static DataRow BuildRow(MySqlTriggerSchemaBuildContext context, DataRow triggerRow)
        {
            if (context?.SchemaTable == null || triggerRow == null)
            {
                return null;
            }

            var triggerName = triggerRow.GetSafeString("TriggerName");
            var schemaType = $"Triggers{MyGlobal.Separator}({DataSizeFormatter.FormatInt(context.TriggerRows.Count)})";
            var row = context.SchemaTable.NewRow();

            row["SchemaObject"] = context.ConnectionName;
            row["SchemaNode"] = context.DatabaseName;
            row["SchemaType"] = schemaType;

            if (context.SchemaTable.Columns.Contains("Schema_Browser"))
            {
                row["Schema_Browser"] = triggerName;
            }
            else
            {
                row["SchemaName"] = triggerName;
            }

            if (context.SchemaTable.Columns.Contains("CreateDate") && !triggerRow.IsNull("Create_Date"))
            {
                row["CreateDate"] = triggerRow["Create_Date"];
            }

            return row;
        }
    }
}