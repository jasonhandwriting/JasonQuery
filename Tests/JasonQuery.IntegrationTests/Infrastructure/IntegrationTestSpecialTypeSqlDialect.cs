using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestSpecialTypeSqlDialect
    {
        private const string OracleMainTableName = "JQ_IT_ORACLE_TYPES";
        private const string OracleLongTableName = "JQ_IT_ORACLE_LONG";
        private const string OracleLongRawTableName = "JQ_IT_ORACLE_LONG_RAW";
        private const string PostgreSqlTableName = "JQ_IT_POSTGRES_TYPES";
        private const string SqlServerTableName = "JQ_IT_SQLSERVER_TYPES";
        private const string MySqlTableName = "JQ_IT_MYSQL_TYPES";

        private IntegrationTestSpecialTypeSqlDialect(IntegrationTestDatabaseSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IntegrationTestDatabaseSettings Settings { get; private set; }
        public DataSourceType SourceType => Settings.SourceType;

        public static IntegrationTestSpecialTypeSqlDialect Create(IntegrationTestDatabaseSettings settings)
        {
            return new IntegrationTestSpecialTypeSqlDialect(settings);
        }

        public IReadOnlyList<string> BuildDropTableSqls()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return new[]
                        {
                            $"DROP TABLE {GetOracleQualifiedTableName(OracleLongRawTableName)} PURGE",
                            $"DROP TABLE {GetOracleQualifiedTableName(OracleLongTableName)} PURGE",
                            $"DROP TABLE {GetOracleQualifiedTableName(OracleMainTableName)} PURGE"
                        };
                    }
                case DataSourceType.PostgreSql:
                    {
                        return new[] { $"DROP TABLE IF EXISTS {GetPostgreSqlQualifiedTableName(PostgreSqlTableName)}" };
                    }
                case DataSourceType.SqlServer:
                    {
                        return new[] { $"DROP TABLE IF EXISTS {GetSqlServerQualifiedTableName(SqlServerTableName)}" };
                    }
                case DataSourceType.MySql:
                    {
                        return new[] { $"DROP TABLE IF EXISTS {GetMySqlQualifiedTableName(MySqlTableName)}" };
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public IReadOnlyList<string> BuildCreateTableSqls()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return new[]
                        {
                            $"CREATE TABLE {GetOracleQualifiedTableName(OracleMainTableName)} (\"ID\" NUMBER(10) NOT NULL, \"C_CHAR\" CHAR(5), \"C_NCHAR\" NCHAR(5), \"C_CLOB\" CLOB, \"C_NCLOB\" NCLOB, \"C_XML\" XMLTYPE, \"C_RAW\" RAW(8), \"C_BLOB\" BLOB, \"C_NUMBER\" NUMBER(12,3), \"C_DATE\" DATE, \"C_TIMESTAMP\" TIMESTAMP(6), CONSTRAINT \"JQ_IT_ORACLE_TYPES_PK\" PRIMARY KEY (\"ID\"))",
                            $"CREATE TABLE {GetOracleQualifiedTableName(OracleLongTableName)} (\"ID\" NUMBER(10) NOT NULL, \"C_LONG\" LONG, CONSTRAINT \"JQ_IT_ORACLE_LONG_PK\" PRIMARY KEY (\"ID\"))",
                            $"CREATE TABLE {GetOracleQualifiedTableName(OracleLongRawTableName)} (\"ID\" NUMBER(10) NOT NULL, \"C_LONG_RAW\" LONG RAW, CONSTRAINT \"JQ_IT_ORACLE_LRAW_PK\" PRIMARY KEY (\"ID\"))"
                        };
                    }
                case DataSourceType.PostgreSql:
                    {
                        return new[]
                        {
                            $"CREATE TABLE {GetPostgreSqlQualifiedTableName(PostgreSqlTableName)} (\"ID\" integer NOT NULL PRIMARY KEY, \"C_BYTEA\" bytea, \"C_BYTEA_ARRAY\" bytea[], \"C_VARCHAR_ARRAY\" varchar(10)[], \"C_TEXT_ARRAY\" text[], \"C_JSON\" json, \"C_JSONB\" jsonb, \"C_NUMERIC\" numeric(12,3), \"C_TIMESTAMP\" timestamp(6) without time zone)"
                        };
                    }
                case DataSourceType.SqlServer:
                    {
                        return new[]
                        {
                            $"CREATE TABLE {GetSqlServerQualifiedTableName(SqlServerTableName)} ([ID] int NOT NULL CONSTRAINT [PK_JQ_IT_SQLSERVER_TYPES] PRIMARY KEY, [C_BIT] bit NULL, [C_BINARY] binary(4) NULL, [C_VARBINARY] varbinary(max) NULL, [C_ROWVERSION] rowversion NOT NULL, [C_TEXT] text NULL, [C_NTEXT] ntext NULL, [C_XML] xml NULL, [C_NVARCHAR] nvarchar(50) NULL, [C_DECIMAL] decimal(12,3) NULL)"
                        };
                    }
                case DataSourceType.MySql:
                    {
                        return new[]
                        {
                            $"CREATE TABLE {GetMySqlQualifiedTableName(MySqlTableName)} (`ID` int NOT NULL, `C_BLOB` blob NULL, `C_TEXT` text NULL, `C_JSON` json NULL, `C_GEOMETRY` geometry NULL, `C_ENUM` enum('A','B') NULL, `C_SET` set('X','Y','Z') NULL, `C_UINT` int unsigned NULL, `C_DATE` date NULL, `C_TIME` time(6) NULL, `C_DATETIME` datetime(6) NULL, `C_TIMESTAMP` timestamp(6) NULL, PRIMARY KEY (`ID`)) ENGINE=InnoDB"
                        };
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public IReadOnlyList<string> BuildInsertSqls()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return new[]
                        {
                            $"INSERT INTO {GetOracleQualifiedTableName(OracleMainTableName)} (\"ID\", \"C_CHAR\", \"C_NCHAR\", \"C_CLOB\", \"C_NCLOB\", \"C_XML\", \"C_RAW\", \"C_BLOB\", \"C_NUMBER\", \"C_DATE\", \"C_TIMESTAMP\") VALUES (1, 'A', N'中', TO_CLOB('CLOB-CONTENT-1234567890'), TO_NCLOB(N'NCLOB-內容'), XMLTYPE('<root><value>1</value></root>'), HEXTORAW('01020304'), TO_BLOB(HEXTORAW('0A0B0C0D')), 12345.678, DATE '2026-07-25', TIMESTAMP '2026-07-25 12:34:56.123456')",
                            $"INSERT INTO {GetOracleQualifiedTableName(OracleLongTableName)} (\"ID\", \"C_LONG\") VALUES (1, 'LONG-CONTENT-1234567890')",
                            $"INSERT INTO {GetOracleQualifiedTableName(OracleLongRawTableName)} (\"ID\", \"C_LONG_RAW\") VALUES (1, HEXTORAW('11223344'))"
                        };
                    }
                case DataSourceType.PostgreSql:
                    {
                        return new[]
                        {
                            $"INSERT INTO {GetPostgreSqlQualifiedTableName(PostgreSqlTableName)} (\"ID\", \"C_BYTEA\", \"C_BYTEA_ARRAY\", \"C_VARCHAR_ARRAY\", \"C_TEXT_ARRAY\", \"C_JSON\", \"C_JSONB\", \"C_NUMERIC\", \"C_TIMESTAMP\") VALUES (1, decode('01020304', 'hex'), ARRAY[decode('0A0B', 'hex'), decode('0C0D', 'hex')], ARRAY['alpha','beta']::varchar(10)[], ARRAY['gamma','delta']::text[], '{{\"value\":1}}'::json, '{{\"value\":2}}'::jsonb, 12345.678, TIMESTAMP '2026-07-25 12:34:56.123456')"
                        };
                    }
                case DataSourceType.SqlServer:
                    {
                        return new[]
                        {
                            $"INSERT INTO {GetSqlServerQualifiedTableName(SqlServerTableName)} ([ID], [C_BIT], [C_BINARY], [C_VARBINARY], [C_TEXT], [C_NTEXT], [C_XML], [C_NVARCHAR], [C_DECIMAL]) VALUES (1, 1, 0x01020304, 0x0A0B0C0D, 'TEXT-CONTENT-1234567890', N'NTEXT-內容', N'<root><value>1</value></root>', N'台北-101', 12345.678)"
                        };
                    }
                case DataSourceType.MySql:
                    {
                        return new[]
                        {
                            $"INSERT INTO {GetMySqlQualifiedTableName(MySqlTableName)} (`ID`, `C_BLOB`, `C_TEXT`, `C_JSON`, `C_GEOMETRY`, `C_ENUM`, `C_SET`, `C_UINT`, `C_DATE`, `C_TIME`, `C_DATETIME`, `C_TIMESTAMP`) VALUES (1, X'01020304', 'TEXT-CONTENT-1234567890', '{{\"value\":1}}', ST_GeomFromText('POINT(1 2)'), 'B', 'X,Z', 4000000000, '2026-07-25', '12:34:56.123456', '2026-07-25 12:34:56.123456', '2026-07-25 12:34:56.123456')"
                        };
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildMainSelectSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"SELECT \"ID\", \"C_CHAR\", \"C_NCHAR\", \"C_CLOB\", \"C_NCLOB\", \"C_XML\", \"C_RAW\", \"C_BLOB\", \"C_NUMBER\", \"C_DATE\", \"C_TIMESTAMP\" FROM {GetOracleQualifiedTableName(OracleMainTableName)} ORDER BY \"ID\"";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return $"SELECT \"ID\", \"C_BYTEA\", \"C_BYTEA_ARRAY\", \"C_VARCHAR_ARRAY\", \"C_TEXT_ARRAY\", \"C_JSON\", \"C_JSONB\", \"C_NUMERIC\", \"C_TIMESTAMP\" FROM {GetPostgreSqlQualifiedTableName(PostgreSqlTableName)} ORDER BY \"ID\"";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"SELECT [ID], [C_BIT], [C_BINARY], [C_VARBINARY], [C_ROWVERSION], [C_TEXT], [C_NTEXT], [C_XML], [C_NVARCHAR], [C_DECIMAL] FROM {GetSqlServerQualifiedTableName(SqlServerTableName)} ORDER BY [ID]";
                    }
                case DataSourceType.MySql:
                    {
                        return $"SELECT `ID`, `C_BLOB`, `C_TEXT`, `C_JSON`, `C_GEOMETRY`, `C_ENUM`, `C_SET`, `C_UINT`, `C_DATE`, `C_TIME`, `C_DATETIME`, `C_TIMESTAMP` FROM {GetMySqlQualifiedTableName(MySqlTableName)} ORDER BY `ID`";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildOracleLongSelectSql()
        {
            EnsureOracle();

            return $"SELECT \"ID\", \"C_LONG\" FROM {GetOracleQualifiedTableName(OracleLongTableName)} ORDER BY \"ID\"";
        }

        public string BuildOracleLongRawSelectSql()
        {
            EnsureOracle();

            return $"SELECT \"ID\", \"C_LONG_RAW\" FROM {GetOracleQualifiedTableName(OracleLongRawTableName)} ORDER BY \"ID\"";
        }

        public bool IsExpectedDropMissingError(string errorMessage)
        {
            return SourceType == DataSourceType.Oracle
                   && !string.IsNullOrEmpty(errorMessage)
                   && (errorMessage.IndexOf("ORA-00942", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("table or view does not exist", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("表格或視觀表不存在", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private string GetOracleQualifiedTableName(string tableName)
        {
            return $"{QuoteDouble(Settings.SchemaName)}.{QuoteDouble(tableName)}";
        }

        private string GetPostgreSqlQualifiedTableName(string tableName)
        {
            return $"{QuoteDouble(Settings.SchemaName)}.{QuoteDouble(tableName)}";
        }

        private string GetSqlServerQualifiedTableName(string tableName)
        {
            return $"{QuoteBracket(Settings.SchemaName)}.{QuoteBracket(tableName)}";
        }

        private string GetMySqlQualifiedTableName(string tableName)
        {
            return string.IsNullOrWhiteSpace(Settings.DatabaseName)
                   ? QuoteBacktick(tableName)
                   : $"{QuoteBacktick(Settings.DatabaseName)}.{QuoteBacktick(tableName)}";
        }

        private void EnsureOracle()
        {
            if (SourceType != DataSourceType.Oracle)
            {
                throw new InvalidOperationException("This SQL is available only for Oracle integration tests.");
            }
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
                throw new InvalidOperationException($"Integration-test identifier '{value}' is invalid. Use a simple database or schema name.");
            }
        }
    }
}
