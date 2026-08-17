using JasonQuery.Core.QueryEngine.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Formatters
{
    [TestClass]
    public sealed class LargeTextPreviewLengthPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void IsSupported_WithAllowedLength_ReturnsTrue(int length)
        {
            Assert.IsTrue(LargeTextPreviewLengthPolicy.IsSupported(length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(49)]
        [DataRow(51)]
        [DataRow(99)]
        [DataRow(101)]
        [DataRow(999)]
        [DataRow(1001)]
        public void IsSupported_WithUnsupportedLength_ReturnsFalse(int length)
        {
            Assert.IsFalse(LargeTextPreviewLengthPolicy.IsSupported(length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Normalize_WithAllowedLength_ReturnsOriginal(int length)
        {
            Assert.AreEqual(length, LargeTextPreviewLengthPolicy.Normalize(length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(49)]
        [DataRow(51)]
        [DataRow(99)]
        [DataRow(101)]
        [DataRow(999)]
        [DataRow(1001)]
        public void Normalize_WithUnsupportedLength_ReturnsDefault(int length)
        {
            Assert.AreEqual(50, LargeTextPreviewLengthPolicy.Normalize(length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetAllowedLengths_ReturnsExpectedValuesAndIndependentArray()
        {
            var first = LargeTextPreviewLengthPolicy.GetAllowedLengths();
            var second = LargeTextPreviewLengthPolicy.GetAllowedLengths();

            CollectionAssert.AreEqual(new[] { 50, 100, 200, 500, 1000 }, first);
            Assert.AreNotSame(first, second);

            first[0] = 999;

            CollectionAssert.AreEqual(new[] { 50, 100, 200, 500, 1000 }, second);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Constants_UseFiftyForDefaultAndRawDataMode()
        {
            Assert.AreEqual(50, LargeTextPreviewLengthPolicy.DefaultLength);
            Assert.AreEqual(50, LargeTextPreviewLengthPolicy.RawDataModeLength);
        }
    }
}