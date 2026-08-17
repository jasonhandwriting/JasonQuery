using System;
using System.Text;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorCommentService : EditorLineTransformServiceBase
    {
        private readonly Action _hideAutoCompleteGrid;

        public EditorCommentService(ITextEditor editor, Action hideAutoCompleteGrid = null) : base(editor)
        {
            _hideAutoCompleteGrid = hideAutoCompleteGrid;
        }

        public void AddComment(int shift = 1)
        {
            ExecuteTransform
            (
                CreateAddCommentOptions(shift),
                TransformAddComment
            );
        }

        public void RemoveComment(int shift = 1)
        {
            ExecuteTransform
            (
                CreateRemoveCommentOptions(shift),
                TransformRemoveComment
            );
        }

        private EditorLineTransformOptions CreateAddCommentOptions(int shift)
        {
            return new EditorLineTransformOptions
            {
                InitialSelectionMode = EditorSelectionMode.AddComment,
                FinalSelectionShift = shift,
                UseShiftEndSelectionFix = true,
                TrimTrailingCarriageReturnFromExpandedSelection = false,
                AfterSelectionFixed = _hideAutoCompleteGrid
            };
        }

        private EditorLineTransformOptions CreateRemoveCommentOptions(int shift)
        {
            return new EditorLineTransformOptions
            {
                InitialSelectionMode = EditorSelectionMode.Normal,
                FinalSelectionShift = shift,
                UseShiftEndSelectionFix = true,
                TrimTrailingCarriageReturnFromExpandedSelection = false,
                AfterSelectionFixed = _hideAutoCompleteGrid
            };
        }

        private static EditorLineTransformResult TransformAddComment(EditorLineTransformContext context)
        {
            var sbResult = new StringBuilder();
            var totalAdded = 0;
            var insertIndex = context.FirstNonSpaceStart;

            for (var i = 0; i < context.Lines.Length; i++)
            {
                var line = context.Lines[i] ?? string.Empty;
                var safeInsertIndex = Math.Max(0, Math.Min(insertIndex, line.Length));

                sbResult.Append(line.Substring(0, safeInsertIndex));
                sbResult.Append("--");
                totalAdded += 2;

                if (line.Length > safeInsertIndex)
                {
                    sbResult.Append(line.Substring(safeInsertIndex));
                }

                if (i < context.Lines.Length - 1)
                {
                    sbResult.AppendLine();
                }
            }

            return new EditorLineTransformResult
            {
                ReplacementText = sbResult.ToString(),
                SelectionLengthDelta = totalAdded
            };
        }

        private static EditorLineTransformResult TransformRemoveComment(EditorLineTransformContext context)
        {
            var sbResult = new StringBuilder();
            var totalRemoved = 0;

            for (var i = 0; i < context.Lines.Length; i++)
            {
                var line = context.Lines[i] ?? string.Empty;
                var transformedLine = line;
                var firstNonSpaceIndex = FindFirstNonSpaceIndex(line);

                if (firstNonSpaceIndex >= 0 && firstNonSpaceIndex < line.Length && line[firstNonSpaceIndex] == '-')
                {
                    if (firstNonSpaceIndex + 1 < line.Length && line[firstNonSpaceIndex + 1] == '-')
                    {
                        transformedLine = line.Substring(0, firstNonSpaceIndex) + line.Substring(firstNonSpaceIndex + 2);
                        totalRemoved += 2;
                    }
                    else
                    {
                        transformedLine = line.Substring(0, firstNonSpaceIndex) + line.Substring(firstNonSpaceIndex + 1);
                        totalRemoved += 1;
                    }
                }

                sbResult.Append(transformedLine);

                if (i < context.Lines.Length - 1)
                {
                    sbResult.AppendLine();
                }
            }

            return new EditorLineTransformResult
            {
                ReplacementText = sbResult.ToString(),
                SelectionLengthDelta = -totalRemoved
            };
        }

        private static int FindFirstNonSpaceIndex(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return -1;
            }

            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] != ' ')
                {
                    return i;
                }
            }

            return -1;
        }
    }
}