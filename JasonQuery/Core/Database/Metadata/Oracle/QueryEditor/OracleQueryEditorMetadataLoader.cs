using JasonLibrary.Core;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Metadata.Oracle.Function;
using JasonQuery.Core.Database.Metadata.Oracle.Index;
using JasonQuery.Core.Database.Metadata.Oracle.Package;
using JasonQuery.Core.Database.Metadata.Oracle.Procedure;
using JasonQuery.Core.Database.Metadata.Oracle.Table;
using JasonQuery.Core.Database.Metadata.Oracle.Trigger;
using JasonQuery.Core.Database.Metadata.Oracle.View;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor;
using JasonQuery.Core.Logging;
using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle.QueryEditor
{
    //Candidate object names are not published to MyLibrary/MyGlobal during Prepare.
    internal sealed class OracleSchemaObjectNameBatch
    {
        public System.Collections.Generic.IEnumerable<string> Functions { get; set; }
        public System.Collections.Generic.IEnumerable<string> Tables { get; set; }
        public System.Collections.Generic.IEnumerable<string> Triggers { get; set; }
        public System.Collections.Generic.IEnumerable<string> Views { get; set; }
    }

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

                    if (dtNewTableAndViewName == null)
                    {
                        throw new InvalidOperationException("Oracle table/view name metadata query returned null; the schema refresh is rejected.");
                    }
                }

                DataTableLifecycleHelper.ReplaceDataTable(ref dtTableAndViewName, ref dtNewTableAndViewName);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtNewTableAndViewName);
            }
        }

        //The atomic Oracle loader uses this no-global-write entry during Prepare.
        //The original LoadSchemaMetadata method remains available for existing callers.
        internal static OracleSchemaObjectNameBatch CollectSchemaMetadata(OracleMetadataContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var distinctFunctionNames = OracleFunctionMetadataOrganizer.Organize(context);
            var distinctTableNames = OracleTableMetadataOrganizer.Organize(context);
            var distinctTriggerNames = OracleTriggerMetadataOrganizer.Organize(context);
            var distinctViewNames = OracleViewMetadataOrganizer.Organize(context);

            //Procedure, Package, Index organize schema rows only; no global writes.
            OracleProcedureMetadataOrganizer.Organize(context);
            OraclePackageMetadataOrganizer.Organize(context);
            OracleIndexMetadataOrganizer.Organize(context);

            return new OracleSchemaObjectNameBatch
            {
                Functions = distinctFunctionNames,
                Tables = distinctTableNames,
                Triggers = distinctTriggerNames,
                Views = distinctViewNames
            };
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

            var batch = CollectSchemaMetadata(context);

            SchemaObjectKeywordApplyHelper.Apply
            (
                batch.Functions,
                options.EnableFunctionAutoComplete,
                AutoCompleteSourceNames.Functions,
                keywordText => MyLibrary.KeywordsUserDefinedFunctions = keywordText
            );

            SchemaObjectKeywordApplyHelper.Apply
            (
                batch.Tables,
                options.EnableTableAutoComplete,
                AutoCompleteSourceNames.Tables,
                keywordText => MyLibrary.KeywordsUserDefinedTables = keywordText
            );

            SchemaObjectKeywordApplyHelper.Apply
            (
                batch.Triggers,
                options.EnableTriggerAutoComplete,
                AutoCompleteSourceNames.Triggers,
                keywordText => MyLibrary.KeywordsUserDefinedTriggers = keywordText
            );

            SchemaObjectKeywordApplyHelper.Apply
            (
                batch.Views,
                options.EnableViewAutoComplete,
                AutoCompleteSourceNames.Views,
                keywordText => MyLibrary.KeywordsUserDefinedViews = keywordText
            );
        }
    }
}
