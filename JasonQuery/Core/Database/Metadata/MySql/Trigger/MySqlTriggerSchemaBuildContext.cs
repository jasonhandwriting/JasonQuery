using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Trigger
{
    internal sealed class MySqlTriggerSchemaBuildContext
    {
        public DataTable SchemaTable { get; set; }

        public string ConnectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public List<DataRow> TriggerRows { get; set; } = new List<DataRow>();

        public bool AddSchemaRow { get; set; }

        public bool AutoCompleteTrigger { get; set; }
    }
}