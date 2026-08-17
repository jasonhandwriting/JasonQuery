using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Trigger;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Trigger
{
    internal static class OracleTriggerMetadataOrganizer
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

            DataTable dtTriggerInfo = null;

            try
            {
                dtTriggerInfo = GetTriggerInfo(context);

                var triggerSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Triggers, dtTriggerInfo?.Rows.Count ?? 0);
                var distinctTriggerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (context.NeedSchemaRows)
                {
                    context.TargetSchemaTable.BeginLoadData();
                }

                try
                {
                    using (TraceLogger.Time("Organize Trigger Info"))
                    {
                        foreach (DataRow drTriggerInfo in dtTriggerInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                        {
                            var triggerName = drTriggerInfo.GetSafeString("TriggerName");

                            if (context.NeedSchemaRows)
                            {
                                var row = OracleTriggerSchemaRowBuilder.Build
                                (
                                    context.TargetSchemaTable,
                                    context.DbConnectionName,
                                    triggerSchemaType,
                                    triggerName
                                );

                                context.TargetSchemaTable.Rows.Add(row);
                            }

                            if (!string.IsNullOrWhiteSpace(triggerName))
                            {
                                distinctTriggerNames.Add(triggerName);
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

                return distinctTriggerNames;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtTriggerInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetTriggerInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Trigger Information"))
            {
                var sql = OracleTriggerSqlBuilder.BuildGetTriggerInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion
    }
}