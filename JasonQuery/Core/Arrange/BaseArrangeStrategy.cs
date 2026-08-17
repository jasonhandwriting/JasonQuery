using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Builders;
using JasonQuery.Core.Arrange.Formatting;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Value;
using JasonQuery.Core.QueryEngine.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Arrange
{
    public abstract class BaseArrangeStrategy : IArrangeStrategy
    {
        private readonly ColumnValueFormatterRegistry _formatterRegistry;

        private struct ColumnMap
        {
            public int SourceIdx;
            public int TargetIdx;
            public ColumnInfo Info;
        }

        protected BaseArrangeStrategy(IEnumerable<IColumnValueFormatter> formatters = null)
        {
            _formatterRegistry = new ColumnValueFormatterRegistry(formatters);
        }

        protected virtual void PrepareSchema(ArrangeContext context)
        {
            //Default: do nothing
        }

        public void Execute(ArrangeContext context)
        {
            PrepareSchema(context);
            Arrange(context);
        }

        public void Arrange(ArrangeContext context)
        {
            context.SortedData = BuildSortedData(context);
        }

        protected DataTable BuildSortedData(ArrangeContext context)
        {
            var shouldLoadColumnComments = context.ShowColumnComments || context.LoadColumnCommentsForCellTip;

            if (shouldLoadColumnComments)
            {
                var tables = context.SchemaTable.AsEnumerable()
                    .Select
                     (
                         r => (
                                  Schema: r.GetSafeString("BaseSchemaName"),
                                  Table: r.GetSafeString("BaseTableName")
                              )
                     )
                    .Where(t => !string.IsNullOrWhiteSpace(t.Schema) && !string.IsNullOrWhiteSpace(t.Table))
                    .Distinct()
                    .ToList();

                var dtColumnComments = DatabaseSqlExecutor.GetColumnComments(tables);

                //更新所有欄位的註解
                DataTableSearchHelper.UpdateColumnComments(context.SchemaTable, dtColumnComments);

                //判斷是否有非空的 Comment 欄位
                var firstNonEmptyComment = (context.SchemaTable.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    .FirstOrDefault
                     (
                         dr => !string.IsNullOrWhiteSpace(dr.GetSafeString("Comment") ?? string.Empty)
                     );

                if (firstNonEmptyComment != null)
                {
                    context.HasAnyComment = true;
                }
            }

            if (context.ShowColumnDefaultValue)
            {
                var baseSchemaName = string.Empty;
                var baseSchemaNode = string.Empty;
                var baseTableName = string.Empty;

                foreach (var row in context.SchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var schemaName = row.GetSafeString("BaseSchemaName");
                    var tableName = row.GetSafeString("BaseTableName");
                    var catalogName = row.GetSafeString("BaseCatalogName");

                    if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(tableName))
                    {
                        continue;
                    }

                    baseSchemaName = schemaName;
                    baseTableName = tableName;
                    baseSchemaNode = catalogName;
                    break;
                }

                var dtColumnDefaultValue = DatabaseSqlExecutor.GetColumnDefaultValue(baseSchemaName, baseTableName, baseSchemaNode);

                if (dtColumnDefaultValue != null && dtColumnDefaultValue.Rows.Count > 0)
                {
                    //更新所有欄位的預設值，此處的 DefaultValue 是給 SchemaBrowserForm 的 Data 頁籤用的，用於「新增一筆空白資料」要帶入的 DefaultValue
                    DataTableSearchHelper.UpdateColumnDefaultValues(context.SchemaTable, dtColumnDefaultValue, context.NullDisplayText);
                }
            }

            var dtSortedData = new DataTable();
            var commentMap = shouldLoadColumnComments ? BuildSchemaColumnValueMap(context.SchemaTable, "Comment") : null;
            var defaultValueMap = context.ShowColumnDefaultValue ? BuildSchemaColumnValueMap(context.SchemaTable, "DefaultValue") : null;

            foreach (DataColumn column in context.SourceData.Columns)
            {
                var columnName = column.ColumnName;

                if (!context.columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    dtSortedData.Columns.Add(columnName, typeof(string));
                    continue;
                }

                var needUpdateCollector = false;

                if (commentMap != null && commentMap.TryGetValue(columnName, out var comment))
                {
                    columnInfo.ColumnComment = comment;
                    needUpdateCollector = true;
                }

                if (defaultValueMap != null && defaultValueMap.TryGetValue(columnName, out var defaultValue))
                {
                    columnInfo.ColumnDefaultValue = defaultValue;
                    needUpdateCollector = true;
                }

                if (needUpdateCollector)
                {
                    context.columnInfoCollector.AddOrUpdate(columnInfo);
                }

                //Determine the field type of dtSortedData
                if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
                {
                    dtSortedData.Columns.Add(columnName, typeof(LargeBinaryDataType));
                }
                else if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
                {
                    dtSortedData.Columns.Add(columnName, typeof(LargeTextDataType));
                }
                else
                {
                    //All other fields should use the string type, because null might be filled with a specified string such as <NULL>
                    dtSortedData.Columns.Add(columnName, typeof(string));
                }
            }

            var maps = new List<ColumnMap>();

            foreach (DataColumn col in context.SourceData.Columns)
            {
                if (!context.columnInfoCollector.TryGet(col.ColumnName, out var info))
                {
                    continue;
                }

                var targetCol = dtSortedData.Columns[col.ColumnName];

                if (targetCol == null)
                {
                    continue;
                }

                maps.Add(new ColumnMap
                {
                    SourceIdx = col.Ordinal,
                    TargetIdx = targetCol.Ordinal,
                    Info = info
                });
            }

            var activeColumns = maps.ToArray();
            var columnCount = dtSortedData.Columns.Count;
            var rowValue = new object[columnCount];

            dtSortedData.BeginLoadData();

            try
            {
                var nullShowAs = context.NullDisplayText ?? string.Empty;

                foreach (DataRow row in context.SourceData.Rows)
                {
                    if (context.IsCancellationRequested?.Invoke() == true)
                    {
                        break;
                    }

                    Array.Clear(rowValue, 0, columnCount);

                    foreach (var item in activeColumns)
                    {
                        var rawValue = row[item.SourceIdx];
                        var dbNull = DbValueHelper.IsDbNull(rawValue);

                        if (dbNull)
                        {
                            //20260314 Null value: use GetNullCellValue to determine the display value,
                            //especially for LargeBinary and LargeText which require special handling
                            rowValue[item.TargetIdx] = GetNullCellValue(item.Info, nullShowAs);
                            continue;
                        }

                        //20260312 LargeBinary: Grid 僅顯示型別與長度，完整內容交由 CellViewer 載入
                        if (item.Info.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
                        {
                            rowValue[item.TargetIdx] = LargeBinaryValueBuilder.Build(rawValue, item.Info, context);
                            continue;
                        }

                        //20260312 LargeText: Grid 顯示摘要與預覽，完整內容交由 CellViewer 載入
                        if (item.Info.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
                        {
                            rowValue[item.TargetIdx] = LargeTextValueBuilder.Build(rawValue, item.Info, context, _formatterRegistry);
                            continue;
                        }

                        if (_formatterRegistry.TryFormat(rawValue, item.Info, context, out var formatted))
                        {
                            rowValue[item.TargetIdx] = formatted;
                            continue;
                        }

                        rowValue[item.TargetIdx] = rawValue?.ToString() ?? string.Empty;
                    }

                    dtSortedData.LoadDataRow(rowValue, true);
                }
            }
            finally
            {
                dtSortedData.EndLoadData();
            }

            return dtSortedData;
        }

        private static Dictionary<string, string> BuildSchemaColumnValueMap(DataTable schemaTable, string valueColumnName)
        {
            if (schemaTable == null || schemaTable.Rows.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            if (!schemaTable.Columns.Contains("ColumnName") || !schemaTable.Columns.Contains(valueColumnName))
            {
                return new Dictionary<string, string>();
            }

            return schemaTable.AsEnumerable().Where
                                              (
                                                  r => !string.IsNullOrWhiteSpace(r.GetSafeString("ColumnName"))
                                              )
                                             .GroupBy
                                              (
                                                  r => r.GetSafeString("ColumnName").Trim()
                                              )
                                             .ToDictionary
                                              (
                                                  g => g.Key,
                                                  g => g.First().GetSafeString(valueColumnName)
                                              );
        }

        /// <summary>
        /// For LargeText/LargeBinary columns, DB NULL must still be wrapped
        /// in the correct typed container, otherwise DataTable type assignment may fail.
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="nullShowAs"></param>
        /// <returns></returns>
        private static object GetNullCellValue(ColumnInfo columnInfo, string nullShowAs)
        {
            if (columnInfo == null)
            {
                return nullShowAs;
            }

            if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
            {
                return LargeBinaryDataType.CreateNull(nullShowAs);
            }

            if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
            {
                return LargeTextDataType.CreateNull(nullShowAs);
            }

            return nullShowAs;
        }
    }
}