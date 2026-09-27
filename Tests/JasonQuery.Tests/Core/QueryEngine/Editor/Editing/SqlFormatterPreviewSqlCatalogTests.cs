using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class SqlFormatterPreviewSqlCatalogTests
    {
        [TestMethod]
        [DataRow(DatabaseProviderKind.Oracle, "ALL_TAB_COLUMNS")]
        [DataRow(DatabaseProviderKind.PostgreSql, "pg_catalog.pg_attribute")]
        [DataRow(DatabaseProviderKind.SqlServer, "sys.columns")]
        [DataRow(DatabaseProviderKind.MySql, "information_schema.COLUMNS")]
        [DataRow(DatabaseProviderKind.Sqlite, "pragma_table_info")]
        public void Get_ReturnsProviderSpecificMetadataSql(DatabaseProviderKind providerKind, string expectedMetadataObject)
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(providerKind);
            var normalizedSql = sql.ToLowerInvariant();

            Assert.Contains(expectedMetadataObject.ToLowerInvariant(), normalizedSql);
            Assert.Contains("column_name", normalizedSql);
            Assert.Contains("order by", normalizedSql);
        }

        [TestMethod]
        [DataRow(DatabaseProviderKind.Oracle)]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.SqlServer)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void Get_DefaultEngineFormatsMetadataSqlSafely(DatabaseProviderKind providerKind)
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(providerKind);
            var coordinator = new SqlFormatterCoordinator();

            var result = coordinator.Format
            (
                sql,
                providerKind,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(4, 120, 2, SqlFormatterKeywordCase.Upper)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreNotEqual(sql, result.FormattedSql);
            Assert.Contains("\r\n", result.FormattedSql);
        }

        [TestMethod]
        public void Get_UnknownProvider_ReturnsDefensiveGenericSql()
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(DatabaseProviderKind.Unknown);

            Assert.Contains("information_schema.columns", sql);
        }

        [TestMethod]
        public void Get_OraclePreview_DefaultEngineDoesNotInsertWhitespaceBeforeDot()
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(DatabaseProviderKind.Oracle);
            var coordinator = new SqlFormatterCoordinator();

            var result = coordinator.Format
            (
                sql,
                DatabaseProviderKind.Oracle,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(4, 999, 2, SqlFormatterKeywordCase.Upper)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("C.OWNER", result.FormattedSql);
            Assert.DoesNotContain("C .", result.FormattedSql);
        }

        [TestMethod]
        public void Get_OraclePreview_RepeatedSqlUsesSpacingAndLowercaseSafely()
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(DatabaseProviderKind.Oracle);
            var repeatedSql = sql + "\r\n" + sql;
            var coordinator = new SqlFormatterCoordinator();

            var result = coordinator.Format
            (
                repeatedSql,
                DatabaseProviderKind.Oracle,
                SqlFormatterEngineKind.Hogimn,
                new SqlFormatOptions(4, 999, 4, SqlFormatterKeywordCase.Lower)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("C.COLUMN_ID\r\n\r\n\r\n\r\nselect", result.FormattedSql);
            Assert.Contains("user", result.FormattedSql);
            Assert.DoesNotContain("C .", result.FormattedSql);
        }
    }
}
