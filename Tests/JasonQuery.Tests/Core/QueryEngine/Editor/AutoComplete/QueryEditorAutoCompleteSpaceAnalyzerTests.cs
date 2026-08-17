using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSpaceAnalyzerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(DataSourceType.Oracle, "UPDATE ", (int)QueryEditorAutoCompleteSpaceKeyword.TableTarget)]
        [DataRow(DataSourceType.PostgreSql, "INSERT INTO ", (int)QueryEditorAutoCompleteSpaceKeyword.TableTarget)]
        [DataRow(DataSourceType.SqlServer, "MERGE INTO ", (int)QueryEditorAutoCompleteSpaceKeyword.TableTarget)]
        [DataRow(DataSourceType.MySql, "DELETE FROM ", (int)QueryEditorAutoCompleteSpaceKeyword.TableTarget)]
        public void Analyze_TableTargetKeywords_ReturnTableOnlyObjects(DataSourceType dataSourceType, string sql, int expectedKeywordValue)
        {
            var result = Analyze(sql, sql.Length, dataSourceType);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceKeyword)expectedKeywordValue, result.Keyword);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceIntent.ListObjects, result.Intent);
            Assert.AreEqual(AutoCompleteObjectLookupMode.TableOnly, result.LookupMode);
            Assert.IsTrue(result.TableOnly);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(DataSourceType.SqlServer, "USE ")]
        [DataRow(DataSourceType.MySql, "use ")]
        public void Analyze_UseForSupportedDatabase_ReturnsDatabaseList(DataSourceType dataSourceType, string sql)
        {
            var result = Analyze(sql, sql.Length, dataSourceType);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceKeyword.Use, result.Keyword);
            Assert.AreEqual(QueryEditorAutoCompleteSpaceIntent.ListDatabases, result.Intent);
            Assert.AreEqual(AutoCompleteObjectLookupMode.DatabaseOnly, result.LookupMode);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        public void Analyze_UseForUnsupportedDatabase_ReturnsFalse(DataSourceType dataSourceType)
        {
            Assert.IsFalse(Analyze("USE ", 4, dataSourceType).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM ", (int)QueryEditorAutoCompleteSpaceKeyword.From, (int)QueryEditorAutoCompleteSpaceIntent.ListObjects)]
        [DataRow("SELECT * FROM customer WHERE ", (int)QueryEditorAutoCompleteSpaceKeyword.Where, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT * FROM customer WHERE id = 1 AND ", (int)QueryEditorAutoCompleteSpaceKeyword.And, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT * FROM customer WHERE id = 1 OR ", (int)QueryEditorAutoCompleteSpaceKeyword.Or, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT * FROM customer ORDER BY ", (int)QueryEditorAutoCompleteSpaceKeyword.By, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT * FROM customer GROUP BY ", (int)QueryEditorAutoCompleteSpaceKeyword.By, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("UPDATE customer SET ", (int)QueryEditorAutoCompleteSpaceKeyword.Set, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT  FROM customer", (int)QueryEditorAutoCompleteSpaceKeyword.SelectList, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        [DataRow("SELECT id,  FROM customer", (int)QueryEditorAutoCompleteSpaceKeyword.SelectList, (int)QueryEditorAutoCompleteSpaceIntent.ListColumns)]
        public void Analyze_CoreSpaceKeywords_ReturnExpectedIntent(string sql, int expectedKeywordValue, int expectedIntentValue)
        {
            var caretPosition = ResolveCaret(sql);
            var result = Analyze(sql, caretPosition, DataSourceType.PostgreSql);

            Assert.IsTrue(result.CanResolve);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceKeyword)expectedKeywordValue, result.Keyword);
            Assert.AreEqual((QueryEditorAutoCompleteSpaceIntent)expectedIntentValue, result.Intent);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT 'WHERE ' FROM customer", "WHERE ")]
        [DataRow("SELECT \"WHERE\"  FROM customer", "\"WHERE\" ")]
        [DataRow("SELECT [WHERE]  FROM customer", "[WHERE] ")]
        [DataRow("SELECT `WHERE`  FROM customer", "`WHERE` ")]
        [DataRow("SELECT * FROM customer -- WHERE ", "WHERE ")]
        [DataRow("SELECT * FROM customer # WHERE ", "WHERE ")]
        [DataRow("SELECT * FROM customer /* WHERE  */", "WHERE ")]
        public void Analyze_KeywordInsideQuotedTextOrComment_DoesNotTrigger(string sql, string caretAnchor)
        {
            var caretPosition = sql.IndexOf(caretAnchor, System.StringComparison.Ordinal) + caretAnchor.Length;

            Assert.IsFalse(Analyze(sql, caretPosition, DataSourceType.MySql).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT COALESCE(a, ) FROM customer")]
        [DataRow("SELECT 'a, ' FROM customer")]
        [DataRow("SELECT * FROM customer, ")]
        [DataRow("SELECT * FROM customer WHERE id IN (1, )")]
        public void Analyze_CommaOutsideSelectList_DoesNotTrigger(string sql)
        {
            Assert.IsFalse(Analyze(sql, ResolveCaret(sql), DataSourceType.PostgreSql).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(null, 0, DataSourceType.Oracle)]
        [DataRow("", 0, DataSourceType.Oracle)]
        [DataRow("SELECT ", 7, DataSourceType.None)]
        [DataRow("SELECT 1", -1, DataSourceType.Oracle)]
        public void Analyze_InvalidInput_ReturnsFalse(string sql, int caretPosition, DataSourceType dataSourceType)
        {
            Assert.IsFalse(Analyze(sql, caretPosition, dataSourceType).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT ")]
        [DataRow("SELECT id, ")]
        [DataRow("SELECT USE ")]
        [DataRow("SELECT * FROM customer BY ")]
        [DataRow("UPDATE customer ")]
        [DataRow("INSERT INTO customer ")]
        public void Analyze_IncompleteOrNonTriggerContext_ReturnsFalse(string sql)
        {
            Assert.IsFalse(Analyze(sql, sql.Length, DataSourceType.PostgreSql).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer; SELECT ", "SELECT ")]
        [DataRow("SELECT * FROM customer; SELECT id, ", "id, ")]
        public void Analyze_DoesNotUseFromFromPreviousStatement(string sql, string caretAnchor)
        {
            var caretPosition = sql.LastIndexOf(caretAnchor, System.StringComparison.Ordinal) + caretAnchor.Length;

            Assert.IsFalse(Analyze(sql, caretPosition, DataSourceType.PostgreSql).CanResolve);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("SELECT * FROM customer WHERE name = 'unclosed WHERE ")]
        [DataRow("SELECT * FROM customer /* unclosed WHERE ")]
        [DataRow("SELECT * FROM customer -- WHERE ")]
        public void Analyze_CaretInsideUnclosedProtectedText_ReturnsFalse(string sql)
        {
            Assert.IsFalse(Analyze(sql, sql.Length - 1, DataSourceType.PostgreSql).CanResolve);
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
    }
}