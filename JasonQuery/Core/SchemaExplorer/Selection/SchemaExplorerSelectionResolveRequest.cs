using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public sealed class SchemaExplorerSelectionResolveRequest
    {
        public DataSourceType SourceType { get; set; }

        public int DisplayRowIndex { get; set; } = -1;

        public bool IsExternalSelection { get; set; }

        public bool IsShowColumnInfo { get; set; }

        public string CurrentSchemaNode { get; set; } = string.Empty;

        public string CurrentSchemaType { get; set; } = string.Empty;

        public string CurrentSchemaName { get; set; } = string.Empty;

        public string CurrentSchemaDbo { get; set; } = string.Empty;

        public string CurrentObjectId { get; set; } = string.Empty;
    }
}
