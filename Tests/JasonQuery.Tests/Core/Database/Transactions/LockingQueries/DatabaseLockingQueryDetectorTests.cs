using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Transactions.LockingQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Transactions.LockingQueries
{
    [TestClass]
    public sealed class DatabaseLockingQueryDetectorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [TestCategory("Oracle")]
        [DataRow("SELECT * FROM a_test FOR UPDATE", "ForUpdate")]
        [DataRow("select * from a_test for update nowait", "ForUpdate")]
        [DataRow("select * from a_test for update wait 5", "ForUpdate")]
        [DataRow("select * from a_test for update skip locked", "ForUpdate")]
        [DataRow("select * from a_test for update of t1, t2", "ForUpdate")]
        [DataRow("WITH q AS (SELECT * FROM a_test) SELECT * FROM q FOR UPDATE", "ForUpdate")]
        [DataRow("\r\nselect *\r\nfrom a_test\r\nFoR   UpDaTe\r\n", "ForUpdate")]
        [DataRow("select * from a_test where id = 1 FOR UPDATE;\r\n-- end", "ForUpdate")]
        public void Detect_OracleLockingQueries_ReturnExpectedKind(string sql, string expectedKindName)
        {
            AssertDetected
            (
                DataSourceType.Oracle,
                sql,
                ParseLockingQueryKind(expectedKindName)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [TestCategory("PostgreSql")]
        [DataRow("SELECT * FROM a_test FOR UPDATE", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR UPDATE NOWAIT", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR UPDATE SKIP LOCKED", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR UPDATE OF a", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR NO KEY UPDATE", "ForNoKeyUpdate")]
        [DataRow("SELECT * FROM a_test FOR NO KEY UPDATE NOWAIT", "ForNoKeyUpdate")]
        [DataRow("SELECT * FROM a_test FOR SHARE", "ForShare")]
        [DataRow("SELECT * FROM a_test FOR SHARE SKIP LOCKED", "ForShare")]
        [DataRow("SELECT * FROM a_test FOR KEY SHARE", "ForKeyShare")]
        [DataRow("WITH q AS (SELECT * FROM a_test) SELECT * FROM q FOR KEY SHARE NOWAIT", "ForKeyShare")]
        public void Detect_PostgreSqlLockingQueries_ReturnExpectedKind(string sql, string expectedKindName)
        {
            AssertDetected
            (
                DataSourceType.PostgreSql,
                sql,
                ParseLockingQueryKind(expectedKindName)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [TestCategory("MySql")]
        [DataRow("SELECT * FROM a_test FOR UPDATE", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR UPDATE NOWAIT", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR UPDATE SKIP LOCKED", "ForUpdate")]
        [DataRow("SELECT * FROM a_test FOR SHARE", "ForShare")]
        [DataRow("SELECT * FROM a_test FOR SHARE NOWAIT", "ForShare")]
        [DataRow("SELECT * FROM a_test LOCK IN SHARE MODE", "LockInShareMode")]
        [DataRow("select *\r\nfrom a_test\r\nlock in share mode", "LockInShareMode")]
        [DataRow("WITH q AS (SELECT * FROM a_test) SELECT * FROM q FOR UPDATE", "ForUpdate")]
        [DataRow("SELECT * FROM `for update table` FOR UPDATE", "ForUpdate")]
        public void Detect_MySqlLockingQueries_ReturnExpectedKind(string sql, string expectedKindName)
        {
            AssertDetected
            (
                DataSourceType.MySql,
                sql,
                ParseLockingQueryKind(expectedKindName)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [TestCategory("SqlServer")]
        [DataRow("SELECT * FROM dbo.a_test WITH (UPDLOCK)", "SqlServerUpdateLock")]
        [DataRow("SELECT * FROM dbo.a_test WITH (XLOCK)", "SqlServerExclusiveLock")]
        [DataRow("SELECT * FROM dbo.a_test WITH (HOLDLOCK)", "SqlServerHoldLock")]
        [DataRow("SELECT * FROM dbo.a_test WITH (TABLOCKX)", "SqlServerTableExclusiveLock")]
        [DataRow("SELECT * FROM dbo.a_test WITH (SERIALIZABLE)", "SqlServerSerializable")]
        [DataRow("SELECT * FROM dbo.a_test WITH (ROWLOCK, UPDLOCK)", "SqlServerUpdateLock")]
        [DataRow("SELECT * FROM dbo.a_test WITH (NOLOCK, XLOCK)", "SqlServerExclusiveLock")]
        [DataRow("SELECT * FROM dbo.a_test AS a WITH(UPDLOCK, ROWLOCK)", "SqlServerUpdateLock")]
        [DataRow("WITH q AS (SELECT * FROM dbo.a_test WITH (HOLDLOCK)) SELECT * FROM q", "SqlServerHoldLock")]
        [DataRow("SELECT * FROM [UPDLOCK] WITH (UPDLOCK)", "SqlServerUpdateLock")]
        [DataRow(";WITH q AS (SELECT * FROM dbo.a_test WITH (UPDLOCK)) SELECT * FROM q", "SqlServerUpdateLock")]
        public void Detect_SqlServerLockingQueries_ReturnExpectedKind(string sql, string expectedKindName)
        {
            AssertDetected
            (
                DataSourceType.SqlServer,
                sql,
                ParseLockingQueryKind(expectedKindName)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "SELECT 'FOR UPDATE' FROM dual")]
        [DataRow("Oracle", "SELECT q'[FOR UPDATE]' FROM dual")]
        [DataRow("Oracle", "SELECT q'{FOR UPDATE}' FROM dual")]
        [DataRow("Oracle", "SELECT \"FOR UPDATE\" FROM a_test")]
        [DataRow("Oracle", "-- SELECT * FROM a_test FOR UPDATE\r\nSELECT 1 FROM dual")]
        [DataRow("Oracle", "/* SELECT * FROM a_test FOR UPDATE */ SELECT 1 FROM dual")]
        [DataRow("Oracle", "SELECT before_update FROM a_test")]
        [DataRow("Oracle", "SELECT 1 FROM dual; SELECT * FROM a_test FOR UPDATE")]
        [DataRow("PostgreSql", "SELECT 'FOR UPDATE'")]
        [DataRow("PostgreSql", "SELECT \"FOR UPDATE\" FROM a_test")]
        [DataRow("PostgreSql", "SELECT $$FOR UPDATE$$")]
        [DataRow("PostgreSql", "SELECT $tag$FOR NO KEY UPDATE$tag$")]
        [DataRow("PostgreSql", "-- FOR SHARE\r\nSELECT 1")]
        [DataRow("PostgreSql", "/* FOR KEY SHARE */ SELECT 1")]
        [DataRow("PostgreSql", "SELECT before_update FROM a_test")]
        [DataRow("MySql", "SELECT 'FOR UPDATE'")]
        [DataRow("MySql", "SELECT \"FOR UPDATE\"")]
        [DataRow("MySql", "SELECT `FOR UPDATE` FROM a_test")]
        [DataRow("MySql", "# FOR UPDATE\r\nSELECT 1")]
        [DataRow("MySql", "-- LOCK IN SHARE MODE\r\nSELECT 1")]
        [DataRow("MySql", "/* FOR SHARE */ SELECT 1")]
        [DataRow("MySql", "SELECT before_update FROM a_test")]
        [DataRow("SqlServer", "SELECT 'WITH (UPDLOCK)'")]
        [DataRow("SqlServer", "SELECT [WITH (UPDLOCK)] FROM dbo.a_test")]
        [DataRow("SqlServer", "SELECT \"WITH (XLOCK)\" FROM dbo.a_test")]
        [DataRow("SqlServer", "-- WITH (UPDLOCK)\r\nSELECT 1")]
        [DataRow("SqlServer", "/* WITH (TABLOCKX) */ SELECT 1")]
        [DataRow("SqlServer", "SELECT * FROM dbo.a_test WITH (ROWLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM dbo.a_test WITH (PAGLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM dbo.a_test WITH (TABLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM dbo.a_test WITH (READPAST, NOLOCK)")]
        public void Detect_NonLockingOrMaskedText_ReturnsNotDetected(string dataSourceTypeName, string sql)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);
            var result = DatabaseLockingQueryDetector.Detect(dataSourceType, sql);

            Assert.IsFalse(result.IsLockingQuery);
            Assert.AreEqual(DatabaseLockingQueryKind.None, result.Kind);
            Assert.AreEqual(-1, result.MatchedPosition);
            Assert.AreEqual(string.Empty, result.MatchedText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "prefix\r\nSELECT * FROM a_test FoR   UpDaTe", "FoR   UpDaTe")]
        [DataRow("PostgreSql", "prefix\r\nSELECT * FROM a_test FOR NO KEY UPDATE", "FOR NO KEY UPDATE")]
        [DataRow("MySql", "prefix\r\nSELECT * FROM a_test LOCK IN SHARE MODE", "LOCK IN SHARE MODE")]
        [DataRow("SqlServer", "prefix\r\nSELECT * FROM a_test WITH (ROWLOCK, UpDlOcK)", "UpDlOcK")]
        public void Detect_PreservesOriginalMatchedTextAndPosition(string dataSourceTypeName, string sql, string expectedText)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);
            var result = DatabaseLockingQueryDetector.Detect(dataSourceType, sql);

            Assert.IsTrue(result.IsLockingQuery);
            Assert.AreEqual(expectedText, result.MatchedText);

            Assert.AreEqual
            (
                sql.IndexOf(expectedText, StringComparison.Ordinal),
                result.MatchedPosition
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Detect_WithEmptySql_ReturnsNotDetected()
        {
            AssertNotDetected(DataSourceType.Oracle, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Detect_WithWhiteSpaceSql_ReturnsNotDetected()
        {
            AssertNotDetected(DataSourceType.PostgreSql, "   \r\n  ");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Detect_WithNoneDataSource_ReturnsNotDetected()
        {
            AssertNotDetected(DataSourceType.None, "SELECT * FROM a_test FOR UPDATE");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Detect_WhenMultipleLockingClausesExist_ReturnsEarliestClause()
        {
            const string sql = "SELECT * FROM a_test FOR SHARE;\r\n" +
                               "SELECT * FROM b_test FOR UPDATE";

            var result = DatabaseLockingQueryDetector.Detect(DataSourceType.PostgreSql, sql);

            Assert.IsTrue(result.IsLockingQuery);
            Assert.AreEqual(DatabaseLockingQueryKind.ForShare, result.Kind);

            Assert.AreEqual
            (
                sql.IndexOf("FOR SHARE", StringComparison.Ordinal),
                result.MatchedPosition
            );
        }

        private static DataSourceType ParseDataSourceType(string value)
        {
            return (DataSourceType)Enum.Parse(typeof(DataSourceType), value, ignoreCase: true);
        }

        private static DatabaseLockingQueryKind ParseLockingQueryKind(string value)
        {
            return (DatabaseLockingQueryKind)Enum.Parse
            (
                typeof(DatabaseLockingQueryKind),
                value,
                ignoreCase: true
            );
        }

        private static void AssertDetected(DataSourceType dataSourceType, string sql, DatabaseLockingQueryKind expectedKind)
        {
            var result = DatabaseLockingQueryDetector.Detect(dataSourceType, sql);

            Assert.IsTrue(result.IsLockingQuery);
            Assert.AreEqual(expectedKind, result.Kind);
            Assert.IsTrue(result.MatchedPosition >= 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.MatchedText));
        }

        private static void AssertNotDetected(DataSourceType dataSourceType, string sql)
        {
            var result = DatabaseLockingQueryDetector.Detect(dataSourceType, sql);

            Assert.IsFalse(result.IsLockingQuery);
            Assert.AreEqual(DatabaseLockingQueryKind.None, result.Kind);
            Assert.AreEqual(-1, result.MatchedPosition);
        }
    }
}
