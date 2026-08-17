using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterStep5CRegressionTests
    {
        private const string FourColumnSelect = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C ORDER BY C.A,C.B,C.C,C.D;";

        [DataTestMethod]
        [DataRow(SqlFormatterEngineKind.Unknown, SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(SqlFormatterEngineKind.MicrosoftScriptDom, SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(SqlFormatterEngineKind.Hogimn, SqlFormatterEngineKind.Hogimn)]
        public void Coordinator_SqlServerListSetting_IsAppliedByResolvedEngine(SqlFormatterEngineKind requestedEngineKind,
                                                                               SqlFormatterEngineKind expectedEngineKind)
        {
            var result = Format
            (
                requestedEngineKind,
                FourColumnSelect,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(expectedEngineKind, result.EngineKind);
            StringAssert.Contains(result.FormattedSql, "C.A, C.B,");
            StringAssert.Contains(result.FormattedSql, "C.C, C.D");
            Assert.IsFalse(result.FormattedSql.Contains("C.A, C.B, C.C"));
        }

        [DataTestMethod]
        [DataRow(SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(SqlFormatterEngineKind.Hogimn)]
        public void Coordinator_SqlServerListSetting_BoundariesProduceDistinctOutput(SqlFormatterEngineKind engineKind)
        {
            var oneItemResult = Format
            (
                engineKind,
                FourColumnSelect,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 1)
            );

            var tenItemResult = Format
            (
                engineKind,
                FourColumnSelect,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 10)
            );

            Assert.IsTrue(oneItemResult.Success, oneItemResult.ErrorMessage);
            Assert.IsTrue(tenItemResult.Success, tenItemResult.ErrorMessage);
            Assert.AreEqual(engineKind, oneItemResult.EngineKind);
            Assert.AreEqual(engineKind, tenItemResult.EngineKind);
            Assert.AreNotEqual(oneItemResult.FormattedSql, tenItemResult.FormattedSql);
            Assert.IsFalse(oneItemResult.FormattedSql.Contains("C.A, C.B"));
            StringAssert.Contains(tenItemResult.FormattedSql, "C.A, C.B, C.C, C.D");
        }

        [TestMethod]
        public void ScriptDom_CteApplyAndNestedSelect_KeepIndependentListContexts()
        {
            const string sql = "WITH CTE AS (SELECT A,B,C,D FROM dbo.SOURCE_TABLE) SELECT C.A,C.B,X.E,X.F FROM CTE C CROSS APPLY (SELECT E,F,G,H FROM dbo.CHILD_TABLE X WHERE X.ID=C.A) X ORDER BY C.A,C.B,X.E,X.F;";

            var result = Format
            (
                SqlFormatterEngineKind.MicrosoftScriptDom,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT A, B,");
            StringAssert.Contains(result.FormattedSql, "SELECT E, F,");
            StringAssert.Contains(result.FormattedSql, "SELECT C.A, C.B,");
            StringAssert.Contains(result.FormattedSql, "ORDER BY C.A, C.B,");
            Assert.IsFalse(result.FormattedSql.Contains("SELECT A, B, C"));
            Assert.IsFalse(result.FormattedSql.Contains("SELECT E, F, G"));
        }

        [TestMethod]
        public void ScriptDom_StringAggWindowAndInList_InternalCommasAreNotListItems()
        {
            const string sql = "SELECT STRING_AGG(C.NAME, ',') WITHIN GROUP (ORDER BY C.NAME,C.ID) AS NAMES,ROW_NUMBER() OVER (PARTITION BY C.TYPE,C.STATUS ORDER BY C.ID,C.NAME) AS ROW_NO,C.ID FROM dbo.CUSTOMER C WHERE C.ID IN (1,2,3);";

            var result = Format
            (
                SqlFormatterEngineKind.MicrosoftScriptDom,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "STRING_AGG(C.NAME, ',')");
            StringAssert.Contains(result.FormattedSql, "ORDER BY C.NAME, C.ID");
            StringAssert.Contains(result.FormattedSql, "PARTITION BY C.TYPE, C.STATUS");
            StringAssert.Contains(result.FormattedSql, "ORDER BY C.ID, C.NAME");
            StringAssert.Contains(result.FormattedSql, "C.ID IN (1, 2, 3)");
        }

        [DataTestMethod]
        [DataRow("UPDATE dbo.CUSTOMER SET A=1,B=2,C=3,D=4 WHERE ID=1;")]
        [DataRow("INSERT INTO dbo.CUSTOMER (A,B,C,D) VALUES (1,2,3,4);")]
        public void ScriptDom_NonQueryCommaLists_AreIndependentOfListSetting(string sql)
        {
            var oneItemResult = Format
            (
                SqlFormatterEngineKind.MicrosoftScriptDom,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 1)
            );

            var tenItemResult = Format
            (
                SqlFormatterEngineKind.MicrosoftScriptDom,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 10)
            );

            Assert.IsTrue(oneItemResult.Success, oneItemResult.ErrorMessage);
            Assert.IsTrue(tenItemResult.Success, tenItemResult.ErrorMessage);
            Assert.AreEqual(oneItemResult.FormattedSql, tenItemResult.FormattedSql);
        }

        [TestMethod]
        public void Coordinator_InvalidScriptDomSql_RestoresOriginalSqlForAnyListSetting()
        {
            const string sql = "SELECT A,B,C,D FROM;";

            var result = Format
            (
                SqlFormatterEngineKind.MicrosoftScriptDom,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 1)
            );

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.MicrosoftScriptDom, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "rejected");
        }

        private static SqlFormatResult Format(SqlFormatterEngineKind engineKind, string sql, SqlFormatOptions options)
        {
            return new SqlFormatterCoordinator().Format
            (
                sql,
                DatabaseProviderKind.SqlServer,
                engineKind,
                options
            );
        }
    }
}