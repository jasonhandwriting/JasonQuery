using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private int _autoCompleteForAllTriggerPosition;
        private int _autoCompleteForAllCaretPosition;
        private bool _isKeyUpFromAutoCompleteForAllGrid;

        private bool TryHandleAutoCompleteForAllProcessCmdKey(Keys keyData)
        {
            if (!MyLibrary.EnableAutoComplete || !c1GridAutoCompleteForAll.Visible)
            {
                return false;
            }

            if (!editor.Focused && !c1GridAutoCompleteForAll.Focused)
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.Tab:
                    {
                        _autoCompleteForAllCaretPosition = editor.CurrentPosition;
                        PasteAutoCompleteForAllObjectName();
                        return true;
                    }
                case Keys.Escape:
                    {
                        QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        editor.CurrentPosition = _autoCompleteForAllTriggerPosition;
                        editor.Focus();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryHandleAutoCompleteForAllEditorKeyDown(KeyEventArgs e)
        {
            if (e == null || !c1GridAutoCompleteForAll.Visible)
            {
                return false;
            }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                case Keys.Space:
                case Keys.Tab:
                    {
                        QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        return true;
                    }
                case Keys.Back:
                    {
                        if (editor.CurrentPosition <= _autoCompleteForAllTriggerPosition)
                        {
                            QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        }

                        return false;
                    }
                case Keys.Up:
                    {
                        QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        return true;
                    }
                case Keys.Down:
                    {
                        _autoCompleteForAllCaretPosition = editor.CurrentPosition;
                        e.SuppressKeyPress = true;
                        c1GridAutoCompleteForAll.Focus();
                        return true;
                    }
                case Keys.Enter:
                    {
                        e.SuppressKeyPress = true;
                        QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                        return true;
                    }
                case Keys.Left:
                case Keys.Right:
                    {
                        HideAutoCompleteForAllIfCaretLeavesKeywordByArrow(e.KeyCode);
                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private void HideAutoCompleteForAllIfCaretLeavesKeywordByArrow(Keys keyCode)
        {
            var keywordStart = FindIdentifierStartBefore(_autoCompleteForAllTriggerPosition);
            var keywordEnd = FindIdentifierEndFrom(_autoCompleteForAllTriggerPosition);

            if (keyCode == Keys.Left && editor.CurrentPosition - 1 < keywordStart)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
            }
            else if (keyCode == Keys.Right && editor.CurrentPosition + 1 > keywordEnd)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
            }
        }

        private bool TryHandleAutoCompleteForAllOnEditorKeyUp(KeyEventArgs e, ref bool checkEditorContent)
        {
            if (e == null || !MyLibrary.EnableAutoComplete)
            {
                return false;
            }

            if (c1GridAutoCompleteForPeriod.Visible || c1GridAutoCompleteForSpace.Visible)
            {
                return false;
            }

            if (_isKeyUpFromAutoCompleteForAllGrid)
            {
                _isKeyUpFromAutoCompleteForAllGrid = false;
                return true;
            }

            var currentText = editor.Text ?? string.Empty;
            var currentPosition = editor.CurrentPosition;
            var currentIndex = currentPosition - 1;

            if (currentIndex < 0)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                return true;
            }

            if (IsAutoCompleteForAllIgnoredNavigationKey(e))
            {
                return false;
            }

            if (IsAutoCompleteForAllForcedHideKey(e))
            {
                HideAutoCompleteGrid(false);
                return true;
            }

            if (e.KeyData == Keys.Menu)
            {
                HideAutoCompleteGrid(false);
                return true;
            }

            if (e.KeyData == Keys.Space || e.KeyData == Keys.OemPeriod || e.KeyData == Keys.Decimal || !IsIdentifierCharAt(currentText, currentIndex))
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                return true;
            }

            if (e.KeyData == Keys.Enter || (e.KeyCode == Keys.F && e.Modifiers == Keys.Control))
            {
                return false;
            }

            if (!c1GridAutoCompleteForAll.Visible && IsAutoCompleteForAllIgnoredInvisiblePopupKey(e))
            {
                return false;
            }

            if (IsAutoCompleteForAllCloseOnlyKey(e))
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                return true;
            }

            return TryShowAutoCompleteForAll(currentText, currentIndex);
        }

        private bool TryShowAutoCompleteForAll(string currentText, int currentIndex)
        {
            var keywordStart = FindIdentifierStart(currentText, currentIndex);
            var keywordEnd = FindIdentifierEnd(currentText, currentIndex);
            var keyword = currentText.Substring(keywordStart, keywordEnd - keywordStart + 1);

            _autoCompleteForAllTriggerPosition = editor.CurrentPosition;

            var previousChar = keywordStart > 0 ? currentText.Substring(keywordStart - 1, 1) : string.Empty;
            var nextChar = keywordEnd + 1 < currentText.Length ? currentText.Substring(keywordEnd + 1, 1) : string.Empty;
            var allowedPreviousChars = new[] { string.Empty, " ", "\n", ",", ")" };
            var isBoundaryAllowed = allowedPreviousChars.Contains(previousChar) && nextChar != "'" && nextChar != "\"";

            int.TryParse(keyword, out var numericValue);

            if (numericValue != 0 || string.IsNullOrEmpty(keyword.Replace("0", string.Empty)) || !isBoundaryAllowed || keyword.Length < GetAutoCompleteForAllMinFragmentLength())
            {
                HideAutoCompleteGrid(false);
                return true;
            }

            if (GetAutoCompleteForAllFirstCharChecking() && !IsEnglishAlphabet(keyword.Substring(0, 1)))
            {
                HideAutoCompleteGrid(false);
                return true;
            }

            var rowCount = BuildAutoCompleteForAllData(keyword);

            if (rowCount <= 0)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                return true;
            }

            ShowAutoCompleteForAllPopup(keywordStart, rowCount);
            return true;
        }

        private int BuildAutoCompleteForAllData(string keyword)
        {
            var source = MyGlobal.dtAutoCompleteForAll;

            if (source == null || source.Rows.Count == 0)
            {
                c1GridAutoCompleteForAll.DataSource = null;
                return 0;
            }

            if (!source.Columns.Contains("ObjectName"))
            {
                c1GridAutoCompleteForAll.DataSource = source.Copy();
                return source.Rows.Count;
            }

            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword
            (
                source,
                keyword,
                "ObjectName",
                fallbackToAllWhenNoMatch: true,
                tieBreakerColumns: new[] { "ObjectName", "ObjectSource" }
            );

            c1GridAutoCompleteForAll.DataSource = result;
            return result.Rows.Count;
        }

        private void ShowAutoCompleteForAllPopup(int keywordStartPosition, int rowCount)
        {
            //ForAll popup 與 Period/Space popup 一樣，應該掛在 QueryForm 本身
            //若掛在 splitContainer3.Panel1，當游標接近 editor 底部時，popup 會被 Panel1 的高度限制逼到上方顯示
            EnsureAutoCompleteForAllPopupParent();
            ResizeAutoCompleteForAllGrid(c1GridAutoCompleteForAll, rowCount);

            var caretPoint = PointToClient
            (
                editor.PointToScreen
                (
                    new Point
                    (
                        editor.PointXFromPosition(keywordStartPosition),
                        editor.PointYFromPosition(keywordStartPosition)
                    )
                )
            );

            var left = caretPoint.X;
            var top = caretPoint.Y + editor.Font.Height + 3;

            if (left + c1GridAutoCompleteForAll.Width > ClientSize.Width)
            {
                left = Math.Max(0, ClientSize.Width - c1GridAutoCompleteForAll.Width - 1);
            }

            var bottomLimit = c1StatusBar1.Top > 0 ? c1StatusBar1.Top : ClientSize.Height;

            //只有整個 QueryForm 的可視範圍真的不夠時，才改顯示在游標上方
            //不要用 editor panel / c1StatusBar2 作為 bottom limit，否則底部幾行會過早翻到上方
            if (top + c1GridAutoCompleteForAll.Height > bottomLimit)
            {
                top = Math.Max(0, caretPoint.Y - c1GridAutoCompleteForAll.Height - 3);
            }

            c1GridAutoCompleteForAll.Splits[0].RecordSelectors = false;
            c1GridAutoCompleteForAll.Location = new Point(left, top);
            c1GridAutoCompleteForAll.Visible = true;
            c1GridAutoCompleteForAll.BringToFront();
            _autoCompleteMousePosition = Cursor.Position;
        }

        private void EnsureAutoCompleteForAllPopupParent()
        {
            if (c1GridAutoCompleteForAll.Parent == this)
            {
                return;
            }

            c1GridAutoCompleteForAll.Parent?.Controls.Remove(c1GridAutoCompleteForAll);
            Controls.Add(c1GridAutoCompleteForAll);
            c1GridAutoCompleteForAll.BringToFront();
        }

        private static void ResizeAutoCompleteForAllGrid(C1TrueDBGrid grid, int rowCount)
        {
            if (grid == null)
            {
                return;
            }

            var width = GridHelper.ResizeGridColumnWidth(grid);

            width += rowCount <= 9 ? 3 : grid.VScrollBar.Width + 5;

            const int height = 181;

            grid.Size = new Size(width, height);
        }

        private void c1GridAutoCompleteForAll_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Up || c1GridAutoCompleteForAll.Row != 0)
            {
                return;
            }

            _isKeyUpFromAutoCompleteForAllGrid = true;
            editor.Focus();
        }

        private void c1GridAutoCompleteForAll_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13 || e.KeyChar == 9)
                {
                    PasteAutoCompleteForAllObjectName();
                }
                else if (e.KeyChar == 8)
                {
                    DeletePreviousCharForAutoCompleteForAll();
                }
                else if (TextHelper.IsEngAlphabetOrNumber(e.KeyChar, '_'))
                {
                    InsertCharForAutoCompleteForAll(e.KeyChar);
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void c1GridAutoCompleteForAll_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PasteAutoCompleteForAllObjectName();
        }

        private void PasteAutoCompleteForAllObjectName()
        {
            try
            {
                if (c1GridAutoCompleteForAll.Row < 0)
                {
                    return;
                }

                var cellText = c1GridAutoCompleteForAll[c1GridAutoCompleteForAll.Row, 0]?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(cellText))
                {
                    return;
                }

                var fullText = $"{editor.Text} ";
                var keywordReplaceEnd = FindAutoCompleteForAllReplaceEnd(fullText);
                var keywordStart = FindIdentifierStartBefore(_autoCompleteForAllTriggerPosition);
                var keywordEnd = FindIdentifierEndFrom(_autoCompleteForAllTriggerPosition);

                editor.SelectionStart = Math.Min(_autoCompleteForAllTriggerPosition, keywordStart);
                editor.SelectionEnd = Math.Max(keywordReplaceEnd, keywordEnd);
                editor.ReplaceSelection(cellText);
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                editor.Focus();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private int FindAutoCompleteForAllReplaceEnd(string fullText)
        {
            var replaceEnd = 0;

            for (var i = _autoCompleteForAllTriggerPosition; i < fullText.Length; i++)
            {
                char currentChar = fullText[i];

                if (!TextHelper.IsEngAlphabetOrNumber(currentChar, '_'))
                {
                    if (replaceEnd == 0)
                    {
                        replaceEnd = Math.Min(_autoCompleteForAllCaretPosition, editor.CurrentPosition);
                    }

                    break;
                }

                replaceEnd = i + 1;
            }

            return replaceEnd;
        }

        private void DeletePreviousCharForAutoCompleteForAll()
        {
            var position = Math.Min(_autoCompleteForAllCaretPosition, editor.CurrentPosition);

            if (position <= 0)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                return;
            }

            editor.DeleteRange(position - 1, 1);
            editor.CurrentPosition = position - 1;
            editor.SelectionStart = editor.CurrentPosition;
            editor.SelectionEnd = editor.SelectionStart;
            _autoCompleteForAllCaretPosition = editor.CurrentPosition;
            editor.Focus();

            if (editor.CurrentPosition < _autoCompleteForAllTriggerPosition)
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
            }
        }

        private void InsertCharForAutoCompleteForAll(char value)
        {
            editor.CurrentPosition = Math.Min(_autoCompleteForAllCaretPosition, editor.CurrentPosition);
            editor.SelectionStart = editor.CurrentPosition;
            editor.SelectionEnd = editor.SelectionStart;
            editor.ReplaceSelection(value.ToString());
            _autoCompleteForAllCaretPosition = editor.CurrentPosition;
            editor.Focus();
        }

        private bool IsEditorPositionOutsideAutoCompleteForAllKeywordRange()
        {
            if (editor.CurrentPosition == _autoCompleteForAllTriggerPosition)
            {
                return false;
            }

            if (editor.CurrentPosition < _autoCompleteForAllTriggerPosition)
            {
                return true;
            }

            var keywordStart = FindIdentifierStartBefore(_autoCompleteForAllTriggerPosition);
            var keywordEnd = FindIdentifierEndFrom(_autoCompleteForAllTriggerPosition);

            return editor.CurrentPosition < keywordStart || editor.CurrentPosition > keywordEnd;
        }

        private void HandleEditorLeftMouseDownForAllAutoComplete()
        {
            if (IsEditorPositionOutsideAutoCompleteForAllKeywordRange())
            {
                QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
            }
        }

        private static bool IsAutoCompleteForAllIgnoredNavigationKey(KeyEventArgs e)
        {
            return e.KeyCode == Keys.Down || e.KeyCode == Keys.Up || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right;
        }

        private static bool IsAutoCompleteForAllIgnoredInvisiblePopupKey(KeyEventArgs e)
        {
            return e.KeyData == Keys.Up || e.KeyData == Keys.Down || e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Escape || e.KeyData == Keys.Tab;
        }

        private static bool IsAutoCompleteForAllCloseOnlyKey(KeyEventArgs e)
        {
            return e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.ControlKey;
        }

        private static bool IsAutoCompleteForAllForcedHideKey(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
                   || (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
                   || (e.KeyCode == Keys.U && e.Modifiers == Keys.Control)
                   || (e.KeyCode == Keys.U && e.Shift && e.Control)
                   || e.KeyCode == Keys.PrintScreen
                   || (e.KeyCode == Keys.PrintScreen && e.Modifiers == Keys.Control)
                   || (e.KeyCode == Keys.PrintScreen && e.Modifiers == Keys.Alt);
        }

        private static bool IsIdentifierCharAt(string text, int index)
        {
            return !string.IsNullOrEmpty(text) && index >= 0 && index < text.Length && TextHelper.IsEngAlphabetOrNumber(text[index], '_');
        }

        private int FindIdentifierStart(string text, int index)
        {
            var start = index;

            for (var i = index; i >= 0; i--)
            {
                if (TextHelper.IsEngAlphabetOrNumber(text[i], '_'))
                {
                    start = i;
                    continue;
                }

                break;
            }

            return start;
        }

        private int FindIdentifierEnd(string text, int index)
        {
            var end = index;

            for (var i = index; i < text.Length; i++)
            {
                if (TextHelper.IsEngAlphabetOrNumber(text[i], '_'))
                {
                    end = i;
                    continue;
                }

                break;
            }

            return end;
        }

        private int FindIdentifierStartBefore(int position)
        {
            var text = editor.Text ?? string.Empty;
            var start = Math.Max(0, Math.Min(position, text.Length));

            for (var i = start - 1; i >= 0; i--)
            {
                if (TextHelper.IsEngAlphabetOrNumber(text[i], '_'))
                {
                    start = i;
                    continue;
                }

                break;
            }

            return start;
        }

        private int FindIdentifierEndFrom(int position)
        {
            var text = editor.Text ?? string.Empty;
            var end = Math.Max(0, Math.Min(position, text.Length));

            for (var i = position; i < text.Length; i++)
            {
                if (TextHelper.IsEngAlphabetOrNumber(text[i], '_'))
                {
                    end = i;
                    continue;
                }

                break;
            }

            return end;
        }

        private static bool IsEnglishAlphabet(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 1)
            {
                return false;
            }

            var ch = value[0];

            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }

        private static int GetAutoCompleteForAllMinFragmentLength()
        {
            var value = GetStaticMyLibraryValue<int>("AutoCompleteMinFragmentLength", "iAutoCompleteMinFragmentLength");

            return value > 0 ? value : 1;
        }

        private static bool GetAutoCompleteForAllFirstCharChecking()
        {
            return GetStaticMyLibraryValue<bool>("AutoCompleteFirstCharChecking", "IsAutoCompleteFirstCharChecking", "bAutoCompleteFirstCharChecking");
        }

        private static T GetStaticMyLibraryValue<T>(params string[] names)
        {
            var flags = BindingFlags.Public | BindingFlags.Static;
            var type = typeof(MyLibrary);

            foreach (var name in names)
            {
                var property = type.GetProperty(name, flags);

                if (property != null && property.PropertyType == typeof(T))
                {
                    return (T)property.GetValue(null, null);
                }

                var field = type.GetField(name, flags);

                if (field != null && field.FieldType == typeof(T))
                {
                    return (T)field.GetValue(null);
                }
            }

            return default(T);
        }
    }
}