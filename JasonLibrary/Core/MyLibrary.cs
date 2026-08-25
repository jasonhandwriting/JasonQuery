using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Update;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JasonLibrary.Core
{
    public static class MyLibrary
    {
        private static string _bookmarkStyle = string.Empty;
        private static string _colorToolstripBackground = string.Empty;
        private static string _colorEditorBackground = string.Empty;
        private static string _colorCurrentLineBackground = string.Empty;
        private static string _colorSelectedTextBackground = string.Empty;
        private static string _colorErrorLineBackground = string.Empty;
        private static string _colorBookmarkBackground = string.Empty;
        private static string _colorComments = string.Empty;
        private static string _colorTextIdentifier = string.Empty;
        private static string _colorBuiltInKeywords = string.Empty;
        private static string _colorUserDefinedKeywords = string.Empty;
        private static string _colorNumber = string.Empty;
        private static string _colorOperatorSymbol = string.Empty;
        private static string _colorOperatorKeywords = string.Empty;
        private static string _colorString = string.Empty;
        private static string _colorCharacter = string.Empty;
        private static string _colorBuiltInFunctions = string.Empty;
        private static string _colorWhiteSpace = string.Empty;
        private static string _colorUserDefinedTablesViews = string.Empty;
        private static string _colorUserDefinedFunctionsTriggers = string.Empty;
        private static string _colorOptionsTabActiveForeColor = string.Empty;
        private static string _colorOptionsTabActiveBackColor = string.Empty;
        private static string _colorOptionsTabInactiveForeColor = string.Empty;
        private static UpdateMetadataSourceKind _updateMetadataSource = UpdateMetadataSettingsContract.DefaultSource;
        private static string _updateMetadataLocalFolder = string.Empty;

        private static string _colorNewRowForeColor = string.Empty;
        private static string _colorNewRowBackColor = string.Empty;
        private static string _colorDeletedRowForeColor = string.Empty;
        private static string _colorDeletedRowBackColor = string.Empty;
        private static string _colorChangedCellForeColor = string.Empty;
        private static string _colorChangedCellBackColor = string.Empty;

        private static string _tabStyle = string.Empty; //IDE, Plain
        private static string _tabAppearance = string.Empty; //MultiDocument, MultiForm, MultiBox
        private static int _autoCompleteMinFragmentLength = 2;
        private static int _checkForUpdate = 7;
        private static int _sqlFormatterIndentSize = 4;
        private static int _sqlFormatterBlankLinesBetweenStatements = 1;
        private static int _sqlFormatterListItemsPerLine = SqlFormatOptions.DefaultListItemsPerLine;

        private static int _recentFilesQty = 20;
        private static int _myFavoriteQty = 20;
        private static string _gridQuotationMarks = string.Empty;
        private static string _gridFieldSeparator = string.Empty;
        private static string _dateFormat = string.Empty;
        private static string _gridNullShowAs = string.Empty;
        private static string _gridRowsPerPage = string.Empty;
        private static string _gridNullShowColor = string.Empty;
        private static string _gridVisualStyle = string.Empty;
        private static string _gridFontName = string.Empty;

        #region 20260531 重構 FontSize/Zoom 的定義及用法
        private const int DefaultQueryEditorFontSize = 12;
        private static int _queryEditorFontSize = DefaultQueryEditorFontSize;
        private const int DefaultQueryEditorZoom = 1;
        private static int _queryEditorZoom = DefaultQueryEditorZoom;
        private const float DefaultGridZoom = 1F;
        private static float _gridZoom = DefaultGridZoom;
        private const float DefaultGridFontSize = 12F;
        private static float _gridFontSize = DefaultGridFontSize;
        #endregion

        private static string _gridSheetName = string.Empty;
        private static string _gridHeadingForeColor = string.Empty;
        private static string _gridEvenRowForeColor = string.Empty;
        private static string _gridEvenRowBackColor = string.Empty;
        private static string _gridOddRowForeColor = string.Empty;
        private static string _gridOddRowBackColor = string.Empty;
        private static string _gridHighlightForeColor = string.Empty;
        private static string _gridHighlightBackColor = string.Empty;
        private static string _gridSelectedForeColor = string.Empty;
        private static string _gridSelectedBackColor = string.Empty;
        private static string _gridExcelSaveAsType = string.Empty;

        //Query Editor 頁籤：Highlight
        private static string _highlightColorForeColor = string.Empty;
        private static string _highlightColorStyle = string.Empty;
        private static string _highlightColorOutlineAlpha = string.Empty;
        private static string _highlightColorAlpha = string.Empty;

        //Query Editor 頁籤：Preferences
        private static string _queryEditorFontName = string.Empty;
        private static string _wordWrapIndentMode = string.Empty;

        //SQL To Code 頁籤：
        private static string _sqlToCodeSqlVariableName = string.Empty;
        private static string _sqlToCodeStringBuilderVariableName = string.Empty;

        //SQL Formatter 頁籤：
        private static string _generateSqlConvertCase = string.Empty;
        private static int _generateSqlNumbers = 5;

        private static string _rowSizing = string.Empty;
        private static string _gridInteractionDirection = string.Empty;

        public static int CommitRollbackIcon = -1; //20240724

        public static bool IsDarkMode { get; set; }
        public static bool IsBlobReadOnly { get; set; } = false;

        public static readonly HashSet<string> AllowedMaxWidthKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "500", "1000", "1500", "2000" };

        public static string TabStyle
        {
            get => _tabStyle;
            set => _tabStyle = "`IDE`Plain`".Contains($"`{value}`") ? value : "IDE";
        }

        public static string TabAppearance
        {
            get => _tabAppearance;
            set => _tabAppearance = "`MultiDocument`MultiForm`MultiBox`".Contains($"`{value}`") ? value : "MultiForm";
        }

        public static string ColorNewRowForeColor
        {
            get => _colorNewRowForeColor;
            set => _colorNewRowForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //Black
        }

        public static string ColorNewRowBackColor
        {
            get => _colorNewRowBackColor;
            set => _colorNewRowBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#A3EBF1"; //Light Blue
        }

        public static string ColorDeletedRowForeColor
        {
            get => _colorDeletedRowForeColor;
            set => _colorDeletedRowForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#FFFFFF"; //White
        }

        public static string ColorDeletedRowBackColor
        {
            get => _colorDeletedRowBackColor;
            set => _colorDeletedRowBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#C00000"; //Dark Red
        }

        public static string ColorChangedCellForeColor
        {
            get => _colorChangedCellForeColor;
            set => _colorChangedCellForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //Black
        }

        public static string ColorChangedCellBackColor
        {
            get => _colorChangedCellBackColor;
            set => _colorChangedCellBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#FF6EC7"; //Neon Pink
        }

        public static string ColorOptionsTabActiveForeColor
        {
            get => _colorOptionsTabActiveForeColor;
            set => _colorOptionsTabActiveForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //Black
        }

        public static string ColorOptionsTabActiveBackColor
        {
            get => _colorOptionsTabActiveBackColor;
            set => _colorOptionsTabActiveBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#E6FFFF"; //淡藍
        }

        public static string ColorOptionsTabInactiveForeColor
        {
            get => _colorOptionsTabInactiveForeColor;
            set => _colorOptionsTabInactiveForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#595959"; //DarkGray
        }

        public static string HighlightColorForeColor
        {
            get => _highlightColorForeColor;
            set => _highlightColorForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#0000FF"; //Color.Blue
        }

        public static string HighlightColorStyle
        {
            get => _highlightColorStyle;
            set => _highlightColorStyle = "`Box`CompositionThick`Dash`Diagonal`StraightBox`".Contains($"`{value}`") ? value : "StraightBox";
        }

        public static string HighlightColorOutlineAlpha
        {
            get => _highlightColorOutlineAlpha;
            set => _highlightColorOutlineAlpha = "`50`60`70`80`90`100`110`120`130`140`150`160`170`180`190`200`".Contains($"`{value}`") ? value : "50";
        }

        public static string HighlightColorAlpha
        {
            get => _highlightColorAlpha;
            set => _highlightColorAlpha = "`50`60`70`80`90`100`110`120`130`140`150`160`170`180`190`200`".Contains($"`{value}`") ? value : "50";
        }

        public static string QueryEditorFontName
        {
            get => _queryEditorFontName;
            set => _queryEditorFontName = !string.IsNullOrWhiteSpace(value) ? value : "Consolas";
        }

        #region 20260531 重構 QueryEditorFontSize/QueryEditorZoom 的定義及用法
        public static int QueryEditorFontSize
        {
            get => _queryEditorFontSize;
            set => _queryEditorFontSize = IsValidQueryEditorFontSize(value) ? value : DefaultQueryEditorFontSize;
        }

        public static string QueryEditorFontSizeText => QueryEditorFontSize.ToString(CultureInfo.InvariantCulture);

        public static void SetQueryEditorFontSizeFromText(string value)
        {
            QueryEditorFontSize = ParseQueryEditorFontSize(value);
        }

        private static int ParseQueryEditorFontSize(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fontSize) ? fontSize : DefaultQueryEditorFontSize;
        }

        private static bool IsValidQueryEditorFontSize(int value)
        {
            return value == 10 || value == 12 || value == 14 || value == 16 || value == 18;
        }

        public static int QueryEditorZoom
        {
            get => _queryEditorZoom;
            set => _queryEditorZoom = IsValidQueryEditorZoom(value) ? value : DefaultQueryEditorZoom;
        }

        public static string QueryEditorZoomText =>
            QueryEditorZoom.ToString(CultureInfo.InvariantCulture);

        public static void SetQueryEditorZoomFromText(string value)
        {
            QueryEditorZoom = ParseQueryEditorZoom(value);
        }

        private static int ParseQueryEditorZoom(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var zoom)
                ? zoom
                : DefaultQueryEditorZoom;
        }

        private static bool IsValidQueryEditorZoom(int value)
        {
            return value == -2 || value == -1 || value == 0 || value == 1 || value == 2;
        }
        #endregion 20260531 重構 QueryEditorFontSize/QueryEditorZoom 的定義及用法

        public static string RecentFilesHistory { get; set; } = string.Empty; //Recent Files History

        public static string DefaultDirectory { get; set; } = string.Empty; //Default Directory

        public static string GridQuotationMarks //Gird's Visual Style
        {
            get => _gridQuotationMarks;
            set => _gridQuotationMarks = "`None`\"`'`".Contains($"`{value}`") ? value : "None";
        }

        public static string GridFieldSeparator
        {
            get => _gridFieldSeparator;
            set => _gridFieldSeparator = !string.IsNullOrWhiteSpace(value) && "`,`;`|`TAB`".Contains("`" + value.ToUpper() + "`") ? value.ToUpper() : ",";
        }

        public static string DateFormat
        {
            get => _dateFormat;
            set => _dateFormat = "`YYYY/MM/DD`YYYY-MM-DD`MM/DD/YYYY`MM-DD-YYYY`DD/MM/YYYY`DD-MM-YYYY`".Contains("`" + value.ToUpper() + "`") ? value : "yyyy/MM/dd";
        }

        public static bool ShowDatabaseName { get; set; } = true; //是否顯示資料庫名稱

        public static bool ShowVersion { get; set; } = true; //是否顯示版號？

        public static bool ShowIP { get; set; } = true; //是否顯示IP？

        public static bool GridShowColumnDataType { get; set; } //是否啟用 Show Column's Data Type？

        public static bool GridShowFilterRow { get; set; } //是否啟用 Show Filter Row？

        public static bool GridShowGroupingRow { get; set; } //是否啟用 Show Filter Row？

        public static bool GridResize { get; set; } //是否啟用 Auto Resize？

        public static bool GridShowColumnComment { get; set; } //是否啟用 顯示欄位註解？

        public static bool GridRawDataMode { get; set; } //是否啟用 Raw Data Mode？

        public static string GridNullShowAs //Null Values 顯示方式
        {
            get => _gridNullShowAs;
            set => _gridNullShowAs = "`none`<null>`{null}`(null)`".ToUpper().Contains("`" + value.ToUpper() + "`") ? value : "<NULL>";
        }

        public static bool GridPagingQuery { get; set; } = true; //是否啟用 分頁查詢？

        public static string GridRowsPerPage //分頁查詢每頁的筆數
        {
            get => _gridRowsPerPage;
            set => _gridRowsPerPage = "`100`200`300`400`500`1000`2000`5000`".Contains($"`{value}`") ? value : "500";
        }

        public static bool GridAppendingQueries { get; set; } = true; //是否啟用 附加查詢？

        public static bool GridSetFocusAfterQuery { get; set; } = true; //是否啟用 查詢後切換到 Grid？

        public static string GridNullShowColor
        {
            get => _gridNullShowColor;
            set
            {
                if (value.Length == 7 && CheckColorCode(value))
                {
                    _gridNullShowColor = value;
                }
                else
                {
                    _gridNullShowColor = "#0000FF"; //標準藍色
                }
            }
        }

        public static string GridVisualStyle //Gird's Visual Style
        {
            get => _gridVisualStyle;
            set => _gridVisualStyle = "`Office 2007 Blue`Office 2007 Silver`Office 2007 Black`Office 2010 Blue`Office 2010 Silver`Office 2010 Black`".Contains($"`{value}`") ? value : "Office 2010 Blue";
        }

        public static string GridFontName
        {
            get => _gridFontName;
            set => _gridFontName = !string.IsNullOrWhiteSpace(value) ? value : "Consolas";
        }

        #region 20260531 重構 GridZoom/GridFontSize 的定義及用法
        public static float GridZoom
        {
            get => _gridZoom;
            set => _gridZoom = IsValidGridZoom(value) ? value : DefaultGridZoom;
        }

        public static void SetGridZoomFromText(string value)
        {
            GridZoom = ParseGridZoom(value);
        }

        private static float ParseGridZoom(string value)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var zoom) ? zoom : DefaultGridZoom;
        }

        private static bool IsValidGridZoom(float value)
        {
            return value == -2F || value == -1F || value == 0F || value == 1F || value == 2F;
        }

        public static float GridFontSize
        {
            get => _gridFontSize;
            set => _gridFontSize = IsValidGridFontSize(value) ? value : DefaultGridFontSize;
        }

        public static string GridFontSizeText => GridFontSize.ToString("0.##", CultureInfo.InvariantCulture);

        public static void SetGridFontSizeFromText(string value)
        {
            GridFontSize = ParseGridFontSize(value);
        }

        private static float ParseGridFontSize(string value)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize) ? fontSize : DefaultGridFontSize;
        }

        private static bool IsValidGridFontSize(float value)
        {
            return value == 9F || value == 10F || value == 11F || value == 12F || value == 13F || value == 14F || value == 15F || value == 16F || value == 17F || value == 18F;
        }
        #endregion 20260531 重構 GridZoom/GridFontSize 的定義及用法

        public static string GridSheetName
        {
            get => _gridSheetName;
            set => _gridSheetName = !string.IsNullOrWhiteSpace(value) ? value : "Data";
        }

        public static string GridHeadingForeColor
        {
            get => _gridHeadingForeColor;
            set
            {
                if (value.Length == 7 && CheckColorCode(value))
                {
                    _gridHeadingForeColor = value;
                }
                else
                {
                    _gridHeadingForeColor = "#000000"; //白色=#FFFFFF, 黑色=#000000
                }
            }
        }

        public static string GridEvenRowForeColor
        {
            get => _gridEvenRowForeColor;
            set
            {
                if (value.Length == 7 && CheckColorCode(value))
                {
                    _gridEvenRowForeColor = value;
                }
                else
                {
                    _gridEvenRowForeColor = "#000000"; //白色=#FFFFFF, 黑色=#000000
                }
            }
        }

        public static string GridEvenRowBackColor
        {
            get => _gridEvenRowBackColor;
            set => _gridEvenRowBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#FFFFFF"; //白色=#FFFFFF, 黑色=#000000
        }

        public static string GridOddRowForeColor
        {
            get => _gridOddRowForeColor;
            set => _gridOddRowForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //白色=#FFFFFF, 黑色=#000000
        }

        public static string GridOddRowBackColor
        {
            get => _gridOddRowBackColor;
            set => _gridOddRowBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#FFFFC1"; //淺藍色=#DBEEF3, 淺黃色=#FFFFC1
        }

        public static string GridHighlightForeColor
        {
            get => _gridHighlightForeColor;
            set => _gridHighlightForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //黑色=#000000
        }

        public static string GridHighlightBackColor
        {
            get => _gridHighlightBackColor;
            set => _gridHighlightBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#92D050"; //翠綠色=#92D050
        }

        public static string GridSelectedForeColor
        {
            get => _gridSelectedForeColor;
            set => _gridSelectedForeColor = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //黑色=#000000
        }

        public static string GridSelectedBackColor
        {
            get => _gridSelectedBackColor;
            set => _gridSelectedBackColor = value.Length == 7 && CheckColorCode(value) ? value : "#C6D9F0"; //淡藍色=#C6D9F0
        }

        public static string GridExcelFileName { get; set; } = string.Empty; //Excel 匯出的檔名

        public static string GridCSVDelimiters { get; set; } = string.Empty; //匯出 CSV 的分隔符號

        public static string GridEncoding { get; set; } = string.Empty; //匯出 CSV 的編碼

        public static string GridExcelWorksheetName { get; set; } = string.Empty; //Excel 匯出的工作表名稱

        public static string GridExcelSaveAsType //Excel 匯出的存檔類型
        {
            get => _gridExcelSaveAsType;

            set
            {
                switch (value)
                {
                    case @"Excel 2007 (*.xlsx)":
                    case @"Excel 2003 (*.xls)":
                    case @"XML A (*.xml)":
                    case @"XML B (*.xml)":
                    case @"XML C (*.xml)":
                    case @"XML DataPacket 2.0 (*.xml)":
                    case @"XML Access (*.xml)":
                    case @"JSON (*.json)":
                    case @"HTML (*.html)":
                    case @"PDF (*.pdf)":
                        {
                            _gridExcelSaveAsType = value;
                            break;
                        }
                    default:
                        {
                            _gridExcelSaveAsType = @"CSV (*.csv)";
                            break;
                        }
                }
            }
        }

        public static bool GridConvertCRLF { get; set; } = false; //是否置換 CR LF

        public static bool GridExcelAutoOpen { get; set; } = true; //是否自動開啟 Excel？

        public static bool GridExcelAutoColumnResize { get; set; } = true; //是否自動調整欄寬？

        public static bool GridExcelCustom { get; set; } //是否自訂外觀樣式？

        public static string GridExcelHeadingBackColor { get; set; } = string.Empty;

        public static string GridExcelEvenRowBackColor { get; set; } = string.Empty;

        public static string GridExcelOddRowBackColor { get; set; } = string.Empty;

        public static string GridExcelFontName { get; set; } = string.Empty;

        public static string GridExcelFontSize { get; set; } = string.Empty;

        public static string GridExcelRowHeight { get; set; } = string.Empty;

        public static bool EnableAutoReplace { get; set; } //是否啟用 Auto Replace？

        public static bool CheckForUpdate { get; set; } = true; //是否啟用 Check For Update？

        public static int CheckForUpdateValue //Check for Update 頻率
        {
            get => _checkForUpdate;
            set => _checkForUpdate = "`0`1`7`".Contains($"`{value}`") ? value : 7;
        }

        public static UpdateMetadataSourceKind UpdateMetadataSource
        {
            get => _updateMetadataSource;
            set => _updateMetadataSource = UpdateMetadataSettingsContract.NormalizeSource(value);
        }

        public static string UpdateMetadataLocalFolder
        {
            get => _updateMetadataLocalFolder;
            set => _updateMetadataLocalFolder = (value ?? string.Empty).Trim();
        }

        public static int RecentFilesQty
        {
            get => _recentFilesQty;
            set => _recentFilesQty = value >= 10 && value <= 99 ? value : 20;
        }

        public static int MyFavoriteQty
        {
            get => _myFavoriteQty;
            set => _myFavoriteQty = value >= 10 && value <= 99 ? value : 20;
        }

        public static bool EnableAutoComplete { get; set; } //是否啟用 Auto Complete？

        public static int AutoCompleteMinFragmentLength //Auto Complete 最小觸發長度
        {
            get => _autoCompleteMinFragmentLength;
            set => _autoCompleteMinFragmentLength = "`2`3`4`5`6`7`8`9`".Contains($"`{value}`") ? value : 2;
        }

        public static bool AutoCompleteFirstCharChecking { get; set; } //是否啟用 Auto Complete - First Char Checking？

        public static bool AutoCompleteBuiltInKeywords { get; set; } //是否啟用 Auto Complete - BuiltInKeywords？

        public static bool AutoCompleteBuiltInFunctions { get; set; } //是否啟用 Auto Complete - BuiltInKeywords？

        public static bool AutoCompleteUserDefinedKeywords { get; set; } //是否啟用 Auto Complete - UserDefinedKeywords？

        public static bool AutoCompleteUserDefinedFunctions { get; set; } //是否啟用 Auto Complete - UserDefinedFunctions？

        public static bool AutoCompleteUserDefinedTables { get; set; } //是否啟用 Auto Complete - UserDefinedTables？

        public static bool AutoCompleteUserDefinedTriggers { get; set; } //是否啟用 Auto Complete - UserDefinedTriggers？

        public static bool AutoCompleteUserDefinedViews { get; set; } //是否啟用 Auto Complete - UserDefinedViews？

        public static string WordWrapIndentMode
        {
            get => _wordWrapIndentMode;
            set => _wordWrapIndentMode = "`Fixed`Same`Indent`".Contains($"`{value}`") ? value : "Same";
        }

        public static string ColorToolstripBackground
        {
            get => _colorToolstripBackground;
            set => _colorToolstripBackground = value.Length == 7 && CheckColorCode(value) ? value : "#E3FDCA"; //#E6FFFF=淡藍；#FFFFD0=淡黃；#E3FDCA=淡綠
        }

        public static string ColorEditorBackground
        {
            get => _colorEditorBackground;
            set => _colorEditorBackground = value.Length == 7 && CheckColorCode(value) ? value : "#FFFFFF";
        }

        public static string ColorCurrentLineBackground
        {
            get => _colorCurrentLineBackground;
            set => _colorCurrentLineBackground = value.Length == 7 && CheckColorCode(value) ? value : "#FFFFE0"; //LightYellow
        }

        public static string ColorSelectedTextBackground
        {
            get => _colorSelectedTextBackground;
            set => _colorSelectedTextBackground = value.Length == 7 && CheckColorCode(value) ? value : "#ADD8E6"; //LightBlue
        }

        public static string ColorErrorLineBackground
        {
            get => _colorErrorLineBackground;
            set => _colorErrorLineBackground = value.Length == 7 && CheckColorCode(value) ? value : "#FF0000"; //Red
        }

        public static string BookmarkStyle
        {
            get => _bookmarkStyle;
            set => _bookmarkStyle = "`Arrow`Circle`RoundRect`ShortArrow`SmallRect`".Contains($"`{value}`") ? value : "ShortArrow";
        }

        public static string ColorBookmarkBackground
        {
            get => _colorBookmarkBackground;
            set => _colorBookmarkBackground = value.Length == 7 && CheckColorCode(value) ? value : "#00FFFF"; //Cyan 亮青
        }

        public static string ColorComments
        {
            get => _colorComments;
            set => _colorComments = value.Length == 7 && CheckColorCode(value) ? value : "#008000"; //Green

        }

        public static string ColorTextIdentifier
        {
            get => _colorTextIdentifier;
            set => _colorTextIdentifier = value.Length == 7 && CheckColorCode(value) ? value : "#000000"; //Text, Black
        }

        public static string ColorBuiltInKeywords
        {
            get => _colorBuiltInKeywords;
            set => _colorBuiltInKeywords = value.Length == 7 && CheckColorCode(value) ? value : "#0000FF"; //LightSeaGreen
        }

        public static string ColorUserDefinedKeywords
        {
            get => _colorUserDefinedKeywords;
            set => _colorUserDefinedKeywords = value.Length == 7 && CheckColorCode(value) ? value : "#0000FF"; //LightSeaGreen
        }

        public static string ColorNumber
        {
            get => _colorNumber;
            set => _colorNumber = value.Length == 7 && CheckColorCode(value) ? value : "#800000"; //Maroon
        }

        public static string ColorOperatorSymbol
        {
            get => _colorOperatorSymbol;
            set => _colorOperatorSymbol = value.Length == 7 && CheckColorCode(value) ? value : "#800000"; //Maroon
        }

        public static string ColorOperatorKeywords
        {
            get => _colorOperatorKeywords;
            set => _colorOperatorKeywords = value.Length == 7 && CheckColorCode(value) ? value : "#366092"; //靛藍色
        }

        public static string ColorString
        {
            get => _colorString;
            set => _colorString = value.Length == 7 && CheckColorCode(value) ? value : "#FF0000"; //Red
        }

        public static string ColorCharacter
        {
            get => _colorCharacter;
            set => _colorCharacter = value.Length == 7 && CheckColorCode(value) ? value : "#FF0000"; //Red
        }

        public static string ColorBuiltInFunctions
        {
            get => _colorBuiltInFunctions;
            set => _colorBuiltInFunctions = value.Length == 7 && CheckColorCode(value) ? value : "#FF00FF"; //Magenta
        }

        public static string ColorWhiteSpace
        {
            get => _colorWhiteSpace;
            set => _colorWhiteSpace = value.Length == 7 && CheckColorCode(value) ? value : "#00FFFF"; //Cyan
        }

        public static string ColorUserDefinedTablesViews
        {
            get => _colorUserDefinedTablesViews;
            set => _colorUserDefinedTablesViews = value.Length == 7 && CheckColorCode(value) ? value : "#808000"; //Olive (仿 Toad Table Name Color)
        }

        public static string ColorUserDefinedFunctionsTriggers
        {
            get => _colorUserDefinedFunctionsTriggers;
            set => _colorUserDefinedFunctionsTriggers = value.Length == 7 && CheckColorCode(value) ? value : "#808000"; //Olive (仿 Toad Table Name Color)
        }

        public static string KeywordsOperatorKeywords { get; set; } = string.Empty;

        public static string KeywordsBuiltInFunctions { get; set; } = string.Empty;

        public static string KeywordsBuiltInKeywords { get; set; } = string.Empty;

        public static string KeywordsUserDefinedKeywords { get; set; } = string.Empty;

        public static string KeywordsUserDefinedTables { get; set; } = string.Empty;

        public static string KeywordsUserDefinedViews { get; set; } = string.Empty;

        public static string KeywordsUserDefinedFunctions { get; set; } = string.Empty;

        public static string KeywordsUserDefinedTriggers { get; set; } = string.Empty;

        public static string SqlToCodeSqlVariableName
        {
            get => _sqlToCodeSqlVariableName;
            set => _sqlToCodeSqlVariableName = !string.IsNullOrWhiteSpace(value) ? value : "sql";
        }

        public static string SqlToCodeStringBuilderVariableName
        {
            get => _sqlToCodeStringBuilderVariableName;
            set => _sqlToCodeStringBuilderVariableName = !string.IsNullOrWhiteSpace(value) ? value : "sbSql";
        }

        public static SqlFormatterEngineKind SqlFormatterEngine { get; set; } = SqlFormatterEngineKind.Unknown;

        public static int SqlFormatterIndentSize
        {
            get => _sqlFormatterIndentSize;
            set => _sqlFormatterIndentSize = value == 2 || value == 4 || value == 8 ? value : 4;
        }

        public static int SqlFormatterMaxLineWidth { get; set; } = 999;

        public static int SqlFormatterBlankLinesBetweenStatements
        {
            get => _sqlFormatterBlankLinesBetweenStatements;
            set => _sqlFormatterBlankLinesBetweenStatements = value >= 0 && value <= 4 ? value : 1;
        }

        public static int SqlFormatterListItemsPerLine
        {
            get => _sqlFormatterListItemsPerLine;
            set => _sqlFormatterListItemsPerLine = value >= 1 && value <= 10 ? value : SqlFormatOptions.DefaultListItemsPerLine;
        }

        public static bool SqlFormatterConvertCaseForKeywords { get; set; }

        public static int SqlFormatterConvertCaseForKeywordsCase { get; set; } = 1;

        public static string GenerateSqlConvertCase
        {
            get => _generateSqlConvertCase;
            set => _generateSqlConvertCase = "`UpperAll`UpperKeywords`LowerAll`".Contains($"`{value}`") ? value : "UpperAll";
        }

        public static int GenerateSqlNumbers //GenerateSql: 每列 Select 欄位名稱的數量
        {
            get => _generateSqlNumbers;
            set => _generateSqlNumbers = "`1`2`3`4`5`6`7`8`9`10`".Contains($"`{value}`") ? value : 5;
        }

        public static bool KeywordFontBold { get; set; }

        public static bool ShowAllCharacters { get; set; }

        public static bool ShowSaveAsButton { get; set; }

        public static bool ShowIndentGuide { get; set; } = true;

        public static bool EntireBlankRowAsEmptyRow { get; set; } = true;

        public static bool HighlightSelection { get; set; } = true;

        public static bool CopyAsHTML { get; set; }

        public static bool WordWrap { get; set; }

        public static bool WordWrapVisualFlags_Start { get; set; }

        public static bool WordWrapVisualFlags_End { get; set; }

        public static bool WordWrapVisualFlags_Margin { get; set; }

        public static string RowSizing
        {
            get => _rowSizing;
            set => _rowSizing = "`AllRows`IndividualRows`".Contains($"`{value}`") ? value : "AllRows";
        }

        public static string GridInteractionDirection
        {
            get => _gridInteractionDirection;
            set => _gridInteractionDirection = "`Down`Right`Up`Left`".Contains($"`{value}`") ? value : "Down";
        }

        private static bool CheckColorCode(string sColorCode)
        {
            var regexColorCode = new Regex("^#[a-fA-F0-9]{6}$");

            return regexColorCode.IsMatch(sColorCode.Trim());
        }
    }
}
