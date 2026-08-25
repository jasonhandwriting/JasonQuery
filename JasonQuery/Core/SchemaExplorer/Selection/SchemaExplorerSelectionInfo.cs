using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public sealed class SchemaExplorerSelectionInfo
    {
        public SchemaExplorerSelectionKind Kind { get; set; } = SchemaExplorerSelectionKind.None;

        public DataSourceType SourceType { get; set; }

        public int DisplayRowIndex { get; set; } = -1;

        public int GroupLevel { get; set; } = -1;

        public bool IsGroupRow { get; set; }

        public bool IsColumnInfoRow { get; set; }

        public bool CanDisplayObject
        {
            get
            {
                return Kind == SchemaExplorerSelectionKind.SchemaObject || Kind == SchemaExplorerSelectionKind.ExternalObject;
            }
        }

        public string SchemaNode { get; set; } = string.Empty;

        public string SchemaType { get; set; } = string.Empty;

        public string SchemaName { get; set; } = string.Empty;

        public string SchemaDbo { get; set; } = string.Empty;

        public string ObjectId { get; set; } = string.Empty;

        public string CreateDate { get; set; } = string.Empty;

        public string ModifyDate { get; set; } = string.Empty;

        public string PackageSpecBody { get; set; } = string.Empty;
    }
}
