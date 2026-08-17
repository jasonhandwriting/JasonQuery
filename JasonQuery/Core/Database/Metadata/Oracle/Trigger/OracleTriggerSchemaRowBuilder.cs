using JasonQuery.Core.Database.Metadata.Oracle.Common;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Trigger
{
    internal static class OracleTriggerSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string triggerSchemaType, string triggerName)
        {
            return OracleSimpleSchemaRowBuilder.Build(targetSchemaTable, connectionName, triggerSchemaType, triggerName);
        }
    }
}