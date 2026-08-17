using JasonQuery.Core.Text;
using System;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorTextCaseService
    {
        private readonly ITextEditor _editor;

        public EditorTextCaseService(ITextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public void ConvertSelectionCase(bool toUpper)
        {
            var tagText = TextHelper.GetSafeString(_editor.Tag);
            var selectedText = string.IsNullOrEmpty(tagText) ? _editor.SelectedText : tagText;

            if (string.IsNullOrEmpty(selectedText))
            {
                return;
            }

            var start = _editor.SelectionStart;
            var end = _editor.SelectionEnd;

            var result = TextHelper.GetTransferString(toUpper, selectedText);

            _editor.SelectionStart = start;
            _editor.SelectionEnd = end;

            _editor.ReplaceSelection(result);

            _editor.SelectionStart = start;
            _editor.SelectionEnd = end;
        }

        public void ConvertSelectionToUpper()
        {
            ConvertSelectionCase(true);
        }

        public void ConvertSelectionToLower()
        {
            ConvertSelectionCase(false);
        }
    }
}