using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
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
        //B3-B1 opt-in only. A qualified F7 bridge must supply authoritative metadata-owner and selection-connection identity. Legacy callers remain unchanged until B3-B2; do not treat their DbUser/SchemaName as proof.
        internal OracleDdlTargetIdentity OracleTargetIdentity { get; set; }

        //New shared F7 only: cached SQL generation is not permission to execute.
        internal bool OfflineSqlPreviewOnly { get; set; }

        //Shared F7: the cached SQL preview is not authorization. An explicit Execute click may reconnect normally, then checks the current Oracle/PDB/target before dispatch.
        internal bool RequireOracleF7ExecuteGate { get; set; }
        internal SharedOracleMetadataCompletedLoadReceipt OracleF7Receipt { get; set; }
        internal string OracleF7SelectedOwner { get; set; }

        //New shared F7, Table Comment only. Legacy/Oracle form entry points remain unchanged.
        internal bool RequireNonOracleF7TableDdlGate { get; set; }
        internal string NonOracleF7ExpectedDatabase { get; set; }
        internal string NonOracleF7ExpectedSchema { get; set; }
        internal string NonOracleF7ExpectedTable { get; set; }
        internal string NonOracleF7ExpectedConnectionString { get; set; }

        public string Result { get; set; } //20250204 回傳值給呼叫方，判斷是否需要更新 Schema Info
        public string SchemaDatabase { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public EditTableMode EditMode { get; set; }

        private string _tableComment = string.Empty;
        private bool _hasSqlServerTableComment;
        private string _operationObject = string.Empty;
        private ContextMenuStrip _editorMenu = new ContextMenuStrip();
        private bool _commentMetadataReady = true;

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
                LocalizationHelper.ApplyLanguageInfo(this, false);

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

                _editorMenu.Items[1].Click += btnCopy_Click;

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

                if (!MyLibrary.IsDarkMode)
                {
                    tsTool.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                }

                _tableComment = string.Empty;
                _hasSqlServerTableComment = false;

                switch (EditMode)
                {
                    case EditTableMode.Comment:
                        {
                            tabTable.SelectedTab = tabComment;
                            Text += $" - {lblCommentInfo.Text.Replace("...", string.Empty)}";
                            txtTableName_Comment.Text = TableName;
                            _operationObject = lblCommentInfo.Text;

                            _commentMetadataReady = false;

                            if (!OfflineSqlPreviewOnly)
                            {
                                LoadCommentMetadata();
                            }

                            //Offline preview has no authoritative original Comment; do not claim it is empty in the database.
                            txtComment.Text = _tableComment;
                            _commentMetadataReady = true;

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

                if (OfflineSqlPreviewOnly && IsOracle)
                {
                    Text += " [SQL Preview Only]";
                    lblPrompt1.Text = "Cached metadata may be stale. Review or copy the generated SQL; Execute is disabled.";
                    btnExecute.Enabled = false;
                }

                UpdateSqlPreviewCopyState();
            }
            catch (Exception ex)
            {
                if (EditMode == EditTableMode.Comment)
                {
                    _commentMetadataReady = false;
                    txtComment.Enabled = false;
                    UpdateSqlPreviewText(string.Empty);
                    btnExecute.Enabled = false;
                }

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableFormHeight", Size.Height.ToString());
        }

        private bool TryRefreshOracleTarget(out OracleDdlTargetIdentity verified)
        {
            verified = null;

            var reader = MyGlobal.OracleReader;

            return DatabaseSqlExecutor.CurrentDataSource == DataSourceType.Oracle && reader != null && OracleDdlGuiGate.TryRefresh(OracleTargetIdentity, reader.VerifiedSessionUser, reader.VerifiedConnectionId, out verified);
        }

        private string GetCommentMetadataOwner()
        {
            if (!IsOracle || OracleTargetIdentity == null)
            {
                return SchemaName;
            }

            if (!TryRefreshOracleTarget(out var verified))
            {
                throw new InvalidOperationException("Oracle DDL target identity is not current. Comment metadata is blocked.");
            }

            return "\"" + verified.MetadataOwner.Replace("\"", "\"\"") + "\"";
        }

        private void LoadCommentMetadata()
        {
            var request = new MetadataCommentQueryRequest
            {
                DataSourceType = _currentSourceType,
                ObjectKind = MetadataCommentObjectKind.Table,
                SchemaDatabase = SchemaDatabase,
                SchemaName = GetCommentMetadataOwner(),
                TableName = TableName
            };

            var metadataSql = MetadataCommentQueryBuilder.Build(request);

            if (string.IsNullOrWhiteSpace(metadataSql))
            {
                throw new InvalidOperationException("Table comment metadata identity is invalid. DDL execution is blocked.");
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Comment");
            sbSql.Append(metadataSql);

            var sql = sbSql.ToString();

            System.Data.DataTable dtData;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        dtData = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, false);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        //D1-A: preserve the dedicated metadata savepoint path.
                        dtData = MyGlobal.PostgreSqlReader.ExecuteMetadataQueryToDataTable(sql);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        dtData = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        dtData = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Unsupported metadata provider. DDL execution is blocked.");
                    }
            }

            if (!MetadataCommentQualificationPolicy.TryQualify(dtData, out var metadata))
            {
                throw new InvalidOperationException("Table comment metadata did not identify exactly one valid object. DDL execution is blocked.");
            }

            _tableComment = metadata.Comment;
            _hasSqlServerTableComment = metadata.HasExistingComment;
        }

        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_commentMetadataReady)
                {
                    UpdateSqlPreviewText(string.Empty);
                    return;
                }

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
                _commentMetadataReady = false;
                txtComment.Enabled = false;
                UpdateSqlPreviewText(string.Empty);
                btnExecute.Enabled = false;

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            if (operation == TableDdlOperation.Comment && !_commentMetadataReady)
            {
                return string.Empty;
            }

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

            OracleDdlTargetIdentity validatedTarget = null;

            if (IsOracle && OracleTargetIdentity != null && !TryRefreshOracleTarget(out validatedTarget))
            {
                return string.Empty;
            }

            var request = new TableDdlRequest
            {
                DataSourceType = _currentSourceType,
                Operation = operation,
                SchemaDatabase = SchemaDatabase,
                SchemaName = SchemaName,
                RequireOracleDdlTargetIdentity = IsOracle && OracleTargetIdentity != null,
                OracleTargetIdentity = validatedTarget,
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
            UpdateSqlPreviewCopyState();
            btnExecute.Enabled = !OfflineSqlPreviewOnly && _commentMetadataReady && !string.IsNullOrEmpty(editorSqlPreview.Text);
        }

        private void UpdateSqlPreviewCopyState()
        {
            var hasText = !string.IsNullOrEmpty(editorSqlPreview.Text);

            btnSelectAll.Enabled = hasText;
            btnCopy.Enabled = hasText;
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

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_CascadeConstraints_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If specified, all referential integrity constraints will be dropped as well.", "form", GetType().Name, "msg", "Help_CascadeConstraints", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_PurgeSpace_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If specified, the table and its dependent objects will be purged from the recycle bin and you will not be able to recover the table.", "form", GetType().Name, "msg", "Help_PurgeSpace", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Only_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If ONLY is specified before the table name, only that table is truncated. If ONLY is not specified, the table and all its descendant tables (if any) are truncated. Optionally, * can be specified after the table name to explicitly indicate that descendant tables are included.", "form", GetType().Name, "msg", "Help_Only", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Restart_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Automatically restart sequences owned by columns of the truncated table(s).", "form", GetType().Name, "msg", "Help_Restart", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_TruncateCascade_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Automatically truncate all tables that have foreign-key references to any of the named tables, or to any tables added to the group due to CASCADE.", "form", GetType().Name, "msg", "Help_TruncateCascade", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DefaultDrop_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Deallocates the storage used by the rows and returns the space to the free space pool. This is the default.", "form", GetType().Name, "msg", "Help_DefaultDrop", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Reuse_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Retains the space used by the deleted rows. This is useful if the table or cluster will be reloaded with data.", "form", GetType().Name, "msg", "Help_Reuse", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editorSqlPreview.SelectionStart = 0;
            editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(editorSqlPreview.Text))
            {
                return;
            }

            if (editorSqlPreview.SelectedText.Length == 0)
            {
                editorSqlPreview.SelectionStart = 0;
                editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
            }

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
            _editorMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text); //未選取時也可透過 btnCopy_Click 全選並複製 SQL

            editorSqlPreview.ContextMenuStrip = _editorMenu;

            _editorMenu.Show(editorSqlPreview, new Point(e.X, e.Y));
        }

        private TableDdlOperation GetNonOracleF7TableOperation()
        {
            switch (EditMode)
            {
                case EditTableMode.Comment:
                    {
                        return TableDdlOperation.Comment;
                    }
                case EditTableMode.Rename:
                    {
                        return TableDdlOperation.Rename;
                    }
                case EditTableMode.Drop:
                    {
                        return TableDdlOperation.Drop;
                    }
                case EditTableMode.Truncate:
                    {
                        return TableDdlOperation.Truncate;
                    }
                default:
                    {
                        return TableDdlOperation.None;
                    }
            }
        }

        private string GetNonOracleF7DisplayedTableName()
        {
            switch (EditMode)
            {
                case EditTableMode.Comment:
                    {
                        return txtTableName_Comment.Text;
                    }
                case EditTableMode.Rename:
                    {
                        return txtTableName_Rename.Text;
                    }
                case EditTableMode.Drop:
                    {
                        return txtTableName_Drop.Text;
                    }
                case EditTableMode.Truncate:
                    {
                        return txtTableName_Truncate.Text;
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private string TryExecuteNonOracleF7TableDdl(out string errorMessage, out bool hasPendingTransactionAfterExecute, out bool isTransactionClosedBySqlCommand, out bool didDispatch)
        {
            var readerError = string.Empty;
            var pending = false;
            var closed = false;
            var dispatched = false;
            var pgSavepointRestored = false;
            var operation = GetNonOracleF7TableOperation();

            errorMessage = string.Empty;
            hasPendingTransactionAfterExecute = false;
            isTransactionClosedBySqlCommand = false;
            didDispatch = false;

            if (operation == TableDdlOperation.None || IsOracle || !RequireNonOracleF7TableDdlGate
                || string.IsNullOrWhiteSpace(NonOracleF7ExpectedConnectionString)
                || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
            {
                return "F7U2C2_CONNECTION_UNVERIFIED";
            }

            if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal))
            {
                return "F7U2C2_CONNECTION_CHANGED";
            }

            var pendingOrUnknownTransaction = AppConfigHelper.IsNotCommitYet;
            var mysqlAcknowledged = !IsMySql;

            if (!pendingOrUnknownTransaction && (operation == TableDdlOperation.Drop || operation == TableDdlOperation.Truncate))
            {
                //One deliberate confirmation for destructive operations, with exact qualified target and preview SQL.
                var action = operation == TableDdlOperation.Drop ? lblDropInfo.Text : lblTruncateInfo.Text;
                var defaultText = "The selected operation can permanently remove the table or its data. Review the target and SQL before continuing.";
                var warning = LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", "NonOracleF7DestructiveTableDdlConfirmation", "Text");
                var qualifiedTarget = IsMySql ? NonOracleF7ExpectedDatabase + "." + NonOracleF7ExpectedTable : IsSqlServer ? NonOracleF7ExpectedDatabase + "." + NonOracleF7ExpectedSchema + "." + NonOracleF7ExpectedTable : NonOracleF7ExpectedSchema + "." + NonOracleF7ExpectedTable;
                var prompt = action + "\r\n" + qualifiedTarget + "\r\n\r\n" + warning + "\r\n\r\n" + editorSqlPreview.Text;

                if (IsMySql)
                {
                    prompt += "\r\n\r\n" + LocalizationHelper.GetLanguageString
                    (
                        "MySQL DDL may implicitly commit the transaction, including uncommitted changes. This operation cannot be undone with a normal ROLLBACK.",
                        "form", "EditColumnForm", "msg", "MySqlDdlImplicitCommitConfirmation", "Text"
                    );
                }

                if (MessageBox.Show(prompt, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return "F7U2C98_USER_CANCELLED";
                }

                if (IsMySql)
                {
                    mysqlAcknowledged = true;
                }
            }

            if (IsMySql && !pendingOrUnknownTransaction && !mysqlAcknowledged)
            {
                var defaultMessage = "MySQL DDL may implicitly commit the transaction, including uncommitted changes.\r\nThis operation cannot be undone with a normal ROLLBACK. Do you want to continue?";
                var message = LocalizationHelper.GetLanguageString(defaultMessage, "form", "EditColumnForm", "msg", "MySqlDdlImplicitCommitConfirmation", "Text");

                mysqlAcknowledged = MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

                if (!mysqlAcknowledged)
                {
                    return "F7U2C98_USER_CANCELLED";
                }
            }

            var status = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                _currentSourceType,
                pendingOrUnknownTransaction,
                mysqlAcknowledged,
                () =>
                {
                    if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal) || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
                    {
                        return "F7U2C2_CONNECTION_CHANGED";
                    }

                    var connectionError = string.Empty;
                    bool opened = false;

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                opened = MyGlobal.PostgreSqlReader != null && MyGlobal.PostgreSqlReader.TryEnsureConnectionOpen(out connectionError);
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                opened = MyGlobal.SqlServerReader != null && MyGlobal.SqlServerReader.TryEnsureConnectionOpen(out connectionError);
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                opened = MyGlobal.MySqlReader != null && MyGlobal.MySqlReader.TryEnsureConnectionOpen(out connectionError);
                                break;
                            }
                    }

                    if (!opened)
                    {
                        readerError = connectionError;
                        return "F7U2C2_CONNECTION_FAILED";
                    }

                    return string.Empty;
                },
                () =>
                {
                    if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal) || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
                    {
                        return "F7U2C2_CONNECTION_CHANGED";
                    }

                    //The preview is rebuilt from this visible TextBox, not just from the form's TableName property.
                    //Do not allow the SQL generator to retarget a different Table while preserving the original F7 identity.
                    if (!string.Equals(GetNonOracleF7DisplayedTableName(), NonOracleF7ExpectedTable, StringComparison.Ordinal))
                    {
                        return "F7U2C2_TABLE_MISMATCH";
                    }

                    var scopeStatus = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                    (
                        _currentSourceType,
                        NonOracleF7ExpectedDatabase,
                        NonOracleF7ExpectedSchema,
                        NonOracleF7ExpectedTable,
                        SchemaDatabase,
                        SchemaName,
                        TableName,
                        IsPostgreSql ? MyGlobal.PostgreSqlReader?.CurrentOpenDatabaseName : string.Empty,
                        editorSqlPreview.Text,
                        BuildTableDdlSql(operation)
                    );

                    if (!string.IsNullOrEmpty(scopeStatus))
                    {
                        return scopeStatus;
                    }

                    //A successful PostgreSQL RENAME may silently truncate an overlong new name.
                    if (IsPostgreSql && operation == TableDdlOperation.Rename)
                    {
                        var reader = MyGlobal.PostgreSqlReader;

                        return reader == null ? "F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED" : reader.ValidateNewIdentifierLengthOnVerifiedConnection(reader.CurrentOpenConnectionId, txtNewTableName.Text);
                    }

                    return string.Empty;
                },
                () =>
                {
                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                return MyGlobal.PostgreSqlReader?.CurrentOpenConnectionId ?? Guid.Empty;
                            }
                        case DataSourceType.SqlServer:
                            {
                                return MyGlobal.SqlServerReader?.CurrentOpenConnectionId ?? Guid.Empty;
                            }
                        case DataSourceType.MySql:
                            {
                                return MyGlobal.MySqlReader?.CurrentOpenConnectionId ?? Guid.Empty;
                            }
                        default:
                            {
                                return Guid.Empty;
                            }
                    }
                },
                connectionId =>
                {
                    var sql = editorSqlPreview.Text;

                    if (!NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedTableDdlSql(_currentSourceType, operation, sql))
                    {
                        return "F7U3B2_SQL_OPERATION_MISMATCH";
                    }

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                MyGlobal.PostgreSqlReader.ExecuteTableDdlOnVerifiedConnection(sql, connectionId, operation, out readerError, out pending, out closed, out pgSavepointRestored, out dispatched);
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                MyGlobal.SqlServerReader.ExecuteTableDdlOnVerifiedConnection(sql, connectionId, operation, out readerError, out pending, out closed);
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                MyGlobal.MySqlReader.ExecuteTableDdlOnVerifiedConnection(sql, connectionId, operation, out readerError, out pending, out closed);
                                break;
                            }
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    if (!string.IsNullOrEmpty(readerError))
                    {
                        dispatched = IsPostgreSql ? dispatched : readerError.StartsWith("F7U2C3B_EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal);
                        return readerError;
                    }

                    dispatched = true;
                    return string.Empty;
                }
            );

            errorMessage = readerError;
            hasPendingTransactionAfterExecute = pending;
            isTransactionClosedBySqlCommand = closed;
            didDispatch = dispatched;

            return status;
        }

        //Return an actionable localized message; machine-readable status belongs in SQL History.
        //F7-U3-C3: explain confirmed truncation with the exact measured UTF-8 bytes and live server limit.
        //Other safety failures keep their established handling (including unverified server limits).
        private string GetPostgreSqlIdentifierTruncationMessage(string status)
        {
            if (!NonOracleSchemaExplorerDdlExecuteGate.TryGetPostgreSqlIdentifierTruncationDetails
                (status, out var actualBytes, out var maxIdentifierBytes))
            {
                return string.Empty;
            }

            var fallback = "PostgreSQL identifier length exceeds the limit.\r\n\r\nNew name: {0} UTF-8 bytes\r\nServer limit: {1} bytes"
                + "\r\n\r\nPostgreSQL may silently truncate the name, causing the actual identifier to differ from the requested name. The operation was blocked."
                + "\r\n\r\nShorten the name and try again.";

            return LocalizationHelper.GetLanguageString(fallback, "form", GetType().Name, "msg", "PostgreSqlIdentifierWouldTruncate", "Text")
                   .Replace("{0}", actualBytes.ToString(System.Globalization.CultureInfo.InvariantCulture))
                   .Replace("{1}", maxIdentifierBytes.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private string GetNonOracleF7TableDdlFailureMessage(string status, string errorMessage)
        {
            var truncationMessage = GetPostgreSqlIdentifierTruncationMessage(status);

            if (!string.IsNullOrEmpty(truncationMessage))
            {
                return truncationMessage;
            }

            string messageId;
            string fallback;

            if (status.StartsWith("F7U2C3B_SQL_ERROR_SAVEPOINT_RESTORED", StringComparison.Ordinal))
            {
                return FormatPostgreSqlGuidedDdlFailure(status);
            }

            if (status.StartsWith("F7U2C3B_SAVEPOINT_", StringComparison.Ordinal))
            {
                return FormatPostgreSqlGuidedDdlFailure(status);
            }

            if (status.IndexOf("EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal) >= 0)
            {
                messageId = "NonOracleF7ExecutionOutcomeUnknown";
                fallback = "The execution result could not be confirmed. Verify the schema and transaction before retrying.";
            }
            else if (status.IndexOf("CONNECTION", StringComparison.Ordinal) >= 0)
            {
                messageId = "NonOracleF7ConnectionChanged";
                fallback = "The database connection is not the one selected. Close this form and reopen Table Comment from Schema Explorer.";
            }
            else if (status.IndexOf("TRANSACTION", StringComparison.Ordinal) >= 0)
            {
                messageId = "NonOracleF7TransactionUnsafe";
                fallback = "An unfinished or uncertain transaction prevents this schema change. Confirm its state before continuing.";
            }
            else
            {
                messageId = "NonOracleF7SafetyCheckFailed";
                fallback = "Table Comment was stopped by a safety check. Verify the selected database, table and SQL.";
            }

            //Reuse the generic localized transaction/outcome warnings. Never display Add Column-specific connection/safety text while the user is commenting a Table.
            var message = messageId == "NonOracleF7ConnectionChanged" || messageId == "NonOracleF7SafetyCheckFailed"
                ? LocalizationHelper.GetLanguageString("Table operation for {0} failed.", "form", GetType().Name, "msg", $"{EditMode}NG", "Text").Replace("{0}", TableName) + "\r\n" + fallback
                : LocalizationHelper.GetLanguageString(fallback, "form", "EditColumnForm", "msg", messageId, "Text");

            var separator = status.IndexOf(": ", StringComparison.Ordinal);
            var detail = separator >= 0 ? status.Substring(separator + 2) : errorMessage;

            if (!string.IsNullOrWhiteSpace(detail) && !detail.StartsWith("F7U", StringComparison.Ordinal))
            {
                message += "\r\n\r\n" + detail;
            }

            return message;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            //Defense in depth: programmatic PerformClick must not bypass preview-only restrictions.
            if (OfflineSqlPreviewOnly)
            {
                btnExecute.Enabled = false;
                return;
            }

            if (!_commentMetadataReady)
            {
                return; //Fail-closed: unqualified Comment metadata cannot execute DDL.
            }

            var errorMessage = string.Empty;
            var sqlText = editorSqlPreview.Text;
            var hasPendingTransactionAfterExecute = false;
            var isTransactionClosedBySqlCommand = false;

            try
            {
                if (IsOracle && RequireOracleF7ExecuteGate && EditMode != EditTableMode.None && !string.IsNullOrWhiteSpace(sqlText))
                {
                    var defaultMessage = "Oracle implicitly commits the current transaction before executing DDL, even if the DDL fails. A successful DDL is also committed afterward.\r\nCommitted changes cannot be undone with ROLLBACK. Do you want to continue?";

                    var message = LocalizationHelper.GetLanguageString
                    (
                        defaultMessage,
                        "form",
                        GetType().Name,
                        "msg",
                        "OracleDdlImplicitCommitConfirmation",
                        "Text"
                    );

                    if (MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                if (IsOracle && OracleTargetIdentity != null)
                {
                    TableDdlOperation operation = TableDdlOperation.None;

                    switch (EditMode)
                    {
                        case EditTableMode.Comment:
                            {
                                operation = TableDdlOperation.Comment;
                                break;
                            }
                        case EditTableMode.Drop:
                            {
                                operation = TableDdlOperation.Drop;
                                break;
                            }
                        case EditTableMode.Rename:
                            {
                                operation = TableDdlOperation.Rename;
                                break;
                            }
                        case EditTableMode.Truncate:
                            {
                                operation = TableDdlOperation.Truncate;
                                break;
                            }
                    }

                    var rebuiltSql = BuildTableDdlSql(operation);
                    var reader = MyGlobal.OracleReader;
                    var sessionUser = reader?.VerifiedSessionUser;
                    var connectionId = reader?.VerifiedConnectionId ?? Guid.Empty;

                    if (!OracleDdlGuiGate.CanExecute(OracleTargetIdentity, sessionUser, connectionId, editorSqlPreview.Text, rebuiltSql))
                    {
                        //Fail closed if the connection changed after Preview, or the displayed SQL no longer matches the rebuilt SQL.
                        UpdateSqlPreviewText(string.Empty);

                        btnExecute.Enabled = false;
                        return;
                    }
                }

                var newF7OracleDispatched = false;

                if (IsOracle && RequireOracleF7ExecuteGate)
                {
                    TableDdlOperation operation = TableDdlOperation.None;

                    switch (EditMode)
                    {
                        case EditTableMode.Comment:
                            {
                                operation = TableDdlOperation.Comment;
                                break;
                            }
                        case EditTableMode.Drop:
                            {
                                operation = TableDdlOperation.Drop;
                                break;
                            }
                        case EditTableMode.Rename:
                            {
                                operation = TableDdlOperation.Rename;
                                break;
                            }
                        case EditTableMode.Truncate:
                            {
                                operation = TableDdlOperation.Truncate;
                                break;
                            }
                    }

                    var guardedStatus = OracleSchemaExplorerExecuteTimeHelper.Execute
                    (
                        OracleF7Receipt,
                        OracleF7SelectedOwner,
                        TableName,
                        string.Empty,
                        false,
                        sqlText,
                        BuildTableDdlSql(operation),
                        out errorMessage,
                        out hasPendingTransactionAfterExecute,
                        out isTransactionClosedBySqlCommand,
                        out newF7OracleDispatched
                    );

                    if (!string.IsNullOrEmpty(guardedStatus))
                    {
                        var message = OracleSchemaExplorerExecuteTimeHelper.GetFailureMessage(guardedStatus, errorMessage, newF7OracleDispatched);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        if (newF7OracleDispatched)
                        {
                            UpdateSqlHistory(0, "Error", message, sqlText);
                        }

                        return;
                    }
                }

                var newF7NonOracleDispatched = false;

                if (RequireNonOracleF7TableDdlGate)
                {
                    var guardedStatus = TryExecuteNonOracleF7TableDdl
                    (
                        out errorMessage,
                        out hasPendingTransactionAfterExecute,
                        out isTransactionClosedBySqlCommand,
                        out newF7NonOracleDispatched
                    );

                    if (string.Equals(guardedStatus, "F7U2C98_USER_CANCELLED", StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(guardedStatus))
                    {
                        var message = GetNonOracleF7TableDdlFailureMessage(guardedStatus, errorMessage);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        if (newF7NonOracleDispatched)
                        {
                            UpdateSqlHistory(0, "Error", message + "\r\n" + guardedStatus, sqlText);
                        }

                        if (IsPostgreSql && hasPendingTransactionAfterExecute)
                        {
                            TransferValueToMainForm("UpdateCommitRollbackButton`");
                        }

                        return;
                    }
                }

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            if (!newF7OracleDispatched)
                            {
                                MyGlobal.OracleReader.ExecuteSingleNonQuery
                                (
                                    sqlText,
                                    out errorMessage,
                                    out hasPendingTransactionAfterExecute,
                                    out isTransactionClosedBySqlCommand
                                );
                            }

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            if (!newF7NonOracleDispatched)
                            {
                                MyGlobal.PostgreSqlReader.ExecuteGuidedDdlWithSavepoint
                                (
                                    sqlText,
                                    out errorMessage,
                                    out hasPendingTransactionAfterExecute,
                                    out isTransactionClosedBySqlCommand,
                                    out _
                                );
                            }

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            if (!newF7NonOracleDispatched)
                            {
                                MyGlobal.SqlServerReader.ExecuteSingleNonQuery
                                (
                                    sqlText,
                                    out errorMessage,
                                    out hasPendingTransactionAfterExecute,
                                    out isTransactionClosedBySqlCommand
                                );
                            }

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            if (!newF7NonOracleDispatched)
                            {
                                MyGlobal.MySqlReader.ExecuteSingleNonQuery
                                (
                                    sqlText,
                                    out errorMessage,
                                    out hasPendingTransactionAfterExecute,
                                    out isTransactionClosedBySqlCommand
                                );
                            }

                            break;
                        }
                }

                if (IsPostgreSql)
                {
                    errorMessage = FormatPostgreSqlGuidedDdlFailure(errorMessage);

                    if (!string.IsNullOrWhiteSpace(errorMessage) && hasPendingTransactionAfterExecute)
                    {
                        //A failed/uncertain Savepoint is not a completed transaction.
                        //Keep the manual Commit/Rollback controls available.
                        TransferValueToMainForm("UpdateCommitRollbackButton`");
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

                    MessageBox.Show(languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var languageText = LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}NG", "Text").Replace("{0}", TableName).Replace("{1}", txtNewTableName.Text);

                    MessageBox.Show($"{languageText}\r\n\r\n{errorMessage}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                var lineBreak = completionDecision.Succeeded ? string.Empty : $"\r\n{errorMessage}";

                //更新 SQL 歷史記錄
                UpdateSqlHistory(0, resultExecuted, $"{note}{lineBreak}", editorSqlPreview.Text);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        private string FormatPostgreSqlGuidedDdlFailure(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage) || !errorMessage.StartsWith("F7U2C3B_", StringComparison.Ordinal))
            {
                return errorMessage;
            }

            var separator = errorMessage.IndexOf(": ", StringComparison.Ordinal);
            var status = separator >= 0 ? errorMessage.Substring(0, separator) : errorMessage;
            var detail = separator >= 0 ? errorMessage.Substring(separator + 2) : string.Empty;
            string messageId;
            string fallback;

            switch (status)
            {
                case "F7U2C3B_SQL_ERROR_SAVEPOINT_RESTORED":
                    {
                        messageId = "PostgreSqlGuiDdlSavepointRestored";
                        fallback = "The SQL failed. This operation was rolled back to a savepoint. Earlier uncommitted changes remain in the transaction.";

                        break;
                    }
                case "F7U2C3B_SAVEPOINT_CREATION_FAILED":
                    {
                        messageId = "PostgreSqlGuiDdlSavepointCreationFailed";
                        fallback = "A transaction savepoint could not be created; the SQL was not executed. Check the connection and transaction state.";

                        break;
                    }
                default:
                    {
                        messageId = "PostgreSqlGuiDdlSavepointStateUncertain";
                        fallback = "The SQL execution or savepoint recovery could not be fully verified. Check the transaction and schema before continuing.";

                        break;
                    }
            }

            var message = LocalizationHelper.GetLanguageString(fallback, "form", GetType().Name, "msg", messageId, "Text");

            return string.IsNullOrWhiteSpace(detail) ? message : message + "\r\n\r\n" + detail;
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

                btnExecute.Enabled = !OfflineSqlPreviewOnly && !string.IsNullOrEmpty(sql);
                UpdateSqlPreviewText(sql);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
