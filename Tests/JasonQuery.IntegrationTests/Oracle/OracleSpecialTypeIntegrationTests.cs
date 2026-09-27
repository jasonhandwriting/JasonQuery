using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.IntegrationTests.Oracle
{
    [TestClass]
    [DoNotParallelize]
    public sealed class OracleSpecialTypeIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesCharacterNumberDateAndTimestampTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, result);

                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_CHAR", "CHAR", CategoryDataTypeKind.String, SpecialDataTypeKind.Char);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_NCHAR", "NCHAR", CategoryDataTypeKind.String, SpecialDataTypeKind.Char);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_NUMBER", "NUMBER", CategoryDataTypeKind.Number, SpecialDataTypeKind.Number);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_DATE", "DATE", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.DateTime);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TIMESTAMP", "TIMESTAMP", CategoryDataTypeKind.DateTime, SpecialDataTypeKind.Timestamp);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesLargeTextLargeBinaryAndLegacyTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var mainResult = fixture.ExecuteMainQuery();
                var mainCollector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, mainResult);

                IntegrationTestSpecialTypeAssertions.AssertColumn(mainCollector, "C_CLOB", "CLOB", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(mainCollector, "C_NCLOB", "NCLOB", CategoryDataTypeKind.LargeText, SpecialDataTypeKind.NString);
                IntegrationTestSpecialTypeAssertions.AssertColumn(mainCollector, "C_XML", "XMLTYPE", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(mainCollector, "C_RAW", "RAW", CategoryDataTypeKind.LargeBinary);
                IntegrationTestSpecialTypeAssertions.AssertColumn(mainCollector, "C_BLOB", "BLOB", CategoryDataTypeKind.LargeBinary);

                var longResult = fixture.ExecuteOracleLongQuery();
                var longCollector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, longResult);

                IntegrationTestSpecialTypeAssertions.AssertColumn(longCollector, "C_LONG", "LONG", CategoryDataTypeKind.LargeText);

                var longRawResult = fixture.ExecuteOracleLongRawQuery();
                var longRawCollector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, longRawResult);
                IntegrationTestSpecialTypeAssertions.AssertColumn(longRawCollector, "C_LONG_RAW", "LONG RAW", CategoryDataTypeKind.LargeBinary);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("ProviderValue")]
        public void DataReader_ReturnsSpecialTypeValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();

                IntegrationTestSpecialTypeAssertions.AssertPagedSucceeded(result, DataSourceType.Oracle);
                Assert.AreEqual("A", Convert.ToString(result.Data.Rows[0]["C_CHAR"])?.TrimEnd());
                Assert.AreEqual("中", Convert.ToString(result.Data.Rows[0]["C_NCHAR"])?.TrimEnd());
                IntegrationTestSpecialTypeAssertions.AssertContains("CLOB-CONTENT", result.Data.Rows[0]["C_CLOB"], "C_CLOB");
                IntegrationTestSpecialTypeAssertions.AssertContains("NCLOB-內容", result.Data.Rows[0]["C_NCLOB"], "C_NCLOB");
                IntegrationTestSpecialTypeAssertions.AssertContains("<root>", result.Data.Rows[0]["C_XML"], "C_XML");
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_RAW"], typeof(byte[]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_BLOB"], typeof(byte[]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_DATE"], typeof(DateTime));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_TIMESTAMP"], typeof(DateTime));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Formatter")]
        public void Arrange_CharAndNChar_ArePaddedToDeclaredLength()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.Oracle, result, collector);
                var charInfo = IntegrationTestSpecialTypeAssertions.GetColumnInfo(collector, "C_CHAR");
                var ncharInfo = IntegrationTestSpecialTypeAssertions.GetColumnInfo(collector, "C_NCHAR");
                var charValue = Convert.ToString(arranged.Rows[0]["C_CHAR"]);
                var ncharValue = Convert.ToString(arranged.Rows[0]["C_NCHAR"]);

                Assert.AreEqual(charInfo.ColumnSize, charValue.Length);
                Assert.AreEqual(ncharInfo.ColumnSize, ncharValue.Length);
                Assert.IsTrue(charValue.StartsWith("A", StringComparison.Ordinal));
                Assert.IsTrue(ncharValue.StartsWith("中", StringComparison.Ordinal));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Formatter")]
        public void Arrange_LargeTextAndBinary_CreateTypedValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.Oracle, result, collector);
                var clob = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_CLOB");
                var nclob = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_NCLOB");
                var xml = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_XML");
                var raw = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_RAW");
                var blob = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_BLOB");

                Assert.Contains("CLOB-CONTENT", clob.LoadContent());
                Assert.Contains("NCLOB-內容", nclob.LoadContent());
                Assert.Contains("<root>", xml.LoadContent());
                Assert.HasCount(4, raw.LoadContent());
                Assert.HasCount(4, blob.LoadContent());
                Assert.AreEqual(string.Empty, raw.PreviewText);
                Assert.AreEqual(string.Empty, blob.PreviewText);

                var longResult = fixture.ExecuteOracleLongQuery();
                var longCollector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, longResult);
                var longArranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.Oracle, longResult, longCollector);
                var longValue = IntegrationTestSpecialTypeAssertions.GetLargeText(longArranged, "C_LONG");

                var longRawResult = fixture.ExecuteOracleLongRawQuery();
                var longRawCollector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, longRawResult);
                var longRawArranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.Oracle, longRawResult, longRawCollector);
                var longRawValue = IntegrationTestSpecialTypeAssertions.GetLargeBinary(longRawArranged, "C_LONG_RAW");

                Assert.Contains("LONG-CONTENT", longValue.LoadContent());
                Assert.HasCount(4, longRawValue.LoadContent());
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Formatter")]
        public void Arrange_NumberDateAndTimestamp_ReturnExpectedText()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.Oracle, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.Oracle, result, collector);

                Assert.AreEqual("12345.678", Convert.ToString(arranged.Rows[0]["C_NUMBER"]));
                Assert.AreEqual("2026/07/25 00:00:00", Convert.ToString(arranged.Rows[0]["C_DATE"]));
                Assert.AreEqual("2026/07/25 12:34:56.123456", Convert.ToString(arranged.Rows[0]["C_TIMESTAMP"]));
            }
        }

        private IntegrationTestSpecialTypeFixture CreateFixture()
        {
            var settings = IntegrationTestSpecialTypeAssertions.LoadRequiredSettings(TestContext, DataSourceType.Oracle);

            return new IntegrationTestSpecialTypeFixture(settings);
        }
    }
}
