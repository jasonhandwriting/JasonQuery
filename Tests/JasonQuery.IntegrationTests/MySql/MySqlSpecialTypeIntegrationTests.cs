using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.IntegrationTests.MySql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class MySqlSpecialTypeIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesBlobTextJsonAndGeometryTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var geometrySchemaRow = IntegrationTestSpecialTypeAssertions.GetSchemaRow(result.Schema, "C_GEOMETRY");

                Assert.AreEqual(21, Convert.ToInt32(geometrySchemaRow["ProviderType"]));

                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.MySql, result);

                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_BLOB", "blob", CategoryDataTypeKind.LargeBinary);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TEXT", "text", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_JSON", "json", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_GEOMETRY", "geometry", CategoryDataTypeKind.String);

                var geometryInfo = IntegrationTestSpecialTypeAssertions.GetColumnInfo(collector, "C_GEOMETRY");

                Assert.IsFalse(geometryInfo.UsedProviderFallback);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesEnumSetUnsignedAndTemporalTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.MySql, result);

                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_ENUM", "enum", CategoryDataTypeKind.String);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_SET", "set", CategoryDataTypeKind.String);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_UINT", "int unsigned", CategoryDataTypeKind.Number, SpecialDataTypeKind.Number);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_DATE", "date", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.Date);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TIME", "time", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.Time);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_DATETIME", "datetime", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.DateTime);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TIMESTAMP", "timestamp", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.Timestamp);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("ProviderValue")]
        public void DataReader_ReturnsBlobTextJsonGeometryEnumSetUnsignedAndTemporalValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();

                IntegrationTestSpecialTypeAssertions.AssertPagedSucceeded(result, DataSourceType.MySql);
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_BLOB"], typeof(byte[]));
                IntegrationTestSpecialTypeAssertions.AssertContains("TEXT-CONTENT", result.Data.Rows[0]["C_TEXT"], "C_TEXT");
                IntegrationTestSpecialTypeAssertions.AssertContains("value", result.Data.Rows[0]["C_JSON"], "C_JSON");
                Assert.IsFalse(Convert.IsDBNull(result.Data.Rows[0]["C_GEOMETRY"]));
                Assert.AreEqual("B", Convert.ToString(result.Data.Rows[0]["C_ENUM"]));
                Assert.AreEqual("X,Z", Convert.ToString(result.Data.Rows[0]["C_SET"]));
                Assert.AreEqual(4000000000UL, Convert.ToUInt64(result.Data.Rows[0]["C_UINT"]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_DATE"], typeof(DateTime));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_TIME"], typeof(TimeSpan));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_DATETIME"], typeof(DateTime));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_TIMESTAMP"], typeof(DateTime));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Formatter")]
        public void Arrange_BlobTextAndJson_CreateTypedValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.MySql, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.MySql, result, collector);
                var blob = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_BLOB");
                var text = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_TEXT");
                var json = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_JSON");

                Assert.AreEqual(4, blob.LoadContent().Length);
                Assert.AreEqual(string.Empty, blob.PreviewText);
                StringAssert.Contains(text.LoadContent(), "TEXT-CONTENT");
                StringAssert.Contains(json.LoadContent(), "value");
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Formatter")]
        public void Arrange_EnumSetUnsignedAndGeometry_PreserveReadableValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.MySql, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.MySql, result, collector);

                Assert.AreEqual("B", Convert.ToString(arranged.Rows[0]["C_ENUM"]));
                Assert.AreEqual("X,Z", Convert.ToString(arranged.Rows[0]["C_SET"]));
                Assert.AreEqual("4000000000", Convert.ToString(arranged.Rows[0]["C_UINT"]));
                Assert.IsFalse(string.IsNullOrWhiteSpace(Convert.ToString(arranged.Rows[0]["C_GEOMETRY"])));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Formatter")]
        public void Arrange_DateTimeAndTimestamp_ReturnExpectedText()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.MySql, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.MySql, result, collector);

                Assert.AreEqual("2026/07/25", Convert.ToString(arranged.Rows[0]["C_DATE"]));
                Assert.AreEqual("12:34:56.123456", Convert.ToString(arranged.Rows[0]["C_TIME"]));
                Assert.AreEqual("2026/07/25 12:34:56.123456", Convert.ToString(arranged.Rows[0]["C_DATETIME"]));
                Assert.AreEqual("2026/07/25 12:34:56.123456", Convert.ToString(arranged.Rows[0]["C_TIMESTAMP"]));
            }
        }

        private IntegrationTestSpecialTypeFixture CreateFixture()
        {
            var settings = IntegrationTestSpecialTypeAssertions.LoadRequiredSettings(TestContext, DataSourceType.MySql);

            return new IntegrationTestSpecialTypeFixture(settings);
        }
    }
}
