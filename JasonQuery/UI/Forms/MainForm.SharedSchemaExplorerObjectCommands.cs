using IconLibrary;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //L2-C3-C2: guarded read-only object actions. Structural changes remain disabled until L2-C3-D.
    public partial class MainForm
    {
        private void SharedSchemaObjectMenuClick(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is SharedSchemaExplorerMenuCommand command) || !item.Enabled || !ReferenceEquals(item.Owner, _sharedSchemaCopyContextMenu))
            {
                return;
            }

            var selection = _sharedSchemaCopySelection;
            var state = CaptureSharedLeftPanelHostState();
            var context = CaptureLeftPanelContext();
            var activeTab = tabControl1.SelectedTab;
            var activeForm = activeTab?.Control;

            if (selection == null || state == null || context == null || !context.IsConnected || !context.SchemaReady
                || state.ConnectionRevision != _sharedSchemaMenuConnectionRevision
                || !ReferenceEquals(activeForm, _sharedSchemaMenuActiveForm)
                || context.ActiveTarget != _sharedSchemaMenuTarget
                || selection.SourceType != _currentSourceType
                || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType
                || (!SharedSchemaDdlInteractionPolicy.CanOpenGenerator(context.ActiveTarget, selection, command, CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)) && !CanHandleSharedSchemaExplorerSessionAction())
                || !SharedSchemaExplorerObjectCommandPolicy.CanExecute(context.ActiveTarget, selection, command))
            {
                return;
            }

            //Tab remains the active, correctly-typed receiving form at execution time.
            if (context.ActiveTarget == LeftPanelTargetKind.QueryForm && !(activeForm is QueryForm))
            {
                return;
            }

            if (context.ActiveTarget == LeftPanelTargetKind.SchemaBrowserForm && !(activeForm is SchemaBrowserForm))
            {
                return;
            }

            try
            {
                switch (command)
                {
                    case SharedSchemaExplorerMenuCommand.ShowDdl:
                        {
                            ShowSharedSchemaDdl(selection);
                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.OpenSchemaBrowser:
                        {
                            OpenSharedSchemaBrowser(selection);
                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.GenerateSqlStatement:
                        {
                            OpenSharedGenerateSql(selection, activeTab.AccessibleDescription);
                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.CommentTable:
                    case SharedSchemaExplorerMenuCommand.DropTable:
                    case SharedSchemaExplorerMenuCommand.RenameTable:
                    case SharedSchemaExplorerMenuCommand.TruncateTable:
                        {
                            if (_currentSourceType == DataSourceType.Oracle)
                            {
                                TryOpenSharedOracleTableDdl(selection, command);
                            }
                            else
                            {
                                TryOpenSharedNonOracleTableDdl(selection, command);
                            }

                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.AddColumn:
                        {
                            if (_currentSourceType == DataSourceType.Oracle)
                            {
                                TryOpenSharedOracleColumnDdl(selection, command);
                            }
                            else
                            {
                                TryOpenSharedNonOracleAddColumnPreview(selection);
                            }

                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.CommentColumn:
                    case SharedSchemaExplorerMenuCommand.DropColumn:
                    case SharedSchemaExplorerMenuCommand.RenameColumn:
                        {
                            if (_currentSourceType == DataSourceType.Oracle)
                            {
                                TryOpenSharedOracleColumnDdl(selection, command);
                            }
                            else
                            {
                                TryOpenSharedNonOracleColumnDdl(selection, command);
                            }

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //Click-time identity and row re-resolution: a menu captured before a rebind or a reconnect cannot authorize a DDL form, including reconnect as the same user.
        private bool TryOpenSharedOracleTableDdl(SchemaExplorerSelectionInfo captured, SharedSchemaExplorerMenuCommand command)
        {
            if (_currentSourceType != DataSourceType.Oracle || _sharedSchemaMetadataSnapshot == null || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid) || !SharedSchemaDdlInteractionPolicy.CanOpenGenerator(CaptureLeftPanelContext().ActiveTarget, captured, command, true))
            {
                return false;
            }

            var current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, captured.DisplayRowIndex);

            if (!OracleSharedTableDdlBridgePolicy.IsSameTableDataRow(captured, current))
            {
                return false;
            }

            //This is an offline, copy-only SQL generator. Never synthesize a current Oracle DDL identity from cached F7 data.
            if (string.IsNullOrWhiteSpace(current.OracleSelectedOwner))
            {
                return false;
            }

            EditTableMode mode;
            string icon;

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.CommentTable:
                    {
                        mode = EditTableMode.Comment;
                        icon = "Table Comment.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.DropTable:
                    {
                        mode = EditTableMode.Drop;
                        icon = "Table Drop.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.RenameTable:
                    {
                        mode = EditTableMode.Rename;
                        icon = "Table Rename.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                    {
                        mode = EditTableMode.Truncate;
                        icon = "Table Truncate.ico";

                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            using (var form = new EditTableForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, icon);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;

                //Keep the raw metadata owner case, including quoted mixed-case Oracle identifiers.
                form.SchemaName = "\"" + current.OracleSelectedOwner.Replace("\"", "\"\"") + "\"";

                form.TableName = current.SchemaName;
                form.EditMode = mode;
                form.OfflineSqlPreviewOnly = false;
                form.RequireOracleF7ExecuteGate = true;
                form.OracleF7Receipt = DatabaseSqlExecutor.LastOracleCompletedLoadReceipt;
                form.OracleF7SelectedOwner = current.OracleSelectedOwner;
                form.OracleTargetIdentity = null;

                UIHelper.ApplyEditTableFormSize(form);
                form.ShowDialog(this);

                if (string.Equals(form.Result, "REFRESH", StringComparison.Ordinal) && !IsDisposed && !Disposing && CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
                {
                    RefreshSharedSchemaExplorerPreviewMetadata();
                }
            }

            return true;
        }

        //B3-B2-C2: the click-time raw column row must match the menu-time row exactly.
        //The form rechecks the live OracleReader and byte-exact rebuilt SQL before Execute.
        private bool TryOpenSharedOracleColumnDdl(SchemaExplorerSelectionInfo captured, SharedSchemaExplorerMenuCommand command)
        {
            if (_currentSourceType != DataSourceType.Oracle || _sharedSchemaMetadataSnapshot == null || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid) || !SharedSchemaDdlInteractionPolicy.CanOpenGenerator(CaptureLeftPanelContext().ActiveTarget, captured, command, true))
            {
                return false;
            }

            //F7-U1: Table Add Column has no pre-existing column identity. Re-resolve the
            //same qualified Table object; all other column actions retain raw Column checks.
            var fromTable = command == SharedSchemaExplorerMenuCommand.AddColumn && OracleSharedTableDdlBridgePolicy.IsTableObjectRow(captured);

            var current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, captured.DisplayRowIndex);

            if (fromTable && !OracleSharedTableDdlBridgePolicy.IsSameTableDataRow(captured, current))
            {
                return false;
            }

            if (!fromTable && (!OracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(captured, current) || !OracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(current)))
            {
                return false;
            }

            EditColumnMode mode;
            string icon;

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.AddColumn:
                    {
                        mode = EditColumnMode.Add;
                        icon = "Column Add.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                    {
                        mode = EditColumnMode.Comment;
                        icon = "Column Comment.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.DropColumn:
                    {
                        mode = EditColumnMode.Drop;
                        icon = "Column Drop.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        mode = EditColumnMode.Rename;
                        icon = "Column Rename.ico";

                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            using (var form = new EditColumnForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, icon);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SourceType = DataSourceType.Oracle;
                form.SchemaName = "\"" + current.OracleSelectedOwner.Replace("\"", "\"\"") + "\"";

                //Table entry supplies owner/table only. Never invent an existing Column.
                form.TableName = fromTable ? current.SchemaName : current.OracleMetadataTableName;

                form.ColumnName = fromTable ? string.Empty : current.OracleMetadataColumnName;
                form.EditMode = mode;
                form.OfflineSqlPreviewOnly = false;
                form.RequireOracleF7ExecuteGate = true;
                form.OracleF7Receipt = DatabaseSqlExecutor.LastOracleCompletedLoadReceipt;
                form.OracleF7SelectedOwner = current.OracleSelectedOwner;
                form.OracleTargetIdentity = null;

                UIHelper.ApplyEditColumnFormSize(form);
                form.ShowDialog(this);

                if (string.Equals(form.Result, "REFRESH", StringComparison.Ordinal) && !IsDisposed && !Disposing && CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
                {
                    RefreshSharedSchemaExplorerPreviewMetadata();
                }
            }

            return true;
        }

        //U3-B2: one click-time qualified Table form for Comment/Rename/Drop/Truncate, never a database authorization by itself.
        private bool TryOpenSharedNonOracleTableDdl(SchemaExplorerSelectionInfo captured, SharedSchemaExplorerMenuCommand command)
        {
            if (captured == null || captured.SourceType != _currentSourceType || _currentSourceType == DataSourceType.Oracle
                || _sharedSchemaMetadataSnapshot == null || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerObjectCommandPolicy.CanExecute(CaptureLeftPanelContext().ActiveTarget, captured, command))
            {
                return false;
            }

            EditTableMode mode;
            string icon;

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.CommentTable:
                    {
                        mode = EditTableMode.Comment;
                        icon = "Table Comment.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.RenameTable:
                    {
                        mode = EditTableMode.Rename;
                        icon = "Table Rename.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.DropTable:
                    {
                        mode = EditTableMode.Drop;
                        icon = "Table Drop.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                    {
                        mode = EditTableMode.Truncate;
                        icon = "Table Truncate.ico";

                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            var current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, captured.DisplayRowIndex);

            if (!NonOracleSharedAddColumnPreviewPolicy.IsSameSelection(captured, current)
                || !NonOracleSharedAddColumnPreviewPolicy.TryCreate(current, out var target)
                || current.Kind != SchemaExplorerSelectionKind.SchemaObject || current.IsColumnInfoRow)
            {
                return false;
            }

            using (var form = new EditTableForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, icon);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SchemaDatabase = target.SchemaDatabase;
                form.SchemaName = target.SchemaName;
                form.TableName = target.TableName;
                form.EditMode = mode;
                form.OfflineSqlPreviewOnly = false;
                form.RequireOracleF7ExecuteGate = false;
                form.RequireNonOracleF7TableDdlGate = true;
                form.NonOracleF7ExpectedDatabase = target.SourceType == DataSourceType.PostgreSql ? DatabaseSqlExecutor.DatabaseName : target.SchemaDatabase;
                form.NonOracleF7ExpectedSchema = target.SchemaName;
                form.NonOracleF7ExpectedTable = target.TableName;
                form.NonOracleF7ExpectedConnectionString = DatabaseSqlExecutor.DbConnectionString;

                UIHelper.ApplyEditTableFormSize(form);
                form.ShowDialog(this);

                //Comment does not change structural metadata; Rename and Drop do. PG/SQL Server wait for explicit Commit.
                if (mode != EditTableMode.Comment && mode != EditTableMode.Truncate && string.Equals(form.Result, "REFRESH", StringComparison.Ordinal)
                    && !IsDisposed && !Disposing && CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
                {
                    if (target.SourceType == DataSourceType.MySql)
                    {
                        RefreshSharedSchemaExplorerPreviewMetadata();
                    }
                    else if (target.SourceType == DataSourceType.PostgreSql || target.SourceType == DataSourceType.SqlServer)
                    {
                        MarkF7TransactionBoundAddColumn();
                    }
                }
            }

            return true;
        }

        //F7-U3-C2B3A: re-resolve the selected raw Column before opening the non-Oracle form.
        //This first UI bridge is SQL Preview Only; no Existing Column DDL may execute yet.
        private bool TryOpenSharedNonOracleColumnDdl(SchemaExplorerSelectionInfo captured, SharedSchemaExplorerMenuCommand command)
        {
            if (captured == null || captured.SourceType != _currentSourceType || _currentSourceType == DataSourceType.Oracle
                || _sharedSchemaMetadataSnapshot == null || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerObjectCommandPolicy.CanExecute(CaptureLeftPanelContext().ActiveTarget, captured, command))
            {
                return false;
            }

            EditColumnMode mode;
            string icon;

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                    {
                        mode = EditColumnMode.Comment;
                        icon = "Column Comment.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        mode = EditColumnMode.Rename;
                        icon = "Column Rename.ico";

                        break;
                    }
                case SharedSchemaExplorerMenuCommand.DropColumn:
                    {
                        mode = EditColumnMode.Drop;
                        icon = "Column Drop.ico";

                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            var current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, captured.DisplayRowIndex);

            if (!NonOracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(captured, current)
                || !NonOracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(current)
                || !NonOracleSharedAddColumnPreviewPolicy.TryCreate(current, out var target))
            {
                return false;
            }

            var sqlServerObjectId = 0;

            if (target.SourceType == DataSourceType.SqlServer && (!int.TryParse(current.ObjectId, out sqlServerObjectId) || sqlServerObjectId <= 0))
            {
                return false;
            }

            //A query-only Editor execution may disconnect the shared Reader.
            //An explicit F7 Rename request may restore the connection when no transaction is pending.
            //Preview-time capability is never Execute authorization.
            //Legacy MySQL CHANGE COLUMN and COMMENT remain SQL Preview Only.
            var modernMySqlRenamePreview = false;

            if (target.SourceType == DataSourceType.MySql && mode == EditColumnMode.Rename
                && !AppConfigHelper.IsNotCommitYet && MyGlobal.MySqlReader != null
                && DatabaseSqlExecutor.CurrentDataSource == DataSourceType.MySql)
            {
                var expectedConnectionString = DatabaseSqlExecutor.DbConnectionString;
                var reader = MyGlobal.MySqlReader;

                if (!string.IsNullOrWhiteSpace(expectedConnectionString)
                    && reader.TryEnsureConnectionOpen(expectedConnectionString, out _)
                    && string.Equals(expectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal)
                    && DatabaseSqlExecutor.CurrentDataSource == DataSourceType.MySql)
                {
                    var previewConnectionId = reader.CurrentOpenConnectionId;
                    string serverVersion;
                    string versionError;

                    if (previewConnectionId != Guid.Empty && reader.TryReadServerVersionOnVerifiedConnection(previewConnectionId, out serverVersion, out versionError))
                    {
                        modernMySqlRenamePreview = EditColumnForm.CanEnableVerifiedMySqlModernRename
                        (
                            target.SourceType, mode, serverVersion
                        );
                    }
                }
            }

            //B3-B2B: a verified definition receipt is an opt-in preview prerequisite, NOT Execute authorization.
            //Never reconnect to obtain one while a transaction is pending. Modern Rename keeps its original path.
            MySqlVerifiedColumnDefinitionSnapshot completeDefinitionPreview = null;
            var completeDefinitionBlockReason = string.Empty;

            var requiresMySqlDefinition = target.SourceType == DataSourceType.MySql && (mode == EditColumnMode.Comment
                || (mode == EditColumnMode.Rename && !modernMySqlRenamePreview));

            if (requiresMySqlDefinition)
            {
                completeDefinitionBlockReason = "F7U3C4B3_DEFINITION_UNVERIFIED";

                if (AppConfigHelper.IsNotCommitYet)
                {
                    completeDefinitionBlockReason = "F7U2C2_TRANSACTION_UNSAFE";
                }
                else if (MyGlobal.MySqlReader == null || DatabaseSqlExecutor.CurrentDataSource != DataSourceType.MySql)
                {
                    completeDefinitionBlockReason = "F7U3C4B3_CONNECTION_UNVERIFIED";
                }
                else
                {
                    var expectedConnectionString = DatabaseSqlExecutor.DbConnectionString;
                    var reader = MyGlobal.MySqlReader;
                    string openError;

                    if (string.IsNullOrWhiteSpace(expectedConnectionString) || !reader.TryEnsureConnectionOpen(expectedConnectionString, out openError))
                    {
                        completeDefinitionBlockReason = "F7U3C4B3_CONNECTION_UNVERIFIED";
                    }
                    else if (!string.Equals(expectedConnectionString, DatabaseSqlExecutor.DbConnectionString, StringComparison.Ordinal)
                             || DatabaseSqlExecutor.CurrentDataSource != DataSourceType.MySql)
                    {
                        completeDefinitionBlockReason = "F7U3C4B3_CONNECTION_CHANGED";
                    }
                    else
                    {
                        var connectionId = reader.CurrentOpenConnectionId;
                        MySqlVerifiedColumnDefinitionSnapshot candidate;
                        string readFailure;

                        if (connectionId == Guid.Empty)
                        {
                            completeDefinitionBlockReason = "F7U3C4B3_CONNECTION_UNVERIFIED";
                        }
                        else if (!reader.TryReadVerifiedColumnDefinitionOnConnection(connectionId, target.SchemaDatabase, target.TableName, current.NonOracleRawColumnName, out candidate, out readFailure))
                        {
                            //Do not put raw READ_FAILED exception content in UI or SQL History.
                            completeDefinitionBlockReason = readFailure;
                        }
                        else if (EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(target.SourceType, mode, candidate, modernMySqlRenamePreview))
                        {
                            completeDefinitionPreview = candidate;
                            completeDefinitionBlockReason = string.Empty;
                        }
                        else
                        {
                            completeDefinitionBlockReason = "F7U3C4B3_DEFINITION_NOT_QUALIFIED";
                        }
                    }
                }
            }

            using (var form = new EditColumnForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, icon);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SourceType = target.SourceType;
                form.SchemaDatabase = target.SchemaDatabase;
                form.SchemaName = target.SchemaName;
                form.TableName = target.TableName;
                form.ColumnName = current.NonOracleRawColumnName;
                form.EditMode = mode;

                //Existing Column Execute is enabled only for qualified operations with last-mile validation.
                //MySQL legacy CHANGE COLUMN and COMMENT remain Preview Only until full definitions are verified.
                form.NonOracleF7MySqlModernRenamePreview = modernMySqlRenamePreview;
                form.NonOracleF7MySqlCompleteDefinitionPreview = completeDefinitionPreview;
                form.NonOracleF7MySqlPreviewBlockReasonCode = completeDefinitionBlockReason;

                form.OfflineSqlPreviewOnly = !EditColumnForm.CanEnableNonOracleExistingColumnDdl(target.SourceType, mode) && !modernMySqlRenamePreview && completeDefinitionPreview == null;
                form.RequireOracleF7ExecuteGate = false;
                form.RequireNonOracleF7ExecuteGate = false;
                form.RequireNonOracleF7ExistingColumnGate = true;
                form.NonOracleF7ExpectedColumn = current.NonOracleRawColumnName;
                form.NonOracleF7ExpectedSqlServerObjectId = sqlServerObjectId;
                form.NonOracleF7ExpectedDatabase = target.SourceType == DataSourceType.PostgreSql ? DatabaseSqlExecutor.DatabaseName : target.SchemaDatabase;
                form.NonOracleF7ExpectedSchema = target.SchemaName;
                form.NonOracleF7ExpectedTable = target.TableName;
                form.NonOracleF7ExpectedConnectionString = DatabaseSqlExecutor.DbConnectionString;

                UIHelper.ApplyEditColumnFormSize(form);
                form.ShowDialog(this);

                //Rename/Drop affect the Schema tree. Column Comment does not require a full metadata reload.
                if ((mode == EditColumnMode.Rename || mode == EditColumnMode.Drop)
                    && string.Equals(form.Result, "REFRESH", StringComparison.Ordinal)
                    && !IsDisposed && !Disposing && CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
                {
                    if (target.SourceType == DataSourceType.MySql)
                    {
                        RefreshSharedSchemaExplorerPreviewMetadata();
                    }
                    else if (target.SourceType == DataSourceType.PostgreSql || target.SourceType == DataSourceType.SqlServer)
                    {
                        MarkF7TransactionBoundAddColumn();
                    }
                }
            }

            return true;
        }

        //Refresh F7 only after the Add Column transaction is resolved.
        private bool _pendingF7AddColumnMetadataRefresh;

        //F7-U2-C98: open a qualified Add Column form with an opt-in last-mile Execute Gate.
        //Legacy F7 and Oracle paths are not changed.
        private bool TryOpenSharedNonOracleAddColumnPreview(SchemaExplorerSelectionInfo captured)
        {
            if (captured == null || _currentSourceType == DataSourceType.Oracle
                || captured.SourceType != _currentSourceType
                || _sharedSchemaMetadataSnapshot == null
                || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerObjectCommandPolicy.CanExecute(CaptureLeftPanelContext().ActiveTarget, captured, SharedSchemaExplorerMenuCommand.AddColumn))
            {
                return false;
            }

            var current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, captured.DisplayRowIndex);

            if (!NonOracleSharedAddColumnPreviewPolicy.IsSameSelection(captured, current)
                || !NonOracleSharedAddColumnPreviewPolicy.TryCreate(current, out var target)
                || !target.OfflineSqlPreviewOnly)
            {
                return false;
            }

            using (var form = new EditColumnForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Column Add.ico");
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SourceType = target.SourceType;
                form.SchemaDatabase = target.SchemaDatabase;
                form.SchemaName = target.SchemaName;
                form.TableName = target.TableName;
                form.ColumnName = string.Empty;
                form.EditMode = EditColumnMode.Add;
                form.OfflineSqlPreviewOnly = false;
                form.RequireOracleF7ExecuteGate = false;
                form.OracleTargetIdentity = null;
                form.RequireNonOracleF7ExecuteGate = true;
                form.NonOracleF7ExpectedDatabase = target.SourceType == DataSourceType.PostgreSql ? DatabaseSqlExecutor.DatabaseName : target.SchemaDatabase;
                form.NonOracleF7ExpectedSchema = target.SchemaName;
                form.NonOracleF7ExpectedTable = target.TableName;
                form.NonOracleF7ExpectedConnectionString = DatabaseSqlExecutor.DbConnectionString;

                UIHelper.ApplyEditColumnFormSize(form);
                form.ShowDialog(this);

                if (string.Equals(form.Result, "REFRESH", StringComparison.Ordinal) && !IsDisposed && !Disposing && CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
                {
                    if (target.SourceType == DataSourceType.MySql)
                    {
                        //MySQL ALTER TABLE implicitly commits.
                        RefreshSharedSchemaExplorerPreviewMetadata();
                    }
                    else if (target.SourceType == DataSourceType.PostgreSql || target.SourceType == DataSourceType.SqlServer)
                    {
                        //Wait for an explicit successful Commit or Rollback.
                        MarkF7TransactionBoundAddColumn();
                    }
                }
            }

            return true;
        }

        private static void ShowSharedSchemaDdl(SchemaExplorerSelectionInfo selection)
        {
            if (selection.SourceType != DataSourceType.SqlServer)
            {
                UIHelper.ShowDDL(selection.SchemaType, selection.SchemaName, selection.SchemaNode, selection.SchemaDbo, selection.PackageSpecBody);
                return;
            }

            //Unlike the generic ShowDDL overload, the original SQL Server F7 passes ObjectId.
            using (var form = new SqlStatementViewerForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                form.SchemaType = selection.SchemaType;
                form.SchemaName = selection.SchemaName;
                form.CellText = DatabaseSqlExecutor.GetCreateScript_SqlServer(selection.SchemaType, selection.SchemaNode, selection.SchemaDbo, selection.SchemaName, selection.ObjectId);

                var size = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", form.ClientSize.Width, form.ClientSize.Height);

                form.ClientSize = new System.Drawing.Size(size.Width - 16, size.Height - 38);
                form.ShowDialog();
            }
        }

        private static void OpenSharedSchemaBrowser(SchemaExplorerSelectionInfo selection)
        {
            switch (selection.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        MyGlobal.DisplaySchemaBrowser(string.Empty, selection.SchemaType, selection.SchemaName);
                        break;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        MyGlobal.DisplaySchemaBrowser(selection.SchemaNode, selection.SchemaType, selection.SchemaName);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        MyGlobal.DisplaySchemaBrowser(selection.SchemaNode, selection.SchemaType, selection.SchemaName, selection.SchemaDbo, selection.ObjectId);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void OpenSharedGenerateSql(SchemaExplorerSelectionInfo selection, string activeTabIdentity)
        {
            using (var form = new GenerateSqlForm())
            {
                form.SchemaType = selection.SchemaType;
                form.SchemaNode = selection.SchemaNode;
                form.SchemaDbo = selection.SchemaDbo;
                form.SchemaName = SharedSchemaExplorerObjectCommandPolicy.GetGenerateSqlObjectName(selection);
                form.ObjectId = selection.ObjectId;
                form.AccessibleDescriptionString = activeTabIdentity ?? string.Empty;

                var size = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", form.ClientSize.Width, form.ClientSize.Height);

                form.ClientSize = new System.Drawing.Size(size.Width - 16, size.Height - 38);
                form.ShowDialog();
            }
        }
    }
}
