using C1.Win.C1Input;
using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {

        private static GridNumericValueKind GetPostgreSqlNumericValueKind(string dataType, int numericPrecision)
        {
            switch (dataType)
            {
                case "smallint":
                    {
                        return GridNumericValueKind.Int16;
                    }
                case "integer":
                    {
                        return GridNumericValueKind.Int32;
                    }
                case "bigint":
                    {
                        return GridNumericValueKind.Int64;
                    }
                case "real":
                    {
                        return GridNumericValueKind.Single;
                    }
                case "double precision":
                    {
                        return GridNumericValueKind.Double;
                    }
                case "numeric":
                case "decimal":
                    {
                        return numericPrecision > 28 || numericPrecision <= 0
                               ? GridNumericValueKind.ArbitraryPrecisionDecimal
                               : GridNumericValueKind.Decimal;
                    }
                default:
                    {
                        return GridNumericValueKind.ArbitraryPrecisionDecimal;
                    }
            }
        }

        private void ConfigurePostgreSqlGridDataEditors()
        {
            var gridColumnIndex = 0;

            foreach (DataRow rawSchemaRow in _dtRawSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var columnName = rawSchemaRow.GetSafeString("ColumnName");

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase) || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    gridColumnIndex++;
                    continue;
                }

                if (!TryAlignGridDataColumnIndex(ref gridColumnIndex, columnName))
                {
                    gridColumnIndex++;
                    continue;
                }

                var hasColumnInfo = TryGetGridDataColumnInfo(columnName, out var columnInfo);
                var structuredSchemaRow = DataTableSearchHelper.FindDataTableFirstRow(_dtStructuredSchemaTable, ("ColumnName", columnName));

                //共通 Schema 資訊優先使用 ColumnInfoCollector；Default 顯示值仍沿用 StructuredSchema，避免改變既有的 DEFAULT 關鍵字與字串去引號行為
                var editorDataType = hasColumnInfo ? columnInfo.FullDataType : structuredSchemaRow.GetSafeString("DataType");
                var defaultValue = structuredSchemaRow.GetSafeString("Default");
                var isNullable = hasColumnInfo ? columnInfo.IsNullable
                                 : string.Equals(structuredSchemaRow.GetSafeString("Nullable"), "Y", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(defaultValue) && (defaultValue.StartsWith("'", StringComparison.Ordinal) || defaultValue.StartsWith("N'", StringComparison.Ordinal)))
                {
                    var unquotedDefaultValue = TextHelper.GetStringBetween(defaultValue, "'", "'");

                    if (!string.IsNullOrEmpty(unquotedDefaultValue))
                    {
                        defaultValue = unquotedDefaultValue;
                    }
                }

                var dataType = rawSchemaRow.GetSafeString("DataType");
                var providerSpecificDataType = rawSchemaRow.GetSafeString("ProviderSpecificDataType");
                var dataTypeDetail = string.Empty;
                var numericPrecision = hasColumnInfo ? columnInfo.NumericPrecision : rawSchemaRow.GetSafeInt("NumericPrecision");
                var numericScale = hasColumnInfo ? columnInfo.NumericScale : rawSchemaRow.GetSafeInt("NumericScale");
                var columnSize = hasColumnInfo ? columnInfo.ColumnSize : rawSchemaRow.GetSafeInt("ColumnSize");

                if (hasColumnInfo && IsReadOnlyLargeTextColumn(columnInfo))
                {
                    RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                  isNullable, defaultValue);

                    gridColumnIndex++;
                    continue;
                }

                if (string.IsNullOrEmpty(editorDataType))
                {
                    editorDataType = dataType;
                }

                if (editorDataType.EndsWith("(user-defined)", StringComparison.Ordinal))
                {
                    dataTypeDetail = columnName;
                    editorDataType = "user-defined";
                }
                else if (TextHelper.CheckTextStartEndWith(editorDataType, "character(", "character varying(", ")"))
                {
                    dataTypeDetail = TextHelper.GetStringBetween2(editorDataType, "(", ")", true);
                }
                else if (TextHelper.CheckTextStartEndWith(editorDataType, "character(", ")[]"))
                {
                    dataTypeDetail = TextHelper.GetStringBetween2(editorDataType, "(", ")", true);
                    editorDataType = "character[]";
                }
                else if (TextHelper.CheckTextStartEndWith(editorDataType, "character varying(", ")[]"))
                {
                    dataTypeDetail = TextHelper.GetStringBetween2(editorDataType, "(", ")", true);
                    editorDataType = "character varying[]";
                }
                else if (TextHelper.CheckTextStartEndWith(editorDataType, "bit(", "bit varying(", ")[]"))
                {
                    dataTypeDetail = TextHelper.GetStringBetween2(editorDataType, "(", ")", true);
                }

                var isLockedColumnType = CheckLockedColumn_PostgreSql(editorDataType, out _);

                if (isLockedColumnType)
                {
                    editorDataType = "LockedColumnType";
                }

                switch (editorDataType)
                {
                    case "\"char\"":
                        {
                            var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                            {
                                MaxLength = 1 //"char" 長度固定為 1
                            };

                            RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                             isNullable, defaultValue,
                                                             new TextGridEditorOptions
                                                             {
                                                                 MaxLength = 1,
                                                                 LengthMode = GridTextLengthMode.CharacterCount,
                                                                 UseCellEditorResult = true,
                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                             });
                            break;
                        }
                    case "\"char\"[]": //範例：array['a','b'] 或 '{a,b}'
                        {

                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                           isNullable, defaultValue);

                            break;
                        }
                    case "character[]":
                    case "character varying[]":
                        {
                            //這兩種型態，UPDATE 陳述式的 SET 指令範例如下，確保使用者輸入的值，前、後有加 {} 即可
                            //SET c20_character_ar = '{value11,value22,value33}'
                            //SET c02_character_varying_ar = '{"value,1234",value12,value13}'

                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                           isNullable, defaultValue);
                            break;
                        }
                    case "bytea":
                        {
                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.Binary,
                                                           isNullable, defaultValue);

                            break;
                        }
                    case "user-defined":
                        {
                            /* 以下 SQL 可以判斷該欄位是否為 enum 類型
                              SELECT Column_Name, Data_Type
                                FROM Information_Schema.Columns c JOIN pg_type t ON c.udt_name = t.typname
                               WHERE Table_Schema = 'public'
                                 AND Table_Name = 'aaa4'
                                 AND Column_Name = 'my_rating'
                                 AND t.typtype = 'e'
                             */

                            var postgreSqlReader = MyGlobal.PostgreSqlReader ?? throw new InvalidOperationException("PostgreSqlReader has not been initialized.");

                            postgreSqlReader.EnsureConnectionOpen();

                            if (MyGlobal.ShouldSavePoint)
                            {
                                var savepointSql = SqlTraceHelper.BuildHeaderNewLine("---SavePoint: Determine whether it is an enum type");

                                MyGlobal.PostgreSqlReader.SavePoint("jqtt1688ttqj", savepointSql);
                            }

                            var sbSql = new StringBuilder();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Enum Value");

                            //20240914 判斷是否為 enum 類型，是的話，取得清單
                            sbSql.AppendLine("SELECT e.EnumLabel AS Enum_Value");
                            sbSql.AppendLine("  FROM pg_type t");
                            sbSql.AppendLine("       JOIN pg_enum e ON t.oid = e.enumtypid");
                            sbSql.AppendLine("       JOIN Information_Schema.Columns c ON t.typname = c.udt_name");
                            sbSql.AppendLine($" WHERE c.Table_Schema = '{SchemaNode}'");
                            sbSql.AppendLine($"   AND c.Table_Name = '{SchemaName}'");
                            sbSql.Append($"   AND c.Column_Name = '{dataTypeDetail}';");

                            var enumQuerySql = sbSql.ToString();
                            var enumValuesTable = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(enumQuerySql, false);

                            if (enumValuesTable?.Rows.Count > 0)
                            {
                                var enumComboBox = new C1ComboBox
                                {
                                    DropDownStyle = DropDownStyle.DropDownList,
                                    VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue,
                                    VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
                                };

                                for (var enumValueIndex = 0; enumValueIndex < enumValuesTable.Rows.Count; enumValueIndex++)
                                {
                                    enumComboBox.Items.Add(enumValuesTable.Rows[enumValueIndex]["Enum_Value"].ToString());
                                }

                                RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, enumComboBox,
                                                                 isNullable, defaultValue,
                                                                 new TextGridEditorOptions
                                                                 {
                                                                     MaxLength = 0,
                                                                     LengthMode = GridTextLengthMode.CharacterCount,
                                                                     UseCellEditorResult = false,
                                                                     DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                     RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                     NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                                 });
                            }
                            else
                            {
                                //不是 ENUM 的自定義型態，當成文字輸入
                                var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                {
                                    MaxLength = 0 //設定字串最大的可輸入長度，0 表示不限定！
                                };

                                RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                 isNullable, defaultValue,
                                                                 new TextGridEditorOptions
                                                                 {
                                                                     MaxLength = 0,
                                                                     LengthMode = GridTextLengthMode.CharacterCount,
                                                                     UseCellEditorResult = true,
                                                                     DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                     RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                     NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                                 });
                            }

                            if (MyGlobal.ShouldSavePoint)
                            {
                                var releasePointSql = SqlTraceHelper.BuildHeaderNewLine("---ReleasePoint: Determine whether it is an enum type");

                                MyGlobal.PostgreSqlReader.ReleasePoint("jqtt1688ttqj", releasePointSql);
                            }

                            break;
                        }
                    case "LockedColumnType": //不清楚它們的 Insert/Update 陳述式，直接列為不可編輯的欄位類型
                        {
                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.None,
                                                           isNullable, defaultValue);

                            break;
                        }
                    default:
                        {
                            var formattedDataType = string.Empty;
                            dataType = string.Empty;
                            var millisecondsFormat = string.Empty;

                            if (numericPrecision > 0)
                            {
                                var fractionDigitsFormat = new string('f', Math.Min(7, numericPrecision));

                                millisecondsFormat = $".{fractionDigitsFormat}";
                            }

                            var rawSchemaMetadataRow = DataTableSearchHelper.FindDataTableFirstRow(_dtRawSchemaTable, ("ColumnName", columnName));

                            formattedDataType = DatabaseSqlExecutor.GetDataTypeFormat_PostgreSql(rawSchemaMetadataRow, out dataType);

                            var parenthesisIndex = formattedDataType.IndexOf('(');

                            formattedDataType = parenthesisIndex == -1 ? formattedDataType : formattedDataType.Substring(0, parenthesisIndex);

                            switch (formattedDataType)
                            {
                                case "smallint": //-32768 to +32767
                                case "integer": //-2147483648 to +2147483647
                                case "bigint": //-9223372036854775808 to +9223372036854775807
                                case "numeric": //小數點前最多 131072 位數；小數點後最多 16383 位數。沒有指定任何 precision 或 scale 的話，可以儲存任何 precision 和 scale 的數值，直到達到 precision 和 scale 的極限。這種型別的欄位不會將輸入值強制轉為任何特定的 scale，其中具有聲明比例的數字欄位會將輸入值強制為該 scale。
                                case "real": //6 位小數精度
                                case "double precision": //15 位小數精度
                                case "decimal": //小數點前最多 131072 位數；小數點後最多 16383 位數
                                    {
                                        var numericEditor = new C1NumericEdit
                                        {
                                            TextAlign = HorizontalAlignment.Right,
                                            InitialSelection = InitialSelectionEnum.SelectAll,
                                            VisibleButtons = DropDownControlButtonFlags.DropDown
                                        };

                                        RegisterNumericGridDataColumnEditor(gridColumnIndex, columnName, numericEditor,
                                                                            isNullable, defaultValue,
                                                                            new NumericGridEditorOptions
                                                                            {
                                                                                NumericValueKind = GetPostgreSqlNumericValueKind(formattedDataType, numericPrecision),
                                                                                Precision = numericPrecision,
                                                                                Scale = numericScale,
                                                                                AllowNegative = true,
                                                                                AllowExponent = formattedDataType == "real" || formattedDataType == "double precision",
                                                                                AllowNonFinite = formattedDataType == "real" || formattedDataType == "double precision",
                                                                                DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.Zero,
                                                                                NullableEmptyBehavior = GridEditorNullableEmptyBehavior.Zero
                                                                            });
                                        break;
                                    }
                                case "smallint[]": //-32768 to +32767
                                case "integer[]": //-2147483648 to +2147483647
                                case "bigint[]": //-9223372036854775808 to +9223372036854775807
                                case "numeric[]": //小數點前最多 131072 位數；小數點後最多 16383 位數。沒有指定任何 precision 或 scale 的話，可以儲存任何 precision 和 scale 的數值，直到達到 precision 和 scale 的極限。這種型別的欄位不會將輸入值強制轉為任何特定的 scale，其中具有聲明比例的數字欄位會將輸入值強制為該 scale。
                                case "real[]": //6 位小數精度
                                case "double precision[]": //15 位小數精度
                                case "decimal[]": //小數點前最多 131072 位數；小數點後最多 16383 位數
                                    {

                                        RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                                       isNullable, defaultValue);
                                        break;
                                    }
                                case "int4range": //20240830 範例 '[4,4]' or '(4,4)'
                                case "int8range": //20240830 範例 '[4,4]' or '(4,4)'
                                case "numrange": //20240830 範例 '[1,10)'
                                case "inet": //20240814 inet 視為文字，填入的值可能為 IPV4 192.168.1.1/24 (IPV6 範例 2001:0db8:85a3:0000:0000:8a2e:0370:7334)
                                case "daterange": //填值範例：'[2023-01-01, 2023-05-31]'::daterange，後面的 ::daterange 可加可不加
                                case "datemultirange": //填值範例：'{[2016-07-02,2016-07-24),[2017-10-05,2017-10-12),[2018-05-23,2021-03-08)}'::datemultirange，後面的 ::datemultirange 可加可不加
                                case "tsrange": //填值範例：'[2021-10-01 6:00,2021-10-01 10:00]'::tsrange，後面的 ::tsrange 可加可不加
                                case "tstzrange": //填值範例：'[TIMESTAMPTZ "2023-04-01 12:00:00-4", TIMESTAMPTZ "2023-04-01 13:00:00-4"]
                                case "tsmultirange": //填值範例：'{[2021-10-01 6:00, 2021-10-01 10:00],[2021-10-01 14:00,2021-10-01 20:00]}'::tsmultirange，後面的 ::tsmultirange 可加可不加
                                case "macaddr": //20241003 視為文字
                                case "macaddr8": //20241003 視為文字
                                    {
                                        var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                        {
                                            MaxLength = 0 //設定字串最大的可輸入長度，0 表示不限定！
                                        };

                                        RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                         isNullable, defaultValue,
                                                                         new TextGridEditorOptions
                                                                         {
                                                                             MaxLength = 0,
                                                                             LengthMode = GridTextLengthMode.CharacterCount,
                                                                             UseCellEditorResult = true,
                                                                             DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                             RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                             NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                                         });
                                        break;
                                    }
                                case "int4multirange": //20240830 範例 '{[3,7), [8,9)}'
                                case "int8multirange": //20240830 範例 '{[3,7), [8,9)}'
                                case "nummultirange": //20240830 範例 '{[5,6),[7,8],(9,10)}'
                                case "int4range[]": //20240830 範例 array['(3,7)','(3,7)']
                                case "int4multirange[]": //20240830 範例 array['{[3,7),[8,9)}','{[3,7),[8,9)}']
                                case "int8range[]": //20240830 範例 array['(3,7)','(3,7)']
                                case "int8multirange[]": //20240830 範例 array['{[3,7), [8,9)}', '{[3,7), [8,9)}']
                                case "nummultirange[]": //20240906 範例 '{{[1,3),[5,7)},[9,11)}'
                                case "numrange[]": //20241009 範例 '{}' 或 【update abc set c76_numrange_ar = '{"[1,4)","[5,8)","[10,15)"}' where c01_char='1'】 或 【update abc set c76_numrange_ar = array[numrange(5.5, 15.75), numrange(30.1, 50.5, '[]')] where c01_char='1'】
                                case "inet[]": //20240814 inet[] 視為文字，填入的值可能為 '{192.168.1.1,192.168.1.2}'
                                case "text[]": //填值範例：'{Electronics,Computers,Gadgets}' 或 ARRAY['PostgreSQL','Database','SQL'] 或 '{{"element1", "element2"}, {"element3", "element4"}}'
                                case "cstring[]": //20241025
                                case "daterange[]": //填值範例：array[daterange('2024-01-01', '2024-01-31'),daterange('2024-02-01', '2024-02-28')]
                                case "datemultirange[]": //填值範例：array[datemultirange(daterange('2024-01-01','2024-01-15'),daterange('2024-02-01','2024-02-10')),datemultirange(daterange('2024-03-01', '2024-03-15'))]
                                case "tsrange[]": //填值範例：array['[2021-10-01 6:00, 2021-10-01 10:00]'::tsrange,'[2021-10-01 6:00,2021-10-01 10:00]'::tsrange]，後面的 ::tsrange[] 可加可不加
                                case "tstzrange[]": //填值範例：'[["2023-04-01 10:00:00+00","2023-04-02 15:00:00+00"),["2023-04-03 09:00:00+00","2023-04-04 17:00:00+00")]'::tstzrange[]，後面的 ::tstzrange[] 可加可不加
                                case "tsmultirange[]": //填值範例：array['{[2021-10-01 6:00, 2021-10-01 10:00],[2021-10-01 14:00,2021-10-01 20:00]}'::tsmultirange,'{[2021-10-01 6:00, 2021-10-01 10:00],[2021-10-01 14:00, 2021-10-01 20:00]}'::tsmultirange]，後面的 ::tsmultirange 可加可不加
                                case "macaddr[]": //20241003 視為文字，範例 update abc set c64_macaddr_ar='{01:02:03:04:05:06,02:03:04:05:06:07}' where c01_char='1'
                                case "macaddr8[]": //20241003 視為文字，範例 update abc set c66_macaddr8_ar='{"08:00:27:03:fb:19","08:00:27:62:35:51"}' where c01_char='1' (是否有雙引號不影響)
                                case "time with time zone[]":
                                case "time without time zone[]":
                                case "timestamp with time zone[]":
                                case "timestamp without time zone[]":
                                    {
                                        //20241009 這幾個型別，如果遇到以 array[ 開頭 ] 結尾的，不用處理；其餘的自動以 { 開頭 } 結尾

                                        RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                                       isNullable, defaultValue);
                                        break;
                                    }
                                case "Devart.Data.PostgreSql.PgSqlBlob":
                                    {
                                        RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.Binary,
                                                                       isNullable, defaultValue);
                                        break;
                                    }
                                case "boolean": //true 或 false
                                    {
                                        var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                        {
                                            MaxLength = 5 //設定字串最大的可輸入長度
                                        };

                                        RegisterBooleanGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                            isNullable, defaultValue, false);
                                        break;
                                    }
                                case "boolean[]": //陣列 true 或 false
                                    {

                                        RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                                       isNullable, defaultValue);
                                        break;
                                    }
                                case "bit": //0 或 1
                                case "bit varying": //0 或 1
                                    {
                                        var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                        {
                                            MaxLength = columnSize //設定字串最大的可輸入長度
                                        };

                                        RegisterBitGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                        isNullable, defaultValue, columnSize, formattedDataType == "bit", true, true);
                                        break;
                                    }
                                case "bit[]": //0 或 1
                                case "bit varying[]": //0 或 1 陣列
                                    {
                                        RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                                       isNullable, defaultValue);

                                        break;
                                    }
                                case "date":
                                    {
                                        var customFormat = MyLibrary.DateFormat;

                                        var dateEditor = new C1DateEdit
                                        {
                                            CustomFormat = customFormat,
                                            FormatType = FormatTypeEnum.CustomFormat,
                                            ShowUpDownButtons = false,
                                            NullText = MyLibrary.GridNullShowAs
                                        };

                                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.EditFormat.CustomFormat = customFormat;
                                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                             isNullable, defaultValue,
                                                                             new DateTimeGridEditorOptions
                                                                             {
                                                                                 ValueKind = GridDateTimeValueKind.DateTime,
                                                                                 Format = customFormat,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                             });

                                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                                        break;
                                    }
                                case "time with time zone": //無日期，有時區
                                    {
                                        var customFormat = $"HH:mm:ss{millisecondsFormat}zzz";

                                        var dateEditor = new C1DateEdit
                                        {
                                            CustomFormat = customFormat,
                                            FormatType = FormatTypeEnum.CustomFormat,
                                            ShowDropDownButton = false,
                                            ShowUpDownButtons = false,
                                            NullText = MyLibrary.GridNullShowAs
                                        };

                                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.EditFormat.CustomFormat = customFormat;
                                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                             isNullable, defaultValue,
                                                                             new DateTimeGridEditorOptions
                                                                             {
                                                                                 ValueKind = GridDateTimeValueKind.DateTimeOffset,
                                                                                 Format = customFormat,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                             });

                                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                                        break;
                                    }
                                case "time without time zone": //無日期、無時區
                                    {
                                        var customFormat = $"HH:mm:ss{millisecondsFormat}";

                                        var dateEditor = new C1DateEdit
                                        {
                                            CustomFormat = customFormat,
                                            FormatType = FormatTypeEnum.CustomFormat,
                                            //ShowCustomButton = false,
                                            ShowDropDownButton = false,
                                            ShowUpDownButtons = false,
                                            NullText = MyLibrary.GridNullShowAs
                                        };

                                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.EditFormat.CustomFormat = customFormat;
                                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                             isNullable, defaultValue,
                                                                             new DateTimeGridEditorOptions
                                                                             {
                                                                                 ValueKind = GridDateTimeValueKind.TimeSpan,
                                                                                 Format = customFormat,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                             });

                                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                                        break;
                                    }
                                case "timestamp with time zone": //日期+時間+時區
                                    {
                                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{millisecondsFormat}zz";

                                        var dateEditor = new C1DateEdit
                                        {
                                            CustomFormat = customFormat,
                                            FormatType = FormatTypeEnum.CustomFormat,
                                            ShowUpDownButtons = false,
                                            NullText = MyLibrary.GridNullShowAs
                                        };

                                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.EditFormat.CustomFormat = customFormat;
                                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                             isNullable, defaultValue,
                                                                             new DateTimeGridEditorOptions
                                                                             {
                                                                                 ValueKind = GridDateTimeValueKind.DateTimeOffset,
                                                                                 Format = customFormat,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                             });

                                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                                        break;
                                    }
                                case "timestamp without time zone": //日期+時間
                                    {
                                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{millisecondsFormat}";

                                        var dateEditor = new C1DateEdit
                                        {
                                            CustomFormat = customFormat,
                                            FormatType = FormatTypeEnum.CustomFormat,
                                            ShowUpDownButtons = false,
                                            NullText = MyLibrary.GridNullShowAs
                                        };

                                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.EditFormat.CustomFormat = customFormat;
                                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                             isNullable, defaultValue,
                                                                             new DateTimeGridEditorOptions
                                                                             {
                                                                                 ValueKind = GridDateTimeValueKind.DateTime,
                                                                                 Format = customFormat,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                             });

                                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                                        break;
                                    }
                                case "character":
                                case "character varying":
                                case "text":
                                    {
                                        if (columnSize < 0 || formattedDataType == "text")
                                        {
                                            columnSize = 0; //TextBox MaxLength
                                        }

                                        var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                        {
                                            MaxLength = columnSize //設定字串最大的可輸入長度
                                        };

                                        RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                         isNullable, defaultValue,
                                                                         new TextGridEditorOptions
                                                                         {
                                                                             MaxLength = columnSize,
                                                                             LengthMode = GridTextLengthMode.CharacterCount,
                                                                             UseCellEditorResult = true,
                                                                             DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                             RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                             NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                                         });

                                        break;
                                    }
                                default:
                                    {
                                        if (formattedDataType.EndsWith("[]", StringComparison.Ordinal)) //陣列，視為文字
                                        {
                                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                                           isNullable, defaultValue);
                                        }
                                        else //20241010 其他(例如新的型別)視為文字
                                        {
                                            var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                                            {
                                                MaxLength = 0 //設定字串最大的可輸入長度，0 表示不限定！
                                            };

                                            RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                             isNullable, defaultValue,
                                                                             new TextGridEditorOptions
                                                                             {
                                                                                 MaxLength = 0,
                                                                                 LengthMode = GridTextLengthMode.CharacterCount,
                                                                                 UseCellEditorResult = true,
                                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                                             });
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }

                gridColumnIndex++;
            }
        }
    }
}