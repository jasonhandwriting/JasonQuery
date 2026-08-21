using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.QueryEditor;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Builders
{
    internal static class QueryEditorAutoCompleteDataTableBuilder
    {
        //for Space, 場景一：from / insert into / delete from / update + 空白
        public static DataTable BuildAutoCompleteSourceRowsForSpace(DataTable dtTableAndViewName, DataSourceType currentSourceType,
                                                                    AutoCompleteObjectLookupMode lookupMode, string currentDatabase)
        {
            var dt = CreateAutoCompleteSourceRowsTable();

            if (dtTableAndViewName == null || dtTableAndViewName.Rows.Count == 0)
            {
                return dt;
            }

            IEnumerable<DataRow> rows = dtTableAndViewName.AsEnumerable();

            switch (lookupMode)
            {
                case AutoCompleteObjectLookupMode.DatabaseOnly:
                    {
                        rows = rows.Where(r => string.Equals(r.GetSafeString("Memo"), "DB", StringComparison.OrdinalIgnoreCase));
                        break;
                    }
                case AutoCompleteObjectLookupMode.TableOnly:
                    {
                        rows = rows.Where(r => string.Equals(r.GetSafeString("SchemaType"), QueryEditorSchemaTypeNames.Table, StringComparison.Ordinal));
                        break;
                    }
                case AutoCompleteObjectLookupMode.TableOrView:
                    {
                        rows = rows.Where(r => r.GetSafeString("SchemaType").StartsWith(QueryEditorSchemaTypeNames.Table, StringComparison.Ordinal) ||
                                               r.GetSafeString("SchemaType").StartsWith(QueryEditorSchemaTypeNames.View, StringComparison.Ordinal));
                        break;
                    }
            }

            //SQL Server, MySQL：依目前 DB 過濾
            if ((currentSourceType == DataSourceType.SqlServer || currentSourceType == DataSourceType.MySql) &&
                 lookupMode != AutoCompleteObjectLookupMode.DatabaseOnly && !string.IsNullOrWhiteSpace(currentDatabase))
            {
                var currentDb = currentDatabase.ToUpperInvariant();

                rows = rows.Where(r => string.Equals(r.GetSafeString("DB"), currentDb, StringComparison.OrdinalIgnoreCase));
            }

            dt.BeginLoadData();

            try
            {
                if (lookupMode == AutoCompleteObjectLookupMode.DatabaseOnly)
                {
                    foreach (var dbName in rows.Select(r => r.GetSafeString("DB"))
                                               .Where(s => !string.IsNullOrWhiteSpace(s))
                                               .Distinct(StringComparer.OrdinalIgnoreCase)
                                               .OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
                    {
                        var row = dt.NewRow();

                        row["SchemaName"] = dbName;
                        row["SchemaType"] = QueryEditorSchemaTypeNames.Database;
                        row["AllowDBNull"] = string.Empty;

                        dt.Rows.Add(row);
                    }
                }
                else
                {
                    foreach (var dr in rows.OrderBy(r => r.GetSafeString("SchemaName"), StringComparer.OrdinalIgnoreCase)
                                           .ThenBy(r => r.GetSafeString("SchemaType"), StringComparer.OrdinalIgnoreCase))
                    {
                        var row = dt.NewRow();
                        var schemaType = dr.GetSafeString("SchemaType");
                        var schemaName = dr.GetSafeString("SchemaName");
                        var suffix = BuildSchemaTypeSuffix(dr, currentSourceType);

                        row["SchemaName"] = schemaName;
                        row["SchemaType"] = $"{schemaType}{suffix}";
                        row["AllowDBNull"] = string.Empty;

                        dt.Rows.Add(row);
                    }
                }
            }
            finally
            {
                dt.EndLoadData();
            }

            return dt;
        }

        //for Space, 場景二：select _ from mytable (_ 代表按下空白鍵，或是 mytable 後面是 where/and/or/by 等關鍵字再接著空白鍵)，先取得 mytable 的 schema，再整理至 _dtAutoCompleteForSpace
        public static DataTable BuildAutoCompleteSourceRowsForSpace(DataTable dtSchemaTable, DataSourceType currentSourceType)
        {
            var dt = CreateAutoCompleteSourceRowsTable();

            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                return dt;
            }

            var columnInfoCollector = SchemaColumnInfoBuilder.Build(currentSourceType, dtSchemaTable);

            dt.BeginLoadData();

            try
            {
                foreach (DataRow dr in dtSchemaTable.AsEnumerable())
                {
                    var columnName = dr.GetSafeString("ColumnName");

                    if (columnInfoCollector != null && columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        var row = dt.NewRow();
                        var dataType = string.Empty;
                        var isKey = columnInfo.IsPrimaryKey;
                        var allowDBNull = isKey ? "P" : columnInfo.IsNullable ? string.Empty : "N";

                        if (!string.IsNullOrWhiteSpace(columnInfo.FullDataType))
                        {
                            dataType = columnInfo.FullDataType;
                        }
                        else if (!string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
                        {
                            dataType = columnInfo.BaseDataType;
                        }

                        row["SchemaName"] = columnName;
                        row["SchemaType"] = dataType;
                        row["AllowDBNull"] = allowDBNull;

                        dt.Rows.Add(row);
                    }
                }
            }
            finally
            {
                dt.EndLoadData();
            }

            return dt;
        }

        //for Space
        public static DataTable BuildAutoCompleteSpaceTable(DataTable dtSourceRows)
        {
            var dt = CreateAutoCompleteSpaceTable();

            if (dtSourceRows == null || dtSourceRows.Rows.Count == 0)
            {
                return dt;
            }

            dt.BeginLoadData();

            try
            {
                foreach (DataRow dr in dtSourceRows.AsEnumerable())
                {
                    var row = dt.NewRow();

                    row["ColumnName"] = dr.GetSafeString("SchemaName");
                    row["DataType"] = dr.GetSafeString("SchemaType");
                    row["AllowDBNull"] = dtSourceRows.Columns.Contains("AllowDBNull") ? dr.GetSafeString("AllowDBNull") : string.Empty;

                    dt.Rows.Add(row);
                }
            }
            finally
            {
                dt.EndLoadData();
            }

            return dt;
        }

        private static DataTable CreateAutoCompleteSourceRowsTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("SchemaName");
            dt.Columns.Add("SchemaType");
            dt.Columns.Add("AllowDBNull");
            return dt;
        }

        private static DataTable CreateAutoCompleteSpaceTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("ColumnName");
            dt.Columns.Add("DataType");
            dt.Columns.Add("AllowDBNull");
            return dt;
        }

        private static string BuildSchemaTypeSuffix(DataRow dr, DataSourceType currentSourceType)
        {
            switch (currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var schema = dr.GetSafeString("Schema");

                        return string.IsNullOrEmpty(schema) ? string.Empty : $", [{schema}]";
                    }
                case DataSourceType.SqlServer:
                    {
                        return string.Empty;
                    }
                case DataSourceType.PostgreSql:
                    {
                        var schemaNode = dr.GetSafeString("SchemaNode");

                        return string.IsNullOrEmpty(schemaNode) ? string.Empty : $", [{schemaNode}]";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        public static DataTable BuildAutoCompleteSourceRowsForPeriod(DataTable dtSchemaTable, DataSourceType currentSourceType)
        {
            var dt = CreateAutoCompleteSourceRowsTableForPeriod();

            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                return dt;
            }

            //20260322 針對不同資料庫來源類型，建立對應的 ColumnInfoCollector，一次性收集欄位相關訊息
            var columnInfoCollector = SchemaColumnInfoBuilder.Build(currentSourceType, dtSchemaTable);

            dt.BeginLoadData();

            try
            {
                foreach (DataRow dr in dtSchemaTable.AsEnumerable())
                {
                    var columnName = dr.GetSafeString("ColumnName");

                    if (columnInfoCollector != null &&
                        columnInfoCollector.TryGet
                        (
                            columnName,
                            dr.GetSafeString("BaseSchemaName"),
                            dr.GetSafeString("BaseTableName"),
                            out var columnInfo
                        ))
                    {
                        var baseSchemaName = columnInfo.BaseSchemaName;
                        var baseTableName = columnInfo.BaseTableName;
                        var isKey = columnInfo.IsPrimaryKey;
                        var allowDBNull = columnInfo.IsNullable;
                        var row = dt.NewRow();

                        if (!string.IsNullOrWhiteSpace(baseSchemaName))
                        {
                            baseSchemaName = $"{baseSchemaName}.";
                        }

                        row["ColumnName"] = columnName;
                        row["DataType"] = columnInfo.FullDataType;

                        var allowDBNullValue = isKey ? "P" : allowDBNull ? string.Empty : "N";

                        row["BaseTableName"] = $"[{baseSchemaName}{baseTableName}]";
                        row["AllowDBNull"] = allowDBNullValue;

                        dt.Rows.Add(row);
                    }
                }
            }
            finally
            {
                dt.EndLoadData();
            }

            return dt;
        }

        private static DataTable CreateAutoCompleteSourceRowsTableForPeriod()
        {
            var dt = new DataTable();

            dt.Columns.Add("ColumnName");
            dt.Columns.Add("DataType");
            dt.Columns.Add("BaseTableName");
            dt.Columns.Add("AllowDBNull");
            return dt;
        }
    }
}