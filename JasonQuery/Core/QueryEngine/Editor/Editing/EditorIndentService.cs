using System;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorIndentService
    {
        private readonly ITextEditor _editor;

        public EditorIndentService(ITextEditor editor)
        {
            _editor = editor;
        }

        public void IndentSelection(int indentSize)
        {
            int shift = 1;
            var allText = _editor.Text;
            var selectedText = _editor.SelectedText;
            var indentCount = 0;
            var start = _editor.SelectionStart;
            var end = _editor.SelectionEnd;

            var range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, start, end, EditorSelectionMode.Normal);

            var start2 = range.Start;
            var end2 = range.End;

            if (end2 >= 0 && end2 < _editor.Text.Length && _editor.Text[end2] == '\r')
            {
                end2--;
            }

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2;

            selectedText = _editor.SelectedText;

            var parts = selectedText.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sbResult = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var sbTemp = new StringBuilder();

                for (var j = 0; j < indentSize; j++)
                {
                    sbTemp.Append(" ");
                    indentCount++;
                }

                if (!string.IsNullOrEmpty(parts[i]))
                {
                    sbResult.Append(sbTemp);
                    sbResult.Append(parts[i]);
                }

                if (i < parts.Length - 1)
                {
                    sbResult.AppendLine();
                }
            }

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2;

            _editor.ReplaceSelection(sbResult.ToString());

            allText = _editor.Text;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2 + indentCount;

            range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, start2, end2 + indentCount, EditorSelectionMode.Normal);
            start2 = range.Start;
            end2 = range.End;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2 + shift;

            _editor.Select();

            if (shift == 1)
            {
                SendKeys.SendWait("+{END}");
            }
        }

        public void UnindentSelection(int indentSize)
        {
            int shift = 1;
            var allText = _editor.Text;
            var selectedText = _editor.SelectedText;
            var unindentCount = 0;
            var start = _editor.SelectionStart;
            var end = _editor.SelectionEnd;

            var range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, start, end, EditorSelectionMode.Normal);

            var start2 = range.Start;
            var end2 = range.End;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2;

            selectedText = _editor.SelectedText;

            var parts = selectedText.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sbResult = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var temp = parts[i];

                if (!string.IsNullOrEmpty(temp))
                {
                    for (var j = 0; j < indentSize; j++)
                    {
                        if (string.IsNullOrEmpty(temp))
                        {
                            break;
                        }

                        if (temp[0] != ' ')
                        {
                            continue;
                        }

                        temp = temp.Substring(1, temp.Length - 1);
                        unindentCount++;
                    }
                }

                sbResult.Append(temp);

                if (i < parts.Length - 1)
                {
                    sbResult.Append("\r\n");
                }
            }

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2;

            _editor.ReplaceSelection(sbResult.ToString());

            allText = _editor.Text;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2 - unindentCount;

            range = EditorSelectionHelper.GetExpandedSelectionRange(_editor, allText, start2, end2 - unindentCount, EditorSelectionMode.Normal);
            start2 = range.Start;
            end2 = range.End;

            _editor.SelectionStart = start2;
            _editor.SelectionEnd = end2 + shift;

            _editor.Select();

            if (shift == 1)
            {
                SendKeys.SendWait("+{END}");
            }
        }
    }
}