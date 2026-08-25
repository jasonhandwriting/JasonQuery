using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Data.Readers;
using JasonQuery.Core.Schema;
using System;
using System.Data;

namespace JasonQuery.Core.Data.Schema
{
    internal static class SchemaColumnInfoBuilderHelper
    {
        public static ColumnInfo CreateColumnInfo(DataRow dr, string dataType, string fullDataType, string baseDataType,
                                                  int columnSize, int numericPrecision, int numericScale, string providerSpecificDataType,
                                                  SpecialDataTypeKind specialDataTypeKind, CategoryDataTypeKind categoryDataTypeKind,
                                                  Action<ColumnInfo> configure = null)
        {
            var info = new ColumnInfo
            {
                ColumnName = SchemaRowReader.GetColumnName(dr),
                BaseSchemaName = SchemaRowReader.GetBaseSchemaName(dr),
                BaseTableName = SchemaRowReader.GetBaseTableName(dr),
                DataType = dataType,
                FullDataType = fullDataType,
                BaseDataType = baseDataType,
                IsPrimaryKey = SchemaRowReader.GetIsKey(dr),
                IsNullable = SchemaRowReader.GetAllowDBNull(dr),
                ColumnSize = columnSize,
                NumericPrecision = numericPrecision,
                NumericScale = numericScale,
                ColumnComment = SchemaRowReader.GetComment(dr),
                ProviderType = SchemaRowReader.GetProviderType(dr),
                ProviderSpecificDataType = providerSpecificDataType,
                SpecialDataTypeKind = specialDataTypeKind,
                CategoryDataTypeKind = categoryDataTypeKind
            };

            configure?.Invoke(info);
            return info;
        }

        public static void AddOrUpdateColumnInfo(ColumnInfoCollector collector, DataRow dr, string dataType, string fullDataType,
                                                 string baseDataType, int columnSize, int numericPrecision, int numericScale, string providerSpecificDataType,
                                                 SpecialDataTypeKind specialDataTypeKind, CategoryDataTypeKind categoryDataTypeKind,
                                                 Action<ColumnInfo> configure = null)
        {
            if (collector == null)
            {
                throw new ArgumentNullException(nameof(collector));
            }

            var info = CreateColumnInfo(dr, dataType, fullDataType, baseDataType, columnSize, numericPrecision, numericScale,
                                        providerSpecificDataType, specialDataTypeKind, categoryDataTypeKind, configure);

            collector.AddOrUpdate(info);
        }
    }
}
