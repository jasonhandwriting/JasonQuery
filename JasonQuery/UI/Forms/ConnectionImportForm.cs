using C1.C1Excel;
using C1.C1Zip;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class ConnectionImportForm : Form
    {
        public ConnectionImportForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this);

                //20260726 變更前景顏色
                lblRemember.ForeColor = Color.DarkRed;

                txtFileName.Location = new Point(lblFileName.Left + lblFileName.Width, txtFileName.Top);
                btnBrowseFile.Location = new Point(txtFileName.Left + txtFileName.Width + 5, btnBrowseFile.Top);
                txtEncryptPassword.Location = new Point(lblPassword.Left + lblPassword.Width, txtEncryptPassword.Top);
                btnEncryptPasswordView.Location = new Point(txtEncryptPassword.Left + txtEncryptPassword.Width + 5, btnEncryptPasswordView.Top);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtEncryptPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V || e.Control && e.KeyCode == Keys.Space || e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
            }

            //20241207
            if (e.KeyCode == Keys.Enter)
            {
                btnImport.PerformClick();
            }
        }

        private void txtEncryptPassword_TextChanged(object sender, EventArgs e)
        {
            var count = txtEncryptPassword.Text.Length - 1;

            for (var i = count; i >= 0; i--)
            {
                if (!TextHelper.IsEngAlphabetOrNumberOrSpecialCharacters(txtEncryptPassword.Text.Substring(i, 1), " "))
                {
                    txtEncryptPassword.Text = txtEncryptPassword.Text.Replace(txtEncryptPassword.Text.Substring(i, 1), string.Empty);
                    txtEncryptPassword.SelectionStart = i;
                    break;
                }
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            var sf = new OpenFileDialog
            {
                Multiselect = false,
                Title = LocalizationHelper.GetLanguageString("Open File", "Global", "Global", "msg", "OpenFile", "Text"),
                Filter = @"JasonQuery file (*.jqc)|*.jqc"
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            txtFileName.Text = sf.FileName;
            txtEncryptPassword.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            var message = string.Empty;

            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                message = LocalizationHelper.GetLanguageString("Please select the file name to import!", "form", GetType().Name, "msg", "NoneImportFromFileName", "Text");
                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnBrowseFile.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtEncryptPassword.Text))
            {
                message = LocalizationHelper.GetLanguageString("Please enter password.", "form", GetType().Name, "msg", "NonePassword", "Text");
                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtEncryptPassword.Focus();
                return;
            }

            var book = new C1XLBook();

            try
            {
                byte[] inData = null;

                TextEngine.BinRead(txtFileName.Text, ref inData);

                inData[0] = 80; //50
                inData[1] = 75; //4B
                inData[2] = 3; //03
                inData[3] = 4; //04
                inData[4] = 20; //14

                var fileNameZip = Path.GetTempFileName();
                var fileNameXls = Path.GetTempFileName();

                TextEngine.WriteBinaryFile(fileNameZip, inData);

                var zip = new C1ZipFile
                {
                    UseUtf8Encoding = true,
                    Password = $"{JasonQueryRepository.DbConnectionPasswordPrefix}{txtEncryptPassword.Text}{JasonQueryRepository.DbConnectionExportPasswordSuffix}"
                };

                var isOpenNG = false;

                try
                {
                    zip.Open(fileNameZip);
                    zip.Entries.Extract(0, fileNameXls);
                }
                catch (Exception)
                {
                    message = LocalizationHelper.GetLanguageString("Wrong password!", "form", "ConnectionForm", "msg", "WrongPassword", "Text") + "\r\n";
                    message += LocalizationHelper.GetLanguageString("You must re-enter your password.", "form", "ConnectionForm", "msg", "ReEnter", "Text");
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEncryptPassword.Focus();
                    isOpenNG = false;
                }
                finally
                {
                    zip.Close();
                }

                if (isOpenNG)
                {
                    return;
                }

                if (!File.Exists(fileNameXls))
                {
                    return;
                }

                var sbSql = new StringBuilder();

                book.Load(fileNameXls);

                var count = 0;

                for (var i = 0; i < 9999; i++) //從 A1 儲存格開始讀取資料
                {
                    if (book.Sheets[0][i, 0].Value == null || string.IsNullOrEmpty(book.Sheets[0][i, 0].Value.ToString()))
                    {
                        break;
                    }

                    var col = 0;
                    var connectionName = book.Sheets[0][i, col].Value.ToString();
                    var sql0 = string.Empty;
                    var sbSql0 = new StringBuilder();

                    sbSql0.AppendLine("SELECT * FROM DBInfo");
                    sbSql0.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql0.Append($"   AND ConnectionName = '{connectionName}'");

                    sql0 = sbSql0.ToString();

                    var dtData = JasonQueryRepository.ExecQuery(sql0);

                    if (dtData?.Rows.Count > 0)
                    {
                        connectionName += $"{DateTime.Now:_MMddHHmmss}";
                    }

                    col++;

                    var dataSource = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var server = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var sid = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var directMode = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var database = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var connectAs = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var port = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var user = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var password = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    if (!string.IsNullOrEmpty(password))
                    {
                        password = TextEngine.Encode(TextEngine.Encrypt(password, MyGlobal.DomainUser));
                    }

                    col++;

                    var autoRollback = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var unicode = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var tabBackColor = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var tabActiveForeColor = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var tabInactiveForeColor = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var remarks = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s1 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s2 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s3 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s4 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s5 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s6 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s7 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s8 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s9 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s10 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s11 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s12 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s13 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s14 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    col++;

                    var s15 = book.Sheets[0][i, col].Value == null ? string.Empty : book.Sheets[0][i, col].Value.ToString();

                    sbSql.AppendLine("INSERT INTO DBInfo (DomainUser, ConnectionName, DataSource, Server, SID,");
                    sbSql.AppendLine("       DirectMode, Database, ConnectAs, Port, User, Password,");
                    sbSql.AppendLine("       AutoRollback, Unicode, TabBackColor, TabActiveForeColor,");
                    sbSql.AppendLine("       TabInactiveForeColor, Remarks, O1, O2, O3, O4, O5, O6, O7, O8,");
                    sbSql.AppendLine("       O9, O10, O11, O12, O13, O14, O15)");
                    sbSql.AppendLine($"VALUES ('{MyGlobal.DomainUser}', '{connectionName}', '{dataSource}', '{server}', '{sid}',");
                    sbSql.AppendLine($"        '{directMode}', '{database}', '{connectAs}', '{port}', '{user}', '{password}',");
                    sbSql.AppendLine($"        '{autoRollback}', '{unicode}', '{tabBackColor}', '{tabActiveForeColor}',");
                    sbSql.AppendLine($"        '{tabInactiveForeColor}', '{remarks}', '{s1}', '{s2}', '{s3}', '{s4}', '{s5}', '{s6}', '{s7}', '{s8}',");
                    sbSql.AppendLine($"        '{s9}', '{s10}', '{s11}', '{s12}', '{s13}', '{s14}', '{s15}');");

                    count++;
                }

                var sql = sbSql.ToString();

                JasonQueryRepository.ExecNonQuery(sql);

                message = LocalizationHelper.GetLanguageString("{qty} connection information imported successfully!", "form", GetType().Name, "msg", "ImportOK", "Text").Replace("{qty}", count.ToString());
                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);
                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                book.Dispose();
                Dispose();
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
    }
}
