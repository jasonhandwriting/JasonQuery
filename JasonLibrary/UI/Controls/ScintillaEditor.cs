using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using ScintillaNET;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace JasonLibrary.UI.Controls
{
    public sealed class ScintillaEditor : Scintilla
    {
        public ScintillaEditor()
        {
            _styler = null;
        }

        #region Private Fields

        private int _lastCaretPos;
        private ScintillaStyler _styler;
        #endregion

        #region Private Methods
        private void Reset()
        {
            StyleResetDefault();

            //20190314 變更字型
            if (string.IsNullOrEmpty(MyLibrary.QueryEditorFontName))
            {
                MyLibrary.QueryEditorFontName = "Consolas";
            }

            Styles[Style.Default].Font = MyLibrary.QueryEditorFontName;
            Styles[Style.Default].Size = MyLibrary.QueryEditorFontSize; //設定預設的字型大小
            StyleClearAll();

            _styler?.RemoveStyle(this);

            SetKeywords(0, string.Empty);
            SetKeywords(1, string.Empty);
            SetKeywords(2, string.Empty);
            SetKeywords(3, string.Empty);
            SetKeywords(4, string.Empty);

            //Some base styles
            IndentationGuides = IndentView.LookBoth;
            Styles[Style.BraceLight].BackColor = Color.LightGray;
            Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            Styles[Style.BraceBad].ForeColor = Color.Red;
        }
        #endregion

        #region Public Properties
        public ScintillaStyler Styler
        {
            get => _styler;
            set
            {
                Reset();

                _styler = value;

                if (_styler == null)
                {
                    SetCodeFolding(false);
                    SetLineNumbering(false);
                    SetBraceMatching(false);
                    SetAutoIndent(false);
                }
                else
                {
                    Lexer = _styler.Lexer;
                    SetCodeFolding(_styler.CodeFolding);
                    SetLineNumbering(_styler.ShowLineNumbers);
                    SetBraceMatching(_styler.BraceMatching);
                    SetAutoIndent(_styler.AutoIndent);
                    _styler.ApplyStyle(this);
                    _styler.SetKeywords(this);
                }
            }
        }
        #endregion

        #region Public Methods
        public void SetCodeFolding(bool enabled)
        {
            if (enabled)
            {
                SetProperty("fold", "1");

                Margins[2].Type = MarginType.Symbol;
                Margins[2].Mask = Marker.MaskFolders;
                Margins[2].Sensitive = true;
                Margins[2].Width = 20;

                for (var i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
                {
                    Markers[i].SetForeColor(SystemColors.ControlLightLight);
                    Markers[i].SetBackColor(SystemColors.ControlDark);
                }

                Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
                Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
                Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
                Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
                Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
                Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
                Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
                Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
                Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

                AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
            }
            else
            {
                SetProperty("fold", "0");
                Margins[2].Width = 0;
            }
        }

        public void SetLineNumbering(bool enabled)
        {
            if (enabled)
            {
                Margins[0].Width = 24;
                TextChanged += LineNumbers_TextChanged;
            }
            else
            {
                Margins[0].Width = 0;
                TextChanged -= LineNumbers_TextChanged;
            }
        }

        public void SetBraceMatching(bool enabled)
        {
            if (enabled)
            {
                UpdateUI += BraceMatching_UpdateUI;
            }
            else
            {
                UpdateUI -= BraceMatching_UpdateUI;
            }
        }

        public void SetAutoIndent(bool enabled)
        {
            if (enabled)
            {
                InsertCheck += AutoIndent_InsertCheck;
                CharAdded += AutoIndent_CharAdded;
            }
            else
            {
                InsertCheck -= AutoIndent_InsertCheck;
                CharAdded -= AutoIndent_CharAdded;
            }
        }
        #endregion

        #region Event Handlers
        private void LineNumbers_TextChanged(object sender, EventArgs e)
        {
            var maxLineNumberCharLength = Lines.Count.ToString().Length;

            //mark 以下程式碼，如果用 Ctrl+MouseWheel，按下 Enter 鍵後，才會有作用
            //if (maxLineNumberCharLength == _maxLineNumberCharLength)
            //{
            //    return;
            //}

            const int padding = 2;

            Margins[0].Width = TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
        }

        private void BraceMatching_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            if (_styler == null || !_styler.BraceMatching)
            {
                return;
            }

            var caretPos = CurrentPosition;

            if (_lastCaretPos == caretPos)
            {
                return;
            }

            _lastCaretPos = caretPos;

            var bracePos1 = -1;

            if (caretPos > 0 && _styler.IsBrace(GetCharAt(caretPos - 1)))
            {
                bracePos1 = (caretPos - 1);
            }
            else if (_styler.IsBrace(GetCharAt(caretPos)))
            {
                bracePos1 = caretPos;
            }

            if (bracePos1 >= 0)
            {
                var bracePos2 = BraceMatch(bracePos1);

                if (bracePos2 == InvalidPosition)
                {
                    BraceBadLight(bracePos1);
                }
                else
                {
                    BraceHighlight(bracePos1, bracePos2);
                }
            }
            else
            {
                BraceHighlight(InvalidPosition, InvalidPosition);
            }
        }

        private void AutoIndent_InsertCheck(object sender, InsertCheckEventArgs e)
        {
            if (_styler != null)
            {
                if (e.Text.EndsWith("\r") || e.Text.EndsWith("\n"))
                {
                    var startPos = Lines[LineFromPosition(CurrentPosition)].Position;
                    var endPos = e.Position;
                    var curLineText = GetTextRange(startPos, (endPos - startPos));
                    var indent = Regex.Match(curLineText, "^[ \\t]*");

                    e.Text = (e.Text + indent.Value);

                    if (Regex.IsMatch(curLineText, _styler.IndentChar + "\\s*$"))
                    {
                        e.Text = (e.Text + "    ");
                    }
                }
            }
        }

        private void AutoIndent_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (_styler != null)
            {
                if (_styler.IndentChar == e.Char)
                {
                    var curLine = LineFromPosition(CurrentPosition);

                    if (Lines[curLine].Text.Trim() == "}")
                    {
                        SetIndent(curLine, GetIndent(curLine) - 4);
                    }
                }
                else if (_styler.OutdentChar == e.Char)
                {
                    var curLine = LineFromPosition(CurrentPosition);

                    if (Lines[curLine].Text.Trim() == _styler.OutdentChar.ToString())
                    {
                        SetIndent(curLine, GetIndent(curLine) - 4);
                    }
                }
            }
        }
        #endregion

        #region "CodeIndent Handlers"
        const int SCI_SETLINEINDENTATION = 2126;
        const int SCI_GETLINEINDENTATION = 2127;

        private void SetIndent(int line, int indent)
        {
            DirectMessage(SCI_SETLINEINDENTATION, new IntPtr(line), new IntPtr(indent));
        }

        private int GetIndent(int line)
        {
            return (DirectMessage(SCI_GETLINEINDENTATION, new IntPtr(line), IntPtr.Zero).ToInt32());
        }
        #endregion
    }
}
