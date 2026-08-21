using JasonQuery.Core.Data.DataRows;
using JasonLibrary.Providers.PostgreSql.Mapping;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Readers;
using JasonQuery.Core.Schema;
using JasonQuery.Database.Providers.Executor;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Data.Schema
{
    internal static class PostgreSqlSchemaColumnInfoBuilder
    {
        public static ColumnInfoCollector Build(DataTable dtSchemaTable)
        {
            var isUsedProviderFallback = false;
            var collector = new ColumnInfoCollector();

            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "UsedProviderFallback"); //20260306 新增 UsedProviderFallback 欄位

            foreach (DataRow dr in dtSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var columnSize = SchemaRowReader.GetColumnSize(dr);
                var numericScale = SchemaRowReader.GetNumericScale(dr);
                var numericPrecision = SchemaRowReader.GetNumericPrecision(dr);
                var providerType = SchemaRowReader.GetProviderType(dr);
                var resolver = PostgreSqlColumnTypeResolver.Resolve(providerType, columnSize, numericPrecision, numericScale);

                if (resolver.UsedProviderFallback)
                {
                    isUsedProviderFallback = true;
                    dr["UsedProviderFallback"] = "Y";
                }

                SchemaColumnInfoBuilderHelper.AddOrUpdateColumnInfo
                (
                    collector,
                    dr,
                    SchemaRowReader.GetDataType(dr),
                    resolver.FullDataType,
                    resolver.BaseDataType,
                    columnSize,
                    numericPrecision,
                    numericScale,
                    string.Empty,
                    resolver.SpecialDataTypeKind,
                    resolver.CategoryDataTypeKind,
                    info =>
                    {
                        info.UsedProviderFallback = resolver.UsedProviderFallback;
                        info.IsArray = resolver.IsArray;
                        info.IsUpdateValueSupported = resolver.IsUpdateValueSupported;
                        info.UpdateValueSupportKind = resolver.UpdateValueSupportKind;
                    }
                );
            }

            if (isUsedProviderFallback)
            {
                //取得需要補查的「不重複 SchemaName + TableName」的組合
                var targetTables = dtSchemaTable.AsEnumerable()
                                                .Where(dr => dr.GetSafeString("UsedProviderFallback") == "Y")
                                                .Select
                                                 (
                                                     dr => new
                                                     {
                                                         SchemaName = dr.GetSafeString("BaseSchemaName"),
                                                         TableName = dr.GetSafeString("BaseTableName")
                                                     }
                                                 )
                                                .Where(t => !string.IsNullOrWhiteSpace(t.SchemaName) && !string.IsNullOrWhiteSpace(t.TableName))
                                                .Distinct()
                                                .ToList();

                if (targetTables.Any())
                {
                    //用一段 SQL 查出指定的 SchemaName + TableName 的所有正確的欄位型別
                    DataTable dtCatalogInfo = PostgreSqlSqlExecutor.GetPostgreSqlCatalogTypeInfo(targetTables);

                    //Create Catalog Map：Key = Schema.Table.Column
                    var catalogMap = dtCatalogInfo.AsEnumerable()
                                                  .ToDictionary
                                                   (
                                                       r => $"{r["SchemaName"]}.{r["TableName"]}.{r["ColumnName"]}".ToLowerInvariant(),
                                                       r => $"{r["TrueTypeName"]}"
                                                   );

                    foreach (DataRow dr in dtSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var usedProviderFallback = dr.GetSafeString("UsedProviderFallback");

                        if (usedProviderFallback == "Y")
                        {
                            var schema = dr.GetSafeString("BaseSchemaName");
                            var table = dr.GetSafeString("BaseTableName");
                            var column = dr.GetSafeString("BaseColumnName");
                            var cacheKey = $"{schema}.{table}.{column}".ToLowerInvariant();

                            if (catalogMap.TryGetValue(cacheKey, out string sTrueType))
                            {
                                dr["UsedProviderFallback"] = string.Empty;

                                var columnName = SchemaRowReader.GetColumnName(dr);

                                if (collector.TryGet(columnName, schema, table, out var info))
                                {
                                    //update collector
                                    info.BaseDataType = sTrueType;
                                    info.FullDataType = sTrueType;
                                    info.UsedProviderFallback = false;
                                }
                            }
                        }
                    }
                }
            }

            return collector;
        }
    }
}