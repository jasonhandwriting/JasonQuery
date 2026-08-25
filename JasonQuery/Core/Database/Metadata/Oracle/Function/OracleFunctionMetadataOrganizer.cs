using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Function;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Function
{
    internal static class OracleFunctionMetadataOrganizer
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

            DataTable dtFunctionInfo = null;

            try
            {
                dtFunctionInfo = GetFunctionInfo(context);

                var functionSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Functions, dtFunctionInfo?.Rows.Count ?? 0);
                var distinctFunctionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (context.NeedSchemaRows)
                {
                    context.TargetSchemaTable.BeginLoadData();
                }

                try
                {
                    using (TraceLogger.Time("Organize Function Info"))
                    {
                        foreach (DataRow drFunctionInfo in dtFunctionInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                        {
                            var functionName = drFunctionInfo.GetSafeString("FunctionName");

                            if (context.NeedSchemaRows)
                            {
                                var row = OracleFunctionSchemaRowBuilder.Build
                                (
                                    context.TargetSchemaTable,
                                    context.DbConnectionName,
                                    functionSchemaType,
                                    functionName
                                );

                                context.TargetSchemaTable.Rows.Add(row);
                            }

                            if (!string.IsNullOrWhiteSpace(functionName))
                            {
                                distinctFunctionNames.Add(functionName);
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

                return distinctFunctionNames;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtFunctionInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetFunctionInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Function Information"))
            {
                var sql = OracleFunctionSqlBuilder.BuildGetFunctionInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion
    }
}
