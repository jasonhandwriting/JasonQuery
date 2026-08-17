using JasonLibrary.Core;
using JasonQuery.Core.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HighlightSelection(bool isMouseClick = false)
        {
            ClearHighlightSelectionCopyState();

            var position = editor.CurrentPosition;
            var wordStart = editor.WordStartPosition(position, true);
            var wordEnd = editor.WordEndPosition(position, true);
            var textUpper = editor.Text.ToUpperInvariant();
            var word = editor.GetTextRange(wordStart, wordEnd - wordStart);
            var value = true;

            editor.AdditionalCaretsVisible = false; //選取的字串，最前面的 | 要不要顯示

            //如果選取文字包含了換行符號或空白，不用 multi-select！
            if (editor.SelectedText.Any(c => c == '\r' || c == '\n' || c == ' '))
            {
                value = false;
            }

            if (string.IsNullOrEmpty(word) || !value)
            {
                return;
            }

            _selectedTextOnDoubleClick = word;
            _isHighlightSelectionCopyMode = true;

            var mainSelection = 0;
            var matches = Regex.Matches(textUpper, Regex.Escape(word.ToUpperInvariant()));

            editor.MultipleSelection = true;

            foreach (Match m in matches)
            {
                if (position >= m.Index && position - m.Index <= word.Length)
                {
                    //記住游標所在處的單字
                    mainSelection = m.Index;
                }
                else
                {
                    editor.AddSelection(m.Index, m.Index + word.Length);
                }
            }

            //20250906 模仿 Notepad++ 將游標停留在單字的最後方
            editor.AddSelection(mainSelection + word.Length, mainSelection);
        }

        private void ClearHighlightSelectionCopyState()
        {
            _isHighlightSelectionCopyMode = false;
            _selectedTextOnDoubleClick = string.Empty;
        }

        private bool TryCopyHighlightSelectionText(string traceSource)
        {
            if (_isHighlightSelectionCopyMode && !string.IsNullOrEmpty(_selectedTextOnDoubleClick))
            {
                TextHelper.CopyTextToClipboard(_selectedTextOnDoubleClick, traceSource);
                return true;
            }

            var selectedText = editor.SelectedText ?? string.Empty;
            var currentWord = GetCurrentEditorWord();

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

        private string GetCurrentEditorWord()
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

        private void CopyEditorSelectionForShortcut(string traceSource)
        {
            Clipboard.Clear();

            if (TryCopyHighlightSelectionText(traceSource))
            {
                return;
            }

            if (MyLibrary.CopyAsHTML)
            {
                editor.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
            }
            else
            {
                editor.Copy();
            }
        }
    }
}