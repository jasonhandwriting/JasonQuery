using System;
using System.Data;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace Updater
{
    internal class MyGlobal
    {
        public static DataTable dtLocalization = new DataTable();
        public static string sLocalization = "en-US";
        public static string sXmlFilename = "english.xml";
        public static string sEnvironment = "PROD";
        public static string sAnUnexpectedErrorHasOccurred = string.Empty; //20250527
        public static string sStackTrace = string.Empty; //20250527
        public static string sLogFilename = string.Empty; //20250626 Log Filename for 記錄執行日誌

        public static void ApplyLanguageInfo(Form frmForm, bool bColor = true)
        {
            var sonControls = frmForm.Controls;

            //更新 Form's Title
            frmForm.Text = GetLanguageString(frmForm.Text, "form", frmForm.Name, "object", "this", "Text");

            foreach (Control control in sonControls)
            {
                if ("`Label`Button`CheckBox`RadioButton`".Contains("`" + control.GetType().Name + "`"))
                {
                    control.Text = GetLanguageString(control.Text, "form", frmForm.Name, "object", control.Name, "Text");
                }
                else if (control.GetType().Name == "GroupBox")
                {
                    control.Text = GetLanguageString(control.Text, "form", frmForm.Name, "object", control.Name, "Text");

                    foreach (Control ctrlInGroupBox in ((GroupBox)control).Controls)
                    {
                        if ("`Label`Button`CheckBox`RadioButton`".Contains("`" + ctrlInGroupBox.GetType().Name + "`"))
                        {
                            ctrlInGroupBox.Text = GetLanguageString(ctrlInGroupBox.Text, "form", frmForm.Name, "object", ctrlInGroupBox.Name, "Text");
                        }
                    }
                }
            }
        }

        public static string GetLanguageString(string sOriginalText, string sCategory, string sClass, string sType, string sID, string sAttribute)
        {
            var sResult = sOriginalText;

            try
            {
                if (dtLocalization != null && dtLocalization.Rows.Count > 0 && dtLocalization.Columns.Count == 6)
                {
                    var sSQL = "Category = '" + sCategory + "' AND Class = '" + sClass + "' AND Type = '" + sType + "' AND ID = '" + sID + "' AND Attribute = '" + sAttribute + "'";
                    var dtRow = dtLocalization.Select(sSQL);

                    if (dtRow.Length > 0 && !string.IsNullOrEmpty(dtRow[0]["name"].ToString().Trim()))
                    {
                        sResult = dtRow[0]["name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var sLangText = GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");

                MessageBox.Show(sLangText + "\r\n\r\n" + ex.Message, @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return sResult;
        }

        public static DataTable XmlToDataTable(string sFilename)
        {
            //使用範例：
            //DataTable dd = XmlToDataTable(@"D:\temp\lang.xml");
            var dt = new DataTable();

            try
            {
                var sXml = File.ReadAllText(sFilename);

                //新建XML文件類別
                var xmldoc = new XmlDocument();
                //從指定的字串載入XML文件
                xmldoc.LoadXml(sXml);
                //建立此物件，並輸入透過StringReader讀取Xmldoc中的Xmldoc字串輸出
                var xmlreader = XmlReader.Create(new StringReader(xmldoc.OuterXml));
                //建立DataSet
                var ds = new DataSet();
                //透過DataSet的ReadXml方法來讀取Xmlreader資料
                ds.ReadXml(xmlreader);
                //建立DataTable並將DataSet中的第0個Table資料給DataTable
                dt = ds.Tables[0];
            }
            catch (Exception)
            {
                //MessageBox.Show(@"An error has occurred while loading localization file:" + "\r\n\r\n" + sFilename + "\r\n\r\n" + ex.Message, @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //回傳DataTable
            return dt;
        }

        public static string CheckAndCreateDirectory(string sPath)
        {
            var sResult = string.Empty;

            if (!Directory.Exists(sPath))
            {
                try
                {
                    Directory.CreateDirectory(sPath); //可以一次建立多層的子目錄
                }
                catch (Exception ex)
                {
                    sResult = ex.Message;
                }
            }

            if (Directory.Exists(sPath))
            {
                sResult = string.Empty;
            }

            return sResult;
        }

        #region 將內容寫入到指定的檔案
        /// <summary>
        /// 將內容寫入到指定的檔案
        /// </summary>
        /// <param name="sFileContent">要寫入的檔案內容</param>
        /// <param name="sFileName">檔案名稱</param>
        /// <param name="EncodeMethod">文字編碼方式</param>
        public static void WriteContentToFile(string sFileContent, string sFileName, FileMode FM = FileMode.Append, FileAccess FA = FileAccess.Write)
        {
            if (Directory.Exists(Path.GetDirectoryName(sFileName)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sFileName));
            }

            var fs = new FileStream(sFileName, FM, FA);
            var sw = new StreamWriter(fs, System.Text.Encoding.Default);

            sw.WriteLine(sFileContent);
            sw.Close();
        }
        #endregion
    }
}