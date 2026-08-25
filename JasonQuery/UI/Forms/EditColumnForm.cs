using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public enum EditColumnMode
    {
        None,
        Comment,
        Drop,
        Rename,
        Add
    }

    public partial class EditColumnForm : Form
    {
        public string Result { get; set; } //20250204 回傳值給呼叫方，判斷是否需要更新 Schema Info
        public string SchemaDatabase { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public EditColumnMode EditMode { get; set; } //Comment/Drop/Rename/Add

        //未來 MainForm 共用 SchemaExplorer、多連線時，可明確傳入所屬連線的資料庫類型
        //未指定時，仍使用目前的 DatabaseSqlExecutor.CurrentDataSource，維持既有呼叫相容
        public DataSourceType SourceType { get; set; }

        private string _columnComment = string.Empty; //資料表的註解(舊值)
        private string _operationObject = string.Empty;
        private ContextMenuStrip _editorContextMenu = new ContextMenuStrip();
        private bool _isFormClosing = true;

        //20250602 簡化資料庫判斷方式
        private DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public EditColumnForm()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                if (SourceType != DataSourceType.None)
                {
                    _currentSourceType = SourceType;
                }

                ApplyEditorSetting();
                ApplyLocalizedFormSettings();

                txtTableName.Text = TableName;

                var languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");

                _editorContextMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorContextMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.A;

                _editorContextMenu.Items[0].Click += delegate
                {
                    editorSqlPreview.SelectionStart = 0;
                    editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
                };

                _editorContextMenu.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
                _editorContextMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorContextMenu.Items[1]).ShortcutKeys = Keys.Control | Keys.C;

                _editorContextMenu.Items[1].Click += delegate
                {
                    editorSqlPreview.Copy();
                };

                _editorContextMenu.Items[1].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");

                if (MyLibrary.WordWrap)
                {
                    btnWordWrap.PerformClick();
                }
                else
                {
                    btnWordWrap2.PerformClick();
                }

                switch (MyLibrary.WordWrapIndentMode)
                {
                    case "Fixed":
                        {
                            editorSqlPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Fixed;
                            break;
                        }
                    case "Indent":
                        {
                            editorSqlPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Indent;
                            break;
                        }
                    default:
                        {
                            editorSqlPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
                            break;
                        }
                }

                if (MyLibrary.ShowAllCharacters)
                {
                    editorSqlPreview.ViewEol = true;
                    editorSqlPreview.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
                    btnShowAllCharacters.Visible = false;
                    btnShowAllCharacters2.Visible = true;
                }
                else
                {
                    editorSqlPreview.ViewEol = false;
                    editorSqlPreview.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
                    btnShowAllCharacters.Visible = true;
                    btnShowAllCharacters2.Visible = false;
                }

                tsTool.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                txtNewColumnName.Tag = string.Empty;

                switch (EditMode)
                {
                    case EditColumnMode.Comment:
                        {
                            tabColumn.SelectedTab = tabComment;
                            Text += $" - {lblCommentInfo.Text.Replace("...", string.Empty)}";
                            txtColumnName_Comment.Text = ColumnName;
                            _operationObject = lblCommentInfo.Text;

                            var sbSql = new StringBuilder();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comment");

                            switch (_currentSourceType)
                            {
                                case DataSourceType.Oracle:
                                    {
                                        sbSql.AppendLine("SELECT cc.Comments");
                                        sbSql.AppendLine("  FROM User_Tab_columns ss");
                                        sbSql.AppendLine("       LEFT JOIN User_Col_Comments cc on (ss.Column_Name = cc.Column_Name AND ss.Table_Name = cc.Table_Name)");
                                        sbSql.AppendLine(" WHERE ss.Table_Name IN (SELECT Object_Name FROM All_Objects WHERE Object_Type IN ('TABLE')");
                                        sbSql.AppendLine($"   AND UPPER(Owner) = '{(SchemaName ?? string.Empty).ToUpperInvariant()}')");
                                        sbSql.AppendLine($"   AND ss.Table_Name = '{TableName}'");
                                        sbSql.Append($"   AND ss.Column_Name = '{ColumnName}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _columnComment = dtData.Rows[0].GetSafeString("Comments");
                                        }

                                        break;
                                    }
                                case DataSourceType.PostgreSql:
                                    {
                                        sbSql.AppendLine("SELECT (pg_catalog.COL_DESCRIPTION((Table_Schema || '.' || Table_Name)::regclass::oid, Ordinal_Position::int)) AS Comments");
                                        sbSql.AppendLine("  FROM Information_Schema.Columns cols");
                                        sbSql.AppendLine($" WHERE cols.Table_Schema = '{SchemaName}'");
                                        sbSql.AppendLine($"   AND cols.Table_Name = '{TableName}'");
                                        sbSql.Append($"   AND cols.Column_Name = '{ColumnName}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _columnComment = dtData.Rows[0].GetSafeString("Comments");
                                        }

                                        break;
                                    }
                                case DataSourceType.SqlServer:
                                    {
                                        sbSql.AppendLine("SELECT c.Name AS Column_Name, prop.Value AS Comments");
                                        sbSql.AppendLine($"  FROM {SchemaDatabase}.sys.Extended_Properties AS prop");
                                        sbSql.AppendLine($"       INNER JOIN {SchemaDatabase}.sys.All_Objects o ON prop.Major_ID = o.Object_ID");
                                        sbSql.AppendLine($"       INNER JOIN {SchemaDatabase}.sys.Schemas s ON o.Schema_ID = s.Schema_ID");
                                        sbSql.AppendLine($"       INNER JOIN {SchemaDatabase}.sys.Columns AS c ON prop.Major_ID = c.Object_ID AND prop.Minor_ID = c.Column_ID");
                                        sbSql.AppendLine(" WHERE prop.Name = 'MS_Description'");
                                        sbSql.AppendLine($"   AND s.Name = '{SchemaName}'");
                                        sbSql.AppendLine($"   AND o.Name = '{TableName.Replace("{sSchemaName}.", string.Empty)}'");
                                        sbSql.Append($"   AND c.Name = '{ColumnName}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _columnComment = dtData.Rows[0].GetSafeString("Comments");
                                            txtNewColumnName.Tag = "NOTNULL"; //判斷註解狀態：空值代表 add；其餘代表 update (只有 SQL Server 變更註解時要判斷它是 add/update)
                                        }

                                        break;
                                    }
                                case DataSourceType.MySql:
                                    {
                                        sbSql.AppendLine("SELECT Column_Comment AS Comments");
                                        sbSql.AppendLine("  FROM Information_Schema.Columns");
                                        sbSql.AppendLine($" WHERE Table_Schema = '{SchemaDatabase}'");
                                        sbSql.AppendLine($"   AND Table_Name = '{TableName}'");
                                        sbSql.Append($"   AND Column_Name = '{ColumnName}';");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _columnComment = dtData.Rows[0].GetSafeString("Comments");
                                        }

                                        break;
                                    }
                            }

                            txtComment.Tag = _columnComment; //必須先指定 Tag (事件觸發時，Tag 才不會為 null)
                            txtComment.Text = _columnComment;
                            break;
                        }
                    case EditColumnMode.Drop:
                        {
                            tabColumn.SelectedTab = tabDrop;
                            Text += $" - {lblDropInfo.Text.Replace("...", string.Empty)}";
                            txtColumnName_Drop.Text = ColumnName;
                            _operationObject = lblDropInfo.Text;

                            UpdateSqlPreviewText(BuildColumnDdlSql(ColumnDdlOperation.Drop));

                            break;
                        }
                    case EditColumnMode.Rename:
                        {
                            tabColumn.SelectedTab = tabRename;
                            Text += $" - {lblRenameInfo.Text.Replace("...", string.Empty)}";
                            txtColumnName_Rename.Text = ColumnName;
                            txtNewColumnName.Text = ColumnName;
                            _operationObject = lblRenameInfo.Text;
                            break;
                        }
                    case EditColumnMode.Add:
                        {
                            tabColumn.SelectedTab = tabAdd;
                            Text += $" - {lblAddInfo.Text.Replace("...", string.Empty)}";
                            _operationObject = lblAddInfo.Text;
                            PrepareAddColumnEditor();

                            break;
                        }
                }

                Text += EditMode == EditColumnMode.Add ? $" - {TableName}" : $" - {ColumnName}";
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isFormClosing)
            {
                e.Cancel = true;
                _isFormClosing = true;
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditColumnFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditColumnFormHeight", Size.Height.ToString());
        }

        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtComment.Text == TextHelper.GetSafeString(txtComment.Tag))
                {
                    UpdateSqlPreviewText("");
                }
                else
                {
                    UpdateSqlPreviewText(BuildColumnDdlSql(ColumnDdlOperation.Comment));
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtNewColumnName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                btnExecute.Enabled = !string.IsNullOrWhiteSpace(txtNewColumnName.Text) && txtColumnName_Rename.Text != txtNewColumnName.Text;

                if (!btnExecute.Enabled)
                {
                    UpdateSqlPreviewText("");
                }
                else
                {
                    UpdateSqlPreviewText(BuildColumnDdlSql(ColumnDdlOperation.Rename));
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string BuildColumnDdlSql(ColumnDdlOperation operation)
        {
            var columnName = ColumnName;

            switch (operation)
            {
                case ColumnDdlOperation.Comment:
                    {
                        columnName = txtColumnName_Comment.Text;
                        break;
                    }
                case ColumnDdlOperation.Drop:
                    {
                        columnName = txtColumnName_Drop.Text;
                        break;
                    }
                case ColumnDdlOperation.Rename:
                    {
                        columnName = txtColumnName_Rename.Text;
                        break;
                    }
                case ColumnDdlOperation.Add:
                    {
                        columnName = txtColumnName_Add.Text;
                        break;
                    }
            }

            var request = new ColumnDdlRequest
            {
                DataSourceType = _currentSourceType,
                Operation = operation,
                SchemaDatabase = SchemaDatabase,
                SchemaName = SchemaName,
                TableName = TableName,
                ColumnName = columnName,
                NewColumnName = txtNewColumnName.Text,
                ColumnType = ColumnType,
                Comment = txtComment.Text,
                HasExistingSqlServerComment = !TextHelper.IsNullOrEmptyTag(txtNewColumnName.Tag),
                ColumnDefinition = operation == ColumnDdlOperation.Add ? CreateAddColumnDefinition() : null
            };

            return ColumnDdlSqlBuilder.Build(request);
        }

        private void UpdateSqlPreviewText(string sText)
        {
            editorSqlPreview.ReadOnly = false;
            editorSqlPreview.Text = sText;
            editorSqlPreview.ReadOnly = true;
        }

        private void editorSqlPreview_TextChanged(object sender, EventArgs e)
        {
            btnExecute.Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);
        }

        private void ApplyLocalizedFormSettings()
        {
            LocalizationHelper.ApplyLanguageInfo(this);

            //ApplyLanguageInfo 會將一般 Label 的 ForeColor 統一套用為標準前景色，
            //因此這些具有特殊提示語意的 Label 必須在語系套用後恢復指定顏色。
            ApplySpecialLabelColors();
            ApplyLocalizedControlLayout();
        }

        private void ApplySpecialLabelColors()
        {
            var informationColor = Color.FromArgb(0, 0, 192);

            lblAddInfo.ForeColor = informationColor;
            lblCommentInfo.ForeColor = informationColor;
            lblDropInfo.ForeColor = informationColor;
            lblRenameInfo.ForeColor = informationColor;

            lblPrompt1.ForeColor = Color.DarkRed;
            lblPrompt2.ForeColor = Color.DarkRed;
            lblAddValidationMessage.ForeColor = Color.DarkRed;
        }

        private void ApplyLocalizedControlLayout()
        {
            ControlLayoutHelper.PlaceRightOf(txtTableName, lblTableName, 0);
            ControlLayoutHelper.PlaceRightOf(txtColumnName_Comment, lblColumnName_Comment, 0);
            ControlLayoutHelper.PlaceRightOf(txtComment, lblComment, 0);
            ControlLayoutHelper.PlaceRightOf(txtColumnName_Drop, lblColumnName_Drop, 0);
            ControlLayoutHelper.PlaceRightOf(txtColumnName_Rename, lblColumnName_Rename, 0);
            ControlLayoutHelper.PlaceRightOf(txtNewColumnName, lblNewColumnName, 0);
            ControlLayoutHelper.PlaceRightOf(txtColumnName_Add, lblColumnName_Add, 0);

            ApplyAddColumnControlLayout();
        }

        private void ApplyEditorSetting()
        {
            editorSqlPreview.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorSqlPreview.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorSqlPreview.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
            editorSqlPreview.Zoom = Convert.ToInt16(MyLibrary.QueryEditorZoom);

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

            editorSqlPreview.Styler = new SqlStyler();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editorSqlPreview.SelectionStart = 0;
            editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            editorSqlPreview.Copy();
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            if (editorSqlPreview.WrapMode == ScintillaNET.WrapMode.Word)
            {
                btnWordWrap.Visible = true;
                btnWordWrap2.Visible = false;
                editorSqlPreview.WrapMode = ScintillaNET.WrapMode.None;
            }
            else
            {
                btnWordWrap.Visible = false;
                btnWordWrap2.Visible = true;
                editorSqlPreview.WrapMode = ScintillaNET.WrapMode.Word;
                editorSqlPreview.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSqlPreview.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            if (editorSqlPreview.ViewEol)
            {
                btnShowAllCharacters.Visible = true;
                btnShowAllCharacters2.Visible = false;
                editorSqlPreview.ViewEol = false;
                editorSqlPreview.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            }
            else
            {
                btnShowAllCharacters.Visible = false;
                btnShowAllCharacters2.Visible = true;
                editorSqlPreview.ViewEol = true;
                editorSqlPreview.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            }
        }

        private void editorSqlPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _editorContextMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);

            //Copy
            _editorContextMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorSqlPreview.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            editorSqlPreview.ContextMenuStrip = _editorContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                _editorContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _editorContextMenu.ForeColor = Color.White;
                _editorContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            _editorContextMenu.Show(editorSqlPreview, new Point(e.X, e.Y));
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var errorMessage = string.Empty;
            var hasPendingTransactionAfterExecute = false;
            var isTransactionClosedBySqlCommand = false;

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            MyGlobal.OracleReader.ExecuteSingleNonQuery
                            (
                                editorSqlPreview.Text,
                                out errorMessage,
                                out hasPendingTransactionAfterExecute,
                                out isTransactionClosedBySqlCommand
                            );

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            MyGlobal.PostgreSqlReader.ExecuteSingleNonQuery
                            (
                                editorSqlPreview.Text,
                                out errorMessage,
                                out hasPendingTransactionAfterExecute,
                                out isTransactionClosedBySqlCommand
                            );

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            MyGlobal.SqlServerReader.ExecuteSingleNonQuery
                            (
                                editorSqlPreview.Text,
                                out errorMessage,
                                out hasPendingTransactionAfterExecute,
                                out isTransactionClosedBySqlCommand
                            );

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            MyGlobal.MySqlReader.ExecuteSingleNonQuery
                            (
                                editorSqlPreview.Text,
                                out errorMessage,
                                out hasPendingTransactionAfterExecute,
                                out isTransactionClosedBySqlCommand
                            );

                            break;
                        }
                }

                var completionDecision = DdlExecutionCompletionPolicy.Evaluate
                                         (
                                             errorMessage,
                                             hasPendingTransactionAfterExecute,
                                             isTransactionClosedBySqlCommand
                                         );

                var note = string.Empty;

                switch (EditMode)
                {
                    case EditColumnMode.Comment:
                        {
                            var temp = LocalizationHelper.GetLanguageString("--Original Comment:", "form", GetType().Name, "msg", "OriginalComment", "Text");

                            //記錄舊的註解，呈現在 SQL History 中
                            note = $"{temp}\r\n{txtComment.Tag}";
                            break;
                        }
                    case EditColumnMode.Rename:
                        {
                            var temp = LocalizationHelper.GetLanguageString("--Original Column Name:", "form", GetType().Name, "msg", "OriginalColumnName", "Text");

                            //記錄舊的 Table Name，呈現在 SQL History 中
                            note = $"{temp}\r\n{txtColumnName_Rename.Text}";
                            break;
                        }
                }

                var resultExecuted = string.Empty;

                Result = string.Empty;

                if (completionDecision.Succeeded)
                {
                    Result = "REFRESH";
                    resultExecuted = "Complete";

                    btnExecute.Enabled = false;
                    txtComment.Enabled = false;
                    txtNewColumnName.Enabled = false;

                    if (EditMode == EditColumnMode.Add)
                    {
                        SetAddColumnEditorEnabled(false);
                    }

                    var message = EditMode == EditColumnMode.Add
                                  ? GetAddColumnExecutionMessage(true)
                                  : LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}OK", "Text").Replace("{0}", ColumnName).Replace("{1}", txtNewColumnName.Text);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    resultExecuted = "Error";

                    var message = EditMode == EditColumnMode.Add
                                  ? GetAddColumnExecutionMessage(false)
                                  : LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}NG", "Text").Replace("{0}", ColumnName).Replace("{1}", txtNewColumnName.Text);

                    MessageBox.Show($"{message}\r\n\r\n{errorMessage}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                var messageFinal = completionDecision.Succeeded ? string.Empty : $"\r\n{errorMessage}";

                //更新 SQL 歷史記錄
                UpdateSqlHistory(0, resultExecuted, $"{note}{messageFinal}", editorSqlPreview.Text);

                UpdateMainFormTransactionState(completionDecision);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            _isFormClosing = false;
        }

        private void UpdateMainFormTransactionState(DdlExecutionCompletionDecision decision)
        {
            if (decision == null || !decision.Succeeded)
            {
                return;
            }

            switch (decision.NotificationKind)
            {
                case DdlMainFormNotificationKind.PendingTransaction:
                    {
                        TransferValueToMainForm("UpdateCommitRollbackButton`");
                        break;
                    }
                case DdlMainFormNotificationKind.TransactionClosed:
                    {
                        TransferValueToMainForm("ExecuteCommitRollback`");
                        break;
                    }
            }
        }

        private void TransferValueToMainForm(string value)
        {
            MainForm mainForm = Owner as MainForm;

            if (mainForm == null && Owner is Form ownerForm)
            {
                mainForm = ownerForm.MdiParent as MainForm;
            }

            if (mainForm == null)
            {
                mainForm = MdiParent as MainForm;
            }

            if (mainForm == null)
            {
                foreach (Form openForm in Application.OpenForms)
                {
                    mainForm = openForm as MainForm;

                    if (mainForm != null)
                    {
                        break;
                    }
                }
            }

            mainForm?.ReceiveValueFromChildForm(value);
        }

        private void UpdateSqlHistory(int rows, string result, string message, string sql, string seqNo = "")
        {
            var executionTime = string.Empty;
            var queryTime = string.Empty;

            for (var i = 0; i < 5; i++)
            {
                if (!message.StartsWith("\r\n", StringComparison.Ordinal))
                {
                    break;
                }
                else
                {
                    message = message.Substring(2);
                }
            }

            //20240723 寫入 SQL 歷史記錄
            JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), executionTime, queryTime, rows, result, message, sql, _operationObject, seqNo);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            editorSqlPreview.ZoomIn();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            editorSqlPreview.ZoomOut();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            //等表單、TabPage 與 C1 控制項完成顯示及焦點初始化後再套用
            BeginInvoke(new MethodInvoker(ApplyInitialFocus));
        }

        private void ApplyInitialFocus()
        {
            Control targetControl = null;

            switch (EditMode)
            {
                case EditColumnMode.Comment:
                    {
                        targetControl = txtComment;
                        break;
                    }
                case EditColumnMode.Rename:
                    {
                        targetControl = txtNewColumnName;
                        break;
                    }
                case EditColumnMode.Add:
                    {
                        targetControl = txtColumnName_Add;
                        break;
                    }
                default:
                    {
                        targetControl = btnExecute;
                        break;
                    }
            }

            if (targetControl == null || !targetControl.CanSelect)
            {
                return;
            }

            ActiveControl = targetControl;
            targetControl.Select();
        }

        private void cboDataType_Add_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingAddColumnEditor)
            {
                return;
            }

            ApplySelectedAddColumnTypeDefinition(true);
            UpdateAddColumnPreview();
        }

        private void cboDataType_Add_TextUpdate(object sender, EventArgs e)
        {
            if (_isUpdatingAddColumnEditor)
            {
                return;
            }

            ApplySelectedAddColumnTypeDefinition(false);
            UpdateAddColumnPreview();
        }

        private void AddColumnEditorValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingAddColumnEditor)
            {
                return;
            }

            UpdateAddColumnPreview();
        }

        private void cboDefaultKind_Add_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingAddColumnEditor)
            {
                return;
            }

            ApplyAddColumnDefaultControlState();
            UpdateAddColumnPreview();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
