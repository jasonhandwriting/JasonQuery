using JasonQuery.Core.Database.Metadata.Oracle.Common;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.Package
{
    internal static class OraclePackageSchemaRowBuilder
    {
        public static DataRow Build(DataTable targetSchemaTable, string connectionName, string packageSchemaType, string packageSchemaName)
        {
            return OracleSimpleSchemaRowBuilder.Build(targetSchemaTable, connectionName, packageSchemaType, packageSchemaName);
        }
    }
}
