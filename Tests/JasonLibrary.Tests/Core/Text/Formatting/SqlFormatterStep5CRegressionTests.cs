using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterStep5CRegressionTests
    {
        private const string FourColumnSelect = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C ORDER BY C.A,C.B,C.C,C.D;";

        [TestMethod]
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
            Assert.Contains("C.A, C.B,", result.FormattedSql);
            Assert.Contains("C.C, C.D", result.FormattedSql);
            Assert.DoesNotContain("C.A, C.B, C.C", result.FormattedSql);
        }

        [TestMethod]
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
            Assert.DoesNotContain("C.A, C.B", oneItemResult.FormattedSql);
            Assert.Contains("C.A, C.B, C.C, C.D", tenItemResult.FormattedSql);
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
            Assert.Contains("SELECT A, B,", result.FormattedSql);
            Assert.Contains("SELECT E, F,", result.FormattedSql);
            Assert.Contains("SELECT C.A, C.B,", result.FormattedSql);
            Assert.Contains("ORDER BY C.A, C.B,", result.FormattedSql);
            Assert.DoesNotContain("SELECT A, B, C", result.FormattedSql);
            Assert.DoesNotContain("SELECT E, F, G", result.FormattedSql);
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
            Assert.Contains("STRING_AGG(C.NAME, ',')", result.FormattedSql);
            Assert.Contains("ORDER BY C.NAME, C.ID", result.FormattedSql);
            Assert.Contains("PARTITION BY C.TYPE, C.STATUS", result.FormattedSql);
            Assert.Contains("ORDER BY C.ID, C.NAME", result.FormattedSql);
            Assert.Contains("C.ID IN (1, 2, 3)", result.FormattedSql);
        }

        [TestMethod]
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
            Assert.Contains("rejected", result.ErrorMessage);
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
