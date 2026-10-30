using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public sealed class SchemaExplorerSelectionResolveRequest
    {
        public DataSourceType SourceType { get; set; }

        public int DisplayRowIndex { get; set; } = -1;

        public bool IsExternalSelection { get; set; }

        public bool IsShowColumnInfo { get; set; }

        //Explicit opt-in. Existing legacy selection remains unchanged until the metadata loader can supply an authoritative per-object OWNER.
        public bool RequireOracleOwnerIdentity { get; set; }

        public string OracleVerifiedSessionUser { get; set; } = string.Empty;

        public string OracleMetadataOwner { get; set; } = string.Empty;

        public string CurrentSchemaNode { get; set; } = string.Empty;

        public string CurrentSchemaType { get; set; } = string.Empty;

        public string CurrentSchemaName { get; set; } = string.Empty;

        public string CurrentSchemaDbo { get; set; } = string.Empty;

        public string CurrentObjectId { get; set; } = string.Empty;
    }
}
