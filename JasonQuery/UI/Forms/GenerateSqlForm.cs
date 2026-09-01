using C1.Win.C1Themes;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.CreateScript.PostgreSql;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Schema;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class GenerateSqlForm : Form
    {
        public DataTable dtColumnName { get; set; }
        public string SchemaType { get; set; } = string.Empty; //Tables or Views
        public string SchemaNode { get; set; } = string.Empty;
        public string SchemaDbo { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public string SqlType { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string AccessibleDescriptionString { get; set; } = string.Empty;

        private string _languageText;
        private string columnInfoSql = string.Empty;
        private DataTable _dtTable;
        private DataTable _dtView;
        private bool _isKeypressComboBox = false;
        private bool _isKeyPressTab = false; //記住是否按下 Tab 鍵 (KeyUp 可以偵測 Tab，但 Tab 鍵已被提前觸發了，故不在 KeyUp 攔截)
        private bool _isKeyPressESC = false; //記住是否按下 ESC 鍵
        private bool _isKeyPressDelete = false; //記住是否按下 Delete 鍵
        private ColumnInfoCollector columnInfoCollector;

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public GenerateSqlForm()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                //20260726 統一圖示風格
                btnSelectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnUnselectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Unselect All 16x16.ico");

                LocalizationHelper.ApplyLanguageInfo(this, false, false);
                ApplyLocalizationSetting();

                Text += $" - {DatabaseSqlExecutor.DataSourceDisplayName}";
                btnUnselectAll.Location = new Point(btnSelectAll.Left + btnSelectAll.Width + 15, btnUnselectAll.Top);

                if (MyLibrary.IsDarkMode)
                {
                    C1ThemeController.ApplicationTheme = "VS2013Dark";
                    c1ThemeController1.SetTheme(lblNumbers, "VS2013Dark");
                }

                GridHelper.SetGridVisualStyle(c1Grid, 10);
                c1Grid.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                GridHelper.SetGridVisualStyle(c1GridTable, 10);
                c1GridTable.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                GridHelper.SetGridVisualStyle(c1GridView, 10);
                c1GridView.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

                txtDatabase.Location = new Point(lblDatabase.Left + lblDatabase.Width, txtDatabase.Top);
                cboTable.Location = new Point(rdoTable.Left + rdoTable.Width, cboTable.Top);
                c1GridTable.Location = new Point(cboTable.Left, c1GridTable.Top);
                cboView.Location = new Point(rdoView.Left + rdoView.Width, cboView.Top);
                c1GridView.Location = new Point(cboView.Left, c1GridView.Top);
                txtAliasName.Location = new Point(chkAliasName.Left + chkAliasName.Width, txtAliasName.Top);
                cboNumbers.Location = new Point(lblNumbers.Left + lblNumbers.Width, cboNumbers.Top);
                cboSchema.Location = new Point(lblSchema.Left + lblSchema.Width, cboSchema.Top);
                txtDatabase.Text = string.IsNullOrEmpty(DatabaseSqlExecutor.DatabaseName) ? string.Empty : DatabaseSqlExecutor.DatabaseName;
                txtDatabase.Enabled = false;

                var enabled = false;
                var dtSchema = new DataTable();

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            lblDatabase.Visible = false;
                            txtDatabase.Visible = false;
                            rdoTable.Location = new Point(rdoTable.Left - 306, rdoTable.Top);
                            cboTable.Location = new Point(cboTable.Left - 306, cboTable.Top);
                            c1GridTable.Location = new Point(cboTable.Left, c1GridTable.Top);
                            rdoView.Location = new Point(rdoView.Left - 306, rdoView.Top);
                            cboView.Location = new Point(cboView.Left - 306, cboView.Top);
                            c1GridView.Location = new Point(cboView.Left, c1GridView.Top);
                            btnSelectObject.Location = new Point(btnSelectObject.Left - 306, btnSelectObject.Top);

                            DatabaseSqlExecutor.GetTableInfo_Oracle(SchemaNode, out _dtTable);
                            DatabaseSqlExecutor.GetViewInfo_Oracle(SchemaNode, out _dtView);

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            DatabaseSqlExecutor.GetSchemaInfo_PostgreSql(out dtSchema);
                            UIHelper.SetC1ComboBoxItemsFromDataTable(cboSchema, dtSchema);

                            lblSchema.Visible = true;
                            cboSchema.Visible = true;
                            cboSchema.Text = SchemaNode;

                            if (!string.IsNullOrEmpty(AccessibleDescriptionString))
                            {
                                DatabaseSqlExecutor.GetTableInfo_PostgreSql(out _dtTable, cboSchema.Text);
                                DatabaseSqlExecutor.GetViewInfo_PostgreSql(out _dtView, cboSchema.Text);
                            }

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            enabled = true;

                            if (string.IsNullOrEmpty(DatabaseSqlExecutor.DatabaseName) && string.IsNullOrEmpty(SchemaName))
                            {
                                return;
                            }

                            if (string.IsNullOrEmpty(SchemaNode))
                            {
                                SchemaNode = txtDatabase.Text;
                            }

                            DatabaseSqlExecutor.GetSchemaInfo_SqlServer(out dtSchema);
                            UIHelper.SetC1ComboBoxItemsFromDataTable(cboSchema, dtSchema);

                            lblSchema.Visible = true;
                            cboSchema.Visible = true;

                            if (cboSchema.Items.Contains("dbo"))
                            {
                                cboSchema.Text = "dbo";
                            }
                            else
                            {
                                cboSchema.Text = dtSchema.Rows[0].GetSafeString(0);
                            }

                            if (!string.IsNullOrEmpty(SchemaNode))
                            {
                                DatabaseSqlExecutor.GetTableInfo_SqlServer(SchemaNode, cboSchema.Text, out _dtTable);
                                DatabaseSqlExecutor.GetViewInfo_SqlServer(SchemaNode, cboSchema.Text, out _dtView);
                            }

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            enabled = true;
                            SchemaNode = string.IsNullOrEmpty(SchemaNode) ? txtDatabase.Text : SchemaNode;

                            DatabaseSqlExecutor.GetTableInfo_MySql(SchemaNode, out _dtTable);
                            DatabaseSqlExecutor.GetViewInfo_MySql(SchemaNode, out _dtView);

                            break;
                        }
                }

                UIHelper.SetC1ComboBoxItemsFromDataTable(cboTable, _dtTable);
                UIHelper.SetC1ComboBoxItemsFromDataTable(cboView, _dtView);

                txtDatabase.Enabled = enabled;

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                    case DataSourceType.PostgreSql:
                        {
                            chkEncloseGraveAccent.Visible = false;
                            chkEncloseBrackets.Visible = true;
                            chkEncloseBrackets.Enabled = false;
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            chkEncloseGraveAccent.Visible = false;
                            chkEncloseBrackets.Visible = true;
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            chkEncloseGraveAccent.Visible = true;
                            chkEncloseBrackets.Visible = false;
                            break;
                        }
                }

                txtDatabase.ReadOnly = true;

                if (string.IsNullOrEmpty(AccessibleDescriptionString))
                {
                    btnPreview.Enabled = false;
                    btnPasteToQueryEditor.Enabled = false;
                    return;
                }
                else
                {
                    if (SchemaObjectTypeHelper.Is(SchemaType, SchemaObjectNames.Views))
                    {
                        rdoView.Checked = true;
                        cboView.Text = SchemaName;
                    }
                    else
                    {
                        rdoTable.Checked = true;
                        cboTable.Text = SchemaName;
                    }
                }

                RefreshSchemaInfo();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "GenerateSQLFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "GenerateSQLFormHeight", Size.Height.ToString());
        }

        private void Form_MouseClick(object sender, MouseEventArgs e)
        {
            c1GridTable.Visible = false;
            c1GridView.Visible = false;
        }

        private void Initial()
        {
            if (string.IsNullOrEmpty(PrimaryKey))
            {
                chkPKInfo.Checked = false;
                chkPKInfo.Enabled = false;
            }
            else
            {
                chkPKInfo.Checked = true;
                chkPKInfo.Enabled = true;
            }

            cboNumbers.Text = "5";
            c1Grid.DataSource = CreateColumnTable(dtColumnName, "1"); //不能直接指定 dtColumnName，會造成 CheckBox 唯讀！

            GridHelper.ReplaceColumnCaptionByLanguageInfo(c1Grid, Name, true);
            SetCheckBox(" ");

            if (SchemaObjectTypeHelper.Is(SchemaType, SchemaObjectNames.Views))
            {
                rdoDelete.Enabled = false;
                rdoInsert.Enabled = false;
                rdoUpdate.Enabled = false;
                rdoAlter.Enabled = false;
                rdoCreate.Enabled = false;
                rdoDrop.Enabled = false;
                rdoRename.Enabled = false;
                rdoTruncate.Enabled = false;
                rdoView.Checked = true;
            }
            else
            {
                rdoDelete.Enabled = true;
                rdoInsert.Enabled = true;
                rdoUpdate.Enabled = true;
                rdoAlter.Enabled = true;
                rdoCreate.Enabled = true;
                rdoDrop.Enabled = true;
                rdoRename.Enabled = true;
                rdoTruncate.Enabled = true;
                rdoTable.Checked = true;
            }

            rdoSelectStar.Checked = false;
            rdoSelectStar.Checked = true;
        }

        private void SetCheckBox(string sColumn)
        {
            var items = c1Grid.Columns[sColumn].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.CheckBox;

            items.Values.Clear();
            items.Values.Add(new ValueItem("0", false)); //unchecked
            items.Values.Add(new ValueItem("1", true));  //checked

            //指定哪一個 Column 要套用 FetchCellStyle
            c1Grid.Splits[0].DisplayColumns[1].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[2].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[3].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[4].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[5].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[6].FetchStyle = true;
            c1Grid.Splits[0].DisplayColumns[7].FetchStyle = true;

            c1Grid.Splits[0].DisplayColumns[" "].Style.HorizontalAlignment = C1.Win.C1TrueDBGrid.AlignHorzEnum.Center;

            foreach (C1DisplayColumn col in c1Grid.Splits[0].DisplayColumns)
            {
                col.AutoSize();

                if (col.Name == " ")
                {
                    col.AllowSizing = false;
                    col.Locked = false;
                }
            }

            c1Grid.Splits[0].DisplayColumns["TypeName"].Visible = false;
            c1Grid.Splits[0].DisplayColumns["DataTypeName"].Visible = false;
        }

        private void ApplyLocalizationSetting()
        {
            if (MyLibrary.IsDarkMode)
            {
                C1ThemeController.ApplicationTheme = "VS2013Dark";
            }

            GridHelper.SetGridVisualStyle(c1Grid);
            GridFontAndBackColor();
            GridZoom();

            GridHelper.SetGridVisualStyle(c1Grid, 10);
            ApplyEditorSetting();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GridFontAndBackColor()
        {
            const int iFontSize = 10;

            c1Grid.Font = new Font(MyLibrary.GridFontName, iFontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
            c1Grid.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        private void GridZoom()
        {
            const int iFontSize = 10;
            const float pcnt = 1.0F;
            var iRowHeight = c1Grid.RowHeight;
            //var iRecSelWidth = c1Grid.RecordSelectorWidth;

            c1Grid.RowHeight = (int)(iRowHeight * pcnt) + 5;
            c1Grid.Splits[0].ColumnCaptionHeight = (int)(iRowHeight * pcnt) + 5;
            c1Grid.Styles["Normal"].Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, iFontSize * pcnt);
        }

        private void c1Grid_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            if (e.Col > 0) //除了 CheckBox，其餘皆 Lock
            {
                e.CellStyle.Locked = true;
            }
        }

        private void ApplyEditorSetting()
        {
            editor.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editor.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
            editor.Zoom = Convert.ToInt16(MyLibrary.QueryEditorZoom);

            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;
            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editor.Styler = new SqlStyler();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            editor.Text = string.Empty;

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            GenerateSql_Oracle();
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            GenerateSql_PostgreSql();
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            GenerateSql_SqlServer();
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            GenerateSql_MySql();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void GenerateSql_Oracle()
        {
            var aliasName = string.Empty;

            Cursor = Cursors.WaitCursor;

            if (txtAliasName.Enabled && !string.IsNullOrEmpty(txtAliasName.Text))
            {
                if (rdoUpperAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToUpper();
                }
                else if (rdoLowerAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToLower();
                }
                else
                {
                    aliasName = txtAliasName.Text;
                }
            }

            try
            {
                var text = string.Empty;
                var isKeywordsToLower = rdoLowerKeywords.Checked;
                var pk2 = string.IsNullOrWhiteSpace(PrimaryKey) ? string.Empty : $"\r\n--Primary Key{PrimaryKey}`";
                var alterTable = TextHelper.TransferWordCase("ALTER TABLE ", isKeywordsToLower);
                var dropTable = TextHelper.TransferWordCase("DROP TABLE ", isKeywordsToLower);
                var truncateTable = TextHelper.TransferWordCase("TRUNCATE TABLE ", isKeywordsToLower);
                var add = TextHelper.TransferWordCase("ADD ", isKeywordsToLower);
                var varchar2 = TextHelper.TransferWordCase("VARCHAR2", isKeywordsToLower);
                var intString = TextHelper.TransferWordCase("INT", isKeywordsToLower);
                var modify = TextHelper.TransferWordCase("MODIFY ", isKeywordsToLower);
                var nullString = TextHelper.TransferWordCase("NOT NULL", isKeywordsToLower);
                var dropColumn = TextHelper.TransferWordCase("DROP COLUMN ", isKeywordsToLower);
                var renameColumn = TextHelper.TransferWordCase("RENAME COLUMN ", isKeywordsToLower);
                var renameTo = TextHelper.TransferWordCase("RENAME TO ", isKeywordsToLower);
                var toString = TextHelper.TransferWordCase("TO ", isKeywordsToLower);

                switch (grpFunction.AccessibleDescription?.ToString())
                {
                    case "rdoAlter":
                        {
                            var temp01 = $"--add a column\r\n{alterTable}{SchemaName}\r\n  {add}New_Column_Name {varchar2}(50);\r\n\r\n";
                            var temp02 = $"--add multiple columns\r\n{alterTable}{SchemaName}\r\n  {add}(New_Column_Name1 {intString},\r\n       New_Column_Name2 {varchar2}(100));\r\n\r\n";
                            var temp03 = $"--modify a column\r\n{alterTable}{SchemaName}\r\n  {modify}Old_Column_Name {intString};\r\n\r\n";
                            var temp04 = $"--modify multiple columns\r\n{alterTable}{SchemaName}\r\n  {modify}(Old_Column_Name1 {varchar2}(100) {nullString},\r\n          Old_Column_Name2 {varchar2}(75));";

                            text = $"{temp01}{temp02}{temp03}{temp04}";
                            break;
                        }
                    case "rdoCreate":
                        {
                            text = DatabaseSqlExecutor.GetCreateScript_Oracle(SchemaType, SchemaName.ToUpper());
                            break;
                        }
                    case "rdoDrop":
                        {
                            text = $"--drop a column\r\n{alterTable}{SchemaName}\r\n  {dropColumn}Old_Column_Name;\r\n\r\n--drop a table\r\n{dropTable}{SchemaName};";
                            break;
                        }
                    case "rdoRename":
                        {
                            text = $"--rename a column\r\n{alterTable}{SchemaName}\r\n  {renameColumn}Old_Column_Name {toString}New_Column_Name;\r\n\r\n--rename a table\r\n{alterTable}{SchemaName}\r\n  {renameTo}New_Table_Name;";
                            break;
                        }
                    case "rdoTruncate":
                        {
                            text = $"{truncateTable}{SchemaName};";
                            break;
                        }
                    case "rdoSelectStar":
                    case "rdoSelect":
                    case "rdoInsert":
                    case "rdoDelete":
                    case "rdoUpdate":
                        {
                            var schema = string.Empty;
                            var hasClobColumn = false;
                            var hasClobColumnString = string.Empty;
                            var columnInformation = string.Empty;
                            var columnValue = string.Empty;
                            var selectedColumnName = string.Empty;
                            var nullValue = TextHelper.TransferWordCase("NULL", isKeywordsToLower);
                            int j = 0;
                            var countSelected = 0;

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                if (c1Grid[i, " "].ToString() == "1")
                                {
                                    countSelected++;
                                }
                            }

                            if (countSelected == 0)
                            {
                                c1Grid[0, " "] = "1";
                            }

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                var columnName = c1Grid[i, "Column_Name"].ToString();

                                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                                {
                                    continue;
                                }

                                var isNullable = columnInfo.IsNullable;
                                var dataType = columnInfo.BaseDataType;

                                if (chkPKInfo.Checked && columnInfo.IsPrimaryKey)
                                {
                                    var aliasNameTemp = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                    schema = $", {columnInfo.FullDataType}";
                                    pk2 = pk2.Replace($"`{columnName}`", $"`{aliasNameTemp}{columnName}{schema}`");
                                }

                                if (c1Grid[i, " "].ToString() != "1")
                                {
                                    continue;
                                }

                                schema = chkColumnTypeInfo.Checked ? $" --{columnInfo.FullDataType}" : string.Empty;

                                string temp;

                                switch (dataType)
                                {
                                    case "INTEGER":
                                    case "NUMBER":
                                    case "FLOAT":
                                        {
                                            temp = isNullable ? nullValue : "0";
                                            break;
                                        }
                                    case "DATE":
                                        {
                                            temp = TextHelper.TransferWordCase($"TIMESTAMP '{DateTime.Now:yyyy-MM-dd HH:mm:ss}'", isKeywordsToLower);
                                            break;
                                        }
                                    case "TIMESTAMP":
                                        {
                                            var msFormat = string.Empty;

                                            if (columnInfo.NumericScale > 0)
                                            {
                                                msFormat = "." + new string('f', Math.Min(7, columnInfo.NumericScale));
                                            }

                                            var fullFormat = $"yyyy-MM-dd HH:mm:ss{msFormat}";

                                            temp = TextHelper.TransferWordCase($"TIMESTAMP '{DateTime.Now.ToString(fullFormat)}'", isKeywordsToLower);
                                            break;
                                        }
                                    case "TIMESTAMP WITH TIME ZONE":
                                    case "TIMESTAMP WITH LOCAL TIME ZONE":
                                        {
                                            var msFormat = string.Empty;

                                            if (columnInfo.NumericScale > 0)
                                            {
                                                msFormat = "." + new string('f', Math.Min(7, columnInfo.NumericScale));
                                            }

                                            var fullFormat = $"yyyy-MM-dd HH:mm:ss{msFormat} zzz";

                                            temp = TextHelper.TransferWordCase($"TIMESTAMP '{DateTime.Now.ToString(fullFormat)}'", isKeywordsToLower);
                                            break;
                                        }
                                    case "CLOB":
                                    case "NCLOB":
                                        {
                                            if (rdoDelete.Checked)
                                            {
                                                temp = "''";
                                            }
                                            else
                                            {
                                                temp = TextHelper.TransferWordCase("TO_CLOB('')", isKeywordsToLower);
                                            }

                                            hasClobColumn = true;
                                            break;
                                        }
                                    default:
                                        {
                                            temp = isNullable ? nullValue : "''";
                                            break;
                                        }
                                }

                                temp = grpFunction.AccessibleDescription == "rdoInsert" && chkDisplayAsParameter.Checked ? $":{columnName}" : temp;

                                var aliasNamePoint = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                if (j < Convert.ToInt16(cboNumbers.Text == "All" ? "9999" : cboNumbers.Text) - 1)
                                {
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", ";

                                    j++;
                                    selectedColumnName += $"{aliasNamePoint}{columnName}, ";
                                    columnValue += $"{temp}{comma}";
                                }
                                else
                                {
                                    var insert = rdoInsert.Checked ? " " : string.Empty;
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", \r\n        ";

                                    j = 0;
                                    selectedColumnName += $"{aliasNamePoint}{columnName},  \r\n       {insert}";
                                    columnValue += $"{temp}{comma}";
                                }

                                var temp1 = i == 0 ? string.Empty : $"   {MyGlobal.Separator3} ";
                                var temp2 = i == c1Grid.RowCount - 1 ? string.Empty : MyGlobal.Separator;

                                if (rdoDelete.Checked && string.Equals(dataType, "CLOB", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnInformation = $"{columnInformation}{temp1}DBMS_LOB.SUBSTR({columnName}, 4000, 1) = {temp}{temp2}{schema}\r\n";
                                }
                                else
                                {
                                    columnInformation = $"{columnInformation}{temp1}{columnName} = {temp}{temp2}{schema}\r\n";
                                }

                                if (string.Equals(dataType, "CLOB", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasClobColumnString += $"\r\n   --AND DBMS_LOB.SUBSTR({columnName}, 4000, 1 ) = '';";
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(pk2))
                            {
                                pk2 = pk2.Replace("`", "\r\n--").TrimEnd('-', '\r', '\n');
                            }

                            columnInformation = columnInformation.Trim();
                            columnValue = columnValue.Trim(' ', ',');

                            var pkContent = chkPKInfo.Checked ? pk2 : string.Empty;
                            var whereContent = TextHelper.TransferWordCase("WHERE ", isKeywordsToLower);

                            if (rdoDelete.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("DELETE FROM ", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, TextHelper.TransferWordCase("AND", isKeywordsToLower)).Replace(MyGlobal.Separator, string.Empty);
                                var temp05 = $"{temp03}{pkContent}";

                                text = $"{temp01}{SchemaName}\r\n {whereContent}{temp05}";
                            }
                            else if (rdoSelect.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("SELECT ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("FROM ", isKeywordsToLower);
                                var temp04 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp06 = TextHelper.TransferWordCase("\r\n   AND ROWNUM <= 10", isKeywordsToLower);
                                var temp07 = chkLimitInfo.Checked ? temp06 : string.Empty;
                                var temp08 = hasClobColumn ? hasClobColumnString : string.Empty;

                                text = $"{temp01}{temp02}\r\n  {temp03}{SchemaName}{temp04}\r\n {whereContent}1 = 1{temp07};{temp08}{pkContent}";
                            }
                            else if (rdoInsert.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("INSERT INTO ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("VALUES (", isKeywordsToLower);

                                columnValue = columnValue.TrimEnd('\r', '\n', ',', ' ');
                                text = $"{temp01}{SchemaName}\r\n       ({temp02})\r\n{temp03}{columnValue});{pkContent}";

                                if (!string.IsNullOrEmpty(aliasName))
                                {
                                    text = text.Replace($"{aliasName}.", string.Empty);
                                }
                            }
                            else if (rdoUpdate.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("UPDATE ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("SET ", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, "   ").Replace(MyGlobal.Separator, ",");

                                text = $"{temp01}{SchemaName}\r\n   {temp02}{temp03}\r\n {whereContent}conditions;{pkContent}";
                            }
                            else if (rdoSelectStar.Checked)
                            {
                                var temp01 = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";
                                var temp02 = TextHelper.TransferWordCase($"SELECT {temp01}* FROM ", isKeywordsToLower);
                                var temp03 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp04 = TextHelper.TransferWordCase("\r\n   AND ROWNUM <= 10", isKeywordsToLower);
                                var temp05 = chkLimitInfo.Checked ? temp04 : string.Empty;

                                text = $"{temp02}{SchemaName}{temp03}\r\n {whereContent}1 = 1{temp05};{pkContent}";
                            }

                            break;
                        }
                }

                if (rdoUpperAll.Checked)
                {
                    text = text.ToUpper().Replace(" \r\n", "\r\n");
                }
                else if (rdoLowerAll.Checked)
                {
                    text = text.ToLower().Replace(" \r\n", "\r\n");
                }

                UpdateEditorText(text);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GenerateSql_PostgreSql()
        {
            var aliasName = string.Empty;

            Cursor = Cursors.WaitCursor;

            if (txtAliasName.Enabled && !string.IsNullOrEmpty(txtAliasName.Text))
            {
                if (rdoUpperAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToUpper();
                }
                else if (rdoLowerAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToLower();
                }
                else
                {
                    aliasName = txtAliasName.Text;
                }
            }

            try
            {
                var text = string.Empty;
                var isKeywordsToLower = rdoLowerKeywords.Checked;
                var pk2 = string.IsNullOrWhiteSpace(PrimaryKey) ? string.Empty : $"\r\n--Primary Key{PrimaryKey}`";
                var alterTable = TextHelper.TransferWordCase("ALTER TABLE ", isKeywordsToLower);
                var dropTable = TextHelper.TransferWordCase("DROP TABLE ", isKeywordsToLower);
                var truncateTable = TextHelper.TransferWordCase("TRUNCATE TABLE ", isKeywordsToLower);
                var add = TextHelper.TransferWordCase("ADD ", isKeywordsToLower);
                var varchar2 = TextHelper.TransferWordCase("VARCHAR2", isKeywordsToLower);
                var intString = TextHelper.TransferWordCase("INT", isKeywordsToLower);
                var modify = TextHelper.TransferWordCase("MODIFY ", isKeywordsToLower);
                var nullString = TextHelper.TransferWordCase("NOT NULL", isKeywordsToLower);
                var dropColumn = TextHelper.TransferWordCase("DROP COLUMN ", isKeywordsToLower);
                var renameColumn = TextHelper.TransferWordCase("RENAME COLUMN ", isKeywordsToLower);
                var renameTo = TextHelper.TransferWordCase("RENAME TO ", isKeywordsToLower);
                var toString = TextHelper.TransferWordCase("TO ", isKeywordsToLower);

                switch (grpFunction.AccessibleDescription?.ToString())
                {
                    case "rdoAlter":
                        {
                            var temp01 = $"--add a column\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {add}New_Column_Name {varchar2}(50);\r\n\r\n";
                            var temp02 = $"--add multiple columns\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {add}(New_Column_Name1 {intString},\r\n       New_Column_Name2 {varchar2}(100));\r\n\r\n";
                            var temp03 = $"--modify a column\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {modify}Old_Column_Name {intString};\r\n\r\n";
                            var temp04 = $"--modify multiple columns\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {modify}(Old_Column_Name1 {varchar2}(100) {nullString},\r\n          Old_Column_Name2 {varchar2}(75));";

                            text = $"{temp01}{temp02}{temp03}{temp04}";
                            break;
                        }
                    case "rdoCreate":
                        {
                            var script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(SchemaNode, SchemaType, SchemaName);

                            if (SchemaObjectTypeHelper.Is(SchemaType, SchemaObjectNames.Tables))
                            {
                                script = PostgreSqlCreateTableScriptBeautifier.Beautify(script);
                            }

                            text = script;
                            break;
                        }
                    case "rdoDrop":
                        {
                            text = $"--drop a column\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {dropColumn}Old_Column_Name;\r\n\r\n--drop a table\r\n{dropTable}{SchemaNode}.{SchemaName};";
                            break;
                        }
                    case "rdoRename":
                        {
                            text = $"--rename a column\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {renameColumn}Old_Column_Name {toString}New_Column_Name;\r\n\r\n--rename a table\r\n{alterTable}{SchemaNode}.{SchemaName}\r\n  {renameTo}New_Table_Name;";
                            break;
                        }
                    case "rdoTruncate":
                        {
                            text = $"{truncateTable}{SchemaNode}.{SchemaName};";
                            break;
                        }
                    case "rdoSelectStar":
                    case "rdoSelect":
                    case "rdoInsert":
                    case "rdoDelete":
                    case "rdoUpdate":
                        {
                            var schema = string.Empty;
                            var columnInformation = string.Empty;
                            var columnValue = string.Empty;
                            var selectedColumnName = string.Empty;
                            var nullValue = "null";
                            int j = 0;
                            var countSelected = 0;

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                if (c1Grid[i, " "].ToString() == "1")
                                {
                                    countSelected++;
                                }
                            }

                            if (countSelected == 0)
                            {
                                c1Grid[0, " "] = "1";
                            }

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                var columnName = c1Grid[i, "Column_Name"].ToString();

                                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                                {
                                    continue;
                                }

                                var isNullable = columnInfo.IsNullable;
                                var dataType = columnInfo.BaseDataType;

                                if (chkPKInfo.Checked && columnInfo.IsPrimaryKey)
                                {
                                    var aliasNameTemp = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                    schema = $", {columnInfo.FullDataType}";
                                    pk2 = pk2.Replace($"`{columnName}`", $"`{aliasNameTemp}{columnName}{schema}`");
                                }

                                if (c1Grid[i, " "].ToString() != "1")
                                {
                                    continue;
                                }

                                schema = chkColumnTypeInfo.Checked ? $" --{columnInfo.FullDataType}" : string.Empty;

                                string temp;

                                switch (dataType)
                                {
                                    case "bigint":
                                    case "bit":
                                    case "double precision":
                                    case "integer":
                                    case "numeric":
                                    case "money":
                                    case "real":
                                    case "serial":
                                    case "smallint":
                                        {
                                            temp = isNullable ? nullValue : "0";
                                            break;
                                        }
                                    case "boolean":
                                        {
                                            temp = isNullable ? nullValue : "false";
                                            break;
                                        }
                                    case "datetime":
                                        {
                                            temp = TextHelper.TransferWordCase($"TO_DATE('{MyGlobal.DateTimeNowWithDateFormat()}', '", isKeywordsToLower);
                                            temp += $"{MyLibrary.DateFormat} HH:mm:ss')";
                                            break;
                                        }
                                    default:
                                        {
                                            temp = isNullable ? nullValue : "''";
                                            break;
                                        }
                                }

                                temp = grpFunction.AccessibleDescription == "rdoInsert" && chkDisplayAsParameter.Checked ? $":{columnName}" : temp;

                                var aliasNamePoint = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                if (j < Convert.ToInt16(cboNumbers.Text == "All" ? "9999" : cboNumbers.Text) - 1)
                                {
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", ";

                                    j++;
                                    selectedColumnName += $"{aliasNamePoint}{columnName}, ";
                                    columnValue += $"{temp}{comma}";
                                }
                                else
                                {
                                    var insert = (rdoInsert.Checked ? " " : string.Empty);
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", \r\n        ";

                                    j = 0;
                                    selectedColumnName += $"{aliasNamePoint}{columnName}, \r\n       {insert}";
                                    columnValue += $"{temp}{comma}";
                                }

                                var temp1 = i == 0 ? string.Empty : $"   {MyGlobal.Separator3} ";
                                var temp2 = i == c1Grid.RowCount - 1 ? string.Empty : MyGlobal.Separator;

                                columnInformation = $"{columnInformation}{temp1}{columnName} = {temp}{temp2}{schema}\r\n";
                            }

                            if (!string.IsNullOrWhiteSpace(pk2))
                            {
                                pk2 = pk2.Replace("`", "\r\n--").TrimEnd('-', '\r', '\n');
                            }

                            columnInformation = columnInformation.Trim();
                            columnValue = columnValue.Trim(' ', ',');

                            var pkContent = chkPKInfo.Checked ? pk2 : string.Empty;
                            var whereContent = TextHelper.TransferWordCase("WHERE ", isKeywordsToLower);

                            if (rdoDelete.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("DELETE FROM ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("AND", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, temp02).Replace(MyGlobal.Separator, string.Empty);
                                var temp05 = $"{temp03};{pkContent}";

                                text = $"{temp01}{SchemaNode}.{SchemaName}\r\n {whereContent}{temp05}";
                            }
                            else if (rdoSelect.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("SELECT ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("FROM ", isKeywordsToLower);
                                var temp04 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp05 = TextHelper.TransferWordCase(" LIMIT 10", isKeywordsToLower);
                                var temp06 = chkLimitInfo.Checked ? temp05 : string.Empty;

                                text = $"{temp01}{temp02}\r\n  {temp03}{SchemaNode}.{SchemaName}{temp04}\r\n {whereContent}1 = 1{temp06};{pkContent}";
                            }
                            else if (rdoInsert.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("INSERT INTO ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("VALUES (", isKeywordsToLower);

                                columnValue = columnValue.TrimEnd('\r', '\n', ',', ' ');
                                text = $"{temp01}{SchemaNode}.{SchemaName}\r\n       ({temp02})\r\n{temp03}{columnValue});{pkContent}";

                                if (!string.IsNullOrEmpty(aliasName))
                                {
                                    text = text.Replace($"{aliasName}.", string.Empty);
                                }
                            }
                            else if (rdoUpdate.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("UPDATE ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("SET ", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, "   ").Replace(MyGlobal.Separator, ",");

                                text = $"{temp01}{SchemaNode}.{SchemaName}\r\n   {temp02}{temp03}\r\n {whereContent}conditions;{pkContent}";
                            }
                            else if (rdoSelectStar.Checked)
                            {
                                var temp01 = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";
                                var temp02 = TextHelper.TransferWordCase($"SELECT {temp01}* FROM ", isKeywordsToLower);
                                var temp03 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp04 = TextHelper.TransferWordCase(" LIMIT 10", isKeywordsToLower);
                                var temp05 = chkLimitInfo.Checked ? temp04 : string.Empty;

                                text = $"{temp02}{SchemaNode}.{SchemaName}{temp03}\r\n {whereContent}1 = 1{temp05};{pkContent}";
                            }

                            break;
                        }
                }

                if (rdoUpperAll.Checked)
                {
                    text = text.ToUpper().Replace(" \r\n", "\r\n");
                }
                else if (rdoLowerAll.Checked)
                {
                    text = text.ToLower().Replace(" \r\n", "\r\n");
                }

                UpdateEditorText(text);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GenerateSql_SqlServer()
        {
            var aliasName = string.Empty;

            Cursor = Cursors.WaitCursor;

            if (txtAliasName.Enabled && !string.IsNullOrEmpty(txtAliasName.Text))
            {
                if (rdoUpperAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToUpper();
                }
                else if (rdoLowerAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToLower();
                }
                else
                {
                    aliasName = txtAliasName.Text;
                }
            }

            try
            {
                var text = string.Empty;
                var isKeywordsToLower = rdoLowerKeywords.Checked;
                var pk2 = string.IsNullOrWhiteSpace(PrimaryKey) ? string.Empty : $"\r\n--Primary Key{PrimaryKey}`";
                var alterTable = TextHelper.TransferWordCase("ALTER TABLE ", isKeywordsToLower);
                var dropTable = TextHelper.TransferWordCase("DROP TABLE ", isKeywordsToLower);
                var truncateTable = TextHelper.TransferWordCase("TRUNCATE TABLE ", isKeywordsToLower);
                var add = TextHelper.TransferWordCase("ADD ", isKeywordsToLower);
                var varchar2 = TextHelper.TransferWordCase("VARCHAR", isKeywordsToLower);
                var exec = TextHelper.TransferWordCase("EXEC ", isKeywordsToLower);
                var alterColumn = TextHelper.TransferWordCase("ALTER COLUMN ", isKeywordsToLower);
                var dropColumn = TextHelper.TransferWordCase("DROP COLUMN ", isKeywordsToLower);
                var nullString = TextHelper.TransferWordCase("NOT NULL;", isKeywordsToLower);

                switch (grpFunction.AccessibleDescription?.ToString())
                {
                    case "rdoAlter":
                        {
                            var temp01 = $"--add a column\r\n{alterTable}{SchemaName}\r\n {add}New_Column_Name {varchar2}(50);\r\n\r\n";
                            var temp02 = $"--modify a column\r\n{alterTable}{SchemaName}\r\n  {alterColumn}Old_Column_Name {varchar2}(100) {nullString}";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoCreate":
                        {
                            text = DatabaseSqlExecutor.GetCreateScript_SqlServer(SchemaType, SchemaNode, SchemaDbo, SchemaName);
                            break;
                        }
                    case "rdoDrop":
                        {
                            var temp01 = $"--drop a column\r\n{alterTable}{SchemaName}\r\n  {dropColumn}Old_Column_Name;\r\n\r\n";
                            var temp02 = $"--drop a table\r\n{dropTable}{SchemaName};";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoRename":
                        {
                            var temp01 = $"--rename a column\r\n{exec}sp_rename '{SchemaName}.Old_Column_Name', 'New_Column_Name', 'COLUMN';\r\n\r\n";
                            var temp02 = $"--rename a table\r\n{exec}sp_rename '{SchemaName}', 'New_Table_Name';";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoTruncate":
                        {
                            text = $"{truncateTable}{SchemaName};";
                            break;
                        }
                    case "rdoSelectStar":
                    case "rdoSelect":
                    case "rdoInsert":
                    case "rdoDelete":
                    case "rdoUpdate":
                        {
                            var schema = string.Empty;
                            var columnInformation = string.Empty;
                            var columnValue = string.Empty;
                            var selectedColumnName = string.Empty;
                            var nullValue = TextHelper.TransferWordCase("NULL", isKeywordsToLower);
                            var j = 0;
                            var countSelected = 0;

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                if (c1Grid[i, " "].ToString() == "1")
                                {
                                    countSelected++;
                                }
                            }

                            if (countSelected == 0)
                            {
                                c1Grid[0, " "] = "1";
                            }

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                if (c1Grid[i, " "].ToString() != "1")
                                {
                                    countSelected++;
                                }
                            }

                            if (countSelected == 0)
                            {
                                c1Grid[0, " "] = "1";
                            }

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                var columnName = c1Grid[i, "Column_Name"].ToString();

                                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                                {
                                    continue;
                                }

                                var isNullable = columnInfo.IsNullable;
                                var dataType = columnInfo.BaseDataType;

                                if (chkPKInfo.Checked && $"{PrimaryKey}`".IndexOf($"`{columnName}`", StringComparison.Ordinal) >= 0)
                                {
                                    var aliasNameTemp = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                    schema = $", {columnInfo.FullDataType}";
                                    pk2 = pk2.Replace($"`{columnName}`", $"`{aliasNameTemp}{columnName}{schema}`");
                                }

                                if (c1Grid[i, " "].ToString() != "1")
                                {
                                    continue;
                                }

                                schema = chkColumnTypeInfo.Checked ? $" --{columnInfo.FullDataType}" : string.Empty;

                                string temp;

                                switch (dataType)
                                {
                                    case "int":
                                    case "number":
                                    case "bigint":
                                    case "integer":
                                    case "decimal":
                                    case "numeric":
                                    case "long":
                                    case "float":
                                    case "smallint":
                                    case "tinyint":
                                        {
                                            temp = isNullable ? nullValue : "0";
                                            break;
                                        }
                                    case "time":
                                    case "date":
                                        {
                                            temp = TextHelper.TransferWordCase("GETDATE()", isKeywordsToLower);
                                            break;
                                        }
                                    case "timestamp":
                                        {
                                            temp = TextHelper.TransferWordCase("DEFAULT", isKeywordsToLower); //YYYY-MM-DD HH:mm:ss
                                            break;
                                        }
                                    case "datetime": //YYYY-MM-DD HH:mm:ss.fff 精確度到後 3 位
                                        {
                                            temp = TextHelper.TransferWordCase("GETDATE()", isKeywordsToLower);
                                            break;
                                        }
                                    case "datetime2": //YYYY-MM-DD HH:mm:ss.fffffff 精確度到後 7 位
                                        {
                                            temp = TextHelper.TransferWordCase("SYSDATETIME()", isKeywordsToLower);
                                            break;
                                        }
                                    case "datetimeoffset": //YYYY-MM-DD HH:mm:ss.fffffff 精確度到後 7 位再加上時區
                                        {
                                            temp = TextHelper.TransferWordCase("SYSDATETIMEOFFSET()", isKeywordsToLower);
                                            break;
                                        }
                                    case "uniqueidentifier":
                                        {
                                            temp = grpFunction.AccessibleDescription == "rdoInsert" ? "NEWID()" : "N''";
                                            break;
                                        }
                                    case "nchar":
                                    case "nvarchar":
                                    case "ntext":
                                        {
                                            temp = isNullable ? nullValue : "N''";
                                            break;
                                        }
                                    default:
                                        {
                                            temp = isNullable ? nullValue : "''";
                                            break;
                                        }
                                }

                                var temp01 = temp == "N''" ? "N:" : ":";

                                temp = grpFunction.AccessibleDescription == "rdoInsert" && chkDisplayAsParameter.Checked ? $"{temp01}{columnName}" : temp;

                                var aliasNamePoint = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";
                                var encloseBracketsStart = chkEncloseBrackets.Checked ? "[" : string.Empty;
                                var encloseBracketsEnd = chkEncloseBrackets.Checked ? "]" : string.Empty;

                                if (j < Convert.ToInt16(cboNumbers.Text == "All" ? "9999" : cboNumbers.Text) - 1)
                                {
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", ";

                                    j++;
                                    selectedColumnName += $"{aliasNamePoint}{encloseBracketsStart}{columnName}{encloseBracketsEnd}, ";
                                    columnValue += $"{temp}{comma}";
                                }
                                else
                                {
                                    var insert = rdoInsert.Checked ? " " : string.Empty;
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", \r\n        ";

                                    j = 0;
                                    selectedColumnName += $"{aliasNamePoint}{encloseBracketsStart}{columnName}{encloseBracketsEnd}, \r\n       {insert}";
                                    columnValue += $"{temp}{comma}";
                                }

                                var temp0 = i == 0 ? string.Empty : $"   {MyGlobal.Separator3} ";
                                var temp1 = i == c1Grid.RowCount - 1 ? string.Empty : MyGlobal.Separator;

                                columnInformation = $"{columnInformation}{temp0}{encloseBracketsStart}{columnName}{encloseBracketsEnd} = {temp}{temp1}{schema}\r\n";
                            }

                            if (!string.IsNullOrWhiteSpace(pk2))
                            {
                                pk2 = pk2.Replace("`", "\r\n--").TrimEnd('-', '\r', '\n');
                            }

                            columnInformation = columnInformation.Trim();
                            columnValue = columnValue.Trim(' ', ',');

                            var pkContent = chkPKInfo.Checked ? pk2 : string.Empty;
                            var sWhereContent = TextHelper.TransferWordCase("WHERE ", isKeywordsToLower);

                            if (rdoDelete.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("DELETE FROM ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("AND", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, temp02).Replace(MyGlobal.Separator, string.Empty);
                                var temp04 = $"{temp03}{pkContent}";

                                text = $"{temp01}{SchemaName}\r\n {sWhereContent}{temp04}";
                            }
                            else if (rdoSelect.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("SELECT ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("TOP (100) ", isKeywordsToLower);
                                var temp03 = chkLimitInfo.Checked ? temp02 : string.Empty;
                                var temp04 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp05 = TextHelper.TransferWordCase("FROM ", isKeywordsToLower);
                                var temp06 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";

                                text = $"{temp01}{temp03}{temp04}\r\n  {temp05}{SchemaName}{temp06}\r\n {sWhereContent}1 = 1;{pkContent}";
                            }
                            else if (rdoInsert.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("INSERT INTO ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("VALUES (", isKeywordsToLower);

                                columnValue = columnValue.TrimEnd('\r', '\n', ',', ' ');
                                text = $"{temp01}{SchemaName}\r\n       ({temp02})\r\n{temp03}{columnValue});{pkContent}";

                                if (!string.IsNullOrEmpty(aliasName))
                                {
                                    text = text.Replace($"{aliasName}.", string.Empty);
                                }
                            }
                            else if (rdoUpdate.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("UPDATE ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("SET ", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, "   ").Replace(MyGlobal.Separator, ",");

                                text = $"{temp01}{SchemaName}\r\n   {temp02}{temp03}\r\n {sWhereContent}conditions;{pkContent}";
                            }
                            else if (rdoSelectStar.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("SELECT ", isKeywordsToLower);
                                var temp02 = chkLimitInfo.Checked ? TextHelper.TransferWordCase("TOP (100) ", isKeywordsToLower) : string.Empty;
                                var temp03 = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";
                                var temp04 = TextHelper.TransferWordCase($"{temp03}* FROM ", isKeywordsToLower);
                                var temp05 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";

                                text = $"{temp01}{temp02}{temp04}{SchemaName}{temp05}\r\n {sWhereContent}1 = 1;{pkContent}";
                            }

                            break;
                        }
                }

                if (rdoUpperAll.Checked)
                {
                    text = text.ToUpper().Replace(" \r\n", "\r\n");
                }
                else if (rdoLowerAll.Checked)
                {
                    text = text.ToLower().Replace(" \r\n", "\r\n");
                }

                UpdateEditorText(text);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GenerateSql_MySql()
        {
            var aliasName = string.Empty;

            Cursor = Cursors.WaitCursor;

            if (txtAliasName.Enabled && !string.IsNullOrEmpty(txtAliasName.Text))
            {
                if (rdoUpperAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToUpper();
                }
                else if (rdoLowerAll.Checked)
                {
                    aliasName = txtAliasName.Text.ToLower();
                }
                else
                {
                    aliasName = txtAliasName.Text;
                }
            }

            try
            {
                var text = string.Empty;
                var isKeywordsToLower = rdoLowerKeywords.Checked;
                var pk2 = string.IsNullOrWhiteSpace(PrimaryKey) ? string.Empty : $"\r\n--Primary Key{PrimaryKey}`"; var alterTable = TextHelper.TransferWordCase("ALTER TABLE ", isKeywordsToLower);
                var dropTable = TextHelper.TransferWordCase("DROP TABLE ", isKeywordsToLower);
                var addColumn = TextHelper.TransferWordCase("ADD COLUMN ", isKeywordsToLower);
                var changeColumn = TextHelper.TransferWordCase("CHANGE COLUMN ", isKeywordsToLower);
                var dropColumn = TextHelper.TransferWordCase("DROP COLUMN ", isKeywordsToLower);
                var varchar = TextHelper.TransferWordCase("VARCHAR", isKeywordsToLower);
                var intString = TextHelper.TransferWordCase("INT", isKeywordsToLower);
                var modify = TextHelper.TransferWordCase("MODIFY ", isKeywordsToLower);
                var nullString = TextHelper.TransferWordCase("NULL", isKeywordsToLower);
                var renameTo = TextHelper.TransferWordCase("RENAME TO ", isKeywordsToLower);
                var truncate = TextHelper.TransferWordCase("TRUNCATE ", isKeywordsToLower);

                switch (grpFunction.AccessibleDescription?.ToString())
                {
                    case "rdoAlter":
                        {
                            var temp01 = $"--add a column\r\n{alterTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName}\r\n  {addColumn}`New_Column_Name` {varchar}2(50) {nullString};\r\n\r\n";
                            var temp02 = $"--modify a column\r\n{alterTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName}\r\n  {changeColumn}`Old_Column_Name` `Old_Column_Name` {intString};";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoCreate":
                        {
                            text = DatabaseSqlExecutor.GetCreateScript_MySql(SchemaType, SchemaNode, SchemaName);
                            break;
                        }
                    case "rdoDrop":
                        {
                            var temp01 = $"--drop a column\r\n{alterTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName}\r\n  {dropColumn}`Old_Column_Name`;\r\n\r\n";
                            var temp02 = $"--drop a table\r\n{dropTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName};";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoRename":
                        {
                            var temp01 = $"--rename a column\r\n{alterTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName}\r\n  {changeColumn}`Old_Column_Name` `New_Column_Name` {varchar}(50);\r\n\r\n";
                            var temp02 = $"--rename a table\r\n{alterTable}{DatabaseSqlExecutor.DatabaseName}.{SchemaName}\r\n  {renameTo}`New_Table_Name`;";

                            text = $"{temp01}{temp02}";
                            break;
                        }
                    case "rdoTruncate":
                        {
                            text = $"{truncate}{DatabaseSqlExecutor.DatabaseName}.{SchemaName};";
                            break;
                        }
                    case "rdoSelectStar":
                    case "rdoSelect":
                    case "rdoInsert":
                    case "rdoDelete":
                    case "rdoUpdate":
                        {
                            var schema = string.Empty;
                            var columnInformation = string.Empty;
                            var columnValue = string.Empty;
                            var selectedColumnName = string.Empty;
                            var nullValue = TextHelper.TransferWordCase("NULL", isKeywordsToLower);
                            var j = 0;
                            var countSelected = 0;

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                if (c1Grid[i, " "].ToString() == "1")
                                {
                                    countSelected++;
                                }
                            }

                            if (countSelected == 0)
                            {
                                c1Grid[0, " "] = "1";
                            }

                            for (var i = 0; i < c1Grid.RowCount; i++)
                            {
                                var columnName = c1Grid[i, "Column_Name"].ToString();

                                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                                {
                                    continue;
                                }

                                var isNullable = columnInfo.IsNullable;
                                var dataType = columnInfo.BaseDataType;

                                if (chkPKInfo.Checked && $"{PrimaryKey}`".IndexOf($"`{columnName}`", StringComparison.Ordinal) >= 0)
                                {
                                    var aliasNameTemp = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                    schema = $", {columnInfo.FullDataType}";
                                    pk2 = pk2.Replace($"`{columnName}`", $"`{aliasNameTemp}{columnName}{schema}`");
                                }

                                if (c1Grid[i, " "].ToString() != "1")
                                {
                                    continue;
                                }

                                schema = chkColumnTypeInfo.Checked ? $" --{columnInfo.FullDataType}" : string.Empty;

                                string temp;

                                switch (dataType)
                                {
                                    case "int":
                                    case "number":
                                    case "bigint":
                                    case "integer":
                                    case "decimal":
                                    case "numeric":
                                    case "long":
                                    case "float":
                                    case "smallint":
                                    case "tinyint":
                                        {
                                            temp = isNullable ? nullValue : "0";
                                            break;
                                        }
                                    case "time":
                                    case "timestamp":
                                    case "date":
                                    case "datetime":
                                    case "datetime2":
                                    case "datetimeoffset":
                                        {
                                            temp = "CONVERT(CHAR(19), GETDATE(), 120)"; //YYYY-MM-DD HH:mm:ss
                                            break;
                                        }
                                    default:
                                        {
                                            temp = isNullable ? nullValue : "''";
                                            break;
                                        }
                                }

                                temp = grpFunction.AccessibleDescription == "rdoInsert" && chkDisplayAsParameter.Checked ? $":{columnName}" : temp;

                                var aliasNamePoint = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";

                                if (j < Convert.ToInt16(cboNumbers.Text == "All" ? "9999" : cboNumbers.Text) - 1)
                                {
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", ";

                                    j++;
                                    selectedColumnName += $"{aliasNamePoint}{columnName}, ";
                                    columnValue += $"{temp}{comma}";
                                }
                                else
                                {
                                    var insert = rdoInsert.Checked ? " " : string.Empty;
                                    var comma = i == c1Grid.RowCount - 1 ? "," : ", \r\n        ";

                                    j = 0;
                                    selectedColumnName += $"{aliasNamePoint}{columnName}, \r\n       {insert}";
                                    columnValue += $"{temp}{comma}";
                                }

                                var temp1 = i == 0 ? string.Empty : $"   {MyGlobal.Separator3} ";
                                var temp2 = i == c1Grid.RowCount - 1 ? string.Empty : MyGlobal.Separator;

                                columnInformation = $"{columnInformation}{temp1}{columnName} = {temp}{temp2}{schema}\r\n";
                            }

                            if (!string.IsNullOrWhiteSpace(pk2))
                            {
                                pk2 = pk2.Replace("`", "\r\n--").TrimEnd('-', '\r', '\n');
                            }

                            columnInformation = columnInformation.Trim();
                            columnValue = columnValue.Trim(' ', ',');

                            var pkContent = chkPKInfo.Checked ? pk2 : string.Empty;
                            var whereContent = TextHelper.TransferWordCase("WHERE ", isKeywordsToLower);

                            if (rdoDelete.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("DELETE FROM ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("AND", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, temp02).Replace(MyGlobal.Separator, string.Empty);
                                var temp04 = $"{temp03}{pkContent}";

                                text = $"{temp01}{SchemaName}\r\n {whereContent}{temp04}";
                            }
                            else if (rdoSelect.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("SELECT ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("FROM ", isKeywordsToLower);
                                var temp04 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp05 = TextHelper.TransferWordCase(" LIMIT 10", isKeywordsToLower);
                                var temp06 = chkLimitInfo.Checked ? temp05 : string.Empty;

                                text = $"{temp01}{temp02}\r\n  {temp03}{SchemaName}{temp04}\r\n {whereContent}1 = 1{temp06};{pkContent}";
                            }
                            else if (rdoInsert.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("INSERT INTO ", isKeywordsToLower);
                                var temp02 = selectedColumnName.Trim().Substring(0, selectedColumnName.Trim().Length - 1);
                                var temp03 = TextHelper.TransferWordCase("VALUES (", isKeywordsToLower);

                                columnValue = columnValue.TrimEnd('\r', '\n', ',', ' ');
                                text = $"{temp01}{SchemaName}\r\n       ({temp02})\r\n{temp03}{columnValue});{pkContent}";

                                if (!string.IsNullOrEmpty(aliasName))
                                {
                                    text = text.Replace($"{aliasName}.", string.Empty);
                                }
                            }
                            else if (rdoUpdate.Checked)
                            {
                                var temp01 = TextHelper.TransferWordCase("UPDATE ", isKeywordsToLower);
                                var temp02 = TextHelper.TransferWordCase("SET ", isKeywordsToLower);
                                var temp03 = columnInformation.Replace(MyGlobal.Separator3, "   ").Replace(MyGlobal.Separator, ",");

                                text = $"{temp01}{SchemaName}\r\n   {temp02}{temp03}\r\n {whereContent}conditions;{pkContent}";
                            }
                            else if (rdoSelectStar.Checked)
                            {
                                var temp01 = string.IsNullOrEmpty(aliasName) ? string.Empty : $"{aliasName}.";
                                var temp02 = TextHelper.TransferWordCase($"SELECT {temp01}* FROM ", isKeywordsToLower);
                                var temp03 = string.IsNullOrEmpty(aliasName) ? string.Empty : $" {aliasName}";
                                var temp04 = TextHelper.TransferWordCase(" LIMIT 10", isKeywordsToLower);
                                var temp05 = chkLimitInfo.Checked ? temp04 : string.Empty;

                                text = $"{temp02}{SchemaName}{temp03}\r\n {whereContent}1 = 1{temp05};{pkContent}";
                            }

                            break;
                        }
                }

                if (rdoUpperAll.Checked)
                {
                    text = text.ToUpper().Replace(" \r\n", "\r\n");
                }
                else if (rdoLowerAll.Checked)
                {
                    text = text.ToLower().Replace(" \r\n", "\r\n");
                }

                UpdateEditorText(text);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateEditorText(string sText)
        {
            editor.ReadOnly = false;
            editor.Text = sText;
            editor.ReadOnly = true;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var sValue = sender is C1.Win.C1Input.C1Button btn && TextHelper.GetSafeString(btn.Tag) == "SelectAll" ? "1" : "0";
            var iCurrentRow = c1Grid.Row;
            var iCurrentCol = c1Grid.Col;

            try
            {
                var iCount = c1Grid.RowCount - 1;

                for (var j = iCount; j >= 0; j--)
                {
                    c1Grid[j, 0] = sValue;
                }

                c1Grid.Row = iCurrentRow;
                c1Grid.Col = iCurrentCol;
                c1Grid.Select(); //Focus 切換到指定的 Cell
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Function_CheckedChanged(object sender, EventArgs e)
        {
            var bNumbersValue = false;
            var bColumnTypeInfoValue = false;
            var bPKValue = false;
            var bLimitValue = false;
            var bAliasNameValue = false;
            var bBracketsValue = false;
            var bKeywordsValue = true;
            var bDisplayAsParameterValue = false;
            var rdo = sender as RadioButton;

            if (!rdo.Checked)
            {
                return; //避免執行兩次 (第一次會是「前一次」的選項，且為「未核取」，忽略它！)
            }

            grpFunction.AccessibleDescription = rdo.Name;

            rdoDoNothing.Visible = false;

            switch (rdo.Name)
            {
                case "rdoSelect":
                    {
                        bNumbersValue = true;
                        bPKValue = !string.IsNullOrEmpty(PrimaryKey);
                        bLimitValue = true;
                        bAliasNameValue = true;
                        bBracketsValue = IsSqlServer || IsMySql;
                        break;
                    }
                case "rdoInsert":
                    {
                        bNumbersValue = true;
                        bPKValue = !string.IsNullOrEmpty(PrimaryKey);
                        bDisplayAsParameterValue = true;
                        bBracketsValue = IsSqlServer || IsMySql;
                        break;
                    }
                case "rdoDelete":
                case "rdoUpdate":
                    {
                        bColumnTypeInfoValue = true;
                        bPKValue = !string.IsNullOrEmpty(PrimaryKey);
                        bBracketsValue = IsSqlServer || IsMySql;
                        break;
                    }
                case "rdoSelectStar":
                    {
                        bPKValue = !string.IsNullOrEmpty(PrimaryKey);
                        bLimitValue = true;
                        bAliasNameValue = true;
                        break;
                    }
                case "rdoCreate":
                    {
                        rdoUpperAll.CheckedChanged -= ChangeCase_CheckedChanged;
                        rdoUpperAll.Checked = true;
                        rdoUpperAll.CheckedChanged += ChangeCase_CheckedChanged;

                        bKeywordsValue = false;
                        rdoDoNothing.Visible = true;

                        rdoDoNothing.CheckedChanged -= ChangeCase_CheckedChanged;
                        rdoDoNothing.Checked = true;
                        rdoDoNothing.CheckedChanged += ChangeCase_CheckedChanged;

                        break;
                    }
            }

            if (rdo.Name != "rdoCreate" && rdoDoNothing.Checked)
            {
                rdoUpperAll.CheckedChanged -= ChangeCase_CheckedChanged;
                rdoUpperAll.Checked = true;
                rdoUpperAll.CheckedChanged += ChangeCase_CheckedChanged;
            }

            lblNumbers.Enabled = bNumbersValue;
            cboNumbers.Enabled = bNumbersValue;
            chkColumnTypeInfo.Enabled = bColumnTypeInfoValue;
            chkPKInfo.Enabled = bPKValue;
            chkLimitInfo.Enabled = bLimitValue;
            chkAliasName.Enabled = bAliasNameValue;
            txtAliasName.Enabled = bAliasNameValue;
            chkEncloseBrackets.Enabled = bBracketsValue;
            chkEncloseGraveAccent.Enabled = bBracketsValue;
            rdoUpperKeywords.Enabled = bKeywordsValue;
            rdoLowerKeywords.Enabled = bKeywordsValue;
            chkDisplayAsParameter.Visible = bDisplayAsParameterValue;

            btnPreview.PerformClick();
        }

        private static DataTable CreateColumnTable(DataTable dtColumnSource, string value)
        {
            var dtColumn = new DataTable();

            dtColumn.Columns.Add(" ");
            dtColumn.Columns.Add("Column_Name");
            dtColumn.Columns.Add("Column_ID", typeof(int));
            dtColumn.Columns.Add("TypeName");
            dtColumn.Columns.Add("DataTypeName");
            dtColumn.Columns.Add("DataType");
            dtColumn.Columns.Add("ColumnSize");
            dtColumn.Columns.Add("NumericScale");
            dtColumn.Columns.Add("NumericPrecision");

            if (dtColumnSource == null)
            {
                return dtColumn;
            }

            foreach (DataRow dr in dtColumnSource?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var row = dtColumn.NewRow();

                row[" "] = string.IsNullOrEmpty(value) ? dr.GetSafeString(" ") : value;
                row["Column_Name"] = dr.GetSafeString("Column_Name");
                row["Column_ID"] = dr.GetSafeInt("Column_ID");
                row["TypeName"] = dr.GetSafeString("TypeName");
                row["DataTypeName"] = dr.GetSafeString("DataTypeName");
                row["DataType"] = dr.GetSafeString("DataType");
                row["ColumnSize"] = dr.GetSafeString("ColumnSize");
                row["NumericScale"] = dr.GetSafeString("NumericScale");
                row["NumericPrecision"] = dr.GetSafeString("NumericPrecision");

                dtColumn.Rows.Add(row);
            }

            return dtColumn;
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
            Clipboard.SetDataObject(editor.Text, false);

            Close();
        }

        private void btnPasteToQueryEditor_Click(object sender, EventArgs e)
        {
            btnPreview.PerformClick();

            //20230923 改用 "PasteFromSchemaBrowser"，直接複製到新的 SQL Editor，避免干擾到原有 SQL Editor 的 SQL (可能插入到原本 SQL 的中間，視使用者游標所在位置而定)
            MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{editor.Text}\r\n";

            Close();
        }

        private void txtAliasName_KeyPress(object sender, KeyPressEventArgs e)
        {
            var ch = e.KeyChar;
            var regex = new Regex(@"[^a-zA-Z0-9]");

            if (ch == 46 || (regex.IsMatch(e.KeyChar.ToString()) && ch != 8)) //8：倒退建; 46：句點
            {
                e.Handled = true;
            }

            CheckAliasName();
        }

        private void txtAliasName_Leave(object sender, EventArgs e)
        {
            CheckAliasName();

            if (!string.IsNullOrEmpty(txtAliasName.Text))
            {
                btnPreview.PerformClick();
            }
        }

        private void CheckAliasName()
        {
            chkAliasName.Checked = !string.IsNullOrWhiteSpace(txtAliasName.Text);
        }

        private void chkAliasName_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkAliasName.Checked)
            {
                txtAliasName.Text = string.Empty;
            }
            else
            {
                txtAliasName.Focus();
            }
        }

        private void ChangeCase_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = sender as RadioButton;

            if (!rdo.Checked)
            {
                return; //避免執行兩次 (第一次會是「前一次」的選項，忽略它！)
            }

            btnPreview.PerformClick();
        }

        private void cboNumbers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void chkColumnTypeInfo_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void chkPKInfo_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void chkLimitInfo_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void chkEncloseBrackets_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void chkEncloseGraveAccent_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void Options_CheckedChanged(object sender, EventArgs e)
        {
            btnPreview.PerformClick();
        }

        private void btnShowSql_Click(object sender, EventArgs e)
        {
            var pos = columnInfoSql.IndexOf(MyGlobal.Separator7);

            if (pos > 0)
            {
                var text = columnInfoSql.Substring(pos + MyGlobal.Separator7.Length).TrimStart('\r', '\n');

                columnInfoSql = text;
            }

            MessageBox.Show(columnInfoSql, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cboSchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboSchema.Text))
            {
                return;
            }

            cboTable.Text = string.Empty;
            cboView.Text = string.Empty;

            if (IsPostgreSql)
            {
                DatabaseSqlExecutor.GetTableInfo_PostgreSql(out _dtTable, cboSchema.Text);
                DatabaseSqlExecutor.GetViewInfo_PostgreSql(out _dtView, cboSchema.Text);
            }
            else if (IsSqlServer)
            {
                DatabaseSqlExecutor.GetTableInfo_SqlServer(txtDatabase.Text, cboSchema.Text, out _dtTable);
                DatabaseSqlExecutor.GetViewInfo_SqlServer(txtDatabase.Text, cboSchema.Text, out _dtView);
            }

            UIHelper.SetC1ComboBoxItemsFromDataTable(cboTable, _dtTable);
            UIHelper.SetC1ComboBoxItemsFromDataTable(cboView, _dtView);
        }

        private void cboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isKeypressComboBox)
            {
                return; //鍵盤輸入，忽略！
            }

            _isKeyPressTab = true;

            if (!string.IsNullOrEmpty(cboTable.Text))
            {
                SchemaType = SchemaObjectNames.Tables;
                rdoTable.Checked = true;
                RefreshSchemaInfo();
            }
        }

        private void cboView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isKeypressComboBox)
            {
                return; //鍵盤輸入，忽略！
            }

            _isKeyPressTab = true;

            if (!string.IsNullOrEmpty(cboView.Text))
            {
                SchemaType = SchemaObjectNames.Views;
                rdoView.Checked = true;
                RefreshSchemaInfo();
            }
        }

        private void btnSelectObject_Click(object sender, EventArgs e)
        {
            RefreshSchemaInfo();
        }

        private void RefreshSchemaInfo()
        {
            var dtColumnInfo = new DataTable();
            var dtSchema = new DataTable();
            var pkInfo = string.Empty;

            columnInfoCollector = null;

            try
            {
                PrimaryKey = string.Empty;
                SchemaType = rdoTable.Checked ? SchemaObjectNames.Tables : (rdoView.Checked ? SchemaObjectNames.Views : string.Empty);
                SchemaName = rdoTable.Checked ? cboTable.Text : (rdoView.Checked ? cboView.Text : string.Empty);

                if (string.IsNullOrEmpty(SchemaType) || string.IsNullOrEmpty(SchemaName))
                {
                    return;
                }

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            DatabaseSqlExecutor.GetColumnInfoAndPKInfo_Oracle(SchemaType, SchemaName, ref dtColumnInfo, ref pkInfo, ref columnInfoSql);

                            var sql = $"SELECT * FROM {SchemaName} WHERE 1 = 2";

                            MyGlobal.OracleReader.ExecuteQueryPaged100Rows(sql, 0, 0, out _, out dtSchema);

                            PrimaryKey = pkInfo;
                            dtColumnName = dtColumnInfo == null ? null : dtColumnInfo.Copy();
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            SchemaNode = cboSchema.Text;
                            DatabaseSqlExecutor.GetColumnInfoAndPKInfo_PostgreSql(SchemaNode, SchemaType, SchemaName, ref dtColumnInfo, ref pkInfo, ref columnInfoSql);

                            var sql = $"SELECT * FROM {SchemaNode}.{SchemaName} WHERE 1 = 2";

                            MyGlobal.PostgreSqlReader.ExecuteQueryPaged100Rows(sql, 0, 0, out _, out _, out _, out _, out dtSchema);

                            PrimaryKey = pkInfo;
                            dtColumnName = dtColumnInfo == null ? null : dtColumnInfo.Copy();
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            SchemaNode = txtDatabase.Text;
                            SchemaDbo = cboSchema.Text;
                            ObjectId = string.Empty;

                            DatabaseSqlExecutor.GetColumnInfoAndPKInfo_SqlServer(SchemaNode, SchemaType, SchemaName, SchemaDbo, ObjectId, ref dtColumnInfo, ref pkInfo, ref columnInfoSql);

                            var sql = $"SELECT * FROM {SchemaName} WHERE 1 = 2";

                            MyGlobal.SqlServerReader.ExecuteQueryPaged100Rows(sql, 0, 0, out _, out dtSchema);

                            PrimaryKey = pkInfo;
                            dtColumnName = dtColumnInfo == null ? null : dtColumnInfo.Copy();
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            SchemaNode = txtDatabase.Text;

                            DatabaseSqlExecutor.GetColumnInfoAndPKInfo_MySql(SchemaNode, SchemaType, SchemaName, ref dtColumnInfo, ref pkInfo, ref columnInfoSql);

                            var sql = $"SELECT * FROM {SchemaName} WHERE 1 = 2";

                            MyGlobal.MySqlReader.ExecuteQueryPaged100Rows(sql, 0, 0, out _, out dtSchema);

                            PrimaryKey = pkInfo;
                            dtColumnName = dtColumnInfo == null ? null : dtColumnInfo.Copy();
                            break;
                        }
                }

                btnPreview.Enabled = true;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (dtSchema != null && dtSchema.Rows.Count > 0)
                {
                    columnInfoCollector = SchemaColumnInfoBuilder.Build(DatabaseSqlExecutor.CurrentDataSource, dtSchema);
                }

                Initial();
            }
        }

        //調整下拉清單的大小
        private void ResizeAutoCompleteGrid(C1TrueDBGrid c1Grid1, int rowCount, int width)
        {
            var height = 0;

            try
            {
                if (rowCount <= 6)
                {
                    width = GridHelper.ResizeGridColumnWidth(c1Grid1) + 4;
                }
                else
                {
                    width = GridHelper.ResizeGridColumnWidth(c1Grid1) + c1Grid1.VScrollBar.Width + 5;
                }

                height = 124;

                c1Grid1.Size = new Size(width, height);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboTable_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isKeypressComboBox = false;

            if (IsPostgreSql && string.IsNullOrEmpty(cboSchema.Text))
            {
                e.Cancel = true;
                _languageText = LocalizationHelper.GetLanguageString("Please select a schema first.", "form", GetType().Name, "msg", "SelectSchemaFirst", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboSchema.Focus();
            }
            else if ((IsSqlServer || IsMySql) && string.IsNullOrEmpty(txtDatabase.Text))
            {
                e.Cancel = true;
                _languageText = LocalizationHelper.GetLanguageString("Please select a specific database first.", "form", GetType().Name, "msg", "SelectDatabaseFirst", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboSchema.Focus();
            }
        }

        private void cboTable_DropDownClosed(object sender, C1.Win.C1Input.DropDownClosedEventArgs e)
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                c1GridTable.Visible = false;
                return;
            }
        }

        private void cboTable_DropDownOpened(object sender, EventArgs e)
        {
            c1GridTable.Visible = false;
        }

        private void cboTable_EnterOrFocus()
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                return;
            }

            try
            {
                var rowCount = c1GridTable_Filter(cboTable.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridTable, rowCount, 0);
                    c1GridTable.Visible = true;
                }
                else
                {
                    c1GridTable.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private int c1GridTable_Filter(string condition0)
        {
            if (_dtTable == null)
            {
                return 0;
            }

            var dataView = _dtTable.DefaultView;

            try
            {
                var condition = $"[SchemaName] LIKE '*{condition0}*'";

                dataView.RowFilter = condition;

                if (dataView.Count == 0)
                {
                    dataView.RowFilter = "[SchemaName] LIKE '*'";
                }

                //20220915 將前面幾個字母相符的優先顯示在最前面
                #region
                dataView.Sort = "SchemaName";

                var dtSorted = dataView.ToTable();

                dtSorted.Columns.Add("Sort", typeof(int));

                //20250317 ChatGPT 優化的建議寫法
                var j = -1000;
                var filterKeyword = TextHelper.GetStringBetween(condition, "'", "'").Replace("*", string.Empty);

                if (!string.IsNullOrEmpty(filterKeyword))
                {
                    foreach (DataRow row in dtSorted.Rows)
                    {
                        var schemaName = row.GetSafeString("SchemaName");

                        if (!string.IsNullOrEmpty(schemaName) && schemaName.StartsWith(filterKeyword, StringComparison.OrdinalIgnoreCase))
                        {
                            row["Sort"] = j++;
                        }
                        else
                        {
                            row["Sort"] = dtSorted.Rows.IndexOf(row);
                        }
                    }
                }

                dataView = dtSorted.DefaultView;
                dataView.Sort = "Sort, SchemaName";
                dtSorted = dataView.ToTable();
                dtSorted.Columns.Remove("Sort");
                #endregion

                c1GridTable.Splits[0].RecordSelectors = false;
                c1GridTable.DataSource = dtSorted;

                return dtSorted.Rows.Count;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 0; //按下 Ctrl+J 可能會進到這個例外錯誤
            }
        }

        private void cboTable_Enter(object sender, EventArgs e)
        {
            cboTable_EnterOrFocus();
        }

        private void cboTable_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        c1GridTable.Focus();
                        e.SuppressKeyPress = false;
                        break;
                    }
                case Keys.Delete:
                    {
                        _isKeyPressDelete = true; //不能在此處理 Delete 按鍵，因為此時的 cboTable.Text 的值是按下 Delete 鍵之前的！
                        break;
                    }
            }
        }

        private void cboTable_KeyPress(object sender, KeyPressEventArgs e)
        {
            _isKeypressComboBox = true;
        }

        private void cboTable_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (_isKeyPressTab)
                {
                    _isKeyPressTab = false; //Tab 會回到這裡，不重複處理！
                }
                else if (_isKeyPressESC)
                {
                    _isKeyPressESC = false; //ESC 會回到這裡，不重複處理！
                }
                else
                {
                    var rowCount = c1GridTable_Filter(cboTable.Text);

                    if (rowCount > 0)
                    {
                        ResizeAutoCompleteGrid(c1GridTable, rowCount, 0);
                        c1GridTable.Visible = true;
                    }
                    else
                    {
                        c1GridTable.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboTable_Leave(object sender, EventArgs e)
        {
            if (!c1GridTable.Focused)
            {
                c1GridTable.Visible = false;
            }
        }

        private void cboTable_TextChanged(object sender, EventArgs e)
        {
            if (cboTable.Items.Count == 0)
            {
                return;
            }

            if (_isKeyPressDelete)
            {
                _isKeyPressDelete = false;
            }
            else
            {
                return;
            }

            try
            {
                var rowCount = c1GridTable_Filter(cboTable.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridTable, rowCount, 0);
                    c1GridTable.Visible = true;
                }
                else
                {
                    c1GridTable.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridTable_Leave(object sender, EventArgs e)
        {
            c1GridTable.Visible = false;
        }

        private void c1GridTable_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 && e.KeyChar != 9)
            {
                return;
            }

            try
            {
                _isKeyPressTab = true;
                c1GridTable_PasteColumnName();
                _isKeyPressTab = true;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridTable_PasteColumnName()
        {
            var cellText = c1GridTable[c1GridTable.Row, 0].ToString();

            cboTable.Text = cellText;
            c1GridTable.Visible = false;
            cboTable.Focus();
            _isKeyPressTab = true;
            rdoTable.Checked = true;

            if (!string.IsNullOrEmpty(cboTable.Text))
            {
                SchemaType = SchemaObjectNames.Tables;
                RefreshSchemaInfo();
            }
        }

        private void c1GridTable_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                _isKeyPressTab = true;
                c1GridTable_PasteColumnName();
                _isKeyPressTab = false;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboView_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isKeypressComboBox = false;

            if (IsPostgreSql && string.IsNullOrEmpty(cboSchema.Text))
            {
                e.Cancel = true;
                _languageText = LocalizationHelper.GetLanguageString("Please select a schema first.", "form", GetType().Name, "msg", "SelectSchemaFirst", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboSchema.Focus();
            }
            else if ((IsSqlServer || IsMySql) && string.IsNullOrEmpty(txtDatabase.Text))
            {
                e.Cancel = true;
                _languageText = LocalizationHelper.GetLanguageString("Please select a specific database first.", "form", GetType().Name, "msg", "SelectDatabaseFirst", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboSchema.Focus();
            }
        }

        private void cboView_DropDownClosed(object sender, C1.Win.C1Input.DropDownClosedEventArgs e)
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                c1GridView.Visible = false;
                return;
            }
        }

        private void cboView_DropDownOpened(object sender, EventArgs e)
        {
            c1GridView.Visible = false;
        }

        private void cboView_EnterOrFocus()
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                return;
            }

            try
            {
                var rowCount = c1GridView_Filter(cboView.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridView, rowCount, 0);
                    c1GridView.Visible = true;
                }
                else
                {
                    c1GridView.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private int c1GridView_Filter(string condition0)
        {
            if (_dtView == null)
            {
                return 0;
            }

            var dataView = _dtView.DefaultView;

            try
            {
                var condition = $"[SchemaName] LIKE '*{condition0}*'";

                dataView.RowFilter = condition;

                if (dataView.Count == 0)
                {
                    dataView.RowFilter = "[SchemaName] LIKE '*'";
                }

                //20220915 將前面幾個字母相符的優先顯示在最前面
                #region
                dataView.Sort = "SchemaName";

                var dtSorted = dataView.ToTable();

                dtSorted.Columns.Add("Sort", typeof(int));

                //20250317 ChatGPT 優化的建議寫法
                var j = -1000;
                var filterKeyword = TextHelper.GetStringBetween(condition, "'", "'").Replace("*", string.Empty);

                if (!string.IsNullOrEmpty(filterKeyword))
                {
                    foreach (DataRow row in dtSorted.Rows)
                    {
                        var schemaName = row.GetSafeString("SchemaName");

                        if (!string.IsNullOrEmpty(schemaName) && schemaName.StartsWith(filterKeyword, StringComparison.OrdinalIgnoreCase))
                        {
                            row["Sort"] = j++;
                        }
                        else
                        {
                            row["Sort"] = dtSorted.Rows.IndexOf(row);
                        }
                    }
                }

                dataView = dtSorted.DefaultView;
                dataView.Sort = "Sort, SchemaName";
                dtSorted = dataView.ToTable();
                dtSorted.Columns.Remove("Sort");
                #endregion

                c1GridView.Splits[0].RecordSelectors = false;
                c1GridView.DataSource = dtSorted;

                return dtSorted.Rows.Count;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 0; //按下 Ctrl+J 可能會進到這個例外錯誤
            }
        }

        private void cboView_Enter(object sender, EventArgs e)
        {
            cboView_EnterOrFocus();
        }

        private void cboView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        c1GridView.Focus();
                        e.SuppressKeyPress = false;
                        break;
                    }
                case Keys.Delete:
                    {
                        _isKeyPressDelete = true; //不能在此處理 Delete 按鍵，因為此時的 cboView.Text 的值是按下 Delete 鍵之前的！
                        break;
                    }
            }
        }

        private void cboView_KeyPress(object sender, KeyPressEventArgs e)
        {
            _isKeypressComboBox = true;
        }

        private void cboView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (_isKeyPressTab)
                {
                    _isKeyPressTab = false; //Tab 會回到這裡，不重複處理！
                }
                else if (_isKeyPressESC)
                {
                    _isKeyPressESC = false; //ESC 會回到這裡，不重複處理！
                }
                else
                {
                    var rowCount = c1GridView_Filter(cboView.Text);

                    if (rowCount > 0)
                    {
                        ResizeAutoCompleteGrid(c1GridView, rowCount, 0);
                        c1GridView.Visible = true;
                    }
                    else
                    {
                        c1GridView.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboView_Leave(object sender, EventArgs e)
        {
            if (!c1GridView.Focused)
            {
                c1GridView.Visible = false;
            }
        }

        private void cboView_TextChanged(object sender, EventArgs e)
        {
            if (cboView.Items.Count == 0)
            {
                return;
            }

            if (_isKeyPressDelete)
            {
                _isKeyPressDelete = false;
            }
            else
            {
                return;
            }

            try
            {
                var rowCount = c1GridView_Filter(cboView.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridView, rowCount, 0);
                    c1GridView.Visible = true;
                }
                else
                {
                    c1GridView.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridView_Leave(object sender, EventArgs e)
        {
            c1GridView.Visible = false;
        }

        private void c1GridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 && e.KeyChar != 9)
            {
                return;
            }

            try
            {
                _isKeyPressTab = true;
                c1GridView_PasteColumnName();
                _isKeyPressTab = true;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridView_PasteColumnName()
        {
            var sCellText = c1GridView[c1GridView.Row, 0].ToString();

            cboView.Text = sCellText;
            c1GridView.Visible = false;
            cboView.Focus();
            _isKeyPressTab = true;
            rdoView.Checked = true;

            if (!string.IsNullOrEmpty(cboTable.Text))
            {
                SchemaType = SchemaObjectNames.Views;
                RefreshSchemaInfo();
            }
        }

        private void c1GridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                _isKeyPressTab = true;
                c1GridView_PasteColumnName();
                _isKeyPressTab = false;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void GroupBoxe_FocusedOrClick(object sender, EventArgs e)
        {
            c1GridTable.Visible = false;
            c1GridView.Visible = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //20250311 取消 ESC 關閉功能，由使用者按下 X 按鈕或關閉按鈕決定是否關閉
            //if (keyData == Keys.Escape)
            //{
            //    Close();
            //    return true;
            //}

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
