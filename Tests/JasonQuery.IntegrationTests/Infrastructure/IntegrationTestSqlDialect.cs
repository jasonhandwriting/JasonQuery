using JasonQuery.Core.Database.Connection;
using System;
using System.Text.RegularExpressions;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestSqlDialect
    {
        private const string TableName = "JQ_IT_KEYINFO";

        private IntegrationTestSqlDialect(DataSourceType sourceType, string qualifiedTableName)
        {
            SourceType = sourceType;
            QualifiedTableName = qualifiedTableName;
        }

        public DataSourceType SourceType { get; private set; }
        public string QualifiedTableName { get; private set; }

        public static IntegrationTestSqlDialect Create(IntegrationTestDatabaseSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            switch (settings.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return new IntegrationTestSqlDialect
                        (
                            settings.SourceType,
                            $"{QuoteDouble(settings.SchemaName)}.{QuoteDouble(TableName)}"
                        );
                    }
                case DataSourceType.PostgreSql:
                    {
                        return new IntegrationTestSqlDialect
                        (
                            settings.SourceType,
                            $"{QuoteDouble(settings.SchemaName)}.{QuoteDouble(TableName)}"
                        );
                    }
                case DataSourceType.SqlServer:
                    {
                        return new IntegrationTestSqlDialect
                        (
                            settings.SourceType,
                            $"{QuoteBracket(settings.SchemaName)}.{QuoteBracket(TableName)}"
                        );
                    }
                case DataSourceType.MySql:
                    {
                        return new IntegrationTestSqlDialect
                        (
                            settings.SourceType,
                            string.IsNullOrWhiteSpace(settings.DatabaseName) ? QuoteBacktick(TableName) : $"{QuoteBacktick(settings.DatabaseName)}.{QuoteBacktick(TableName)}"
                        );
                    }
                default:
                    {
                        throw new NotSupportedException($"Unsupported data source type: {settings.SourceType}");
                    }
            }
        }

        public string BuildDropTableSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"DROP TABLE {QualifiedTableName} PURGE";
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return $"DROP TABLE IF EXISTS {QualifiedTableName}";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildCreateTableSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"CREATE TABLE {QualifiedTableName} (\"ID\" NUMBER(10) NOT NULL, \"NAME\" VARCHAR2(50), CONSTRAINT \"JQ_IT_KEYINFO_PK\" PRIMARY KEY (\"ID\"))";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return $"CREATE TABLE {QualifiedTableName} (\"ID\" integer NOT NULL PRIMARY KEY, \"NAME\" varchar(50) NULL)";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"CREATE TABLE {QualifiedTableName} ([ID] int NOT NULL CONSTRAINT [PK_JQ_IT_KEYINFO] PRIMARY KEY, [NAME] nvarchar(50) NULL)";
                    }
                case DataSourceType.MySql:
                    {
                        return $"CREATE TABLE {QualifiedTableName} (`ID` int NOT NULL, `NAME` varchar(50) NULL, PRIMARY KEY (`ID`)) ENGINE=InnoDB";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildInsertSql(int id)
        {
            var escapedName = $"Name {id}".Replace("'", "''");

            switch (SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"INSERT INTO {QualifiedTableName} (\"ID\", \"NAME\") VALUES ({id}, '{escapedName}')";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"INSERT INTO {QualifiedTableName} ([ID], [NAME]) VALUES ({id}, N'{escapedName}')";
                    }
                case DataSourceType.MySql:
                    {
                        return $"INSERT INTO {QualifiedTableName} (`ID`, `NAME`) VALUES ({id}, '{escapedName}')";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildSelectSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"SELECT \"ID\", \"NAME\" FROM {QualifiedTableName} ORDER BY \"ID\"";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"SELECT [ID], [NAME] FROM {QualifiedTableName} ORDER BY [ID]";
                    }
                case DataSourceType.MySql:
                    {
                        return $"SELECT `ID`, `NAME` FROM {QualifiedTableName} ORDER BY `ID`";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildScalarSql()
        {
            return SourceType == DataSourceType.Oracle ? "SELECT 1 AS VALUE FROM DUAL" : "SELECT 1 AS VALUE";
        }

        public bool IsExpectedDropMissingError(string errorMessage)
        {
            return SourceType == DataSourceType.Oracle
                   && !string.IsNullOrEmpty(errorMessage)
                   && (errorMessage.IndexOf("ORA-00942", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("table or view does not exist", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("表格或視觀表不存在", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string QuoteDouble(string value)
        {
            ValidateIdentifier(value);

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static string QuoteBracket(string value)
        {
            ValidateIdentifier(value);

            return $"[{value.Replace("]", "]]")}]";
        }

        private static string QuoteBacktick(string value)
        {
            ValidateIdentifier(value);

            return $"`{value.Replace("`", "``")}`";
        }

        private static void ValidateIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, @"^[A-Za-z_][A-Za-z0-9_$#]*$"))
            {
                throw new InvalidOperationException
                (
                    $"Integration-test identifier '{value}' is invalid. Use a simple database or schema name."
                );
            }
        }
    }
}
