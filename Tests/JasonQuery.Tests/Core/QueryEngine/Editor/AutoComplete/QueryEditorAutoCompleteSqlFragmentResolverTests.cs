using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSqlFragmentResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("WITH x AS (SELECT id FROM customer) SELECT x.",
                 "SELECT id FROM customer")]
        [DataRow("WITH x AS (SELECT COALESCE(name, 'A') AS name FROM customer) SELECT x.",
                 "SELECT COALESCE(name, 'A') AS name FROM customer")]
        [DataRow("WITH x AS (SELECT '(' AS value1, ')' AS value2 FROM customer) SELECT x.",
                 "SELECT '(' AS value1, ')' AS value2 FROM customer")]
        [DataRow("WITH x AS (SELECT \"(\" AS value1 FROM customer) SELECT x.",
                 "SELECT \"(\" AS value1 FROM customer")]
        [DataRow("WITH x AS (\r\nSELECT id\r\nFROM customer\r\n) SELECT x.",
                 "SELECT id\r\nFROM customer")]
        public void GetAutoCompleteSqlForWithAs_ReturnsInnerSql(string sql, string expected)
        {
            var start = sql.IndexOf('(');
            var result = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sql, start);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void GetAutoCompleteSqlForWithAs_WithNestedSubquery_ReturnsWholeInnerSql()
        {
            const string sql = "WITH x AS (SELECT * FROM (SELECT id FROM customer) c) SELECT x.";
            const string expected = "SELECT * FROM (SELECT id FROM customer) c";

            var start = sql.IndexOf('(');
            var result = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sql, start);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("WITH x AS SELECT id FROM customer", 9)]
        [DataRow("WITH x AS (SELECT id FROM customer", 10)]
        [DataRow("", 0)]
        public void GetAutoCompleteSqlForWithAs_WithoutClosingParenthesis_ReturnsEmpty(string sql, int start)
        {
            var result = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sql, start);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM (SELECT id FROM customer) c",
                 "SELECT id FROM customer")]
        [DataRow("SELECT * FROM (SELECT COALESCE(name, 'A') AS name FROM customer) c",
                 "SELECT COALESCE(name, 'A') AS name FROM customer")]
        [DataRow("SELECT * FROM (SELECT '(' AS value1, ')' AS value2 FROM customer) c",
                 "SELECT '(' AS value1, ')' AS value2 FROM customer")]
        [DataRow("SELECT * FROM (SELECT \"(\" AS value1 FROM customer) c",
                 "SELECT \"(\" AS value1 FROM customer")]
        [DataRow("SELECT *\r\nFROM (\r\nSELECT id\r\nFROM customer\r\n) c",
                 "SELECT id\r\nFROM customer")]
        public void GetAutoCompleteSqlForSubquery_ReturnsInnerSql(string sql, string expected)
        {
            var start = sql.LastIndexOf(')');
            var result = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void GetAutoCompleteSqlForSubquery_WithNestedSubquery_ReturnsWholeInnerSql()
        {
            const string sql = "SELECT * FROM (SELECT * FROM (SELECT id FROM customer) c) x";
            const string expected = "SELECT * FROM (SELECT id FROM customer) c";

            var start = sql.LastIndexOf(')');
            var result = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM SELECT id FROM customer) c")]
        [DataRow("")]
        public void GetAutoCompleteSqlForSubquery_WithoutOpeningParenthesis_ReturnsEmpty(string sql)
        {
            var start = sql.LastIndexOf(')');

            if (start < 0)
            {
                start = Math.Max(sql.Length - 1, 0);
            }

            var result = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(string.Empty, result);
        }
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("WITH x AS (SELECT id FROM customer) SELECT x.", "SELECT id FROM customer")]
        [DataRow("WITH x AS (SELECT * FROM (SELECT id FROM customer) c) SELECT x.", "SELECT * FROM (SELECT id FROM customer) c")]
        [DataRow("WITH x AS (SELECT '(' AS a, ')' AS b FROM customer) SELECT x.", "SELECT '(' AS a, ')' AS b FROM customer")]
        [DataRow("WITH x AS (SELECT \"(\" AS a FROM customer) SELECT x.", "SELECT \"(\" AS a FROM customer")]
        [DataRow("WITH x AS (\r\nSELECT id\r\nFROM customer\r\n) SELECT x.", "SELECT id\r\nFROM customer")]
        [DataRow("WITH x AS (SELECT 'A''B' AS name FROM customer) SELECT x.", "SELECT 'A''B' AS name FROM customer")]
        [DataRow("WITH x AS (SELECT \"A\"\"B\" AS name FROM customer) SELECT x.", "SELECT \"A\"\"B\" AS name FROM customer")]
        [DataRow("WITH x AS (SELECT /* note */ id FROM customer) SELECT x.", "SELECT /* note */ id FROM customer")]
        [DataRow("WITH x AS (SELECT -- note\r\n id FROM customer) SELECT x.", "SELECT -- note\r\n id FROM customer")]
        [DataRow("WITH x AS (SELECT [id], `name` FROM customer) SELECT x.", "SELECT [id], `name` FROM customer")]
        public void WithAsSharedTokenizer_MatchesGoldenFragment(string sql, string expected)
        {
            var start = sql.IndexOf('(');
            var shared = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sql, start);

            Assert.AreEqual(expected, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("SELECT * FROM (SELECT id FROM customer) c", "SELECT id FROM customer")]
        [DataRow("SELECT * FROM (SELECT * FROM (SELECT id FROM customer) c) x", "SELECT * FROM (SELECT id FROM customer) c")]
        [DataRow("SELECT * FROM (SELECT '(' AS a, ')' AS b FROM customer) c", "SELECT '(' AS a, ')' AS b FROM customer")]
        [DataRow("SELECT * FROM (SELECT \"(\" AS a FROM customer) c", "SELECT \"(\" AS a FROM customer")]
        [DataRow("SELECT *\r\nFROM (\r\nSELECT id\r\nFROM customer\r\n) c", "SELECT id\r\nFROM customer")]
        [DataRow("SELECT * FROM (SELECT 'A''B' AS name FROM customer) c", "SELECT 'A''B' AS name FROM customer")]
        [DataRow("SELECT * FROM (SELECT \"A\"\"B\" AS name FROM customer) c", "SELECT \"A\"\"B\" AS name FROM customer")]
        [DataRow("SELECT * FROM (SELECT [id], `name` FROM customer) c", "SELECT [id], `name` FROM customer")]
        [DataRow("SELECT * FROM (SELECT 1 + 2 AS value FROM customer) c", "SELECT 1 + 2 AS value FROM customer")]
        [DataRow("SELECT * FROM (SELECT ';' AS value FROM customer) c", "SELECT ';' AS value FROM customer")]
        public void SubquerySharedTokenizer_MatchesGoldenFragment(string sql, string expected)
        {
            var start = sql.LastIndexOf(')');
            var shared = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(expected, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        public void SubquerySharedTokenizer_BlockCommentWithParenthesis_ReturnsWholeFragment()
        {
            const string sql = "SELECT * FROM (SELECT /* ( ignored */ id FROM customer) c";
            const string expected = "SELECT /* ( ignored */ id FROM customer";
            var start = sql.LastIndexOf(')');

            var shared = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(expected, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        public void SubquerySharedTokenizer_LineCommentWithParenthesis_ReturnsWholeFragment()
        {
            const string sql = "SELECT * FROM (SELECT -- ( ignored\r\n id FROM customer) c";
            const string expected = "SELECT -- ( ignored\r\n id FROM customer";
            var start = sql.LastIndexOf(')');

            var shared = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(expected, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        public void WithAsSharedTokenizer_MoreThanNineNestedParentheses_ReturnsWholeFragment()
        {
            var inner = "SELECT id FROM customer";

            for (var index = 0; index < 10; index++)
            {
                inner = "SELECT * FROM (" + inner + ") q";
            }

            var sql = "WITH x AS (" + inner + ") SELECT x.";
            var start = sql.IndexOf('(');

            var shared = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sql, start);

            Assert.AreEqual(inner, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        public void SubquerySharedTokenizer_MoreThanNineNestedParentheses_ReturnsWholeFragment()
        {
            var inner = "SELECT id FROM customer";

            for (var index = 0; index < 10; index++)
            {
                inner = "SELECT * FROM (" + inner + ") q";
            }

            var sql = "SELECT * FROM (" + inner + ") x";
            var start = sql.LastIndexOf(')');

            var shared = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(sql, start);

            Assert.AreEqual(inner, shared);
        }
    }
}
