using JasonLibrary.Core;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Metadata.PostgreSql.Function;
using JasonQuery.Core.Database.Metadata.PostgreSql.Table;
using JasonQuery.Core.Database.Metadata.PostgreSql.Trigger;
using JasonQuery.Core.Database.Metadata.PostgreSql.View;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Function;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Trigger;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.View;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.QueryEditor
{
    internal static class PostgreSqlQueryEditorMetadataLoader
    {
        public delegate DataTable LoadDataTableDelegate(string sql);

        public static void LoadTableAndViewNames(ref DataTable dtTableAndViewName, LoadDataTableDelegate loadDataTable)
        {
            if (loadDataTable == null)
            {
                throw new ArgumentNullException(nameof(loadDataTable));
            }

            DataTable dtNewTableAndViewName = null;

            try
            {
                using (TraceLogger.Time("SQL: Get all Table Name and View Name for Query Editor"))
                {
                    var sqlAllTableAndView = PostgreSqlQueryEditorSqlBuilder.BuildTableAndViewNameSql();

                    dtNewTableAndViewName = loadDataTable(sqlAllTableAndView);
                }

                DataTableLifecycleHelper.ReplaceDataTable(ref dtTableAndViewName, ref dtNewTableAndViewName);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtNewTableAndViewName);
            }
        }

        public static void LoadSchemaMetadata(PostgreSqlQueryEditorMetadataLoadOptions options, LoadDataTableDelegate loadDataTable)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (loadDataTable == null)
            {
                throw new ArgumentNullException(nameof(loadDataTable));
            }

            options.Validate();

            DataTable dtFunctionInfo = null;
            DataTable dtTableSchemaInfo = null;
            DataTable dtTriggerInfo = null;
            DataTable dtViewSchemaInfo = null;

            try
            {
                using (TraceLogger.Time("SQL: Get Function Information"))
                {
                    var sqlFunctionInfo = options.IsVersion11OrGreater
                                          ? PostgreSqlFunctionMetadataSqlBuilder.Build_Post11()
                                          : PostgreSqlFunctionMetadataSqlBuilder.Build_Pre11();

                    dtFunctionInfo = loadDataTable(sqlFunctionInfo);
                }

                using (TraceLogger.Time("SQL: Get Table Schema Information"))
                {
                    var sqlTableSchemaInfo = PostgreSqlTableSchemaMetadataSqlBuilder.Build(options.SortByColumnName);

                    dtTableSchemaInfo = loadDataTable(sqlTableSchemaInfo);
                }

                using (TraceLogger.Time("SQL: Get Trigger Information"))
                {
                    var sqlTriggerInfo = PostgreSqlTriggerMetadataSqlBuilder.Build(options.IsVersion11OrGreater);

                    dtTriggerInfo = loadDataTable(sqlTriggerInfo);
                }

                using (TraceLogger.Time("SQL: Get View Schema Information"))
                {
                    var sqlViewSchemaInfo = PostgreSqlViewSchemaMetadataSqlBuilder.Build();

                    dtViewSchemaInfo = loadDataTable(sqlViewSchemaInfo);
                }

                var functionCountMap = PostgreSqlFunctionCountMapBuilder.Build(dtFunctionInfo);
                var triggerCountMap = PostgreSqlTriggerCountMapBuilder.Build(dtTriggerInfo);

                var functionContext = new PostgreSqlFunctionSchemaBuildContext
                {
                    SourceFunctionSchemaInfo = dtFunctionInfo,
                    TargetSchemaTable = options.TargetSchemaTable,
                    DbConnectionName = options.DbConnectionName,
                    FunctionCountMap = functionCountMap,
                    AddSchemaRow = true,
                    DoEventsInterval = options.DoEventsInterval
                };

                var tableContext = new PostgreSqlTableSchemaBuildContext
                {
                    SourceTableSchemaInfo = dtTableSchemaInfo,
                    TargetSchemaTable = options.TargetSchemaTable,
                    DbConnectionName = options.DbConnectionName,
                    DoEventsInterval = options.DoEventsInterval
                };

                var triggerContext = new PostgreSqlTriggerSchemaBuildContext
                {
                    SourceTriggerSchemaInfo = dtTriggerInfo,
                    TargetSchemaTable = options.TargetSchemaTable,
                    DbConnectionName = options.DbConnectionName,
                    TriggerCountMap = triggerCountMap,
                    AddSchemaRow = true,
                    DoEventsInterval = options.DoEventsInterval
                };

                var viewContext = new PostgreSqlViewSchemaBuildContext
                {
                    SourceViewSchemaInfo = dtViewSchemaInfo,
                    TargetSchemaTable = options.TargetSchemaTable,
                    DbConnectionName = options.DbConnectionName,
                    DoEventsInterval = options.DoEventsInterval
                };

                HashSet<string> distinctFunctionNames;
                HashSet<string> distinctTableNames;
                HashSet<string> distinctTriggerNames;
                HashSet<string> distinctViewNames;

                using (TraceLogger.Time("Organize Function Info"))
                {
                    distinctFunctionNames = PostgreSqlFunctionSchemaOrganizer.Organize(functionContext);
                }

                using (TraceLogger.Time("Organize Table Info"))
                {
                    distinctTableNames = PostgreSqlTableSchemaOrganizer.Organize(tableContext);
                }

                using (TraceLogger.Time("Organize Trigger Info"))
                {
                    distinctTriggerNames = PostgreSqlTriggerSchemaOrganizer.Organize(triggerContext);
                }

                using (TraceLogger.Time("Organize View Info"))
                {
                    distinctViewNames = PostgreSqlViewSchemaOrganizer.Organize(viewContext);
                }

                SchemaObjectKeywordApplyHelper.Apply
                (
                    distinctFunctionNames,
                    options.EnableFunctionAutoComplete,
                    AutoCompleteSourceNames.Functions,
                    keywordText => MyLibrary.KeywordsUserDefinedFunctions = keywordText
                );

                SchemaObjectKeywordApplyHelper.Apply
                (
                    distinctTableNames,
                    options.EnableTableAutoComplete,
                    AutoCompleteSourceNames.Tables,
                    keywordText => MyLibrary.KeywordsUserDefinedTables = keywordText
                );

                SchemaObjectKeywordApplyHelper.Apply
                (
                    distinctTriggerNames,
                    options.EnableTriggerAutoComplete,
                    AutoCompleteSourceNames.Triggers,
                    keywordText => MyLibrary.KeywordsUserDefinedTriggers = keywordText
                );

                SchemaObjectKeywordApplyHelper.Apply
                (
                    distinctViewNames,
                    options.EnableViewAutoComplete,
                    AutoCompleteSourceNames.Views,
                    keywordText => MyLibrary.KeywordsUserDefinedViews = keywordText
                );
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtFunctionInfo);
                DataTableLifecycleHelper.DisposeDataTable(ref dtTableSchemaInfo);
                DataTableLifecycleHelper.DisposeDataTable(ref dtTriggerInfo);
                DataTableLifecycleHelper.DisposeDataTable(ref dtViewSchemaInfo);
            }
        }
    }
}