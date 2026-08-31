using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.HexBox;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class BlobViewerForm : Form
    {
        private int _fontSize;
        private string _languageText;
        private BlobGotoDialog _blobGotoForm = new BlobGotoDialog();
        private string _fileName, _fileName2, _fieldName;
        private List<string> _menuList = new List<string>();
        private ContextMenuStrip _gridContextMenu = new ContextMenuStrip(); //右鍵選單

        public string FileName
        {
            set => _fileName2 = value;
        }

        public string FieldName
        {
            set => _fieldName = value;
        }

        public int FontSize
        {
            set => _fontSize = value;
        }

        public BlobViewerForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false, false);
                ApplyEditorMenu();

                Tag = Text;
                nudFontSize.Location = new Point(lblEncoding2.Left + lblEncoding.Width + cboEncoding.Width + lblFontSize.Width + 12, nudFontSize.Top);
                chkBits.Location = new Point(nudFontSize.Left + nudFontSize.Width + 15, chkBits.Top);
                chkReadOnly.Location = new Point(chkBits.Left + chkBits.Width + 3, chkReadOnly.Top);
                chkTopMost.Location = new Point(chkReadOnly.Left + chkReadOnly.Width + 3, chkTopMost.Top);

                toolStrip.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkBits.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkReadOnly.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkTopMost.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                var defConverter = new DefaultByteCharConverter();
                var ebcdicConverter = new EbcdicByteCharProvider();

                cboEncoding.Items.Add(defConverter);
                cboEncoding.Items.Add(ebcdicConverter);
                cboEncoding.SelectedIndex = 0;

                if (MyLibrary.IsDarkMode)
                {
                    lblEncoding.ForeColor = Color.White;
                    lblFontSize.ForeColor = Color.White;
                    hexBox.BackColor = ColorTranslator.FromHtml("#2D2D30");
                    hexBox.ForeColor = Color.White;
                }

                //20230902 NumericUpDown 的 ValueChanged 事件要特殊處理，否則有時會沒有觸發成功
                if (_fontSize == 15)
                {
                    nudFontSize.Value = 14;
                    nudFontSize.UpButton();
                }
                else
                {
                    nudFontSize.Value = _fontSize + 1;
                    nudFontSize.DownButton();
                }

                nudFontSize.Value = _fontSize;

                hexBox.CurrentLineChanged += new EventHandler(Position_Changed);
                hexBox.CurrentPositionInLineChanged += new EventHandler(Position_Changed);

                DisplayText();
                ManageAbility();

                if (File.Exists(_fileName2))
                {
                    OpenFile(_fileName2);
                }

                chkReadOnly.Checked = MyLibrary.IsBlobReadOnly;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            var res = CloseFile();

            if (res == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFormHeight", Size.Height.ToString());
        }

        private void ApplyEditorMenu()
        {
            _menuList = new List<string>();

            _languageText = LocalizationHelper.GetLanguageString("Cut", "form", GetType().Name, "menueditor", "Cut", "Text");
            _menuList.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
            _menuList.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Paste", "form", GetType().Name, "menueditor", "Paste", "Text");
            _menuList.Add(_languageText);
            _menuList.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Copy Hex", "form", GetType().Name, "menueditor", "CopyHex", "Text");
            _menuList.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Paste Hex", "form", GetType().Name, "menueditor", "PasteHex", "Text");
            _menuList.Add(_languageText);
            _menuList.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Go To", "form", GetType().Name, "menueditor", "GoTo", "Text");
            _menuList.Add(_languageText);
            _menuList.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
            _menuList.Add(_languageText);

            _gridContextMenu = new ContextMenuStrip();

            _gridContextMenu.Items.Add(_menuList[MapColumn.Cut]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.Copy]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.Paste]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.Dash1]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.CopyHex]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.PasteHex]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.Dash2]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.GoTo]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.Dash3]);
            _gridContextMenu.Items.Add(_menuList[MapColumn.SelectAll]);

            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.Cut]).ShortcutKeys = Keys.Control | Keys.X;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.Copy]).ShortcutKeys = Keys.Control | Keys.C;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.Paste]).ShortcutKeys = Keys.Control | Keys.V;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.CopyHex]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.PasteHex]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.GoTo]).ShortcutKeys = Keys.Control | Keys.G;
            ((ToolStripMenuItem)_gridContextMenu.Items[MapColumn.SelectAll]).ShortcutKeys = Keys.Control | Keys.A;

            _gridContextMenu.Items[MapColumn.Cut].Click += delegate
            {
                hexBox.Cut();
            };

            _gridContextMenu.Items[MapColumn.Copy].Click += delegate
            {
                hexBox.Copy();
            };

            _gridContextMenu.Items[MapColumn.Paste].Click += delegate
            {
                hexBox.Paste();
            };

            _gridContextMenu.Items[MapColumn.CopyHex].Click += delegate
            {
                hexBox.CopyHex();
            };

            _gridContextMenu.Items[MapColumn.PasteHex].Click += delegate
            {
                hexBox.PasteHex();
            };

            _gridContextMenu.Items[MapColumn.GoTo].Click += delegate
            {
                GoTo();
            };

            _gridContextMenu.Items[MapColumn.SelectAll].Click += delegate
            {
                hexBox.SelectAll();
            };
        }

        private bool CheckModify()
        {
            var result = true;

            if (btnSave.Enabled)
            {
                _languageText = LocalizationHelper.GetLanguageString("Save File?", "form", GetType().Name, "msg", "SaveFile", "Text");

                var res = MessageBox.Show(_languageText, Tag.ToString(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (res == DialogResult.Yes)
                {
                    SaveFile();
                    CleanUp();
                }
                else if (res == DialogResult.No)
                {
                    CleanUp();
                }
                else if (res == DialogResult.Cancel)
                {
                    result = false;
                }
            }

            return result;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckModify())
                {
                    btnSave.Enabled = false;
                    OpenFile();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OpenFile()
        {
            var of = new OpenFileDialog
            {
                Multiselect = false,
                Title = LocalizationHelper.GetLanguageString("Open Hex File...", "form", GetType().Name, "msg", "OpenFile", "Text"),
                Filter = @"All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text"))
            };

            if (of.ShowDialog() == DialogResult.OK)
            {
                MyLibrary.IsBlobReadOnly = false;
                chkReadOnly.Checked = false;
                btnSave.Enabled = false;

                OpenFile(of.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            if (!CheckModify())
            {
                return;
            }

            btnSave.Enabled = false;
            lblPosition.Visible = true;
            sp1.Visible = true;
            lblBitPosition.Visible = true;
            sp2.Visible = true;
            lblFileSize.Visible = true;

            try
            {
                DynamicFileByteProvider dynamicFileByteProvider;

                try
                {
                    //try to open in write mode
                    dynamicFileByteProvider = new DynamicFileByteProvider(fileName);
                    dynamicFileByteProvider.Changed += new EventHandler(byteProvider_Changed);
                    dynamicFileByteProvider.LengthChanged += new EventHandler(byteProvider_LengthChanged);
                }
                catch (IOException)
                {
                    try
                    {
                        //try to open in read-only mode
                        dynamicFileByteProvider = new DynamicFileByteProvider(fileName, true);
                    }
                    catch (IOException ex)
                    {
                        //file cannot be opened
                        _languageText = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                        MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                hexBox.ByteProvider = dynamicFileByteProvider;
                _fileName = fileName;

                DisplayText();
                UpdateFileSizeStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void byteProvider_Changed(object sender, EventArgs e)
        {
            ManageAbility();
        }

        private void byteProvider_LengthChanged(object sender, EventArgs e)
        {
            UpdateFileSizeStatus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            try
            {
                if (hexBox.ByteProvider == null)
                {
                    return;
                }

                DynamicFileByteProvider dynamicFileByteProvider = hexBox.ByteProvider as DynamicFileByteProvider;
                dynamicFileByteProvider.ApplyChanges();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                ManageAbility();
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                if (hexBox.ByteProvider == null)
                {
                    return;
                }

                var fileNameSaveAs = string.Empty;

                var sf = new SaveFileDialog
                {
                    Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                    Filter = @"All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")),
                    RestoreDirectory = true
                };

                if (File.Exists(_fileName))
                {
                    sf.InitialDirectory = Path.GetDirectoryName(_fileName);
                    sf.FileName = Path.GetFileName(_fileName);
                }

                if (sf.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                else
                {
                    fileNameSaveAs = sf.FileName;
                }

                var inData = new byte[hexBox.ByteProvider.Length];

                for (var i = 0; i < hexBox.ByteProvider.Length; i++)
                {
                    inData[i] = hexBox.ByteProvider.ReadByte(i);
                }

                try
                {
                    TextEngine.WriteBinaryFile(fileNameSaveAs, inData);
                    btnSave.Enabled = false;
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                //20230903 關閉原先的檔案
                CleanUp();

                //20230903 另存沒有錯誤，重新開啟另存後的檔案
                OpenFile(fileNameSaveAs);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void hexBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void hexBox_DragDrop(object sender, DragEventArgs e)
        {
            object allFileNames = e.Data.GetData(DataFormats.FileDrop);
            var fileNames = (string[])allFileNames;

            if (fileNames.Length > 0)
            {
                OpenFile(fileNames[0]);
            }
        }

        private DialogResult CloseFile()
        {
            if (hexBox.ByteProvider == null)
            {
                return DialogResult.OK;
            }

            try
            {
                if (hexBox.ByteProvider != null && hexBox.ByteProvider.HasChanges())
                {
                    _languageText = LocalizationHelper.GetLanguageString("Save File?", "form", GetType().Name, "msg", "SaveFile", "Text");

                    var res = MessageBox.Show(_languageText, Tag.ToString(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    if (res == DialogResult.Yes)
                    {
                        SaveFile();
                        CleanUp();
                    }
                    else if (res == DialogResult.No)
                    {
                        CleanUp();
                    }

                    return res;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                ManageAbility();
            }

            CleanUp();
            return DialogResult.OK;
        }

        private void CleanUp()
        {
            if (hexBox.ByteProvider != null)
            {
                var byteProvider = hexBox.ByteProvider as IDisposable;

                if (byteProvider != null)
                {
                    byteProvider.Dispose();
                }

                hexBox.ByteProvider = null;
            }

            _fileName = null;
            DisplayText();
        }

        private void ManageAbility()
        {
            if (hexBox.ByteProvider == null)
            {
                btnSave.Enabled = false;
            }
            else
            {
                btnSave.Enabled = hexBox.ByteProvider.HasChanges();
            }
        }

        private void DisplayText()
        {
            if (_fileName != null && _fileName.Length > 0)
            {
                var textFormat = "{0} - {1}";
                var fileName = Path.GetFileName(_fileName);

                if (fileName.StartsWith("tmp", StringComparison.Ordinal) && fileName.EndsWith("tmp", StringComparison.Ordinal))
                {
                    var temp = string.IsNullOrEmpty(_fieldName) ? string.Empty : $" ({_fieldName})";

                    Text = string.Format(textFormat, Tag.ToString(), $"{fileName}{temp}");
                }
                else
                {
                    Text = string.Format(textFormat, Tag.ToString(), fileName);
                }
            }
            else
            {
                Text = Tag.ToString();
            }
        }

        private void UpdateFileSizeStatus()
        {
            if (hexBox.ByteProvider == null)
            {
                lblFileSize.Text = string.Empty;
            }
            else
            {
                lblFileSize.Text = Util.GetDisplayBytes(hexBox.ByteProvider.Length).Replace("Bytes", lblBytes.Text);
            }
        }

        void Position_Changed(object sender, EventArgs e)
        {
            lblPosition.Text = $"{lblLn.Text} {hexBox.CurrentLine}  {lblCol.Text} {hexBox.CurrentPositionInLine}";

            byte? currentByte = hexBox.ByteProvider != null && hexBox.ByteProvider.Length > hexBox.SelectionStart
                                ? hexBox.ByteProvider.ReadByte(hexBox.SelectionStart) : (byte?)null;

            var bitPresentation = string.Empty;

            BitInfo bitInfo = currentByte != null ? new BitInfo((byte)currentByte, hexBox.SelectionStart) : null;

            if (bitInfo != null)
            {
                byte currentByteNotNull = (byte)currentByte;

                bitPresentation = $"Bits of Byte {hexBox.SelectionStart}: {bitInfo}";
            }

            lblBitPosition.Text = bitPresentation;
            sp1.Visible = !string.IsNullOrEmpty(bitPresentation);
            bitControl1.BitInfo = bitInfo;
        }

        void bitControl1_BitChanged(object sender, EventArgs e)
        {
            hexBox.ByteProvider.WriteByte(bitControl1.BitInfo.Position, bitControl1.BitInfo.Value);
            hexBox.Invalidate();
        }

        private void hexBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _gridContextMenu.Items[MapColumn.Cut].Enabled = !MyLibrary.IsBlobReadOnly && hexBox.CanCut();
            _gridContextMenu.Items[MapColumn.Cut].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Cut 16x16.ico");
            _gridContextMenu.Items[MapColumn.Copy].Enabled = hexBox.CanCopy();
            _gridContextMenu.Items[MapColumn.Copy].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            _gridContextMenu.Items[MapColumn.Paste].Enabled = !MyLibrary.IsBlobReadOnly && hexBox.CanPaste();
            _gridContextMenu.Items[MapColumn.Paste].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste 16x16.ico");
            _gridContextMenu.Items[MapColumn.CopyHex].Enabled = hexBox.CanCopy();
            _gridContextMenu.Items[MapColumn.CopyHex].Image = IconManager.GetImage(MyGlobal.IconLibrary, "CopyBlob 16x16.ico");
            _gridContextMenu.Items[MapColumn.PasteHex].Enabled = !MyLibrary.IsBlobReadOnly && hexBox.CanPasteHex();
            _gridContextMenu.Items[MapColumn.PasteHex].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Blob 16x16.ico");
            _gridContextMenu.Items[MapColumn.GoTo].Enabled = hexBox.ByteProvider != null && hexBox.ByteProvider.Length > 0; //20250520 修正 Enabled 的判斷方法
            _gridContextMenu.Items[MapColumn.GoTo].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Goto XY 16x16.ico");
            _gridContextMenu.Items[MapColumn.SelectAll].Enabled = hexBox.CanSelectAll();
            _gridContextMenu.Items[MapColumn.SelectAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            hexBox.ContextMenuStrip = _gridContextMenu;
            _gridContextMenu.Show(hexBox, new Point(e.X, e.Y));
        }

        private void cboEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            hexBox.ByteCharConverter = cboEncoding.SelectedItem as IByteCharConverter;
        }

        private void chkBits_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBits.Checked)
            {
                hexBox.Height -= bitControl1.Height;
                bitControl1.Visible = true;
            }
            else
            {
                hexBox.Height += bitControl1.Height;
                bitControl1.Visible = false;
            }
        }

        private void chkReadOnly_CheckedChanged(object sender, EventArgs e)
        {
            MyLibrary.IsBlobReadOnly = chkReadOnly.Checked;
        }

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = chkTopMost.Checked;
        }

        private void nudFontSize_ValueChanged(object sender, EventArgs e)
        {
            hexBox.Font = new Font("Consolas", (float)nudFontSize.Value, FontStyle.Regular, GraphicsUnit.Point, 136);

            JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFontSize", nudFontSize.Value.ToString());
        }

        private void nudFontSize_Enter(object sender, EventArgs e)
        {
            UpDownBase text = nudFontSize;
            nudFontSize.Select(0, text.Text.Length);
        }

        private void nudFontSize_MouseClick(object sender, MouseEventArgs e)
        {
            UpDownBase text = nudFontSize;
            nudFontSize.Select(0, text.Text.Length);
        }

        private void nudFontSize_Leave(object sender, EventArgs e)
        {
            UpDownBase text = nudFontSize;

            if (string.IsNullOrEmpty(text.Text))
            {
                nudFontSize.UpButton();
                nudFontSize.DownButton();
            }
        }

        void GoTo()
        {
            _blobGotoForm.SetMaxByteIndex(hexBox.ByteProvider.Length - 1);
            _blobGotoForm.SetDefaultValue(hexBox.SelectionStart);

            if (_blobGotoForm.ShowDialog() == DialogResult.OK)
            {
                hexBox.SelectionStart = _blobGotoForm.GetByteIndex();
                hexBox.SelectionLength = 1;
                hexBox.Focus();
            }
        }

        private void hexBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Next: //PageDown
                case Keys.PageUp:
                    {
                        break;
                    }
                case Keys.Home: //20230902 Home/End 特殊處理
                case Keys.End:
                    {
                        e.SuppressKeyPress = true;

                        var quotient = hexBox.SelectionStart / 16; //取商數

                        if (e.KeyData == Keys.Home)
                        {
                            hexBox.SelectionStart = quotient * 16;
                        }
                        else
                        {
                            hexBox.SelectionStart = quotient * 16 + 15;

                            if (hexBox.SelectionStart > hexBox.ByteProvider.Length - 1)
                            {
                                hexBox.SelectionStart = hexBox.ByteProvider.Length - 1;
                            }
                        }

                        hexBox.SelectionLength = 1;
                        hexBox.Focus();

                        break;
                    }
                default:
                    {
                        if (MyLibrary.IsBlobReadOnly)
                        {
                            //20230831 唯讀處理
                            switch (e.KeyData)
                            {
                                case Keys.Back:
                                    {
                                        e.SuppressKeyPress = true;
                                        SendKeys.SendWait("{LEFT}"); //左移一格
                                        break;
                                    }
                                default:
                                    {
                                        if (e.Control && e.KeyCode == Keys.Home || e.Control && e.KeyCode == Keys.End)
                                        {
                                            //允許！
                                        }
                                        else if (e.Shift && e.KeyCode == Keys.Up || e.Shift && e.KeyCode == Keys.Down || e.Shift && e.KeyCode == Keys.Left || e.Shift && e.KeyCode == Keys.Right)
                                        {
                                            //允許！
                                        }
                                        else
                                        {
                                            e.SuppressKeyPress = true;
                                        }

                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            switch (keyData)
            {
                case Keys.Escape:
                    {
                        Close();
                        return true;
                    }
                case Keys.Control | Keys.O: //Ctrl+O
                    {
                        OpenFile();
                        return true;
                    }
                case Keys.Control | Keys.S: //Ctrl+S
                    {
                        if (btnSave.Enabled)
                        {
                            SaveFile();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.Home:
                    {
                        hexBox.SelectionStart = 0;
                        hexBox.SelectionLength = 1;
                        hexBox.Focus();
                        return true;
                    }
                case Keys.Control | Keys.End:
                    {
                        hexBox.SelectionStart = hexBox.ByteProvider.Length - 1;
                        hexBox.SelectionLength = 1;
                        hexBox.Focus();
                        return true;
                    }
                case Keys.Control | Keys.G:
                    {
                        GoTo();
                        return true;
                    }
                case Keys.Control | Keys.A:
                    {
                        hexBox.SelectAll();
                        return true;
                    }
                case Keys.F12:
                    {
                        btnSaveAs.PerformClick();
                        return true;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static class MapColumn
        {
            public const int Cut = 0;
            public const int Copy = 1;
            public const int Paste = 2;
            public const int Dash1 = 3;
            public const int CopyHex = 4;
            public const int PasteHex = 5;
            public const int Dash2 = 6;
            public const int GoTo = 7;
            public const int Dash3 = 8;
            public const int SelectAll = 9;
        }
    }
}
