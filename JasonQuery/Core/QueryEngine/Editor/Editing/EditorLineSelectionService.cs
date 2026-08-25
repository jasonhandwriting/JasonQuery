using System;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorLineSelectionService
    {
        private readonly ITextEditor _editor;

        public EditorLineSelectionService(ITextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public string SelectActiveLine()
        {
            var text = _editor.Text ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var currentPosition = _editor.CurrentPosition;

            if (currentPosition < 0)
            {
                currentPosition = 0;
            }
            else if (currentPosition >= text.Length)
            {
                currentPosition = text.Length - 1;
            }

            var start = currentPosition;
            var end = currentPosition;

            while (start > 0 && text[start - 1] != '\n')
            {
                start--;
            }

            while (end < text.Length && text[end] != '\r' && text[end] != '\n')
            {
                end++;
            }

            _editor.SelectionStart = end;
            _editor.CurrentPosition = start;
            _editor.ScrollCaret();

            return _editor.SelectedText;
        }
    }
}
