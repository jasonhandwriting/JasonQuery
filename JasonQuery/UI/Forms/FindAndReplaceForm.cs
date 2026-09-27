using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Editor.FindAndReplace;
using JasonQuery.UI.Helpers;
using ScintillaNET;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CharacterRange = JasonQuery.Editor.FindAndReplace.CharacterRange;

namespace JasonQuery.UI.Forms
{
    public partial class FindAndReplaceForm : Form
    {
        private bool _autoPosition;
        private bool _reloadLocalizationProcessed;
        private CharacterRange _searchRange;
        private Scintilla _scintilla;

        //20230705 Color
        private Color _colorRed;
        private Color _colorGreen;
        private Color _colorBlue;
        private Color _colorNormal;

        //20230628 語系訊息
        private string _sErrorRegularExpression;
        private string _sFindNoMatch;
        private string _sFindFirstMatchInFile;
        private string _sFindFirstMatchInSelection;
        private string _sCountQtyMatchInFile;
        private string _sCountQtyMatchInSelection;
        private string _sCountQtyMarkInFile;
        private string _sCountQtyMarkInSelection;
        private string _sReplaceNoMatch;
        private string _sReplaceAllQtyReplacedInFile;
        private string _sReplaceAllQtyReplacedInSelection;

        public event KeyPressedHandler KeyPressed;
        public delegate void KeyPressedHandler(object sender, KeyEventArgs e);

        private const int EM_SETCUEBANNER = 0x1501;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        #region Properties/// <summary>
        /// Gets or sets whether the dialog should automatically move away from the current
        /// selection to prevent obscuring it.
        /// </summary>
        /// <returns>true to automatically move away from the current selection; otherwise, false.</returns>
        public bool AutoPosition
        {
            get
            {
                return _autoPosition;
            }
            set
            {
                _autoPosition = value;
            }
        }

        public Scintilla Scintilla
        {
            get
            {
                return _scintilla;
            }
            set
            {
                _scintilla = value;
            }
        }

        public FindAndReplace FindAndReplace { get; set; }
        #endregion Properties

        public FindAndReplaceForm()
        {
            InitializeComponent();
            _autoPosition = true;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                ApplyLocalizationSetting();

                _colorGreen = MyLibrary.IsDarkMode ? Color.FromArgb(0, 255, 0) : Color.Green;
                _colorRed = MyLibrary.IsDarkMode ? Color.FromArgb(162, 12, 12) : Color.Red;
                _colorBlue = MyLibrary.IsDarkMode ? Color.Cyan : Color.Blue;
                _colorNormal = MyLibrary.IsDarkMode ? Color.White : Color.Blue;

                //20250523 加入按鈕圖示
                btnFindNext.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Replace 16x16.ico");
                btnFindNext.ImageAlign = ContentAlignment.MiddleLeft;
                btnFindNext.TextImageRelation = TextImageRelation.ImageBeforeText;
                btnFindPrevious.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Replace 16x16.ico");
                btnFindPrevious.ImageAlign = ContentAlignment.MiddleLeft;
                btnFindPrevious.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SearchMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoStandard.Checked)
            {
                pnlStandardOptions.BringToFront();
            }
            else
            {
                pnlRegexpOptions.BringToFront();
            }
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorNormal;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            UpdateFindList();

            CharacterRange foundRange;

            try
            {
                foundRange = FindNext(false);
            }
            catch (ArgumentException ex)
            {
                lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                lblStatus.ForeColor = _colorRed;
                return;
            }

            if (foundRange.cpMin == foundRange.cpMax)
            {
                lblStatus.Text = _sFindNoMatch.Replace("{text}", cboFind.Text);
                lblStatus.ForeColor = _colorRed;
            }
            else
            {
                if (foundRange.cpMin < Scintilla.AnchorPosition)
                {
                    if (chkInSelection.Checked)
                    {
                        lblStatus.Text = _sFindFirstMatchInSelection; //Search match wrapped to the beginning of the selection
                    }
                    else
                    {
                        lblStatus.Text = _sFindFirstMatchInFile; //Search match wrapped to the beginning of the document
                    }
                }

                lblStatus.ForeColor = _colorGreen;
                Scintilla.SetSel(foundRange.cpMin, foundRange.cpMax);
                MoveFormAwayFromSelection();
            }
        }

        private void btnFindPrevious_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorNormal;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            UpdateFindList();
            CharacterRange foundRange;

