using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal static class ColumnTypeCatalog
    {
        private static readonly IReadOnlyList<ColumnTypeDefinition> OracleTypes = CreateOracleTypes();

        private static readonly IReadOnlyList<ColumnTypeDefinition> PostgreSqlTypes = CreatePostgreSqlTypes();

        private static readonly IReadOnlyList<ColumnTypeDefinition> SqlServerTypes = CreateSqlServerTypes();

        private static readonly IReadOnlyList<ColumnTypeDefinition> MySqlTypes = CreateMySqlTypes();

        public static IReadOnlyList<ColumnTypeDefinition> GetDefinitions(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return OracleTypes;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return PostgreSqlTypes;
                    }
                case DataSourceType.SqlServer:
                    {
                        return SqlServerTypes;
                    }
                case DataSourceType.MySql:
                    {
                        return MySqlTypes;
                    }
                default:
                    {
                        return Array.Empty<ColumnTypeDefinition>();
                    }
            }
        }

        public static ColumnTypeDefinition Find(DataSourceType dataSourceType, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return GetDefinitions(dataSourceType).FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        public static ColumnTypeDefinition GetDefault(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return Find(dataSourceType, "VARCHAR2");
                    }
                case DataSourceType.PostgreSql:
                    {
                        return Find(dataSourceType, "VARCHAR");
                    }
                case DataSourceType.SqlServer:
                    {
                        return Find(dataSourceType, "NVARCHAR");
                    }
                case DataSourceType.MySql:
                    {
                        return Find(dataSourceType, "VARCHAR");
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public static ColumnTypeDefinition CreateCustom(DataSourceType dataSourceType, string typeText)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = dataSourceType,
                Key = "__CUSTOM__",
                SqlTypeName = typeText ?? string.Empty,
                DisplayName = typeText ?? string.Empty,
                ArgumentKind = ColumnTypeArgumentKind.Custom,
                SupportsDefault = true,
                DefaultLiteralKind = ColumnDefaultLiteralKind.Other,
                IsCustom = true
            };
        }

        private static IReadOnlyList<ColumnTypeDefinition> CreateOracleTypes()
        {
            return new[]
            {
                Length(DataSourceType.Oracle, "VARCHAR2", "VARCHAR2", "50", 1, 4000, true, false, true, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.Oracle, "NVARCHAR2", "NVARCHAR2", "50", 1, 2000, true, false, false, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.Oracle, "CHAR", "CHAR", "1", 1, 2000, true, false, true, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.Oracle, "NCHAR", "NCHAR", "1", 1, 1000, true, false, false, ColumnDefaultLiteralKind.String),
                PrecisionScale(DataSourceType.Oracle, "NUMBER", "NUMBER", "18", "2", 1, 38, -84, 127, false, false, ColumnDefaultLiteralKind.Decimal),
                Precision(DataSourceType.Oracle, "FLOAT", "FLOAT", "126", 1, 126, false, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.Oracle, "BINARY_FLOAT", "BINARY_FLOAT", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.Oracle, "BINARY_DOUBLE", "BINARY_DOUBLE", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.Oracle, "DATE", "DATE", true, ColumnDefaultLiteralKind.Date),
                Fractional(DataSourceType.Oracle, "TIMESTAMP", "TIMESTAMP", "6", 0, 9, true, ColumnDefaultLiteralKind.DateTime),
                FractionalWithSuffix(DataSourceType.Oracle, "TIMESTAMP_WITH_TIME_ZONE", "TIMESTAMP", " WITH TIME ZONE", "6", 0, 9, true, ColumnDefaultLiteralKind.DateTime),
                FractionalWithSuffix(DataSourceType.Oracle, "TIMESTAMP_WITH_LOCAL_TIME_ZONE", "TIMESTAMP", " WITH LOCAL TIME ZONE", "6", 0, 9, true, ColumnDefaultLiteralKind.DateTime),
                LengthNoDefault(DataSourceType.Oracle, "RAW", "RAW", "16", 1, 2000, true, false, false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.Oracle, "CLOB", "CLOB", false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.Oracle, "NCLOB", "NCLOB", false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.Oracle, "BLOB", "BLOB", false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.Oracle, "XMLTYPE", "XMLTYPE", false, ColumnDefaultLiteralKind.String)
            };
        }

        private static IReadOnlyList<ColumnTypeDefinition> CreatePostgreSqlTypes()
        {
            return new[]
            {
                Length(DataSourceType.PostgreSql, "VARCHAR", "varchar", "50", 1, 10485760, true, false, false, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.PostgreSql, "CHAR", "char", "1", 1, 10485760, true, false, false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.PostgreSql, "TEXT", "text", true, ColumnDefaultLiteralKind.String),
                None(DataSourceType.PostgreSql, "SMALLINT", "smallint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.PostgreSql, "INTEGER", "integer", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.PostgreSql, "BIGINT", "bigint", true, ColumnDefaultLiteralKind.Integer),
                PrecisionScale(DataSourceType.PostgreSql, "NUMERIC", "numeric", "18", "2", 1, 1000, 0, 1000, false, false, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.PostgreSql, "REAL", "real", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.PostgreSql, "DOUBLE_PRECISION", "double precision", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.PostgreSql, "BOOLEAN", "boolean", true, ColumnDefaultLiteralKind.Boolean),
                None(DataSourceType.PostgreSql, "DATE", "date", true, ColumnDefaultLiteralKind.Date),
                Fractional(DataSourceType.PostgreSql, "TIME", "time", "6", 0, 6, true, ColumnDefaultLiteralKind.Time),
                FractionalWithSuffix(DataSourceType.PostgreSql, "TIME_WITH_TIME_ZONE", "time", " with time zone", "6", 0, 6, true, ColumnDefaultLiteralKind.Time),
                Fractional(DataSourceType.PostgreSql, "TIMESTAMP", "timestamp", "6", 0, 6, true, ColumnDefaultLiteralKind.DateTime),
                FractionalWithSuffix(DataSourceType.PostgreSql, "TIMESTAMP_WITH_TIME_ZONE", "timestamp", " with time zone", "6", 0, 6, true, ColumnDefaultLiteralKind.DateTime),
                None(DataSourceType.PostgreSql, "UUID", "uuid", true, ColumnDefaultLiteralKind.String),
                None(DataSourceType.PostgreSql, "BYTEA", "bytea", false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.PostgreSql, "JSON", "json", true, ColumnDefaultLiteralKind.String),
                None(DataSourceType.PostgreSql, "JSONB", "jsonb", true, ColumnDefaultLiteralKind.String),
                None(DataSourceType.PostgreSql, "XML", "xml", true, ColumnDefaultLiteralKind.String)
            };
        }

        private static IReadOnlyList<ColumnTypeDefinition> CreateSqlServerTypes()
        {
            return new[]
            {
                Length(DataSourceType.SqlServer, "VARCHAR", "varchar", "50", 1, 8000, true, true, false, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.SqlServer, "NVARCHAR", "nvarchar", "50", 1, 4000, true, true, false, ColumnDefaultLiteralKind.UnicodeString),
                Length(DataSourceType.SqlServer, "CHAR", "char", "1", 1, 8000, true, false, false, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.SqlServer, "NCHAR", "nchar", "1", 1, 4000, true, false, false, ColumnDefaultLiteralKind.UnicodeString),
                LengthNoDefault(DataSourceType.SqlServer, "VARBINARY", "varbinary", "50", 1, 8000, true, true, false, ColumnDefaultLiteralKind.Other),
                LengthNoDefault(DataSourceType.SqlServer, "BINARY", "binary", "1", 1, 8000, true, false, false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.SqlServer, "TINYINT", "tinyint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.SqlServer, "SMALLINT", "smallint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.SqlServer, "INT", "int", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.SqlServer, "BIGINT", "bigint", true, ColumnDefaultLiteralKind.Integer),
                PrecisionScale(DataSourceType.SqlServer, "DECIMAL", "decimal", "18", "2", 1, 38, 0, 38, true, true, ColumnDefaultLiteralKind.Decimal),
                PrecisionScale(DataSourceType.SqlServer, "NUMERIC", "numeric", "18", "2", 1, 38, 0, 38, true, true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.SqlServer, "BIT", "bit", true, ColumnDefaultLiteralKind.Boolean),
                None(DataSourceType.SqlServer, "REAL", "real", true, ColumnDefaultLiteralKind.Decimal),
                Precision(DataSourceType.SqlServer, "FLOAT", "float", "53", 1, 53, false, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.SqlServer, "MONEY", "money", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.SqlServer, "SMALLMONEY", "smallmoney", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.SqlServer, "DATE", "date", true, ColumnDefaultLiteralKind.Date),
                Fractional(DataSourceType.SqlServer, "TIME", "time", "7", 0, 7, true, ColumnDefaultLiteralKind.Time),
                None(DataSourceType.SqlServer, "DATETIME", "datetime", true, ColumnDefaultLiteralKind.DateTime),
                Fractional(DataSourceType.SqlServer, "DATETIME2", "datetime2", "7", 0, 7, true, ColumnDefaultLiteralKind.DateTime),
                Fractional(DataSourceType.SqlServer, "DATETIMEOFFSET", "datetimeoffset", "7", 0, 7, true, ColumnDefaultLiteralKind.DateTime),
                None(DataSourceType.SqlServer, "UNIQUEIDENTIFIER", "uniqueidentifier", true, ColumnDefaultLiteralKind.String),
                None(DataSourceType.SqlServer, "XML", "xml", false, ColumnDefaultLiteralKind.UnicodeString)
            };
        }

        private static IReadOnlyList<ColumnTypeDefinition> CreateMySqlTypes()
        {
            return new[]
            {
                Length(DataSourceType.MySql, "VARCHAR", "varchar", "50", 1, 65535, true, false, false, ColumnDefaultLiteralKind.String),
                Length(DataSourceType.MySql, "CHAR", "char", "1", 1, 255, true, false, false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.MySql, "TINYINT", "tinyint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "SMALLINT", "smallint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "MEDIUMINT", "mediumint", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "INT", "int", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "BIGINT", "bigint", true, ColumnDefaultLiteralKind.Integer),
                PrecisionScale(DataSourceType.MySql, "DECIMAL", "decimal", "18", "2", 1, 65, 0, 30, true, true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.MySql, "FLOAT", "float", true, ColumnDefaultLiteralKind.Decimal),
                None(DataSourceType.MySql, "DOUBLE", "double", true, ColumnDefaultLiteralKind.Decimal),
                Length(DataSourceType.MySql, "BIT", "bit", "1", 1, 64, true, false, false, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "DATE", "date", true, ColumnDefaultLiteralKind.Date),
                Fractional(DataSourceType.MySql, "TIME", "time", "0", 0, 6, true, ColumnDefaultLiteralKind.Time),
                Fractional(DataSourceType.MySql, "DATETIME", "datetime", "0", 0, 6, true, ColumnDefaultLiteralKind.DateTime),
                Fractional(DataSourceType.MySql, "TIMESTAMP", "timestamp", "0", 0, 6, true, ColumnDefaultLiteralKind.DateTime),
                None(DataSourceType.MySql, "YEAR", "year", true, ColumnDefaultLiteralKind.Integer),
                None(DataSourceType.MySql, "TINYTEXT", "tinytext", false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.MySql, "TEXT", "text", false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.MySql, "MEDIUMTEXT", "mediumtext", false, ColumnDefaultLiteralKind.String),
                None(DataSourceType.MySql, "LONGTEXT", "longtext", false, ColumnDefaultLiteralKind.String),
                LengthNoDefault(DataSourceType.MySql, "BINARY", "binary", "1", 1, 255, true, false, false, ColumnDefaultLiteralKind.Other),
                LengthNoDefault(DataSourceType.MySql, "VARBINARY", "varbinary", "50", 1, 65535, true, false, false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.MySql, "BLOB", "blob", false, ColumnDefaultLiteralKind.Other),
                None(DataSourceType.MySql, "JSON", "json", false, ColumnDefaultLiteralKind.String)
            };
        }

        private static ColumnTypeDefinition None(DataSourceType sourceType, string key, string sqlTypeName, bool supportsDefault, ColumnDefaultLiteralKind literalKind)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = sourceType,
                Key = key,
                SqlTypeName = sqlTypeName,
                DisplayName = sqlTypeName,
                ArgumentKind = ColumnTypeArgumentKind.None,
                SupportsDefault = supportsDefault,
                DefaultLiteralKind = literalKind
            };
        }

        private static ColumnTypeDefinition LengthNoDefault(DataSourceType sourceType, string key, string sqlTypeName, string defaultLength, int minimumLength,
                                                            int maximumLength, bool required, bool allowsMax, bool supportsOracleSemantics, ColumnDefaultLiteralKind literalKind)
        {
            var definition = Length(sourceType, key, sqlTypeName, defaultLength, minimumLength, maximumLength,
                                    required, allowsMax, supportsOracleSemantics, literalKind);

            definition.SupportsDefault = false;

            return definition;
        }

        private static ColumnTypeDefinition Length(DataSourceType sourceType, string key, string sqlTypeName, string defaultLength, int minimumLength, int maximumLength,
                                                   bool required, bool allowsMax, bool supportsOracleSemantics, ColumnDefaultLiteralKind literalKind)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = sourceType,
                Key = key,
                SqlTypeName = sqlTypeName,
                DisplayName = sqlTypeName,
                ArgumentKind = ColumnTypeArgumentKind.Length,
                Parameter1Label = "Size:",
                DefaultParameter1 = defaultLength,
                MinimumParameter1 = minimumLength,
                MaximumParameter1 = maximumLength,
                Parameter1Required = required,
                AllowsMax = allowsMax,
                SupportsOracleLengthSemantics = supportsOracleSemantics,
                SupportsDefault = true,
                DefaultLiteralKind = literalKind
            };
        }

        private static ColumnTypeDefinition PrecisionScale(DataSourceType sourceType, string key, string sqlTypeName, string defaultPrecision, string defaultScale, int minimumPrecision,
                                                           int maximumPrecision, int minimumScale, int maximumScale, bool precisionRequired, bool scaleRequired, ColumnDefaultLiteralKind literalKind)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = sourceType,
                Key = key,
                SqlTypeName = sqlTypeName,
                DisplayName = sqlTypeName,
                ArgumentKind = ColumnTypeArgumentKind.PrecisionScale,
                Parameter1Label = "Precision:",
                Parameter2Label = "Scale:",
                DefaultParameter1 = defaultPrecision,
                DefaultParameter2 = defaultScale,
                MinimumParameter1 = minimumPrecision,
                MaximumParameter1 = maximumPrecision,
                MinimumParameter2 = minimumScale,
                MaximumParameter2 = maximumScale,
                Parameter1Required = precisionRequired,
                Parameter2Required = scaleRequired,
                SupportsDefault = true,
                DefaultLiteralKind = literalKind
            };
        }

        private static ColumnTypeDefinition Precision(DataSourceType sourceType, string key, string sqlTypeName, string defaultPrecision,
                                                      int minimumPrecision, int maximumPrecision, bool required, ColumnDefaultLiteralKind literalKind)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = sourceType,
                Key = key,
                SqlTypeName = sqlTypeName,
                DisplayName = sqlTypeName,
                ArgumentKind = ColumnTypeArgumentKind.Length,
                Parameter1Label = "Precision:",
                DefaultParameter1 = defaultPrecision,
                MinimumParameter1 = minimumPrecision,
                MaximumParameter1 = maximumPrecision,
                Parameter1Required = required,
                SupportsDefault = true,
                DefaultLiteralKind = literalKind
            };
        }

        private static ColumnTypeDefinition FractionalWithSuffix(DataSourceType sourceType, string key, string sqlTypeName, string sqlTypeSuffix, string defaultPrecision,
                                                                 int minimumPrecision, int maximumPrecision, bool supportsDefault, ColumnDefaultLiteralKind literalKind)
        {
            var definition = Fractional(sourceType, key, sqlTypeName, defaultPrecision, minimumPrecision, maximumPrecision, supportsDefault, literalKind);

            definition.SqlTypeSuffix = sqlTypeSuffix;
            definition.DisplayName = sqlTypeName + sqlTypeSuffix;

            return definition;
        }

        private static ColumnTypeDefinition Fractional(DataSourceType sourceType, string key, string sqlTypeName, string defaultPrecision,
                                                       int minimumPrecision, int maximumPrecision, bool supportsDefault, ColumnDefaultLiteralKind literalKind)
        {
            return new ColumnTypeDefinition
            {
                DataSourceType = sourceType,
                Key = key,
                SqlTypeName = sqlTypeName,
                DisplayName = sqlTypeName,
                ArgumentKind = ColumnTypeArgumentKind.FractionalSecondsPrecision,
                Parameter1Label = "Fractional Seconds:",
                DefaultParameter1 = defaultPrecision,
                MinimumParameter1 = minimumPrecision,
                MaximumParameter1 = maximumPrecision,
                Parameter1Required = false,
                SupportsDefault = supportsDefault,
                DefaultLiteralKind = literalKind
            };
        }
    }
}
