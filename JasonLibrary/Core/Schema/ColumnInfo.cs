using JasonLibrary.Core.Schema.Enums;

namespace JasonLibrary.Core.Schema
{
    public sealed class ColumnInfo
    {
        public string ColumnName { get; set; }

        public string BaseSchemaName { get; set; } = string.Empty;

        public string BaseTableName { get; set; } = string.Empty;

        public string DataType { get; set; }

        public string FullDataType { get; set; }

        public string BaseDataType { get; set; }

        public string PrimaryOrNotNull { get; set; } = string.Empty;

        public bool UsedProviderFallback { get; set; }

        public SpecialDataTypeKind SpecialDataTypeKind { get; set; }

        public CategoryDataTypeKind CategoryDataTypeKind { get; set; }

        public bool IsUpdateValueSupported { get; set; } = false; //型別層級：是否支援產生 UPDATE 值

        public ColumnUpdateValueSupportKind UpdateValueSupportKind { get; set; } = ColumnUpdateValueSupportKind.UnknownDataType;

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsArray { get; set; }

        public int ColumnSize { get; set; }

        public int NumericPrecision { get; set; }

        public int NumericScale { get; set; }

        public string ColumnComment { get; set; }

        public string ColumnDefaultValue { get; set; }

        public int ProviderType { get; set; }

        public string ProviderSpecificDataType { get; set; }
    }
}