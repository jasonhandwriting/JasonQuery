using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Execution
{
    [TestClass]
    public sealed class QueryExecutionMutationKeywordDetectorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT * FROM c", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) UPDATE dbo.t SET a = 1", true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) DELETE FROM dbo.t", true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) INSERT INTO dbo.t VALUES (1)", true)]
        [DataRow(DataSourceType.SqlServer, "DECLARE @x int; SELECT @x", false)]
        [DataRow(DataSourceType.SqlServer, "DECLARE @x int; UPDATE dbo.t SET a = 1", true)]
        [DataRow(DataSourceType.PostgreSql, "WITH c AS (SELECT 1) SELECT * FROM c", false)]
        [DataRow(DataSourceType.PostgreSql, "WITH c AS (SELECT 1) UPDATE t SET a = 1", true)]
        [DataRow(DataSourceType.Oracle, "WITH c AS (SELECT 1) DELETE FROM t", true)]
        [DataRow(DataSourceType.MySql, "WITH c AS (SELECT 1) INSERT INTO t VALUES (1)", true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT ' UPDATE ' AS value FROM c", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT \" UPDATE \" FROM c", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT /* UPDATE */ 1 FROM c", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT UPDATED FROM c", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT 1 -- UPDATE", false)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1)\nUPDATE t SET a = 1", true)]
        public void LegacyParity_StableCases(DataSourceType source, string sql, bool expected)
        {
            var legacy = QueryExecutionMutationKeywordDetector.ContainsMutationKeywordLegacyForParityTest(sql);
            var shared = QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(source, sql);

            Assert.AreEqual(expected, legacy, "Legacy contract for stable input.");
            Assert.AreEqual(legacy, shared, "New lexer must preserve stable legacy cases.");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) -- ordinary comment\r\nUPDATE t SET a = 1", false, true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) -- ordinary comment\nDELETE FROM t", false, true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) -- ordinary comment\rINSERT INTO t VALUES (1)", false, true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) /* note ' */ UPDATE t SET a = 1", false, true)]
        [DataRow(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT [ UPDATE ] FROM c", true, false)]
        [DataRow(DataSourceType.MySql, "WITH c AS (SELECT 1) SELECT ` UPDATE ` FROM c", true, false)]
        [DataRow(DataSourceType.PostgreSql, "WITH c AS (SELECT 1) SELECT $$ UPDATE $$ FROM c", true, false)]
        [DataRow(DataSourceType.MySql, "WITH c AS (SELECT 1) SELECT # UPDATE\n 1 FROM c", true, false)]
        [DataRow(DataSourceType.MySql, "WITH c AS (SELECT 1) SELECT 1# UPDATE\n 1 FROM c", true, false)]
        public void IntentionalCorrection_ExplicitlyDiffersFromLegacy(DataSourceType source, string sql, bool expectedLegacy, bool expectedShared)
        {
            var legacy = QueryExecutionMutationKeywordDetector.ContainsMutationKeywordLegacyForParityTest(sql);
            var shared = QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(source, sql);

            Assert.AreNotEqual(expectedLegacy, expectedShared, "Correction must be an intentional behavior delta.");
            Assert.AreEqual(expectedLegacy, legacy);
            Assert.AreEqual(expectedShared, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EmptySql_IsMutationFree()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, null));
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, string.Empty));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MutationKeywords_AreCaseInsensitive()
        {
            Assert.IsTrue(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "with c as (select 1) uPdAtE t set x=1"));
            Assert.IsTrue(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.Oracle, "WITH C AS (SELECT 1) deLEte FROM t"));
            Assert.IsTrue(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.MySql, "WITH c AS (SELECT 1) insERT INTO t VALUES(1)"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NonMutationWords_AreNotMatched()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT UPDATED, DELETED, INSERTED FROM c"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Merge_RemainsOutsideTheLegacyMutationKeywordPolicy()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) MERGE INTO t USING c ON 1=1"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void OracleAlternativeQuotedText_DoesNotCountAsMutation()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.Oracle, "WITH c AS (SELECT 1) SELECT q'[ UPDATE ]' AS t FROM c"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SqlServerBracketEscapedIdentifier_DoesNotCountAsMutation()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT [abc]] UPDATE ] FROM c"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IdentifierMemberAndParameterNames_DoNotBroadenRouting()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) SELECT x.UPDATE, @UPDATE FROM c"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TerminalWordRetainsLegacySpaceDelimitedBoundary()
        {
            Assert.IsFalse(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) UPDATE"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RemovedCommentTextCanBridgeLegacyWordBoundaries()
        {
            Assert.IsTrue(QueryExecutionMutationKeywordDetector.ContainsMutationKeyword(DataSourceType.SqlServer, "WITH c AS (SELECT 1) UPDATE/* preserved deletion boundary */ t SET a = 1"));
        }
    }
}
