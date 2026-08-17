using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Handlers
{
    internal sealed class QueryEditorAutoCompleteEditorKeyUpHandler
    {
        public bool TryHandle(QueryEditorAutoCompleteEditorKeyUpContext context, KeyEventArgs e, ref bool checkEditorContent)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            context.Validate();

            if (TryHandleTransientState(context, e, ref checkEditorContent))
            {
                return true;
            }

            if (TryHandleNavigationOrClipboardKey(context, e))
            {
                return true;
            }

            if (TryHandleSpecialCharacterKey(context, e))
            {
                return true;
            }

            if (TryHandleTriggerKey(context, e))
            {
                return true;
            }

            return false;
        }

        private bool TryHandleNavigationOrClipboardKey(QueryEditorAutoCompleteEditorKeyUpContext context, KeyEventArgs e)
        {
            if (!IsNavigationOrClipboardKey(e))
            {
                return false;
            }

            if (ConsumeReturnedToEditorByGridUp(context))
            {
                return true;
            }

            CloseSessions(context, QueryEditorAutoCompleteSessionCloseReason.Navigation);
            context.HideAllAutoCompletePopups();
            return true;
        }

        private bool IsNavigationOrClipboardKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                return true;
            }

            if (e.KeyCode == Keys.V && e.Control)
            {
                return true;
            }

            if (e.KeyCode == Keys.X && e.Control)
            {
                return true;
            }

            if (e.KeyCode == Keys.U && e.Control)
            {
                return true;
            }

            return false;
        }

        private bool TryHandleSpecialCharacterKey(QueryEditorAutoCompleteEditorKeyUpContext context, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Multiply || e.KeyCode == Keys.D8) && context.PeriodSession != null && context.PeriodSession.IsVisible)
            {
                context.PeriodSession.Close(QueryEditorAutoCompleteSessionCloseReason.InvalidInput);
                context.HidePeriodPopup();
                return true;
            }

            return false;
        }

        private bool TryHandleTriggerKey(QueryEditorAutoCompleteEditorKeyUpContext context, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
            {
                if (context.PeriodSession != null && context.PeriodSession.IsVisible)
                {
                    context.PeriodSession.Close(QueryEditorAutoCompleteSessionCloseReason.Navigation);
                    context.HidePeriodPopup();
                }

                context.TriggerSpaceAutoComplete();
                return true;
            }

            if (e.KeyData == Keys.OemPeriod || e.KeyData == Keys.Decimal)
            {
                if (context.SpaceSession != null && context.SpaceSession.IsVisible)
                {
                    context.SpaceSession.Close(QueryEditorAutoCompleteSessionCloseReason.Navigation);
                    context.HideSpacePopup();
                }

                context.TriggerPeriodAutoComplete();
                return true;
            }

            return false;
        }

        private bool TryHandleTransientState(QueryEditorAutoCompleteEditorKeyUpContext context, KeyEventArgs e, ref bool checkEditorContent)
        {
            if (context.IsCtrlJPressed)
            {
                checkEditorContent = false;

                //Ctrl+J 通常會有兩次 KeyUp：
                //1. J
                //2. ControlKey
                //第一個先吃掉，但不要立刻 reset；等 ControlKey（或 Ctrl 已經不在 ModifierKeys 中）再 reset
                if (ShouldResetCtrlJPressed(e))
                {
                    context.ResetCtrlJPressed();
                }

                return true;
            }

            if (IsAnyCommittedByTab(context))
            {
                ResetCommitByTabFlags(context);
                return true;
            }

            return false;
        }

        private static bool ShouldResetCtrlJPressed(KeyEventArgs e)
        {
            if (e == null)
            {
                return true;
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                return true;
            }

            //若是 J 的 KeyUp，但此時 Ctrl 已經不在 ModifierKeys 中，代表 Ctrl 可能先放掉，這時也可以 reset
            if (e.KeyCode == Keys.J && (Control.ModifierKeys & Keys.Control) != Keys.Control)
            {
                return true;
            }

            return false;
        }

        private bool IsAnyCommittedByTab(QueryEditorAutoCompleteEditorKeyUpContext context)
        {
            return (context.PeriodSession != null && context.PeriodSession.CommitByTab)
                    || (context.SpaceSession != null && context.SpaceSession.CommitByTab);
        }

        private void ResetCommitByTabFlags(QueryEditorAutoCompleteEditorKeyUpContext context)
        {
            context.PeriodSession?.ConsumeCommitByTab();
            context.SpaceSession?.ConsumeCommitByTab();
        }

        private static bool ConsumeReturnedToEditorByGridUp(QueryEditorAutoCompleteEditorKeyUpContext context)
        {
            var periodConsumed = context.PeriodSession != null && context.PeriodSession.ConsumeReturnedToEditorByGridUp();
            var spaceConsumed = context.SpaceSession != null && context.SpaceSession.ConsumeReturnedToEditorByGridUp();

            return periodConsumed || spaceConsumed;
        }

        private static void CloseSessions(QueryEditorAutoCompleteEditorKeyUpContext context, QueryEditorAutoCompleteSessionCloseReason closeReason)
        {
            context.PeriodSession?.Close(closeReason);
            context.SpaceSession?.Close(closeReason);
        }
    }
}