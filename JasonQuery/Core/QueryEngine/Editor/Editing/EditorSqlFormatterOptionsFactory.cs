using JasonLibrary.Core.Text.Formatting;

//設定邊界、預設值與關鍵字大小寫對應
namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal static class SqlFormatterSettingsContract
    {
        public const string SectionName = "SQLFormatterConfig";
        public const string ListItemsPerLineSettingName = "ListItemsPerLine";
    }

    internal static class EditorSqlFormatterOptionsFactory
    {
        private const int DefaultIndentSize = 4;
        private const int DefaultBlankLinesBetweenStatements = 1;

        public static SqlFormatOptions Create(int indentSize, int maxLineWidth, int blankLinesBetweenStatements,
                                              bool convertKeywordCase, int keywordCaseValue,
                                              int listItemsPerLine = SqlFormatOptions.DefaultListItemsPerLine)
        {
            return new SqlFormatOptions
            (
                NormalizeIndentSize(indentSize),
                NormalizeMaxLineWidth(maxLineWidth),
                NormalizeBlankLinesBetweenStatements(blankLinesBetweenStatements) + 1,
                ResolveKeywordCase(convertKeywordCase, keywordCaseValue),
                NormalizeListItemsPerLine(listItemsPerLine)
            );
        }

        private static int NormalizeIndentSize(int indentSize)
        {
            return indentSize == 2 || indentSize == 4 || indentSize == 8
                   ? indentSize
                   : DefaultIndentSize;
        }

        private static int NormalizeMaxLineWidth(int maxLineWidth)
        {
            return maxLineWidth >= 20 && maxLineWidth <= 1000
                   ? maxLineWidth
                   : SqlFormatOptions.DefaultMaxLineWidth;
        }

        private static int NormalizeBlankLinesBetweenStatements(int blankLinesBetweenStatements)
        {
            return blankLinesBetweenStatements >= 0 && blankLinesBetweenStatements <= 4
                   ? blankLinesBetweenStatements
                   : DefaultBlankLinesBetweenStatements;
        }

        private static int NormalizeListItemsPerLine(int listItemsPerLine)
        {
            return listItemsPerLine >= 1 && listItemsPerLine <= 10
                   ? listItemsPerLine
                   : SqlFormatOptions.DefaultListItemsPerLine;
        }

        private static SqlFormatterKeywordCase ResolveKeywordCase(bool convertKeywordCase, int keywordCaseValue)
        {
            if (!convertKeywordCase)
            {
                return SqlFormatterKeywordCase.Preserve;
            }

            switch (keywordCaseValue)
            {
                case 1:
                    {
                        return SqlFormatterKeywordCase.Upper;
                    }
                case 2:
                    {
                        return SqlFormatterKeywordCase.Lower;
                    }
                default:
                    {
                        return SqlFormatterKeywordCase.Preserve;
                    }
            }
        }
    }
}