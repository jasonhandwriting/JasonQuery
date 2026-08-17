using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class SqlFormatterPreviewSqlCatalogTests
    {
        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle, "ALL_TAB_COLUMNS")]
        [DataRow(DatabaseProviderKind.PostgreSql, "pg_catalog.pg_attribute")]
        [DataRow(DatabaseProviderKind.SqlServer, "sys.columns")]
        [DataRow(DatabaseProviderKind.MySql, "information_schema.COLUMNS")]
        [DataRow(DatabaseProviderKind.Sqlite, "pragma_table_info")]
        public void Get_ReturnsProviderSpecificMetadataSql(DatabaseProviderKind providerKind, string expectedMetadataObject)
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(providerKind);
            var normalizedSql = sql.ToLowerInvariant();

            StringAssert.Contains(normalizedSql, expectedMetadataObject.ToLowerInvariant());
            StringAssert.Contains(normalizedSql, "column_name");
            StringAssert.Contains(normalizedSql, "order by");
        }

        [DataTestMethod]
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
            StringAssert.Contains(result.FormattedSql, "\r\n");
        }

        [TestMethod]
        public void Get_UnknownProvider_ReturnsDefensiveGenericSql()
        {
            var sql = SqlFormatterPreviewSqlCatalog.Get(DatabaseProviderKind.Unknown);

            StringAssert.Contains(sql, "information_schema.columns");
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
            StringAssert.Contains(result.FormattedSql, "C.OWNER");
            Assert.IsFalse(result.FormattedSql.Contains("C ."));
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
            StringAssert.Contains(result.FormattedSql, "C.COLUMN_ID\r\n\r\n\r\n\r\nselect");
            StringAssert.Contains(result.FormattedSql, "user");
            Assert.IsFalse(result.FormattedSql.Contains("C ."));
        }
    }
}