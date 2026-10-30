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

        //Qualified only when the caller provides BOTH an authenticated session user and a metadata-derived owner with exact ordinal equality.
        //This is not authorization to execute DDL; B3 must validate the target again.
        //B3-B2-C2: internal authoritative identifiers from a single Oracle metadata DataRow.
        //Never parse SchemaName/ColumnInfo to identify a DDL target.
        public string OracleMetadataTableName { get; set; } = string.Empty;
        public string OracleMetadataColumnName { get; set; } = string.Empty;
        public bool HasQualifiedOracleColumnIdentity { get; set; }

        public string OracleSelectedOwner { get; set; } = string.Empty;

        public bool HasQualifiedOracleSelectedOwner { get; set; }

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
