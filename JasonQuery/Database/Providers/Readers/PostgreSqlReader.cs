using Devart.Data.PostgreSql;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Diagnostics.PostgreSql;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

//Devart PostgreSQL free version does not provide PgSqlScript.
//Use OracleScript only as a statement splitter.

namespace JasonQuery.Database.Providers.Readers
{
    public class PostgreSqlReader
    {
        public delegate void QueryCompletedEventHandler();
        public event QueryCompletedEventHandler QueryCompleted;
        public PgSqlDataReader DataReader { get; private set; }
        public DataTable dtQuerySchema { get; set; }
        public int QueryTimeout { get; set; }

        private PgSqlConnection _conn;
        private PgSqlTransaction _transaction;
        private PgSqlCommand _command;
        private string _sqlType = string.Empty;
        private string _sqlStatementType = string.Empty;


        public string ConnectTo(string connectionString)
        {
            var errorMessage = string.Empty;

            _command = new PgSqlCommand();
            _conn = new PgSqlConnection(connectionString);

            try
            {
                _conn.Open();
                _transaction = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (PgSqlException ex)
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
            catch (PgSqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                if (ex.Message != "This PgSqlTransaction has completed; it is no longer usable.") //出現此訊息，實際還是有 Commit 成功！
                {
                    result = $"ErrorMsg: {ex.Message}";
                }
            }

            return result;
        }

        public string Rollback()
        {
            var result = string.Empty;

            EnsureConnectionOpen();

            try
            {
                _transaction?.Rollback();
                _transaction?.Dispose();
                _transaction = null;
            }
            catch (PgSqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                if (ex.Message != "This PgSqlTransaction has completed; it is no longer usable.")
                {
                    result = $"ErrorMsg: {ex.Message}";
                }
            }

            return result;
        }

        public string SavePoint(string savePoint, string saveSqlHistory = "")
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
            catch (PgSqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                if (ex.Message != "This PgSqlTransaction has completed; it is no longer usable.") //出現此訊息，實際還是有 Commit 成功！
                {
                    result = $"ErrorMsg: {ex.Message}";
                }
            }

            return result;
        }

        public string RollbackPoint(string savePoint, string saveSqlHistory = "")
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
            catch (PgSqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                if (ex.Message != "This PgSqlTransaction has completed; it is no longer usable.") //出現此訊息，實際還是有 Commit 成功！
                {
                    result = $"ErrorMsg: {ex.Message}";
                }
                else
                {
                    result = "ERROR: no such savepoint\r\nSQL State: 3B001";
                }
            }

            return result;
        }

        public string ReleasePoint(string savePoint, string saveSqlHistory = "")
        {
            var result = string.Empty;

            try
            {
                _transaction.Release(savePoint);
            }
            catch (PgSqlException ex)
            {
                result = $"ErrorCode: {ex.ErrorCode}\r\nErrorMsg: {ex.Message}";
            }
            catch (Exception ex)
            {
                if (ex.Message != "This PgSqlTransaction has completed; it is no longer usable.") //出現此訊息，實際還是有 Commit 成功！
                {
                    result = $"ErrorMsg: {ex.Message}";
                }
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
            var positionAdjust = 0;
            var positionOffset = 0;
            var sql = string.Empty;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL

            //20231104 將以下變數抽出，以利在 try/catch 的 finally 中處理
            var position = -1;
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 20)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    sql = parts[2].TrimEnd(' ', ';', '\r', '\n');
                    int.TryParse(parts[3], out startRow);
                    int.TryParse(parts[4], out pageLength);
                }

