using C1.C1Excel;
using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Export;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Schema;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JasonQuery.UI.Forms
{
    public partial class ExportToFileForm : Form
    {
        private bool _busy = false;
        private bool _progressCancel = false;
        private const int ExportPreviewRowCount = 5;

        private const int Excel2003MaxRows = 65536;
        private const int Excel2003MaxColumns = 256;
        private const int Excel2007MaxRows = 1048576;
        private const int Excel2007MaxColumns = 16384;

        //避免大量資料匯出時，UI 每個 cell 都 DoEvents
        private const int ExcelUiRefreshCellInterval = 1000;

        //AutoFit 不掃完整資料，避免大量資料或 LargeText 拖慢匯出
        private const int ExcelAutoSizeDataRowSampleLimit = 2000;

        //避免 CLOB / LargeText 把欄寬撐爆
        private const int ExcelAutoSizeMaxPixelWidth = 500;

        public DataTable dtData { get; set; }
        public DataTable dtSchemaTable { get; set; }
        public string FontName { get; set; } = string.Empty;
        public float FontSize { get; set; } = 11;
        public string SheetName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsTopMost { get; set; } = false;
        public ColumnInfoCollector ColumnInfoCollector2 { get; set; }

        public ExportToFileForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                if (MyLibrary.IsDarkMode)
                {
                    editorPreview.SetSelectionBackColor(true, ColorTranslator.FromHtml("#ADD8E6"));
                    editorPreview.CaretLineBackColor = ColorTranslator.FromHtml("#FFFFE0");
                }
                else
                {
                    editorPreview.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                    editorPreview.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                }

                //Begin: 設定 ComboBox Color
                var colorType = typeof(Color);
                var propInfoList = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);

                var allowedLightColors = new HashSet<string>
                {
                    "LightBlue", "LightGreen", "LightSalmon", "Plum", "LightSteelBlue", "LightGoldenrodYellow", "LightSlateGray"
                };

                var disallowedColors = new HashSet<string>
                {
                    "YellowGreen", "Yellow", "Gold", "Orange", "Cyan", "White", "Blue", "Green"
                };

                foreach (var c in propInfoList)
                {
                    if ((!c.Name.StartsWith("Light", StringComparison.Ordinal) || allowedLightColors.Contains(c.Name)) && !disallowedColors.Contains(c.Name))
                    {
                        continue;
                    }

                    cboHeadingBackColor.Items.Add(c.Name);
                    cboEvenRowBackColor.Items.Add(c.Name);
                    cboOddRowBackColor.Items.Add(c.Name);
                }
                //End: 設定 ComboBox Color

                LocalizationHelper.ApplyLanguageInfo(this, false, false);

                Text += string.IsNullOrEmpty(Title) ? string.Empty : $" - {Title}";

                CreateVisualStyleInfo();

                FontName = FontName;
                FontSize = FontSize;

                cboFileName.Location = new Point(lblFileName.Left + lblFileName.Width, cboFileName.Top);
                btnBrowseFile.Location = new Point(cboFileName.Left + cboFileName.Width + 5, btnBrowseFile.Top);
                cboSaveAsType.Location = new Point(lblSaveAsType.Left + lblSaveAsType.Width, cboSaveAsType.Top);
                txtWorksheetName.Location = new Point(lblWorksheetName.Left + lblWorksheetName.Width, txtWorksheetName.Top);
                lblEncoding.Location = new Point(cboSaveAsType.Left + cboSaveAsType.Width + 50, lblEncoding.Top);
                cboEncoding.Location = new Point(lblEncoding.Left + lblEncoding.Width, cboEncoding.Top);
                lblCSVDelimiters.Location = new Point(cboEncoding.Left + cboEncoding.Width + 50, lblCSVDelimiters.Top);
                cboCSVDelimiters.Location = new Point(lblCSVDelimiters.Left + lblCSVDelimiters.Width, cboCSVDelimiters.Top);
                chkConvertCRLF.Location = new Point(cboEncoding.Left + cboEncoding.Width + 50, chkConvertCRLF.Top);
                rdoCustom.Location = new Point(rdoDefault.Left + rdoDefault.Width + 20, rdoCustom.Top);
                chkColumnResize.Location = new Point(chkAutoOpenExportedFile.Left + chkAutoOpenExportedFile.Width + 30, chkColumnResize.Top);
                cboHeadingBackColor.Location = new Point(lblHeadingBackColor.Left + lblHeadingBackColor.Width, cboHeadingBackColor.Top);
                cboEvenRowBackColor.Location = new Point(lblEvenRowBackColor.Left + lblEvenRowBackColor.Width, cboEvenRowBackColor.Top);
                cboOddRowBackColor.Location = new Point(lblOddRowBackColor.Left + lblOddRowBackColor.Width, cboOddRowBackColor.Top);
                cboGridFontName.Location = new Point(lblFontName.Left + lblFontName.Width, cboGridFontName.Top);
                cboGridFontSize.Location = new Point(lblFontSize.Left + lblFontSize.Width, cboGridFontSize.Top);
                cboGridRowHeight.Location = new Point(lblRowHeight.Left + lblRowHeight.Width, cboGridRowHeight.Top);

                LoadFileNameList();

                cboSaveAsType.Text = MyLibrary.GridExcelSaveAsType;
                txtWorksheetName.Text = string.IsNullOrEmpty(SheetName) ? MyLibrary.GridExcelWorksheetName : SheetName;
                chkConvertCRLF.Checked = MyLibrary.GridConvertCRLF;
                chkAutoOpenExportedFile.Checked = MyLibrary.GridExcelAutoOpen;
                chkColumnResize.Checked = MyLibrary.GridExcelAutoColumnResize;

                UIHelper.SetC1ComboBoxItemsFromDictionary(cboCSVDelimiters, MyGlobal.dicCsvDelimiters);

                cboCSVDelimiters.Text = MyGlobal.CsvDelimiters;
                cboEncoding.Text = MyLibrary.GridEncoding;

                if (cboSaveAsType.Text.StartsWith("Excel", StringComparison.Ordinal) && dtData.Rows.Count > 65535)
                {
                    cboSaveAsType.SelectedIndex = 1;
                }

                cboFileName.Text = MyLibrary.GridExcelFileName;
                cboFileName.Tag = cboFileName.Text;

                rdoCustom.Checked = true;

                cboHeadingBackColor.Invalidate();
                cboEvenRowBackColor.Invalidate();
                cboOddRowBackColor.Invalidate();

                cboHeadingBackColor.Text = MyLibrary.GridExcelHeadingBackColor;
                cboEvenRowBackColor.Text = MyLibrary.GridExcelEvenRowBackColor;
                cboOddRowBackColor.Text = MyLibrary.GridExcelOddRowBackColor;

                cboGridFontName.Text = MyLibrary.GridExcelFontName;
                cboGridFontSize.Text = MyLibrary.GridExcelFontSize;
                cboGridRowHeight.Text = MyLibrary.GridExcelRowHeight;

                ApplyVisualStyle();

                TopMost = IsTopMost;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            Close();
        }

        private void txtWorksheetName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWorksheetName.Text))
            {
                txtWorksheetName.Text = @"data";
            }
        }

        private void CreateVisualStyleInfo()
        {
            var nullShowAs = string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs;

            c1GridVisualStyle.DataSource = dtData;
            c1GridVisualStyle.Splits[0].ColumnCaptionHeight = 25;
            c1GridVisualStyle.Splits[0].RecordSelectors = false; //20230824 不顯示最左側的指示條
            c1GridVisualStyle.RowHeight = 25;

            //Grid's 選取顏色
            c1GridVisualStyle.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridVisualStyle.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);

            c1GridVisualStyle.HeadingStyle.ForeColor = ColorTranslator.FromHtml("#000000");

            foreach (C1DisplayColumn col in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 500;
                }

                if (col.Width > 500)
                {
                    col.Width = 500;
                }
            }

            //變更 Cell = NULL 的前景顏色 (不能使用 FetchCellStyle 事件，因為會變成整列都變色)
            if (string.IsNullOrWhiteSpace(nullShowAs))
            {
                return;
            }

            var colorNull = new Style
            {
                ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
            };

            for (var i = 0; i < c1GridVisualStyle.Columns.Count; i++)
            {
                c1GridVisualStyle.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, nullShowAs);
            }
        }

        private void btnExportAndClose_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cboFileName.Text))
            {
                var languageText = LocalizationHelper.GetLanguageString("Please select the name of the exported file!", "form", GetType().Name, "msg", "SelectSavedFileName", "Text");

                MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnBrowseFile.Focus();
                return;
            }

            btnExport.PerformClick();
            Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            try
            {
                ExportToFile(false);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ExportToFile(bool isPreview = false)
        {
            if (isPreview)
            {
                if (cboSaveAsType.Text.StartsWith("Excel", StringComparison.Ordinal))
                {
                    c1GridVisualStyle.Visible = true;
                    editorPreview.Visible = false;
                    return;
                }

                c1GridVisualStyle.Visible = false;
                editorPreview.Visible = true;
            }

            _progressCancel = false;

            var previewResult = string.Empty;
            var step = string.Empty;
            var isExportResult = true;
            var path = string.Empty;
            var saveAsType = cboSaveAsType.Text.Substring(0, cboSaveAsType.Text.IndexOf(' '));
            var languageText = string.Empty;

            lblInfo.Text = string.Empty;
            lblInfo.ForeColor = Color.Green;

            switch (isPreview)
            {
                case false when string.IsNullOrWhiteSpace(cboFileName.Text):
                    {
                        languageText = LocalizationHelper.GetLanguageString("Please select the name of the exported file!", "form", GetType().Name, "msg", "SelectSavedFileName", "Text");

                        MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnBrowseFile.Focus();
                        return;
                    }
                case false:
                    {
                        try
                        {
                            path = Path.GetDirectoryName(cboFileName.Text); //路徑不存在不會有錯誤；路徑不合法才會引發 catch 錯誤
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{MyGlobal.AnErrorHasOccurred}\r\n\r\n{ex.Message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnBrowseFile.Focus();
                            return;
                        }

                        break;
                    }
            }

            switch (isPreview)
            {
                case false when string.IsNullOrEmpty(path):
                    {
                        languageText = LocalizationHelper.GetLanguageString("Invalid path!", "form", GetType().Name, "msg", "InvalidPath", "Text");
                        MessageBox.Show($"{languageText}\r\n\r\n{cboFileName.Text}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnBrowseFile.Focus();
                        return;
                    }
                case false when File.Exists(cboFileName.Text):
                    {
                        var temp = LocalizationHelper.GetLanguageString("Confirm Save As", "Global", "Global", "msg", "ConfirmSaveAs", "Text");

                        languageText = LocalizationHelper.GetLanguageString("The file you are trying to save already exists!", "Global", "Global", "msg", "TheFileExists", "Text");
                        languageText += $"\r\n{cboFileName.Text}\r\n\r\n";
                        languageText += LocalizationHelper.GetLanguageString("Do you want to replace it?", "Global", "Global", "msg", "ReplaceFile", "Text");

                        if (MessageBox.Show(languageText, temp, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            return;
                        }

                        break;
                    }
            }

            if (!isPreview)
            {
                try
                {
                    step = "CreateFolder";
                    Directory.CreateDirectory(path ?? string.Empty);
                    step = "SaveFile";

                    TextEngine.WriteContentToFile(string.Empty, cboFileName.Text, TextEncodes.Default);
                    File.Delete(cboFileName.Text);
                }
                catch (Exception ex)
                {
                    languageText = LocalizationHelper.GetLanguageString("An error occurred while saving {FileType} file.", "Global", "Global", "msg", "ErrorToSaveFile", "Text");
                    languageText = languageText.Replace("{FileType}", saveAsType);

                    var temp = step == "CreateFolder" ? string.Empty : $"\r\n\r\n{MyGlobal.PleaseTryAgain}";

                    MessageBox.Show($"{languageText}\r\n\r\n{ex.Message}{temp}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            Application.UseWaitCursor = true;
            _busy = true;

            if (!isPreview)
            {
                UpdateFileNameList(); //以上都沒有錯誤，更新 ComboBox 下拉清單
            }

            if (!cboSaveAsType.Text.StartsWith("Excel", StringComparison.Ordinal))
            {
                if (!isPreview)
                {
                    btnCancel.Visible = true;

                    progressBar1.Visible = true;
                    progressBar1.Value = 0;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = dtData.Rows.Count;
                }

                try
                {
                    if (cboSaveAsType.Text.StartsWith("CSV", StringComparison.Ordinal))
                    {
                        if (!ExportToCsv(isPreview))
                        {
                            return;
                        }
                    }
                    else if (cboSaveAsType.Text.StartsWith("PDF", StringComparison.Ordinal))
                    {
                        c1GridVisualStyle.ExportToPDF(cboFileName.Text);
                    }
                    else if (cboSaveAsType.Text.StartsWith("JSON", StringComparison.Ordinal))
                    {
                        if (!ExportToJson(isPreview))
                        {
                            return;
                        }
                    }
                    else if (cboSaveAsType.Text.StartsWith("XML", StringComparison.Ordinal))
                    {
                        if (!ExportToXml(isPreview))
                        {
                            return;
                        }
                    }
                    else if (cboSaveAsType.Text.StartsWith("HTML", StringComparison.Ordinal))
                    {
                        c1GridVisualStyle.ExportToHTML(cboFileName.Text);
                    }
                    else if (cboSaveAsType.Text.StartsWith("RTF", StringComparison.Ordinal))
                    {
                        c1GridVisualStyle.ExportToRTF(cboFileName.Text);
                    }
                }
                catch (Exception ex)
                {
                    languageText = LocalizationHelper.GetLanguageString("An error occurred while saving {FileType} file.", "Global", "Global", "msg", "ErrorToSaveFile", "Text");
                    languageText = languageText.Replace("{FileType}", saveAsType);

                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message, true);

                    MessageBox.Show($"{languageText}\r\n\r\n{message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                finally
                {
                    Application.UseWaitCursor = false;
                    _busy = false;
                    progressBar1.Visible = false;
                    btnCancel.Visible = false;
                }

                if (isPreview)
                {
                    lblInfo.Text = string.Empty;
                    return;
                }

                languageText = LocalizationHelper.GetLanguageString("All data has been exported.", "form", GetType().Name, "msg", "ExportOK", "Text");
                lblInfo.Tag = languageText;
                lblInfo.Text = languageText;

                if (!chkAutoOpenExportedFile.Checked)
                {
                    return;
                }

                try
                {
                    Process.Start(cboFileName.Text);
                }
                catch (Exception ex)
                {
                    languageText = LocalizationHelper.GetLanguageString("An error occurred while opening {FileType} file.", "Global", "Global", "msg", "ErrorToOpenFile", "Text");
                    languageText = languageText.Replace("{FileType}", saveAsType);

                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show($"{languageText}\r\n\r\n{message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                return;
            }

            try
            {
                ExportToExcel(cboSaveAsType.Text == "Excel 2007 (*.xlsx)" ? "2007" : "2003", dtData, cboFileName.Text);
            }
            catch (Exception ex)
            {
                isExportResult = false;
                languageText = LocalizationHelper.GetLanguageString("An error occurred while saving {FileType} file.", "Global", "Global", "msg", "ErrorToSaveFile", "Text");
                languageText = languageText.Replace("{FileType}", saveAsType);

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show($"{languageText}\r\n\r\n{message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _busy = false;
                Application.UseWaitCursor = false;
            }

            btnCancel.Visible = false;
            progressBar1.Visible = false;

            if (_progressCancel)
            {
                return;
            }

            if (!isExportResult)
            {
                return;
            }

            languageText = chkAutoOpenExportedFile.Checked ? LocalizationHelper.GetLanguageString("All data has been exported and displayed in an Excel spreadsheet.", "form", GetType().Name, "msg", "ExportAndOpenByExcel", "Text") : LocalizationHelper.GetLanguageString("All data has been exported to Excel.", "form", GetType().Name, "msg", "ExportOK", "Text");
            lblInfo.Tag = languageText;
            lblInfo.Text = languageText;

            if (!chkAutoOpenExportedFile.Checked || _progressCancel)
            {
                return;
            }

            try
            {
                tmrExcelDetect.Enabled = true;
                lblExcelDetect.Text = " ";

                Process.Start(cboFileName.Text);

                //5 秒後 lblExcelDetect.Text 沒有變成空值，表示該 Excel 檔並沒有被成功地開啟 (可能狀況：1. Excel 處於「儲存格編輯模式」；2. Excel 跳出錯誤小視窗，但使用者未按下「確定」；3. Excel 檔案太大，開啟超過 5 秒)
                lblExcelDetect.Text = string.Empty;
                tmrExcelDetect.Enabled = false;
                lblInfo.ForeColor = Color.Green;
                lblInfo.Text = TextHelper.GetSafeString(lblInfo.Tag);
            }
            catch (Exception ex)
            {
                languageText = LocalizationHelper.GetLanguageString("An error occurred while opening {FileType} file.", "Global", "Global", "msg", "ErrorToOpenFile", "Text");
                languageText = languageText.Replace("{FileType}", saveAsType);

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show($"{languageText}\r\n\r\n{message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Enabled = true;
            }
        }

        private void VisualStyleChanged(object sender, EventArgs e)
        {
            ApplyVisualStyle();
        }

        private void ApplyVisualStyle()
        {
            if (rdoDefault.Checked)
            {
                c1GridVisualStyle.HeadingStyle.BackColor = Color.FromName("LightSkyBlue");
                c1GridVisualStyle.OddRowStyle.BackColor = Color.FromName("White");
                c1GridVisualStyle.EvenRowStyle.BackColor = Color.FromName("LightYellow");
            }
            else //custom
            {
                c1GridVisualStyle.HeadingStyle.BackColor = Color.FromName(cboHeadingBackColor.Text);
                c1GridVisualStyle.OddRowStyle.BackColor = Color.FromName(cboOddRowBackColor.Text);
                c1GridVisualStyle.EvenRowStyle.BackColor = Color.FromName(cboEvenRowBackColor.Text);
            }
        }

        private void btnSaveAllTheSettings_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            lblInfo.Text = string.Empty;

            try
            {
                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelFilename", cboFileName.Text);
                MyLibrary.GridExcelFileName = cboFileName.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelSaveAsType", cboSaveAsType.Text);
                MyLibrary.GridExcelSaveAsType = cboSaveAsType.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "CSVDelimiters", TextHelper.GetKeyFromDictionary(MyGlobal.dicCsvDelimiters, cboCSVDelimiters.Text));

                JasonQueryRepository.UpdateSetting("GridConfig", "ConvertCRLF", !chkConvertCRLF.Checked ? "0" : "1");
                MyLibrary.GridConvertCRLF = chkConvertCRLF.Checked;

                JasonQueryRepository.UpdateSetting("GridConfig", "Encoding", cboEncoding.Text);
                MyLibrary.GridEncoding = cboEncoding.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelWorksheetName", txtWorksheetName.Text);
                MyLibrary.GridExcelWorksheetName = txtWorksheetName.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelAutoOpen", !chkAutoOpenExportedFile.Checked ? "0" : "1");
                MyLibrary.GridExcelAutoOpen = chkAutoOpenExportedFile.Checked;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelAutoColumnResize", !chkColumnResize.Checked ? "0" : "1");
                MyLibrary.GridExcelAutoColumnResize = chkColumnResize.Checked;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelCustom", rdoCustom.Checked ? "1" : "0");
                MyLibrary.GridExcelCustom = rdoCustom.Checked;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelHeadingBackColor", cboHeadingBackColor.Text);
                MyLibrary.GridExcelHeadingBackColor = cboHeadingBackColor.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelEvenRowBackColor", cboEvenRowBackColor.Text);
                MyLibrary.GridExcelEvenRowBackColor = cboEvenRowBackColor.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelOddRowBackColor", cboOddRowBackColor.Text);
                MyLibrary.GridExcelOddRowBackColor = cboOddRowBackColor.Text;

                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelFontName", cboGridFontName.Text);
                MyLibrary.GridExcelFontName = cboGridFontName.Text;
                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelFontSize", cboGridFontSize.Text);
                MyLibrary.GridExcelFontSize = cboGridFontSize.Text;
                JasonQueryRepository.UpdateSetting("GridConfig", "ExcelRowHeight", cboGridRowHeight.Text);
                MyLibrary.GridExcelRowHeight = cboGridRowHeight.Text;

                lblInfo.Text = LocalizationHelper.GetLanguageString("All the settings have been saved!", "form", GetType().Name, "msg", "SaveAllTheSettings", "Text");
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            string ext;
            var sf = new SaveFileDialog();

            var languageText = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text");

            languageText += @" - " + LocalizationHelper.GetLanguageString("Export All Data to File", "form", GetType().Name, "menugrid", "ExportAllDataToFile", "Text");

            sf.Title = languageText;

            switch (cboSaveAsType.Text)
            {
                case "Excel 2007 (*.xlsx)":
                    {
                        ext = ".xlsx";
                        sf.Filter = @"Excel files (*.xlsx)|*.xlsx";
                        break;
                    }
                case "Excel 2003 (*.xls)":
                    {
                        ext = ".xls";
                        sf.Filter = @"Excel files (*.xls)|*.xls";
                        break;
                    }
                case "CSV (*.csv)":
                    {
                        ext = ".csv";
                        sf.Filter = @"CSV files (*.csv)|*.csv";
                        break;
                    }
                case "JSON (*.json)":
                    {
                        ext = ".json";
                        sf.Filter = @"JSON files (*.json)|*.json";
                        break;
                    }
                case "PDF (*.pdf)":
                    {
                        ext = ".pdf";
                        sf.Filter = @"PDF files (*.pdf)|*.pdf";
                        break;
                    }
                case "RTF (*.rtf)":
                    {
                        ext = ".rtf";
                        sf.Filter = @"RTF files (*.rtf)|*.rtf";
                        break;
                    }
                case "HTML (*.html)":
                    {
                        ext = ".html";
                        sf.Filter = @"HTML files (*.html)|*.html";
                        break;
                    }
                default:
                    {
                        ext = ".xml";
                        sf.Filter = @"XML files (*.xml)|*.xml";
                        break;
                    }
            }

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return; //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
            }

            if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
            {
                sf.FileName += ext;
            }

            cboFileName.Text = sf.FileName;
        }

        private void BackColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            rdoCustom.Checked = true;
            ApplyVisualStyle();
        }

        private void BackColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var rect = e.Bounds;

            if (e.Index < 0)
            {
                return;
            }

            var n = ((ComboBox)sender).Items[e.Index].ToString();
            var c = Color.FromName(n);
            Brush b = new SolidBrush(c);

            g.FillRectangle(b, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
        }

        private void cboFileName_Leave(object sender, EventArgs e)
        {
            var ext = TextHelper.GetStringBetween2(cboSaveAsType.Text, "*", ")", false);

            if (cboFileName.Text.Length <= 5)
            {
                return;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(cboFileName.Text)))
            {
                if (cboFileName.Text.EndsWith(".", StringComparison.Ordinal))
                {
                    cboFileName.Text += ext.Replace(".", string.Empty);
                }
                else
                {
                    var extTemp = Path.GetExtension(cboFileName.Text);
                    var temp = cboFileName.Text.Substring(0, cboFileName.Text.Length - extTemp.Length);

                    cboFileName.Text = $"{temp}{ext}";
                }

                return;
            }
        }

        private void UpdateFileNameList() //判斷是否要更新「檔案清單」
        {
            try
            {
                if (cboFileName.Items.Count > 0 && cboFileName.Text == cboFileName.Items[0].ToString())
                {
                    //檔名的是下拉清單的第一個，不用更新
                }
                else
                {
                    SaveFileNameList(cboFileName.Text);
                }

                cboFileName.Tag = cboFileName.Text;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SaveFileNameList(string sFileName)
        {
            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'FilenameList'");
                sbSql.Append($"   AND AttributeValue = '{sFileName.Replace("'", "''")}'");

                var sql = sbSql.ToString();
                var dtFileName = JasonQueryRepository.ExecQuery(sql);

                if (dtFileName?.Rows.Count > 0)
                {
                    sbSql.Clear();
                    sbSql.AppendLine("UPDATE SystemConfig");
                    sbSql.AppendLine($"    SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.AppendLine("   AND AttributeKey = 'FilenameList'");
                    sbSql.Append($"   AND AttributeValue = '{sFileName.Replace("'", "''")}'");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }
                else
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine("        (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                    sbSql.Append($" VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'FilenameList', '{sFileName.Replace("'", "''")}', '{MyGlobal.DateTimeNow()}')");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }

                LoadFileNameList();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void LoadFileNameList()
        {
            var i = 0;

            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'FilenameList'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtFileName = JasonQueryRepository.ExecQuery(sql);

                if (dtFileName.Rows.Count <= 0)
                {
                    return;
                }

                for (var row = 0; row < dtFileName.Rows.Count; row++)
                {
                    if (i > 20)
                    {
                        break;
                    }

                    cboFileName.Items.Add(dtFileName.Rows[row]["AttributeValue"].ToString());

                    i++;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tmrExcelDetect_Tick(object sender, EventArgs e)
        {
            if (lblExcelDetect.Text != " ")
            {
                return;
            }

            lblInfo.ForeColor = Color.DarkRed;
            lblInfo.Text = LocalizationHelper.GetLanguageString("Your Excel may be in \"cell-edit\" mode, so the file cannot be opened normally!", "form", GetType().Name, "msg", "CellEditMode", "Text");
        }

        private void cboGridFontName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float.TryParse(cboGridFontSize.Text, out var fontSize);

                if (fontSize == 0)
                {
                    fontSize = 12;
                }

                c1GridVisualStyle.Font = new Font(cboGridFontName.Text, fontSize, FontStyle.Regular, GraphicsUnit.Point);

                if (chkColumnResize.Checked)
                {
                    AutoSizeGrid();
                }

                c1GridVisualStyle.Refresh();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboGridFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                float.TryParse(cboGridFontSize.Text, out var fontSize);

                if (fontSize == 0)
                {
                    fontSize = 12;
                }

                c1GridVisualStyle.Font = new Font(cboGridFontName.Text, fontSize, FontStyle.Regular, GraphicsUnit.Point);

                if (chkColumnResize.Checked)
                {
                    AutoSizeGrid();
                }

                c1GridVisualStyle.Refresh();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboGridRowHeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            int.TryParse(cboGridRowHeight.Text, out var height);

            if (height == 0)
            {
                height = 12;
            }

            c1GridVisualStyle.Splits[0].ColumnCaptionHeight = height;
            c1GridVisualStyle.RowHeight = height;
        }

        private void AutoSizeGrid()
        {
            foreach (C1DisplayColumn col in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 500;
                }

                if (col.Width > 500)
                {
                    col.Width = 500;
                }
            }

            c1GridVisualStyle.Refresh();
        }

        private void cboCSVDelimiters_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var style = TextHelper.GetKeyFromDictionary(MyGlobal.dicCsvDelimiters, cboCSVDelimiters.Text);

                switch (style)
                {
                    case "Tab":
                        {
                            cboCSVDelimiters.Tag = "\t";
                            break;
                        }
                    case "Semicolon":
                        {
                            cboCSVDelimiters.Tag = ";";
                            break;
                        }
                    case "Comma":
                        {
                            cboCSVDelimiters.Tag = ",";
                            break;
                        }
                    case "Space":
                        {
                            cboCSVDelimiters.Tag = " ";
                            break;
                        }
                    case "Colon":
                        {
                            cboCSVDelimiters.Tag = ":";
                            break;
                        }
                    case "Slash":
                        {
                            cboCSVDelimiters.Tag = "/";
                            break;
                        }
                    case "Backslash":
                        {
                            cboCSVDelimiters.Tag = "\\";
                            break;
                        }
                    case "Pipe":
                        {
                            cboCSVDelimiters.Tag = "|";
                            break;
                        }
                    default:
                        {
                            cboCSVDelimiters.Tag = ",";
                            break;
                        }
                }

                ExportToFile(true);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SetSaveAsType()
        {
            var value = false;
            var fileExt = Path.GetExtension(cboFileName.Text);
            var saveAsExt = TextHelper.GetStringBetween2(cboSaveAsType.Text, "*", ")", false);

            lblCSVDelimiters.Visible = false;
            cboCSVDelimiters.Visible = false;
            lblEncoding.Visible = false;
            cboEncoding.Visible = false;
            lblWorksheetName.Visible = false;
            txtWorksheetName.Visible = false;
            cboEncoding.Enabled = true;
            chkConvertCRLF.Visible = false;
            c1GridVisualStyle.Visible = false;
            editorPreview.Visible = false;

            if (cboSaveAsType.Text.StartsWith("XML", StringComparison.Ordinal))
            {
                cboEncoding.Text = "UTF-8";
                cboEncoding.Enabled = false;
                lblEncoding.Visible = true;
                cboEncoding.Visible = true;
                chkConvertCRLF.Visible = true;
                editorPreview.Visible = true;
            }
            else if (cboSaveAsType.Text.StartsWith("JSON", StringComparison.Ordinal))
            {
                cboEncoding.Text = "UTF-8";
                cboEncoding.Enabled = false;
                lblEncoding.Visible = true;
                cboEncoding.Visible = true;
                editorPreview.Visible = true;
            }
            else if (cboSaveAsType.Text == "CSV (*.csv)")
            {
                if (string.IsNullOrEmpty(cboCSVDelimiters.Text))
                {
                    cboCSVDelimiters.SelectedIndex = 0;
                }

                if (string.IsNullOrEmpty(cboEncoding.Text))
                {
                    cboEncoding.SelectedIndex = 0;
                }

                lblCSVDelimiters.Visible = true;
                cboCSVDelimiters.Visible = true;
                lblEncoding.Visible = true;
                cboEncoding.Visible = true;
                editorPreview.Visible = true;
            }
            else
            {
                if (cboSaveAsType.Text.StartsWith("Excel", StringComparison.Ordinal))
                {
                    lblWorksheetName.Visible = true;
                    txtWorksheetName.Visible = true;
                }

                value = true;
                c1GridVisualStyle.Visible = true;
            }

            grpDataGrid.Enabled = value;
            chkColumnResize.Visible = value;

            try
            {
                ExportToFile(true);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            cboSaveAsType.Focus(); //先 Focus 到 ComBoBox，後面再切換到 Grid or Editor，避免切換失效
            Application.DoEvents();

            if (cboSaveAsType.Text.StartsWith("Excel", StringComparison.Ordinal))
            {
                c1GridVisualStyle.Focus();
            }
            else
            {
                editorPreview.Focus();
            }

            if (string.IsNullOrWhiteSpace(cboFileName.Text))
            {
                return;
            }

            cboFileName.Text = cboFileName.Text.Replace(fileExt, saveAsExt);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _progressCancel = true;
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyGlobal.ClearMemory();
        }

        private void chkConvertCRLF_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                ExportToFile(true);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chkColumnResize_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkColumnResize.Checked)
                {
                    AutoSizeGrid();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboSaveAsType_TextChanged(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }

            btnBrowseFile.Focus();

            try
            {
                SetSaveAsType();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private int GetExportRowCount(bool isPreview)
        {
            var rowCount = dtData?.Rows.Count ?? 0;

            return isPreview ? Math.Min(ExportPreviewRowCount, rowCount) : rowCount;
        }

        private void UpdateExportProgress(int value)
        {
            if (!progressBar1.Visible)
            {
                return;
            }

            if (value < progressBar1.Minimum)
            {
                value = progressBar1.Minimum;
            }

            if (value > progressBar1.Maximum)
            {
                value = progressBar1.Maximum;
            }

            progressBar1.Value = value;
        }

        private bool TryGetColumnInfo(string columnName, out ColumnInfo columnInfo)
        {
            columnInfo = null;

            return ColumnInfoCollector2 != null && ColumnInfoCollector2.TryGet(columnName, out columnInfo);
        }

        private void SetExportCancelledInfo()
        {
            lblInfo.Text = LocalizationHelper.GetLanguageString("This operation has been cancelled.", "Global", "Global", "msg", "CancelByUser", "Text");
        }

        private static StreamWriter CreateUtf8StreamWriter(string fileName)
        {
            return new StreamWriter(fileName, false, new UTF8Encoding(false));
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }

            public override Encoding Encoding => new UTF8Encoding(false);
        }

        #region 產生 CSV 檔案
        private bool ExportToCsv(bool isPreview)
        {
            var rowCount = GetExportRowCount(isPreview);
            var fieldSeparator = TextHelper.GetSafeString(cboCSVDelimiters.Tag);

            if (string.IsNullOrEmpty(fieldSeparator))
            {
                fieldSeparator = ",";
            }

            var sb = new StringBuilder();

            AppendCsvHeader(sb, fieldSeparator);

            if (!isPreview)
            {
                TextEngine.WriteContentToFile(sb.ToString(), cboFileName.Text, cboEncoding.Text, FileMode.Append);
                sb.Clear();
            }

            for (var row = 0; row < rowCount; row++)
            {
                if (_progressCancel)
                {
                    SetExportCancelledInfo();
                    return false;
                }

                var vr = c1GridVisualStyle.Splits[0].Rows[row];

                AppendCsvRow(sb, vr.DataRowIndex, fieldSeparator);

                if (!isPreview)
                {
                    TextEngine.WriteContentToFile(sb.ToString(), cboFileName.Text, cboEncoding.Text, FileMode.Append);
                    sb.Clear();
                }

                UpdateExportProgress(row + 1);
                Application.DoEvents();
            }

            if (isPreview)
            {
                editorPreview.Text = sb.ToString();
            }

            return true;
        }

        private void AppendCsvHeader(StringBuilder sb, string fieldSeparator)
        {
            var values = new List<string>();

            foreach (C1DataColumn column in c1GridVisualStyle.Columns)
            {
                values.Add(EscapeCsvField(column.DataField, fieldSeparator));
            }

            sb.AppendLine(string.Join(fieldSeparator, values));
        }

        private void AppendCsvRow(StringBuilder sb, int dataRowIndex, string fieldSeparator)
        {
            var values = new List<string>();

            foreach (C1DataColumn column in c1GridVisualStyle.Columns)
            {
                var cellValue = column.CellValue(dataRowIndex);
                var resolution = ExportCellValueResolver.Resolve(cellValue, column.CellText(dataRowIndex));

                values.Add(resolution.IsNull ? string.Empty : EscapeCsvField(resolution.Text, fieldSeparator));
            }

            sb.AppendLine(string.Join(fieldSeparator, values));
        }

        private static string EscapeCsvField(string value, string fieldSeparator)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var escaped = value.Replace("\"", "\"\"");
            var mustQuote = escaped.Contains("\"") || escaped.Contains("\r") || escaped.Contains("\n")
                            || (!string.IsNullOrEmpty(fieldSeparator) && escaped.Contains(fieldSeparator));

            return mustQuote ? $"\"{escaped}\"" : escaped;
        }
        #endregion 產生 CSV 檔案

        #region 產生 JSON 檔案
        private bool ExportToJson(bool isPreview)
        {
            var rowCount = GetExportRowCount(isPreview);
            var sb = isPreview ? new StringBuilder() : null;

            TextWriter textWriter;

            if (isPreview)
            {
                textWriter = new StringWriter(sb);
            }
            else
            {
                textWriter = CreateUtf8StreamWriter(cboFileName.Text);
            }

            using (textWriter)
            using (var writer = new JsonTextWriter(textWriter))
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                writer.Indentation = 2;
                writer.IndentChar = ' ';

                writer.WriteStartArray();

                for (var row = 0; row < rowCount; row++)
                {
                    if (_progressCancel)
                    {
                        SetExportCancelledInfo();
                        return false;
                    }

                    var vr = c1GridVisualStyle.Splits[0].Rows[row];

                    writer.WriteStartObject();

                    foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                    {
                        var columnName = column.DataField;
                        var cellValue = column.CellValue(vr.DataRowIndex);
                        var cellText = column.CellText(vr.DataRowIndex);

                        writer.WritePropertyName(columnName);
                        WriteJsonValue(writer, ExportCellValueResolver.Resolve(cellValue, cellText));
                    }

                    writer.WriteEndObject();

                    UpdateExportProgress(row + 1);
                    Application.DoEvents();
                }

                writer.WriteEndArray();
            }

            if (isPreview)
            {
                editorPreview.Text = sb.ToString();
            }

            return true;
        }

        private static void WriteJsonValue(JsonWriter writer, ExportCellValueResolution resolution)
        {
            if (resolution == null || resolution.IsNull)
            {
                writer.WriteNull();
                return;
            }

            var cellValue = resolution.Value;

            switch (cellValue)
            {
                case bool value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case byte value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case sbyte value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case short value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case ushort value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case int value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case uint value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case long value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case ulong value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case float value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case double value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case decimal value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case DateTime value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case DateTimeOffset value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case TimeSpan value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case Guid value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case byte[] value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                case char value:
                    {
                        writer.WriteValue(value);
                        return;
                    }
                default:
                    {
                        writer.WriteValue(resolution.Text);
                        return;
                    }
            }
        }
        #endregion 產生 JSON 檔案

        #region 產生 XML 檔案
        private bool ExportToXml(bool isPreview)
        {
            var rowCount = GetExportRowCount(isPreview);
            var sb = isPreview ? new StringBuilder() : null;

            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.None,
                CheckCharacters = true
            };

            TextWriter textWriter;

            if (isPreview)
            {
                textWriter = new Utf8StringWriter(sb);
            }
            else
            {
                textWriter = CreateUtf8StreamWriter(cboFileName.Text);
            }

            using (textWriter)
            using (var writer = XmlWriter.Create(textWriter, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("jasonQueryDataSet");
                writer.WriteAttributeString("source", "JasonQuery");
                writer.WriteAttributeString("version", "1.0");

                WriteXmlColumnMetadata(writer);
                WriteXmlRows(writer, rowCount);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            if (isPreview)
            {
                editorPreview.Text = sb.ToString();
            }

            return true;
        }

        private void WriteXmlColumnMetadata(XmlWriter writer)
        {
            writer.WriteStartElement("columns");

            var ordinal = 0;

            foreach (C1DataColumn column in c1GridVisualStyle.Columns)
            {
                var columnName = column.DataField;

                writer.WriteStartElement("column");
                writer.WriteAttributeString("ordinal", ordinal.ToString());
                writer.WriteAttributeString("name", RemoveInvalidXmlChars(columnName));

                if (TryGetColumnInfo(columnName, out var columnInfo))
                {
                    if (!string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
                    {
                        writer.WriteAttributeString("dataType", RemoveInvalidXmlChars(columnInfo.BaseDataType));
                    }

                    writer.WriteAttributeString("category", columnInfo.CategoryDataTypeKind.ToString());

                    if (columnInfo.IsArray)
                    {
                        writer.WriteAttributeString("isArray", "true");
                    }
                }

                writer.WriteEndElement();

                ordinal++;
            }

            writer.WriteEndElement();
        }

        private void WriteXmlRows(XmlWriter writer, int rowCount)
        {
            writer.WriteStartElement("rows");

            for (var row = 0; row < rowCount; row++)
            {
                if (_progressCancel)
                {
                    SetExportCancelledInfo();
                    return;
                }

                var vr = c1GridVisualStyle.Splits[0].Rows[row];

                writer.WriteStartElement("row");
                writer.WriteAttributeString("index", row.ToString());

                foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                {
                    var columnName = column.DataField;
                    var cellValue = column.CellValue(vr.DataRowIndex);
                    var resolution = ExportCellValueResolver.Resolve(cellValue, column.CellText(vr.DataRowIndex));

                    writer.WriteStartElement("column");
                    writer.WriteAttributeString("name", RemoveInvalidXmlChars(columnName));

                    if (TryGetColumnInfo(columnName, out var columnInfo))
                    {
                        if (!string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
                        {
                            writer.WriteAttributeString("dataType", RemoveInvalidXmlChars(columnInfo.BaseDataType));
                        }
                    }

                    if (resolution.IsNull)
                    {
                        writer.WriteAttributeString("isNull", "true");
                    }
                    else
                    {
                        WriteXmlText(writer, RemoveInvalidXmlChars(resolution.Text));
                    }

                    writer.WriteEndElement();

                    if (_progressCancel)
                    {
                        break;
                    }
                }

                writer.WriteEndElement();

                UpdateExportProgress(row + 1);
                Application.DoEvents();

                if (_progressCancel)
                {
                    SetExportCancelledInfo();
                    return;
                }
            }

            writer.WriteEndElement();
        }

        private void WriteXmlText(XmlWriter writer, string value)
        {
            if (!chkConvertCRLF.Checked)
            {
                writer.WriteString(value);
                return;
            }

            var escaped = EscapeXmlTextForRaw(value).Replace("\r", "&#xD;").Replace("\n", "&#xA;");

            writer.WriteRaw(escaped);
        }

        private static string EscapeXmlTextForRaw(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string RemoveInvalidXmlChars(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);
            var changed = false;

            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];

                if (char.IsHighSurrogate(ch) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    var codePoint = char.ConvertToUtf32(ch, value[i + 1]);

                    if (codePoint >= 0x10000 && codePoint <= 0x10FFFF)
                    {
                        sb.Append(ch);
                        sb.Append(value[i + 1]);
                        i++;
                        continue;
                    }

                    changed = true;
                    continue;
                }

                if (XmlConvert.IsXmlChar(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    changed = true;
                }
            }

            return changed ? sb.ToString() : value;
        }
        #endregion 產生 XML 檔案

        #region 產生 Excel 檔案

        private void ExportToExcel(string excelVersion, DataTable dt, string fileName)
        {
            if (dt == null)
            {
                throw new ArgumentNullException(nameof(dt));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The file name is empty.", nameof(fileName));
            }

            var isExcel2003 = string.Equals(excelVersion, "2003", StringComparison.OrdinalIgnoreCase);

            c1XLBook1.Author = "JasonQuery";
            c1XLBook1.CompatibilityMode = isExcel2003
                                          ? CompatibilityMode.Excel2003
                                          : CompatibilityMode.Excel2007;

            var sheet = c1XLBook1.Sheets[0];
            sheet.Name = GetSafeExcelWorksheetName(txtWorksheetName.Text);

            var maxColumns = isExcel2003 ? Excel2003MaxColumns : Excel2007MaxColumns;
            var maxRows = isExcel2003 ? Excel2003MaxRows : Excel2007MaxRows;

            var columnCount = Math.Min(dt.Columns.Count, maxColumns);
            var headerRowCount = GetExcelHeaderRowCount(dt, columnCount);

            //Excel 的 row limit 包含 header，所以資料列數要扣掉 headerRowCount
            var maxDataRowCount = Math.Max(0, maxRows - headerRowCount);
            var dataRowCount = Math.Min(dt.Rows.Count, maxDataRowCount);

            var styles = CreateExcelExportStyles();

            btnCancel.Visible = true;
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = Math.Max(1, dataRowCount);

            WriteExcelHeaders(sheet, dt, columnCount, headerRowCount, styles.Header);

            if (_progressCancel)
            {
                SetExcelExportCancelledInfo();
                return;
            }

            WriteExcelRows(sheet, dt, columnCount, dataRowCount, headerRowCount, styles);

            if (_progressCancel)
            {
                SetExcelExportCancelledInfo();
                return;
            }

            sheet.Rows.Frozen = headerRowCount;
            SetExcelProgressValue(dataRowCount);

            lblInfo.Text = LocalizationHelper.GetLanguageString("Saving file...", "Global", "Global", "msg", "SavingFile", "Text");
            lblInfo.Visible = true;
            lblInfo.Refresh();
            Application.DoEvents();

            if (chkColumnResize.Checked)
            {
                var usedRowCount = headerRowCount + dataRowCount;
                var rowsToMeasure = Math.Min(usedRowCount, headerRowCount + ExcelAutoSizeDataRowSampleLimit);

                AutoSizeColumns(sheet, columnCount, rowsToMeasure);

                if (_progressCancel)
                {
                    SetExcelExportCancelledInfo();
                    return;
                }
            }

            //不要在這裡 catch，讓外層 ExportToFile() 的 try/catch 接住
            c1XLBook1.Save(fileName);
        }

        private sealed class ExcelExportStyles
        {
            public XLStyle Header { get; set; }
            public XLStyle OddText { get; set; }
            public XLStyle EvenText { get; set; }
            public XLStyle OddNumber { get; set; }
            public XLStyle EvenNumber { get; set; }
        }

        private ExcelExportStyles CreateExcelExportStyles()
        {
            var colors = GetExcelExportColors();
            var font = CreateExcelExportFont();

            return new ExcelExportStyles
            {
                Header = CreateExcelStyle(font, colors.Heading, XLAlignHorzEnum.Left, true),
                OddText = CreateExcelStyle(font, colors.Odd, XLAlignHorzEnum.Left, false),
                EvenText = CreateExcelStyle(font, colors.Even, XLAlignHorzEnum.Left, false),
                OddNumber = CreateExcelStyle(font, colors.Odd, XLAlignHorzEnum.Right, false),
                EvenNumber = CreateExcelStyle(font, colors.Even, XLAlignHorzEnum.Right, false)
            };
        }

        private (string Heading, string Odd, string Even) GetExcelExportColors()
        {
            if (rdoCustom.Checked)
            {
                return(cboHeadingBackColor.Text, cboOddRowBackColor.Text, cboEvenRowBackColor.Text);
            }

            return("LightSkyBlue", "LightYellow", "White");
        }

        private Font CreateExcelExportFont()
        {
            var fontName = string.IsNullOrWhiteSpace(cboGridFontName.Text) ? FontName : cboGridFontName.Text;

            if (string.IsNullOrWhiteSpace(fontName))
            {
                fontName = "Arial";
            }

            if (!float.TryParse(cboGridFontSize.Text, out var fontSize) || fontSize <= 0)
            {
                fontSize = FontSize <= 0 ? 12 : FontSize;
            }

            return new Font(fontName, fontSize, FontStyle.Regular);
        }

        private XLStyle CreateExcelStyle(Font font, string backColor, XLAlignHorzEnum alignHorz, bool isHeader)
        {
            var style = new XLStyle(c1XLBook1)
            {
                AlignHorz = alignHorz,
                AlignVert = XLAlignVertEnum.Center,
                Font = font,
                ForeColor = Color.Black,
                BackColor = Color.FromName(backColor)
            };

            style.SetBorderColor(Color.Gray);
            style.BorderBottom = isHeader ? XLLineStyleEnum.Thin : XLLineStyleEnum.Hair;
            style.BorderTop = XLLineStyleEnum.Hair;
            style.BorderLeft = XLLineStyleEnum.Hair;
            style.BorderRight = XLLineStyleEnum.Hair;

            return style;
        }

        private void WriteExcelHeaders(XLSheet sheet, DataTable table, int columnCount, int headerRowCount, XLStyle headerStyle)
        {
            for (var c = 0; c < columnCount; c++)
            {
                var parts = GetExcelHeaderParts(table.Columns[c].ColumnName);

                for (var r = 0; r < headerRowCount; r++)
                {
                    sheet[r, c].Value = r < parts.Length ? parts[r] : string.Empty;
                    sheet[r, c].Style = headerStyle;
                }

                if (_progressCancel)
                {
                    break;
                }
            }
        }

        private void WriteExcelRows(XLSheet sheet, DataTable table, int columnCount, int dataRowCount, int headerRowCount, ExcelExportStyles styles)
        {
            var isNumericColumns = BuildNumericColumnFlags(table, columnCount);
            var cellCounter = 0;

            for (var r = 0; r < dataRowCount; r++)
            {
                var dataRow = table.Rows[r];
                var sheetRow = r + headerRowCount;

                for (var c = 0; c < columnCount; c++)
                {
                    sheet[sheetRow, c].Value = GetExcelCellValue(dataRow[c]);
                    sheet[sheetRow, c].Style = GetExcelCellStyle(r, isNumericColumns[c], styles);

                    cellCounter++;

                    if (cellCounter % ExcelUiRefreshCellInterval == 0)
                    {
                        Application.DoEvents();

                        if (_progressCancel)
                        {
                            break;
                        }
                    }
                }

                SetExcelProgressValue(r + 1);

                if ((r + 1) % 100 == 0 || r + 1 == dataRowCount)
                {
                    Application.DoEvents();
                }

                if (_progressCancel)
                {
                    break;
                }
            }
        }

        private bool[] BuildNumericColumnFlags(DataTable table, int columnCount)
        {
            var result = new bool[columnCount];

            for (var c = 0; c < columnCount; c++)
            {
                result[c] = IsNumericExcelColumn(table.Columns[c]);
            }

            return result;
        }

        private static XLStyle GetExcelCellStyle(int dataRowIndex, bool isNumericColumn, ExcelExportStyles styles)
        {
            //保留原本邏輯：第 1 筆資料列使用 Even style，第 2 筆使用 Odd style
            var useOddStyle = (dataRowIndex + 1) % 2 == 0;

            if (isNumericColumn)
            {
                return useOddStyle ? styles.OddNumber : styles.EvenNumber;
            }

            return useOddStyle ? styles.OddText : styles.EvenText;
        }

        private static object GetExcelCellValue(object value)
        {
            var resolution = ExportCellValueResolver.Resolve(value, Convert.ToString(value));

            return resolution.IsNull ? null : resolution.Value;
        }

        private static bool IsNumericExcelColumn(DataColumn column)
        {
            if (column == null)
            {
                return false;
            }

            var type = Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static int GetExcelHeaderRowCount(DataTable table, int columnCount)
        {
            var result = 1;

            for (var c = 0; c < columnCount; c++)
            {
                var parts = GetExcelHeaderParts(table.Columns[c].ColumnName);

                if (parts.Length > result)
                {
                    result = parts.Length;
                }
            }

            return result;
        }

        private static string[] GetExcelHeaderParts(string header)
        {
            var normalized = (header ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");

            return normalized.Split(new[] { '\n' }, StringSplitOptions.None);
        }

        private static string GetSafeExcelWorksheetName(string value)
        {
            var sheetName = string.IsNullOrWhiteSpace(value) ? "data" : value.Trim();

            var invalidChars = new HashSet<char>
    {
        ':', '\\', '/', '?', '*', '[', ']'
    };

            var sb = new StringBuilder(sheetName.Length);

            foreach (var ch in sheetName)
            {
                sb.Append(invalidChars.Contains(ch) ? '_' : ch);
            }

            var result = sb.ToString().Trim('\'');

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "data";
            }

            return result.Length <= 31 ? result : result.Substring(0, 31);
        }

        private void SetExcelProgressValue(int value)
        {
            if (!progressBar1.Visible)
            {
                return;
            }

            if (value < progressBar1.Minimum)
            {
                value = progressBar1.Minimum;
            }

            if (value > progressBar1.Maximum)
            {
                value = progressBar1.Maximum;
            }

            progressBar1.Value = value;
        }

        private void SetExcelExportCancelledInfo()
        {
            lblInfo.Text = LocalizationHelper.GetLanguageString("This operation has been cancelled.", "Global", "Global", "msg", "CancelByUser", "Text");
        }
        #endregion 產生 Excel 檔案

        private void AutoSizeColumns(XLSheet sheet, int columnCount, int rowCount)
        {
            try
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    for (var c = 0; c < columnCount; c++)
                    {
                        var colWidth = -1;

                        for (var r = 0; r < rowCount; r++)
                        {
                            var value = sheet[r, c].Value;

                            if (value == null)
                            {
                                continue;
                            }

                            var text = value.ToString();
                            var style = sheet[r, c].Style;

                            if (!string.IsNullOrEmpty(style?.Format) && value is IFormattable formattable)
                            {
                                var format = XLStyle.FormatXLToDotNet(style.Format);

                                text = formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
                            }

                            var font = c1XLBook1.DefaultFont;

                            if (style?.Font != null)
                            {
                                font = style.Font;
                            }

                            var size = Size.Ceiling(g.MeasureString(text + "XX", font));

                            if (size.Width > colWidth)
                            {
                                colWidth = size.Width;
                            }
                        }

                        if (colWidth > ExcelAutoSizeMaxPixelWidth)
                        {
                            colWidth = ExcelAutoSizeMaxPixelWidth;
                        }

                        if (colWidth > -1)
                        {
                            sheet.Columns[c].Width = C1XLBook.PixelsToTwips(colWidth);
                        }

                        if ((c + 1) % 10 == 0)
                        {
                            Application.DoEvents();

                            if (_progressCancel)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && !_busy)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
