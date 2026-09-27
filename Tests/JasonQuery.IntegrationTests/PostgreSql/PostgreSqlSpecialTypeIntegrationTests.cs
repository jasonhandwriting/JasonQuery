using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.IntegrationTests.PostgreSql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class PostgreSqlSpecialTypeIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesBinaryArrayJsonNumericAndTimestampTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.PostgreSql, result);

                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_BYTEA", "bytea", CategoryDataTypeKind.LargeBinary, expectedIsArray: false);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_BYTEA_ARRAY", "bytea[]", CategoryDataTypeKind.LargeText, expectedIsArray: true);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_VARCHAR_ARRAY", "character varying[]", CategoryDataTypeKind.LargeText, SpecialDataTypeKind.CharacterArray, true);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TEXT_ARRAY", "text[]", CategoryDataTypeKind.LargeText, expectedIsArray: true);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_JSON", "json", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_JSONB", "jsonb", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_NUMERIC", "numeric", CategoryDataTypeKind.Number);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TIMESTAMP", "timestamp without time zone", CategoryDataTypeKind.DateTime,
                                                                  SpecialDataTypeKind.TimestampWithoutTimeZone);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("ProviderValue")]
        public void DataReader_ReturnsBinaryArrayJsonNumericAndTimestampValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();

                IntegrationTestSpecialTypeAssertions.AssertPagedSucceeded(result, DataSourceType.PostgreSql);
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_BYTEA"], typeof(byte[]));
                Assert.IsFalse(Convert.IsDBNull(result.Data.Rows[0]["C_BYTEA_ARRAY"]));
                Assert.IsFalse(Convert.IsDBNull(result.Data.Rows[0]["C_VARCHAR_ARRAY"]));
                Assert.IsFalse(Convert.IsDBNull(result.Data.Rows[0]["C_TEXT_ARRAY"]));
                IntegrationTestSpecialTypeAssertions.AssertContains("value", result.Data.Rows[0]["C_JSON"], "C_JSON");
                IntegrationTestSpecialTypeAssertions.AssertContains("value", result.Data.Rows[0]["C_JSONB"], "C_JSONB");
                Assert.AreEqual(12345.678m, Convert.ToDecimal(result.Data.Rows[0]["C_NUMERIC"]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_TIMESTAMP"], typeof(DateTime));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("ProviderSchema")]
        public void GetSchemaTable_ReportsExpectedProviderTypeIdentifiers()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();

                IntegrationTestSpecialTypeAssertions.AssertPagedSucceeded(result, DataSourceType.PostgreSql);
                Assert.AreEqual(17, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_BYTEA")["ProviderType"]));
                Assert.AreEqual(1001, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_BYTEA_ARRAY")["ProviderType"]));
                Assert.AreEqual(1015, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_VARCHAR_ARRAY")["ProviderType"]));
                Assert.AreEqual(1009, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_TEXT_ARRAY")["ProviderType"]));
                Assert.AreEqual(114, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_JSON")["ProviderType"]));
                Assert.AreEqual(3802, Convert.ToInt32(IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_JSONB")["ProviderType"]));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Formatter")]
        public void Arrange_ArrayJsonAndBinary_CreateTypedValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.PostgreSql, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.PostgreSql, result, collector);
                var bytea = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_BYTEA");
                var byteaArray = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_BYTEA_ARRAY");
                var varcharArray = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_VARCHAR_ARRAY");
                var textArray = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_TEXT_ARRAY");
                var json = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_JSON");
                var jsonb = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_JSONB");

                Assert.HasCount(4, bytea.LoadContent());
                Assert.IsFalse(string.IsNullOrWhiteSpace(byteaArray.LoadContent()));
                Assert.IsFalse(string.IsNullOrWhiteSpace(varcharArray.LoadContent()));
                Assert.IsFalse(string.IsNullOrWhiteSpace(textArray.LoadContent()));
                Assert.Contains("value", json.LoadContent());
                Assert.Contains("value", jsonb.LoadContent());
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Formatter")]
        public void Arrange_NumericAndTimestamp_ReturnExpectedText()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.PostgreSql, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.PostgreSql, result, collector);

                Assert.AreEqual("12345.678", Convert.ToString(arranged.Rows[0]["C_NUMERIC"]));
                Assert.AreEqual("2026/07/25 12:34:56.123456", Convert.ToString(arranged.Rows[0]["C_TIMESTAMP"]));
            }
        }

        private IntegrationTestSpecialTypeFixture CreateFixture()
        {
            var settings = IntegrationTestSpecialTypeAssertions.LoadRequiredSettings(TestContext, DataSourceType.PostgreSql);

            return new IntegrationTestSpecialTypeFixture(settings);
        }
    }
}
