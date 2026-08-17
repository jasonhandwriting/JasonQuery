using JasonQuery.UI.QueryEditor.KeyCommands;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void InitializeKeyCommandContext()
        {
            _keyCommandContext = new QueryEditorKeyCommandContext
            {
                TryHandleAutoCompleteProcessCmdKey = TryHandleAutoCompleteProcessCmdKey,
                TryHandleResultGridProcessCmdKey = TryHandleResultGridProcessCmdKey,

                Editor = editor,
                EditorMessage = editorMessage,
                ResultGrid = c1TrueDBGrid1,

                FindInEditor = bForward =>
                {
                    _findAndReplace.Window.FindNext(bForward);
                },

                FindInGrid = bForward =>
                {
                    if (bForward)
                    {
                        btnFindNextGrid.PerformClick();
                    }
                    else
                    {
                        btnFindPreviousGrid.PerformClick();
                    }
                },

                AfterEditorUndoRedo = () =>
                {
                    lblInfo.Text = string.Empty;
                    CheckEditorContent();
                },

                ShowEditorCommandException = ShowExceptionMessage
            };
        }
    }
}