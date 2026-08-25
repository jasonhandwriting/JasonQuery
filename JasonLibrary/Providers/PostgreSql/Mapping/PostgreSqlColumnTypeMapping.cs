using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Providers.PostgreSql.Enums;
using System;

namespace JasonLibrary.Providers.PostgreSql.Mapping
{
    public class PostgreSqlColumnTypeMapping
    {
        public string SimpleDataType { get; set; } = "string";
        public string BaseDataType { get; set; } = string.Empty;
        public string FullDataType { get; set; } = string.Empty;
        public bool UsedProviderFallback { get; set; } = false; //is Catalog 3?
        public bool IsArray { get; set; } = false; //ex. char[]
        public PostgreSqlResolutionMode.ResolutionMode Mode { get; set; }

        /// <summary>
        /// 20260218 定義如何格式化長度的委派
        /// 1: string：BaseDataType
        /// 2: int：ColumnSize
        /// 3: int：NumericPrecision
        /// 4: int：NumericScale
        /// 5: string：Return Value, ex. "numeric(10,2)"
        /// </summary>
        public Func<string, int, int, int, string> SizeFormatter { get; set; }
        public SpecialDataTypeKind SpecialDataTypeKind { get; set; } = SpecialDataTypeKind.String;
        public CategoryDataTypeKind CategoryDataTypeKind { get; set; } = CategoryDataTypeKind.String;

        public bool IsUpdateValueSupported { get; set; } = false; //型別層級：是否支援產生 UPDATE 值

        public ColumnUpdateValueSupportKind UpdateValueSupportKind { get; set; }
               = ColumnUpdateValueSupportKind.UnknownDataType;
    }
}
