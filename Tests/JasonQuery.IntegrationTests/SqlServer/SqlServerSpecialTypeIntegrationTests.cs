using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.IntegrationTests.SqlServer
{
    [TestClass]
    [DoNotParallelize]
    public sealed class SqlServerSpecialTypeIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("SpecialTypes")]
        public void ProviderSchema_ResolvesBitBinaryRowVersionTextXmlAndNumericTypes()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.SqlServer, result);

                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_BIT", "bit", CategoryDataTypeKind.Number, SpecialDataTypeKind.Bit);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_BINARY", "binary", CategoryDataTypeKind.LargeBinary);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_VARBINARY", "varbinary", CategoryDataTypeKind.LargeBinary);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_TEXT", "text", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_NTEXT", "ntext", CategoryDataTypeKind.LargeText, SpecialDataTypeKind.NString);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_XML", "xml", CategoryDataTypeKind.LargeText);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_NVARCHAR", "nvarchar", CategoryDataTypeKind.String, SpecialDataTypeKind.NString);
                IntegrationTestSpecialTypeAssertions.AssertColumn(collector, "C_DECIMAL", "decimal", CategoryDataTypeKind.Number, SpecialDataTypeKind.Number);

                var rowVersionInfo = IntegrationTestSpecialTypeAssertions.GetColumnInfo(collector, "C_ROWVERSION");

                Assert.IsTrue(string.Equals(rowVersionInfo.BaseDataType, "rowversion", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(rowVersionInfo.BaseDataType, "timestamp", StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(CategoryDataTypeKind.String, rowVersionInfo.CategoryDataTypeKind);
                Assert.AreEqual(SpecialDataTypeKind.RowVersion, rowVersionInfo.SpecialDataTypeKind);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("ProviderValue")]
        public void DataReader_ReturnsBitBinaryRowVersionTextXmlAndNumericValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();

                IntegrationTestSpecialTypeAssertions.AssertPagedSucceeded(result, DataSourceType.SqlServer);
                Assert.AreEqual(true, Convert.ToBoolean(result.Data.Rows[0]["C_BIT"]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_BINARY"], typeof(byte[]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_VARBINARY"], typeof(byte[]));
                Assert.IsInstanceOfType(result.Data.Rows[0]["C_ROWVERSION"], typeof(byte[]));
                IntegrationTestSpecialTypeAssertions.AssertContains("TEXT-CONTENT", result.Data.Rows[0]["C_TEXT"], "C_TEXT");
                IntegrationTestSpecialTypeAssertions.AssertContains("NTEXT-內容", result.Data.Rows[0]["C_NTEXT"], "C_NTEXT");
                IntegrationTestSpecialTypeAssertions.AssertContains("<root>", result.Data.Rows[0]["C_XML"], "C_XML");
                Assert.AreEqual("台北-101", Convert.ToString(result.Data.Rows[0]["C_NVARCHAR"]));
                Assert.AreEqual(12345.678m, Convert.ToDecimal(result.Data.Rows[0]["C_DECIMAL"]));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Formatter")]
        public void Arrange_BitAndRowVersion_ReturnExpectedText()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.SqlServer, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.SqlServer, result, collector);
                var rowVersion = Convert.ToString(arranged.Rows[0]["C_ROWVERSION"]);

                Assert.AreEqual("1", Convert.ToString(arranged.Rows[0]["C_BIT"]));
                Assert.IsTrue(rowVersion.StartsWith("0x", StringComparison.OrdinalIgnoreCase));
                Assert.AreEqual(18, rowVersion.Length);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Formatter")]
        public void Arrange_LargeTextAndBinary_CreateTypedValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.SqlServer, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.SqlServer, result, collector);
                var binary = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_BINARY");
                var varbinary = IntegrationTestSpecialTypeAssertions.GetLargeBinary(arranged, "C_VARBINARY");
                var text = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_TEXT");
                var ntext = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_NTEXT");
                var xml = IntegrationTestSpecialTypeAssertions.GetLargeText(arranged, "C_XML");

                Assert.AreEqual(4, binary.LoadContent().Length);
                Assert.AreEqual(4, varbinary.LoadContent().Length);
                Assert.AreEqual(string.Empty, binary.PreviewText);
                Assert.AreEqual(string.Empty, varbinary.PreviewText);
                StringAssert.Contains(text.LoadContent(), "TEXT-CONTENT");
                StringAssert.Contains(ntext.LoadContent(), "NTEXT-內容");
                StringAssert.Contains(xml.LoadContent(), "<root>");
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Formatter")]
        public void Arrange_NVarCharAndDecimal_PreserveExpectedValues()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Prepare();

                var result = fixture.ExecuteMainQuery();
                var collector = IntegrationTestSpecialTypeAssertions.BuildCollector(DataSourceType.SqlServer, result);
                var arranged = IntegrationTestSpecialTypeAssertions.Arrange(DataSourceType.SqlServer, result, collector);

                Assert.AreEqual("台北-101", Convert.ToString(arranged.Rows[0]["C_NVARCHAR"]));
                Assert.AreEqual("12345.678", Convert.ToString(arranged.Rows[0]["C_DECIMAL"]));
            }
        }

        private IntegrationTestSpecialTypeFixture CreateFixture()
        {
            var settings = IntegrationTestSpecialTypeAssertions.LoadRequiredSettings(TestContext, DataSourceType.SqlServer);

            return new IntegrationTestSpecialTypeFixture(settings);
        }
    }
}