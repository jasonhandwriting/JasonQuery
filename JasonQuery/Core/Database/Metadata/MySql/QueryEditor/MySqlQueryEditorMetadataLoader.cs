using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Metadata.MySql.Function;
using JasonQuery.Core.Database.Metadata.MySql.Procedure;
using JasonQuery.Core.Database.Metadata.MySql.Table;
using JasonQuery.Core.Database.Metadata.MySql.Trigger;
using JasonQuery.Core.Database.Metadata.MySql.View;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Function;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Procedure;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Trigger;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.View;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.MySql.QueryEditor
{
    internal static class MySqlQueryEditorMetadataLoader
    {
        public static void LoadSchemaMetadata(MySqlMetadataContext context, MySqlQueryEditorMetadataLoadOptions options)
        {
            Validate(context, options);

            var snapshot = LoadMetadataSnapshot(context);
            var dtNewSchema = BuildSchemaTable(context, options, snapshot);

            context.ResultSchemaTable = dtNewSchema ?? context.SchemaTable;
        }

        private static void Validate(MySqlMetadataContext context, MySqlQueryEditorMetadataLoadOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (context.SchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.SchemaTable));
            }

            if (context.ExecuteQuery == null)
            {
                throw new InvalidOperationException("MySqlMetadataContext.ExecuteQuery 尚未指定。");
            }

            if (string.IsNullOrWhiteSpace(context.DatabaseName))
            {
                throw new InvalidOperationException("MySqlMetadataContext.DatabaseName 尚未指定。");
            }
        }

        private static MySqlQueryEditorMetadataSnapshot LoadMetadataSnapshot(MySqlMetadataContext context)
        {
            var snapshot = new MySqlQueryEditorMetadataSnapshot();
            var databaseName = BuildDatabaseName(context);

            var tableResult = LoadMySqlTableMetadata(context, databaseName);
            var viewResult = LoadMySqlViewMetadata(context, databaseName);

            snapshot.FunctionInfo = LoadMySqlFunctionMetadata(context, databaseName);
            snapshot.TriggerInfo = LoadMySqlTriggerMetadata(context, databaseName);
            snapshot.ProcedureInfo = LoadMySqlProcedureMetadata(context, databaseName);

            snapshot.TableInfo = tableResult.TableInfo;
            snapshot.TableColumnInfo = tableResult.TableColumnInfo;
            snapshot.TableColumnInfoMap = tableResult.TableColumnInfoMap;
            snapshot.RowCountMap = tableResult.RowCountMap;

            snapshot.ViewInfo = viewResult.ViewInfo;
            snapshot.ViewColumnInfo = viewResult.ViewColumnInfo;
            snapshot.ViewColumnInfoMap = viewResult.ViewColumnInfoMap;

            return snapshot;
        }

        private static DataTable BuildSchemaTable(MySqlMetadataContext context, MySqlQueryEditorMetadataLoadOptions options,
                                                  MySqlQueryEditorMetadataSnapshot snapshot)
        {
            snapshot ??= new MySqlQueryEditorMetadataSnapshot();

            var dtNewSchema = context.SchemaTable;
            var dbName = GetSelectedDatabaseName(context);

            if (dtNewSchema.Rows.Count > 0)
            {
                dtNewSchema.Clear();
            }

            var dtFunctionInfo = snapshot.FunctionInfo;
            var dtProcedureInfo = snapshot.ProcedureInfo;
            var dtTableInfo = snapshot.TableInfo;
            var dtTriggerInfo = snapshot.TriggerInfo;
            var dtViewInfo = snapshot.ViewInfo;

            var tableColumnInfoMap = snapshot.TableColumnInfoMap
                                     ?? new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

            var viewColumnInfoMap = snapshot.ViewColumnInfoMap
                                    ?? new Dictionary<(string SchemaName, string TableName), List<DataRow>>();

            var rowCountMap = snapshot.RowCountMap
                              ?? new Dictionary<(string SchemaName, string TableName), long>();

            dtNewSchema.BeginLoadData();

            try
            {
                //Functions
                #region Get info of Functions
                var listForFunctionSchema = (dtFunctionInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                            .Where(r => r.GetSafeString("DbName") == dbName)
                                            .ToList();

                var functionBuildContext = new MySqlFunctionSchemaBuildContext
                {
                    SchemaTable = dtNewSchema,
                    ConnectionName = context.ConnectionName,
                    DatabaseName = dbName,
                    FunctionRows = listForFunctionSchema,
                    AddSchemaRow = options.AddSchemaRow,
                    AutoCompleteFunction = options.AutoCompleteFunction
                };

                var distinctNamesFunction = MySqlFunctionSchemaOrganizer.Organize(functionBuildContext);

                MyLibrary.KeywordsUserDefinedFunctions = $"{string.Join(" ", distinctNamesFunction)} ";
                #endregion

                //Tables
                #region Get info of Tables
                var listForTableSchema = (dtTableInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                         .Where(r => r.GetSafeString("DbName") == dbName)
                                         .ToList();

                var tableBuildContext = new MySqlTableSchemaBuildContext
                {
                    SchemaTable = dtNewSchema,
                    ConnectionName = context.ConnectionName,
                    DatabaseName = dbName,
                    TableRows = listForTableSchema,
                    TableColumnInfoMap = tableColumnInfoMap,
                    RowCountMap = rowCountMap,
                    AddSchemaRow = options.AddSchemaRow,
                    AutoCompleteTable = options.AutoCompleteTable
                };

                var distinctNamesTable = MySqlTableSchemaOrganizer.Organize(tableBuildContext);

                MyLibrary.KeywordsUserDefinedTables = $"{string.Join(" ", distinctNamesTable)} ";
                #endregion

                //Triggers
                #region Get info of Triggers
                var listForTriggerSchema = (dtTriggerInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                           .Where(r => r.GetSafeString("DbName") == dbName)
                                           .ToList();

                var triggerBuildContext = new MySqlTriggerSchemaBuildContext
                {
                    SchemaTable = dtNewSchema,
                    ConnectionName = context.ConnectionName,
                    DatabaseName = dbName,
                    TriggerRows = listForTriggerSchema,
                    AddSchemaRow = options.AddSchemaRow,
                    AutoCompleteTrigger = options.AutoCompleteTrigger
                };

                var distinctNamesTrigger = MySqlTriggerSchemaOrganizer.Organize(triggerBuildContext);

                MyLibrary.KeywordsUserDefinedTriggers = $"{string.Join(" ", distinctNamesTrigger)} ";
                #endregion

                //Views
                #region Get info of Views
                var listForViewSchema = (dtViewInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                        .Where(r => r.GetSafeString("DbName") == dbName)
                                        .ToList();

                var viewBuildContext = new MySqlViewSchemaBuildContext
                {
                    SchemaTable = dtNewSchema,
                    ConnectionName = context.ConnectionName,
                    DatabaseName = dbName,
                    ViewRows = listForViewSchema,
                    ViewColumnInfoMap = viewColumnInfoMap,
                    AddSchemaRow = options.AddSchemaRow,
                    AutoCompleteView = options.AutoCompleteView
                };

                var distinctNamesView = MySqlViewSchemaOrganizer.Organize(viewBuildContext);

                MyLibrary.KeywordsUserDefinedViews = $"{string.Join(" ", distinctNamesView)} ";
                #endregion

                //Procedures
                #region Get info of Procedures
                var listForProcedureSchema = (dtProcedureInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                                 .Where(r => r.GetSafeString("DbName") == dbName)
                                                 .ToList();

                var procedureBuildContext = new MySqlProcedureSchemaBuildContext
                {
                    SchemaTable = dtNewSchema,
                    ConnectionName = context.ConnectionName,
                    DatabaseName = dbName,
                    ProcedureRows = listForProcedureSchema,
                    AddSchemaRow = true
                };

                MySqlProcedureSchemaOrganizer.Organize(procedureBuildContext);
                #endregion
            }
            finally
            {
                dtNewSchema.EndLoadData();
            }

            return dtNewSchema;
        }

        private static MySqlTableMetadataLoadResult LoadMySqlTableMetadata(MySqlMetadataContext context, string sDatabaseName)
        {
            var result = new MySqlTableMetadataLoadResult();

            using (TraceLogger.Time("SQL: Get all Column Information of the Specified Table"))
            {
                var sqlTableColumnInfo = MySqlTableColumnMetadataSqlBuilder.Build
                (
                    sDatabaseName,
                    MyGlobal.IsSortByColumnName
                );

                result.TableColumnInfo = ExecuteMetadataQuery(context, sqlTableColumnInfo);
            }

            result.TableColumnInfoMap = MySqlTableColumnInfoMapBuilder.Build(result.TableColumnInfo);

            using (TraceLogger.Time("SQL: Get Table Information"))
            {
                var sqlTable = MySqlTableMetadataSqlBuilder.Build(sDatabaseName);

                result.TableInfo = ExecuteMetadataQuery(context, sqlTable);
            }

            result.RowCountMap = MySqlTableRowCountMapBuilder.Build(result.TableInfo);

            return result;
        }

        private static MySqlViewMetadataLoadResult LoadMySqlViewMetadata(MySqlMetadataContext context, string sDatabaseName)
        {
            var result = new MySqlViewMetadataLoadResult();

            using (TraceLogger.Time("SQL: Get View Information"))
            {
                var sqlView = MySqlViewMetadataSqlBuilder.Build(sDatabaseName);

                result.ViewInfo = ExecuteMetadataQuery(context, sqlView);
            }

            using (TraceLogger.Time("SQL: Get View Information (Including Column Name and Type)"))
            {
                var sqlViewColumnInfo = MySqlViewColumnMetadataSqlBuilder.Build(sDatabaseName);

                result.ViewColumnInfo = ExecuteMetadataQuery(context, sqlViewColumnInfo);
            }

            result.ViewColumnInfoMap = MySqlViewColumnInfoMapBuilder.Build(result.ViewColumnInfo);

            return result;
        }

        private static DataTable LoadMySqlFunctionMetadata(MySqlMetadataContext context, string sDatabaseName)
        {
            using (TraceLogger.Time("SQL: Get Function Information"))
            {
                var sqlFunction = MySqlFunctionMetadataSqlBuilder.Build(sDatabaseName);

                return ExecuteMetadataQuery(context, sqlFunction);
            }
        }

        private static DataTable LoadMySqlTriggerMetadata(MySqlMetadataContext context, string sDatabaseName)
        {
            using (TraceLogger.Time("SQL: Get Trigger Information"))
            {
                var sqlTrigger = MySqlTriggerMetadataSqlBuilder.Build(sDatabaseName);

                return ExecuteMetadataQuery(context, sqlTrigger);
            }
        }

        private static DataTable LoadMySqlProcedureMetadata(MySqlMetadataContext context, string sDatabaseName)
        {
            using (TraceLogger.Time("SQL: Get Procedure Information"))
            {
                var sqlProcedure = MySqlProcedureMetadataSqlBuilder.Build(sDatabaseName);

                return ExecuteMetadataQuery(context, sqlProcedure);
            }
        }

        private static string GetSelectedDatabaseName(MySqlMetadataContext context)
        {
            var databaseName = context?.DatabaseName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("MySqlMetadataContext.DatabaseName 尚未指定。");
            }

            return databaseName;
        }

        private static string BuildDatabaseName(MySqlMetadataContext context)
        {
            var databaseName = GetSelectedDatabaseName(context);

            return $"'{EscapeMySql(databaseName)}'";
        }

        private static DataTable ExecuteMetadataQuery(MySqlMetadataContext context, string sql)
        {
            if (context?.ExecuteQuery == null)
            {
                return new DataTable();
            }

            return context.ExecuteQuery(sql) ?? new DataTable();
        }

        private static string EscapeMySql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
