using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Providers.Readers;
using System;
using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestReaderClient : IDisposable
    {
        private readonly IntegrationTestDatabaseSettings _settings;
        private readonly OracleReader _oracleReader;
        private readonly PostgreSqlReader _postgreSqlReader;
        private readonly SqlServerReader _sqlServerReader;
        private readonly MySqlReader _mySqlReader;

        public IntegrationTestReaderClient(IntegrationTestDatabaseSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            ApplyConnectionContext();

            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        _oracleReader = new OracleReader { QueryTimeout = _settings.QueryTimeoutSeconds };
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        _postgreSqlReader = new PostgreSqlReader { QueryTimeout = _settings.QueryTimeoutSeconds };
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        _sqlServerReader = new SqlServerReader { QueryTimeout = _settings.QueryTimeoutSeconds };
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        _mySqlReader = new MySqlReader { QueryTimeout = _settings.QueryTimeoutSeconds };
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException($"Unsupported data source type: {_settings.SourceType}");
                    }
            }
        }

        public ConnectionState State
        {
            get
            {
                switch (_settings.SourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            return _oracleReader.GetState();
                        }
                    case DataSourceType.PostgreSql:
                        {
                            return _postgreSqlReader.GetState();
                        }
                    case DataSourceType.SqlServer:
                        {
                            return _sqlServerReader.GetState();
                        }
                    case DataSourceType.MySql:
                        {
                            return _mySqlReader.GetState();
                        }
                    default:
                        {
                            return ConnectionState.Closed;
                        }
                }
            }
        }

        public string Connect()
        {
            ApplyConnectionContext();

            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _oracleReader.ConnectTo();
                    }
                case DataSourceType.PostgreSql:
                    {
                        return _postgreSqlReader.ConnectTo(_settings.ConnectionString);
                    }
                case DataSourceType.SqlServer:
                    {
                        return _sqlServerReader.ConnectTo(_settings.ConnectionString);
                    }
                case DataSourceType.MySql:
                    {
                        return _mySqlReader.ConnectTo(_settings.ConnectionString);
                    }
                default:
                    {
                        return "Unsupported data source type.";
                    }
            }
        }

        public string Commit()
        {
            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _oracleReader.Commit();
                    }
                case DataSourceType.PostgreSql:
                    {
                        return _postgreSqlReader.Commit();
                    }
                case DataSourceType.SqlServer:
                    {
                        return _sqlServerReader.Commit();
                    }
                case DataSourceType.MySql:
                    {
                        return _mySqlReader.Commit();
                    }
                default:
                    {
                        return "Unsupported data source type.";
                    }
            }
        }

        public string Rollback()
        {
            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _oracleReader.Rollback();
                    }
                case DataSourceType.PostgreSql:
                    {
                        return _postgreSqlReader.Rollback();
                    }
                case DataSourceType.SqlServer:
                    {
                        return _sqlServerReader.Rollback();
                    }
                case DataSourceType.MySql:
                    {
                        return _mySqlReader.Rollback();
                    }
                default:
                    {
                        return "Unsupported data source type.";
                    }
            }
        }

        public bool Disconnect()
        {
            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _oracleReader.Disconnect();
                    }
                case DataSourceType.PostgreSql:
                    {
                        return _postgreSqlReader.Disconnect();
                    }
                case DataSourceType.SqlServer:
                    {
                        return _sqlServerReader.Disconnect();
                    }
                case DataSourceType.MySql:
                    {
                        return _mySqlReader.Disconnect();
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        public int ExecuteNonQuery(string sql, out string errorMessage)
        {
            return ExecuteNonQuery(sql, out errorMessage, out var hasPendingTransactionAfterExecute, out var isTransactionClosedBySqlCommand);
        }

        public int ExecuteNonQuery(string sql, out string errorMessage, out bool hasPendingTransactionAfterExecute, out bool isTransactionClosedBySqlCommand)
        {
            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _oracleReader.ExecuteSingleNonQuery
                        (
                            sql,
                            out errorMessage,
                            out hasPendingTransactionAfterExecute,
                            out isTransactionClosedBySqlCommand
                        );
                    }
                case DataSourceType.PostgreSql:
                    {
                        return _postgreSqlReader.ExecuteSingleNonQuery
                        (
                            sql,
                            out errorMessage,
                            out hasPendingTransactionAfterExecute,
                            out isTransactionClosedBySqlCommand
                        );
                    }
                case DataSourceType.SqlServer:
                    {
                        return _sqlServerReader.ExecuteSingleNonQuery
                        (
                            sql,
                            out errorMessage,
                            out hasPendingTransactionAfterExecute,
                            out isTransactionClosedBySqlCommand
                        );
                    }
                case DataSourceType.MySql:
                    {
                        return _mySqlReader.ExecuteSingleNonQuery
                        (
                            sql,
                            out errorMessage,
                            out hasPendingTransactionAfterExecute,
                            out isTransactionClosedBySqlCommand
                        );
                    }
                default:
                    {
                        errorMessage = "Unsupported data source type.";
                        hasPendingTransactionAfterExecute = false;
                        isTransactionClosedBySqlCommand = false;

                        return 0;
                    }
            }
        }

        public IntegrationTestPagedQueryResult ExecutePaged(string sql, int startRow, int pageLength)
        {
            var result = new IntegrationTestPagedQueryResult();

            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        result.Data = _oracleReader.ExecuteQueryPaged100Rows
                        (
                            sql,
                            startRow,
                            pageLength,
                            out var oracleError,
                            out var oracleSchema
                        );

                        result.ErrorMessage = oracleError;
                        result.Schema = oracleSchema;

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        result.Data = _postgreSqlReader.ExecuteQueryPaged100Rows
                        (
                            sql,
                            startRow,
                            pageLength,
                            out var isRollback,
                            out var isPermissionDenied,
                            out var postgreSqlError,
                            out var postgreSqlErrorCode,
                            out var postgreSqlSchema
                        );

                        result.ErrorCode = postgreSqlErrorCode;
                        result.ErrorMessage = postgreSqlError;
                        result.Schema = postgreSqlSchema;

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        result.Data = _sqlServerReader.ExecuteQueryPaged100Rows
                        (
                            sql,
                            startRow,
                            pageLength,
                            out var sqlServerError,
                            out var sqlServerSchema
                        );

                        result.ErrorMessage = sqlServerError;
                        result.Schema = sqlServerSchema;

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        result.Data = _mySqlReader.ExecuteQueryPaged100Rows
                        (
                            sql,
                            startRow,
                            pageLength,
                            out var mySqlError,
                            out var mySqlSchema
                        );

                        result.ErrorMessage = mySqlError;
                        result.Schema = mySqlSchema;

                        break;
                    }
                default:
                    {
                        result.ErrorMessage = "Unsupported data source type.";
                        break;
                    }
            }

            return result;
        }


        public IntegrationTestQueryExecutionResult ExecuteSinglePassQuery(string sql)
        {
            var result = new IntegrationTestQueryExecutionResult();
            var payload = BuildExecuteQueryPayload(sql);

            MyGlobal.GlobalTemp = string.Empty;

            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        _oracleReader.ExecuteQuery(payload);
                        result.Schema = _oracleReader.dtQuerySchema?.Copy();

                        LoadDataReader(result, _oracleReader.DataReader);

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        _postgreSqlReader.ExecuteQuery(payload);
                        result.Schema = _postgreSqlReader.dtQuerySchema?.Copy();

                        LoadDataReader(result, _postgreSqlReader.DataReader);

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        _sqlServerReader.ExecuteQuery(payload);
                        result.Schema = _sqlServerReader.dtQuerySchema?.Copy();

                        LoadDataReader(result, _sqlServerReader.DataReader);

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        _mySqlReader.ExecuteQuery(payload);
                        result.Schema = _mySqlReader.dtQuerySchema?.Copy();

                        LoadDataReader(result, _mySqlReader.DataReader);

                        break;
                    }
                default:
                    {
                        result.ErrorPayload = "Unsupported data source type.";
                        break;
                    }
            }

            if (MyGlobal.GlobalTemp.StartsWith("SQLExecuteErrorPos", StringComparison.Ordinal))
            {
                result.ErrorPayload = MyGlobal.GlobalTemp;
            }

            MyGlobal.GlobalTemp = string.Empty;
            return result;
        }

        public void Dispose()
        {
            try
            {
                Disconnect();
            }
            finally
            {
                DatabaseSqlExecutor.ResetCurrentConnection();
                DatabaseSqlExecutor.CurrentDataSource = DataSourceType.None;
                DatabaseSqlExecutor.DataSourceDisplayName = string.Empty;
            }
        }

        private static void LoadDataReader(IntegrationTestQueryExecutionResult result, IDataReader dataReader)
        {
            if (result == null || dataReader == null)
            {
                return;
            }

            try
            {
                result.Data.Load(dataReader);
            }
            catch (Exception ex)
            {
                result.ErrorPayload = $"{ex.GetType().FullName}: {ex.Message}";
            }
        }

        private string BuildExecuteQueryPayload(string sql)
        {
            switch (_settings.SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"Step10C{MyGlobal.Separator5}0{MyGlobal.Separator5}{sql ?? string.Empty}";
                    }
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return $"Step10C{MyGlobal.Separator5}0{MyGlobal.Separator5}0{MyGlobal.Separator5}{sql ?? string.Empty}";
                    }
                default:
                    {
                        return sql ?? string.Empty;
                    }
            }
        }

        private void ApplyConnectionContext()
        {
            JasonQueryRepository.DbMotherPid = string.Empty;

            DatabaseSqlExecutor.ResetCurrentConnection();
            DatabaseSqlExecutor.CurrentDataSource = _settings.SourceType;
            DatabaseSqlExecutor.QueryTimeoutSeconds = _settings.QueryTimeoutSeconds;
            DatabaseSqlExecutor.UseConnectionPooling = _settings.UseConnectionPooling;
            DatabaseSqlExecutor.UseUnicode = _settings.UseUnicode;
            DatabaseSqlExecutor.DatabaseName = _settings.DatabaseName;
            DatabaseSqlExecutor.DbConnectionString = _settings.ConnectionString;

            if (_settings.SourceType != DataSourceType.Oracle)
            {
                return;
            }

            DatabaseSqlExecutor.DbConnectionServer = _settings.Server;
            DatabaseSqlExecutor.DbConnectionPort = _settings.Port;
            DatabaseSqlExecutor.DbUser = _settings.UserName;
            DatabaseSqlExecutor.DbPassword = _settings.Password;
            DatabaseSqlExecutor.OracleSid = _settings.OracleSid;
            DatabaseSqlExecutor.OracleConnectAs = _settings.OracleConnectAs;
            DatabaseSqlExecutor.UseDirectMode = _settings.UseDirectMode;
        }
    }
}