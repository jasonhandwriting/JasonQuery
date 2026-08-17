using JasonQuery.Core.QueryEngine.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Formatters
{
    [TestClass]
    public sealed class TextPreviewFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void FormatText_WithZeroValues_ReturnsByteText()
        {
            Assert.AreEqual("(0B of 0B)", TextPreviewFormatter.FormatText(0, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatText_WithKilobyteValues_ReturnsReadableText()
        {
            Assert.AreEqual("(1kB of 2.5kB)", TextPreviewFormatter.FormatText(1000, 2500));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatText_WithNegativeValues_NormalizesToZero()
        {
            Assert.AreEqual("(0B of 0B)", TextPreviewFormatter.FormatText(-100, -200));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatText_WithDifferentUnits_FormatsEachValueIndependently()
        {
            Assert.AreEqual("(1.5kB of 2MB)", TextPreviewFormatter.FormatText(1500, 2000000));
        }
    }
}