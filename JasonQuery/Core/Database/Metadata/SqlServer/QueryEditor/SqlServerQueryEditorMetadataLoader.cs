using JasonLibrary.Core;
using JasonQuery.Core.Database.Metadata.SqlServer.Function;
using JasonQuery.Core.Database.Metadata.SqlServer.Index;
using JasonQuery.Core.Database.Metadata.SqlServer.Procedure;
using JasonQuery.Core.Database.Metadata.SqlServer.Table;
using JasonQuery.Core.Database.Metadata.SqlServer.Trigger;
using JasonQuery.Core.Database.Metadata.SqlServer.View;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Function;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Index;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Procedure;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Trigger;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.View;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.QueryEditor
{
    internal sealed class SqlServerQueryEditorMetadataLoader
    {
        private readonly SqlServerFunctionMetadataSqlBuilder _functionSqlBuilder;
        private readonly SqlServerTableRowCountSqlBuilder _tableRowCountSqlBuilder;
        private readonly SqlServerTableColumnMetadataSqlBuilder _tableColumnSqlBuilder;
        private readonly SqlServerTableMetadataSqlBuilder _tableSqlBuilder;
        private readonly SqlServerTriggerMetadataSqlBuilder _triggerSqlBuilder;
        private readonly SqlServerViewMetadataSqlBuilder _viewSqlBuilder;
        private readonly SqlServerViewColumnMetadataSqlBuilder _viewColumnSqlBuilder;
        private readonly SqlServerProcedureMetadataSqlBuilder _procedureSqlBuilder;
        private readonly SqlServerIndexMetadataSqlBuilder _indexSqlBuilder;

        private readonly SqlServerFunctionMetadataOrganizer _functionOrganizer;
        private readonly SqlServerTableSchemaOrganizer _tableOrganizer;
        private readonly SqlServerTriggerMetadataOrganizer _triggerOrganizer;
        private readonly SqlServerViewSchemaOrganizer _viewOrganizer;
        private readonly SqlServerProcedureMetadataOrganizer _procedureOrganizer;
        private readonly SqlServerIndexMetadataOrganizer _indexOrganizer;

        private readonly SqlServerTableRowCountMapBuilder _tableRowCountMapBuilder;
        private readonly SqlServerTableColumnInfoMapBuilder _tableColumnInfoMapBuilder;
        private readonly SqlServerViewColumnInfoMapBuilder _viewColumnInfoMapBuilder;

        public SqlServerQueryEditorMetadataLoader() : this
        (
            new SqlServerFunctionMetadataSqlBuilder(),
            new SqlServerTableRowCountSqlBuilder(),
            new SqlServerTableColumnMetadataSqlBuilder(),
            new SqlServerTableMetadataSqlBuilder(),
            new SqlServerTriggerMetadataSqlBuilder(),
            new SqlServerViewMetadataSqlBuilder(),
            new SqlServerViewColumnMetadataSqlBuilder(),
            new SqlServerProcedureMetadataSqlBuilder(),
            new SqlServerIndexMetadataSqlBuilder(),
            new SqlServerFunctionMetadataOrganizer(),
            new SqlServerTableSchemaOrganizer(),
            new SqlServerTriggerMetadataOrganizer(),
            new SqlServerViewSchemaOrganizer(),
            new SqlServerProcedureMetadataOrganizer(),
            new SqlServerIndexMetadataOrganizer(),
            new SqlServerTableRowCountMapBuilder(),
            new SqlServerTableColumnInfoMapBuilder(),
            new SqlServerViewColumnInfoMapBuilder()
        )
        {
        }

        public SqlServerQueryEditorMetadataLoader(SqlServerFunctionMetadataSqlBuilder functionSqlBuilder,
                                                  SqlServerTableRowCountSqlBuilder tableRowCountSqlBuilder,
                                                  SqlServerTableColumnMetadataSqlBuilder tableColumnSqlBuilder,
                                                  SqlServerTableMetadataSqlBuilder tableSqlBuilder,
                                                  SqlServerTriggerMetadataSqlBuilder triggerSqlBuilder,
                                                  SqlServerViewMetadataSqlBuilder viewSqlBuilder,
                                                  SqlServerViewColumnMetadataSqlBuilder viewColumnSqlBuilder,
                                                  SqlServerProcedureMetadataSqlBuilder procedureSqlBuilder,
                                                  SqlServerIndexMetadataSqlBuilder indexSqlBuilder,
                                                  SqlServerFunctionMetadataOrganizer functionOrganizer,
                                                  SqlServerTableSchemaOrganizer tableOrganizer,
                                                  SqlServerTriggerMetadataOrganizer triggerOrganizer,
                                                  SqlServerViewSchemaOrganizer viewOrganizer,
                                                  SqlServerProcedureMetadataOrganizer procedureOrganizer,
                                                  SqlServerIndexMetadataOrganizer indexOrganizer,
                                                  SqlServerTableRowCountMapBuilder tableRowCountMapBuilder,
                                                  SqlServerTableColumnInfoMapBuilder tableColumnInfoMapBuilder,
                                                  SqlServerViewColumnInfoMapBuilder viewColumnInfoMapBuilder)
        {
            _functionSqlBuilder = functionSqlBuilder ?? throw new ArgumentNullException(nameof(functionSqlBuilder));
            _tableRowCountSqlBuilder = tableRowCountSqlBuilder ?? throw new ArgumentNullException(nameof(tableRowCountSqlBuilder));
            _tableColumnSqlBuilder = tableColumnSqlBuilder ?? throw new ArgumentNullException(nameof(tableColumnSqlBuilder));
            _tableSqlBuilder = tableSqlBuilder ?? throw new ArgumentNullException(nameof(tableSqlBuilder));
            _triggerSqlBuilder = triggerSqlBuilder ?? throw new ArgumentNullException(nameof(triggerSqlBuilder));
            _viewSqlBuilder = viewSqlBuilder ?? throw new ArgumentNullException(nameof(viewSqlBuilder));
            _viewColumnSqlBuilder = viewColumnSqlBuilder ?? throw new ArgumentNullException(nameof(viewColumnSqlBuilder));
            _procedureSqlBuilder = procedureSqlBuilder ?? throw new ArgumentNullException(nameof(procedureSqlBuilder));
            _indexSqlBuilder = indexSqlBuilder ?? throw new ArgumentNullException(nameof(indexSqlBuilder));

            _functionOrganizer = functionOrganizer ?? throw new ArgumentNullException(nameof(functionOrganizer));
            _tableOrganizer = tableOrganizer ?? throw new ArgumentNullException(nameof(tableOrganizer));
            _triggerOrganizer = triggerOrganizer ?? throw new ArgumentNullException(nameof(triggerOrganizer));
            _viewOrganizer = viewOrganizer ?? throw new ArgumentNullException(nameof(viewOrganizer));
            _procedureOrganizer = procedureOrganizer ?? throw new ArgumentNullException(nameof(procedureOrganizer));
            _indexOrganizer = indexOrganizer ?? throw new ArgumentNullException(nameof(indexOrganizer));

            _tableRowCountMapBuilder = tableRowCountMapBuilder ?? throw new ArgumentNullException(nameof(tableRowCountMapBuilder));
            _tableColumnInfoMapBuilder = tableColumnInfoMapBuilder ?? throw new ArgumentNullException(nameof(tableColumnInfoMapBuilder));
            _viewColumnInfoMapBuilder = viewColumnInfoMapBuilder ?? throw new ArgumentNullException(nameof(viewColumnInfoMapBuilder));
        }

        public void Load(SqlServerMetadataContext metadataContext, SqlServerQueryEditorMetadataLoadOptions options, Func<string, DataTable> executeQuery)
        {
            if (metadataContext == null)
            {
                throw new ArgumentNullException(nameof(metadataContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            metadataContext.Validate();
            options.Validate();

            var databaseName = metadataContext.databaseName;

            var dtFunctionInfo = ExecuteQuery
            (
                "SQL: Get Function Information",
                _functionSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var dtTableRowCount = ExecuteQuery
            (
                "SQL: Get Table Row Count Information",
                _tableRowCountSqlBuilder.Build(databaseName),
                executeQuery
            );

            var dtTableColumnInfo = ExecuteQuery
            (
                "SQL: Get Table Column Information",
                _tableColumnSqlBuilder.Build(databaseName, options.sortByColumnName),
                executeQuery
            );

            var dtTableInfo = ExecuteQuery
            (
                "SQL: Get Table Information",
                _tableSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var dtTriggerInfo = ExecuteQuery
            (
                "SQL: Get Trigger Information",
                _triggerSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var dtViewInfo = ExecuteQuery
            (
                "SQL: Get View Information",
                _viewSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var dtViewColumnInfo = ExecuteQuery
            (
                "SQL: Get View Column Information",
                _viewColumnSqlBuilder.Build(databaseName),
                executeQuery
            );

            var dtProcedureInfo = ExecuteQuery
            (
                "SQL: Get Procedure Information",
                _procedureSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var dtIndexInfo = ExecuteQuery
            (
                "SQL: Get Index Information",
                _indexSqlBuilder.Build(databaseName, options.excludeIsMsShippedClause),
                executeQuery
            );

            var rowTableRowCountMap = _tableRowCountMapBuilder.Build(dtTableRowCount);
            var tableColumnInfoMap = _tableColumnInfoMapBuilder.Build(dtTableColumnInfo);
            var viewColumnInfoMap = _viewColumnInfoMapBuilder.Build(dtViewColumnInfo);

            var tableBuildContext = new SqlServerTableSchemaBuildContext
            {
                metadataContext = metadataContext,
                rowCountMap = rowTableRowCountMap,
                columnInfoMap = tableColumnInfoMap
            };

            var viewBuildContext = new SqlServerViewSchemaBuildContext
            {
                metadataContext = metadataContext,
                columnInfoMap = viewColumnInfoMap
            };

            HashSet<string> distinctFunctionNames;
            HashSet<string> distinctTableNames;
            HashSet<string> distinctTriggerNames;
            HashSet<string> distinctViewNames;
            HashSet<string> distinctProcedureNames;
            HashSet<string> distinctIndexNames;

            using (TraceLogger.Time($"Organize Function Info for {databaseName}"))
            {
                distinctFunctionNames = _functionOrganizer.Organize(metadataContext, dtFunctionInfo);
            }

            using (TraceLogger.Time($"Organize Table Info for {databaseName}"))
            {
                distinctTableNames = _tableOrganizer.Organize(tableBuildContext, dtTableInfo);
            }

            using (TraceLogger.Time($"Organize Trigger Info for {databaseName}"))
            {
                distinctTriggerNames = _triggerOrganizer.Organize(metadataContext, dtTriggerInfo);
            }

            using (TraceLogger.Time($"Organize View Info for {databaseName}"))
            {
                distinctViewNames = _viewOrganizer.Organize(viewBuildContext, dtViewInfo);
            }

            using (TraceLogger.Time($"Organize Procedure Info for {databaseName}"))
            {
                distinctProcedureNames = _procedureOrganizer.Organize(metadataContext, dtProcedureInfo);
            }

            using (TraceLogger.Time($"Organize Index Info for {databaseName}"))
            {
                distinctIndexNames = _indexOrganizer.Organize(metadataContext, dtIndexInfo);
            }

            //套用 AutoComplete 關鍵字：Function
            SchemaObjectKeywordApplyHelper.Apply
            (
                distinctFunctionNames,
                options.enableFunctionAutoComplete,
                AutoCompleteSourceNames.Functions,
                keywordText => MyLibrary.KeywordsUserDefinedFunctions = keywordText
            );

            //套用 AutoComplete 關鍵字：Table
            SchemaObjectKeywordApplyHelper.Apply
            (
                distinctTableNames,
                options.enableTableAutoComplete,
                AutoCompleteSourceNames.Tables,
                keywordText => MyLibrary.KeywordsUserDefinedTables = keywordText
            );

            //套用 AutoComplete 關鍵字：Trigger
            SchemaObjectKeywordApplyHelper.Apply
            (
                distinctTriggerNames,
                options.enableTriggerAutoComplete,
                AutoCompleteSourceNames.Triggers,
                keywordText => MyLibrary.KeywordsUserDefinedTriggers = keywordText
            );

            //套用 AutoComplete 關鍵字：View
            SchemaObjectKeywordApplyHelper.Apply
            (
                distinctViewNames,
                options.enableViewAutoComplete,
                AutoCompleteSourceNames.Views,
                keywordText => MyLibrary.KeywordsUserDefinedViews = keywordText
            );

            //Procedure, Index 不自動加入 AutoComplete，也不寫入額外關鍵字欄位。
            _ = distinctProcedureNames;
            _ = distinctIndexNames;
        }

        private static DataTable ExecuteQuery(string traceName, string sql, Func<string, DataTable> executeQuery)
        {
            using (TraceLogger.Time(traceName))
            {
                return executeQuery(sql);
            }
        }
    }
}
