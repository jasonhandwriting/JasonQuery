using JasonQuery.Core.Config;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Runtime;
using JasonQuery.Database.Internal.Runtime.Modern;
using JasonQuery.UI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Database.Internal.Repositories
{
    public static class JasonQueryRepository //for JasonQuery.db
    {
        private static readonly LegacySystemDataSQLiteDatabaseRuntime LegacyRuntime = new LegacySystemDataSQLiteDatabaseRuntime();

        private static IJasonQueryDatabaseRuntime _runtime = LegacyRuntime;

        public static string DbConnectionString = string.Empty;
        public static string DbConnectionPassword = JasonQueryDbLegacySecurity.LegacyDefaultDatabasePassword; //Legacy default; V2 overwrites this at startup.
        public static string DbMotherPid = string.Empty;
        public static string DbFileName = string.Empty;

        internal static void ConfigureRuntime(IJasonQueryDatabaseRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        internal static void ResetRuntimeToLegacy()
        {
            _runtime = LegacyRuntime;
        }

        internal static void ReleaseRuntimeDatabaseHandleForSecurityTransition()
        {
            var runtime = GetRuntime();

            if (runtime is LegacySystemDataSQLiteDatabaseRuntime)
            {
                return;
            }

            if (runtime is ModernSqlCipherDatabaseRuntime modernRuntime)
            {
                modernRuntime.ReleaseDatabaseHandle();
                return;
            }

            throw new InvalidOperationException
            (
                "The configured JasonQuery internal database runtime does not support database-security transitions."
            );
        }

        private static IJasonQueryDatabaseRuntime GetRuntime()
        {
            return _runtime ?? throw new InvalidOperationException("The JasonQuery internal database runtime is not configured.");
        }

        //R4F2A startup compatibility seam：目前 credential startup gate 仍只允許 Legacy runtime 提供本機 IDbConnection。
        //R4F2C 切換 production startup 前必須移除此限制，Modern runtime 不得 fallback 到 legacy provider。
        internal static IDbConnection OpenValidatedCurrentDatabaseConnection()
        {
            var runtime = GetRuntime();

            if (!(runtime is LegacySystemDataSQLiteDatabaseRuntime legacyRuntime))
            {
                throw new InvalidOperationException
                (
                    "The current JasonQuery internal database runtime cannot expose a legacy in-process connection."
                );
            }

            return legacyRuntime.OpenValidatedConnection
            (
                DbConnectionString,
                DbConnectionPassword
            );
        }

        //Legacy：驗證舊版預設密碼或舊版自訂密碼
        public static bool CheckDBPassword(string password)
        {
            var legacyDatabasePassword = string.IsNullOrWhiteSpace(password) ? DbConnectionPassword : JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(password);

            return CanOpenDatabase(legacyDatabasePassword);
        }

        //V2：驗證目前已解析完成的資料庫密碼，不套用任何 Legacy prefix/suffix
        public static bool CheckCurrentDatabasePassword()
        {
            return CanOpenDatabase(DbConnectionPassword);
        }

        internal static bool CanOpenDatabase(string databasePassword)
        {
            return GetRuntime().CanOpenDatabase
            (
                DbConnectionString,
                databasePassword
            );
        }

        //變更密碼！
        public static string ResetDBPassword(string oldPassword, string newPassword, bool isUseDefaultPassword)
        {
            string targetDatabasePassword;

            try
            {
                targetDatabasePassword = isUseDefaultPassword ? JasonQueryDbLegacySecurity.LegacyDefaultDatabasePassword : JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(newPassword);
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }

            try
            {
                GetRuntime().ChangePassword
                (
                    DbConnectionString,
                    oldPassword,
                    targetDatabasePassword
                );
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

            var restoreResult = TryRestoreDatabasePassword
            (
                targetDatabasePassword,
                oldPassword
            );

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
                GetRuntime().ChangePassword
                (
                    DbConnectionString,
                    currentPassword,
                    previousPassword
                );

                return CanOpenDatabase(previousPassword) ? string.Empty : "The previous database password could not be validated after restoration.";
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }
        }

        public static DataTable ExecQuery(string sql)
        {
            try
            {
                return GetRuntime().ExecuteQuery
                (
                    DbConnectionString,
                    DbConnectionPassword,
                    sql
                );
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
                return new DataTable();
            }
        }

        //對資料表進行新增、修改及刪除等功能:
        public static void ExecNonQuery(string sql, bool showAlertOnError = true)
        {
            ExecNonQuery
            (
                sql,
                null,
                showAlertOnError
            );
        }

        //Credential, runtime write path：敏感欄位一律透過 provider-neutral parameter 寫入，避免 logical password 被拼接進 SQL literal
        public static void ExecNonQuery(string sql, JasonQueryDatabaseParameter[] parameters, bool showAlertOnError = true)
        {
            try
            {
                GetRuntime().ExecuteNonQuery
                (
                    DbConnectionString,
                    DbConnectionPassword,
                    sql,
                    parameters
                );
            }
            catch (Exception ex)
            {
                var message = ExceptionDialogService.BuildMessage(ex);

                if (showAlertOnError)
                {
                    MessageBox.Show
                    (
                        message,
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        public static string BatchDeleteRecord(string sqls)
        {
            try
            {
                var sqlStatements = sqls.Split
                (
                    new[] { ";" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                GetRuntime().ExecuteBatchNonQuery
                (
                    DbConnectionString,
                    DbConnectionPassword,
                    sqlStatements
                );

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ExceptionDialogService.BuildMessage(ex);
            }
        }

        public static void UpdateSqlHistory(string motherPid, string executionDate, string executionTime, string queryTime, int rows,
                                            string result, string message, string sql, string operationObject, string seqNo = "")
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

        public sealed class SettingUpdateBatch
        {
            private readonly List<string> _sqlStatements = new List<string>();
            private int _settingCount;
            private bool _committed;

            internal SettingUpdateBatch()
            {
            }

            public void UpdateSetting(string attributeKey, string attributeName, string attributeValue, bool isInsertDirectly = false, bool isAttributeText = false)
            {
                ThrowIfCommitted();

                var fieldName = isAttributeText ? "AttributeText" : "AttributeValue";
                var motherPid = ResolveSettingMotherPid(attributeKey);
                var dateTimeNow = ResolveSettingDateTime(attributeName);
                var safeDomainUser = EscapeSqlLiteral(MyGlobal.DomainUser);
                var safeAttributeKey = EscapeSqlLiteral(attributeKey);
                var safeAttributeName = EscapeSqlLiteral(attributeName);
                var safeAttributeValue = EscapeSqlLiteral(attributeValue);

                if (isInsertDirectly)
                {
                    _sqlStatements.Add
                    (
                        BuildSettingInsertSql
                        (
                            safeDomainUser,
                            motherPid,
                            safeAttributeKey,
                            safeAttributeName,
                            fieldName,
                            safeAttributeValue,
                            dateTimeNow,
                            false
                        )
                    );

                    _settingCount++;
                    return;
                }

                _sqlStatements.Add
                (
                    BuildSettingUpdateSql
                    (
                        safeDomainUser,
                        motherPid,
                        safeAttributeKey,
                        safeAttributeName,
                        fieldName,
                        safeAttributeValue,
                        dateTimeNow
                    )
                );

                _sqlStatements.Add
                (
                    BuildSettingInsertSql
                    (
                        safeDomainUser,
                        motherPid,
                        safeAttributeKey,
                        safeAttributeName,
                        fieldName,
                        safeAttributeValue,
                        dateTimeNow,
                        true
                    )
                );

                _settingCount++;
            }

            public void QueueNonQuery(string sql)
            {
                ThrowIfCommitted();

                if (string.IsNullOrWhiteSpace(sql))
                {
                    return;
                }

                _sqlStatements.Add(sql);
            }

            public bool Commit()
            {
                ThrowIfCommitted();

                if (_sqlStatements.Count == 0)
                {
                    _committed = true;
                    return true;
                }

                var statements = new List<string>(_sqlStatements);

                var stopwatch = Stopwatch.StartNew();

                TraceLogger.LogStage
                (
                    $"Options.SettingBatch.Begin; SettingCount={_settingCount}; StatementCount={statements.Count}"
                );

                try
                {
                    GetRuntime().ExecuteBatchNonQuery
                    (
                        DbConnectionString,
                        DbConnectionPassword,
                        statements
                    );

                    stopwatch.Stop();
                    _committed = true;

                    TraceLogger.LogStage
                    (
                        $"Options.SettingBatch.End; Status=Success; SettingCount={_settingCount}; StatementCount={statements.Count}; ElapsedMs={stopwatch.ElapsedMilliseconds}"
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    TraceLogger.LogStage
                    (
                        $"Options.SettingBatch.End; Status=Failure; SettingCount={_settingCount}; StatementCount={statements.Count}; ElapsedMs={stopwatch.ElapsedMilliseconds}; ExceptionType={ex.GetType().FullName}"
                    );

                    MessageBox.Show
                    (
                        ExceptionDialogService.BuildMessage(ex),
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return false;
                }
            }

            private void ThrowIfCommitted()
            {
                if (_committed)
                {
                    throw new InvalidOperationException("This setting update batch has already been committed.");
                }
            }
        }

        public static SettingUpdateBatch CreateSettingUpdateBatch()
        {
            return new SettingUpdateBatch();
        }

        private static string ResolveSettingMotherPid(string attributeKey)
        {
            if (string.Equals(attributeKey, "GlobalConfig", StringComparison.Ordinal))
            {
                return "0";
            }

            return string.IsNullOrWhiteSpace(DbMotherPid) ? "0" : DbMotherPid;
        }

        private static string ResolveSettingDateTime(string attributeName)
        {
            if (string.Equals(attributeName, "CheckForUpdateDays", StringComparison.Ordinal))
            {
                return DateTime.Now.ToString("yyyy/MM/dd 00:00:00", CultureInfo.InvariantCulture);
            }

            return MyGlobal.DateTimeNow();
        }

        private static string EscapeSqlLiteral(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static string BuildSettingUpdateSql(string safeDomainUser, string motherPid, string safeAttributeKey, string safeAttributeName, string fieldName, string safeAttributeValue, string dateTimeNow)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("UPDATE SystemConfig");

            if (string.Equals(safeAttributeName, "CheckForUpdateDays", StringComparison.Ordinal))
            {
                sbSql.AppendLine($"   SET {fieldName} = '{safeAttributeValue}'");
            }
            else
            {
                sbSql.AppendLine($"   SET {fieldName} = '{safeAttributeValue}', AttributeDate = '{dateTimeNow}'");
            }

            sbSql.AppendLine($" WHERE DomainUser = '{safeDomainUser}'");
            sbSql.AppendLine($"   AND MPID = {motherPid}");
            sbSql.AppendLine($"   AND AttributeKey = '{safeAttributeKey}'");
            sbSql.Append($"   AND AttributeName = '{safeAttributeName}'");

            return sbSql.ToString();
        }

        private static string BuildSettingInsertSql(string safeDomainUser, string motherPid, string safeAttributeKey, string safeAttributeName, string fieldName, string safeAttributeValue, string dateTimeNow, bool onlyWhenMissing)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine($"       (DomainUser, MPID, AttributeKey, AttributeName, {fieldName}, AttributeDate)");

            if (!onlyWhenMissing)
            {
                sbSql.Append($"VALUES ('{safeDomainUser}', {motherPid}, '{safeAttributeKey}', '{safeAttributeName}', '{safeAttributeValue}', '{dateTimeNow}')");
                return sbSql.ToString();
            }

            sbSql.AppendLine($"SELECT '{safeDomainUser}', {motherPid}, '{safeAttributeKey}', '{safeAttributeName}', '{safeAttributeValue}', '{dateTimeNow}'");
            sbSql.AppendLine(" WHERE NOT EXISTS");
            sbSql.AppendLine("       (");
            sbSql.AppendLine("           SELECT 1");
            sbSql.AppendLine("             FROM SystemConfig");
            sbSql.AppendLine($"            WHERE DomainUser = '{safeDomainUser}'");
            sbSql.AppendLine($"              AND MPID = {motherPid}");
            sbSql.AppendLine($"              AND AttributeKey = '{safeAttributeKey}'");
            sbSql.AppendLine($"              AND AttributeName = '{safeAttributeName}'");
            sbSql.Append("       )");

            return sbSql.ToString();
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
