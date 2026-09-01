using JasonQuery.Core.Config;
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
        public static string DbConnectionPassword = "ytec1688"; //20221107 連線至 JasonQuery.db 的密碼
        public static string DbConnectionPasswordPrefix = "jasonquery1231"; //20221107 連線至 JasonQuery.db 的密碼
        public static string DbConnectionPasswordSuffix = "encryptDB!nf0"; //20221107 連線至 JasonQuery.db 的密碼
        public static string DbConnectionExportPasswordSuffix = "exportDB!nf0"; //20221107 連線至 JasonQuery.db 的密碼
        public static string DbMotherPid = string.Empty;
        public static string DbFileName = string.Empty;

        //資料庫初始化
        private static SQLiteConnection OleDbOpenConn(string password = "", string newPassword = "", bool isUseDefaultPassword = false)
        {
            password = string.IsNullOrWhiteSpace(password) ? DbConnectionPassword : password;

            var conn = new SQLiteConnection { ConnectionString = DbConnectionString };

            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn.SetPassword(password);
                conn.Open();

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    DbConnectionPassword = $"{DbConnectionPasswordPrefix}{newPassword}{DbConnectionPasswordSuffix}";
                    conn.ChangePassword(DbConnectionPassword);
                }
                else if (isUseDefaultPassword && string.IsNullOrWhiteSpace(newPassword))
                {
                    conn.ChangePassword("ytec1688");
                    DbConnectionPassword = "ytec1688";
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return conn;
        }

        //是否使用預設密碼？
        public static bool CheckDBPassword(string password)
        {
            password = string.IsNullOrWhiteSpace(password) ? string.Empty : $"{DbConnectionPasswordPrefix}{password}{DbConnectionPasswordSuffix}";

            var isResult = true;
            var conn = OleDbOpenConn(password);
            var myDataTable = new DataTable();
            var da = new SQLiteDataAdapter("SELECT * FROM SystemConfig WHERE 1 = 2", conn);
            var ds = new DataSet();

            try
            {
                ds.Clear();
                da.Fill(ds);
                myDataTable = ds.Tables[0];
            }
            catch (Exception)
            {
                //密碼錯誤！
                isResult = false;
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            return isResult;
        }

        //變更密碼！
        public static string ResetDBPassword(string oldPassword, string newPassword, bool isUseDefaultPassword)
        {
            var result = string.Empty;
            var mdbConn = OleDbOpenConn(oldPassword, newPassword, isUseDefaultPassword);
            var myDataTable = new DataTable();
            var da = new SQLiteDataAdapter("SELECT * FROM SystemConfig WHERE 1 = 2", mdbConn);
            var ds = new DataSet();

            try
            {
                ds.Clear();
                da.Fill(ds);
                myDataTable = ds.Tables[0];
            }
            catch (Exception ex)
            {
                //密碼錯誤！
                result = ex.Message;
            }

            if (mdbConn.State == ConnectionState.Open)
            {
                mdbConn.Close();
            }

            return result;
        }

        //取得資料表:
        public static DataTable ExecQuery(string sql)
        {
            var conn = OleDbOpenConn();
            var myDataTable = new DataTable();
            var da = new SQLiteDataAdapter(sql, conn);
            var ds = new DataSet();

            try
            {
                ds.Clear();
                da.Fill(ds);
                myDataTable = ds.Tables[0];
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            return myDataTable;
        }

        //對資料表進行新增、修改及刪除等功能:
        public static void ExecNonQuery(string sql, bool showAlertOnError = true)
        {
            var conn = OleDbOpenConn();

            try
            {
                var cmd = new SQLiteCommand(sql, conn);
                var count = cmd.ExecuteNonQuery();

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
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

        public static string BatchDeleteRecord(string sqls)
        {
            var result = string.Empty;
            var conn = OleDbOpenConn();

            try
            {
                var sql = sqls.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var t in sql)
                {
                    var cmd = new SQLiteCommand(t, conn);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                result = ExceptionDialogService.BuildMessage(ex);
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
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
                var dtTemp = ExecQuery(sql);

                if (dtTemp?.Rows.Count > 0)
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
