using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class CellViewerForm : Form
    {
        private ContextMenuStrip _editorMenu = new ContextMenuStrip();
        private string _selectedTextDoubleClick = string.Empty; //20250709 記住 Double Click 的字串
        private Func<byte[]> _binaryContentLoader;
        private byte[] _binaryContent;

        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string CellText { get; set; }
        public bool HasBlobContent { get; set; } //是否包含 BLOB 內容？
        public bool IsBlobColumn { get; set; } //是否為 BLOB 欄位？
        public bool IsFromSingleRecordForm { get; set; }
        public byte[] Blob { get; set; }
        public int TotalQty { get; set; }
        public int CurrentRow { get; set; }
        public CategoryDataTypeKind CategoryDataTypeKindValue { get; set; } = CategoryDataTypeKind.String;
        public Func<int, CellViewerCellContext> CellValueLoader { get; set; }
        public Action<int> CurrentRowChanged { get; set; }

        public void ApplyContext(CellViewerCellContext context)
        {
            if (context == null)
            {
                return;
            }

            ColumnName = context.ColumnName;
            ColumnType = context.ColumnType;
            CellText = context.DisplayText;

            UpdateEditorText(context.DisplayText);

            HasBlobContent = context.HasBinaryContent;
            IsBlobColumn = context.IsBinaryColumn;
            CategoryDataTypeKindValue = context.CategoryDataTypeKindValue;

            _binaryContentLoader = context.BinaryContentLoader;
            _binaryContent = context.BinaryContent;

            //保留 Blob 是為了降低這一波修改範圍；後續可以再把 Blob property 退場
            Blob = _binaryContent;

            lblColumnName2.Text = ColumnName;
            lblColumnType2.Text = ColumnType;

            btnViewDataAsHex.Enabled = IsBlobColumn && HasBlobContent && (_binaryContent != null || _binaryContentLoader != null);
        }

        public CellViewerForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

                Tag = Text;

                if (MyLibrary.IsDarkMode)
                {
                    C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";

                    ApplySqlStyler();

                    lblColumnName2.ForeColor = Color.Yellow;
                    lblColumnType2.ForeColor = Color.Yellow;
                }
                else
                {
                    lblColumnName2.ForeColor = Color.Blue;
                    lblColumnType2.ForeColor = Color.Blue;
                }

                lblColumnName2.Text = ColumnName;

                if (!string.IsNullOrEmpty(ColumnType))
                {
                    lblColumnType2.Text = ColumnType;

                    var columnTypeSpace = lblColumnType.Text.EndsWith(":", StringComparison.Ordinal) ? " " : string.Empty;

                    //20230912 表單標題顯示「欄位型態」
                    Text += $" - {lblColumnType.Text}{columnTypeSpace}{lblColumnType2.Text}";
                }

                lblColumnName.Visible = true;
                lblColumnName2.Visible = true;
                lblSpaceColumnName.Visible = false;
                lblColumnType.Visible = false;
                lblColumnType2.Visible = false;
                lblSpaceColumnType.Visible = false;
                toolStripSeparator1.Visible = true;

                editor.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                editor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editor.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

                UpdateEditorText(CellText ?? string.Empty);
                btnViewDataAsHex.Enabled = IsBlobColumn && HasBlobContent && (_binaryContent != null || _binaryContentLoader != null || Blob != null);

                var languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");

                _editorMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.A;

                _editorMenu.Items[0].Click += delegate
                {
                    editor.SelectionStart = 0;
                    editor.SelectionEnd = editor.Text.Length;
                };

                _editorMenu.Items.Add("-");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
                _editorMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_editorMenu.Items[2]).ShortcutKeys = Keys.Control | Keys.C;

                _editorMenu.Items[2].Click += delegate
                {
                    CopyCellViewer();
                };

                if (MyLibrary.WordWrap)
                {
                    btnWordWrap.PerformClick();
                }
                else
                {
                    btnWordWrap2.PerformClick();
                }

                switch (MyLibrary.WordWrapIndentMode)
                {
                    case "Fixed":
                        {
                            editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Fixed;
                            break;
                        }
                    case "Indent":
                        {
                            editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Indent;
                            break;
                        }
                    default:
                        {
                            editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
                            break;
                        }
                }

                if (MyLibrary.ShowAllCharacters)
                {
                    editor.ViewEol = true;
                    editor.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
                    btnShowAllCharacters.Visible = false;
                    btnShowAllCharacters2.Visible = true;
                }
                else
                {
                    editor.ViewEol = false;
                    editor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
                    btnShowAllCharacters.Visible = true;
                    btnShowAllCharacters2.Visible = false;
                }

                CheckButtonStatus(); //決定按鈕狀態

                if (IsFromSingleRecordForm)
                {
                    lblColumnName.Visible = false;
                    lblColumnName2.Visible = false;
                    toolStripSeparator1.Visible = false;
                    btnFirst.Visible = false;
                    btnPrevious.Visible = false;
                    btnNext.Visible = false;
                    btnLast.Visible = false;
                    toolStripSeparator2.Visible = false;
                }

                tsTool.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "CellViewerFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "CellViewerFormHeight", Size.Height.ToString());
        }

        private void ApplySqlStyler()
        {
            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorNumber = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorString = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorCharacter = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorTextIdentifier;
            SqlStyler.IsKeywordFontBold = false;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.ColorTextIdentifier;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.ColorTextIdentifier;

            editor.Styler = new SqlStyler();
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            if (editor.WrapMode == ScintillaNET.WrapMode.Word)
            {
                btnWordWrap.Visible = true;
                btnWordWrap2.Visible = false;
                editor.WrapMode = ScintillaNET.WrapMode.None;
            }
            else
            {
                btnWordWrap.Visible = false;
                btnWordWrap2.Visible = true;
                editor.WrapMode = ScintillaNET.WrapMode.Word;
                editor.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editor.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            if (editor.ViewEol)
            {
                btnShowAllCharacters.Visible = true;
                btnShowAllCharacters2.Visible = false;
                editor.ViewEol = false;
                editor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            }
            else
            {
                btnShowAllCharacters.Visible = false;
                btnShowAllCharacters2.Visible = true;
                editor.ViewEol = true;
                editor.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            }
        }

        private void editor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _editorMenu.Items[0].Enabled = !string.IsNullOrEmpty(editor.Text);

            //Copy
            _editorMenu.Items[2].Enabled = !string.IsNullOrEmpty(editor.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            editor.ContextMenuStrip = _editorMenu;

            if (MyLibrary.IsDarkMode)
            {
                _editorMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _editorMenu.ForeColor = Color.White;
                _editorMenu.RenderMode = ToolStripRenderMode.System;
            }

            _editorMenu.Show(editor, new Point(e.X, e.Y));
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopyCellViewer();
        }

        private void btnViewDataAsHex_Click(object sender, EventArgs e)
        {
            ViewDataAsHex(true);
        }

        private void ViewDataAsHex(bool isViewMode = false)
        {
            try
            {
                if (!EnsureBinaryContentLoaded())
                {
                    return;
                }

                var fileName = string.Empty;

                if (isViewMode)
                {
                    //預覽：儲存到 Temp 路徑暫存檔，再直接預覽
                    fileName = Path.GetTempFileName();
                }
                else
                {
                    var sf = new SaveFileDialog
                    {
                        Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                        Filter = @"All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")),
                        RestoreDirectory = true,
                        FileName = IsFromSingleRecordForm ? string.Empty : lblColumnName2.Text
                    };

                    if (sf.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    fileName = sf.FileName;
                }

                TextEngine.WriteBinaryFile(fileName, _binaryContent);

                if (isViewMode && File.Exists(fileName))
                {
                    using (var form = new BlobViewerForm())
                    {
                        MyLibrary.IsBlobReadOnly = true;
                        form.FileName = fileName;
                        form.FieldName = IsFromSingleRecordForm ? string.Empty : $"{lblColumnName.Text}{lblColumnName2.Text}";

                        var blobViewerFontSize = 0;
                        var sbSql = new StringBuilder();

                        sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                        sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                        sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                        sbSql.Append("   AND AttributeName = 'BlobViewerFontSize'");

                        var sql = sbSql.ToString();
                        var dtData = JasonQueryRepository.ExecQuery(sql);

                        if (dtData?.Rows.Count > 0)
                        {
                            int.TryParse(dtData.Rows[0].GetSafeString(0), out blobViewerFontSize);
                        }
                        else
                        {
                            blobViewerFontSize = 10;
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFontSize", "10");
                        }

                        var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "BlobViewerFormWidth", "BlobViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                        form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                        form.FontSize = blobViewerFontSize;
                        form.ShowDialog();
                    }

                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool EnsureBinaryContentLoaded()
        {
            if (!IsBlobColumn || !HasBlobContent)
            {
                return false;
            }

            if (_binaryContent != null)
            {
                return true;
            }

            if (Blob != null)
            {
                _binaryContent = Blob;
                return true;
            }

            if (_binaryContentLoader == null)
            {
                return false;
            }

            Cursor = Cursors.WaitCursor;
            tsTool.Cursor = Cursors.WaitCursor;

            try
            {
                var loadResult = LargeValueContentLoader.LoadBinary(_binaryContentLoader);

                if (!loadResult.Succeeded)
                {
                    throw loadResult.Error;
                }

                _binaryContent = loadResult.Content;
                Blob = _binaryContent;

                return _binaryContent != null;
            }
            finally
            {
                Cursor = Cursors.Default;
                tsTool.Cursor = Cursors.Default;
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editor.SelectionStart = 0;
            editor.SelectionEnd = editor.Text.Length;
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            MoveToRow(0);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            MoveToRow(CurrentRow - 1);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            MoveToRow(CurrentRow + 1);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            MoveToRow(TotalQty - 1);
        }

        private void MoveToRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= TotalQty)
            {
                return;
            }

            CurrentRow = rowIndex;
            LoadCurrentCell();
        }

        private void LoadCurrentCell()
        {
            if (CellValueLoader == null)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            tsTool.Cursor = Cursors.WaitCursor;

            try
            {
                var context = CellValueLoader(CurrentRow);

                if (context == null)
                {
                    return;
                }

                ApplyContext(context);
                CurrentRowChanged?.Invoke(CurrentRow);
                CheckButtonStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
                tsTool.Cursor = Cursors.Default;
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }

        private void CheckButtonStatus()
        {
            btnFirst.Enabled = true;
            btnPrevious.Enabled = true;
            btnNext.Enabled = true;
            btnLast.Enabled = true;

            if (TotalQty == 1)
            {
                btnFirst.Enabled = false;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
            else if (CurrentRow == 0)
            {
                btnFirst.Enabled = false;
                btnPrevious.Enabled = false;
            }
            else if (CurrentRow == TotalQty - 1)
            {
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
        }

        private void editor_DoubleClick(object sender, ScintillaNET.DoubleClickEventArgs e)
        {
            HighlightSelection(true);
        }

        private void HighlightSelection(bool isMouseClick = false)
        {
            var pos = editor.CurrentPosition;
            var wordStart = editor.WordStartPosition(pos, true);
            var wordEnd = editor.WordEndPosition(pos, true);
            var textUpper = editor.Text.ToUpper();
            var word = editor.GetTextRange(wordStart, wordEnd - wordStart);
            var value = true;

            editor.AdditionalCaretsVisible = false; //選取的字串，最前面的 | 要不要顯示

            //如果選取文字包含了換行符號或空白，不用 multi-select！
            if (editor.SelectedText.Any(c => c == '\r' || c == '\n' || c == ' '))
            {
                value = false;
            }

            //for Mouse Click，如果沒有選取文字，需要重新處理 multi selection
            //20190320 此處還有bug 需要調整：左右鍵移動時，在單字移動，例如 word，游標在 d 按左鍵移動，此時不會選取 (忽略 bMouseClick，會造成左右鍵失效)
            if (string.IsNullOrEmpty(editor.SelectedText) && isMouseClick)
            {
                editor.Tag = string.Empty;
            }

            if (string.IsNullOrEmpty(word) || !value)
            {
                return;
            }

            editor.Tag = word;
            _selectedTextDoubleClick = word;

            var mainSelection = 0;
            var matches = Regex.Matches(textUpper, word.ToUpper());

            editor.MultipleSelection = true;

            foreach (Match m in matches)
            {
                if (pos >= m.Index && pos - m.Index <= word.Length)
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

        private void CopyCellViewer()
        {
            Clipboard.Clear();

            if (!TextHelper.IsNullOrEmptyTag(editor.Tag))
            {
                TextHelper.CopyTextToClipboard(TextHelper.GetSafeString(editor.Tag), "CopyCellViewer(01)");
            }
            else
            {
                //20250709 判斷是否為「重複按下 Ctrl+C」
                if (!string.IsNullOrEmpty(_selectedTextDoubleClick) && string.Equals(editor.SelectedText, _selectedTextDoubleClick, StringComparison.OrdinalIgnoreCase))
                {
                    TextHelper.CopyTextToClipboard(_selectedTextDoubleClick, "CopyCellViewer(02)");
                }
                else
                {
                    editor.Copy();
                }
            }
        }

        private void UpdateEditorText(string text)
        {
            editor.ReadOnly = false;
            editor.Text = text;
            editor.ReadOnly = true;
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