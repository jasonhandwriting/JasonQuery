using System;
using System.IO;
using System.Runtime.CompilerServices;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Text.Formatting.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterStep4DRegressionTests
    {
        [DataTestMethod]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.SqlServer)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void Hogimn_NonOracleProvider_ListSettingChangesPackingButKeepsStep3ClauseLayout(DatabaseProviderKind providerKind)
        {
            const string sql = "SELECT C.ID,C.NAME FROM CUSTOMER C WHERE C.ID=1 ORDER BY C.NAME;";

            var oneItemResult = FormatWithHogimn
            (
                providerKind,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 1)
            );

            var tenItemResult = FormatWithHogimn
            (
                providerKind,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 10)
            );

            Assert.IsTrue(oneItemResult.Success, oneItemResult.ErrorMessage);
            Assert.IsTrue(tenItemResult.Success, tenItemResult.ErrorMessage);
            Assert.AreNotEqual(oneItemResult.FormattedSql, tenItemResult.FormattedSql);
            StringAssert.Contains(oneItemResult.FormattedSql, "SELECT\r\n");
            StringAssert.Contains(oneItemResult.FormattedSql, "\r\nFROM\r\n");
        }

        [DataTestMethod]
        [DataRow(SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(SqlFormatterEngineKind.Hogimn)]
        public void SqlServer_BothEngines_ListSettingChangesOutput(SqlFormatterEngineKind engineKind)
        {
            const string sql = "SELECT C.ID,C.NAME FROM dbo.CUSTOMER C WHERE C.ID=1 ORDER BY C.NAME;";

            var oneItemResult = FormatSqlServer
            (
                engineKind,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 1)
            );

            var tenItemResult = FormatSqlServer
            (
                engineKind,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 10)
            );

            Assert.IsTrue(oneItemResult.Success, oneItemResult.ErrorMessage);
            Assert.IsTrue(tenItemResult.Success, tenItemResult.ErrorMessage);
            Assert.AreEqual(engineKind, oneItemResult.EngineKind);
            Assert.AreEqual(engineKind, tenItemResult.EngineKind);

            Assert.AreNotEqual(oneItemResult.FormattedSql, tenItemResult.FormattedSql);
        }

        [TestMethod]
        public void Oracle_ComplexFunctionAndInListCommas_AreNotTreatedAsOuterListItems()
        {
            const string sql = "SELECT DECODE(C.STATUS,'A','Active','I','Inactive','Unknown') AS STATUS_TEXT,NVL2(C.OWNER,C.TABLE_NAME,C.COLUMN_NAME) AS DISPLAY_VALUE,C.COLUMN_ID FROM ALL_TAB_COLUMNS C WHERE C.COLUMN_ID IN (1,2,3) ORDER BY C.OWNER,C.TABLE_NAME,C.COLUMN_ID;";

            var result = FormatWithHogimn
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "DECODE(C.STATUS, 'A', 'Active', 'I', 'Inactive', 'Unknown')");
            StringAssert.Contains(result.FormattedSql, "NVL2(C.OWNER, C.TABLE_NAME, C.COLUMN_NAME)");
            StringAssert.Contains(result.FormattedSql, "C.COLUMN_ID IN (1, 2, 3)");
            StringAssert.Contains(result.FormattedSql, "AS STATUS_TEXT, NVL2");
            StringAssert.Contains(result.FormattedSql, "AS DISPLAY_VALUE,\r\n       C.COLUMN_ID");
            AssertSemanticallySafe(sql, result.FormattedSql);
        }

        [TestMethod]
        public void Oracle_CommentsStringsAndQuotedIdentifiers_RemainUnchanged()
        {
            const string sql = "SELECT \"Mixed,Case\" AS \"Alias,Name\", -- keep, line comment\r\n'A,B' AS TEXT_VALUE,q'[C,D]' AS ALT_VALUE,C.OWNER /* keep, block comment */ FROM ALL_TAB_COLUMNS C WHERE C.OWNER=USER;";

            var result = FormatWithHogimn
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 3)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "\"Mixed,Case\"");
            StringAssert.Contains(result.FormattedSql, "\"Alias,Name\"");
            StringAssert.Contains(result.FormattedSql, "'A,B'");
            StringAssert.Contains(result.FormattedSql, "q'[C,D]'");
            StringAssert.Contains(result.FormattedSql, "-- keep, line comment");
            StringAssert.Contains(result.FormattedSql, "/* keep, block comment */");
            AssertSemanticallySafe(sql, result.FormattedSql);
        }

        [TestMethod]
        public void Oracle_MultipleNestedSelects_KeepIndependentAlignmentAndPacking()
        {
            const string sql = "SELECT C.OWNER,(SELECT X.A,X.B,X.C FROM CHILD X WHERE X.ID=C.ID AND X.KIND='A') AS CHILD_VALUE,C.TABLE_NAME FROM CUSTOMER C WHERE EXISTS (SELECT Y.A,Y.B,Y.C FROM CHILD Y WHERE Y.ID=C.ID) ORDER BY C.OWNER,C.TABLE_NAME,C.COLUMN_ID;";

            var result = FormatWithHogimn
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT X.A, X.B,");
            StringAssert.Contains(result.FormattedSql, "SELECT Y.A, Y.B,");
            Assert.IsFalse(result.FormattedSql.Contains("SELECT X.A, X.B, X.C"));
            Assert.IsFalse(result.FormattedSql.Contains("SELECT Y.A, Y.B, Y.C"));
            StringAssert.Contains(result.FormattedSql, " ORDER BY C.OWNER, C.TABLE_NAME,");
            AssertSemanticallySafe(sql, result.FormattedSql);
        }

        [TestMethod]
        public void HogimnPipeline_TokenValidatorRunsAfterAllFormattingStages()
        {
            var sourcePath = GetHogimnEngineSourcePath();
            var source = File.ReadAllText(sourcePath);
            var statementSpacingIndex = FindRequired(source, "formattedSql = ApplyStatementSpacing");

            var oracleGuardIndex = FindRequired
            (
                source,
                "if (request.ProviderKind == DatabaseProviderKind.Oracle)",
                statementSpacingIndex
            );

            var clauseAlignerIndex = FindRequired(source, "formattedSql = OracleSqlClauseAligner.Apply", oracleGuardIndex);
            var listPackerIndex = FindRequired(source, "formattedSql = SqlListPacker.Apply", clauseAlignerIndex);
            var normalizerIndex = FindRequired(source, "formattedSql = SqlTextNormalizer.Normalize", listPackerIndex);
            var validatorIndex = FindRequired(source, "var validation = SqlTokenSemanticValidator.Validate", normalizerIndex);

            Assert.IsTrue(statementSpacingIndex < oracleGuardIndex);
            Assert.IsTrue(oracleGuardIndex < clauseAlignerIndex);
            Assert.IsTrue(clauseAlignerIndex < listPackerIndex);
            Assert.IsTrue(listPackerIndex < normalizerIndex);
            Assert.IsTrue(normalizerIndex < validatorIndex);
        }

        [TestMethod]
        public void MicrosoftScriptDomPipeline_TokenValidatorRunsAfterListPacking()
        {
            var sourcePath = GetFormatterEngineSourcePath("MicrosoftScriptDomSqlFormatterEngine.cs");
            var source = File.ReadAllText(sourcePath);
            var generatorIndex = FindRequired(source, "generator.GenerateScript");
            var baselineIndex = FindRequired(source, "var scriptDomFormattedSql = SqlTextNormalizer.Normalize", generatorIndex);
            var statementSpacingIndex = FindRequired(source, "formattedSql = SqlStatementSpacingNormalizer.Apply", baselineIndex);
            var listPackerIndex = FindRequired(source, "formattedSql = SqlListPacker.Apply", statementSpacingIndex);
            var normalizerIndex = FindRequired(source, "formattedSql = SqlTextNormalizer.Normalize", listPackerIndex);
            var validatorIndex = FindRequired(source, "var validation = SqlTokenSemanticValidator.Validate", normalizerIndex);

            Assert.IsTrue(generatorIndex < baselineIndex);
            Assert.IsTrue(baselineIndex < statementSpacingIndex);
            Assert.IsTrue(statementSpacingIndex < listPackerIndex);
            Assert.IsTrue(listPackerIndex < normalizerIndex);
            Assert.IsTrue(normalizerIndex < validatorIndex);
        }

        private static SqlFormatResult FormatWithHogimn(DatabaseProviderKind providerKind, string sql, SqlFormatOptions options)
        {
            return new HogimnSqlFormatterEngine().Format
            (
                new SqlFormatRequest
                (
                    sql,
                    providerKind,
                    SqlFormatterEngineKind.Hogimn,
                    options
                )
            );
        }

        private static SqlFormatResult FormatSqlServer(SqlFormatterEngineKind engineKind, string sql, SqlFormatOptions options)
        {
            ISqlFormatterEngine engine = engineKind == SqlFormatterEngineKind.MicrosoftScriptDom
                                         ? (ISqlFormatterEngine)new MicrosoftScriptDomSqlFormatterEngine()
                                         : new HogimnSqlFormatterEngine();

            return engine.Format
            (
                new SqlFormatRequest
                (
                    sql,
                    DatabaseProviderKind.SqlServer,
                    engineKind,
                    options
                )
            );
        }

        private static void AssertSemanticallySafe(string originalSql, string formattedSql)
        {
            var validation = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.Oracle,
                originalSql,
                formattedSql
            );

            Assert.IsTrue(validation.IsSafe, validation.ErrorMessage);
        }

        private static int FindRequired(string source, string value, int startIndex = 0)
        {
            var index = source.IndexOf(value, startIndex, StringComparison.Ordinal);

            Assert.IsTrue(index >= 0, "Required pipeline stage was not found: " + value);

            return index;
        }

        private static string GetHogimnEngineSourcePath([CallerFilePath] string testSourcePath = null)
        {
            return GetFormatterEngineSourcePath("HogimnSqlFormatterEngine.cs", testSourcePath);
        }

        private static string GetFormatterEngineSourcePath(string fileName, [CallerFilePath] string testSourcePath = null)
        {
            var repositoryRoot = Path.GetFullPath
            (
                Path.Combine
                (
                    Path.GetDirectoryName(testSourcePath),
                    "..",
                    "..",
                    "..",
                    "..",
                    ".."
                )
            );

            var sourcePath = Path.Combine
            (
                repositoryRoot,
                "JasonLibrary",
                "Core",
                "Text",
                "Formatting",
                "Engines",
                fileName
            );

            Assert.IsTrue(File.Exists(sourcePath), "Formatter source was not found: " + sourcePath);
            return sourcePath;
        }
    }
}
