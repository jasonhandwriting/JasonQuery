using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class ColumnDefaultValueFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Format_WithNone_ReturnsEmptyClause()
        {
            var result = Format(DataSourceType.Oracle, "VARCHAR2", ColumnDefaultValueKind.None, "ignored");

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(string.Empty, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "Jason's", " DEFAULT 'Jason''s'")]
        [DataRow(DataSourceType.PostgreSql, "TEXT", "日本", " DEFAULT '日本'")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "台北'101", " DEFAULT N'台北''101'")]
        [DataRow(DataSourceType.SqlServer, "VARCHAR", "ABC", " DEFAULT 'ABC'")]
        [DataRow(DataSourceType.MySql, "VARCHAR", "", " DEFAULT ''")]
        public void Format_StringLiteral_ReturnsQuotedClause(DataSourceType dataSourceType, string typeKey, string value, string expected)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, value);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(expected, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.PostgreSql, "INTEGER", "123", " DEFAULT 123")]
        [DataRow(DataSourceType.SqlServer, "BIGINT", "-20", " DEFAULT -20")]
        [DataRow(DataSourceType.MySql, "YEAR", "2026", " DEFAULT 2026")]
        [DataRow(DataSourceType.Oracle, "NUMBER", "123.45", " DEFAULT 123.45")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC", "-0.25", " DEFAULT -0.25")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "12.500", " DEFAULT 12.500")]
        public void Format_NumericLiteral_ReturnsUnquotedClause(DataSourceType dataSourceType, string typeKey, string value, string expected)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, value);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(expected, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.PostgreSql, "BOOLEAN", "true", " DEFAULT TRUE")]
        [DataRow(DataSourceType.PostgreSql, "BOOLEAN", "0", " DEFAULT FALSE")]
        [DataRow(DataSourceType.SqlServer, "BIT", "yes", " DEFAULT 1")]
        [DataRow(DataSourceType.SqlServer, "BIT", "false", " DEFAULT 0")]
        public void Format_BooleanLiteral_ReturnsDatabaseSpecificClause(DataSourceType dataSourceType, string typeKey, string value, string expected)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, value);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(expected, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "DATE", "2026-07-19", " DEFAULT DATE '2026-07-19'")]
        [DataRow(DataSourceType.PostgreSql, "DATE", "2026-07-19", " DEFAULT DATE '2026-07-19'")]
        [DataRow(DataSourceType.SqlServer, "DATE", "2026-07-19", " DEFAULT '2026-07-19'")]
        [DataRow(DataSourceType.MySql, "DATE", "2026-07-19", " DEFAULT '2026-07-19'")]
        [DataRow(DataSourceType.PostgreSql, "TIME", "07:43:15.123456", " DEFAULT TIME '07:43:15.123456'")]
        [DataRow(DataSourceType.Oracle, "TIMESTAMP", "2026-07-19 07:43:15.123456", " DEFAULT TIMESTAMP '2026-07-19 07:43:15.123456'")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP", "2026-07-19T07:43:15", " DEFAULT TIMESTAMP '2026-07-19 07:43:15'")]
        [DataRow(DataSourceType.SqlServer, "DATETIME2", "2026-07-19 07:43:15", " DEFAULT '2026-07-19 07:43:15'")]
        [DataRow(DataSourceType.MySql, "DATETIME", "2026-07-19 07:43:15.123456", " DEFAULT '2026-07-19 07:43:15.123456'")]
        public void Format_DateTimeLiteral_ReturnsDatabaseSpecificClause(DataSourceType dataSourceType, string typeKey, string value, string expected)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, value);

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual(expected, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "SYS_GUID()", " DEFAULT SYS_GUID()")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP", "CURRENT_TIMESTAMP", " DEFAULT CURRENT_TIMESTAMP")]
        [DataRow(DataSourceType.SqlServer, "UNIQUEIDENTIFIER", "NEWID()", " DEFAULT NEWID()")]
        [DataRow(DataSourceType.MySql, "TIMESTAMP", "CURRENT_TIMESTAMP", " DEFAULT CURRENT_TIMESTAMP")]
        public void Format_SqlExpression_ReturnsExpressionWithoutQuotes(DataSourceType dataSourceType, string typeKey, string value, string expected)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.SqlExpression, value);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(expected, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.PostgreSql, "INTEGER", "abc")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "12,5")]
        [DataRow(DataSourceType.PostgreSql, "BOOLEAN", "maybe")]
        [DataRow(DataSourceType.Oracle, "DATE", "2026/07/19")]
        [DataRow(DataSourceType.MySql, "DATETIME", "not-date")]
        [DataRow(DataSourceType.SqlServer, "INT", "1; DROP TABLE a")]
        public void Format_InvalidLiteral_ReturnsFailure(DataSourceType dataSourceType, string typeKey, string value)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, value);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(string.Empty, result.SqlClause);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("CURRENT_TIMESTAMP; DROP TABLE a")]
        [DataRow("CURRENT_TIMESTAMP -- comment")]
        [DataRow("func(")]
        public void Format_UnsafeExpression_ReturnsFailure(string expression)
        {
            var result = Format(DataSourceType.PostgreSql, "TIMESTAMP", ColumnDefaultValueKind.SqlExpression, expression);

            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "BLOB")]
        [DataRow(DataSourceType.Oracle, "CLOB")]
        [DataRow(DataSourceType.SqlServer, "XML")]
        [DataRow(DataSourceType.MySql, "TEXT")]
        public void Format_WhenTypeDoesNotSupportDefault_ReturnsFailure(DataSourceType dataSourceType, string typeKey)
        {
            var result = Format(dataSourceType, typeKey, ColumnDefaultValueKind.Literal, "abc");

            Assert.IsFalse(result.Succeeded);
        }

        private static ColumnDefaultFormatResult Format(DataSourceType dataSourceType, string typeKey, ColumnDefaultValueKind kind, string value)
        {
            return ColumnDefaultValueFormatter.Format
                   (
                       dataSourceType,
                       ColumnTypeCatalog.Find
                       (
                           dataSourceType,
                           typeKey
                       ),
                       new ColumnDefinition
                       {
                           DefaultValueKind = kind,
                           DefaultValue = value
                       }
                   );
        }
    }
}
