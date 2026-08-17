using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class EditorSqlFormatterOptionsFactoryTests
    {
        [DataTestMethod]
        [DataRow(4)]
        [DataRow(19)]
        [DataRow(1001)]
        public void Create_InvalidMaxLineWidth_UsesDefault(int maxLineWidth)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, maxLineWidth, 1, false, 1);

            Assert.AreEqual(SqlFormatOptions.DefaultMaxLineWidth, options.MaxLineWidth);
        }

        [DataTestMethod]
        [DataRow(20)]
        [DataRow(120)]
        [DataRow(1000)]
        public void Create_ValidMaxLineWidth_PreservesValue(int maxLineWidth)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, maxLineWidth, 1, false, 1);

            Assert.AreEqual(maxLineWidth, options.MaxLineWidth);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void Create_KeywordConversionDisabled_PreservesKeywordCase(int keywordCaseValue)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, false, keywordCaseValue);

            Assert.AreEqual(SqlFormatterKeywordCase.Preserve, options.KeywordCase);
        }

        [TestMethod]
        public void Create_UpperKeywordSetting_UsesUpperCase()
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, true, 1);

            Assert.AreEqual(SqlFormatterKeywordCase.Upper, options.KeywordCase);
        }

        [TestMethod]
        public void Create_LowerKeywordSetting_UsesLowerCase()
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, true, 2);

            Assert.AreEqual(SqlFormatterKeywordCase.Lower, options.KeywordCase);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(3)]
        public void Create_InvalidKeywordSetting_PreservesKeywordCase(int keywordCaseValue)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, true, keywordCaseValue);

            Assert.AreEqual(SqlFormatterKeywordCase.Preserve, options.KeywordCase);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(4)]
        [DataRow(8)]
        public void Create_ValidIndentSize_PreservesValue(int indentSize)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(indentSize, 99, 1, false, 1);

            Assert.AreEqual(indentSize, options.IndentSize);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(16)]
        public void Create_InvalidIndentSize_UsesDefault(int indentSize)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(indentSize, 99, 1, false, 1);

            Assert.AreEqual(4, options.IndentSize);
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 2)]
        [DataRow(4, 5)]
        public void Create_BlankLinesBetweenStatements_MapsToNewlineCount(int blankLines,
                                                                          int expectedNewlineCount)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, blankLines, false, 1);

            Assert.AreEqual(expectedNewlineCount, options.LinesBetweenStatements);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(5)]
        public void Create_InvalidBlankLinesBetweenStatements_UsesDefault(int blankLines)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, blankLines, false, 1);

            Assert.AreEqual(2, options.LinesBetweenStatements);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public void Create_ValidListItemsPerLine_PreservesValue(int itemsPerLine)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, false, 1, itemsPerLine);

            Assert.AreEqual(itemsPerLine, options.ListItemsPerLine);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(11)]
        public void Create_InvalidListItemsPerLine_UsesDefault(int itemsPerLine)
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, false, 1, itemsPerLine);

            Assert.AreEqual(SqlFormatOptions.DefaultListItemsPerLine, options.ListItemsPerLine);
        }

        [TestMethod]
        public void Create_OmittedListItemsPerLine_UsesDefault()
        {
            var options = EditorSqlFormatterOptionsFactory.Create(4, 99, 1, false, 1);

            Assert.AreEqual(SqlFormatOptions.DefaultListItemsPerLine, options.ListItemsPerLine);
        }

        [TestMethod]
        public void ListItemsPerLinePersistence_UsesStableSettingNameAndDefault()
        {
            Assert.AreEqual("SQLFormatterConfig", SqlFormatterSettingsContract.SectionName);
            Assert.AreEqual("ListItemsPerLine", SqlFormatterSettingsContract.ListItemsPerLineSettingName);
            Assert.AreEqual(3, SqlFormatOptions.DefaultListItemsPerLine);
        }
    }
}