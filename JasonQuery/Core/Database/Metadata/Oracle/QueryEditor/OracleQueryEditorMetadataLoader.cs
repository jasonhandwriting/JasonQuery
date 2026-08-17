using JasonLibrary.Core;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Metadata.Oracle.Function;
using JasonQuery.Core.Database.Metadata.Oracle.Index;
using JasonQuery.Core.Database.Metadata.Oracle.Package;
using JasonQuery.Core.Database.Metadata.Oracle.Procedure;
using JasonQuery.Core.Database.Metadata.Oracle.Table;
using JasonQuery.Core.Database.Metadata.Oracle.Trigger;
using JasonQuery.Core.Database.Metadata.Oracle.View;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor;
using JasonQuery.Core.Logging;
using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.QueryEditor
{
    internal static class OracleQueryEditorMetadataLoader
    {
        public delegate DataTable LoadDataTableDelegate(string sql);

        public static void LoadTableAndViewNames(ref DataTable dtTableAndViewName, LoadDataTableDelegate loadDataTable, string ownerUppercase)
        {
            if (loadDataTable == null)
            {
                throw new ArgumentNullException(nameof(loadDataTable));
            }

            DataTable dtNewTableAndViewName = null;

            try
            {
                using (TraceLogger.Time("SQL: Get Table and View Information"))
                {
                    var sqlAllTableAndView = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql(ownerUppercase);

                    dtNewTableAndViewName = loadDataTable(sqlAllTableAndView);
                }

                DataTableLifecycleHelper.ReplaceDataTable(ref dtTableAndViewName, ref dtNewTableAndViewName);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtNewTableAndViewName);
            }
        }

        public static void LoadSchemaMetadata(OracleMetadataContext context, OracleQueryEditorMetadataLoadOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var distinctFunctionNames = OracleFunctionMetadataOrganizer.Organize(context);
            var distinctTableNames = OracleTableMetadataOrganizer.Organize(context);
            var distinctTriggerNames = OracleTriggerMetadataOrganizer.Organize(context);
            var distinctViewNames = OracleViewMetadataOrganizer.Organize(context);

            //Procedure, Package, Index 僅整理 Schema Rows，不處理 AutoComplete / Keywords
            OracleProcedureMetadataOrganizer.Organize(context);
            OraclePackageMetadataOrganizer.Organize(context);
            OracleIndexMetadataOrganizer.Organize(context);

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
    }
}