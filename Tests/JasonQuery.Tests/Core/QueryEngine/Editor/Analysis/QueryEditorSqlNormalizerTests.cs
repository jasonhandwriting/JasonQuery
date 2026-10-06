using JasonQuery.Core.QueryEngine.Editor.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Analysis
{
    [TestClass]
    public sealed class QueryEditorSqlNormalizerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("SqlNormalizer")]
        [DataRow("select * from t", true, false)]
        [DataRow("select /* comment */ a from t", true, false)]
        [DataRow("select a -- comment\r\nfrom t", true, false)]
        [DataRow("-- comment\r\nselect 1", true, false)]
        [DataRow("select /* one */ a /* two */ from t", true, false)]
        [DataRow("select a,\r\nb from t;", true, false)]
        [DataRow("select (a) from t", true, false)]
        [DataRow("select a from t", false, false)]
        [DataRow("select a from t", true, true)]
        [DataRow("select a from t", false, true)]
        [DataRow("select /* -- x */ a from t", true, false)]
        [DataRow("select a -- /* x */\r\nfrom t", true, false)]
        [DataRow("select /**/a from t", true, false)]
        [DataRow("select a--x\r\nfrom t", true, false)]
        [DataRow("select a /*x*/--y\r\nfrom t", true, false)]
        [DataRow("select a\r\n\r\nfrom t", true, false)]
        public void GetSingleLineSql_SharedTokenizer_MatchesLegacy(string sql, bool toUpperCase, bool appendTrailingSpace)
        {
            var shared = QueryEditorSqlNormalizer.GetSingleLineSql(sql, toUpperCase, appendTrailingSpace);
            var legacy = QueryEditorSqlNormalizer.GetSingleLineSqlLegacyForParityTest(sql, toUpperCase, appendTrailingSpace);

            Assert.AreEqual(legacy, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("SqlNormalizer")]
        [DataRow("select 'a--b' as value", "SELECT 'A--B' AS VALUE")]
        [DataRow("select 'a/*b*/c' as value", "SELECT 'A/*B*/C' AS VALUE")]
        [DataRow("select \"a--b\" from t", "SELECT \"A--B\" FROM T")]
        [DataRow("select [a--b] from t", "SELECT [A--B] FROM T")]
        [DataRow("select `a/*b*/c` from t", "SELECT `A/*B*/C` FROM T")]
        [DataRow("select a /* unfinished", "SELECT A")]
        [DataRow("select a -- comment\nfrom t", "SELECT A FROM T")]
        [DataRow("select a -- comment\rfrom t", "SELECT A FROM T")]
        public void GetSingleLineSql_SharedTokenizer_CorrectsLegacyCommentClassification(string sql, string expected)
        {
            var shared = QueryEditorSqlNormalizer.GetSingleLineSql(sql);
            var legacy = QueryEditorSqlNormalizer.GetSingleLineSqlLegacyForParityTest(sql);

            Assert.AreEqual(expected, shared);
            Assert.AreNotEqual(legacy, shared);
        }
    }
}
