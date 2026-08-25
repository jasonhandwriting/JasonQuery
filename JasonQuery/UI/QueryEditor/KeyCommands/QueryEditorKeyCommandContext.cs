using System;
using System.Windows.Forms;
using ScintillaNET;

namespace JasonQuery.UI.QueryEditor.KeyCommands
{
    internal sealed class QueryEditorKeyCommandContext
    {
        /// <summary>
        /// 讓 QueryForm 可先攔截 AutoComplete 專屬按鍵，例如 Escape / Tab / Enter / Up / Down。
        /// </summary>
        public Func<Keys, bool> TryHandleAutoCompleteProcessCmdKey { get; set; }

        /// <summary>
        /// 讓 QueryForm 可攔截 Result Grid 專屬按鍵，例如 Ctrl+Home / Ctrl+End / Down / PageDown。
        /// </summary>
        public Func<Keys, bool> TryHandleResultGridProcessCmdKey { get; set; }

        public Scintilla Editor { get; set; }

        public Control EditorMessage { get; set; }

        public Control ResultGrid { get; set; }

        public Action<bool> FindInEditor { get; set; }

        public Action<bool> FindInGrid { get; set; }

        public Action AfterEditorUndoRedo { get; set; }

        public Action<Exception> ShowEditorCommandException { get; set; }
    }
}
