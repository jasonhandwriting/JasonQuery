using JasonLibrary.Core;
using JasonLibrary.UI.Controls;
using JasonQuery.Core.Text;
using ScintillaNET;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void ClearSqlPaneHighlightSelectionCopyState()
        {
            _isSqlPaneHighlightSelectionCopyMode = false;
            _selectedTextOnDoubleClickSqlPane = string.Empty;
        }

        private void ClearSqlPreviewHighlightSelectionCopyState()
        {
            _isSqlPreviewHighlightSelectionCopyMode = false;
            _selectedTextOnDoubleClickSqlPreview = string.Empty;
        }

        private bool TryCopySqlPaneHighlightSelectionText(string traceSource)
        {
            return TryCopySqlEditorHighlightSelectionText(
                editorSqlPane,
                _isSqlPaneHighlightSelectionCopyMode,
                _selectedTextOnDoubleClickSqlPane,
                traceSource);
        }

        private bool TryCopySqlPreviewHighlightSelectionText(string traceSource)
        {
            return TryCopySqlEditorHighlightSelectionText(
                editorSqlPreview,
                _isSqlPreviewHighlightSelectionCopyMode,
                _selectedTextOnDoubleClickSqlPreview,
                traceSource);
        }

        private bool TryCopySqlEditorHighlightSelectionText(ScintillaEditor editor, bool isHighlightSelectionCopyMode, string selectedTextOnDoubleClick, string traceSource)
        {
            if (isHighlightSelectionCopyMode && !string.IsNullOrEmpty(selectedTextOnDoubleClick))
            {
                TextHelper.CopyTextToClipboard(selectedTextOnDoubleClick, traceSource);
                return true;
            }

            var selectedText = editor.SelectedText ?? string.Empty;
            var currentWord = GetCurrentSqlEditorWord(editor);

            if (string.IsNullOrEmpty(selectedText) || string.IsNullOrEmpty(currentWord))
            {
                return false;
            }

            if (!IsRepeatedHighlightSelectionText(selectedText, currentWord))
            {
                return false;
            }

            TextHelper.CopyTextToClipboard(currentWord, traceSource);
            return true;
        }

        private string GetCurrentSqlEditorWord(ScintillaEditor editor)
        {
            var position = editor.CurrentPosition;
            var wordStart = editor.WordStartPosition(position, true);
            var wordEnd = editor.WordEndPosition(position, true);

            if (wordEnd <= wordStart)
            {
                return string.Empty;
            }

            return editor.GetTextRange(wordStart, wordEnd - wordStart);
        }

        private static bool IsRepeatedHighlightSelectionText(string selectedText, string singleText)
        {
            if (string.IsNullOrEmpty(selectedText) || string.IsNullOrEmpty(singleText))
            {
                return false;
            }

            if (selectedText.Length <= singleText.Length)
            {
                return false;
            }

            if (selectedText.Length % singleText.Length != 0)
            {
                return false;
            }

            var count = selectedText.Length / singleText.Length;

            for (var i = 0; i < count; i++)
            {
                var startIndex = i * singleText.Length;

                if (!string.Equals(selectedText.Substring(startIndex, singleText.Length), singleText, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private void CopySqlPaneSelection(string traceSource)
        {
            CopySqlEditorSelection(editorSqlPane,
                                   copyAsHtml: true,
                                   tryCopyHighlightSelectionText: () => TryCopySqlPaneHighlightSelectionText(traceSource));
        }

        private void CopySqlPreviewSelection(string traceSource)
        {
            CopySqlEditorSelection(editorSqlPreview,
                                   copyAsHtml: chkCopyAsHTML.Checked,
                                   tryCopyHighlightSelectionText: () => TryCopySqlPreviewHighlightSelectionText(traceSource));
        }

        private void CopySqlEditorSelection(ScintillaEditor editor, bool copyAsHtml, Func<bool> tryCopyHighlightSelectionText)
        {
            if (editor == null || string.IsNullOrEmpty(editor.Text))
            {
                return;
            }

            Clipboard.Clear();

            if (tryCopyHighlightSelectionText())
            {
                return;
            }

            var originalStart = -1;
            var originalEnd = -1;

            if (string.IsNullOrEmpty(editor.SelectedText))
            {
                originalStart = editor.SelectionStart;
                originalEnd = editor.SelectionEnd;

                editor.SelectionStart = 0;
                editor.SelectionEnd = editor.Text.Length;
            }

            if (copyAsHtml)
            {
                editor.Copy(CopyFormat.Text | CopyFormat.Rtf | CopyFormat.Html);
            }
            else
            {
                editor.Copy();
            }

            if (originalStart >= 0)
            {
                editor.SelectionStart = originalStart;
                editor.SelectionEnd = originalEnd;
            }
        }

        private void HighlightSqlEditorSelection(ScintillaEditor editor, Action clearCopyState, Action<string> setCopyState)
        {
            clearCopyState();

            var position = editor.CurrentPosition;
            var wordStart = editor.WordStartPosition(position, true);
            var wordEnd = editor.WordEndPosition(position, true);
            var word = editor.GetTextRange(wordStart, wordEnd - wordStart);
            var selectedText = editor.SelectedText ?? string.Empty;

            editor.AdditionalCaretsVisible = false;

            if (selectedText.Any(c => c == '\r' || c == '\n' || c == ' '))
            {
                return;
            }

            if (string.IsNullOrEmpty(word))
            {
                return;
            }

            setCopyState(word);

            var mainSelection = 0;
            var textUpper = editor.Text.ToUpperInvariant();
            var wordUpper = word.ToUpperInvariant();
            var matches = Regex.Matches(textUpper, Regex.Escape(wordUpper));

            editor.MultipleSelection = true;

            foreach (Match match in matches)
            {
                if (position >= match.Index && position - match.Index <= word.Length)
                {
                    mainSelection = match.Index;
                }
                else
                {
                    editor.AddSelection(match.Index, match.Index + word.Length);
                }
            }

            editor.AddSelection(mainSelection + word.Length, mainSelection);
        }

        private static bool IsSqlEditorCopyShortcut(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            return e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert);
        }

        private bool TryHandleSqlPaneCopyShortcutKeyDown(KeyEventArgs e)
        {
            if (!IsSqlEditorCopyShortcut(e))
            {
                return false;
            }

            e.SuppressKeyPress = true;
            e.Handled = true;

            CopySqlPaneSelection("editorSqlPane_KeyDown(CtrlC)");
            return true;
        }

        private bool TryHandleSqlPreviewCopyShortcutKeyDown(KeyEventArgs e)
        {
            if (!IsSqlEditorCopyShortcut(e))
            {
                return false;
            }

            e.SuppressKeyPress = true;
            e.Handled = true;

            CopySqlPreviewSelection("editorSqlPreview_KeyDown(CtrlC)");
            return true;
        }
    }
}