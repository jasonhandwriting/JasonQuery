using JasonQuery.Core.Data.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Data.Formatters
{
    [TestClass]
    public sealed class DataSizeFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void FormatInt_WithZero_ReturnsZero()
        {
            Assert.AreEqual("0", DataSizeFormatter.FormatInt(0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatInt_WithPositiveValue_UsesThousandsSeparators()
        {
            Assert.AreEqual("1,234,567", DataSizeFormatter.FormatInt(1234567));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatInt_WithNegativeValue_UsesThousandsSeparators()
        {
            Assert.AreEqual("-1,234", DataSizeFormatter.FormatInt(-1234));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatNullableInt_WithNull_ReturnsZero()
        {
            Assert.AreEqual("0", DataSizeFormatter.FormatInt((long?)null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(-1L, "0B")]
        [DataRow(0L, "0B")]
        [DataRow(999L, "999B")]
        [DataRow(1000L, "1kB")]
        [DataRow(1500L, "1.5kB")]
        [DataRow(1005L, "1.01kB")]
        [DataRow(1000000L, "1MB")]
        [DataRow(1500000L, "1.5MB")]
        [DataRow(1000000000L, "1GB")]
        [DataRow(2500000000L, "2.5GB")]
        public void FormatBytes_WithDifferentSizes_ReturnsExpectedText(long bytes, string expected)
        {
            Assert.AreEqual(expected, DataSizeFormatter.FormatBytes(bytes));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatPrecisionScale_WithZeroScale_ReturnsPrecisionOnly()
        {
            Assert.AreEqual("10", DataSizeFormatter.FormatPrecisionScale(10, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatPrecisionScale_WithScale_ReturnsPrecisionAndScale()
        {
            Assert.AreEqual("10, 2", DataSizeFormatter.FormatPrecisionScale(10, 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatDecimal_WithDefaultPlaces_RoundsAwayFromZero()
        {
            Assert.AreEqual("1,234.5679", DataSizeFormatter.FormatDecimal(1234.56785m));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatDecimal_WithTwoPlaces_RoundsAwayFromZero()
        {
            Assert.AreEqual("1,234.57", DataSizeFormatter.FormatDecimal(1234.565m, 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatDecimal_WithNegativeMaxPlaces_TreatsAsZero()
        {
            Assert.AreEqual("13", DataSizeFormatter.FormatDecimal(12.5m, -1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatNullableDecimal_WithNull_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, DataSizeFormatter.FormatDecimal((decimal?)null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FormatNullableDouble_WithNull_ReturnsZero()
        {
            Assert.AreEqual(
                "0",
                DataSizeFormatter.FormatDecimal5((double?)null));
        }
    }
}