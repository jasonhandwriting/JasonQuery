using JasonQuery.Core.QueryEngine.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Formatters
{
    [TestClass]
    public sealed class LargeTextPreviewBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithNull_ReturnsEmptyNonTruncatedResult()
        {
            var result = LargeTextPreviewBuilder.Build(null, 50, "...(truncated)");

            Assert.AreEqual(string.Empty, result.DisplayText);
            Assert.AreEqual(string.Empty, result.PreviewText);
            Assert.AreEqual(0, result.FullLength);
            Assert.AreEqual(50, result.PreviewLength);
            Assert.IsFalse(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithEmpty_ReturnsEmptyNonTruncatedResult()
        {
            var result = LargeTextPreviewBuilder.Build(string.Empty, 50, "...(truncated)");

            Assert.AreEqual(string.Empty, result.DisplayText);
            Assert.AreEqual(string.Empty, result.PreviewText);
            Assert.IsFalse(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithTextShorterThanThreshold_ReturnsFullText(int threshold)
        {
            var text = new string('A', threshold - 1);
            var result = LargeTextPreviewBuilder.Build(text, threshold, "...(truncated)");

            Assert.AreEqual(string.Empty, result.DisplayText);
            Assert.AreEqual(text, result.PreviewText);
            Assert.IsFalse(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithTextEqualToThreshold_ReturnsFullText(int threshold)
        {
            var text = new string('A', threshold);
            var result = LargeTextPreviewBuilder.Build(text, threshold, "...(truncated)");

            Assert.AreEqual(string.Empty, result.DisplayText);
            Assert.AreEqual(text, result.PreviewText);
            Assert.IsFalse(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithTextLongerThanThreshold_ReturnsPrefixAndSuffix(int threshold)
        {
            var text = new string('A', threshold + 1);
            var result = LargeTextPreviewBuilder.Build(text, threshold, "...(truncated)");

            Assert.AreEqual(TextPreviewFormatter.FormatText(threshold, threshold + 1), result.DisplayText);
            Assert.AreEqual(new string('A', threshold) + "...(truncated)", result.PreviewText);
            Assert.AreEqual(threshold + 1, result.FullLength);
            Assert.AreEqual(threshold, result.PreviewLength);
            Assert.IsTrue(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(49)]
        [DataRow(51)]
        [DataRow(999)]
        [DataRow(1001)]
        public void Build_WithUnsupportedThreshold_UsesFifty(int threshold)
        {
            var text = new string('A', 51);
            var result = LargeTextPreviewBuilder.Build(text, threshold, "...(truncated)");

            Assert.AreEqual(50, result.PreviewLength);
            Assert.AreEqual(new string('A', 50) + "...(truncated)", result.PreviewText);
            Assert.IsTrue(result.IsTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithCustomSuffix_UsesCustomSuffix()
        {
            var result = LargeTextPreviewBuilder.Build(new string('A', 51), 50, "…(截斷)");

            Assert.AreEqual(new string('A', 50) + "…(截斷)", result.PreviewText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithEmptySuffix_ReturnsPrefixOnly()
        {
            var result = LargeTextPreviewBuilder.Build(new string('A', 51), 50, string.Empty);

            Assert.AreEqual(new string('A', 50), result.PreviewText);
            Assert.IsTrue(result.IsTruncated);
        }
    }
}