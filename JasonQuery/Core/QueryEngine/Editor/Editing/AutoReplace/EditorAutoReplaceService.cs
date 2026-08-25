using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;

namespace JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace
{
    internal sealed class EditorAutoReplaceService
    {
        public bool TryHandleSpaceKeyUp(EditorAutoReplaceContext context, KeyEventArgs e)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.KeyCode != Keys.Space)
            {
                return false;
            }

            if (e.Control)
            {
                return false;
            }

            var editor = context.Editor;

            if (editor.Text.Trim().Length < 2 || editor.CurrentPosition <= 2)
            {
                return false;
            }

            if (editor.Text[editor.CurrentPosition - 1] != ' ')
            {
                return false;
            }

            int posStart = 0;
            int posEnd = 0;
            var text = editor.Text;
            var count = editor.CurrentPosition - 2; //扣掉剛輸入的空白

            for (var i = count; i >= 0; i--)
            {
                char chTemp = text[i];

                //找到第一個空白或換行符號，或已到字串開頭
                if (chTemp == ' ' || chTemp == '\n' || i == 0)
                {
                    posStart = (i == 0) ? i : i + 1;
                    posEnd = editor.CurrentPosition - 1;
                    break;
                }
            }

            editor.SelectionStart = posStart;
            editor.SelectionEnd = posEnd + 1; //多選取最後的空白

            var temp = TextHelper.GetValueFromDictionary(context.AutoReplaceMap, editor.SelectedText.Trim());

            if (string.IsNullOrEmpty(temp))
            {
                editor.SelectionStart = editor.CurrentPosition;
                editor.SelectionEnd = editor.CurrentPosition;
                return false;
            }

            var caretMarker = temp.IndexOf('^');

            if (caretMarker > 0)
            {
                caretMarker += editor.CurrentPosition - editor.SelectedText.Length;
                temp = temp.Replace("^", string.Empty);
            }

            editor.ReplaceSelection(string.Format("{0} ", temp));

            if (caretMarker > 0)
            {
                editor.SelectionStart = caretMarker;
                editor.SelectionEnd = caretMarker;
            }
            else
            {
                editor.SelectionStart = editor.CurrentPosition;
                editor.SelectionEnd = editor.CurrentPosition;
            }

            return true;
        }
    }
}
