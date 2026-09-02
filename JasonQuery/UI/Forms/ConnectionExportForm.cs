using C1.C1Excel;
using C1.C1Zip;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.Legacy;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class ConnectionExportForm : Form
    {
        public DataTable dtDbInfo { get; set; }

        public ConnectionExportForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                //20260726 統一圖示風格
                btnSelectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnUnselectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Unselect All 16x16.ico");

                LocalizationHelper.ApplyLanguageInfo(this);

                //20260726 變更前景顏色
                lblCaution.ForeColor = Color.DarkRed;
                lblRemember.ForeColor = Color.DarkRed;

                txtFileName.Location = new Point(lblFileName.Left + lblFileName.Width, txtFileName.Top);
                btnBrowseFile.Location = new Point(txtFileName.Left + txtFileName.Width + 5, btnBrowseFile.Top);
                txtEncryptPassword.Location = new Point(lblPassword.Left + lblPassword.Width, txtEncryptPassword.Top);
                btnEncryptPasswordView.Location = new Point(txtEncryptPassword.Left + txtEncryptPassword.Width + 5, btnEncryptPasswordView.Top);
                btnHelp_Password.Location = new Point(btnEncryptPasswordView.Left + btnEncryptPasswordView.Width + 5, btnHelp_Password.Top);
                chkIncludeDBPassword.Location = new Point(chkEncrypt.Left + chkEncrypt.Width + 15, chkIncludeDBPassword.Top);

                var CheckHideFields = new HashSet<string>
                {
                    "PID", "DirectMode", "SID", "ConnectAs", "Database", "DomainUser", "Password", "TabBackColor", "TabActiveForeColor", "TabInactiveForeColor", "Unicode", "AutoRollback", "Pooling", "ExcludeNativeDatabases", "DatabaseFile", "DatabaseType", "WithPassword", "QueryTimeout"
                };

                c1GridDbInfo.VisualStyle = VisualStyle.Office2010Blue;
                c1GridDbInfo.DataSource = CreateColumnTable(dtDbInfo, "1");
                c1GridDbInfo.Refresh();

                foreach (C1DisplayColumn col in c1GridDbInfo.Splits[0].DisplayColumns)
                {
                    var name = col.Name;

                    if (CheckHideFields.Contains(name))
                    {
                        col.Visible = false;
                        col.Frozen = true;
                    }
                    else if (name == "Remarks")
                    {
                        col.Width = 150;
                    }
                    else
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

                    col.Style.VerticalAlignment = AlignVertEnum.Center;
                }

                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridDbInfo, "ConnectionForm", true);
                SetCheckBox(" ");
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                c1GridDbInfo.RowHeight = 20;
                c1GridDbInfo.Splits[0].ColumnCaptionHeight = 20;
                c1GridDbInfo.Refresh();
            }
        }

        private static DataTable CreateColumnTable(DataTable dtTemp, string sValue)
        {
            var dt = new DataTable();

            dt.Columns.Add(" ");

            for (var i = 0; i < dtTemp.Columns.Count; i++)
            {
                dt.Columns.Add(dtTemp.Columns[i].ColumnName, typeof(string));
            }

            for (var i = 0; i < dtTemp.Rows.Count; i++)
            {
                var row = dt.NewRow();

                row[" "] = string.IsNullOrEmpty(sValue) ? dtTemp.Rows[i].GetSafeString(" ") : sValue;

                for (var j = 0; j < dtTemp.Columns.Count; j++)
                {
                    row[j + 1] = dtTemp.Rows[i].GetSafeString(j);
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private void SetCheckBox(string sColumn)
        {
            var items = c1GridDbInfo.Columns[sColumn].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.CheckBox;
            items.Values.Clear();
            items.Values.Add(new ValueItem("0", false)); //unchecked
            items.Values.Add(new ValueItem("1", true)); //checked

            c1GridDbInfo.Splits[0].DisplayColumns[" "].Style.HorizontalAlignment = AlignHorzEnum.Center;

            foreach (C1DisplayColumn col in c1GridDbInfo.Splits[0].DisplayColumns)
            {
                col.AutoSize();

                if (col.Name == " ")
                {
                    col.AllowSizing = false;
                    col.Locked = false;
                }
                else
                {
                    col.Locked = true;
                }
            }
        }

        private void txtEncryptPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V || e.Control && e.KeyCode == Keys.Space || e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                btnExport.PerformClick();
            }
        }

        private void txtEncryptPassword_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var text = txtEncryptPassword.Text;
                var count = txtEncryptPassword.Text.Length - 1;

                for (var i = count; i >= 0; i--)
                {
                    if (!TextHelper.IsEngAlphabetOrNumberOrSpecialCharacters(text.Substring(i, 1), " "))
                    {
                        txtEncryptPassword.Text = text.Replace(text.Substring(i, 1), string.Empty);
                        txtEncryptPassword.SelectionStart = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                Filter = @"JasonQuery file (*.jqc)|*.jqc".Replace("JasonQuery file", LocalizationHelper.GetLanguageString("JasonQuery file", "Global", "Global", "msg", "JasonQueryFile", "Text"))
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
            {
                sf.FileName += ".jqc";
            }
            else if (Path.GetExtension(sf.FileName) != ".jqc")
            {
                var directoryName = Path.GetDirectoryName(sf.FileName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sf.FileName);

                sf.FileName = $"{directoryName}\\{fileNameWithoutExtension}.jqc";
            }

            txtFileName.Text = sf.FileName;
            txtEncryptPassword.Focus();
        }

        private void chkEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkEncrypt.Checked)
            {
                chkEncrypt.Checked = true;
            }

            txtEncryptPassword.Focus();
        }

        private void btnHelp_Password_Click(object sender, EventArgs e)
        {
            //Available characters for password.
            var message = LocalizationHelper.GetLanguageString("A valid password can contain the following characters:", "form", GetType().Name, "msg", "PasswordHelp1", "Text") + "\r\n\r\n";

            message += LocalizationHelper.GetLanguageString("Lowercase characters a-z", "form", GetType().Name, "msg", "PasswordHelp2", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Uppercase characters A-Z", "form", GetType().Name, "msg", "PasswordHelp3", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Numbers 0-9", "form", GetType().Name, "msg", "PasswordHelp4", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Special characters", "form", GetType().Name, "msg", "PasswordHelp5", "Text").Trim() + " `~!@#$%^&*()_-+=[{]}|\\;:'\",<.>/?";

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var message = string.Empty;

                if (string.IsNullOrEmpty(txtFileName.Text))
                {
                    message = LocalizationHelper.GetLanguageString("Please select the file name to export!", "form", GetType().Name, "msg", "NoneExportToFileName", "Text");
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    btnBrowseFile.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtEncryptPassword.Text))
                {
                    message = LocalizationHelper.GetLanguageString("Please enter password.", "form", GetType().Name, "msg", "NonePassword", "Text");
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtEncryptPassword.Focus();
                    return;
                }

                var count = 0;

                for (var i = 0; i < c1GridDbInfo.RowCount; i++)
                {
                    var value = c1GridDbInfo[i, " "].ToString();

                    if (value == "1")
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    message = LocalizationHelper.GetLanguageString("You must select at least one connection information!", "form", GetType().Name, "msg", "SelectOne", "Text");
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                var book = new C1XLBook();
                var sheet = book.Sheets[0];
                var excelRowIndex = 0;
                var dtData = c1GridDbInfo.GetDataTableSourceOrNull();

                for (var row = 0; row < dtData.Rows.Count; row++)
                {
                    var dr = dtData.Rows[row];
                    var value = dr.GetSafeString(" ");

                    if (value == "1") //20230930 改為有勾選的才要匯出
                    {
                        var col = 0;

                        //sheet[iRowIndex, iCol].Value = "DomainUser"; //保持空值
                        //iCol++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("ConnectionName");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("DataSource");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Server");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("SID");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("DirectMode");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Database");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("ConnectAs");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Port");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("UserID");
                        col++;

                        if (chkIncludeDBPassword.Checked)
                        {
                            var password = dr.GetSafeString("Password");
                            var passwordResult = TextEngine.Decrypt(TextEngine.Decode(password), MyGlobal.DomainUser);

                            sheet[excelRowIndex, col].Value = passwordResult;
                        }
                        else
                        {
                            sheet[excelRowIndex, col].Value = string.Empty;
                        }

                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("AutoRollback");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Unicode");
                        //iCol++;
                        //sheet[iRowIndex, iCol].Value = dr.GetSafeString("LastConnect"); //保持空值
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("TabBackColor");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("TabActiveForeColor");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("TabInactiveForeColor");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Remarks");
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("Pooling"); //O1
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("ExcludeNativeDatabases"); //O2
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("QueryTimeout"); //O3
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("DatabaseFile"); //O4
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("DatabaseType"); //O5
                        col++;
                        sheet[excelRowIndex, col].Value = dr.GetSafeString("WithPassword"); //O6

                        //20230930 以下欄位，後續有使用到，再依實際情況新增
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O7");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O8");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O9");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O10");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O11");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O12");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O13");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O14");
                        //col++;
                        //sheet[excelRowIndex, col].Value = dr.GetSafeString("O15");

                        excelRowIndex++;
                    }
                }

                var fileName = Path.GetTempFileName().Replace(".tmp", ".xls");

                book.Save(fileName);

                var zip = new C1ZipFile();

                zip.Create(txtFileName.Text);
                zip.UseUtf8Encoding = true;
                zip.Password = LegacyConnectionExportSecurity.CreateArchivePassword(txtEncryptPassword.Text);
                zip.CompressionLevel = CompressionLevelEnum.BestCompression;
                zip.Comment = "Connection Information - Exported by JasonQuery";
                zip.Entries.Add(fileName);
                zip.Close();
                File.Delete(fileName);
                File.Delete(fileName.Replace(".xls", ".tmp"));

                byte[] inData = null;

                TextEngine.BinRead(txtFileName.Text, ref inData);
                inData[0] = 8; //50
                inData[1] = 7; //4B
                inData[2] = 0; //03
                inData[3] = 1; //04
                inData[4] = 1; //14
                TextEngine.WriteBinaryFile(txtFileName.Text, inData);

                message = LocalizationHelper.GetLanguageString("All the connection information you specified has been exported to", "form", GetType().Name, "msg", "ExportOK", "Text");
                MessageBox.Show($"{message}\r\n{txtFileName.Text}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnEncryptPasswordView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                txtEncryptPassword.PasswordChar = '\0';
            }
        }

        private void btnEncryptPasswordView_MouseUp(object sender, MouseEventArgs e)
        {
            txtEncryptPassword.PasswordChar = '*';
            txtEncryptPassword.Focus();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            try
            {
                var value = sender is C1.Win.C1Input.C1Button btn && TextHelper.GetSafeString(btn.Tag) == "SelectAll" ? "1" : "0";
                var currentRow = c1GridDbInfo.Row;
                var currentCol = c1GridDbInfo.Col;
                var count = c1GridDbInfo.RowCount - 1;

                for (var i = count; i >= 0; i--)
                {
                    c1GridDbInfo[i, 0] = value;
                }

                c1GridDbInfo.Row = currentRow;
                c1GridDbInfo.Col = currentCol;
                c1GridDbInfo.Select(); //Focus 切換到指定的 Cell
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
