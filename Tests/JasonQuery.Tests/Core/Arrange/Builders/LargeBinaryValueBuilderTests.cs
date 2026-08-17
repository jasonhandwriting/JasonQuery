using JasonLibrary.Core.Schema;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Arrange.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Arrange.Builders
{
    [TestClass]
    public sealed class LargeBinaryValueBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("BLOB")]
        [DataRow("LONG RAW")]
        [DataRow("BYTEA")]
        [DataRow("VARBINARY")]
        [DataRow("ROWVERSION")]
        [DataRow("GEOMETRY")]
        public void Build_ForBinaryType_AlwaysDisplaysAliasOnly(string baseDataType)
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03 };
            var value = LargeBinaryValueBuilder.Build(bytes, new ColumnInfo { BaseDataType = baseDataType }, new ArrangeContext());

            Assert.AreEqual($"({baseDataType})(3)", value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
            Assert.AreEqual($"({baseDataType})(3)", value.ToString());
            Assert.IsFalse(value.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithAnyLargeTextThreshold_DoesNotExposeBinaryPreview(int threshold)
        {
            var context = new ArrangeContext { LargeTextPreviewLength = threshold };
            var value = LargeBinaryValueBuilder.Build(new byte[] { 0xAB, 0xCD }, new ColumnInfo { BaseDataType = "BLOB" }, context);

            Assert.AreEqual("(BLOB)(2)", value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithNullRawValue_ReturnsZeroLengthAlias()
        {
            var value = LargeBinaryValueBuilder.Build(null, new ColumnInfo { BaseDataType = "BLOB" }, new ArrangeContext());

            Assert.AreEqual("(BLOB)(0)", value.DisplayText);
            Assert.AreEqual(0, value.Length);
            Assert.AreEqual(0, value.LoadContent().Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_LoadContent_ReturnsOriginalByteArray()
        {
            var bytes = new byte[] { 0x01, 0x02 };
            var value = LargeBinaryValueBuilder.Build(bytes, new ColumnInfo { BaseDataType = "BLOB" }, new ArrangeContext());

            Assert.AreSame(bytes, value.LoadContent());
        }
    }
}