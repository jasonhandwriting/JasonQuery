using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.View
{
    internal sealed class MySqlViewSchemaBuildContext
    {
        public DataTable SchemaTable { get; set; }

        public string ConnectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public List<DataRow> ViewRows { get; set; } = new List<DataRow>();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> ViewColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

        public bool AddSchemaRow { get; set; }

        public bool AutoCompleteView { get; set; }
    }
}