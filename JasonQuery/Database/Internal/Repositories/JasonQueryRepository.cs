using JasonQuery.Core.Config;
using JasonQuery.Core.Security.Database;
using JasonQuery.Core.Text;
using JasonQuery.UI.Services;
using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Database.Internal.Repositories
{
    public static class JasonQueryRepository //for JasonQuery.db
    {
        public static string DbConnectionString = string.Empty;
        public static string DbConnectionPassword = LegacyDatabaseSecurity.LegacyDefaultDatabasePassword; //Legacy default; V2 overwrites this at startup.
        public static string DbMotherPid = string.Empty;
        public static string DbFileName = string.Empty;

        //資料庫初始化
        private static SQLiteConnection OleDbOpenConn()
        {
            var connection = new SQLiteConnection { ConnectionString = DbConnectionString };

            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.SetPassword(DbConnectionPassword);
                connection.Open();
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return connection;
        }

        //V2 startup：使用已解析完成的資料庫密碼開啟並驗證 JasonQuery.db
        //呼叫端負責 Dispose；成功回傳時 connection 必須保持 Open，供後續安全啟動 gate 使用
        internal static SQLiteConnection OpenValidatedCurrentDatabaseConnection()
        {
            var connection = new SQLiteConnection { ConnectionString = DbConnectionString };

            try
            {
                connection.SetPassword(DbConnectionPassword);
                connection.Open();

                if (connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("The JasonQuery database connection did not open.");
                }

                using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                {
                    command.ExecuteScalar();
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        //Legacy：驗證舊版預設密碼或舊版自訂密碼
        public static bool CheckDBPassword(string password)
        {
            var legacyDatabasePassword = string.IsNullOrWhiteSpace(password) ? DbConnectionPassword : LegacyDatabaseSecurity.CreateCustomDatabasePassword(password);

            return CanOpenDatabase(legacyDatabasePassword);
        }

        //V2：驗證目前已解析完成的資料庫密碼，不套用任何 Legacy prefix/suffix
        public static bool CheckCurrentDatabasePassword()
        {
            return CanOpenDatabase(DbConnectionPassword);
        }

        private static bool CanOpenDatabase(string databasePassword)
        {
            using (var connection = new SQLiteConnection { ConnectionString = DbConnectionString })
            {
                try
                {
                    connection.SetPassword(databasePassword);
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                    {
                        command.ExecuteScalar();
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        //變更密碼！
        public static string ResetDBPassword(string oldPassword, string newPassword, bool isUseDefaultPassword)
        {
            string targetDatabasePassword;

            try
            {
                targetDatabasePassword = isUseDefaultPassword ? LegacyDatabaseSecurity.LegacyDefaultDatabasePassword : LegacyDatabaseSecurity.CreateCustomDatabasePassword(newPassword);
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }

            try
            {
                using (var connection = new SQLiteConnection { ConnectionString = DbConnectionString })
                {
                    connection.SetPassword(oldPassword);
                    connection.Open();
                    connection.ChangePassword(targetDatabasePassword);
                }
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }

            if (CanOpenDatabase(targetDatabasePassword))
            {
                DbConnectionPassword = targetDatabasePassword;
                return string.Empty;
            }

            var restoreResult = TryRestoreDatabasePassword(targetDatabasePassword, oldPassword);

            if (string.IsNullOrEmpty(restoreResult))
            {
                DbConnectionPassword = oldPassword;
                return "The database password change could not be validated. The previous password was restored.";
            }

            return "The database password change could not be validated, and restoring the previous password also failed. " + restoreResult;
        }

        private static string TryRestoreDatabasePassword(string currentPassword, string previousPassword)
        {
            try
            {
                using (var connection = new SQLiteConnection { ConnectionString = DbConnectionString })
                {
                    connection.SetPassword(currentPassword);
                    connection.Open();
                    connection.ChangePassword(previousPassword);
                }

                return CanOpenDatabase(previousPassword) ? string.Empty : "The previous database password could not be validated after restoration.";
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }
        }

        //取得資料表:
        public static DataTable ExecQuery(string sql)
        {
            var connection = OleDbOpenConn();
            var dataTable = new DataTable();
            var dataAdapter = new SQLiteDataAdapter(sql, connection);
            var dataSet = new DataSet();

            try
            {
                dataSet.Clear();
                dataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            return dataTable;
        }

        //對資料表進行新增、修改及刪除等功能:
        public static void ExecNonQuery(string sql, bool showAlertOnError = true)
        {
            var connection = OleDbOpenConn();

            try
            {
                var command = new SQLiteCommand(sql, connection);

                command.ExecuteNonQuery();

                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                var message = ExceptionDialogService.BuildMessage(ex);

                if (showAlertOnError)
                {
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Credential, runtime write path：敏感欄位一律以 SQLite parameter 寫入，避免 logical password 被拼接進 SQL literal
        public static void ExecNonQuery(string sql, SQLiteParameter[] parameters, bool showAlertOnError = true)
        {
            var connection = OleDbOpenConn();

            try
            {
                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                var message = ExceptionDialogService.BuildMessage(ex);

                if (showAlertOnError)
                {
                    MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.Dispose();
            }
        }

        public static string BatchDeleteRecord(string sqls)
        {
            var result = string.Empty;
            var connection = OleDbOpenConn();

            try
            {
                var sqlStatements = sqls.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var sqlStatement in sqlStatements)
                {
                    var command = new SQLiteCommand(sqlStatement, connection);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                result = ExceptionDialogService.BuildMessage(ex);
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            return result;
        }

        public static void UpdateSqlHistory(string motherPid, string executionDate, string executionTime, string queryTime, int rows, string result, string message, string sql, string operationObject, string seqNo = "")
        {
            result = result.Replace("'", "''");
            message = message.Replace("'", "''");
            sql = sql.Replace("'", "''");
            operationObject = operationObject.Replace("'", "''");
            seqNo = seqNo.Replace("'", "''");

            var isStart3Dash = sql.StartsWith("---", StringComparison.Ordinal);
            var isContainSeparator3s = sql.Contains(MyGlobal.Separator3s);
            var isContainSeparator7 = sql.Contains(MyGlobal.Separator7);

            if (isStart3Dash && isContainSeparator3s && isContainSeparator7)
            {
                var messageTemp = string.IsNullOrWhiteSpace(message) ? string.Empty : $"\r\n\r\n{message}";
                var sqlTemp = TextHelper.GetStringBetween2(sql, MyGlobal.Separator3s, MyGlobal.Separator7, true);

                sql = sql.Replace($"{MyGlobal.Separator3s}{sqlTemp}{MyGlobal.Separator7}", string.Empty);
                message = $"--{sqlTemp}{messageTemp}";
            }

            var sbSql = new StringBuilder();

            sbSql.AppendLine("INSERT INTO SqlHistory");
            sbSql.AppendLine("       (MPID, ExecutionDate, ExecutionTime, QueryTime, Rows, Result, Message, SQL, O1, O2)");
            sbSql.AppendLine($"VALUES ({motherPid}, '{executionDate}', '{executionTime}', '{queryTime}', {rows}, '{result}', '{message}', '{sql}', '{operationObject}', '{seqNo}')");

            var sqlFinal = sbSql.ToString();

            ExecNonQuery(sqlFinal);
        }

        public static void UpdateSqlHistory(string keyId, int count)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("Update SqlHistory");
            sbSql.AppendLine($"   SET Rows = {count}");
            sbSql.AppendLine($" WHERE KeyID = '{keyId}'");

            var sql = sbSql.ToString();

            ExecNonQuery(sql);
        }

        public static string GetSettingValue(string attributeKey, string attributeName, string defaultValue = "")
        {
            var motherPid = string.IsNullOrWhiteSpace(DbMotherPid) ? "0" : DbMotherPid;

            if (attributeKey == "GlobalConfig")
            {
                motherPid = "0";
            }

            var safeAttributeKey = (attributeKey ?? string.Empty).Replace("'", "''");
            var safeAttributeName = (attributeName ?? string.Empty).Replace("'", "''");
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {motherPid}");
            sbSql.AppendLine($"   AND AttributeKey = '{safeAttributeKey}'");
            sbSql.Append($"   AND AttributeName = '{safeAttributeName}'");

            var result = ExecQuery(sbSql.ToString());

            if (result == null || result.Rows.Count == 0 || result.Rows[0]["AttributeValue"] == DBNull.Value)
            {
                return defaultValue ?? string.Empty;
            }

            return result.Rows[0]["AttributeValue"].ToString();
        }

        public static void UpdateSetting(string attributeKey, string attributeName, string attributeValue, bool isInsertDirectly = false, bool isAttributeText = false)
        {
            var fieldName = "AttributeValue";
            var motherPid = string.IsNullOrWhiteSpace(DbMotherPid) ? "0" : DbMotherPid;
            var dateTimeNow = MyGlobal.DateTimeNow();

            if (attributeName == "CheckForUpdateDays")
            {
                dateTimeNow = DateTime.Now.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture);
            }

            if (attributeKey == "GlobalConfig")
            {
                motherPid = "0";
            }

            attributeValue = attributeValue.Replace("'", "''");

            var sql = string.Empty;
            var sbSql = new StringBuilder();

            if (isInsertDirectly) //Auto Replace & Auto Complete 必須先刪除後再 Insert，才不會殘留舊資料
            {
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue, AttributeDate)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {motherPid}, '{attributeKey}', '{attributeName}', '{attributeValue}', '{dateTimeNow}')");

                sql = sbSql.ToString();
                ExecNonQuery(sql);
            }
            else
            {
                if (isAttributeText)
                {
                    fieldName = "AttributeText";
                }

                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {motherPid}");
                sbSql.AppendLine($"   AND AttributeKey = '{attributeKey}'");
                sbSql.Append($"   AND AttributeName = '{attributeName}'");

                sql = sbSql.ToString();

                //依據資料是否存在，進行 Update / Insert
                var dataTable = ExecQuery(sql);

                if (dataTable?.Rows.Count > 0)
                {
                    sbSql.Clear();

                    if (attributeName == "CheckForUpdateDays") //不更新日期欄位，避免異常，每次都影響檢查更新的判斷
                    {
                        sbSql.AppendLine("UPDATE SystemConfig");
                        sbSql.AppendLine($"   SET {fieldName} = '{attributeValue}'");
                        sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                        sbSql.AppendLine($"   AND MPID = {motherPid}");
                        sbSql.AppendLine($"   AND AttributeKey = '{attributeKey}'");
                        sbSql.Append($"   AND AttributeName = '{attributeName}'");

                        sql = sbSql.ToString();
                    }
                    else
                    {
                        sbSql.AppendLine("UPDATE SystemConfig");
                        sbSql.AppendLine($"   SET {fieldName} = '{attributeValue}', AttributeDate = '{dateTimeNow}'");
                        sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                        sbSql.AppendLine($"   AND MPID = {motherPid}");
                        sbSql.AppendLine($"   AND AttributeKey = '{attributeKey}'");
                        sbSql.Append($"   AND AttributeName = '{attributeName}'");

                        sql = sbSql.ToString();
                    }

                    ExecNonQuery(sql);
                }
                else
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine($"       (DomainUser, MPID, AttributeKey, AttributeName, {fieldName}, AttributeDate)");
                    sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {motherPid}, '{attributeKey}', '{attributeName}', '{attributeValue}', '{dateTimeNow}')");

                    sql = sbSql.ToString();
                    ExecNonQuery(sql);
                }
            }
        }
    }
}
