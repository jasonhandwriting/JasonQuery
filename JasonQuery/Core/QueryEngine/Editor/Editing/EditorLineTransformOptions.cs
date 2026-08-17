using System;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorLineTransformOptions
    {
        public EditorSelectionMode InitialSelectionMode { get; set; } = EditorSelectionMode.Normal;

        public int FinalSelectionShift { get; set; } = 1;

        public bool UseShiftEndSelectionFix { get; set; } = true;

        public bool TrimTrailingCarriageReturnFromExpandedSelection { get; set; } = false;

        public Action AfterSelectionFixed { get; set; }
    }
}