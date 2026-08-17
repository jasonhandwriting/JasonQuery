using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.View
{
    internal sealed class MySqlViewMetadataLoadResult
    {
        public DataTable ViewInfo { get; set; } = new DataTable();

        public DataTable ViewColumnInfo { get; set; } = new DataTable();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> ViewColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();
    }
}