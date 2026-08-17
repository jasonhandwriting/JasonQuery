namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorLineTransformResult
    {
        public string ReplacementText { get; set; }

        public int SelectionLengthDelta { get; set; }
    }
}