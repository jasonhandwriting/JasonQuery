using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using System;
using System.Data;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal sealed class QueryEditorAutoCompleteSchemaQueryExecutor
    {
        private readonly DataSourceType _currentSourceType;

        public QueryEditorAutoCompleteSchemaQueryExecutor(DataSourceType currentSourceType)
        {
            _currentSourceType = currentSourceType;
        }

        public bool TryExecute(string sql, string traceScenario, out DataTable dtSchemaTable)
        {
            dtSchemaTable = new DataTable();

             switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        try
                        {
                            var dtData = MyGlobal.OracleReader.ExecuteQueryPaged100Rows(sql, 0, 0, out string errorMessage, out dtSchemaTable);

                            return dtData != null && string.IsNullOrEmpty(errorMessage);
                        }
                        catch (Exception)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                            return false;
                        }
                    }
                case DataSourceType.PostgreSql:
                    {
                        var errorMessage = string.Empty;

                        if (MyGlobal.ShouldSavePoint)
                        {
                            var sql0 = SqlTraceHelper.BuildHeaderNewLine(string.Format("---SavePoint for getting the AutoComplete List ({0})", traceScenario));

                            MyGlobal.PostgreSqlReader.SavePoint("jqcc1688ccqj", sql0);
                        }

                        try
                        {
                            var dtData = MyGlobal.PostgreSqlReader.ExecuteQueryPaged100Rows(sql, 0, 0, out bool rollback, out bool permissionDenied,
                                                                                            out errorMessage, out var errorCode, out dtSchemaTable);

                            return dtData != null && string.IsNullOrEmpty(errorMessage);
                        }
                        catch (Exception)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                            return false;
                        }
                        finally
                        {
                            if (MyGlobal.ShouldSavePoint)
                            {
                                if (string.IsNullOrEmpty(errorMessage))
                                {
                                    var sql0 = SqlTraceHelper.BuildHeaderNewLine(string.Format("---ReleasePoint for getting the AutoComplete List ({0})", traceScenario));

                                    MyGlobal.PostgreSqlReader.ReleasePoint("jqcc1688ccqj", sql0);
                                }
                                else
                                {
                                    var sql0 = SqlTraceHelper.BuildHeaderNewLine(string.Format("---RollbackPoint for getting the AutoComplete List ({0})", traceScenario));

                                    MyGlobal.PostgreSqlReader.RollbackPoint("jqcc1688ccqj", sql0);
                                }
                            }
                        }
                    }
                case DataSourceType.SqlServer:
                    {
                        try
                        {
                            var dtData = MyGlobal.SqlServerReader.ExecuteQueryPaged100Rows(sql, 0, 0, out string errorMessage, out dtSchemaTable);

                            return dtData != null && string.IsNullOrEmpty(errorMessage);
                        }
                        catch (Exception)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                            return false;
                        }
                    }
                case DataSourceType.MySql:
                    {
                        try
                        {
                            var dtData = MyGlobal.MySqlReader.ExecuteQueryPaged100Rows(sql, 0, 0, out string errorMessage, out dtSchemaTable);

                            return dtData != null && string.IsNullOrEmpty(errorMessage);
                        }
                        catch (Exception)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                            return false;
                        }
                    }
                default:
                    {
                        DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                        return false;
                    }
            }
        }
    }
}