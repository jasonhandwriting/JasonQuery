using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Function
{
    internal sealed class PostgreSqlFunctionSchemaBuildContext
    {
        public DataTable SourceFunctionSchemaInfo { get; set; }

        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; }

        public Dictionary<string, int> FunctionCountMap { get; set; }

        public bool AddSchemaRow { get; set; }

        public int DoEventsInterval { get; set; } = 500;
    }
}
