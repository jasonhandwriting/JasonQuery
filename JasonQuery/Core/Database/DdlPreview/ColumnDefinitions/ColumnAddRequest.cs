using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnAddRequest
    {
        public DataSourceType DataSourceType { get; set; }

        public string SchemaDatabase { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public ColumnDefinition Column { get; set; }
    }
}
