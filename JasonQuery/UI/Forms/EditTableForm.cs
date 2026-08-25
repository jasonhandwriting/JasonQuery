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
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public enum EditTableMode
    {
        None,
        Comment,
        Drop,
        Rename,
        Truncate
    }

    public partial class EditTableForm : Form
    {
        public string Result { get; set; } //20250204 回傳值給呼叫方，判斷是否需要更新 Schema Info
        public string SchemaDatabase { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public EditTableMode EditMode { get; set; }

        private string _tableComment = string.Empty;
        private bool _hasSqlServerTableComment;
        private string _operationObject = string.Empty;
        private ContextMenuStrip _editorMenu = new ContextMenuStrip();
        private bool _isFormClosing = true;

        private readonly HashSet<char> _invalidChars = new HashSet<char>
        {
            ' ', '\'', '*', '[', ']', '/', '\\',
            '"', ';', ':', '<', '>', '?', '!', '@', '#', '$', '%', '^', '&',
            '(', ')', '+', '=', '{', '}', '|', '~', '`',
            '\t', '\n', '\r', '\v', '\f'
        };

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public EditTableForm()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                ApplyEditorSetting();
                LocalizationHelper.ApplyLanguageInfo(this);

                lblPrompt2_Oracle.Visible = IsOracle;
                lblPrompt2_PostgreSql.Visible = IsPostgreSql;
                lblPrompt2_SqlServer.Visible = IsSqlServer;
                lblPrompt2_MySql.Visible = IsMySql;

                //重設 ForeColor
                lblCommentInfo.ForeColor = Color.FromArgb(0, 0, 192);
                lblDropInfo.ForeColor = Color.FromArgb(0, 0, 192);
                lblRenameInfo.ForeColor = Color.FromArgb(0, 0, 192);
                lblTruncateInfo.ForeColor = Color.FromArgb(0, 0, 192);
                lblPrompt1.ForeColor = Color.DarkRed;
                lblPrompt2_Oracle.ForeColor = Color.DarkRed;
                lblPrompt2_PostgreSql.ForeColor = Color.DarkRed;
                lblPrompt2_SqlServer.ForeColor = Color.DarkRed;
                lblPrompt2_MySql.ForeColor = Color.DarkRed;

                txtTableName_Comment.Location = new Point(lblTableName_Comment.Left + lblTableName_Comment.Width, txtTableName_Comment.Top);
                txtComment.Location = new Point(lblComment.Left + lblComment.Width, txtComment.Top);
                txtTableName_Drop.Location = new Point(lblTableName_Drop.Left + lblTableName_Drop.Width, txtTableName_Drop.Top);
                txtTableName_Rename.Location = new Point(lblTableName_Rename.Left + lblTableName_Rename.Width, txtTableName_Rename.Top);
                txtNewTableName.Location = new Point(lblNewTableName.Left + lblNewTableName.Width, txtNewTableName.Top);
                txtTableName_Truncate.Location = new Point(lblTableName_Truncate.Left + lblTableName_Truncate.Width, txtTableName_Truncate.Top);
                btnHelp_DropDependent.Location = new Point(chkDropDependent.Left + chkDropDependent.Width, btnHelp_DropDependent.Top);
                btnHelp_CascadeConstraints.Location = new Point(chkCascadeConstraints.Left + chkCascadeConstraints.Width, btnHelp_CascadeConstraints.Top);
                btnHelp_PurgeSpace.Location = new Point(chkPurgeSpace.Left + chkPurgeSpace.Width, btnHelp_PurgeSpace.Top);
                btnHelp_Only.Location = new Point(chkOnly.Left + chkOnly.Width, btnHelp_Only.Top);
                btnHelp_Restart.Location = new Point(chkRestart.Left + chkRestart.Width, btnHelp_Restart.Top);
                btnHelp_TruncateCascade.Location = new Point(chkTruncateCascade.Left + chkTruncateCascade.Width, btnHelp_TruncateCascade.Top);
                btnHelp_DefaultDrop.Location = new Point(rdoDefaultDrop.Left + rdoDefaultDrop.Width, btnHelp_DefaultDrop.Top);
                btnHelp_Reuse.Location = new Point(rdoReuse.Left + rdoReuse.Width, btnHelp_Reuse.Top);

                var languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");

                _editorMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.A;

                _editorMenu.Items[0].Click += delegate
                {
                    editorSqlPreview.SelectionStart = 0;
                    editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
                };

                _editorMenu.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
                _editorMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorMenu.Items[1]).ShortcutKeys = Keys.Control | Keys.C;

                _editorMenu.Items[1].Click += delegate
                {
                    editorSqlPreview.Copy();
                };

                _editorMenu.Items[1].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");

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
                _tableComment = string.Empty;
                _hasSqlServerTableComment = false;

                var sbSql = new StringBuilder();

                switch (EditMode)
                {
                    case EditTableMode.Comment:
                        {
                            tabTable.SelectedTab = tabComment;
                            Text += $" - {lblCommentInfo.Text.Replace("...", string.Empty)}";
                            txtTableName_Comment.Text = TableName;
                            _operationObject = lblCommentInfo.Text;

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Comment");

                            switch (_currentSourceType)
                            {
                                case DataSourceType.Oracle:
                                    {
                                        sbSql.AppendLine("SELECT Comments AS TableComment FROM All_Tab_Comments");
                                        sbSql.AppendLine($" WHERE Table_Name = '{TableName}'");
                                        sbSql.Append($"   AND UPPER(Owner) = '{(SchemaName ?? string.Empty).ToUpperInvariant()}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _tableComment = dtData.Rows[0].GetSafeString("TableComment");
                                        }

                                        break;
                                    }
                                case DataSourceType.PostgreSql:
                                    {
                                        sbSql.AppendLine("SELECT pg_catalog.COL_DESCRIPTION(c.oid, 0) AS TableComment");
                                        sbSql.AppendLine("  FROM pg_catalog.pg_class c");
                                        sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace");
                                        sbSql.AppendLine(" WHERE c.relkind = 'r' --'r': regular table");
                                        sbSql.AppendLine("   AND n.nspname NOT LIKE 'pg_%'");
                                        sbSql.AppendLine("   AND n.nspname NOT LIKE 'information_schema%'");
                                        sbSql.Append($"   AND c.relname = '{SchemaName}.{TableName}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _tableComment = dtData.Rows[0].GetSafeString("TableComment");
                                        }

                                        break;
                                    }
                                case DataSourceType.SqlServer:
                                    {
                                        sbSql.AppendLine("SELECT t.name AS TableName,");
                                        sbSql.AppendLine("       CAST(ep.value AS nvarchar(max)) AS TableComment,");
                                        sbSql.AppendLine("       CASE WHEN ep.major_id IS NULL THEN 0 ELSE 1 END AS HasTableComment");
                                        sbSql.AppendLine("  FROM sys.tables t");
                                        sbSql.AppendLine("       INNER JOIN sys.schemas s");
                                        sbSql.AppendLine("               ON s.schema_id = t.schema_id");
                                        sbSql.AppendLine("       LEFT JOIN sys.extended_properties ep");
                                        sbSql.AppendLine("              ON ep.class = 1");
                                        sbSql.AppendLine("             AND ep.major_id = t.object_id");
                                        sbSql.AppendLine("             AND ep.minor_id = 0"); //minor_id = 0 → Table / View / Procedure 等物件本身的 extended property
                                        sbSql.AppendLine("             AND ep.name = N'MS_Description'");
                                        sbSql.AppendLine($" WHERE s.name = N'{SchemaName}'");
                                        sbSql.Append($"   AND t.name = N'{TableName}';");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            var row = dtData.Rows[0];

                                            _tableComment = row.GetSafeString("TableComment");
                                            _hasSqlServerTableComment = row.GetSafeString("HasTableComment") == "1"; //判斷註解狀態：空值代表 add；其餘代表 update
                                        }

                                        break;
                                    }
                                case DataSourceType.MySql:
                                    {
                                        sbSql.AppendLine("SELECT Table_Comment AS TableComment");
                                        sbSql.AppendLine("  FROM Information_Schema.Tables");
                                        sbSql.AppendLine($" WHERE Table_Schema = '{SchemaName}'");
                                        sbSql.Append($"   AND Table_Name = '{TableName}'");

                                        var sql = sbSql.ToString();
                                        var dtData = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);

                                        if (dtData?.Rows.Count > 0)
                                        {
                                            _tableComment = dtData.Rows[0].GetSafeString("TableComment");
                                        }

                                        break;
                                    }
                            }

                            txtComment.Text = _tableComment;
                            break;
                        }
                    case EditTableMode.Drop:
                        {
                            tabTable.SelectedTab = tabDrop;
                            Text += $" - {lblDropInfo.Text.Replace("...", string.Empty)}";
                            txtTableName_Drop.Text = TableName;
                            _operationObject = lblDropInfo.Text;

                            pnlDrop_Oracle.Visible = IsOracle;
                            pnlDrop_PostgreSQL.Visible = IsPostgreSql;

                            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Drop));

                            break;
                        }
                    case EditTableMode.Rename:
                        {
                            tabTable.SelectedTab = tabRename;
                            Text += $" - {lblRenameInfo.Text.Replace("...", string.Empty)}";
                            txtTableName_Rename.Text = TableName;
                            txtNewTableName.Text = TableName;
                            _operationObject = lblRenameInfo.Text;
                            break;
                        }
                    case EditTableMode.Truncate:
                        {
                            tabTable.SelectedTab = tabTruncate;
                            Text += $" - {lblTruncateInfo.Text.Replace("...", string.Empty)}";
                            txtTableName_Truncate.Text = TableName;
                            _operationObject = lblTruncateInfo.Text;

                            pnlTruncate_Oracle.Visible = IsOracle;
                            pnlTruncate_PostgreSQL.Visible = IsPostgreSql;

                            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Truncate));

                            break;
                        }
                }

                Text += $" - {TableName}";
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
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableFormHeight", Size.Height.ToString());
        }

        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.Equals(txtComment.Text, _tableComment, StringComparison.Ordinal))
                {
                    UpdateSqlPreviewText("");
                }
                else
                {
                    UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Comment));
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void DropOracle_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Drop));
        }

        private void DropPostgreSQL_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Drop));
        }

        private void TruncateOracle_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Truncate));
        }

        private void TruncatePostgreSQL_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSqlPreviewText(BuildTableDdlSql(TableDdlOperation.Truncate));
        }

        private string BuildTableDdlSql(TableDdlOperation operation)
        {
            var tableName = TableName;

            switch (operation)
            {
                case TableDdlOperation.Comment:
                    {
                        tableName = txtTableName_Comment.Text;
                        break;
                    }
                case TableDdlOperation.Drop:
                    {
                        tableName = txtTableName_Drop.Text;
                        break;
                    }
                case TableDdlOperation.Rename:
                    {
                        tableName = txtTableName_Rename.Text;
                        break;
                    }
                case TableDdlOperation.Truncate:
                    {
                        tableName = txtTableName_Truncate.Text;
                        break;
                    }
            }

            var request = new TableDdlRequest
            {
                DataSourceType = _currentSourceType,
                Operation = operation,
                SchemaDatabase = SchemaDatabase,
                SchemaName = SchemaName,
                TableName = tableName,
                NewTableName = txtNewTableName.Text,
                Comment = txtComment.Text,
                HasExistingSqlServerComment = _hasSqlServerTableComment,
                OracleCascadeConstraints = chkCascadeConstraints.Checked,
                OraclePurge = chkPurgeSpace.Checked,
                OracleReuseStorage = rdoReuse.Checked,
                PostgreSqlDropCascade = chkDropDependent.Checked,
                PostgreSqlOnly = chkOnly.Checked,
                PostgreSqlRestartIdentity = chkRestart.Checked,
                PostgreSqlTruncateCascade = chkTruncateCascade.Checked
            };

            return TableDdlSqlBuilder.Build(request);
        }

        private void UpdateSqlPreviewText(string text)
        {
            editorSqlPreview.ReadOnly = false;
            editorSqlPreview.Text = text;
            editorSqlPreview.ReadOnly = true;
        }

        private void editorSqlPreview_TextChanged(object sender, EventArgs e)
        {
            btnExecute.Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);
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

        private void btnHelp_DropDependent_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Remove the table and all the object(s) that depend on it.", "form", GetType().Name, "msg", "Help_DropDependent", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_CascadeConstraints_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If specified, all referential integrity constraints will be dropped as well.", "form", GetType().Name, "msg", "Help_CascadeConstraints", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_PurgeSpace_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If specified, the table and its dependent objects will be purged from the recycle bin and you will not be able to recover the table.", "form", GetType().Name, "msg", "Help_PurgeSpace", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Only_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If ONLY is specified before the table name, only that table is truncated. If ONLY is not specified, the table and all its descendant tables (if any) are truncated. Optionally, * can be specified after the table name to explicitly indicate that descendant tables are included.", "form", GetType().Name, "msg", "Help_Only", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Restart_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Automatically restart sequences owned by columns of the truncated table(s).", "form", GetType().Name, "msg", "Help_Restart", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_TruncateCascade_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Automatically truncate all tables that have foreign-key references to any of the named tables, or to any tables added to the group due to CASCADE.", "form", GetType().Name, "msg", "Help_TruncateCascade", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DefaultDrop_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Deallocates the storage used by the rows and returns the space to the free space pool. This is the default.", "form", GetType().Name, "msg", "Help_DefaultDrop", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Reuse_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Retains the space used by the deleted rows. This is useful if the table or cluster will be reloaded with data.", "form", GetType().Name, "msg", "Help_Reuse", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            _editorMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);

            //Copy
            _editorMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorSqlPreview.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            editorSqlPreview.ContextMenuStrip = _editorMenu;

            if (MyLibrary.IsDarkMode)
            {
                _editorMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _editorMenu.ForeColor = Color.White;
                _editorMenu.RenderMode = ToolStripRenderMode.System;
            }

            _editorMenu.Show(editorSqlPreview, new Point(e.X, e.Y));
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var errorMessage = string.Empty;
            var sqlText = editorSqlPreview.Text;
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
                                sqlText,
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
                                sqlText,
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
                                sqlText,
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
                                sqlText,
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

                var resultExecuted = completionDecision.Succeeded ? "Complete" : "Error";

                UpdateMainFormTransactionState(completionDecision);

                var note = string.Empty;

                switch (EditMode)
                {
                    case EditTableMode.Comment:
                        {
                            var temp = LocalizationHelper.GetLanguageString("--Original Comment:", "form", GetType().Name, "msg", "OriginalComment", "Text");

                            //記錄舊的註解，呈現在 SQL History 中
                            note = $"{temp}\r\n{_tableComment}";
                            break;
                        }
                    case EditTableMode.Rename:
                        {
                            var temp = LocalizationHelper.GetLanguageString("--Original Table Name:", "form", GetType().Name, "msg", "OriginalTableName", "Text");

                            //記錄舊的 Table Name，呈現在 SQL History 中
                            note = $"{temp}\r\n{txtTableName_Rename.Text}";
                            break;
                        }
                }

                Result = string.Empty;

                if (completionDecision.Succeeded)
                {
                    ApplySuccessfulExecutionControlState();

                    if (EditMode == EditTableMode.Drop || EditMode == EditTableMode.Rename)
                    {
                        Result = "REFRESH";
                    }

                    var languageText = LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}OK", "Text").Replace("{0}", TableName).Replace("{1}", txtNewTableName.Text);

                    MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var languageText = LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}NG", "Text").Replace("{0}", TableName).Replace("{1}", txtNewTableName.Text);

                    MessageBox.Show($"{languageText}\r\n\r\n{errorMessage}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                var lineBreak = completionDecision.Succeeded ? string.Empty : $"\r\n{errorMessage}";

                //更新 SQL 歷史記錄
                UpdateSqlHistory(0, resultExecuted, $"{note}{lineBreak}", editorSqlPreview.Text);

                _isFormClosing = false;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ApplySuccessfulExecutionControlState()
        {
            btnExecute.Enabled = false;
            txtComment.Enabled = false;
            txtNewTableName.Enabled = false;

            switch (EditMode)
            {
                case EditTableMode.Drop:
                    {
                        //停用整個選項區塊，避免 CheckedChanged 再次產生可執行 SQL
                        pnlDrop_Oracle.Enabled = false;
                        pnlDrop_PostgreSQL.Enabled = false;
                        break;
                    }
                case EditTableMode.Truncate:
                    {
                        //停用整個選項區塊，避免 CheckedChanged 再次產生可執行 SQL
                        pnlTruncate_Oracle.Enabled = false;
                        pnlTruncate_PostgreSQL.Enabled = false;
                        break;
                    }
            }
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
                        TransferValueToMainForm(
                            "UpdateCommitRollbackButton`");
                        break;
                    }
                case DdlMainFormNotificationKind.TransactionClosed:
                    {
                        TransferValueToMainForm(
                            "ExecuteCommitRollback`");
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

        private void txtNewTableName_KeyPress(object sender, KeyPressEventArgs e)
        {
            //允许控制按键 (倒退、删除、方向键、Ctrl+C/V 等)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            e.Handled = _invalidChars.Contains(e.KeyChar); //阻止非法字元输入
        }

        private void txtNewTableName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //20260429 防範使用者可能透過滑鼠右鍵「貼上」繞過 KeyPress 事件
                bool hasInvalidChar = false;

                foreach (char c in txtNewTableName.Text)
                {
                    if (_invalidChars.Contains(c))
                    {
                        hasInvalidChar = true;
                        break;
                    }
                }

                //20260801 執行按鈕與 SQL 預覽共用同一個結果，避免按鈕已啟用但預覽仍為空白
                var sql = hasInvalidChar ? string.Empty : BuildTableDdlSql(TableDdlOperation.Rename);

                //未來在產生 SQL 時，可以考慮根據不同的資料庫自動加上對應的「引號」來保護物件名稱：
                //SQL Server: [{TableName}]
                //PostgreSQL, Oracle: "{TableName}"
                //MySQL: `{TableName}`

                btnExecute.Enabled = !string.IsNullOrEmpty(sql);
                UpdateSqlPreviewText(sql);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
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
                case EditTableMode.Comment:
                    {
                        targetControl = txtComment;
                        break;
                    }
                case EditTableMode.Rename:
                    {
                        targetControl = txtNewTableName;
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
