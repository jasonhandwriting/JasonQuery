using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal sealed class MySqlTableMetadataLoadResult
    {
        public DataTable TableInfo { get; set; } = new DataTable();

        public DataTable TableColumnInfo { get; set; } = new DataTable();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> TableColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

        public Dictionary<(string SchemaName, string TableName), long> RowCountMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), long>();
    }
}