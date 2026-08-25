using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Providers.PostgreSql.Enums;
using System.Collections.Generic;

namespace JasonLibrary.Providers.PostgreSql.Mapping
{
    public class PostgreSqlColumnTypeResolver
    {
        public static PostgreSqlColumnTypeMapping Resolve(int providerType, int columnSize, int numericPrecision, int numericScale)
        {
            if (!PostgreSqlColumnTypeTypeRegistry.TryGetValue(providerType, out var template))
            {
                return new PostgreSqlColumnTypeMapping
                {
                    SimpleDataType = "string",
                    BaseDataType = "unknown",
                    FullDataType = "unknown",
                    UsedProviderFallback = true,
                    SpecialDataTypeKind = SpecialDataTypeKind.String,
                    CategoryDataTypeKind = CategoryDataTypeKind.String,
                    IsUpdateValueSupported = false,
                    UpdateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType
                };
            }

            //Clone
            var result = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = template.SimpleDataType,
                BaseDataType = template.BaseDataType,
                FullDataType = template.FullDataType,
                UsedProviderFallback = template.UsedProviderFallback,
                IsArray = template.IsArray,
                Mode = template.Mode,
                SpecialDataTypeKind = template.SpecialDataTypeKind,
                CategoryDataTypeKind = template.CategoryDataTypeKind,
                IsUpdateValueSupported = template.IsUpdateValueSupported,
                UpdateValueSupportKind = template.UpdateValueSupportKind
            };

            switch (template.Mode)
            {
                case PostgreSqlResolutionMode.ResolutionMode.WithColumnSize:
                case PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize:
                case PostgreSqlResolutionMode.ResolutionMode.WithSize:
                    {
                        result.FullDataType = template.SizeFormatter?.Invoke(template.BaseDataType, columnSize, numericPrecision, numericScale)
                                              ?? template.BaseDataType;
                        break;
                    }
                case PostgreSqlResolutionMode.ResolutionMode.Catalog:
                    {
                        result.BaseDataType = "unknown";
                        result.FullDataType = "unknown";
                        break;
                    }
                case PostgreSqlResolutionMode.ResolutionMode.Direct:
                    {
                        result.FullDataType = result.BaseDataType;
                        break;
                    }
            }

            return result;
        }

