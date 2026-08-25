using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class DatabaseTransactionStatePolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetImpact_WithNullStatement_ReturnsUnchanged()
        {
            AssertImpact(DataSourceType.Oracle, null, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow("")]
        [DataRow("   ")]
        public void GetImpact_WithEmptyStatement_ReturnsUnchanged(string statementType)
        {
            AssertImpact(DataSourceType.Oracle, statementType, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetImpact_WithUnsupportedDataSource_ReturnsUnchanged()
        {
            AssertImpact(DataSourceType.None, "UPDATE", DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow("Oracle", "COMMIT")]
        [DataRow("Oracle", "ROLLBACK")]
        [DataRow("PostgreSql", "COMMIT")]
        [DataRow("PostgreSql", "ROLLBACK")]
        [DataRow("SqlServer", "COMMIT")]
        [DataRow("SqlServer", "ROLLBACK")]
        [DataRow("MySql", "COMMIT")]
        [DataRow("MySql", "ROLLBACK")]
        public void GetImpact_CommitOrRollback_ReturnsClosed(string dataSourceName, string statementType)
        {
            AssertImpact(ParseDataSourceType(dataSourceName), statementType, DatabaseTransactionImpact.Closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Oracle")]
        [DataRow("INSERT")]
        [DataRow("UPDATE")]
        [DataRow("DELETE")]
        [DataRow("MERGE")]
        [DataRow("LOCK")]
        [DataRow("CALL")]
        [DataRow("EXECUTE")]
        [DataRow("WITH INSERT")]
        [DataRow("WITH UPDATE")]
        [DataRow("WITH DELETE")]
        public void GetImpact_OraclePendingStatement_ReturnsPending(string statementType)
        {
            AssertImpact(DataSourceType.Oracle, statementType, DatabaseTransactionImpact.Pending);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Oracle")]
        [DataRow("CREATE")]
        [DataRow("ALTER")]
        [DataRow("DROP")]
        [DataRow("TRUNCATE")]
        [DataRow("COMMENT")]
        [DataRow("GRANT")]
        [DataRow("REVOKE")]
        [DataRow("ANALYZE")]
        [DataRow("AUDIT")]
        [DataRow("NOAUDIT")]
        [DataRow("RENAME")]
        public void GetImpact_OracleImplicitCommitStatement_ReturnsClosed(string statementType)
        {
            AssertImpact(DataSourceType.Oracle, statementType, DatabaseTransactionImpact.Closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Oracle")]
        [DataRow("SELECT")]
        [DataRow("WITH SELECT")]
        [DataRow("BEGIN")]
        [DataRow("SAVEPOINT")]
        [DataRow("UPDATEABLE")]
        public void GetImpact_OracleReadOnlyOrUnknownStatement_ReturnsUnchanged(string statementType)
        {
            AssertImpact(DataSourceType.Oracle, statementType, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("PostgreSql")]
        [DataRow("INSERT")]
        [DataRow("UPDATE")]
        [DataRow("DELETE")]
        [DataRow("MERGE")]
        [DataRow("CALL")]
        [DataRow("EXECUTE")]
        [DataRow("LOCK")]
        [DataRow("CREATE")]
        [DataRow("ALTER")]
        [DataRow("DROP")]
        [DataRow("TRUNCATE")]
        [DataRow("COMMENT")]
        [DataRow("RENAME")]
        [DataRow("GRANT")]
        [DataRow("REVOKE")]
        [DataRow("REFRESH")]
        [DataRow("CLUSTER")]
        [DataRow("REINDEX")]
        [DataRow("WITH INSERT")]
        [DataRow("WITH UPDATE")]
        [DataRow("WITH DELETE")]
        public void GetImpact_PostgreSqlTransactionalStatement_ReturnsPending(string statementType)
        {
            AssertImpact(DataSourceType.PostgreSql, statementType, DatabaseTransactionImpact.Pending);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("PostgreSql")]
        [DataRow("SELECT")]
        [DataRow("WITH SELECT")]
        [DataRow("SHOW")]
        [DataRow("EXPLAIN")]
        [DataRow("VACUUM")]
        [DataRow("SAVEPOINT")]
        public void GetImpact_PostgreSqlReadOnlyOrUntrackedStatement_ReturnsUnchanged(string statementType)
        {
            AssertImpact(DataSourceType.PostgreSql, statementType, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("SqlServer")]
        [DataRow("INSERT")]
        [DataRow("UPDATE")]
        [DataRow("DELETE")]
        [DataRow("MERGE")]
        [DataRow("EXEC")]
        [DataRow("EXECUTE")]
        [DataRow("CREATE")]
        [DataRow("ALTER")]
        [DataRow("DROP")]
        [DataRow("TRUNCATE")]
        [DataRow("GRANT")]
        [DataRow("REVOKE")]
        [DataRow("DENY")]
        [DataRow("WITH INSERT")]
        [DataRow("WITH UPDATE")]
        [DataRow("WITH DELETE")]
        public void GetImpact_SqlServerTransactionalStatement_ReturnsPending(string statementType)
        {
            AssertImpact(DataSourceType.SqlServer, statementType, DatabaseTransactionImpact.Pending);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("SqlServer")]
        [DataRow("SELECT")]
        [DataRow("WITH SELECT")]
        [DataRow("USE")]
        [DataRow("GO")]
        [DataRow("PRINT")]
        [DataRow("SAVE")]
        public void GetImpact_SqlServerReadOnlyOrUntrackedStatement_ReturnsUnchanged(string statementType)
        {
            AssertImpact(DataSourceType.SqlServer, statementType, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("MySql")]
        [DataRow("INSERT")]
        [DataRow("UPDATE")]
        [DataRow("DELETE")]
        [DataRow("MERGE")]
        [DataRow("CALL")]
        [DataRow("EXEC")]
        [DataRow("EXECUTE")]
        [DataRow("WITH INSERT")]
        [DataRow("WITH UPDATE")]
        [DataRow("WITH DELETE")]
        public void GetImpact_MySqlPendingStatement_ReturnsPending(string statementType)
        {
            AssertImpact(DataSourceType.MySql, statementType, DatabaseTransactionImpact.Pending);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("MySql")]
        [DataRow("CREATE")]
        [DataRow("ALTER")]
        [DataRow("DROP")]
        [DataRow("TRUNCATE")]
        [DataRow("RENAME")]
        [DataRow("GRANT")]
        [DataRow("REVOKE")]
        [DataRow("ANALYZE")]
        [DataRow("OPTIMIZE")]
        [DataRow("REPAIR")]
        [DataRow("CHECK")]
        [DataRow("LOCK")]
        [DataRow("UNLOCK")]
        public void GetImpact_MySqlImplicitCommitStatement_ReturnsClosed(string statementType)
        {
            AssertImpact(DataSourceType.MySql, statementType, DatabaseTransactionImpact.Closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("MySql")]
        [DataRow("SELECT")]
        [DataRow("WITH SELECT")]
        [DataRow("SHOW")]
        [DataRow("USE")]
        [DataRow("DESCRIBE")]
        [DataRow("SAVEPOINT")]
        public void GetImpact_MySqlReadOnlyOrUntrackedStatement_ReturnsUnchanged(string statementType)
        {
            AssertImpact(DataSourceType.MySql, statementType, DatabaseTransactionImpact.Unchanged);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(" update ")]
        [DataRow("\r\nUpdate\r\n")]
        [DataRow("uPdAtE")]
        public void GetImpact_NormalizesWhitespaceAndCase(string statementType)
        {
            AssertImpact(DataSourceType.PostgreSql, statementType, DatabaseTransactionImpact.Pending);
        }

        private static void AssertImpact(DataSourceType dataSourceType, string statementType, DatabaseTransactionImpact expected)
        {
            var actual = DatabaseTransactionStatePolicy.GetImpact(dataSourceType, statementType);

            Assert.AreEqual(expected, actual);
        }

        private static DataSourceType ParseDataSourceType(string dataSourceName)
        {
            switch (dataSourceName)
            {
                case "Oracle":
                    {
                        return DataSourceType.Oracle;
                    }
                case "PostgreSql":
                    {
                        return DataSourceType.PostgreSql;
                    }
                case "SqlServer":
                    {
                        return DataSourceType.SqlServer;
                    }
                case "MySql":
                    {
                        return DataSourceType.MySql;
                    }
                default:
                    {
                        return DataSourceType.None;
                    }
            }
        }
    }
}
