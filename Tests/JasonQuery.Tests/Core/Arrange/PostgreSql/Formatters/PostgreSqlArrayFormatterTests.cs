using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Arrange.PostgreSql.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Arrange.PostgreSql.Formatters
{
    [TestClass]
    public sealed class PostgreSqlArrayFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TryFormat_WithNullColumnInfo_ReturnsFalse()
        {
            var formatter = new PostgreSqlArrayFormatter();
            var success = formatter.TryFormat("{1,2}", null, new ArrangeContext(), out object formatted);

            Assert.IsFalse(success);
            Assert.IsNull(formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryFormat_WithNonArrayKind_ReturnsFalse()
        {
            var formatter = new PostgreSqlArrayFormatter();
            var columnInfo = new ColumnInfo { SpecialDataTypeKind = SpecialDataTypeKind.String };
            var success = formatter.TryFormat("{1,2}", columnInfo, new ArrangeContext(), out object formatted);

            Assert.IsFalse(success);
            Assert.IsNull(formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void TryFormat_WithTextEqualToThreshold_ReturnsFullArray(int threshold)
        {
            var text = new string('A', threshold);
            var formatted = Format(text, threshold);

            Assert.AreEqual(text, formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(50)]
        [DataRow(100)]
        [DataRow(200)]
        [DataRow(500)]
        [DataRow(1000)]
        public void TryFormat_WithTextLongerThanThreshold_ReturnsPreview(int threshold)
        {
            var formatted = Format(new string('A', threshold + 1), threshold);

            Assert.AreEqual(new string('A', threshold) + "...(truncated)", formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(49)]
        [DataRow(51)]
        [DataRow(1001)]
        public void TryFormat_WithUnsupportedThreshold_UsesFifty(int threshold)
        {
            var formatted = Format(new string('A', 51), threshold);

            Assert.AreEqual(new string('A', 50) + "...(truncated)", formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryFormat_WithCustomSuffix_UsesContextSuffix()
        {
            var formatted = Format(new string('A', 51), 50, "…(截斷)");

            Assert.AreEqual(new string('A', 50) + "…(截斷)", formatted);
        }

        private static string Format(string value, int threshold, string suffix = "...(truncated)")
        {
            var formatter = new PostgreSqlArrayFormatter();
            var columnInfo = new ColumnInfo { SpecialDataTypeKind = SpecialDataTypeKind.Array };
            var context = new ArrangeContext { LargeTextPreviewLength = threshold, TruncatedText = suffix };
            var success = formatter.TryFormat(value, columnInfo, context, out object formatted);

            Assert.IsTrue(success);

            return formatted as string;
        }
    }
}
