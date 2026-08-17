using JasonQuery.Core.Config;
using JasonQuery.Core.Text;
using System;
using System.IO;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool? _lastEditorHasContent;
        private bool? _lastEditorHasSelection;

        private void CheckEditorContent()
        {
            if (_isBusy)
            {
                return; //查詢尚未結束，不需要處理
            }

            RefreshEditorCommandButtonsState();
            RefreshEditorDirtyState();
            RefreshSqlNavigatorPanel();
        }

        private void RefreshEditorCommandButtonsState()
        {
            var isEditorHasContent = HasEditorContent();
            var hasEditorSelection = HasEditorSelection();

            if (_lastEditorHasContent == isEditorHasContent && _lastEditorHasSelection == hasEditorSelection)
            {
                return;
            }

            _lastEditorHasContent = isEditorHasContent;
            _lastEditorHasSelection = hasEditorSelection;

            ApplyEditorCommandButtonsState(isEditorHasContent, hasEditorSelection);
        }

        private bool HasEditorContent()
        {
            return !string.IsNullOrWhiteSpace(editor.Text);
        }

        private bool HasEditorSelection()
        {
            return editor.SelectionStart != editor.SelectionEnd;
        }

        private void RefreshEditorDirtyState()
        {
            if (_isBusy)
            {
                return;
            }

            var canUndo = editor.CanUndo;

            ApplyEditorSaveButtonsState(canUndo);

            var tabTitle = BuildEditorTabTitle(canUndo);

            UpdateEditorBackupInfoIfNeeded(canUndo);

            tmrBackup.Enabled = AppConfigHelper.IsBackupFile && canUndo;

            //更新主畫面頁籤文字
            TransferValueToMainForm($"UpdateCanUndo`{AccessibleDescription}`{tabTitle}");
        }

        private void ApplyEditorCommandButtonsState(bool isEditorHasContent, bool hasEditorSelection)
        {
            //editor 有內容
            btnQuery.Enabled = isEditorHasContent;
            btnSelectCurrentBlock.Enabled = isEditorHasContent;
            btnExecuteCurrentBlock.Enabled = isEditorHasContent;
            btnSelectCurrentLine.Enabled = isEditorHasContent;
            btnExecuteCurrentLine.Enabled = isEditorHasContent;
            btnRemoveTrailingBlanks.Enabled = isEditorHasContent;

            //editor 有選取範圍
            btnIndent.Enabled = hasEditorSelection;
            btnUnIndent.Enabled = hasEditorSelection;
            btnComment.Enabled = hasEditorSelection;
            btnRemoveComment.Enabled = hasEditorSelection;
            btnCode2Sql.Enabled = hasEditorSelection;
            btnSql2Code.Enabled = hasEditorSelection;
        }

        private void ApplyEditorSaveButtonsState(bool canUndo)
        {
            btnSave.Visible = !canUndo;
            btnSaveRed.Visible = canUndo;
        }

        private string BuildEditorTabTitle(bool canUndo)
        {
            var tabTitle = (btnSave.Tag ?? Tag)?.ToString() ?? string.Empty;

            if (!canUndo)
            {
                return tabTitle;
            }

            return tabTitle.StartsWith("*", StringComparison.Ordinal) ? tabTitle : $"*{tabTitle}";
        }

        private void UpdateEditorBackupInfoIfNeeded(bool canUndo)
        {
            if (!canUndo)
            {
                return;
            }

            var tabTitle = Path.GetFileName(TextHelper.GetSafeString(Tag));

            if (!tabTitle.StartsWith("*", StringComparison.Ordinal))
            {
                tabTitle = $"*{tabTitle}";
            }

            UpdateBackupFileInfo(_backupFileName, editor.CurrentPosition, tabTitle, TextHelper.GetSafeString(btnSave.Tag));
        }
    }
}