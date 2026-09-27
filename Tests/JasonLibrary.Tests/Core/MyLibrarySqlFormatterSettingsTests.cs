using JasonLibrary.Core;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core
{
    [TestClass]
    [DoNotParallelize]
    public sealed class MyLibrarySqlFormatterSettingsTests
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public void SqlFormatterListItemsPerLine_ValidValue_IsPreserved(int value)
        {
            try
            {
                MyLibrary.SqlFormatterListItemsPerLine = value;

                Assert.AreEqual(value, MyLibrary.SqlFormatterListItemsPerLine);
            }
            finally
            {
                MyLibrary.SqlFormatterListItemsPerLine = SqlFormatOptions.DefaultListItemsPerLine;
            }
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(11)]
        public void SqlFormatterListItemsPerLine_InvalidValue_UsesDefault(int value)
        {
            try
            {
                MyLibrary.SqlFormatterListItemsPerLine = value;

                Assert.AreEqual(SqlFormatOptions.DefaultListItemsPerLine, MyLibrary.SqlFormatterListItemsPerLine);
            }
            finally
            {
                MyLibrary.SqlFormatterListItemsPerLine = SqlFormatOptions.DefaultListItemsPerLine;
            }
        }
    }
}
