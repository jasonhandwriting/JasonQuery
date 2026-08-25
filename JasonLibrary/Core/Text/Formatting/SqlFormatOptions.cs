using System;

namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatOptions
    {
        public const int DefaultIndentSize = 4;
        public const int DefaultMaxLineWidth = 99;
        public const int DefaultLinesBetweenStatements = 2;
        public const int DefaultListItemsPerLine = 3;

        public SqlFormatOptions(int indentSize = DefaultIndentSize, int maxLineWidth = DefaultMaxLineWidth,
                                int linesBetweenStatements = DefaultLinesBetweenStatements,
                                SqlFormatterKeywordCase keywordCase = SqlFormatterKeywordCase.Preserve,
                                int listItemsPerLine = DefaultListItemsPerLine)
        {
            if (indentSize < 1 || indentSize > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(indentSize));
            }

            if (maxLineWidth < 20 || maxLineWidth > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLineWidth));
            }

            if (linesBetweenStatements < 0 || linesBetweenStatements > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(linesBetweenStatements));
            }

            if (listItemsPerLine < 1 || listItemsPerLine > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(listItemsPerLine));
            }

            IndentSize = indentSize;
            MaxLineWidth = maxLineWidth;
            LinesBetweenStatements = linesBetweenStatements;
            KeywordCase = keywordCase;
            ListItemsPerLine = listItemsPerLine;
        }

        public int IndentSize { get; }

        public int MaxLineWidth { get; }

        public int LinesBetweenStatements { get; }

        public SqlFormatterKeywordCase KeywordCase { get; }

        public int ListItemsPerLine { get; }

        public string GetIndentString()
        {
            return new string(' ', IndentSize);
        }
    }
}
