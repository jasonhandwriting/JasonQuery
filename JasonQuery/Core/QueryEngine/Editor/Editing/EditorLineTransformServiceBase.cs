using System;
using System.Windows.Forms;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal abstract class EditorLineTransformServiceBase
    {
        private readonly ITextEditor _editor;

        protected EditorLineTransformServiceBase(ITextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        protected ITextEditor Editor => _editor;

        protected void ExecuteTransform(EditorLineTransformOptions options, Func<EditorLineTransformContext, EditorLineTransformResult> transform)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            var allText = _editor.Text ?? string.Empty;
            var originalSelectedText = _editor.SelectedText ?? string.Empty;
            var originalStart = _editor.SelectionStart;
            var originalEnd = _editor.SelectionEnd;

            var range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, originalStart, originalEnd, options.InitialSelectionMode);

            var start2 = range.Start;
            var end2 = range.End;

            if (options.TrimTrailingCarriageReturnFromExpandedSelection && end2 >= 0 && end2 < allText.Length && allText[end2] == '\r')
            {
                end2--;
            }

            var endExclusive = ToSelectionEndExclusive(end2, allText);

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = endExclusive;

            var expandedSelectedText = _editor.SelectedText ?? string.Empty;

            var context = new EditorLineTransformContext
            {
                AllText = allText,
                OriginalSelectedText = originalSelectedText,
                ExpandedSelectedText = expandedSelectedText,
                OriginalSelectionStart = originalStart,
                OriginalSelectionEnd = originalEnd,
                ExpandedSelectionStart = start2,
                ExpandedSelectionEnd = end2,
                FirstNonSpaceStart = range.FirstNonSpaceStart,
                IsNoSelection = originalStart == originalEnd,
                IsSingleLineSelection = originalSelectedText.Length == originalSelectedText.Replace("\r\n", string.Empty).Length,
                Lines = expandedSelectedText.Split(new[] { "\r\n" }, StringSplitOptions.None)
            };

            context.IsNoSelectionOrSingleLine = context.IsNoSelection || context.IsSingleLineSelection;

            var result = transform(context) ?? new EditorLineTransformResult();
            var replacementText = result.ReplacementText ?? string.Empty;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = endExclusive;
            _editor.ReplaceSelection(replacementText);

            allText = _editor.Text ?? string.Empty;

            var selectionEndAfterTransform = endExclusive + result.SelectionLengthDelta;

            if (selectionEndAfterTransform < 0)
            {
                selectionEndAfterTransform = 0;
            }

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = selectionEndAfterTransform;

            range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, start2, selectionEndAfterTransform, EditorSelectionMode.Normal);

            start2 = range.Start;
            end2 = range.End;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2 + options.FinalSelectionShift;

            _editor.Select();

            if (options.UseShiftEndSelectionFix && options.FinalSelectionShift == 1)
            {
                SendKeys.SendWait("+{END}");
            }

            options.AfterSelectionFixed?.Invoke();
        }

        private static int ToSelectionEndExclusive(int inclusiveEnd, string text)
        {
            var textLength = text?.Length ?? 0;

            if (textLength == 0)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(inclusiveEnd + 1, textLength));
        }
    }
}
