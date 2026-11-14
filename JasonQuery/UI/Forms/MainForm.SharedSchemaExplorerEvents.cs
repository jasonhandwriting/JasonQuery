using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Localization;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.SchemaExplorer;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //B1-C3C2A: limited central F7 interactions; legacy engine-specific menus remain untouched.
    public partial class MainForm
    {
        private ContextMenuStrip _sharedSchemaCopyContextMenu;
        private ToolStripMenuItem _sharedSchemaCopyNameItem;
        private SchemaExplorerSelectionInfo _sharedSchemaCopySelection;
        private Control _sharedSchemaMenuActiveForm;
        private LeftPanelTargetKind _sharedSchemaMenuTarget;
        private int _sharedSchemaMenuConnectionRevision;
        private ToolStripMenuItem _sharedSchemaRecoveryEntryItem;
        private SharedOracleRecoveryEntryKind _sharedSchemaRecoveryEntryKind;

        private void AttachSharedSchemaExplorerInputHandlers(C1TrueDBGrid grid)
        {
            if (grid == null || grid.IsDisposed)
            {
                return;
            }

            //Each freshly bound grid is independent. Never subscribe twice to the same instance.
            grid.MouseDoubleClick -= SharedSchemaExplorerGridMouseDoubleClick;
            grid.MouseDoubleClick += SharedSchemaExplorerGridMouseDoubleClick;
            grid.KeyDown -= SharedSchemaExplorerGridKeyDown;
            grid.KeyDown += SharedSchemaExplorerGridKeyDown;
            grid.MouseUp -= SharedSchemaExplorerGridMouseUp;
            grid.MouseUp += SharedSchemaExplorerGridMouseUp;
        }

        private void DetachSharedSchemaExplorerInputHandlers(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            grid.MouseDoubleClick -= SharedSchemaExplorerGridMouseDoubleClick;
            grid.KeyDown -= SharedSchemaExplorerGridKeyDown;
            grid.MouseUp -= SharedSchemaExplorerGridMouseUp;
            _sharedSchemaCopyContextMenu?.Hide();
            _sharedSchemaCopySelection = null;
        }

        private bool CanHandleSharedSchemaExplorerInput(C1TrueDBGrid grid)
        {
            return !_sharedSchemaPreviewBinding
                   && _sharedSchemaPreviewRequested
                   && _sharedLeftPanelHostPanel != null
                   && _sharedLeftPanelHostPanel.Visible
                   && grid != null
                   && !grid.IsDisposed
                   && grid.Visible
                   && ReferenceEquals(grid, _sharedSchemaExplorerGrid)
                   && SharedSchemaLifecyclePolicy.CanReadCachedMetadata(SharedSchemaMetadataSnapshotPolicy.CanBindForScope(CaptureSharedLeftPanelHostState(), _sharedSchemaMetadataSnapshot, _currentSourceType, GetSharedSchemaDatabaseScope()));
        }

        //Never reuse a snapshot after reconnect, including reconnecting as the SAME Oracle user.
        private bool IsSharedOracleSessionIdentityCurrent()
        {
            if (_currentSourceType != DataSourceType.Oracle)
            {
                return true;
            }

            var reader = MyGlobal.OracleReader;
            var snapshot = _sharedSchemaMetadataSnapshot;

            return reader != null
                   && snapshot != null
                   && snapshot.OracleConnectionId != Guid.Empty
                   && string.Equals(snapshot.OracleVerifiedSessionUser, reader.VerifiedSessionUser, StringComparison.Ordinal)
                   && snapshot.OracleConnectionId == reader.VerifiedConnectionId;
        }

        //Cached F7 input is read-only until a live, verified Oracle session authorizes DB actions.
        private bool CanHandleSharedSchemaExplorerSessionAction()
        {
            var liveIdentityCurrent = _currentSourceType != DataSourceType.Oracle || IsSharedOracleSessionIdentityCurrent();

            return SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid), liveIdentityCurrent);
        }

        private SchemaExplorerSelectionInfo ResolveSharedSchemaExplorerInput(C1TrueDBGrid grid, int displayRowIndex)
        {
            if (!CanHandleSharedSchemaExplorerInput(grid) || displayRowIndex < 0 || grid.Splits.Count == 0 || displayRowIndex >= grid.Splits[0].Rows.Count)
            {
                return null;
            }

            var request = new SchemaExplorerSelectionResolveRequest
            {
                SourceType = _currentSourceType,
                DisplayRowIndex = displayRowIndex,
                IsExternalSelection = false,
                IsShowColumnInfo = Array.IndexOf(_sharedSchemaMetadataSnapshot.GroupColumns, "SchemaName") >= 0,
                //Only Table/View metadata currently has authoritative per-row OracleMetadataOwner.
                RequireOracleOwnerIdentity = _currentSourceType == DataSourceType.Oracle,
                OracleVerifiedSessionUser = _currentSourceType == DataSourceType.Oracle ? _sharedSchemaMetadataSnapshot.OracleVerifiedSessionUser : string.Empty
            };

            //Resolve from the MainForm grid. Never reuse a QueryForm or SchemaBrowserForm row index.
            var rowReader = new C1SchemaExplorerRowReader(grid);
            var selection = SchemaExplorerSelectionResolver.Resolve(request, rowReader);

            //C2: only the selected Oracle ColumnInfo DataRow can provide the raw dictionary identifiers.
            //Do not change the separately protected legacy SchemaExplorerSelectionResolver.
            if (_currentSourceType == DataSourceType.Oracle && selection != null && selection.IsColumnInfoRow)
            {
                OracleSharedColumnDdlBridgePolicy.TryQualifySelection(selection, rowReader.GetDataRow(displayRowIndex), _sharedSchemaMetadataSnapshot.OracleVerifiedSessionUser);
            }

            //F7-U2-B: the shared Grid's selected Column DataRow supplies its parent Table.
            if (_currentSourceType != DataSourceType.Oracle && selection != null && selection.IsColumnInfoRow)
            {
                var selectedDataRow = rowReader.GetDataRow(displayRowIndex);

                NonOracleSharedAddColumnPreviewPolicy.TryQualifyColumn(selection, selectedDataRow);
                NonOracleSharedColumnDdlBridgePolicy.TryQualifySelection(selection, selectedDataRow);
            }

            return selection;
        }

        private bool TryActivateSharedSchemaExplorerInput(C1TrueDBGrid grid, int displayRowIndex, SharedSchemaExplorerInputKind input)
        {
            var selection = ResolveSharedSchemaExplorerInput(grid, displayRowIndex);
            var state = CaptureSharedLeftPanelHostState();
            SchemaExplorerActionKind action;

            if (!SharedSchemaExplorerInteractionPolicy.TryGetAction(state.ActiveTarget, selection, input, out action))
            {
                return false;
            }

            //Double-click/Enter may execute a DB operation; stale sessions must fail closed.
            if (!CanHandleSharedSchemaExplorerSessionAction())
            {
                return false;
            }

            //The existing bridge copies portable semantics, zeros local indices, then checks the active tab.
            return TryDispatchActiveSchemaExplorerAction(selection, action);
        }

        private void SharedSchemaExplorerGridMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || !(sender is C1TrueDBGrid grid))
            {
                return;
            }

            try
            {
                TryActivateSharedSchemaExplorerInput(grid, grid.RowContaining(e.Y), SharedSchemaExplorerInputKind.DoubleClick);
            }
            catch (Exception)
            {
                //The preview must not crash a working legacy F7 when C1 is regrouping.
            }
        }

        private void SharedSchemaExplorerGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !(sender is C1TrueDBGrid grid))
            {
                return;
            }

            try
            {
                if (TryActivateSharedSchemaExplorerInput(grid, grid.Row, SharedSchemaExplorerInputKind.Enter))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch (Exception)
            {
                //Leave the original F7 route available; do not consume an unsuccessful action.
            }
        }

        private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || !(sender is C1TrueDBGrid grid) || !CanHandleSharedSchemaExplorerInput(grid))
            {
                return;
            }

            SchemaExplorerSelectionInfo selection;

            var displayRowIndex = grid.RowContaining(e.Y);

            try
            {
                selection = ResolveSharedSchemaExplorerInput(grid, displayRowIndex);
            }
            catch (Exception)
            {
                return;
            }

            var context = CaptureLeftPanelContext();
            var state = CaptureSharedLeftPanelHostState();
            var activeForm = tabControl1.SelectedTab?.Control;

            if (context == null || !context.IsConnected || state == null || activeForm == null || selection == null || selection.SourceType != _currentSourceType)
            {
                return;
            }

            var commands = SharedSchemaExplorerContextMenuPolicy.Plan(context.ActiveTarget, selection);

            if (commands.Count == 0 || !SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(selection, out var copyText))
            {
                return;
            }

            //Right-click target becomes the current F7 working focus.
            new C1SchemaExplorerRowReader(grid).SetCurrentRow(displayRowIndex);

            //L2-C3-B1: render the complete command plan, but execute Copy only.
            //Other commands stay disabled until their target-specific handlers are qualified.
            DisposeSharedSchemaCopyContextMenu();

            var menu = new ContextMenuStrip();
            var heading = menu.Items.Add(copyText);

            heading.Enabled = false;
            menu.Items.Add("-");

            foreach (var command in commands)
            {
                //L2-C3-C2 / F7-U1: separate Table and Column commands for all providers.
                if (command == SharedSchemaExplorerMenuCommand.OpenSchemaBrowser || command == SharedSchemaExplorerMenuCommand.CommentTable || (command == SharedSchemaExplorerMenuCommand.AddColumn && selection.Kind != SchemaExplorerSelectionKind.ColumnInfo && !selection.IsColumnInfoRow))
                {
                    menu.Items.Add("-");
                }

                if (command == SharedSchemaExplorerMenuCommand.CopyToClipboard && menu.Items.Count > 2)
                {
                    menu.Items.Add("-");
                }

                var item = new ToolStripMenuItem(GetSharedSchemaMenuCaption(context.ActiveTarget, selection, command));

                item.Tag = command;

                var isCopy = command == SharedSchemaExplorerMenuCommand.CopyToClipboard;
                var isPaste = SharedSchemaExplorerPastePolicy.IsPasteCommand(command) && SharedSchemaExplorerPastePolicy.TryBuildText(context.ActiveTarget, selection, command, out _);
                var pasteReceiverReady = context.ActiveTarget == LeftPanelTargetKind.SchemaBrowserForm || (activeForm is QueryForm query && query.CanAcceptSharedSchemaExplorerPaste);

                //SQL generation is a cached-data action; live Oracle authorization belongs to the form's Execute gate.
                var isDdlGenerator = SharedSchemaDdlInteractionPolicy.CanOpenGenerator
                (
                    context.ActiveTarget, selection, command, CanHandleSharedSchemaExplorerInput(grid)
                );

                var isObjectAction = (isDdlGenerator || CanHandleSharedSchemaExplorerSessionAction()) && SharedSchemaExplorerObjectCommandPolicy.CanExecute(context.ActiveTarget, selection, command);

                item.Enabled = isCopy || (isPaste && pasteReceiverReady) || isObjectAction;

                if (isCopy)
                {
                    item.Click += SharedSchemaCopyNameClick;
                    _sharedSchemaCopyNameItem = item;
                    item.ShortcutKeys = Keys.Control | Keys.C;
                }

                if (isPaste)
                {
                    item.ShortcutKeys = GetSharedSchemaPasteShortcut(command);

                    if (item.Enabled)
                    {
                        item.Click += SharedSchemaPasteClick;
                    }
                }

                if (isObjectAction && item.Enabled)
                {
                    item.Click += SharedSchemaObjectMenuClick;
                }

                if (!item.Enabled)
                {
                    item.ToolTipText = "Available in original F7 until the next L2-C3 parity batch.";
                }

                menu.Items.Add(item);
            }

            //L2B-1: information-only recovery entry. NEVER reconnect or refresh during menu construction.
            var recoveryKind = ResolveSharedOracleRecoveryEntryKind(context.ActiveTarget, selection);
            var recoveryContext = CreateSharedOracleRecoveryEntryContext(context, state, activeForm, false, true);

            if (SharedOracleRecoveryEntryPolicy.CanOfferEntry(recoveryContext, recoveryKind))
            {
                menu.Items.Add("-");

                var recoveryItem = new ToolStripMenuItem("Oracle connection and fresh schema required...");

                recoveryItem.ToolTipText = "L2B-1 guidance only. No reconnect, metadata query or DDL is performed.";
                recoveryItem.Click += SharedOracleRecoveryEntryClick;

                menu.Items.Add(recoveryItem);

                _sharedSchemaRecoveryEntryItem = recoveryItem;
                _sharedSchemaRecoveryEntryKind = recoveryKind;
            }

            _sharedSchemaCopyContextMenu = menu;
            _sharedSchemaCopySelection = selection;
            _sharedSchemaMenuActiveForm = activeForm;
            _sharedSchemaMenuTarget = context.ActiveTarget;
            _sharedSchemaMenuConnectionRevision = state.ConnectionRevision;

            menu.Show(grid, new Point(e.X, e.Y));
        }

        //L2B-1: resolve from the already-qualified row and existing target/menu policy.
        //This is an entry classification, NOT permission to access Oracle.
        private static SharedOracleRecoveryEntryKind ResolveSharedOracleRecoveryEntryKind(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            if (SharedSchemaExplorerObjectCommandPolicy.CanExecute(target, selection, SharedSchemaExplorerMenuCommand.CommentTable))
            {
                return SharedOracleRecoveryEntryKind.Table;
            }

            if (SharedSchemaExplorerObjectCommandPolicy.CanExecute(target, selection, SharedSchemaExplorerMenuCommand.CommentColumn))
            {
                return SharedOracleRecoveryEntryKind.Column;
            }

            return SharedOracleRecoveryEntryKind.None;
        }

        private SharedOracleSessionRecoveryContext CreateSharedOracleRecoveryEntryContext(LeftPanelContext context, SharedLeftPanelHostState state, Control activeForm, bool explicitInvocation, bool menuCurrent)
        {
            var sourceCurrent = _currentSourceType == DataSourceType.Oracle && DatabaseSqlExecutor.CurrentDataSource == _currentSourceType;
            var activeTarget = context != null && (context.ActiveTarget == LeftPanelTargetKind.QueryForm || context.ActiveTarget == LeftPanelTargetKind.SchemaBrowserForm);

            return new SharedOracleSessionRecoveryContext
            {
                IsOracleWorkspace = sourceCurrent,
                HasCurrentLogicalSnapshot = CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid),
                IsDatabaseBackedAction = true,
                IsActiveTabAndMenuCurrent = menuCurrent && state != null && context != null && context.IsConnected && context.SchemaReady && activeTarget && activeForm != null && ReferenceEquals(tabControl1.SelectedTab?.Control, activeForm),
                IsExplicitUserInvocation = explicitInvocation,
                HasUnsafeTransactionState = AppConfigHelper.IsNotCommitYet,
                IsLiveSessionVerified = CanHandleSharedSchemaExplorerSessionAction(),
                //Fresh evidence is deliberately unavailable until L2B-2; never infer it from old dtSchema.
                IsFreshMetadataFromLiveSession = false,
                HasSameDatabaseAndOwnerScope = false,
                IsExactTargetReResolved = false,
                IsFreshSnapshotSessionIdentityCurrent = false
            };
        }

        private void SharedOracleRecoveryEntryClick(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var selection = _sharedSchemaCopySelection;
            var state = CaptureSharedLeftPanelHostState();
            var context = CaptureLeftPanelContext();
            var activeForm = tabControl1.SelectedTab?.Control;

            if (item == null || !item.Enabled || !ReferenceEquals(item, _sharedSchemaRecoveryEntryItem)
                || !ReferenceEquals(item.Owner, _sharedSchemaCopyContextMenu) || selection == null || state == null || context == null
                || state.ConnectionRevision != _sharedSchemaMenuConnectionRevision || !ReferenceEquals(activeForm, _sharedSchemaMenuActiveForm)
                || context.ActiveTarget != _sharedSchemaMenuTarget || selection.SourceType != _currentSourceType
                || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
            {
                return;
            }

            var kind = ResolveSharedOracleRecoveryEntryKind(context.ActiveTarget, selection);

            if (kind == SharedOracleRecoveryEntryKind.None || kind != _sharedSchemaRecoveryEntryKind)
            {
                return;
            }

            //A menu captured before a grid rebind or tab change cannot even request recovery guidance.
            SchemaExplorerSelectionInfo current;

            try
            {
                current = ResolveSharedSchemaExplorerInput(_sharedSchemaExplorerGrid, selection.DisplayRowIndex);
            }
            catch (Exception)
            {
                return;
            }

            var sameRow = kind == SharedOracleRecoveryEntryKind.Table ? OracleSharedTableDdlBridgePolicy.IsSameTableDataRow(selection, current) : OracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(selection, current);

            if (!sameRow)
            {
                return;
            }

            var recoveryContext = CreateSharedOracleRecoveryEntryContext(context, state, activeForm, true, true);

            if (!SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(recoveryContext, kind))
            {
                return;
            }

            //L2B-1 does not open Oracle, republish metadata, or invoke any DDL form.
            MessageBox.Show("This Oracle object action requires a newly verified connection and fresh schema metadata. "
                            + "Automatic recovery is not enabled yet. Use the SQL Editor's Refresh Schema, then reselect the object. "
                            + "No database action has been executed.",
                            AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetSharedSchemaMenuCaption(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command)
        {
            var pasteTitle = LocalizationHelper.GetLanguageString("Paste to Query Editor", "form", "GenerateSqlForm", "object", "btnPasteToQueryEditor", "Text");

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.ShowDdl:
                    {
                        return LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                    }
                case SharedSchemaExplorerMenuCommand.OpenSchemaBrowser:
                    {
                        var text = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.GenerateSqlStatement:
                    {
                        var text = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.CommentTable:
                    {
                        var text = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.DropTable:
                    {
                        var text = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.RenameTable:
                    {
                        var text = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                    {
                        var text = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.AddColumn:
                    {
                        var text = LocalizationHelper.GetLanguageString("Add Column...", "form", "EditColumnForm", "object", "AddColumn", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                    {
                        var text = LocalizationHelper.GetLanguageString("Comment Column...", "form", "EditColumnForm", "object", "CommentColumn", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.DropColumn:
                    {
                        var text = LocalizationHelper.GetLanguageString("Drop Column...", "form", "EditColumnForm", "object", "DropColumn", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        var text = LocalizationHelper.GetLanguageString("Rename Column...", "form", "EditColumnForm", "object", "RenameColumn", "Text");

                        return $"{text}...";
                    }
                case SharedSchemaExplorerMenuCommand.CopyToClipboard:
                    {
                        return LocalizationHelper.GetLanguageString("Copy to Clipboard", "form", "GenerateSqlForm", "object", "btnCopyToClipboard", "Text");
                    }
                case SharedSchemaExplorerMenuCommand.PasteSelectAll:
                    {
                        if (SharedSchemaExplorerPastePolicy.TryBuildText(target, selection, command, out var sql))
                        {
                            return $"{pasteTitle} {sql}";
                        }

                        return $"{pasteTitle} SELECT * FROM ...";
                    }
                case SharedSchemaExplorerMenuCommand.PasteName:
                    {
                        return pasteTitle;
                    }
                case SharedSchemaExplorerMenuCommand.PasteComma:
                    {
                        return $"{pasteTitle} + \", \"";
                    }
                case SharedSchemaExplorerMenuCommand.PasteCommaNewLine:
                    {
                        return $"{pasteTitle} + \", \\r\\n\"";
                    }
                case SharedSchemaExplorerMenuCommand.PasteNewLine:
                    {
                        return $"{pasteTitle} + \"\\r\\n\"";
                    }
                default:
                    {
                        return command.ToString();
                    }
            }
        }

        private static Keys GetSharedSchemaPasteShortcut(SharedSchemaExplorerMenuCommand command)
        {
            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.PasteSelectAll:
                    {
                        return Keys.Control | Keys.V;
                    }
                case SharedSchemaExplorerMenuCommand.PasteName:
                    {
                        return Keys.Control | Keys.Alt | Keys.V;
                    }
                case SharedSchemaExplorerMenuCommand.PasteComma:
                    {
                        return Keys.Control | Keys.Shift | Keys.V;
                    }
                case SharedSchemaExplorerMenuCommand.PasteCommaNewLine:
                    {
                        return Keys.Alt | Keys.Shift | Keys.V;
                    }
                case SharedSchemaExplorerMenuCommand.PasteNewLine:
                    {
                        return Keys.Control | Keys.Alt | Keys.Shift | Keys.V;
                    }
                default:
                    {
                        return Keys.None;
                    }
            }
        }

        private void SharedSchemaPasteClick(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is SharedSchemaExplorerMenuCommand command) || !item.Enabled || !ReferenceEquals(item.Owner, _sharedSchemaCopyContextMenu))
            {
                return;
            }

            var selection = _sharedSchemaCopySelection;
            var state = CaptureSharedLeftPanelHostState();
            var context = CaptureLeftPanelContext();
            var activeForm = tabControl1.SelectedTab?.Control;

            if (selection == null || state == null || context == null || !context.IsConnected || !context.SchemaReady
                || state.ConnectionRevision != _sharedSchemaMenuConnectionRevision
                || !ReferenceEquals(activeForm, _sharedSchemaMenuActiveForm)
                || context.ActiveTarget != _sharedSchemaMenuTarget
                || selection.SourceType != _currentSourceType
                || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerPastePolicy.TryBuildText(context.ActiveTarget, selection, command, out var pasteText))
            {
                return;
            }

            if (context.ActiveTarget == LeftPanelTargetKind.QueryForm && activeForm is QueryForm query)
            {
                query.TryPasteSharedSchemaExplorerText(pasteText);
                return;
            }

            if (context.ActiveTarget == LeftPanelTargetKind.SchemaBrowserForm && activeForm is SchemaBrowserForm)
            {
                //Preserve the legacy SchemaBrowserForm behavior: transfer to an SQL Editor, creating one when necessary. Never insert into the SchemaBrowser editor pane.
                PasteFromSchemaBrowser(pasteText);
            }
        }

        private void SharedSchemaCopyNameClick(object sender, EventArgs e)
        {
            var selection = _sharedSchemaCopySelection;
            var state = CaptureSharedLeftPanelHostState();
            var context = CaptureLeftPanelContext();

            if (selection == null || state == null || context == null
                || state.ConnectionRevision != _sharedSchemaMenuConnectionRevision
                || !ReferenceEquals(tabControl1.SelectedTab?.Control, _sharedSchemaMenuActiveForm)
                || context.ActiveTarget != _sharedSchemaMenuTarget
                || selection.SourceType != _currentSourceType
                || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(selection, out var copyText))
            {
                return;
            }

            try
            {
                Clipboard.SetText(copyText);
            }
            catch (Exception)
            {
                //Clipboard availability is outside the preview's ownership.
            }
        }

        private void DisposeSharedSchemaCopyContextMenu()
        {
            _sharedSchemaCopySelection = null;
            _sharedSchemaMenuActiveForm = null;
            _sharedSchemaMenuTarget = LeftPanelTargetKind.None;
            _sharedSchemaMenuConnectionRevision = 0;

            if (_sharedSchemaCopyNameItem != null)
            {
                _sharedSchemaCopyNameItem.Click -= SharedSchemaCopyNameClick;
                _sharedSchemaCopyNameItem = null;
            }

            if (_sharedSchemaRecoveryEntryItem != null)
            {
                _sharedSchemaRecoveryEntryItem.Click -= SharedOracleRecoveryEntryClick;
                _sharedSchemaRecoveryEntryItem = null;
            }

            _sharedSchemaRecoveryEntryKind = SharedOracleRecoveryEntryKind.None;

            if (_sharedSchemaCopyContextMenu != null)
            {
                _sharedSchemaCopyContextMenu.Dispose();
                _sharedSchemaCopyContextMenu = null;
            }
        }

        private void DisposeSharedSchemaExplorerInputHandlers()
        {
            DisposeSharedSchemaCopyContextMenu();
        }
    }
}
