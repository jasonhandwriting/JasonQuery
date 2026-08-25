using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Oracle.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Arrange.Oracle.Formatters
{
    [TestClass]
    public sealed class OracleCharFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void SupportedKind_ReturnsChar()
        {
            var formatter = new OracleCharFormatter();

            Assert.AreEqual(SpecialDataTypeKind.Char, formatter.SupportedKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithNullColumnInfo_ReturnsFalseAndNull()
        {
            var formatter = new OracleCharFormatter();
            var success = formatter.TryFormat("abc", null, null, out object formatted);

            Assert.IsFalse(success);
            Assert.IsNull(formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithDifferentKind_ReturnsFalseAndNull()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.String, 5);
            var success = formatter.TryFormat("abc", columnInfo, null, out object formatted);

            Assert.IsFalse(success);
            Assert.IsNull(formatted);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithNullRawValue_PadsEmptyTextToColumnSize()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, 3);
            var success = formatter.TryFormat(null, columnInfo, null, out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("   ", formatted as string);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithZeroColumnSize_ReturnsOriginalText()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, 0);
            var success = formatter.TryFormat("abc", columnInfo, null, out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("abc", formatted as string);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithNegativeColumnSize_ReturnsOriginalText()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, -5);
            var success = formatter.TryFormat("abc", columnInfo, null, out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("abc", formatted as string);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithShorterText_PadsTrailingSpaces()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, 5);
            var success = formatter.TryFormat("abc", columnInfo, null, out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("abc  ", formatted as string);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithEqualLength_ReturnsOriginalText()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, 3);
            var success = formatter.TryFormat("abc", columnInfo, null,out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("abc", formatted as string);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void TryFormat_WithLongerText_DoesNotCutContent()
        {
            var formatter = new OracleCharFormatter();
            var columnInfo = CreateColumnInfo(SpecialDataTypeKind.Char, 3);
            var success = formatter.TryFormat("abcdef", columnInfo, null, out object formatted);

            Assert.IsTrue(success);
            Assert.AreEqual("abcdef", formatted as string);
        }

        private static ColumnInfo CreateColumnInfo(SpecialDataTypeKind kind, int columnSize)
        {
            return new ColumnInfo
            {
                SpecialDataTypeKind = kind,
                ColumnSize = columnSize
            };
        }
    }
}
