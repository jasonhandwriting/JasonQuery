using JasonQuery.Core.Database.Metadata.Oracle.Common;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Procedure
{
    internal static class OracleProcedureSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string procedureSchemaType, string procedureName)
        {
            return OracleSimpleSchemaRowBuilder.Build(targetSchemaTable, connectionName, procedureSchemaType, procedureName);
        }
    }
}