                using (var script = new Devart.Data.Oracle.OracleScript(sql))
                {
                    sql = Regex.Replace(script.Statements[0].Text, @"(?<!\r)\n", "\r\n"); //經過 OracleScript(sql) 解析的 SQL 指令，部份換行符號會被替換成 \n，故此處要統一替換為 \r\n
                    positionOffset = script.Statements[0].Offset; //去掉註解後，真正要執行的 SQL 指令
                }

                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.FetchSize = 1000;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                var useSinglePassReader = DatabaseLockingQueryExecutionPolicy.ShouldUseSinglePassReader
                (
                    DataSourceType.PostgreSql,
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

                    dtQuerySchema =
                        DataReader.GetSchemaTable()?.Copy();
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

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PgSqlException ex)
            {
                position = ex.Position;
                errorMessage = ex.Message;

                if (errorMessage == "Unexpected server response.")
                {
                    errorMessage += " If you keep getting this error message, please reopen JasonQuery and try again.";
                }

                //202410123 改為判斷 "permission denied for" 開頭，先前判斷的是 "permission denied for relation"
                if ((errorMessage.StartsWith("permission denied for", StringComparison.OrdinalIgnoreCase) || errorMessage.StartsWith("current transaction is aborted, commands ignored", StringComparison.OrdinalIgnoreCase)))
                {
                    position = 0;
                }

                //判斷是否有變數沒有指定？此處 PostgreSQL 並不會回傳 ErrorCode/Hint/Position，所以只能透過字串判斷正確的位置
                if (TextHelper.CheckTextStartWithAndContains(errorMessage, "Parameter", "' is missing"))
                {
                    var temp = TextHelper.GetStringBetween(errorMessage, "'", "'");
                    var word = $":{temp}";

                    position = sql.IndexOf(word, StringComparison.Ordinal) + 1;
                }

                var index = errorMessage.IndexOf("\r\n ) AS PAGE_READ_T\r\n LIMIT", StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    errorMessage = errorMessage.Substring(0, index);
                }

                errorCode = string.IsNullOrEmpty(ex.ErrorCode) ? string.Empty : $"ErrorCode: {ex.ErrorCode}\r\nErrorPosition: {position}\r\n";
                errorHint = string.IsNullOrEmpty(ex.Hint) ? string.Empty : $"\r\n\r\n{ex.Hint}";
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
                if (position != -1)
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
                    sql
                );

