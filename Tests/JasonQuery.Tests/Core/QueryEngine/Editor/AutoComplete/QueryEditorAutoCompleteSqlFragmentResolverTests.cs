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
    }
}
