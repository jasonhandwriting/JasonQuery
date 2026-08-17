using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.KeyCommands
{
    internal static class QueryEditorKeyCommandHandler
    {
        public static bool TryHandle(Keys keyData, QueryEditorKeyCommandContext context)
        {
            if (context == null)
            {
                return false;
            }

            if (context.TryHandleAutoCompleteProcessCmdKey != null && context.TryHandleAutoCompleteProcessCmdKey(keyData))
            {
                return true;
            }

            if (context.TryHandleResultGridProcessCmdKey != null && context.TryHandleResultGridProcessCmdKey(keyData))
            {
                return true;
            }

            switch (keyData)
            {
                case Keys.F3:
                    {
                        return TryHandleFind(context, true);
                    }
                case Keys.Shift | Keys.F3:
                    {
                        return TryHandleFind(context, false);
                    }
                case Keys.Alt | Keys.Back: //Alt+Backspace
                    {
                        return TryHandleUndo(context);
                    }
                case Keys.Control | Keys.Shift | Keys.Z: //Ctrl+Shift+Z
                    {
                        return TryHandleRedo(context);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool TryHandleFind(QueryEditorKeyCommandContext context, bool bForward)
        {
            if (IsAnyEditorSearchTargetFocused(context))
            {
                if (context.FindInEditor == null)
                {
                    return false;
                }

                context.FindInEditor(bForward);
                return true;
            }

            if (IsControlFocused(context.ResultGrid))
            {
                if (context.FindInGrid == null)
                {
                    return false;
                }

                context.FindInGrid(bForward);
                return true;
            }

            return false;
        }

        private static bool TryHandleUndo(QueryEditorKeyCommandContext context)
        {
            if (!IsControlFocused(context.Editor))
            {
                return false;
            }

            try
            {
                context.Editor.Undo();
                context.AfterEditorUndoRedo?.Invoke();
            }
            catch (Exception ex)
            {
                context.ShowEditorCommandException?.Invoke(ex);
            }

            return true;
        }

        private static bool TryHandleRedo(QueryEditorKeyCommandContext context)
        {
            if (!IsControlFocused(context.Editor))
            {
                return false;
            }

            try
            {
                context.Editor.Redo();
                context.AfterEditorUndoRedo?.Invoke();
            }
            catch (Exception ex)
            {
                context.ShowEditorCommandException?.Invoke(ex);
            }

            return true;
        }

        private static bool IsAnyEditorSearchTargetFocused(QueryEditorKeyCommandContext context)
        {
            return IsControlFocused(context.Editor) || IsControlFocused(context.EditorMessage);
        }

        private static bool IsControlFocused(Control control)
        {
            return control != null && control.Focused;
        }
    }
}