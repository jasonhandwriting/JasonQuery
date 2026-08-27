using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Core.Config
{
    public static class MyGlobal
    {
        public static Assembly IconLibrary = null;
        public static string Separator = " ";
        public static string Separator3 = "   ";
        public static string Separator3s = " ` ";
        public static string Separator5 = "     ";
        public static string Separator7 = "       ";
        public static string SeparatorPlus1 = "!~|||~!";
        public static string SeparatorPlus2 = "|~!!!~|";
        public static string SeparatorPlus3 = "¡i¡i¡";
        public static string SeparatorPlus4 = "¡j¡j¡";
        public static string Separator00A1 = "¡"; //20240301 for 定時備份
        public static string Row_Id_PK_JQ = "R_O_W_1_D_P_K_J_Q"; //20240518 RowID 的識別欄位名稱改為全域變數，以避免與使用者 Table 原始欄位名稱重複，引發例外錯誤

        public static string ExecuteNonQuerySqlHistoryScript = string.Empty; //執行「非查詢SQL」，逐筆寫入 SQL History
        public static string CheckExistTabResult = string.Empty;
        public static string TabBackColor = string.Empty; //Main Form Tab Color
        public static string TabActiveForeColor = string.Empty; //Main Form Tab Color
        public static string TabInactiveForeColor = string.Empty; //Main Form Tab Color
        public static bool IsTabShrinkPages = true;
        public static bool IsTabShowArrows = true;
        public static bool IsTabHoverSelect = false;
        public static bool IsTabMultiLine = false;
        public static bool IsTabBold = true;
        public static string DomainUser = WindowsIdentity.GetCurrent().Name;
        public static string OpenFileFromMDIForm = string.Empty; //由主表單觸發，要開啟指定的檔案
        public static string InfoFromMDIForm = string.Empty; //由主表單觸發，傳送特定訊息至指定的子表單
        public static string InfoFromReloadLocalization = string.Empty; //由主表單觸發，傳送特定訊息至所有已開啟的子表單
        public static string CheckFileFromMDIForm = string.Empty; //由主表單觸發，傳送特定訊息至指定的子表單
        public static string FormSchemaBrowserKey = string.Empty; //開啟 Schema Browser，記錄它的 Key
        public static string FormSqlHistoryKey = string.Empty; //開啟 SQL History，記錄它的 Key
        public static string FormCreateTableKey = string.Empty; //開啟 Create Table，記錄它的 Key
        public static string FormOptionsKey = string.Empty; //開啟 Options，記錄它的 Key
        public static bool IsContainsFocusFormOptionsKey = true; //記錄「本程式是否作用中？」
        public static string PendingExternalOpenFileName = string.Empty; //從外面呼叫，開啟指定的檔案
        public static string CancelOpenAndCloseTab = string.Empty; //開啟檔案失敗或使用者取消開啟檔案，關閉空白的 Tab
        public static string SendMessageToInfo = string.Empty;
        public static int CommitRollbackCheck = -1; //-1:未檢查；0:Rollback；1:Commit；2:取消
        public static string BookmarkStyle = string.Empty;
        public static string CsvDelimiters = string.Empty; //匯出至 CSV 的分隔符號
        public static string RequireToRestart = string.Empty;
        public static string AnUnexpectedErrorHasOccurred = string.Empty; //20250514
        public static string StackTrace = string.Empty; //20250514
        public static string PleaseTryAgain = string.Empty; //20250514
        public static string AnErrorHasOccurred = string.Empty; //20250514
        public static string LineName = string.Empty; //20250617

        public static DataTable dtAutoCompleteForAll; //for Query Editor AutoComplete，取得指定的關鍵字
        public static DataTable dtTabList; //for Query Editor，顯示 Tab List

        public static int CommitRollbackIcon = 1;

        public static Dictionary<string, string> dicBookmarkStyle = new Dictionary<string, string>();
        public static Dictionary<string, string> dicWordWrapIndentMode = new Dictionary<string, string>();
        public static Dictionary<string, string> dicRowSizing = new Dictionary<string, string>();
        public static Dictionary<string, string> dicCsvDelimiters = new Dictionary<string, string>();
        public static Dictionary<string, string> dicDirection = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, string>> dicAll = new Dictionary<string, Dictionary<string, string>>();

        public static string RowSize = string.Empty;
        public static string AutoDisconnect = string.Empty;
        public static bool IsPendingTransactionWarning = true;
        public static int PendingTransactionWarningIntervalMilliseconds = 5 * 60 * 1000; //5分鐘
        public static string SpecifiedSqlFile1 = string.Empty;
        public static string SpecifiedSqlFile2 = string.Empty;

        //20241115 透過此變數，判斷是否要、何時要加載 Table 的所有欄位訊息
        public static int ShowColumnInfo = -1;

        public static bool IsShowColumnInfo = false;
        public static bool IsSortByColumnName = false;
        public static bool IsDefaultTabSchemaBrowser = false;
        public static bool IsAutoListMembers = false;
        public static bool ShouldSavePoint = false;
        public static int ProgressInsertInto = 0;
        public static bool IsProgressCancel = false;
        public static string WordWrapIndentMode = string.Empty;
        public static int TabWidth = 4;
        public static bool IsPreviewCLOBData = false;

        public static string GlobalTemp = string.Empty; //暫存變數，判斷用！
        public static string GlobalTemp2 = string.Empty; //暫存變數，判斷用！
        public static string GlobalTemp3 = string.Empty; //暫存變數，判斷用！
        public static string GlobalTemp4 = string.Empty; //暫存變數，判斷用！
        public static string GlobalTemp5 = string.Empty; //暫存變數，判斷用！
        public static string GlobalTemp6 = string.Empty; //20241207 暫存變數，判斷用！
        public static string GlobalExecuteCommitRollback = string.Empty; //判斷Commit/Rollback！
        public static string GlobalTempDialog = string.Empty; //判斷 Dialog 視窗

        public static OracleReader OracleReader = new OracleReader();
        public static PostgreSqlReader PostgreSqlReader = new PostgreSqlReader();
        public static SqlServerReader SqlServerReader = new SqlServerReader();
        public static MySqlReader MySqlReader = new MySqlReader();

        public static string SchemaAccessibleDescription = string.Empty;

        //20250603 允許的空值樣式
        public static readonly HashSet<string> AllowedNullValueIndicators = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "<NULL>", "{NULL}", "(NULL)" };

        public static string OptionsTabName = string.Empty;
        public static string SchemaBrowserTabName = string.Empty;
        public static string SqlHistoryTabName = string.Empty;
        public static string CreateTableTabName = string.Empty;
        public static string OptionsTabName_Before = string.Empty;
        public static string SchemaBrowserTabName_Before = string.Empty;
        public static string SqlHistoryTabName_Before = string.Empty;
        public static string CreateTableTabName_Before = string.Empty;

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        public static string GetIPAddress()
        {
            var ipAddress = string.Empty;
            var hostName = Dns.GetHostName().ToUpper();

            try
            {
                var ipHostEntry = Dns.GetHostEntry(hostName);

                //取得所有 IP 位址 (只取 IP V4 的 Address)
                ipAddress = ipHostEntry.AddressList.Where(ipaddress => ipaddress.AddressFamily == AddressFamily.InterNetwork).Aggregate(ipAddress, (current, ipaddress) => $"{current}{ipaddress};");

                //20230129 忽略 169.254 網段的 IP
                var parts = ipAddress.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                ipAddress = string.Empty;

                for (var i = 0; i < parts.Length; i++)
                {
                    if (!parts[i].StartsWith("169.254.", StringComparison.Ordinal))
                    {
                        ipAddress += $"{parts[i]};";
                    }
                }

                ipAddress = ipAddress.TrimEnd(';');
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = hostName;
            }

            return ipAddress;
        }

        public static string DateDiff(DateTime dateTime1, DateTime dateTime2, string minutes = "")
        {
            var ts1 = new TimeSpan(dateTime1.Ticks);
            var ts2 = new TimeSpan(dateTime2.Ticks);
            var ts = ts1.Subtract(ts2).Duration();
            var dateDiff = $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";

            return string.IsNullOrEmpty(minutes) ? dateDiff : ts.Minutes.ToString("00");
        }

        public static bool IsNumeric(string expression)
        {
            var isNumeric = double.TryParse(expression, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _);

            return isNumeric;
        }

        public static void AddAutoCompleteData(string objectName, string objectSource)
        {
            if (dtAutoCompleteForAll == null || string.IsNullOrWhiteSpace(objectName))
            {
                return;
            }

            AddAutoCompleteDataCore(dtAutoCompleteForAll, objectName, objectSource);
        }

        public static void AddAutoCompleteDataRange(IEnumerable<string> objectNames, string objectSource)
        {
            if (dtAutoCompleteForAll == null || objectNames == null)
            {
                return;
            }

            dtAutoCompleteForAll.BeginLoadData();

            try
            {
                foreach (var objectName in objectNames)
                {
                    if (string.IsNullOrWhiteSpace(objectName))
                    {
                        continue;
                    }

                    AddAutoCompleteDataCore(dtAutoCompleteForAll, objectName, objectSource);
                }
            }
            finally
            {
                dtAutoCompleteForAll.EndLoadData();
            }
        }

        private static void AddAutoCompleteDataCore(DataTable dtTarget, string objectName, string objectSource)
        {
            var row = dtTarget.NewRow();

            row["ObjectName"] = objectName;
            row["ObjectSource"] = $"[{objectSource}]";

            dtTarget.Rows.Add(row);
        }

        public static void MouseDownDataGridExportDataToFile(C1TrueDBGrid c1Grid, int x, int y, ContextMenuStrip gridContextMenu)
        {
            var isCornerSelected = false;

            c1Grid.ContextMenuStrip = null;

            //取得滑鼠所在列的儲存格行列位置
            var row = c1Grid.RowContaining(y);
            var col = c1Grid.ColContaining(x);

            if (row == -1 && col == -1)
            {
                isCornerSelected = y <= c1Grid.Splits[0].ColumnCaptionHeight;
            }

            if (isCornerSelected) //按下 Grid's 左上角
            {
                return;
            }

            if (row == -1)
            {
                return;
            }

            if (col == -1) //使用者點到最左側，略過！
            {
                return;
            }

            var columnName = c1Grid.Splits[0].DisplayColumns[col].ToString();
            var temp = LocalizationHelper.GetLanguageString("Export All Data to File (Excel/CSV...)", "form", "QueryForm", "menugrid", "ExportAllDataToFile", "Text");

            gridContextMenu.Items.Add(temp);
            gridContextMenu.Items[0].Image = IconManager.GetImage(IconLibrary, "Export 16x16.ico");

            gridContextMenu.Items[0].Click += delegate
            {
                using (var form = new ExportToFileForm())
                {
                    var dt = c1Grid.GetDataTableSourceOrNull();

                    form.dtData = dt;
                    form.dtSchemaTable = dt;
                    form.FontName = c1Grid.Font.Name;
                    form.FontSize = c1Grid.Font.Size;
                    form.ShowDialog();
                }
            };

            c1Grid.ContextMenuStrip = gridContextMenu;

            gridContextMenu.BackColor = ColorTranslator.FromHtml("#FAFAD2"); //淺黃
            gridContextMenu.Show(c1Grid, new Point(x, y));

            c1Grid.SetActiveCell(row, col);
        }

        public static void DisplaySchemaBrowser(string schemaNode, string schemaType, string schemaName, string schemaDbo = "", string objectID = "", bool isGenerateAccessibleDescription = false)
        {
            using (var form = new SchemaBrowserForm())
            {
                form.DisplayRowIndex = -1;
                form.SchemaNode = schemaNode;
                form.SchemaType = schemaType;
                form.SchemaName = DatabaseSqlExecutor.CurrentDataSource == DataSourceType.Oracle ? schemaName.ToUpper() : schemaName;
                form.SchemaDbo = schemaDbo;
                form.ObjectId = objectID;

                if (isGenerateAccessibleDescription)
                {
                    form.AccessibleDescriptionString = $"{DateTime.Now:yyyyMMddHHmmssfff}";
                }

                //20250426 改寫 Width/Height 取值方法
                var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(DomainUser, "SchemaBrowserFormWidth", "SchemaBrowserFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                form.ShowDialog();
            }
        }

        /// <summary>
        /// 關閉頁籤時，刪除此筆資料庫連線所儲存的暫存檔訊息
        /// </summary>
        /// <param name="backupFileName">【對應 SystemConfig 欄位：AttributeValue】備份的檔案訊息：DomainUser+MPID+年月日時分秒毫秒</param>
        public static void DeleteBackupFileInfo(string backupFileName)
        {
            backupFileName = Path.GetFileName(backupFileName);

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
            sbSql.Append($"   AND AttributeValue = '{backupFileName.Replace("'", "''")}'");

            var sql = sbSql.ToString();
            var dtBackup = JasonQueryRepository.ExecQuery(sql);

            //關閉頁籤時，一併刪除 Backup 暫存檔訊息
            if (dtBackup?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("DELETE FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
                sbSql.Append($"   AND AttributeValue = '{backupFileName.Replace("'", "''")}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql, false);
            }

            try
            {
                var fileName = $"{AppConfigHelper.BackupPath}{backupFileName}";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        public static string DateTimeNowfff()
        {
            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}";
        }

        public static string DateTimeNow()
        {
            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }

        public static string DateTimeNowWithDateFormat()
        {
            return DateTime.Now.ToString($"{MyLibrary.DateFormat} HH:mm:ss");
        }

        public static void ClearMemory()
        {
            long memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
            double memoryUsedMB = memoryUsed / (1024.0 * 1024.0); //轉換為 MB
            const double memoryThresholdMB = 200.0; //記憶體閾值，當使用量超過此值時進行回收

            //檢查是否超過閾值
            if (memoryUsedMB > memoryThresholdMB)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
    }
}
