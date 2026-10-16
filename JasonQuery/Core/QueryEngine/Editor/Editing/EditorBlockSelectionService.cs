using System;
using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorBlockSelectionService
    {
        private readonly ITextEditor _editor;

        public EditorBlockSelectionService(ITextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public string SelectCurrentBlock(bool select = true)
        {
            var allText = _editor.Text ?? string.Empty;
            var selectedText = _editor.SelectedText ?? string.Empty;

            if (string.IsNullOrEmpty(allText))
            {
                return string.Empty;
            }

            if (allText == selectedText)
            {
                _editor.SelectionEnd = _editor.SelectionStart;
            }

            var range = EditorSelectionHelper.GetCurrentBlockRangeTreatEntireBlankRowAsEmptyRow(_editor);

            if (!select)
            {
                return _editor.GetTextRange(range.Start, range.End - range.Start);
            }

            _editor.SelectionStart = range.End;
            _editor.CurrentPosition = range.Start;

            return string.Empty;
        }

        //Interactive SQL selection only. AutoComplete continues to use the unchanged SelectCurrentBlock(false) legacy blank-row behavior.
        public string SelectCurrentStatement(DataSourceType dataSourceType)
        {
            var allText = _editor.Text ?? string.Empty;

            if (string.IsNullOrEmpty(allText))
            {
                return string.Empty;
            }

            if (allText == (_editor.SelectedText ?? string.Empty))
            {
                _editor.SelectionEnd = _editor.SelectionStart;
            }

            var legacy = EditorSelectionHelper.GetCurrentBlockRangeTreatEntireBlankRowAsEmptyRow(_editor);
            var range = EditorSqlStatementSelectionPolicy.Resolve(allText, _editor.CurrentPosition, dataSourceType, legacy);

            _editor.SelectionStart = range.End;
            _editor.CurrentPosition = range.Start;

            return string.Empty;
        }
    }
}
