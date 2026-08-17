using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Procedure;
using JasonQuery.Core.Logging;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Procedure
{
    internal static class OracleProcedureMetadataOrganizer
    {
        #region Entry
        public static void Organize(OracleMetadataContext context)
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

            if (!context.NeedSchemaRows)
            {
                return;
            }

            DataTable dtProcedureInfo = null;

            try
            {
                dtProcedureInfo = GetProcedureInfo(context);

                var procedureSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Procedures, dtProcedureInfo?.Rows.Count ?? 0);

                OrganizeSchemaRows(context, dtProcedureInfo, procedureSchemaType);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtProcedureInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetProcedureInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Procedure Information"))
            {
                var sql = OracleProcedureSqlBuilder.BuildGetProcedureInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion

        #region Organize / Create
        private static void OrganizeSchemaRows(OracleMetadataContext context, DataTable dtProcedureInfo, string procedureSchemaType)
        {
            context.TargetSchemaTable.BeginLoadData();

            try
            {
                using (TraceLogger.Time("Organize Procedure Info"))
                {
                    foreach (DataRow drProcedureInfo in dtProcedureInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var procedureName = drProcedureInfo.GetSafeString("ProcedureName");

                        var row = OracleProcedureSchemaRowBuilder.Build
                        (
                            context.TargetSchemaTable,
                            context.DbConnectionName,
                            procedureSchemaType,
                            procedureName
                        );

                        context.TargetSchemaTable.Rows.Add(row);
                    }
                }
            }
            finally
            {
                context.TargetSchemaTable.EndLoadData();
            }
        }
        #endregion
    }
}