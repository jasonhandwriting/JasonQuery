using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.QueryEditor
{
    internal sealed class MySqlQueryEditorMetadataSnapshot
    {
        public DataTable FunctionInfo { get; set; } = new DataTable();

        public DataTable ProcedureInfo { get; set; } = new DataTable();

        public DataTable TableInfo { get; set; } = new DataTable();

        public DataTable TriggerInfo { get; set; } = new DataTable();

        public DataTable ViewInfo { get; set; } = new DataTable();

        public DataTable TableColumnInfo { get; set; } = new DataTable();

        public DataTable ViewColumnInfo { get; set; } = new DataTable();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> TableColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

        public Dictionary<(string SchemaName, string TableName), List<DataRow>> ViewColumnInfoMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

        public Dictionary<(string SchemaName, string TableName), long> RowCountMap { get; set; }
               = new Dictionary<(string SchemaName, string TableName), long>();
    }
}