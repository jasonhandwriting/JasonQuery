using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSpaceSourceTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer WHERE ", "customer", "")]
        [DataRow("SELECT * FROM customer c WHERE ", "customer", "c")]
        [DataRow("SELECT * FROM customer AS c WHERE ", "customer", "c")]
        [DataRow("SELECT * FROM public.customer c WHERE ", "public.customer", "c")]
        [DataRow("SELECT * FROM \"public\".\"Customer\" AS c WHERE ", "\"public\".\"Customer\"", "c")]
        [DataRow("SELECT * FROM [MyDB].[dbo].[Customer] AS c WHERE ", "[MyDB].[dbo].[Customer]", "c")]
        [DataRow("SELECT * FROM `sakila`.`customer` c WHERE ", "`sakila`.`customer`", "c")]
        [DataRow("SELECT * FROM customer \"WHERE\" WHERE ", "customer", "\"WHERE\"")]
        public void Analyze_WhereSource_ReturnsObjectAndAlias(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, ResolveSourceType(sql));

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceSourceKind.ObjectName, result.SourceKind);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("UPDATE customer SET ", "customer", "")]
        [DataRow("UPDATE public.customer c SET ", "public.customer", "c")]
        [DataRow("UPDATE \"Customer\" AS c SET ", "\"Customer\"", "c")]
        [DataRow("UPDATE [dbo].[Customer] SET ", "[dbo].[Customer]", "")]
        [DataRow("UPDATE `sakila`.`customer` c SET ", "`sakila`.`customer`", "c")]
        public void Analyze_SetSource_ReturnsUpdateObject(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, sql.Length, ResolveSourceType(sql));

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceSourceKind.ObjectName, result.SourceKind);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT  FROM customer", "customer", "")]
        [DataRow("SELECT id,  FROM customer c", "customer", "c")]
        [DataRow("SELECT COALESCE(id, 0),  FROM public.customer AS c", "public.customer", "c")]
        [DataRow("SELECT  FROM [MyDB].[dbo].[Customer] c", "[MyDB].[dbo].[Customer]", "c")]
        public void Analyze_SelectListSource_UsesFollowingFrom(string sql, string expectedObject, string expectedAlias)
        {
            var result = Analyze(sql, ResolveCaret(sql), ResolveSourceType(sql));

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceKeyword.SelectList, result.Keyword);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT  FROM (SELECT id, name FROM customer) c", "SELECT id, name FROM customer", "c")]
        [DataRow("SELECT * FROM (SELECT id FROM customer WHERE name = ')') AS c WHERE ", "SELECT id FROM customer WHERE name = ')'", "c")]
        [DataRow("SELECT * FROM (WITH x AS (SELECT id FROM customer) SELECT id FROM x) q WHERE ",
                 "WITH x AS (SELECT id FROM customer) SELECT id FROM x", "q")]
        public void Analyze_SubquerySource_ReturnsInnerSql(string sql, string expectedSourceSql, string expectedAlias)
        {
            var result = Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceSourceKind.SubquerySql, result.SourceKind);
            Assert.AreEqual(expectedSourceSql, result.SourceSql);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("WITH x AS (SELECT id FROM customer) SELECT  FROM x", "x", "SELECT id FROM customer", "")]
        [DataRow("WITH x AS (SELECT id FROM customer) SELECT * FROM x c WHERE ", "x", "SELECT id FROM customer", "c")]
        [DataRow("WITH x(id) AS (SELECT id FROM customer) SELECT  FROM x", "x", "SELECT id FROM customer", "")]
        [DataRow("WITH x AS (SELECT id FROM customer), y AS (SELECT id FROM orders) SELECT  FROM y", "y", "SELECT id FROM orders", "")]
        [DataRow("WITH RECURSIVE x AS (SELECT id FROM customer) SELECT  FROM x", "x", "SELECT id FROM customer", "")]
        [DataRow("WITH \"Mixed Cte\" AS (SELECT id FROM customer) SELECT  FROM \"Mixed Cte\"", "\"Mixed Cte\"", "SELECT id FROM customer", "")]
        public void Analyze_CteSource_ReturnsCteSql(string sql, string expectedObject, string expectedSourceSql, string expectedAlias)
        {
            var result = Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceSourceKind.CteSql, result.SourceKind);
            Assert.AreEqual(expectedObject, result.ObjectName);
            Assert.AreEqual(expectedSourceSql, result.SourceSql);
            Assert.AreEqual(expectedAlias, result.AliasName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM ONLY customer WHERE ", "customer")]
        [DataRow("SELECT * FROM LATERAL customer WHERE ", "customer")]
        public void Analyze_SourceModifier_IsSkipped(string sql, string expectedObject)
        {
            var result = Analyze(sql, sql.Length, DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(expectedObject, result.ObjectName);
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