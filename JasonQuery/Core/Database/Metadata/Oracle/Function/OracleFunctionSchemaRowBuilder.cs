using JasonQuery.Core.Database.Metadata.Oracle.Common;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Function
{
    internal static class OracleFunctionSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string functionSchemaType, string functionName)
        {
            return OracleSimpleSchemaRowBuilder.Build(targetSchemaTable, connectionName, functionSchemaType, functionName);
        }
    }
}
