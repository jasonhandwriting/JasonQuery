using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Table
{
    internal sealed class PostgreSqlTableSchemaBuildContext
    {
        public DataTable SourceTableSchemaInfo { get; set; }

        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; }

        public int DoEventsInterval { get; set; } = 500;
    }
}