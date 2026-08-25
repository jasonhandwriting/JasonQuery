using JasonLibrary.Core;
using JasonQuery.Core.Text;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.KeyCommands;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (TryHandleAutoCompleteForAllProcessCmdKey(keyData))
            {
                return true;
            }

            if (TryHandleCopyShortcuts(keyData))
            {
                return true;
            }

            if (QueryEditorKeyCommandHandler.TryHandle(keyData, _keyCommandContext))
            {
                return true;
            }

            if (TryHandleFormFunctionKeys(keyData))
            {
                return true;
            }

            if (TryHandleFileShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleEditorClipboardShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleEditorDeleteKey(keyData))
            {
                return true;
            }

            if (TryHandleSchemaBrowserPasteShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleExtendedSchemaBrowserPasteShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleEditorSelectionAndPasteShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleWindowAndTabShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleEditorExecutionAndTransformShortcuts(keyData))
            {
                return true;
            }

            if (TryHandleSearchAndAutoCompleteShortcuts(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool TryHandleFormFunctionKeys(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    {
                        btnQuery.PerformClick();
                        return true;
                    }
                case Keys.F6:
                    {
                        c1DockingTab2.SelectedTab = tabAutoReplace;
                        return true;
                    }
                case Keys.F7:
                    {
                        c1DockingTab2.SelectedTab = tabSchemaInformation;
                        return true;
                    }
                case Keys.F8:
                    {
                        c1DockingTab2.SelectedTab = tabTabList;
                        return true;
                    }
                case Keys.F9:
                    {
                        if (tabSqlNavigator.TabVisible)
                        {
                            c1DockingTab2.SelectedTab = tabSqlNavigator;
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleFileShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    {
                        ExecuteSaveCommand();
                        return true;
                    }
                case Keys.F12:
                    {
                        ExecuteSaveAsCommand();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleEditorClipboardShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Space:
                    {
                        if (editor.Focused)
                        {
                            return true;
                        }

                        return false;
                    }
                case Keys.Control | Keys.X:
                case Keys.Shift | Keys.Delete:
                    {
                        if (!editor.Focused)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        try
                        {
                            if (TryCopyHighlightSelectionText("ProcessCmdKey(CtrlX)"))
                            {
                                editor.Clear();
                            }
                            else
                            {
                                if (MyLibrary.CopyAsHTML)
                                {
                                    editor.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                                }
                                else
                                {
                                    editor.Copy();
                                }

                                editor.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowExceptionMessage(ex);
                        }

                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleEditorDeleteKey(Keys keyData)
        {
            if (keyData != Keys.Delete)
            {
                return false;
            }

            if (!editor.Focused)
            {
                return false; //其他控制項維持原本行為
            }

            try
            {
                editor.Clear();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }

            return true;
        }

        private bool TryHandleSchemaBrowserPasteShortcuts(Keys keyData)
        {
            if (!c1GridSchemaBrowser.Focused)
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.Control | Keys.Alt | Keys.V:
                    {
                        ArrangeSchemaDataForCopyPaste("Paste3");
                        return true;
                    }
                case Keys.Control | Keys.Shift | Keys.V:
                    {
                        ArrangeSchemaDataForCopyPaste("Paste4");
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleExtendedSchemaBrowserPasteShortcuts(Keys keyData)
        {
            if (!c1GridSchemaBrowser.Focused)
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.Alt | Keys.Shift | Keys.V:
                    {
                        ArrangeSchemaDataForCopyPaste("Paste5");
                        return true;
                    }
                case Keys.Control | Keys.Alt | Keys.Shift | Keys.V:
                    {
                        ArrangeSchemaDataForCopyPaste("Paste6");
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleCopyShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                case Keys.Control | Keys.Insert:
                    {
                        if (c1TrueDBGrid1.Focused)
                        {
                            ArrangeData(QueryResultCopyMode.Copy);
                            return true;
                        }

                        if (c1GridSchemaBrowser.Focused)
                        {
                            ArrangeSchemaDataForCopyPaste("Copy");
                            return true;
                        }

                        if (editorMessage.Focused)
                        {
                            editorMessage.Copy(); //20260716 補上 editorMessage 的 Ctrl+C 快捷鍵
                            return true;
                        }

                        if (!editor.Focused)
                        {
                            return false;
                        }

                        try
                        {
                            CopyEditorSelectionForShortcut("ProcessCmdKey(editor_CtrlC)");
                        }
                        catch (Exception ex)
                        {
                            ShowExceptionMessage(ex);
                        }

                        return true;
                    }
                case Keys.Control | Keys.A:
                    {
                        if (c1TrueDBGrid1.Focused)
                        {
                            c1TrueDBGrid1.SelectedRows.Clear();

                            var rowCount = c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count;

                            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                            {
                                c1TrueDBGrid1.SelectedRows.Add(rowIndex);
                            }

                            return true;
                        }

                        return false;
                    }
                case Keys.Control | Keys.B:
                    {
                        if (editor.Focused)
                        {
                            SelectCurrentBlock();
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleEditorSelectionAndPasteShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.V:
                case Keys.Shift | Keys.Insert:
                    {
                        if (nudQueryTimeout.Focused)
                        {
                            return true; //不允許貼上
                        }

                        if (editor.Focused && editor.CanPaste)
                        {
                            var originalText = Clipboard.GetText();

                            TextHelper.CopyTextToClipboard(originalText.Replace("\t", "    "), "ProcessCmdKey_CtrlV");
                            editor.Paste();
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleWindowAndTabShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    {
                        TransferValueToMainForm("CreateNewTab`");
                        return true;
                    }
                case Keys.Control | Keys.O:
                    {
                        //20191005 按快速鍵，改為「在新的頁籤開啟」
                        TransferValueToMainForm("CreateNewTab`OPENFILE");
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleEditorExecutionAndTransformShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Shift | Keys.Enter:
                    {
                        if (!editor.Focused)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        ExecuteCurrentLine();
                        return true;
                    }
                case Keys.Control | Keys.Enter:
                    {
                        if (!editor.Focused)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        if (c1GridAutoCompleteForPeriod.Visible)
                        {
                            HidePeriodAutoCompletePopup();
                        }

                        if (c1GridAutoCompleteForSpace.Visible)
                        {
                            HideSpaceAutoCompletePopup();
                        }

                        if (c1GridAutoCompleteForAll.Visible)
                        {
                            QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        }

                        SelectCurrentBlock();
                        btnQuery.PerformClick();
                        return true;
                    }
                case Keys.Control | Keys.Shift | Keys.U:
                    {
                        _compoundKeyCtrlShift = 3;

                        if (!editor.Focused)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        ConvertSelectionCase(true);
                        return true;
                    }
                case Keys.Control | Keys.U:
                    {
                        if (!editor.Focused)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        ConvertSelectionCase(false);
                        return true;
                    }
                case Keys.Control | Keys.Y:
                    {
                        if (!editor.Focused || !editor.CanRedo)
                        {
                            return false; //20250903 其他控件維持原動作
                        }

                        editor.Redo();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleSearchAndAutoCompleteShortcuts(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.F:
                    {
                        HideAutoCompleteGrid(false); //顯示搜尋視窗時，強制隱藏 AutoComplete 清單
                        _findAndReplace.ShowFind();
                        return true;
                    }
                case Keys.Control | Keys.H:
                    {
                        HideAutoCompleteGrid(false); //顯示搜尋視窗時，強制隱藏 AutoComplete 清單
                        _findAndReplace.ShowReplace();
                        return true;
                    }
                case Keys.Escape:
                    {
                        if (c1GridAutoCompleteForPeriod.Visible)
                        {
                            var triggerPosition = _periodAutoCompleteSession.TriggerPosition;

                            HidePeriodAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason.Escape);
                            editor.CurrentPosition = triggerPosition;
                            editor.Focus();
                            return true;
                        }

                        if (c1GridAutoCompleteForSpace.Visible)
                        {
                            var triggerPosition = _spaceAutoCompleteSession.TriggerPosition;

                            HideSpaceAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason.Escape);
                            editor.CurrentPosition = triggerPosition;
                            editor.Focus();
                            return true;
                        }

                        if (c1GridAutoCompleteForAll.Visible)
                        {
                            QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                            editor.CurrentPosition = _autoCompleteForAllTriggerPosition;
                            editor.Focus();
                            return true;
                        }

                        return false; //20250903 其他控件維持原動作
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
