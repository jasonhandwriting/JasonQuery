using JasonQuery.Core.Database.Connection;
using System;
using System.Text.RegularExpressions;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestTransactionSqlDialect
    {
        private const string TransactionTableName = "JQ_IT_TX_LOCK";
        private const string DdlTableName = "JQ_IT_DDL_TX";

        private IntegrationTestTransactionSqlDialect(IntegrationTestDatabaseSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            TransactionTable = BuildQualifiedName(TransactionTableName);
            DdlTable = BuildQualifiedName(DdlTableName);
        }

        public IntegrationTestDatabaseSettings Settings { get; private set; }
        public DataSourceType SourceType => Settings.SourceType;
        public string TransactionTable { get; private set; }
        public string DdlTable { get; private set; }
        public bool DdlUsesImplicitCommit => SourceType == DataSourceType.Oracle || SourceType == DataSourceType.MySql;

        public static IntegrationTestTransactionSqlDialect Create(IntegrationTestDatabaseSettings settings)
        {
            return new IntegrationTestTransactionSqlDialect(settings);
        }

        public string BuildCreateTransactionTableSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"CREATE TABLE {TransactionTable} (\"ID\" NUMBER(10) NOT NULL, \"NAME\" VARCHAR2(50), CONSTRAINT \"JQ_IT_TX_LOCK_PK\" PRIMARY KEY (\"ID\"))";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return $"CREATE TABLE {TransactionTable} (\"ID\" integer NOT NULL PRIMARY KEY, \"NAME\" varchar(50) NULL)";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"CREATE TABLE {TransactionTable} ([ID] int NOT NULL CONSTRAINT [PK_JQ_IT_TX_LOCK] PRIMARY KEY, [NAME] nvarchar(50) NULL)";
                    }
                case DataSourceType.MySql:
                    {
                        return $"CREATE TABLE {TransactionTable} (`ID` int NOT NULL, `NAME` varchar(50) NULL, PRIMARY KEY (`ID`)) ENGINE=InnoDB";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildCreateDdlTableSql()
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"CREATE TABLE {DdlTable} (\"ID\" NUMBER(10) NOT NULL)";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return $"CREATE TABLE {DdlTable} (\"ID\" integer NOT NULL)";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"CREATE TABLE {DdlTable} ([ID] int NOT NULL)";
                    }
                case DataSourceType.MySql:
                    {
                        return $"CREATE TABLE {DdlTable} (`ID` int NOT NULL) ENGINE=InnoDB";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildDropTransactionTableSql()
        {
            return BuildDropTableSql(TransactionTable);
        }

        public string BuildDropDdlTableSql()
        {
            return BuildDropTableSql(DdlTable);
        }

        public string BuildInsertSql(int id, string name)
        {
            var escaped = (name ?? string.Empty).Replace("'", "''");

            switch (SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"INSERT INTO {TransactionTable} (\"ID\", \"NAME\") VALUES ({id}, '{escaped}')";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"INSERT INTO {TransactionTable} ([ID], [NAME]) VALUES ({id}, N'{escaped}')";
                    }
                case DataSourceType.MySql:
                    {
                        return $"INSERT INTO {TransactionTable} (`ID`, `NAME`) VALUES ({id}, '{escaped}')";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildUpdateSql(int id, string name)
        {
            var escaped = (name ?? string.Empty).Replace("'", "''");

            switch (SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"UPDATE {TransactionTable} SET \"NAME\" = '{escaped}' WHERE \"ID\" = {id}";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"UPDATE {TransactionTable} SET [NAME] = N'{escaped}' WHERE [ID] = {id}";
                    }
                case DataSourceType.MySql:
                    {
                        return $"UPDATE {TransactionTable} SET `NAME` = '{escaped}' WHERE `ID` = {id}";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildDeleteSql(int id)
        {
            return SourceType == DataSourceType.SqlServer
                   ? $"DELETE FROM {TransactionTable} WHERE [ID] = {id}"
                   : SourceType == DataSourceType.MySql
                     ? $"DELETE FROM {TransactionTable} WHERE `ID` = {id}"
                     : $"DELETE FROM {TransactionTable} WHERE \"ID\" = {id}";
        }

        public string BuildSelectByIdSql(int id)
        {
            return SourceType == DataSourceType.SqlServer
                   ? $"SELECT [ID], [NAME] FROM {TransactionTable} WHERE [ID] = {id}"
                   : SourceType == DataSourceType.MySql
                     ? $"SELECT `ID`, `NAME` FROM {TransactionTable} WHERE `ID` = {id}"
                     : $"SELECT \"ID\", \"NAME\" FROM {TransactionTable} WHERE \"ID\" = {id}";
        }

        public string BuildObserverSelectByIdSql(int id)
        {
            return SourceType == DataSourceType.SqlServer
                   ? $"SELECT [ID], [NAME] FROM {TransactionTable} WITH (READPAST) WHERE [ID] = {id}"
                   : BuildSelectByIdSql(id);
        }

        public string BuildLockingQuerySql(int id, bool failFast)
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return $"SELECT \"ID\", \"NAME\" FROM {TransactionTable} WHERE \"ID\" = {id} FOR UPDATE{(failFast ? " NOWAIT" : string.Empty)}";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return $"SELECT \"ID\", \"NAME\" FROM {TransactionTable} WHERE \"ID\" = {id} FOR UPDATE{(failFast ? " NOWAIT" : string.Empty)}";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"SELECT [ID], [NAME] FROM {TransactionTable} WITH (UPDLOCK, ROWLOCK) WHERE [ID] = {id}";
                    }
                case DataSourceType.MySql:
                    {
                        return $"SELECT `ID`, `NAME` FROM {TransactionTable} WHERE `ID` = {id} FOR UPDATE{(failFast ? " NOWAIT" : string.Empty)}";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        public string BuildSetLockTimeoutSql()
        {
            switch (SourceType)
            {
                case DataSourceType.SqlServer:
                    {
                        return "SET LOCK_TIMEOUT 1000";
                    }
                case DataSourceType.MySql:
                    {
                        return "SET SESSION innodb_lock_wait_timeout = 1";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        public string BuildInvalidSql()
        {
            return SourceType == DataSourceType.SqlServer
                   ? $"UPDATE {TransactionTable} SET [COLUMN_DOES_NOT_EXIST] = 1 WHERE [ID] = 1"
                   : SourceType == DataSourceType.MySql
                     ? $"UPDATE {TransactionTable} SET `COLUMN_DOES_NOT_EXIST` = 1 WHERE `ID` = 1"
                     : $"UPDATE {TransactionTable} SET \"COLUMN_DOES_NOT_EXIST\" = 1 WHERE \"ID\" = 1";
        }

        public bool IsExpectedDropMissingError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return false;
            }

            return SourceType == DataSourceType.Oracle
                   && (errorMessage.IndexOf("ORA-00942", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("table or view does not exist", StringComparison.OrdinalIgnoreCase) >= 0
                       || errorMessage.IndexOf("表格或視觀表不存在", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public bool IsExpectedLockConflict(string errorPayload)
        {
            if (string.IsNullOrWhiteSpace(errorPayload))
            {
                return false;
            }

            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return ContainsAny(errorPayload, "ORA-00054", "resource busy", "資源正忙碌");
                    }
                case DataSourceType.PostgreSql:
                    {
                        return ContainsAny(errorPayload, "55P03", "could not obtain lock", "lock not available");
                    }
                case DataSourceType.SqlServer:
                    {
                        return ContainsAny(errorPayload, "1222", "Lock request time out period exceeded", "lock request time out");
                    }
                case DataSourceType.MySql:
                    {
                        return ContainsAny(errorPayload, "3572", "could not be acquired immediately", "Lock wait timeout exceeded");
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private string BuildQualifiedName(string tableName)
        {
            switch (SourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"{QuoteDouble(Settings.SchemaName)}.{QuoteDouble(tableName)}";
                    }
                case DataSourceType.SqlServer:
                    {
                        return $"{QuoteBracket(Settings.SchemaName)}.{QuoteBracket(tableName)}";
                    }
                case DataSourceType.MySql:
                    {
                        return string.IsNullOrWhiteSpace(Settings.DatabaseName) ? QuoteBacktick(tableName) : $"{QuoteBacktick(Settings.DatabaseName)}.{QuoteBacktick(tableName)}";
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private string BuildDropTableSql(string qualifiedName)
        {
            return SourceType == DataSourceType.Oracle ? $"DROP TABLE {qualifiedName} PURGE" : $"DROP TABLE IF EXISTS {qualifiedName}";
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string QuoteDouble(string value)
        {
            ValidateIdentifier(value);
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static string QuoteBracket(string value)
        {
            ValidateIdentifier(value);
            return $"[{value.Replace("]", "]]" )}]";
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
                throw new InvalidOperationException($"Integration-test identifier '{value}' is invalid.");
            }
        }
    }
}
