using JasonLibrary.Core.Schema.Enums;

namespace JasonQuery.Display.Columns
{
    public sealed class ColumnDisplayContext
    {
        public ColumnMode Mode { get; }

        public int MultiLineCount { get; }

        public ColumnDisplayContext(ColumnMode mode, int multiLineCount)
        {
            Mode = mode;
            MultiLineCount = multiLineCount;
        }
    }
}
