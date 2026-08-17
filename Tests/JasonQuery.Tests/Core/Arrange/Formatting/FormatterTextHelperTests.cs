using JasonQuery.Core.Arrange.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Arrange.Formatting
{
    [TestClass]
    public sealed class FormatterTextHelperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithNull_ReturnsEmptyAndNotTruncated()
        {
            var actual = FormatterTextHelper.TrimToMaxLength(null, 5, out bool isTruncated);

            Assert.AreEqual(string.Empty, actual);
            Assert.IsFalse(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithEmpty_ReturnsEmptyAndNotTruncated()
        {
            var actual = FormatterTextHelper.TrimToMaxLength(string.Empty, 5, out bool isTruncated);

            Assert.AreEqual(string.Empty, actual);
            Assert.IsFalse(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithNegativeLength_TreatsLengthAsZero()
        {
            var actual = FormatterTextHelper.TrimToMaxLength("abc", -1, out bool isTruncated);

            Assert.AreEqual(string.Empty, actual);
            Assert.IsTrue(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithZeroLength_TruncatesNonEmptyText()
        {
            var actual = FormatterTextHelper.TrimToMaxLength("abc", 0, out bool isTruncated);

            Assert.AreEqual(string.Empty, actual);
            Assert.IsTrue(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithShorterText_ReturnsOriginal()
        {
            var actual = FormatterTextHelper.TrimToMaxLength( "abc", 5, out bool isTruncated);

            Assert.AreEqual("abc", actual);
            Assert.IsFalse(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithEqualLength_ReturnsOriginal()
        {
            var actual = FormatterTextHelper.TrimToMaxLength("abc", 3, out bool isTruncated);

            Assert.AreEqual("abc", actual);
            Assert.IsFalse(isTruncated);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrimToMaxLength_WithLongerText_ReturnsPrefixAndMarksTruncated()
        {
            var actual = FormatterTextHelper.TrimToMaxLength("abcdef", 3, out bool isTruncated);

            Assert.AreEqual("abc", actual);
            Assert.IsTrue(isTruncated);
        }
    }
}