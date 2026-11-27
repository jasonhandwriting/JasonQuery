using JasonQuery.Core.Config;
using JasonQuery.UI.QueryEditor;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool? _lastEditorHasContent;
        private bool? _lastEditorHasSelection;
        private QueryEditorTabState _editorTabState = QueryEditorTabState.CreateNew("SQL Editor");
        private bool _restoreBackupWasDirty;
        private bool _requiresSaveWithoutUndo;

        internal event Action<QueryForm, string, string, bool> EditorTabStateChanged;

        internal string EditorTabDisplayName => _editorTabState.DisplayName;
        internal string SavedFilePath => _editorTabState.SavedFilePath;
        internal bool HasSavedFile => _editorTabState.HasSavedFile;
        internal bool IsEditorDirty => editor.CanUndo || _requiresSaveWithoutUndo;
        internal bool CanRenameEditorTab => _isFormLoadFinished && !_isBusy && !IsDisposed && !Disposing;

        internal void InitializeEditorTabState(string tabTitle, bool isRestoringBackup)
        {
            _restoreBackupWasDirty = false;
            _requiresSaveWithoutUndo = false;
            _editorTabState = isRestoringBackup ? QueryEditorTabState.Restore(tabTitle, string.Empty, out _restoreBackupWasDirty) : QueryEditorTabState.CreateNew(tabTitle.TrimStart('*'));
        }

        internal bool MatchesSavedFilePath(string filePath)
        {
            return _editorTabState.MatchesSavedFilePath(filePath);
        }

        internal bool TryRenameEditorTab(string displayName)
        {
            if (!CanRenameEditorTab || !_editorTabState.TryRename(displayName))
            {
                return false;
            }

            var isDirty = IsEditorDirty;

            PublishEditorTabState(isDirty);

            try
            {
                UpdateExistingBackupTabTitle(BuildEditorTabTitle(isDirty));
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }

            return true;
        }

        private void PublishEditorTabState(bool isDirty)
        {
            EditorTabStateChanged?.Invoke(this, BuildEditorTabTitle(isDirty), _editorTabState.BuildTooltip(isDirty), isDirty);
        }

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

            var isDirty = IsEditorDirty;

            ApplyEditorSaveButtonsState(isDirty);

            //Never perform backup metadata database I/O on TextChanged/KeyUp.
            //Debounce actual backup file + metadata persistence until typing is idle.
            tmrBackup.Stop();

            if (AppConfigHelper.IsBackupFile && isDirty)
            {
                tmrBackup.Interval = 2000;
                tmrBackup.Start();
            }

            //更新主畫面頁籤文字
            PublishEditorTabState(isDirty);
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

        private void ApplyEditorSaveButtonsState(bool isDirty)
        {
            btnSave.Visible = !isDirty;
            btnSaveRed.Visible = isDirty;
        }

        private string BuildEditorTabTitle(bool isDirty)
        {
            return _editorTabState.BuildTitle(isDirty);
        }

        private void UpdateEditorBackupInfoIfNeeded(bool isDirty)
        {
            if (!isDirty)
            {
                return;
            }

            UpdateBackupFileInfo(_backupFileName, editor.CurrentPosition, BuildEditorTabTitle(isDirty), SavedFilePath);
        }
    }
}
