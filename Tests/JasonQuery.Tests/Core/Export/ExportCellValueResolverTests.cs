using JasonQuery.Core.Export;
using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Export
{
    [TestClass]
    public sealed class ExportCellValueResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNull_ReturnsNullResolution()
        {
            var result = ExportCellValueResolver.Resolve(null, "ignored");

            Assert.IsTrue(result.IsNull);
            Assert.IsNull(result.Value);
            Assert.AreEqual(string.Empty, result.Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithDbNull_ReturnsNullResolution()
        {
            var result = ExportCellValueResolver.Resolve(DBNull.Value, "ignored");

            Assert.IsTrue(result.IsNull);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithLargeText_LoadsFullContentAndIgnoresPreview()
        {
            var value = new LargeTextDataType("(CLOB)(20)", "preview...(truncated)", 20, true, () => "complete large text");

            var result = ExportCellValueResolver.Resolve(value, value.ToString());

            Assert.IsFalse(result.IsNull);
            Assert.AreEqual("complete large text", result.Value);
            Assert.AreEqual("complete large text", result.Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithLargeText_UsesCachedLoader()
        {
            var callCount = 0;

            var value = new LargeTextDataType("(CLOB)(4)", "data", 4, false, () =>
            {
                callCount++;
                return "data";
            });

            ExportCellValueResolver.Resolve(value, value.ToString());
            ExportCellValueResolver.Resolve(value, value.ToString());

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNullLargeText_ReturnsNullResolution()
        {
            var result = ExportCellValueResolver.Resolve(LargeTextDataType.CreateNull("<NULL>"), "<NULL>");

            Assert.IsTrue(result.IsNull);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithLargeTextLoaderError_PropagatesError()
        {
            var value = new LargeTextDataType("(CLOB)(10)", "preview", 10, true, () => throw new InvalidOperationException("load failed"));

            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => ExportCellValueResolver.Resolve(value, value.ToString()));

            Assert.AreEqual("load failed", ex.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithLargeBinary_ReturnsAliasWithoutLoadingContent()
        {
            var callCount = 0;

            var value = new LargeBinaryDataType("(BLOB)(3)", string.Empty, 3, false, () =>
            {
                callCount++;
                return new byte[] { 1, 2, 3 };
            });

            var result = ExportCellValueResolver.Resolve(value, value.ToString());

            Assert.AreEqual("(BLOB)(3)", result.Value);
            Assert.AreEqual("(BLOB)(3)", result.Text);
            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNullLargeBinary_ReturnsNullResolution()
        {
            var result = ExportCellValueResolver.Resolve(LargeBinaryDataType.CreateNull("<NULL>"), "<NULL>");

            Assert.IsTrue(result.IsNull);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithByteArray_ReturnsHexText()
        {
            var result = ExportCellValueResolver.Resolve(new byte[] { 0x01, 0xAF, 0x10 }, "ignored");

            Assert.AreEqual("01AF10", result.Value);
            Assert.AreEqual("01AF10", result.Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(true)]
        [DataRow(false)]
        [DataRow(123)]
        [DataRow(123.45)]
        public void Resolve_WithPrimitiveValue_PreservesValue(object value)
        {
            var result = ExportCellValueResolver.Resolve(value, $"text:{value}");

            Assert.AreEqual(value, result.Value);
            Assert.AreEqual($"text:{value}", result.Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithString_PreservesStringValue()
        {
            var result = ExportCellValueResolver.Resolve("ABC", "display ABC");

            Assert.AreEqual("ABC", result.Value);
            Assert.AreEqual("display ABC", result.Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNullCellText_UsesValueString()
        {
            var result = ExportCellValueResolver.Resolve(123, null);

            Assert.AreEqual("123", result.Text);
        }
    }
}
