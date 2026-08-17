namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal interface ITextEditor
    {
        string Text { get; set; }

        string SelectedText { get; }

        int SelectionStart { get; set; }

        int SelectionEnd { get; set; }

        int CurrentPosition { get; set; }

        int CurrentLine { get; }

        object Tag { get; set; }

        void ReplaceSelection(string text);

        void Select();

        void ScrollCaret();

        string GetTextRange(int start, int length);
    }
}