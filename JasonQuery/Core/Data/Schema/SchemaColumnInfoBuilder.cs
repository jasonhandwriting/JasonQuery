using JasonQuery.Core.Schema;
using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Data.Schema
{
    internal static class SchemaColumnInfoBuilder
    {
        private static readonly IReadOnlyDictionary<DataSourceType, Func<DataTable, ColumnInfoCollector>> _builders
                                = new Dictionary<DataSourceType, Func<DataTable, ColumnInfoCollector>>
                                {
                                    [DataSourceType.Oracle] = OracleSchemaColumnInfoBuilder.Build,
                                    [DataSourceType.PostgreSql] = PostgreSqlSchemaColumnInfoBuilder.Build,
                                    [DataSourceType.SqlServer] = SqlServerSchemaColumnInfoBuilder.Build,
                                    [DataSourceType.MySql] = MySqlSchemaColumnInfoBuilder.Build
                                };

        public static ColumnInfoCollector Build(DataSourceType sourceType, DataTable dtSchemaTable)
        {
            if (_builders.TryGetValue(sourceType, out var builder))
            {
                return builder(dtSchemaTable);
            }

            throw new NotSupportedException($"Unsupported data source type: {sourceType}");
        }
    }
}