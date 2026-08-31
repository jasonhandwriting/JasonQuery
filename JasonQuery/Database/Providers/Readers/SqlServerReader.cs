using Devart.Data.SqlServer;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Transactions;
using JasonQuery.Core.Database.Transactions.LockingQueries;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Forms;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace JasonQuery.Database.Providers.Readers
{
    public class SqlServerReader
    {
        public delegate void QueryCompletedEventHandler();
        public event QueryCompletedEventHandler QueryCompleted;
        public SqlDataReader DataReader { get; private set; }
        public DataTable dtQuerySchema { get; set; }
        public int QueryTimeout { get; set; }

        private SqlConnection _conn;
        private SqlTransaction _transaction;
        private SqlCommand _command;
        private string _sqlType = string.Empty;
        private string _sqlStatementType = string.Empty;


        public string ConnectTo(string connectionString)
        {
            var errorMessage = string.Empty;

            _command = new SqlCommand();
            _conn = new SqlConnection(connectionString);

            try
            {
                _conn.Open();
                _transaction = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (SqlException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return GetState() == ConnectionState.Open ? string.Empty : errorMessage;
        }

        public string Commit()
        {
            var result = string.Empty;

            try
            {
                _transaction?.Commit();
                _transaction?.Dispose();
                _transaction = null;
            }
            catch (SqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                result = $"ErrorMsg: {ex.Message}";
            }

            return result;
        }

        public string Rollback()
        {
            var result = string.Empty;

            try
            {
                _transaction?.Rollback();
                _transaction?.Dispose();
                _transaction = null;
            }
            catch (SqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                result = $"ErrorMsg: {ex.Message}";
            }

            return result;
        }

        public string SavePoint(string savePoint)
        {
            var result = string.Empty;

            if (string.IsNullOrWhiteSpace(savePoint))
            {
                return "ErrorMsg: SavePoint name is empty.";
            }

            if (!TryEnsureTransactionOpen(out result))
            {
                return result;
            }

            try
            {
                _transaction.Save(savePoint);
            }
            catch (SqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                result = $"ErrorMsg: {ex.Message}";
            }

            return result;
        }

        public string RollbackPoint(string savePoint)
        {
            var result = string.Empty;

            if (string.IsNullOrWhiteSpace(savePoint))
            {
                return "ErrorMsg: SavePoint name is empty.";
            }

            if (!TryEnsureTransactionOpen(out result))
            {
                return result;
            }

            try
            {
                _transaction.Rollback(savePoint);
            }
            catch (SqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                result = $"ErrorMsg: {ex.Message}";
            }

            return result;
        }

        public bool Disconnect()
        {
            var success = true;

            success &= TryDisposeAndClearDataReader();
            success &= TryDisposeAndClear(ref _command);
            success &= TryDisposeAndClear(ref _transaction);
            success &= TryCloseDisposeAndClearConnection();

            return success;
        }

        private static bool TryDisposeAndClear<T>(ref T disposable) where T : class, IDisposable
        {
            if (disposable == null)
            {
                return true;
            }

            try
            {
                disposable.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                disposable = null;
            }
        }

        private bool TryCloseDisposeAndClearConnection()
        {
            var conn = _conn;

            if (conn == null)
            {
                return true;
            }

            var success = true;

            try
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
            catch (Exception)
            {
                success = false;
            }

            try
            {
                conn.Dispose();
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                _conn = null;
            }

            return success;
        }

        public ConnectionState GetState()
        {
            return _conn?.State ?? ConnectionState.Closed;
        }

        public string InterruptQuery()
        {
            var result = string.Empty;

            try
            {
                _command.Cancel();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public void ExecuteQueryPaged(object sqlQuery)
        {
            var startRow = 0;
            var pageLength = 0;
            var selectionStart = 0;
            var positionAdjust = 1;
            var positionOffset = 0;
            var position = -1;
            var sql = string.Empty;
            var crlf = 0;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL

            //20231104 將以下變數抽出，以利在 try/catch 的 finally 中處理
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;
            var errorMessage2 = string.Empty;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 25)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    int.TryParse(parts[2], out crlf);
                    sql = parts[3].TrimEnd(' ', ';', '\r', '\n');
                    int.TryParse(parts[4], out startRow);
                    int.TryParse(parts[5], out pageLength);
                }

                using (var script = new SqlScript(sql))
                {
                    sql = Regex.Replace(script.Statements[0].Text, @"(?<!\r)\n", "\r\n");
                    positionOffset = script.Statements[0].Offset; //去掉註解後，真正要執行的 SQL 指令
                }

                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                var useSinglePassReader = DatabaseLockingQueryExecutionPolicy.ShouldUseSinglePassReader
                (
                    DataSourceType.SqlServer,
                    sql
                );

                if (useSinglePassReader)
                {
                    //Locking queries must be sent to the database only once.
                    //A separate KeyInfo probe would execute the locking SQL a second time. Automatic paging is also bypassed.
                    TryDisposeAndClearDataReader();

                    DataReader = _command.ExecuteReader
                    (
                        CommandBehavior.Default
                    );

                    dtQuerySchema = DataReader.GetSchemaTable()?.Copy();
                }
                else
                {
                    //20241105 原廠建議，將 CommandBehavior.Default 改為 CommandBehavior.KeyInfo，可以獲取到更多資訊
                    //20250929 改為從第 0 筆開始取資料，只取 0筆 (目的是要取得 SchemaInfo)，取得後立即釋放資源
                    using (var schemaReader = _command.ExecutePageReader(CommandBehavior.KeyInfo, 0, 0))
                    {
                        //20250929 取得 SchemaInfo
                        dtQuerySchema = schemaReader.GetSchemaTable();
                    }

                    TryDisposeAndClearDataReader();

                    //20250929 此處改用 CommandBehavior.Default，因為 CommandBehavior.KeyInfo 會強制生成 DataTable Constraint
                    //         可能會干擾 SQL 的查詢結果，尤其是複雜的 SQL (例如 JOIN 很多資料表)
                    DataReader = _command.ExecutePageReader
                    (
                        CommandBehavior.Default,
                        startRow,
                        pageLength
                    );
                }
            }
            catch (ThreadAbortException)
            {
                var message = LocalizationHelper.GetLanguageString("Current operation was aborted.", "Global", "Global", "msg", "OperationAborted", "Text");

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                errorCode = string.Empty; //20220820 拿掉 ErrorCode，因為它看起來像是一串無意義的負數。
                errorHint = string.Empty;
                errorMessage = ex.Message.Replace("\r\n資料指標並未宣告。", string.Empty).Replace("The cursor was not declared.", string.Empty);
                errorMessage = $"\r\n{errorMessage}";

                var index = errorMessage.IndexOf("\r\n ) AS PAGE_READ_T\r\n LIMIT", StringComparison.OrdinalIgnoreCase);

                if (index > 0)
                {
                    errorMessage = errorMessage.Substring(0, index);
                }

                if (errorMessage.StartsWith("\r\n\r\n", StringComparison.Ordinal))
                {
                    errorMessage = errorMessage.Substring(2);
                }

                var numberMessage = LocalizationHelper.GetLanguageString("Number", "Global", "Global", "msg", "Number", "Text");
                var classMessage = LocalizationHelper.GetLanguageString("Class", "Global", "Global", "msg", "Class", "Text");
                var stateMessage = LocalizationHelper.GetLanguageString("State", "Global", "Global", "msg", "State", "Text");

                var errorMessageFull = AnalysisErrorMessageWithLineNumber(sql, ex.Errors, out errorMessage2, numberMessage, classMessage, stateMessage, MyGlobal.LineName);

                if (string.IsNullOrEmpty(errorMessage2))
                {
                    errorMessageFull = AnalysisErrorMessageWithoutLineNumber(sql, ex.Errors, out errorMessage2, numberMessage, classMessage, stateMessage, MyGlobal.LineName);
                }

                if (string.IsNullOrEmpty(errorMessage2))
                {
                    var from = errorMessage.IndexOf('\'') + 1;
                    var to = errorMessage.LastIndexOf('\'');

                    //20230909 判斷是否有兩個以上的單引號
                    if (from > 0 && to > 0 && to > from)
                    {
                        to = errorMessage.Substring(from).IndexOf('\'') + from;

                        var tempWord = errorMessage.Substring(from, to - from);
                        var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);

                        if (ex.LineNumber > 0 && ex.LineNumber <= parts.Length && parts[ex.LineNumber - 1].IndexOf(tempWord, StringComparison.Ordinal) >= 0)
                        {
                            var temp1 = 0;
                            var temp2 = 0;
                            var positionPerErrLine = parts[ex.LineNumber - 1].IndexOf(tempWord, StringComparison.Ordinal);

                            foreach (var t in parts)
                            {
                                if (ex.LineNumber == temp1 + 1)
                                {
                                    temp2 += positionPerErrLine;
                                    break;
                                }
                                else
                                {
                                    temp2 += t.Length + 2;
                                }

                                temp1++;
                            }

                            position = temp2;
                        }
                        else
                        {
                            var sqlUpper = sql.ToUpper();
                            var tempWordUpper = tempWord.ToUpper();

                            position = sqlUpper.IndexOf(tempWordUpper, StringComparison.Ordinal);
                        }
                    }
                }
                else
                {
                    errorMessage = errorMessageFull;
                }

                position = 0;
            }
            catch (InvalidOperationException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (ExternalException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            finally
            {
                //20231104 改在 finally 判斷要不要處理 Exception 錯誤
                if (position != -1) //20231104 如果不是 -1，表示有進入其中一個 Exception
                {
                    //傳回 SQL 錯誤的字串位置、錯誤訊息，由「該 QueryEditor」將錯誤的字串標示波浪底線
                    selectionStart = selectionStart + positionOffset + position + positionAdjust;
                }

                PublishSqlExecuteErrorIfNeeded
                (
                    position,
                    accessibleDescription,
                    executedResult: string.Empty,
                    errorCode,
                    errorMessage,
                    errorHint,
                    selectionStart,
                    sql,
                    errorMessage2
                );

                NotifyQueryCompleted();
            }
        }

        public DataTable ExecuteQueryPaged100Rows(string sql, int startRow, int pageLength, out string errorMessage, out DataTable dtSchema)
        {
            errorMessage = string.Empty;
            dtSchema = null;

            var rowsCount = 0;
            var dtData = new DataTable();

            EnsureConnectionOpen();

            try
            {
                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                //20260717 只取得完整 Schema, KeyInfo (比照 ExecuteQueryPaged 的兩階段設計，因為 ExecutePageReader(KeyInfo, 0, x) 所建立的分頁結果集，沒有正確保留底層主鍵的 IsKey Metadata)
                var schemaContext = new TraceLogContext { Category = "Database", RequestedRows = 0, Message = "Two-phase paged query" };

                using (TraceLogger.Time("Database", "ExecutePageReader.KeyInfo", schemaContext))
                using (var schemaReader = _command.ExecutePageReader(CommandBehavior.KeyInfo, 0, 0))
                {
                    dtSchema = schemaReader.GetSchemaTable();
                    schemaContext.ColumnCount = dtSchema?.Rows.Count;
                }

                //20260717 只負責讀取實際分頁資料
                if (pageLength > 0)
                {
                    var dataContext = new TraceLogContext { Category = "Database", RequestedRows = pageLength, Message = "Two-phase paged query" };

                    using (TraceLogger.Time("Database", "ExecutePageReader.DataPage", dataContext))
                    using (var dataReader = _command.ExecutePageReader(CommandBehavior.Default, startRow, pageLength))
                    {
                        dtData.Load(dataReader);
                        dataContext.ReturnedRows = dtData.Rows.Count;
                        dataContext.ColumnCount = dtData.Columns.Count;
                    }
                }

                rowsCount = dtData.Rows.Count;
            }
            catch (ThreadAbortException ex)
            {
                errorMessage = ex.Message;
            }
            catch (SqlException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(JasonQueryRepository.DbMotherPid))
                {
                    var result = string.IsNullOrEmpty(errorMessage) ? "Complete" : "Error";

                    //20250217 添加 SQL 歷史記錄
                    JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), string.Empty, string.Empty, rowsCount, result, errorMessage, sql, "JQ_SYSTEM");
                }
            }

            return dtData;
        }

        //sql example: "INSERT INTO Pictures (ID, PicName, Picture) VALUES (1, 'pict1', :Pictures)"
        public int UploadFileToBlobField(string sql, string fieldName, string fileName, out string errorMessage)
        {
            errorMessage = string.Empty;

            EnsureConnectionOpen();

            var affectedRows = 0;

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(stream))
                {
                    //20231018 Devart.Data.SqlServer 沒有 Blob 相關方法，故借用 PostgreSql.PgSqlBlob
                    var lob = new Devart.Data.PostgreSql.PgSqlBlob(reader.ReadBytes((int)stream.Length));
                    var blob = lob.Value;
                    var hex = new StringBuilder(blob.Length * 2);

                    foreach (byte b in blob)
                    {
                        hex.AppendFormat("{0:x2}", b);
                    }

                    var hexValue = hex.ToString();
                    var sqlText = sql.Replace("{HEX}", hexValue);

                    using (var command = new SqlCommand(sqlText, _conn))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = QueryTimeout;

                        affectedRows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                errorMessage = ex.Message;
            }
            catch (SqlException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return affectedRows;
        }

        public void ExecuteQuery(object sqlQuery)
        {
            var selectionStart = 0;
            var positionAdjust = 1;
            var positionOffset = 0;
            var position = -1;
            var sql = string.Empty;
            var crlf = 0;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL

            //20231104 將以下變數抽出，以利在 try/catch 的 finally 中處理
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;
            var errorMessage2 = string.Empty;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 15)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    int.TryParse(parts[2], out crlf);
                    sql = parts[3].TrimEnd(' ', ';', '\r', '\n');
                }

                using (var script = new SqlScript(sql))
                {
                    sql = Regex.Replace(script.Statements[0].Text, @"(?<!\r)\n", "\r\n");
                    positionOffset = script.Statements[0].Offset; //去掉註解後，真正要執行的 SQL 指令
                }

                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                var useSinglePassReader = DatabaseLockingQueryExecutionPolicy.ShouldUseSinglePassReader
                (
                    DataSourceType.SqlServer,
                    sql
                );

                if (useSinglePassReader)
                {
                    //Obtain data and schema from the same execution. This avoids executing SELECT ... FOR UPDATE / locking table hints twice.
                    TryDisposeAndClearDataReader();

                    DataReader = _command.ExecuteReader
                    (
                        CommandBehavior.Default
                    );

                    dtQuerySchema = DataReader.GetSchemaTable()?.Copy();
                }
                else
                {
                    //20250929 改為從第 0 筆開始取資料，只取 0筆 (目的是要取得 SchemaInfo)，取得後立即釋放資源
                    using (var schemaReader = _command.ExecutePageReader(CommandBehavior.KeyInfo, 0, 0))
                    {
                        //20250929 取得 SchemaInfo
                        dtQuerySchema = schemaReader.GetSchemaTable();
                    }

                    TryDisposeAndClearDataReader();

                    //20250929 此處改用 CommandBehavior.Default，因為 CommandBehavior.KeyInfo 會強制生成 DataTable Constraint
                    //         可能會干擾 SQL 的查詢結果，尤其是複雜的 SQL (例如 JOIN 很多資料表)
                    DataReader = _command.ExecuteReader
                    (
                        CommandBehavior.Default
                    );
                }
            }
            catch (ThreadAbortException)
            {
                var message = LocalizationHelper.GetLanguageString("Current operation was aborted.", "Global", "Global", "msg", "OperationAborted", "Text");

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                position = 0;

                if (TextHelper.CheckTextStartWithAndContains(ex.Message, "Parameter", "' is missing"))
                {
                    var temp = TextHelper.GetStringBetween(ex.Message, "'", "'");
                    var word = $":{temp}";

                    position = sql.IndexOf(word, StringComparison.Ordinal) + 1;
                }

                errorCode = string.Empty; //20220820 拿掉 ErrorCode，因為它看起來像是一串無意義的負數。
                errorHint = string.Empty;
                errorMessage = ex.Message.Replace("\r\n資料指標並未宣告。", string.Empty).Replace("The cursor was not declared.", string.Empty);
                errorMessage = $"\r\n{errorMessage}";

                var numberMessage = LocalizationHelper.GetLanguageString("Number", "Global", "Global", "msg", "Number", "Text");
                var classMessage = LocalizationHelper.GetLanguageString("Class", "Global", "Global", "msg", "Class", "Text");
                var stateMessage = LocalizationHelper.GetLanguageString("State", "Global", "Global", "msg", "State", "Text");

                AnalysisErrorMessageWithLineNumber(sql, ex.Errors, out errorMessage2, numberMessage, classMessage, stateMessage, MyGlobal.LineName);

                if (string.IsNullOrEmpty(errorMessage2))
                {
                    AnalysisErrorMessageWithoutLineNumber(sql, ex.Errors, out errorMessage2, numberMessage, classMessage, stateMessage, MyGlobal.LineName);
                }

                if (string.IsNullOrEmpty(errorMessage2))
                {
                    var from = errorMessage.IndexOf('\'') + 1;
                    var to = errorMessage.LastIndexOf('\'');

                    //20230909 判斷是否有兩個以上的單引號
                    if (from >= 0 && to >= 0 && to > from)
                    {
                        to = errorMessage.Substring(from).IndexOf('\'') + from;

                        var tempWord = errorMessage.Substring(from, to - from);
                        var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);

                        if (ex.LineNumber > 0 && ex.LineNumber <= parts.Length && parts[ex.LineNumber - 1].IndexOf(tempWord, StringComparison.Ordinal) >= 0)
                        {
                            var temp1 = 0;
                            var temp2 = 0;
                            var positionPerErrLine = parts[ex.LineNumber - 1].IndexOf(tempWord, StringComparison.Ordinal);

                            foreach (var t in parts)
                            {
                                if (ex.LineNumber == temp1 + 1)
                                {
                                    temp2 += positionPerErrLine;
                                    break;
                                }
                                else
                                {
                                    temp2 += t.Length + 2;
                                }

                                temp1++;
                            }

                            position = temp2;
                        }
                        else
                        {
                            var sqlUpper = sql.ToUpper();
                            var tempWordUpper = tempWord.ToUpper();

                            position = sqlUpper.IndexOf(tempWordUpper, StringComparison.Ordinal);
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (ExternalException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            finally
            {
                //20231104 改在 finally 判斷要不要處理 Exception 錯誤
                if (position != -1) //20231104 如果不是 -1，表示有進入其中一個 Exception
                {
                    //傳回 SQL 錯誤的字串位置、錯誤訊息，由「該 QueryEditor」將錯誤的字串標示波浪底線
                    selectionStart = selectionStart + positionOffset + position + positionAdjust;
                }

                PublishSqlExecuteErrorIfNeeded
                (
                    position,
                    accessibleDescription,
                    executedResult: string.Empty,
                    errorCode,
                    errorMessage,
                    errorHint,
                    selectionStart,
                    sql,
                    errorMessage2
                );

                NotifyQueryCompleted();
            }
        }

        private static ProgressDialog CreateExecuteNonQueryProgressDialog(string titleName, int totalQty)
        {
            return new ProgressDialog
            {
                TitleName = titleName,
                TotalQty = totalQty,
                Interval = 500,
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopLevel = true,
                StartPosition = FormStartPosition.CenterScreen
            };
        }

        private static void CloseProgressDialog(ProgressDialog form)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            try
            {
                form.Close();
            }
            catch
            {
                //ProgressDialog 只是進度提示視窗，關閉失敗不應該覆蓋原本 SQL 執行結果或錯誤
            }
        }

        public void ExecuteNonQuery(object sqlQuery)
        {
            var i = 0;
            var selectionStart = 0;
            var positionAdjust = 1;
            var affectedRows = 0;
            var positionOffset = 0;
            var batchRunQty = 0;
            var dtStartTime = DateTime.Now;
            var queryTime = string.Empty;
            var sql = string.Empty;
            var sqlExecuted = string.Empty;
            var executedResult = string.Empty;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL
            var crlf = 0;
            var queryEditor = LocalizationHelper.GetLanguageString("Query Editor", "Global", "Global", "msg", "QueryEditor", "Text");

            var position = -1;
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;
            var errorMessage2 = string.Empty;
            var seqNo = $"{DateTime.Now:_mmssfff}";

            var hasPendingTransactionAfterExecute = false;
            var isTransactionClosedBySqlCommand = false;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 15)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    int.TryParse(parts[2], out crlf);
                    sql = parts[3].TrimEnd(' ', ';', '\r', '\n');
                }

                using (var script = new SqlScript(sql))
                {
                    var result = string.Empty;

                    _command.CommandTimeout = QueryTimeout;

                    if (script.Statements.Count > 1)
                    {
                        _command.CommandType = CommandType.Text;
                        _command.Connection = _conn;

                        MyGlobal.ProgressInsertInto = 0;
                        MyGlobal.IsProgressCancel = false;

                        var executeNonQueryScripts = LocalizationHelper.GetLanguageString("Execute non-query SQL scripts", "Global", "Global", "msg", "ExecuteNonQueryScripts", "Text");

                        batchRunQty = script.Statements.Count;

                        using (var form = CreateExecuteNonQueryProgressDialog(executeNonQueryScripts, batchRunQty))
                        {
                            try
                            {
                                form.Show();

                                for (i = 0; i < batchRunQty; i++)
                                {
                                    dtStartTime = DateTime.Now;
                                    positionOffset = script.Statements[i].Offset;
                                    sqlExecuted = script.Statements[i].Text;
                                    affectedRows = 0;

                                    var statementType = script.Statements[i].StatementType.ToString();
                                    var sqlExecutedFirstWord = TextHelper.GetFirstWord(sqlExecuted.ToUpper());

                                    if (!string.Equals(statementType, "SELECT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        _command.CommandText = sqlExecuted;
                                        affectedRows = _command.ExecuteNonQuery();

                                        queryTime = MyGlobal.DateDiff(dtStartTime, DateTime.Now);
                                    }

                                    result = GetResultString(statementType, sqlExecutedFirstWord, affectedRows, out _sqlStatementType, sqlExecuted);

                                    executedResult += $"{result}\r\n";

                                    if (result == "Use script - Database changed!")
                                    {
                                        var temp01 = script.Statements[i].Text.Substring(4).Trim();
                                        var databaseName = TextHelper.GetFirstWord(temp01);

                                        executedResult = $"Database ({databaseName}) changed!\r\n";
                                        DatabaseSqlExecutor.DatabaseName = databaseName;
                                        MyGlobal.GlobalTemp2 = $"SQLServerSwitchDatabase{MyGlobal.Separator}{accessibleDescription};{databaseName}";
                                        MyGlobal.GlobalTemp3 = $"SQLServerSwitchDatabaseByUsing`{databaseName}";
                                    }

                                    DatabaseTransactionStatePolicy.Apply
                                    (
                                        DataSourceType.SqlServer,
                                        _sqlStatementType,
                                        ref hasPendingTransactionAfterExecute,
                                        ref isTransactionClosedBySqlCommand
                                    );

                                    MyGlobal.ExecuteNonQuerySqlHistoryScript = "OK";

                                    var batch = $"/*Batch {i + 1} of {batchRunQty}*/{sqlExecuted}";
                                    var seqNoTemp = $"{_sqlType}{seqNo}";

                                    JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), queryTime, queryTime, affectedRows, "Complete", result, batch, queryEditor, seqNoTemp);

                                    MyGlobal.ProgressInsertInto = i + 1;
                                    Thread.Sleep(1);

                                    if (MyGlobal.IsProgressCancel)
                                    {
                                        break;
                                    }
                                }

                                if (MyGlobal.IsProgressCancel)
                                {
                                    temp = LocalizationHelper.GetLanguageString("This operation has been cancelled.", "Global", "Global", "msg", "CancelByUser", "Text");
                                    executedResult += $"{temp}\r\n";

                                    MyGlobal.ExecuteNonQuerySqlHistoryScript = "OK";

                                    var batch = $"/*Batch {i + 1} of {batchRunQty}*/";
                                    var seqNoTemp = $"{_sqlType}_Cancel{seqNo}";

                                    JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), "00:00.001", "00:00.001", affectedRows, "Cancel", temp, batch, queryEditor, seqNoTemp);
                                }
                            }
                            finally
                            {
                                CloseProgressDialog(form);
                            }
                        }

                        executedResult = executedResult.TrimEnd('\r', '\n');
                    }
                    else //ExecuteNonQuery, 單一 SQL
                    {
                        var scriptText = script.Statements[0].Text;
                        var statementType = script.Statements[0].StatementType.ToString();

                        dtStartTime = DateTime.Now;
                        sqlExecuted = sql;

                        _command.CommandType = CommandType.Text;
                        _command.CommandText = scriptText; //20240706 只有一筆，改用 script.Statements[0].Text，因為 sql 裡面可能會包含註釋
                        _command.CommandTimeout = QueryTimeout;
                        _command.Connection = _conn;

                        affectedRows = _command.ExecuteNonQuery();
                        queryTime = MyGlobal.DateDiff(dtStartTime, DateTime.Now);
                        result = GetResultString(statementType, TextHelper.GetFirstWord(scriptText.ToUpper()), affectedRows, out _sqlStatementType, scriptText);

                        executedResult = result;

                        if (executedResult == "Use script - Database changed!")
                        {
                            var databaseName = TextHelper.GetFirstWord(scriptText.Substring(4).Trim());

                            executedResult = $"Database ({databaseName}) changed!";
                            DatabaseSqlExecutor.DatabaseName = databaseName;
                            MyGlobal.GlobalTemp2 = $"SQLServerSwitchDatabase{MyGlobal.Separator}{accessibleDescription};{databaseName}";
                            MyGlobal.GlobalTemp3 = $"SQLServerSwitchDatabaseByUsing`{databaseName}";
                        }

                        DatabaseTransactionStatePolicy.Apply
                        (
                            DataSourceType.SqlServer,
                            _sqlStatementType,
                            ref hasPendingTransactionAfterExecute,
                            ref isTransactionClosedBySqlCommand
                        );

                        MyGlobal.ExecuteNonQuerySqlHistoryScript = "OK";

                        var seqNoTemp = $"{_sqlType}{seqNo}";

                        //20241110 定義 seqNo 的值
                        JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), queryTime, queryTime, affectedRows, "Complete", result, sqlExecuted, queryEditor, seqNoTemp);
                    }
                }

                var transactionState = DatabaseTransactionStatePolicy.ResolveStateToken
                (
                    hasPendingTransactionAfterExecute,
                    isTransactionClosedBySqlCommand
                );

                //正常結束 or 取消結束：回傳異動筆數
                MyGlobal.GlobalTemp = $"SQLExecuteAffected{MyGlobal.Separator}{accessibleDescription};{executedResult};{transactionState}";
            }
            catch (ThreadAbortException)
            {
                var message = LocalizationHelper.GetLanguageString("Current operation was aborted.", "Global", "Global", "msg", "OperationAborted", "Text");

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                if (batchRunQty > 0)
                {
                    MyGlobal.ProgressInsertInto = batchRunQty; //for Batch Run
                }

                queryTime = MyGlobal.DateDiff(dtStartTime, DateTime.Now);

                position = 0;
                errorCode = string.Empty; //20220820 拿掉 ErrorCode，因為它看起來像是一串無意義的負數。
                errorHint = string.Empty;
                errorMessage = ex.Message;

                var numberName = LocalizationHelper.GetLanguageString("Number", "Global", "Global", "msg", "Number", "Text");
                var className = LocalizationHelper.GetLanguageString("Class", "Global", "Global", "msg", "Class", "Text");
                var stateName = LocalizationHelper.GetLanguageString("State", "Global", "Global", "msg", "State", "Text");

                var errorMessageFull = AnalysisErrorMessageWithLineNumber(sqlExecuted, ex.Errors, out errorMessage2, numberName, className, stateName, MyGlobal.LineName);

                if (string.IsNullOrEmpty(errorMessage2))
                {
                    errorMessageFull = AnalysisErrorMessageWithoutLineNumber(sqlExecuted, ex.Errors, out errorMessage2, numberName, className, stateName, MyGlobal.LineName);
                }

                if (!string.IsNullOrEmpty(errorMessageFull))
                {
                    errorMessage = errorMessageFull;
                }

                MyGlobal.ExecuteNonQuerySqlHistoryScript = "OK";

                var errorMessageTemp = $"ErrorMsg: {errorMessage}";
                var temp = batchRunQty == 0 ? string.Empty : $"/*Batch {i + 1} of {batchRunQty}*/";
                var batch = $"{temp}{sqlExecuted}";
                var seqNoTemp = $"{_sqlType}{seqNo}";

                //20240723 寫入 SQL 歷史記錄
                JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), queryTime, queryTime, affectedRows, "Error", errorMessageTemp, batch, queryEditor, seqNoTemp);
            }
            catch (InvalidOperationException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (ExternalException ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                position = 0;
                errorMessage = ex.Message;
            }
            finally
            {
                //20231104 改在 finally 判斷要不要處理 Exception 錯誤
                if (position != -1) //20231104 如果不是 -1，表示有進入其中一個 Exception
                {
                    //錯誤的定位點
                    selectionStart += positionOffset + position + positionAdjust;
                }

                PublishSqlExecuteErrorIfNeeded
                (
                    position,
                    accessibleDescription,
                    executedResult,
                    errorCode,
                    errorMessage,
                    errorHint,
                    selectionStart,
                    sqlExecuted,
                    errorMessage2
                );

                NotifyQueryCompleted();
            }
        }

        public int ExecuteSingleNonQuery(string sql, out string errorMessage)
        {
            bool hasPendingTransactionAfterExecute;
            bool isTransactionClosedBySqlCommand;

            return ExecuteSingleNonQuery
            (
                sql,
                out errorMessage,
                out hasPendingTransactionAfterExecute,
                out isTransactionClosedBySqlCommand
            );
        }

        public int ExecuteSingleNonQuery(string sql, out string errorMessage, out bool hasPendingTransactionAfterExecute, out bool isTransactionClosedBySqlCommand)
        {
            errorMessage = string.Empty;
            hasPendingTransactionAfterExecute = false;
            isTransactionClosedBySqlCommand = false;

            var affectedRows = 0;

            EnsureConnectionOpen();

            try
            {
                using (var script = new SqlScript(sql)) //20241110 避免傳入的 SQL 有分號，將 sql 拆解之後，取第一個 SQL
                {
                    var scriptText = script.Statements[0].Text;
                    var statementType = script.Statements[0].StatementType.ToString();

                    _command.CommandType = CommandType.Text;
                    _command.CommandText = scriptText;
                    _command.CommandTimeout = QueryTimeout;
                    _command.Connection = _conn;
                    affectedRows = _command.ExecuteNonQuery();

                    GetResultString(statementType, TextHelper.GetFirstWord(scriptText.ToUpper()), affectedRows, out _sqlStatementType, scriptText);
                    DatabaseTransactionStatePolicy.Apply
                    (
                        DataSourceType.SqlServer,
                        _sqlStatementType,
                        ref hasPendingTransactionAfterExecute,
                        ref isTransactionClosedBySqlCommand
                    );
                }
            }
            catch (SqlException ex)
            {
                errorMessage = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (ExternalException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                NotifyQueryCompleted();
            }

            return affectedRows;
        }

        public string GetResultString(string mode, string firstWord, int rowsCount, out string sqlStatementType, string sql = "")
        {
            var result = string.Empty;
            var affectedRows = $"{rowsCount} row{(rowsCount > 1 ? "s" : string.Empty)}";

            switch (mode.ToUpper())
            {
                case "SELECT":
                    {
                        _sqlType = "DML";
                        result = "Select script - process ignored! (ExecuteNonQuery)";
                        sqlStatementType = "Select";
                        break;
                    }
                case "WITH":
                    {
                        //20240818 判斷 With 後面跟著的是什麼指令
                        if (!string.IsNullOrEmpty(sql))
                        {
                            var temp = TextHelper.GetTransferString(true, sql, true, true);

                            if (temp.Contains(") DELETE FROM "))
                            {
                                _sqlType = "DML";
                                result = $"With+Delete script - process OK! {affectedRows} deleted.";
                                sqlStatementType = "With\nDelete";
                            }
                            else if (temp.Contains(") SELECT "))
                            {
                                _sqlType = "DML";
                                result = "With+Select script - process OK!";
                                sqlStatementType = "With\nSelect";
                            }
                            else if (temp.Contains(") UPDATE "))
                            {
                                _sqlType = "DML";
                                result = $"With+Update script - process OK! {affectedRows} updated.";
                                sqlStatementType = "With\nUpdate";
                            }
                            else if (temp.Contains(") INSERT INTO "))
                            {
                                _sqlType = "DML";
                                result = $"With+Insert script - process OK! {affectedRows} inserted.";
                                sqlStatementType = "With\nInsert";
                            }
                            else
                            {
                                _sqlType = "With";
                                result = "With script - process OK!";
                                sqlStatementType = "With";
                            }
                        }
                        else
                        {
                            _sqlType = "With";
                            result = "With script - process OK!";
                            sqlStatementType = "With";
                        }

                        break;
                    }
                case "CREATE":
                    {
                        _sqlType = "DDL";
                        result = "Create script - process OK!";
                        sqlStatementType = "Create";
                        break;
                    }
                case "ALTER":
                    {
                        _sqlType = "DDL";
                        result = "Alter script - process OK!";
                        sqlStatementType = "Alter";
                        break;
                    }
                case "BATCH":
                    {
                        _sqlType = "Batch";
                        result = "Batch script - process OK!";
                        sqlStatementType = "Batch";
                        break;
                    }
                case "DROP":
                    {
                        _sqlType = "DDL";
                        result = "Drop script - process OK!";
                        sqlStatementType = "Drop";
                        break;
                    }
                case "EXEC":
                    {
                        _sqlType = "Exec";
                        result = "Exec script - process OK!";
                        sqlStatementType = "Exec";
                        break;
                    }
                case "EXECUTE":
                    {
                        _sqlType = "Execute";
                        result = "Execute script - process OK!";
                        sqlStatementType = "Execute";
                        break;
                    }
                case "EXTENDED":
                    {
                        _sqlType = "Extended";
                        result = "Extended script - process OK!";
                        sqlStatementType = "Extended";
                        break;
                    }
                case "TRUNCATE":
                    {
                        _sqlType = "DDL";
                        result = "Truncate script - process OK!";
                        sqlStatementType = "Truncate";
                        break;
                    }
                case "ROLLBACK":
                    {
                        _sqlType = "TCL";
                        result = "Rollback script - process OK!";
                        sqlStatementType = "Rollback";
                        break;
                    }
                case "COMMIT":
                    {
                        _sqlType = "TCL";
                        result = "Commit script - process OK!";
                        sqlStatementType = "Commit";
                        break;
                    }
                case "UNKNOWN":
                    {
                        switch (firstWord.ToUpper())
                        {
                            case "GRANT":
                                {
                                    _sqlType = "DCL";
                                    result = "Grant script - process OK!";
                                    sqlStatementType = "Grant";
                                    break;
                                }
                            case "REVOKE":
                                {
                                    _sqlType = "DCL";
                                    result = "Revoke script - process OK!";
                                    sqlStatementType = "Revoke";
                                    break;
                                }
                            case "SAVEPOINT":
                                {
                                    _sqlType = "TCL";
                                    result = "Savepoint script - process OK!";
                                    sqlStatementType = "Savepoint";
                                    break;
                                }
                            case "MERGE":
                                {
                                    _sqlType = "DML";
                                    result = "Merge script - process OK!";
                                    sqlStatementType = "Merge";
                                    break;
                                }
                            case "CALL":
                                {
                                    _sqlType = "DML";
                                    result = "Call script - process OK!";
                                    sqlStatementType = "Call";
                                    break;
                                }
                            case "EXPLAIN":
                                {
                                    _sqlType = "DML";
                                    result = "Explain script - process OK!";
                                    sqlStatementType = "Explain";
                                    break;
                                }
                            case "LOCK":
                                {
                                    _sqlType = "DML";
                                    result = "Lock script - process OK!";
                                    sqlStatementType = "Lock";
                                    break;
                                }
                            case "COMMENT":
                                {
                                    _sqlType = "DDL";
                                    result = "Comment script - process OK!";
                                    sqlStatementType = "Comment";
                                    break;
                                }
                            case "USE":
                                {
                                    _sqlType = "Use";
                                    result = "Use script - Database changed!";
                                    sqlStatementType = "Use";
                                    break;
                                }
                            default:
                                {
                                    _sqlType = firstWord;
                                    result = $"{firstWord} script - process OK!";
                                    sqlStatementType = "Unknown";
                                    break;
                                }
                        }

                        break;
                    }
                case "INSERT":
                    {
                        _sqlType = "DML";
                        result = $"{affectedRows} inserted.";
                        sqlStatementType = "Insert";
                        break;
                    }
                case "UPDATE":
                    {
                        _sqlType = "DML";
                        result = $"{affectedRows} updated.";
                        sqlStatementType = "Update";
                        break;
                    }
                case "DELETE":
                    {
                        _sqlType = "DML";
                        result = $"{affectedRows} deleted.";
                        sqlStatementType = "Delete";
                        break;
                    }
                default:
                    {
                        _sqlType = firstWord;
                        result = $"{affectedRows} {mode} (undefineMode@JasonQuery)";
                        sqlStatementType = "Unknown";
                        break;
                    }
            }

            return result;
        }

        public DataTable ExecuteQueryToDataTable(string sql, bool showAlertOnError = true)
        {
            var queryWatch = new Stopwatch();
            var rowsCount = 0;
            var hasError = false;
            var dtData = new DataTable();
            var errorMessage = string.Empty;

            try
            {
                EnsureConnectionOpen();

                queryWatch.Start();

                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                using (var dataReader = _command.ExecuteReader(CommandBehavior.Default))
                {
                    dtData.Load(dataReader);
                }

                rowsCount = dtData.Rows.Count;
            }
            catch (ThreadAbortException)
            {
                hasError = true;

                var message = LocalizationHelper.GetLanguageString("Current operation was aborted.", "Global", "Global", "msg", "OperationAborted", "Text");

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) //如果沒有權限也會跑來這裡
            {
                hasError = true;
                errorMessage = $"{ex.Message}\r\n\r\n{sql}";
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"{ex.Message}\r\n\r\n{sql}";
            }
            finally
            {
                if (queryWatch.IsRunning)
                {
                    queryWatch.Stop();
                }

                if (!string.IsNullOrEmpty(JasonQueryRepository.DbMotherPid)) //20250510 JasonQueryRepository.DbMotherPid 為空值，表示這是新建立的連線，但尚未保存，不需要「添加 SQL 歷史記錄」
                {
                    var elapsed = queryWatch.Elapsed;
                    var time = string.Empty;

                    if (elapsed.TotalMilliseconds < 1 && elapsed.TotalMilliseconds > 0)
                    {
                        //小於 1 毫秒，直接顯示為微秒級別的值
                        time = "< 0.001";
                    }
                    else
                    {
                        time = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}";
                    }

                    var result = hasError ? "Error" : "Complete";

                    //20250217 添加 SQL 歷史記錄
                    JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), time, time, rowsCount, result, errorMessage, sql, "JQ_SYSTEM");
                }

                if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return hasError ? null : dtData;
        }

        private static bool TryExtractQuotedToken(string message, string symbolFrom, string symbolTo, out string token)
        {
            token = string.Empty;

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(symbolFrom) || string.IsNullOrEmpty(symbolTo))
            {
                return false;
            }

            var fromIndex = message.IndexOf(symbolFrom, StringComparison.Ordinal);

            if (fromIndex < 0)
            {
                return false;
            }

            var from = fromIndex + symbolFrom.Length;
            var to = message.IndexOf(symbolTo, from, StringComparison.Ordinal);

            if (to <= from)
            {
                return false;
            }

            token = message.Substring(from, to - from);

            return !string.IsNullOrEmpty(token);
        }

        private static bool IsSqlServerColumnPrefixError(string message)
        {
            return !string.IsNullOrEmpty(message)
                   && message.StartsWith("The column prefix ", StringComparison.OrdinalIgnoreCase)
                   && message.EndsWith(" does not match with a table name or alias name used in the query.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsInsideSingleQuotedString(string text, int index)
        {
            if (string.IsNullOrEmpty(text) || index <= 0)
            {
                return false;
            }

            var inside = false;

            for (var i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] != '\'')
                {
                    continue;
                }

                //處理 SQL 字串中的 escaped quote：''
                if (inside && i + 1 < index && i + 1 < text.Length && text[i + 1] == '\'')
                {
                    i++;
                    continue;
                }

                inside = !inside;
            }

            return inside;
        }

        private static bool TryFindSqlServerErrorTokenPosition(string lineText, string message, string token, int startPosition, out int position, out int matchedLength)
        {
            position = -1;
            matchedLength = 0;

            if (string.IsNullOrEmpty(lineText) || string.IsNullOrEmpty(token))
            {
                return false;
            }

            startPosition = Math.Max(0, Math.Min(startPosition, lineText.Length));

            if (IsSqlServerColumnPrefixError(message))
            {
                var prefixToken = $"{token}.";

                if (TryFindSqlServerErrorTokenPositionCore(lineText, prefixToken, startPosition, out position, out matchedLength))
                {
                    return true;
                }
            }

            return TryFindSqlServerErrorTokenPositionCore(lineText, token, startPosition, out position, out matchedLength);
        }

        private static bool TryFindSqlServerErrorTokenPositionCore(string lineText, string token, int startPosition, out int position, out int matchedLength)
        {
            position = -1;
            matchedLength = 0;

            if (string.IsNullOrEmpty(lineText) || string.IsNullOrEmpty(token))
            {
                return false;
            }

            var searchStart = Math.Max(0, Math.Min(startPosition, lineText.Length));

            while (searchStart <= lineText.Length)
            {
                var index = lineText.IndexOf(token, searchStart, StringComparison.Ordinal);

                if (index < 0)
                {
                    return false;
                }

                //只避開 SQL Server 的單引號字串 literal
                //不避開雙引號，因為 SQL Server 的 "xxx" 常代表 identifier，正是 Invalid column name 的來源
                if (!IsInsideSingleQuotedString(lineText, index))
                {
                    position = index;
                    matchedLength = token.Length;
                    return true;
                }

                searchStart = index + token.Length;
            }

            return false;
        }

        private static string SortSqlServerSecondaryErrorMessage(string errorMessage2)
        {
            if (string.IsNullOrEmpty(errorMessage2))
            {
                return string.Empty;
            }

            var dtLines = new DataTable();

            dtLines.Columns.Add("Line", typeof(int));
            dtLines.Columns.Add("Data");

            var parts = errorMessage2.Split(new[] { MyGlobal.SeparatorPlus3 }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var fields = part.Split(new[] { MyGlobal.SeparatorPlus4 }, StringSplitOptions.None);

                if (fields.Length < 3)
                {
                    continue;
                }

                if (!int.TryParse(fields[0], out var line))
                {
                    continue;
                }

                var row = dtLines.NewRow();

                row["Line"] = line;
                row["Data"] = part;
                dtLines.Rows.Add(row);
            }

            if (dtLines.Rows.Count == 0)
            {
                return string.Empty;
            }

            var dv = dtLines.DefaultView;

            dv.Sort = "Line, Data";

            var dtSorted = dv.ToTable();
            var sb = new StringBuilder();

            foreach (DataRow row in dtSorted.Rows)
            {
                sb.Append(row.GetSafeString("Data"));
                sb.Append(MyGlobal.SeparatorPlus3);
            }

            return sb.ToString();
        }

        //使用 Error Line Number 定位字串位置，抓出錯誤字串出現的位置
        private static string AnalysisErrorMessageWithLineNumber(string sql, SqlErrorCollection sqlError, out string errorMessage2, string numberMessage, string classMessage, string stateMessage, string lineNumber)
        {
            errorMessage2 = string.Empty;

            var errorMessageFull = string.Empty;
            var parts = (sql ?? string.Empty).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var dtLines = new DataTable();

            dtLines.Columns.Add("Line", typeof(int));
            dtLines.Columns.Add("Count", typeof(int));
            dtLines.Columns.Add("Position", typeof(int));

            try
            {
                if (sqlError == null || sqlError.Count <= 0)
                {
                    return string.Empty;
                }

                for (var j = 0; j < sqlError.Count; j++)
                {
                    var number = sqlError[j].Number;
                    var lineNumber2 = sqlError[j].LineNumber;
                    var state = sqlError[j].State;
                    var byteClass = sqlError[j].Class;
                    var message = sqlError[j].Message ?? string.Empty;

                    if (number != 16945 && state != 2)
                    {
                        errorMessageFull += $"{numberMessage} {number}, {classMessage} {byteClass}, {stateMessage} {state}, {lineNumber} {lineNumber2}\r\n{message}\r\n";
                    }

                    if (lineNumber2 <= 0 || lineNumber2 > parts.Length)
                    {
                        continue;
                    }

                    var drFilter = dtLines.Select($"Line = {lineNumber2}");
                    var startPosition = 0;

                    if (drFilter.Length > 0)
                    {
                        startPosition = (int)drFilter[0]["Position"];
                    }
                    else
                    {
                        var row = dtLines.NewRow();

                        row["Line"] = lineNumber2;
                        row["Count"] = 0;
                        row["Position"] = 0;
                        dtLines.Rows.Add(row);

                        drFilter = dtLines.Select($"Line = {lineNumber2}");
                    }

                    var lineText = parts[lineNumber2 - 1];

                    for (var i = 0; i < 3; i++) //處理關鍵字的已知三種情境：單引號、雙引號、特殊中文符號
                    {
                        string symbolFrom;
                        string symbolTo;

                        if (i == 0)
                        {
                            symbolFrom = "'";
                            symbolTo = "'";
                        }
                        else if (i == 1)
                        {
                            symbolFrom = "\"";
                            symbolTo = "\"";
                        }
                        else
                        {
                            symbolFrom = "“"; //20250515 SQL Server 11 在簡體中文環境下，會出現這兩個符號
                            symbolTo = "”";
                        }

                        if (!TryExtractQuotedToken(message, symbolFrom, symbolTo, out var tempWord))
                        {
                            continue;
                        }

                        if (!TryFindSqlServerErrorTokenPosition(lineText, message, tempWord, startPosition, out var tempValue, out var matchedLength))
                        {
                            continue;
                        }

                        if (drFilter.Length > 0)
                        {
                            drFilter[0]["Position"] = tempValue + matchedLength;
                        }

                        errorMessage2 += $"{lineNumber2}{MyGlobal.SeparatorPlus4}{tempWord}{MyGlobal.SeparatorPlus4}{tempValue}{MyGlobal.SeparatorPlus3}";
                    }
                }

                errorMessage2 = SortSqlServerSecondaryErrorMessage(errorMessage2);
                errorMessageFull = errorMessageFull.TrimEnd('\r', '\n');
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return errorMessageFull;
        }

        //無法透過 Error Line Number 定位字串位置，改抓錯誤字串第一次出現的位置
        private static string AnalysisErrorMessageWithoutLineNumber(string sql, SqlErrorCollection sqlError, out string errorMessage2, string numberMessage, string classMessage, string stateMessage, string lineNumber)
        {
            errorMessage2 = string.Empty;

            var errorMessageFull = string.Empty;
            var parts = (sql ?? string.Empty).Split(new[] { "\r\n" }, StringSplitOptions.None);

            try
            {
                if (sqlError == null || sqlError.Count <= 0)
                {
                    return string.Empty;
                }

                for (var j = 0; j < sqlError.Count; j++)
                {
                    var number = sqlError[j].Number;
                    var lineNumber2 = sqlError[j].LineNumber;
                    var state = sqlError[j].State;
                    var byteClass = sqlError[j].Class;
                    var message = sqlError[j].Message ?? string.Empty;

                    if (number != 16945 && state != 2)
                    {
                        errorMessageFull += $"{numberMessage} {number}, {classMessage} {byteClass}, {stateMessage} {state}, {lineNumber} {lineNumber2}\r\n{message}\r\n";
                    }

                    for (var i = 0; i < 3; i++) //處理關鍵字的已知三種情境：單引號、雙引號、特殊中文符號
                    {
                        string symbolFrom;
                        string symbolTo;

                        if (i == 0)
                        {
                            symbolFrom = "'";
                            symbolTo = "'";
                        }
                        else if (i == 1)
                        {
                            symbolFrom = "\"";
                            symbolTo = "\"";
                        }
                        else
                        {
                            symbolFrom = "“"; //20250515 SQL Server 11 在簡體中文環境下，會出現這兩個符號
                            symbolTo = "”";
                        }

                        if (!TryExtractQuotedToken(message, symbolFrom, symbolTo, out var tempWord))
                        {
                            continue;
                        }

                        for (var k = 0; k < parts.Length; k++)
                        {
                            if (!TryFindSqlServerErrorTokenPosition(parts[k], message, tempWord, 0, out var tempValue, out _))
                            {
                                continue;
                            }

                            errorMessage2 += $"{k + 1}{MyGlobal.SeparatorPlus4}{tempWord}{MyGlobal.SeparatorPlus4}{tempValue}{MyGlobal.SeparatorPlus3}";
                            break;
                        }
                    }
                }

                errorMessage2 = SortSqlServerSecondaryErrorMessage(errorMessage2);
                errorMessageFull = errorMessageFull.TrimEnd('\r', '\n');
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return errorMessageFull;
        }

        private void PublishSqlExecuteErrorIfNeeded(int errorPosition, string accessibleDescription, string executedResult, string errorCode,
                                                    string errorMessage, string errorHint, int finalPosition, string executedSql, string errorMessage2)
        {
            if (errorPosition == -1)
            {
                return;
            }

            SqlExecuteErrorPayloadBuilder.Publish
            (
                new SqlExecuteErrorPayload
                {
                    AccessibleDescription = accessibleDescription,
                    ExecutedResult = executedResult,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage,
                    ErrorHint = errorHint,
                    Position = finalPosition,
                    ExecutedSql = executedSql,
                    SecondaryErrorMessage = errorMessage2,
                    IncludeSecondaryErrorMessage = true
                }
            );
        }

        public void EnsureConnectionOpen()
        {
            EnsureConnectionOpen(DatabaseSqlExecutor.DbConnectionString);
        }

        public void EnsureConnectionOpen(string connectionString)
        {
            DatabaseConnectionGuard.EnsureOpen
            (
                GetState,
                () => ConnectTo(connectionString)
            );
        }

        public bool TryEnsureConnectionOpen(out string errorMessage)
        {
            return TryEnsureConnectionOpen(DatabaseSqlExecutor.DbConnectionString, out errorMessage);
        }

        public bool TryEnsureConnectionOpen(string connectionString, out string errorMessage)
        {
            return DatabaseConnectionGuard.TryEnsureOpen
            (
                GetState,
                () => ConnectTo(connectionString),
                out errorMessage
            );
        }

        private bool TryDisposeAndClearDataReader()
        {
            var reader = DataReader;

            if (reader == null)
            {
                return true;
            }

            try
            {
                reader.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DataReader = null;
            }
        }

        private bool TryEnsureTransactionOpen(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                EnsureConnectionOpen();

                if (_conn == null || _conn.State != ConnectionState.Open)
                {
                    errorMessage = "SQL Server connection is not open.";
                    return false;
                }

                if (_transaction == null)
                {
                    _transaction = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                return true;
            }
            catch (SqlException ex)
            {
                errorMessage = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"ErrorMsg: {ex.Message}";
                return false;
            }
        }

        private void NotifyQueryCompleted()
        {
            QueryCompleted?.Invoke();
        }
    }
}
