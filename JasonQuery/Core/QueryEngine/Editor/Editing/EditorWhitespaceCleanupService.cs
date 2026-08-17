using System.Text;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorWhitespaceCleanupService : EditorLineTransformServiceBase
    {
        public EditorWhitespaceCleanupService(ITextEditor editor) : base(editor) { }

        public void RemoveTrailingBlanks(int shift = 1)
        {
            ExecuteTransform(CreateRemoveTrailingBlanksOptions(shift), TransformRemoveTrailingBlanks);
        }

        private static EditorLineTransformOptions CreateRemoveTrailingBlanksOptions(int shift)
        {
            return new EditorLineTransformOptions
            {
                InitialSelectionMode = EditorSelectionMode.AddComment,
                FinalSelectionShift = shift,
                UseShiftEndSelectionFix = true,
                TrimTrailingCarriageReturnFromExpandedSelection = false
            };
        }

        private static EditorLineTransformResult TransformRemoveTrailingBlanks(EditorLineTransformContext context)
        {
            var sbResult = new StringBuilder();
            var trimLength = 0;

            for (var i = 0; i < context.Lines.Length; i++)
            {
                var line = context.Lines[i] ?? string.Empty;
                var trimmedLine = line.TrimEnd();

                trimLength += line.Length - trimmedLine.Length;
                sbResult.Append(trimmedLine);

                if (i < context.Lines.Length - 1)
                {
                    sbResult.Append("\r\n");
                }
            }

            return new EditorLineTransformResult
            {
                ReplacementText = sbResult.ToString(),
                SelectionLengthDelta = -trimLength
            };
        }
    }
}