using JasonQuery.Core.QueryEngine.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Formatters
{
    [TestClass]
    public sealed class BinaryPreviewFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithNull_ReturnsEmptyString()
        {
            var actual = BinaryPreviewFormatter.ToReadableHexString(null);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithEmptyArray_ReturnsEmptyString()
        {
            var actual = BinaryPreviewFormatter.ToReadableHexString(new byte[0]);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithZeroByte_ReturnsTwoHexDigits()
        {
            var data = new byte[] { 0x00 };
            var actual = BinaryPreviewFormatter.ToReadableHexString(data);

            Assert.AreEqual("00", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithLeadingZero_PreservesLeadingZero()
        {
            var data = new byte[] { 0x00, 0x0A, 0xFF };
            var actual = BinaryPreviewFormatter.ToReadableHexString(data);

            Assert.AreEqual("000AFF", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithAlphabeticHexDigits_UsesUppercase()
        {
            var data = new byte[] { 0xAB, 0xCD, 0xEF };
            var actual = BinaryPreviewFormatter.ToReadableHexString(data);

            Assert.AreEqual("ABCDEF", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToReadableHexString_WithMultipleBytes_DoesNotInsertSeparators()
        {
            var data = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89 };
            var actual = BinaryPreviewFormatter.ToReadableHexString(data);

            Assert.AreEqual("0123456789", actual);
        }
    }
}