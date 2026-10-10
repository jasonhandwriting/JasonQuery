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

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        [DataRow(DataSourceType.Oracle, "SELECT * FROM customer WHERE ", "MXwwfDZ8MHxVMFZNUlVOVQo0fDd8OHwwfEtnPT0KMXw5fDEzfDB8UmxKUFRRPT0KMXwxNHwyMnwwfFkzVnpkRzl0WlhJPQoxfDIzfDI4fDB8VjBoRlVrVT0KI1BST1RFQ1RFRCMK")]
        [DataRow(DataSourceType.Oracle, "SELECT q'[WHERE ]' FROM dual", "MXwwfDZ8MHxVMFZNUlVOVQoxfDd8OHwwfGNRPT0KM3w4fDE4fDB8SjF0WFNFVlNSU0JkSnc9PQoxfDE5fDIzfDB8UmxKUFRRPT0KMXwyNHwyOHwwfFpIVmhiQT09CiNQUk9URUNURUQjCjh8MTg=")]
        [DataRow(DataSourceType.Oracle, "SELECT q'[x' WHERE y]' FROM dual", "MXwwfDZ8MHxVMFZNUlVOVQoxfDd8OHwwfGNRPT0KM3w4fDEyfDB8SjF0NEp3PT0KMXwxM3wxOHwwfFYwaEZVa1U9CjF8MTl8MjB8MHxlUT09CjR8MjB8MjF8MHxYUT09CjN8MjF8MzJ8MHxKeUJHVWs5TklHUjFZV3c9CiNQUk9URUNURUQjCjh8MTIKMjF8MzI=")]
        [DataRow(DataSourceType.Oracle, "SELECT /* outer /* WHERE */ WHERE */ * FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoxfDI4fDMzfDB8VjBoRlVrVT0KNHwzNHwzNXwwfEtnPT0KNHwzNXwzNnwwfEx3PT0KNHwzN3wzOHwwfEtnPT0KMXwzOXw0M3wwfFJsSlBUUT09CjF8NDR8NTJ8MHxZM1Z6ZEc5dFpYST0KI1BST1RFQ1RFRCMKN3wyNw==")]
        [DataRow(DataSourceType.PostgreSql, "SELECT $$WHERE $$ FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoxfDd8MTR8MHxKQ1JYU0VWU1JRPT0KMXwxNXwxN3wwfEpDUT0KMXwxOHwyMnwwfFJsSlBUUT09CjF8MjN8MzF8MHxZM1Z6ZEc5dFpYST0KI1BST1RFQ1RFRCMK")]
        [DataRow(DataSourceType.PostgreSql, "SELECT $tag$WHERE (x)$tag$ FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoxfDd8MTd8MHxKSFJoWnlSWFNFVlNSUT09CjR8MTh8MTl8MHxLQT09CjF8MTl8MjB8MXxlQT09CjR8MjB8MjF8MHxLUT09CjF8MjF8MjZ8MHxKSFJoWnlRPQoxfDI3fDMxfDB8UmxKUFRRPT0KMXwzMnw0MHwwfFkzVnpkRzl0WlhJPQojUFJPVEVDVEVEIwo=")]
        [DataRow(DataSourceType.PostgreSql, "/* outer /* WHERE */ outer */ SELECT * FROM customer", "MXwyMXwyNnwwfGIzVjBaWEk9CjR8Mjd8Mjh8MHxLZz09CjR8Mjh8Mjl8MHxMdz09CjF8MzB8MzZ8MHxVMFZNUlVOVQo0fDM3fDM4fDB8S2c9PQoxfDM5fDQzfDB8UmxKUFRRPT0KMXw0NHw1MnwwfFkzVnpkRzl0WlhJPQojUFJPVEVDVEVEIwowfDIw")]
        [DataRow(DataSourceType.PostgreSql, "SELECT \"WHERE\" FROM [customer]", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTR8MHxJbGRJUlZKRklnPT0KMXwxNXwxOXwwfFJsSlBUUT09CjJ8MjB8MzB8MHxXMk4xYzNSdmJXVnlYUT09CiNQUk9URUNURUQjCg==")]
        [DataRow(DataSourceType.SqlServer, "SELECT [A]]WHERE] FROM dbo.customer", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTd8MHxXMEZkWFZkSVJWSkZYUT09CjF8MTh8MjJ8MHxSbEpQVFE9PQoxfDIzfDI2fDB8WkdKdgo0fDI2fDI3fDB8TGc9PQoxfDI3fDM1fDB8WTNWemRHOXRaWEk9CiNQUk9URUNURUQjCg==")]
        [DataRow(DataSourceType.SqlServer, "SELECT \"WHERE\" FROM [dbo].[customer]", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTR8MHxJbGRJUlZKRklnPT0KMXwxNXwxOXwwfFJsSlBUUT09CjJ8MjB8MjV8MHxXMlJpYjEwPQo0fDI1fDI2fDB8TGc9PQoyfDI2fDM2fDB8VzJOMWMzUnZiV1Z5WFE9PQojUFJPVEVDVEVEIwo=")]
        [DataRow(DataSourceType.MySql, "-- WHERE\r\nSELECT * FROM customer", "MXwxMHwxNnwwfFUwVk1SVU5VCjR8MTd8MTh8MHxLZz09CjF8MTl8MjN8MHxSbEpQVFE9PQoxfDI0fDMyfDB8WTNWemRHOXRaWEk9CiNQUk9URUNURUQjCjB8OA==")]
        [DataRow(DataSourceType.MySql, "--WHERE\r\nSELECT * FROM customer", "NHwwfDF8MHxMUT09CjR8MXwyfDB8TFE9PQoxfDJ8N3wwfFYwaEZVa1U9CjF8OXwxNXwwfFUwVk1SVU5VCjR8MTZ8MTd8MHxLZz09CjF8MTh8MjJ8MHxSbEpQVFE9PQoxfDIzfDMxfDB8WTNWemRHOXRaWEk9CiNQUk9URUNURUQjCg==")]
        [DataRow(DataSourceType.MySql, "# WHERE\r\nSELECT * FROM customer", "MXw5fDE1fDB8VTBWTVJVTlUKNHwxNnwxN3wwfEtnPT0KMXwxOHwyMnwwfFJsSlBUUT09CjF8MjN8MzF8MHxZM1Z6ZEc5dFpYST0KI1BST1RFQ1RFRCMKMHw3")]
        [DataRow(DataSourceType.MySql, "SELECT abc#WHERE FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoxfDd8MTZ8MHxZV0pqSTFkSVJWSkYKMXwxN3wyMXwwfFJsSlBUUT09CjF8MjJ8MzB8MHxZM1Z6ZEc5dFpYST0KI1BST1RFQ1RFRCMK")]
        [DataRow(DataSourceType.MySql, "SELECT 'a\\' WHERE ' FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQozfDd8MTF8MHxKMkZjSnc9PQoxfDEyfDE3fDB8VjBoRlVrVT0KM3wxOHwzM3wwfEp5QkdVazlOSUdOMWMzUnZiV1Z5CiNQUk9URUNURUQjCjd8MTEKMTh8MzM=")]
        [DataRow(DataSourceType.MySql, "SELECT \"a\\\" WHERE\" FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTF8MHxJbUZjSWc9PQoxfDEyfDE3fDB8VjBoRlVrVT0KMnwxN3wzMnwwfElpQkdVazlOSUdOMWMzUnZiV1Z5CiNQUk9URUNURUQjCg==")]
        [DataRow(DataSourceType.MySql, "SELECT `a\\` WHERE` FROM customer", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTF8MHxZR0ZjWUE9PQoxfDEyfDE3fDB8VjBoRlVrVT0KMnwxN3wzMnwwfFlDQkdVazlOSUdOMWMzUnZiV1Z5CiNQUk9URUNURUQjCg==")]
        [DataRow(DataSourceType.Oracle, "SELECT [WHERE], `ORDER`, \"GROUP\" FROM dual", "MXwwfDZ8MHxVMFZNUlVOVQoyfDd8MTR8MHxXMWRJUlZKRlhRPT0KNHwxNHwxNXwwfExBPT0KMnwxNnwyM3wwfFlFOVNSRVZTWUE9PQo0fDIzfDI0fDB8TEE9PQoyfDI1fDMyfDB8SWtkU1QxVlFJZz09CjF8MzN8Mzd8MHxSbEpQVFE9PQoxfDM4fDQyfDB8WkhWaGJBPT0KI1BST1RFQ1RFRCMK")]
        [DataRow(DataSourceType.PostgreSql, "SELECT * FROM customer WHERE name = 'unterminated WHERE ", "MXwwfDZ8MHxVMFZNUlVOVQo0fDd8OHwwfEtnPT0KMXw5fDEzfDB8UmxKUFRRPT0KMXwxNHwyMnwwfFkzVnpkRzl0WlhJPQoxfDIzfDI4fDB8VjBoRlVrVT0KMXwyOXwzM3wwfGJtRnRaUT09CjR8MzR8MzV8MHxQUT09CjN8MzZ8NTZ8MHxKM1Z1ZEdWeWJXbHVZWFJsWkNCWFNFVlNSU0E9CiNQUk9URUNURUQjCjM2fDU2")]
        [DataRow(DataSourceType.PostgreSql, "WITH 中文 AS (SELECT (1 + 2) AS 值) SELECT * FROM 中文 WHERE ", "MXwwfDR8MHxWMGxVU0E9PQoxfDV8N3wwfDVMaXQ1cGFICjF8OHwxMHwwfFFWTT0KNHwxMXwxMnwwfEtBPT0KMXwxMnwxOHwxfFUwVk1SVU5VCjR8MTl8MjB8MXxLQT09CjF8MjB8MjF8MnxNUT09CjR8MjJ8MjN8MnxLdz09CjF8MjR8MjV8MnxNZz09CjR8MjV8MjZ8MXxLUT09CjF8Mjd8Mjl8MXxRVk09CjF8MzB8MzF8MXw1WUM4CjR8MzF8MzJ8MHxLUT09CjF8MzN8Mzl8MHxVMFZNUlVOVQo0fDQwfDQxfDB8S2c9PQoxfDQyfDQ2fDB8UmxKUFRRPT0KMXw0N3w0OXwwfDVMaXQ1cGFICjF8NTB8NTV8MHxWMGhGVWtVPQojUFJPVEVDVEVEIwo=")]
        public void SharedTokenizerAdapter_MatchesGoldenSnapshot(DataSourceType dataSourceType, string sql, string expectedSnapshotUtf8Base64)
        {
            var actual = QueryEditorAutoCompleteSpaceAnalyzer.CreateSharedTokenizerSnapshotForParityTest(dataSourceType, sql);
            var expected = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(expectedSnapshotUtf8Base64));

            Assert.AreEqual(expected, actual);
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