        private static readonly Dictionary<int, PostgreSqlColumnTypeMapping> PostgreSqlColumnTypeTypeRegistry = new Dictionary<int, PostgreSqlColumnTypeMapping>()
        {
            //Catalog 1：Direct mapping
            [16] = new PostgreSqlColumnTypeMapping()
            {
                BaseDataType = "boolean",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                SpecialDataTypeKind = SpecialDataTypeKind.Boolean,
                IsUpdateValueSupported = true, //20260503 boolean 視為文字，填入的值可能為 false/true
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1000] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "boolean[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                SpecialDataTypeKind = SpecialDataTypeKind.BooleanArray,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [17] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bytea",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeBinary,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType
            },
            [1001] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bytea[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [20] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "bigint",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1016] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number", //20260220 Refer to pgAdmin settings for numerical values
                BaseDataType = "bigint[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [21] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "smallint",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1005] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "smallint[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [25] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "text",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1009] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "text[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [700] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "real",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1021] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "real[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [701] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "double precision",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1022] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "double precision[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1002] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "\"char\"[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                SpecialDataTypeKind = SpecialDataTypeKind.CharArray,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1007] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "integer[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1034] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "aclitem[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1012] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "cid[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1028] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "oid[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1082] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "datetime",
                BaseDataType = "date",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                SpecialDataTypeKind = SpecialDataTypeKind.Date,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1182] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "date[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2950] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "uuid",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [2951] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "uuid[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [142] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "xml",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType
            },
            [143] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "xml[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1011] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "xid[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [271] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "xid8[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [603] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "box",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1020] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "box[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                SpecialDataTypeKind = SpecialDataTypeKind.BoxArray,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [650] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "cidr",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [651] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "cidr[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [718] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "circle",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 幾何與路徑類 (長度不可控，可能極長)
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [719] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "circle[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3912] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "daterange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3913] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "daterange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4535] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "datemultirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6155] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "datemultirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3644] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "gtsvector[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [869] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "inet",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260503 inet 視為文字，填入的值可能為 192.168.1.1/24
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1041] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "inet[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [22] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int2vector",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //資料庫系統內部運作或開發高階預存程序在用的，一般使用者不應該在 Grid 上去手動修改這些欄位
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1006] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int2vector[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4451] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int4multirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6150] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int4multirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4536] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int8multirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6157] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int8multirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3904] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int4range",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3905] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int4range[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3926] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int8range",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3927] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "int8range[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [114] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "json",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 視為大型文字，因為 json 可能包含多行的長內容
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType
            },
            [199] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "json[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3802] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "jsonb",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 視為大型文字，因為 jsonb 可能包含多行的長內容
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType
            },
            [3807] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "jsonb[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4073] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "jsonpath[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [628] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "line",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [629] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "line[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [601] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "lseg",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1018] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "lseg[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [829] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "macaddr",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1040] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "macaddr[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [774] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "macaddr8",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [775] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "macaddr8[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [790] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "money",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [791] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "money[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1003] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "name[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4532] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "nummultirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6151] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "nummultirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3906] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "numrange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3907] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "numrange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [10001] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "oidvector",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //資料庫系統內部運作或開發高階預存程序在用的，一般使用者不應該在 Grid 上去手動修改這些欄位
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [1013] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "oidvector[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3221] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "pg_lsn[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [602] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "path",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 幾何與路徑類 (長度不可控，可能極長)
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [1019] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "path[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [5039] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "pg_snapshot[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [600] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "point",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1017] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "point[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [604] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "polygon",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 幾何與路徑類 (長度不可控，可能極長)
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [1027] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "polygon[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1790] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "refcursor",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [2201] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "refcursor[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2210] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regclass[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4192] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regcollation[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3735] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regconfig[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3770] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regdictionary[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4090] = new PostgreSqlColumnTypeMapping
            {

                BaseDataType = "regnamespace[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2208] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regoper[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2209] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regoperator[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1008] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regproc[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2207] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regprocedure[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4097] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regrole[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2211] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "regtype[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1010] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tid[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3645] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsquery[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3908] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsrange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3909] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsrange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3910] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tstzrange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [3911] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tstzrange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4534] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tstzmultirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6153] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tstzmultirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [3643] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsvector[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [2949] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "txid_snapshot[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [4533] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsmultirange",
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText, //20260715 雖然可以用字串寫入，但語法極度嚴格，容易出現 PostgreSQL 直接拋出語法錯誤的狀況
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated
            },
            [6152] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "tsmultirange[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },
            [1015] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "character varying[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Direct,
                SpecialDataTypeKind = SpecialDataTypeKind.CharacterArray,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType
            },

            //Catalog 2：WithColumnSize - 帶入 Column 長度，例如 bit(n); 如果 n=0, 則最終結果為 bit
            [1042] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "character",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },
            [1014] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "character[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                SpecialDataTypeKind = SpecialDataTypeKind.CharacterArray,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },
            [1560] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bit", //pgAdmin 視為文字，靠左顯示
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                SpecialDataTypeKind = SpecialDataTypeKind.Bit,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },
            [1561] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bit[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                SpecialDataTypeKind = SpecialDataTypeKind.Array,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },
            [1562] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bit varying",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                SpecialDataTypeKind = SpecialDataTypeKind.Bit,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },
            [1563] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "bit varying[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithColumnSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                SpecialDataTypeKind = SpecialDataTypeKind.Array,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({size})" : baseName
            },

            //Catalog 2：WithPrecisionSize - 帶入 Precision 長度，例如 interval(n); 如果 n=0, 則最終結果為 interval
            [1186] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "interval",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                IsUpdateValueSupported = true, //20260715 短文字，允許編輯
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({prec})" : baseName
            },
            [1187] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "interval[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => size > 0 ? $"{baseName}({prec})" : baseName
            },
            [1266] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "time with time zone",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                SpecialDataTypeKind = SpecialDataTypeKind.TimeWithTimeZone,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1270] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "time with time zone[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1083] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "time without time zone",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                SpecialDataTypeKind = SpecialDataTypeKind.TimeWithoutTimeZone,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1183] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "time without time zone[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1184] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "timestamp with time zone",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                SpecialDataTypeKind = SpecialDataTypeKind.TimestampWithTimeZone,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1185] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "timestamp with time zone[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1114] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "timestamp without time zone",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                SpecialDataTypeKind = SpecialDataTypeKind.TimestampWithoutTimeZone,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },
            [1115] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "timestamp without time zone[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithPrecisionSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => prec > 0 ? $"{baseName}({prec})" : baseName
            },

            //Catalog 2：WithSize - 帶入長度，例如 numeric(10,2)
            [1700] = new PostgreSqlColumnTypeMapping
            {
                SimpleDataType = "number",
                BaseDataType = "numeric",
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithSize,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported,
                SizeFormatter = (baseName, size, prec, scale) => scale > 0 ? $"{baseName}({prec},{scale})" : $"{baseName}({prec})"
            },
            [1231] = new PostgreSqlColumnTypeMapping
            {
                BaseDataType = "numeric[]",
                IsArray = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.WithSize,
                CategoryDataTypeKind = CategoryDataTypeKind.LargeText,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.ArrayType,
                SizeFormatter = (baseName, size, prec, scale) => scale > 0 ? $"{baseName}({prec},{scale})" : $"{baseName}({prec})"
            },

            //Catalog 3：Need to search from catalog
            [23] = new PostgreSqlColumnTypeMapping //integer, oid (serial=>integer)
            {
                SimpleDataType = "number",
                UsedProviderFallback = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Catalog,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.Supported
            },
            [1043] = new PostgreSqlColumnTypeMapping //string array, xid, xid8, cid, gtsvector, jsonpath, name, pg_lsn, pg_dependencies, pg_brin_minmax_multi_summary, pg_brin_bloom_summary, pg_mcv_list, pg_ndistinct, pg_node_tree, pg_snapshot, regclass, regcollation, regconfig, regdictionary, regnamespace, regoper, regoperator, regproc, regprocedure, regrole, regtype, tid, tsquery, tsvector, txid_snapshot
            {
                UsedProviderFallback = true,
                Mode = PostgreSqlResolutionMode.ResolutionMode.Catalog,
                IsUpdateValueSupported = true,
                UpdateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType
            }
        };
    }
}
