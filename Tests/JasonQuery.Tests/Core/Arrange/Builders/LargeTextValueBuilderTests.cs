using JasonLibrary.Core.Schema;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Arrange.Builders;
using JasonQuery.Core.Arrange.Formatting;
using JasonQuery.Core.QueryEngine.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Arrange.Builders
{
    [TestClass]
    public sealed class LargeTextValueBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithTextEqualToAllowedThreshold_ReturnsFullText(int threshold)
        {
            var text = new string('A', threshold);
            var value = Build(text, threshold);

            Assert.AreEqual(string.Empty, value.DisplayText);
            Assert.AreEqual(text, value.PreviewText);
            Assert.IsFalse(value.IsTruncated);
            Assert.AreEqual(text, value.LoadContent());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Build_WithTextLongerThanAllowedThreshold_ReturnsPreview(int threshold)
        {
            var text = new string('A', threshold + 1);
            var value = Build(text, threshold);

            Assert.AreEqual(TextPreviewFormatter.FormatText(threshold, threshold + 1), value.DisplayText);
            Assert.AreEqual(new string('A', threshold) + "...(truncated)", value.PreviewText);
            Assert.IsTrue(value.IsTruncated);
            Assert.AreEqual(text, value.LoadContent());
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
            var value = Build(new string('A', 51), threshold);

            Assert.AreEqual(TextPreviewFormatter.FormatText(50, 51), value.DisplayText);
            Assert.AreEqual(new string('A', 50) + "...(truncated)", value.PreviewText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithNullRawValue_ReturnsEmptyValue()
        {
            var value = Build(null, 50);

            Assert.AreEqual(string.Empty, value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
            Assert.AreEqual(0, value.Length);
            Assert.AreEqual(string.Empty, value.LoadContent());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithCustomSuffix_UsesContextSuffix()
        {
            var value = Build(new string('A', 51), 50, "…(截斷)");

            Assert.AreEqual(new string('A', 50) + "…(截斷)", value.PreviewText);
        }

        private static JasonQuery.Core.QueryEngine.Types.LargeTextDataType Build(object rawValue, int threshold, string suffix = "...(truncated)")
        {
            var context = new ArrangeContext
            {
                LargeTextPreviewLength = threshold,
                TruncatedText = suffix
            };

            var registry = new ColumnValueFormatterRegistry(null);
            var columnInfo = new ColumnInfo { BaseDataType = "CLOB" };

            return LargeTextValueBuilder.Build(rawValue, columnInfo, context, registry);
        }
    }
}