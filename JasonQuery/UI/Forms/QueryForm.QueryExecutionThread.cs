using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Database.Providers.Readers;
using System;
using System.Threading;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void StartQueryExecutionThread(string queryText, QueryExecutionKindResult queryKind)
        {
            _threadQuery = CreateQueryExecutionThread(queryKind);

            if (_threadQuery == null)
            {
                ResetLockingQueryExecutionState();
                return;
            }

            _threadQuery.IsBackground = true;
            _threadQuery.Start(queryText);

            MarkLockingQueryExecutionStarted(queryKind);
        }

        private Thread CreateQueryExecutionThread(QueryExecutionKindResult queryKind)
        {
            if (queryKind == null)
            {
                return null;
            }

            ApplyCurrentCommandTimeout();

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle when queryKind.IsQuery:
                    {
                        return CreateOracleQueryThread(queryKind);
                    }
                case DataSourceType.Oracle:
                    {
                        return new Thread(MyGlobal.OracleReader.ExecuteNonQuery);
                    }
                case DataSourceType.PostgreSql when queryKind.IsQuery:
                    {
                        return CreatePostgreSqlQueryThread(queryKind);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return new Thread(MyGlobal.PostgreSqlReader.ExecuteNonQuery);
                    }
                case DataSourceType.SqlServer when queryKind.IsQuery:
                    {
                        return CreateSqlServerQueryThread(queryKind);
                    }
                case DataSourceType.SqlServer:
                    {
                        return new Thread(MyGlobal.SqlServerReader.ExecuteNonQuery);
                    }
                case DataSourceType.MySql when queryKind.IsQuery:
                    {
                        return CreateMySqlQueryThread(queryKind);
                    }
                case DataSourceType.MySql:
                    {
                        return new Thread(MyGlobal.MySqlReader.ExecuteNonQuery);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private Thread CreateOracleQueryThread(QueryExecutionKindResult queryKind)
        {
            var oracleReader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

            return queryKind.IsLockingQuery || btnPaginationOff.Visible ? new Thread(oracleReader.ExecuteQuery) : new Thread(oracleReader.ExecuteQueryPaged);
        }

        private Thread CreatePostgreSqlQueryThread(QueryExecutionKindResult queryKind)
        {
            var postgreSqlReader = MyGlobal.PostgreSqlReader ?? throw new InvalidOperationException("PostgreSqlReader has not been initialized.");

            return queryKind.IsLockingQuery || queryKind.IsExtended || btnPaginationOff.Visible ? new Thread(postgreSqlReader.ExecuteQuery) : new Thread(postgreSqlReader.ExecuteQueryPaged);
        }

        private Thread CreateSqlServerQueryThread(QueryExecutionKindResult queryKind)
        {
            var sqlServerReader = MyGlobal.SqlServerReader ?? throw new InvalidOperationException("SqlServerReader has not been initialized.");

            return queryKind.IsLockingQuery || queryKind.IsExtended || btnPaginationOff.Visible ? new Thread(sqlServerReader.ExecuteQuery) : new Thread(sqlServerReader.ExecuteQueryPaged);
        }

        private Thread CreateMySqlQueryThread(QueryExecutionKindResult queryKind)
        {
            var mySqlReader = MyGlobal.MySqlReader ?? throw new InvalidOperationException("MySqlReader has not been initialized.");

            return queryKind.IsLockingQuery || queryKind.IsExtended || btnPaginationOff.Visible ? new Thread(mySqlReader.ExecuteQuery) : new Thread(mySqlReader.ExecuteQueryPaged);
        }

        private void ApplyCurrentCommandTimeout()
        {
            var commandTimeout = decimal.ToInt32(nudQueryTimeout.Value);

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var reader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

                        reader.QueryTimeout = commandTimeout;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        var reader = MyGlobal.PostgreSqlReader ?? throw new InvalidOperationException("PostgreSqlReader has not been initialized.");

                        reader.QueryTimeout = commandTimeout;
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        var reader = MyGlobal.SqlServerReader ?? throw new InvalidOperationException("SqlServerReader has not been initialized.");

                        reader.QueryTimeout = commandTimeout;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        var reader = MyGlobal.MySqlReader ?? throw new InvalidOperationException("MySqlReader has not been initialized.");

                        reader.QueryTimeout = commandTimeout;
                        break;
                    }
            }
        }
    }
}