            try
            {
                foundRange = FindNext(true);
            }
            catch (ArgumentException ex)
            {
                lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                lblStatus.ForeColor = _colorRed;
                return;
            }

            if (foundRange.cpMin == foundRange.cpMax)
            {
                lblStatus.Text = _sFindNoMatch.Replace("{text}", cboFind.Text);
                lblStatus.ForeColor = _colorRed;
            }
            else
            {
                if (foundRange.cpMin > Scintilla.CurrentPosition)
                {
                    if (chkInSelection.Checked)
                    {
                        lblStatus.Text = _sFindFirstMatchInSelection; //Search match wrapped to the end of the selection
                    }
                    else
                    {
                        lblStatus.Text = _sFindFirstMatchInFile; //Search match wrapped to the end of the document
                    }
                }

                lblStatus.ForeColor = _colorGreen;
                Scintilla.SetSel(foundRange.cpMin, foundRange.cpMax);
                MoveFormAwayFromSelection();
            }
        }

        public CharacterRange FindNext(bool searchUp)
        {
            CharacterRange foundRange;

            if (rdoRegularExpression.Checked)
            {
                var rr = new Regex(cboFind.Text, GetRegexOptions());

                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    if (searchUp)
                    {
                        foundRange = FindAndReplace.FindPrevious(rr, chkWrapAround.Checked, _searchRange);
                    }
                    else
                    {
                        foundRange = FindAndReplace.FindNext(rr, chkWrapAround.Checked, _searchRange);
                    }
                }
                else
                {
                    _searchRange = new CharacterRange();

                    if (searchUp)
                    {
                        foundRange = FindAndReplace.FindPrevious(rr, chkWrapAround.Checked);
                    }
                    else
                    {
                        foundRange = FindAndReplace.FindNext(rr, chkWrapAround.Checked);
                    }
                }
            }
            else
            {
                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    if (searchUp)
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindPrevious(textToFind, chkWrapAround.Checked, GetSearchFlags(), _searchRange);
                    }
                    else
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindNext(textToFind, chkWrapAround.Checked, GetSearchFlags(), _searchRange);
                    }
                }
                else
                {
                    _searchRange = new CharacterRange();

                    if (searchUp)
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindPrevious(textToFind, chkWrapAround.Checked, GetSearchFlags());
                    }
                    else
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindNext(textToFind, chkWrapAround.Checked, GetSearchFlags());
                    }
                }
            }

            return foundRange;
        }

        public virtual void MoveFormAwayFromSelection()
        {
            if (!Visible)
            {
                return;
            }

            if (!AutoPosition)
            {
                return;
            }

            int pos = Scintilla.CurrentPosition;
            int x = Scintilla.PointXFromPosition(pos);
            int y = Scintilla.PointYFromPosition(pos);
            var cursorPoint = Scintilla.PointToScreen(new Point(x, y));
            var r = new Rectangle(Location, Size);

            if (r.Contains(cursorPoint))
            {
                Point newLocation;

                if (cursorPoint.Y < (Screen.PrimaryScreen.Bounds.Height / 2))
                {
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = Scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();

                    newLocation = Scintilla.PointToClient(new Point(Location.X, cursorPoint.Y + lineHeight * 2));
                }
                else
                {
                    int SCI_TEXTHEIGHT = 2279;
                    int lineHeight = Scintilla.DirectMessage(SCI_TEXTHEIGHT, IntPtr.Zero, IntPtr.Zero).ToInt32();

                    newLocation = Scintilla.PointToClient(new Point(Location.X, cursorPoint.Y - Height - (lineHeight * 2)));
                }

                newLocation = Scintilla.PointToScreen(newLocation);
                Location = newLocation;
            }
        }

        private CharacterRange FindNext(bool searchUp, ref Regex rr)
        {
            CharacterRange foundRange;

            if (rdoRegularExpression.Checked)
            {
                if (rr == null)
                {
                    rr = new Regex(cboFind.Text, GetRegexOptions());
                }

                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    if (searchUp)
                    {
                        foundRange = FindAndReplace.FindPrevious(rr, chkWrapAround.Checked, _searchRange);
                    }
                    else
                    {
                        foundRange = FindAndReplace.FindNext(rr, chkWrapAround.Checked, _searchRange);
                    }
                }
                else
                {
                    _searchRange = new CharacterRange();

                    if (searchUp)
                    {
                        foundRange = FindAndReplace.FindPrevious(rr, chkWrapAround.Checked);
                    }
                    else
                    {
                        foundRange = FindAndReplace.FindNext(rr, chkWrapAround.Checked);
                    }
                }
            }
            else
            {
                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    if (searchUp)
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindPrevious(textToFind, chkWrapAround.Checked, GetSearchFlags(), _searchRange);
                    }
                    else
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindNext(textToFind, chkWrapAround.Checked, GetSearchFlags(), _searchRange);
                    }
                }
                else
                {
                    _searchRange = new CharacterRange();

                    if (searchUp)
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindPrevious(textToFind, chkWrapAround.Checked, GetSearchFlags());
                    }
                    else
                    {
                        var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                        foundRange = FindAndReplace.FindNext(textToFind, chkWrapAround.Checked, GetSearchFlags());
                    }
                }
            }

            return foundRange;
        }

        public RegexOptions GetRegexOptions()
        {
            RegexOptions ro = RegexOptions.None;

            if (chkCompiled.Checked)
            {
                ro |= RegexOptions.Compiled;
            }

            if (chkCultureInvariant.Checked)
            {
                ro |= RegexOptions.Compiled;
            }

            if (chkEcmaScript.Checked)
            {
                ro |= RegexOptions.ECMAScript;
            }

            if (chkExplicitCapture.Checked)
            {
                ro |= RegexOptions.ExplicitCapture;
            }

            if (chkIgnoreCase.Checked)
            {
                ro |= RegexOptions.IgnoreCase;
            }

            if (chkIgnorePatternWhitespace.Checked)
            {
                ro |= RegexOptions.IgnorePatternWhitespace;
            }

            if (chkMultiline.Checked)
            {
                ro |= RegexOptions.Multiline;
            }

            if (chkRightToLeft.Checked)
            {
                ro |= RegexOptions.RightToLeft;
            }

            if (chkSingleLine.Checked)
            {
                ro |= RegexOptions.Singleline;
            }

            return ro;
        }

        public SearchFlags GetSearchFlags()
        {
            var sf = SearchFlags.None;

            if (chkMatchCase.Checked)
            {
                sf |= SearchFlags.MatchCase;
            }

            if (chkWholeWord.Checked)
            {
                sf |= SearchFlags.WholeWord;
            }

            if (chkWordStart.Checked)
            {
                sf |= SearchFlags.WordStart;
            }

            return sf;
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorBlue;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            UpdateFindList();
            UpdateReplaceList();

            var foundCount = 0;

            #region RegEx
            if (rdoRegularExpression.Checked)
            {
                Regex rr = null;

                try
                {
                    rr = new Regex(cboFind.Text, GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                    lblStatus.ForeColor = _colorRed;
                    return;
                }

                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    foundCount = FindAndReplace.ReplaceAll(_searchRange, rr, cboReplace.Text, false, false);
                }
                else
                {
                    _searchRange = new CharacterRange();
                    foundCount = FindAndReplace.ReplaceAll(rr, cboReplace.Text, false, false);
                }
            }
            #endregion

            #region Non-RegEx
            if (!rdoRegularExpression.Checked)
            {
                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;
                    var textToReplace = rdoExtended.Checked ? FindAndReplace.Transform(cboReplace.Text) : cboReplace.Text;

                    foundCount = FindAndReplace.ReplaceAll(_searchRange, textToFind, textToReplace, GetSearchFlags(), false, false);
                }
                else
                {
                    _searchRange = new CharacterRange();

                    var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;
                    var textToReplace = rdoExtended.Checked ? FindAndReplace.Transform(cboReplace.Text) : cboReplace.Text;

                    foundCount = FindAndReplace.ReplaceAll(textToFind, textToReplace, GetSearchFlags(), false, false);
                }
            }
            #endregion

            if (chkInSelection.Checked)
            {
                lblStatus.Text = _sReplaceAllQtyReplacedInSelection.Replace("{qty}", foundCount.ToString());
            }
            else
            {
                lblStatus.Text = _sReplaceAllQtyReplacedInFile.Replace("{qty}", foundCount.ToString());
            }

            lblStatus.ForeColor = foundCount == 0 ? _colorRed : _colorBlue;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void btnClearHighlights_Click(object sender, EventArgs e)
        {
            FindAndReplace.ClearAllHighlights();
            lblStatus.Text = string.Empty;
        }

        private void chkEcmaScript_CheckedChanged(object sender, EventArgs e)
        {
            if (((C1.Win.C1Input.C1CheckBox)sender).Checked)
            {
                chkExplicitCapture.Checked = false;
                chkExplicitCapture.Enabled = false;
                chkIgnorePatternWhitespace.Checked = false;
                chkIgnorePatternWhitespace.Enabled = false;
                chkRightToLeft.Checked = false;
                chkRightToLeft.Enabled = false;
                chkSingleLine.Checked = false;
            }
            else
            {
                chkExplicitCapture.Enabled = true;
                chkIgnorePatternWhitespace.Enabled = true;
                chkRightToLeft.Enabled = true;
                chkSingleLine.Enabled = true;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!string.IsNullOrEmpty(Scintilla.SelectedText))
            {
                chkInSelection.Enabled = true;
            }
            else
            {
                chkInSelection.Enabled = false;
                chkInSelection.Checked = false;
            }

            //var bValue = Scintilla.CanPaste;
            //btnReplaceAll.Enabled = bValue;
            //btnReplaceNext.Enabled = bValue;
            //btnReplacePrevious.Enabled = bValue;

            _searchRange = new CharacterRange();

            MoveFormAwayFromSelection();

            lblStatus.Text = string.Empty;
            base.OnActivated(e);

            //20230703 檢查目前具有焦點的控制項是否為按鈕
            //if (ActiveControl is ToolStripButton clickedButton)
            //{
            //    //在這裡處理按鈕被按下的邏輯
            //    MessageBox.Show($"Button {clickedButton.Name} was clicked!");
            //}
        }

        private void btnReplaceNext_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorNormal;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            UpdateFindList();
            UpdateReplaceList();

            CharacterRange nextRange;

            try
            {
                nextRange = ReplaceNext(false);
            }
            catch (ArgumentException ex)
            {
                lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                lblStatus.ForeColor = _colorRed;
                return;
            }

            if (nextRange.cpMin == nextRange.cpMax)
            {
                lblStatus.Text = _sFindNoMatch.Replace("{text}", cboFind.Text);
                lblStatus.ForeColor = _colorRed;
            }
            else
            {
                if (nextRange.cpMin < Scintilla.AnchorPosition)
                {
                    if (chkInSelection.Checked)
                    {
                        lblStatus.Text = _sFindFirstMatchInSelection; //Search match wrapped to the beginning of the selection
                    }
                    else
                    {
                        lblStatus.Text = _sFindFirstMatchInFile; //Search match wrapped to the beginning of the document
                    }
                }

                lblStatus.ForeColor = _colorGreen;
                Scintilla.SetSel(nextRange.cpMin, nextRange.cpMax);
                MoveFormAwayFromSelection();
            }
        }

        private CharacterRange ReplaceNext(bool searchUp)
        {
            Regex rr = null;
            var selRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);

            if (selRange.cpMax - selRange.cpMin > 0)
            {
                if (rdoRegularExpression.Checked)
                {
                    rr = new Regex(cboFind.Text, GetRegexOptions());

                    var selRangeText = Scintilla.GetTextRange(selRange.cpMin, selRange.cpMax - selRange.cpMin + 1);

                    if (selRange.Equals(FindAndReplace.Find(selRange, rr, false)))
                    {
                        if (searchUp)
                        {
                            _scintilla.SelectionStart = selRange.cpMin;
                            _scintilla.SelectionEnd = selRange.cpMax;
                            _scintilla.ReplaceSelection(rr.Replace(selRangeText, cboReplace.Text));
                            _scintilla.GotoPosition(selRange.cpMin);
                        }
                        else
                        {
                            Scintilla.ReplaceSelection(rr.Replace(selRangeText, cboReplace.Text));
                        }
                    }
                }
                else
                {
                    var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                    if (selRange.Equals(FindAndReplace.Find(selRange, textToFind, false)))
                    {
                        if (searchUp)
                        {
                            var textToReplace = rdoExtended.Checked ? FindAndReplace.Transform(cboReplace.Text) : cboReplace.Text;

                            _scintilla.SelectionStart = selRange.cpMin;
                            _scintilla.SelectionEnd = selRange.cpMax;
                            _scintilla.ReplaceSelection(textToReplace);

                            _scintilla.GotoPosition(selRange.cpMin);
                        }
                        else
                        {
                            var textToReplace = rdoExtended.Checked ? FindAndReplace.Transform(cboReplace.Text) : cboReplace.Text;

                            Scintilla.ReplaceSelection(textToReplace);
                        }
                    }
                }
            }

            return FindNext(searchUp, ref rr);
        }

        private void btnReplacePrevious_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorNormal;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            UpdateFindList();
            UpdateReplaceList();

            CharacterRange nextRange;

            try
            {
                nextRange = ReplaceNext(true);
            }
            catch (ArgumentException ex)
            {
                lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                lblStatus.ForeColor = _colorRed;
                return;
            }

            if (nextRange.cpMin == nextRange.cpMax)
            {
                lblStatus.Text = _sFindNoMatch.Replace("{text}", cboFind.Text);
                lblStatus.ForeColor = _colorRed;
            }
            else
            {
                if (nextRange.cpMin > _scintilla.AnchorPosition)
                {
                    if (chkInSelection.Checked)
                    {
                        lblStatus.Text = _sFindFirstMatchInSelection; //Search match wrapped to the beginning of the selection
                    }
                    else
                    {
                        lblStatus.Text = _sFindFirstMatchInFile; //Search match wrapped to the beginning of the document
                    }
                }

                lblStatus.ForeColor = _colorGreen;
                _scintilla.SetSel(nextRange.cpMin, nextRange.cpMax);
                MoveFormAwayFromSelection();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnHighlightAll_Click(object sender, EventArgs e)
        {
            CountOrHighlight(false, true, true);
        }

        private void btnCount_Click(object sender, EventArgs e)
        {
            CountOrHighlight(false, false, false);
        }

        private void CountOrHighlight(bool isMarkLine, bool isHighlight, bool isHighlight2)
        {
            lblStatus.Text = string.Empty;
            lblStatus.ForeColor = _colorNormal;

            var textInFile = isHighlight2 ? _sCountQtyMarkInFile : _sCountQtyMatchInFile;
            var textInSelection = isHighlight2 ? _sCountQtyMarkInSelection : _sCountQtyMatchInSelection;

            if (string.IsNullOrEmpty(cboFind.Text))
            {
                return;
            }

            btnClearHighlights_Click(null, null);

            var foundCount = 0;

            #region RegEx
            if (rdoRegularExpression.Checked)
            {
                Regex rr = null;

                try
                {
                    rr = new Regex(cboFind.Text, GetRegexOptions());
                }
                catch (ArgumentException ex)
                {
                    lblStatus.Text = $"{_sErrorRegularExpression}{ex.Message}";
                    lblStatus.ForeColor = _colorRed;
                    return;
                }

                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    foundCount = FindAndReplace.FindAll(_searchRange, rr, isMarkLine, isHighlight).Count;
                }
                else
                {
                    _searchRange = new CharacterRange();
                    foundCount = FindAndReplace.FindAll(rr, isMarkLine, isHighlight).Count;
                }
            }
            #endregion

            #region Non-RegEx
            if (!rdoRegularExpression.Checked)
            {
                if (chkInSelection.Checked)
                {
                    if (_searchRange.cpMin == _searchRange.cpMax)
                    {
                        _searchRange = new CharacterRange(_scintilla.Selections[0].Start, _scintilla.Selections[0].End);
                    }

                    var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                    foundCount = FindAndReplace.FindAll(_searchRange, textToFind, GetSearchFlags(), isMarkLine, isHighlight).Count;
                }
                else
                {
                    _searchRange = new CharacterRange();

                    var textToFind = rdoExtended.Checked ? FindAndReplace.Transform(cboFind.Text) : cboFind.Text;

                    foundCount = FindAndReplace.FindAll(textToFind, GetSearchFlags(), isMarkLine, isHighlight).Count;
                }
            }
            #endregion

            if (chkInSelection.Checked)
            {
                lblStatus.Text = textInSelection.Replace("{qty}", foundCount.ToString());
            }
            else
            {
                lblStatus.Text = textInFile.Replace("{qty}", foundCount.ToString());
            }

            lblStatus.ForeColor = _colorBlue;
        }

        private void btnHelp_Options_Click(object sender, EventArgs e)
        {
            var objectName = string.Empty;
            var buttonTemp = sender as C1.Win.C1Input.C1Button;
            var button = buttonTemp.Name.Replace("btnHelp_", string.Empty);
            var message = LocalizationHelper.GetLanguageString(string.Empty, "form", GetType().Name, "object", $"chk{button}", "ToolTipText");

            foreach (Control obj in pnlStandardOptions.Controls)
            {
                var typeName = obj.GetType().Name;
                var name = obj.Name;

                if (typeName != "C1CheckBox")
                {
                    continue;
                }

                if (name == $"chk{button}")
                {
                    objectName = obj.Text;
                    break;
                }
            }

            if (string.IsNullOrEmpty(objectName))
            {
                foreach (Control obj in pnlRegexpOptions.Controls)
                {
                    var typeName = obj.GetType().Name;
                    var name = obj.Name;

                    if (typeName != "C1CheckBox")
                    {
                        continue;
                    }

                    if (name == $"chk{button}")
                    {
                        objectName = obj.Text;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(message))
            {
                message = objectName;
            }

            var text = $"{Text} - {objectName}";

            MessageBoxHelper.ShowNearCursor(message, text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cboFind_BeforeDropDownOpen(object sender, CancelEventArgs e)
        {
            LoadFindList("Editor");
        }

        private void cboReplace_BeforeDropDownOpen(object sender, CancelEventArgs e)
        {
            UpdateReplaceList();
        }

        private void cboReplace_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 || string.IsNullOrWhiteSpace(cboReplace.Text))
            {
                return;
            }

            btnReplaceNext.Focus();
        }

        private void UpdateFindList() //判斷是否要更新「搜尋清單」
        {
            if (TextHelper.GetSafeString(cboFind.Tag) == cboFind.Text)
            {
                return;
            }

            if (cboFind.Items.Count > 0 && cboFind.Text == cboFind.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Editor", cboFind.Text); //UpdateFindList
            }

            cboFind.Tag = cboFind.Text;
        }

        private void UpdateReplaceList() //判斷是否要更新「搜尋清單」
        {
            if (TextHelper.GetSafeString(cboReplace.Tag) == cboReplace.Text)
            {
                return;
            }

            if (cboReplace.Items.Count > 0 && cboReplace.Text == cboReplace.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveReplaceList("Editor", cboReplace.Text); //UpdateFindList
            }

            cboReplace.Tag = cboFind.Text;
        }

        private void LoadFindList(string function)
        {
            var i = 0;

            cboFind.Items.Clear();

            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = 'FindList_{function}'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtRecent = JasonQueryRepository.ExecQuery(sql);

                if (dtRecent.Rows.Count <= 0)
                {
                    return;
                }

                foreach (DataRow dr in dtRecent?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    if (i > 20)
                    {
                        break;
                    }

                    var recent = dr.GetSafeString("AttributeValue");

                    cboFind.Items.Add(recent);

                    i++;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SaveFindList(string function, string findText)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey = 'FindList_{function}'");
            sbSql.Append($"   AND AttributeValue = '{findText.Replace("'", "''")}'");

            var sql = sbSql.ToString();
            var dtRecent = JasonQueryRepository.ExecQuery(sql);

            if (dtRecent?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = 'FindList_{function}'");
                sbSql.Append($"   AND AttributeValue = '{findText.Replace("'", "''")}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("        (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                sbSql.Append($" VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'FindList_{function}', '{findText.Replace("'", "''")}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }

            //Reload
            LoadFindList(function); //SaveFindList
        }

        private void LoadReplaceList()
        {
            var i = 0;

            cboReplace.Items.Clear();

            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'ReplaceList_Editor'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtReplace = JasonQueryRepository.ExecQuery(sql);

                if (dtReplace.Rows.Count <= 0)
                {
                    return;
                }

                foreach (DataRow dr in dtReplace?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    if (i > 20)
                    {
                        break;
                    }

                    var replace = dr.GetSafeString("AttributeValue");

                    cboReplace.Items.Add(replace);

                    i++;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SaveReplaceList(string function, string replaceText)
        {
            var sql = string.Empty;
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey = 'ReplaceList_{function}'");
            sbSql.Append($"   AND AttributeValue = '{replaceText.Replace("'", "''")}'");

            sql = sbSql.ToString();

            var dtRecent = JasonQueryRepository.ExecQuery(sql);

            if (dtRecent?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = 'ReplaceList_{function}'");
                sbSql.Append($"   AND AttributeValue = '{replaceText.Replace("'", "''")}'");
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("        (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                sbSql.Append($" VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'ReplaceList_{function}', '{replaceText.Replace("'", "''")}', '{MyGlobal.DateTimeNow()}')");
            }

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            //Reload
            LoadReplaceList(); //SaveReplaceList
        }

        private void btnExchange_Click(object sender, EventArgs e)
        {
            var temp = cboFind.Text;

            cboFind.Text = cboReplace.Text;
            cboReplace.Text = temp;
        }

        //從母表單傳遞資訊至指定的子表單
        private void timerMother2Child_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp5) && MyGlobal.GlobalTemp5.StartsWith("CanPaste", StringComparison.Ordinal)) //判斷是否允許 Replace 動作
            {
                var enabled = MyGlobal.GlobalTemp5 == "CanPasteY";

                btnExchange.Enabled = enabled;
                btnReplaceAll.Enabled = enabled;
                btnReplaceNext.Enabled = enabled;
                btnReplacePrevious.Enabled = enabled;
                lblReplaceWith.Enabled = enabled;
                cboReplace.Enabled = enabled;

                if (enabled && lblReplaceWith.Tag != null)
                {
                    SendMessage(cboReplace.Handle, EM_SETCUEBANNER, 0, TextHelper.GetSafeString(lblReplaceWith.Tag));
                }

                MyGlobal.GlobalTemp5 = string.Empty;
                return;
            }

            //是否為 Reload Localization 套用？
            if (string.IsNullOrEmpty(MyGlobal.InfoFromReloadLocalization) || !MyGlobal.InfoFromReloadLocalization.StartsWith("ReloadLocalization`", StringComparison.Ordinal))
            {
                _reloadLocalizationProcessed = false;
                return;
            }

            if (_reloadLocalizationProcessed)
            {
                return;
            }

            _reloadLocalizationProcessed = true;

            //20240224
            if (!string.IsNullOrEmpty(AccessibleDescription))
            {
                MyGlobal.InfoFromReloadLocalization = MyGlobal.InfoFromReloadLocalization.Replace($"{AccessibleDescription};", string.Empty);
            }

            if (MyGlobal.InfoFromReloadLocalization == "ReloadLocalization`")
            {
                MyGlobal.InfoFromReloadLocalization = string.Empty;
            }

            ApplyLocalizationSetting();
        }

        private void ApplyLocalizationSetting()
        {
            LocalizationHelper.ApplyLanguageInfo(this, false);

            var a = Assembly.GetExecutingAssembly(); //20250518 pnlStatus.BackgroundImage 維持原寫法 (新寫法有 bug，會出現記憶體不足的嚴重錯誤)

            if (MyLibrary.IsDarkMode)
            {
                C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";
                c1ThemeController1.SetTheme(c1StatusBar1, "ExpressionDark");
                pnlStatus.BackgroundImage = new Bitmap(a.GetManifestResourceStream($"{a.GetName().Name}.Image.Panel_C1Status_Dark.png"));
                btnCount.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Count 24x24 Dark.ico");
            }
            else
            {
                pnlStatus.BackgroundImage = new Bitmap(a.GetManifestResourceStream($"{a.GetName().Name}.Image.Panel_C1Status.png"));
                btnCount.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Count 24x24 Dark.ico");
            }

            var languageText = LocalizationHelper.GetLanguageString("Find...", "form", GetType().Name, "object", "cboFind", "ToolTipText");

            SendMessage(cboFind.Handle, EM_SETCUEBANNER, 0, languageText);
            languageText = LocalizationHelper.GetLanguageString("Replace...", "form", GetType().Name, "object", "cboReplace", "ToolTipText");
            SendMessage(cboReplace.Handle, EM_SETCUEBANNER, 0, languageText);
            lblReplaceWith.Tag = languageText;

            languageText = LocalizationHelper.GetLanguageString("Count", "form", GetType().Name, "object", "btnCount", "ToolTipText");
            toolTip1.SetToolTip(btnCount, languageText);
            languageText = LocalizationHelper.GetLanguageString("Highlight All", "form", GetType().Name, "object", "btnHighlightAll", "ToolTipText");
            toolTip1.SetToolTip(btnHighlightAll, languageText);
            languageText = LocalizationHelper.GetLanguageString("Clear Highlights", "form", GetType().Name, "object", "btnClearHighlights", "ToolTipText");
            toolTip1.SetToolTip(btnClearHighlights, languageText);
            languageText = LocalizationHelper.GetLanguageString("Exchange", "form", GetType().Name, "object", "btnExchange", "ToolTipText");
            toolTip1.SetToolTip(btnExchange, languageText);

            //載入搜尋記錄
            LoadFindList("Editor");
            LoadReplaceList();

            cboFind.Tag = string.Empty;
            cboReplace.Tag = string.Empty;

            var width = -2;

            btnHelp_MatchCase.Location = new Point(chkMatchCase.Left + chkMatchCase.Width + width, btnHelp_MatchCase.Top);
            btnHelp_WholeWord.Location = new Point(chkWholeWord.Left + chkWholeWord.Width + width, btnHelp_WholeWord.Top);
            btnHelp_WordStart.Location = new Point(chkWordStart.Left + chkWordStart.Width + width, btnHelp_WordStart.Top);
            btnHelp_Compiled.Location = new Point(chkCompiled.Left + chkCompiled.Width + width, btnHelp_Compiled.Top);
            btnHelp_CultureInvariant.Location = new Point(chkCultureInvariant.Left + chkCultureInvariant.Width + width, btnHelp_CultureInvariant.Top);
            btnHelp_EcmaScript.Location = new Point(chkEcmaScript.Left + chkEcmaScript.Width + width, btnHelp_EcmaScript.Top);
            btnHelp_ExplicitCapture.Location = new Point(chkExplicitCapture.Left + chkExplicitCapture.Width + width, btnHelp_ExplicitCapture.Top);
            btnHelp_IgnoreCase.Location = new Point(chkIgnoreCase.Left + chkIgnoreCase.Width + width, btnHelp_IgnoreCase.Top);
            btnHelp_IgnorePatternWhitespace.Location = new Point(chkIgnorePatternWhitespace.Left + chkIgnorePatternWhitespace.Width + width, btnHelp_IgnorePatternWhitespace.Top);
            btnHelp_Multiline.Location = new Point(chkMultiline.Left + chkMultiline.Width + width, btnHelp_Multiline.Top);
            btnHelp_RightToLeft.Location = new Point(chkRightToLeft.Left + chkRightToLeft.Width + width, btnHelp_RightToLeft.Top);
            btnHelp_SingleLine.Location = new Point(chkSingleLine.Left + chkSingleLine.Width + width, btnHelp_SingleLine.Top);

            _sErrorRegularExpression = LocalizationHelper.GetLanguageString("Error in Regular Expression: ", "form", GetType().Name, "msg", "ErrorRegularExpression", "Text");
            _sFindNoMatch = LocalizationHelper.GetLanguageString("Find: Can't find the text \"{text}\"", "form", GetType().Name, "msg", "FindNoMatch", "Text");
            _sFindFirstMatchInFile = LocalizationHelper.GetLanguageString("Find: Found the 1st occurrence from the top. The end of the document has been reached in entire file", "form", GetType().Name, "msg", "FindFirstMatchInFile", "Text");
            _sFindFirstMatchInSelection = LocalizationHelper.GetLanguageString("Find: Found the 1st occurrence from the top. The end of the document has been reached in selection", "form", GetType().Name, "msg", "FindFirstMatchInSelection", "Text");
            _sCountQtyMatchInFile = LocalizationHelper.GetLanguageString("Count: {qty} matches in entire file", "form", GetType().Name, "msg", "CountQtyMatchInFile", "Text");
            _sCountQtyMatchInSelection = LocalizationHelper.GetLanguageString("Count: {qty} matches in selection", "form", GetType().Name, "msg", "CountQtyMatchInSelection", "Text");
            _sCountQtyMarkInFile = LocalizationHelper.GetLanguageString("Mark: {qty} matches in entire file", "form", GetType().Name, "msg", "CountQtyMarkInFile", "Text");
            _sCountQtyMarkInSelection = LocalizationHelper.GetLanguageString("Mark: {qty} matches in selection", "form", GetType().Name, "msg", "CountQtyMarkInSelection", "Text");
            _sReplaceNoMatch = LocalizationHelper.GetLanguageString("Replace: no occurrence was found", "form", GetType().Name, "msg", "ReplaceNoMatch", "Text");
            _sReplaceAllQtyReplacedInFile = LocalizationHelper.GetLanguageString("Replace All: {qty} occurrence(s) were replaced in entire file", "form", GetType().Name, "msg", "ReplaceAllQtyReplacedInFile", "Text");
            _sReplaceAllQtyReplacedInSelection = LocalizationHelper.GetLanguageString("Replace All: {qty} occurrence(s) were replaced in selection", "form", GetType().Name, "msg", "ReplaceAllQtyReplacedInSelection", "Text");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
