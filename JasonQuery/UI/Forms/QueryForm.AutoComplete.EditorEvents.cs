using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void editor_Insert(object sender, ScintillaNET.ModificationEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(AccessibleDefaultActionDescription) && string.IsNullOrEmpty(editor.Text))
                {
                    CheckEditorContent();
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void editor_KeyDown(object sender, KeyEventArgs e)
        {
            //會先經過 ProcessCmdKey()，且 ProcessCmdKey() 回傳 false，才會再進到 editor_KeyDown()

            try
            {
                if (TryHandleEditorCopyShortcutKeyDown(e))
                {
                    return;
                }

                if (TryHandleAutoCompleteForAllEditorKeyDown(e))
                {
                    return;
                }

                var session = GetActiveAutoCompleteSession();

                if (session == null)
                {
                    return;
                }

                QueryEditorAutoCompleteEditorKeyHandler.HandleKeyDown(_autoCompleteContext, session, e);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar < 32)
            {
                //Prevent control characters from getting inserted into the text buffer
                e.Handled = true;
            }
        }

        private void editor_KeyUp(object sender, KeyEventArgs e)
        {
            var shouldCheckEditorContentAfterKeyUp = !ShouldSkipEditorContentCheckOnKeyUp(e);

            if (IsEditorCopyShortcutKeyUp(e))
            {
                try
                {
                    HideAutoCompleteGrid(false);
                    CopyEditorSelectionForShortcut("editor_KeyUp(editor_CtrlC)");
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
                finally
                {
                    ClearHighlightSelectionCopyState();
                }

                return;
            }

            ClearHighlightSelectionCopyState();

            if (TryConsumeCompoundCtrlShiftKeyUp())
            {
                return;
            }

            try
            {
                if (ShouldIgnoreEditorKeyUp(e))
                {
                    return;
                }

                HandleAutoReplaceOnSpaceKeyUp(e);

                if (ShouldSuppressSpaceAutoCompleteAfterComma(e))
                {
                    HideAutoCompleteGrid(false);
                    return;
                }

                var wasAutoCompleteForAllVisible = c1GridAutoCompleteForAll.Visible;

                if (wasAutoCompleteForAllVisible && (e.KeyData == Keys.OemPeriod || e.KeyData == Keys.Decimal))
                {
                    //ForAll 已顯示時再輸入句點，要讓 Period 路線重新判斷
                    QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                    wasAutoCompleteForAllVisible = false;
                }

                if (MyGlobal.IsAutoListMembers && !wasAutoCompleteForAllVisible)
                {
                    if (_autoCompleteEditorKeyUpHandler.TryHandle(CreateEditorKeyUpContext(), e, ref shouldCheckEditorContentAfterKeyUp))
                    {
                        return;
                    }

                    //20260425 判斷是否為 Period 的輸入情境，若符合則 Popup
                    if (_periodTriggerCoordinator.TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(e))
                    {
                        return;
                    }

                    //20260424 按下不合法字元，嘗試關閉 Space 的 AutoComplete Popup
                    if (_autoCompleteFilterCoordinator.TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(e))
                    {
                        return;
                    }

                    RefreshVisibleAutoCompleteOnEditorKeyUp(e);
                }

                TryHandleAutoCompleteForAllOnEditorKeyUp(e, ref shouldCheckEditorContentAfterKeyUp);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                if (shouldCheckEditorContentAfterKeyUp)
                {
                    CheckEditorContent();
                }
            }
        }

        private bool ShouldSuppressSpaceAutoCompleteAfterComma(KeyEventArgs e)
        {
            if (e == null || e.KeyCode != Keys.Space || e.Control || e.Alt)
            {
                return false;
            }

            var isAnyPopupVisible = c1GridAutoCompleteForAll.Visible || c1GridAutoCompleteForPeriod.Visible || c1GridAutoCompleteForSpace.Visible;

            return QueryEditorAutoCompleteTriggerPolicy.ShouldSuppressSpaceAfterComma
            (
                editor.Text,
                editor.CurrentPosition,
                isAnyPopupVisible
            );
        }

        private bool TryHandleEditorCopyShortcutKeyDown(KeyEventArgs e)
        {
            if (!IsEditorCopyShortcut(e))
            {
                return false;
            }

            e.SuppressKeyPress = true;
            e.Handled = true;

            CopyEditorSelectionForShortcut("editor_KeyDown(editor_CtrlC)");
            return true;
        }

        private static bool IsEditorCopyShortcut(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            return e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert);
        }

        private static bool IsEditorCopyShortcutKeyUp(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            return (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
                   || (e.KeyCode == Keys.Insert && e.Modifiers == Keys.Control);
        }

        #region 20260405 for AutoReplace
        private EditorAutoReplaceContext CreateEditorAutoReplaceContext()
        {
            return new EditorAutoReplaceContext
            {
                Editor = editor,
                AutoReplaceMap = _autoReplacements
            };
        }
        #endregion

        #region editor_KeyUp 20260404 重構
        private bool TryConsumeCompoundCtrlShiftKeyUp() //忽略 Ctrl+Shift 組合鍵收尾
        {
            if (_compoundKeyCtrlShift > 0)
            {
                _compoundKeyCtrlShift--;
                return true;
            }

            return false;
        }

        private bool ShouldIgnoreEditorKeyUp(KeyEventArgs e) //忽略不需要處理的 KeyUp
        {
            switch (e.KeyCode)
            {
                case Keys.Insert:
                case Keys.Capital:
                case Keys.NumLock:
                case Keys.Scroll:
                case Keys.PrintScreen:
                case Keys.Pause:
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool ShouldSkipEditorContentCheckOnKeyUp(KeyEventArgs e) //哪些 KeyUp 不需要 CheckEditorContent()
        {
            return e.KeyData == Keys.Up || e.KeyData == Keys.Down || e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Escape
                   || e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown
                   || e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ProcessKey || e.KeyCode == Keys.Apps;
        }

        private void HandleAutoReplaceOnSpaceKeyUp(KeyEventArgs e)
        {
            var context = CreateEditorAutoReplaceContext();

            _editorAutoReplaceService.TryHandleSpaceKeyUp(context, e);
        }

        private QueryEditorAutoCompleteEditorKeyUpContext CreateEditorKeyUpContext()
        {
            return _autoCompleteUiContextBuilder.CreateEditorKeyUpContext
            (
                _periodAutoCompleteSession,
                _spaceAutoCompleteSession,
                _periodTriggerCoordinator.IsCtrlJKeyPressPending,
                _periodTriggerCoordinator.ResetCtrlJKeyPressPending,
                () => HideAutoCompleteGrid(false),
                c1GridAutoCompleteForPeriod,
                c1GridAutoCompleteForSpace,
                () => HandleAutoCompletePeriodKey(),
                () => HandleAutoCompleteSpaceKey()
            );
        }

        private void RefreshVisibleAutoCompleteOnEditorKeyUp(KeyEventArgs e) //可見 popup 的重新整理區: 入口
        {
            var session = GetActiveAutoCompleteSession();

            if (session == null)
            {
                return;
            }

            switch (session.Kind)
            {
                case QueryEditorAutoCompletePopupKind.Period:
                    {
                        var context = CreateVisiblePopupRefreshContext
                        (
                            session,
                            e,
                            BuildPeriodAutoCompleteData,
                            request => _autoCompletePopupPresenter.ShowResolvedPeriodAutoComplete(request),
                            () => _autoCompletePopupPresenter.ResizePeriodPopup()
                        );

                        _visiblePopupRefresher.TryRefreshForwardTriggerPopup(context);
                        break;
                    }

                case QueryEditorAutoCompletePopupKind.Space:
                    {
                        var context = CreateVisiblePopupRefreshContext
                        (
                            session,
                            e,
                            BuildSpaceAutoCompleteData,
                            request => _autoCompletePopupPresenter.ShowResolvedSpaceAutoComplete(request),
                            () => _autoCompletePopupPresenter.ResizeSpacePopup()
                        );

                        _visiblePopupRefresher.TryRefreshForwardTriggerPopup(context);
                        break;
                    }
            }
        }

        private QueryEditorAutoCompleteVisiblePopupRefreshContext CreateVisiblePopupRefreshContext
        (
            QueryEditorAutoCompleteSession session,
            KeyEventArgs e,
            Func<string, DataTable> buildData,
            Action<QueryEditorAutoCompleteRequest> showResolvedRequest,
            Action afterShow
        )
        {
            return _autoCompleteUiContextBuilder.CreateVisiblePopupRefreshContext
            (
                session,
                e,
                buildData,
                showResolvedRequest,
                afterShow
            );
        }
        #endregion
    }
}