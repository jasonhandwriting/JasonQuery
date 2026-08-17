using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSpaceRegressionTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT  FROM customer", "customer")]
        [DataRow("SELECT customer_id,  FROM customer", "customer")]
        [DataRow("SELECT customer_id,\r\n        FROM customer", "customer")]
        public void SelectListSpace_ResolvesObjectAfterCaret(string sql, string expectedObject)
        {
            var result = Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceKeyword.SelectList, result.Keyword);
            Assert.AreEqual(expectedObject, result.ObjectName);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer \"WHERE\" WHERE ", "customer", "\"WHERE\"")]
        [DataRow("SELECT * FROM customer [ORDER] ORDER BY ", "customer", "[ORDER]")]
        [DataRow("SELECT * FROM customer `GROUP` GROUP BY ", "customer", "`GROUP`")]
        public void QuotedKeywordAlias_DoesNotReplaceActualClauseKeyword(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, ResolveSourceType(sql));

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT COALESCE(first_name, last_name),  FROM customer")]
        [DataRow("SELECT CONCAT('a,b', name),  FROM customer")]
        [DataRow("SELECT (SELECT MAX(id) FROM orders),  FROM customer")]
        public void TopLevelComma_TriggersAfterNestedComma(string sql)
        {
            var result = Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceKeyword.SelectList, result.Keyword);
            Assert.AreEqual("customer", result.ObjectName);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer WHERE (status = 1 OR )")]
        [DataRow("SELECT * FROM customer WHERE id IN (1, )")]
        [DataRow("SELECT * FROM customer WHERE name = 'AND '")]
        public void NestedOrQuotedKeyword_DoesNotCreateInvalidSourceResolution(string sql)
        {
            Assert.IsFalse(Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql).CanResolve);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("WITH x AS (SELECT id FROM customer) SELECT id,  FROM x", (int)QueryEditorAutoCompleteSpaceSourceKind.CteSql)]
        [DataRow("SELECT id,  FROM (SELECT id FROM customer) x", (int)QueryEditorAutoCompleteSpaceSourceKind.SubquerySql)]
        [DataRow("SELECT id,  FROM public.customer x", (int)QueryEditorAutoCompleteSpaceSourceKind.ObjectName)]
        public void SelectList_ResolvesCteSubqueryAndObjectSource(string sql, int expectedSourceKindValue)
        {
            var result = Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceSourceKind)expectedSourceKindValue, result.SourceKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where)]
        [DataRow("SELECT * FROM customer WHERE id = 1 AND ", (int)QueryEditorAutoCompleteSpaceKeyword.And)]
        [DataRow("SELECT * FROM customer WHERE id = 1 OR ", (int)QueryEditorAutoCompleteSpaceKeyword.Or)]
        [DataRow("SELECT * FROM customer ORDER BY ", (int)QueryEditorAutoCompleteSpaceKeyword.By)]
        [DataRow("UPDATE customer SET ", (int)QueryEditorAutoCompleteSpaceKeyword.Set)]
        public void ClauseKeyword_ResolvesSamePrimaryObject(string sql, int expectedKeywordValue)
        {
            var result = Analyze(sql, sql.Length, DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceKeyword)expectedKeywordValue, result.Keyword);
            Assert.AreEqual("customer", result.ObjectName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("UPDATE customer SET name = 'A' WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, "customer", "")]
        [DataRow("UPDATE abc\r\nSET T2_NUMBER25_5 = 1231\r\nWHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, "abc", "")]
        [DataRow("UPDATE customer c SET name = 'A' WHERE id = 1 AND ", (int)QueryEditorAutoCompleteSpaceKeyword.And, "customer", "c")]
        [DataRow("UPDATE customer AS c SET name = 'A' WHERE id = 1 OR ", (int)QueryEditorAutoCompleteSpaceKeyword.Or, "customer", "c")]
        [DataRow("UPDATE public.customer c SET name = 'A' WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, "public.customer", "c")]
        [DataRow("UPDATE [dbo].[Customer] SET [Name] = 'A' WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, "[dbo].[Customer]", "")]
        [DataRow("UPDATE `sakila`.`customer` c SET `name` = 'A' WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, "`sakila`.`customer`", "c")]
        public void UpdateWhereAndOrSpace_ResolvesUpdateTarget(string sql, int expectedKeywordValue, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, ResolveSourceType(sql));

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceKeyword)expectedKeywordValue, result.Keyword);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceIntent.ListColumns, result.Intent);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceSourceKind.ObjectName, result.SourceKind);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("UPDATE customer c SET name = s.name FROM source_customer s WHERE ", "source_customer", "s")]
        [DataRow("UPDATE c SET name = s.name FROM dbo.customer c JOIN dbo.source_customer s ON s.id = c.id WHERE ", "dbo.customer", "c")]
        public void UpdateFromWhere_PreservesFromSourcePriority(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, DataSourceType.SqlServer);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceKeyword.Where, result.Keyword);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer WHERE id IN (SELECT * FROM orders WHERE )", "orders")]
        [DataRow("SELECT * FROM (SELECT  FROM orders) q", "orders")]
        public void NestedQuery_UsesSourceAtCaretDepth(string sql, string expectedObject)
        {
            var caretPosition = sql.IndexOf(" )", System.StringComparison.Ordinal) + 1;

            if (caretPosition <= 0)
            {
                caretPosition = sql.IndexOf("  FROM", System.StringComparison.Ordinal) + 1;
            }

            var result = Analyze(sql, caretPosition, DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(expectedObject, result.ObjectName);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer c JOIN orders o ON o.customer_id = c.id WHERE ", "customer", "c")]
        [DataRow("SELECT * FROM customer AS c LEFT JOIN orders AS o ON o.customer_id = c.id ORDER BY ", "customer", "c")]
        public void MultipleSources_UsesPrimaryFromSource(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        private static QueryEditorAutoCompleteSpaceAnalysisResult Analyze(string sql, int caretPosition, DataSourceType dataSourceType)
        {
            return QueryEditorAutoCompleteSpaceAnalyzer.Analyze
            (
                new QueryEditorAutoCompleteSpaceAnalysisRequest
                {
                    Sql = sql,
                    CaretPosition = caretPosition,
                    DataSourceType = dataSourceType
                }
            );
        }

        private static int ResolveCaret(string sql)
        {
            var doubleSpaceIndex = sql.IndexOf("  ", System.StringComparison.Ordinal);

            return doubleSpaceIndex >= 0 ? doubleSpaceIndex + 1 : sql.Length;
        }

        private static DataSourceType ResolveSourceType(string sql)
        {
            if (sql.IndexOf('[') >= 0)
            {
                return DataSourceType.SqlServer;
            }

            if (sql.IndexOf('`') >= 0)
            {
                return DataSourceType.MySql;
            }

            return DataSourceType.PostgreSql;
        }
    }
}