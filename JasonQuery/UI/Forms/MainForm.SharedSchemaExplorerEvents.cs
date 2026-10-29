using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
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
                   && SharedSchemaMetadataSnapshotPolicy.CanBind(CaptureSharedLeftPanelHostState(), _sharedSchemaMetadataSnapshot, _currentSourceType)
                   && IsSharedOracleSessionIdentityCurrent();
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
            return SchemaExplorerSelectionResolver.Resolve(request, new C1SchemaExplorerRowReader(grid));
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

            try
            {
                selection = ResolveSharedSchemaExplorerInput(grid, grid.RowContaining(e.Y));
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

            //L2-C3-B1: render the complete command plan, but execute Copy only.
            //Other commands stay disabled until their target-specific handlers are qualified.
            DisposeSharedSchemaCopyContextMenu();

            var menu = new ContextMenuStrip();
            var heading = menu.Items.Add(copyText);

            heading.Enabled = false;
            menu.Items.Add("-");

            foreach (var command in commands)
            {
                //L2-C3-C2: 保留舊 F7 的選單分類分隔線
                if (command == SharedSchemaExplorerMenuCommand.OpenSchemaBrowser || command == SharedSchemaExplorerMenuCommand.CommentTable)
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
                var isObjectAction = SharedSchemaExplorerObjectCommandPolicy.CanExecute(context.ActiveTarget, selection, command);

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

            _sharedSchemaCopyContextMenu = menu;
            _sharedSchemaCopySelection = selection;
            _sharedSchemaMenuActiveForm = activeForm;
            _sharedSchemaMenuTarget = context.ActiveTarget;
            _sharedSchemaMenuConnectionRevision = state.ConnectionRevision;

            menu.Show(grid, new Point(e.X, e.Y));
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
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is SharedSchemaExplorerMenuCommand command)
                || !item.Enabled || !ReferenceEquals(item.Owner, _sharedSchemaCopyContextMenu))
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
                //Preserve the legacy SchemaBrowserForm behavior: transfer to an SQL Editor,
                //creating one when necessary. Never insert into the SchemaBrowser editor pane.
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
