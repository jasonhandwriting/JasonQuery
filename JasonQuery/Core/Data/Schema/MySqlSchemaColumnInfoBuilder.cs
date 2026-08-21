using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Readers;
using JasonQuery.Core.Schema;
using JasonQuery.Database.Providers.Executor;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Data.Schema
{
    internal static class MySqlSchemaColumnInfoBuilder
    {
        public static ColumnInfoCollector Build(DataTable dtSchemaTable)
        {
            var isUsedProviderFallback = false;
            var collector = new ColumnInfoCollector();

            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "UsedProviderFallback"); //20260306 新增 UsedProviderFallback 欄位
            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "DataType2"); //20260322 新增 DataType2 欄位

            foreach (DataRow dr in dtSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var baseDataType = SchemaRowReader.GetDataType(dr);
                var dataType = SchemaRowReader.GetDataType(dr);
                var columnSize = SchemaRowReader.GetColumnSize(dr);
                var numericScale = SchemaRowReader.GetNumericScale(dr);
                var numericPrecision = SchemaRowReader.GetNumericPrecision(dr);
                var providerSpecificDataType = SchemaRowReader.GetProviderSpecificDataType(dr);
                var providerType = SchemaRowReader.GetProviderType(dr);
                var isEnum = SchemaRowReader.GetIsEnum(dr);
                var isSet = SchemaRowReader.GetIsSet(dr);

                var resolver = MySqlColumnTypeResolver.Resolve(baseDataType, dataType, columnSize, numericPrecision,
                                                               numericScale, providerType, isEnum, isSet);

                if (resolver.UsedProviderFallback)
                {
                    isUsedProviderFallback = true;
                    dr["UsedProviderFallback"] = "Y";
                }
                else
                {
                    dr["DataType2"] = resolver.BaseDataType;
                }

                SchemaColumnInfoBuilderHelper.AddOrUpdateColumnInfo
                (
                    collector,
                    dr,
                    dataType,
                    resolver.FullDataType,
                    resolver.BaseDataType,
                    columnSize,
                    numericPrecision,
                    numericScale,
                    providerSpecificDataType,
                    resolver.SpecialDataTypeKind,
                    resolver.CategoryDataTypeKind,
                    info =>
                    {
                        info.UsedProviderFallback = resolver.UsedProviderFallback;
                        info.IsUpdateValueSupported = resolver.IsUpdateValueSupported;
                        info.UpdateValueSupportKind = resolver.UpdateValueSupportKind;
                    }
                );
            }

            if (isUsedProviderFallback)
            {
                //取得需要補查的「不重複 SchemaName + TableName」的組合
                var targetTables = dtSchemaTable.AsEnumerable()
                                                .Where(dr => string.Equals(dr.GetSafeString("UsedProviderFallback"), "Y", StringComparison.OrdinalIgnoreCase))
                                                .Select
                                                 (
                                                     dr => new
                                                     {
                                                         SchemaName = dr.Field<string>("BaseSchemaName"),
                                                         TableName = dr.Field<string>("BaseTableName")
                                                     }
                                                 )
                                                .Where(t => !string.IsNullOrWhiteSpace(t.SchemaName) && !string.IsNullOrWhiteSpace(t.TableName))
                                                .Distinct()
                                                .ToList();

                if (targetTables.Any())
                {
                    //用一段 SQL 查出指定的 SchemaName + TableName 的所有正確的欄位型別
                    var dtCatalogInfo = MySqlSqlExecutor.GetMySqlCatalogTypeInfo(targetTables);

                    //Create Catalog Map：Key = Schema.Table.Column
                    var catalogMap = dtCatalogInfo.AsEnumerable()
                                                  .Where
                                                   (
                                                       r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName")) &&
                                                            !string.IsNullOrWhiteSpace(r.GetSafeString("TableName")) &&
                                                            !string.IsNullOrWhiteSpace(r.GetSafeString("ColumnName")) &&
                                                            !string.IsNullOrWhiteSpace(r.GetSafeString("TrueTypeName"))
                                                   )
                                                  .ToDictionary
                                                   (
                                                       r => $"{r["SchemaName"]}.{r["TableName"]}.{r["ColumnName"]}".ToLowerInvariant(),
                                                       r => r.GetSafeString("TrueTypeName"), StringComparer.OrdinalIgnoreCase
                                                   );

                    foreach (DataRow dr in dtSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var usedProviderFallback = dr.GetSafeString("UsedProviderFallback");

                        if (!string.Equals(usedProviderFallback, "Y", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var schema = dr.GetSafeString("BaseSchemaName");
                        var table = dr.GetSafeString("BaseTableName");
                        var column = dr.GetSafeString("BaseColumnName");

                        if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(column))
                        {
                            continue;
                        }

                        var cacheKey = $"{schema}.{table}.{column}".ToLowerInvariant();

                        if (catalogMap.TryGetValue(cacheKey, out string trueType))
                        {
                            dr["DataType2"] = trueType;
                            dr["UsedProviderFallback"] = string.Empty;

                            var columnName = SchemaRowReader.GetColumnName(dr);

                            if (collector.TryGet(columnName, schema, table, out var info))
                            {
                                //update collector
                                info.BaseDataType = trueType;
                                info.FullDataType = trueType;
                                info.UsedProviderFallback = false;
                            }
                        }
                    }
                }
            }

            return collector;
        }
    }
}