using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.View;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.View
{
    internal static class OracleViewMetadataOrganizer
    {
        #region Entry
        public static HashSet<string> Organize(OracleMetadataContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            if (context.ExecuteQuery == null)
            {
                throw new ArgumentNullException(nameof(context.ExecuteQuery));
            }

            DataTable dtViewInfo = null;
            DataTable dtViewColumnInfo = null;

            try
            {
                dtViewInfo = GetViewInfo(context);

                Dictionary<string, List<DataRow>> viewColumnInfoMap = null;
                var viewSchemaType = string.Empty;

                if (context.NeedSchemaRows)
                {
                    dtViewColumnInfo = GetViewColumnInfo(context);
                    viewColumnInfoMap = BuildViewColumnInfoMap(dtViewColumnInfo);
                    viewSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Views, dtViewInfo?.Rows.Count ?? 0);
                }

                var distinctViewNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var schemaColumns = context.TargetSchemaTable.Columns;

                if (context.NeedSchemaRows)
                {
                    context.TargetSchemaTable.BeginLoadData();
                }

                try
                {
                    using (TraceLogger.Time("Organize View Info"))
                    {
                        foreach (DataRow drViewInfo in dtViewInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                        {
                            var viewName = drViewInfo.GetSafeString("ViewName");

                            if (context.NeedSchemaRows)
                            {
                                AddSchemaRows(context, schemaColumns, viewSchemaType, viewName, viewColumnInfoMap);
                            }

                            if (!string.IsNullOrWhiteSpace(viewName))
                            {
                                distinctViewNames.Add(viewName);
                            }
                        }
                    }
                }
                finally
                {
                    if (context.NeedSchemaRows)
                    {
                        context.TargetSchemaTable.EndLoadData();
                    }
                }

                return distinctViewNames;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtViewInfo);
                DataTableLifecycleHelper.DisposeDataTable(ref dtViewColumnInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetViewInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get View Information"))
            {
                var sql = OracleViewSqlBuilder.BuildGetViewInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }

        private static DataTable GetViewColumnInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get View Information (Including Column Name and Type)"))
            {
                var sql = OracleViewSqlBuilder.BuildGetViewColumnInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion

        #region Build / Format
        private static Dictionary<string, List<DataRow>> BuildViewColumnInfoMap(DataTable dtViewColumnInfo)
        {
            return (dtViewColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                   .GroupBy
                    (
                        r => r.GetSafeString("ViewName"),
                        StringComparer.OrdinalIgnoreCase
                    )
                   .ToDictionary
                    (
                        g => g.Key,
                        g => g.ToList(),
                        StringComparer.OrdinalIgnoreCase
                    );
        }

        private static string BuildColumnInfo(DataRow drViewColumnInfo)
        {
            var columnName = drViewColumnInfo.GetSafeString("ColumnName");
            var columnType = drViewColumnInfo.GetSafeString("ColumnType");

            return string.Format("{0}, {1}", columnName, columnType);
        }
        #endregion

        #region Organize / Create
        private static void AddSchemaRows(OracleMetadataContext context, DataColumnCollection schemaColumns, string viewSchemaType,
                                          string viewName, Dictionary<string, List<DataRow>> viewColumnInfoMap)
        {
            List<DataRow> viewColumns = null;

            if (viewColumnInfoMap != null)
            {
                viewColumnInfoMap.TryGetValue(viewName, out viewColumns);
            }

            foreach (var drViewColumnInfo in viewColumns ?? Enumerable.Empty<DataRow>())
            {
                var row = context.TargetSchemaTable.NewRow();

                row["SchemaObject"] = context.DbConnectionName;
                row["SchemaType"] = viewSchemaType;

                if (schemaColumns.Contains("Schema_Browser"))
                {
                    row["Schema_Browser"] = viewName;
                }
                else
                {
                    row["SchemaName"] = viewName;
                }

                var columnInfo = BuildColumnInfo(drViewColumnInfo);

                if (schemaColumns.Contains("ColumnInfo"))
                {
                    row["ColumnInfo"] = columnInfo;
                }
                else
                {
                    row["SchemaName"] = columnInfo;
                }

                context.TargetSchemaTable.Rows.Add(row);
            }
        }
        #endregion
    }
}
