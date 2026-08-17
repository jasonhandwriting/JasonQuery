using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Function
{
    internal sealed class MySqlFunctionSchemaBuildContext
    {
        public DataTable SchemaTable { get; set; }

        public string ConnectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public List<DataRow> FunctionRows { get; set; } = new List<DataRow>();

        public bool AddSchemaRow { get; set; }

        public bool AutoCompleteFunction { get; set; }
    }
}