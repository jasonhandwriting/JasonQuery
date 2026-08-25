namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorSelectionRange
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int FirstNonSpaceStart { get; set; }
    }
}
