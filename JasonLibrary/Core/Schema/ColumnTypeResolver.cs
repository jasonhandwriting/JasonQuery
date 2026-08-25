using JasonLibrary.Core.Schema.Enums;

namespace JasonLibrary.Core.Schema
{
    public sealed class ColumnTypeResolver
    {
        public string SimpleDataType { get; set; } = "string"; //only "string", "number", "datetime", nothing else.
        public string BaseDataType { get; set; } = string.Empty; //varchar, int, datetime, etc.
        public string FullDataType { get; set; } = string.Empty; //varchar(25), numeric(10,2)
        public bool UsedProviderFallback { get; set; } = false;
        public SpecialDataTypeKind SpecialDataTypeKind { get; set; } = SpecialDataTypeKind.String;
        public CategoryDataTypeKind CategoryDataTypeKind { get; set; } = CategoryDataTypeKind.String;

        public bool IsUpdateValueSupported { get; set; } = false; //型別層級：是否支援產生 UPDATE 值

        public ColumnUpdateValueSupportKind UpdateValueSupportKind { get; set; } = ColumnUpdateValueSupportKind.UnknownDataType;
    }
}
