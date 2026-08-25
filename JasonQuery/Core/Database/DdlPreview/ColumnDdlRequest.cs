using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal sealed class ColumnDdlRequest
    {
        public DataSourceType DataSourceType { get; set; }

        public ColumnDdlOperation Operation { get; set; }

        public string SchemaDatabase { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string NewColumnName { get; set; }

        public string ColumnType { get; set; }

        public string Comment { get; set; }

        public bool HasExistingSqlServerComment { get; set; }

        public ColumnDefinition ColumnDefinition { get; set; }
    }
}
