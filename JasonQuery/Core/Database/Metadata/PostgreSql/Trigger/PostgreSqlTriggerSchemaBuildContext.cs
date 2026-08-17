using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Trigger
{
    internal sealed class PostgreSqlTriggerSchemaBuildContext
    {
        public DataTable SourceTriggerSchemaInfo { get; set; }

        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; }

        public Dictionary<string, int> TriggerCountMap { get; set; }

        public bool AddSchemaRow { get; set; }

        public int DoEventsInterval { get; set; } = 500;
    }
}