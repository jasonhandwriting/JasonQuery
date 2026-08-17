using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.View
{
    internal sealed class PostgreSqlViewSchemaBuildContext
    {
        public DataTable SourceViewSchemaInfo { get; set; }

        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; }

        public int DoEventsInterval { get; set; } = 500;
    }
}