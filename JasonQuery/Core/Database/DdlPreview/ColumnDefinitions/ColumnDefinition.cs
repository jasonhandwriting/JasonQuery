namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnDefinition
    {
        public string ColumnName { get; set; }

        public string TypeKey { get; set; }

        public string CustomTypeText { get; set; }

        public string Parameter1 { get; set; }

        public string Parameter2 { get; set; }

        public OracleLengthSemantics OracleLengthSemantics { get; set; }

        public bool NullAllowed { get; set; }

        public ColumnDefaultValueKind DefaultValueKind { get; set; }

        public string DefaultValue { get; set; }
    }
}
