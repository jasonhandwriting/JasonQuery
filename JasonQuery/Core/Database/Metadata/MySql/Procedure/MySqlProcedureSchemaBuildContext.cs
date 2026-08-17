using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Procedure
{
    internal sealed class MySqlProcedureSchemaBuildContext
    {
        public DataTable SchemaTable { get; set; }

        public string ConnectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public List<DataRow> ProcedureRows { get; set; } = new List<DataRow>();

        public bool AddSchemaRow { get; set; }
    }
}