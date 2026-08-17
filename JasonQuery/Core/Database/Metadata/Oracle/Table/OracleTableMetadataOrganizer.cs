using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Formatters;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Table
{
    internal static class OracleTableMetadataOrganizer
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

            DataTable dtTableInfo = null;
            DataTable dtTableQty = null;

            try
            {
                dtTableInfo = GetTableInfo(context);

                Dictionary<string, long> tableRowCountMap = null;
                var tableCount = 0;

                if (context.NeedSchemaRows)
                {
                    dtTableQty = GetTableQuantity(context);
                    tableRowCountMap = BuildTableRowCountMap(dtTableQty);
                    tableCount = dtTableQty?.Rows.Count ?? 0;
                }

                var tableSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Tables, tableCount);
                var distinctTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (context.NeedSchemaRows)
                {
                    context.TargetSchemaTable.BeginLoadData();
                }

                try
                {
                    using (TraceLogger.Time("Organize Table Info"))
                    {
                        foreach (DataRow drTableInfo in dtTableInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                        {
                            var tableName = drTableInfo.GetSafeString("TableName");

                            if (context.NeedSchemaRows)
                            {
                                var tableSchemaName = BuildSchemaNameText(tableName, tableRowCountMap);
                                var columnInfo = BuildColumnInfo(drTableInfo);

                                var row = OracleTableSchemaRowBuilder.Build
                                (
                                    context.TargetSchemaTable,
                                    context.DbConnectionName,
                                    tableSchemaType,
                                    tableSchemaName,
                                    columnInfo
                                );

                                context.TargetSchemaTable.Rows.Add(row);
                            }

                            if (!string.IsNullOrWhiteSpace(tableName))
                            {
                                distinctTableNames.Add(tableName);
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

                return distinctTableNames;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtTableInfo);
                DataTableLifecycleHelper.DisposeDataTable(ref dtTableQty);
            }
        }
        #endregion

        #region Get
        private static DataTable GetTableInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Column Information"))
            {
                var sql = OracleTableSqlBuilder.BuildGetTableInfoSql(context.DbUserNameUppercase, context.SortByColumnName);

                return context.ExecuteQuery(sql);
            }
        }

        private static DataTable GetTableQuantity(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get the Total Number of Entries for all Tables"))
            {
                var sql = OracleTableSqlBuilder.BuildGetTableQuantitySql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion

        #region Build / Format
        private static Dictionary<string, long> BuildTableRowCountMap(DataTable dtTableQty)
        {
            return (dtTableQty?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                               .ToDictionary
                                (
                                    r => r.GetSafeString("TableName"),
                                    r => r.GetSafeLong("NumRows"),
                                    StringComparer.Ordinal
                                );
        }

        private static string BuildSchemaNameText(string tableName, Dictionary<string, long> tableRowCountMap)
        {
            long rowCount = 0;

            if (tableRowCountMap != null)
            {
                tableRowCountMap.TryGetValue(tableName, out rowCount);
            }

            return string.Format("{0}{1}({2})", tableName, MyGlobal.Separator, DataSizeFormatter.FormatInt(rowCount));
        }

        private static string BuildColumnInfo(DataRow drTableInfo)
        {
            var columnName = drTableInfo.GetSafeString("ColumnName");
            var dataType = drTableInfo.GetSafeString("DataType");
            var dataLength = drTableInfo.GetSafeString("DataLength");
            var scale = drTableInfo.GetSafeString("Scale").Trim(',');

            var formattedDataType = FormatColumnDataType(dataType, dataLength, scale);

            return string.Format("{0}, {1}", columnName, formattedDataType);
        }

        private static string FormatColumnDataType(string dataType, string dataLength, string scale)
        {
            switch (dataType)
            {
                case "RAW":
                case "CHAR":
                case "NCHAR":
                case "VARCHAR":
                case "VARCHAR2":
                case "NVARCHAR2":
                    {
                        return string.Format("{0}({1})", dataType, dataLength);
                    }
                case "FLOAT":
                    {
                        return string.Format("{0}({1})", dataType, scale);
                    }
                case "NUMBER":
                    {
                        if (string.IsNullOrWhiteSpace(scale))
                        {
                            return "NUMBER";
                        }

                        return scale == "0" ? "INTEGER" : string.Format("{0}({1})", dataType, scale);
                    }
                default:
                    {
                        return dataType;
                    }
            }
        }
        #endregion
    }
}