using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnTypeDefinition
    {
        public DataSourceType DataSourceType { get; set; }

        public string Key { get; set; }

        public string SqlTypeName { get; set; }

        public string DisplayName { get; set; }

        public string SqlTypeSuffix { get; set; }

        public ColumnTypeArgumentKind ArgumentKind { get; set; }

        public string Parameter1Label { get; set; }

        public string Parameter2Label { get; set; }

        public string DefaultParameter1 { get; set; }

        public string DefaultParameter2 { get; set; }

        public int MinimumParameter1 { get; set; }

        public int MaximumParameter1 { get; set; }

        public int MinimumParameter2 { get; set; }

        public int MaximumParameter2 { get; set; }

        public bool Parameter1Required { get; set; }

        public bool Parameter2Required { get; set; }

        public bool AllowsMax { get; set; }

        public bool SupportsOracleLengthSemantics { get; set; }

        public bool SupportsDefault { get; set; }

        public ColumnDefaultLiteralKind DefaultLiteralKind { get; set; }

        public bool IsCustom { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DisplayName) ? SqlTypeName ?? string.Empty : DisplayName;
        }
    }
}