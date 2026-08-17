using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using System;
using System.Data;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryResultLoadResult
        {
            public DataTable Data { get; set; } = new DataTable();
            public DataTable SchemaTable { get; set; } = new DataTable();
            public string Sql { get; set; } = string.Empty;
            public string ErrorCode { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public int ErrorOffset { get; set; } = -1;

            public bool HasPublishedExecutionError { get; set; }

            public bool HasError
            {
                get
                {
                    return ErrorOffset != -1 || HasPublishedExecutionError;
                }
            }
        }

        private QueryResultLoadResult LoadQueryResultData()
        {
            var result = new QueryResultLoadResult
            {
                Sql = editor.SelectedText
            };

            if (HasPendingSqlExecuteError())
            {
                result.HasPublishedExecutionError = true;
                return result;
            }

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            LoadOracleQueryResultData(result);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            LoadPostgreSqlQueryResultData(result);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            LoadSqlServerQueryResultData(result);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            LoadMySqlQueryResultData(result);
                            break;
                        }
                }
            }
            finally
            {
                tmrQueryTime.Enabled = false;

                if (result.HasError)
                {
                    SetSqlExecuteErrorGlobalTemp(result);
                }
            }

            return result;
        }

        private bool HasPendingSqlExecuteError()
        {
            return MyGlobal.GlobalTemp.IndexOf("SQLExecuteErrorPos", StringComparison.Ordinal) != -1;
        }

        private void SetQueryLoadError(QueryResultLoadResult result, Exception ex, int errorOffset)
        {
            SetQueryLoadError(result, ex, errorOffset, string.Empty);
        }

        private void SetQueryLoadError(QueryResultLoadResult result, Exception ex, int errorOffset, string errorCode)
        {
            if (result == null)
            {
                return;
            }

            result.ErrorOffset = errorOffset;
            result.ErrorMessage = ex?.Message ?? string.Empty;
            result.ErrorCode = errorCode ?? string.Empty;
        }

        private void SetQueryLoadErrorFromQuotedWord(QueryResultLoadResult result, Exception ex, int defaultOffset)
        {
            if (result == null)
            {
                return;
            }

            var offset = ResolveErrorOffsetFromQuotedWord(ex?.Message, result.Sql, defaultOffset);

            SetQueryLoadError(result, ex, offset);
        }

        private int GetOracleErrorOffset(int offset)
        {
            return offset + _queryTextParametersStart;
        }

        private int GetPostgreSqlErrorOffset(int offset)
        {
            return offset + (_isNextPageQuery ? -15 : 0) + _queryTextParametersStart;
        }

        private string BuildOracleErrorCodeText(Devart.Data.Oracle.OracleException ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }

            var code = ex.Code.ToString();

            return string.IsNullOrEmpty(code) ? string.Empty : $"ErrorCode: {code}\r\n";
        }

        private void LoadOracleQueryResultData(QueryResultLoadResult result)
        {
            _oracleDataReader = MyGlobal.OracleReader.DataReader;

            try
            {
                if (_oracleDataReader != null && !_oracleDataReader.IsClosed)
                {
                    result.SchemaTable = MyGlobal.OracleReader.dtQuerySchema;
                    result.Data.Load(_oracleDataReader);
                    _oracleDataReader.Close();
                }
            }
            catch (InvalidOperationException ex)
            {
                SetQueryLoadError(result, ex, GetOracleErrorOffset(0));
            }
            catch (Devart.Data.Oracle.OracleException ex)
            {
                SetQueryLoadError
                (
                    result,
                    ex,
                    GetOracleErrorOffset(ex.Offset),
                    BuildOracleErrorCodeText(ex)
                );
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetQueryLoadError(result, ex, GetOracleErrorOffset(0));
            }
        }

        private void LoadPostgreSqlQueryResultData(QueryResultLoadResult result)
        {
            _postgreSqlDataReader = MyGlobal.PostgreSqlReader.DataReader;

            try
            {
                if (_postgreSqlDataReader != null && !_postgreSqlDataReader.IsClosed)
                {
                    result.SchemaTable = MyGlobal.PostgreSqlReader.dtQuerySchema;
                    result.Data.Load(_postgreSqlDataReader);
                    _postgreSqlDataReader.Close();
                }
            }
            catch (InvalidOperationException ex)
            {
                SetQueryLoadError(result, ex, GetPostgreSqlErrorOffset(0));
            }
            catch (Devart.Data.PostgreSql.PgSqlException ex)
            {
                SetQueryLoadError(result, ex, GetPostgreSqlErrorOffset(ex.Position));
            }
            catch (ArgumentException ex)
            {
                SetQueryLoadError(result, ex, GetPostgreSqlErrorOffset(0));
            }
            catch (ConstraintException ex)
            {
                SetQueryLoadError(result, ex, GetPostgreSqlErrorOffset(0));
            }
        }

        private void LoadSqlServerQueryResultData(QueryResultLoadResult result)
        {
            _sqlServerDataReader = MyGlobal.SqlServerReader.DataReader;

            try
            {
                if (_sqlServerDataReader != null && !_sqlServerDataReader.IsClosed)
                {
                    result.SchemaTable = MyGlobal.SqlServerReader.dtQuerySchema;
                    result.Data.Load(_sqlServerDataReader);
                    _sqlServerDataReader.Close();
                }
            }
            catch (InvalidOperationException ex)
            {
                SetQueryLoadError(result, ex, 0);
            }
            catch (Devart.Data.SqlServer.SqlException ex)
            {
                SetQueryLoadErrorFromQuotedWord(result, ex, 0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetQueryLoadError(result, ex, 0);
            }
        }

        private void LoadMySqlQueryResultData(QueryResultLoadResult result)
        {
            _mySqlDataReader = MyGlobal.MySqlReader.DataReader;

            try
            {
                if (_mySqlDataReader != null && !_mySqlDataReader.IsClosed)
                {
                    result.SchemaTable = MyGlobal.MySqlReader.dtQuerySchema;
                    result.Data.Load(_mySqlDataReader);
                    _mySqlDataReader.Close();
                }
            }
            catch (InvalidOperationException ex)
            {
                SetQueryLoadError(result, ex, 0);
            }
            catch (ConstraintException ex)
            {
                //某些 SQL 會引發 ConstraintException 錯誤，例如：
                //SELECT * FROM Information_Schema.Triggers cc WHERE Trigger_Schema = 'sakila';
                SetQueryLoadError(result, ex, 0);
            }
            catch (Devart.Data.MySql.MySqlException ex)
            {
                SetQueryLoadErrorFromQuotedWord(result, ex, 0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetQueryLoadError(result, ex, 0);
            }
        }

        private int ResolveErrorOffsetFromQuotedWord(string errorMessage, string sql, int defaultOffset)
        {
            if (string.IsNullOrEmpty(errorMessage) || string.IsNullOrEmpty(sql))
            {
                return defaultOffset;
            }

            var iFrom = errorMessage.IndexOf('\'') + 1;
            var iTo = errorMessage.LastIndexOf('\'');

            if (iFrom > 0 && iTo > 0 && iTo > iFrom)
            {
                //可能情況：出現兩個單字，前後都有雙引號；故從第一個雙引號之後繼續找
                iTo = errorMessage.Substring(iFrom).IndexOf('\'') + iFrom;

                //錯誤訊息有明確指出哪個字串
                var tempWord = errorMessage.Substring(iFrom, iTo - iFrom);

                //判斷 tempWord 是否有出現在執行的 SQL 裡面
                var index = sql.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    return index;
                }
            }

            return defaultOffset;
        }

        private void SetSqlExecuteErrorGlobalTemp(QueryResultLoadResult result)
        {
            if (result == null || !result.HasError)
            {
                return;
            }

            MyGlobal.GlobalTemp = BuildSqlExecuteErrorGlobalTemp(result);
        }

        private string BuildSqlExecuteErrorGlobalTemp(QueryResultLoadResult result)
        {
            return $"SQLExecuteErrorPos{MyGlobal.Separator}" +
                   $"{AccessibleDescription};" +
                   $"{MyGlobal.Separator5}{result.ErrorCode}" +
                   $"{MyGlobal.Separator5}{result.ErrorMessage}" +
                   $"{MyGlobal.Separator5}" +
                   $"{MyGlobal.Separator5}{result.ErrorOffset}" +
                   $"{MyGlobal.Separator5}{result.Sql}";
        }
    }
}