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
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
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
        //B3-B1 opt-in only. A qualified F7 bridge must supply authoritative metadata-owner and selection-connection identity. Legacy callers remain unchanged until B3-B2; do not treat their DbUser/SchemaName as proof.
        internal OracleDdlTargetIdentity OracleTargetIdentity { get; set; }

        //New shared F7 only: cached SQL generation is not permission to execute.
        internal bool OfflineSqlPreviewOnly { get; set; }

        //F7-U2-C98: immutable-by-convention selection intent; never infer it from the live Reader.
        internal bool RequireNonOracleF7ExecuteGate { get; set; }

        //F7-U3-C2B3A: Existing Column requires a distinct Execute permit.
        //Preview-only and a separate click-time fail-closed guard protect this staged bridge.
        internal bool RequireNonOracleF7ExistingColumnGate { get; set; }

        //Only allows a modern MySQL Rename preview; Execute must independently verify VERSION() again.
        internal bool NonOracleF7MySqlModernRenamePreview { get; set; }

        //B3-B2A contract only. B3-B2B must wire a qualified Reader receipt into preview and last-mile Execute.
        internal MySqlVerifiedColumnDefinitionSnapshot NonOracleF7MySqlCompleteDefinitionPreview { get; set; }

        //Diagnostic text only. Does not participate in any Preview or Execute eligibility check.
        internal string NonOracleF7MySqlPreviewBlockReasonCode { get; set; }

        internal string NonOracleF7ExpectedColumn { get; set; }
        internal int NonOracleF7ExpectedSqlServerObjectId { get; set; }

        //Only existing-column operations with a qualified live-metadata Reader path may opt in.
        //MySQL CHANGE COLUMN can silently alter other attributes; keep Comment/Rename disabled until full DDL reconstruction is qualified.
        internal static bool CanEnableNonOracleExistingColumnDdl(DataSourceType provider, EditColumnMode mode)
        {
            if (provider == DataSourceType.PostgreSql || provider == DataSourceType.SqlServer)
            {
                return mode == EditColumnMode.Comment || mode == EditColumnMode.Rename || mode == EditColumnMode.Drop;
            }

            return provider == DataSourceType.MySql && mode == EditColumnMode.Drop;
        }

        //A version string alone is never authorization; caller also requires a verified Reader connection.
        internal static bool CanEnableVerifiedMySqlModernRename(DataSourceType provider, EditColumnMode mode, string serverVersion)
        {
            return provider == DataSourceType.MySql && mode == EditColumnMode.Rename
                   && MySqlColumnDdlVersionPolicy.SelectRenameSyntax(serverVersion) == MySqlColumnRenameSyntax.RenameColumn;
        }

        //A receipt is only a preview prerequisite. The Reader must issue it on a verified physical connection; the last-mile Execute gate will re-read it.
        internal static bool CanEnableVerifiedMySqlCompleteDefinition(DataSourceType provider, EditColumnMode mode, MySqlVerifiedColumnDefinitionSnapshot preview, bool modernRenamePreview)
        {
            if (provider != DataSourceType.MySql || preview == null || preview.ConnectionId == Guid.Empty
                || preview.ServerConnectionId <= 0 || string.IsNullOrWhiteSpace(preview.ServerVersion)
                || string.IsNullOrWhiteSpace(preview.DatabaseName) || string.IsNullOrWhiteSpace(preview.TableName)
                || string.IsNullOrWhiteSpace(preview.ColumnName)
                || !MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(preview.CompleteDefinition, out _))
            {
                return false;
            }

            var syntax = MySqlColumnDdlVersionPolicy.SelectRenameSyntax(preview.ServerVersion);

            return mode == EditColumnMode.Comment && syntax != MySqlColumnRenameSyntax.Unknown
                   || mode == EditColumnMode.Rename && !modernRenamePreview && syntax == MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition;
        }

        private bool CanEnableNonOracleF7ExistingColumnNow()
        {
            return CanEnableNonOracleExistingColumnDdl(_currentSourceType, EditMode)
                   || (RequireNonOracleF7ExistingColumnGate && IsMySql && EditMode == EditColumnMode.Rename && NonOracleF7MySqlModernRenamePreview)
                   || (RequireNonOracleF7ExistingColumnGate && CanEnableVerifiedMySqlCompleteDefinition(_currentSourceType, EditMode, NonOracleF7MySqlCompleteDefinitionPreview, NonOracleF7MySqlModernRenamePreview));
        }

        internal string NonOracleF7ExpectedDatabase { get; set; }
        internal string NonOracleF7ExpectedSchema { get; set; }
        internal string NonOracleF7ExpectedTable { get; set; }
        internal string NonOracleF7ExpectedConnectionString { get; set; }

        //Shared F7: the cached SQL preview is not authorization. An explicit Execute click may reconnect normally, then checks the current Oracle/PDB/target before dispatch.
        internal bool RequireOracleF7ExecuteGate { get; set; }
        internal SharedOracleMetadataCompletedLoadReceipt OracleF7Receipt { get; set; }
        internal string OracleF7SelectedOwner { get; set; }

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
        private bool _commentMetadataReady = true;

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

                _editorContextMenu.Items[1].Click += btnCopy_Click;

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

                if (!MyLibrary.IsDarkMode)
                {
                    tsTool.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                }

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

                            _commentMetadataReady = false;

                            if (!OfflineSqlPreviewOnly)
                            {
                                LoadCommentMetadata();
                            }

                            //Offline preview has no authoritative original Comment.
                            txtComment.Tag = _columnComment; //必須先指定 Tag (事件觸發時，Tag 才不會為 null)
                            txtComment.Text = _columnComment;
                            _commentMetadataReady = true;

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

                if (OfflineSqlPreviewOnly)
                {
                    Text += " [SQL Preview Only]";
                    btnExecute.Enabled = false;
                    ShowMySqlCompleteDefinitionBlockReason();
                }

                UpdateSqlPreviewCopyState();
            }
            catch (Exception ex)
            {
                if (EditMode == EditColumnMode.Comment)
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

        private void ShowMySqlCompleteDefinitionBlockReason()
        {
            if (!IsMySql || !RequireNonOracleF7ExistingColumnGate
                || (EditMode != EditColumnMode.Comment && EditMode != EditColumnMode.Rename)
                || string.IsNullOrWhiteSpace(NonOracleF7MySqlPreviewBlockReasonCode))
            {
                return;
            }

            //Only a stable classification key reaches localization; raw Reader exception text must never be shown to the user. No SQL or Execute state changes here.
            var reasonKey = MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey(NonOracleF7MySqlPreviewBlockReasonCode);
            var fallback = MySqlColumnDdlPreviewFeedbackPolicy.GetDefaultMessage(reasonKey);

            var message = LocalizationHelper.GetLanguageString
            (
                fallback, "form", GetType().Name, "msg", reasonKey, "Text"
            );

            var warning = new Label
            {
                Name = "lblMySqlCompleteDefinitionBlockReason",
                Location = new Point(17, 158),
                Size = new Size(Math.Max(120, tabColumn.SelectedTab.ClientSize.Width - 34), 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                ForeColor = Color.DarkRed,
                BackColor = Color.Transparent,
                Text = message
            };

            tabColumn.SelectedTab.Controls.Add(warning);
            warning.BringToFront();
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditColumnFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditColumnFormHeight", Size.Height.ToString());
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
            //B3-B2B: qualified MySQL F7 uses Reader-owned preview metadata. Do not call the legacy auto-reconnecting ExecuteQueryToDataTable to load the original Comment.
            if (IsMySql && RequireNonOracleF7ExistingColumnGate && NonOracleF7MySqlCompleteDefinitionPreview != null)
            {
                var definition = NonOracleF7MySqlCompleteDefinitionPreview.CompleteDefinition;

                if (!MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(definition, out var withoutComment))
                {
                    throw new InvalidOperationException("F7U3C4B3B2_DEFINITION_NOT_QUALIFIED");
                }

                var suffix = definition.Substring(withoutComment.Length);

                const string commentPrefix = " COMMENT '";

                if (suffix.Length == 0)
                {
                    _columnComment = string.Empty;
                }
                else if (suffix.StartsWith(commentPrefix, StringComparison.Ordinal) && suffix.EndsWith("'", StringComparison.Ordinal))
                {
                    _columnComment = suffix.Substring(commentPrefix.Length, suffix.Length - commentPrefix.Length - 1).Replace("''", "'");
                }
                else
                {
                    throw new InvalidOperationException("F7U3C4B3B2_COMMENT_METADATA_UNQUALIFIED");
                }

                txtNewColumnName.Tag = _columnComment.Length > 0 ? "NOTNULL" : string.Empty;
                return;
            }

            var request = new MetadataCommentQueryRequest
            {
                DataSourceType = _currentSourceType,
                ObjectKind = MetadataCommentObjectKind.Column,
                SchemaDatabase = SchemaDatabase,
                SchemaName = GetCommentMetadataOwner(),
                TableName = TableName,
                ColumnName = ColumnName
            };

            var metadataSql = MetadataCommentQueryBuilder.Build(request);

            if (string.IsNullOrWhiteSpace(metadataSql))
            {
                throw new InvalidOperationException("Column comment metadata identity is invalid. DDL execution is blocked.");
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comment");
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
                throw new InvalidOperationException("Column comment metadata did not identify exactly one valid object. DDL execution is blocked.");
            }

            _columnComment = metadata.Comment;
            txtNewColumnName.Tag = metadata.HasExistingComment ? "NOTNULL" : string.Empty;
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
                _commentMetadataReady = false;
                txtComment.Enabled = false;

                UpdateSqlPreviewText(string.Empty);

                btnExecute.Enabled = false;

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtNewColumnName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var hasValidRename = !string.IsNullOrWhiteSpace(txtNewColumnName.Text) && txtColumnName_Rename.Text != txtNewColumnName.Text;

                btnExecute.Enabled = !OfflineSqlPreviewOnly && hasValidRename;

                if (!hasValidRename)
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

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string BuildColumnDdlSql(ColumnDdlOperation operation)
        {
            if (operation == ColumnDdlOperation.Comment && !_commentMetadataReady)
            {
                return string.Empty;
            }

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

            OracleDdlTargetIdentity validatedTarget = null;

            if (IsOracle && OracleTargetIdentity != null && !TryRefreshOracleTarget(out validatedTarget))
            {
                return string.Empty;
            }

            var request = new ColumnDdlRequest
            {
                DataSourceType = _currentSourceType,
                Operation = operation,
                SchemaDatabase = SchemaDatabase,
                SchemaName = SchemaName,
                RequireOracleDdlTargetIdentity = IsOracle && OracleTargetIdentity != null,
                OracleTargetIdentity = validatedTarget,
                TableName = TableName,
                ColumnName = columnName,
                NewColumnName = txtNewColumnName.Text,
                ColumnType = ColumnType,
                Comment = txtComment.Text,
                HasExistingSqlServerComment = !TextHelper.IsNullOrEmptyTag(txtNewColumnName.Tag),
                ColumnDefinition = operation == ColumnDdlOperation.Add ? CreateAddColumnDefinition() : null
            };

            if (IsMySql && RequireNonOracleF7ExistingColumnGate && NonOracleF7MySqlCompleteDefinitionPreview != null
                && (operation == ColumnDdlOperation.Comment || (operation == ColumnDdlOperation.Rename && !NonOracleF7MySqlModernRenamePreview)))
            {
                string verifiedSql;
                var definition = NonOracleF7MySqlCompleteDefinitionPreview.CompleteDefinition;
                var built = operation == ColumnDdlOperation.Comment
                    ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, definition, out verifiedSql)
                    : MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, definition, out verifiedSql);

                return built ? verifiedSql : string.Empty;
            }

            if (IsMySql && operation == ColumnDdlOperation.Rename && RequireNonOracleF7ExistingColumnGate && NonOracleF7MySqlModernRenamePreview)
            {
                return ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql(request);
            }

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
            UpdateSqlPreviewCopyState();

            btnExecute.Enabled = !OfflineSqlPreviewOnly && _commentMetadataReady && !string.IsNullOrEmpty(editorSqlPreview.Text);
        }

        private void UpdateSqlPreviewCopyState()
        {
            var hasText = !string.IsNullOrEmpty(editorSqlPreview.Text);

            btnSelectAll.Enabled = hasText;
            btnCopy.Enabled = hasText;
        }

        private void ApplyLocalizedFormSettings()
        {
            LocalizationHelper.ApplyLanguageInfo(this, false);

            //根據目前資料庫類型，載入對應的交易安全提示
            var prompt2Id = $"lblPrompt2_{_currentSourceType}";

            lblPrompt2.Text = LocalizationHelper.GetLanguageString(lblPrompt2.Text, "form", GetType().Name, "object", prompt2Id, "Text");

            //ApplyLanguageInfo 會將一般 Label 的 ForeColor 統一套用為標準前景色，因此這些具有特殊提示語意的 Label 必須在語系套用後恢復指定顏色。
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
            _editorContextMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);

            //Copy
            _editorContextMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text); //未選取時也可透過 btnCopy_Click 全選並複製 SQL

            editorSqlPreview.ContextMenuStrip = _editorContextMenu;

            _editorContextMenu.Show(editorSqlPreview, new Point(e.X, e.Y));
        }

        //F7-U2-C98: only the NEW shared F7 Add Column entry opts in.
        //All legacy non-Oracle DDL and Oracle execution paths remain unchanged.
        private string TryExecuteNonOracleF7(out string errorMessage, out bool hasPendingTransactionAfterExecute, out bool isTransactionClosedBySqlCommand, out bool didDispatch, out bool savepointRestored)
        {
            savepointRestored = false;

            var postgresSavepointRestored = false;
            var executionError = string.Empty;
            var pending = false;
            var closed = false;
            var dispatched = false;

            if (EditMode != EditColumnMode.Add || string.IsNullOrWhiteSpace(NonOracleF7ExpectedConnectionString) || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
            {
                errorMessage = string.Empty;
                hasPendingTransactionAfterExecute = false;
                isTransactionClosedBySqlCommand = false;
                didDispatch = false;

                return "F7U2C2_CONNECTION_UNVERIFIED";
            }

            if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal))
            {
                errorMessage = string.Empty;
                hasPendingTransactionAfterExecute = false;
                isTransactionClosedBySqlCommand = false;
                didDispatch = false;

                return "F7U2C2_CONNECTION_CHANGED";
            }

            //Do not even ask for an implicit-commit confirmation when there is a pending transaction.
            var pendingOrUnknownTransaction = AppConfigHelper.IsNotCommitYet;
            var mysqlAcknowledged = _currentSourceType != DataSourceType.MySql;

            if (!pendingOrUnknownTransaction && _currentSourceType == DataSourceType.MySql)
            {
                var defaultMessage = "MySQL DDL may implicitly commit the transaction, including uncommitted changes.\r\nThis operation cannot be undone with a normal ROLLBACK. Do you want to continue?";
                var message = LocalizationHelper.GetLanguageString(defaultMessage, "form", GetType().Name, "msg", "MySqlDdlImplicitCommitConfirmation", "Text");

                mysqlAcknowledged = MessageBox.Show
                (
                    message,
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                ) == DialogResult.Yes;

                if (!mysqlAcknowledged)
                {
                    errorMessage = string.Empty;
                    hasPendingTransactionAfterExecute = false;
                    isTransactionClosedBySqlCommand = false;
                    didDispatch = false;

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
                    bool opened;

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
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    if (!opened)
                    {
                        executionError = connectionError;
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

                    var rebuilt = BuildAddColumnResult();
                    var actualPostgreSqlDatabase = _currentSourceType == DataSourceType.PostgreSql ? MyGlobal.PostgreSqlReader?.CurrentOpenDatabaseName : string.Empty;

                    var scopeStatus = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                    (
                        _currentSourceType,
                        NonOracleF7ExpectedDatabase,
                        NonOracleF7ExpectedSchema,
                        NonOracleF7ExpectedTable,
                        SchemaDatabase,
                        SchemaName,
                        TableName,
                        actualPostgreSqlDatabase,
                        editorSqlPreview.Text,
                        rebuilt != null && rebuilt.Succeeded ? rebuilt.Sql : string.Empty
                    );

                    if (!string.IsNullOrEmpty(scopeStatus))
                    {
                        return scopeStatus;
                    }

                    if (IsPostgreSql)
                    {
                        var reader = MyGlobal.PostgreSqlReader;

                        return reader == null ? "F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED" : reader.ValidateNewIdentifierLengthOnVerifiedConnection(reader.CurrentOpenConnectionId, txtColumnName_Add.Text);
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
                    var readerError = string.Empty;
                    var postgresDdlDispatched = false;

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                MyGlobal.PostgreSqlReader.ExecuteSingleNonQueryOnVerifiedConnection
                                (
                                    editorSqlPreview.Text, connectionId, out readerError, out pending, out closed, out postgresSavepointRestored, out postgresDdlDispatched
                                );

                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                MyGlobal.SqlServerReader.ExecuteSingleNonQueryOnVerifiedConnection
                                (
                                    editorSqlPreview.Text, connectionId, out readerError, out pending, out closed
                                );

                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                MyGlobal.MySqlReader.ExecuteSingleNonQueryOnVerifiedConnection
                                (
                                    editorSqlPreview.Text, connectionId, out readerError, out pending, out closed
                                );

                                break;
                            }
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    executionError = readerError;

                    if (!string.IsNullOrEmpty(readerError))
                    {
                        //A confirmed PostgreSQL SQL failure has still been dispatched and must be recorded in SQL History, without retrying the DDL.
                        dispatched = _currentSourceType == DataSourceType.PostgreSql ? postgresDdlDispatched : readerError.StartsWith("F7U2C3B_EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal);

                        return readerError;
                    }

                    dispatched = true;
                    return string.Empty;
                }
            );

            errorMessage = executionError;
            hasPendingTransactionAfterExecute = pending;
            isTransactionClosedBySqlCommand = closed;
            didDispatch = dispatched;
            savepointRestored = postgresSavepointRestored;

            return status;
        }

        //F7-U3-C2B3B: Existing Column DDL is a separate privileged path; it never falls through to generic ExecuteSingleNonQuery.
        //Recheck the selected scope, exact original Column, rebuilt SQL and live metadata after opening the verified connection.
        private string TryExecuteNonOracleF7ExistingColumn(out string errorMessage, out bool hasPendingTransactionAfterExecute, out bool isTransactionClosedBySqlCommand, out bool didDispatch, out bool savepointRestored)
        {
            errorMessage = string.Empty;
            hasPendingTransactionAfterExecute = false;
            isTransactionClosedBySqlCommand = false;
            didDispatch = false;
            savepointRestored = false;

            if (!RequireNonOracleF7ExistingColumnGate || RequireNonOracleF7ExecuteGate || !CanEnableNonOracleF7ExistingColumnNow()
                || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType || string.IsNullOrWhiteSpace(NonOracleF7ExpectedConnectionString) || string.IsNullOrWhiteSpace(NonOracleF7ExpectedColumn))
            {
                return "F7U3C2B3_COLUMN_EXECUTION_NOT_QUALIFIED";
            }

            if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal))
            {
                return "F7U2C2_CONNECTION_CHANGED";
            }

            if (IsSqlServer && NonOracleF7ExpectedSqlServerObjectId <= 0)
            {
                return "F7U3C2B3_SQLSERVER_OBJECT_ID_UNQUALIFIED";
            }


            var operation = ColumnDdlOperation.None;

            switch (EditMode)
            {
                case EditColumnMode.Comment:
                    {
                        operation = ColumnDdlOperation.Comment;
                        break;
                    }
                case EditColumnMode.Rename:
                    {
                        operation = ColumnDdlOperation.Rename;
                        break;
                    }
                case EditColumnMode.Drop:
                    {
                        operation = ColumnDdlOperation.Drop;
                        break;
                    }
                default:
                    {
                        return "F7U3C2B3_INVALID_COLUMN_OPERATION";
                    }
            }

            //Only the qualified F7 MainForm bridge can opt into this definition-bearing path.
            var completeDefinitionPreview = NonOracleF7MySqlCompleteDefinitionPreview;

            var useCompleteDefinition = RequireNonOracleF7ExistingColumnGate
                && CanEnableVerifiedMySqlCompleteDefinition (_currentSourceType, EditMode, completeDefinitionPreview, NonOracleF7MySqlModernRenamePreview);

            //Never ask for permission while a pending or unknown transaction could be lost.
            var pendingOrUnknownTransaction = AppConfigHelper.IsNotCommitYet;

            if (pendingOrUnknownTransaction)
            {
                return "F7U2C2_TRANSACTION_UNSAFE";
            }

            var mysqlAcknowledged = !IsMySql;

            if (operation == ColumnDdlOperation.Drop)
            {
                var tableScope = IsMySql ? NonOracleF7ExpectedDatabase + "." + NonOracleF7ExpectedTable
                    : IsSqlServer ? NonOracleF7ExpectedDatabase + "." + NonOracleF7ExpectedSchema + "." + NonOracleF7ExpectedTable
                    : NonOracleF7ExpectedSchema + "." + NonOracleF7ExpectedTable;

                var fallback = "Dropping a column may permanently destroy data. Review the exact target and SQL before continuing.";

                var warning = LocalizationHelper.GetLanguageString
                (
                    fallback, "form", GetType().Name, "msg", "NonOracleF7DestructiveColumnDdlConfirmation", "Text"
                );

                var prompt = warning + "\r\n" + tableScope + "." + NonOracleF7ExpectedColumn + "\r\n\r\n" + editorSqlPreview.Text;

                if (IsMySql)
                {
                    prompt += "\r\n\r\n" + LocalizationHelper.GetLanguageString
                    (
                        "MySQL DDL may implicitly commit the transaction, including uncommitted changes.\r\nThis operation cannot be undone with a normal ROLLBACK. Do you want to continue?",
                        "form",
                        GetType().Name,
                        "msg",
                        "MySqlDdlImplicitCommitConfirmation",
                        "Text"
                    );
                }

                if (MessageBox.Show(prompt, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return "F7U3C2B3_USER_CANCELLED";
                }

                //For MySQL Drop this same explicit confirmation also acknowledges implicit-commit semantics.
                mysqlAcknowledged = true;
            }

            if (IsMySql && ((operation == ColumnDdlOperation.Rename && NonOracleF7MySqlModernRenamePreview) || useCompleteDefinition))
            {
                var warning = LocalizationHelper.GetLanguageString
                (
                    "MySQL DDL may implicitly commit the transaction, including uncommitted changes.\r\nThis operation cannot be undone with a normal ROLLBACK. Do you want to continue?",
                    "form", GetType().Name, "msg", "MySqlDdlImplicitCommitConfirmation", "Text"
                );

                var target = NonOracleF7ExpectedDatabase + "." + NonOracleF7ExpectedTable + "." + NonOracleF7ExpectedColumn;
                var prompt = target + "\r\n\r\n" + editorSqlPreview.Text + "\r\n\r\n" + warning;

                if (MessageBox.Show(prompt, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return "F7U3C2B3_USER_CANCELLED";
                }

                mysqlAcknowledged = true;
            }

            var pending = false;
            var closed = false;
            var dispatched = false;
            var pgSavepointRestored = false;
            var readerError = string.Empty;

            var status = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                _currentSourceType,
                pendingOrUnknownTransaction,
                mysqlAcknowledged,
                () =>
                {
                    if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal)
                        || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
                    {
                        return "F7U2C2_CONNECTION_CHANGED";
                    }

                    //A completed-definition receipt is bound to one physical connection.
                    //Unlike an ordinary preview, this path must NEVER auto-reconnect.
                    if (useCompleteDefinition)
                    {
                        return MyGlobal.MySqlReader != null && MyGlobal.MySqlReader.CurrentOpenConnectionId == completeDefinitionPreview.ConnectionId ? string.Empty : "F7U3C4B3B2_PHYSICAL_CONNECTION_CHANGED";
                    }

                    string connectionError;
                    bool opened;

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
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    return opened ? string.Empty : "F7U2C2_CONNECTION_FAILED";
                },
                () =>
                {
                    if (!string.Equals(NonOracleF7ExpectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal)
                        || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
                    {
                        return "F7U2C2_CONNECTION_CHANGED";
                    }

                    //The UI's preview-time version is never an Execute permit.
                    //Read VERSION() again from the same Reader connection captured by TryExecuteF7.
                    if (IsMySql && operation == ColumnDdlOperation.Rename && !useCompleteDefinition)
                    {
                        if (!NonOracleF7MySqlModernRenamePreview || MyGlobal.MySqlReader == null)
                        {
                            return "F7U3C4_MYSQL_RENAME_VERSION_UNVERIFIED";
                        }

                        var versionConnectionId = MyGlobal.MySqlReader.CurrentOpenConnectionId;
                        string liveVersion;
                        string versionError;

                        if (versionConnectionId == Guid.Empty
                            || !MyGlobal.MySqlReader.TryReadServerVersionOnVerifiedConnection(versionConnectionId, out liveVersion, out versionError))
                        {
                            return "F7U3C4_MYSQL_RENAME_VERSION_UNVERIFIED";
                        }

                        if (!CanEnableVerifiedMySqlModernRename(_currentSourceType, EditMode, liveVersion))
                        {
                            return "F7U3C4_MYSQL_RENAME_VERSION_UNSUPPORTED";
                        }
                    }

                    var rebuiltSql = BuildColumnDdlSql(operation);

                    var scope = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
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
                        rebuiltSql
                    );

                    if (!string.IsNullOrEmpty(scope))
                    {
                        return scope;
                    }

                    var columnIntent = NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                    (
                        _currentSourceType, operation, true, NonOracleF7ExpectedColumn, ColumnName, editorSqlPreview.Text, rebuiltSql
                    );

                    if (!string.IsNullOrEmpty(columnIntent))
                    {
                        return columnIntent;
                    }

                    //B3-B2B: bypass ordinary auto-reconnecting metadata reads. Reader verifies the live definition, version, physical session and displayed SQL here, and repeats the check immediately before the sole DDL dispatch.
                    if (useCompleteDefinition)
                    {
                        var reader = MyGlobal.MySqlReader;
                        var id = reader?.CurrentOpenConnectionId ?? Guid.Empty;
                        MySqlVerifiedColumnDefinitionSnapshot live;
                        string definitionError;

                        if (reader == null || id != completeDefinitionPreview.ConnectionId
                            || !reader.TryReadVerifiedColumnDefinitionOnConnection
                            (
                                id, completeDefinitionPreview.DatabaseName, completeDefinitionPreview.TableName,
                                completeDefinitionPreview.ColumnName, out live, out definitionError
                            ))
                        {
                            return "F7U3C4B3B2_DEFINITION_UNVERIFIED";
                        }

                        var verifiedRequest = new ColumnDdlRequest
                        {
                            DataSourceType = DataSourceType.MySql,
                            Operation = operation,
                            SchemaDatabase = SchemaDatabase,
                            TableName = TableName,
                            ColumnName = ColumnName,
                            NewColumnName = txtNewColumnName.Text,
                            Comment = txtComment.Text
                        };

                        return MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                        (
                            operation, completeDefinitionPreview, live, verifiedRequest, editorSqlPreview.Text
                        );
                    }

                    //A cached F7 row is not sufficient: ask the current Reader for the named live Column.
                    var metadataSql = MetadataCommentQueryBuilder.Build
                    (
                        new MetadataCommentQueryRequest
                        {
                            DataSourceType = _currentSourceType,
                            ObjectKind = MetadataCommentObjectKind.Column,
                            SchemaDatabase = SchemaDatabase,
                            SchemaName = SchemaName,
                            TableName = TableName,
                            ColumnName = NonOracleF7ExpectedColumn
                        }
                    );

                    if (string.IsNullOrWhiteSpace(metadataSql))
                    {
                        return "F7U3C2B3_COLUMN_METADATA_UNQUALIFIED";
                    }

                    System.Data.DataTable liveData;

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                liveData = MyGlobal.PostgreSqlReader.ExecuteMetadataQueryToDataTable(metadataSql);
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                liveData = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(metadataSql, false);
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                liveData = MyGlobal.MySqlReader.ExecuteQueryToDataTable(metadataSql, false);
                                break;
                            }
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    if (!MetadataCommentQualificationPolicy.TryQualify(liveData, out var liveMetadata))
                    {
                        return "F7U3C2B3_COLUMN_NOT_FOUND_OR_UNVERIFIED";
                    }

                    if (IsSqlServer)
                    {
                        //SQL Server ObjectID is part of the captured F7 identity; a dropped/recreated same-name table must not inherit permission.
                        var quotedDatabase = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.SqlServer, SchemaDatabase);
                        var schemaValue = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(DataSourceType.SqlServer, SchemaName);
                        var tableValue = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(DataSourceType.SqlServer, TableName);

                        if (string.IsNullOrWhiteSpace(quotedDatabase) || string.IsNullOrWhiteSpace(schemaValue) || string.IsNullOrWhiteSpace(tableValue))
                        {
                            return "F7U3C2B3_SQLSERVER_OBJECT_ID_UNQUALIFIED";
                        }

                        var identitySql = "SELECT t.object_id AS LiveObjectId FROM " + quotedDatabase + ".sys.tables t "
                            + "INNER JOIN " + quotedDatabase + ".sys.schemas s ON s.schema_id = t.schema_id "
                            + "WHERE s.name = N'" + schemaValue.Replace("'", "''")
                            + "' AND t.name = N'" + tableValue.Replace("'", "''") + "'";

                        var identityRows = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(identitySql, false);

                        if (identityRows == null || identityRows.Rows.Count != 1 || !identityRows.Columns.Contains("LiveObjectId")
                            || !int.TryParse(Convert.ToString(identityRows.Rows[0]["LiveObjectId"]), out var liveObjectId)
                            || liveObjectId != NonOracleF7ExpectedSqlServerObjectId)
                        {
                            return "F7U3C2B3_SQLSERVER_OBJECT_ID_CHANGED";
                        }
                    }

                    //A concurrent SQL Server Comment add/remove changes which extended-property procedure is legal.
                    if (IsSqlServer && operation == ColumnDdlOperation.Comment && liveMetadata.HasExistingComment != !TextHelper.IsNullOrEmptyTag(txtNewColumnName.Tag))
                    {
                        return "F7U3C2B3_COLUMN_COMMENT_STATE_CHANGED";
                    }

                    //Preserve the selected/live Column identity checks above. Guard only the NEW rename target.
                    if (IsPostgreSql && operation == ColumnDdlOperation.Rename)
                    {
                        var reader = MyGlobal.PostgreSqlReader;

                        return reader == null ? "F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED" : reader.ValidateNewIdentifierLengthOnVerifiedConnection(reader.CurrentOpenConnectionId, txtNewColumnName.Text);
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
                    //Never dispatch legacy CHANGE COLUMN through the modern, definition-free Rename permit.
                    if (IsMySql && operation == ColumnDdlOperation.Rename && !useCompleteDefinition
                        && (!NonOracleF7MySqlModernRenamePreview || !string.Equals(editorSqlPreview.Text, BuildColumnDdlSql(operation), StringComparison.Ordinal)))
                    {
                        return "F7U3C4_MYSQL_RENAME_SQL_NOT_VERIFIED";
                    }

                    if (!NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(_currentSourceType, operation, editorSqlPreview.Text))
                    {
                        return "F7U3C2B3_COLUMN_SQL_OPERATION_MISMATCH";
                    }

                    var sql = editorSqlPreview.Text;

                    //Conservatively mark an attempted Reader call, including exceptions with uncertain outcomes.
                    dispatched = true;

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                MyGlobal.PostgreSqlReader.ExecuteColumnDdlOnVerifiedConnection
                                (
                                    sql, connectionId, operation,
                                    out readerError, out pending, out closed, out pgSavepointRestored, out var pgDispatched
                                );

                                if (!string.IsNullOrEmpty(readerError))
                                {
                                    dispatched = pgDispatched;
                                }

                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                MyGlobal.SqlServerReader.ExecuteColumnDdlOnVerifiedConnection
                                (
                                    sql, connectionId, operation, out readerError, out pending, out closed
                                );

                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                if (useCompleteDefinition)
                                {
                                    MyGlobal.MySqlReader.ExecuteCompleteColumnDdlOnVerifiedConnection
                                    (
                                        sql, connectionId, operation, completeDefinitionPreview,
                                        out readerError, out pending, out closed
                                    );
                                }
                                else
                                {
                                    MyGlobal.MySqlReader.ExecuteColumnDdlOnVerifiedConnection
                                    (
                                        sql, connectionId, operation, out readerError, out pending, out closed
                                    );
                                }

                                break;
                            }
                        default:
                            {
                                return "F7U2C2_INVALID_PROVIDER";
                            }
                    }

                    return readerError ?? "F7U2C2_EXECUTION_OUTCOME_UNKNOWN";
                }
            );

            errorMessage = readerError;
            hasPendingTransactionAfterExecute = pending;
            isTransactionClosedBySqlCommand = closed;
            didDispatch = dispatched;
            savepointRestored = pgSavepointRestored;

            return status;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            //Defense in depth: a preview-only form must never issue DDL, including programmatic clicks.
            if (OfflineSqlPreviewOnly)
            {
                btnExecute.Enabled = false;
                return;
            }

            //An unsupported Existing Column operation must never reach the generic SQL execution switch.
            if (RequireNonOracleF7ExistingColumnGate && (RequireNonOracleF7ExecuteGate || !CanEnableNonOracleF7ExistingColumnNow()))
            {
                btnExecute.Enabled = false;
                return;
            }

            if (!_commentMetadataReady)
            {
                return; //Fail-closed: unqualified Comment metadata cannot execute DDL.
            }

            var errorMessage = string.Empty;
            var hasPendingTransactionAfterExecute = false;
            var isTransactionClosedBySqlCommand = false;

            try
            {
                if (IsOracle && RequireOracleF7ExecuteGate && EditMode != EditColumnMode.None && !string.IsNullOrWhiteSpace(editorSqlPreview.Text))
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
                    ColumnDdlOperation operation = ColumnDdlOperation.None;

                    switch (EditMode)
                    {
                        case EditColumnMode.Comment:
                            {
                                operation = ColumnDdlOperation.Comment;
                                break;
                            }
                        case EditColumnMode.Drop:
                            {
                                operation = ColumnDdlOperation.Drop;
                                break;
                            }
                        case EditColumnMode.Rename:
                            {
                                operation = ColumnDdlOperation.Rename;
                                break;
                            }
                        case EditColumnMode.Add:
                            {
                                operation = ColumnDdlOperation.Add;
                                break;
                            }
                    }

                    var rebuiltSql = BuildColumnDdlSql(operation);
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
                    ColumnDdlOperation operation = ColumnDdlOperation.None;

                    switch (EditMode)
                    {
                        case EditColumnMode.Comment:
                            {
                                operation = ColumnDdlOperation.Comment;
                                break;
                            }
                        case EditColumnMode.Drop:
                            {
                                operation = ColumnDdlOperation.Drop;
                                break;
                            }
                        case EditColumnMode.Rename:
                            {
                                operation = ColumnDdlOperation.Rename;
                                break;
                            }
                        case EditColumnMode.Add:
                            {
                                operation = ColumnDdlOperation.Add;
                                break;
                            }
                    }

                    var guardedStatus = OracleSchemaExplorerExecuteTimeHelper.Execute
                    (
                        OracleF7Receipt,
                        OracleF7SelectedOwner,
                        TableName,
                        ColumnName,
                        EditMode != EditColumnMode.Add,
                        editorSqlPreview.Text,
                        BuildColumnDdlSql(operation),
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
                            UpdateSqlHistory(0, "Error", message, editorSqlPreview.Text);
                        }

                        return;
                    }
                }

                var newF7NonOracleDispatched = false;
                var newF7NonOracleSavepointRestored = false;

                if (RequireNonOracleF7ExistingColumnGate)
                {
                    var existingColumnStatus = TryExecuteNonOracleF7ExistingColumn
                    (
                        out errorMessage,
                        out hasPendingTransactionAfterExecute,
                        out isTransactionClosedBySqlCommand,
                        out newF7NonOracleDispatched,
                        out newF7NonOracleSavepointRestored
                    );

                    if (string.Equals(existingColumnStatus, "F7U3C2B3_USER_CANCELLED", StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(existingColumnStatus))
                    {
                        var message = GetPostgreSqlIdentifierTruncationMessage(existingColumnStatus);

                        if (string.IsNullOrEmpty(message))
                        {
                            var failure = LocalizationHelper.GetLanguageString
                            (
                                "The existing Column operation was blocked by a safety check. Verify the live column, connection and SQL before retrying.",
                                "form", GetType().Name, "msg", "NonOracleF7ExistingColumnSafetyCheckFailed", "Text"
                            );

                            message = failure + "\r\n\r\n" + existingColumnStatus;

                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                message += "\r\n" + errorMessage;
                            }
                        }

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        if (newF7NonOracleDispatched || existingColumnStatus.IndexOf("EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal) >= 0)
                        {
                            UpdateSqlHistory(0, "Error", message, editorSqlPreview.Text);
                        }

                        if (IsPostgreSql && hasPendingTransactionAfterExecute)
                        {
                            TransferValueToMainForm("UpdateCommitRollbackButton`");
                        }

                        return;
                    }

                    //Successful Reader dispatch suppresses all legacy provider paths below.
                    newF7NonOracleDispatched = true;
                }

                if (RequireNonOracleF7ExecuteGate)
                {
                    var guardedStatus = TryExecuteNonOracleF7
                    (
                        out errorMessage,
                        out hasPendingTransactionAfterExecute,
                        out isTransactionClosedBySqlCommand,
                        out newF7NonOracleDispatched,
                        out newF7NonOracleSavepointRestored
                    );

                    if (string.Equals(guardedStatus, "F7U2C98_USER_CANCELLED", StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(guardedStatus))
                    {
                        var truncationMessage = GetPostgreSqlIdentifierTruncationMessage(guardedStatus);

                        if (!string.IsNullOrEmpty(truncationMessage))
                        {
                            MessageBox.Show(truncationMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        var outcomeUnknown = guardedStatus.StartsWith("F7U2C3B_EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal) || string.Equals(guardedStatus, "F7U2C2_EXECUTION_OUTCOME_UNKNOWN", StringComparison.Ordinal);
                        string messageId;
                        string defaultMessage;

                        if (guardedStatus.StartsWith("F7U2C3B_SQL_ERROR_SAVEPOINT_RESTORED", StringComparison.Ordinal))
                        {
                            messageId = "PostgreSqlGuiDdlSavepointRestored";
                            defaultMessage = "The SQL failed. This operation was rolled back to a savepoint. Earlier uncommitted changes remain in the transaction.";
                        }
                        else if (guardedStatus.StartsWith("F7U2C3B_SAVEPOINT_CREATION_FAILED", StringComparison.Ordinal))
                        {
                            messageId = "PostgreSqlGuiDdlSavepointCreationFailed";
                            defaultMessage = "A transaction savepoint could not be created; the SQL was not executed. Check the connection and transaction state.";
                        }
                        else if (guardedStatus.StartsWith("F7U2C3B_SAVEPOINT_", StringComparison.Ordinal))
                        {
                            messageId = "PostgreSqlGuiDdlSavepointStateUncertain";
                            defaultMessage = "The SQL execution or savepoint recovery could not be fully verified. Check the transaction and schema before continuing.";
                        }
                        else if (outcomeUnknown)
                        {
                            messageId = "NonOracleF7ExecutionOutcomeUnknown";
                            defaultMessage = "The execution result could not be confirmed.\r\nVerify the database schema and transaction status before retrying.";
                        }
                        else if (guardedStatus.IndexOf("CONNECTION_UNVERIFIED", StringComparison.Ordinal) >= 0)
                        {
                            messageId = "NonOracleF7ConnectionUnverified";
                            defaultMessage = "The current database connection could not be verified.\r\nReconnect, refresh Schema Explorer, and reopen the Add Column form.";
                        }
                        else if (guardedStatus.IndexOf("CONNECTION_CHANGED", StringComparison.Ordinal) >= 0)
                        {
                            messageId = "NonOracleF7ConnectionChanged";
                            defaultMessage = "The database connection has changed.\r\nClose this form and reopen Add Column from the current Schema Explorer.";
                        }
                        else if (guardedStatus.IndexOf("TRANSACTION_UNSAFE", StringComparison.Ordinal) >= 0)
                        {
                            messageId = "NonOracleF7TransactionUnsafe";
                            defaultMessage = "There is an unfinished or uncertain transaction.\r\nConfirm its state before attempting another schema change.";
                        }
                        else
                        {
                            messageId = "NonOracleF7SafetyCheckFailed";
                            defaultMessage = "The Add Column operation was stopped by a safety check.\r\nVerify the database, table and SQL, then reopen this form.";
                        }

                        var message = LocalizationHelper.GetLanguageString
                        (
                            defaultMessage,
                            "form",
                            GetType().Name,
                            "msg",
                            messageId,
                            "Text"
                        );

                        var detail = string.Empty;
                        var colon = guardedStatus.IndexOf(": ", StringComparison.Ordinal);

                        if (colon >= 0)
                        {
                            detail = guardedStatus.Substring(colon + 2).Trim();
                        }
                        else if (!string.IsNullOrWhiteSpace(errorMessage) && !errorMessage.StartsWith("F7U2", StringComparison.Ordinal))
                        {
                            detail = errorMessage.Trim();
                        }

                        if (!string.IsNullOrWhiteSpace(detail))
                        {
                            message += "\r\n\r\n" + detail;
                        }

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        if (newF7NonOracleDispatched)
                        {
                            //Machine-readable reason is retained only in SQL History for diagnostics.
                            UpdateSqlHistory(0, "Error", message + "\r\n" + guardedStatus, editorSqlPreview.Text);
                        }

                        //ROLLBACK TO SAVEPOINT does not close the managed transaction.
                        //Notify pending only when the Reader has confirmed it remains active and manual transaction resolution may be required.
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
                                    editorSqlPreview.Text,
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
                                    editorSqlPreview.Text,
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
                                    editorSqlPreview.Text,
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
                                    editorSqlPreview.Text,
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
                    Result = EditMode == EditColumnMode.Comment ? string.Empty : "REFRESH";
                    resultExecuted = "Complete";

                    btnExecute.Enabled = false;
                    txtComment.Enabled = false;
                    txtNewColumnName.Enabled = false;

                    if (EditMode == EditColumnMode.Add)
                    {
                        SetAddColumnEditorEnabled(false);
                    }

                    var message = EditMode == EditColumnMode.Add ? GetAddColumnExecutionMessage(true) : LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}OK", "Text").Replace("{0}", ColumnName).Replace("{1}", txtNewColumnName.Text);

                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    resultExecuted = "Error";

                    var message = EditMode == EditColumnMode.Add ? GetAddColumnExecutionMessage(false) : LocalizationHelper.GetLanguageString("", "form", GetType().Name, "msg", $"{EditMode}NG", "Text").Replace("{0}", ColumnName).Replace("{1}", txtNewColumnName.Text);

                    MessageBox.Show($"{message}\r\n\r\n{errorMessage}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                var messageFinal = completionDecision.Succeeded ? string.Empty : $"\r\n{errorMessage}";

                //更新 SQL 歷史記錄
                UpdateSqlHistory(0, resultExecuted, $"{note}{messageFinal}", editorSqlPreview.Text);

                UpdateMainFormTransactionState(completionDecision);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //F7-U3-C3: explain confirmed truncation with the exact measured UTF-8 bytes and live server limit.
        //Other safety failures keep their established handling (including unverified server limits).
        private string GetPostgreSqlIdentifierTruncationMessage(string status)
        {
            if (!NonOracleSchemaExplorerDdlExecuteGate.TryGetPostgreSqlIdentifierTruncationDetails(status, out var actualBytes, out var maxIdentifierBytes))
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
