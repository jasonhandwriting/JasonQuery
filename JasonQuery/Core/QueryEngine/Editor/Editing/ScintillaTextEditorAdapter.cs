using ScintillaNET;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class ScintillaTextEditorAdapter : ITextEditor
    {
        private readonly Scintilla _editor;

        public ScintillaTextEditorAdapter(Scintilla editor)
        {
            _editor = editor;
        }

        public string Text
        {
            get => _editor.Text;
            set => _editor.Text = value;
        }

        public string SelectedText => _editor.SelectedText;

        public int SelectionStart
        {
            get => _editor.SelectionStart;
            set => _editor.SelectionStart = value;
        }

        public int SelectionEnd
        {
            get => _editor.SelectionEnd;
            set => _editor.SelectionEnd = value;
        }

        public int CurrentPosition
        {
            get => _editor.CurrentPosition;
            set => _editor.CurrentPosition = value;
        }

        public int CurrentLine => _editor.LineFromPosition(_editor.CurrentPosition);

        public object Tag
        {
            get => _editor.Tag;
            set => _editor.Tag = value;
        }

        public void ReplaceSelection(string text)
        {
            _editor.ReplaceSelection(text);
        }

        public void Select()
        {
            _editor.Select();
        }

        public void ScrollCaret()
        {
            _editor.ScrollCaret();
        }

        public string GetTextRange(int start, int length)
        {
            return _editor.GetTextRange(start, length);
        }
    }
}