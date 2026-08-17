using JasonQuery.Core.Data.Readers;
using JasonQuery.Core.Schema;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Data.Schema
{
    internal static class SqlServerSchemaColumnInfoBuilder
    {
        public static ColumnInfoCollector Build(DataTable dtSchemaTable)
        {
            var collector = new ColumnInfoCollector();

            foreach (DataRow dr in dtSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var baseDataType = SchemaRowReader.GetDataTypeName(dr);
                var dataType = SchemaRowReader.GetDataType(dr);
                var columnSize = SchemaRowReader.GetColumnSize(dr);
                var numericScale = SchemaRowReader.GetNumericScale(dr);
                var numericPrecision = SchemaRowReader.GetNumericPrecision(dr);
                var providerSpecificDataType = SchemaRowReader.GetProviderSpecificDataType(dr);

                var resolver = SqlServerColumnTypeResolver.Resolve(baseDataType, dataType, columnSize,
                                                                   numericPrecision, numericScale, providerSpecificDataType);

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
                        info.IsUpdateValueSupported = resolver.IsUpdateValueSupported;
                        info.UpdateValueSupportKind = resolver.UpdateValueSupportKind;
                    }
                );
            }

            return collector;
        }
    }
}