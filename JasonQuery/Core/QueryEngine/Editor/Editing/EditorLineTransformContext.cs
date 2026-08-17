namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorLineTransformContext
    {
        public string AllText { get; set; }

        public string OriginalSelectedText { get; set; }

        public string ExpandedSelectedText { get; set; }

        public int OriginalSelectionStart { get; set; }

        public int OriginalSelectionEnd { get; set; }

        public int ExpandedSelectionStart { get; set; }

        public int ExpandedSelectionEnd { get; set; }

        public int FirstNonSpaceStart { get; set; }

        public bool IsNoSelection { get; set; }

        public bool IsSingleLineSelection { get; set; }

        public bool IsNoSelectionOrSingleLine { get; set; }

        public string[] Lines { get; set; }
    }
}