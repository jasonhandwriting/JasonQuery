using System;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterCoordinatorTests
    {
        [TestMethod]
        public void Format_SqlServerWithoutOverride_UsesMicrosoftScriptDom()
        {
            var result = new SqlFormatterCoordinator().Format
            (
                "select CustomerId from dbo.Customers",
                DatabaseProviderKind.SqlServer
            );

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.MicrosoftScriptDom, result.EngineKind);
        }

        [TestMethod]
        public void Format_PostgreSqlWithoutOverride_UsesHogimn()
        {
            var result = new SqlFormatterCoordinator().Format
            (
                "select customer_id from public.customers",
                DatabaseProviderKind.PostgreSql
            );

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
        }

        [TestMethod]
        public void Format_SqliteWithoutOverride_UsesHogimnForFutureProviderSupport()
        {
            var result = new SqlFormatterCoordinator().Format
            (
                "select customer_id from customers",
                DatabaseProviderKind.Sqlite
            );

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
        }

        [TestMethod]
        public void Format_SqlServerHogimnOverride_UsesHogimn()
        {
            var result = new SqlFormatterCoordinator().Format
            (
                "select CustomerId from dbo.Customers",
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.Hogimn
            );

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.SqlServer)]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        public void Format_UnionAll_DoesNotAddBlankLinesAroundSetOperator(DatabaseProviderKind providerKind)
        {
            const string sql = "select id from table_a union all select id from table_b";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Upper);

            var result = new SqlFormatterCoordinator().Format
            (
                sql,
                providerKind,
                SqlFormatterEngineKind.Unknown,
                options
            );

            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.FormattedSql, "\r\nUNION ALL\r\n");
            Assert.IsFalse(result.FormattedSql.Contains("\r\n\r\nUNION ALL"));
            Assert.IsFalse(result.FormattedSql.Contains("UNION ALL\r\n\r\n"));
        }

        [TestMethod]
        public void Format_UnsupportedOverride_ReturnsOriginalSql()
        {
            const string sql = "select customer_id from customers";

            var result = new SqlFormatterCoordinator().Format
            (
                sql,
                DatabaseProviderKind.Oracle,
                SqlFormatterEngineKind.MicrosoftScriptDom
            );

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.MicrosoftScriptDom, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "does not support");
        }

        [TestMethod]
        public void Format_RejectedOverride_ReturnsOriginalSql()
        {
            const string sql = "select CustomerId from dbo.Customers";

            var result = new SqlFormatterCoordinator().Format
            (
                sql,
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.Laan
            );

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Laan, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "not ready");
        }

        [TestMethod]
        public void Format_UnknownProvider_ReturnsOriginalSql()
        {
            const string sql = "select 1";

            var result = new SqlFormatterCoordinator().Format(sql, DatabaseProviderKind.Unknown);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Unknown, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "No default formatter engine");
        }

        [TestMethod]
        public void Format_EngineThrows_ReturnsOriginalSql()
        {
            const string sql = "select 1 from dual";

            var coordinator = CreateCoordinator
            (
                request => throw new InvalidOperationException("formatter failed")
            );

            var result = coordinator.Format(sql, DatabaseProviderKind.Oracle);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "formatter failed");
        }

        [TestMethod]
        public void Format_EngineFailureContainsChangedSql_RestoresOriginalSql()
        {
            const string sql = "select 1 from dual";

            var coordinator = CreateCoordinator
            (
                request => SqlFormatResult.Failed
                (
                    SqlFormatterEngineKind.Hogimn,
                    "changed SQL",
                    "unsafe output"
                )
            );

            var result = coordinator.Format(sql, DatabaseProviderKind.Oracle);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            Assert.AreEqual("unsafe output", result.ErrorMessage);
        }

        [TestMethod]
        public void Format_EngineReturnsDifferentKind_ReturnsOriginalSql()
        {
            const string sql = "select 1 from dual";

            var coordinator = CreateCoordinator
            (
                request => SqlFormatResult.Succeeded
                (
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    "changed SQL"
                )
            );

            var result = coordinator.Format(sql, DatabaseProviderKind.Oracle);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "different engine kind");
        }

        [TestMethod]
        public void Format_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => new SqlFormatterCoordinator().Format((SqlFormatRequest)null)
            );
        }

        [TestMethod]
        public void Constructor_NullResolver_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => new SqlFormatterCoordinator(null)
            );
        }

        private static SqlFormatterCoordinator CreateCoordinator(Func<SqlFormatRequest, SqlFormatResult> format)
        {
            var resolver = new SqlFormatterEngineResolver
            (
                new ISqlFormatterEngine[]
                {
                    new FakeHogimnEngine(format)
                }
            );

            return new SqlFormatterCoordinator(resolver);
        }

        private sealed class FakeHogimnEngine : ISqlFormatterEngine
        {
            private readonly Func<SqlFormatRequest, SqlFormatResult> _format;

            public FakeHogimnEngine(Func<SqlFormatRequest, SqlFormatResult> format)
            {
                _format = format;
            }

            public SqlFormatterEngineKind Kind => SqlFormatterEngineKind.Hogimn;

            public bool Supports(DatabaseProviderKind providerKind)
            {
                return providerKind == DatabaseProviderKind.Oracle;
            }

            public SqlFormatResult Format(SqlFormatRequest request)
            {
                return _format(request);
            }
        }
    }
}