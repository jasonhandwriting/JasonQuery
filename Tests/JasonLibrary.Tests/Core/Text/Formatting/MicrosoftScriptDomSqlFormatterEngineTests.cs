using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Text.Formatting.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class MicrosoftScriptDomSqlFormatterEngineTests
    {
        private readonly MicrosoftScriptDomSqlFormatterEngine _engine = new MicrosoftScriptDomSqlFormatterEngine();

        [TestMethod]
        public void Supports_OnlySqlServer()
        {
            Assert.IsTrue(_engine.Supports(DatabaseProviderKind.SqlServer));
            Assert.IsFalse(_engine.Supports(DatabaseProviderKind.Oracle));
            Assert.IsFalse(_engine.Supports(DatabaseProviderKind.PostgreSql));
            Assert.IsFalse(_engine.Supports(DatabaseProviderKind.MySql));
            Assert.IsFalse(_engine.Supports(DatabaseProviderKind.Sqlite));
        }

        [TestMethod]
        public void Format_Select_ReturnsNormalizedFormattedSql()
        {
            var result = Format
            (
                "SELECT TOP (100) a.CustomerId,a.Name FROM dbo.Customer a WHERE a.IsActive=1 ORDER BY a.Name;"
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT TOP (100)");
            StringAssert.Contains(result.FormattedSql, "FROM dbo.Customer AS a");
            StringAssert.Contains(result.FormattedSql, "\r\nWHERE a.IsActive = 1");
            Assert.IsFalse(result.FormattedSql.Contains("\t"));
            Assert.IsFalse(result.FormattedSql.Replace("\r\n", string.Empty).Contains("\n"));
        }

        [TestMethod]
        public void Format_RecursiveCte_ReturnsSuccess()
        {
            var result = Format
            (
                ";WITH cte AS (SELECT EmployeeId,ManagerId,1 AS LevelNo FROM dbo.Employee WHERE ManagerId IS NULL " +
                "UNION ALL SELECT e.EmployeeId,e.ManagerId,c.LevelNo+1 FROM dbo.Employee e INNER JOIN cte c " +
                "ON e.ManagerId=c.EmployeeId) SELECT * FROM cte OPTION (MAXRECURSION 100);"
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "WITH cte");
            StringAssert.Contains(result.FormattedSql, "OPTION (MAXRECURSION 100);");
        }

        [TestMethod]
        public void Format_Merge_ReturnsSuccess()
        {
            var result = Format
            (
                "MERGE dbo.Target AS t USING dbo.Source AS s ON t.Id=s.Id " +
                "WHEN MATCHED THEN UPDATE SET t.Name=s.Name " +
                "WHEN NOT MATCHED BY TARGET THEN INSERT(Id,Name) VALUES(s.Id,s.Name) " +
                "WHEN NOT MATCHED BY SOURCE THEN DELETE OUTPUT $action,inserted.Id;"
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "MERGE INTO dbo.Target");
            StringAssert.Contains(result.FormattedSql, "WHEN MATCHED THEN UPDATE");
            StringAssert.Contains(result.FormattedSql, "OUTPUT $ACTION, inserted.Id;");
        }

        [TestMethod]
        public void Format_InvalidSql_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT * FROM";

            var result = Format(sql);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "rejected the SQL");
        }

        [TestMethod]
        public void Format_UnsupportedProvider_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT 1 FROM dual";

            var request = new SqlFormatRequest
            (
                sql,
                DatabaseProviderKind.Oracle,
                SqlFormatterEngineKind.MicrosoftScriptDom
            );

            var result = _engine.Format(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "SQL Server only");
        }

        [TestMethod]
        public void Format_MismatchedEngineKind_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT 1";

            var request = new SqlFormatRequest
            (
                sql,
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.Hogimn
            );

            var result = _engine.Format(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "does not match");
        }

        [TestMethod]
        public void Format_LowerKeywordCase_UsesLowercaseKeywords()
        {
            var request = new SqlFormatRequest
            (
                "SELECT Id FROM dbo.Customer WHERE Id=1",
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.MicrosoftScriptDom,
                new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower)
            );

            var result = _engine.Format(request);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "select Id");
            StringAssert.Contains(result.FormattedSql, "from dbo.Customer");
        }

        [TestMethod]
        public void Format_ZeroLinesBetweenStatements_DoesNotViolateScriptDomMinimum()
        {
            var request = new SqlFormatRequest
            (
                "SELECT 1; SELECT 2;",
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.MicrosoftScriptDom,
                new SqlFormatOptions(linesBetweenStatements: 0)
            );

            var result = _engine.Format(request);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT 1;");
            StringAssert.Contains(result.FormattedSql, "SELECT 2;");
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public void Format_LinesBetweenStatements_UsesConfiguredNewlineCount(int newlineCount)
        {
            var result = Format
            (
                "SELECT 1; SELECT 2;",
                new SqlFormatOptions(linesBetweenStatements: newlineCount)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);

            var firstStatementEnd = result.FormattedSql.IndexOf(';');
            var secondStatementStart = result.FormattedSql.IndexOf("SELECT 2", firstStatementEnd, StringComparison.Ordinal);

            var separator = result.FormattedSql.Substring
            (
                firstStatementEnd + 1,
                secondStatementStart - firstStatementEnd - 1
            );

            Assert.AreEqual(new string('\n', newlineCount), separator.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void Format_ListItemsPerLine_DefaultsToThree()
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C;";

            var result = Format(sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(SqlFormatOptions.DefaultListItemsPerLine, new SqlFormatOptions().ListItemsPerLine);
            StringAssert.Contains(result.FormattedSql, "SELECT C.A, C.B, C.C,\r\n       C.D");
        }

        [DataTestMethod]
        [DataRow(1, "SELECT C.A,\r\n       C.B,\r\n       C.C,\r\n       C.D")]
        [DataRow(2, "SELECT C.A, C.B,\r\n       C.C, C.D")]
        [DataRow(3, "SELECT C.A, C.B, C.C,\r\n       C.D")]
        [DataRow(10, "SELECT C.A, C.B, C.C, C.D")]
        public void Format_ListItemsPerLine_UsesConfiguredUpperBound(int itemsPerLine, string expectedSelect)
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C;";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: itemsPerLine)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, expectedSelect);
        }

        [TestMethod]
        public void Format_ListItemsPerLine_AppliesToGroupByAndOrderBy()
        {
            const string sql = "SELECT C.A,C.B,C.C FROM dbo.CUSTOMER C GROUP BY C.A,C.B,C.C ORDER BY C.A,C.B,C.C;";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT C.A, C.B,\r\n       C.C");
            StringAssert.Contains(result.FormattedSql, "GROUP BY C.A, C.B,\r\n         C.C");
            StringAssert.Contains(result.FormattedSql, "ORDER BY C.A, C.B,\r\n         C.C;");
        }

        [TestMethod]
        public void Format_ListItemsPerLine_WrapsBeforeCountWhenMaxWidthIsReached()
        {
            const string sql = "SELECT C.OWNER,C.TABLE_NAME FROM dbo.ALL_TAB_COLUMNS C;";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 20, listItemsPerLine: 10)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT C.OWNER,\r\n       C.TABLE_NAME");
        }

        [TestMethod]
        public void Format_ListItemsPerLine_DoesNotCountFunctionInListOrWindowCommas()
        {
            const string sql = "SELECT COALESCE(C.A,C.B) AS VALUE,CONCAT(C.C,C.D) AS TEXT_VALUE,ROW_NUMBER() OVER (PARTITION BY C.A,C.B ORDER BY C.C,C.D) AS ROW_NO,C.E FROM dbo.CUSTOMER C WHERE C.ID IN (1,2,3);";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "COALESCE (C.A, C.B) AS VALUE, CONCAT(C.C, C.D) AS TEXT_VALUE,");
            StringAssert.Contains(result.FormattedSql, "PARTITION BY C.A, C.B");
            StringAssert.Contains(result.FormattedSql, "ORDER BY C.C, C.D");
            StringAssert.Contains(result.FormattedSql, "C.ID IN (1, 2, 3)");
        }

        [TestMethod]
        public void Format_ListItemsPerLine_AppliesIndependentlyInsideNestedSelect()
        {
            const string sql = "SELECT C.A,(SELECT X.A,X.B,X.C FROM dbo.CHILD X WHERE X.ID=C.ID) AS CHILD_VALUE,C.B FROM dbo.CUSTOMER C;";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT X.A, X.B,");
            Assert.IsFalse(result.FormattedSql.Contains("SELECT X.A, X.B, X.C"));
        }

        [TestMethod]
        public void Format_ListItemsPerLine_PreservesCommentsStringsAndQuotedIdentifiers()
        {
            const string sql = "SELECT C.[Name,WithComma] AS [Alias,Name],'A,B' AS TEXT_VALUE,C.C /* keep, block comment */,C.D FROM dbo.CUSTOMER C;";

            var result = Format
            (
                sql,
                new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "[Name,WithComma]");
            StringAssert.Contains(result.FormattedSql, "[Alias,Name]");
            StringAssert.Contains(result.FormattedSql, "'A,B'");
            StringAssert.Contains(result.FormattedSql, "/* keep, block comment */");
        }

        [TestMethod]
        public void Format_WhitespaceOnly_ReturnsNormalizedInputWithoutParsing()
        {
            var result = Format("\t\n");

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("    ", result.FormattedSql);
        }

        private SqlFormatResult Format(string sql)
        {
            return Format(sql, null);
        }

        private SqlFormatResult Format(string sql, SqlFormatOptions options)
        {
            return _engine.Format
            (
                new SqlFormatRequest
                (
                    sql,
                    DatabaseProviderKind.SqlServer,
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    options
                )
            );
        }
    }
}
