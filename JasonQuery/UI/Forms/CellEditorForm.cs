using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class CellEditorForm : Form
    {
        public event EventHandler<CellEditorValueChangedEventArgs> CellValueChanged;
        public event EventHandler<CellEditorBinaryValueAppliedEventArgs> BinaryValueApplied;

        public string Result { get; set; }

        public bool HasBlobContent { get; set; }

        public bool IsBlobColumn { get; set; }

        public byte[] Blob { get; set; }

        public Func<byte[]> BinaryContentLoader { get; set; }

        public Func<CellEditorBinaryUpdateRequest, CellEditorBinaryUpdateResult> BinaryUpdateExecutor { get; set; }

        public enum CellEditorValueChangeReason
        {
            Apply = 0,
            SetNull = 1,
            Cancel = 2
        }

        public sealed class CellEditorValueChangedEventArgs : EventArgs
        {
            public CellEditorValueChangedEventArgs(string value, CellEditorValueChangeReason reason, bool isNullValue)
            {
                Value = value ?? string.Empty;
                Reason = reason;
                IsNullValue = isNullValue;
            }

            public string Value { get; private set; }

            public CellEditorValueChangeReason Reason { get; private set; }

            public bool IsNullValue { get; private set; }
        }

        private ContextMenuStrip _cMenu = new ContextMenuStrip();
        private string _columnName;
        private string _columnType;
        private string _originalCellText;
        private bool _canSetNull = false;
        private int _lengthTotal = 0; //總長度
        private int _lengthSet = 0; //每一組的長度限制
        private bool _suppressTextChanged;
        private ColumnInfo _columnInfo;
        private byte[] _binaryContent;
        private bool _isBinaryMode;

        public ColumnInfo ColumnInformation
        {
            get
            {
                return _columnInfo;
            }
            set
            {
                _columnInfo = value;
            }
        }

        public string ColumnName
        {
            set => _columnName = value;
        }

        public string ColumnType
        {
            set => _columnType = value;
        }

        public int LengthTotal
        {
            get => _lengthTotal;
            set => _lengthTotal = value;
        }

        public int LengthSet
        {
            get => _lengthSet;
            set => _lengthSet = value;
        }

        public string OriginalCellText
        {
            get => _originalCellText;
            set => _originalCellText = value;
        }

        public bool CanSetNull
        {
            set => _canSetNull = value;
        }

        public CellEditorForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

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

                lblColumnName2.Text = _columnName;

                if (!string.IsNullOrEmpty(_columnType))
                {
                    lblColumnType2.Text = _columnType;

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

                editorCellEditor.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                editorCellEditor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorCellEditor.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

                SetEditorText(_originalCellText);
                Result = _originalCellText;
                editorCellEditor.ReadOnly = false; //20240609 一般編輯模式，允許變更值

                var languageText = string.Empty;

                languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
                _cMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_cMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.A;

                _cMenu.Items[0].Click += delegate
                {
                    editorCellEditor.SelectionStart = 0;
                    editorCellEditor.SelectionEnd = editorCellEditor.Text.Length;
                };

                _cMenu.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
                _cMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_cMenu.Items[1]).ShortcutKeys = Keys.Control | Keys.C;

                _cMenu.Items[1].Click += delegate
                {
                    editorCellEditor.Copy();
                };

                _cMenu.Items[1].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");

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
                            editorCellEditor.WrapIndentMode = ScintillaNET.WrapIndentMode.Fixed;
                            break;
                        }
                    case "Indent":
                        {
                            editorCellEditor.WrapIndentMode = ScintillaNET.WrapIndentMode.Indent;
                            break;
                        }
                    default:
                        {
                            editorCellEditor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
                            break;
                        }
                }

                if (MyLibrary.ShowAllCharacters)
                {
                    editorCellEditor.ViewEol = true;
                    editorCellEditor.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
                    btnShowAllCharacters.Visible = false;
                    btnShowAllCharacters2.Visible = true;
                }
                else
                {
                    editorCellEditor.ViewEol = false;
                    editorCellEditor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
                    btnShowAllCharacters.Visible = true;
                    btnShowAllCharacters2.Visible = false;
                }

                UpdateEditorCommandStatus();

                //預設全選
                editorCellEditor.SelectionStart = 0;
                editorCellEditor.SelectionEnd = editorCellEditor.Text.Length;

                lblLength.Text = LocalizationHelper.GetLanguageString(lblLength.Text, "form", GetType().Name, "object", "lblLength", "Text");
                lblLength2.Text = _lengthTotal.ToString();

                tsTool.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                InitializeBinaryMode();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnApplyEdit.Enabled)
            {
                //20240910 尚未套用就離開！
                //MessageBox.Show("?", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "CellEditorFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "CellEditorFormHeight", Size.Height.ToString());
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

            editorCellEditor.Styler = new SqlStyler();
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            if (editorCellEditor.WrapMode == ScintillaNET.WrapMode.Word)
            {
                btnWordWrap.Visible = true;
                btnWordWrap2.Visible = false;
                editorCellEditor.WrapMode = ScintillaNET.WrapMode.None;
            }
            else
            {
                btnWordWrap.Visible = false;
                btnWordWrap2.Visible = true;
                editorCellEditor.WrapMode = ScintillaNET.WrapMode.Word;
                editorCellEditor.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorCellEditor.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            if (editorCellEditor.ViewEol)
            {
                btnShowAllCharacters.Visible = true;
                btnShowAllCharacters2.Visible = false;
                editorCellEditor.ViewEol = false;
                editorCellEditor.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            }
            else
            {
                btnShowAllCharacters.Visible = false;
                btnShowAllCharacters2.Visible = true;
                editorCellEditor.ViewEol = true;
                editorCellEditor.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            }
        }

        private void editorCellViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _cMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorCellEditor.Text);

            //Copy
            _cMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorCellEditor.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            editorCellEditor.ContextMenuStrip = _cMenu;

            if (MyLibrary.IsDarkMode)
            {
                _cMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenu.ForeColor = Color.White;
                _cMenu.RenderMode = ToolStripRenderMode.System;
            }

            _cMenu.Show(editorCellEditor, new Point(e.X, e.Y));
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            editorCellEditor.Copy();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editorCellEditor.SelectionStart = 0;
            editorCellEditor.SelectionEnd = editorCellEditor.Text.Length;
        }

        private void btnSetNull_Click(object sender, EventArgs e)
        {
            SetEditorText(GetNullDisplayText());

            editorCellEditor.SelectionStart = 0;

            ApplyEditorValueToOwner(CellEditorValueChangeReason.SetNull);
        }

        private void btnApplyEdit_Click(object sender, EventArgs e)
        {
            ApplyEditorValueToOwner(CellEditorValueChangeReason.Apply);
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            SetEditorText(_originalCellText);

            editorCellEditor.SelectionStart = 0;

            ApplyEditorValueToOwner(CellEditorValueChangeReason.Cancel);
        }

        private void btnLoadFileToBlobField_Click(object sender, EventArgs e)
        {
            if (!_isBinaryMode || BinaryUpdateExecutor == null)
            {
                return;
            }

            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = LocalizationHelper.GetLanguageString("Select the file to save to the {ColumnName} field...", "form", Name, "msg", "SelectFileToSaveTo", "Text")
                                                     .Replace("{ColumnName}", _columnInfo.ColumnName);
                    dialog.Filter = @"All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text"));
                    dialog.RestoreDirectory = true;

                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    var fileInfo = new FileInfo(dialog.FileName);

                    if (!fileInfo.Exists)
                    {
                        return;
                    }

                    var confirmText = LocalizationHelper.GetLanguageString
                    (
                        "Loading this file will immediately execute an UPDATE for the current binary cell. Continue?",
                        "form",
                        Name,
                        "msg",
                        "ConfirmLoadFileToBinaryField",
                        "Text"
                    );

                    var fileLabel = LocalizationHelper.GetLanguageString("File", "Global", "Global", "msg", "File", "Text");
                    var sizeLabel = LocalizationHelper.GetLanguageString("Size", "Global", "Global", "msg", "Size", "Text");
                    var columnLabel = LocalizationHelper.GetLanguageString("Column", "Global", "Global", "msg", "Column", "Text");
                    var confirmMessage = $"{confirmText}\r\n\r\n{columnLabel}: {_columnInfo.ColumnName}\r\n{fileLabel}: {fileInfo.Name}\r\n{sizeLabel}: {fileInfo.Length:N0} bytes";

                    if (MessageBox.Show(confirmMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    {
                        return;
                    }

                    Cursor = Cursors.WaitCursor;
                    tsTool.Cursor = Cursors.WaitCursor;

                    //先確認檔案可以完整讀取，再執行資料庫 UPDATE；避免 UPDATE 成功後 UI 無法建立新內容狀態。
                    var selectedBinaryContent = File.ReadAllBytes(fileInfo.FullName);

                    var request = new CellEditorBinaryUpdateRequest
                    {
                        FileName = fileInfo.FullName,
                        FileLength = fileInfo.Length
                    };

                    var result = BinaryUpdateExecutor(request);

                    if (result == null || !result.Success || result.AffectedRows != 1)
                    {
                        var failedText = LocalizationHelper.GetLanguageString("The file you specified failed to load!", "form", Name, "msg", "FileLoadedNG", "Text");
                        var errorText = result == null ? string.Empty : result.ErrorMessage;
                        var affectedRows = result == null ? 0 : result.AffectedRows;

                        if (affectedRows != 0 && affectedRows != 1)
                        {
                            errorText = $"{errorText}\r\nAffected rows: {affectedRows:N0}".Trim();
                        }

                        MessageBox.Show($"{failedText}\r\n\r\n{errorText}".Trim(), AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    _binaryContent = selectedBinaryContent;
                    Blob = _binaryContent;
                    HasBlobContent = true;

                    var displayText = string.IsNullOrWhiteSpace(result.DisplayText) ? BuildBinaryDisplayText(result.BinaryTypeName, _binaryContent.LongLength) : result.DisplayText;

                    Result = displayText;
                    SetEditorText(displayText);
                    UpdateBinaryCommandStatus();

                    BinaryValueApplied?.Invoke
                    (
                        this,
                        new CellEditorBinaryValueAppliedEventArgs(_binaryContent, fileInfo.FullName, result)
                    );

                    var successText = LocalizationHelper.GetLanguageString("The file you specified has been loaded!", "form", Name, "msg", "FileLoadedOK", "Text");

                    MessageBox.Show(successText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
                tsTool.Cursor = Cursors.Default;
            }
        }

        private void editorCellEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Enter))
            {
                if (btnApplyEdit.Enabled)
                {
                    btnApplyEdit.PerformClick();
                }

                e.SuppressKeyPress = true;
            }
        }

        private void editorCellEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            //不再限制使用者輸入
            //資料格式正確性由資料庫執行 UPDATE 時決定
        }

        private void editorCellEditor_BeforeInsert(object sender, ScintillaNET.BeforeModificationEventArgs e)
        {
            //不再做輸入前文字備份
        }

        private void editorCellEditor_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTextChanged)
            {
                return;
            }

            try
            {
                UpdateEditorCommandStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tmrUpdateText_Tick(object sender, EventArgs e)
        {
            var selectionStart = editorCellEditor.SelectionStart;
            var text = TextHelper.GetTwoByteCharSubString(editorCellEditor.Text, _lengthTotal); //一個中文字算 2個字元

            editorCellEditor.Text = text;
            editorCellEditor.SelectionStart = selectionStart - 1;

            tmrUpdateText.Enabled = false;
        }

        private void SetEditorText(string text)
        {
            _suppressTextChanged = true;

            try
            {
                editorCellEditor.Text = text ?? string.Empty;
            }
            finally
            {
                _suppressTextChanged = false;
            }

            UpdateEditorCommandStatus();
        }

        private void ApplyEditorValueToOwner(CellEditorValueChangeReason reason)
        {
            Result = editorCellEditor.Text ?? string.Empty;

            var isNullValue = IsNullDisplayText(Result);

            CellValueChanged?.Invoke
            (
                this,
                new CellEditorValueChangedEventArgs(Result, reason, isNullValue)
            );

            UpdateEditorCommandStatus();
        }

        private void UpdateEditorCommandStatus()
        {
            if (_isBinaryMode)
            {
                UpdateBinaryCommandStatus();
                UpdateTextLengthStatus();
                return;
            }

            var editorText = editorCellEditor.Text ?? string.Empty;
            var synchronizedText = Result ?? string.Empty;

            btnApplyEdit.Enabled = !string.Equals(editorText, synchronizedText, StringComparison.Ordinal);
            btnCancelEdit.Enabled = !string.Equals(synchronizedText, _originalCellText ?? string.Empty, StringComparison.Ordinal)
                                    || !string.Equals(editorText, _originalCellText ?? string.Empty, StringComparison.Ordinal);

            btnSetNull.Enabled = _canSetNull && !IsNullDisplayText(editorText);

            UpdateTextLengthStatus();
        }

        private string GetNullDisplayText()
        {
            return string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs;
        }

        private bool IsNullDisplayText(string text)
        {
            text ??= string.Empty;

            if (string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(text);
            }

            return string.Equals(text, MyLibrary.GridNullShowAs, StringComparison.OrdinalIgnoreCase);
        }

        private int GetEffectiveTextLength(string text)
        {
            if (IsNullDisplayText(text))
            {
                return 0;
            }

            return string.IsNullOrEmpty(text) ? 0 : text.Length;
        }

        private bool HasColumnLengthLimit()
        {
            if (_lengthTotal <= 0)
            {
                return false;
            }

            if (_columnInfo == null)
            {
                return true;
            }

            //LargeText 通常不應該用一般 ColumnSize 強制限制
            if (_columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
            {
                return false;
            }

            return true;
        }

        private void UpdateTextLengthStatus()
        {
            var length = GetEffectiveTextLength(editorCellEditor.Text);

            if (!HasColumnLengthLimit())
            {
                lblLength2.Text = length.ToString();
                return;
            }

            lblLength2.Text = $"{length}/{_lengthTotal}";
        }

        private void btnSaveBlobToFile_Click(object sender, EventArgs e)
        {
            SaveBinaryContent(false);
        }

        private void btnViewDataAsHex_Click(object sender, EventArgs e)
        {
            SaveBinaryContent(true);
        }

        private void InitializeBinaryMode()
        {
            _isBinaryMode = _columnInfo != null && _columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary;
            IsBlobColumn = _isBinaryMode;

            if (!_isBinaryMode)
            {
                btnLoadFileToBlobField.Enabled = false;
                btnSaveBlobToFile.Enabled = false;
                btnViewDataAsHex.Enabled = false;
                return;
            }

            _binaryContent = Blob;
            HasBlobContent = HasBlobContent || _binaryContent != null || BinaryContentLoader != null;

            editorCellEditor.ReadOnly = true;
            btnSetNull.Enabled = false;
            btnApplyEdit.Enabled = false;
            btnCancelEdit.Enabled = false;

            UpdateBinaryCommandStatus();
        }

        private void UpdateBinaryCommandStatus()
        {
            var hasBinarySource = _binaryContent != null || Blob != null || BinaryContentLoader != null;

            btnLoadFileToBlobField.Enabled = _isBinaryMode && BinaryUpdateExecutor != null;
            btnSaveBlobToFile.Enabled = _isBinaryMode && HasBlobContent && hasBinarySource;
            btnViewDataAsHex.Enabled = _isBinaryMode && HasBlobContent && hasBinarySource;

            if (_isBinaryMode)
            {
                btnApplyEdit.Enabled = false;
                btnCancelEdit.Enabled = false;
                btnSetNull.Enabled = false;
            }
        }

        private bool EnsureBinaryContentLoaded()
        {
            if (!_isBinaryMode || !HasBlobContent)
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

            if (BinaryContentLoader == null)
            {
                return false;
            }

            Cursor = Cursors.WaitCursor;
            tsTool.Cursor = Cursors.WaitCursor;

            try
            {
                _binaryContent = BinaryContentLoader();
                Blob = _binaryContent;
                HasBlobContent = _binaryContent != null;

                return _binaryContent != null;
            }
            finally
            {
                Cursor = Cursors.Default;
                tsTool.Cursor = Cursors.Default;
            }
        }

        private void SaveBinaryContent(bool viewAsHex)
        {
            string tempFileName = null;

            try
            {
                if (!EnsureBinaryContentLoaded())
                {
                    return;
                }

                string fileName;

                if (viewAsHex)
                {
                    tempFileName = Path.GetTempFileName();
                    fileName = tempFileName;
                }
                else
                {
                    using (var dialog = new SaveFileDialog())
                    {
                        dialog.Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text");
                        dialog.Filter = @"All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text"));
                        dialog.RestoreDirectory = true;
                        dialog.FileName = lblColumnName2.Text;

                        if (dialog.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }

                        fileName = dialog.FileName;
                    }
                }

                TextEngine.WriteBinaryFile(fileName, _binaryContent);

                if (!viewAsHex || !File.Exists(fileName))
                {
                    return;
                }

                using (var form = new BlobViewerForm())
                {
                    MyLibrary.IsBlobReadOnly = true;
                    form.FileName = fileName;
                    form.FieldName = $"{lblColumnName.Text}{lblColumnName2.Text}";

                    var blobViewerFontSize = GetBlobViewerFontSize();
                    var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings
                    (
                        MyGlobal.DomainUser,
                        "BlobViewerFormWidth",
                        "BlobViewerFormHeight",
                        defaultWidth: form.ClientSize.Width,
                        defaultHeight: form.ClientSize.Height
                    );

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.FontSize = blobViewerFontSize;
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFileName))
                {
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                        //暫存檔刪除失敗不影響使用者操作
                    }
                }
            }
        }

        private int GetBlobViewerFontSize()
        {
            var blobViewerFontSize = 0;
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
            sbSql.Append("   AND AttributeName = 'BlobViewerFontSize'");

            var dtData = JasonQueryRepository.ExecQuery(sbSql.ToString());

            if (dtData != null && dtData.Rows.Count > 0)
            {
                int.TryParse(dtData.Rows[0][0].ToString(), out blobViewerFontSize);
            }

            if (blobViewerFontSize > 0)
            {
                return blobViewerFontSize;
            }

            JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFontSize", "10");
            return 10;
        }

        private static string BuildBinaryDisplayText(string binaryTypeName, long length)
        {
            var typeName = string.IsNullOrWhiteSpace(binaryTypeName) ? "Binary" : binaryTypeName;

            return $"({typeName})({length:N0})";
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