                NotifyQueryCompleted();
            }
        }

        public DataTable ExecuteQueryPaged100Rows(string sql, int startRow, int pageLength, out bool isRollback, out bool isPermissionDenied, out string errorMessage, out string errorCode, out DataTable dtSchema)
        {
            errorMessage = string.Empty;
            errorCode = string.Empty;
            isRollback = false;
            isPermissionDenied = false;
            dtSchema = null;

            var rowsCount = 0;
            var dtData = new DataTable();

            EnsureConnectionOpen();

            try
            {
                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.CommandTimeout = QueryTimeout;
                _command.FetchSize = 1000;
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
            catch (PgSqlException ex)
            {
                if (ex.Message == "current transaction is aborted, commands ignored until end of transaction block")
                {
                    isRollback = true;
                    Rollback();
                }
                else if (ex.Message.StartsWith("permission denied for", StringComparison.OrdinalIgnoreCase)) //202410123 改為判斷 "permission denied for" 開頭，先前判斷的是 "permission denied for relation"
                {
                    isPermissionDenied = true;
                }

                var temp = isPermissionDenied ? $"\r\n\r\n{sql}" : string.Empty;

                errorMessage = $"{ex.Message}{temp}";
                errorCode = ex.ErrorCode;
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
                using (var command = new PgSqlCommand(sql, _conn))
                {
                    var lob = new PgSqlBlob(reader.ReadBytes((int)stream.Length));
                    var param = command.Parameters.Add(fieldName, lob);

                    affectedRows = command.ExecuteNonQuery();
                }
            }
            catch (ThreadAbortException ex)
            {
                errorMessage = ex.Message;
            }
            catch (PgSqlException ex)
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
            var positionAdjust = 0;
            var positionOffset = 0;
            var sql = string.Empty;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL

            //20231104 將以下變數抽出，以利在 try/catch 的 finally 中處理
            var position = -1;
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 10)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    sql = parts[2].TrimEnd(' ', ';', '\r', '\n');
                }

                using (var script = new Devart.Data.Oracle.OracleScript(sql))
                {
                    sql = Regex.Replace(script.Statements[0].Text, @"(?<!\r)\n", "\r\n"); //經過 OracleScript(sql) 解析的 SQL 指令，部份換行符號會被替換成 \n，故此處要統一替換為 \r\n
                    positionOffset = script.Statements[0].Offset; //去掉註解後，真正要執行的 SQL 指令
                }

                _command.CommandType = CommandType.Text;
                _command.CommandText = sql;
                _command.FetchSize = 1000;
                _command.CommandTimeout = QueryTimeout;
                _command.Connection = _conn;

                var useSinglePassReader = DatabaseLockingQueryExecutionPolicy.ShouldUseSinglePassReader
                (
                    DataSourceType.PostgreSql,
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

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PgSqlException ex)
            {
                position = ex.Position;
                errorMessage = ex.Message;

                if (errorMessage == "Unexpected server response.")
                {
                    errorMessage += " If you keep getting this error message, please reopen JasonQuery and try again.";
                }

                //錯誤的定位點若往前抓，就要調整這個地方
                //202410123 改為判斷 "permission denied for" 開頭
                if (position == -15 && (errorMessage.StartsWith("permission denied for", StringComparison.OrdinalIgnoreCase) || errorMessage.StartsWith("current transaction is aborted, commands ignored", StringComparison.OrdinalIgnoreCase)))
                {
                    position = 0;
                }

                //判斷是否有變數沒有指定？此處 PostgreSQL 並不會回傳 ErrorCode/Hint/Position，所以只能透過字串判斷正確的位置
                if (TextHelper.CheckTextStartWithAndContains(errorMessage, "Parameter", "' is missing"))
                {
                    var temp = TextHelper.GetStringBetween(errorMessage, "'", "'");
                    var word = $":{temp}";

                    position = sql.IndexOf(word, StringComparison.Ordinal) + 1;
                }

                errorCode = string.IsNullOrEmpty(ex.ErrorCode) ? string.Empty : $"ErrorCode: {ex.ErrorCode}\r\nErrorPosition: {position}\r\n";
                errorHint = string.IsNullOrEmpty(ex.Hint) ? string.Empty : $"\r\n\r\n{ex.Hint}";
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
                    sql
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
            var affectedRows = 0;
            var positionOffset = 0;
            var batchRunQty = 0;
            var dtStartTime = DateTime.Now;
            var queryTime = string.Empty;
            var sql = string.Empty;
            var sqlExecuted = string.Empty;
            var executedResult = string.Empty;
            var accessibleDescription = string.Empty; //識別從哪一個 QueryEditor 傳過來的 SQL
            var queryEditor = LocalizationHelper.GetLanguageString("Query Editor", "Global", "Global", "msg", "QueryEditor", "Text");

            var position = -1;
            var errorCode = string.Empty;
            var errorHint = string.Empty;
            var errorMessage = string.Empty;
            var seqNo = $"{DateTime.Now:_mmssfff}";

            var hasPendingTransactionAfterExecute = false;
            var isTransactionClosedBySqlCommand = false;

            EnsureConnectionOpen();

            try
            {
                var temp = Convert.ToString(sqlQuery);

                if (temp.Length > 10 && temp.Length - temp.Replace(MyGlobal.Separator5, string.Empty).Length == 10)
                {
                    var parts = temp.Replace(MyGlobal.Separator5, MyGlobal.Separator).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

                    accessibleDescription = parts[0];
                    int.TryParse(parts[1], out selectionStart);
                    sql = parts[2].TrimEnd(' ', ';', '\r', '\n');
                }

                using (var script = new Devart.Data.Oracle.OracleScript(sql))
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

                                    if (!string.Equals(statementType, "SELECT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        _command.CommandText = sqlExecuted;
                                        affectedRows = _command.ExecuteNonQuery();

                                        queryTime = MyGlobal.DateDiff(dtStartTime, DateTime.Now);
                                    }

                                    result = GetResultString(statementType, GetFirstWord(sqlExecuted), affectedRows, out _sqlStatementType, sqlExecuted);

                                    executedResult += $"{result}\r\n";

                                    DatabaseTransactionStatePolicy.Apply
                                    (
                                        DataSourceType.PostgreSql,
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

                        result = GetResultString(statementType, GetFirstWord(scriptText), affectedRows, out _sqlStatementType, scriptText); //單一 SQL
                        executedResult = result;

                        DatabaseTransactionStatePolicy.Apply
                        (
                            DataSourceType.PostgreSql,
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

                MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PgSqlException ex)
            {
                if (batchRunQty > 0)
                {
                    MyGlobal.ProgressInsertInto = batchRunQty; //for Batch Run
                }

                queryTime = MyGlobal.DateDiff(dtStartTime, DateTime.Now);

                position = ex.Position;
                errorCode = string.IsNullOrEmpty(ex.ErrorCode) ? string.Empty : $"ErrorCode: {ex.ErrorCode}\r\nErrorPosition: {position}\r\n";
                errorHint = string.IsNullOrEmpty(ex.Hint) ? string.Empty : $"{(string.IsNullOrEmpty(errorCode) ? string.Empty : "\r\n")}{ex.Hint}";

                var temp01 = !string.IsNullOrEmpty(ex.DetailMessage) ? $"{"\r\nDetailMsg: "}{ex.DetailMessage}" : string.Empty;

                errorMessage = $"{ex.Message}{temp01}";

                MyGlobal.ExecuteNonQuerySqlHistoryScript = "OK";

                var errorMessageTemp = $"ErrorMsg: {errorMessage}";
                var temp02 = batchRunQty == 0 ? string.Empty : $"/*Batch {i + 1} of {batchRunQty}*/";
                var batch = $"{temp02}{sqlExecuted}";
                var seqNoTemp = $"{_sqlType}{seqNo}";

                if (ex.ErrorCode == "22001")
                {
                    var diagnosticSql = string.IsNullOrWhiteSpace(_command?.CommandText) ? sqlExecuted : _command.CommandText;
                    var metadataProvider = new PostgreSqlColumnLengthMetadataProvider(DatabaseSqlExecutor.dtSchema);

                    var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze
                    (
                        diagnosticSql,
                        ex.Message,
                        metadataProvider
                    );

                    if (diagnostic.HasDiagnosticMessage)
                    {
                        errorMessage = $"{errorMessage}\r\n\r\n{diagnostic.Message}";
                        errorMessageTemp = $"ErrorMsg: {errorMessage}";
                    }
                }

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
                    //傳回 SQL 錯誤的字串位置、錯誤訊息，由「該 QueryEditor」將錯誤的字串標示波浪底線
                    selectionStart = selectionStart + positionOffset + position; //錯誤的定位點
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
                    sqlExecuted
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
                using (var script = new Devart.Data.Oracle.OracleScript(sql)) //20241110 避免傳入的 SQL 有分號，將 sql 拆解之後，取第一個 SQL
                {
                    var scriptText = script.Statements[0].Text;
                    var statementType = script.Statements[0].StatementType.ToString();

                    _command.CommandType = CommandType.Text;
                    _command.CommandText = scriptText;
                    _command.CommandTimeout = QueryTimeout;
                    _command.Connection = _conn;
                    affectedRows = _command.ExecuteNonQuery();

                    GetResultString(statementType, GetFirstWord(scriptText), affectedRows, out _sqlStatementType, scriptText);
                    DatabaseTransactionStatePolicy.Apply
                    (
                        DataSourceType.PostgreSql,
                        _sqlStatementType,
                        ref hasPendingTransactionAfterExecute,
                        ref isTransactionClosedBySqlCommand
                    );
                }
            }
            catch (PgSqlException ex)
            {
                var temp = !string.IsNullOrEmpty(ex.DetailMessage) ? $"\r\nDetailMsg: {ex.DetailMessage}" : string.Empty;

                errorMessage = $"{ex.Message}{temp}";
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
                            case "RENAME":
                                {
                                    _sqlType = "DDL";
                                    result = "Rename script - process OK!";
                                    sqlStatementType = "Rename";
                                    break;
                                }
                            case "REFRESH":
                                {
                                    _sqlType = "DDL";
                                    result = "Refresh script - process OK!";
                                    sqlStatementType = "Refresh";
                                    break;
                                }
                            case "CLUSTER":
                                {
                                    _sqlType = "DDL";
                                    result = "Cluster script - process OK!";
                                    sqlStatementType = "Cluster";
                                    break;
                                }
                            case "REINDEX":
                                {
                                    _sqlType = "DDL";
                                    result = "Reindex script - process OK!";
                                    sqlStatementType = "Reindex";
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
                _command.FetchSize = 1000;
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

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PgSqlException ex)
            {
                hasError = true;
                errorMessage = $"{ex.Message}\r\n\r\n{sql}";

                //20241024 有錯誤提示，緊接著 Rollback，以免造成之後的 SQL 全部無法正常執行
                Rollback();
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"{ex.Message}\r\n\r\n{sql}";

                //20241024 有錯誤提示，緊接著 Rollback，以免造成之後的 SQL 全部無法正常執行
                Rollback();
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
                    MessageBox.Show(errorMessage, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return hasError ? null : dtData;
        }

        private void PublishSqlExecuteErrorIfNeeded(int errorPosition, string accessibleDescription, string executedResult, string errorCode,
                                                    string errorMessage, string errorHint, int finalPosition, string executedSql)
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
                    ExecutedSql = executedSql
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

        private static string GetFirstWord(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return string.Empty;
            }

            var match = Regex.Match(sql.TrimStart(), @"^[A-Za-z_]+");

            return match.Success ? match.Value.ToUpperInvariant() : string.Empty;
        }

        private bool TryEnsureTransactionOpen(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                EnsureConnectionOpen();

                if (_conn == null || _conn.State != ConnectionState.Open)
                {
                    errorMessage = "PostgreSQL connection is not open.";
                    return false;
                }

                if (_transaction == null)
                {
                    _transaction = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                return true;
            }
            catch (PgSqlException ex)
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
