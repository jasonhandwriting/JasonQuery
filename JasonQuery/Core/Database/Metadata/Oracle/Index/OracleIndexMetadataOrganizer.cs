using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Index;
using JasonQuery.Core.Logging;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Index
{
    internal static class OracleIndexMetadataOrganizer
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

            DataTable dtIndexInfo = null;

            try
            {
                dtIndexInfo = GetIndexInfo(context);

                var indexCount = CountDistinctIndexes(dtIndexInfo);
                var indexSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Indexes, indexCount);

                context.TargetSchemaTable.BeginLoadData();

                try
                {
                    using (TraceLogger.Time("Organize Index Info"))
                    {
                        foreach (DataRow drIndexInfo in dtIndexInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                        {
                            var indexName = drIndexInfo.GetSafeString("IndexName");
                            var tableName = drIndexInfo.GetSafeString("TableName");
                            var uniqueness = drIndexInfo.GetSafeString("Uniqueness");
                            var indexType = drIndexInfo.GetSafeString("IndexType");

                            var indexSchemaName = BuildSchemaNameText(indexName, tableName, uniqueness, indexType);
                            var columnInfo = BuildColumnInfo(drIndexInfo);

                            var row = OracleIndexSchemaRowBuilder.Build
                            (
                                context.TargetSchemaTable,
                                context.DbConnectionName,
                                indexSchemaType,
                                indexSchemaName,
                                columnInfo
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
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtIndexInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetIndexInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Index Information"))
            {
                var sql = OracleIndexSqlBuilder.BuildGetIndexInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion

        #region Build / Format
        private static int CountDistinctIndexes(DataTable dtIndexInfo)
        {
            return (dtIndexInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                   .Select(r => r.GetSafeString("IndexName"))
                   .Where(indexName => !string.IsNullOrWhiteSpace(indexName))
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .Count();
        }

        private static string BuildSchemaNameText(string indexName, string tableName, string uniqueness, string indexType)
        {
            var uniquenessText = string.Equals(uniqueness, "UNIQUE", StringComparison.OrdinalIgnoreCase) ? "Unique" : "Non-Unique";
            var indexTypeText = string.IsNullOrWhiteSpace(indexType) ? string.Empty : $", {indexType}";
            var suffixText = $"({uniquenessText}{indexTypeText})";

            if (string.IsNullOrWhiteSpace(tableName))
            {
                return string.Format("{0}{1}{2}", indexName, MyGlobal.Separator, suffixText);
            }

            return string.Format("{0}{1}{2} {3}", indexName, MyGlobal.Separator, tableName, suffixText);
        }

        private static string BuildColumnInfo(DataRow drIndexInfo)
        {
            var columnExpression = drIndexInfo.GetSafeString("ColumnExpression");
            var columnName = drIndexInfo.GetSafeString("ColumnName");
            var descend = drIndexInfo.GetSafeString("Descend");

            var indexedTarget = !string.IsNullOrWhiteSpace(columnExpression) ? columnExpression : columnName;

            if (string.IsNullOrWhiteSpace(indexedTarget))
            {
                return string.Empty;
            }

            return string.Equals(descend, "DESC", StringComparison.OrdinalIgnoreCase)
                   ? string.Format("{0}, DESC", indexedTarget)
                   : string.Format("{0}, ASC", indexedTarget);
        }
        #endregion
    }
}