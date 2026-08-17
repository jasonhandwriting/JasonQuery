using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal sealed class MySqlTableSchemaBuildContext
    {
        public DataTable SchemaTable { get; set; }

        public string ConnectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public List<DataRow> TableRows { get; set; } = new List<DataRow>();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> TableColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

        public Dictionary<(string SchemaName, string TableName), long> RowCountMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), long>();

        public bool AddSchemaRow { get; set; }

        public bool AutoCompleteTable { get; set; }
    }
}