using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal static class IntegrationTestAssertions
    {
        public static void AssertConnectionSmoke(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);

            using (var client = new IntegrationTestReaderClient(settings))
            {
                var errorMessage = client.Connect();

                Assert.AreEqual(string.Empty, errorMessage, $"Unable to connect to {sourceType}: {errorMessage}");
                Assert.AreEqual(ConnectionState.Open, client.State);
                Assert.IsTrue(client.Disconnect(), $"Disconnect failed for {sourceType}.");
                Assert.AreEqual(ConnectionState.Closed, client.State);
            }
        }

        public static void AssertScalarQuery(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);
            var dialect = IntegrationTestSqlDialect.Create(settings);

            using (var client = new IntegrationTestReaderClient(settings))
            {
                var connectMessage = client.Connect();

                Assert.AreEqual(string.Empty, connectMessage, $"Unable to connect to {sourceType}: {connectMessage}");

                var result = client.ExecutePaged(dialect.BuildScalarSql(), 0, 1);

                AssertPagedSucceeded(result, sourceType);
                Assert.HasCount(1, result.Data.Rows);
                Assert.AreEqual(1, Convert.ToInt32(result.Data.Rows[0][0]));
            }
        }

        public static void AssertPagedQuery(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);

            using (var fixture = new IntegrationTestTableFixture(settings))
            {
                fixture.Prepare();

                var result = fixture.Client.ExecutePaged(fixture.Dialect.BuildSelectSql(), 1, 2);

                AssertPagedSucceeded(result, sourceType);
                Assert.HasCount(2, result.Data.Rows);
                Assert.AreEqual(2, Convert.ToInt32(result.Data.Rows[0]["ID"]));
                Assert.AreEqual(3, Convert.ToInt32(result.Data.Rows[1]["ID"]));
            }
        }

        public static void AssertSchemaTable(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);

            using (var fixture = new IntegrationTestTableFixture(settings))
            {
                fixture.Prepare();

                var result = fixture.Client.ExecutePaged(fixture.Dialect.BuildSelectSql(), 0, 1);

                AssertPagedSucceeded(result, sourceType);
                Assert.IsNotNull(result.Schema);
                Assert.IsGreaterThanOrEqualTo(2, result.Schema.Rows.Count);

                CollectionAssert.IsSubsetOf(new[] { "ColumnName", "ColumnOrdinal", "DataType", "AllowDBNull" },
                                            result.Schema.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());

                Assert.IsNotNull(FindSchemaRow(result.Schema, "ID"));
                Assert.IsNotNull(FindSchemaRow(result.Schema, "NAME"));
            }
        }

        public static void AssertColumnInfoCollector(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);

            using (var fixture = new IntegrationTestTableFixture(settings))
            {
                fixture.Prepare();

                var result = fixture.Client.ExecutePaged(fixture.Dialect.BuildSelectSql(), 0, 1);

                AssertPagedSucceeded(result, sourceType);

                var collector = SchemaColumnInfoBuilder.Build(sourceType, result.Schema);

                Assert.IsTrue(collector.TryGet("ID", out var idInfo));
                Assert.IsTrue(collector.TryGet("NAME", out var nameInfo));

                Assert.AreEqual(CategoryDataTypeKind.Number, idInfo.CategoryDataTypeKind);
                Assert.AreEqual(CategoryDataTypeKind.String, nameInfo.CategoryDataTypeKind);
                Assert.IsFalse(string.IsNullOrWhiteSpace(idInfo.BaseDataType));
                Assert.IsFalse(string.IsNullOrWhiteSpace(nameInfo.BaseDataType));
            }
        }

        public static void AssertPrimaryKeyMetadata(TestContext testContext, DataSourceType sourceType)
        {
            var settings = LoadRequiredSettings(testContext, sourceType);

            using (var fixture = new IntegrationTestTableFixture(settings))
            {
                fixture.Prepare();

                var result = fixture.Client.ExecutePaged(fixture.Dialect.BuildSelectSql(), 0, 1);

                AssertPagedSucceeded(result, sourceType);

                var schemaRow = FindSchemaRow(result.Schema, "ID");

                Assert.IsNotNull(schemaRow);
                Assert.Contains("IsKey", result.Schema.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
                Assert.IsFalse(schemaRow.IsNull("IsKey"));
                Assert.IsTrue(Convert.ToBoolean(schemaRow["IsKey"]));

                var collector = SchemaColumnInfoBuilder.Build(sourceType, result.Schema);

                Assert.IsTrue(collector.TryGet("ID", out var idInfo));
                Assert.IsTrue(idInfo.IsPrimaryKey);
            }
        }

        private static IntegrationTestDatabaseSettings LoadRequiredSettings(TestContext testContext, DataSourceType sourceType)
        {
            var settings = IntegrationTestDatabaseSettings.Load(testContext, sourceType);

            settings.RequireConfigured();

            return settings;
        }

        private static void AssertPagedSucceeded(IntegrationTestPagedQueryResult result, DataSourceType sourceType)
        {
            Assert.IsNotNull(result);

            Assert.AreEqual
            (
                string.Empty,
                result.ErrorMessage,
                $"Paged query failed for {sourceType}. ErrorCode: {result.ErrorCode}\r\n{result.ErrorMessage}"
            );

            Assert.IsNotNull(result.Data);
            Assert.IsNotNull(result.Schema);
        }

        private static DataRow FindSchemaRow(DataTable schema, string columnName)
        {
            if (schema == null || !schema.Columns.Contains("ColumnName"))
            {
                return null;
            }

            return schema.Rows.Cast<DataRow>()
                         .FirstOrDefault(row => string.Equals(Convert.ToString(row["ColumnName"]), columnName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
